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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.Controllers;

public class ServiceAccountControllerTests
{
    private const string IamUserId = "4C1A6851-D4E7-4E10-A011-3732CD045E8A";
    private readonly IdentityData _identity = new(IamUserId, Guid.NewGuid(), IdentityTypeId.COMPANY_USER, Guid.NewGuid());
    private readonly IFixture _fixture;
    private readonly IServiceAccountBusinessLogic _logic;
    private readonly ServiceAccountController _controller;

    public ServiceAccountControllerTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _logic = A.Fake<IServiceAccountBusinessLogic>();
        this._controller = new ServiceAccountController(_logic);
        _controller.AddControllerContextWithClaim(IamUserId, _identity);
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
        A.CallTo(() => _logic.CreateOwnCompanyServiceAccountAsync(data, IamUserId))
            .ReturnsLazily(() => responseData);

        // Act
        var result = await _controller.ExecuteCompanyUserCreation(data).ConfigureAwait(false);

        // Assert
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
        A.CallTo(() => _logic.GetServiceAccountRolesAsync(_identity, null))
            .Returns(data.ToAsyncEnumerable());

        // Act
        var result = await _controller.GetServiceAccountRolesAsync().ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(5);
    }
}
