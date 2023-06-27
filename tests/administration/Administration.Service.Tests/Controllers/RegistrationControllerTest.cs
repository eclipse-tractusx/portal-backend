/********************************************************************************
 * Copyright (c) 2021, 2023 BMW Group AG
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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;
using System.Text;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.Controllers;

public class RegistrationControllerTest
{
    private static readonly string IamUserId = "4C1A6851-D4E7-4E10-A011-3732CD045E8A";
    private static readonly string AccessToken = "THISISTHEACCESSTOKEN";
    private readonly IRegistrationBusinessLogic _logic;
    private readonly RegistrationController _controller;
    private readonly IFixture _fixture;
    public RegistrationControllerTest()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
        .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _logic = A.Fake<IRegistrationBusinessLogic>();
        this._controller = new RegistrationController(_logic);
        _controller.AddControllerContextWithClaimAndBearer(IamUserId, AccessToken);
    }

    [Fact]
    public async Task GetCompanyApplicationDetailsAsync_ReturnsCompanyApplicationDetails()
    {
        //Arrange
        var paginationResponse = new Pagination.Response<CompanyApplicationDetails>(new Pagination.Metadata(15, 1, 1, 15), _fixture.CreateMany<CompanyApplicationDetails>(5));
        A.CallTo(() => _logic.GetCompanyApplicationDetailsAsync(0, 15, null, null))
                  .Returns(paginationResponse);

        //Act
        var result = await this._controller.GetApplicationDetailsAsync(0, 15, null, null).ConfigureAwait(false);

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
            .ReturnsLazily(() => data);

        //Act
        var result = await this._controller.GetCompanyWithAddressAsync(applicationId).ConfigureAwait(false);

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
        var result = await this._controller.ApproveApplication(applicationId).ConfigureAwait(false);

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
        var result = await this._controller.DeclineApplication(applicationId, new RegistrationDeclineData("test")).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.DeclineRegistrationVerification(applicationId, "test")).MustHaveHappenedOnceExactly();
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task ProcessClearinghouseResponse_ReturnsExpectedResult()
    {
        //Arrange
        var bpn = _fixture.Create<string>();
        var data = _fixture.Create<ClearinghouseResponseData>();
        A.CallTo(() => _logic.ProcessClearinghouseResponseAsync(data, A<CancellationToken>._))
            .ReturnsLazily(() => Task.CompletedTask);

        //Act
        var result = await this._controller.ProcessClearinghouseResponse(data, CancellationToken.None).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.ProcessClearinghouseResponseAsync(data, CancellationToken.None)).MustHaveHappenedOnceExactly();
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
            .ReturnsLazily(() => list);

        //Act
        var result = await this._controller.GetChecklistForApplication(applicationId).ConfigureAwait(false);

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
        A.CallTo(() => _logic.TriggerChecklistAsync(applicationId, A<ApplicationChecklistEntryTypeId>._, A<ProcessStepTypeId>._))
            .ReturnsLazily(() => Task.CompletedTask);

        // Act
        var result = await this._controller.RetriggerClearinghouseChecklist(applicationId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _logic.TriggerChecklistAsync(applicationId, ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ProcessStepTypeId.RETRIGGER_CLEARING_HOUSE)).MustHaveHappenedOnceExactly();
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task OverrideClearinghouse_ReturnsExpectedResult()
    {
        // Arrange
        var applicationId = _fixture.Create<Guid>();
        A.CallTo(() => _logic.TriggerChecklistAsync(applicationId, A<ApplicationChecklistEntryTypeId>._, A<ProcessStepTypeId>._))
            .ReturnsLazily(() => Task.CompletedTask);

        // Act
        var result = await this._controller.OverrideClearinghouseChecklist(applicationId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _logic.TriggerChecklistAsync(applicationId, ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ProcessStepTypeId.TRIGGER_OVERRIDE_CLEARING_HOUSE)).MustHaveHappenedOnceExactly();
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task TriggerIdentityWallet_ReturnsExpectedResult()
    {
        // Arrange
        var applicationId = _fixture.Create<Guid>();
        A.CallTo(() => _logic.TriggerChecklistAsync(applicationId, A<ApplicationChecklistEntryTypeId>._, A<ProcessStepTypeId>._))
            .ReturnsLazily(() => Task.CompletedTask);

        // Act
        var result = await this._controller.TriggerIdentityWallet(applicationId);

        // Assert
        A.CallTo(() => _logic.TriggerChecklistAsync(applicationId, ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ProcessStepTypeId.RETRIGGER_IDENTITY_WALLET)).MustHaveHappenedOnceExactly();
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task TriggerSelfDescription_ReturnsExpectedResult()
    {
        // Arrange
        var applicationId = _fixture.Create<Guid>();
        A.CallTo(() => _logic.TriggerChecklistAsync(applicationId, A<ApplicationChecklistEntryTypeId>._, A<ProcessStepTypeId>._))
            .ReturnsLazily(() => Task.CompletedTask);

        // Act
        var result = await this._controller.TriggerSelfDescription(applicationId);

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
        A.CallTo(() => _logic.TriggerChecklistAsync(applicationId, A<ApplicationChecklistEntryTypeId>._, A<ProcessStepTypeId>._))
            .ReturnsLazily(() => Task.CompletedTask);

        // Act
        var result = await this._controller.TriggerBpn(applicationId, processStepTypeId);

        // Assert
        A.CallTo(() => _logic.TriggerChecklistAsync(applicationId, ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, processStepTypeId)).MustHaveHappenedOnceExactly();
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task ProcessClearinghouseSelfDescription_ReturnsExpectedResult()
    {
        // Arrange
        var data = new SelfDescriptionResponseData(Guid.NewGuid(), SelfDescriptionStatus.Confirm, null, JsonDocument.Parse("{ \"test\": true }"));
        A.CallTo(() => _logic.ProcessClearinghouseSelfDescription(data, A<CancellationToken>._))
            .ReturnsLazily(() => Task.CompletedTask);

        // Act
        var result = await this._controller.ProcessClearinghouseSelfDescription(data, CancellationToken.None);

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
            .ReturnsLazily(() => (fileName, content, contentType));

        //Act
        await this._controller.GetDocumentContentFileAsync(id).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.GetDocumentAsync(id)).MustHaveHappenedOnceExactly();
    }
}
