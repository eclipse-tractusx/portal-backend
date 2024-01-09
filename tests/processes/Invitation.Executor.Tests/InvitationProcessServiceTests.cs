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
using Org.Eclipse.TractusX.Portal.Backend.Mailing.SendMail;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.Invitation.Executor.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;
using System.Security.Cryptography;
using System.Text;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.Invitation.Executor.Tests;

public class InvitationProcessServiceTests
{
    private readonly ICompanyInvitationRepository _companyInvitationRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IIdentityProviderRepository _identityProviderRepository;
    private readonly IApplicationRepository _applicationRepository;
    private readonly IIdpManagement idpManagement;
    private readonly IUserProvisioningService _userProvisioningService;
    private readonly IMailingService _mailingService;
    private readonly IInvitationProcessService _sut;
    private readonly IOptions<InvitationSettings> _setting;
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

        idpManagement = A.Fake<IIdpManagement>();
        _userProvisioningService = A.Fake<IUserProvisioningService>();
        _mailingService = A.Fake<IMailingService>();

        A.CallTo(() => portalRepositories.GetInstance<ICompanyInvitationRepository>())
            .Returns(_companyInvitationRepository);
        A.CallTo(() => portalRepositories.GetInstance<ICompanyRepository>())
            .Returns(_companyRepository);
        A.CallTo(() => portalRepositories.GetInstance<IIdentityProviderRepository>())
            .Returns(_identityProviderRepository);
        A.CallTo(() => portalRepositories.GetInstance<IApplicationRepository>())
            .Returns(_applicationRepository);

        _setting = Options.Create(new InvitationSettings
        {
            EncryptionKey = "test1234Test1234",
            InitialLoginTheme = "TestLoginTheme",
            PasswordResendAddress = "https://example.org/resend",
            RegistrationAppAddress = "https://example.org/registration",
            InvitedUserInitialRoles = Enumerable.Repeat(new UserRoleConfig("Cl1", new[]
            {
                "ur 1",
                "ur 2"
            }), 1),
        });

        _sut = new InvitationProcessService(
            idpManagement,
            _userProvisioningService,
            portalRepositories,
            _mailingService,
            _setting);
    }

    #region SetupIdp

    [Fact]
    public async Task SetupIdp_WithValid_ReturnsExpected()
    {
        // Arrange
        var companyInvitation = _fixture.Create<CompanyInvitation>();
        A.CallTo(() => _companyInvitationRepository.GetOrganisationNameForInvitation(companyInvitation.Id))
            .Returns("testCorp");
        A.CallTo(() => idpManagement.GetNextCentralIdentityProviderNameAsync())
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
    public async Task SetupIdp_WithNotExisting_ThrowsConflictException()
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

    #region CreateIdpDatabase

    [Fact]
    public async Task CreateIdpDatabase_WithValid_ReturnsExpected()
    {
        // Arrange
        var companyInvitation = _fixture.Create<CompanyInvitation>();
        var company = _fixture.Build<Company>().With(x => x.Name, "testCorp").Create();
        var applicationId = Guid.NewGuid();
        var idpId = Guid.NewGuid();

        A.CallTo(() => _companyInvitationRepository.GetInvitationIdpCreationData(companyInvitation.Id))
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
        A.CallTo(() => _companyInvitationRepository.GetInvitationIdpCreationData(companyInvitation.Id))
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
        A.CallTo(() => _companyInvitationRepository.GetInvitationIdpCreationData(companyInvitation.Id))
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
        companyInvitation.Password.Should().NotBeNull();
        result.modified.Should().BeTrue();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().ContainSingle()
            .Which.Should().Be(ProcessStepTypeId.INVITATION_SEND_MAIL);
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

    #region SendMail

    [Fact]
    public async Task SendMail_WithoutExisting_ThrowsNotFoundException()
    {
        // Arrange
        var companyInvitationId = Guid.NewGuid();
        A.CallTo(() => _companyInvitationRepository.GetMailData(companyInvitationId))
            .Returns((false, string.Empty, (byte[]?)null, string.Empty));
        A.CallTo(() => _userProvisioningService.GetRoleDatas(A<IEnumerable<UserRoleConfig>>._))
            .Throws(new ConflictException("test"));

        // Act
        async Task Act() => await _sut.SendMail(companyInvitationId).ConfigureAwait(false);
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act).ConfigureAwait(false);

        // Act
        ex.Message.Should().Be($"CompanyInvitation {companyInvitationId} does not exist");
    }

    [Fact]
    public async Task SendMail_WithWrongUserRoles_ThrowsConflictException()
    {
        // Arrange
        var companyInvitationId = Guid.NewGuid();
        A.CallTo(() => _companyInvitationRepository.GetMailData(companyInvitationId))
            .Returns((true, string.Empty, (byte[]?)null, string.Empty));
        A.CallTo(() => _userProvisioningService.GetRoleDatas(A<IEnumerable<UserRoleConfig>>._))
            .Throws(new ConflictException("test"));

        // Act
        async Task Act() => await _sut.SendMail(companyInvitationId).ConfigureAwait(false);
        var ex = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);

        // Act
        ex.Message.Should().Be("Password needs to be set");
    }

    [Fact]
    public async Task SendMail_WithValid_ThrowsConflictException()
    {
        // Arrange
        var companyInvitationId = Guid.NewGuid();
        var pw = "test";
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(_setting.Value.EncryptionKey);
        aes.Mode = CipherMode.ECB;
        aes.Padding = PaddingMode.PKCS7;
        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using (var memoryStream = new MemoryStream())
        {
            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
            {
                using var sw = new StreamWriter(cryptoStream, Encoding.UTF8);
                sw.Write(pw);
            }

            var secret = memoryStream.ToArray();
            A.CallTo(() => _companyInvitationRepository.GetMailData(companyInvitationId))
                .Returns((true, "testCorp", secret, "test@email.com"));
            A.CallTo(() => _userProvisioningService.GetRoleDatas(A<IEnumerable<UserRoleConfig>>._))
                .Throws(new ConflictException("test"));
        }

        // Act
        var result = await _sut.SendMail(companyInvitationId).ConfigureAwait(false);

        // Act
        A.CallTo(() => _mailingService.SendMails("test@email.com", A<IDictionary<string, string>>._, "RegistrationTemplate"))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _mailingService.SendMails("test@email.com", A<IDictionary<string, string>>._, "PasswordForRegistrationTemplate"))
            .MustHaveHappenedOnceExactly();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.modified.Should().BeTrue();
        result.nextStepTypeIds.Should().BeNull();
    }

    #endregion
}
