/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
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
using Org.CatenaX.Ng.Portal.Backend.Apps.Service.ViewModels;
using Org.CatenaX.Ng.Portal.Backend.Framework.ErrorHandling;
using Org.CatenaX.Ng.Portal.Backend.Notification.Library;
using Org.CatenaX.Ng.Portal.Backend.Offers.Library.Service;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.CatenaX.Ng.Portal.Backend.Tests.Shared;
using PortalBackend.DBAccess.Models;
using Xunit;

namespace Org.CatenaX.Ng.Portal.Backend.Apps.Service.BusinessLogic.Tests;

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
    private readonly INotificationService _notificationService;
    private readonly Guid _differentCompanyUserId = Guid.NewGuid();
    private readonly Guid _noSalesManagerUserId = Guid.NewGuid();
    private readonly Guid _notExistingAppId = Guid.NewGuid();
    private readonly Guid _activeAppId = Guid.NewGuid();
    private readonly Guid _differentCompanyAppId = Guid.NewGuid();
    private readonly Guid _existingAppId = Guid.NewGuid();
    private readonly ILanguageRepository _languageRepository;

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
        _notificationService = A.Fake<INotificationService>();
        _options = A.Fake<IOptions<AppsSettings>>();
        _companyUser = _fixture.Build<CompanyUser>()
            .Without(u => u.IamUser)
            .Create();
        _iamUser = _fixture.Build<IamUser>()
            .With(u => u.CompanyUser, _companyUser)
            .Create();
        _companyUser.IamUser = _iamUser;
        A.CallTo(() => _portalRepositories.GetInstance<IOfferRepository>()).Returns(_offerRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRolesRepository>()).Returns(_userRolesRepository);
    }

    [Fact]
    public async Task CreateServiceOffering_WithValidDataAndEmptyDescriptions_ReturnsCorrectDetails()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var iamUserId = _fixture.Create<string>();
        var appUserRoleDescription = new [] {
            new AppUserRoleDescription("en","this is test1"),
            new AppUserRoleDescription("de","this is test2"),
            new AppUserRoleDescription("fr","this is test3")
        };
        var appUserRoles = new [] {
            new AppUserRole("IT Admin",appUserRoleDescription)
        };
        A.CallTo(() => _offerRepository.IsProviderCompanyUserAsync(A<Guid>.That.IsEqualTo(appId), A<string>.That.IsEqualTo(iamUserId), A<OfferTypeId>.That.IsEqualTo(OfferTypeId.APP))).Returns((true,true));
        var sut = new AppReleaseBusinessLogic(_portalRepositories, _options, null!, null!);

        // Act
        var result = await sut.AddAppUserRoleAsync(appId, appUserRoles, iamUserId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _offerRepository.IsProviderCompanyUserAsync(A<Guid>._, A<string>._, A<OfferTypeId>._)).MustHaveHappened();
        foreach(var appRole in appUserRoles)
        {
            A.CallTo(() => _userRolesRepository.CreateAppUserRole(A<Guid>._, A<string>.That.IsEqualTo(appRole.role))).MustHaveHappened();
            foreach(var item in appRole.descriptions)
            {
                A.CallTo(() => _userRolesRepository.CreateAppUserRoleDescription(A<Guid>._, A<string>.That.IsEqualTo(item.languageCode), A<string>.That.IsEqualTo(item.description))).MustHaveHappened();
            }
        }

        Assert.NotNull(result);
        Assert.IsAssignableFrom<IEnumerable<AppRoleData>>(result);
    }

    #region AddAppAsync
    
    [Fact]
    public async Task AddAppAsync_WithoutEmptyLanguageCodes_ThrowsException()
    {
        // Arrange
        var data = new AppRequestModel("test", "test", null, Guid.NewGuid(), new List<Guid>(), new List<LocalizedDescription>(), new [] { string.Empty }, "123");
        var settings = new AppsSettings();
        var sut = new AppReleaseBusinessLogic(_portalRepositories, Options.Create(settings), _offerService, _notificationService);
     
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
        var data = new AppRequestModel("test", "test", null, Guid.NewGuid(), new []{ Guid.Empty }, new List<LocalizedDescription>(), new [] { "de" }, "123");
        var settings = new AppsSettings();
        var sut = new AppReleaseBusinessLogic(_portalRepositories, Options.Create(settings), _offerService, _notificationService);
     
        // Act
        async Task Act() => await sut.AddAppAsync(data, _iamUser.UserEntityId).ConfigureAwait(false);

        // Assert
        var error = await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);
        error.ParamName.Should().Be("UseCaseIds");
    }

    [Fact]
    public async Task AddAppAsync_WithInvalidSalesManager_ThrowsException()
    {
        // Arrange
        SetupValidateSalesManager();
        var data = new AppRequestModel("test", "test", null, Guid.NewGuid(), new []{ Guid.NewGuid() }, new List<LocalizedDescription>(), new [] { "de" }, "123");
        var settings = new AppsSettings();
        var sut = new AppReleaseBusinessLogic(_portalRepositories, Options.Create(settings), _offerService, _notificationService);
     
        // Act
        async Task Act() => await sut.AddAppAsync(data, _iamUser.UserEntityId).ConfigureAwait(false);

        // Assert
        var error = await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);
        error.ParamName.Should().Be("SalesManagerId");
    }

    [Fact]
    public async Task AddAppAsync_WithUserFromOtherCompany_ThrowsException()
    {
        // Arrange
        SetupValidateSalesManager();
        var data = new AppRequestModel("test", "test", null, _differentCompanyUserId, new []{ Guid.NewGuid() }, new List<LocalizedDescription>(), new [] { "de" }, "123");
        var settings = new AppsSettings();
        var sut = new AppReleaseBusinessLogic(_portalRepositories, Options.Create(settings), _offerService, _notificationService);
     
        // Act
        async Task Act() => await sut.AddAppAsync(data, _iamUser.UserEntityId).ConfigureAwait(false);

        // Assert
        var error = await Assert.ThrowsAsync<ForbiddenException>(Act).ConfigureAwait(false);
        error.Message.Should().Contain("is not a member of the company");
    }

    [Fact]
    public async Task AddAppAsync_WithUserWithoutSalesManagerRole_ThrowsException()
    {
        // Arrange
        SetupValidateSalesManager();
        var data = new AppRequestModel("test", "test", null, _noSalesManagerUserId, new []{ Guid.NewGuid() }, new List<LocalizedDescription>(), new [] { "de" }, "123");
        var settings = new AppsSettings();
        var sut = new AppReleaseBusinessLogic(_portalRepositories, Options.Create(settings), _offerService, _notificationService);
     
        // Act
        async Task Act() => await sut.AddAppAsync(data, _iamUser.UserEntityId).ConfigureAwait(false);

        // Assert
        var error = await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);
        error.ParamName.Should().Be("SalesManagerId");
    }

    [Fact]
    public async Task AddAppAsync_WithValidData_ReturnsExpected()
    {
        // Arrange
        SetupValidateSalesManager();
        var data = new AppRequestModel("test", "test", null, _companyUser.Id, new []{ Guid.NewGuid() }, new List<LocalizedDescription>{ new("de", "Long description", "desc")}, new [] { "de" }, "123");
        var settings = new AppsSettings();
        var sut = new AppReleaseBusinessLogic(_portalRepositories, Options.Create(settings), _offerService, _notificationService);
     
        // Act
        await sut.AddAppAsync(data, _iamUser.UserEntityId).ConfigureAwait(false);

        // Assert
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
    }

    #endregion
    
    #region UpdateAppReleaseAsync
    
    [Fact]
    public async Task UpdateAppReleaseAsync_WithoutApp_ThrowsException()
    {
        // Arrange
        SetupUpdateApp();
        var data = new AppRequestModel("test", "test", null, Guid.NewGuid(), new List<Guid>(), new List<LocalizedDescription>(), new [] { string.Empty }, "123");
        var settings = new AppsSettings();
        var sut = new AppReleaseBusinessLogic(_portalRepositories, Options.Create(settings), _offerService, _notificationService);
     
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
        var data = new AppRequestModel("test", "test", null, Guid.NewGuid(), new []{ Guid.Empty }, new List<LocalizedDescription>(), new [] { "de" }, "123");
        var settings = new AppsSettings();
        var sut = new AppReleaseBusinessLogic(_portalRepositories, Options.Create(settings), _offerService, _notificationService);
     
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
        var data = new AppRequestModel("test", "test", null, Guid.NewGuid(), new []{ Guid.NewGuid() }, new List<LocalizedDescription>(), new [] { "de" }, "123");
        var settings = new AppsSettings();
        var sut = new AppReleaseBusinessLogic(_portalRepositories, Options.Create(settings), _offerService, _notificationService);
     
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
        var data = new AppRequestModel("test", "test", null, _companyUser.Id, new []{ Guid.NewGuid() }, new List<LocalizedDescription>(), new [] { "de", "en", "invalid" }, "123");
        var settings = new AppsSettings();
        var sut = new AppReleaseBusinessLogic(_portalRepositories, Options.Create(settings), _offerService, _notificationService);
        
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
        var data = new AppRequestModel(
            "test",
            "test", 
            null, 
            _companyUser.Id, 
            new []{ Guid.NewGuid() },
            new List<LocalizedDescription>
            {
               new("de", "Long description", "desc") 
            }, 
            new [] { "de", "en" },
            "43");
        var settings = new AppsSettings();
        var sut = new AppReleaseBusinessLogic(_portalRepositories, Options.Create(settings), _offerService, _notificationService);
        
        // Act
        await sut.UpdateAppReleaseAsync(_existingAppId, data, _iamUser.UserEntityId).ConfigureAwait(false);
        
        // Assert
        A.CallTo(() => _offerRepository.AttachAndModifyOffer(A<Guid>._, A<Action<Offer>?>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.AddOfferDescriptions(A<IEnumerable<(Guid appId, string languageShortName, string descriptionLong, string descriptionShort)>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.AddAppLanguages(A<IEnumerable<(Guid appId, string languageShortName)>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.RemoveAppLanguages(A<IEnumerable<(Guid,string)>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.AddAppAssignedUseCases(A<IEnumerable<(Guid appId, Guid useCaseId)>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.AttachAndModifyOfferLicense(A<Guid>._,A<Action<OfferLicense>>._))
            .MustHaveHappenedOnceExactly();
    }

    #endregion
    
    #region Create App Document
        
    [Fact]
    public async Task CreateAppDocumentAsync_ExecutesSuccessfully()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var documentId = _fixture.Create<Guid>();
        var file = FormFileHelper.GetFormFile("this is just a test", "superFile.pdf", "application/pdf");
        var documents = new List<Document>();
        var offerAssignedDocuments = new List<OfferAssignedDocument>();
        SetupCreateAppDocument(appId);
        A.CallTo(() => _documentRepository.CreateDocument(A<string>._, A<byte[]>._, A<byte[]>._, A<DocumentTypeId>._,A<Action<Document>?>._))
            .Invokes((string documentName, byte[] documentContent, byte[] hash, DocumentTypeId documentType, Action<Document>? setupOptionalFields) =>
            {
                var document = new Document(documentId, documentContent, hash, documentName, DateTimeOffset.UtcNow, DocumentStatusId.PENDING, documentType);
                setupOptionalFields?.Invoke(document);
                documents.Add(document);
            });
        A.CallTo(() => _offerRepository.CreateOfferAssignedDocument(A<Guid>._, A<Guid>._))
            .Invokes((Guid offerId, Guid docId) =>
            {
                var offerAssignedDocument = new OfferAssignedDocument(offerId, docId);
                offerAssignedDocuments.Add(offerAssignedDocument);
            });
        var settings = new AppsSettings
        {
            ContentTypeSettings = new[] {"application/pdf"},
            DocumentTypeIds = new[] {DocumentTypeId.APP_CONTRACT}
        };
        
        var sut = new AppReleaseBusinessLogic(_portalRepositories, Options.Create(settings), _offerService, _notificationService);

        // Act
        await sut.CreateAppDocumentAsync(appId, DocumentTypeId.APP_CONTRACT, file, _iamUser.UserEntityId, CancellationToken.None);

        // Assert
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        documents.Should().HaveCount(1);
        offerAssignedDocuments.Should().HaveCount(1);
    }
    
    [Fact]
    public async Task CreateAppDocumentAsync_WrongContentTypeThrows()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var file = FormFileHelper.GetFormFile("this is just a test", "superFile.pdf", "application/pdf");
        SetupCreateAppDocument(appId);

        var settings = new AppsSettings()
        {
            ContentTypeSettings = new[] {"text/csv"},
            DocumentTypeIds = new[] {DocumentTypeId.APP_CONTRACT}
        };
        var sut = new AppReleaseBusinessLogic(_portalRepositories, Options.Create(settings), _offerService, _notificationService);
     
        // Act
        async Task Act() => await sut.CreateAppDocumentAsync(appId, DocumentTypeId.APP_CONTRACT, file, _iamUser.UserEntityId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        
        var error = await Assert.ThrowsAsync<UnsupportedMediaTypeException>(Act).ConfigureAwait(false);
       
        error.Message.Should().Be($"Document type not supported. File with contentType :{string.Join(",", settings.ContentTypeSettings)} are allowed.");
    }

    #endregion

    #region Setup

    private void SetupUpdateApp()
    {
        SetupValidateSalesManager();

        A.CallTo(() => _languageRepository.GetLanguageCodesUntrackedAsync(A<IEnumerable<string>>._))
            .Returns(new List<string>() {"de", "en"}.ToAsyncEnumerable());
        A.CallTo(() => _offerRepository.GetAppUpdateData(_notExistingAppId, _iamUser.UserEntityId, A<IEnumerable<string>>._, A<IEnumerable<Guid>>._, A<string>._))
            .ReturnsLazily(() => (AppUpdateData?)null);
        A.CallTo(() => _offerRepository.GetAppUpdateData(_activeAppId, _iamUser.UserEntityId, A<IEnumerable<string>>._, A<IEnumerable<Guid>>._, A<string>._))
            .ReturnsLazily(() => new AppUpdateData(OfferStatusId.ACTIVE, false, Array.Empty<(string, string, string)>(), Array.Empty<(string Shortname, bool IsMatch)>(), Array.Empty<Guid>(), new ValueTuple<Guid, string, bool>()));
        A.CallTo(() => _offerRepository.GetAppUpdateData(_differentCompanyAppId, _iamUser.UserEntityId, A<IEnumerable<string>>._, A<IEnumerable<Guid>>._, A<string>._))
            .ReturnsLazily(() => new AppUpdateData(OfferStatusId.CREATED, false, Array.Empty<(string, string, string)>(), Array.Empty<(string Shortname, bool IsMatch)>(), Array.Empty<Guid>(), new ValueTuple<Guid, string, bool>()));
        A.CallTo(() => _offerRepository.GetAppUpdateData(_existingAppId, _iamUser.UserEntityId, A<IEnumerable<string>>._, A<IEnumerable<Guid>>._, A<string>._))
            .ReturnsLazily(() => new AppUpdateData(OfferStatusId.CREATED, true, Array.Empty<(string, string, string)>(), Array.Empty<(string Shortname, bool IsMatch)>(), Array.Empty<Guid>(), new ValueTuple<Guid, string, bool>(Guid.NewGuid(), "123", false)));
    }

    private void SetupValidateSalesManager()
    {
        var roleIds = _fixture.CreateMany<Guid>(2);
        A.CallTo(() => _userRolesRepository.GetUserRoleIdsUntrackedAsync(A<IDictionary<string, IEnumerable<string>>>._))
            .Returns(roleIds.ToAsyncEnumerable());
        A.CallTo(() => _userRepository.GetRolesAndCompanyMembershipUntrackedAsync(A<string>._, A<IEnumerable<Guid>>._, A<Guid>.That.Matches(x => x == _companyUser.Id)))
            .ReturnsLazily(() => new ValueTuple<IEnumerable<Guid>, bool, Guid>(roleIds, true, _companyUser.CompanyId));
        A.CallTo(() => _userRepository.GetRolesAndCompanyMembershipUntrackedAsync(A<string>._, A<IEnumerable<Guid>>._, A<Guid>.That.Matches(x => x == _differentCompanyUserId)))
            .ReturnsLazily(() => new ValueTuple<IEnumerable<Guid>, bool, Guid>(Enumerable.Repeat(roleIds.First(), 1), false, Guid.NewGuid()));
        A.CallTo(() => _userRepository.GetRolesAndCompanyMembershipUntrackedAsync(A<string>._, A<IEnumerable<Guid>>._, A<Guid>.That.Matches(x => x == _noSalesManagerUserId)))
            .ReturnsLazily(() => new ValueTuple<IEnumerable<Guid>, bool, Guid>(Enumerable.Repeat(roleIds.First(), 1), true, _companyUser.CompanyId));
        A.CallTo(() => _userRepository.GetRolesAndCompanyMembershipUntrackedAsync(A<string>._, A<IEnumerable<Guid>>._, A<Guid>.That.Not.Matches(x => x == _companyUser.Id || x == _differentCompanyUserId || x == _noSalesManagerUserId)))
            .ReturnsLazily(() => new ValueTuple<IEnumerable<Guid>, bool, Guid>());
        A.CallTo(() => _portalRepositories.GetInstance<IUserRolesRepository>()).Returns(_userRolesRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ILanguageRepository>()).Returns(_languageRepository);
    }

    private void SetupCreateAppDocument(Guid appId)
    {
        A.CallTo(() => _offerRepository.GetProviderCompanyUserIdForOfferUntrackedAsync(appId, _iamUser.UserEntityId, OfferStatusId.CREATED, OfferTypeId.APP))
            .ReturnsLazily(() => (true, _companyUser.Id));
        A.CallTo(() => _portalRepositories.GetInstance<IOfferRepository>()).Returns(_offerRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IDocumentRepository>()).Returns(_documentRepository);
    }

    #endregion
}
