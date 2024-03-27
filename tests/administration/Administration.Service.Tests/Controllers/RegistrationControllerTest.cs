/********************************************************************************
 * Copyright (c) 2021, 2023 Contributors to the Eclipse Foundation
 *
 * See the NOTICE file(s) distributed with this work for additional
 * information regarding copyright ownership.
 *
 * This program and the accompanying materials are made available under the
 * terms of the Apache License, Version 2.0 which is available at
 * https://www.apache.org/licenses/LICENSE-2.0.
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 *
 * SPDX-License-Identifier: Apache-2.0
 ********************************************************************************/

using Microsoft.AspNetCore.Mvc;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Controllers;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Clearinghouse.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Dim.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;
using Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;
using System.Text;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.Controllers;

public class RegistrationControllerTest
{
    private static readonly string AccessToken = "THISISTHEACCESSTOKEN";
    private readonly IIdentityData _identity;
    private readonly IRegistrationBusinessLogic _logic;
    private readonly RegistrationController _controller;
    private readonly IFixture _fixture;

    public RegistrationControllerTest()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.ConfigureFixture();

        _identity = A.Fake<IIdentityData>();
        A.CallTo(() => _identity.IdentityId).Returns(Guid.NewGuid());
        A.CallTo(() => _identity.IdentityTypeId).Returns(IdentityTypeId.COMPANY_USER);
        A.CallTo(() => _identity.CompanyId).Returns(Guid.NewGuid());
        _logic = A.Fake<IRegistrationBusinessLogic>();
        _controller = new RegistrationController(_logic);
        _controller.AddControllerContextWithClaimAndBearer(AccessToken, _identity);
    }

    [Fact]
    public async Task GetCompanyApplicationDetailsAsync_ReturnsCompanyApplicationDetails()
    {
        //Arrange
        var paginationResponse = new Pagination.Response<CompanyApplicationDetails>(new Pagination.Metadata(15, 1, 1, 15), _fixture.CreateMany<CompanyApplicationDetails>(5));
        A.CallTo(() => _logic.GetCompanyApplicationDetailsAsync(0, 15, null, null))
                  .Returns(paginationResponse);

        //Act
        var result = await _controller.GetApplicationDetailsAsync(0, 15, null, null);

        //Assert
        A.CallTo(() => _logic.GetCompanyApplicationDetailsAsync(0, 15, null, null)).MustHaveHappenedOnceExactly();
        Assert.IsType<Pagination.Response<CompanyApplicationDetails>>(result);
        result.Content.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetCompanyWithAddressAsync_ReturnsExpectedResult()
    {
        //Arrange
        var applicationId = _fixture.Create<Guid>();
        var data = _fixture.Create<CompanyWithAddressData>();
        A.CallTo(() => _logic.GetCompanyWithAddressAsync(applicationId))
            .Returns(data);

        //Act
        var result = await _controller.GetCompanyWithAddressAsync(applicationId);

        //Assert
        A.CallTo(() => _logic.GetCompanyWithAddressAsync(applicationId)).MustHaveHappenedOnceExactly();
        Assert.IsType<CompanyWithAddressData>(result);
    }

    [Fact]
    public async Task ApproveApplication_ReturnsExpectedResult()
    {
        //Arrange
        var applicationId = _fixture.Create<Guid>();

        //Act
        var result = await _controller.ApproveApplication(applicationId);

        //Assert
        A.CallTo(() => _logic.ApproveRegistrationVerification(applicationId)).MustHaveHappenedOnceExactly();
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeclineApplication_ReturnsExpectedResult()
    {
        //Arrange
        var applicationId = _fixture.Create<Guid>();

        //Act
        var result = await _controller.DeclineApplication(applicationId, new RegistrationDeclineData("test"), CancellationToken.None);

        //Assert
        A.CallTo(() => _logic.DeclineRegistrationVerification(applicationId, "test", A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task ProcessClearinghouseResponse_ReturnsExpectedResult()
    {
        //Arrange
        var data = _fixture.Create<ClearinghouseResponseData>();
        var cancellationToken = CancellationToken.None;

        //Act
        var result = await _controller.ProcessClearinghouseResponse(data, cancellationToken);

        //Assert
        A.CallTo(() => _logic.ProcessClearinghouseResponseAsync(data, cancellationToken)).MustHaveHappenedOnceExactly();
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task GetChecklistForApplication_ReturnsExpectedResult()
    {
        //Arrange
        var applicationId = _fixture.Create<Guid>();
        var list = new List<ChecklistDetails>
        {
            new(ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE, null, new List<ProcessStepTypeId>()),
            new(ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE, null, new List<ProcessStepTypeId>()),
            new(ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.FAILED, "error occured", new List<ProcessStepTypeId>
                {
                    ProcessStepTypeId.RETRIGGER_IDENTITY_WALLET
                }),
            new(ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP, ApplicationChecklistEntryStatusId.IN_PROGRESS, null, new List<ProcessStepTypeId>()),
            new(ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ApplicationChecklistEntryStatusId.IN_PROGRESS, null, new List<ProcessStepTypeId>()),
        };
        A.CallTo(() => _logic.GetChecklistForApplicationAsync(applicationId))
            .Returns(list);

        //Act
        var result = await _controller.GetChecklistForApplication(applicationId);

        //Assert
        A.CallTo(() => _logic.GetChecklistForApplicationAsync(applicationId)).MustHaveHappenedOnceExactly();
        result.Should().NotBeNull().And.NotBeEmpty().And.HaveCount(5);
        result.Where(x => x.RetriggerableProcessSteps.Any()).Should().HaveCount(1);
        result.Where(x => x.Status == ApplicationChecklistEntryStatusId.FAILED).Should().ContainSingle();
    }

    [Fact]
    public async Task ReTriggerClearinghouse_ReturnsExpectedResult()
    {
        // Arrange
        var applicationId = _fixture.Create<Guid>();

        // Act
        var result = await _controller.RetriggerClearinghouseChecklist(applicationId);

        // Assert
        A.CallTo(() => _logic.TriggerChecklistAsync(applicationId, ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ProcessStepTypeId.RETRIGGER_CLEARING_HOUSE)).MustHaveHappenedOnceExactly();
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task OverrideClearinghouse_ReturnsExpectedResult()
    {
        // Arrange
        var applicationId = _fixture.Create<Guid>();

        // Act
        var result = await _controller.OverrideClearinghouseChecklist(applicationId);

        // Assert
        A.CallTo(() => _logic.TriggerChecklistAsync(applicationId, ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ProcessStepTypeId.TRIGGER_OVERRIDE_CLEARING_HOUSE)).MustHaveHappenedOnceExactly();
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task TriggerIdentityWallet_ReturnsExpectedResult()
    {
        // Arrange
        var applicationId = _fixture.Create<Guid>();

        // Act
        var result = await _controller.TriggerIdentityWallet(applicationId);

        // Assert
        A.CallTo(() => _logic.TriggerChecklistAsync(applicationId, ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ProcessStepTypeId.RETRIGGER_IDENTITY_WALLET)).MustHaveHappenedOnceExactly();
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task TriggerSelfDescription_ReturnsExpectedResult()
    {
        // Arrange
        var applicationId = _fixture.Create<Guid>();

        // Act
        var result = await _controller.TriggerSelfDescription(applicationId);

        // Assert
        A.CallTo(() => _logic.TriggerChecklistAsync(applicationId, ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP, ProcessStepTypeId.RETRIGGER_SELF_DESCRIPTION_LP)).MustHaveHappenedOnceExactly();
        result.Should().BeOfType<NoContentResult>();
    }

    [Theory]
    [InlineData(ProcessStepTypeId.RETRIGGER_BUSINESS_PARTNER_NUMBER_PULL)]
    [InlineData(ProcessStepTypeId.RETRIGGER_BUSINESS_PARTNER_NUMBER_PUSH)]
    public async Task TriggerBpn_ReturnsExpectedResult(ProcessStepTypeId processStepTypeId)
    {
        // Arrange
        var applicationId = _fixture.Create<Guid>();

        // Act
        var result = await _controller.TriggerBpn(applicationId, processStepTypeId);

        // Assert
        A.CallTo(() => _logic.TriggerChecklistAsync(applicationId, ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, processStepTypeId)).MustHaveHappenedOnceExactly();
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task ProcessClearinghouseSelfDescription_ReturnsExpectedResult()
    {
        // Arrange
        var data = new SelfDescriptionResponseData(Guid.NewGuid(), SelfDescriptionStatus.Confirm, null, "{ \"test\": true }");

        // Act
        var result = await _controller.ProcessClearinghouseSelfDescription(data, CancellationToken.None);

        // Assert
        A.CallTo(() => _logic.ProcessClearinghouseSelfDescription(data, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task GetDocumentContentFileAsync_WithValidData_ReturnsOk()
    {
        //Arrange
        const string fileName = "test.pdf";
        const string contentType = "application/pdf";
        var id = Guid.NewGuid();
        var content = Encoding.UTF8.GetBytes("This is just test content");
        A.CallTo(() => _logic.GetDocumentAsync(id))
            .Returns((fileName, content, contentType));

        //Act
        await _controller.GetDocumentContentFileAsync(id);

        //Assert
        A.CallTo(() => _logic.GetDocumentAsync(id)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ProcessDimResponse_WithValidData_ReturnsOk()
    {
        //Arrange
        var data = _fixture.Create<DimWalletData>();

        //Act
        await _controller.ProcessDimResponse("bpn", data, CancellationToken.None);

        //Assert
        A.CallTo(() => _logic.ProcessDimResponseAsync("bpn", data, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RetriggerCreateDimWallet_ReturnsExpectedResult()
    {
        // Arrange
        var applicationId = _fixture.Create<Guid>();

        // Act
        var result = await _controller.RetriggerCreateDimWallet(applicationId);

        // Assert
        A.CallTo(() => _logic.TriggerChecklistAsync(applicationId, ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ProcessStepTypeId.RETRIGGER_CREATE_DIM_WALLET)).MustHaveHappenedOnceExactly();
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task RetriggerValidateDid_ReturnsExpectedResult()
    {
        // Arrange
        var applicationId = _fixture.Create<Guid>();

        // Act
        var result = await _controller.RetriggerValidateDid(applicationId);

        // Assert
        A.CallTo(() => _logic.TriggerChecklistAsync(applicationId, ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ProcessStepTypeId.RETRIGGER_VALIDATE_DID_DOCUMENT)).MustHaveHappenedOnceExactly();
        result.Should().BeOfType<NoContentResult>();
    }
}
