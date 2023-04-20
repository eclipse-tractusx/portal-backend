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

using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Apps.Service.ViewModels;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Service;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using System.Collections.Immutable;
using Xunit;

namespace Org.Eclipse.TractusX.Portal.Backend.Apps.Service.BusinessLogic.Tests;

public class AppReleaseBusinessLogicTest
{
    private readonly IFixture _fixture;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IOfferRepository _offerRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserRolesRepository _userRolesRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly IOptions<AppsSettings> _options;
    private readonly CompanyUser _companyUser;
    private readonly IamUser _iamUser;
    private readonly IOfferService _offerService;
    private readonly Guid _notExistingAppId = Guid.NewGuid();
    private readonly Guid _activeAppId = Guid.NewGuid();
    private readonly Guid _differentCompanyAppId = Guid.NewGuid();
    private readonly Guid _existingAppId = Guid.NewGuid();
    private readonly IEnumerable<Guid> _useCases = new [] { Guid.NewGuid(), Guid.NewGuid() };
    private readonly IEnumerable<string> _languageCodes = new [] { "de", "en" };
    private readonly AppUpdateData _appUpdateData;
    private readonly ILanguageRepository _languageRepository;
    private readonly AppsSettings _settings;
    private const string ClientId = "catenax-portal";
    private static readonly Guid ValidDocumentId = Guid.NewGuid();
    private static readonly string IamUserId = Guid.NewGuid().ToString();

    public AppReleaseBusinessLogicTest()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _portalRepositories = A.Fake<IPortalRepositories>();
        _offerRepository = A.Fake<IOfferRepository>();
        _userRolesRepository = A.Fake<IUserRolesRepository>();
        _userRepository = A.Fake<IUserRepository>();
        _documentRepository = A.Fake<IDocumentRepository>();
        _languageRepository = A.Fake<ILanguageRepository>();
        _offerService = A.Fake<IOfferService>();
        _options = A.Fake<IOptions<AppsSettings>>();
        _companyUser = _fixture.Build<CompanyUser>()
            .Without(u => u.IamUser)
            .Create();
        _iamUser = _fixture.Build<IamUser>()
            .With(u => u.CompanyUser, _companyUser)
            .Create();
        _companyUser.IamUser = _iamUser;
        
        _settings = A.Fake<AppsSettings>();
        _settings.OfferStatusIds = new [] 
        {
            OfferStatusId.IN_REVIEW,
            OfferStatusId.ACTIVE
        };
        _settings.ActiveAppNotificationTypeIds = new []
        {
            NotificationTypeId.APP_ROLE_ADDED
        };
        _settings.SubmitAppNotificationTypeIds = new []
        {
            NotificationTypeId.APP_RELEASE_REQUEST
        };
         _settings.ActiveAppCompanyAdminRoles = new Dictionary<string, IEnumerable<string>>
        {
            { ClientId, new [] { "Company Admin" } }
        };
        A.CallTo(() => _portalRepositories.GetInstance<IOfferRepository>()).Returns(_offerRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRolesRepository>()).Returns(_userRolesRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IDocumentRepository>()).Returns(_documentRepository);

        _appUpdateData = _fixture.Build<AppUpdateData>()
            .With(x => x.OfferState, OfferStatusId.CREATED)
            .With(x => x.IsUserOfProvider, true)
            .With(x => x.MatchingUseCases, _useCases)
            .With(x => x.Languages, _languageCodes.Select(x => (x, true)))
            .Create();
    }

    [Fact]
    public async Task CreateServiceOffering_WithValidDataAndEmptyDescriptions_ReturnsCorrectDetails()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var iamUserId = _fixture.Create<string>();
        var appUserRoles = _fixture.CreateMany<string>(3).Select(role => new AppUserRole(role, _fixture.CreateMany<AppUserRoleDescription>(2).ToImmutableArray())).ToImmutableArray();        

        A.CallTo(() => _offerRepository.IsProviderCompanyUserAsync(A<Guid>.That.IsEqualTo(appId), A<string>.That.IsEqualTo(iamUserId), A<OfferTypeId>.That.IsEqualTo(OfferTypeId.APP)))
            .Returns((true,true));

        IEnumerable<UserRole>? userRoles = null;
        A.CallTo(() => _userRolesRepository.CreateAppUserRoles(A<IEnumerable<(Guid,string)>>._))
            .ReturnsLazily((IEnumerable<(Guid AppId, string Role)> appRoles) =>
            {
                userRoles = appRoles.Select(x => new UserRole(Guid.NewGuid(), x.Role, x.AppId)).ToImmutableArray();
                return userRoles;
            });

        var userRoleDescriptions = new List<IEnumerable<UserRoleDescription>>();
        A.CallTo(() => _userRolesRepository.CreateAppUserRoleDescriptions(A<IEnumerable<(Guid,string,string)>>._))
            .ReturnsLazily((IEnumerable<(Guid RoleId, string LanguageCode, string Description)> roleLanguageDescriptions) =>
            {
                var createdUserRoleDescriptions = roleLanguageDescriptions.Select(x => new UserRoleDescription(x.RoleId, x.LanguageCode, x.Description)).ToImmutableArray();
                userRoleDescriptions.Add(createdUserRoleDescriptions);
                return createdUserRoleDescriptions;
            });

        var sut = new AppReleaseBusinessLogic(_portalRepositories, _options, null!);

        // Act
        var result = await sut.AddAppUserRoleAsync(appId, appUserRoles, iamUserId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _offerRepository.IsProviderCompanyUserAsync(A<Guid>._, A<string>._, A<OfferTypeId>._)).MustHaveHappened();
        
        A.CallTo(() => _userRolesRepository.CreateAppUserRoles(A<IEnumerable<(Guid,string)>>._)).MustHaveHappenedOnceExactly();
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

        A.CallTo(() => _userRolesRepository.CreateAppUserRoleDescriptions(A<IEnumerable<(Guid,string,string)>>._)).MustHaveHappened(appUserRoles.Length, Times.Exactly);
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

    #region AddAppAsync
    
    [Fact]
    public async Task AddAppAsync_WithoutEmptyLanguageCodes_ThrowsException()
    {
        // Arrange
        var data = _fixture.Build<AppRequestModel>()
            .With(x => x.SupportedLanguageCodes, new []{ String.Empty })
            .Create();
        var settings = new AppsSettings();
        var sut = new AppReleaseBusinessLogic(_portalRepositories, Options.Create(settings), _offerService);
     
        // Act
        async Task Act() => await sut.AddAppAsync(data, _iamUser.UserEntityId).ConfigureAwait(false);

        // Assert
        var error = await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);
        error.ParamName.Should().Be("SupportedLanguageCodes");
    }
    
    [Fact]
    public async Task AddAppAsync_WithEmptyUseCaseIds_ThrowsException()
    {
        // Arrange
        var data = _fixture.Build<AppRequestModel>()
            .With(x => x.UseCaseIds, new []{ Guid.Empty })
            .Create();
        var settings = new AppsSettings();
        var sut = new AppReleaseBusinessLogic(_portalRepositories, Options.Create(settings), _offerService);
     
        // Act
        async Task Act() => await sut.AddAppAsync(data, _iamUser.UserEntityId).ConfigureAwait(false);

        // Assert
        var error = await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);
        error.ParamName.Should().Be("UseCaseIds");
    }

    [Fact]
    public async Task AddAppAsync_WithSalesManagerValidData_ReturnsExpected()
    {
        // Arrange
        var data = _fixture.Build<AppRequestModel>()
            .With(x => x.SalesManagerId, _companyUser.Id)
            .Create();

        A.CallTo(() => _offerService.ValidateSalesManager(_companyUser.Id, _iamUser.UserEntityId, A<IDictionary<string, IEnumerable<string>>>._)).Returns(_companyUser.CompanyId);

        Offer? created = null;
        var offerId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        A.CallTo(() => _offerRepository.CreateOffer(A<string>._, A<OfferTypeId>._, A<Action<Offer>?>._))
            .ReturnsLazily((string provider, OfferTypeId offerTypeId, Action<Offer>? modify) =>
            {
                created = new Offer(offerId, provider, now, offerTypeId);
                modify?.Invoke(created);
                return created;
            });

        OfferLicense? offerLicense = null;
        A.CallTo(() => _offerRepository.CreateOfferLicenses(data.Price)).ReturnsLazily((string text) =>
        {
            offerLicense = new OfferLicense(Guid.NewGuid(), text);
            return offerLicense;
        });

        var settings = new AppsSettings();
        var sut = new AppReleaseBusinessLogic(_portalRepositories, Options.Create(settings), _offerService);

        // Act
        await sut.AddAppAsync(data, _iamUser.UserEntityId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _offerService.ValidateSalesManager(_companyUser.Id, _iamUser.UserEntityId, A<IDictionary<string, IEnumerable<string>>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.CreateOffer(A<string>._, A<OfferTypeId>._, A<Action<Offer>?>._))
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
                x.ProviderCompanyId == _companyUser.CompanyId &&
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
        var data = _fixture.Build<AppRequestModel>()
            .With(x => x.SalesManagerId, (Guid?)null)
            .Create();
        A.CallTo(() => _userRepository.GetOwnCompanyId(A<string>.That.IsEqualTo(_iamUser.UserEntityId))).Returns(_companyUser.CompanyId);

        Offer? created = null;
        var offerId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        A.CallTo(() => _offerRepository.CreateOffer(A<string>._, A<OfferTypeId>._, A<Action<Offer>?>._))
            .ReturnsLazily((string provider, OfferTypeId offerTypeId, Action<Offer>? modify) =>
            {
                created = new Offer(offerId, provider, now, offerTypeId);
                modify?.Invoke(created);
                return created;
            });

        var settings = new AppsSettings();
        var sut = new AppReleaseBusinessLogic(_portalRepositories, Options.Create(settings), _offerService);
     
        // Act
        await sut.AddAppAsync(data, _iamUser.UserEntityId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _offerService.ValidateSalesManager(A<Guid>._, A<string>._, A<IDictionary<string, IEnumerable<string>>>._)).MustNotHaveHappened();
        A.CallTo(() => _userRepository.GetOwnCompanyId(_iamUser.UserEntityId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.CreateOffer(A<string>._, A<OfferTypeId>._, A<Action<Offer>?>._))
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
                x.ProviderCompanyId == _companyUser.CompanyId &&
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
        var settings = new AppsSettings();
        var sut = new AppReleaseBusinessLogic(_portalRepositories, Options.Create(settings), _offerService);
     
        // Act
        async Task Act() => await sut.UpdateAppReleaseAsync(_notExistingAppId, data, _iamUser.UserEntityId).ConfigureAwait(false);

        // Assert
        var error = await Assert.ThrowsAsync<NotFoundException>(Act).ConfigureAwait(false);
        error.Message.Should().Be($"App {_notExistingAppId} does not exists");
    }
    
    [Fact]
    public async Task UpdateAppReleaseAsync_WithActiveApp_ThrowsException()
    {
        // Arrange
        SetupUpdateApp();
        var data = _fixture.Create<AppRequestModel>();
        var settings = new AppsSettings();
        var sut = new AppReleaseBusinessLogic(_portalRepositories, Options.Create(settings), _offerService);
     
        // Act
        async Task Act() => await sut.UpdateAppReleaseAsync(_activeAppId, data, _iamUser.UserEntityId).ConfigureAwait(false);

        // Assert
        var error = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);
        error.Message.Should().Be("Apps in State ACTIVE can't be updated");
    }

    [Fact]
    public async Task UpdateAppReleaseAsync_WithInvalidUser_ThrowsException()
    {
        // Arrange
        SetupUpdateApp();
        var data = _fixture.Create<AppRequestModel>();
        var settings = new AppsSettings();
        var sut = new AppReleaseBusinessLogic(_portalRepositories, Options.Create(settings), _offerService);
     
        // Act
        async Task Act() => await sut.UpdateAppReleaseAsync(_differentCompanyAppId, data, _iamUser.UserEntityId).ConfigureAwait(false);

        // Assert
        var error = await Assert.ThrowsAsync<ForbiddenException>(Act).ConfigureAwait(false);
        error.Message.Should().Be($"User {_iamUser.UserEntityId} is not allowed to change the app.");
    }

    [Fact]
    public async Task UpdateAppReleaseAsync_WithInvalidLanguage_ThrowsException()
    {
        // Arrange
        SetupUpdateApp();
        var data = _fixture.Build<AppRequestModel>()
            .With(x => x.SupportedLanguageCodes, new [] { "de", "en", "invalid" })
            .Create();
        var settings = new AppsSettings();
        var sut = new AppReleaseBusinessLogic(_portalRepositories, Options.Create(settings), _offerService);
        
        // Act
        async Task Act() => await sut.UpdateAppReleaseAsync(_existingAppId, data, _iamUser.UserEntityId).ConfigureAwait(false);

        // Assert
        var error = await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);
        error.ParamName.Should().Be("SupportedLanguageCodes");
    }

    [Fact]
    public async Task UpdateAppReleaseAsync_WithValidData_ReturnsExpected()
    {
        // Arrange
        SetupUpdateApp();
        var data = _fixture.Build<AppRequestModel>()
            .With(x => x.SupportedLanguageCodes, new [] { "de", "en" })
            .Create();
        var settings = new AppsSettings();
        var sut = new AppReleaseBusinessLogic(_portalRepositories, Options.Create(settings), _offerService);

        Offer? initial = null;
        Offer? modified = null;
        A.CallTo(() => _offerRepository.AttachAndModifyOffer(A<Guid>._, A<Action<Offer>>._, A<Action<Offer>>._))
            .Invokes((Guid offerId, Action<Offer> modify, Action<Offer>? initialize) =>
            {
                initial = new Offer(offerId, null!, default, default);
                modified = new Offer(offerId, null!, default, default);
                initialize?.Invoke(initial);
                modify.Invoke(modified);
            });

        // Act
        await sut.UpdateAppReleaseAsync(_existingAppId, data, _iamUser.UserEntityId).ConfigureAwait(false);
        
        // Assert
        A.CallTo(() => _offerRepository.AttachAndModifyOffer(A<Guid>._, A<Action<Offer>>._, A<Action<Offer>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerService.UpsertRemoveOfferDescription(_existingAppId, A<IEnumerable<LocalizedDescription>>.That.IsSameSequenceAs(data.Descriptions), A<IEnumerable<LocalizedDescription>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.AddAppLanguages(A<IEnumerable<(Guid appId, string languageShortName)>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.RemoveAppLanguages(A<IEnumerable<(Guid,string)>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.CreateDeleteAppAssignedUseCases(_existingAppId, A<IEnumerable<Guid>>.That.Matches(x => x.SequenceEqual(_useCases)), A<IEnumerable<Guid>>.That.Matches(x => x.SequenceEqual(data.UseCaseIds))))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerService.CreateOrUpdateOfferLicense(_existingAppId, A<string>._, A<(Guid OfferLicenseId, string LicenseText, bool AssingedToMultipleOffers)>._)).MustHaveHappenedOnceExactly();

        initial.Should().NotBeNull()
            .And.Match<Offer>(
                x => x.Name == _appUpdateData.Name &&
                x.Provider == _appUpdateData.Provider &&
                x.SalesManagerId == _appUpdateData.SalesManagerId &&
                x.ContactEmail == _appUpdateData.ContactEmail &&
                x.ContactNumber == _appUpdateData.ContactNumber &&
                x.MarketingUrl == _appUpdateData.MarketingUrl
            );

        modified.Should().NotBeNull()
            .And.Match<Offer>(
                x => x.Name == data.Title &&
                x.Provider == data.Provider &&
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

        var settings = new AppsSettings()
        {
            ContentTypeSettings = new[] { "application/pdf" },
            DocumentTypeIds = new[] { DocumentTypeId.APP_CONTRACT }
        };

        var sut = new AppReleaseBusinessLogic(_portalRepositories, Options.Create(settings), _offerService);

        // Act
        await sut.CreateAppDocumentAsync(appId, DocumentTypeId.APP_CONTRACT, file, _iamUser.UserEntityId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _offerService.UploadDocumentAsync(appId, DocumentTypeId.APP_CONTRACT, file, _iamUser.UserEntityId, OfferTypeId.APP, settings.DocumentTypeIds, settings.ContentTypeSettings, CancellationToken.None)).MustHaveHappenedOnceExactly();
    }
    
    #endregion

    #region SubmitAppReleaseRequestAsync

    [Fact]
    public async Task SubmitAppReleaseRequestAsync_CallsOfferService()
    {
        // Arrange
        var sut = new AppReleaseBusinessLogic(null!, _options, _offerService);

        // Act
        await sut.SubmitAppReleaseRequestAsync(_existingAppId, _iamUser.UserEntityId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => 
                _offerService.SubmitOfferAsync(
                    _existingAppId,
                    _iamUser.UserEntityId,
                    OfferTypeId.APP,
                    A<IEnumerable<NotificationTypeId>>._,
                    A<IDictionary<string, IEnumerable<string>>>._,A<IEnumerable<DocumentTypeId>>._))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region SubmitOfferConsentAsync
    
    [Fact]
    public async Task SubmitOfferConsentAsync_WithEmptyAppId_ThrowsControllerArgumentException()
    {
        // Arrange
        var sut = new AppReleaseBusinessLogic(null!, _options, _offerService);

        // Act
        async Task Act() => await sut.SubmitOfferConsentAsync(Guid.Empty, _fixture.Create<OfferAgreementConsent>(), _iamUser.UserEntityId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);
        ex.Message.Should().Be("AppId must not be empty");
    }

    [Fact]
    public async Task SubmitOfferConsentAsync_WithAppId_CallsOfferService()
    {
        // Arrange
        var data = _fixture.Create<OfferAgreementConsent>();
        var sut = new AppReleaseBusinessLogic(null!, _options, _offerService);

        // Act
        await sut.SubmitOfferConsentAsync(_existingAppId, data, _iamUser.UserEntityId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _offerService.CreateOrUpdateProviderOfferAgreementConsent(_existingAppId, data, _iamUser.UserEntityId, OfferTypeId.APP)).MustHaveHappenedOnceExactly();
    }

    #endregion

    #region GetAllInReviewStatusApps

    [Fact]
    public async Task GetAllInReviewStatusAppsAsync_DefaultRequest()
    {
        // Arrange
        var offerStatus = new[] { OfferStatusId.ACTIVE , OfferStatusId.IN_REVIEW };
        var InReviewData = new[] {
            new InReviewAppData(Guid.NewGuid(),null,null!, OfferStatusId.IN_REVIEW),
            new InReviewAppData(Guid.NewGuid(),null,null!, OfferStatusId.IN_REVIEW),
            new InReviewAppData(Guid.NewGuid(),null,null!, OfferStatusId.ACTIVE),
            new InReviewAppData(Guid.NewGuid(),null,null!, OfferStatusId.ACTIVE),
            new InReviewAppData(Guid.NewGuid(),null,null!, OfferStatusId.ACTIVE)
        };
        var paginationResult = (int skip, int take) => Task.FromResult(new Pagination.Source<InReviewAppData>(5, InReviewData.Skip(skip).Take(take)));
        A.CallTo(() => _offerRepository.GetAllInReviewStatusAppsAsync(A<IEnumerable<OfferStatusId>>._,A<OfferSorting>._))
            .Returns(paginationResult);
        var sut = new AppReleaseBusinessLogic(_portalRepositories, Options.Create(_settings), _offerService);

        // Act
        var result = await sut.GetAllInReviewStatusAppsAsync(0, 5, OfferSorting.DateAsc, null).ConfigureAwait(false);
        
        // Assert
        A.CallTo(() => _offerRepository.GetAllInReviewStatusAppsAsync(A<IEnumerable<OfferStatusId>>
            .That.Matches(x => x.Count() == 2 && x.All(y => offerStatus.Contains(y))),A<OfferSorting>._)).MustHaveHappenedOnceExactly();
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
        var InReviewData = new[]{
            new InReviewAppData(Guid.NewGuid(),null,null!, OfferStatusId.IN_REVIEW),
            new InReviewAppData(Guid.NewGuid(),null,null!, OfferStatusId.IN_REVIEW),
            new InReviewAppData(Guid.NewGuid(),null,null!, OfferStatusId.IN_REVIEW),
            new InReviewAppData(Guid.NewGuid(),null,null!, OfferStatusId.IN_REVIEW),
            new InReviewAppData(Guid.NewGuid(),null,null!, OfferStatusId.IN_REVIEW)
        };
        var paginationResult = (int skip, int take) => Task.FromResult(new Pagination.Source<InReviewAppData>(5, InReviewData.Skip(skip).Take(take)));
        A.CallTo(() => _offerRepository.GetAllInReviewStatusAppsAsync(A<IEnumerable<OfferStatusId>>._,A<OfferSorting>._))
            .Returns(paginationResult);
        var sut = new AppReleaseBusinessLogic(_portalRepositories, Options.Create(_settings), _offerService);

        // Act
        var result = await sut.GetAllInReviewStatusAppsAsync(0, 5, OfferSorting.DateAsc, OfferStatusIdFilter.InReview).ConfigureAwait(false);
        
        // Assert
        A.CallTo(() => _offerRepository.GetAllInReviewStatusAppsAsync(A<IEnumerable<OfferStatusId>>
            .That.Matches(x => x.Count() == 1 && x.All(y => offerStatus.Contains(y))),A<OfferSorting>._)).MustHaveHappenedOnceExactly();
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
        string IamUserId = "3e8343f7-4fe5-4296-8312-f33aa6dbde5d";
        var appId = _fixture.Create<Guid>();
        var data = new OfferDeclineRequest("Just a test");
        var settings = new AppsSettings
        {
            ServiceManagerRoles = _fixture.Create<Dictionary<string, IEnumerable<string>>>(),
            BasePortalAddress = "test"
        };
        var sut = new AppReleaseBusinessLogic(_portalRepositories, Options.Create(_settings), _offerService);
     
        // Act
        await sut.DeclineAppRequestAsync(appId, IamUserId, data).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _offerService.DeclineOfferAsync(appId, IamUserId, data,
            OfferTypeId.APP, NotificationTypeId.APP_RELEASE_REJECTION,
            A<IDictionary<string, IEnumerable<string>>>._, A<string>._)).MustHaveHappenedOnceExactly();
    }

    #endregion

    #region DeleteAppDocument

    [Fact]
    public async Task DeleteAppDocumentsAsync_ReturnsExpected()
    {
        // Arrange
        var documentId = _fixture.Create<Guid>();
        var settings = new AppsSettings
        {
            DeleteDocumentTypeIds = new[] { DocumentTypeId.APP_CONTRACT }
        };
        var sut = new AppReleaseBusinessLogic(_portalRepositories, Options.Create(settings), _offerService);

        // Act
        await sut.DeleteAppDocumentsAsync(documentId, IamUserId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _offerService.DeleteDocumentsAsync(documentId, IamUserId, A<IEnumerable<DocumentTypeId>>._, OfferTypeId.APP)).MustHaveHappenedOnceExactly();

    }

    #endregion

    #region DeleteApp
    
    [Fact]
    public async Task DeleteAppAsync_ReturnsExpectedResult()
    {
        //Arrange
        var appId = Guid.NewGuid();
        var appDeleteData = _fixture.Create<AppDeleteData>();
        
        A.CallTo(() => _offerRepository.GetAppDeleteDataAsync(appId, OfferTypeId.APP, IamUserId, OfferStatusId.CREATED))
            .Returns((true,true,true,true,appDeleteData));

        var sut = new AppReleaseBusinessLogic(_portalRepositories, _options, _offerService);

        //Act
        await sut.DeleteAppAsync(appId, IamUserId).ConfigureAwait(false);

        // Assert 
        A.CallTo(() => _offerRepository.GetAppDeleteDataAsync(appId, OfferTypeId.APP, IamUserId, OfferStatusId.CREATED))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.RemoveOfferAssignedLicenses(A<IEnumerable<(Guid,Guid)>>.That.Matches((IEnumerable<(Guid OfferId,Guid LicenceId)> offerLicenseIds) =>
            offerLicenseIds.All(x => x.OfferId == appId) && offerLicenseIds.Select(x => x.LicenceId).SequenceEqual(appDeleteData.OfferLicenseIds)))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.RemoveOfferAssignedUseCases(A<IEnumerable<(Guid,Guid)>>.That.Matches((IEnumerable<(Guid OfferId, Guid UseCaseId)> offerUseCaseIds) =>
            offerUseCaseIds.All(x => x.OfferId == appId) && offerUseCaseIds.Select(x => x.UseCaseId).SequenceEqual(appDeleteData.UseCaseIds)))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.RemoveOfferAssignedPrivacyPolicies(A<IEnumerable<(Guid,PrivacyPolicyId)>>.That.Matches((IEnumerable<(Guid OfferId,PrivacyPolicyId PolicyId)> offerPolicyIds) =>
            offerPolicyIds.All(x => x.OfferId == appId) && offerPolicyIds.Select(x => x.PolicyId).SequenceEqual(appDeleteData.PolicyIds)))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.RemoveOfferAssignedDocuments(A<IEnumerable<(Guid,Guid)>>.That.Matches((IEnumerable<(Guid OfferId,Guid DocumentId)> offerDocIds) =>
            offerDocIds.All(x => x.OfferId == appId) && offerDocIds.Select(x => x.DocumentId).SequenceEqual(appDeleteData.DocumentIdStatus.Select(x => x.DocumentId))))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.RemoveAppLanguages(A<IEnumerable<(Guid,string)>>.That.Matches((IEnumerable<(Guid OfferId,string Language)> offerLanguages) =>
            offerLanguages.All(x => x.OfferId == appId) && offerLanguages.Select(x => x.Language).SequenceEqual(appDeleteData.LanguageCodes)))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.RemoveOfferTags(A<IEnumerable<(Guid, string)>>.That.Matches((IEnumerable<(Guid OfferId,string Tag)> offerTags) =>
            offerTags.All(x => x.OfferId == appId) && offerTags.Select(x => x.Tag).SequenceEqual(appDeleteData.TagNames)))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.RemoveOfferDescriptions(A<IEnumerable<(Guid,string)>>.That.Matches((IEnumerable<(Guid OfferId,string Langugage)> descriptionLanguages) =>
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
        
        A.CallTo(() => _offerRepository.GetAppDeleteDataAsync(appId, OfferTypeId.APP, IamUserId, OfferStatusId.CREATED))
            .Returns((true, true, true, false, appDeleteData));

        var sut = new AppReleaseBusinessLogic(_portalRepositories, _options, _offerService);

        //Act
        async Task Act() =>  await sut.DeleteAppAsync(appId, IamUserId).ConfigureAwait(false);

        // Assert 
        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be($"user {IamUserId} is not a member of the providercompany of app {appId}");
    }

    [Fact]
    public async Task DeleteAppAsync_WithInvalidOfferStatus_ThrowsConflictException()
    {
        //Arrange
        var appId = Guid.NewGuid();
        var appDeleteData = _fixture.Create<AppDeleteData>();
        
        A.CallTo(() => _offerRepository.GetAppDeleteDataAsync(appId, OfferTypeId.APP, IamUserId, OfferStatusId.CREATED))
            .Returns((true, true, false, true, appDeleteData));

        var sut = new AppReleaseBusinessLogic(_portalRepositories, _options, _offerService);

        //Act
        async Task Act() =>  await sut.DeleteAppAsync(appId, IamUserId).ConfigureAwait(false);

        // Assert 
        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"App {appId} is not in Created State");
    }

    #endregion 

    #region Setup

    private void SetupUpdateApp()
    {
        A.CallTo(() => _languageRepository.GetLanguageCodesUntrackedAsync(A<IEnumerable<string>>._))
            .Returns(_languageCodes.ToAsyncEnumerable());
        A.CallTo(() => _offerRepository.GetAppUpdateData(_notExistingAppId, _iamUser.UserEntityId, A<IEnumerable<string>>._))
            .Returns((AppUpdateData?)null);
        A.CallTo(() => _offerRepository.GetAppUpdateData(_activeAppId, _iamUser.UserEntityId, A<IEnumerable<string>>._))
            .Returns(_fixture.Build<AppUpdateData>()
                .With(x => x.OfferState, OfferStatusId.ACTIVE)
                .With(x => x.IsUserOfProvider, false)
                .With(x => x.MatchingUseCases, _useCases)
                .With(x => x.Languages, _languageCodes.Select(x => (x, true)))
                .Create());
        A.CallTo(() => _offerRepository.GetAppUpdateData(_differentCompanyAppId, _iamUser.UserEntityId, A<IEnumerable<string>>._))
            .Returns(_fixture.Build<AppUpdateData>()
                .With(x => x.OfferState, OfferStatusId.CREATED)
                .With(x => x.IsUserOfProvider, false)
                .With(x => x.MatchingUseCases, _useCases)
                .With(x => x.Languages, _languageCodes.Select(x => (x, true)))
                .Create());
        A.CallTo(() => _offerRepository.GetAppUpdateData(_existingAppId, _iamUser.UserEntityId, A<IEnumerable<string>>._))
            .Returns(_appUpdateData);
        A.CallTo(() => _offerService.ValidateSalesManager(A<Guid>._, A<string>._, A<IDictionary<string, IEnumerable<string>>>._)).Returns(_companyUser.CompanyId);
        
        A.CallTo(() => _portalRepositories.GetInstance<ILanguageRepository>()).Returns(_languageRepository);
    }

    #endregion
}
