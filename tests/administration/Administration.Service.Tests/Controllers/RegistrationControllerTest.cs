/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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

using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Controllers;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;
using Xunit;

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
    public async Task Test1()
    {
        //Arrange
        var id = new Guid("d90995fe-1241-4b8d-9f5c-f3909acc6383");
        A.CallTo(() => _logic.ApprovePartnerRequest(IamUserId, AccessToken, id, A<CancellationToken>._))
                  .Returns(true);

        //Act
        var result = await this._controller.ApprovePartnerRequest(id, CancellationToken.None).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.ApprovePartnerRequest(IamUserId, AccessToken, id, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        Assert.IsType<bool>(result);
        Assert.True(result);
    }

    [Fact]
    public async Task Test2()
    {
        //Arrange
        A.CallTo(() => _logic.ApprovePartnerRequest(IamUserId, AccessToken, Guid.Empty, A<CancellationToken>._))
                  .Returns(false);

        //Act
        var result = await this._controller.ApprovePartnerRequest(Guid.Empty, CancellationToken.None).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.ApprovePartnerRequest(IamUserId, AccessToken, Guid.Empty, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        Assert.IsType<bool>(result);
        Assert.False(result);
    }

    [Fact]
    public async Task GetCompanyApplicationDetailsAsync_ReturnsCompanyApplicationDetails()
    {
         //Arrange
        var paginationResponse = new Pagination.Response<CompanyApplicationDetails>(new Pagination.Metadata(15, 1, 1, 15), _fixture.CreateMany<CompanyApplicationDetails>(5));
        A.CallTo(() => _logic.GetCompanyApplicationDetailsAsync(0, 15,null))
                  .Returns(paginationResponse);

        //Act
        var result = await this._controller.GetApplicationDetailsAsync(0, 15,null).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.GetCompanyApplicationDetailsAsync(0, 15,null)).MustHaveHappenedOnceExactly();
        Assert.IsType<Pagination.Response<CompanyApplicationDetails>>(result);
        result.Content.Should().HaveCount(5);
    }

    [Fact]
    public async Task TriggerBpnDataPush_ReturnsNoContent()
    {
        //Arrange
        var applicationId = _fixture.Create<Guid>();
        A.CallTo(() => _logic.TriggerBpnDataPushAsync(IamUserId, applicationId, CancellationToken.None))
            .ReturnsLazily(() => Task.CompletedTask);

        //Act
        var result = await this._controller.TriggerBpnDataPush(applicationId, CancellationToken.None).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.TriggerBpnDataPushAsync(IamUserId, applicationId, CancellationToken.None)).MustHaveHappenedOnceExactly();
        Assert.IsType<NoContentResult>(result);
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
}
