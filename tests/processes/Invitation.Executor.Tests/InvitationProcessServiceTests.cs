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
using Org.Eclipse.TractusX.Portal.Backend.ExternalSystems.Provisioning.Library;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Encryption;
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

        var portalRepositories = A.Fake<IPortalRepositories>();
        _companyInvitationRepository = A.Fake<ICompanyInvitationRepository>();
        _companyRepository = A.Fake<ICompanyRepository>();
        _identityProviderRepository = A.Fake<IIdentityProviderRepository>();
        _applicationRepository = A.Fake<IApplicationRepository>();

        _idpManagement = A.Fake<IIdpManagement>();
        _userProvisioningService = A.Fake<IUserProvisioningService>();
        _mailingProcessCreation = A.Fake<IMailingProcessCreation>();

        A.CallTo(() => portalRepositories.GetInstance<ICompanyInvitationRepository>())
            .Returns(_companyInvitationRepository);
        A.CallTo(() => portalRepositories.GetInstance<ICompanyRepository>())
            .Returns(_companyRepository);
        A.CallTo(() => portalRepositories.GetInstance<IIdentityProviderRepository>())
            .Returns(_identityProviderRepository);
        A.CallTo(() => portalRepositories.GetInstance<IApplicationRepository>())
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
            portalRepositories,
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
        var result = await _sut.CreateCentralIdp(companyInvitation.Id).ConfigureAwait(false);

        // Act
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
        async Task Act() => await _sut.CreateCentralIdp(companyInvitation.Id).ConfigureAwait(false);
        var ex = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);

        // Act
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
        var result = await _sut.CreateSharedIdpServiceAccount(companyInvitation.Id).ConfigureAwait(false);

        // Act
        companyInvitation.ClientId.Should().Be("cl1");
        companyInvitation.ClientSecret.Should().NotBeNull();
        result.modified.Should().BeTrue();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().ContainSingle()
            .Which.Should().Be(ProcessStepTypeId.INVITATION_ADD_REALM_ROLE);
    }

    [Fact]
    public async Task CreateSharedIdpServiceAccount_WithNotExisting_ThrowsConflictException()
    {
        // Arrange
        var companyInvitation = _fixture.Create<CompanyInvitation>();
        A.CallTo(() => _companyInvitationRepository.GetIdpNameForInvitationId(companyInvitation.Id))
            .Returns((string?)null);

        // Act
        async Task Act() => await _sut.CreateSharedIdpServiceAccount(companyInvitation.Id).ConfigureAwait(false);
        var ex = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);

        // Act
        ex.Message.Should().Be("Idp name must not be null");
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
        var result = await _sut.AddRealmRoleMappingsToUserAsync(companyInvitationId).ConfigureAwait(false);

        // Act
        result.modified.Should().BeTrue();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().ContainSingle()
            .Which.Should().Be(ProcessStepTypeId.INVITATION_UPDATE_CENTRAL_IDP_URLS);
    }

    [Fact]
    public async Task AddRealmRoleMappingsToUserAsync_WithNotExisting_ThrowsConflictException()
    {
        // Arrange
        var companyInvitation = _fixture.Create<CompanyInvitation>();
        A.CallTo(() => _companyInvitationRepository.GetServiceAccountUserIdForInvitation(companyInvitation.Id))
            .Returns((string?)null);

        // Act
        async Task Act() => await _sut.AddRealmRoleMappingsToUserAsync(companyInvitation.Id).ConfigureAwait(false);
        var ex = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);

        // Act
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
        var (secret, initializationVector) = CryptoHelper.Encrypt(password, _encryptionKey, CipherMode.CBC, PaddingMode.PKCS7);
        A.CallTo(() => _companyInvitationRepository.GetUpdateCentralIdpUrlData(companyInvitation.Id))
            .Returns(("testCorp", "idp1", "cl1", secret, initializationVector, 0));

        // Act
        var result = await _sut.UpdateCentralIdpUrl(companyInvitation.Id).ConfigureAwait(false);

        // Act
        A.CallTo(() => _idpManagement.UpdateCentralIdentityProviderUrlsAsync("idp1", "testCorp", "TestLoginTheme", "cl1", password))
            .MustHaveHappenedOnceExactly();
        result.modified.Should().BeTrue();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().ContainSingle()
            .Which.Should().Be(ProcessStepTypeId.INVITATION_CREATE_CENTRAL_IDP_ORG_MAPPER);
    }

    [Fact]
    public async Task UpdateCentralIdpUrl_WithClientSecretNotSet_ThrowsConflictException()
    {
        // Arrange
        var companyInvitation = _fixture.Create<CompanyInvitation>();
        A.CallTo(() => _companyInvitationRepository.GetUpdateCentralIdpUrlData(companyInvitation.Id))
            .Returns(("testCorp", "cl1", "idp1", null, null, null));

        // Act
        async Task Act() => await _sut.UpdateCentralIdpUrl(companyInvitation.Id).ConfigureAwait(false);
        var ex = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);

        // Act
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
        async Task Act() => await _sut.UpdateCentralIdpUrl(companyInvitation.Id).ConfigureAwait(false);
        var ex = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);

        // Act
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
        async Task Act() => await _sut.UpdateCentralIdpUrl(companyInvitation.Id).ConfigureAwait(false);
        var ex = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);

        // Act
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
        var result = await _sut.CreateCentralIdpOrgMapper(companyInvitation.Id).ConfigureAwait(false);

        // Act
        A.CallTo(() => _idpManagement.CreateCentralIdentityProviderOrganisationMapperAsync("idp1", "testCorp"))
            .MustHaveHappenedOnceExactly();
        result.modified.Should().BeTrue();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().ContainSingle()
            .Which.Should().Be(ProcessStepTypeId.INVITATION_CREATE_SHARED_REALM);
    }

    [Fact]
    public async Task CreateCentralIdpOrgMapper_WithNotExisting_ThrowsConflictException()
    {
        // Arrange
        var companyInvitation = _fixture.Create<CompanyInvitation>();
        A.CallTo(() => _companyInvitationRepository.GetIdpAndOrgName(companyInvitation.Id))
            .Returns((true, "testCorp", null));

        // Act
        async Task Act() => await _sut.CreateCentralIdpOrgMapper(companyInvitation.Id).ConfigureAwait(false);
        var ex = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);

        // Act
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
        var (secret, initializationVector) = CryptoHelper.Encrypt(password, _encryptionKey, CipherMode.CBC, PaddingMode.PKCS7);
        A.CallTo(() => _companyInvitationRepository.GetUpdateCentralIdpUrlData(companyInvitation.Id))
            .Returns(("testCorp", "idp1", "cl1", secret, initializationVector, 0));

        // Act
        var result = await _sut.CreateSharedIdpRealm(companyInvitation.Id).ConfigureAwait(false);

        // Act
        A.CallTo(() => _idpManagement.CreateSharedRealmIdpClientAsync("idp1", "TestLoginTheme", "testCorp", "cl1", password))
            .MustHaveHappenedOnceExactly();
        result.modified.Should().BeTrue();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().ContainSingle()
            .Which.Should().Be(ProcessStepTypeId.INVITATION_CREATE_SHARED_CLIENT);
    }

    [Fact]
    public async Task CreateSharedIdpRealmIdpClient_WithClientSecretNotSet_ThrowsConflictException()
    {
        // Arrange
        var companyInvitation = _fixture.Create<CompanyInvitation>();
        A.CallTo(() => _companyInvitationRepository.GetUpdateCentralIdpUrlData(companyInvitation.Id))
            .Returns(("testCorp", "cl1", "idp1", null, null, null));

        // Act
        async Task Act() => await _sut.CreateSharedIdpRealm(companyInvitation.Id).ConfigureAwait(false);
        var ex = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);

        // Act
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
        async Task Act() => await _sut.CreateSharedIdpRealm(companyInvitation.Id).ConfigureAwait(false);
        var ex = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);

        // Act
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
        async Task Act() => await _sut.CreateSharedIdpRealm(companyInvitation.Id).ConfigureAwait(false);
        var ex = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);

        // Act
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
        var (secret, initializationVector) = CryptoHelper.Encrypt(password, _encryptionKey, CipherMode.CBC, PaddingMode.PKCS7);
        A.CallTo(() => _companyInvitationRepository.GetUpdateCentralIdpUrlData(companyInvitation.Id))
            .Returns(("testCorp", "idp1", "cl1", secret, initializationVector, 0));

        // Act
        var result = await _sut.CreateSharedClient(companyInvitation.Id).ConfigureAwait(false);

        // Act
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
            .Returns(("testCorp", "cl1", "idp1", null, null, null));

        // Act
        async Task Act() => await _sut.CreateSharedClient(companyInvitation.Id).ConfigureAwait(false);
        var ex = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);

        // Act
        ex.Message.Should().Be("ClientSecret must not be null");
    }

    [Fact]
    public async Task CreateSharedClient_WithClientIdNotSet_ThrowsConflictException()
    {
        // Arrange
        var companyInvitation = _fixture.Create<CompanyInvitation>();
        A.CallTo(() => _companyInvitationRepository.GetUpdateCentralIdpUrlData(companyInvitation.Id))
            .Returns(("testCorp", "cl1", null, null, null, null));

        // Act
        async Task Act() => await _sut.CreateSharedClient(companyInvitation.Id).ConfigureAwait(false);
        var ex = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);

        // Act
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
        async Task Act() => await _sut.CreateSharedClient(companyInvitation.Id).ConfigureAwait(false);
        var ex = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);

        // Act
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
        var result = await _sut.EnableCentralIdp(companyInvitation.Id).ConfigureAwait(false);

        // Act
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
        async Task Act() => await _sut.EnableCentralIdp(companyInvitation.Id).ConfigureAwait(false);
        var ex = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);

        // Act
        ex.Message.Should().Be("Idp name must not be null");
    }

    #endregion

    #region CreateIdpDatabase

    [Fact]
    public async Task CreateIdpDatabase_WithValid_ReturnsExpected()
    {
        // Arrange
        var companyInvitation = _fixture.Create<CompanyInvitation>();
        var company = _fixture.Build<Company>().With(x => x.Name, "testCorp").With(x => x.CompanyWalletData, (CompanyWalletData?)null).Create();
        var applicationId = Guid.NewGuid();
        var idpId = Guid.NewGuid();

        A.CallTo(() => _companyInvitationRepository.GetIdpAndOrgName(companyInvitation.Id))
            .Returns((true, "testCorp", "cl1-testCorp"));
        A.CallTo(() => _companyRepository.CreateCompany("testCorp", A<Action<Company>>._)).Returns(company);
        A.CallTo(() => _companyInvitationRepository.AttachAndModifyCompanyInvitation(companyInvitation.Id, A<Action<CompanyInvitation>>._, A<Action<CompanyInvitation>>._))
            .Invokes((Guid _, Action<CompanyInvitation>? initialize, Action<CompanyInvitation> modify) =>
            {
                initialize?.Invoke(companyInvitation);
                modify(companyInvitation);
            });
        A.CallTo(() => _identityProviderRepository.CreateIdentityProvider(IdentityProviderCategoryId.KEYCLOAK_OIDC, IdentityProviderTypeId.SHARED, company.Id, null))
            .Returns(new IdentityProvider(idpId, IdentityProviderCategoryId.KEYCLOAK_OIDC, IdentityProviderTypeId.SHARED, company.Id, DateTimeOffset.UtcNow));
        A.CallTo(() => _applicationRepository.CreateCompanyApplication(company.Id, CompanyApplicationStatusId.CREATED, CompanyApplicationTypeId.INTERNAL, null))
            .Returns(new CompanyApplication(applicationId, company.Id, CompanyApplicationStatusId.CREATED, CompanyApplicationTypeId.INTERNAL, DateTimeOffset.UtcNow));
        A.CallTo(() => _companyInvitationRepository.AttachAndModifyCompanyInvitation(companyInvitation.Id, A<Action<CompanyInvitation>>._, A<Action<CompanyInvitation>>._))
            .Invokes((Guid _, Action<CompanyInvitation>? initialize, Action<CompanyInvitation> modify) =>
            {
                initialize?.Invoke(companyInvitation);
                modify(companyInvitation);
            });

        // Act
        var result = await _sut.CreateIdpDatabase(companyInvitation.Id).ConfigureAwait(false);

        // Act
        A.CallTo(() => _identityProviderRepository.CreateIdentityProvider(IdentityProviderCategoryId.KEYCLOAK_OIDC, IdentityProviderTypeId.SHARED, company.Id, null))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _identityProviderRepository.CreateIamIdentityProvider(idpId, "cl1-testCorp"))
            .MustHaveHappenedOnceExactly();
        companyInvitation.ApplicationId.Should().Be(applicationId);
        result.modified.Should().BeTrue();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().ContainSingle()
            .Which.Should().Be(ProcessStepTypeId.INVITATION_CREATE_USER);
    }

    [Fact]
    public async Task CreateIdpDatabase_WithNotExisting_ThrowsNotFoundException()
    {
        // Arrange
        var companyInvitation = _fixture.Create<CompanyInvitation>();
        A.CallTo(() => _companyInvitationRepository.GetIdpAndOrgName(companyInvitation.Id))
            .Returns((false, "testCorp", (string?)null));

        // Act
        async Task Act() => await _sut.CreateIdpDatabase(companyInvitation.Id).ConfigureAwait(false);
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act).ConfigureAwait(false);

        // Act
        ex.Message.Should().Be($"CompanyInvitation {companyInvitation.Id} does not exist");
    }

    [Fact]
    public async Task CreateIdpDatabase_WithIdpNotSet_ThrowsConflictException()
    {
        // Arrange
        var companyInvitation = _fixture.Create<CompanyInvitation>();
        A.CallTo(() => _companyInvitationRepository.GetIdpAndOrgName(companyInvitation.Id))
            .Returns((true, "testCorp", (string?)null));

        // Act
        async Task Act() => await _sut.CreateIdpDatabase(companyInvitation.Id).ConfigureAwait(false);
        var ex = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);

        // Act
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
        A.CallTo(() => _userProvisioningService.CreateOwnCompanyIdpUsersAsync(A<CompanyNameIdpAliasData>._, A<IAsyncEnumerable<UserCreationRoleDataIdpInfo>>._, A<CancellationToken>._))
            .Returns(Enumerable.Repeat<ValueTuple<Guid, string, string?, Exception?>>((companyUserId, "ironman", "testPw", null), 1).ToAsyncEnumerable());
        A.CallTo(() => _companyInvitationRepository.AttachAndModifyCompanyInvitation(companyInvitation.Id, A<Action<CompanyInvitation>>._, A<Action<CompanyInvitation>>._))
            .Invokes((Guid _, Action<CompanyInvitation>? initialize, Action<CompanyInvitation> modify) =>
            {
                initialize?.Invoke(companyInvitation);
                modify(companyInvitation);
            });

        // Act
        var result = await _sut.CreateUser(companyInvitation.Id, CancellationToken.None).ConfigureAwait(false);

        // Act
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
        A.CallTo(() => _userProvisioningService.CreateOwnCompanyIdpUsersAsync(A<CompanyNameIdpAliasData>._, A<IAsyncEnumerable<UserCreationRoleDataIdpInfo>>._, A<CancellationToken>._))
            .Returns(Enumerable.Repeat<ValueTuple<Guid, string, string?, Exception?>>((companyId, "ironman", "testPw", new ConflictException("test")), 1).ToAsyncEnumerable());

        // Act
        async Task Act() => await _sut.CreateUser(companyInvitation.Id, CancellationToken.None).ConfigureAwait(false);
        var ex = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);

        // Act
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
        async Task Act() => await _sut.CreateUser(companyInvitation.Id, CancellationToken.None).ConfigureAwait(false);
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act).ConfigureAwait(false);

        // Act
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
        async Task Act() => await _sut.CreateUser(companyInvitation.Id, CancellationToken.None).ConfigureAwait(false);
        var ex = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);

        // Act
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
        async Task Act() => await _sut.CreateUser(companyInvitation.Id, CancellationToken.None).ConfigureAwait(false);
        var ex = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);

        // Act
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
        async Task Act() => await _sut.CreateUser(companyInvitation.Id, CancellationToken.None).ConfigureAwait(false);
        var ex = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);

        // Act
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
        async Task Act() => await _sut.CreateUser(companyInvitation.Id, CancellationToken.None).ConfigureAwait(false);
        var ex = await Assert.ThrowsAsync<ConfigurationException>(Act).ConfigureAwait(false);

        // Act
        ex.Message.Should().Be("InvitedUserInitialRoles: test");
    }

    #endregion
}
