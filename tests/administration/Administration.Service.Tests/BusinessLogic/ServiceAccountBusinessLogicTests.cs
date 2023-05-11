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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
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
    private static readonly string ValidAdminId = Guid.NewGuid().ToString();
    private readonly IEnumerable<Guid> _userRoleIds = Enumerable.Repeat(Guid.NewGuid(), 1);
    private readonly IServiceAccountCreation _serviceAccountCreation;
    private readonly IUserRepository _userRepository;
    private readonly IUserRolesRepository _userRolesRepository;
    private readonly IServiceAccountRepository _serviceAccountRepository;
    private readonly IConnectorsRepository _connectorsRepository;
    private readonly IProvisioningManager _provisioningManager;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IFixture _fixture;
    private readonly IOptions<ServiceAccountSettings> _options;

    public ServiceAccountBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());  

        _userRepository = A.Fake<IUserRepository>();
        _userRolesRepository = A.Fake<IUserRolesRepository>();
        _serviceAccountRepository = A.Fake<IServiceAccountRepository>();
        _connectorsRepository = A.Fake<IConnectorsRepository>();
        _provisioningManager = A.Fake<IProvisioningManager>();
        _portalRepositories = A.Fake<IPortalRepositories>();
        _serviceAccountCreation = A.Fake<IServiceAccountCreation>();

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
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, _serviceAccountCreation);

        // Act
        var result = await sut.CreateOwnCompanyServiceAccountAsync(serviceAccountCreationInfos, ValidAdminId).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.IamClientAuthMethod.Should().Be(IamClientAuthMethod.SECRET);
    }
    
    [Fact]
    public async Task CreateOwnCompanyServiceAccountAsync_WithInvalidUser_NotFoundException()
    {
        // Arrange
        SetupCreateOwnCompanyServiceAccount();
        var serviceAccountCreationInfos = new ServiceAccountCreationInfo("TheName", "Just a short description", IamClientAuthMethod.SECRET, Enumerable.Repeat(UserRoleId1, 1));
        var invalidUserId = Guid.NewGuid().ToString();
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, _serviceAccountCreation);

        // Act
        async Task Act() => await sut.CreateOwnCompanyServiceAccountAsync(serviceAccountCreationInfos, invalidUserId).ConfigureAwait(false);

        // Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(Act);
        exception.Message.Should().Be($"user {invalidUserId} is not associated with any company");
    }
    
    [Fact]
    public async Task CreateOwnCompanyServiceAccountAsync_WithEmptyName_ThrowsControllerArgumentException()
    {
        // Arrange
        SetupCreateOwnCompanyServiceAccount();
        var serviceAccountCreationInfos = new ServiceAccountCreationInfo(string.Empty, "Just a short description", IamClientAuthMethod.SECRET, Enumerable.Repeat(UserRoleId1, 1));
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, _serviceAccountCreation);

        // Act
        async Task Act() => await sut.CreateOwnCompanyServiceAccountAsync(serviceAccountCreationInfos, ValidAdminId).ConfigureAwait(false);

        // Assert
        var exception = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        exception.Message.Should().Be("name must not be empty (Parameter 'name')");
        exception.ParamName.Should().Be("name");
    }
    
    [Fact]
    public async Task CreateOwnCompanyServiceAccountAsync_WithInvalidIamClientAuthMethod_ThrowsControllerArgumentException()
    {
        // Arrange
        SetupCreateOwnCompanyServiceAccount();
        var serviceAccountCreationInfos = new ServiceAccountCreationInfo("TheName", "Just a short description", IamClientAuthMethod.JWT, Enumerable.Repeat(UserRoleId1, 1));
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, _serviceAccountCreation);

        // Act
        async Task Act() => await sut.CreateOwnCompanyServiceAccountAsync(serviceAccountCreationInfos, ValidAdminId).ConfigureAwait(false);

        // Assert
        var exception = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        exception.Message.Should().Be("other authenticationType values than SECRET are not supported yet (Parameter 'authenticationType')");
        exception.ParamName.Should().Be("authenticationType");
    }

    [Fact]
    public async Task CreateOwnCompanyServiceAccountAsync_WithInvalidUserRoleId_ThrowsControllerArgumentException()
    {
        // Arrange
        var wrongUserRoleId = Guid.NewGuid();
        SetupCreateOwnCompanyServiceAccount();
        var serviceAccountCreationInfos = new ServiceAccountCreationInfo("TheName", "Just a short description", IamClientAuthMethod.SECRET, new []{ UserRoleId1, wrongUserRoleId });
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, _serviceAccountCreation);

        // Act
        async Task Act() => await sut.CreateOwnCompanyServiceAccountAsync(serviceAccountCreationInfos, ValidAdminId).ConfigureAwait(false);

        // Assert
        var exception = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        exception.Message.Should().Be($"The roles {wrongUserRoleId} are not assignable to a service account (Parameter 'userRoleIds')");
        exception.ParamName.Should().Be("userRoleIds");
    }

    #endregion

    #region GetOwnCompanyServiceAccountDetailsAsync
    
    [Fact]
    public async Task GetOwnCompanyServiceAccountDetailsAsync_WithValidInput_GetsAllData()
    {
        // Arrange
        SetupGetOwnCompanyServiceAccountDetails();
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, null!);

        // Act
        var result = await sut.GetOwnCompanyServiceAccountDetailsAsync(ValidServiceAccountId, ValidAdminId).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.IamClientAuthMethod.Should().Be(IamClientAuthMethod.SECRET);
    }
    
    [Fact]
    public async Task GetOwnCompanyServiceAccountDetailsAsync_WithInvalidUser_NotFoundException()
    {
        // Arrange
        SetupGetOwnCompanyServiceAccountDetails();
        var invalidUserId = Guid.NewGuid().ToString();
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, null!);

        // Act
        async Task Act() => await sut.GetOwnCompanyServiceAccountDetailsAsync(ValidServiceAccountId, invalidUserId).ConfigureAwait(false);

        // Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(Act);
        exception.Message.Should().Be($"serviceAccount {ValidServiceAccountId} not found in company of {invalidUserId}");
    }

    [Fact]
    public async Task GetOwnCompanyServiceAccountDetailsAsync_WithInvalidServiceAccount_NotFoundException()
    {
        // Arrange
        SetupGetOwnCompanyServiceAccountDetails();
        var invalidServiceAccountId = Guid.NewGuid();
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, null!);

        // Act
        async Task Act() => await sut.GetOwnCompanyServiceAccountDetailsAsync(invalidServiceAccountId, ValidAdminId).ConfigureAwait(false);

        // Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(Act);
        exception.Message.Should().Be($"serviceAccount {invalidServiceAccountId} not found in company of {ValidAdminId}");
    }

    #endregion
    
    #region ResetOwnCompanyServiceAccountSecretAsync
    
    [Fact]
    public async Task ResetOwnCompanyServiceAccountSecretAsync_WithValidInput_GetsAllData()
    {
        // Arrange
        SetupResetOwnCompanyServiceAccountSecret();
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, null!);

        // Act
        var result = await sut.ResetOwnCompanyServiceAccountSecretAsync(ValidServiceAccountId, ValidAdminId).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.IamClientAuthMethod.Should().Be(IamClientAuthMethod.SECRET);
    }
    
    [Fact]
    public async Task ResetOwnCompanyServiceAccountSecretAsync_WithInvalidUser_NotFoundException()
    {
        // Arrange
        SetupResetOwnCompanyServiceAccountSecret();
        var invalidUserId = Guid.NewGuid().ToString();
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, null!);

        // Act
        async Task Act() => await sut.ResetOwnCompanyServiceAccountSecretAsync(ValidServiceAccountId, invalidUserId).ConfigureAwait(false);

        // Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(Act);
        exception.Message.Should().Be($"serviceAccount {ValidServiceAccountId} not found in company of {invalidUserId}");
    }

    [Fact]
    public async Task ResetOwnCompanyServiceAccountSecretAsync_WithInvalidServiceAccount_NotFoundException()
    {
        // Arrange
        SetupResetOwnCompanyServiceAccountSecret();
        var invalidServiceAccountId = Guid.NewGuid();
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, null!);

        // Act
        async Task Act() => await sut.ResetOwnCompanyServiceAccountSecretAsync(invalidServiceAccountId, ValidAdminId).ConfigureAwait(false);

        // Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(Act);
        exception.Message.Should().Be($"serviceAccount {invalidServiceAccountId} not found in company of {ValidAdminId}");
    }

    #endregion
    
    #region UpdateOwnCompanyServiceAccountDetailsAsync
    
    [Fact]
    public async Task UpdateOwnCompanyServiceAccountDetailsAsync_WithValidData_ReturnsExpected()
    {
        // Arrange
        SetupUpdateOwnCompanyServiceAccountDetails();
        var data = new ServiceAccountEditableDetails(ValidServiceAccountId, "new name", "changed description", IamClientAuthMethod.SECRET); 
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, null!);

        // Act
        var result = await sut.UpdateOwnCompanyServiceAccountDetailsAsync(ValidServiceAccountId, data, ValidAdminId).ConfigureAwait(false);

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
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, null!);

        // Act
        async Task Act() => await sut.UpdateOwnCompanyServiceAccountDetailsAsync(ValidServiceAccountId, data, ValidAdminId).ConfigureAwait(false);

        // Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(Act);
        exception.ParamName.Should().Be("authenticationType");
    }

    [Fact]
    public async Task UpdateOwnCompanyServiceAccountDetailsAsync_WithDifferentServiceAccountIds_ThrowsArgumentException()
    {
        // Arrange
        SetupUpdateOwnCompanyServiceAccountDetails();
        var data = new ServiceAccountEditableDetails(ValidServiceAccountId, "new name", "changed description", IamClientAuthMethod.SECRET); 
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, null!);

        // Act
        async Task Act() => await sut.UpdateOwnCompanyServiceAccountDetailsAsync(Guid.NewGuid(), data, ValidAdminId).ConfigureAwait(false);

        // Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(Act);
        exception.ParamName.Should().Be("serviceAccountId");
    }

    [Fact]
    public async Task UpdateOwnCompanyServiceAccountDetailsAsync_WithNotExistingServiceAccount_ThrowsNotFoundException()
    {
        // Arrange
        SetupUpdateOwnCompanyServiceAccountDetails();
        var invalidServiceAccountId = Guid.NewGuid();
        A.CallTo(() => _serviceAccountRepository.GetOwnCompanyServiceAccountWithIamClientIdAsync(invalidServiceAccountId, A<string>.That.Matches(x => x == ValidAdminId)))
            .Returns((CompanyServiceAccountWithRoleDataClientId?)null);
        var data = new ServiceAccountEditableDetails(invalidServiceAccountId, "new name", "changed description", IamClientAuthMethod.SECRET); 
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, null!);

        // Act
        async Task Act() => await sut.UpdateOwnCompanyServiceAccountDetailsAsync(invalidServiceAccountId, data, ValidAdminId).ConfigureAwait(false);

        // Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(Act);
        exception.Message.Should().Be($"serviceAccount {invalidServiceAccountId} not found in company of {ValidAdminId}");
    }

    [Fact]
    public async Task UpdateOwnCompanyServiceAccountDetailsAsync_WithInactiveServiceAccount_ThrowsArgumentException()
    {
        // Arrange
        SetupUpdateOwnCompanyServiceAccountDetails();
        var inactive = _fixture.Build<CompanyServiceAccountWithRoleDataClientId>()
            .With(x => x.CompanyServiceAccountStatusId, CompanyServiceAccountStatusId.INACTIVE)
            .Create();
        A.CallTo(() => _serviceAccountRepository.GetOwnCompanyServiceAccountWithIamClientIdAsync(InactiveServiceAccount, A<string>.That.Matches(x => x == ValidAdminId)))
            .Returns(inactive);
        var data = new ServiceAccountEditableDetails(InactiveServiceAccount, "new name", "changed description", IamClientAuthMethod.SECRET); 
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, null!);

        // Act
        async Task Act() => await sut.UpdateOwnCompanyServiceAccountDetailsAsync(InactiveServiceAccount, data, ValidAdminId).ConfigureAwait(false);

        // Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(Act);
        exception.Message.Should().Be($"serviceAccount {InactiveServiceAccount} is already INACTIVE");
    }

    #endregion

    #region GetOwnCompanyServiceAccountsDataAsync
    
    [Fact]
    public async Task GetOwnCompanyServiceAccountsDataAsync_GetsExpectedData()
    {
        // Arrange
        var data = _fixture.CreateMany<CompanyServiceAccountData>(15);
        A.CallTo(() => _serviceAccountRepository.GetOwnCompanyServiceAccountsUntracked(A<string>.That.Matches(x => x == ValidAdminId)))
            .Returns((int skip, int take) => Task.FromResult((Pagination.Source<CompanyServiceAccountData>?)new Pagination.Source<CompanyServiceAccountData>(data.Count(), data.Skip(skip).Take(take))));
        
        A.CallTo(() => _portalRepositories.GetInstance<IServiceAccountRepository>()).Returns(_serviceAccountRepository);
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, null!);

        // Act
        var result = await sut.GetOwnCompanyServiceAccountsDataAsync(1, 10, ValidAdminId).ConfigureAwait(false);

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

        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, null!);
        
        // Act
        async Task Act() => await sut.DeleteOwnCompanyServiceAccountAsync(serviceAccountId, ValidAdminId).ConfigureAwait(false);
        
        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"serviceAccount {serviceAccountId} not found in company of user {ValidAdminId}");
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public async Task DeleteOwnCompanyServiceAccountAsync_WithoutClient_CallsExpected(bool withClient, bool withServiceAccount)
    {
        // Arrange
        var serviceAccount = _fixture.Build<CompanyServiceAccount>()
            .With(x => x.CompanyServiceAccountStatusId, CompanyServiceAccountStatusId.ACTIVE)
            .Create();
        var connector = _fixture.Build<Connector>()
            .With(x => x.CompanyServiceAccountId, ValidServiceAccountId)
            .Create();
        SetupDeleteOwnCompanyServiceAccount(withServiceAccount, withClient, connector, serviceAccount);
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, null!);

        // Act
        await sut.DeleteOwnCompanyServiceAccountAsync(ValidServiceAccountId, ValidAdminId).ConfigureAwait(false);
        
        // Assert
        if (withClient)
        {
            A.CallTo(() => _provisioningManager.DeleteCentralClientAsync(ClientId)).MustHaveHappenedOnceExactly(); 
            A.CallTo(() => _serviceAccountRepository.RemoveIamServiceAccount(ClientId)).MustHaveHappenedOnceExactly();
        }

        if (withServiceAccount)
        {
            A.CallTo(() => _connectorsRepository.AttachAndModifyConnector(ValidConnectorId, A<Action<Connector>>._, A<Action<Connector>>._)).MustHaveHappenedOnceExactly();
            connector.CompanyServiceAccountId.Should().BeNull();
        }

        serviceAccount.CompanyServiceAccountStatusId.Should().Be(CompanyServiceAccountStatusId.INACTIVE);
        A.CallTo(() => _serviceAccountRepository.RemoveCompanyServiceAccountAssignedRoles(A<IEnumerable<(Guid,Guid)>>.That.IsSameSequenceAs(_userRoleIds.Select(userRoleId => new ValueTuple<Guid,Guid>(ValidServiceAccountId, userRoleId))))).MustHaveHappenedOnceExactly();
    }

    #endregion

    #region GetServiceAccountRolesAsync
    
    [Fact]
    public async Task GetServiceAccountRolesAsync_GetsExpectedData()
    {
        // Arrange
        var data = _fixture.CreateMany<UserRoleWithDescription>(15);
        A.CallTo(() => _userRolesRepository.GetServiceAccountRolesAsync(ValidAdminId, ClientId, A<string>._))
            .Returns(data.ToAsyncEnumerable());
        
        A.CallTo(() => _portalRepositories.GetInstance<IUserRolesRepository>()).Returns(_userRolesRepository);
        IServiceAccountBusinessLogic sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, null!);

        // Act
        var result = await sut.GetServiceAccountRolesAsync(ValidAdminId, null).ToListAsync().ConfigureAwait(false);

        // Assert
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
        A.CallTo(() => _userRepository.GetCompanyIdAndBpnRolesForIamUserUntrackedAsync(ValidAdminId, ClientId))
            .Returns((ValidCompanyId, ValidBpn, new []{ UserRoleId1, UserRoleId2 }));
        A.CallTo(() => _userRepository.GetCompanyIdAndBpnRolesForIamUserUntrackedAsync(A<string>.That.Not.Matches(x => x == ValidAdminId), ClientId))
            .Returns(((Guid, string, IEnumerable<Guid>))default);
        
        A.CallTo(() => _serviceAccountCreation.CreateServiceAccountAsync(A<ServiceAccountCreationInfo>._, A<Guid>.That.Matches(x => x == ValidCompanyId), A<IEnumerable<string>>._, CompanyServiceAccountTypeId.OWN, A<bool>._, null))
            .Returns((ClientId, new ServiceAccountData(ClientId, Guid.NewGuid().ToString(), new ClientAuthData(IamClientAuthMethod.SECRET)), Guid.NewGuid(), Enumerable.Empty<UserRoleData>()));

        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
    }

    private void SetupGetOwnCompanyServiceAccountDetails()
    {
        var authData = new ClientAuthData(IamClientAuthMethod.SECRET) { Secret = "topsecret" };
        SetupGetOwnCompanyServiceAccount();

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
            .With(x => x.CompanyServiceAccountStatusId, CompanyServiceAccountStatusId.ACTIVE)
            .Create();
        A.CallTo(() => _serviceAccountRepository.GetOwnCompanyServiceAccountWithIamClientIdAsync(ValidServiceAccountId, A<string>.That.Matches(x => x == ValidAdminId)))
            .Returns(data);
        A.CallTo(() => _serviceAccountRepository.GetOwnCompanyServiceAccountWithIamClientIdAsync(ValidServiceAccountId, A<string>.That.Not.Matches(x => x == ValidAdminId)))
            .Returns((CompanyServiceAccountWithRoleDataClientId?)null);

        A.CallTo(() => _provisioningManager.ResetCentralClientAuthDataAsync(A<string>._))
            .Returns(authData);

        A.CallTo(() => _portalRepositories.GetInstance<IServiceAccountRepository>()).Returns(_serviceAccountRepository);
    }

    private void SetupGetOwnCompanyServiceAccount()
    {
        var data = _fixture.Create<CompanyServiceAccountDetailedData>();
        A.CallTo(() => _serviceAccountRepository.GetOwnCompanyServiceAccountDetailedDataUntrackedAsync(
                A<Guid>.That.Matches(x => x == ValidServiceAccountId), A<string>.That.Matches(x => x == ValidAdminId)))
            .Returns(data);
        A.CallTo(() => _serviceAccountRepository.GetOwnCompanyServiceAccountDetailedDataUntrackedAsync(
                A<Guid>.That.Not.Matches(x => x == ValidServiceAccountId), A<string>.That.Matches(x => x == ValidAdminId)))
            .Returns((CompanyServiceAccountDetailedData?) null);
        A.CallTo(() => _serviceAccountRepository.GetOwnCompanyServiceAccountDetailedDataUntrackedAsync(
                A<Guid>.That.Matches(x => x == ValidServiceAccountId), A<string>.That.Not.Matches(x => x == ValidAdminId)))
            .Returns((CompanyServiceAccountDetailedData?) null);
    }

    private void SetupDeleteOwnCompanyServiceAccount(bool withServiceAccount, bool withClient, Connector? connector = null, CompanyServiceAccount? companyServiceAccount = null)
    {
        A.CallTo(() => _serviceAccountRepository.GetOwnCompanyServiceAccountWithIamServiceAccountRolesAsync(ValidServiceAccountId, ValidAdminId))
            .Returns((_userRoleIds, withServiceAccount ? ValidConnectorId : null, withClient ? ClientId : null));
        A.CallTo(() => _serviceAccountRepository.GetOwnCompanyServiceAccountWithIamServiceAccountRolesAsync(A<Guid>.That.Not.Matches(x => x == ValidServiceAccountId), ValidAdminId))
            .Returns(((IEnumerable<Guid>, Guid?, string?))default);

        if (companyServiceAccount != null)
        {
            A.CallTo(() => _serviceAccountRepository.AttachAndModifyCompanyServiceAccount(ValidServiceAccountId, null, A<Action<CompanyServiceAccount>>._))
                .Invokes((Guid _, Action<CompanyServiceAccount>? _, Action<CompanyServiceAccount> modify) =>
                {
                    modify.Invoke(companyServiceAccount);
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
    }

    #endregion
}
