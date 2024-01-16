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

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Library;
using Org.Eclipse.TractusX.Portal.Backend.Framework.IO;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Mailing.SendMail;
using Org.Eclipse.TractusX.Portal.Backend.Mailing.Service;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.ErrorHandling;
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
    private readonly IUserRolesRepository _userRolesRepository;
    private readonly IOptions<IdentityProviderSettings> _options;
    private readonly IdentityProviderCsvSettings _csvSettings;
    private readonly IIdentityService _identityService;
    private readonly IErrorMessageService _errorMessageService;
    private readonly ILogger<IdentityProviderBusinessLogic> _logger;
    private readonly IFormFile _document;
    private readonly Encoding _encoding;
    private readonly Guid _companyId;
    private readonly Guid _invalidCompanyId;
    private readonly IIdentityData _identity;
    private readonly Guid _sharedIdentityProviderId;
    private readonly string _sharedIdpAlias;
    private readonly Guid _otherIdentityProviderId;
    private readonly string _otherIdpAlias;
    private readonly Guid _identityProviderId;
    private readonly IRoleBaseMailService _roleBaseMailService;
    private readonly IMailingService _mailingService;

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
        _userRolesRepository = A.Fake<IUserRolesRepository>();
        _identityService = A.Fake<IIdentityService>();
        _roleBaseMailService = A.Fake<IRoleBaseMailService>();
        _mailingService = A.Fake<IMailingService>();
        _options = A.Fake<IOptions<IdentityProviderSettings>>();
        _document = A.Fake<IFormFile>();
        _logger = A.Fake<ILogger<IdentityProviderBusinessLogic>>();
        _identity = A.Fake<IIdentityData>();

        _companyId = _fixture.Create<Guid>();
        _invalidCompanyId = _fixture.Create<Guid>();
        _identityProviderId = _fixture.Create<Guid>();
        _sharedIdentityProviderId = _fixture.Create<Guid>();
        _sharedIdpAlias = _fixture.Create<string>();
        _otherIdentityProviderId = _fixture.Create<Guid>();
        _otherIdpAlias = _fixture.Create<string>();
        _encoding = _fixture.Create<Encoding>();

        A.CallTo(() => _identity.IdentityId).Returns(Guid.NewGuid());
        A.CallTo(() => _identity.IdentityTypeId).Returns(IdentityTypeId.COMPANY_USER);
        A.CallTo(() => _identity.CompanyId).Returns(_companyId);
        A.CallTo(() => _identityService.IdentityData).Returns(_identity);

        _errorMessageService = A.Fake<IErrorMessageService>();
        A.CallTo(() => _errorMessageService.GetMessage(typeof(ProvisioningServiceErrors), A<int>._))
            .ReturnsLazily((Type type, int code) => $"type: {type.Name} code: {Enum.GetName(type, code)} userName: {{userName}} realm: {{realm}}");

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

        A.CallTo(() => _portalRepositories.GetInstance<IIdentityProviderRepository>())
            .Returns(_identityProviderRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>())
            .Returns(_companyRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>())
            .Returns(_userRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRolesRepository>())
            .Returns(_userRolesRepository);
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
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);

        var result = await sut.UploadOwnCompanyUsersIdentityProviderLinkDataAsync(_document, CancellationToken.None).ConfigureAwait(false);

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
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);

        async Task Act() => await sut.UploadOwnCompanyUsersIdentityProviderLinkDataAsync(_document, CancellationToken.None).ConfigureAwait(false);

        var error = await Assert.ThrowsAsync<UnsupportedMediaTypeException>(Act).ConfigureAwait(false);
        error.Message.Should().Be($"Only contentType {_csvSettings.ContentType} files are allowed.");
    }

    [Fact]
    public async Task TestUploadOwnCompanyUsersIdentityProviderLinkDataAsyncEmailChangedSuccess()
    {
        const int numUsers = 5;

        var changedEmail = _fixture.Create<string>();

        var unchanged = _fixture.Create<TestUserData>();
        var changed = unchanged with { Email = changedEmail };

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
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);

        var result = await sut.UploadOwnCompanyUsersIdentityProviderLinkDataAsync(_document, CancellationToken.None).ConfigureAwait(false);

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
        var changed = unchanged with { SharedIdpUserName = _fixture.Create<string>() };

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
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);

        var result = await sut.UploadOwnCompanyUsersIdentityProviderLinkDataAsync(_document, CancellationToken.None).ConfigureAwait(false);

        result.Updated.Should().Be(0);
        result.Unchanged.Should().Be(numUsers - 1);
        result.Error.Should().Be(1);
        result.Total.Should().Be(numUsers);
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().Should().Match<UserUpdateError>(x =>
            x.Line == 3 &&
            x.Message == $"unexpected update of shared identityProviderLink, alias '{_sharedIdpAlias}', companyUser '{changed.CompanyUserId}', providerUserId: '{changed.SharedIdpUserId}', providerUserName: '{changed.SharedIdpUserName}'");

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
        var changed = unchanged with { OtherIdpUserName = _fixture.Create<string>() };

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
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);

        var result = await sut.UploadOwnCompanyUsersIdentityProviderLinkDataAsync(_document, CancellationToken.None).ConfigureAwait(false);

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
        var unknown = unchanged with { CompanyUserId = _fixture.Create<Guid>() };

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
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);

        var result = await sut.UploadOwnCompanyUsersIdentityProviderLinkDataAsync(_document, CancellationToken.None).ConfigureAwait(false);

        result.Updated.Should().Be(0);
        result.Unchanged.Should().Be(numUsers - 1);
        result.Error.Should().Be(1);
        result.Total.Should().Be(numUsers);
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().Should().Match<UserUpdateError>(x =>
            x.Line == 3 &&
            x.Message == $"unexpected value of UserId: '{unknown.CompanyUserId}'");

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
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);

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
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);

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
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);

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
        A.CallTo(() => _identity.CompanyId).Returns(companyId);

        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);

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
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);

        SetupCreateOwnCompanyIdentityProvider();
        A.CallTo(() => _identity.CompanyId).Returns(_invalidCompanyId);

        // Act
        async Task Act() => await sut.CreateOwnCompanyIdentityProviderAsync(IamIdentityProviderProtocol.SAML, IdentityProviderTypeId.MANAGED, null).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be("Not allowed to create an identityProvider of type MANAGED");
        A.CallTo(() => _companyRepository.CheckCompanyAndCompanyRolesAsync(_invalidCompanyId, A<IEnumerable<CompanyRoleId>>.That.IsSameSequenceAs(new[] { CompanyRoleId.OPERATOR, CompanyRoleId.ONBOARDING_SERVICE_PROVIDER }))).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CreateOwnCompanyIdentityProviderAsync_WithShared_ThrowsForbiddenException()
    {
        // Arrange
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);

        // Act
        async Task Act() => await sut.CreateOwnCompanyIdentityProviderAsync(IamIdentityProviderProtocol.OIDC, IdentityProviderTypeId.SHARED, null).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be($"creation of identityProviderType {IdentityProviderTypeId.SHARED} is not supported");
        A.CallTo(() => _companyRepository.CheckCompanyAndCompanyRolesAsync(_invalidCompanyId, A<IEnumerable<CompanyRoleId>>._)).MustNotHaveHappened();
    }

    [Theory]
    [InlineData(IamIdentityProviderProtocol.SAML, IdentityProviderTypeId.OWN, new CompanyRoleId[] { })]
    [InlineData(IamIdentityProviderProtocol.OIDC, IdentityProviderTypeId.OWN, new CompanyRoleId[] { })]
    [InlineData(IamIdentityProviderProtocol.SAML, IdentityProviderTypeId.MANAGED, new[] { CompanyRoleId.OPERATOR, CompanyRoleId.ONBOARDING_SERVICE_PROVIDER })]
    [InlineData(IamIdentityProviderProtocol.OIDC, IdentityProviderTypeId.MANAGED, new[] { CompanyRoleId.OPERATOR, CompanyRoleId.ONBOARDING_SERVICE_PROVIDER })]
    public async Task CreateOwnCompanyIdentityProviderAsync_WithValidData_ExecutesExpected(IamIdentityProviderProtocol protocol, IdentityProviderTypeId typeId, IEnumerable<CompanyRoleId> companyRoleIds)
    {
        // Arrange
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);

        var idpName = _fixture.Create<string>();

        var idps = new List<IdentityProvider>();
        var companyIdps = new List<CompanyIdentityProvider>();
        var iamIdps = new List<IamIdentityProvider>();
        SetupCreateOwnCompanyIdentityProvider(protocol, idps, companyIdps, iamIdps);
        A.CallTo(() => _provisioningManager.CreateOwnIdpAsync(A<string>._, A<string>._, A<IamIdentityProviderProtocol>._))
            .Returns(idpName);

        var expectedProtocol = protocol switch
        {
            IamIdentityProviderProtocol.OIDC => IdentityProviderCategoryId.KEYCLOAK_OIDC,
            IamIdentityProviderProtocol.SAML => IdentityProviderCategoryId.KEYCLOAK_SAML,
            _ => throw new NotImplementedException()
        };
        var expectedOwner = _companyId;

        // Act
        var result = await sut.CreateOwnCompanyIdentityProviderAsync(protocol, typeId, "test-company").ConfigureAwait(false);

        // Assert
        A.CallTo(() => _companyRepository.CheckCompanyAndCompanyRolesAsync(_companyId, A<IEnumerable<CompanyRoleId>>.That.IsSameSequenceAs(companyRoleIds))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.CreateOwnIdpAsync("test-company", "test", protocol)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();

        idps.Should().ContainSingle().Which.Should().Match<IdentityProvider>(x =>
            x.Id == _identityProviderId &&
            x.OwnerId == expectedOwner &&
            x.IdentityProviderCategoryId == expectedProtocol &&
            x.IdentityProviderTypeId == typeId);

        switch (typeId)
        {
            case IdentityProviderTypeId.OWN:
                companyIdps.Should().ContainSingle().Which.Should().Match<CompanyIdentityProvider>(x =>
                    x.CompanyId == _companyId &&
                    x.IdentityProviderId == _identityProviderId);
                break;
            case IdentityProviderTypeId.MANAGED:
                companyIdps.Should().BeEmpty();
                break;
            default: throw new NotImplementedException();
        }

        iamIdps.Should().ContainSingle().Which.Should().Match<IamIdentityProvider>(x =>
            x.IdentityProviderId == _identityProviderId &&
            x.IamIdpAlias == idpName);

        result.Should().Match<IdentityProviderDetails>(x =>
            x.mappers != null &&
            x.mappers.Count() == 3 &&
            x.enabled == true &&
            x.redirectUrl == "https://redirect.com/*" &&
            protocol == IamIdentityProviderProtocol.OIDC
                ? x.displayName == "test-oidc" &&
                  x.saml == null &&
                  x.oidc != null &&
                  x.oidc.clientAuthMethod == IamIdentityProviderClientAuthMethod.SECRET_JWT &&
                  x.oidc.signatureAlgorithm == IamIdentityProviderSignatureAlgorithm.RS512
                : x.displayName == "test-saml" &&
                  x.oidc == null &&
                  x.saml != null &&
                  x.saml.singleSignOnServiceUrl == "https://sso.com");
    }

    #endregion

    #region DeleteCompanyIdentityProviderAsync

    [Fact]
    public async Task DeleteCompanyIdentityProviderAsync_WithNotExistingProvider_ThrowsNotFoundException()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync(A<Guid>._, A<Guid>._, A<bool>._))
            .Returns(((bool, string?, IdentityProviderCategoryId, IdentityProviderTypeId, IEnumerable<(Guid, IEnumerable<string>)>?, bool, string))default);

        // Act
        async Task Act() => await sut.DeleteCompanyIdentityProviderAsync(invalidId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"identityProvider {invalidId} does not exist");
        A.CallTo(() => _identityProviderRepository.GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync(invalidId, _companyId, true)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DeleteCompanyIdentityProviderAsync_WithInvalidCompany_ThrowsConflictException()
    {
        // Arrange
        var identityProviderId = Guid.NewGuid();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync(A<Guid>._, A<Guid>._, A<bool>._))
            .Returns((false, string.Empty, IdentityProviderCategoryId.KEYCLOAK_OIDC, IdentityProviderTypeId.OWN, Enumerable.Empty<(Guid, IEnumerable<string>)>(), false, string.Empty));

        // Act
        async Task Act() => await sut.DeleteCompanyIdentityProviderAsync(identityProviderId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be($"company {_companyId} is not the owner of identityProvider {identityProviderId}");
        A.CallTo(() => _identityProviderRepository.GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync(identityProviderId, _companyId, true)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DeleteCompanyIdentityProviderAsync_WithDisabledIdp_ThrowsControllerArgumentException()
    {
        // Arrange
        var identityProviderId = Guid.NewGuid();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync(A<Guid>._, A<Guid>._, A<bool>._))
            .Returns((true, "test", IdentityProviderCategoryId.KEYCLOAK_OIDC, IdentityProviderTypeId.OWN, Enumerable.Empty<(Guid, IEnumerable<string>)>(), false, string.Empty));
        A.CallTo(() => _provisioningManager.IsCentralIdentityProviderEnabled(A<string>._))
            .Returns(true);

        // Act
        async Task Act() => await sut.DeleteCompanyIdentityProviderAsync(identityProviderId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be($"cannot delete identityProvider {identityProviderId} as it is enabled");
        A.CallTo(() => _identityProviderRepository.GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync(identityProviderId, _companyId, true)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.IsCentralIdentityProviderEnabled("test")).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DeleteCompanyIdentityProviderAsync_WithSharedKeycloakValid_CallsExpected()
    {
        // Arrange
        var identityProviderId = Guid.NewGuid();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetIdpLinkedCompanyUserIds(identityProviderId, _companyId))
            .Returns(_fixture.CreateMany<Guid>(3).ToAsyncEnumerable());
        A.CallTo(() => _identityProviderRepository.GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync(A<Guid>._, A<Guid>._, A<bool>._))
            .Returns((true, "test", IdentityProviderCategoryId.KEYCLOAK_OIDC, IdentityProviderTypeId.SHARED, new[] { (_companyId, new[] { "other-alias" }.AsEnumerable()) }, false, string.Empty));
        A.CallTo(() => _provisioningManager.IsCentralIdentityProviderEnabled("test"))
            .Returns(false);
        A.CallTo(() => _provisioningManager.IsCentralIdentityProviderEnabled("other-alias"))
            .Returns(true);

        // Act
        await sut.DeleteCompanyIdentityProviderAsync(identityProviderId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _identityProviderRepository.GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync(identityProviderId, _companyId, true)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.IsCentralIdentityProviderEnabled("test")).MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.IsCentralIdentityProviderEnabled("other-alias")).MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.DeleteSharedIdpRealmAsync("test")).MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.DeleteCentralIdentityProviderAsync("test")).MustHaveHappenedOnceExactly();
        A.CallTo(() => _userRepository.RemoveCompanyUserAssignedIdentityProviders(A<IEnumerable<(Guid CompanyUserId, Guid IdentityProviderId)>>.That.Matches(x => x.Count() == 3)))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _identityProviderRepository.DeleteCompanyIdentityProvider(_companyId, identityProviderId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _identityProviderRepository.DeleteIamIdentityProvider("test")).MustHaveHappenedOnceExactly();
        A.CallTo(() => _identityProviderRepository.DeleteIdentityProvider(identityProviderId)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DeleteCompanyIdentityProviderAsync_WithValid_CallsExpected()
    {
        // Arrange
        var identityProviderId = Guid.NewGuid();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync(A<Guid>._, A<Guid>._, A<bool>._))
            .Returns((true, "test", IdentityProviderCategoryId.KEYCLOAK_OIDC, IdentityProviderTypeId.OWN, new[] { (_companyId, new[] { "other-alias" }.AsEnumerable()) }, false, string.Empty));
        A.CallTo(() => _identityProviderRepository.GetIdpLinkedCompanyUserIds(identityProviderId, _companyId))
            .Returns(_fixture.CreateMany<Guid>(3).ToAsyncEnumerable());
        A.CallTo(() => _provisioningManager.IsCentralIdentityProviderEnabled("test"))
            .Returns(false);
        A.CallTo(() => _provisioningManager.IsCentralIdentityProviderEnabled("other-alias"))
            .Returns(true);

        // Act
        await sut.DeleteCompanyIdentityProviderAsync(identityProviderId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _provisioningManager.IsCentralIdentityProviderEnabled("test")).MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.IsCentralIdentityProviderEnabled("other-alias")).MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.DeleteSharedIdpRealmAsync("test")).MustNotHaveHappened();
        A.CallTo(() => _provisioningManager.DeleteCentralIdentityProviderAsync("test")).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _userRepository.RemoveCompanyUserAssignedIdentityProviders(A<IEnumerable<(Guid CompanyUserId, Guid IdentityProviderId)>>.That.Matches(x => x.Count() == 3)))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _identityProviderRepository.DeleteCompanyIdentityProvider(_companyId, identityProviderId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _identityProviderRepository.DeleteIamIdentityProvider("test")).MustHaveHappenedOnceExactly();
        A.CallTo(() => _identityProviderRepository.DeleteIdentityProvider(identityProviderId)).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DeleteCompanyIdentityProviderAsync_WithManagedIdp_ExecutesExpected(bool multipleIdps)
    {
        // Arrange
        var identityProviderId = Guid.NewGuid();
        var role1 = Guid.NewGuid();
        var role2 = Guid.NewGuid();
        var company = _fixture.Build<Company>().With(x => x.CompanyStatusId, CompanyStatusId.ACTIVE).Create();
        var identity = _fixture.Build<Identity>().With(x => x.UserStatusId, UserStatusId.ACTIVE).Create();
        A.CallTo(() => _identity.CompanyId).Returns(company.Id);
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync(A<Guid>._, A<Guid>._, A<bool>._))
            .Returns((true, "test", IdentityProviderCategoryId.KEYCLOAK_OIDC, IdentityProviderTypeId.MANAGED, Enumerable.Repeat(new ValueTuple<Guid, IEnumerable<string>>(company.Id, Enumerable.Empty<string>()), 1), false, string.Empty));
        A.CallTo(() => _provisioningManager.IsCentralIdentityProviderEnabled("test"))
            .Returns(false);
        A.CallTo(() => _roleBaseMailService.GetRoleData(_options.Value.DeleteIdpRoles)).Returns(new List<Guid> { role1, role2 });
        A.CallTo(() => _identityProviderRepository.GetManagedIdpLinkedData(identityProviderId, A<IEnumerable<Guid>>.That.Matches(x => x.Count() == 2 && x.Contains(role1) && x.Contains(role2)))).Returns(
            new ValueTuple<Guid, CompanyStatusId, bool, IEnumerable<(Guid IdentityId, bool IsLinkedCompanyUser, (string? UserMail, string? FirstName, string? LastName) Userdata, bool IsInUserRoles, IEnumerable<Guid> UserRoleIds)>>[] {
                new (company.Id, CompanyStatusId.ACTIVE, multipleIdps, Enumerable.Repeat((identity.Id, true, new ValueTuple<string?, string?, string?>("test@example.org", "Test", "User"), true, _fixture.CreateMany<Guid>(5)), 1))
            }.ToAsyncEnumerable());
        A.CallTo(() => _companyRepository.AttachAndModifyCompany(company.Id, A<Action<Company>>._, A<Action<Company>>._))
            .Invokes((Guid _, Action<Company>? initialize, Action<Company> modify) =>
            {
                initialize?.Invoke(company);
                modify(company);
            });
        A.CallTo(() => _userRepository.AttachAndModifyIdentities(A<IEnumerable<ValueTuple<Guid, Action<Identity>>>>._))
            .Invokes((IEnumerable<(Guid IdentityId, Action<Identity> Modify)> identityData) =>
            {
                var initial = identityData.Select(x => (Identity: identity, x.Modify)).ToList();
                initial.ForEach(x => x.Modify(x.Identity));
            });

        // Act
        await sut.DeleteCompanyIdentityProviderAsync(identityProviderId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _provisioningManager.DeleteSharedIdpRealmAsync("test")).MustNotHaveHappened();
        A.CallTo(() => _provisioningManager.DeleteCentralIdentityProviderAsync("test")).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _identityProviderRepository.DeleteCompanyIdentityProvider(company.Id, identityProviderId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _identityProviderRepository.DeleteIamIdentityProvider("test")).MustHaveHappenedOnceExactly();
        A.CallTo(() => _identityProviderRepository.DeleteIdentityProvider(identityProviderId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _userRolesRepository.DeleteCompanyUserAssignedRoles(A<IEnumerable<(Guid, Guid)>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _mailingService.SendMails("test@example.org", A<IDictionary<string, string>>._, A<IEnumerable<string>>.That.Matches(x => x.Count() == 1 && x.Single() == "DeleteManagedIdp")))
            .MustHaveHappenedOnceExactly();
        company.CompanyStatusId.Should().Be(multipleIdps ? CompanyStatusId.ACTIVE : CompanyStatusId.INACTIVE);
        if (!multipleIdps)
        {
            identity.UserStatusId.Should().Be(UserStatusId.INACTIVE);
        }
    }

    #endregion

    #region GetOwnCompanyIdentityProvidersAsync

    [Fact]
    public async Task GetOwnCompanyIdentityProvidersAsync_WithValidId_ReturnsExpected()
    {
        // Arrange
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        var oidcGuid = Guid.NewGuid();
        var samlGuid = Guid.NewGuid();
        var oidc = (oidcGuid, IdentityProviderCategoryId.KEYCLOAK_OIDC, (string?)"oidc-alias", IdentityProviderTypeId.OWN);
        var saml = (samlGuid, IdentityProviderCategoryId.KEYCLOAK_SAML, (string?)"saml-alias", IdentityProviderTypeId.OWN);
        A.CallTo(() => _identityProviderRepository.GetCompanyIdentityProviderCategoryDataUntracked(A<Guid>._))
            .Returns(new[] { oidc, saml }.ToAsyncEnumerable());
        A.CallTo(() => _provisioningManager.GetCentralIdentityProviderDataOIDCAsync("oidc-alias"))
            .Returns(_fixture.Build<IdentityProviderConfigOidc>().With(x => x.Enabled, true).With(x => x.DisplayName, "dis-oidc").Create());
        A.CallTo(() => _provisioningManager.GetIdentityProviderMappers("oidc-alias"))
            .Returns(_fixture.CreateMany<IdentityProviderMapperModel>(3).ToAsyncEnumerable());
        A.CallTo(() => _provisioningManager.GetCentralIdentityProviderDataSAMLAsync("saml-alias"))
            .Returns(_fixture.Build<IdentityProviderConfigSaml>().With(x => x.Enabled, true).With(x => x.DisplayName, "dis-saml").Create());
        A.CallTo(() => _provisioningManager.GetIdentityProviderMappers("saml-alias"))
            .Returns(_fixture.CreateMany<IdentityProviderMapperModel>(2).ToAsyncEnumerable());

        // Act
        var result = await sut.GetOwnCompanyIdentityProvidersAsync().ToListAsync().ConfigureAwait(false);

        // Assert
        A.CallTo(() => _identityProviderRepository.GetCompanyIdentityProviderCategoryDataUntracked(_companyId)).MustHaveHappenedOnceExactly();
        result.Should().HaveCount(2).And.Satisfy(
            x => x.displayName == "dis-oidc" && x.mappers != null && x.mappers.Count() == 3,
            x => x.displayName == "dis-saml" && x.mappers != null && x.mappers.Count() == 2
        );
    }

    #endregion

    #region GetOwnCompanyIdentityProviderAsync

    [Fact]
    public async Task GetOwnCompanyIdentityProviderAsync_WithDifferentCompany_ThrowsConflictException()
    {
        // Arrange
        var identityProviderId = Guid.NewGuid();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetOwnCompanyIdentityProviderAliasUntrackedAsync(identityProviderId, _companyId))
            .Returns((string.Empty, IdentityProviderCategoryId.KEYCLOAK_OIDC, false, IdentityProviderTypeId.OWN));

        // Act
        async Task Act() => await sut.GetOwnCompanyIdentityProviderAsync(identityProviderId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"identityProvider {identityProviderId} is not associated with company {_companyId}");
    }

    [Fact]
    public async Task GetOwnCompanyIdentityProviderAsync_WithAliasNull_ThrowsNotFoundException()
    {
        // Arrange
        var identityProviderId = Guid.NewGuid();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetOwnCompanyIdentityProviderAliasUntrackedAsync(identityProviderId, _companyId))
            .Returns((null, IdentityProviderCategoryId.KEYCLOAK_OIDC, true, IdentityProviderTypeId.OWN));

        // Act
        async Task Act() => await sut.GetOwnCompanyIdentityProviderAsync(identityProviderId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"identityProvider {identityProviderId} does not exist");
    }

    [Fact]
    public async Task GetOwnCompanyIdentityProviderAsync_WithOidcWithoutExistingKeycloakClient_CallsExpected()
    {
        // Arrange
        var identityProviderId = Guid.NewGuid();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetOwnCompanyIdentityProviderAliasUntrackedAsync(identityProviderId, _companyId))
            .Returns(("cl1", IdentityProviderCategoryId.KEYCLOAK_OIDC, true, IdentityProviderTypeId.OWN));
        A.CallTo(() => _provisioningManager.GetCentralIdentityProviderDataOIDCAsync("cl1"))
            .Throws(new KeycloakEntityNotFoundException("cl1 not existing"));

        // Act
        var result = await sut.GetOwnCompanyIdentityProviderAsync(identityProviderId).ConfigureAwait(false);

        // Assert
        result.mappers.Should().BeNull();
        result.displayName.Should().BeNull();
        result.enabled.Should().BeNull();
    }

    [Fact]
    public async Task GetOwnCompanyIdentityProviderAsync_WithValidOidc_CallsExpected()
    {
        // Arrange
        var identityProviderId = Guid.NewGuid();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetOwnCompanyIdentityProviderAliasUntrackedAsync(identityProviderId, _companyId))
            .Returns(("cl1", IdentityProviderCategoryId.KEYCLOAK_OIDC, true, IdentityProviderTypeId.OWN));
        A.CallTo(() => _provisioningManager.GetCentralIdentityProviderDataOIDCAsync("cl1"))
            .Returns(_fixture.Build<IdentityProviderConfigOidc>().With(x => x.Enabled, true).With(x => x.DisplayName, "dis-oidc").Create());
        A.CallTo(() => _provisioningManager.GetIdentityProviderMappers("cl1"))
            .Returns(_fixture.CreateMany<IdentityProviderMapperModel>(3).ToAsyncEnumerable());

        // Act
        var result = await sut.GetOwnCompanyIdentityProviderAsync(identityProviderId).ConfigureAwait(false);

        // Assert
        result.mappers.Should().HaveCount(3);
        result.displayName.Should().Be("dis-oidc");
        result.enabled.Should().BeTrue();
    }

    [Fact]
    public async Task GetOwnCompanyIdentityProviderAsync_WithSamlWithoutExistingKeycloakClient_CallsExpected()
    {
        // Arrange
        var identityProviderId = Guid.NewGuid();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetOwnCompanyIdentityProviderAliasUntrackedAsync(identityProviderId, _companyId))
            .Returns(("saml-alias", IdentityProviderCategoryId.KEYCLOAK_SAML, true, IdentityProviderTypeId.OWN));
        A.CallTo(() => _provisioningManager.GetCentralIdentityProviderDataSAMLAsync("saml-alias"))
            .Throws(new KeycloakEntityNotFoundException("saml-alias"));

        // Act
        var result = await sut.GetOwnCompanyIdentityProviderAsync(identityProviderId).ConfigureAwait(false);

        // Assert
        result.mappers.Should().BeNull();
        result.displayName.Should().BeNull();
        result.enabled.Should().BeNull();
    }

    [Fact]
    public async Task GetOwnCompanyIdentityProviderAsync_WithValidSaml_CallsExpected()
    {
        // Arrange
        var identityProviderId = Guid.NewGuid();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetOwnCompanyIdentityProviderAliasUntrackedAsync(identityProviderId, _companyId))
            .Returns(("saml-alias", IdentityProviderCategoryId.KEYCLOAK_SAML, true, IdentityProviderTypeId.OWN));
        A.CallTo(() => _provisioningManager.GetCentralIdentityProviderDataSAMLAsync("saml-alias"))
            .Returns(_fixture.Build<IdentityProviderConfigSaml>().With(x => x.Enabled, true).With(x => x.DisplayName, "dis-saml").Create());
        A.CallTo(() => _provisioningManager.GetIdentityProviderMappers("saml-alias"))
            .Returns(_fixture.CreateMany<IdentityProviderMapperModel>(2).ToAsyncEnumerable());

        // Act
        var result = await sut.GetOwnCompanyIdentityProviderAsync(identityProviderId).ConfigureAwait(false);

        // Assert
        result.mappers.Should().HaveCount(2);
        result.displayName.Should().Be("dis-saml");
        result.enabled.Should().BeTrue();
    }

    #endregion

    #region SetOwnCompanyIdentityProviderStatusAsync

    [Fact]
    public async Task SetOwnCompanyIdentityProviderStatusAsync_WithDifferentCompany_ThrowsNotFoundExecption()
    {
        // Arrange
        var identityProviderId = Guid.NewGuid();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync(A<Guid>._, A<Guid>._, A<bool>._))
            .Returns(((bool, string?, IdentityProviderCategoryId, IdentityProviderTypeId, IEnumerable<(Guid, IEnumerable<string>)>?, bool, string))default);

        // Act
        async Task Act() => await sut.SetOwnCompanyIdentityProviderStatusAsync(identityProviderId, false).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        A.CallTo(() => _identityProviderRepository.GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync(identityProviderId, _companyId, true)).MustHaveHappenedOnceExactly();
        ex.Message.Should().Be($"identityProvider {identityProviderId} does not exist");
    }

    [Fact]
    public async Task SetOwnCompanyIdentityProviderStatusAsync_WithDifferentCompany_ThrowsConflictException()
    {
        // Arrange
        var identityProviderId = Guid.NewGuid();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync(A<Guid>._, A<Guid>._, A<bool>._))
            .Returns((false, string.Empty, IdentityProviderCategoryId.KEYCLOAK_OIDC, IdentityProviderTypeId.OWN, Enumerable.Empty<(Guid, IEnumerable<string>)>(), false, string.Empty));

        // Act
        async Task Act() => await sut.SetOwnCompanyIdentityProviderStatusAsync(identityProviderId, false).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        A.CallTo(() => _identityProviderRepository.GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync(identityProviderId, _companyId, true)).MustHaveHappenedOnceExactly();
        ex.Message.Should().Be($"company {_companyId} is not the owner of identityProvider {identityProviderId}");
    }

    [Fact]
    public async Task SetOwnCompanyIdentityProviderStatusAsync_WithNoOtherEnabledIdp_ThrowsControllerArgumentException()
    {
        // Arrange
        var identityProviderId = Guid.NewGuid();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync(A<Guid>._, A<Guid>._, A<bool>._))
            .Returns((true, "cl1", IdentityProviderCategoryId.KEYCLOAK_OIDC, IdentityProviderTypeId.OWN, new[] { (_companyId, new[] { "alt-cl1" }.AsEnumerable()) }, false, string.Empty));
        A.CallTo(() => _provisioningManager.IsCentralIdentityProviderEnabled("alt-cl1")).Returns(false);

        // Act
        async Task Act() => await sut.SetOwnCompanyIdentityProviderStatusAsync(identityProviderId, false).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        A.CallTo(() => _identityProviderRepository.GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync(identityProviderId, _companyId, true)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.IsCentralIdentityProviderEnabled("alt-cl1")).MustHaveHappenedOnceExactly();
        ex.Message.Should().Be($"cannot disable indentityProvider {identityProviderId} as no other active identityProvider exists for this company");
    }

    [Fact]
    public async Task SetOwnCompanyIdentityProviderStatusAsync_WithNoOtherCompany_CallsExpected()
    {
        // Arrange
        var identityProviderId = Guid.NewGuid();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync(A<Guid>._, A<Guid>._, A<bool>._))
            .Returns((true, "cl1", IdentityProviderCategoryId.KEYCLOAK_OIDC, IdentityProviderTypeId.OWN, Enumerable.Empty<(Guid, IEnumerable<string>)>(), false, string.Empty));
        A.CallTo(() => _provisioningManager.GetCentralIdentityProviderDataOIDCAsync("cl1"))
            .Returns(_fixture.Build<IdentityProviderConfigOidc>().With(x => x.Enabled, true).With(x => x.DisplayName, "dis-oidc").Create());
        A.CallTo(() => _provisioningManager.GetIdentityProviderMappers("cl1"))
            .Returns(_fixture.CreateMany<IdentityProviderMapperModel>(3).ToAsyncEnumerable());

        // Act
        var result = await sut.SetOwnCompanyIdentityProviderStatusAsync(identityProviderId, false).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _identityProviderRepository.GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync(identityProviderId, _companyId, true)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.SetCentralIdentityProviderStatusAsync("cl1", false)).MustHaveHappenedOnceExactly();
        result.mappers.Should().HaveCount(3);
        result.displayName.Should().Be("dis-oidc");
        result.enabled.Should().BeTrue();
    }

    [Fact]
    public async Task SetOwnCompanyIdentityProviderStatusAsync_WithValidOidc_CallsExpected()
    {
        // Arrange
        var identityProviderId = Guid.NewGuid();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync(A<Guid>._, A<Guid>._, A<bool>._))
            .Returns((true, "cl1", IdentityProviderCategoryId.KEYCLOAK_OIDC, IdentityProviderTypeId.OWN, new[] { (_companyId, new[] { "alt-cl1" }.AsEnumerable()) }, false, string.Empty));
        A.CallTo(() => _provisioningManager.IsCentralIdentityProviderEnabled("alt-cl1")).Returns(true);
        A.CallTo(() => _provisioningManager.GetCentralIdentityProviderDataOIDCAsync("cl1"))
            .Returns(_fixture.Build<IdentityProviderConfigOidc>().With(x => x.Enabled, true).With(x => x.DisplayName, "dis-oidc").Create());
        A.CallTo(() => _provisioningManager.GetIdentityProviderMappers("cl1"))
            .Returns(_fixture.CreateMany<IdentityProviderMapperModel>(3).ToAsyncEnumerable());

        // Act
        var result = await sut.SetOwnCompanyIdentityProviderStatusAsync(identityProviderId, false).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _identityProviderRepository.GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync(identityProviderId, _companyId, true)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.IsCentralIdentityProviderEnabled("alt-cl1")).MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.SetCentralIdentityProviderStatusAsync("cl1", false)).MustHaveHappenedOnceExactly();
        result.mappers.Should().HaveCount(3);
        result.displayName.Should().Be("dis-oidc");
        result.enabled.Should().BeTrue();
    }

    [Fact]
    public async Task SetOwnCompanyIdentityProviderStatusAsync_WithValidSaml_CallsExpected()
    {
        // Arrange
        var identityProviderId = Guid.NewGuid();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync(A<Guid>._, A<Guid>._, A<bool>._))
            .Returns((true, "cl1", IdentityProviderCategoryId.KEYCLOAK_SAML, IdentityProviderTypeId.OWN, new[] { (_companyId, new[] { "alt-cl1" }.AsEnumerable()) }, false, string.Empty));
        A.CallTo(() => _provisioningManager.IsCentralIdentityProviderEnabled("alt-cl1")).Returns(true);
        A.CallTo(() => _provisioningManager.GetCentralIdentityProviderDataSAMLAsync("cl1"))
            .Returns(_fixture.Build<IdentityProviderConfigSaml>().With(x => x.Enabled, true).With(x => x.DisplayName, "dis-saml").Create());
        A.CallTo(() => _provisioningManager.GetIdentityProviderMappers("cl1"))
            .Returns(_fixture.CreateMany<IdentityProviderMapperModel>(2).ToAsyncEnumerable());

        // Act
        var result = await sut.SetOwnCompanyIdentityProviderStatusAsync(identityProviderId, false).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _provisioningManager.SetCentralIdentityProviderStatusAsync("cl1", false)).MustHaveHappenedOnceExactly();
        result.mappers.Should().HaveCount(2);
        result.displayName.Should().Be("dis-saml");
        result.enabled.Should().BeTrue();
    }

    [Fact]
    public async Task SetOwnCompanyIdentityProviderStatusAsync_WithValidShared_CallsExpected()
    {
        // Arrange
        var identityProviderId = Guid.NewGuid();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync(A<Guid>._, A<Guid>._, A<bool>._))
            .Returns((true, "cl1", IdentityProviderCategoryId.KEYCLOAK_OIDC, IdentityProviderTypeId.SHARED, new[] { (_companyId, new[] { "alt-cl1" }.AsEnumerable()) }, false, string.Empty));
        A.CallTo(() => _provisioningManager.IsCentralIdentityProviderEnabled("alt-cl1")).Returns(true);
        A.CallTo(() => _provisioningManager.GetCentralIdentityProviderDataOIDCAsync("cl1"))
            .Returns(_fixture.Build<IdentityProviderConfigOidc>().With(x => x.Enabled, true).With(x => x.DisplayName, "dis-oidc").Create());
        A.CallTo(() => _provisioningManager.GetIdentityProviderMappers("cl1"))
            .Returns(_fixture.CreateMany<IdentityProviderMapperModel>(3).ToAsyncEnumerable());

        // Act
        var result = await sut.SetOwnCompanyIdentityProviderStatusAsync(identityProviderId, false).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _provisioningManager.SetSharedIdentityProviderStatusAsync("cl1", false))
            .MustHaveHappenedOnceExactly();
        result.mappers.Should().HaveCount(3);
        result.displayName.Should().Be("dis-oidc");
        result.enabled.Should().BeTrue();
    }

    [Fact]
    public async Task SetOwnCompanyIdentityProviderStatusAsync_DeactivateManaged_CallsExpected()
    {
        // Arrange
        var identityProviderId = Guid.NewGuid();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync(A<Guid>._, A<Guid>._, A<bool>._))
            .Returns((true, "cl1", IdentityProviderCategoryId.KEYCLOAK_OIDC, IdentityProviderTypeId.MANAGED, new[] { (_companyId, new[] { "alt-cl1" }.AsEnumerable()) }, true, string.Empty));
        A.CallTo(() => _provisioningManager.IsCentralIdentityProviderEnabled("alt-cl1")).Returns(true);
        A.CallTo(() => _provisioningManager.GetCentralIdentityProviderDataOIDCAsync("cl1"))
            .Returns(_fixture.Build<IdentityProviderConfigOidc>().With(x => x.Enabled, false).With(x => x.DisplayName, "dis-oidc").Create());
        A.CallTo(() => _provisioningManager.GetIdentityProviderMappers("cl1"))
            .Returns(_fixture.CreateMany<IdentityProviderMapperModel>(3).ToAsyncEnumerable());

        // Act
        var result = await sut.SetOwnCompanyIdentityProviderStatusAsync(identityProviderId, false).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _provisioningManager.SetCentralIdentityProviderStatusAsync("cl1", false))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _roleBaseMailService.RoleBaseSendMailForIdp(A<IEnumerable<UserRoleConfig>>._, A<IEnumerable<(string ParameterName, string ParameterValue)>>._, A<(string ParameterName, string ParameterValue)>._, A<IEnumerable<string>>.That.Matches(x => x.Count() == 1 && x.Single() == "DeactivateManagedIdp"), identityProviderId))
            .MustHaveHappenedOnceExactly();
        result.mappers.Should().HaveCount(3);
        result.displayName.Should().Be("dis-oidc");
        result.enabled.Should().BeFalse();
    }

    #endregion

    #region UpdateOwnCompanyIdentityProviderAsync

    [Theory]
    [InlineData("a", "displayName length must be 2-30 characters")]
    [InlineData("way-too-long-display-name-throws-an-error-should-be-long-enough", "displayName length must be 2-30 characters")]
    [InlineData("$invalid-character", "allowed characters in displayName: 'a-zA-Z0-9!?@&#'\"()_-=/*.,;: '")]
    public async Task UpdateOwnCompanyIdentityProviderAsync_WithInvalidDisplayName_ThrowsControllerArgumentException(string displayName, string errorMessage)
    {
        // Arrange
        var identityProviderId = Guid.NewGuid();
        var data = _fixture.Build<IdentityProviderEditableDetails>()
            .With(x => x.displayName, displayName)
            .Create();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync(A<Guid>._, A<Guid>._, A<bool>._))
            .Returns(((bool, string?, IdentityProviderCategoryId, IdentityProviderTypeId, IEnumerable<(Guid, IEnumerable<string>)>?, bool, string))default);

        // Act
        async Task Act() => await sut.UpdateOwnCompanyIdentityProviderAsync(identityProviderId, data).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be(errorMessage);
    }

    [Fact]
    public async Task UpdateOwnCompanyIdentityProviderAsync_WithNotExistingIdp_ThrowsNotFoundException()
    {
        // Arrange
        var identityProviderId = Guid.NewGuid();
        var data = _fixture.Build<IdentityProviderEditableDetails>()
            .With(x => x.displayName, "new-display-name")
            .Create();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync(A<Guid>._, A<Guid>._, A<bool>._))
            .Returns(((bool, string?, IdentityProviderCategoryId, IdentityProviderTypeId, IEnumerable<(Guid, IEnumerable<string>)>?, bool, string))default);

        // Act
        async Task Act() => await sut.UpdateOwnCompanyIdentityProviderAsync(identityProviderId, data).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"identityProvider {identityProviderId} does not exist");
    }

    [Fact]
    public async Task UpdateOwnCompanyIdentityProviderAsync_NotOwner_ThrowsForbiddenException()
    {
        // Arrange
        var identityProviderId = Guid.NewGuid();
        var data = _fixture.Build<IdentityProviderEditableDetails>()
            .With(x => x.displayName, "new-display-name")
            .Create();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync(A<Guid>._, A<Guid>._, A<bool>._))
            .Returns((false, "cl1", IdentityProviderCategoryId.KEYCLOAK_OIDC, IdentityProviderTypeId.OWN, Enumerable.Empty<(Guid, IEnumerable<string>)>(), false, string.Empty));

        // Act
        async Task Act() => await sut.UpdateOwnCompanyIdentityProviderAsync(identityProviderId, data).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be($"User not allowed to run the change for identity provider {identityProviderId}");
    }

    [Fact]
    public async Task UpdateOwnCompanyIdentityProviderAsync_ForOidcWithOidcNull_ThrowsControllerArgumentException()
    {
        // Arrange
        var identityProviderId = Guid.NewGuid();
        var data = _fixture.Build<IdentityProviderEditableDetails>()
            .With(x => x.oidc, (IdentityProviderEditableDetailsOidc?)null)
            .With(x => x.displayName, "new-display-name")
            .Create();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync(A<Guid>._, A<Guid>._, A<bool>._))
            .Returns((true, "cl1", IdentityProviderCategoryId.KEYCLOAK_OIDC, IdentityProviderTypeId.OWN, Enumerable.Empty<(Guid, IEnumerable<string>)>(), false, string.Empty));

        // Act
        async Task Act() => await sut.UpdateOwnCompanyIdentityProviderAsync(identityProviderId, data).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be("property 'oidc' must not be null (Parameter 'oidc')");
    }

    [Fact]
    public async Task UpdateOwnCompanyIdentityProviderAsync_ForOidcWithSamlNotNull_ThrowsControllerArgumentException()
    {
        // Arrange
        var identityProviderId = Guid.NewGuid();
        var data = _fixture.Build<IdentityProviderEditableDetails>()
            .With(x => x.oidc, _fixture.Create<IdentityProviderEditableDetailsOidc>())
            .With(x => x.saml, _fixture.Create<IdentityProviderEditableDetailsSaml>())
            .With(x => x.displayName, "new-display-name")
            .Create();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync(A<Guid>._, A<Guid>._, A<bool>._))
            .Returns((true, "cl1", IdentityProviderCategoryId.KEYCLOAK_OIDC, IdentityProviderTypeId.OWN, Enumerable.Empty<(Guid, IEnumerable<string>)>(), false, string.Empty));

        // Act
        async Task Act() => await sut.UpdateOwnCompanyIdentityProviderAsync(identityProviderId, data).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be("property 'saml' must be null (Parameter 'saml')");
    }

    [Fact]
    public async Task UpdateOwnCompanyIdentityProviderAsync_WithValidOidc_CallsExpected()
    {
        // Arrange
        var identityProviderId = Guid.NewGuid();
        var data = _fixture.Build<IdentityProviderEditableDetails>()
            .With(x => x.oidc, _fixture.Build<IdentityProviderEditableDetailsOidc>().With(x => x.secret, "test").Create())
            .With(x => x.saml, (IdentityProviderEditableDetailsSaml?)null)
            .With(x => x.displayName, "new-display-name")
            .Create();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync(A<Guid>._, A<Guid>._, A<bool>._))
            .Returns((true, "cl1", IdentityProviderCategoryId.KEYCLOAK_OIDC, IdentityProviderTypeId.OWN, Enumerable.Empty<(Guid, IEnumerable<string>)>(), false, string.Empty));
        A.CallTo(() => _provisioningManager.GetCentralIdentityProviderDataOIDCAsync("cl1"))
            .Returns(_fixture.Build<IdentityProviderConfigOidc>().With(x => x.Enabled, true).With(x => x.DisplayName, "dis-oidc").Create());
        A.CallTo(() => _provisioningManager.GetIdentityProviderMappers("cl1"))
            .Returns(_fixture.CreateMany<IdentityProviderMapperModel>(3).ToAsyncEnumerable());

        // Act
        var result = await sut.UpdateOwnCompanyIdentityProviderAsync(identityProviderId, data).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _provisioningManager.UpdateCentralIdentityProviderDataOIDCAsync(A<IdentityProviderEditableConfigOidc>.That.Matches(x => x.Secret == "test" && x.Alias == "cl1")))
            .MustHaveHappenedOnceExactly();
        result.mappers.Should().HaveCount(3);
        result.displayName.Should().Be("dis-oidc");
        result.enabled.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateOwnCompanyIdentityProviderAsync_ForSamlWithSamlNull_ThrowsControllerArgumentException()
    {
        // Arrange
        var identityProviderId = Guid.NewGuid();
        var data = _fixture.Build<IdentityProviderEditableDetails>()
            .With(x => x.saml, (IdentityProviderEditableDetailsSaml?)null)
            .With(x => x.displayName, "new-display-name")
            .Create();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync(A<Guid>._, A<Guid>._, A<bool>._))
            .Returns((true, "cl1", IdentityProviderCategoryId.KEYCLOAK_SAML, IdentityProviderTypeId.OWN, Enumerable.Empty<(Guid, IEnumerable<string>)>(), false, string.Empty));

        // Act
        async Task Act() => await sut.UpdateOwnCompanyIdentityProviderAsync(identityProviderId, data).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be("property 'saml' must not be null (Parameter 'saml')");
    }

    [Fact]
    public async Task UpdateOwnCompanyIdentityProviderAsync_ForSamlWithOidcNotNull_ThrowsControllerArgumentException()
    {
        // Arrange
        var identityProviderId = Guid.NewGuid();
        var data = _fixture.Build<IdentityProviderEditableDetails>()
            .With(x => x.oidc, _fixture.Create<IdentityProviderEditableDetailsOidc>())
            .With(x => x.saml, _fixture.Create<IdentityProviderEditableDetailsSaml>())
            .With(x => x.displayName, "new-display-name")
            .Create();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync(A<Guid>._, A<Guid>._, A<bool>._))
            .Returns((true, "cl1", IdentityProviderCategoryId.KEYCLOAK_SAML, IdentityProviderTypeId.OWN, Enumerable.Empty<(Guid, IEnumerable<string>)>(), false, string.Empty));

        // Act
        async Task Act() => await sut.UpdateOwnCompanyIdentityProviderAsync(identityProviderId, data).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be("property 'oidc' must be null (Parameter 'oidc')");
    }

    [Fact]
    public async Task UpdateOwnCompanyIdentityProviderAsync_WithValidSaml_CallsExpected()
    {
        // Arrange
        var identityProviderId = Guid.NewGuid();
        var data = _fixture.Build<IdentityProviderEditableDetails>()
            .With(x => x.oidc, (IdentityProviderEditableDetailsOidc?)null)
            .With(x => x.saml, _fixture.Build<IdentityProviderEditableDetailsSaml>().With(x => x.singleSignOnServiceUrl, "https://sso.com").Create())
            .With(x => x.displayName, "new-display-name")
            .Create();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync(A<Guid>._, A<Guid>._, A<bool>._))
            .Returns((true, "cl1", IdentityProviderCategoryId.KEYCLOAK_SAML, IdentityProviderTypeId.OWN, Enumerable.Empty<(Guid, IEnumerable<string>)>(), false, string.Empty));
        A.CallTo(() => _provisioningManager.GetCentralIdentityProviderDataSAMLAsync("cl1"))
            .Returns(_fixture.Build<IdentityProviderConfigSaml>().With(x => x.Enabled, true).With(x => x.DisplayName, "dis-saml").Create());
        A.CallTo(() => _provisioningManager.GetIdentityProviderMappers("cl1"))
            .Returns(_fixture.CreateMany<IdentityProviderMapperModel>(2).ToAsyncEnumerable());

        // Act
        var result = await sut.UpdateOwnCompanyIdentityProviderAsync(identityProviderId, data).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _provisioningManager.UpdateCentralIdentityProviderDataSAMLAsync(A<IdentityProviderEditableConfigSaml>.That.Matches(x => x.singleSignOnServiceUrl == "https://sso.com" && x.alias == "cl1")))
            .MustHaveHappenedOnceExactly();
        result.mappers.Should().HaveCount(2);
        result.displayName.Should().Be("dis-saml");
        result.enabled.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateOwnCompanyIdentityProviderAsync_ForSharedWithOidcNotNull_ThrowsControllerArgumentException()
    {
        // Arrange
        var identityProviderId = Guid.NewGuid();
        var data = _fixture.Build<IdentityProviderEditableDetails>()
            .With(x => x.oidc, _fixture.Create<IdentityProviderEditableDetailsOidc>())
            .With(x => x.displayName, "new-display-name")
            .Create();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync(A<Guid>._, A<Guid>._, A<bool>._))
            .Returns((true, "cl1", IdentityProviderCategoryId.KEYCLOAK_OIDC, IdentityProviderTypeId.SHARED, Enumerable.Empty<(Guid, IEnumerable<string>)>(), false, string.Empty));

        // Act
        async Task Act() => await sut.UpdateOwnCompanyIdentityProviderAsync(identityProviderId, data).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be("property 'oidc' must be null (Parameter 'oidc')");
    }

    [Fact]
    public async Task UpdateOwnCompanyIdentityProviderAsync_ForSharedWithSamlNotNull_ThrowsControllerArgumentException()
    {
        // Arrange
        var identityProviderId = Guid.NewGuid();
        var data = _fixture.Build<IdentityProviderEditableDetails>()
            .With(x => x.oidc, (IdentityProviderEditableDetailsOidc?)null)
            .With(x => x.saml, _fixture.Create<IdentityProviderEditableDetailsSaml>())
            .With(x => x.displayName, "new-display-name")
            .Create();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync(A<Guid>._, A<Guid>._, A<bool>._))
            .Returns((true, "cl1", IdentityProviderCategoryId.KEYCLOAK_OIDC, IdentityProviderTypeId.SHARED, Enumerable.Empty<(Guid, IEnumerable<string>)>(), false, string.Empty));

        // Act
        async Task Act() => await sut.UpdateOwnCompanyIdentityProviderAsync(identityProviderId, data).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be("property 'saml' must be null (Parameter 'saml')");
    }

    [Fact]
    public async Task UpdateOwnCompanyIdentityProviderAsync_WithValidShared_CallsExpected()
    {
        // Arrange
        var identityProviderId = Guid.NewGuid();
        var data = _fixture.Build<IdentityProviderEditableDetails>()
            .With(x => x.displayName, "dis-shared")
            .With(x => x.oidc, (IdentityProviderEditableDetailsOidc?)null)
            .With(x => x.saml, (IdentityProviderEditableDetailsSaml?)null)
            .Create();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync(A<Guid>._, A<Guid>._, A<bool>._))
            .Returns((true, "cl1", IdentityProviderCategoryId.KEYCLOAK_OIDC, IdentityProviderTypeId.SHARED, Enumerable.Empty<(Guid, IEnumerable<string>)>(), false, string.Empty));
        A.CallTo(() => _provisioningManager.GetCentralIdentityProviderDataOIDCAsync("cl1"))
            .Returns(_fixture.Build<IdentityProviderConfigOidc>().With(x => x.Enabled, true).With(x => x.DisplayName, "dis-shared").Create());
        A.CallTo(() => _provisioningManager.GetIdentityProviderMappers("cl1"))
            .Returns(_fixture.CreateMany<IdentityProviderMapperModel>(2).ToAsyncEnumerable());

        // Act
        var result = await sut.UpdateOwnCompanyIdentityProviderAsync(identityProviderId, data).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _provisioningManager.UpdateSharedIdentityProviderAsync("cl1", "dis-shared"))
            .MustHaveHappenedOnceExactly();
        result.mappers.Should().HaveCount(2);
        result.displayName.Should().Be("dis-shared");
        result.enabled.Should().BeTrue();
    }

    #endregion

    #region CreateOwnCompanyUserIdentityProviderLinkDataAsync

    [Fact]
    public async Task CreateOwnCompanyUserIdentityProviderLinkDataAsync_WithoutIamUserId_ThrowsNotFoundException()
    {
        // Arrange
        var identityProviderId = Guid.NewGuid();
        var companyUserId = Guid.NewGuid();
        var data = _fixture.Build<UserIdentityProviderLinkData>()
            .With(x => x.identityProviderId, identityProviderId)
            .Create();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetIamUserIsOwnCompanyIdentityProviderAliasAsync(companyUserId, identityProviderId, _identity.CompanyId))
            .Returns(((bool, string?, bool))default);

        // Act
        async Task Act() => await sut.CreateOwnCompanyUserIdentityProviderLinkDataAsync(companyUserId, data).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"companyUserId {companyUserId} does not exist");
    }

    [Fact]
    public async Task CreateOwnCompanyUserIdentityProviderLinkDataAsync_WithoutIamUserId_ThrowsUnexpectedConditionException()
    {
        // Arrange
        var identityProviderId = Guid.NewGuid();
        var companyUserId = Guid.NewGuid();
        var data = _fixture.Build<UserIdentityProviderLinkData>()
            .With(x => x.identityProviderId, identityProviderId)
            .Create();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetIamUserIsOwnCompanyIdentityProviderAliasAsync(companyUserId, identityProviderId, _identity.CompanyId))
            .Returns((true, "cl1", true));
        A.CallTo(() => _provisioningManager.GetUserByUserName(companyUserId.ToString()))
            .Returns((string?)null);

        // Act
        async Task Act() => await sut.CreateOwnCompanyUserIdentityProviderLinkDataAsync(companyUserId, data).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);
        ex.Message.Should().Be($"companyUserId {companyUserId} is not linked to keycloak");
    }

    [Fact]
    public async Task CreateOwnCompanyUserIdentityProviderLinkDataAsync_WithoutAlias_ThrowsNotFoundException()
    {
        // Arrange
        var identityProviderId = Guid.NewGuid();
        var companyUserId = Guid.NewGuid();
        var data = _fixture.Build<UserIdentityProviderLinkData>()
            .With(x => x.identityProviderId, identityProviderId)
            .Create();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetIamUserIsOwnCompanyIdentityProviderAliasAsync(companyUserId, identityProviderId, _identity.CompanyId))
            .Returns((true, null, false));

        // Act
        async Task Act() => await sut.CreateOwnCompanyUserIdentityProviderLinkDataAsync(companyUserId, data).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"identityProvider {identityProviderId} not found in company of user {companyUserId}");
    }

    [Fact]
    public async Task CreateOwnCompanyUserIdentityProviderLinkDataAsync_WithoutSameCompany_ThrowsForbiddenException()
    {
        // Arrange
        var identityProviderId = Guid.NewGuid();
        var companyUserId = Guid.NewGuid();
        var data = _fixture.Build<UserIdentityProviderLinkData>()
            .With(x => x.identityProviderId, identityProviderId)
            .Create();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetIamUserIsOwnCompanyIdentityProviderAliasAsync(companyUserId, identityProviderId, _identity.CompanyId))
            .Returns((true, "cl1", false));

        // Act
        async Task Act() => await sut.CreateOwnCompanyUserIdentityProviderLinkDataAsync(companyUserId, data).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be($"identityProvider {identityProviderId} is not associated with company {_identity.CompanyId}");
    }

    [Fact]
    public async Task CreateOwnCompanyUserIdentityProviderLinkDataAsync_WithKeycloakFailing_ThrowsForbiddenException()
    {
        // Arrange
        var identityProviderId = Guid.NewGuid();
        var iamUserId = _fixture.Create<string>();
        var companyUserId = Guid.NewGuid();
        var data = _fixture.Build<UserIdentityProviderLinkData>()
            .With(x => x.identityProviderId, identityProviderId)
            .Create();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetIamUserIsOwnCompanyIdentityProviderAliasAsync(companyUserId, identityProviderId, _identity.CompanyId))
            .Returns((true, "cl1", true));
        A.CallTo(() => _provisioningManager.GetUserByUserName(companyUserId.ToString())).Returns(iamUserId);
        A.CallTo(() => _provisioningManager.AddProviderUserLinkToCentralUserAsync(iamUserId, A<IdentityProviderLink>._))
            .Throws(new KeycloakEntityConflictException("test"));

        // Act
        async Task Act() => await sut.CreateOwnCompanyUserIdentityProviderLinkDataAsync(companyUserId, data).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"identityProviderLink for identityProvider {identityProviderId} already exists for user {companyUserId}");
    }

    [Fact]
    public async Task CreateOwnCompanyUserIdentityProviderLinkDataAsync_WithValid_CallsExpected()
    {
        // Arrange
        var identityProviderId = Guid.NewGuid();
        var iamUserId = _fixture.Create<string>();
        var companyUserId = Guid.NewGuid();
        var data = _fixture.Build<UserIdentityProviderLinkData>()
            .With(x => x.identityProviderId, identityProviderId)
            .With(x => x.userName, "test-user")
            .Create();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetIamUserIsOwnCompanyIdentityProviderAliasAsync(companyUserId, identityProviderId, _identity.CompanyId))
            .Returns((true, "cl1", true));
        A.CallTo(() => _provisioningManager.GetUserByUserName(companyUserId.ToString())).Returns(iamUserId);

        // Act
        var result = await sut.CreateOwnCompanyUserIdentityProviderLinkDataAsync(companyUserId, data).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _provisioningManager.AddProviderUserLinkToCentralUserAsync(iamUserId, A<IdentityProviderLink>._))
            .MustHaveHappenedOnceExactly();
        result.userName.Should().Be("test-user");
    }

    #endregion

    #region CreateOrUpdateOwnCompanyUserIdentityProviderLinkDataAsync

    [Fact]
    public async Task CreateOrUpdateOwnCompanyUserIdentityProviderLinkDataAsync_WithoutIamUserId_ThrowsNotFoundException()
    {
        // Arrange
        var identityProviderId = Guid.NewGuid();
        var companyUserId = Guid.NewGuid();
        var data = _fixture.Build<UserLinkData>()
            .With(x => x.userName, "user-name")
            .Create();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetIamUserIsOwnCompanyIdentityProviderAliasAsync(companyUserId, identityProviderId, _identity.CompanyId))
            .Returns(((bool, string?, bool))default);

        // Act
        async Task Act() => await sut.CreateOrUpdateOwnCompanyUserIdentityProviderLinkDataAsync(companyUserId, identityProviderId, data).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"companyUserId {companyUserId} does not exist");
    }

    [Fact]
    public async Task CreateOrUpdateOwnCompanyUserIdentityProviderLinkDataAsync_WithoutIamUserId_ThrowsUnexpectedConditionException()
    {
        // Arrange
        var identityProviderId = Guid.NewGuid();
        var companyUserId = Guid.NewGuid();
        var data = _fixture.Build<UserLinkData>()
            .With(x => x.userName, "user-name")
            .Create();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetIamUserIsOwnCompanyIdentityProviderAliasAsync(companyUserId, identityProviderId, _identity.CompanyId))
            .Returns((true, "cl1", true));
        A.CallTo(() => _provisioningManager.GetUserByUserName(companyUserId.ToString()))
            .Returns((string?)null);

        // Act
        async Task Act() => await sut.CreateOrUpdateOwnCompanyUserIdentityProviderLinkDataAsync(companyUserId, identityProviderId, data).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);
        ex.Message.Should().Be($"companyUserId {companyUserId} is not linked to keycloak");
    }

    [Fact]
    public async Task CreateOrUpdateOwnCompanyUserIdentityProviderLinkDataAsync_WithoutAlias_ThrowsNotFoundException()
    {
        // Arrange
        var identityProviderId = Guid.NewGuid();
        var companyUserId = Guid.NewGuid();
        var data = _fixture.Build<UserLinkData>()
            .With(x => x.userName, "user-name")
            .Create();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetIamUserIsOwnCompanyIdentityProviderAliasAsync(companyUserId, identityProviderId, _identity.CompanyId))
            .Returns((true, null, false));

        // Act
        async Task Act() => await sut.CreateOrUpdateOwnCompanyUserIdentityProviderLinkDataAsync(companyUserId, identityProviderId, data).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"identityProvider {identityProviderId} not found in company of user {companyUserId}");
    }

    [Fact]
    public async Task CreateOrUpdateOwnCompanyUserIdentityProviderLinkDataAsync_WithoutSameCompany_ThrowsForbiddenException()
    {
        // Arrange
        var identityProviderId = Guid.NewGuid();
        var companyUserId = Guid.NewGuid();
        var data = _fixture.Build<UserLinkData>()
            .With(x => x.userName, "user-name")
            .Create();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetIamUserIsOwnCompanyIdentityProviderAliasAsync(companyUserId, identityProviderId, _identity.CompanyId))
            .Returns((true, "cl1", false));

        // Act
        async Task Act() => await sut.CreateOrUpdateOwnCompanyUserIdentityProviderLinkDataAsync(companyUserId, identityProviderId, data).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be($"identityProvider {identityProviderId} is not associated with company {_identity.CompanyId}");
    }

    [Fact]
    public async Task CreateOrUpdateOwnCompanyUserIdentityProviderLinkDataAsync_WithValid_CallsExpected()
    {
        // Arrange
        var iamUserId = _fixture.Create<string>();
        var identityProviderId = Guid.NewGuid();
        var companyUserId = Guid.NewGuid();
        var data = _fixture.Build<UserLinkData>()
            .With(x => x.userName, "user-name")
            .Create();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetIamUserIsOwnCompanyIdentityProviderAliasAsync(companyUserId, identityProviderId, _identity.CompanyId))
            .Returns((true, "cl1", true));
        A.CallTo(() => _provisioningManager.GetUserByUserName(companyUserId.ToString()))
            .Returns(iamUserId);

        // Act
        var result = await sut.CreateOrUpdateOwnCompanyUserIdentityProviderLinkDataAsync(companyUserId, identityProviderId, data).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _provisioningManager.DeleteProviderUserLinkToCentralUserAsync(iamUserId, "cl1"))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.AddProviderUserLinkToCentralUserAsync(iamUserId, A<IdentityProviderLink>._))
            .MustHaveHappenedOnceExactly();
        result.userName.Should().Be("user-name");
    }

    #endregion

    #region GetOwnCompanyUserIdentityProviderLinkDataAsync

    [Fact]
    public async Task GetOwnCompanyUserIdentityProviderLinkDataAsync_WithoutIamUserId_ThrowsNotFoundException()
    {
        // Arrange
        var identityProviderId = Guid.NewGuid();
        var companyUserId = Guid.NewGuid();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetIamUserIsOwnCompanyIdentityProviderAliasAsync(companyUserId, identityProviderId, _identity.CompanyId))
            .Returns(((bool, string?, bool))default);

        // Act
        async Task Act() => await sut.GetOwnCompanyUserIdentityProviderLinkDataAsync(companyUserId, identityProviderId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"companyUserId {companyUserId} does not exist");
    }

    [Fact]
    public async Task GetOwnCompanyUserIdentityProviderLinkDataAsync_WithoutIamUserId_ThrowsUnexpectedConditionException()
    {
        // Arrange
        var identityProviderId = Guid.NewGuid();
        var companyUserId = Guid.NewGuid();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetIamUserIsOwnCompanyIdentityProviderAliasAsync(companyUserId, identityProviderId, _identity.CompanyId))
            .Returns((true, "cl1", true));
        A.CallTo(() => _provisioningManager.GetUserByUserName(companyUserId.ToString()))
            .Returns((string?)null);

        // Act
        async Task Act() => await sut.GetOwnCompanyUserIdentityProviderLinkDataAsync(companyUserId, identityProviderId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);
        ex.Message.Should().Be($"companyUserId {companyUserId} is not linked to keycloak");
    }

    [Fact]
    public async Task GetOwnCompanyUserIdentityProviderLinkDataAsync_WithoutAlias_ThrowsNotFoundException()
    {
        // Arrange
        var identityProviderId = Guid.NewGuid();
        var companyUserId = Guid.NewGuid();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetIamUserIsOwnCompanyIdentityProviderAliasAsync(companyUserId, identityProviderId, _identity.CompanyId))
            .Returns((true, null, false));

        // Act
        async Task Act() => await sut.GetOwnCompanyUserIdentityProviderLinkDataAsync(companyUserId, identityProviderId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"identityProvider {identityProviderId} not found in company of user {companyUserId}");
    }

    [Fact]
    public async Task GetOwnCompanyUserIdentityProviderLinkDataAsync_WithoutSameCompany_ThrowsForbiddenException()
    {
        // Arrange
        var identityProviderId = Guid.NewGuid();
        var companyUserId = Guid.NewGuid();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetIamUserIsOwnCompanyIdentityProviderAliasAsync(companyUserId, identityProviderId, _identity.CompanyId))
            .Returns((true, "cl1", false));

        // Act
        async Task Act() => await sut.GetOwnCompanyUserIdentityProviderLinkDataAsync(companyUserId, identityProviderId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be($"identityProvider {identityProviderId} is not associated with company {_identity.CompanyId}");
    }

    [Fact]
    public async Task GetOwnCompanyUserIdentityProviderLinkDataAsync_WithoutExistingCompanyUser_ThrowsNotFound()
    {
        // Arrange
        var iamUserId = _fixture.Create<string>();
        var identityProviderId = Guid.NewGuid();
        var companyUserId = Guid.NewGuid();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetIamUserIsOwnCompanyIdentityProviderAliasAsync(companyUserId, identityProviderId, _identity.CompanyId))
            .Returns((true, "cl1", true));
        A.CallTo(() => _provisioningManager.GetUserByUserName(companyUserId.ToString()))
            .Returns(iamUserId);
        A.CallTo(() => _provisioningManager.GetProviderUserLinkDataForCentralUserIdAsync(iamUserId))
            .Returns(Enumerable.Empty<IdentityProviderLink>().ToAsyncEnumerable());

        // Act
        async Task Act() => await sut.GetOwnCompanyUserIdentityProviderLinkDataAsync(companyUserId, identityProviderId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"identityProviderLink for identityProvider {identityProviderId} not found in keycloak for user {companyUserId}");

        A.CallTo(() => _provisioningManager.GetProviderUserLinkDataForCentralUserIdAsync(iamUserId))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetOwnCompanyUserIdentityProviderLinkDataAsync_WithValid_CallsExpected()
    {
        // Arrange
        var iamUserId = _fixture.Create<string>();
        var identityProviderId = Guid.NewGuid();
        var companyUserId = Guid.NewGuid();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetIamUserIsOwnCompanyIdentityProviderAliasAsync(companyUserId, identityProviderId, _identity.CompanyId))
            .Returns((true, "cl1", true));
        A.CallTo(() => _provisioningManager.GetUserByUserName(companyUserId.ToString()))
            .Returns(iamUserId);
        A.CallTo(() => _provisioningManager.GetProviderUserLinkDataForCentralUserIdAsync(iamUserId))
            .Returns(Enumerable.Repeat(new IdentityProviderLink("cl1", iamUserId, "user-name"), 1).ToAsyncEnumerable());

        // Act
        var result = await sut.GetOwnCompanyUserIdentityProviderLinkDataAsync(companyUserId, identityProviderId).ConfigureAwait(false);

        // Assert
        result.userName.Should().Be("user-name");
    }

    #endregion

    #region DeleteOwnCompanyUserIdentityProviderDataAsync

    [Fact]
    public async Task DeleteOwnCompanyUserIdentityProviderDataAsync_WithKeycloakError_ThrowsNotFound()
    {
        // Arrange
        var iamUserId = _fixture.Create<string>();
        var identityProviderId = Guid.NewGuid();
        var companyUserId = Guid.NewGuid();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetIamUserIsOwnCompanyIdentityProviderAliasAsync(companyUserId, identityProviderId, _identity.CompanyId))
            .Returns((true, "cl1", true));
        A.CallTo(() => _provisioningManager.GetUserByUserName(companyUserId.ToString()))
            .Returns(iamUserId);
        A.CallTo(() => _provisioningManager.DeleteProviderUserLinkToCentralUserAsync(iamUserId, "cl1"))
            .Throws(new KeycloakEntityNotFoundException("just a test"));

        // Act
        async Task Act() => await sut.DeleteOwnCompanyUserIdentityProviderDataAsync(companyUserId, identityProviderId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"identityProviderLink for identityProvider {identityProviderId} not found in keycloak for user {companyUserId}");
    }

    [Fact]
    public async Task DeleteOwnCompanyUserIdentityProviderDataAsync_WithValid_CallsExpected()
    {
        // Arrange
        var iamUserId = _fixture.Create<string>();
        var identityProviderId = Guid.NewGuid();
        var companyUserId = Guid.NewGuid();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetIamUserIsOwnCompanyIdentityProviderAliasAsync(companyUserId, identityProviderId, _identity.CompanyId))
            .Returns((true, "cl1", true));
        A.CallTo(() => _provisioningManager.GetUserByUserName(companyUserId.ToString()))
            .Returns(iamUserId);

        // Act
        await sut.DeleteOwnCompanyUserIdentityProviderDataAsync(companyUserId, identityProviderId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _provisioningManager.DeleteProviderUserLinkToCentralUserAsync(iamUserId, "cl1"))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region GetOwnCompanyUsersIdentityProviderDataAsync

    [Fact]
    public async Task GetOwnCompanyUsersIdentityProviderDataAsync_WithoutIdentityProviderIds_ThrowsControllerArgumentException()
    {
        // Arrange
        var iamUserId = Guid.NewGuid();
        var identityProviderId = Guid.NewGuid();
        var companyUserId = Guid.NewGuid();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetIamUserIsOwnCompanyIdentityProviderAliasAsync(companyUserId, identityProviderId, _identity.CompanyId))
            .Returns((true, "cl1", true));

        // Act
        async Task Act() => await sut.GetOwnCompanyUsersIdentityProviderDataAsync(Enumerable.Empty<Guid>(), false).ToListAsync().ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be("at least one identityProviderId must be specified (Parameter 'identityProviderIds')");
    }

    [Fact]
    public async Task GetOwnCompanyUsersIdentityProviderDataAsync_WithoutMatchingIdps_ThrowsControllerArgumentException()
    {
        // Arrange
        var identityProviderId = Guid.NewGuid();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetOwnCompanyIdentityProviderAliasDataUntracked(_identity.CompanyId, A<IEnumerable<Guid>>._))
            .Returns(Enumerable.Empty<(Guid, string)>().ToAsyncEnumerable());

        // Act
        async Task Act() => await sut.GetOwnCompanyUsersIdentityProviderDataAsync(Enumerable.Repeat(identityProviderId, 1), false).ToListAsync().ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be($"invalid identityProviders: [{identityProviderId}] for company {_identity.CompanyId} (Parameter 'identityProviderIds')");
    }

    #endregion

    #region GetOwnIdentityProviderWithConnectedCompanies

    [Fact]
    public async Task GetOwnIdentityProviderWithConnectedCompanies_WithDifferentCompany_ThrowsConflictException()
    {
        // Arrange
        var identityProviderId = Guid.NewGuid();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetOwnIdentityProviderWithConnectedCompanies(identityProviderId, _companyId))
            .Returns((string.Empty, IdentityProviderCategoryId.KEYCLOAK_OIDC, false, IdentityProviderTypeId.OWN, Enumerable.Empty<ConnectedCompanyData>()));

        // Act
        async Task Act() => await sut.GetOwnIdentityProviderWithConnectedCompanies(identityProviderId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"identityProvider {identityProviderId} is not associated with company {_companyId}");
    }

    [Fact]
    public async Task GetOwnIdentityProviderWithConnectedCompanies_WithAliasNull_ThrowsNotFoundException()
    {
        // Arrange
        var identityProviderId = Guid.NewGuid();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetOwnIdentityProviderWithConnectedCompanies(identityProviderId, _companyId))
            .Returns((null, IdentityProviderCategoryId.KEYCLOAK_OIDC, true, IdentityProviderTypeId.OWN, Enumerable.Empty<ConnectedCompanyData>()));

        // Act
        async Task Act() => await sut.GetOwnIdentityProviderWithConnectedCompanies(identityProviderId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"identityProvider {identityProviderId} does not exist");
    }

    [Fact]
    public async Task GetOwnIdentityProviderWithConnectedCompanies_WithOidcWithoutExistingKeycloakClient_CallsExpected()
    {
        // Arrange
        var identityProviderId = Guid.NewGuid();
        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetOwnIdentityProviderWithConnectedCompanies(identityProviderId, _companyId))
            .Returns(("cl1", IdentityProviderCategoryId.KEYCLOAK_OIDC, true, IdentityProviderTypeId.OWN, Enumerable.Empty<ConnectedCompanyData>()));
        A.CallTo(() => _provisioningManager.GetCentralIdentityProviderDataOIDCAsync("cl1"))
            .Throws(new KeycloakEntityNotFoundException("cl1 not existing"));

        // Act
        var result = await sut.GetOwnIdentityProviderWithConnectedCompanies(identityProviderId).ConfigureAwait(false);

        // Assert
        result.DisplayName.Should().BeNull();
        result.Enabled.Should().BeNull();
    }

    [Fact]
    public async Task GetOwnIdentityProviderWithConnectedCompanies_WithValidOidc_CallsExpected()
    {
        // Arrange
        var identityProviderId = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetOwnIdentityProviderWithConnectedCompanies(identityProviderId, _companyId))
            .Returns(("cl1", IdentityProviderCategoryId.KEYCLOAK_OIDC, true, IdentityProviderTypeId.OWN, Enumerable.Repeat(new ConnectedCompanyData(companyId, "Test Company"), 1)));
        A.CallTo(() => _provisioningManager.GetCentralIdentityProviderDataOIDCAsync("cl1"))
            .Returns(_fixture.Build<IdentityProviderConfigOidc>().With(x => x.Enabled, true).With(x => x.DisplayName, "dis-oidc").Create());
        A.CallTo(() => _provisioningManager.GetIdentityProviderMappers("cl1"))
            .Returns(_fixture.CreateMany<IdentityProviderMapperModel>(3).ToAsyncEnumerable());

        // Act
        var result = await sut.GetOwnIdentityProviderWithConnectedCompanies(identityProviderId).ConfigureAwait(false);

        // Assert
        result.DisplayName.Should().Be("dis-oidc");
        result.Enabled.Should().BeTrue();
        result.ConnectedCompanies.Should().ContainSingle().And.Satisfy(x => x.CompanyId == companyId);
    }

    [Fact]
    public async Task GetOwnIdentityProviderWithConnectedCompanies_WithSamlWithoutExistingKeycloakClient_CallsExpected()
    {
        // Arrange
        var identityProviderId = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetOwnIdentityProviderWithConnectedCompanies(identityProviderId, _companyId))
            .Returns(("saml-alias", IdentityProviderCategoryId.KEYCLOAK_SAML, true, IdentityProviderTypeId.OWN, Enumerable.Repeat(new ConnectedCompanyData(companyId, "Test Company"), 1)));
        A.CallTo(() => _provisioningManager.GetCentralIdentityProviderDataSAMLAsync("saml-alias"))
            .Throws(new KeycloakEntityNotFoundException("saml-alias"));

        // Act
        var result = await sut.GetOwnIdentityProviderWithConnectedCompanies(identityProviderId).ConfigureAwait(false);

        // Assert
        result.DisplayName.Should().BeNull();
        result.Enabled.Should().BeNull();
        result.ConnectedCompanies.Should().ContainSingle().And.Satisfy(x => x.CompanyId == companyId);
    }

    [Fact]
    public async Task GetOwnIdentityProviderWithConnectedCompanies_WithValidSaml_CallsExpected()
    {
        // Arrange
        var identityProviderId = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        var sut = new IdentityProviderBusinessLogic(
            _portalRepositories,
            _provisioningManager,
            _identityService,
            _errorMessageService,
            _roleBaseMailService,
            _mailingService,
            _options,
            _logger);
        A.CallTo(() => _identityProviderRepository.GetOwnIdentityProviderWithConnectedCompanies(identityProviderId, _companyId))
            .Returns(("saml-alias", IdentityProviderCategoryId.KEYCLOAK_SAML, true, IdentityProviderTypeId.OWN, Enumerable.Repeat(new ConnectedCompanyData(companyId, "Test Company"), 1)));
        A.CallTo(() => _provisioningManager.GetCentralIdentityProviderDataSAMLAsync("saml-alias"))
            .Returns(_fixture.Build<IdentityProviderConfigSaml>().With(x => x.Enabled, true).With(x => x.DisplayName, "dis-saml").Create());
        A.CallTo(() => _provisioningManager.GetIdentityProviderMappers("saml-alias"))
            .Returns(_fixture.CreateMany<IdentityProviderMapperModel>(2).ToAsyncEnumerable());

        // Act
        var result = await sut.GetOwnIdentityProviderWithConnectedCompanies(identityProviderId).ConfigureAwait(false);

        // Assert
        result.DisplayName.Should().Be("dis-saml");
        result.Enabled.Should().BeTrue();
        result.ConnectedCompanies.Should().ContainSingle().And.Satisfy(x => x.CompanyId == companyId);
    }

    #endregion

    #region Setup

    private void SetupCreateOwnCompanyIdentityProvider(IamIdentityProviderProtocol protocol = IamIdentityProviderProtocol.OIDC, ICollection<IdentityProvider>? idps = null, ICollection<CompanyIdentityProvider>? companyIdps = null, ICollection<IamIdentityProvider>? iamIdps = null)
    {
        A.CallTo(() => _companyRepository.CheckCompanyAndCompanyRolesAsync(_identity.CompanyId, A<IEnumerable<CompanyRoleId>>._))
            .Returns((true, "test", true));
        A.CallTo(() => _companyRepository.CheckCompanyAndCompanyRolesAsync(_invalidCompanyId, A<IEnumerable<CompanyRoleId>>._))
            .Returns((true, "test", false));
        A.CallTo(() => _companyRepository.CheckCompanyAndCompanyRolesAsync(A<Guid>.That.Not.Matches(x => x == _identity.CompanyId || x == _invalidCompanyId), A<IEnumerable<CompanyRoleId>>._))
            .Returns(((bool, string, bool))default);

        if (idps != null)
        {
            A.CallTo(() => _identityProviderRepository.CreateIdentityProvider(A<IdentityProviderCategoryId>._, A<IdentityProviderTypeId>._, A<Guid>._, A<Action<IdentityProvider>?>._))
                .ReturnsLazily((IdentityProviderCategoryId identityProviderCategory, IdentityProviderTypeId identityProviderTypeId, Guid owner, Action<IdentityProvider>? setOptionalFields) =>
                {
                    var idp = new IdentityProvider(_identityProviderId, identityProviderCategory, identityProviderTypeId, owner, DateTimeOffset.UtcNow);
                    setOptionalFields?.Invoke(idp);
                    idps.Add(idp);
                    return idp;
                });
        }

        if (companyIdps != null)
        {
            A.CallTo(() => _identityProviderRepository.CreateCompanyIdentityProvider(A<Guid>._, A<Guid>._))
                .ReturnsLazily((Guid companyId, Guid identityProviderId) =>
                {
                    var companyIdp = new CompanyIdentityProvider(companyId, identityProviderId);
                    companyIdps.Add(companyIdp);
                    return companyIdp;
                });
        }

        if (iamIdps != null)
        {
            A.CallTo(() => _identityProviderRepository.CreateIamIdentityProvider(A<Guid>._, A<string>._))
                .ReturnsLazily((Guid identityProviderId, string idpAlias) =>
                {
                    var iamIdp = new IamIdentityProvider(idpAlias, identityProviderId);
                    iamIdps.Add(iamIdp);
                    return iamIdp;
                });
        }

        switch (protocol)
        {
            case IamIdentityProviderProtocol.OIDC:
                A.CallTo(() => _provisioningManager.GetCentralIdentityProviderDataOIDCAsync(A<string>._))
                    .Returns(new IdentityProviderConfigOidc("test-oidc", "https://redirect.com/*", "cl1-oidc", true, "https://auth.com", IamIdentityProviderClientAuthMethod.SECRET_JWT, IamIdentityProviderSignatureAlgorithm.RS512));
                break;
            case IamIdentityProviderProtocol.SAML:
                A.CallTo(() => _provisioningManager.GetCentralIdentityProviderDataSAMLAsync(A<string>._))
                    .Returns(new IdentityProviderConfigSaml("test-saml", "https://redirect.com/*", "cl1-saml", true, Guid.NewGuid().ToString(), "https://sso.com"));
                break;
            default: throw new NotImplementedException();
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
        A.CallTo(() => _document.OpenReadStream()).Returns(new AsyncEnumerableStringStream(lines.ToAsyncEnumerable(), _encoding));

        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IIdentityProviderRepository>()).Returns(_identityProviderRepository);

        A.CallTo(() => _userRepository.GetUserEntityDataAsync(A<Guid>._, A<Guid>._)).ReturnsLazily((Guid companyUserId, Guid _) =>
            userData.Where(d => d.CompanyUserId == companyUserId)
                .Select(d =>
                    (
                        d.FirstName,
                        d.LastName,
                        d.Email
                    )).FirstOrDefault());

        A.CallTo(() => _provisioningManager.GetUserByUserName(A<string>._)).ReturnsLazily((string userName) =>
            userData.SingleOrDefault(x => x.CompanyUserId == Guid.Parse(userName))?.IamUserId);

        A.CallTo(() => _identityProviderRepository.GetCompanyIdentityProviderCategoryDataUntracked(A<Guid>.That.Not.IsEqualTo(_companyId))).Returns(
            Enumerable.Empty<(Guid, IdentityProviderCategoryId, string?, IdentityProviderTypeId)>().ToAsyncEnumerable());
        A.CallTo(() => _identityProviderRepository.GetCompanyIdentityProviderCategoryDataUntracked(A<Guid>.That.IsEqualTo(_companyId))).Returns(
            new (Guid, IdentityProviderCategoryId, string?, IdentityProviderTypeId)[] {
                (_sharedIdentityProviderId, IdentityProviderCategoryId.KEYCLOAK_OIDC, _sharedIdpAlias, IdentityProviderTypeId.SHARED),
                (_otherIdentityProviderId, IdentityProviderCategoryId.KEYCLOAK_OIDC, _otherIdpAlias, IdentityProviderTypeId.OWN),
            }.ToAsyncEnumerable());

        A.CallTo(() => _identityProviderRepository.CreateIdentityProvider(A<IdentityProviderCategoryId>._, A<IdentityProviderTypeId>._, A<Guid>._, A<Action<IdentityProvider>?>._))
            .ReturnsLazily((IdentityProviderCategoryId categoryId, IdentityProviderTypeId typeId, Guid owner, Action<IdentityProvider>? setOptionalFields) =>
            {
                var idp = new IdentityProvider(_sharedIdentityProviderId, categoryId, typeId, owner, _fixture.Create<DateTimeOffset>());
                setOptionalFields?.Invoke(idp);
                return idp;
            });

        A.CallTo(() => _provisioningManager.GetProviderUserLinkDataForCentralUserIdAsync(A<string>._)).ReturnsLazily((Func<string, IAsyncEnumerable<IdentityProviderLink>>)((string iamUserId) =>
        {
            var user = userData.First<TestUserData>((Func<TestUserData, bool>)(u => u.IamUserId == iamUserId));
            return (new[] {
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
            }).ToAsyncEnumerable<IdentityProviderLink>();
        }));
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

    private record TestUserData(Guid CompanyUserId, string IamUserId, string FirstName, string LastName, string Email, string SharedIdpUserId, string SharedIdpUserName, string OtherIdpUserId, string OtherIdpUserName);

    #endregion
}
