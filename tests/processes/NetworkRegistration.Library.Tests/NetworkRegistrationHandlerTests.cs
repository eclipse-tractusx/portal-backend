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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.NetworkRegistration.Library.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.NetworkRegistration.Library.Tests;

public class NetworkRegistrationHandlerTests
{
    private static readonly Guid NetworkRegistrationId = Guid.NewGuid();
    private static readonly Guid UserRoleIds = Guid.NewGuid();

    private readonly IUserProvisioningService _userProvisioningService;

    private readonly IProvisioningManager _provisioningManger;
    private readonly IUserRepository _userRepository;

    private readonly NetworkRegistrationHandler _sut;
    private readonly NetworkRegistrationProcessSettings _settings;

    public NetworkRegistrationHandlerTests()
    {
        var portalRepositories = A.Fake<IPortalRepositories>();
        _userRepository = A.Fake<IUserRepository>();

        _userProvisioningService = A.Fake<IUserProvisioningService>();
        _provisioningManger = A.Fake<IProvisioningManager>();

        _settings = new NetworkRegistrationProcessSettings
        {
            InitialRoles = Enumerable.Repeat(new UserRoleConfig("cl1", Enumerable.Repeat("Company Admin", 1)), 1)
        };
        var options = A.Fake<IOptions<NetworkRegistrationProcessSettings>>();

        A.CallTo(() => options.Value).Returns(_settings);
        A.CallTo(() => portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);

        _sut = new NetworkRegistrationHandler(portalRepositories, _userProvisioningService, _provisioningManger, options);
    }

    [Theory]
    [InlineData(null, "stark", "tony@stark.com")]
    [InlineData("tony", null, "tony@stark.com")]
    [InlineData("tony", "stark", null)]
    public async Task SynchronizeUser_WithUserDataNull_ThrowsConflictException(string? firstName, string? lastName, string? email)
    {
        // Arrange
        var user1Id = Guid.NewGuid();
        var user1 = new CompanyUserIdentityProviderProcessData(user1Id, firstName, lastName, email,
            "123456789", "Test Company", "BPNL00000001TEST",
            Enumerable.Repeat(new ProviderLinkData("ironman", "idp1", "id1234"), 1));

        A.CallTo(() => _userRepository.GetUserAssignedIdentityProviderForNetworkRegistration(NetworkRegistrationId))
            .Returns(new List<CompanyUserIdentityProviderProcessData>
            {
                user1,
            }.ToAsyncEnumerable());
        A.CallTo(() => _userProvisioningService.GetRoleDatas(A<IEnumerable<UserRoleConfig>>._))
            .Returns(Enumerable.Repeat(new UserRoleData(UserRoleIds, "cl1", "Company Admin"), 1).ToAsyncEnumerable());

        // Act
        async Task Act() => await _sut.SynchronizeUser(NetworkRegistrationId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"Firstname, Lastname & Email of CompanyUser {user1Id} must not be null here");
    }

    [Fact]
    public async Task SynchronizeUser_WithValidData_ReturnsExpected()
    {
        // Arrange
        var user1Id = Guid.NewGuid().ToString();
        var user2Id = Guid.NewGuid().ToString();
        var user1 = new CompanyUserIdentityProviderProcessData(Guid.NewGuid(), "tony", "stark", "tony@stark.com",
            "123456789", "Test Company", "BPNL00000001TEST",
            Enumerable.Repeat(new ProviderLinkData("ironman", "idp1", "id1234"), 1));
        var user2 = new CompanyUserIdentityProviderProcessData(Guid.NewGuid(), "steven", "strange",
            "steven@strange.com", "987654321", "Test Company", "BPNL00000001TEST",
            Enumerable.Repeat(new ProviderLinkData("drstrange", "idp1", "id9876"), 1));

        A.CallTo(() => _userRepository.GetUserAssignedIdentityProviderForNetworkRegistration(NetworkRegistrationId))
            .Returns(new List<CompanyUserIdentityProviderProcessData>
            {
                user1,
                user2
            }.ToAsyncEnumerable());
        A.CallTo(() => _userProvisioningService.CreateCentralUserWithProviderLinks(user2.CompanyUserId, A<UserCreationRoleDataIdpInfo>._, A<string>._, A<string>._, A<IEnumerable<IdentityProviderLink>>._))
            .Returns(user2Id);
        A.CallTo(() => _userProvisioningService.GetRoleDatas(A<IEnumerable<UserRoleConfig>>._))
            .Returns(Enumerable.Repeat(new UserRoleData(UserRoleIds, "cl1", "Company Admin"), 1).ToAsyncEnumerable());
        A.CallTo(() => _provisioningManger.GetUserByUserName(user1.CompanyUserId.ToString())).Returns(user1Id);
        A.CallTo(() => _provisioningManger.GetUserByUserName(user2.CompanyUserId.ToString())).Returns((string?)null);

        // Act
        var result = await _sut.SynchronizeUser(NetworkRegistrationId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _userProvisioningService.CreateCentralUserWithProviderLinks(user2.CompanyUserId, A<UserCreationRoleDataIdpInfo>._, A<string>._, A<string>._, A<IEnumerable<IdentityProviderLink>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _userRepository.AttachAndModifyIdentity(user1.CompanyUserId, A<Action<Identity>>._, A<Action<Identity>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _userRepository.AttachAndModifyIdentity(user2.CompanyUserId, A<Action<Identity>>._, A<Action<Identity>>._))
            .MustHaveHappenedOnceExactly();
        result.modified.Should().BeFalse();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().BeNull();
    }
}
