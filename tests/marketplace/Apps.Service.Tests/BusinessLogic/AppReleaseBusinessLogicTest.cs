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

using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Apps.Service.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Apps.Service.ViewModels;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Identity;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Service;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Web;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;
using System.Collections.Immutable;
using Xunit;

namespace Org.Eclipse.TractusX.Portal.Backend.Apps.Service.BusinessLogic.Tests;

public class AppReleaseBusinessLogicTest
{
    private readonly IFixture _fixture;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IOfferRepository _offerRepository;
    private readonly IUserRolesRepository _userRolesRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly IOptions<AppsSettings> _options;
    private readonly CompanyUser _companyUser;
    private readonly Guid _companyId = Guid.NewGuid();
    private readonly IIdentityData _identity;
    private readonly IOfferService _offerService;
    private readonly Guid _notExistingAppId = Guid.NewGuid();
    private readonly Guid _activeAppId = Guid.NewGuid();
    private readonly Guid _differentCompanyAppId = Guid.NewGuid();
    private readonly Guid _existingAppId = Guid.NewGuid();
    private readonly IEnumerable<Guid> _useCases = new[] { Guid.NewGuid(), Guid.NewGuid() };
    private readonly IEnumerable<string> _languageCodes = new[] { "de", "en" };
    private readonly AppUpdateData _appUpdateData;
    private readonly ILanguageRepository _languageRepository;
    private readonly AppsSettings _settings;
    private const string ClientId = "catenax-portal";
    private readonly IOfferSetupService _offerSetupService;
    private readonly AppReleaseBusinessLogic _sut;
    private readonly IOfferDocumentService _offerDocumentService;
    private readonly IIdentityService _identityService;

    public AppReleaseBusinessLogicTest()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.ConfigureFixture();

        _portalRepositories = A.Fake<IPortalRepositories>();
        _offerRepository = A.Fake<IOfferRepository>();
        _userRolesRepository = A.Fake<IUserRolesRepository>();
        _documentRepository = A.Fake<IDocumentRepository>();
        _languageRepository = A.Fake<ILanguageRepository>();
        _offerService = A.Fake<IOfferService>();
        _offerDocumentService = A.Fake<IOfferDocumentService>();
        _offerSetupService = A.Fake<IOfferSetupService>();
        _options = A.Fake<IOptions<AppsSettings>>();

        var identity = new Identity(Guid.NewGuid(), DateTimeOffset.UtcNow, Guid.NewGuid(), UserStatusId.ACTIVE, IdentityTypeId.COMPANY_USER);

        _companyUser = _fixture.Build<CompanyUser>()
            .With(u => u.Identity, identity)
            .Create();
        _identity = A.Fake<IIdentityData>();
        _identityService = A.Fake<IIdentityService>();
        A.CallTo(() => _identity.IdentityId).Returns(_companyUser.Id);
        A.CallTo(() => _identity.IdentityTypeId).Returns(IdentityTypeId.COMPANY_USER);
        A.CallTo(() => _identity.CompanyId).Returns(_companyId);
        A.CallTo(() => _identityService.IdentityData).Returns(_identity);

        _settings = new AppsSettings
        {
            BasePortalAddress = "https://test.com/",
            OfferStatusIds = new[]
            {
                OfferStatusId.IN_REVIEW,
                OfferStatusId.ACTIVE
            },
            ActiveAppNotificationTypeIds = new[]
            {
                NotificationTypeId.APP_ROLE_ADDED
            },
            SubmitAppNotificationTypeIds = new[]
            {
                NotificationTypeId.APP_RELEASE_REQUEST
            },
            ActiveAppCompanyAdminRoles = new[]
            {
                new UserRoleConfig(ClientId, new [] { "Company Admin" })
            },
            OfferSubscriptionAddress = "https://acitvationAppTest.com",
            OfferDetailAddress = "https://detailAppTest.com"
        };

        A.CallTo(() => _options.Value).Returns(_settings);
        A.CallTo(() => _portalRepositories.GetInstance<IOfferRepository>()).Returns(_offerRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRolesRepository>()).Returns(_userRolesRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IDocumentRepository>()).Returns(_documentRepository);

        _appUpdateData = _fixture.Build<AppUpdateData>()
            .With(x => x.OfferState, OfferStatusId.CREATED)
            .With(x => x.IsUserOfProvider, true)
            .With(x => x.MatchingUseCases, _useCases)
            .With(x => x.Languages, _languageCodes.Select(x => (x, true)))
            .Create();

        _sut = new AppReleaseBusinessLogic(_portalRepositories, _options, _offerService, _offerDocumentService, _offerSetupService, _identityService);
    }

    [Fact]
    public async Task GetAppAgreementData_ReturnsExpectedResult()
    {
        //Arrange
        var data = _fixture.CreateMany<AgreementDocumentData>(5).ToAsyncEnumerable();
        A.CallTo(() => _offerService.GetOfferTypeAgreements(OfferTypeId.APP, Constants.DefaultLanguage))
            .Returns(data);

        //Act
        var result = await _sut.GetOfferAgreementDataAsync(Constants.DefaultLanguage).ToListAsync();

        // Assert 
        A.CallTo(() => _offerService.GetOfferTypeAgreements(OfferTypeId.APP, Constants.DefaultLanguage))
            .MustHaveHappenedOnceExactly();
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task CreateServiceOffering_WithValidDataAndEmptyDescriptions_ReturnsCorrectDetails()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var appUserRoles = _fixture.CreateMany<string>(3).Select(role => new AppUserRole(role, _fixture.CreateMany<AppUserRoleDescription>(2).ToImmutableArray())).ToImmutableArray();

        A.CallTo(() => _offerRepository.IsProviderCompanyUserAsync(appId, _identity.CompanyId, OfferTypeId.APP))
            .Returns((true, true));

        A.CallTo(() => _userRolesRepository.GetUserRolesForOfferIdAsync(appId))
            .Returns(new[] { "Exsiting Role" }.ToAsyncEnumerable());

        IEnumerable<UserRole>? userRoles = null;
        A.CallTo(() => _userRolesRepository.CreateAppUserRoles(A<IEnumerable<(Guid, string)>>._))
            .ReturnsLazily((IEnumerable<(Guid AppId, string Role)> appRoles) =>
            {
                userRoles = appRoles.Select(x => new UserRole(Guid.NewGuid(), x.Role, x.AppId)).ToImmutableArray();
                return userRoles;
            });

        var userRoleDescriptions = new List<IEnumerable<UserRoleDescription>>();
        A.CallTo(() => _userRolesRepository.CreateAppUserRoleDescriptions(A<IEnumerable<(Guid, string, string)>>._))
            .ReturnsLazily((IEnumerable<(Guid RoleId, string LanguageCode, string Description)> roleLanguageDescriptions) =>
            {
                var createdUserRoleDescriptions = roleLanguageDescriptions.Select(x => new UserRoleDescription(x.RoleId, x.LanguageCode, x.Description)).ToImmutableArray();
                userRoleDescriptions.Add(createdUserRoleDescriptions);
                return createdUserRoleDescriptions;
            });
        var existingOffer = _fixture.Create<Offer>();
        existingOffer.DateLastChanged = DateTimeOffset.UtcNow;
        A.CallTo(() => _offerRepository.AttachAndModifyOffer(appId, A<Action<Offer>>._, A<Action<Offer>?>._))
            .Invokes((Guid _, Action<Offer> setOptionalParameters, Action<Offer>? initializeParemeters) =>
            {
                initializeParemeters?.Invoke(existingOffer);
                setOptionalParameters(existingOffer);
            });
        // Act
        var result = await _sut.AddAppUserRoleAsync(appId, appUserRoles);

        // Assert
        A.CallTo(() => _offerRepository.IsProviderCompanyUserAsync(A<Guid>._, A<Guid>._, A<OfferTypeId>._)).MustHaveHappened();

        A.CallTo(() => _userRolesRepository.CreateAppUserRoles(A<IEnumerable<(Guid, string)>>._)).MustHaveHappenedOnceExactly();
        userRoles.Should().NotBeNull()
            .And.HaveSameCount(appUserRoles)
            .And.AllSatisfy(x =>
            {
                x.Id.Should().NotBeEmpty();
                x.OfferId.Should().Be(appId);
            })
            .And.Satisfy(
                x => x.UserRoleText == appUserRoles[0].Role,
                x => x.UserRoleText == appUserRoles[1].Role,
                x => x.UserRoleText == appUserRoles[2].Role
            );
        A.CallTo(() => _offerRepository.AttachAndModifyOffer(appId, A<Action<Offer>>._, A<Action<Offer>?>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _userRolesRepository.CreateAppUserRoleDescriptions(A<IEnumerable<(Guid, string, string)>>._)).MustHaveHappened(appUserRoles.Length, Times.Exactly);
        userRoleDescriptions.Should()
            .HaveSameCount(appUserRoles)
            .And.SatisfyRespectively(
                x => x.Should().HaveCount(2).And.Satisfy(
                    x => x.UserRoleId == userRoles!.ElementAt(0).Id && x.LanguageShortName == appUserRoles[0].Descriptions.ElementAt(0).LanguageCode && x.Description == appUserRoles[0].Descriptions.ElementAt(0).Description,
                    x => x.UserRoleId == userRoles!.ElementAt(0).Id && x.LanguageShortName == appUserRoles[0].Descriptions.ElementAt(1).LanguageCode && x.Description == appUserRoles[0].Descriptions.ElementAt(1).Description),
                x => x.Should().HaveCount(2).And.Satisfy(
                    x => x.UserRoleId == userRoles!.ElementAt(1).Id && x.LanguageShortName == appUserRoles[1].Descriptions.ElementAt(0).LanguageCode && x.Description == appUserRoles[1].Descriptions.ElementAt(0).Description,
                    x => x.UserRoleId == userRoles!.ElementAt(1).Id && x.LanguageShortName == appUserRoles[1].Descriptions.ElementAt(1).LanguageCode && x.Description == appUserRoles[1].Descriptions.ElementAt(1).Description),
                x => x.Should().HaveCount(2).And.Satisfy(
                    x => x.UserRoleId == userRoles!.ElementAt(2).Id && x.LanguageShortName == appUserRoles[2].Descriptions.ElementAt(0).LanguageCode && x.Description == appUserRoles[2].Descriptions.ElementAt(0).Description,
                    x => x.UserRoleId == userRoles!.ElementAt(2).Id && x.LanguageShortName == appUserRoles[2].Descriptions.ElementAt(1).LanguageCode && x.Description == appUserRoles[2].Descriptions.ElementAt(1).Description));

        Assert.NotNull(result);
        Assert.IsAssignableFrom<IEnumerable<AppRoleData>>(result);
        result.Should().NotBeNull()
            .And.HaveSameCount(appUserRoles)
            .And.Satisfy(
                x => x.RoleId == userRoles!.ElementAt(0).Id && x.RoleName == appUserRoles[0].Role,
                x => x.RoleId == userRoles!.ElementAt(1).Id && x.RoleName == appUserRoles[1].Role,
                x => x.RoleId == userRoles!.ElementAt(2).Id && x.RoleName == appUserRoles[2].Role
            );
    }

    [Fact]
    public async Task CreateServiceOffering_WithExistingData_NoRowEffecting()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var appUserRoles = _fixture.CreateMany<string>(3).Select(role => new AppUserRole(role, _fixture.CreateMany<AppUserRoleDescription>(2).ToImmutableArray())).ToImmutableArray();

        A.CallTo(() => _offerRepository.IsProviderCompanyUserAsync(appId, _identity.CompanyId, OfferTypeId.APP))
            .Returns((true, true));

        A.CallTo(() => _userRolesRepository.GetUserRolesForOfferIdAsync(appId))
            .Returns(appUserRoles.Select(x => x.Role).ToAsyncEnumerable());

        IEnumerable<UserRole>? userRoles = null;
        A.CallTo(() => _userRolesRepository.CreateAppUserRoles(A<IEnumerable<(Guid, string)>>._))
            .ReturnsLazily((IEnumerable<(Guid AppId, string Role)> appRoles) =>
            {
                userRoles = appRoles.Select(x => new UserRole(Guid.NewGuid(), x.Role, x.AppId)).ToImmutableArray();
                return userRoles;
            });

        var userRoleDescriptions = new List<IEnumerable<UserRoleDescription>>();
        A.CallTo(() => _userRolesRepository.CreateAppUserRoleDescriptions(A<IEnumerable<(Guid, string, string)>>._))
            .ReturnsLazily((IEnumerable<(Guid RoleId, string LanguageCode, string Description)> roleLanguageDescriptions) =>
            {
                var createdUserRoleDescriptions = roleLanguageDescriptions.Select(x => new UserRoleDescription(x.RoleId, x.LanguageCode, x.Description)).ToImmutableArray();
                userRoleDescriptions.Add(createdUserRoleDescriptions);
                return createdUserRoleDescriptions;
            });
        var existingOffer = _fixture.Create<Offer>();
        existingOffer.DateLastChanged = DateTimeOffset.UtcNow;
        A.CallTo(() => _offerRepository.AttachAndModifyOffer(appId, A<Action<Offer>>._, A<Action<Offer>?>._))
            .Invokes((Guid _, Action<Offer> setOptionalParameters, Action<Offer>? initializeParemeters) =>
            {
                initializeParemeters?.Invoke(existingOffer);
                setOptionalParameters(existingOffer);
            });
        // Act
        var result = await _sut.AddAppUserRoleAsync(appId, appUserRoles);

        // Assert
        A.CallTo(() => _offerRepository.IsProviderCompanyUserAsync(A<Guid>._, A<Guid>._, A<OfferTypeId>._)).MustHaveHappened();

        A.CallTo(() => _userRolesRepository.CreateAppUserRoles(A<IEnumerable<(Guid, string)>>._)).MustHaveHappenedOnceExactly();
        userRoles.Should().NotBeNull()
            .And.HaveCount(0);

        A.CallTo(() => _offerRepository.AttachAndModifyOffer(appId, A<Action<Offer>>._, A<Action<Offer>?>._)).MustNotHaveHappened();
        A.CallTo(() => _userRolesRepository.CreateAppUserRoleDescriptions(A<IEnumerable<(Guid, string, string)>>._)).MustNotHaveHappened();
        userRoleDescriptions.Should().NotBeNull()
            .And.HaveCount(0);

        result.Should().NotBeNull()
            .And.HaveCount(0);
    }

    [Fact]
    public async Task CreateServiceOffering_WithValidButDuplicateData_ThrowsControllerArgumentException()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var roleId = _fixture.Create<string>();
        var appUserRoles = _fixture.CreateMany<string>(3).Select(role => new AppUserRole(roleId, _fixture.CreateMany<AppUserRoleDescription>(2).ToImmutableArray())).ToImmutableArray();

        // Act
        async Task Act() => await _sut.AddAppUserRoleAsync(appId, appUserRoles);

        // Assert
        var error = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        error.Message.Should().Be(AppExtensionErrors.APP_ARG_ROLES_ARE_AMBIGUOUS.ToString());
    }

    #region AddAppAsync

    [Fact]
    public async Task AddAppAsync_WithoutEmptyLanguageCodes_ThrowsException()
    {
        // Arrange
        var data = _fixture.Build<AppRequestModel>()
            .With(x => x.SupportedLanguageCodes, new[] { String.Empty })
            .Create();

        // Act
        async Task Act() => await _sut.AddAppAsync(data);

        // Assert
        var error = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        error.Parameters.First().Name.Should().Be("SupportedLanguageCodes");
    }

    [Fact]
    public async Task AddAppAsync_WithEmptyUseCaseIds_ThrowsException()
    {
        // Arrange
        var data = _fixture.Build<AppRequestModel>()
            .With(x => x.UseCaseIds, new[] { Guid.Empty })
            .Create();

        // Act
        async Task Act() => await _sut.AddAppAsync(data);

        // Assert
        var error = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        error.Parameters.First().Name.Should().Be("UseCaseIds");
    }

    [Fact]
    public async Task AddAppAsync_WithSalesManagerValidData_ReturnsExpected()
    {
        var data = new AppRequestModel(
            "Titlebfd7fa50-cb36-4940-96f7-0442bbc41b70",
            _companyUser.Id,
            Enumerable.Repeat(Guid.NewGuid(), 1),
            Enumerable.Repeat(new LocalizedDescription("en", "ProviderXyz", "XYZ"), 1),
            Enumerable.Repeat("en", 1),
            "Price90b6fed6-799a-4238-bee2-5cd84bc3f167",
            Enumerable.Repeat(PrivacyPolicyId.COMPANY_DATA, 1),
            "ProviderUri5ef769cd-9be3-4d3f-8a3e-c886793a732b",
            "ContactNumber13081a1d-24fc-4f32-8163-f77a8a7fb724",
            "ContactEmailb6e3cdcc-7d08-4dd5-9e3c-c9564129558a");

        Offer? created = null;
        var offerId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        A.CallTo(() => _offerRepository.CreateOffer(A<OfferTypeId>._, A<Guid>._, A<Action<Offer>?>._))
            .ReturnsLazily((OfferTypeId offerTypeId, Guid providerCompanyId, Action<Offer>? modify) =>
            {
                created = new Offer(offerId, providerCompanyId, now, offerTypeId);
                modify?.Invoke(created);
                return created;
            });

        OfferLicense? offerLicense = null;
        A.CallTo(() => _offerRepository.CreateOfferLicenses(data.Price)).ReturnsLazily((string text) =>
        {
            offerLicense = new OfferLicense(Guid.NewGuid(), text);
            return offerLicense;
        });

        // Act
        await _sut.AddAppAsync(data);

        // Assert
        A.CallTo(() => _offerService.ValidateSalesManager(_companyUser.Id, A<IEnumerable<UserRoleConfig>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.CreateOffer(A<OfferTypeId>._, A<Guid>._, A<Action<Offer>?>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.AddOfferDescriptions(A<IEnumerable<(Guid appId, string languageShortName, string descriptionLong, string descriptionShort)>>.That.Matches(x => x.All(y => y.appId == offerId))))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.AddAppLanguages(A<IEnumerable<(Guid appId, string languageShortName)>>.That.Matches(x => x.All(y => y.appId == offerId) && x.Select(y => y.languageShortName).SequenceEqual(data.SupportedLanguageCodes))))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.AddAppAssignedUseCases(A<IEnumerable<(Guid appId, Guid useCaseId)>>.That.Matches(x => x.All(y => y.appId == offerId) && x.Select(y => y.useCaseId).SequenceEqual(data.UseCaseIds))))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.CreateOfferLicenses(data.Price))
            .MustHaveHappenedOnceExactly();

        offerLicense.Should().NotBeNull();
        A.CallTo(() => _offerRepository.CreateOfferAssignedLicense(offerId, offerLicense!.Id))
            .MustHaveHappenedOnceExactly();

        created.Should().NotBeNull()
            .And.Match<Offer>(x =>
                x.Id == offerId &&
                x.Name == data.Title &&
                x.ProviderCompanyId == _identity.CompanyId &&
                x.OfferStatusId == OfferStatusId.CREATED &&
                x.SalesManagerId == data.SalesManagerId &&
                x.ContactEmail == data.ContactEmail &&
                x.ContactNumber == data.ContactNumber &&
                x.MarketingUrl == data.ProviderUri
            );
    }

    [Fact]
    public async Task AddAppAsync_WithNullSalesMangerValidData_ReturnsExpected()
    {
        // Arrange
        var data = new AppRequestModel(
            "Titlebfd7fa50-cb36-4940-96f7-0442bbc41b70",
            null,
            Enumerable.Repeat(Guid.NewGuid(), 1),
            Enumerable.Repeat(new LocalizedDescription("en", "ProviderXyz", "XYZ"), 1),
            Enumerable.Repeat("en", 1),
            "Price90b6fed6-799a-4238-bee2-5cd84bc3f167",
            Enumerable.Repeat(PrivacyPolicyId.COMPANY_DATA, 1),
            "ProviderUri5ef769cd-9be3-4d3f-8a3e-c886793a732b",
            "ContactNumber13081a1d-24fc-4f32-8163-f77a8a7fb724",
            "ContactEmailb6e3cdcc-7d08-4dd5-9e3c-c9564129558a"
        );

        Offer? created = null;
        var offerId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        A.CallTo(() => _offerRepository.CreateOffer(A<OfferTypeId>._, A<Guid>._, A<Action<Offer>?>._))
            .ReturnsLazily((OfferTypeId offerTypeId, Guid providerCompanyId, Action<Offer>? modify) =>
            {
                created = new Offer(offerId, providerCompanyId, now, offerTypeId);
                modify?.Invoke(created);
                return created;
            });

        // Act
        await _sut.AddAppAsync(data);

        // Assert
        A.CallTo(() => _offerService.ValidateSalesManager(A<Guid>._, A<IEnumerable<UserRoleConfig>>._)).MustNotHaveHappened();
        A.CallTo(() => _offerRepository.CreateOffer(A<OfferTypeId>._, A<Guid>._, A<Action<Offer>?>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.AddOfferDescriptions(A<IEnumerable<(Guid appId, string languageShortName, string descriptionLong, string descriptionShort)>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.AddAppLanguages(A<IEnumerable<(Guid appId, string languageShortName)>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.AddAppAssignedUseCases(A<IEnumerable<(Guid appId, Guid useCaseId)>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.CreateOfferLicenses(A<string>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.CreateOfferAssignedLicense(A<Guid>._, A<Guid>._))
            .MustHaveHappenedOnceExactly();

        created.Should().NotBeNull()
            .And.Match<Offer>(x =>
                x.Id == offerId &&
                x.Name == data.Title &&
                x.ProviderCompanyId == _identity.CompanyId &&
                x.OfferStatusId == OfferStatusId.CREATED &&
                x.SalesManagerId == data.SalesManagerId &&
                x.ContactEmail == data.ContactEmail &&
                x.ContactNumber == data.ContactNumber &&
                x.MarketingUrl == data.ProviderUri
            );
    }

    #endregion

    #region UpdateAppReleaseAsync

    [Fact]
    public async Task UpdateAppReleaseAsync_WithoutApp_ThrowsException()
    {
        // Arrange
        SetupUpdateApp();
        var data = _fixture.Create<AppRequestModel>();

        // Act
        async Task Act() => await _sut.UpdateAppReleaseAsync(_notExistingAppId, data);

        // Assert
        var error = await Assert.ThrowsAsync<NotFoundException>(Act);
        error.Message.Should().Be(AppReleaseErrors.APP_NOT_EXIST.ToString());
    }

    [Fact]
    public async Task UpdateAppReleaseAsync_WithActiveApp_ThrowsException()
    {
        // Arrange
        SetupUpdateApp();
        var data = _fixture.Create<AppRequestModel>();

        // Act
        async Task Act() => await _sut.UpdateAppReleaseAsync(_activeAppId, data);

        // Assert
        var error = await Assert.ThrowsAsync<ConflictException>(Act);
        error.Message.Should().Be(AppReleaseErrors.APP_CONFLICT_APP_STATE_CANNOT_UPDATED.ToString());
    }

    [Fact]
    public async Task UpdateAppReleaseAsync_WithInvalidUser_ThrowsException()
    {
        // Arrange
        SetupUpdateApp();
        var data = _fixture.Create<AppRequestModel>();

        // Act
        async Task Act() => await _sut.UpdateAppReleaseAsync(_differentCompanyAppId, data);

        // Assert
        var error = await Assert.ThrowsAsync<ForbiddenException>(Act);
        error.Message.Should().Be(AppReleaseErrors.APP_FORBIDDEN_COMPANY_NOT_APP_PROVIDER.ToString());
    }

    [Fact]
    public async Task UpdateAppReleaseAsync_WithInvalidLanguage_ThrowsException()
    {
        // Arrange
        SetupUpdateApp();
        var data = new AppRequestModel(
        "Titlebfd7fa50-cb36-4940-96f7-0442bbc41b70",
        _companyUser.Id,
        Enumerable.Repeat(Guid.NewGuid(), 1),
        Enumerable.Repeat(new LocalizedDescription("en", "ProviderXyz", "XYZ"), 1),
        new[] { "de", "en", "invalid" },
        "Price90b6fed6-799a-4238-bee2-5cd84bc3f167",
        Enumerable.Repeat(PrivacyPolicyId.COMPANY_DATA, 1),
        "ProviderUri5ef769cd-9be3-4d3f-8a3e-c886793a732b",
        "ContactNumber13081a1d-24fc-4f32-8163-f77a8a7fb724",
        "ContactEmailb6e3cdcc-7d08-4dd5-9e3c-c9564129558a"
    );

        // Act
        async Task Act() => await _sut.UpdateAppReleaseAsync(_existingAppId, data);

        // Assert
        var error = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        error.Parameters.First().Name.Should().Be("SupportedLanguageCodes");
    }

    [Fact]
    public async Task UpdateAppReleaseAsync_WithValidData_ReturnsExpected()
    {
        // Arrange
        SetupUpdateApp();
        var data = new AppRequestModel(
        "Titlebfd7fa50-cb36-4940-96f7-0442bbc41b70",
        _companyUser.Id,
        Enumerable.Repeat(Guid.NewGuid(), 1),
        Enumerable.Repeat(new LocalizedDescription("en", "ProviderXyz", "XYZ"), 1),
        new[] { "de", "en" },
        "Price90b6fed6-799a-4238-bee2-5cd84bc3f167",
        Enumerable.Repeat(PrivacyPolicyId.COMPANY_DATA, 1),
        "ProviderUri5ef769cd-9be3-4d3f-8a3e-c886793a732b",
        "ContactNumber13081a1d-24fc-4f32-8163-f77a8a7fb724",
        "ContactEmailb6e3cdcc-7d08-4dd5-9e3c-c9564129558a"
    );

        Offer? initial = null;
        Offer? modified = null;
        A.CallTo(() => _offerRepository.AttachAndModifyOffer(A<Guid>._, A<Action<Offer>>._, A<Action<Offer>>._))
            .Invokes((Guid offerId, Action<Offer> modify, Action<Offer>? initialize) =>
            {
                initial = new Offer(offerId, Guid.Empty, default, default);
                modified = new Offer(offerId, Guid.Empty, default, default);
                initialize?.Invoke(initial);
                modify.Invoke(modified);
            });

        // Act
        await _sut.UpdateAppReleaseAsync(_existingAppId, data);

        // Assert
        A.CallTo(() => _offerRepository.AttachAndModifyOffer(A<Guid>._, A<Action<Offer>>._, A<Action<Offer>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerService.UpsertRemoveOfferDescription(_existingAppId, A<IEnumerable<LocalizedDescription>>.That.IsSameSequenceAs(data.Descriptions), A<IEnumerable<LocalizedDescription>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.AddAppLanguages(A<IEnumerable<(Guid appId, string languageShortName)>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.RemoveAppLanguages(A<IEnumerable<(Guid, string)>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.CreateDeleteAppAssignedUseCases(_existingAppId, A<IEnumerable<Guid>>.That.Matches(x => x.SequenceEqual(_useCases)), A<IEnumerable<Guid>>.That.Matches(x => x.SequenceEqual(data.UseCaseIds))))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerService.CreateOrUpdateOfferLicense(_existingAppId, A<string>._, A<(Guid OfferLicenseId, string LicenseText, bool AssingedToMultipleOffers)>._)).MustHaveHappenedOnceExactly();

        initial.Should().NotBeNull()
            .And.Match<Offer>(
                x => x.Name == _appUpdateData.Name &&
                x.SalesManagerId == _appUpdateData.SalesManagerId &&
                x.ContactEmail == _appUpdateData.ContactEmail &&
                x.ContactNumber == _appUpdateData.ContactNumber &&
                x.MarketingUrl == _appUpdateData.MarketingUrl
            );

        modified.Should().NotBeNull()
            .And.Match<Offer>(
                x => x.Name == data.Title &&
                x.SalesManagerId == data.SalesManagerId &&
                x.ContactEmail == data.ContactEmail &&
                x.ContactNumber == data.ContactNumber &&
                x.MarketingUrl == data.ProviderUri
            );
    }

    #endregion

    #region Create App Document

    [Fact]
    public async Task CreateAppDocumentAsync_ExecutesSuccessfully()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var file = FormFileHelper.GetFormFile("this is just a test", "superFile.pdf", "application/pdf");

        _settings.UploadAppDocumentTypeIds = new[] {
            new UploadDocumentConfig(DocumentTypeId.ADDITIONAL_DETAILS, new []{ MediaTypeId.PDF })
        };

        // Act
        await _sut.CreateAppDocumentAsync(appId, DocumentTypeId.APP_CONTRACT, file, CancellationToken.None);

        // Assert
        A.CallTo(() => _offerDocumentService.UploadDocumentAsync(appId, DocumentTypeId.APP_CONTRACT, file, OfferTypeId.APP, _settings.UploadAppDocumentTypeIds, OfferStatusId.CREATED, CancellationToken.None)).MustHaveHappenedOnceExactly();
    }

    #endregion

    #region SubmitAppReleaseRequestAsync

    [Fact]
    public async Task SubmitAppReleaseRequestAsync_CallsOfferService()
    {
        // Act
        await _sut.SubmitAppReleaseRequestAsync(_existingAppId);

        // Assert
        A.CallTo(() =>
                _offerService.SubmitOfferAsync(
                    _existingAppId,
                    OfferTypeId.APP,
                    A<IEnumerable<NotificationTypeId>>._,
                    A<IEnumerable<UserRoleConfig>>._, A<IEnumerable<DocumentTypeId>>._))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region SubmitOfferConsentAsync

    [Fact]
    public async Task SubmitOfferConsentAsync_WithEmptyAppId_ThrowsControllerArgumentException()
    {
        // Act
        async Task Act() => await _sut.SubmitOfferConsentAsync(Guid.Empty, _fixture.Create<OfferAgreementConsent>());

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be(AppReleaseErrors.APP_ARG_APP_ID_NOT_EMPTY.ToString());
    }

    [Fact]
    public async Task SubmitOfferConsentAsync_WithAppId_CallsOfferService()
    {
        // Arrange
        var data = _fixture.Create<OfferAgreementConsent>();

        // Act
        await _sut.SubmitOfferConsentAsync(_existingAppId, data);

        // Assert
        A.CallTo(() => _offerService.CreateOrUpdateProviderOfferAgreementConsent(_existingAppId, data, OfferTypeId.APP)).MustHaveHappenedOnceExactly();
    }

    #endregion

    #region GetAppDetailsForStatusAsync

    [Fact]
    public async Task GetAppDetailsForStatusAsync_ReturnsExpected()
    {
        // Arrange
        var appId = Guid.NewGuid();
        var data = _fixture.Create<OfferProviderResponse>();
        A.CallTo(() => _offerService.GetProviderOfferDetailsForStatusAsync(A<Guid>._, A<OfferTypeId>._, A<DocumentTypeId>._, Constants.DefaultLanguage))
            .Returns(data);

        // Act
        var result = await _sut.GetAppDetailsForStatusAsync(appId, Constants.DefaultLanguage);

        // Assert
        A.CallTo(() => _offerService.GetProviderOfferDetailsForStatusAsync(appId, OfferTypeId.APP, DocumentTypeId.APP_LEADIMAGE, Constants.DefaultLanguage))
            .MustHaveHappenedOnceExactly();

        result.Title.Should().Be(data.Title);
        result.Provider.Should().Be(data.Provider);
        result.LeadPictureId.Should().Be(data.LeadPictureId);
        result.ProviderName.Should().Be(data.ProviderName);
        result.UseCase.Should().HaveSameCount(data.UseCase).And.ContainInOrder(data.UseCase);
        result.Descriptions.Should().HaveSameCount(data.Descriptions).And.ContainInOrder(data.Descriptions);
        result.Agreements.Should().HaveSameCount(data.Agreements).And.ContainInOrder(data.Agreements);
        result.SupportedLanguageCodes.Should().HaveSameCount(data.SupportedLanguageCodes).And.ContainInOrder(data.SupportedLanguageCodes);
        result.Price.Should().Be(data.Price);
        result.Images.Should().HaveSameCount(data.Images).And.ContainInOrder(data.Images);
        result.ProviderUri.Should().Be(data.ProviderUri);
        result.ContactEmail.Should().Be(data.ContactEmail);
        result.Documents.Should().HaveSameCount(data.Documents).And.ContainInOrder(data.Documents);
        result.SalesManagerId.Should().Be(data.SalesManagerId);
        result.PrivacyPolicies.Should().HaveSameCount(data.PrivacyPolicies).And.ContainInOrder(data.PrivacyPolicies);
        result.TechnicalUserProfile.Should().HaveSameCount(data.TechnicalUserProfile).And.ContainInOrder(data.TechnicalUserProfile);
    }

    [Fact]
    public async Task GetAppDetailsForStatusAsync_NullUseCase_ThrowsUnexpectedConditionException()
    {
        // Arrange
        var appId = Guid.NewGuid();
        var data = _fixture.Build<OfferProviderResponse>()
                        .With(x => x.UseCase, default(IEnumerable<AppUseCaseData>?))
                        .Create();

        A.CallTo(() => _offerService.GetProviderOfferDetailsForStatusAsync(A<Guid>._, A<OfferTypeId>._, A<DocumentTypeId>._, Constants.DefaultLanguage))
            .Returns(data);

        var Act = () => _sut.GetAppDetailsForStatusAsync(appId, Constants.DefaultLanguage);

        // Act
        var result = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);

        // Assert
        A.CallTo(() => _offerService.GetProviderOfferDetailsForStatusAsync(appId, OfferTypeId.APP, DocumentTypeId.APP_LEADIMAGE, Constants.DefaultLanguage))
            .MustHaveHappenedOnceExactly();

        result.Message.Should().Be(AppReleaseErrors.APP_UNEXPECTED_USECASE_NOT_NULL.ToString());
    }

    #endregion

    #region GetAllInReviewStatusApps

    [Fact]
    public async Task GetAllInReviewStatusAppsAsync_DefaultRequest()
    {
        // Arrange
        var offerStatus = new[] { OfferStatusId.ACTIVE, OfferStatusId.IN_REVIEW };
        var inReviewData = new[] {
            new InReviewAppData(Guid.NewGuid(),null,null!, OfferStatusId.IN_REVIEW),
            new InReviewAppData(Guid.NewGuid(),null,null!, OfferStatusId.IN_REVIEW),
            new InReviewAppData(Guid.NewGuid(),null,null!, OfferStatusId.ACTIVE),
            new InReviewAppData(Guid.NewGuid(),null,null!, OfferStatusId.ACTIVE),
            new InReviewAppData(Guid.NewGuid(),null,null!, OfferStatusId.ACTIVE)
        };
        var paginationResult = (int skip, int take) => Task.FromResult(new Pagination.Source<InReviewAppData>(5, inReviewData.Skip(skip).Take(take)));
        A.CallTo(() => _offerRepository.GetAllInReviewStatusAppsAsync(A<IEnumerable<OfferStatusId>>._, A<OfferSorting>._))
            .Returns(paginationResult);

        // Act
        var result = await _sut.GetAllInReviewStatusAppsAsync(0, 5, OfferSorting.DateAsc, null);

        // Assert
        A.CallTo(() => _offerRepository.GetAllInReviewStatusAppsAsync(A<IEnumerable<OfferStatusId>>
            .That.Matches(x => x.Count() == 2 && x.All(y => offerStatus.Contains(y))), A<OfferSorting>._)).MustHaveHappenedOnceExactly();
        Assert.IsType<Pagination.Response<InReviewAppData>>(result);
        result.Content.Should().HaveCount(5);
        result.Content.Should().Contain(x => x.Status == OfferStatusId.ACTIVE);
        result.Content.Should().Contain(x => x.Status == OfferStatusId.IN_REVIEW);
    }

    [Fact]
    public async Task GetAllInReviewStatusAppsAsync_InReviewRequest()
    {
        // Arrange
        var offerStatus = new[] { OfferStatusId.IN_REVIEW };
        var inReviewData = new[]{
            new InReviewAppData(Guid.NewGuid(),null,null!, OfferStatusId.IN_REVIEW),
            new InReviewAppData(Guid.NewGuid(),null,null!, OfferStatusId.IN_REVIEW),
            new InReviewAppData(Guid.NewGuid(),null,null!, OfferStatusId.IN_REVIEW),
            new InReviewAppData(Guid.NewGuid(),null,null!, OfferStatusId.IN_REVIEW),
            new InReviewAppData(Guid.NewGuid(),null,null!, OfferStatusId.IN_REVIEW)
        };
        var paginationResult = (int skip, int take) => Task.FromResult(new Pagination.Source<InReviewAppData>(5, inReviewData.Skip(skip).Take(take)));
        A.CallTo(() => _offerRepository.GetAllInReviewStatusAppsAsync(A<IEnumerable<OfferStatusId>>._, A<OfferSorting>._))
            .Returns(paginationResult);

        // Act
        var result = await _sut.GetAllInReviewStatusAppsAsync(0, 5, OfferSorting.DateAsc, OfferStatusIdFilter.InReview);

        // Assert
        A.CallTo(() => _offerRepository.GetAllInReviewStatusAppsAsync(A<IEnumerable<OfferStatusId>>
            .That.Matches(x => x.Count() == 1 && x.All(y => offerStatus.Contains(y))), A<OfferSorting>._)).MustHaveHappenedOnceExactly();
        Assert.IsType<Pagination.Response<InReviewAppData>>(result);
        result.Content.Should().HaveCount(5);
        result.Content.Should().NotContain(x => x.Status == OfferStatusId.ACTIVE);
        result.Content.Should().Contain(x => x.Status == OfferStatusId.IN_REVIEW);
    }

    #endregion

    #region DeclineAppRequest

    [Fact]
    public async Task DeclineAppRequestAsync_CallsExpected()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var data = new OfferDeclineRequest("Just a test");

        // Act
        await _sut.DeclineAppRequestAsync(appId, data);

        // Assert
        A.CallTo(() => _offerService.DeclineOfferAsync(appId, data,
            OfferTypeId.APP, NotificationTypeId.APP_RELEASE_REJECTION,
            A<IEnumerable<UserRoleConfig>>._,
            A<string>._,
            A<IEnumerable<NotificationTypeId>>._,
            A<IEnumerable<UserRoleConfig>>._)).MustHaveHappenedOnceExactly();
    }

    #endregion

    #region  DeleteAppDocument

    [Fact]
    public async Task DeleteAppDocumentsAsync_ReturnsExpected()
    {
        // Arrange
        var documentId = _fixture.Create<Guid>();

        // Act
        await _sut.DeleteAppDocumentsAsync(documentId);

        // Assert
        A.CallTo(() => _offerService.DeleteDocumentsAsync(documentId, A<IEnumerable<DocumentTypeId>>._, OfferTypeId.APP)).MustHaveHappenedOnceExactly();

    }

    #endregion

    #region DeleteApp

    [Fact]
    public async Task DeleteAppAsync_ReturnsExpectedResult()
    {
        //Arrange
        var appId = Guid.NewGuid();
        var appDeleteData = _fixture.Create<AppDeleteData>();

        A.CallTo(() => _offerRepository.GetAppDeleteDataAsync(appId, OfferTypeId.APP, _identity.CompanyId, OfferStatusId.CREATED))
            .Returns((true, true, true, true, appDeleteData));

        //Act
        await _sut.DeleteAppAsync(appId);

        // Assert 
        A.CallTo(() => _offerRepository.GetAppDeleteDataAsync(appId, OfferTypeId.APP, _identity.CompanyId, OfferStatusId.CREATED))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.RemoveOfferAssignedLicenses(A<IEnumerable<(Guid, Guid)>>.That.Matches((IEnumerable<(Guid OfferId, Guid LicenceId)> offerLicenseIds) =>
            offerLicenseIds.All(x => x.OfferId == appId) && offerLicenseIds.Select(x => x.LicenceId).SequenceEqual(appDeleteData.OfferLicenseIds)))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.RemoveOfferAssignedUseCases(A<IEnumerable<(Guid, Guid)>>.That.Matches((IEnumerable<(Guid OfferId, Guid UseCaseId)> offerUseCaseIds) =>
            offerUseCaseIds.All(x => x.OfferId == appId) && offerUseCaseIds.Select(x => x.UseCaseId).SequenceEqual(appDeleteData.UseCaseIds)))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.RemoveOfferAssignedPrivacyPolicies(A<IEnumerable<(Guid, PrivacyPolicyId)>>.That.Matches((IEnumerable<(Guid OfferId, PrivacyPolicyId PolicyId)> offerPolicyIds) =>
            offerPolicyIds.All(x => x.OfferId == appId) && offerPolicyIds.Select(x => x.PolicyId).SequenceEqual(appDeleteData.PolicyIds)))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.RemoveOfferAssignedDocuments(A<IEnumerable<(Guid, Guid)>>.That.Matches((IEnumerable<(Guid OfferId, Guid DocumentId)> offerDocIds) =>
            offerDocIds.All(x => x.OfferId == appId) && offerDocIds.Select(x => x.DocumentId).SequenceEqual(appDeleteData.DocumentIdStatus.Select(x => x.DocumentId))))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.RemoveAppLanguages(A<IEnumerable<(Guid, string)>>.That.Matches((IEnumerable<(Guid OfferId, string Language)> offerLanguages) =>
            offerLanguages.All(x => x.OfferId == appId) && offerLanguages.Select(x => x.Language).SequenceEqual(appDeleteData.LanguageCodes)))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.RemoveOfferTags(A<IEnumerable<(Guid, string)>>.That.Matches((IEnumerable<(Guid OfferId, string Tag)> offerTags) =>
            offerTags.All(x => x.OfferId == appId) && offerTags.Select(x => x.Tag).SequenceEqual(appDeleteData.TagNames)))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.RemoveOfferDescriptions(A<IEnumerable<(Guid, string)>>.That.Matches((IEnumerable<(Guid OfferId, string Langugage)> descriptionLanguages) =>
            descriptionLanguages.All(x => x.OfferId == appId) && descriptionLanguages.Select(x => x.Langugage).SequenceEqual(appDeleteData.DescriptionLanguageShortNames)))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _documentRepository.RemoveDocuments(A<IEnumerable<Guid>>.That.IsSameSequenceAs(appDeleteData.DocumentIdStatus.Where(x => x.DocumentStatusId != DocumentStatusId.LOCKED).Select(x => x.DocumentId)))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.RemoveOffer(appId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DeleteAppAsync_WithNoProviderCompanyUser_ThrowsForbiddenException()
    {
        //Arrange
        var appId = Guid.NewGuid();
        var appDeleteData = _fixture.Create<AppDeleteData>();

        A.CallTo(() => _offerRepository.GetAppDeleteDataAsync(appId, OfferTypeId.APP, _identity.CompanyId, OfferStatusId.CREATED))
            .Returns((true, true, true, false, appDeleteData));

        //Act
        async Task Act() => await _sut.DeleteAppAsync(appId);

        // Assert 
        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be(AppReleaseErrors.APP_FORBIDDEN_COM_NOT_PROVIDER_COM_APP.ToString());
    }

    [Fact]
    public async Task DeleteAppAsync_WithInvalidOfferStatus_ThrowsConflictException()
    {
        //Arrange
        var appId = Guid.NewGuid();
        var appDeleteData = _fixture.Create<AppDeleteData>();

        A.CallTo(() => _offerRepository.GetAppDeleteDataAsync(appId, OfferTypeId.APP, _identity.CompanyId, OfferStatusId.CREATED))
            .Returns((true, true, false, true, appDeleteData));

        //Act
        async Task Act() => await _sut.DeleteAppAsync(appId);

        // Assert 
        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be(AppReleaseErrors.APP_CONFLICT_APP_NOT_CREATED_STATE.ToString());
    }

    #endregion

    #region ApproveAppRequestAsync

    [Fact]
    public async Task ApproveAppRequestAsync_CallsExpected()
    {
        // Arrange
        var offerId = Guid.NewGuid();

        // Act
        await _sut.ApproveAppRequestAsync(offerId);

        A.CallTo(() => _offerService.ApproveOfferRequestAsync(offerId, OfferTypeId.APP,
                A<IEnumerable<NotificationTypeId>>._,
                A<IEnumerable<UserRoleConfig>>._,
                A<IEnumerable<NotificationTypeId>>._,
                A<IEnumerable<UserRoleConfig>>._,
                A<(string, string)>.That.Matches(x => x.Item1 == _settings.OfferSubscriptionAddress && x.Item2 == _settings.OfferDetailAddress),
                A<IEnumerable<UserRoleConfig>>._))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region SetInstanceType

    [Fact]
    public async Task SetInstanceType_WithSingleInstanceWithoutUrl_ThrowsControllerArgumentException()
    {
        //Arrange
        var appId = Guid.NewGuid();
        var data = new AppInstanceSetupData(true, null);

        //Act
        async Task Act() => await _sut.SetInstanceType(appId, data);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be("InstanceUrl must be set (Parameter 'InstanceUrl')");
    }

    [Fact]
    public async Task SetInstanceType_WithMultiInstanceWithUrl_ThrowsControllerArgumentException()
    {
        //Arrange
        var appId = Guid.NewGuid();
        var data = new AppInstanceSetupData(false, "https://test.de");

        //Act
        async Task Act() => await _sut.SetInstanceType(appId, data);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be(AppReleaseErrors.APP_ARG_MULTI_INSTANCE_APP_URL_SET.ToString());
    }

    [Fact]
    public async Task SetInstanceType_WithNotExistingApp_NotFoundException()
    {
        //Arrange
        var appId = Guid.NewGuid();
        var data = new AppInstanceSetupData(true, "https://test.de");
        A.CallTo(() => _offerRepository.GetOfferWithSetupDataById(appId, _identity.CompanyId, OfferTypeId.APP))
            .Returns<(OfferStatusId, bool, AppInstanceSetupTransferData?, IEnumerable<(Guid, Guid, string)>)>(default);

        //Act
        async Task Act() => await _sut.SetInstanceType(appId, data);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be(AppReleaseErrors.APP_NOT_EXIST.ToString());
    }

    [Fact]
    public async Task SetInstanceType_WithInvalidUser_ThrowsForbiddenException()
    {
        //Arrange
        var appId = Guid.NewGuid();
        var data = new AppInstanceSetupData(true, "https://test.de");
        A.CallTo(() => _offerRepository.GetOfferWithSetupDataById(appId, _identity.CompanyId, OfferTypeId.APP))
            .Returns((OfferStatusId.ACTIVE, false, null, Enumerable.Empty<(Guid, Guid, string)>()));

        //Act
        async Task Act() => await _sut.SetInstanceType(appId, data);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be(AppReleaseErrors.APP_FORBIDDEN_COMP_ID_NOT_PROVIDER_COMPANY.ToString());
    }

    [Fact]
    public async Task SetInstanceType_WithWrongOfferState_ThrowsConflictException()
    {
        //Arrange
        var appId = Guid.NewGuid();
        var data = new AppInstanceSetupData(true, "https://test.de");
        A.CallTo(() => _offerRepository.GetOfferWithSetupDataById(appId, _identity.CompanyId, OfferTypeId.APP))
            .Returns((OfferStatusId.ACTIVE, true, null, Enumerable.Empty<(Guid, Guid, string)>()));

        //Act
        async Task Act() => await _sut.SetInstanceType(appId, data);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be(AppReleaseErrors.APP_CONFLICT_NOT_IN_CREATED_STATE.ToString());
    }

    [Fact]
    public async Task SetInstanceType_FromSingleToMultiWithoutAppInstance_ThrowsConflictException()
    {
        //Arrange
        var appId = Guid.NewGuid();
        var instanceSetupId = Guid.NewGuid();
        var data = new AppInstanceSetupData(false, null);
        var instanceSetupTransferData = new AppInstanceSetupTransferData(instanceSetupId, true, null);
        A.CallTo(() => _offerRepository.GetOfferWithSetupDataById(appId, _identity.CompanyId, OfferTypeId.APP))
            .Returns((OfferStatusId.CREATED, true, instanceSetupTransferData, Enumerable.Empty<(Guid, Guid, string)>()));

        //Act
        async Task Act() => await _sut.SetInstanceType(appId, data);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be(AppReleaseErrors.APP_CONFLICT_ONLY_ONE_APP_INSTANCE_ALLOWED.ToString());
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
    }

    [Fact]
    public async Task SetInstanceType_WithNewEntry_CreatesEntry()
    {
        //Arrange
        var appId = Guid.NewGuid();
        var instanceSetupId = Guid.NewGuid();
        var data = new AppInstanceSetupData(true, "https://test.de");
        AppInstanceSetup? instanceSetupData = null;
        A.CallTo(() => _offerRepository.GetOfferWithSetupDataById(appId, _identity.CompanyId, OfferTypeId.APP))
            .Returns((OfferStatusId.CREATED, true, null, Enumerable.Empty<(Guid, Guid, string)>()));
        A.CallTo(() => _offerRepository.CreateAppInstanceSetup(appId, A<Action<AppInstanceSetup>>._))
            .Invokes((Guid callingAppId, Action<AppInstanceSetup> setOptionalParameters) =>
            {
                instanceSetupData = new AppInstanceSetup(instanceSetupId, callingAppId);
                setOptionalParameters.Invoke(instanceSetupData);
            });

        //Act
        await _sut.SetInstanceType(appId, data);

        // Assert
        instanceSetupData.Should().NotBeNull();
        instanceSetupData!.IsSingleInstance.Should().BeTrue();
        instanceSetupData.InstanceUrl.Should().Be("https://test.de");
        A.CallTo(() => _offerSetupService.SetupSingleInstance(appId, "https://test.de")).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task SetInstanceType_WithUrlUpdate_CreatesEntry()
    {
        //Arrange
        var appId = Guid.NewGuid();
        var instanceSetupId = Guid.NewGuid();
        var internalClientId = Guid.NewGuid().ToString();
        var appInstanceData = new List<(Guid, Guid, string)>
        {
            (Guid.NewGuid(), Guid.NewGuid(), internalClientId)
        };
        var data = new AppInstanceSetupData(true, "https://new-url.de");
        var instanceSetupTransferData = new AppInstanceSetupTransferData(instanceSetupId, true, "https://test.de");
        var instanceSetupData = new AppInstanceSetup(instanceSetupId, appId) { IsSingleInstance = true };
        A.CallTo(() => _offerRepository.GetOfferWithSetupDataById(appId, _identity.CompanyId, OfferTypeId.APP))
            .Returns((OfferStatusId.CREATED, true, instanceSetupTransferData, appInstanceData));
        A.CallTo(() => _offerRepository.AttachAndModifyAppInstanceSetup(instanceSetupId, appId, A<Action<AppInstanceSetup>>._, A<Action<AppInstanceSetup>>._))
            .Invokes((Guid _, Guid _, Action<AppInstanceSetup> setOptionalParameters,
                Action<AppInstanceSetup>? initializeParameter) =>
            {
                initializeParameter?.Invoke(instanceSetupData);
                setOptionalParameters.Invoke(instanceSetupData);
            });

        //Act
        await _sut.SetInstanceType(appId, data);

        // Assert
        instanceSetupData.InstanceUrl.Should().Be("https://new-url.de");
        A.CallTo(() => _offerSetupService.UpdateSingleInstance(internalClientId, "https://new-url.de")).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerSetupService.SetupSingleInstance(A<Guid>._, A<string>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task SetInstanceType_WithExistingEntryButNoAppInstance_ThrowsConflictException()
    {
        //Arrange
        var appId = Guid.NewGuid();
        var instanceSetupId = Guid.NewGuid();
        var data = new AppInstanceSetupData(true, "https://test.de");
        var instanceSetupTransferData = new AppInstanceSetupTransferData(instanceSetupId, false, null);
        var instanceSetupData = new AppInstanceSetup(instanceSetupId, appId) { IsSingleInstance = false };
        A.CallTo(() => _offerRepository.GetOfferWithSetupDataById(appId, _identity.CompanyId, OfferTypeId.APP))
            .Returns((OfferStatusId.CREATED, true, instanceSetupTransferData, Enumerable.Empty<(Guid, Guid, string)>()));
        A.CallTo(() => _offerRepository.AttachAndModifyAppInstanceSetup(instanceSetupId, appId, A<Action<AppInstanceSetup>>._, A<Action<AppInstanceSetup>>._))
            .Invokes((Guid _, Guid _, Action<AppInstanceSetup> setOptionalParameters,
                Action<AppInstanceSetup>? initializeParameter) =>
            {
                initializeParameter?.Invoke(instanceSetupData);
                setOptionalParameters.Invoke(instanceSetupData);
            });

        //Act
        await _sut.SetInstanceType(appId, data);

        // Assert
        instanceSetupData.InstanceUrl.Should().Be(data.InstanceUrl);
        instanceSetupData.IsSingleInstance.Should().BeTrue();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerSetupService.SetupSingleInstance(appId, data.InstanceUrl!)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task SetInstanceType_WithExistingEntry_UpdatesEntry()
    {
        //Arrange
        var appId = Guid.NewGuid();
        var instanceSetupId = Guid.NewGuid();
        var appInstanceData = new List<(Guid, Guid, string)>
        {
            (Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid().ToString())
        };
        var instanceSetupData = new AppInstanceSetup(instanceSetupId, appId) { IsSingleInstance = false };
        var data = new AppInstanceSetupData(true, "https://test.de");
        var instanceSetupTransferData = new AppInstanceSetupTransferData(instanceSetupId, false, null);
        A.CallTo(() => _offerRepository.GetOfferWithSetupDataById(appId, _identity.CompanyId, OfferTypeId.APP))
            .Returns((OfferStatusId.CREATED, true, instanceSetupTransferData, appInstanceData));
        A.CallTo(() => _offerRepository.AttachAndModifyAppInstanceSetup(instanceSetupId, appId, A<Action<AppInstanceSetup>>._, A<Action<AppInstanceSetup>>._))
            .Invokes((Guid _, Guid _, Action<AppInstanceSetup> setOptionalParameters,
                Action<AppInstanceSetup>? initializeParameter) =>
            {
                initializeParameter?.Invoke(instanceSetupData);
                setOptionalParameters.Invoke(instanceSetupData);
            });

        //Act
        await _sut.SetInstanceType(appId, data);

        // Assert
        instanceSetupData.InstanceUrl.Should().Be(data.InstanceUrl);
        instanceSetupData.IsSingleInstance.Should().BeTrue();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerSetupService.SetupSingleInstance(appId, data.InstanceUrl!)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task SetInstanceType_FromSingleToMulti_UpdatesEntry()
    {
        //Arrange
        var appId = Guid.NewGuid();
        var instanceSetupId = Guid.NewGuid();
        var appInstanceData = new List<(Guid, Guid, string)>
        {
            (Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid().ToString())
        };
        var instanceSetupData = new AppInstanceSetup(instanceSetupId, appId) { IsSingleInstance = true };
        var data = new AppInstanceSetupData(false, null);
        var instanceSetupTransferData = new AppInstanceSetupTransferData(instanceSetupId, true, null);
        A.CallTo(() => _offerRepository.GetOfferWithSetupDataById(appId, _identity.CompanyId, OfferTypeId.APP))
            .Returns((OfferStatusId.CREATED, true, instanceSetupTransferData, appInstanceData));
        A.CallTo(() => _offerRepository.AttachAndModifyAppInstanceSetup(instanceSetupId, appId, A<Action<AppInstanceSetup>>._, A<Action<AppInstanceSetup>>._))
            .Invokes((Guid _, Guid _, Action<AppInstanceSetup> setOptionalParameters,
                Action<AppInstanceSetup>? initializeParameter) =>
            {
                initializeParameter?.Invoke(instanceSetupData);
                setOptionalParameters.Invoke(instanceSetupData);
            });

        //Act
        await _sut.SetInstanceType(appId, data);

        // Assert
        instanceSetupData.InstanceUrl.Should().BeNull();
        instanceSetupData.IsSingleInstance.Should().BeFalse();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerSetupService.DeleteSingleInstance(A<Guid>._, A<Guid>._, A<string>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerSetupService.SetupSingleInstance(appId, data.InstanceUrl!)).MustNotHaveHappened();
    }

    #endregion

    #region  GetInReviewAppDetailsById

    [Fact]
    public async Task GetInReviewAppDetailsByIdAsync_ReturnsExpected()
    {
        // Arrange
        var data = _fixture.Create<InReviewOfferData>();
        var appId = _fixture.Create<Guid>();

        A.CallTo(() => _offerRepository.GetInReviewAppDataByIdAsync(appId, OfferTypeId.APP))
            .Returns(data);

        // Act
        var result = await _sut.GetInReviewAppDetailsByIdAsync(appId);

        // Assert
        result.Should().NotBeNull();
        result.ContactNumber.Should().Be(data.ContactNumber);
        result.LicenseType.Should().Be(data.LicenseTypeId);
        result.LeadPictureId.Should().Be(data.leadPictureId);
        result.Provider.Should().Be(data.Provider);
        result.ProviderUri.Should().Be(data.ProviderUri);
        result.OfferStatusId.Should().Be(data.OfferStatusId);
        result.TechnicalUserProfile.Should().HaveSameCount(data.TechnicalUserProfile).And.AllSatisfy(
            x => data.TechnicalUserProfile.Should().ContainSingle(d => d.TechnicalUserProfileId == x.Key).Which.UserRoles.Should().ContainInOrder(x.Value)
        );
    }

    [Fact]
    public async Task GetInReviewAppDetailsByIdAsync_ThrowsNotFoundException()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();

        A.CallTo(() => _offerRepository.GetInReviewAppDataByIdAsync(appId, OfferTypeId.APP))
            .Returns<InReviewOfferData?>(default);

        //Act
        async Task Act() => await _sut.GetInReviewAppDetailsByIdAsync(appId);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be(AppReleaseErrors.APP_NOT_FOUND_OR_INCORRECT_STATUS.ToString());
    }

    #endregion

    #region UpdateTechnicalUserProfiles

    [Fact]
    public async Task UpdateTechnicalUserProfiles_ReturnsExpected()
    {
        // Arrange
        const string clientProfile = "cl";
        var appId = Guid.NewGuid();
        var data = _fixture.CreateMany<TechnicalUserProfileData>(5);
        var sut = new AppReleaseBusinessLogic(null!, Options.Create(new AppsSettings { TechnicalUserProfileClient = clientProfile }), _offerService, _offerDocumentService, null!, _identityService);

        // Act
        await sut
            .UpdateTechnicalUserProfiles(appId, data);

        A.CallTo(() => _offerService.UpdateTechnicalUserProfiles(appId, OfferTypeId.APP,
                A<IEnumerable<TechnicalUserProfileData>>.That.Matches(x => x.Count() == 5), clientProfile, A<IEnumerable<UserRoleConfig>>._))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region GetTechnicalUserProfilesForOffer

    [Fact]
    public async Task GetTechnicalUserProfilesForOffer_ReturnsExpected()
    {
        // Arrange
        var appId = Guid.NewGuid();
        A.CallTo(() => _offerService.GetTechnicalUserProfilesForOffer(appId, OfferTypeId.APP, A<IEnumerable<UserRoleConfig>>._, A<IEnumerable<UserRoleConfig>>._))
            .Returns(_fixture.CreateMany<TechnicalUserProfileInformation>(5));
        var sut = new AppReleaseBusinessLogic(null!, Options.Create(new AppsSettings()), _offerService, _offerDocumentService, null!, _identityService);

        // Act
        var result = await sut.GetTechnicalUserProfilesForOffer(appId);

        result.Should().HaveCount(5);
    }

    #endregion

    #region GetAppProviderRoles

    [Fact]
    public async Task GetAppProviderRolesAsync_Throws_NotFoundException()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var activeAppRoleDetails = default((bool, bool, IEnumerable<ActiveAppRoleDetails>));
        A.CallTo(() => _userRolesRepository.GetOfferProviderRolesAsync(A<Guid>._, A<OfferTypeId>._, A<Guid>._, A<string>._, A<string>._))
            .Returns(activeAppRoleDetails);

        // Act
        Task Act() => _sut.GetAppProviderRolesAsync(appId, null);

        // Assert
        var result = await Assert.ThrowsAsync<NotFoundException>(Act);
        result.Message.Should().Be(AppReleaseErrors.APP_NOT_EXIST.ToString());
        A.CallTo(() => _userRolesRepository.GetOfferProviderRolesAsync(appId, OfferTypeId.APP, _identity.CompanyId, null, "en"))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetAppProviderRolesAsync_Throws_ForbiddenException()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var activeAppRoleDetails = (true, false, default(IEnumerable<ActiveAppRoleDetails>?));
        A.CallTo(() => _userRolesRepository.GetOfferProviderRolesAsync(A<Guid>._, A<OfferTypeId>._, A<Guid>._, A<string>._, A<string>._))
            .Returns(activeAppRoleDetails);

        // Act
        Task Act() => _sut.GetAppProviderRolesAsync(appId, "de");

        // Assert
        var result = await Assert.ThrowsAsync<ForbiddenException>(Act);
        result.Message.Should().Be(AppReleaseErrors.APP_FORBIDDEN_COMP_ID_NOT_PROVIDER_COMPANY.ToString());
        A.CallTo(() => _userRolesRepository.GetOfferProviderRolesAsync(appId, OfferTypeId.APP, _identity.CompanyId, "de", "en"))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetAppProviderRolesAsync_ReturnsExpected()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var roleId1 = _fixture.Create<Guid>();
        var roleId2 = _fixture.Create<Guid>();
        var userRole1 = new ActiveAppRoleDetails(roleId1, "TestRole1", [
            new ActiveAppUserRoleDescription("en", "TestRole1 description")
        ]);
        var userRole2 = new ActiveAppRoleDetails(roleId2, "TestRole2", [
            new ActiveAppUserRoleDescription("en", "TestRole2 description")
        ]);
        var activeAppRoleDetails = (true, true, new[] {
            userRole1,
            userRole2
        });

        A.CallTo(() => _userRolesRepository.GetOfferProviderRolesAsync(A<Guid>._, A<OfferTypeId>._, A<Guid>._, A<string>._, A<string>._))
            .Returns(activeAppRoleDetails);

        // Act
        var result = await _sut.GetAppProviderRolesAsync(appId, "de");

        // Assert
        result.Should().HaveCount(2)
            .And.Satisfy(
                x => x.RoleId == roleId1 && x.Role == "TestRole1" && x.Descriptions.Count() == 1 && x.Descriptions.Single().Description == "TestRole1 description",
                x => x.RoleId == roleId2 && x.Role == "TestRole2" && x.Descriptions.Count() == 1 && x.Descriptions.Single().Description == "TestRole2 description");
        A.CallTo(() => _userRolesRepository.GetOfferProviderRolesAsync(appId, OfferTypeId.APP, _identity.CompanyId, "de", "en"))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region Setup

    private void SetupUpdateApp()
    {
        A.CallTo(() => _languageRepository.GetLanguageCodesUntrackedAsync(A<IEnumerable<string>>._))
            .Returns(_languageCodes.ToAsyncEnumerable());
        A.CallTo(() => _offerRepository.GetAppUpdateData(_notExistingAppId, _identity.CompanyId, A<IEnumerable<string>>._))
            .Returns<AppUpdateData?>(null);
        A.CallTo(() => _offerRepository.GetAppUpdateData(_activeAppId, _identity.CompanyId, A<IEnumerable<string>>._))
            .Returns(_fixture.Build<AppUpdateData>()
                .With(x => x.OfferState, OfferStatusId.ACTIVE)
                .With(x => x.IsUserOfProvider, false)
                .With(x => x.MatchingUseCases, _useCases)
                .With(x => x.Languages, _languageCodes.Select(x => (x, true)))
                .Create());
        A.CallTo(() => _offerRepository.GetAppUpdateData(_differentCompanyAppId, _identity.CompanyId, A<IEnumerable<string>>._))
            .Returns(_fixture.Build<AppUpdateData>()
                .With(x => x.OfferState, OfferStatusId.CREATED)
                .With(x => x.IsUserOfProvider, false)
                .With(x => x.MatchingUseCases, _useCases)
                .With(x => x.Languages, _languageCodes.Select(x => (x, true)))
                .Create());
        A.CallTo(() => _offerRepository.GetAppUpdateData(_existingAppId, _identity.CompanyId, A<IEnumerable<string>>._))
            .Returns(_appUpdateData);

        A.CallTo(() => _portalRepositories.GetInstance<ILanguageRepository>()).Returns(_languageRepository);
    }

    #endregion
}
