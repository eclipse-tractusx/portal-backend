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

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.IO;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using System.Text;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.BusinessLogic;

public class IdentityProviderBusinessLogicTests
{
    private readonly IFixture _fixture;
    private readonly IProvisioningManager _provisioningManager;
    private readonly ICompanyRepository _companyRepository;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IIdentityProviderRepository _identityProviderRepository;
    private readonly IUserRepository _userRepository;
    private readonly IOptions<IdentityProviderSettings> _options;
    private readonly IdentityProviderCsvSettings _csvSettings;
    private readonly IIdentityService _identityService;
    private readonly IFormFile _document;
    private readonly Encoding _encoding;
    private readonly Guid _companyId;
    private readonly Guid _invalidCompanyId;
    private readonly IdentityData _identity;
    private readonly Guid _sharedIdentityProviderId;
    private readonly string _sharedIdpAlias;
    private readonly Guid _otherIdentityProviderId;
    private readonly string _otherIdpAlias;
    private readonly Guid _identityProviderId;

    public IdentityProviderBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _provisioningManager = A.Fake<IProvisioningManager>();
        _portalRepositories = A.Fake<IPortalRepositories>();
        _companyRepository = A.Fake<ICompanyRepository>();
        _identityProviderRepository = A.Fake<IIdentityProviderRepository>();
        _userRepository = A.Fake<IUserRepository>();
        _identityService = A.Fake<IIdentityService>();
        _options = A.Fake<IOptions<IdentityProviderSettings>>();
        _document = A.Fake<IFormFile>();

        _companyId = _fixture.Create<Guid>();
        _invalidCompanyId = _fixture.Create<Guid>();
        _identityProviderId = _fixture.Create<Guid>();
        _sharedIdentityProviderId = _fixture.Create<Guid>();
        _sharedIdpAlias = _fixture.Create<string>();
        _otherIdentityProviderId = _fixture.Create<Guid>();
        _otherIdpAlias = _fixture.Create<string>();
        _encoding = _fixture.Create<Encoding>();
        _identity = new(Guid.NewGuid().ToString(), Guid.NewGuid(), IdentityTypeId.COMPANY_USER, _companyId);

        A.CallTo(() => _identityService.IdentityData).Returns(_identity);

        _csvSettings = new IdentityProviderCsvSettings
        {
            Charset = "",
            Encoding = _encoding,
            FileName = _fixture.Create<string>(),
            ContentType = "text/csv",
            Separator = ",",
            HeaderUserId = "UserId",
            HeaderFirstName = "FirstName",
            HeaderLastName = "LastName",
            HeaderEmail = "Email",
            HeaderProviderAlias = "ProviderAlias",
            HeaderProviderUserId = "ProviderUserId",
            HeaderProviderUserName = "ProviderUserName",
        };
    }

    #region UploadOwnCompanyUsersIdentityProviderLinkDataAsync

    [Fact]
    public async Task TestUploadOwnCompanyUsersIdentityProviderLinkDataAsyncAllUnchangedSuccess()
    {
        const int numUsers = 5;

        var users = _fixture.CreateMany<TestUserData>(numUsers).ToList();

        var lines = new[] { HeaderLine() }.Concat(users.Select(u => NextLine(u)));

        SetupFakes(users, lines);

        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _options);

        var result = await sut.UploadOwnCompanyUsersIdentityProviderLinkDataAsync(_document, _identity.CompanyId, CancellationToken.None).ConfigureAwait(false);

        result.Updated.Should().Be(0);
        result.Unchanged.Should().Be(numUsers);
        result.Error.Should().Be(0);
        result.Total.Should().Be(numUsers);
        result.Errors.Should().BeEmpty();

        A.CallTo(() => _provisioningManager.DeleteProviderUserLinkToCentralUserAsync(A<string>._, A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _provisioningManager.AddProviderUserLinkToCentralUserAsync(A<string>._, A<IdentityProviderLink>._)).MustNotHaveHappened();
        A.CallTo(() => _provisioningManager.UpdateCentralUserAsync(A<string>._, A<string>._, A<string>._, A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _provisioningManager.UpdateSharedRealmUserAsync(A<string>._, A<string>._, A<string>._, A<string>._, A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _userRepository.AttachAndModifyCompanyUser(A<Guid>._, null, A<Action<CompanyUser>>._)).MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
    }

    [Fact]
    public async Task TestUploadOwnCompanyUsersIdentityProviderLinkDataAsyncWrongContentTypeThrows()
    {
        const int numUsers = 1;

        var users = _fixture.CreateMany<TestUserData>(numUsers).ToList();

        var lines = new[] { HeaderLine() }.Concat(users.Select(u => NextLine(u)));

        SetupFakes(users, lines);

        A.CallTo(() => _document.ContentType).Returns(_fixture.Create<string>());

        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _options);

        async Task Act() => await sut.UploadOwnCompanyUsersIdentityProviderLinkDataAsync(_document, _identity.CompanyId, CancellationToken.None).ConfigureAwait(false);

        var error = await Assert.ThrowsAsync<UnsupportedMediaTypeException>(Act).ConfigureAwait(false);
        error.Message.Should().Be($"Only contentType {_csvSettings.ContentType} files are allowed.");
    }

    [Fact]
    public async Task TestUploadOwnCompanyUsersIdentityProviderLinkDataAsyncEmailChangedSuccess()
    {
        const int numUsers = 5;

        var changedEmail = _fixture.Create<string>();

        var unchanged = _fixture.Create<TestUserData>();
        var changed = unchanged with {Email = changedEmail};

        var users = new[] {
            _fixture.Create<TestUserData>(),
            _fixture.Create<TestUserData>(),
            unchanged,
            _fixture.Create<TestUserData>(),
            _fixture.Create<TestUserData>()
        };

        var lines = new[] { HeaderLine() }.Concat(users.Select((user, index) => NextLine(index == 2 ? changed : user)));

        SetupFakes(users, lines);

        var changedEmailResult = (string?)null;

        A.CallTo(() => _userRepository.AttachAndModifyCompanyUser(A<Guid>._, null, A<Action<CompanyUser>>._))
            .Invokes(x =>
            {
                var companyUserId = x.Arguments.Get<Guid>("companyUserId")!;
                var setOptionalParameters = x.Arguments.Get<Action<CompanyUser>>("setOptionalParameters");

                var companyUser = new CompanyUser(companyUserId);
                setOptionalParameters?.Invoke(companyUser);
                changedEmailResult = companyUser.Email;
            }
        );

        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _options);

        var result = await sut.UploadOwnCompanyUsersIdentityProviderLinkDataAsync(_document, _identity.CompanyId, CancellationToken.None).ConfigureAwait(false);

        result.Updated.Should().Be(1);
        result.Unchanged.Should().Be(numUsers - 1);
        result.Error.Should().Be(0);
        result.Total.Should().Be(numUsers);
        result.Errors.Should().BeEmpty();

        A.CallTo(() => _provisioningManager.DeleteProviderUserLinkToCentralUserAsync(A<string>._, A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _provisioningManager.AddProviderUserLinkToCentralUserAsync(A<string>._, A<IdentityProviderLink>._)).MustNotHaveHappened();
        A.CallTo(() => _provisioningManager.UpdateCentralUserAsync(A<string>._, A<string>._, A<string>._, A<string>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.UpdateSharedRealmUserAsync(A<string>._, A<string>._, A<string>._, A<string>._, A<string>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _userRepository.AttachAndModifyCompanyUser(A<Guid>._, null, A<Action<CompanyUser>>._)).MustHaveHappenedOnceExactly();

        changedEmailResult.Should().NotBeNull();
        changedEmailResult.Should().Be(changedEmail);
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappened();
    }

    [Fact]
    public async Task TestUploadOwnCompanyUsersIdentityProviderLinkDataAsyncSharedIdpLinkChangedError()
    {
        const int numUsers = 5;

        var unchanged = _fixture.Create<TestUserData>();
        var changed = unchanged with {SharedIdpUserName = _fixture.Create<string>()};

        var users = new[] {
            _fixture.Create<TestUserData>(),
            _fixture.Create<TestUserData>(),
            unchanged,
            _fixture.Create<TestUserData>(),
            _fixture.Create<TestUserData>()
        };

        var lines = new[] { HeaderLine() }.Concat(users.Select((user, index) => NextLine(index == 2 ? changed : user)));

        SetupFakes(users, lines);

        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _options);

        var result = await sut.UploadOwnCompanyUsersIdentityProviderLinkDataAsync(_document, _identity.CompanyId, CancellationToken.None).ConfigureAwait(false);

        result.Updated.Should().Be(0);
        result.Unchanged.Should().Be(numUsers - 1);
        result.Error.Should().Be(1);
        result.Total.Should().Be(numUsers);
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().Should().Be($"line: 3, message: unexpected update of shared identityProviderLink, alias '{_sharedIdpAlias}', companyUser '{changed.CompanyUserId}', providerUserId: '{changed.SharedIdpUserId}', providerUserName: '{changed.SharedIdpUserName}'");

        A.CallTo(() => _provisioningManager.DeleteProviderUserLinkToCentralUserAsync(A<string>._, A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _provisioningManager.AddProviderUserLinkToCentralUserAsync(A<string>._, A<IdentityProviderLink>._)).MustNotHaveHappened();
        A.CallTo(() => _provisioningManager.UpdateCentralUserAsync(A<string>._, A<string>._, A<string>._, A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _provisioningManager.UpdateSharedRealmUserAsync(A<string>._, A<string>._, A<string>._, A<string>._, A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _userRepository.AttachAndModifyCompanyUser(A<Guid>._, null, A<Action<CompanyUser>>._)).MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
    }

    [Fact]
    public async Task TestUploadOwnCompanyUsersIdentityProviderLinkDataAsyncOtherIdpLinkChangedSuccess()
    {
        const int numUsers = 5;

        var unchanged = _fixture.Create<TestUserData>();
        var changed = unchanged with {OtherIdpUserName = _fixture.Create<string>()};

        var users = new[] {
            _fixture.Create<TestUserData>(),
            _fixture.Create<TestUserData>(),
            unchanged,
            _fixture.Create<TestUserData>(),
            _fixture.Create<TestUserData>()
        };

        var lines = new[] { HeaderLine() }.Concat(users.Select((user, index) => NextLine(index == 2 ? changed : user)));

        SetupFakes(users, lines);

        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _options);

        var result = await sut.UploadOwnCompanyUsersIdentityProviderLinkDataAsync(_document, _identity.CompanyId, CancellationToken.None).ConfigureAwait(false);

        result.Updated.Should().Be(1);
        result.Unchanged.Should().Be(numUsers - 1);
        result.Error.Should().Be(0);
        result.Total.Should().Be(numUsers);
        result.Errors.Should().HaveCount(0);

        A.CallTo(() => _provisioningManager.DeleteProviderUserLinkToCentralUserAsync(A<string>._, A<string>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.AddProviderUserLinkToCentralUserAsync(A<string>._, A<IdentityProviderLink>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.UpdateCentralUserAsync(A<string>._, A<string>._, A<string>._, A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _provisioningManager.UpdateSharedRealmUserAsync(A<string>._, A<string>._, A<string>._, A<string>._, A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _userRepository.AttachAndModifyCompanyUser(A<Guid>._, null, A<Action<CompanyUser>>._)).MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
    }

    [Fact]
    public async Task TestUploadOwnCompanyUsersIdentityProviderLinkDataAsyncUnknownCompanyUserIdError()
    {
        const int numUsers = 5;

        var unchanged = _fixture.Create<TestUserData>();
        var unknown = unchanged with {CompanyUserId = _fixture.Create<Guid>()};

        var users = new[] {
            _fixture.Create<TestUserData>(),
            _fixture.Create<TestUserData>(),
            unchanged,
            _fixture.Create<TestUserData>(),
            _fixture.Create<TestUserData>()
        };

        var lines = new[] { HeaderLine() }.Concat(users.Select((user, index) => NextLine(index == 2 ? unknown : user)));

        SetupFakes(users, lines);

        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _options);

        var result = await sut.UploadOwnCompanyUsersIdentityProviderLinkDataAsync(_document, _identity.CompanyId, CancellationToken.None).ConfigureAwait(false);

        result.Updated.Should().Be(0);
        result.Unchanged.Should().Be(numUsers - 1);
        result.Error.Should().Be(1);
        result.Total.Should().Be(numUsers);
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().Should().Be($"line: 3, message: unexpected value of UserId: '{unknown.CompanyUserId}'");

        A.CallTo(() => _provisioningManager.DeleteProviderUserLinkToCentralUserAsync(A<string>._, A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _provisioningManager.AddProviderUserLinkToCentralUserAsync(A<string>._, A<IdentityProviderLink>._)).MustNotHaveHappened();
        A.CallTo(() => _provisioningManager.UpdateCentralUserAsync(A<string>._, A<string>._, A<string>._, A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _provisioningManager.UpdateSharedRealmUserAsync(A<string>._, A<string>._, A<string>._, A<string>._, A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _userRepository.AttachAndModifyCompanyUser(A<Guid>._, null, A<Action<CompanyUser>>._)).MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
    }

    #endregion

    #region CreateOwnCompanyIdentityProviderAsync

    [Fact]
    public async Task CreateOwnCompanyIdentityProviderAsync_WithNotSupportedProtocol_ThrowsControllerArgumentException()
    {
        // Arrange
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _options);

        // Act
        async Task Act() => await sut.CreateOwnCompanyIdentityProviderAsync(default, IdentityProviderTypeId.OWN, null).ConfigureAwait(false);
        
        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.ParamName.Should().Be("protocol");
    }

    [Theory]
    [InlineData("a")]
    [InlineData("this-is-a-very-long-dispaly-name-and-it-should-be-way-too-long")]
    public async Task CreateOwnCompanyIdentityProviderAsync_WithDisplayNameToLong_ThrowsControllerArgumentException(string display)
    {
        // Arrange
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _options);

        // Act
        async Task Act() => await sut.CreateOwnCompanyIdentityProviderAsync(IamIdentityProviderProtocol.SAML, IdentityProviderTypeId.OWN, display).ConfigureAwait(false);
        
        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be("displayName length must be 2-30 characters");
    }

    [Fact]
    public async Task CreateOwnCompanyIdentityProviderAsync_WithInvalidCharacterInDisplayName_ThrowsControllerArgumentException()
    {
        // Arrange
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _options);

        // Act
        async Task Act() => await sut.CreateOwnCompanyIdentityProviderAsync(IamIdentityProviderProtocol.SAML, IdentityProviderTypeId.OWN, "$invalid-character").ConfigureAwait(false);
        
        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be("allowed characters in displayName: 'a-zA-Z0-9!?@&#'\"()_-=/*.,;: '");
    }

    [Fact]
    public async Task CreateOwnCompanyIdentityProviderAsync_WithInvalidCompany_ThrowsControllerArgumentException()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        SetupCreateOwnCompanyIdentityProvider();
        A.CallTo(() => _identityService.IdentityData).Returns(_identity with { CompanyId = companyId });

        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _options);

        // Act
        async Task Act() => await sut.CreateOwnCompanyIdentityProviderAsync(IamIdentityProviderProtocol.SAML, IdentityProviderTypeId.OWN, null).ConfigureAwait(false);
        
        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be($"company {companyId} does not exist (Parameter 'companyId')");
        ex.ParamName.Should().Be("companyId");
    }

    [Fact]
    public async Task CreateOwnCompanyIdentityProviderAsync_WithNotAllowedCompanyForManaged_ThrowsForbiddenException()
    {
        // Arrange
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _options);

        SetupCreateOwnCompanyIdentityProvider();
        A.CallTo(() => _identityService.IdentityData).Returns(_identity with { CompanyId = _invalidCompanyId });

        // Act
        async Task Act() => await sut.CreateOwnCompanyIdentityProviderAsync(IamIdentityProviderProtocol.SAML, IdentityProviderTypeId.MANAGED, null).ConfigureAwait(false);
        
        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be("Not allowed to create a managed identity");
    }

    [Theory]
    [InlineData(IamIdentityProviderProtocol.SAML)]
    [InlineData(IamIdentityProviderProtocol.OIDC)]
    public async Task CreateOwnCompanyIdentityProviderAsync_WithValidData_ExecutesExpected(IamIdentityProviderProtocol protocol)
    {
        // Arrange
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _options);

        var idps = new List<IdentityProvider>();
        var companyIdps = new List<CompanyIdentityProvider>();
        var iamIdps = new List<IamIdentityProvider>();
        SetupCreateOwnCompanyIdentityProvider(protocol, idps, companyIdps, iamIdps);

        // Act
        var result = await sut.CreateOwnCompanyIdentityProviderAsync(protocol, IdentityProviderTypeId.OWN, "test-company").ConfigureAwait(false);
        
        // Assert
        A.CallTo(() => _provisioningManager.CreateOwnIdpAsync("test-company", "test", protocol)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();

        var expetcedProtocol = protocol == IamIdentityProviderProtocol.OIDC
            ? IdentityProviderCategoryId.KEYCLOAK_OIDC
            : IdentityProviderCategoryId.KEYCLOAK_SAML;
        idps.Should().HaveCount(1).And.Satisfy(x => x.OwnerId == _identity.CompanyId && x.IdentityProviderCategoryId == expetcedProtocol && x.IdentityProviderTypeId == IdentityProviderTypeId.OWN);
        companyIdps.Should().HaveCount(1).And.Satisfy(x => x.CompanyId == _companyId);
        iamIdps.Should().HaveCount(1);

        result.Should().NotBeNull();
        result.displayName.Should().Be(protocol == IamIdentityProviderProtocol.OIDC ? "test-oidc" : "test-saml");
        result.mappers.Should().HaveCount(3);
        result.enabled.Should().BeTrue();
        result.redirectUrl.Should().Be("https://redirect.com/*");
        if (protocol == IamIdentityProviderProtocol.OIDC)
        {
            result.saml.Should().BeNull();
            result.oidc.Should().NotBeNull();
            result.oidc!.clientAuthMethod.Should().Be(IamIdentityProviderClientAuthMethod.SECRET_JWT);
            result.oidc!.signatureAlgorithm.Should().Be(IamIdentityProviderSignatureAlgorithm.RS512);
        }
        else
        {
            result.oidc.Should().BeNull();
            result.saml.Should().NotBeNull();
            result.saml!.singleSignOnServiceUrl.Should().Be("https://sso.com");
        }
    }

    #endregion
    
    #region Setup

    private void SetupCreateOwnCompanyIdentityProvider(IamIdentityProviderProtocol protocol = IamIdentityProviderProtocol.OIDC, ICollection<IdentityProvider>? idps = null, ICollection<CompanyIdentityProvider>? companyIdps = null, ICollection<IamIdentityProvider>? iamIdps = null)
    {
        A.CallTo(() => _companyRepository.CheckCompanyAndIdentityTypeIdAsync(_identity.CompanyId, A<IdentityProviderTypeId>._))
            .Returns(new ValueTuple<bool, string, bool>(true, "test", true));
        A.CallTo(() => _companyRepository.CheckCompanyAndIdentityTypeIdAsync(_invalidCompanyId, IdentityProviderTypeId.MANAGED))
            .Returns(new ValueTuple<bool, string, bool>(true, "test", false));
        A.CallTo(() => _companyRepository.CheckCompanyAndIdentityTypeIdAsync(A<Guid>.That.Not.Matches(x => x == _identity.CompanyId || x == _invalidCompanyId), IdentityProviderTypeId.OWN))
            .Returns(new ValueTuple<bool, string, bool>());

        if (idps != null)
        {
            A.CallTo(() => _identityProviderRepository.CreateIdentityProvider(A<IdentityProviderCategoryId>._, A<IdentityProviderTypeId>._, A<Guid>._))
                .Invokes((IdentityProviderCategoryId identityProviderCategory, IdentityProviderTypeId identityProviderTypeId, Guid ownerId) =>
                {
                    var idp = new IdentityProvider(_identityProviderId, identityProviderCategory, identityProviderTypeId, ownerId, DateTimeOffset.UtcNow);
                    idps.Add(idp);
                });    
        }

        if (companyIdps != null)
        {
            A.CallTo(() => _identityProviderRepository.CreateCompanyIdentityProvider(A<Guid>._, A<Guid>._))
                .Invokes((Guid companyId, Guid identityProviderId) =>
                {
                    var companyIdp = new CompanyIdentityProvider(companyId, identityProviderId);
                    companyIdps.Add(companyIdp);
                });
        }
        
        if (iamIdps != null)
        {
            A.CallTo(() => _identityProviderRepository.CreateIamIdentityProvider(A<Guid>._, A<string>._))
                .Invokes((Guid identityProviderId, string idpAlias) =>
                {
                    var iamIdp = new IamIdentityProvider(idpAlias, identityProviderId);
                    iamIdps.Add(iamIdp);
                });    
        }

        if (protocol == IamIdentityProviderProtocol.OIDC)
        {
            A.CallTo(() => _provisioningManager.GetCentralIdentityProviderDataOIDCAsync(A<string>._))
                .Returns(new IdentityProviderConfigOidc("test-oidc", "https://redirect.com/*", "cl1-oidc", true, "https://auth.com", IamIdentityProviderClientAuthMethod.SECRET_JWT, IamIdentityProviderSignatureAlgorithm.RS512));
        }
        else
        {
            A.CallTo(() => _provisioningManager.GetCentralIdentityProviderDataSAMLAsync(A<string>._))
                .Returns(new IdentityProviderConfigSaml("test-saml", "https://redirect.com/*", "cl1-saml", true, Guid.NewGuid().ToString(), "https://sso.com"));
        }

        A.CallTo(() => _provisioningManager.GetIdentityProviderMappers(A<string>._))
            .Returns(_fixture.CreateMany<IdentityProviderMapperModel>(3).ToAsyncEnumerable());
        
        A.CallTo(() => _portalRepositories.GetInstance<IIdentityProviderRepository>()).Returns(_identityProviderRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>()).Returns(_companyRepository);
    }

    private void SetupFakes(IEnumerable<TestUserData> userData, IEnumerable<string> lines)
    {
        A.CallTo(() => _options.Value).Returns(new IdentityProviderSettings { CsvSettings = _csvSettings });

        A.CallTo(() => _document.ContentType).Returns(_options.Value.CsvSettings.ContentType);
        A.CallTo(() => _document.OpenReadStream()).ReturnsLazily(() => new AsyncEnumerableStringStream(lines.ToAsyncEnumerable(), _encoding));

        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IIdentityProviderRepository>()).Returns(_identityProviderRepository);

        A.CallTo(() => _userRepository.GetUserEntityDataAsync(A<Guid>._, A<Guid>._)).ReturnsLazily((Guid companyUserId, Guid _) =>
            userData.Where(d => d.CompanyUserId == companyUserId)
                .Select(d =>
                    (
                        UserEntityId: d.UserEntityId,
                        FirstName: d.FirstName,
                        LastName: d.LastName,
                        Email: d.Email
                    )).FirstOrDefault());

        A.CallTo(() => _identityProviderRepository.GetCompanyIdentityProviderCategoryDataUntracked(A<Guid>.That.Not.IsEqualTo(_companyId))).Returns(
            Enumerable.Empty<(Guid IdentityProviderId, IdentityProviderCategoryId CategoryId, string Alias)>().ToAsyncEnumerable());
        A.CallTo(() => _identityProviderRepository.GetCompanyIdentityProviderCategoryDataUntracked(A<Guid>.That.IsEqualTo(_companyId))).Returns(
            new[] {
                (IdentityProviderId: _sharedIdentityProviderId, CategoryId: IdentityProviderCategoryId.KEYCLOAK_SHARED, Alias: _sharedIdpAlias),
                (IdentityProviderId: _otherIdentityProviderId, CategoryId: IdentityProviderCategoryId.KEYCLOAK_OIDC, Alias: _otherIdpAlias),
            }.ToAsyncEnumerable());

        A.CallTo(() => _identityProviderRepository.CreateIdentityProvider(A<IdentityProviderCategoryId>._, A<IdentityProviderTypeId>._, A<Guid>._))
            .ReturnsLazily((IdentityProviderCategoryId categoryId, IdentityProviderTypeId typeId, Guid owner) => new IdentityProvider(_sharedIdentityProviderId, categoryId, typeId, owner, _fixture.Create<DateTimeOffset>()));

        A.CallTo(() => _provisioningManager.GetProviderUserLinkDataForCentralUserIdAsync(A<string>._)).ReturnsLazily((string userEntityId) =>
        {
            var user = userData.First(u => u.UserEntityId == userEntityId);
            return new[] {
                new IdentityProviderLink(
                    _sharedIdpAlias,
                    user.SharedIdpUserId,
                    user.SharedIdpUserName
                ),
                new IdentityProviderLink(
                    _otherIdpAlias,
                    user.OtherIdpUserId,
                    user.OtherIdpUserName
                )
            }.ToAsyncEnumerable();
        });
    }

    private string HeaderLine()
    {
        return string.Join(",", new[] {
            _csvSettings.HeaderUserId,
            _csvSettings.HeaderFirstName,
            _csvSettings.HeaderLastName,
            _csvSettings.HeaderEmail,
            _csvSettings.HeaderProviderAlias,
            _csvSettings.HeaderProviderUserId,
            _csvSettings.HeaderProviderUserName,
            _csvSettings.HeaderProviderAlias,
            _csvSettings.HeaderProviderUserId,
            _csvSettings.HeaderProviderUserName
        });
    }

    private string NextLine(TestUserData userData)
    {
        return string.Join(",", new[] {
            userData.CompanyUserId.ToString(),
            userData.FirstName,
            userData.LastName,
            userData.Email,
            _sharedIdpAlias,
            userData.SharedIdpUserId,
            userData.SharedIdpUserName,
            _otherIdpAlias,
            userData.OtherIdpUserId,
            userData.OtherIdpUserName
        });
    }

    private record TestUserData(Guid CompanyUserId, string UserEntityId, string FirstName, string LastName, string Email, string SharedIdpUserId, string SharedIdpUserName, string OtherIdpUserId, string OtherIdpUserName);

    #endregion
}
