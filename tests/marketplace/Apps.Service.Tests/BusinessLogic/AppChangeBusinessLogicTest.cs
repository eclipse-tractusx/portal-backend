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

using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Apps.Service.ViewModels;
using Org.Eclipse.TractusX.Portal.Backend.Framework.DateTimeProvider;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Notifications.Library;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Service;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Web;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;
using System.Collections.Immutable;
using Xunit;

namespace Org.Eclipse.TractusX.Portal.Backend.Apps.Service.BusinessLogic.Tests;

public class AppChangeBusinessLogicTest
{
    private const string ClientId = "catenax-portal";
    private static readonly Guid CompanyUserId = Guid.NewGuid();
    private static readonly Guid CompanyId = Guid.NewGuid();
    private readonly IIdentityData _identity;

    private readonly IFixture _fixture;
    private readonly IProvisioningManager _provisioningManager;
    private readonly IPortalRepositories _portalRepositories;
    private readonly INotificationRepository _notificationRepository;
    private readonly IOfferRepository _offerRepository;
    private readonly IOfferSubscriptionsRepository _offerSubscriptionsRepository;
    private readonly IUserRolesRepository _userRolesRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly INotificationService _notificationService;
    private readonly IOfferService _offerService;
    private readonly IIdentityService _identityService;
    private readonly IOfferDocumentService _offerDocumentService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly DateTimeOffset _now;
    private readonly AppChangeBusinessLogic _sut;

    public AppChangeBusinessLogicTest()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.ConfigureFixture();

        _provisioningManager = A.Fake<IProvisioningManager>();
        _portalRepositories = A.Fake<IPortalRepositories>();
        _notificationRepository = A.Fake<INotificationRepository>();
        _offerRepository = A.Fake<IOfferRepository>();
        _offerSubscriptionsRepository = A.Fake<IOfferSubscriptionsRepository>();
        _userRolesRepository = A.Fake<IUserRolesRepository>();
        _documentRepository = A.Fake<IDocumentRepository>();
        _notificationService = A.Fake<INotificationService>();
        _offerService = A.Fake<IOfferService>();
        _identityService = A.Fake<IIdentityService>();
        _offerDocumentService = A.Fake<IOfferDocumentService>();
        _dateTimeProvider = A.Fake<IDateTimeProvider>();
        _identity = A.Fake<IIdentityData>();
        A.CallTo(() => _identity.IdentityId).Returns(CompanyUserId);
        A.CallTo(() => _identity.IdentityTypeId).Returns(IdentityTypeId.COMPANY_USER);
        A.CallTo(() => _identity.CompanyId).Returns(CompanyId);
        A.CallTo(() => _identityService.IdentityData).Returns(_identity);
        _now = _fixture.Create<DateTimeOffset>();
        A.CallTo(() => _dateTimeProvider.OffsetNow).Returns(_now);

        var settings = new AppsSettings
        {
            ActiveAppNotificationTypeIds = new[]
            {
                NotificationTypeId.APP_ROLE_ADDED
            },
            ActiveAppCompanyAdminRoles = new[]
            {
                new UserRoleConfig(ClientId, new [] { "Company Admin" })
            },
            CompanyAdminRoles = new[]
            {
                new UserRoleConfig(ClientId, new [] { "Company Admin" })
            },
            ActiveAppDocumentTypeIds = new[]
            {
                DocumentTypeId.APP_IMAGE,
                DocumentTypeId.APP_TECHNICAL_INFORMATION,
                DocumentTypeId.APP_CONTRACT,
                DocumentTypeId.ADDITIONAL_DETAILS
            },
            DeleteActiveAppDocumentTypeIds = new[] {
                DocumentTypeId.APP_IMAGE,
                DocumentTypeId.APP_TECHNICAL_INFORMATION,
                DocumentTypeId.APP_CONTRACT,
                DocumentTypeId.ADDITIONAL_DETAILS
            },
            UploadActiveAppDocumentTypeIds = new[] {
                new UploadDocumentConfig(DocumentTypeId.APP_IMAGE , new [] {MediaTypeId.JPEG, MediaTypeId.PNG}),
                new UploadDocumentConfig(DocumentTypeId.APP_TECHNICAL_INFORMATION , new [] {MediaTypeId.PDF}),
                new UploadDocumentConfig(DocumentTypeId.APP_CONTRACT , new [] {MediaTypeId.PDF}),
                new UploadDocumentConfig(DocumentTypeId.ADDITIONAL_DETAILS , new [] {MediaTypeId.PDF})
            }
        };
        A.CallTo(() => _portalRepositories.GetInstance<INotificationRepository>()).Returns(_notificationRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IOfferRepository>()).Returns(_offerRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IOfferSubscriptionsRepository>()).Returns(_offerSubscriptionsRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRolesRepository>()).Returns(_userRolesRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IDocumentRepository>()).Returns(_documentRepository);
        _sut = new AppChangeBusinessLogic(_portalRepositories, _notificationService, _provisioningManager, _offerService, _identityService, Options.Create(settings), _offerDocumentService, _dateTimeProvider);
    }

    #region  AddActiveAppUserRole

    [Fact]
    public async Task AddActiveAppUserRoleAsync_ExecutesSuccessfully()
    {
        //Arrange
        var appId = _fixture.Create<Guid>();
        var appName = _fixture.Create<string>();
        var appAssignedRoleDesc = _fixture.CreateMany<string>(3).Select(role => new AppUserRole(role, _fixture.CreateMany<AppUserRoleDescription>(2).ToImmutableArray())).ToImmutableArray();
        var clientIds = new[] { "client" };

        A.CallTo(() => _portalRepositories.GetInstance<IOfferRepository>().GetInsertActiveAppUserRoleDataAsync(appId, OfferTypeId.APP))
            .Returns((true, appName, _identity.CompanyId, clientIds));

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
        A.CallTo(() => _notificationService.CreateNotifications(A<IEnumerable<UserRoleConfig>>._, A<Guid>._, A<IEnumerable<(string? content, NotificationTypeId notificationTypeId)>>._, A<Guid>._, A<bool?>._))
            .Returns(_fixture.CreateMany<Guid>(4).AsFakeIAsyncEnumerable(out var createNotificationsResultAsyncEnumerator));

        //Act
        var result = await _sut.AddActiveAppUserRoleAsync(appId, appAssignedRoleDesc);

        //Assert
        A.CallTo(() => _offerRepository.GetInsertActiveAppUserRoleDataAsync(appId, OfferTypeId.APP)).MustHaveHappened();

        A.CallTo(() => _userRolesRepository.CreateAppUserRoles(A<IEnumerable<(Guid, string)>>._)).MustHaveHappenedOnceExactly();
        userRoles.Should().NotBeNull()
            .And.HaveSameCount(appAssignedRoleDesc)
            .And.AllSatisfy(x =>
            {
                x.Id.Should().NotBeEmpty();
                x.OfferId.Should().Be(appId);
            })
            .And.Satisfy(
                x => x.UserRoleText == appAssignedRoleDesc[0].Role,
                x => x.UserRoleText == appAssignedRoleDesc[1].Role,
                x => x.UserRoleText == appAssignedRoleDesc[2].Role
            );

        A.CallTo(() => _userRolesRepository.CreateAppUserRoleDescriptions(A<IEnumerable<(Guid, string, string)>>._)).MustHaveHappened(appAssignedRoleDesc.Length, Times.Exactly);
        userRoleDescriptions.Should()
            .HaveSameCount(appAssignedRoleDesc)
            .And.SatisfyRespectively(
                x => x.Should().HaveCount(2).And.Satisfy(
                    x => x.UserRoleId == userRoles!.ElementAt(0).Id && x.LanguageShortName == appAssignedRoleDesc[0].Descriptions.ElementAt(0).LanguageCode && x.Description == appAssignedRoleDesc[0].Descriptions.ElementAt(0).Description,
                    x => x.UserRoleId == userRoles!.ElementAt(0).Id && x.LanguageShortName == appAssignedRoleDesc[0].Descriptions.ElementAt(1).LanguageCode && x.Description == appAssignedRoleDesc[0].Descriptions.ElementAt(1).Description),
                x => x.Should().HaveCount(2).And.Satisfy(
                    x => x.UserRoleId == userRoles!.ElementAt(1).Id && x.LanguageShortName == appAssignedRoleDesc[1].Descriptions.ElementAt(0).LanguageCode && x.Description == appAssignedRoleDesc[1].Descriptions.ElementAt(0).Description,
                    x => x.UserRoleId == userRoles!.ElementAt(1).Id && x.LanguageShortName == appAssignedRoleDesc[1].Descriptions.ElementAt(1).LanguageCode && x.Description == appAssignedRoleDesc[1].Descriptions.ElementAt(1).Description),
                x => x.Should().HaveCount(2).And.Satisfy(
                    x => x.UserRoleId == userRoles!.ElementAt(2).Id && x.LanguageShortName == appAssignedRoleDesc[2].Descriptions.ElementAt(0).LanguageCode && x.Description == appAssignedRoleDesc[2].Descriptions.ElementAt(0).Description,
                    x => x.UserRoleId == userRoles!.ElementAt(2).Id && x.LanguageShortName == appAssignedRoleDesc[2].Descriptions.ElementAt(1).LanguageCode && x.Description == appAssignedRoleDesc[2].Descriptions.ElementAt(1).Description));
        A.CallTo(() => _offerRepository.AttachAndModifyOffer(appId, A<Action<Offer>>._, A<Action<Offer>?>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _notificationService.CreateNotifications(A<IEnumerable<UserRoleConfig>>._, A<Guid>._, A<IEnumerable<(string? content, NotificationTypeId notificationTypeId)>>._, A<Guid>._, A<bool?>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => createNotificationsResultAsyncEnumerator.MoveNextAsync())
            .MustHaveHappened(5, Times.Exactly);
        A.CallTo(() => _provisioningManager.AddRolesToClientAsync("client", A<IEnumerable<string>>.That.IsSameSequenceAs(appAssignedRoleDesc.Select(x => x.Role))))
            .MustHaveHappenedOnceExactly();

        result.Should().NotBeNull()
            .And.HaveSameCount(appAssignedRoleDesc)
            .And.Satisfy(
                x => x.RoleId == userRoles!.ElementAt(0).Id && x.RoleName == appAssignedRoleDesc[0].Role,
                x => x.RoleId == userRoles!.ElementAt(1).Id && x.RoleName == appAssignedRoleDesc[1].Role,
                x => x.RoleId == userRoles!.ElementAt(2).Id && x.RoleName == appAssignedRoleDesc[2].Role
            );
    }

    [Fact]
    public async Task AddActiveAppUserRoleAsync_WithCompanyUserIdNotSet_ThrowsForbiddenException()
    {
        //Arrange
        var appId = _fixture.Create<Guid>();
        var appName = _fixture.Create<string>();

        var appUserRoleDescription = new AppUserRoleDescription[] {
            new("de","this is test1"),
            new("en","this is test2"),
        };
        var appAssignedRoleDesc = new AppUserRole[] { new("Legal Admin", appUserRoleDescription) };
        var clientIds = new[] { "client" };
        A.CallTo(() => _offerRepository.GetInsertActiveAppUserRoleDataAsync(appId, OfferTypeId.APP))
            .Returns((true, appName, Guid.NewGuid(), clientIds));

        //Act
        async Task Act() => await _sut.AddActiveAppUserRoleAsync(appId, appAssignedRoleDesc);

        //Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be($"Company {_identity.CompanyId} is not the provider company of app {appId}");
    }

    [Fact]
    public async Task AddActiveAppUserRoleAsync_WithProviderCompanyNotSet_ThrowsConflictException()
    {
        //Arrange
        const string appName = "app name";
        var appId = _fixture.Create<Guid>();

        var appUserRoleDescription = new AppUserRoleDescription[] {
            new("de","this is test1"),
            new("en","this is test2"),
        };
        var appAssignedRoleDesc = new AppUserRole[] { new("Legal Admin", appUserRoleDescription) };
        var clientIds = new[] { "client" };
        A.CallTo(() => _offerRepository.GetInsertActiveAppUserRoleDataAsync(appId, OfferTypeId.APP))
            .Returns((true, appName, null, clientIds));

        //Act
        async Task Act() => await _sut.AddActiveAppUserRoleAsync(appId, appAssignedRoleDesc);

        //Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"App {appId} providing company is not yet set.");
    }

    #endregion

    #region GetAppUpdateDescriptionById

    [Fact]
    public async Task GetAppUpdateDescriptionByIdAsync_ReturnsExpected()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var offerDescription = _fixture.CreateMany<LocalizedDescription>(3);

        A.CallTo(() => _offerRepository.GetActiveOfferDescriptionDataByIdAsync(appId, OfferTypeId.APP, _identity.CompanyId))
            .Returns((true, true, offerDescription));

        // Act
        var result = await _sut.GetAppUpdateDescriptionByIdAsync(appId);

        // Assert
        result.Should().NotBeNull();
        result.Select(x => x.LanguageCode).Should().Contain(offerDescription.Select(od => od.LanguageCode));
    }

    [Fact]
    public async Task GetAppUpdateDescriptionByIdAsync_ThrowsNotFoundException()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();

        A.CallTo(() => _offerRepository.GetActiveOfferDescriptionDataByIdAsync(appId, OfferTypeId.APP, _identity.CompanyId))
            .Returns<(bool, bool, IEnumerable<LocalizedDescription>?)>(default);

        // Act
        async Task Act() => await _sut.GetAppUpdateDescriptionByIdAsync(appId);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"App {appId} does not exist.");
    }

    [Fact]
    public async Task GetAppUpdateDescriptionByIdAsync_ThrowsConflictException()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var offerDescription = _fixture.CreateMany<LocalizedDescription>(3);

        A.CallTo(() => _offerRepository.GetActiveOfferDescriptionDataByIdAsync(appId, OfferTypeId.APP, _identity.CompanyId))
            .Returns((false, true, offerDescription));

        // Act
        async Task Act() => await _sut.GetAppUpdateDescriptionByIdAsync(appId);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"App {appId} is in InCorrect Status");
    }

    [Fact]
    public async Task GetAppUpdateDescriptionByIdAsync_ThrowsForbiddenException()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var offerDescription = _fixture.CreateMany<LocalizedDescription>(3);

        A.CallTo(() => _offerRepository.GetActiveOfferDescriptionDataByIdAsync(appId, OfferTypeId.APP, _identity.CompanyId))
            .Returns((true, false, offerDescription));

        // Act
        async Task Act() => await _sut.GetAppUpdateDescriptionByIdAsync(appId);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be($"Company {_identity.CompanyId} is not the provider company of App {appId}");
    }

    [Fact]
    public async Task GetAppUpdateDescriptionByIdAsync_withNullDescriptionData_ThrowsUnexpectedConditionException()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        A.CallTo(() => _offerRepository.GetActiveOfferDescriptionDataByIdAsync(appId, OfferTypeId.APP, _identity.CompanyId))
            .Returns((true, true, default(IEnumerable<LocalizedDescription>?)));

        // Act
        var Act = () => _sut.GetAppUpdateDescriptionByIdAsync(appId);

        // Assert
        var result = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);
        result.Message.Should().Be("offerDescriptionDatas should never be null here");

    }

    #endregion

    #region Add / Update App Description

    [Fact]
    public async Task CreateOrUpdateAppDescriptionByIdAsync_withEmptyExistingDescriptionData_ReturnExpectedResult()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var updateDescriptionData = new[]{
            new LocalizedDescription("en", _fixture.Create<string>(), _fixture.Create<string>()),
            new LocalizedDescription("de", _fixture.Create<string>(), _fixture.Create<string>())
            };

        A.CallTo(() => _offerRepository.GetActiveOfferDescriptionDataByIdAsync(appId, OfferTypeId.APP, _identity.CompanyId))
            .Returns((true, true, updateDescriptionData));
        var existingOffer = _fixture.Create<Offer>();
        existingOffer.DateLastChanged = DateTimeOffset.UtcNow;
        A.CallTo(() => _offerRepository.AttachAndModifyOffer(appId, A<Action<Offer>>._, A<Action<Offer>?>._))
            .Invokes((Guid _, Action<Offer> setOptionalParameters, Action<Offer>? initializeParemeters) =>
            {
                initializeParemeters?.Invoke(existingOffer);
                setOptionalParameters(existingOffer);
            });
        // Act
        await _sut.CreateOrUpdateAppDescriptionByIdAsync(appId, updateDescriptionData);

        // Assert
        A.CallTo(() => _offerRepository.CreateUpdateDeleteOfferDescriptions(appId, A<IEnumerable<LocalizedDescription>>._, A<IEnumerable<(string, string, string)>>.That.IsSameSequenceAs(updateDescriptionData.Select(x => new ValueTuple<string, string, string>(x.LanguageCode, x.LongDescription, x.ShortDescription)))))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.AttachAndModifyOffer(appId, A<Action<Offer>>._, A<Action<Offer>?>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CreateOrUpdateAppDescriptionByIdAsync_withNullDescriptionData_ThowsUnexpectedConditionException()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var updateDescriptionData = new[]{
            new LocalizedDescription("en", _fixture.Create<string>(), _fixture.Create<string>()),
            new LocalizedDescription("de", _fixture.Create<string>(), _fixture.Create<string>())
            };

        A.CallTo(() => _offerRepository.GetActiveOfferDescriptionDataByIdAsync(appId, OfferTypeId.APP, _identity.CompanyId))
            .Returns((true, true, default(IEnumerable<LocalizedDescription>?)));

        // Act
        var Act = () => _sut.CreateOrUpdateAppDescriptionByIdAsync(appId, updateDescriptionData);

        // Assert
        var result = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);
        result.Message.Should().Be("offerDescriptionDatas should never be null here");

    }

    [Fact]
    public async Task CreateOrUpdateAppDescriptionByIdAsync_ThrowsForbiddenException()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var updateDescriptionData = new[]{
            new LocalizedDescription("en", _fixture.Create<string>(), _fixture.Create<string>()),
            new LocalizedDescription("de", _fixture.Create<string>(), _fixture.Create<string>())
            };

        A.CallTo(() => _offerRepository.GetActiveOfferDescriptionDataByIdAsync(appId, OfferTypeId.APP, _identity.CompanyId))
            .Returns((true, false, default(IEnumerable<LocalizedDescription>?)));

        // Act
        var Act = () => _sut.CreateOrUpdateAppDescriptionByIdAsync(appId, updateDescriptionData);

        // Assert
        var result = await Assert.ThrowsAsync<ForbiddenException>(Act);
        result.Message.Should().Be($"Company {_identity.CompanyId} is not the provider company of App {appId}");

    }

    [Fact]
    public async Task CreateOrUpdateAppDescriptionByIdAsync_ThrowsConflictException()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var updateDescriptionData = new[]{
            new LocalizedDescription("en", _fixture.Create<string>(), _fixture.Create<string>()),
            new LocalizedDescription("de", _fixture.Create<string>(), _fixture.Create<string>())
            };

        A.CallTo(() => _offerRepository.GetActiveOfferDescriptionDataByIdAsync(appId, OfferTypeId.APP, _identity.CompanyId))
            .Returns((false, true, default(IEnumerable<LocalizedDescription>?)));

        // Act
        var Act = () => _sut.CreateOrUpdateAppDescriptionByIdAsync(appId, updateDescriptionData);

        // Assert
        var result = await Assert.ThrowsAsync<ConflictException>(Act);
        result.Message.Should().Be($"App {appId} is in InCorrect Status");

    }

    [Fact]
    public async Task CreateOrUpdateAppDescriptionByIdAsync_ThrowsNotFoundException()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var updateDescriptionData = new[]{
            new LocalizedDescription("en", _fixture.Create<string>(), _fixture.Create<string>()),
            new LocalizedDescription("de", _fixture.Create<string>(), _fixture.Create<string>())
            };

        A.CallTo(() => _offerRepository.GetActiveOfferDescriptionDataByIdAsync(appId, OfferTypeId.APP, _identity.CompanyId))
            .Returns<(bool, bool, IEnumerable<LocalizedDescription>?)>(default);

        // Act
        var Act = () => _sut.CreateOrUpdateAppDescriptionByIdAsync(appId, updateDescriptionData);

        // Assert
        var result = await Assert.ThrowsAsync<NotFoundException>(Act);
        result.Message.Should().Be($"App {appId} does not exist.");

    }

    #endregion

    #region  UploadOfferAssignedAppLeadImageDocumentById

    [Fact]
    public async Task UploadOfferAssignedAppLeadImageDocumentById_ExpectedCalls()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var documentId = _fixture.Create<Guid>();
        var documentStatusData = _fixture.CreateMany<DocumentStatusData>(2).ToImmutableArray();
        var file = FormFileHelper.GetFormFile("Test Image", "TestImage.jpeg", "image/jpeg");
        var documents = new List<Document>();
        var offerAssignedDocuments = new List<OfferAssignedDocument>();

        A.CallTo(() => _offerRepository.GetOfferAssignedAppLeadImageDocumentsByIdAsync(A<Guid>._, A<Guid>._, A<OfferTypeId>._))
            .Returns((true, true, documentStatusData));

        A.CallTo(() => _documentRepository.CreateDocument(A<string>._, A<byte[]>._, A<byte[]>._, A<MediaTypeId>._, A<DocumentTypeId>._, A<Action<Document>?>._))
            .ReturnsLazily((string documentName, byte[] documentContent, byte[] hash, MediaTypeId mediaTypeId, DocumentTypeId documentType, Action<Document>? setupOptionalFields) =>
            {
                var document = new Document(documentId, documentContent, hash, documentName, mediaTypeId, DateTimeOffset.UtcNow, DocumentStatusId.LOCKED, documentType);
                setupOptionalFields?.Invoke(document);
                documents.Add(document);
                return document;
            });

        A.CallTo(() => _offerRepository.CreateOfferAssignedDocument(A<Guid>._, A<Guid>._))
            .Invokes((Guid offerId, Guid docId) =>
            {
                var offerAssignedDocument = new OfferAssignedDocument(offerId, docId);
                offerAssignedDocuments.Add(offerAssignedDocument);
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
        await _sut.UploadOfferAssignedAppLeadImageDocumentByIdAsync(appId, file, CancellationToken.None);

        // Assert
        A.CallTo(() => _offerRepository.GetOfferAssignedAppLeadImageDocumentsByIdAsync(appId, _identity.CompanyId, OfferTypeId.APP)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _documentRepository.CreateDocument(A<string>._, A<byte[]>._, A<byte[]>._, A<MediaTypeId>._, A<DocumentTypeId>._, A<Action<Document>?>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.CreateOfferAssignedDocument(A<Guid>._, A<Guid>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.RemoveOfferAssignedDocuments(A<IEnumerable<(Guid OfferId, Guid DocumentId)>>.That.IsSameSequenceAs(documentStatusData.Select(data => new ValueTuple<Guid, Guid>(appId, data.DocumentId))))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _documentRepository.RemoveDocuments(A<IEnumerable<Guid>>.That.IsSameSequenceAs(documentStatusData.Select(data => data.DocumentId)))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.AttachAndModifyOffer(appId, A<Action<Offer>>._, A<Action<Offer>?>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        documents.Should().ContainSingle().Which.Should().Match<Document>(x => x.Id == documentId && x.MediaTypeId == MediaTypeId.JPEG && x.DocumentTypeId == DocumentTypeId.APP_LEADIMAGE && x.DocumentStatusId == DocumentStatusId.LOCKED);
        offerAssignedDocuments.Should().ContainSingle().Which.Should().Match<OfferAssignedDocument>(x => x.DocumentId == documentId && x.OfferId == appId);
    }

    [Fact]
    public async Task UploadOfferAssignedAppLeadImageDocumentById_ThrowsUnsupportedMediaTypeException()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var appLeadImageContentTypes = new[] { MediaTypeId.JPEG, MediaTypeId.PNG };
        var file = FormFileHelper.GetFormFile("Test File", "TestImage.pdf", "application/pdf");

        // Act
        var Act = () => _sut.UploadOfferAssignedAppLeadImageDocumentByIdAsync(appId, file, CancellationToken.None);

        // Assert
        var result = await Assert.ThrowsAsync<UnsupportedMediaTypeException>(Act);
        result.Message.Should().Be($"Document type not supported. File must match contentTypes :{string.Join(",", appLeadImageContentTypes.Select(x => x.MapToMediaType()))}");
    }

    [Fact]
    public async Task UploadOfferAssignedAppLeadImageDocumentById_ThrowsConflictException()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var file = FormFileHelper.GetFormFile("Test Image", "TestImage.jpeg", "image/jpeg");

        A.CallTo(() => _offerRepository.GetOfferAssignedAppLeadImageDocumentsByIdAsync(A<Guid>._, A<Guid>._, A<OfferTypeId>._))
            .Returns((false, true, Enumerable.Empty<DocumentStatusData>()));

        // Act
        var Act = () => _sut.UploadOfferAssignedAppLeadImageDocumentByIdAsync(appId, file, CancellationToken.None);

        // Assert
        var result = await Assert.ThrowsAsync<ConflictException>(Act);
        result.Message.Should().Be("offerStatus is in incorrect State");
        A.CallTo(() => _offerRepository.GetOfferAssignedAppLeadImageDocumentsByIdAsync(appId, CompanyId, OfferTypeId.APP)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task UploadOfferAssignedAppLeadImageDocumentById_ThrowsForbiddenException()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var file = FormFileHelper.GetFormFile("Test Image", "TestImage.jpeg", "image/jpeg");

        A.CallTo(() => _offerRepository.GetOfferAssignedAppLeadImageDocumentsByIdAsync(A<Guid>._, A<Guid>._, A<OfferTypeId>._))
            .Returns((true, false, Enumerable.Empty<DocumentStatusData>()));

        // Act
        async Task Act() => await _sut.UploadOfferAssignedAppLeadImageDocumentByIdAsync(appId, file, CancellationToken.None);

        // Assert
        var result = await Assert.ThrowsAsync<ForbiddenException>(Act);
        result.Message.Should().Be($"Company {CompanyId} is not the provider company of App {appId}");
        A.CallTo(() => _offerRepository.GetOfferAssignedAppLeadImageDocumentsByIdAsync(appId, CompanyId, OfferTypeId.APP)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task UploadOfferAssignedAppLeadImageDocumentById_ThrowsNotFoundException()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var file = FormFileHelper.GetFormFile("Test Image", "TestImage.jpeg", "image/jpeg");

        A.CallTo(() => _offerRepository.GetOfferAssignedAppLeadImageDocumentsByIdAsync(A<Guid>._, A<Guid>._, A<OfferTypeId>._))
            .Returns<(bool, bool, IEnumerable<DocumentStatusData>)>(default);

        // Act
        async Task Act() => await _sut.UploadOfferAssignedAppLeadImageDocumentByIdAsync(appId, file, CancellationToken.None);

        // Assert
        var result = await Assert.ThrowsAsync<NotFoundException>(Act);
        result.Message.Should().Be($"App {appId} does not exist.");
        A.CallTo(() => _offerRepository.GetOfferAssignedAppLeadImageDocumentsByIdAsync(appId, CompanyId, OfferTypeId.APP)).MustHaveHappenedOnceExactly();
    }

    #endregion

    #region  DeactivateOfferbyAppId

    [Fact]
    public async Task DeactivateOfferStatusbyAppIdAsync_CallsExpected()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();

        // Act
        await _sut.DeactivateOfferByAppIdAsync(appId);

        // Assert
        A.CallTo(() => _offerService.DeactivateOfferIdAsync(appId, OfferTypeId.APP)).MustHaveHappenedOnceExactly();
    }

    #endregion

    #region Update TenantUrl

    [Fact]
    public async Task UpdateTenantUrlAsync_WithValidData_CallsExpected()
    {
        // Arrange
        const string clientClientId = "sa123";
        const string oldUrl = "https://old-url.com";
        var data = new UpdateTenantData("https://new-url.com");
        var appId = _fixture.Create<Guid>();
        var subscriptionId = _fixture.Create<Guid>();
        var requester = _fixture.Create<Guid>();
        var subscribingCompany = _fixture.Create<Guid>();
        var detailId = _fixture.Create<Guid>();
        var notifications = new List<Notification>();
        var details = new AppSubscriptionDetail(detailId, subscriptionId)
        {
            AppSubscriptionUrl = oldUrl
        };
        A.CallTo(() => _offerSubscriptionsRepository.GetUpdateUrlDataAsync(appId, subscriptionId, _identity.CompanyId))
            .Returns(new OfferUpdateUrlData("testApp", false, true, requester, subscribingCompany, OfferSubscriptionStatusId.ACTIVE, new OfferUpdateUrlSubscriptionDetailData(detailId, clientClientId, oldUrl)));
        A.CallTo(() => _notificationRepository.CreateNotification(A<Guid>._, A<NotificationTypeId>._, A<bool>._, A<Action<Notification>>._))
            .Invokes((Guid receiverUserId, NotificationTypeId notificationTypeId, bool isRead, Action<Notification>? setOptionalParameters) =>
            {
                var notification = new Notification(Guid.NewGuid(), receiverUserId, DateTimeOffset.UtcNow, notificationTypeId, isRead);
                setOptionalParameters?.Invoke(notification);
                notifications.Add(notification);
            });
        A.CallTo(() => _offerSubscriptionsRepository.AttachAndModifyAppSubscriptionDetail(detailId, subscriptionId, A<Action<AppSubscriptionDetail>>._, A<Action<AppSubscriptionDetail>>._))
            .Invokes((Guid _, Guid _, Action<AppSubscriptionDetail>? initialize, Action<AppSubscriptionDetail> setParameters) =>
            {
                initialize?.Invoke(details);
                setParameters.Invoke(details);
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
        await _sut.UpdateTenantUrlAsync(appId, subscriptionId, data);

        // Assert
        A.CallTo(() => _provisioningManager.UpdateClient(clientClientId, data.Url, A<string>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _notificationService.CreateNotifications(A<IEnumerable<UserRoleConfig>>._, A<Guid?>._, A<IEnumerable<(string? content, NotificationTypeId notificationTypeId)>>._, A<Guid>._, A<bool?>._)).MustNotHaveHappened();
        A.CallTo(() => _offerRepository.AttachAndModifyOffer(appId, A<Action<Offer>>._, A<Action<Offer>?>._)).MustHaveHappenedOnceExactly();
        notifications.Should().ContainSingle().Which
            .NotificationTypeId.Should().Be(NotificationTypeId.SUBSCRIPTION_URL_UPDATE);
        details.AppSubscriptionUrl.Should().Be(data.Url);
    }

    [Fact]
    public async Task UpdateTenantUrlAsync_WithoutRequesterButValidData_CallsExpected()
    {
        // Arrange
        const string clientClientId = "sa123";
        const string oldUrl = "https://old-url.com";
        var data = new UpdateTenantData("https://new-url.com");
        var appId = _fixture.Create<Guid>();
        var subscriptionId = _fixture.Create<Guid>();
        var subscribingCompany = _fixture.Create<Guid>();
        var detailId = _fixture.Create<Guid>();
        var notifications = new List<Notification>();
        var details = new AppSubscriptionDetail(detailId, subscriptionId)
        {
            AppSubscriptionUrl = oldUrl
        };
        A.CallTo(() => _offerSubscriptionsRepository.GetUpdateUrlDataAsync(appId, subscriptionId, _identity.CompanyId))
            .Returns(new OfferUpdateUrlData("testApp", false, true, Guid.Empty, subscribingCompany, OfferSubscriptionStatusId.ACTIVE, new OfferUpdateUrlSubscriptionDetailData(detailId, clientClientId, oldUrl)));
        A.CallTo(() => _notificationRepository.CreateNotification(A<Guid>._, A<NotificationTypeId>._, A<bool>._, A<Action<Notification>>._))
            .Invokes((Guid receiverUserId, NotificationTypeId notificationTypeId, bool isRead, Action<Notification>? setOptionalParameters) =>
            {
                var notification = new Notification(Guid.NewGuid(), receiverUserId, DateTimeOffset.UtcNow, notificationTypeId, isRead);
                setOptionalParameters?.Invoke(notification);
                notifications.Add(notification);
            });

        A.CallTo(() => _notificationService.CreateNotifications(A<IEnumerable<UserRoleConfig>>._, null, A<IEnumerable<(string? content, NotificationTypeId notificationTypeId)>>._, A<Guid>._, A<bool?>._))
            .Returns(_fixture.CreateMany<Guid>(4).AsFakeIAsyncEnumerable(out var createNotificationsResultAsyncEnumerator));
        A.CallTo(() => _offerSubscriptionsRepository.AttachAndModifyAppSubscriptionDetail(detailId, subscriptionId, A<Action<AppSubscriptionDetail>>._, A<Action<AppSubscriptionDetail>>._))
            .Invokes((Guid _, Guid _, Action<AppSubscriptionDetail>? initialize, Action<AppSubscriptionDetail> setParameters) =>
            {
                initialize?.Invoke(details);
                setParameters.Invoke(details);
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
        await _sut.UpdateTenantUrlAsync(appId, subscriptionId, data);

        // Assert
        A.CallTo(() => _provisioningManager.UpdateClient(clientClientId, data.Url, A<string>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => createNotificationsResultAsyncEnumerator.MoveNextAsync())
            .MustHaveHappened(5, Times.Exactly);
        A.CallTo(() => _offerRepository.AttachAndModifyOffer(appId, A<Action<Offer>>._, A<Action<Offer>?>._)).MustHaveHappenedOnceExactly();
        notifications.Should().BeEmpty();
        details.AppSubscriptionUrl.Should().Be(data.Url);
    }

    [Fact]
    public async Task UpdateTenantUrlAsync_WithoutClientId_CallsExpected()
    {
        // Arrange
        const string oldUrl = "https://old-url.com";
        const string clientClientId = "sa123";
        var data = new UpdateTenantData("https://new-url.com");
        var appId = _fixture.Create<Guid>();
        var subscriptionId = _fixture.Create<Guid>();
        var subscribingCompany = _fixture.Create<Guid>();
        var detailId = _fixture.Create<Guid>();
        var notifications = new List<Notification>();
        var details = new AppSubscriptionDetail(detailId, subscriptionId)
        {
            AppSubscriptionUrl = oldUrl
        };
        A.CallTo(() => _offerSubscriptionsRepository.GetUpdateUrlDataAsync(appId, subscriptionId, _identity.CompanyId))
            .Returns(new OfferUpdateUrlData("testApp", false, true, Guid.Empty, subscribingCompany, OfferSubscriptionStatusId.ACTIVE, new OfferUpdateUrlSubscriptionDetailData(detailId, null, oldUrl)));
        A.CallTo(() => _notificationRepository.CreateNotification(A<Guid>._, A<NotificationTypeId>._, A<bool>._, A<Action<Notification>>._))
            .Invokes((Guid receiverUserId, NotificationTypeId notificationTypeId, bool isRead, Action<Notification>? setOptionalParameters) =>
            {
                var notification = new Notification(Guid.NewGuid(), receiverUserId, DateTimeOffset.UtcNow, notificationTypeId, isRead);
                setOptionalParameters?.Invoke(notification);
                notifications.Add(notification);
            });
        A.CallTo(() => _offerSubscriptionsRepository.AttachAndModifyAppSubscriptionDetail(detailId, subscriptionId, A<Action<AppSubscriptionDetail>>._, A<Action<AppSubscriptionDetail>>._))
            .Invokes((Guid _, Guid _, Action<AppSubscriptionDetail>? initialize, Action<AppSubscriptionDetail> setParameters) =>
            {
                initialize?.Invoke(details);
                setParameters.Invoke(details);
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
        await _sut.UpdateTenantUrlAsync(appId, subscriptionId, data);

        // Assert
        A.CallTo(() => _provisioningManager.UpdateClient(clientClientId, data.Url, A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _notificationService.CreateNotifications(A<IEnumerable<UserRoleConfig>>._, A<Guid?>._, A<IEnumerable<(string? content, NotificationTypeId notificationTypeId)>>._, A<Guid>._, A<bool?>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.AttachAndModifyOffer(appId, A<Action<Offer>>._, A<Action<Offer>?>._)).MustHaveHappenedOnceExactly();
        notifications.Should().BeEmpty();
        details.AppSubscriptionUrl.Should().Be(data.Url);
    }

    [Fact]
    public async Task UpdateTenantUrlAsync_WithSameUrl_CallsNothing()
    {
        // Arrange
        const string clientClientId = "sa123";
        const string oldUrl = "https://old-url.com";
        var data = new UpdateTenantData(oldUrl);
        var appId = _fixture.Create<Guid>();
        var subscriptionId = _fixture.Create<Guid>();
        var subscribingCompany = _fixture.Create<Guid>();
        var detailId = _fixture.Create<Guid>();
        A.CallTo(() => _offerSubscriptionsRepository.GetUpdateUrlDataAsync(appId, subscriptionId, _identity.CompanyId))
            .Returns(new OfferUpdateUrlData("testApp", false, true, Guid.Empty, subscribingCompany, OfferSubscriptionStatusId.ACTIVE, new OfferUpdateUrlSubscriptionDetailData(detailId, clientClientId, oldUrl)));
        // Act
        await _sut.UpdateTenantUrlAsync(appId, subscriptionId, data);

        // Assert
        A.CallTo(() => _provisioningManager.UpdateClient(clientClientId, oldUrl, A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _notificationRepository.CreateNotification(A<Guid>._, A<NotificationTypeId>._, A<bool>._, A<Action<Notification>>._)).MustNotHaveHappened();
        A.CallTo(() => _notificationService.CreateNotifications(A<IEnumerable<UserRoleConfig>>._, A<Guid?>._, A<IEnumerable<(string? content, NotificationTypeId notificationTypeId)>>._, A<Guid>._, A<bool?>._)).MustNotHaveHappened();
        A.CallTo(() => _offerSubscriptionsRepository.AttachAndModifyAppSubscriptionDetail(detailId, subscriptionId, A<Action<AppSubscriptionDetail>>._, A<Action<AppSubscriptionDetail>>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task UpdateTenantUrlAsync_WithInvalidUrl_ThrowsControllerArgumentException()
    {
        // Arrange
        const string clientClientId = "sa123";
        var data = new UpdateTenantData("https:new-url.com");
        var appId = _fixture.Create<Guid>();
        var subscriptionId = _fixture.Create<Guid>();

        // Act
        async Task Act() => await _sut.UpdateTenantUrlAsync(appId, subscriptionId, data);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.ParamName.Should().Be("Url");
        ex.Message.Should().Be($"url {data.Url} cannot be parsed: Invalid URI: The Authority/Host could not be parsed. (Parameter 'Url')");
        A.CallTo(() => _provisioningManager.UpdateClient(clientClientId, A<string>._, A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _notificationRepository.CreateNotification(A<Guid>._, A<NotificationTypeId>._, A<bool>._, A<Action<Notification>>._)).MustNotHaveHappened();
        A.CallTo(() => _notificationService.CreateNotifications(A<IEnumerable<UserRoleConfig>>._, A<Guid?>._, A<IEnumerable<(string? content, NotificationTypeId notificationTypeId)>>._, A<Guid>._, A<bool?>._)).MustNotHaveHappened();
        A.CallTo(() => _offerSubscriptionsRepository.AttachAndModifyAppSubscriptionDetail(A<Guid>._, subscriptionId, A<Action<AppSubscriptionDetail>>._, A<Action<AppSubscriptionDetail>>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task UpdateTenantUrlAsync_WithoutApp_ThrowsNotFoundException()
    {
        // Arrange
        const string clientClientId = "sa123";
        var data = new UpdateTenantData("https://new-url.com");
        var appId = _fixture.Create<Guid>();
        var subscriptionId = _fixture.Create<Guid>();
        A.CallTo(() => _offerSubscriptionsRepository.GetUpdateUrlDataAsync(appId, subscriptionId, _identity.CompanyId))
            .Returns<OfferUpdateUrlData?>(null);

        // Act
        async Task Act() => await _sut.UpdateTenantUrlAsync(appId, subscriptionId, data);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"Offer {appId} or subscription {subscriptionId} do not exists");
        A.CallTo(() => _provisioningManager.UpdateClient(clientClientId, A<string>._, A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _notificationRepository.CreateNotification(A<Guid>._, A<NotificationTypeId>._, A<bool>._, A<Action<Notification>>._)).MustNotHaveHappened();
        A.CallTo(() => _notificationService.CreateNotifications(A<IEnumerable<UserRoleConfig>>._, A<Guid?>._, A<IEnumerable<(string? content, NotificationTypeId notificationTypeId)>>._, A<Guid>._, A<bool?>._)).MustNotHaveHappened();
        A.CallTo(() => _offerSubscriptionsRepository.AttachAndModifyAppSubscriptionDetail(A<Guid>._, subscriptionId, A<Action<AppSubscriptionDetail>>._, A<Action<AppSubscriptionDetail>>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task UpdateTenantUrlAsync_WithSingleInstanceApp_ThrowsConflictException()
    {
        // Arrange
        const string clientClientId = "sa123";
        const string oldUrl = "https://old-url.com";
        var data = new UpdateTenantData("https://new-url.com");
        var appId = _fixture.Create<Guid>();
        var subscriptionId = _fixture.Create<Guid>();
        var subscribingCompany = _fixture.Create<Guid>();
        var detailId = _fixture.Create<Guid>();
        A.CallTo(() => _offerSubscriptionsRepository.GetUpdateUrlDataAsync(appId, subscriptionId, _identity.CompanyId))
            .Returns(new OfferUpdateUrlData("testApp", true, true, Guid.Empty, subscribingCompany, OfferSubscriptionStatusId.ACTIVE, new OfferUpdateUrlSubscriptionDetailData(detailId, clientClientId, oldUrl)));

        // Act
        async Task Act() => await _sut.UpdateTenantUrlAsync(appId, subscriptionId, data);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("Subscription url of single instance apps can't be changed");
        A.CallTo(() => _provisioningManager.UpdateClient(clientClientId, A<string>._, A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _notificationRepository.CreateNotification(A<Guid>._, A<NotificationTypeId>._, A<bool>._, A<Action<Notification>>._)).MustNotHaveHappened();
        A.CallTo(() => _notificationService.CreateNotifications(A<IEnumerable<UserRoleConfig>>._, A<Guid?>._, A<IEnumerable<(string? content, NotificationTypeId notificationTypeId)>>._, A<Guid>._, A<bool?>._)).MustNotHaveHappened();
        A.CallTo(() => _offerSubscriptionsRepository.AttachAndModifyAppSubscriptionDetail(A<Guid>._, subscriptionId, A<Action<AppSubscriptionDetail>>._, A<Action<AppSubscriptionDetail>>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task UpdateTenantUrlAsync_WithUserNotFromProvidingCompany_ThrowsForbiddenException()
    {
        // Arrange
        const string clientClientId = "sa123";
        const string oldUrl = "https://old-url.com";
        var data = new UpdateTenantData("https://new-url.com");
        var appId = _fixture.Create<Guid>();
        var subscriptionId = _fixture.Create<Guid>();
        var subscribingCompany = _fixture.Create<Guid>();
        var detailId = _fixture.Create<Guid>();
        A.CallTo(() => _offerSubscriptionsRepository.GetUpdateUrlDataAsync(appId, subscriptionId, _identity.CompanyId))
            .Returns(new OfferUpdateUrlData("testApp", false, false, Guid.Empty, subscribingCompany, OfferSubscriptionStatusId.ACTIVE, new OfferUpdateUrlSubscriptionDetailData(detailId, clientClientId, oldUrl)));

        // Act
        async Task Act() => await _sut.UpdateTenantUrlAsync(appId, subscriptionId, data);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be($"Company {_identity.CompanyId} is not the app's providing company");
        A.CallTo(() => _provisioningManager.UpdateClient(clientClientId, A<string>._, A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _notificationRepository.CreateNotification(A<Guid>._, A<NotificationTypeId>._, A<bool>._, A<Action<Notification>>._)).MustNotHaveHappened();
        A.CallTo(() => _notificationService.CreateNotifications(A<IEnumerable<UserRoleConfig>>._, A<Guid?>._, A<IEnumerable<(string? content, NotificationTypeId notificationTypeId)>>._, A<Guid>._, A<bool?>._)).MustNotHaveHappened();
        A.CallTo(() => _offerSubscriptionsRepository.AttachAndModifyAppSubscriptionDetail(A<Guid>._, subscriptionId, A<Action<AppSubscriptionDetail>>._, A<Action<AppSubscriptionDetail>>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task UpdateTenantUrlAsync_WithInActiveApp_ThrowsConflictException()
    {
        // Arrange
        const string clientClientId = "sa123";
        const string oldUrl = "https://old-url.com";
        var data = new UpdateTenantData("https://new-url.com");
        var appId = _fixture.Create<Guid>();
        var subscriptionId = _fixture.Create<Guid>();
        var subscribingCompany = _fixture.Create<Guid>();
        var detailId = _fixture.Create<Guid>();
        A.CallTo(() => _offerSubscriptionsRepository.GetUpdateUrlDataAsync(appId, subscriptionId, _identity.CompanyId))
            .Returns(new OfferUpdateUrlData("testApp", false, true, Guid.Empty, subscribingCompany, OfferSubscriptionStatusId.PENDING, new OfferUpdateUrlSubscriptionDetailData(detailId, clientClientId, oldUrl)));

        // Act
        async Task Act() => await _sut.UpdateTenantUrlAsync(appId, subscriptionId, data);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"Subscription {subscriptionId} must be in status ACTIVE");
        A.CallTo(() => _provisioningManager.UpdateClient(clientClientId, A<string>._, A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _notificationRepository.CreateNotification(A<Guid>._, A<NotificationTypeId>._, A<bool>._, A<Action<Notification>>._)).MustNotHaveHappened();
        A.CallTo(() => _notificationService.CreateNotifications(A<IEnumerable<UserRoleConfig>>._, A<Guid?>._, A<IEnumerable<(string? content, NotificationTypeId notificationTypeId)>>._, A<Guid>._, A<bool?>._)).MustNotHaveHappened();
        A.CallTo(() => _offerSubscriptionsRepository.AttachAndModifyAppSubscriptionDetail(A<Guid>._, subscriptionId, A<Action<AppSubscriptionDetail>>._, A<Action<AppSubscriptionDetail>>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task UpdateTenantUrlAsync_WithoutSubscriptionDetails_ThrowsConflictException()
    {
        // Arrange
        const string clientClientId = "sa123";
        var data = new UpdateTenantData("https://new-url.com");
        var appId = _fixture.Create<Guid>();
        var subscriptionId = _fixture.Create<Guid>();
        var subscribingCompany = _fixture.Create<Guid>();
        A.CallTo(() => _offerSubscriptionsRepository.GetUpdateUrlDataAsync(appId, subscriptionId, _identity.CompanyId))
            .Returns(new OfferUpdateUrlData("testApp", false, true, Guid.Empty, subscribingCompany, OfferSubscriptionStatusId.ACTIVE, null));

        // Act
        async Task Act() => await _sut.UpdateTenantUrlAsync(appId, subscriptionId, data);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"There is no subscription detail data configured for subscription {subscriptionId}");
        A.CallTo(() => _provisioningManager.UpdateClient(clientClientId, A<string>._, A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _notificationRepository.CreateNotification(A<Guid>._, A<NotificationTypeId>._, A<bool>._, A<Action<Notification>>._)).MustNotHaveHappened();
        A.CallTo(() => _notificationService.CreateNotifications(A<IEnumerable<UserRoleConfig>>._, A<Guid?>._, A<IEnumerable<(string? content, NotificationTypeId notificationTypeId)>>._, A<Guid>._, A<bool?>._)).MustNotHaveHappened();
        A.CallTo(() => _offerSubscriptionsRepository.AttachAndModifyAppSubscriptionDetail(A<Guid>._, subscriptionId, A<Action<AppSubscriptionDetail>>._, A<Action<AppSubscriptionDetail>>._)).MustNotHaveHappened();
    }

    #endregion

    #region GetActiveAppDocumentTypeDataAsync

    [Fact]
    public async Task GetActiveAppDocumentTypeDataAsync_ReturnsExpected()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var documentId1 = _fixture.Create<Guid>();
        var documentId2 = _fixture.Create<Guid>();
        var documentId3 = _fixture.Create<Guid>();
        var documentId4 = _fixture.Create<Guid>();
        var documentId5 = _fixture.Create<Guid>();
        var documentData = new[] {
            new DocumentTypeData(DocumentTypeId.ADDITIONAL_DETAILS, documentId1, "TestDoc1"),
            new DocumentTypeData(DocumentTypeId.ADDITIONAL_DETAILS, documentId2, "TestDoc2"),
            new DocumentTypeData(DocumentTypeId.APP_IMAGE, documentId3, "TestDoc3"),
            new DocumentTypeData(DocumentTypeId.APP_IMAGE, documentId4, "TestDoc4"),
            new DocumentTypeData(DocumentTypeId.APP_TECHNICAL_INFORMATION, documentId5, "TestDoc5"),
        }.ToAsyncEnumerable();

        A.CallTo(() => _offerRepository.GetActiveOfferDocumentTypeDataOrderedAsync(A<Guid>._, A<Guid>._, OfferTypeId.APP, A<IEnumerable<DocumentTypeId>>._))
            .Returns(documentData);

        // Act
        var result = await _sut.GetActiveAppDocumentTypeDataAsync(appId);

        // Assert
        A.CallTo(() => _offerRepository.GetActiveOfferDocumentTypeDataOrderedAsync(A<Guid>._, A<Guid>._, OfferTypeId.APP, A<IEnumerable<DocumentTypeId>>._)).MustHaveHappened();
        result.Documents.Should().NotBeNull().And.HaveCount(4).And.Satisfy(
            x => x.Key == DocumentTypeId.APP_IMAGE && x.Value.SequenceEqual(new DocumentData[] { new(documentId3, "TestDoc3"), new(documentId4, "TestDoc4") }),
            x => x.Key == DocumentTypeId.APP_TECHNICAL_INFORMATION && x.Value.SequenceEqual(new DocumentData[] { new(documentId5, "TestDoc5") }),
            x => x.Key == DocumentTypeId.APP_CONTRACT && !x.Value.Any(),
            x => x.Key == DocumentTypeId.ADDITIONAL_DETAILS && x.Value.SequenceEqual(new DocumentData[] { new(documentId1, "TestDoc1"), new(documentId2, "TestDoc2") })
        );
    }

    #endregion

    #region DeleteMulitipleActiveAppDocuments

    [Fact]
    public async Task DeleteActiveAppDocumentAsync_ReturnsExpected()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var documentId = _fixture.Create<Guid>();

        A.CallTo(() => _offerRepository.GetOfferAssignedAppDocumentsByIdAsync(A<Guid>._, A<Guid>._, OfferTypeId.APP, A<Guid>._))
            .Returns((true, true, DocumentTypeId.APP_CONTRACT, DocumentStatusId.LOCKED));

        var initialDocument = new Document(Guid.Empty, null!, null!, null!, default, default, default, default);
        var modifiedDocument = new Document(Guid.Empty, null!, null!, null!, default, default, default, default);
        A.CallTo(() => _documentRepository.AttachAndModifyDocument(A<Guid>._, A<Action<Document>>._, A<Action<Document>>._))
            .Invokes((Guid docId, Action<Document>? initialize, Action<Document> modify)
                =>
            {
                initialize?.Invoke(initialDocument);
                modify(modifiedDocument);
            });
        var existingOffer = new Offer(Guid.Empty, null!, default, default);
        A.CallTo(() => _offerRepository.AttachAndModifyOffer(A<Guid>._, A<Action<Offer>>._, A<Action<Offer>?>._))
            .Invokes((Guid appId, Action<Offer> setOptionalParameters, Action<Offer>? initializeParemeters) =>
            {
                initializeParemeters?.Invoke(existingOffer);
                setOptionalParameters(existingOffer);
            });

        // Act
        await _sut.DeleteActiveAppDocumentAsync(appId, documentId);

        // Assert
        A.CallTo(() => _offerRepository.GetOfferAssignedAppDocumentsByIdAsync(appId, _identity.CompanyId, OfferTypeId.APP, documentId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.RemoveOfferAssignedDocument(appId, documentId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _documentRepository.AttachAndModifyDocument(documentId, A<Action<Document>>._, A<Action<Document>>._)).MustHaveHappenedOnceExactly();
        initialDocument.DocumentStatusId.Should().Be(DocumentStatusId.LOCKED);
        modifiedDocument.DocumentStatusId.Should().Be(DocumentStatusId.INACTIVE);
        A.CallTo(() => _offerRepository.AttachAndModifyOffer(appId, A<Action<Offer>>._, null)).MustHaveHappenedOnceExactly();
        existingOffer.DateLastChanged.Should().Be(_now);
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DeleteActiveAppDocumentAsync_DefaultQueryResult_Throws_NotFoundException()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var documentId = _fixture.Create<Guid>();
        A.CallTo(() => _offerRepository.GetOfferAssignedAppDocumentsByIdAsync(A<Guid>._, A<Guid>._, OfferTypeId.APP, A<Guid>._))
            .Returns<(bool, bool, DocumentTypeId, DocumentStatusId)>(default);

        // Act
        async Task Act() => await _sut.DeleteActiveAppDocumentAsync(appId, documentId);

        // Assert
        var result = await Assert.ThrowsAsync<NotFoundException>(Act);
        result.Message.Should().Be($"Document {documentId} for App {appId} does not exist.");
        A.CallTo(() => _offerRepository.GetOfferAssignedAppDocumentsByIdAsync(appId, _identity.CompanyId, OfferTypeId.APP, documentId))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
    }

    [Fact]
    public async Task DeleteActiveAppDocumentAsync_IncorrectOfferState_Throws_ConflictException()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var documentId = _fixture.Create<Guid>();
        A.CallTo(() => _offerRepository.GetOfferAssignedAppDocumentsByIdAsync(A<Guid>._, A<Guid>._, OfferTypeId.APP, A<Guid>._))
            .Returns((false, true, DocumentTypeId.APP_CONTRACT, DocumentStatusId.LOCKED));

        // Act
        async Task Act() => await _sut.DeleteActiveAppDocumentAsync(appId, documentId);

        // Assert
        var result = await Assert.ThrowsAsync<ConflictException>(Act);
        result.Message.Should().Be("offerStatus is in incorrect State");
        A.CallTo(() => _offerRepository.GetOfferAssignedAppDocumentsByIdAsync(appId, _identity.CompanyId, OfferTypeId.APP, documentId))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
    }

    [Fact]
    public async Task DeleteActiveAppDocumentAsync_InvalidDocumentType_Throws_ConflictException()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var documentId = _fixture.Create<Guid>();
        A.CallTo(() => _offerRepository.GetOfferAssignedAppDocumentsByIdAsync(A<Guid>._, A<Guid>._, OfferTypeId.APP, A<Guid>._))
            .Returns((true, true, DocumentTypeId.CX_FRAME_CONTRACT, DocumentStatusId.LOCKED));

        // Act
        async Task Act() => await _sut.DeleteActiveAppDocumentAsync(appId, documentId);

        // Assert
        var result = await Assert.ThrowsAsync<ConflictException>(Act);
        result.Message.Should().Be($"document {documentId} does not have a valid documentType");
        A.CallTo(() => _offerRepository.GetOfferAssignedAppDocumentsByIdAsync(appId, _identity.CompanyId, OfferTypeId.APP, documentId))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
    }

    [Fact]
    public async Task DeleteActiveAppDocumentAsync_NotProviderCompany_Throws_ForbiddenException()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var documentId = _fixture.Create<Guid>();
        A.CallTo(() => _offerRepository.GetOfferAssignedAppDocumentsByIdAsync(A<Guid>._, A<Guid>._, OfferTypeId.APP, A<Guid>._))
            .Returns((true, false, DocumentTypeId.APP_CONTRACT, DocumentStatusId.LOCKED));

        // Act
        async Task Act() => await _sut.DeleteActiveAppDocumentAsync(appId, documentId);

        // Assert
        var result = await Assert.ThrowsAsync<ForbiddenException>(Act);
        result.Message.Should().Be($"Company {_identity.CompanyId} is not the provider company of App {appId}");
        A.CallTo(() => _offerRepository.GetOfferAssignedAppDocumentsByIdAsync(appId, _identity.CompanyId, OfferTypeId.APP, documentId))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
    }

    #endregion

    #region CreateActiveAppDocument

    [Fact]
    public async Task CreateActiveAppDocumentAsync_ReturnsExpected()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var file = FormFileHelper.GetFormFile("Test Image1", "TestImage1.jpeg", "image/jpeg");
        var documentTypeId = DocumentTypeId.APP_IMAGE;

        // Act
        await _sut.CreateActiveAppDocumentAsync(appId, documentTypeId, file, CancellationToken.None);

        // Assert
        A.CallTo(() => _offerDocumentService.UploadDocumentAsync(appId, documentTypeId, file, OfferTypeId.APP, A<IEnumerable<UploadDocumentConfig>>._, OfferStatusId.ACTIVE, CancellationToken.None)).MustHaveHappenedOnceExactly();
    }

    #endregion

    #region GetActiveAppRoles

    [Fact]
    public async Task GetActiveAppRolesAsync_Throws_NotFoundException()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var activeAppRoleDetails = default((bool, bool, IEnumerable<ActiveAppRoleDetails>));
        A.CallTo(() => _userRolesRepository.GetActiveAppRolesAsync(A<Guid>._, A<OfferTypeId>._, A<string>._, A<string>._))
            .Returns(activeAppRoleDetails);

        // Act
        Task Act() => _sut.GetActiveAppRolesAsync(appId, null);

        // Assert
        var result = await Assert.ThrowsAsync<NotFoundException>(Act);
        result.Message.Should().Be($"App {appId} does not exist");
        A.CallTo(() => _userRolesRepository.GetActiveAppRolesAsync(appId, OfferTypeId.APP, null, "en"))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetActiveAppRolesAsync_Throws_ConflictException()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var activeAppRoleDetails = (true, false, _fixture.CreateMany<ActiveAppRoleDetails>());
        A.CallTo(() => _userRolesRepository.GetActiveAppRolesAsync(A<Guid>._, A<OfferTypeId>._, A<string>._, A<string>._))
            .Returns(activeAppRoleDetails);

        // Act
        Task Act() => _sut.GetActiveAppRolesAsync(appId, "de");

        // Assert
        var result = await Assert.ThrowsAsync<ConflictException>(Act);
        result.Message.Should().Be($"App {appId} is not Active");
        A.CallTo(() => _userRolesRepository.GetActiveAppRolesAsync(appId, OfferTypeId.APP, "de", "en"))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetActiveAppRolesAsync_ReturnsExpected()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var userRole1 = new ActiveAppRoleDetails("TestRole1", [
            new ActiveAppUserRoleDescription("en", "TestRole1 description")
        ]);
        var userRole2 = new ActiveAppRoleDetails("TestRole2", [
            new ActiveAppUserRoleDescription("en", "TestRole2 description")
        ]);
        var activeAppRoleDetails = (true, true, new[] {
            userRole1,
            userRole2
        });

        A.CallTo(() => _userRolesRepository.GetActiveAppRolesAsync(A<Guid>._, A<OfferTypeId>._, A<string>._, A<string>._))
            .Returns(activeAppRoleDetails);

        // Act
        var result = await _sut.GetActiveAppRolesAsync(appId, "de");

        // Assert
        result.Should().HaveCount(2)
            .And.Satisfy(
                x => x.Role == "TestRole1" && x.Descriptions.Count() == 1 && x.Descriptions.Single().Description == "TestRole1 description",
                x => x.Role == "TestRole2" && x.Descriptions.Count() == 1 && x.Descriptions.Single().Description == "TestRole2 description");
        A.CallTo(() => _userRolesRepository.GetActiveAppRolesAsync(appId, OfferTypeId.APP, "de", "en"))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

}
