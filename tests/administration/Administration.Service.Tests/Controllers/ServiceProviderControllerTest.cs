/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
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

using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Org.CatenaX.Ng.Portal.Backend.Administration.Service.BusinessLogic;
using Org.CatenaX.Ng.Portal.Backend.Administration.Service.Controllers;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.CatenaX.Ng.Portal.Backend.Tests.Shared.Extensions;
using Xunit;

namespace Org.CatenaX.Ng.Portal.Backend.Administration.Service.Tests.Controllers;

public class ServiceProviderControllerTest
{
    private const string IamUserId = "4C1A6851-D4E7-4E10-A011-3732CD045E8A";
    private static readonly Guid CompanyId = new("4C1A6851-D4E7-4E10-A011-3732CD049999");
    private readonly IServiceProviderBusinessLogic _logic;
    private readonly ServiceProviderController _controller;

    public ServiceProviderControllerTest()
    {
        _logic = A.Fake<IServiceProviderBusinessLogic>();
        this._controller = new ServiceProviderController(_logic);
        _controller.AddControllerContextWithClaim(IamUserId);
    }

    [Fact]
    public async Task CreateServiceProviderCompanyDetail_WithValidData_ReturnsOk()
    {
        //Arrange
        var id = Guid.NewGuid();
        var data = new ServiceProviderDetailData("https://this-is-a-test.de");  
        A.CallTo(() => _logic.CreateServiceProviderCompanyDetailsAsync(data, IamUserId))
            .ReturnsLazily(() => id);

        //Act
        var result = await this._controller.CreateServiceProviderCompanyDetail(data).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.CreateServiceProviderCompanyDetailsAsync(data, IamUserId)).MustHaveHappenedOnceExactly();
        Assert.IsType<CreatedAtRouteResult>(result);
        result.Value.Should().Be(id);
    }
    
    [Fact]
    public async Task GetServiceProviderCompanyDetail_WithValidData_ReturnsOk()
    {
        //Arrange
        var id = Guid.NewGuid();
        var data = new ProviderDetailReturnData(id, CompanyId, "https://this-is-a-test.de");  
        A.CallTo(() => _logic.GetServiceProviderCompanyDetailsAsync(id, IamUserId))
            .ReturnsLazily(() => data);

        //Act
        var result = await this._controller.GetServiceProviderCompanyDetail(id).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.GetServiceProviderCompanyDetailsAsync(id, IamUserId)).MustHaveHappenedOnceExactly();
        Assert.IsType<ProviderDetailReturnData>(result);
        result.Id.Should().Be(id);
        result.CompanyId.Should().Be(CompanyId);
    }
    
    
    [Fact]
    public async Task UpdateServiceProviderCompanyDetail_WithValidData_ReturnsOk()
    {
        //Arrange
        var id = Guid.NewGuid();
        var data = new ServiceProviderDetailData("https://this-is-a-test.de");  
        A.CallTo(() => _logic.UpdateServiceProviderCompanyDetailsAsync(id, data, IamUserId))
            .ReturnsLazily(() => Task.CompletedTask);

        //Act
        var result = await this._controller.UpdateServiceProviderCompanyDetail(id, data).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.UpdateServiceProviderCompanyDetailsAsync(id, data, IamUserId)).MustHaveHappenedOnceExactly();
        Assert.IsType<NoContentResult>(result);
    }
}
