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
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.BusinessLogic;

public class ServiceAccountBusinessLogicTests
{
    private const string ValidBpn = "BPNL00000003CRHK";
    private const string ClientId = "Cl1-CX-Registration";
    private static readonly Guid UserRoleId1 = Guid.NewGuid();
    private static readonly Guid UserRoleId2 = Guid.NewGuid();
    private static readonly Guid ValidCompanyId = Guid.NewGuid();
    private static readonly Guid ValidConnectorId = Guid.NewGuid();
    private static readonly Guid ValidServiceAccountId = Guid.NewGuid();
    private static readonly Guid InactiveServiceAccount = Guid.NewGuid();
    private readonly IIdentityData _identity;
    private readonly IEnumerable<Guid> _userRoleIds = Enumerable.Repeat(Guid.NewGuid(), 1);
    private readonly IServiceAccountCreation _serviceAccountCreation;
    private readonly ICompanyRepository _companyRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserRolesRepository _userRolesRepository;
    private readonly IServiceAccountRepository _serviceAccountRepository;
    private readonly IConnectorsRepository _connectorsRepository;
    private readonly IProvisioningManager _provisioningManager;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IFixture _fixture;
    private readonly IOptions<ServiceAccountSettings> _options;
    private readonly IIdentityService _identityService;

    public ServiceAccountBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _companyRepository = A.Fake<ICompanyRepository>();
        _userRepository = A.Fake<IUserRepository>();
        _userRolesRepository = A.Fake<IUserRolesRepository>();
        _serviceAccountRepository = A.Fake<IServiceAccountRepository>();
        _connectorsRepository = A.Fake<IConnectorsRepository>();
        _provisioningManager = A.Fake<IProvisioningManager>();
        _portalRepositories = A.Fake<IPortalRepositories>();
        _serviceAccountCreation = A.Fake<IServiceAccountCreation>();

        _identity = A.Fake<IIdentityData>();
        _identityService = A.Fake<IIdentityService>();
        A.CallTo(() => _identity.IdentityId).Returns(Guid.NewGuid());
        A.CallTo(() => _identity.IdentityTypeId).Returns(IdentityTypeId.COMPANY_USER);
        A.CallTo(() => _identity.CompanyId).Returns(ValidCompanyId);
        A.CallTo(() => _identityService.IdentityData).Returns(_identity);

        _options = Options.Create(new ServiceAccountSettings
        {
            ClientId = ClientId
        });
    }

    #region CreateOwnCompanyServiceAccountAsync

    [Fact]
    public async Task CreateOwnCompanyServiceAccountAsync_WithValidInput_ReturnsCreatedConnectorData()
    {
        // Arrange
        SetupCreateOwnCompanyServiceAccount();
        var serviceAccountCreationInfos = new ServiceAccountCreationInfo("TheName", "Just a short description", IamClientAuthMethod.SECRET, Enumerable.Repeat(UserRoleId1, 1));
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, _serviceAccountCreation, _identityService);

        // Act
        var result = await sut.CreateOwnCompanyServiceAccountAsync(serviceAccountCreationInfos).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.IamClientAuthMethod.Should().Be(IamClientAuthMethod.SECRET);
    }

    [Fact]
    public async Task CreateOwnCompanyServiceAccountAsync_WithInvalidUser_NotFoundException()
    {
        // Arrange
        var identity = _fixture.Create<IIdentityData>();
        A.CallTo(() => _identityService.IdentityData).Returns(identity);
        SetupCreateOwnCompanyServiceAccount();
        var serviceAccountCreationInfos = new ServiceAccountCreationInfo("TheName", "Just a short description", IamClientAuthMethod.SECRET, Enumerable.Repeat(UserRoleId1, 1));
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, _serviceAccountCreation, _identityService);

        // Act
        async Task Act() => await sut.CreateOwnCompanyServiceAccountAsync(serviceAccountCreationInfos).ConfigureAwait(false);

        // Assert
        var exception = await Assert.ThrowsAsync<ConflictException>(Act);
        exception.Message.Should().Be(AdministrationServiceAccountErrors.SERVICE_COMPANY_NOT_EXIST_CONFLICT.ToString());
    }

    [Fact]
    public async Task CreateOwnCompanyServiceAccountAsync_WithEmptyName_ThrowsControllerArgumentException()
    {
        // Arrange
        SetupCreateOwnCompanyServiceAccount();
        var serviceAccountCreationInfos = new ServiceAccountCreationInfo(string.Empty, "Just a short description", IamClientAuthMethod.SECRET, Enumerable.Repeat(UserRoleId1, 1));
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, _serviceAccountCreation, _identityService);

        // Act
        async Task Act() => await sut.CreateOwnCompanyServiceAccountAsync(serviceAccountCreationInfos).ConfigureAwait(false);

        // Assert
        var exception = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        exception.Message.Should().Be(AdministrationServiceAccountErrors.SERVICE_NAME_EMPTY_ARGUMENT.ToString());
        exception.Parameters.Should().NotBeNull().And.Satisfy(
           x => x.Name == "name"
        );
    }

    [Fact]
    public async Task CreateOwnCompanyServiceAccountAsync_WithInvalidIamClientAuthMethod_ThrowsControllerArgumentException()
    {
        // Arrange
        SetupCreateOwnCompanyServiceAccount();
        var serviceAccountCreationInfos = new ServiceAccountCreationInfo("TheName", "Just a short description", IamClientAuthMethod.JWT, Enumerable.Repeat(UserRoleId1, 1));
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, _serviceAccountCreation, _identityService);

        // Act
        async Task Act() => await sut.CreateOwnCompanyServiceAccountAsync(serviceAccountCreationInfos).ConfigureAwait(false);

        // Assert
        var exception = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        exception.Message.Should().Be(AdministrationServiceAccountErrors.SERVICE_AUTH_SECRET_ARGUMENT.ToString());
        exception.Parameters.Should().NotBeNull().And.Satisfy(
            x => x.Name == "authenticationType"
        );
    }

    [Fact]
    public async Task CreateOwnCompanyServiceAccountAsync_WithInvalidUserRoleId_ThrowsControllerArgumentException()
    {
        // Arrange
        var wrongUserRoleId = Guid.NewGuid();
        SetupCreateOwnCompanyServiceAccount();
        var serviceAccountCreationInfos = new ServiceAccountCreationInfo("TheName", "Just a short description", IamClientAuthMethod.SECRET, new[] { UserRoleId1, wrongUserRoleId });
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, _serviceAccountCreation, _identityService);

        // Act
        async Task Act() => await sut.CreateOwnCompanyServiceAccountAsync(serviceAccountCreationInfos).ConfigureAwait(false);

        // Assert
        var exception = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        exception.Message.Should().Be(AdministrationServiceAccountErrors.SERVICE_ROLES_NOT_ASSIGN_ARGUMENT.ToString());
        exception.Parameters.Should().NotBeNull().And.Satisfy(
            x => x.Name == "unassignable",
            y => y.Name == "userRoleIds"
        );
    }

    #endregion

    #region GetOwnCompanyServiceAccountDetailsAsync

    [Fact]
    public async Task GetOwnCompanyServiceAccountDetailsAsync_WithValidInput_GetsAllData()
    {
        // Arrange
        SetupGetOwnCompanyServiceAccountDetails();
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, null!, _identityService);

        // Act
        var result = await sut.GetOwnCompanyServiceAccountDetailsAsync(ValidServiceAccountId).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.IamClientAuthMethod.Should().Be(IamClientAuthMethod.SECRET);
    }

    [Fact]
    public async Task GetOwnCompanyServiceAccountDetailsAsync_WithInvalidUser_NotFoundException()
    {
        // Arrange
        SetupGetOwnCompanyServiceAccountDetails();
        var invalidCompanyId = Guid.NewGuid();
        A.CallTo(() => _identity.CompanyId).Returns(invalidCompanyId);
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, null!, _identityService);

        // Act
        async Task Act() => await sut.GetOwnCompanyServiceAccountDetailsAsync(ValidServiceAccountId).ConfigureAwait(false);

        // Assert
        var exception = await Assert.ThrowsAsync<ConflictException>(Act);
        exception.Message.Should().Be(AdministrationServiceAccountErrors.SERVICE_ACCOUNT_NOT_CONFLICT.ToString());
    }

    [Fact]
    public async Task GetOwnCompanyServiceAccountDetailsAsync_WithInvalidServiceAccount_NotFoundException()
    {
        // Arrange
        SetupGetOwnCompanyServiceAccountDetails();
        var invalidServiceAccountId = Guid.NewGuid();
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, null!, _identityService);

        // Act
        async Task Act() => await sut.GetOwnCompanyServiceAccountDetailsAsync(invalidServiceAccountId).ConfigureAwait(false);

        // Assert
        var exception = await Assert.ThrowsAsync<ConflictException>(Act);
        exception.Message.Should().Be(AdministrationServiceAccountErrors.SERVICE_ACCOUNT_NOT_CONFLICT.ToString());
    }

    #endregion

    #region ResetOwnCompanyServiceAccountSecretAsync

    [Fact]
    public async Task ResetOwnCompanyServiceAccountSecretAsync_WithValidInput_GetsAllData()
    {
        // Arrange
        SetupResetOwnCompanyServiceAccountSecret();
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, null!, _identityService);

        // Act
        var result = await sut.ResetOwnCompanyServiceAccountSecretAsync(ValidServiceAccountId).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.IamClientAuthMethod.Should().Be(IamClientAuthMethod.SECRET);
    }

    [Fact]
    public async Task ResetOwnCompanyServiceAccountSecretAsync_WithInvalidUser_NotFoundException()
    {
        // Arrange
        SetupResetOwnCompanyServiceAccountSecret();
        var invalidUser = _fixture.Create<IIdentityData>();
        A.CallTo(() => _identityService.IdentityData).Returns(invalidUser);
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, null!, _identityService);

        // Act
        async Task Act() => await sut.ResetOwnCompanyServiceAccountSecretAsync(ValidServiceAccountId).ConfigureAwait(false);

        // Assert
        var exception = await Assert.ThrowsAsync<ConflictException>(Act);
        exception.Message.Should().Be(AdministrationServiceAccountErrors.SERVICE_ACCOUNT_NOT_CONFLICT.ToString());
    }

    [Fact]
    public async Task ResetOwnCompanyServiceAccountSecretAsync_WithInvalidServiceAccount_NotFoundException()
    {
        // Arrange
        SetupResetOwnCompanyServiceAccountSecret();
        var invalidServiceAccountId = Guid.NewGuid();
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, null!, _identityService);

        // Act
        async Task Act() => await sut.ResetOwnCompanyServiceAccountSecretAsync(invalidServiceAccountId).ConfigureAwait(false);

        // Assert
        var exception = await Assert.ThrowsAsync<ConflictException>(Act);
        exception.Message.Should().Be(AdministrationServiceAccountErrors.SERVICE_ACCOUNT_NOT_CONFLICT.ToString());
    }

    #endregion

    #region UpdateOwnCompanyServiceAccountDetailsAsync

    [Fact]
    public async Task UpdateOwnCompanyServiceAccountDetailsAsync_WithValidData_ReturnsExpected()
    {
        // Arrange
        SetupUpdateOwnCompanyServiceAccountDetails();
        var data = new ServiceAccountEditableDetails(ValidServiceAccountId, "new name", "changed description", IamClientAuthMethod.SECRET);
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, null!, _identityService);

        // Act
        var result = await sut.UpdateOwnCompanyServiceAccountDetailsAsync(ValidServiceAccountId, data).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("new name");
    }

    [Fact]
    public async Task UpdateOwnCompanyServiceAccountDetailsAsync_WithInvalidAuthMethod_ThrowsArgumentException()
    {
        // Arrange
        SetupUpdateOwnCompanyServiceAccountDetails();
        var data = new ServiceAccountEditableDetails(ValidServiceAccountId, "new name", "changed description", IamClientAuthMethod.JWT);
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, null!, _identityService);

        // Act
        async Task Act() => await sut.UpdateOwnCompanyServiceAccountDetailsAsync(ValidServiceAccountId, data).ConfigureAwait(false);

        // Assert
        var exception = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        exception.Parameters.Should().NotBeNull().And.Satisfy(
            x => x.Name == "authenticationType"
        );
    }

    [Fact]
    public async Task UpdateOwnCompanyServiceAccountDetailsAsync_WithDifferentServiceAccountIds_ThrowsArgumentException()
    {
        // Arrange
        SetupUpdateOwnCompanyServiceAccountDetails();
        var data = new ServiceAccountEditableDetails(ValidServiceAccountId, "new name", "changed description", IamClientAuthMethod.SECRET);
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, null!, _identityService);

        // Act
        async Task Act() => await sut.UpdateOwnCompanyServiceAccountDetailsAsync(Guid.NewGuid(), data).ConfigureAwait(false);

        // Assert
        var exception = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        exception.Parameters.Should().NotBeNull().And.Satisfy(
            x => x.Name == "serviceAccountId",
            y => y.Name == "serviceAccountDetailsServiceAccountId"
        );
    }

    [Fact]
    public async Task UpdateOwnCompanyServiceAccountDetailsAsync_WithNotExistingServiceAccount_ThrowsNotFoundException()
    {
        // Arrange
        SetupUpdateOwnCompanyServiceAccountDetails();
        var invalidServiceAccountId = Guid.NewGuid();
        A.CallTo(() => _serviceAccountRepository.GetOwnCompanyServiceAccountWithIamClientIdAsync(invalidServiceAccountId, ValidCompanyId))
            .Returns((CompanyServiceAccountWithRoleDataClientId?)null);
        var data = new ServiceAccountEditableDetails(invalidServiceAccountId, "new name", "changed description", IamClientAuthMethod.SECRET);
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, null!, _identityService);

        // Act
        async Task Act() => await sut.UpdateOwnCompanyServiceAccountDetailsAsync(invalidServiceAccountId, data).ConfigureAwait(false);

        // Assert
        var exception = await Assert.ThrowsAsync<ConflictException>(Act);
        exception.Message.Should().Be(AdministrationServiceAccountErrors.SERVICE_ACCOUNT_NOT_CONFLICT.ToString());
    }

    [Fact]
    public async Task UpdateOwnCompanyServiceAccountDetailsAsync_WithInactiveServiceAccount_ThrowsArgumentException()
    {
        // Arrange
        SetupUpdateOwnCompanyServiceAccountDetails();
        var inactive = _fixture.Build<CompanyServiceAccountWithRoleDataClientId>()
            .With(x => x.UserStatusId, UserStatusId.INACTIVE)
            .Create();
        A.CallTo(() => _serviceAccountRepository.GetOwnCompanyServiceAccountWithIamClientIdAsync(InactiveServiceAccount, ValidCompanyId))
            .Returns(inactive);
        var data = new ServiceAccountEditableDetails(InactiveServiceAccount, "new name", "changed description", IamClientAuthMethod.SECRET);
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, null!, _identityService);

        // Act
        async Task Act() => await sut.UpdateOwnCompanyServiceAccountDetailsAsync(InactiveServiceAccount, data).ConfigureAwait(false);

        // Assert
        var exception = await Assert.ThrowsAsync<ConflictException>(Act);
        exception.Message.Should().Be(AdministrationServiceAccountErrors.SERVICE_INACTIVE_CONFLICT.ToString());
    }

    #endregion

    #region GetOwnCompanyServiceAccountsDataAsync

    [Theory]
    [InlineData(UserStatusId.ACTIVE, false)]
    [InlineData(UserStatusId.INACTIVE, true)]
    public async Task GetOwnCompanyServiceAccountsDataAsync_GetsExpectedData(UserStatusId userStatusId, bool isUserInactive)
    {
        // Arrange
        var data = _fixture.CreateMany<CompanyServiceAccountData>(15);
        A.CallTo(() => _serviceAccountRepository.GetOwnCompanyServiceAccountsUntracked(ValidCompanyId, null, null, userStatusId))
            .Returns((int skip, int take) => Task.FromResult((Pagination.Source<CompanyServiceAccountData>?)new Pagination.Source<CompanyServiceAccountData>(data.Count(), data.Skip(skip).Take(take))));

        A.CallTo(() => _portalRepositories.GetInstance<IServiceAccountRepository>()).Returns(_serviceAccountRepository);
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, null!, _identityService);

        // Act
        var result = await sut.GetOwnCompanyServiceAccountsDataAsync(1, 10, null, null, isUserInactive).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.Content.Should().HaveCount(5);
    }

    #endregion

    #region DeleteOwnCompanyServiceAccountAsync

    [Fact]
    public async Task DeleteOwnCompanyServiceAccountAsync_WithNotExistingServiceAccount_ThrowsNotFoundException()
    {
        // Arrange
        var serviceAccountId = Guid.NewGuid();
        SetupDeleteOwnCompanyServiceAccount(false, false);

        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, null!, _identityService);

        // Act
        async Task Act() => await sut.DeleteOwnCompanyServiceAccountAsync(serviceAccountId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be(AdministrationServiceAccountErrors.SERVICE_ACCOUNT_NOT_CONFLICT.ToString());
    }

    [Fact]
    public async Task DeleteOwnCompanyServiceAccountAsync_WithExistingOfferSubscriptiom_ThrowsNotFoundException()
    {
        // Arrange
        SetupDeleteOwnCompanyServiceAccountForValidOfferSubscription();

        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, null!, _identityService);

        // Act
        async Task Act() => await sut.DeleteOwnCompanyServiceAccountAsync(ValidServiceAccountId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be(AdministrationServiceAccountErrors.SERVICE_USERID_ACTIVATION_ACTIVE_CONFLICT.ToString());
    }

    [Fact]
    public async Task DeleteOwnCompanyServiceAccountAsync_WithInvalidConnectorStatus_ThrowsConflictException()
    {
        // Arrange
        SetupDeleteOwnCompanyServiceAccountForInvalidConnectorStatus();

        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, null!, _identityService);

        // Act
        async Task Act() => await sut.DeleteOwnCompanyServiceAccountAsync(ValidServiceAccountId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be(AdministrationServiceAccountErrors.SERVICE_USERID_ACTIVATION_PENDING_CONFLICT.ToString());
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public async Task DeleteOwnCompanyServiceAccountAsync_WithoutClient_CallsExpected(bool withClient, bool withServiceAccount)
    {
        // Arrange
        var identity = _fixture.Build<Identity>()
            .With(x => x.Id, ValidServiceAccountId)
            .With(x => x.UserStatusId, UserStatusId.ACTIVE)
            .Create();
        var connector = _fixture.Build<Connector>()
            .With(x => x.CompanyServiceAccountId, ValidServiceAccountId)
            .Create();
        SetupDeleteOwnCompanyServiceAccount(withServiceAccount, withClient, connector, identity);
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, null!, _identityService);

        // Act
        await sut.DeleteOwnCompanyServiceAccountAsync(ValidServiceAccountId).ConfigureAwait(false);

        // Assert
        if (withClient)
        {
            A.CallTo(() => _provisioningManager.DeleteCentralClientAsync(ClientId)).MustHaveHappenedOnceExactly();
        }

        if (withServiceAccount)
        {
            A.CallTo(() => _connectorsRepository.AttachAndModifyConnector(ValidConnectorId, A<Action<Connector>>._, A<Action<Connector>>._)).MustHaveHappenedOnceExactly();
            connector.CompanyServiceAccountId.Should().BeNull();
        }

        identity.UserStatusId.Should().Be(UserStatusId.INACTIVE);
        A.CallTo(() => _userRolesRepository.DeleteCompanyUserAssignedRoles(A<IEnumerable<(Guid, Guid)>>.That.IsSameSequenceAs(_userRoleIds.Select(userRoleId => new ValueTuple<Guid, Guid>(ValidServiceAccountId, userRoleId))))).MustHaveHappenedOnceExactly();
    }

    #endregion

    #region GetServiceAccountRolesAsync

    [Fact]
    public async Task GetServiceAccountRolesAsync_GetsExpectedData()
    {
        // Arrange
        var data = _fixture.CreateMany<UserRoleWithDescription>(15);

        A.CallTo(() => _userRolesRepository.GetServiceAccountRolesAsync(A<Guid>._, A<string>._, A<string>._))
            .Returns(data.ToAsyncEnumerable());

        A.CallTo(() => _portalRepositories.GetInstance<IUserRolesRepository>()).Returns(_userRolesRepository);

        IServiceAccountBusinessLogic sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, null!, _identityService);

        // Act
        var result = await sut.GetServiceAccountRolesAsync(null).ToListAsync().ConfigureAwait(false);

        // Assert
        A.CallTo(() => _userRolesRepository.GetServiceAccountRolesAsync(_identity.CompanyId, ClientId, A<string>._)).MustHaveHappenedOnceExactly();

        result.Should().NotBeNull();
        result.Should().HaveCount(15);
        // Sonar fix -> Return value of pure method is not used
        result.Should().AllSatisfy(ur =>
        {
            data.Contains(ur).Should().BeTrue();
        });
    }

    #endregion

    #region Setup

    private void SetupCreateOwnCompanyServiceAccount()
    {
        A.CallTo(() => _companyRepository.GetBpnAndTechnicalUserRoleIds(ValidCompanyId, ClientId))
            .Returns((ValidBpn, new[] { UserRoleId1, UserRoleId2 }));
        A.CallTo(() => _companyRepository.GetBpnAndTechnicalUserRoleIds(A<Guid>.That.Not.Matches(x => x == ValidCompanyId), ClientId))
            .Returns(((string?, IEnumerable<Guid>))default);

        A.CallTo(() => _serviceAccountCreation.CreateServiceAccountAsync(A<ServiceAccountCreationInfo>._, A<Guid>.That.Matches(x => x == ValidCompanyId), A<IEnumerable<string>>._, CompanyServiceAccountTypeId.OWN, A<bool>._, true, null))
            .Returns((ClientId, new ServiceAccountData(ClientId, Guid.NewGuid().ToString(), new ClientAuthData(IamClientAuthMethod.SECRET)), Guid.NewGuid(), Enumerable.Empty<UserRoleData>()));

        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>()).Returns(_companyRepository);
    }

    private void SetupGetOwnCompanyServiceAccountDetails()
    {
        var authData = new ClientAuthData(IamClientAuthMethod.SECRET) { Secret = "topsecret" };
        SetupGetOwnCompanyServiceAccount();

        var internalId = Guid.NewGuid().ToString();

        A.CallTo(() => _provisioningManager.GetIdOfCentralClientAsync(A<string>._))
            .Returns(internalId);

        A.CallTo(() => _provisioningManager.GetCentralClientAuthDataAsync(A<string>._))
            .Returns(authData);

        A.CallTo(() => _portalRepositories.GetInstance<IServiceAccountRepository>()).Returns(_serviceAccountRepository);
    }

    private void SetupResetOwnCompanyServiceAccountSecret()
    {
        var authData = new ClientAuthData(IamClientAuthMethod.SECRET) { Secret = "topsecret" };
        SetupGetOwnCompanyServiceAccount();

        A.CallTo(() => _provisioningManager.ResetCentralClientAuthDataAsync(A<string>._))
            .Returns(authData);

        A.CallTo(() => _portalRepositories.GetInstance<IServiceAccountRepository>()).Returns(_serviceAccountRepository);
    }

    private void SetupUpdateOwnCompanyServiceAccountDetails()
    {
        var authData = new ClientAuthData(IamClientAuthMethod.SECRET) { Secret = "topsecret" };
        var data = _fixture.Build<CompanyServiceAccountWithRoleDataClientId>()
            .With(x => x.UserStatusId, UserStatusId.ACTIVE)
            .Create();
        A.CallTo(() => _serviceAccountRepository.GetOwnCompanyServiceAccountWithIamClientIdAsync(ValidServiceAccountId, ValidCompanyId))
            .Returns(data);
        A.CallTo(() => _serviceAccountRepository.GetOwnCompanyServiceAccountWithIamClientIdAsync(ValidServiceAccountId, A<Guid>.That.Not.Matches(x => x == ValidCompanyId)))
            .Returns((CompanyServiceAccountWithRoleDataClientId?)null);

        A.CallTo(() => _provisioningManager.ResetCentralClientAuthDataAsync(A<string>._))
            .Returns(authData);

        A.CallTo(() => _portalRepositories.GetInstance<IServiceAccountRepository>()).Returns(_serviceAccountRepository);
    }

    private void SetupGetOwnCompanyServiceAccount()
    {
        var data = _fixture.Create<CompanyServiceAccountDetailedData>();

        A.CallTo(() => _serviceAccountRepository.GetOwnCompanyServiceAccountDetailedDataUntrackedAsync(ValidServiceAccountId, ValidCompanyId))
            .Returns(data);
        A.CallTo(() => _serviceAccountRepository.GetOwnCompanyServiceAccountDetailedDataUntrackedAsync(
                A<Guid>.That.Not.Matches(x => x == ValidServiceAccountId), ValidCompanyId))
            .Returns((CompanyServiceAccountDetailedData?)null);
        A.CallTo(() => _serviceAccountRepository.GetOwnCompanyServiceAccountDetailedDataUntrackedAsync(ValidServiceAccountId, A<Guid>.That.Not.Matches(x => x == ValidCompanyId)))
            .Returns((CompanyServiceAccountDetailedData?)null);
    }

    private void SetupDeleteOwnCompanyServiceAccount(bool withServiceAccount, bool withClient, Connector? connector = null, Identity? identity = null)
    {
        A.CallTo(() => _serviceAccountRepository.GetOwnCompanyServiceAccountWithIamServiceAccountRolesAsync(ValidServiceAccountId, ValidCompanyId))
            .Returns((_userRoleIds, withServiceAccount ? ValidConnectorId : null, withClient ? ClientId : null, statusId: ConnectorStatusId.INACTIVE, OfferStatusId: OfferSubscriptionStatusId.PENDING));
        A.CallTo(() => _serviceAccountRepository.GetOwnCompanyServiceAccountWithIamServiceAccountRolesAsync(A<Guid>.That.Not.Matches(x => x == ValidServiceAccountId), A<Guid>._))
            .Returns(((IEnumerable<Guid>, Guid?, string?, ConnectorStatusId?, OfferSubscriptionStatusId?))default);

        if (identity != null)
        {
            A.CallTo(() => _userRepository.AttachAndModifyIdentity(ValidServiceAccountId, null, A<Action<Identity>>._))
                .Invokes((Guid _, Action<Identity>? _, Action<Identity> modify) =>
                {
                    modify.Invoke(identity);
                });
        }

        if (connector != null)
        {
            A.CallTo(() => _connectorsRepository.AttachAndModifyConnector(ValidConnectorId, A<Action<Connector>>._, A<Action<Connector>>._))
                .Invokes((Guid _, Action<Connector>? initialize, Action<Connector> modify) =>
                {
                    initialize?.Invoke(connector);
                    modify.Invoke(connector);
                });
        }

        A.CallTo(() => _portalRepositories.GetInstance<IServiceAccountRepository>()).Returns(_serviceAccountRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IConnectorsRepository>()).Returns(_connectorsRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRolesRepository>()).Returns(_userRolesRepository);
    }

    private void SetupDeleteOwnCompanyServiceAccountForInvalidConnectorStatus()
    {
        A.CallTo(() => _serviceAccountRepository.GetOwnCompanyServiceAccountWithIamServiceAccountRolesAsync(ValidServiceAccountId, ValidCompanyId))
            .Returns((_userRoleIds, null, null, statusId: ConnectorStatusId.ACTIVE, OfferStatusId: OfferSubscriptionStatusId.PENDING));
        A.CallTo(() => _serviceAccountRepository.GetOwnCompanyServiceAccountWithIamServiceAccountRolesAsync(A<Guid>.That.Not.Matches(x => x == ValidServiceAccountId), A<Guid>._))
            .Returns(((IEnumerable<Guid>, Guid?, string?, ConnectorStatusId?, OfferSubscriptionStatusId?))default);

        A.CallTo(() => _portalRepositories.GetInstance<IServiceAccountRepository>()).Returns(_serviceAccountRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRolesRepository>()).Returns(_userRolesRepository);
    }

    private void SetupDeleteOwnCompanyServiceAccountForValidOfferSubscription()
    {
        A.CallTo(() => _serviceAccountRepository.GetOwnCompanyServiceAccountWithIamServiceAccountRolesAsync(ValidServiceAccountId, ValidCompanyId))
            .Returns((_userRoleIds, null, null, statusId: ConnectorStatusId.INACTIVE, OfferSubscriptionStatusId.ACTIVE));
        A.CallTo(() => _serviceAccountRepository.GetOwnCompanyServiceAccountWithIamServiceAccountRolesAsync(A<Guid>.That.Not.Matches(x => x == ValidServiceAccountId), A<Guid>._))
            .Returns(((IEnumerable<Guid>, Guid?, string?, ConnectorStatusId?, OfferSubscriptionStatusId))default);

        A.CallTo(() => _portalRepositories.GetInstance<IServiceAccountRepository>()).Returns(_serviceAccountRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRolesRepository>()).Returns(_userRolesRepository);
    }

    #endregion
}
