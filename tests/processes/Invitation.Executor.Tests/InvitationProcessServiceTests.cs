/********************************************************************************
 * Copyright (c) 2024 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.ExternalSystems.Provisioning.Library;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Encryption;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.Invitation.Executor.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Processes.Mailing.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;
using System.Security.Cryptography;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.Invitation.Executor.Tests;

public class InvitationProcessServiceTests
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly ICompanyInvitationRepository _companyInvitationRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IIdentityProviderRepository _identityProviderRepository;
    private readonly IApplicationRepository _applicationRepository;
    private readonly IIdpManagement _idpManagement;
    private readonly IUserProvisioningService _userProvisioningService;
    private readonly IMailingProcessCreation _mailingProcessCreation;
    private readonly IInvitationProcessService _sut;
    private readonly IOptions<InvitationSettings> _setting;
    private readonly byte[] _encryptionKey;
    private readonly IFixture _fixture;

    public InvitationProcessServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _portalRepositories = A.Fake<IPortalRepositories>();
        _companyInvitationRepository = A.Fake<ICompanyInvitationRepository>();
        _companyRepository = A.Fake<ICompanyRepository>();
        _identityProviderRepository = A.Fake<IIdentityProviderRepository>();
        _applicationRepository = A.Fake<IApplicationRepository>();

        _idpManagement = A.Fake<IIdpManagement>();
        _userProvisioningService = A.Fake<IUserProvisioningService>();
        _mailingProcessCreation = A.Fake<IMailingProcessCreation>();

        A.CallTo(() => _portalRepositories.GetInstance<ICompanyInvitationRepository>())
            .Returns(_companyInvitationRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>())
            .Returns(_companyRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IIdentityProviderRepository>())
            .Returns(_identityProviderRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IApplicationRepository>())
            .Returns(_applicationRepository);

        _encryptionKey = _fixture.CreateMany<byte>(32).ToArray();

        _setting = Options.Create(new InvitationSettings
        {
            InitialLoginTheme = "TestLoginTheme",
            PasswordResendAddress = "https://example.org/resend",
            RegistrationAppAddress = "https://example.org/registration",
            InvitedUserInitialRoles = Enumerable.Repeat(new UserRoleConfig("Cl1", new[]
            {
                "ur 1",
                "ur 2"
            }), 1),
            EncryptionConfigIndex = 0,
            EncryptionConfigs = new[]
            {
                new EncryptionModeConfig
                {
                    Index = 0,
                    CipherMode = CipherMode.CBC,
                    PaddingMode = PaddingMode.PKCS7,
                    EncryptionKey = Convert.ToHexString(_encryptionKey)
                }
            }
        });

        _sut = new InvitationProcessService(
            _idpManagement,
            _userProvisioningService,
            _portalRepositories,
            _mailingProcessCreation,
            _setting);
    }

    #region CreateCentralIdp

    [Fact]
    public async Task CreateCentralIdp_WithValid_ReturnsExpected()
    {
        // Arrange
        var companyInvitation = _fixture.Create<CompanyInvitation>();
        A.CallTo(() => _companyInvitationRepository.GetOrganisationNameForInvitation(companyInvitation.Id))
            .Returns("testCorp");
        A.CallTo(() => _idpManagement.GetNextCentralIdentityProviderNameAsync())
            .Returns("cl1-testCorp");
        A.CallTo(() => _companyInvitationRepository.AttachAndModifyCompanyInvitation(companyInvitation.Id, A<Action<CompanyInvitation>>._, A<Action<CompanyInvitation>>._))
            .Invokes((Guid _, Action<CompanyInvitation>? initialize, Action<CompanyInvitation> modify) =>
            {
                initialize?.Invoke(companyInvitation);
                modify(companyInvitation);
            });

        // Act
        var result = await _sut.CreateCentralIdp(companyInvitation.Id);

        // Assert
        companyInvitation.IdpName.Should().Be("cl1-testCorp");
        result.modified.Should().BeTrue();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().ContainSingle()
            .Which.Should().Be(ProcessStepTypeId.INVITATION_CREATE_SHARED_IDP_SERVICE_ACCOUNT);
    }

    [Fact]
    public async Task CreateCentralIdp_WithNotExisting_ThrowsConflictException()
    {
        // Arrange
        var companyInvitation = _fixture.Create<CompanyInvitation>();
        A.CallTo(() => _companyInvitationRepository.GetOrganisationNameForInvitation(companyInvitation.Id))
            .Returns((string?)null);

        // Act
        Task Act() => _sut.CreateCentralIdp(companyInvitation.Id);
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("Org name must not be null");
    }

    #endregion

    #region CreateSharedIdpServiceAccount

    [Fact]
    public async Task CreateSharedIdpServiceAccount_WithValid_ReturnsExpected()
    {
        // Arrange
        var companyInvitation = _fixture.Create<CompanyInvitation>();
        A.CallTo(() => _companyInvitationRepository.GetIdpNameForInvitationId(companyInvitation.Id))
            .Returns("idp1");
        A.CallTo(() => _idpManagement.CreateSharedIdpServiceAccountAsync("idp1"))
            .Returns(new ValueTuple<string, string, string>("cl1", "test", Guid.NewGuid().ToString()));
        A.CallTo(() => _companyInvitationRepository.AttachAndModifyCompanyInvitation(companyInvitation.Id, A<Action<CompanyInvitation>>._, A<Action<CompanyInvitation>>._))
            .Invokes((Guid _, Action<CompanyInvitation>? initialize, Action<CompanyInvitation> modify) =>
            {
                initialize?.Invoke(companyInvitation);
                modify(companyInvitation);
            });

        // Act
        var result = await _sut.CreateSharedIdpServiceAccount(companyInvitation.Id);

        // Assert
        companyInvitation.ClientId.Should().Be("cl1");
        companyInvitation.ClientSecret.Should().NotBeNull();
        result.modified.Should().BeTrue();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().ContainSingle()
            .Which.Should().Be(ProcessStepTypeId.INVITATION_ADD_REALM_ROLE);
        A.CallTo(() => _companyInvitationRepository.AttachAndModifyCompanyInvitation(A<Guid>._, A<Action<CompanyInvitation>>._, A<Action<CompanyInvitation>>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CreateSharedIdpServiceAccount_WithNotExisting_ThrowsConflictException()
    {
        // Arrange
        var companyInvitation = _fixture.Create<CompanyInvitation>();
        A.CallTo(() => _companyInvitationRepository.GetIdpNameForInvitationId(companyInvitation.Id))
            .Returns((string?)null);

        // Act
        Task Act() => _sut.CreateSharedIdpServiceAccount(companyInvitation.Id);
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("Idp name must not be null");
    }

    [Fact]
    public async Task CreateSharedIdpServiceAccount_WithInvalidEncryptionKey_Throws()
    {
        // Arrange
        var companyInvitation = _fixture.Create<CompanyInvitation>();

        var settings = Options.Create(new InvitationSettings
        {
            EncryptionConfigIndex = 0,
            EncryptionConfigs = [
                new EncryptionModeConfig
                {
                    Index = 0,
                    CipherMode = CipherMode.CBC,
                    PaddingMode = PaddingMode.PKCS7,
                    EncryptionKey = _fixture.Create<string>()
                }
            ]
        });

        var sut = new InvitationProcessService(
            _idpManagement,
            _userProvisioningService,
            _portalRepositories,
            _mailingProcessCreation,
            settings);

        // Act
        Task Act() => sut.CreateSharedIdpServiceAccount(companyInvitation.Id);
        await Assert.ThrowsAsync<ConfigurationException>(Act);

        // Assert
        A.CallTo(() => _idpManagement.GetNextCentralIdentityProviderNameAsync())
            .MustNotHaveHappened();
        A.CallTo(() => _idpManagement.CreateSharedIdpServiceAccountAsync(A<string>._))
            .MustNotHaveHappened();
    }

    #endregion

    #region AddRealmRoleMappingsToUserAsync

    [Fact]
    public async Task AddRealmRoleMappingsToUserAsync_WithValid_ReturnsExpected()
    {
        // Arrange
        var companyInvitationId = Guid.NewGuid();
        var serviceAccountId = Guid.NewGuid().ToString();
        A.CallTo(() => _companyInvitationRepository.GetServiceAccountUserIdForInvitation(companyInvitationId))
            .Returns(serviceAccountId);

        // Act
        var result = await _sut.AddRealmRoleMappingsToUserAsync(companyInvitationId);

        // Assert
        result.modified.Should().BeTrue();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().ContainSingle()
            .Which.Should().Be(ProcessStepTypeId.INVITATION_CREATE_SHARED_REALM);
    }

    [Fact]
    public async Task AddRealmRoleMappingsToUserAsync_WithNotExisting_ThrowsConflictException()
    {
        // Arrange
        var companyInvitation = _fixture.Create<CompanyInvitation>();
        A.CallTo(() => _companyInvitationRepository.GetServiceAccountUserIdForInvitation(companyInvitation.Id))
            .Returns((string?)null);

        // Act
        Task Act() => _sut.AddRealmRoleMappingsToUserAsync(companyInvitation.Id);
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("ServiceAccountUserId must not be null");
    }

    #endregion

    #region UpdateCentralIdpUrl

    [Fact]
    public async Task UpdateCentralIdpUrl_WithValid_ReturnsExpected()
    {
        // Arrange
        var companyInvitation = _fixture.Create<CompanyInvitation>();
        var password = _fixture.Create<string>();
        var helper = _setting.Value.EncryptionConfigs.GetCryptoHelper(_setting.Value.EncryptionConfigIndex);
        var (secret, initializationVector) = helper.Encrypt(password);
        A.CallTo(() => _companyInvitationRepository.GetUpdateCentralIdpUrlData(companyInvitation.Id))
            .Returns(("testCorp", "idp1", "cl1", secret, initializationVector, _setting.Value.EncryptionConfigIndex));

        // Act
        var result = await _sut.UpdateCentralIdpUrl(companyInvitation.Id);

        // Assert
        A.CallTo(() => _idpManagement.UpdateCentralIdentityProviderUrlsAsync("idp1", "testCorp", "TestLoginTheme", "cl1", password))
            .MustHaveHappenedOnceExactly();
        result.modified.Should().BeTrue();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().ContainSingle()
            .Which.Should().Be(ProcessStepTypeId.INVITATION_CREATE_SHARED_CLIENT);
    }

    [Fact]
    public async Task UpdateCentralIdpUrl_WithClientSecretNotSet_ThrowsConflictException()
    {
        // Arrange
        var companyInvitation = _fixture.Create<CompanyInvitation>();
        A.CallTo(() => _companyInvitationRepository.GetUpdateCentralIdpUrlData(companyInvitation.Id))
            .Returns(("testCorp", "cl1", "idp1", null, null, 0));

        // Act
        Task Act() => _sut.UpdateCentralIdpUrl(companyInvitation.Id);
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("ClientSecret must not be null");
    }

    [Fact]
    public async Task UpdateCentralIdpUrl_WithClientIdNotSet_ThrowsConflictException()
    {
        // Arrange
        var companyInvitation = _fixture.Create<CompanyInvitation>();
        A.CallTo(() => _companyInvitationRepository.GetUpdateCentralIdpUrlData(companyInvitation.Id))
            .Returns(("testCorp", "cl1", null, null, null, null));

        // Act
        Task Act() => _sut.UpdateCentralIdpUrl(companyInvitation.Id);
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("ClientId must not be null");
    }

    [Fact]
    public async Task UpdateCentralIdpUrl_WithIdpNotSet_ThrowsConflictException()
    {
        // Arrange
        var companyInvitation = _fixture.Create<CompanyInvitation>();
        A.CallTo(() => _companyInvitationRepository.GetUpdateCentralIdpUrlData(companyInvitation.Id))
            .Returns(("testCorp", null, null, null, null, null));

        // Act
        Task Act() => _sut.UpdateCentralIdpUrl(companyInvitation.Id);
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("Idp name must not be null");
    }

    #endregion

    #region CreateCentralIdpOrgMapper

    [Fact]
    public async Task CreateCentralIdpOrgMapper_WithValid_ReturnsExpected()
    {
        // Arrange
        var companyInvitation = _fixture.Create<CompanyInvitation>();
        A.CallTo(() => _companyInvitationRepository.GetIdpAndOrgName(companyInvitation.Id))
            .Returns((true, "testCorp", "idp1"));

        // Act
        var result = await _sut.CreateCentralIdpOrgMapper(companyInvitation.Id);

        // Assert
        A.CallTo(() => _idpManagement.CreateCentralIdentityProviderOrganisationMapperAsync("idp1", "testCorp"))
            .MustHaveHappenedOnceExactly();
        result.modified.Should().BeTrue();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().ContainSingle()
            .Which.Should().Be(ProcessStepTypeId.INVITATION_UPDATE_CENTRAL_IDP_URLS);
    }

    [Fact]
    public async Task CreateCentralIdpOrgMapper_WithNotExisting_ThrowsConflictException()
    {
        // Arrange
        var companyInvitation = _fixture.Create<CompanyInvitation>();
        A.CallTo(() => _companyInvitationRepository.GetIdpAndOrgName(companyInvitation.Id))
            .Returns((true, "testCorp", null));

        // Act
        Task Act() => _sut.CreateCentralIdpOrgMapper(companyInvitation.Id);
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("Idp name must not be null");
    }

    #endregion

    #region CreateSharedIdpRealmIdpClient

    [Fact]
    public async Task CreateSharedIdpRealmIdpClient_WithValid_ReturnsExpected()
    {
        // Arrange
        var companyInvitation = _fixture.Create<CompanyInvitation>();
        var password = _fixture.Create<string>();
        var helper = _setting.Value.EncryptionConfigs.GetCryptoHelper(_setting.Value.EncryptionConfigIndex);
        var (secret, initializationVector) = helper.Encrypt(password);
        A.CallTo(() => _companyInvitationRepository.GetUpdateCentralIdpUrlData(companyInvitation.Id))
            .Returns(("testCorp", "idp1", "cl1", secret, initializationVector, _setting.Value.EncryptionConfigIndex));

        // Act
        var result = await _sut.CreateSharedIdpRealm(companyInvitation.Id);

        // Assert
        A.CallTo(() => _idpManagement.CreateSharedRealmIdpClientAsync("idp1", "TestLoginTheme", "testCorp", "cl1", password))
            .MustHaveHappenedOnceExactly();
        result.modified.Should().BeTrue();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().ContainSingle()
            .Which.Should().Be(ProcessStepTypeId.INVITATION_CREATE_CENTRAL_IDP_ORG_MAPPER);
    }

    [Fact]
    public async Task CreateSharedIdpRealmIdpClient_WithClientSecretNotSet_ThrowsConflictException()
    {
        // Arrange
        var companyInvitation = _fixture.Create<CompanyInvitation>();
        A.CallTo(() => _companyInvitationRepository.GetUpdateCentralIdpUrlData(companyInvitation.Id))
            .Returns(("testCorp", "cl1", "idp1", null, null, 0));

        // Act
        Task Act() => _sut.CreateSharedIdpRealm(companyInvitation.Id);
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("ClientSecret must not be null");
    }

    [Fact]
    public async Task CreateSharedIdpRealmIdpClient_WithClientIdNotSet_ThrowsConflictException()
    {
        // Arrange
        var companyInvitation = _fixture.Create<CompanyInvitation>();
        A.CallTo(() => _companyInvitationRepository.GetUpdateCentralIdpUrlData(companyInvitation.Id))
            .Returns(("testCorp", "cl1", null, null, null, null));

        // Act
        Task Act() => _sut.CreateSharedIdpRealm(companyInvitation.Id);
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("ClientId must not be null");
    }

    [Fact]
    public async Task CreateSharedIdpRealmIdpClient_WithIdpNotSet_ThrowsConflictException()
    {
        // Arrange
        var companyInvitation = _fixture.Create<CompanyInvitation>();
        A.CallTo(() => _companyInvitationRepository.GetUpdateCentralIdpUrlData(companyInvitation.Id))
            .Returns(("testCorp", null, null, null, null, null));

        // Act
        Task Act() => _sut.CreateSharedIdpRealm(companyInvitation.Id);
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("Idp name must not be null");
    }

    #endregion

    #region CreateSharedClient

    [Fact]
    public async Task CreateSharedClient_WithValid_ReturnsExpected()
    {
        // Arrange
        var companyInvitation = _fixture.Create<CompanyInvitation>();
        var password = _fixture.Create<string>();
        var helper = _setting.Value.EncryptionConfigs.GetCryptoHelper(_setting.Value.EncryptionConfigIndex);
        var (secret, initializationVector) = helper.Encrypt(password);
        A.CallTo(() => _companyInvitationRepository.GetUpdateCentralIdpUrlData(companyInvitation.Id))
            .Returns(("testCorp", "idp1", "cl1", secret, initializationVector, _setting.Value.EncryptionConfigIndex));

        // Act
        var result = await _sut.CreateSharedClient(companyInvitation.Id);

        // Assert
        A.CallTo(() => _idpManagement.CreateSharedClientAsync("idp1", "cl1", password))
            .MustHaveHappenedOnceExactly();
        result.modified.Should().BeTrue();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().ContainSingle()
            .Which.Should().Be(ProcessStepTypeId.INVITATION_ENABLE_CENTRAL_IDP);
    }

    [Fact]
    public async Task CreateSharedClient_WithClientSecretNotSet_ThrowsConflictException()
    {
        // Arrange
        var companyInvitation = _fixture.Create<CompanyInvitation>();
        A.CallTo(() => _companyInvitationRepository.GetUpdateCentralIdpUrlData(companyInvitation.Id))
            .Returns(("testCorp", "cl1", "idp1", null, null, 0));

        // Act
        Task Act() => _sut.CreateSharedClient(companyInvitation.Id);
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("ClientSecret must not be null");
    }

    [Fact]
    public async Task CreateSharedClient_WithEncryptionModeNotSet_ThrowsConflictException()
    {
        // Arrange
        var companyInvitation = _fixture.Create<CompanyInvitation>();
        A.CallTo(() => _companyInvitationRepository.GetUpdateCentralIdpUrlData(companyInvitation.Id))
            .Returns(("testCorp", "cl1", "idp1", null, null, null));

        // Act
        Task Act() => _sut.CreateSharedClient(companyInvitation.Id);
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("EncryptionMode must not be null");
    }

    [Fact]
    public async Task CreateSharedClient_WithClientIdNotSet_ThrowsConflictException()
    {
        // Arrange
        var companyInvitation = _fixture.Create<CompanyInvitation>();
        A.CallTo(() => _companyInvitationRepository.GetUpdateCentralIdpUrlData(companyInvitation.Id))
            .Returns(("testCorp", "cl1", null, null, null, null));

        // Act
        Task Act() => _sut.CreateSharedClient(companyInvitation.Id);
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("ClientId must not be null");
    }

    [Fact]
    public async Task CreateSharedClient_WithIdpNotSet_ThrowsConflictException()
    {
        // Arrange
        var companyInvitation = _fixture.Create<CompanyInvitation>();
        A.CallTo(() => _companyInvitationRepository.GetUpdateCentralIdpUrlData(companyInvitation.Id))
            .Returns(("testCorp", null, null, null, null, null));

        // Act
        Task Act() => _sut.CreateSharedClient(companyInvitation.Id);
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("Idp name must not be null");
    }

    #endregion

    #region EnableCentralIdp

    [Fact]
    public async Task EnableCentralIdp_WithValid_ReturnsExpected()
    {
        // Arrange
        var companyInvitation = _fixture.Create<CompanyInvitation>();
        A.CallTo(() => _companyInvitationRepository.GetIdpNameForInvitationId(companyInvitation.Id))
            .Returns("idp123");

        // Act
        var result = await _sut.EnableCentralIdp(companyInvitation.Id);

        // Assert
        A.CallTo(() => _idpManagement.EnableCentralIdentityProviderAsync("idp123"))
            .MustHaveHappenedOnceExactly();

        result.modified.Should().BeTrue();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().ContainSingle()
            .Which.Should().Be(ProcessStepTypeId.INVITATION_CREATE_DATABASE_IDP);
    }

    [Fact]
    public async Task EnableCentralIdp_WithNotExisting_ThrowsConflictException()
    {
        // Arrange
        var companyInvitation = _fixture.Create<CompanyInvitation>();
        A.CallTo(() => _companyInvitationRepository.GetIdpNameForInvitationId(companyInvitation.Id))
            .Returns((string?)null);

        // Act
        Task Act() => _sut.EnableCentralIdp(companyInvitation.Id);
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("Idp name must not be null");
    }

    #endregion

    #region CreateIdpDatabase

    [Fact]
    public async Task CreateIdpDatabase_WithValid_ReturnsExpected()
    {
        // Arrange
        var companyInvitation = _fixture.Create<CompanyInvitation>();
        var companyId = Guid.NewGuid();
        var idpId = Guid.NewGuid();
        var companyIdentityProviders = new List<CompanyIdentityProvider>();

        A.CallTo(() => _companyInvitationRepository.GetIdpAndCompanyId(companyInvitation.Id))
            .Returns((true, companyId, "cl1-testCorp"));
        A.CallTo(() => _identityProviderRepository.CreateIdentityProvider(A<IdentityProviderCategoryId>._, A<IdentityProviderTypeId>._, A<Guid>._, A<Action<IdentityProvider>>._))
            .ReturnsLazily((IdentityProviderCategoryId identityProviderCategoryId, IdentityProviderTypeId identityProviderTypeId, Guid ownerId, Action<IdentityProvider>? setOptionalFields) =>
            {
                var identityProvider = new IdentityProvider(idpId, identityProviderCategoryId, identityProviderTypeId, ownerId, DateTimeOffset.UtcNow);
                setOptionalFields?.Invoke(identityProvider);
                return identityProvider;
            });
        A.CallTo(() => _identityProviderRepository.CreateCompanyIdentityProvider(A<Guid>._, A<Guid>._))
            .ReturnsLazily((Guid cId, Guid identityProviderId) =>
            {
                var companyIdp = new CompanyIdentityProvider(cId, identityProviderId);
                companyIdentityProviders.Add(companyIdp);
                return companyIdp;
            });

        // Act
        var result = await _sut.CreateIdpDatabase(companyInvitation.Id);

        // Assert
        A.CallTo(() => _identityProviderRepository.CreateIdentityProvider(IdentityProviderCategoryId.KEYCLOAK_OIDC, IdentityProviderTypeId.SHARED, companyId, null))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _identityProviderRepository.CreateIamIdentityProvider(idpId, "cl1-testCorp"))
            .MustHaveHappenedOnceExactly();
        result.modified.Should().BeTrue();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().ContainSingle()
            .Which.Should().Be(ProcessStepTypeId.INVITATION_CREATE_USER);
        companyIdentityProviders.Should().ContainSingle()
            .And.Satisfy(x => x.CompanyId == companyId && x.IdentityProviderId == idpId);
    }

    [Fact]
    public async Task CreateIdpDatabase_WithNotExisting_ThrowsNotFoundException()
    {
        // Arrange
        var companyInvitation = _fixture.Create<CompanyInvitation>();
        A.CallTo(() => _companyInvitationRepository.GetIdpAndCompanyId(companyInvitation.Id))
            .Returns((false, Guid.NewGuid(), null));

        // Act
        Task Act() => _sut.CreateIdpDatabase(companyInvitation.Id);
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);

        // Assert
        ex.Message.Should().Be($"CompanyInvitation {companyInvitation.Id} does not exist");
    }

    [Fact]
    public async Task CreateIdpDatabase_WithIdpNotSet_ThrowsConflictException()
    {
        // Arrange
        var companyInvitation = _fixture.Create<CompanyInvitation>();
        A.CallTo(() => _companyInvitationRepository.GetIdpAndCompanyId(companyInvitation.Id))
            .Returns((true, Guid.NewGuid(), null));

        // Act
        Task Act() => _sut.CreateIdpDatabase(companyInvitation.Id);
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("IdpName must be set for the company invitation");
    }

    #endregion

    #region CreateUser

    [Fact]
    public async Task CreateUser_WithValid_ReturnsExpected()
    {
        // Arrange
        var companyInvitation = _fixture.Create<CompanyInvitation>();
        var applicationId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var companyUserId = Guid.NewGuid();
        A.CallTo(() => _companyInvitationRepository.GetInvitationUserData(companyInvitation.Id))
            .Returns((true, applicationId, companyId, "testCorp", Enumerable.Repeat((Guid.NewGuid(), "idp"), 1), new UserInvitationInformation("tony", "stark", "tony@stark.com", "ironman")));
        A.CallTo(() => _userProvisioningService.GetRoleDatas(A<IEnumerable<UserRoleConfig>>._))
            .Returns(Enumerable.Repeat(new UserRoleData(Guid.NewGuid(), "cl1", "ur 1"), 1).ToAsyncEnumerable());
        A.CallTo(() => _userProvisioningService.CreateOwnCompanyIdpUsersAsync(A<CompanyNameIdpAliasData>._, A<IAsyncEnumerable<UserCreationRoleDataIdpInfo>>._, A<Action<UserCreationCallbackData>>._, A<CancellationToken>._))
            .Returns(Enumerable.Repeat<ValueTuple<Guid, string, string?, Exception?>>((companyUserId, "ironman", "testPw", null), 1).ToAsyncEnumerable());
        A.CallTo(() => _companyInvitationRepository.AttachAndModifyCompanyInvitation(companyInvitation.Id, A<Action<CompanyInvitation>>._, A<Action<CompanyInvitation>>._))
            .Invokes((Guid _, Action<CompanyInvitation>? initialize, Action<CompanyInvitation> modify) =>
            {
                initialize?.Invoke(companyInvitation);
                modify(companyInvitation);
            });

        // Act
        var result = await _sut.CreateUser(companyInvitation.Id, CancellationToken.None);

        // Assert
        result.modified.Should().BeTrue();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().BeNull();
    }

    [Fact]
    public async Task CreateUser_WithCreateOwnCompanyIdpUserThrowsException_ThrowsException()
    {
        // Arrange
        var companyInvitation = _fixture.Create<CompanyInvitation>();
        var applicationId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        A.CallTo(() => _companyInvitationRepository.GetInvitationUserData(companyInvitation.Id))
            .Returns((true, applicationId, companyId, "testCorp", Enumerable.Repeat((Guid.NewGuid(), "idp"), 1), new UserInvitationInformation("tony", "stark", "tony@stark.com", "ironman")));
        A.CallTo(() => _userProvisioningService.GetRoleDatas(A<IEnumerable<UserRoleConfig>>._))
            .Returns(Enumerable.Repeat(new UserRoleData(Guid.NewGuid(), "cl1", "ur 1"), 1).ToAsyncEnumerable());
        A.CallTo(() => _userProvisioningService.CreateOwnCompanyIdpUsersAsync(A<CompanyNameIdpAliasData>._, A<IAsyncEnumerable<UserCreationRoleDataIdpInfo>>._, A<Action<UserCreationCallbackData>>._, A<CancellationToken>._))
            .Returns(Enumerable.Repeat<ValueTuple<Guid, string, string?, Exception?>>((companyId, "ironman", "testPw", new ConflictException("test")), 1).ToAsyncEnumerable());

        // Act
        Task Act() => _sut.CreateUser(companyInvitation.Id, CancellationToken.None);
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("test");
    }

    [Fact]
    public async Task CreateUser_WithNotExisting_ThrowsNotFoundException()
    {
        // Arrange
        var companyInvitation = _fixture.Create<CompanyInvitation>();
        A.CallTo(() => _companyInvitationRepository.GetInvitationUserData(companyInvitation.Id))
            .Returns((false, null, null, string.Empty, Enumerable.Empty<ValueTuple<Guid, string>>(), null!));

        // Act
        Task Act() => _sut.CreateUser(companyInvitation.Id, CancellationToken.None);
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);

        // Assert
        ex.Message.Should().Be($"CompanyInvitation {companyInvitation.Id} does not exist");
    }

    [Fact]
    public async Task CreateUser_WithoutApplication_ThrowsConflictException()
    {
        // Arrange
        var companyInvitation = _fixture.Create<CompanyInvitation>();
        A.CallTo(() => _companyInvitationRepository.GetInvitationUserData(companyInvitation.Id))
            .Returns((true, null, null, string.Empty, Enumerable.Empty<ValueTuple<Guid, string>>(), null!));

        // Act
        Task Act() => _sut.CreateUser(companyInvitation.Id, CancellationToken.None);
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("Application must be set for the company invitation");
    }

    [Fact]
    public async Task CreateUser_WithoutCompany_ThrowsConflictException()
    {
        // Arrange
        var companyInvitation = _fixture.Create<CompanyInvitation>();
        var applicationId = Guid.NewGuid();
        A.CallTo(() => _companyInvitationRepository.GetInvitationUserData(companyInvitation.Id))
            .Returns((true, applicationId, null, string.Empty, Enumerable.Empty<ValueTuple<Guid, string>>(), null!));

        // Act
        Task Act() => _sut.CreateUser(companyInvitation.Id, CancellationToken.None);
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("Company must be set for the company invitation");
    }

    [Fact]
    public async Task CreateUser_WithoutIdp_ThrowsConflictException()
    {
        // Arrange
        var companyInvitation = _fixture.Create<CompanyInvitation>();
        var applicationId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        A.CallTo(() => _companyInvitationRepository.GetInvitationUserData(companyInvitation.Id))
            .Returns((true, applicationId, companyId, "testCorp", Enumerable.Empty<ValueTuple<Guid, string>>(), null!));

        // Act
        Task Act() => _sut.CreateUser(companyInvitation.Id, CancellationToken.None);
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("There must only exist one idp for the company invitation");
    }

    [Fact]
    public async Task CreateUser_WithWrongUserRoles_ThrowsConfigurationException()
    {
        // Arrange
        var companyInvitation = _fixture.Create<CompanyInvitation>();
        var applicationId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        A.CallTo(() => _companyInvitationRepository.GetInvitationUserData(companyInvitation.Id))
            .Returns((true, applicationId, companyId, "testCorp", Enumerable.Repeat((Guid.NewGuid(), "idp"), 1), new UserInvitationInformation("tony", "stark", "tony@stark.com", "ironman")));
        A.CallTo(() => _userProvisioningService.GetRoleDatas(A<IEnumerable<UserRoleConfig>>._))
            .Throws(new ConflictException("test"));

        // Act
        Task Act() => _sut.CreateUser(companyInvitation.Id, CancellationToken.None);
        var ex = await Assert.ThrowsAsync<ConfigurationException>(Act);

        // Assert
        ex.Message.Should().Be("InvitedUserInitialRoles: test");
    }

    #endregion
}
