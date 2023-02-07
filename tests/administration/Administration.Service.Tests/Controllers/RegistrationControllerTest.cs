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
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;

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
        A.CallTo(() => _logic.GetCompanyApplicationDetailsAsync(0, 15,null,null))
                  .Returns(paginationResponse);

        //Act
        var result = await this._controller.GetApplicationDetailsAsync(0, 15,null,null).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.GetCompanyApplicationDetailsAsync(0, 15,null,null)).MustHaveHappenedOnceExactly();
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
        A.CallTo(() => _logic.SetRegistrationVerification(applicationId, true, null)).MustHaveHappenedOnceExactly();
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
        A.CallTo(() => _logic.SetRegistrationVerification(applicationId, false, "test")).MustHaveHappenedOnceExactly();
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task ProcessClearinghouseResponse_ReturnsExpectedResult()
    {
        //Arrange
        var bpn = _fixture.Create<string>();
        var data = _fixture.Create<ClearinghouseResponseData>();
        A.CallTo(() => _logic.ProcessClearinghouseResponseAsync(bpn, data, A<CancellationToken>._))
            .ReturnsLazily(() => Task.CompletedTask);

        //Act
        var result = await this._controller.ProcessClearinghouseResponse(bpn, data, CancellationToken.None).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.ProcessClearinghouseResponseAsync(bpn, data, CancellationToken.None)).MustHaveHappenedOnceExactly();
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task GetChecklistForApplication_ReturnsExpectedResult()
    {
        //Arrange
        var applicationId = _fixture.Create<Guid>();
        var list = new List<ChecklistDetails>
        {
            new(ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE, null, false),
            new(ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE, null, false),
            new(ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.FAILED, "error occured", true),
            new(ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP, ApplicationChecklistEntryStatusId.IN_PROGRESS, null, true),
            new(ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ApplicationChecklistEntryStatusId.IN_PROGRESS, null, true),
        };
        A.CallTo(() => _logic.GetChecklistForApplicationAsync(applicationId))
            .ReturnsLazily(() => list);

        //Act
        var result = await this._controller.GetChecklistForApplication(applicationId).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.GetChecklistForApplicationAsync(applicationId)).MustHaveHappenedOnceExactly();
        result.Should().NotBeNull().And.NotBeEmpty().And.HaveCount(5);
        result.Where(x => x.Retriggerable).Should().HaveCount(3);
        result.Where(x => x.Status == ApplicationChecklistEntryStatusId.FAILED).Should().ContainSingle();
    }

    [Fact]
    public async Task TriggerChecklist_ReturnsExpectedResult()
    {
        // Arrange
        var applicationId = _fixture.Create<Guid>();
        A.CallTo(() => _logic.TriggerChecklistAsync(applicationId, A<ApplicationChecklistEntryTypeId>._))
            .ReturnsLazily(() => Task.CompletedTask);
        
        // Act
        var result = await this._controller.TriggerChecklist(applicationId, ApplicationChecklistEntryTypeId.CLEARING_HOUSE);
        
        // Assert
        A.CallTo(() => _logic.TriggerChecklistAsync(applicationId, ApplicationChecklistEntryTypeId.CLEARING_HOUSE)).MustHaveHappenedOnceExactly();
        result.Should().BeOfType<NoContentResult>();
    }
}
