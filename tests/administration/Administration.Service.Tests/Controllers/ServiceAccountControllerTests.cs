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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.Controllers;

public class ServiceAccountControllerTests
{
    private readonly IIdentityData _identity;
    private readonly IFixture _fixture;
    private readonly IServiceAccountBusinessLogic _logic;
    private readonly ServiceAccountController _controller;

    public ServiceAccountControllerTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _identity = A.Fake<IIdentityData>();
        A.CallTo(() => _identity.IdentityId).Returns(Guid.NewGuid());
        A.CallTo(() => _identity.IdentityTypeId).Returns(IdentityTypeId.COMPANY_USER);
        A.CallTo(() => _identity.CompanyId).Returns(Guid.NewGuid());
        _logic = A.Fake<IServiceAccountBusinessLogic>();
        _controller = new ServiceAccountController(_logic);
        _controller.AddControllerContextWithClaim(_identity);
    }

    [Fact]
    public async Task ExecuteCompanyUserCreation_CallsExpected()
    {
        // Arrange
        var serviceAccountId = Guid.NewGuid();
        var responseData = _fixture.Build<ServiceAccountDetails>()
            .With(x => x.ServiceAccountId, serviceAccountId)
            .Create();
        var data = _fixture.Create<ServiceAccountCreationInfo>();
        A.CallTo(() => _logic.CreateOwnCompanyServiceAccountAsync(A<ServiceAccountCreationInfo>._))
            .Returns(responseData);

        // Act
        var result = await _controller.ExecuteCompanyUserCreation(data);

        // Assert
        A.CallTo(() => _logic.CreateOwnCompanyServiceAccountAsync(data)).MustHaveHappenedOnceExactly();

        result.Should().NotBeNull();
        result.Should().BeOfType<CreatedAtRouteResult>();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeOfType<ServiceAccountDetails>();
        (result.Value as ServiceAccountDetails)?.ServiceAccountId.Should().Be(serviceAccountId);
    }

    [Fact]
    public async Task GetServiceAccountRolesAsync_CallsExpected()
    {
        // Arrange
        var data = _fixture.CreateMany<UserRoleWithDescription>(5);
        A.CallTo(() => _logic.GetServiceAccountRolesAsync(A<string?>._))
            .Returns(data.ToAsyncEnumerable());

        // Act
        var result = await _controller.GetServiceAccountRolesAsync().ToListAsync();

        // Assert
        A.CallTo(() => _logic.GetServiceAccountRolesAsync(null)).MustHaveHappenedOnceExactly();

        result.Should().NotBeNull();
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetServiceAccountDetails_CallsExpected()
    {
        // Arrange
        var serviceAcountId = _fixture.Create<Guid>();

        // Act
        await _controller.GetServiceAccountDetails(serviceAcountId);

        // Assert
        A.CallTo(() => _logic.GetOwnCompanyServiceAccountDetailsAsync(serviceAcountId)).MustHaveHappenedOnceExactly();

    }

    [Fact]
    public async Task GetServiceAccountsData_CallsExpected()
    {
        //Arrange
        var paginationResponse = new Pagination.Response<CompanyServiceAccountData>(new Pagination.Metadata(15, 1, 1, 15), _fixture.CreateMany<CompanyServiceAccountData>(5));
        A.CallTo(() => _logic.GetOwnCompanyServiceAccountsDataAsync(0, 15, null, null, true))
                  .Returns(paginationResponse);

        //Act
        var result = await _controller.GetServiceAccountsData(0, 15, null, null, true);

        //Assert
        A.CallTo(() => _logic.GetOwnCompanyServiceAccountsDataAsync(0, 15, null, null, true)).MustHaveHappenedOnceExactly();
        Assert.IsType<Pagination.Response<CompanyServiceAccountData>>(result);
        result.Content.Should().HaveCount(5);
    }
}
