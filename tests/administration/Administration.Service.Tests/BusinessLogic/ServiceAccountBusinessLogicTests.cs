/********************************************************************************
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Dim.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Identity;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Encryption;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Context;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Entities;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;

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
    private static readonly Guid ValidServiceAccountVersion = Guid.NewGuid();
    private static readonly Guid ValidServiceAccountWithDimDataId = Guid.NewGuid();
    private static readonly Guid InactiveServiceAccount = Guid.NewGuid();
    private static readonly Guid ExternalServiceAccount = Guid.NewGuid();
    private readonly IIdentityData _identity;
    private readonly IEnumerable<Guid> _userRoleIds = Enumerable.Repeat(Guid.NewGuid(), 1);
    private readonly IServiceAccountCreation _serviceAccountCreation;
    private readonly ICompanyRepository _companyRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserRolesRepository _userRolesRepository;
    private readonly IProcessStepRepository<ProcessTypeId, ProcessStepTypeId> _processStepRepository;
    private readonly ITechnicalUserRepository _technicalUserRepository;
    private readonly IConnectorsRepository _connectorsRepository;
    private readonly IProvisioningManager _provisioningManager;
    private readonly IServiceAccountManagement _serviceAccountManagement;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IFixture _fixture;
    private readonly IOptions<ServiceAccountSettings> _options;
    private readonly IIdentityService _identityService;

    public ServiceAccountBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.ConfigureFixture();

        _provisioningManager = A.Fake<IProvisioningManager>();
        _serviceAccountCreation = A.Fake<IServiceAccountCreation>();
        _serviceAccountManagement = A.Fake<IServiceAccountManagement>();

        _companyRepository = A.Fake<ICompanyRepository>();
        _userRepository = A.Fake<IUserRepository>();
        _userRolesRepository = A.Fake<IUserRolesRepository>();
        _technicalUserRepository = A.Fake<ITechnicalUserRepository>();
        _connectorsRepository = A.Fake<IConnectorsRepository>();
        _processStepRepository = A.Fake<IProcessStepRepository<ProcessTypeId, ProcessStepTypeId>>();
        _provisioningManager = A.Fake<IProvisioningManager>();
        _portalRepositories = A.Fake<IPortalRepositories>();
        A.CallTo(() => _portalRepositories.GetInstance<IProcessStepRepository<ProcessTypeId, ProcessStepTypeId>>()).Returns(_processStepRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ITechnicalUserRepository>()).Returns(_technicalUserRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRolesRepository>()).Returns(_userRolesRepository);

        _identity = A.Fake<IIdentityData>();
        _identityService = A.Fake<IIdentityService>();
        A.CallTo(() => _identity.IdentityId).Returns(Guid.NewGuid());
        A.CallTo(() => _identity.IdentityTypeId).Returns(IdentityTypeId.COMPANY_USER);
        A.CallTo(() => _identity.CompanyId).Returns(ValidCompanyId);
        A.CallTo(() => _identityService.IdentityData).Returns(_identity);

        var encryptionKey = _fixture.CreateMany<byte>(32).ToArray();

        _options = Options.Create(new ServiceAccountSettings
        {
            AuthServiceUrl = "https://auth.test/auth",
            ClientId = ClientId,
            EncryptionConfigIndex = 1,
            EncryptionConfigs = new[] { new EncryptionModeConfig() { Index = 1, EncryptionKey = Convert.ToHexString(encryptionKey), CipherMode = System.Security.Cryptography.CipherMode.CBC, PaddingMode = System.Security.Cryptography.PaddingMode.PKCS7 } },
        });
    }

    #region CreateOwnCompanyServiceAccountAsync

    [Fact]
    public async Task CreateOwnCompanyServiceAccountAsync_WithValidInput_ReturnsCreatedConnectorData()
    {
        // Arrange
        SetupCreateOwnCompanyServiceAccount();
        var serviceAccountCreationInfos = new ServiceAccountCreationInfo("TheName", "Just a short description", IamClientAuthMethod.SECRET, Enumerable.Repeat(UserRoleId1, 1));
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, _serviceAccountCreation, _identityService, _serviceAccountManagement);

        // Act
        var result = await sut.CreateOwnCompanyServiceAccountAsync(serviceAccountCreationInfos);

        // Assert
        result.Should().ContainSingle().And.Satisfy(
            x => x.IamClientAuthMethod == IamClientAuthMethod.SECRET);
    }

    [Fact]
    public async Task CreateOwnCompanyServiceAccountAsync_WithInvalidUser_NotFoundException()
    {
        // Arrange
        var identity = _fixture.Create<IIdentityData>();
        A.CallTo(() => _identityService.IdentityData).Returns(identity);
        SetupCreateOwnCompanyServiceAccount();
        var serviceAccountCreationInfos = new ServiceAccountCreationInfo("TheName", "Just a short description", IamClientAuthMethod.SECRET, Enumerable.Repeat(UserRoleId1, 1));
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, _serviceAccountCreation, _identityService, _serviceAccountManagement);

        // Act
        async Task Act() => await sut.CreateOwnCompanyServiceAccountAsync(serviceAccountCreationInfos);

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
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, _serviceAccountCreation, _identityService, _serviceAccountManagement);

        // Act
        async Task Act() => await sut.CreateOwnCompanyServiceAccountAsync(serviceAccountCreationInfos);

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
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, _serviceAccountCreation, _identityService, _serviceAccountManagement);

        // Act
        async Task Act() => await sut.CreateOwnCompanyServiceAccountAsync(serviceAccountCreationInfos);

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
        var serviceAccountCreationInfos = new ServiceAccountCreationInfo("TheName", "Just a short description", IamClientAuthMethod.SECRET, [UserRoleId1, wrongUserRoleId]);
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, _serviceAccountCreation, _identityService, _serviceAccountManagement);

        // Act
        async Task Act() => await sut.CreateOwnCompanyServiceAccountAsync(serviceAccountCreationInfos);

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
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, null!, _identityService, _serviceAccountManagement);

        // Act
        var result = await sut.GetOwnCompanyServiceAccountDetailsAsync(ValidServiceAccountId);

        // Assert
        result.Should().NotBeNull();
        result.IamClientAuthMethod.Should().Be(IamClientAuthMethod.SECRET);
        result.Secret.Should().Be("topsecret");
        result.AuthenticationServiceUrl.Should().Be("https://auth.test/auth");
        result.TechnicalUserKindId.Should().Be(TechnicalUserKindId.INTERNAL);
        A.CallTo(() => _provisioningManager.GetIdOfCentralClientAsync(A<string>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.GetCentralClientAuthDataAsync(A<string>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetOwnCompanyServiceAccountDetailsAsync_WithValidInputAndDimCompanyData_GetsAllData()
    {
        // Arrange
        SetupGetOwnCompanyServiceAccountDetails();
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, null!, _identityService, _serviceAccountManagement);

        // Act
        var result = await sut.GetOwnCompanyServiceAccountDetailsAsync(ValidServiceAccountWithDimDataId);

        // Assert
        result.Should().NotBeNull();
        result.Secret.Should().Be("test");
        result.IamClientAuthMethod.Should().Be(IamClientAuthMethod.SECRET);
        result.AuthenticationServiceUrl.Should().Be("https://example.org/auth");
        result.TechnicalUserKindId.Should().Be(TechnicalUserKindId.EXTERNAL);
        A.CallTo(() => _provisioningManager.GetIdOfCentralClientAsync(A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _provisioningManager.GetCentralClientAuthDataAsync(A<string>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task GetOwnCompanyServiceAccountDetailsAsync_WithInvalidCompany_NotFoundException()
    {
        // Arrange
        SetupGetOwnCompanyServiceAccountDetails();
        var invalidCompanyId = Guid.NewGuid();
        A.CallTo(() => _identity.CompanyId).Returns(invalidCompanyId);
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, null!, _identityService, _serviceAccountManagement);

        // Act
        async Task Act() => await sut.GetOwnCompanyServiceAccountDetailsAsync(ValidServiceAccountId);

        // Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(Act);
        exception.Message.Should().Be(AdministrationServiceAccountErrors.SERVICE_ACCOUNT_NOT_FOUND.ToString());
    }

    [Fact]
    public async Task GetOwnCompanyServiceAccountDetailsAsync_WithInvalidServiceAccount_NotFoundException()
    {
        // Arrange
        SetupGetOwnCompanyServiceAccountDetails();
        var invalidServiceAccountId = Guid.NewGuid();
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, null!, _identityService, _serviceAccountManagement);

        // Act
        async Task Act() => await sut.GetOwnCompanyServiceAccountDetailsAsync(invalidServiceAccountId);

        // Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(Act);
        exception.Message.Should().Be(AdministrationServiceAccountErrors.SERVICE_ACCOUNT_NOT_FOUND.ToString());
    }

    #endregion

    #region ResetOwnCompanyServiceAccountSecretAsync

    [Fact]
    public async Task ResetOwnCompanyServiceAccountSecretAsync_WithValidInput_GetsAllData()
    {
        // Arrange
        SetupResetOwnCompanyServiceAccountSecret();
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, null!, _identityService, _serviceAccountManagement);

        // Act
        var result = await sut.ResetOwnCompanyServiceAccountSecretAsync(ValidServiceAccountId);

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
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, null!, _identityService, _serviceAccountManagement);

        // Act
        async Task Act() => await sut.ResetOwnCompanyServiceAccountSecretAsync(ValidServiceAccountId);

        // Assert
        var exception = await Assert.ThrowsAsync<ConflictException>(Act);
        exception.Message.Should().Be(AdministrationServiceAccountErrors.SERVICE_ACCOUNT_NOT_FOUND.ToString());
    }

    [Fact]
    public async Task ResetOwnCompanyServiceAccountSecretAsync_WithInvalidServiceAccount_NotFoundException()
    {
        // Arrange
        SetupResetOwnCompanyServiceAccountSecret();
        var invalidServiceAccountId = Guid.NewGuid();
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, null!, _identityService, _serviceAccountManagement);

        // Act
        async Task Act() => await sut.ResetOwnCompanyServiceAccountSecretAsync(invalidServiceAccountId);

        // Assert
        var exception = await Assert.ThrowsAsync<ConflictException>(Act);
        exception.Message.Should().Be(AdministrationServiceAccountErrors.SERVICE_ACCOUNT_NOT_FOUND.ToString());
    }

    #endregion

    #region UpdateOwnCompanyServiceAccountDetailsAsync

    [Fact]
    public async Task UpdateOwnCompanyServiceAccountDetailsAsync_WithValidData_ReturnsExpected()
    {
        // Arrange
        SetupUpdateOwnCompanyServiceAccountDetails();
        var data = new ServiceAccountEditableDetails(ValidServiceAccountId, "new name", "changed description", IamClientAuthMethod.SECRET);
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, null!, _identityService, _serviceAccountManagement);

        TechnicalUser? initial = null;
        TechnicalUser? modified = null;

        A.CallTo(() => _technicalUserRepository.AttachAndModifyTechnicalUser(A<Guid>._, A<Guid>._, A<Action<TechnicalUser>>._, A<Action<TechnicalUser>>._))
            .Invokes((Guid id, Guid version, Action<TechnicalUser>? initialize, Action<TechnicalUser> modify) =>
            {
                initial = new TechnicalUser(id, version, null!, null!, default, default);
                initialize?.Invoke(initial);
                modified = new TechnicalUser(id, version, null!, null!, default, default);
                modify(modified);
            });
        // Act
        var result = await sut.UpdateOwnCompanyServiceAccountDetailsAsync(ValidServiceAccountId, data);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("new name");
        A.CallTo(() => _provisioningManager.UpdateCentralClientAsync(A<string>._, A<ClientConfigData>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _technicalUserRepository.AttachAndModifyTechnicalUser(ValidServiceAccountId, ValidServiceAccountVersion, A<Action<TechnicalUser>>._, A<Action<TechnicalUser>>._))
            .MustHaveHappenedOnceExactly();
        initial.Should().NotBeNull().And.Match<TechnicalUser>(
            x => x.Id == ValidServiceAccountId && x.Version == ValidServiceAccountVersion && x.Name == "test" && x.Description == "test");
        modified.Should().NotBeNull().And.Match<TechnicalUser>(
            x => x.Name == "new name" && x.Description == "changed description");
    }

    [Fact]
    public async Task UpdateOwnCompanyServiceAccountDetailsAsync_WithInvalidAuthMethod_ThrowsArgumentException()
    {
        // Arrange
        SetupUpdateOwnCompanyServiceAccountDetails();
        var data = new ServiceAccountEditableDetails(ValidServiceAccountId, "new name", "changed description", IamClientAuthMethod.JWT);
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, null!, _identityService, _serviceAccountManagement);

        // Act
        async Task Act() => await sut.UpdateOwnCompanyServiceAccountDetailsAsync(ValidServiceAccountId, data);

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
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, null!, _identityService, _serviceAccountManagement);

        // Act
        async Task Act() => await sut.UpdateOwnCompanyServiceAccountDetailsAsync(Guid.NewGuid(), data);

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
        A.CallTo(() => _technicalUserRepository.GetTechnicalUserWithRoleDataClientIdAsync(invalidServiceAccountId, ValidCompanyId))
            .Returns<TechnicalUserWithRoleDataClientId?>(null);
        var data = new ServiceAccountEditableDetails(invalidServiceAccountId, "new name", "changed description", IamClientAuthMethod.SECRET);
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, null!, _identityService, _serviceAccountManagement);

        // Act
        async Task Act() => await sut.UpdateOwnCompanyServiceAccountDetailsAsync(invalidServiceAccountId, data);

        // Assert
        var exception = await Assert.ThrowsAsync<ConflictException>(Act);
        exception.Message.Should().Be(AdministrationServiceAccountErrors.SERVICE_ACCOUNT_NOT_FOUND.ToString());
    }

    [Fact]
    public async Task UpdateOwnCompanyServiceAccountDetailsAsync_WithInactiveServiceAccount_ThrowsArgumentException()
    {
        // Arrange
        SetupUpdateOwnCompanyServiceAccountDetails();
        var inactive = _fixture.Build<TechnicalUserWithRoleDataClientId>()
            .With(x => x.UserStatusId, UserStatusId.INACTIVE)
            .Create();
        A.CallTo(() => _technicalUserRepository.GetTechnicalUserWithRoleDataClientIdAsync(InactiveServiceAccount, ValidCompanyId))
            .Returns(inactive);
        var data = new ServiceAccountEditableDetails(InactiveServiceAccount, "new name", "changed description", IamClientAuthMethod.SECRET);
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, null!, _identityService, _serviceAccountManagement);

        // Act
        async Task Act() => await sut.UpdateOwnCompanyServiceAccountDetailsAsync(InactiveServiceAccount, data);

        // Assert
        var exception = await Assert.ThrowsAsync<ConflictException>(Act);
        exception.Message.Should().Be(AdministrationServiceAccountErrors.SERVICE_INACTIVE_CONFLICT.ToString());
    }

    [Fact]
    public async Task UpdateOwnCompanyServiceAccountDetailsAsync_WithExternalServiceAccount_CallsExpected()
    {
        // Arrange
        SetupUpdateOwnCompanyServiceAccountDetails();

        var external = _fixture.Build<TechnicalUserWithRoleDataClientId>()
            .With(x => x.UserStatusId, UserStatusId.ACTIVE)
            .With(x => x.TechnicalUserTypeId, TechnicalUserTypeId.OWN)
            .With(x => x.TechnicalUserKindId, TechnicalUserKindId.EXTERNAL)
            .Create();
        A.CallTo(() => _technicalUserRepository.GetTechnicalUserWithRoleDataClientIdAsync(ExternalServiceAccount, ValidCompanyId))
            .Returns(external);
        var data = new ServiceAccountEditableDetails(ExternalServiceAccount, "new name", "changed description", IamClientAuthMethod.SECRET);
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, null!, _identityService, _serviceAccountManagement);

        // Act
        var result = await sut.UpdateOwnCompanyServiceAccountDetailsAsync(ExternalServiceAccount, data);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("new name");
        A.CallTo(() => _technicalUserRepository.GetTechnicalUserWithRoleDataClientIdAsync(ExternalServiceAccount, ValidCompanyId))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.UpdateCentralClientAsync(A<string>._, A<ClientConfigData>._))
            .MustNotHaveHappened();
        A.CallTo(() => _technicalUserRepository.AttachAndModifyTechnicalUser(ExternalServiceAccount, external.TechnicalUserVersion, A<Action<TechnicalUser>>._, A<Action<TechnicalUser>>._))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region GetOwnCompanyServiceAccountsDataAsync

    [Theory]
    [InlineData(new[] { UserStatusId.INACTIVE, UserStatusId.DELETED, UserStatusId.ACTIVE }, false, new[] { UserStatusId.INACTIVE, UserStatusId.DELETED, UserStatusId.ACTIVE })]
    [InlineData(new[] { UserStatusId.DELETED, UserStatusId.PENDING, UserStatusId.ACTIVE }, true, new[] { UserStatusId.DELETED, UserStatusId.PENDING, UserStatusId.ACTIVE })]
    [InlineData(new UserStatusId[] { }, false, new UserStatusId[] { UserStatusId.ACTIVE, UserStatusId.PENDING, UserStatusId.PENDING_DELETION })]
    [InlineData(new UserStatusId[] { }, true, new UserStatusId[] { UserStatusId.INACTIVE })]
    [InlineData(null, false, new[] { UserStatusId.ACTIVE, UserStatusId.PENDING, UserStatusId.PENDING_DELETION })]
    [InlineData(null, true, new[] { UserStatusId.INACTIVE })]
    public async Task GetOwnCompanyServiceAccountsDataAsync_GetsExpectedData(IEnumerable<UserStatusId>? userStatusIds, bool isUserInactive, IEnumerable<UserStatusId> expectedStatusIds)
    {
        // Arrange
        var count = 0;
        var data = _fixture
            .Build<CompanyServiceAccountData>()
            .With(x => x.TechnicalUserKindId, (IFixture _) =>
            {
                var kind = count % 2 == 0 ? TechnicalUserKindId.INTERNAL
                    : TechnicalUserKindId.EXTERNAL;
                count++;
                return kind;
            })
            .CreateMany(15);
        A.CallTo(() => _technicalUserRepository.GetOwnTechnicalUsersUntracked(A<Guid>._, A<string?>._, A<bool?>._, A<IEnumerable<UserStatusId>>._))
            .Returns((int skip, int take) => Task.FromResult<Pagination.Source<CompanyServiceAccountData>?>(new(data.Count(), data.Skip(skip).Take(take))));

        A.CallTo(() => _portalRepositories.GetInstance<ITechnicalUserRepository>()).Returns(_technicalUserRepository);
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, null!, _identityService, _serviceAccountManagement);

        // Act
        var result = await sut.GetOwnCompanyServiceAccountsDataAsync(1, 10, null, null, isUserInactive, userStatusIds);

        // Assert
        result.Should().NotBeNull();
        result.Content.Should().HaveCount(5);
        result.Content
            .Where(x => x.TechnicalUserKindId == TechnicalUserKindId.INTERNAL)
            .Should()
            .HaveCount(3);
        result.Content
            .Where(x => x.TechnicalUserKindId == TechnicalUserKindId.EXTERNAL)
            .Should()
            .HaveCount(2);
        A.CallTo(() => _technicalUserRepository.GetOwnTechnicalUsersUntracked(ValidCompanyId, null, null, A<IEnumerable<UserStatusId>>.That.IsSameSequenceAs(expectedStatusIds)))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region DeleteOwnCompanyServiceAccountAsync

    [Fact]
    public async Task DeleteOwnCompanyServiceAccountAsync_WithNotExistingServiceAccount_ThrowsNotFoundException()
    {
        // Arrange
        var serviceAccountId = Guid.NewGuid();
        SetupDeleteOwnCompanyServiceAccount();

        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, null!, _identityService, _serviceAccountManagement);

        // Act
        async Task Act() => await sut.DeleteOwnCompanyServiceAccountAsync(serviceAccountId);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be(AdministrationServiceAccountErrors.SERVICE_ACCOUNT_NOT_FOUND.ToString());
    }

    [Fact]
    public async Task DeleteOwnCompanyServiceAccountAsync_WithExistingOfferSubscriptiom_ThrowsNotFoundException()
    {
        // Arrange
        var serviceAccount = new TechnicalUser(Guid.NewGuid(), Guid.NewGuid(), "test-sa", "test", TechnicalUserTypeId.OWN, TechnicalUserKindId.INTERNAL);
        A.CallTo(() => _technicalUserRepository.GetOwnTechnicalUserWithIamUserRolesAsync(ValidServiceAccountId, ValidCompanyId, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns(new OwnTechnicalUserData(_userRoleIds, serviceAccount.Id, UserStatusId.ACTIVE, true, serviceAccount.Version, null, null, ConnectorStatusId.INACTIVE, OfferSubscriptionStatusId.ACTIVE, false, false, null));
        A.CallTo(() => _technicalUserRepository.GetOwnTechnicalUserWithIamUserRolesAsync(A<Guid>.That.Not.Matches(x => x == ValidServiceAccountId), A<Guid>._, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns<OwnTechnicalUserData?>(null);

        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, null!, _identityService, _serviceAccountManagement);

        // Act
        async Task Act() => await sut.DeleteOwnCompanyServiceAccountAsync(ValidServiceAccountId);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be(AdministrationServiceAccountErrors.SERVICE_USERID_ACTIVATION_ACTIVE_CONFLICT.ToString());
    }

    [Fact]
    public async Task DeleteOwnCompanyServiceAccountAsync_WithInvalidConnectorStatus_ThrowsConflictException()
    {
        // Arrange
        var serviceAccount = new TechnicalUser(Guid.NewGuid(), Guid.NewGuid(), "test-sa", "test", TechnicalUserTypeId.OWN, TechnicalUserKindId.INTERNAL);
        A.CallTo(() => _technicalUserRepository.GetOwnTechnicalUserWithIamUserRolesAsync(ValidServiceAccountId, ValidCompanyId, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns(new OwnTechnicalUserData(_userRoleIds, serviceAccount.Id, UserStatusId.ACTIVE, true, serviceAccount.Version, null, null, ConnectorStatusId.ACTIVE, OfferSubscriptionStatusId.PENDING, false, false, null));
        A.CallTo(() => _technicalUserRepository.GetOwnTechnicalUserWithIamUserRolesAsync(A<Guid>.That.Not.Matches(x => x == ValidServiceAccountId), A<Guid>._, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns<OwnTechnicalUserData?>(null);

        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, null!, _identityService, _serviceAccountManagement);

        // Act
        async Task Act() => await sut.DeleteOwnCompanyServiceAccountAsync(ValidServiceAccountId);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be(AdministrationServiceAccountErrors.SERVICE_USERID_ACTIVATION_PENDING_CONFLICT.ToString());
    }

    [Theory]
    [InlineData(UserStatusId.PENDING_DELETION)]
    [InlineData(UserStatusId.PENDING)]
    [InlineData(UserStatusId.DELETED)]
    [InlineData(UserStatusId.INACTIVE)]
    public async Task DeleteOwnCompanyServiceAccountAsync_WithServiceAccountNotActive_ThrowsConflictException(UserStatusId userStatusId)
    {
        // Arrange
        A.CallTo(() => _technicalUserRepository.GetOwnTechnicalUserWithIamUserRolesAsync(ValidServiceAccountId, ValidCompanyId, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns(new OwnTechnicalUserData(_userRoleIds, ValidServiceAccountId, userStatusId, true, Guid.NewGuid(), null, null, ConnectorStatusId.ACTIVE, OfferSubscriptionStatusId.PENDING, false, false, null));

        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, null!, _identityService, _serviceAccountManagement);

        // Act
        async Task Act() => await sut.DeleteOwnCompanyServiceAccountAsync(ValidServiceAccountId);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be(AdministrationServiceAccountErrors.SERVICE_ACCOUNT_NOT_ACTIVE.ToString());
    }

    [Fact]
    public async Task DeleteOwnCompanyServiceAccountAsync_WithUserNotOwnerOrProvider_ThrowsForbiddenException()
    {
        // Arrange
        A.CallTo(() => _technicalUserRepository.GetOwnTechnicalUserWithIamUserRolesAsync(ValidServiceAccountId, ValidCompanyId, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns(new OwnTechnicalUserData(_userRoleIds, ValidServiceAccountId, UserStatusId.ACTIVE, false, Guid.NewGuid(), null, null, ConnectorStatusId.ACTIVE, OfferSubscriptionStatusId.PENDING, false, false, null));

        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, null!, _identityService, _serviceAccountManagement);

        // Act
        async Task Act() => await sut.DeleteOwnCompanyServiceAccountAsync(ValidServiceAccountId);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be(AdministrationServiceAccountErrors.SERVICE_ACCOUNT_NO_PROVIDER_OR_OWNER.ToString());
    }

    [Fact]
    public async Task DeleteOwnCompanyServiceAccountAsync_WithoutClient_CallsExpected()
    {
        // Arrange
        var identity = _fixture.Build<Identity>()
            .With(x => x.Id, ValidServiceAccountId)
            .With(x => x.UserStatusId, UserStatusId.ACTIVE)
            .Create();
        var connector = _fixture.Build<Connector>()
            .With(x => x.TechnicalUserId, ValidServiceAccountId)
            .Create();
        var processId = Guid.NewGuid();
        SetupDeleteOwnCompanyServiceAccount(connector, identity, processId);
        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, null!, _identityService, _serviceAccountManagement);

        // Act
        await sut.DeleteOwnCompanyServiceAccountAsync(ValidServiceAccountId);

        A.CallTo(() => _connectorsRepository.AttachAndModifyConnector(ValidConnectorId, A<Action<Connector>>._, A<Action<Connector>>._)).MustHaveHappenedOnceExactly();
        connector.TechnicalUserId.Should().BeNull();
        A.CallTo(() => _serviceAccountManagement.DeleteServiceAccount(ValidServiceAccountId, A<DeleteServiceAccountData>._))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region GetServiceAccountRolesAsync

    [Fact]
    public async Task GetServiceAccountRolesAsync_GetsExpectedData()
    {
        // Arrange
        var data = _fixture.CreateMany<UserRoleWithDescriptionTransferData>(15);

        A.CallTo(() => _userRolesRepository.GetServiceAccountRolesAsync(A<Guid>._, A<string>._, A<IEnumerable<Guid>>._, A<string>._))
            .Returns(data.ToAsyncEnumerable());

        A.CallTo(() => _portalRepositories.GetInstance<IUserRolesRepository>()).Returns(_userRolesRepository);

        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, null!, _identityService, _serviceAccountManagement);

        // Act
        var result = await sut.GetServiceAccountRolesAsync(null).ToListAsync();

        // Assert
        A.CallTo(() => _userRolesRepository.GetServiceAccountRolesAsync(_identity.CompanyId, ClientId, A<IEnumerable<Guid>>._, A<string>._)).MustHaveHappenedOnceExactly();

        result.Should().NotBeNull();
        result.Should().HaveCount(15);
        // Sonar fix -> Return value of pure method is not used
        result.Should().AllSatisfy(ur =>
        {
            var transferData = new UserRoleWithDescriptionTransferData(ur.UserRoleId, ur.UserRoleText, ur.RoleDescription, ur.RoleType == UserRoleType.External);
            data.Contains(transferData).Should().BeTrue();
        });
    }

    #endregion

    #region HandleServiceAccountCreationCallback

    [Fact]
    public async Task HandleServiceAccountCreationCallback_WithValidOfferSubscription_ExecutesExpected()
    {
        // Arrange
        const ProcessStepTypeId stepToTrigger = ProcessStepTypeId.AWAIT_CREATE_DIM_TECHNICAL_USER_RESPONSE;
        var process = new Process<ProcessTypeId, ProcessStepTypeId>(Guid.NewGuid(), ProcessTypeId.OFFER_SUBSCRIPTION, Guid.NewGuid());
        var context = new VerifyProcessData<ProcessTypeId, ProcessStepTypeId>(process, [new ProcessStep<ProcessTypeId, ProcessStepTypeId>(Guid.NewGuid(), stepToTrigger, ProcessStepStatusId.TODO, process.Id, DateTimeOffset.UtcNow)]);
        A.CallTo(() => _technicalUserRepository.GetProcessDataForTechnicalUserCallback(A<Guid>._, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns((ProcessTypeId.OFFER_SUBSCRIPTION, context, Guid.NewGuid(), Guid.NewGuid()));

        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, _serviceAccountCreation, _identityService, _serviceAccountManagement);

        // Act
        await sut.HandleServiceAccountCreationCallback(process.Id, _fixture.Create<AuthenticationDetail>());

        // Assert
        A.CallTo(() => _technicalUserRepository.GetProcessDataForTechnicalUserCallback(process.Id, A<IEnumerable<ProcessStepTypeId>>.That.Matches(x => x.Count() == 1 && x.Single() == stepToTrigger)))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData(ProcessTypeId.OFFER_SUBSCRIPTION)]
    [InlineData(ProcessTypeId.DIM_TECHNICAL_USER)]
    public async Task HandleServiceAccountCreationCallback_WithNotExistingProcess_ThrowsException(ProcessTypeId processTypeId)
    {
        // Arrange
        var stepToTrigger = ProcessStepTypeId.AWAIT_CREATE_DIM_TECHNICAL_USER_RESPONSE;
        var process = new Process<ProcessTypeId, ProcessStepTypeId>(Guid.NewGuid(), processTypeId, Guid.NewGuid());
        A.CallTo(() => _technicalUserRepository.GetProcessDataForTechnicalUserCallback(A<Guid>._, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns<(ProcessTypeId, VerifyProcessData<ProcessTypeId, ProcessStepTypeId>, Guid?, Guid?)>(default);

        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, _serviceAccountCreation, _identityService, _serviceAccountManagement);

        async Task Act() => await sut.HandleServiceAccountCreationCallback(process.Id, _fixture.Create<AuthenticationDetail>());

        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);

        // Assert
        A.CallTo(() => _technicalUserRepository.GetProcessDataForTechnicalUserCallback(process.Id, A<IEnumerable<ProcessStepTypeId>>.That.Matches(x => x.Count() == 1 && x.Single() == stepToTrigger)))
            .MustHaveHappenedOnceExactly();
        ex.Message.Should().Be($"externalId {process.Id} does not exist");
    }

    [Fact]
    public async Task HandleServiceAccountCreationCallback_WithOfferSubscriptionIdNotExisting_ThrowsException()
    {
        // Arrange
        const ProcessStepTypeId stepToTrigger = ProcessStepTypeId.AWAIT_CREATE_DIM_TECHNICAL_USER_RESPONSE;
        var process = new Process<ProcessTypeId, ProcessStepTypeId>(Guid.NewGuid(), ProcessTypeId.OFFER_SUBSCRIPTION, Guid.NewGuid());
        var context = new VerifyProcessData<ProcessTypeId, ProcessStepTypeId>(process, [new ProcessStep<ProcessTypeId, ProcessStepTypeId>(Guid.NewGuid(), stepToTrigger, ProcessStepStatusId.TODO, process.Id, DateTimeOffset.UtcNow)]);
        A.CallTo(() => _technicalUserRepository.GetProcessDataForTechnicalUserCallback(A<Guid>._, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns((ProcessTypeId.OFFER_SUBSCRIPTION, context, null, null));

        var sut = new ServiceAccountBusinessLogic(_provisioningManager, _portalRepositories, _options, _serviceAccountCreation, _identityService, _serviceAccountManagement);

        async Task Act() => await sut.HandleServiceAccountCreationCallback(process.Id, _fixture.Create<AuthenticationDetail>());

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        A.CallTo(() => _technicalUserRepository.GetProcessDataForTechnicalUserCallback(process.Id, A<IEnumerable<ProcessStepTypeId>>.That.Matches(x => x.Count() == 1 && x.Single() == stepToTrigger)))
            .MustHaveHappenedOnceExactly();
        ex.Message.Should().Be($"ServiceAccountId must be set for process {process.Id}");
    }

    #endregion

    #region Setup

    private void SetupCreateOwnCompanyServiceAccount()
    {
        A.CallTo(() => _companyRepository.GetBpnAndTechnicalUserRoleIds(ValidCompanyId, ClientId))
            .Returns((ValidBpn, new[] { UserRoleId1, UserRoleId2 }));
        A.CallTo(() => _companyRepository.GetBpnAndTechnicalUserRoleIds(A<Guid>.That.Not.Matches(x => x == ValidCompanyId), ClientId))
            .Returns<(string?, IEnumerable<Guid>)>(default);

        A.CallTo(() => _serviceAccountCreation.CreateServiceAccountAsync(A<ServiceAccountCreationInfo>._, A<Guid>.That.Matches(x => x == ValidCompanyId), A<IEnumerable<string>>._, TechnicalUserTypeId.OWN, A<bool>._, true, new ServiceAccountCreationProcessData(ProcessTypeId.DIM_TECHNICAL_USER, null), null))
            .Returns((false, null, [new CreatedServiceAccountData(Guid.NewGuid(), "test", "description", UserStatusId.ACTIVE, ClientId, new(ClientId, Guid.NewGuid().ToString(), new ClientAuthData(IamClientAuthMethod.SECRET)), Enumerable.Empty<UserRoleData>())]));

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
    }

    private void SetupResetOwnCompanyServiceAccountSecret()
    {
        var authData = new ClientAuthData(IamClientAuthMethod.SECRET) { Secret = "topsecret" };
        SetupGetOwnCompanyServiceAccount();

        A.CallTo(() => _provisioningManager.ResetCentralClientAuthDataAsync(A<string>._))
            .Returns(authData);
    }

    private void SetupUpdateOwnCompanyServiceAccountDetails()
    {
        var authData = new ClientAuthData(IamClientAuthMethod.SECRET) { Secret = "topsecret" };
        var data = _fixture.Build<TechnicalUserWithRoleDataClientId>()
            .With(x => x.TechnicalUserVersion, ValidServiceAccountVersion)
            .With(x => x.UserStatusId, UserStatusId.ACTIVE)
            .With(x => x.Name, "test")
            .With(x => x.Description, "test")
            .With(x => x.TechnicalUserTypeId, TechnicalUserTypeId.OWN)
            .With(x => x.TechnicalUserKindId, TechnicalUserKindId.INTERNAL)
            .With(x => x.ClientClientId, "sa-test-1")
            .Create();
        A.CallTo(() => _technicalUserRepository.GetTechnicalUserWithRoleDataClientIdAsync(ValidServiceAccountId, ValidCompanyId))
            .Returns(data);
        A.CallTo(() => _technicalUserRepository.GetTechnicalUserWithRoleDataClientIdAsync(ValidServiceAccountId, A<Guid>.That.Not.Matches(x => x == ValidCompanyId)))
            .Returns<TechnicalUserWithRoleDataClientId?>(null);

        A.CallTo(() => _provisioningManager.ResetCentralClientAuthDataAsync(A<string>._))
            .Returns(authData);
    }

    private void SetupGetOwnCompanyServiceAccount()
    {
        var data = _fixture.Build<TechnicalUserDetailedData>()
            .With(x => x.Status, UserStatusId.ACTIVE)
            .With(x => x.DimServiceAccountData, default(ExternalTechnicalUserData?))
            .With(x => x.TechnicalUserKindId, TechnicalUserKindId.INTERNAL)
            .Create();

        var cryptoHelper = _options.Value.EncryptionConfigs.GetCryptoHelper(_options.Value.EncryptionConfigIndex);
        var (secret, initializationVector) = cryptoHelper.Encrypt("test");

        var dimServiceAccountData = new ExternalTechnicalUserData("https://example.org/auth", secret, initializationVector, _options.Value.EncryptionConfigIndex);
        var dataWithDim = _fixture.Build<TechnicalUserDetailedData>()
            .With(x => x.DimServiceAccountData, dimServiceAccountData)
            .With(x => x.TechnicalUserKindId, TechnicalUserKindId.EXTERNAL)
            .Create();

        A.CallTo(() => _technicalUserRepository.GetOwnTechnicalUserDataUntrackedAsync(ValidServiceAccountId, ValidCompanyId))
            .Returns(data);
        A.CallTo(() => _technicalUserRepository.GetOwnTechnicalUserDataUntrackedAsync(ValidServiceAccountWithDimDataId, ValidCompanyId))
            .Returns(dataWithDim);
        A.CallTo(() => _technicalUserRepository.GetOwnTechnicalUserDataUntrackedAsync(
                A<Guid>.That.Not.Matches(x => x == ValidServiceAccountId || x == ValidServiceAccountWithDimDataId), ValidCompanyId))
            .Returns<TechnicalUserDetailedData?>(null);
        A.CallTo(() => _technicalUserRepository.GetOwnTechnicalUserDataUntrackedAsync(ValidServiceAccountId, A<Guid>.That.Not.Matches(x => x == ValidCompanyId)))
            .Returns<TechnicalUserDetailedData?>(null);
    }

    private void SetupDeleteOwnCompanyServiceAccount(Connector? connector = null, Identity? identity = null, Guid? processId = null)
    {
        var serviceAccount = new TechnicalUser(Guid.NewGuid(), Guid.NewGuid(), "test-sa", "test", TechnicalUserTypeId.OWN, TechnicalUserKindId.INTERNAL);
        A.CallTo(() => _technicalUserRepository.GetOwnTechnicalUserWithIamUserRolesAsync(ValidServiceAccountId, ValidCompanyId, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns(new OwnTechnicalUserData(_userRoleIds, serviceAccount.Id, UserStatusId.ACTIVE, true, serviceAccount.Version, ValidConnectorId, ClientId, ConnectorStatusId.INACTIVE, OfferSubscriptionStatusId.PENDING, false, false, processId));
        A.CallTo(() => _technicalUserRepository.GetOwnTechnicalUserWithIamUserRolesAsync(A<Guid>.That.Not.Matches(x => x == ValidServiceAccountId), A<Guid>._, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns<OwnTechnicalUserData?>(null);

        if (identity != null)
        {
            A.CallTo(() => _userRepository.AttachAndModifyIdentity(ValidServiceAccountId, A<Action<Identity>>._, A<Action<Identity>>._))
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

        A.CallTo(() => _portalRepositories.GetInstance<IConnectorsRepository>()).Returns(_connectorsRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRolesRepository>()).Returns(_userRolesRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IProcessStepRepository<ProcessTypeId, ProcessStepTypeId>>()).Returns(_processStepRepository);
    }

    #endregion
}
