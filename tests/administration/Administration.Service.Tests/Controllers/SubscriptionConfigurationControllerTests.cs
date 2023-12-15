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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.Controllers;

public class SubscriptionConfigurationControllerTests
{
    private static readonly Guid OfferSubscriptionId = new("4C1A6851-D4E7-4E10-A011-3732CD049999");
    private static readonly Guid CompanyId = new("4C1A6851-D4E7-4E10-A011-3732CD049999");
    private readonly IIdentityData _identity;
    private readonly ISubscriptionConfigurationBusinessLogic _logic;
    private readonly SubscriptionConfigurationController _controller;
    private readonly Fixture _fixture;

    public SubscriptionConfigurationControllerTests()
    {
        _fixture = new Fixture();
        _identity = A.Fake<IIdentityData>();
        A.CallTo(() => _identity.IdentityId).Returns(Guid.NewGuid());
        A.CallTo(() => _identity.IdentityTypeId).Returns(IdentityTypeId.COMPANY_USER);
        A.CallTo(() => _identity.CompanyId).Returns(Guid.NewGuid());
        _logic = A.Fake<ISubscriptionConfigurationBusinessLogic>();
        _controller = new SubscriptionConfigurationController(_logic);
        _controller.AddControllerContextWithClaim(_identity);
    }

    [Fact]
    public async Task GetProcessStepData_WithValidData_ReturnsExpected()
    {
        //Arrange
        var list = _fixture.CreateMany<ProcessStepData>(5);
        A.CallTo(() => _logic.GetProcessStepsForSubscription(OfferSubscriptionId))
            .Returns(list.ToAsyncEnumerable());

        //Act
        var result = await this._controller.GetProcessStepsForSubscription(OfferSubscriptionId).ToListAsync().ConfigureAwait(false);

        //Assert
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task RetriggerProvider_WithValidData_ReturnsNoContent()
    {
        //Act
        var result = await this._controller.RetriggerProvider(OfferSubscriptionId).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.RetriggerProvider(OfferSubscriptionId)).MustHaveHappenedOnceExactly();
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task RetriggerCreateClient_WithValidData_ReturnsNoContent()
    {
        //Act
        var result = await this._controller.RetriggerCreateClient(OfferSubscriptionId).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.RetriggerCreateClient(OfferSubscriptionId)).MustHaveHappenedOnceExactly();
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task RetriggerCreateTechnicalUser_WithValidData_ReturnsNoContent()
    {
        //Act
        var result = await this._controller.RetriggerCreateTechnicalUser(OfferSubscriptionId).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.RetriggerCreateTechnicalUser(OfferSubscriptionId)).MustHaveHappenedOnceExactly();
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task RetriggerProviderCallback_WithValidData_ReturnsNoContent()
    {
        //Act
        var result = await this._controller.RetriggerProviderCallback(OfferSubscriptionId).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.RetriggerProviderCallback(OfferSubscriptionId)).MustHaveHappenedOnceExactly();
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task SetCompanyDetail_WithValidData_ReturnsNoContent()
    {
        //Arrange
        var data = new ProviderDetailData("https://this-is-a-test.de", null);
        //Act
        var result = await this._controller.SetProviderCompanyDetail(data).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.SetProviderCompanyDetailsAsync(data)).MustHaveHappenedOnceExactly();
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task GetProviderCompanyDetail_WithValidData_ReturnsOk()
    {
        //Arrange
        var id = Guid.NewGuid();
        var data = new ProviderDetailReturnData(id, CompanyId, "https://this-is-a-test.de");
        A.CallTo(() => _logic.GetProviderCompanyDetailsAsync())
            .Returns(data);

        //Act
        var result = await this._controller.GetServiceProviderCompanyDetail().ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.GetProviderCompanyDetailsAsync()).MustHaveHappenedOnceExactly();
        result.Should().BeOfType<ProviderDetailReturnData>();
        result.Id.Should().Be(id);
        result.CompanyId.Should().Be(CompanyId);
    }
}
