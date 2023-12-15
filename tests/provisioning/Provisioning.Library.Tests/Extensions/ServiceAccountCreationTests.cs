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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;

namespace Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Tests.Extensions;

public class ServiceAccountCreationTests
{
    private const string Bpn = "CAXSDUMMYCATENAZZ";
    private readonly string _iamUserId = Guid.NewGuid().ToString();
    private readonly Guid _companyId = Guid.NewGuid();
    private readonly Guid _serviceAccountId = Guid.NewGuid();
    private readonly Guid _identityId = Guid.NewGuid();
    private readonly Guid _validUserRoleId = Guid.NewGuid();
    private readonly Guid _invalidUserRoleId = Guid.NewGuid();

    private readonly IServiceAccountRepository _serviceAccountRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserRolesRepository _userRolesRepository;

    private readonly IProvisioningManager _provisioningManager;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IProvisioningDBAccess _provisioningDbAccess;
    private readonly IServiceAccountCreation _sut;

    public ServiceAccountCreationTests()
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _serviceAccountRepository = A.Fake<IServiceAccountRepository>();
        _userRepository = A.Fake<IUserRepository>();
        _userRolesRepository = A.Fake<IUserRolesRepository>();

        _provisioningManager = A.Fake<IProvisioningManager>();
        _portalRepositories = A.Fake<IPortalRepositories>();
        _provisioningDbAccess = A.Fake<IProvisioningDBAccess>();

        var settings = new ServiceAccountCreationSettings
        {
            ServiceAccountClientPrefix = "sa"
        };

        A.CallTo(() => _portalRepositories.GetInstance<IServiceAccountRepository>()).Returns(_serviceAccountRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRolesRepository>()).Returns(_userRolesRepository);

        _sut = new ServiceAccountCreation(_provisioningManager, _portalRepositories, _provisioningDbAccess, Options.Create(settings));
    }

    [Fact]
    public async Task CreateServiceAccountAsync_WithInvalidRole_ThrowsNotFoundException()
    {
        // Arrange
        var creationData = new ServiceAccountCreationInfo("testName", "abc", IamClientAuthMethod.SECRET, new[] { _invalidUserRoleId });
        Setup();

        // Act
        async Task Act() => await _sut.CreateServiceAccountAsync(creationData, _companyId, Enumerable.Empty<string>(), CompanyServiceAccountTypeId.OWN, false, true).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"{_invalidUserRoleId} are not a valid UserRoleIds");
        A.CallTo(() => _provisioningManager.AddBpnAttributetoUserAsync(A<string>._, A<IEnumerable<string>>._)).MustNotHaveHappened();
        A.CallTo(() => _provisioningManager.AddProtocolMapperAsync(A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _userRolesRepository.DeleteCompanyUserAssignedRoles(A<IEnumerable<(Guid, Guid)>>._)).MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
    }

    [Fact]
    public async Task CreateServiceAccountAsync_WithValidData_ReturnsExpected()
    {
        // Arrange
        var serviceAccounts = new List<CompanyServiceAccount>();
        var identities = new List<Identity>();
        var creationData = new ServiceAccountCreationInfo("testName", "abc", IamClientAuthMethod.SECRET, new[] { _validUserRoleId });
        var bpns = new[]
        {
            Bpn
        };
        Setup(serviceAccounts, identities);

        // Act
        var result = await _sut.CreateServiceAccountAsync(creationData, _companyId, bpns, CompanyServiceAccountTypeId.OWN, false, true).ConfigureAwait(false);

        // Assert
        result.userRoleData.Should().ContainSingle(x => x.UserRoleId == _validUserRoleId && x.UserRoleText == "UserRole");
        result.serviceAccountData.InternalClientId.Should().Be("internal-sa1");
        result.serviceAccountData.IamUserId.Should().Be(_iamUserId);
        result.serviceAccountData.AuthData.IamClientAuthMethod.Should().Be(IamClientAuthMethod.SECRET);
        A.CallTo(() => _provisioningManager.AddBpnAttributetoUserAsync(_iamUserId, bpns)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.AddProtocolMapperAsync("internal-sa1")).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        serviceAccounts.Should().ContainSingle().Which.Should().Match<CompanyServiceAccount>(
            x => x.Name == "testName" &&
                 x.ClientClientId == "sa1");
        identities.Should().ContainSingle().Which.Should().Match<Identity>(
            x => x.CompanyId == _companyId &&
                 x.UserStatusId == UserStatusId.ACTIVE &&
                 x.IdentityTypeId == IdentityTypeId.COMPANY_SERVICE_ACCOUNT);
    }

    [Fact]
    public async Task CreateServiceAccountAsync_WithNameSetAndValidData_ReturnsExpected()
    {
        // Arrange
        var serviceAccounts = new List<CompanyServiceAccount>();
        var identities = new List<Identity>();
        var creationData = new ServiceAccountCreationInfo("testName", "abc", IamClientAuthMethod.SECRET, new[] { _validUserRoleId });
        var bpns = new[]
        {
            Bpn
        };
        Setup(serviceAccounts, identities);

        // Act
        var result = await _sut.CreateServiceAccountAsync(creationData, _companyId, bpns, CompanyServiceAccountTypeId.OWN, true, true).ConfigureAwait(false);

        // Assert
        result.userRoleData.Should().ContainSingle(x => x.UserRoleId == _validUserRoleId && x.UserRoleText == "UserRole");
        result.serviceAccountData.InternalClientId.Should().Be("internal-sa1");
        result.serviceAccountData.IamUserId.Should().Be(_iamUserId);
        result.serviceAccountData.AuthData.IamClientAuthMethod.Should().Be(IamClientAuthMethod.SECRET);
        A.CallTo(() => _provisioningManager.SetupCentralServiceAccountClientAsync(A<string>._, A<ClientConfigRolesData>.That.Matches(x => x.Name == "sa1-testName"), A<bool>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.AddBpnAttributetoUserAsync(_iamUserId, bpns)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.AddProtocolMapperAsync("internal-sa1")).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        serviceAccounts.Should().ContainSingle().Which.Should().Match<CompanyServiceAccount>(
            x => x.Name == "sa1-testName" &&
                 x.ClientClientId == "sa1");
        identities.Should().ContainSingle().Which.Should().Match<Identity>(
            x => x.CompanyId == _companyId &&
                 x.UserStatusId == UserStatusId.ACTIVE &&
                 x.IdentityTypeId == IdentityTypeId.COMPANY_SERVICE_ACCOUNT);
    }

    #region Setup

    private void Setup(ICollection<CompanyServiceAccount>? serviceAccounts = null, ICollection<Identity>? identities = null)
    {
        A.CallTo(() => _provisioningDbAccess.GetNextClientSequenceAsync())
            .Returns(1);

        A.CallTo(() => _provisioningManager.SetupCentralServiceAccountClientAsync(A<string>._, A<ClientConfigRolesData>._, A<bool>._))
            .Returns(new ServiceAccountData("internal-sa1", _iamUserId, new ClientAuthData(IamClientAuthMethod.SECRET)));

        A.CallTo(() => _userRepository.CreateIdentity(_companyId, A<UserStatusId>._, IdentityTypeId.COMPANY_SERVICE_ACCOUNT, A<Action<Identity>>._))
            .Invokes((Guid companyId, UserStatusId userStatusId, IdentityTypeId identityTypeId, Action<Identity>? setOptionalFields) =>
            {
                var identity = new Identity(Guid.NewGuid(), DateTimeOffset.UtcNow, companyId, userStatusId, identityTypeId);
                setOptionalFields?.Invoke(identity);
                identities?.Add(identity);
            })
            .Returns(new Identity(_identityId, default, default, default, default));
        A.CallTo(() => _serviceAccountRepository.CreateCompanyServiceAccount(_identityId, A<string>._, A<string>._, A<string>._, A<CompanyServiceAccountTypeId>._, A<Action<CompanyServiceAccount>>._))
            .Invokes((Guid identityId, string name, string description, string clientClientId, CompanyServiceAccountTypeId companyServiceAccountTypeId, Action<CompanyServiceAccount>? setOptionalParameters) =>
            {
                var sa = new CompanyServiceAccount(
                    identityId,
                    name,
                    description,
                    companyServiceAccountTypeId)
                {
                    ClientClientId = clientClientId
                };
                setOptionalParameters?.Invoke(sa);
                serviceAccounts?.Add(sa);
            })
            .Returns(new CompanyServiceAccount(_serviceAccountId, null!, null!, default));

        A.CallTo(() => _userRolesRepository.GetUserRoleDataUntrackedAsync(A<IEnumerable<Guid>>.That.Matches(x => x.Count(y => y == _validUserRoleId) == 1)))
            .Returns(new[] { new UserRoleData(_validUserRoleId, Guid.NewGuid().ToString(), "UserRole") }.ToAsyncEnumerable());
        A.CallTo(() => _userRolesRepository.GetUserRoleDataUntrackedAsync(A<IEnumerable<Guid>>.That.Matches(x => x.Count(y => y == _invalidUserRoleId) == 1)))
            .Returns(Enumerable.Empty<UserRoleData>().ToAsyncEnumerable());
    }

    #endregion
}
