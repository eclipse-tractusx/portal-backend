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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Web;
using Org.Eclipse.TractusX.Portal.Backend.Notifications.Library;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Service;
using System.Collections.Immutable;
using Xunit;

namespace Org.Eclipse.TractusX.Portal.Backend.Apps.Service.BusinessLogic.Tests;

public class AppChangeBusinessLogicTest
{
    private const string ClientId = "catenax-portal";
    private readonly Guid _companyUserId = Guid.NewGuid();
    private readonly string _iamUserId = Guid.NewGuid().ToString();

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
    private readonly AppChangeBusinessLogic _sut;

    public AppChangeBusinessLogicTest()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _provisioningManager = A.Fake<IProvisioningManager>();
        _portalRepositories = A.Fake<IPortalRepositories>();
        _notificationRepository = A.Fake<INotificationRepository>();
        _offerRepository = A.Fake<IOfferRepository>();
        _offerSubscriptionsRepository = A.Fake<IOfferSubscriptionsRepository>();
        _userRolesRepository = A.Fake<IUserRolesRepository>();
        _documentRepository = A.Fake<IDocumentRepository>();
        _notificationService = A.Fake<INotificationService>();
        _offerService = A.Fake<IOfferService>();

        var settings = new AppsSettings
        {
            ActiveAppNotificationTypeIds = new []
            {
                NotificationTypeId.APP_ROLE_ADDED
            },
            ActiveAppCompanyAdminRoles = new Dictionary<string, IEnumerable<string>>
            {
                { ClientId, new [] { "Company Admin" } }
            },
            CompanyAdminRoles = new Dictionary<string, IEnumerable<string>>
            {
                { ClientId, new [] { "Company Admin" } }
            }
        };
        A.CallTo(() => _portalRepositories.GetInstance<INotificationRepository>()).Returns(_notificationRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IOfferRepository>()).Returns(_offerRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IOfferSubscriptionsRepository>()).Returns(_offerSubscriptionsRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRolesRepository>()).Returns(_userRolesRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IDocumentRepository>()).Returns(_documentRepository);
        _sut = new AppChangeBusinessLogic(_portalRepositories, _notificationService, _provisioningManager, _offerService, Options.Create(settings));
    }

    #region  AddActiveAppUserRole

    [Fact]
    public async Task AddActiveAppUserRoleAsync_ExecutesSuccessfully()
    {
        //Arrange
        var appId = _fixture.Create<Guid>();
        var companyId = _fixture.Create<Guid>();
        var appName = _fixture.Create<string>();
        var appAssignedRoleDesc = _fixture.CreateMany<string>(3).Select(role => new AppUserRole(role, _fixture.CreateMany<AppUserRoleDescription>(2).ToImmutableArray())).ToImmutableArray();
        var clientIds = new[] {"client"};

        A.CallTo(() => _portalRepositories.GetInstance<IOfferRepository>().GetInsertActiveAppUserRoleDataAsync(appId, _iamUserId, OfferTypeId.APP))
            .Returns((true, appName, _companyUserId, companyId, clientIds));

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

        A.CallTo(() => _notificationService.CreateNotifications(A<IDictionary<string, IEnumerable<string>>>._, A<Guid>._, A<IEnumerable<(string? content, NotificationTypeId notificationTypeId)>>._, A<Guid>._, A<bool?>._))
            .Returns(_fixture.CreateMany<Guid>(4).AsFakeIAsyncEnumerable(out var createNotificationsResultAsyncEnumerator));

        //Act
        var result = await _sut.AddActiveAppUserRoleAsync(appId, appAssignedRoleDesc, _iamUserId).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _offerRepository.GetInsertActiveAppUserRoleDataAsync(appId, _iamUserId, OfferTypeId.APP)).MustHaveHappened();

        A.CallTo(() => _userRolesRepository.CreateAppUserRoles(A<IEnumerable<(Guid,string)>>._)).MustHaveHappenedOnceExactly();
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

        A.CallTo(() => _userRolesRepository.CreateAppUserRoleDescriptions(A<IEnumerable<(Guid,string,string)>>._)).MustHaveHappened(appAssignedRoleDesc.Length, Times.Exactly);
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

        A.CallTo(() => _notificationService.CreateNotifications(A<IDictionary<string, IEnumerable<string>>>._, A<Guid>._, A<IEnumerable<(string? content, NotificationTypeId notificationTypeId)>>._, A<Guid>._, A<bool?>._))
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
        var clientIds = new[] {"client"};
        A.CallTo(() => _offerRepository.GetInsertActiveAppUserRoleDataAsync(appId, _iamUserId, OfferTypeId.APP))
            .Returns((true, appName, Guid.Empty, null, clientIds));

        //Act
        async Task Act() => await _sut.AddActiveAppUserRoleAsync(appId, appAssignedRoleDesc, _iamUserId).ConfigureAwait(false);

        //Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be($"user {_iamUserId} is not a member of the provider company of app {appId}");
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
        var clientIds = new[] {"client"};
        A.CallTo(() => _offerRepository.GetInsertActiveAppUserRoleDataAsync(appId, _iamUserId, OfferTypeId.APP))
            .Returns((true, appName, _companyUserId, null, clientIds));

        //Act
        async Task Act() => await _sut.AddActiveAppUserRoleAsync(appId, appAssignedRoleDesc, _iamUserId).ConfigureAwait(false);

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
        var appDescriptionData = (IsStatusActive: true, IsProviderCompanyUser: true, OfferDescriptionDatas: offerDescription);

        A.CallTo(() => _offerRepository.GetActiveOfferDescriptionDataByIdAsync(appId, OfferTypeId.APP, _iamUserId))
            .Returns(appDescriptionData);
        
        // Act
        var result = await _sut.GetAppUpdateDescriptionByIdAsync(appId, _iamUserId).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.Select(x => x.LanguageCode).Should().Contain(offerDescription.Select(od => od.LanguageCode));
    }

    [Fact]    
    public async Task GetAppUpdateDescriptionByIdAsync_ThrowsNotFoundException()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();

        A.CallTo(() => _offerRepository.GetActiveOfferDescriptionDataByIdAsync(appId, OfferTypeId.APP, _iamUserId))
            .Returns(((bool IsStatusActive, bool IsProviderCompanyUser, IEnumerable<LocalizedDescription> OfferDescriptionDatas))default);
        
        // Act
        async Task Act() => await _sut.GetAppUpdateDescriptionByIdAsync(appId, _iamUserId).ConfigureAwait(false);

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
        var appDescriptionData = (IsStatusActive: false, IsProviderCompanyUser: true, OfferDescriptionDatas: offerDescription);

        A.CallTo(() => _offerRepository.GetActiveOfferDescriptionDataByIdAsync(appId, OfferTypeId.APP, _iamUserId))
            .Returns(appDescriptionData);
        
        // Act
        async Task Act() => await _sut.GetAppUpdateDescriptionByIdAsync(appId, _iamUserId).ConfigureAwait(false);

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
        var appDescriptionData = (IsStatusActive: true, IsProviderCompanyUser: false, OfferDescriptionDatas: offerDescription);

        A.CallTo(() => _offerRepository.GetActiveOfferDescriptionDataByIdAsync(appId, OfferTypeId.APP, _iamUserId))
            .Returns(appDescriptionData);
        
        // Act
        async Task Act() => await _sut.GetAppUpdateDescriptionByIdAsync(appId, _iamUserId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be($"user {_iamUserId} is not a member of the providercompany of App {appId}");  
    }

    [Fact]
    public async Task GetAppUpdateDescriptionByIdAsync_withNullDescriptionData_ThrowsUnexpectedConditionException()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var appDescriptionData = (IsStatusActive: true, IsProviderCompanyUser: true, OfferDescriptionDatas: (IEnumerable<LocalizedDescription>?)null);
        A.CallTo(() => _offerRepository.GetActiveOfferDescriptionDataByIdAsync(appId, OfferTypeId.APP, _iamUserId))
            .Returns(appDescriptionData);

        // Act
        var Act = () => _sut.GetAppUpdateDescriptionByIdAsync(appId, _iamUserId);

        // Assert
        var result = await Assert.ThrowsAsync<UnexpectedConditionException>(Act).ConfigureAwait(false);
        result.Message.Should().Be("offerDescriptionDatas should never be null here");

    }

    #endregion
    
    #region Add / Update App Description
    
    [Fact]
    public async Task CreateOrUpdateAppDescriptionByIdAsync_withEmptyExistingDescriptionData_ReturnExpectedResult()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var updateDescriptionData =  new []{
            new LocalizedDescription("en", _fixture.Create<string>(), _fixture.Create<string>()),
            new LocalizedDescription("de", _fixture.Create<string>(), _fixture.Create<string>())
            };
        var appDescriptionData = (IsStatusActive: true, IsProviderCompanyUser: true, OfferDescriptionDatas: (IEnumerable<LocalizedDescription>)updateDescriptionData);

        A.CallTo(() => _offerRepository.GetActiveOfferDescriptionDataByIdAsync(appId, OfferTypeId.APP, _iamUserId))
            .Returns(appDescriptionData);

        // Act
        await _sut.CreateOrUpdateAppDescriptionByIdAsync(appId, _iamUserId, updateDescriptionData).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _offerRepository.CreateUpdateDeleteOfferDescriptions(appId, A<IEnumerable<LocalizedDescription>>._, A<IEnumerable<(string,string,string)>>.That.IsSameSequenceAs(updateDescriptionData.Select(x => new ValueTuple<string,string,string>(x.LanguageCode, x.LongDescription, x.ShortDescription)))))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CreateOrUpdateAppDescriptionByIdAsync_withNullDescriptionData_ThowsUnexpectedConditionException()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var updateDescriptionData =  new []{
            new LocalizedDescription("en", _fixture.Create<string>(), _fixture.Create<string>()),
            new LocalizedDescription("de", _fixture.Create<string>(), _fixture.Create<string>())
            };
        var appDescriptionData = (IsStatusActive: true, IsProviderCompanyUser: true, OfferDescriptionDatas: (IEnumerable<LocalizedDescription>)null!);

        A.CallTo(() => _offerRepository.GetActiveOfferDescriptionDataByIdAsync(appId, OfferTypeId.APP, _iamUserId))
            .Returns(appDescriptionData);

        // Act
        var Act = () => _sut.CreateOrUpdateAppDescriptionByIdAsync(appId, _iamUserId, updateDescriptionData);

        // Assert
        var result = await Assert.ThrowsAsync<UnexpectedConditionException>(Act).ConfigureAwait(false);
        result.Message.Should().Be("offerDescriptionDatas should never be null here");

    }

    [Fact]
    public async Task CreateOrUpdateAppDescriptionByIdAsync_ThrowsForbiddenException()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var updateDescriptionData =  new []{
            new LocalizedDescription("en", _fixture.Create<string>(), _fixture.Create<string>()),
            new LocalizedDescription("de", _fixture.Create<string>(), _fixture.Create<string>())
            };
        var appDescriptionData = (IsStatusActive: true, IsProviderCompanyUser: false, OfferDescriptionDatas: (IEnumerable<LocalizedDescription>)null!);

        A.CallTo(() => _offerRepository.GetActiveOfferDescriptionDataByIdAsync(appId, OfferTypeId.APP, _iamUserId))
            .Returns(appDescriptionData);

        // Act
        var Act = () => _sut.CreateOrUpdateAppDescriptionByIdAsync(appId, _iamUserId, updateDescriptionData);

        // Assert
        var result = await Assert.ThrowsAsync<ForbiddenException>(Act).ConfigureAwait(false);
        result.Message.Should().Be($"user {_iamUserId} is not a member of the providercompany of App {appId}");
        
    }

    [Fact]
    public async Task CreateOrUpdateAppDescriptionByIdAsync_ThrowsConflictException()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var updateDescriptionData =  new []{
            new LocalizedDescription("en", _fixture.Create<string>(), _fixture.Create<string>()),
            new LocalizedDescription("de", _fixture.Create<string>(), _fixture.Create<string>())
            };
        var appDescriptionData = (IsStatusActive: false, IsProviderCompanyUser: true, OfferDescriptionDatas: (IEnumerable<LocalizedDescription>)null!);

        A.CallTo(() => _offerRepository.GetActiveOfferDescriptionDataByIdAsync(appId, OfferTypeId.APP, _iamUserId))
            .Returns(appDescriptionData);

        // Act
        var Act = () => _sut.CreateOrUpdateAppDescriptionByIdAsync(appId, _iamUserId, updateDescriptionData);

        // Assert
        var result = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);
        result.Message.Should().Be($"App {appId} is in InCorrect Status");
        
    }

     [Fact]
    public async Task CreateOrUpdateAppDescriptionByIdAsync_ThrowsNotFoundException()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var updateDescriptionData =  new []{
            new LocalizedDescription("en", _fixture.Create<string>(), _fixture.Create<string>()),
            new LocalizedDescription("de", _fixture.Create<string>(), _fixture.Create<string>())
            };

        A.CallTo(() => _offerRepository.GetActiveOfferDescriptionDataByIdAsync(appId, OfferTypeId.APP, _iamUserId))
             .Returns(((bool IsStatusActive, bool IsProviderCompanyUser, IEnumerable<LocalizedDescription> OfferDescriptionDatas))default);

        // Act
        var Act = () => _sut.CreateOrUpdateAppDescriptionByIdAsync(appId, _iamUserId, updateDescriptionData);

        // Assert
        var result = await Assert.ThrowsAsync<NotFoundException>(Act).ConfigureAwait(false);
        result.Message.Should().Be($"App {appId} does not exist.");
        
    }

    #endregion

    #region  UploadOfferAssignedAppLeadImageDocumentById

    [Fact]
    public async Task UploadOfferAssignedAppLeadImageDocumentById_ExpectedCalls()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var iamUserId = _fixture.Create<Guid>().ToString();
        var documentId = _fixture.Create<Guid>();
        var documentStatusData = _fixture.CreateMany<DocumentStatusData>(2).ToImmutableArray();
        var companyUserId =  _fixture.Create<Guid>();
        var file = FormFileHelper.GetFormFile("Test Image", "TestImage.jpeg", "image/jpeg");
        var documents = new List<Document>();
        var offerAssignedDocuments = new List<OfferAssignedDocument>();

        A.CallTo(() => _offerRepository.GetOfferAssignedAppLeadImageDocumentsByIdAsync(A<Guid>._, A<string>._, A<OfferTypeId>._))
            .Returns((true, companyUserId, documentStatusData));

        A.CallTo(() => _documentRepository.CreateDocument(A<string>._, A<byte[]>._, A<byte[]>._, A<MediaTypeId>._, A<DocumentTypeId>._,A<Action<Document>?>._))
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

        // Act
        await _sut.UploadOfferAssignedAppLeadImageDocumentByIdAsync(appId, iamUserId, file, CancellationToken.None);

        // Assert
        A.CallTo(() => _offerRepository.GetOfferAssignedAppLeadImageDocumentsByIdAsync(appId, iamUserId, OfferTypeId.APP)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _documentRepository.CreateDocument(A<string>._, A<byte[]>._, A<byte[]>._, A<MediaTypeId>._, A<DocumentTypeId>._,A<Action<Document>?>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.CreateOfferAssignedDocument(A<Guid>._, A<Guid>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.RemoveOfferAssignedDocuments(A<IEnumerable<(Guid OfferId, Guid DocumentId)>>.That.IsSameSequenceAs(documentStatusData.Select(data => new ValueTuple<Guid,Guid>(appId, data.DocumentId))))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _documentRepository.RemoveDocuments(A<IEnumerable<Guid>>.That.IsSameSequenceAs(documentStatusData.Select(data => data.DocumentId)))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        documents.Should().ContainSingle().Which.Should().Match<Document>(x => x.Id == documentId && x.MediaTypeId == MediaTypeId.JPEG && x.DocumentTypeId == DocumentTypeId.APP_LEADIMAGE && x.DocumentStatusId == DocumentStatusId.LOCKED);
        offerAssignedDocuments.Should().ContainSingle().Which.Should().Match<OfferAssignedDocument>(x => x.DocumentId == documentId && x.OfferId == appId);
    }

    [Fact]
    public async Task UploadOfferAssignedAppLeadImageDocumentById_ThrowsUnsupportedMediaTypeException()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var iamUserId = _fixture.Create<Guid>().ToString();
        var appLeadImageContentTypes = new [] {MediaTypeId.JPEG,MediaTypeId.PNG};
        var file = FormFileHelper.GetFormFile("Test File", "TestImage.pdf", "application/pdf");

        // Act
        var Act = () => _sut.UploadOfferAssignedAppLeadImageDocumentByIdAsync(appId, iamUserId, file, CancellationToken.None);

        // Assert
        var result = await Assert.ThrowsAsync<UnsupportedMediaTypeException>(Act).ConfigureAwait(false);
        result.Message.Should().Be($"Document type not supported. File with contentType :{string.Join(",", appLeadImageContentTypes)} are allowed.");
    }

    [Fact]
    public async Task UploadOfferAssignedAppLeadImageDocumentById_ThrowsConflictException()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var iamUserId = _fixture.Create<Guid>().ToString();
        var companyUserId = _fixture.Create<Guid>();
        var file = FormFileHelper.GetFormFile("Test Image", "TestImage.jpeg", "image/jpeg");

        A.CallTo(() => _offerRepository.GetOfferAssignedAppLeadImageDocumentsByIdAsync(appId, iamUserId, OfferTypeId.APP))
            .ReturnsLazily(() => (false, companyUserId, null!));

        // Act
        var Act = () => _sut.UploadOfferAssignedAppLeadImageDocumentByIdAsync(appId, iamUserId, file, CancellationToken.None);

        // Assert
        var result = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);
        result.Message.Should().Be("offerStatus is in incorrect State");
    }

    [Fact]
    public async Task UploadOfferAssignedAppLeadImageDocumentById_ThrowsForbiddenException()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var iamUserId = _fixture.Create<Guid>().ToString();
        var companyUserId = _fixture.Create<Guid>();
        var file = FormFileHelper.GetFormFile("Test Image", "TestImage.jpeg", "image/jpeg");

        A.CallTo(() => _offerRepository.GetOfferAssignedAppLeadImageDocumentsByIdAsync(appId, iamUserId, OfferTypeId.APP))
            .ReturnsLazily(() => (true, Guid.Empty, null!));

        // Act
        async Task Act() => await _sut.UploadOfferAssignedAppLeadImageDocumentByIdAsync(appId, iamUserId, file, CancellationToken.None);

        // Assert
        var result = await Assert.ThrowsAsync<ForbiddenException>(Act).ConfigureAwait(false);
        result.Message.Should().Be($"user {iamUserId} is not a member of the provider company of App {appId}");
    }

    [Fact]
    public async Task UploadOfferAssignedAppLeadImageDocumentById_ThrowsNotFoundException()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var iamUserId = _fixture.Create<Guid>().ToString();
        var file = FormFileHelper.GetFormFile("Test Image", "TestImage.jpeg", "image/jpeg");

        A.CallTo(() => _offerRepository.GetOfferAssignedAppLeadImageDocumentsByIdAsync(appId, iamUserId, OfferTypeId.APP))
            .ReturnsLazily(() => new ValueTuple<bool,Guid,IEnumerable<DocumentStatusData>>());

        // Act
        async Task Act() => await _sut.UploadOfferAssignedAppLeadImageDocumentByIdAsync(appId, iamUserId, file, CancellationToken.None);

        // Assert
        var result = await Assert.ThrowsAsync<NotFoundException>(Act).ConfigureAwait(false);
        result.Message.Should().Be($"App {appId} does not exist.");
    }

    #endregion

    #region  DeactivateOfferbyAppId

    [Fact]
    public async Task DeactivateOfferStatusbyAppIdAsync_CallsExpected()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var settings = new AppsSettings
        {
            ServiceManagerRoles = _fixture.Create<Dictionary<string, IEnumerable<string>>>(),
            BasePortalAddress = "test"
        };
        var sut = new AppsBusinessLogic(_portalRepositories, null!, null!, null!, _fixture.Create<IOptions<AppsSettings>>(), null!);
        
        // Act
        await _sut.DeactivateOfferByAppIdAsync(appId, _iamUserId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _offerService.DeactivateOfferIdAsync(appId, _iamUserId, OfferTypeId.APP)).MustHaveHappenedOnceExactly();
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
        A.CallTo(() => _offerSubscriptionsRepository.GetUpdateUrlDataAsync(appId, subscriptionId, _iamUserId))
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

        // Act
        await _sut.UpdateTenantUrlAsync(appId, subscriptionId, data, _iamUserId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _provisioningManager.UpdateClient(clientClientId, data.Url, A<string>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _notificationService.CreateNotifications(A<IDictionary<string, IEnumerable<string>>>._, A<Guid?>._, A<IEnumerable<(string? content, NotificationTypeId notificationTypeId)>>._, A<Guid>._, A<bool?>._)).MustNotHaveHappened();
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
        A.CallTo(() => _offerSubscriptionsRepository.GetUpdateUrlDataAsync(appId, subscriptionId, _iamUserId))
            .Returns(new OfferUpdateUrlData("testApp", false, true, Guid.Empty, subscribingCompany, OfferSubscriptionStatusId.ACTIVE, new OfferUpdateUrlSubscriptionDetailData(detailId, clientClientId, oldUrl)));
        A.CallTo(() => _notificationRepository.CreateNotification(A<Guid>._, A<NotificationTypeId>._, A<bool>._, A<Action<Notification>>._))
            .Invokes((Guid receiverUserId, NotificationTypeId notificationTypeId, bool isRead, Action<Notification>? setOptionalParameters) =>
            {
                var notification = new Notification(Guid.NewGuid(), receiverUserId, DateTimeOffset.UtcNow, notificationTypeId, isRead);
                setOptionalParameters?.Invoke(notification);
                notifications.Add(notification);
            });

        A.CallTo(() => _notificationService.CreateNotifications(A<IDictionary<string, IEnumerable<string>>>._, null, A<IEnumerable<(string? content, NotificationTypeId notificationTypeId)>>._, A<Guid>._, A<bool?>._))
            .Returns(_fixture.CreateMany<Guid>(4).AsFakeIAsyncEnumerable(out var createNotificationsResultAsyncEnumerator));
        A.CallTo(() => _offerSubscriptionsRepository.AttachAndModifyAppSubscriptionDetail(detailId, subscriptionId, A<Action<AppSubscriptionDetail>>._, A<Action<AppSubscriptionDetail>>._))
            .Invokes((Guid _, Guid _, Action<AppSubscriptionDetail>? initialize, Action<AppSubscriptionDetail> setParameters) =>
            {
                initialize?.Invoke(details);
                setParameters.Invoke(details);
            });

        // Act
        await _sut.UpdateTenantUrlAsync(appId, subscriptionId, data, _iamUserId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _provisioningManager.UpdateClient(clientClientId, data.Url, A<string>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => createNotificationsResultAsyncEnumerator.MoveNextAsync())
            .MustHaveHappened(5, Times.Exactly);
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
        A.CallTo(() => _offerSubscriptionsRepository.GetUpdateUrlDataAsync(appId, subscriptionId, _iamUserId))
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

        // Act
        await _sut.UpdateTenantUrlAsync(appId, subscriptionId, data, _iamUserId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _provisioningManager.UpdateClient(clientClientId, data.Url, A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _notificationService.CreateNotifications(A<IDictionary<string, IEnumerable<string>>>._, A<Guid?>._, A<IEnumerable<(string? content, NotificationTypeId notificationTypeId)>>._, A<Guid>._, A<bool?>._)).MustHaveHappenedOnceExactly();
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
        A.CallTo(() => _offerSubscriptionsRepository.GetUpdateUrlDataAsync(appId, subscriptionId, _iamUserId))
            .Returns(new OfferUpdateUrlData("testApp", false, true, Guid.Empty, subscribingCompany, OfferSubscriptionStatusId.ACTIVE, new OfferUpdateUrlSubscriptionDetailData(detailId, clientClientId, oldUrl)));
        
        // Act
        await _sut.UpdateTenantUrlAsync(appId, subscriptionId, data, _iamUserId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _provisioningManager.UpdateClient(clientClientId, oldUrl, A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _notificationRepository.CreateNotification(A<Guid>._, A<NotificationTypeId>._, A<bool>._, A<Action<Notification>>._)).MustNotHaveHappened();
        A.CallTo(() => _notificationService.CreateNotifications(A<IDictionary<string, IEnumerable<string>>>._, A<Guid?>._, A<IEnumerable<(string? content, NotificationTypeId notificationTypeId)>>._, A<Guid>._, A<bool?>._)).MustNotHaveHappened();
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
        async Task Act() => await _sut.UpdateTenantUrlAsync(appId, subscriptionId, data, _iamUserId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.ParamName.Should().Be("Url");
        ex.Message.Should().Be($"url {data.Url} cannot be parsed: Invalid URI: The Authority/Host could not be parsed. (Parameter 'Url')");
        A.CallTo(() => _provisioningManager.UpdateClient(clientClientId, A<string>._, A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _notificationRepository.CreateNotification(A<Guid>._, A<NotificationTypeId>._, A<bool>._, A<Action<Notification>>._)).MustNotHaveHappened();
        A.CallTo(() => _notificationService.CreateNotifications(A<IDictionary<string, IEnumerable<string>>>._, A<Guid?>._, A<IEnumerable<(string? content, NotificationTypeId notificationTypeId)>>._, A<Guid>._, A<bool?>._)).MustNotHaveHappened();
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
        A.CallTo(() => _offerSubscriptionsRepository.GetUpdateUrlDataAsync(appId, subscriptionId, _iamUserId))
            .Returns((OfferUpdateUrlData?)null);

        // Act
        async Task Act() => await _sut.UpdateTenantUrlAsync(appId, subscriptionId, data, _iamUserId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"Offer {appId} or subscription {subscriptionId} do not exists");
        A.CallTo(() => _provisioningManager.UpdateClient(clientClientId, A<string>._, A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _notificationRepository.CreateNotification(A<Guid>._, A<NotificationTypeId>._, A<bool>._, A<Action<Notification>>._)).MustNotHaveHappened();
        A.CallTo(() => _notificationService.CreateNotifications(A<IDictionary<string, IEnumerable<string>>>._, A<Guid?>._, A<IEnumerable<(string? content, NotificationTypeId notificationTypeId)>>._, A<Guid>._, A<bool?>._)).MustNotHaveHappened();
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
        A.CallTo(() => _offerSubscriptionsRepository.GetUpdateUrlDataAsync(appId, subscriptionId, _iamUserId))
            .Returns(new OfferUpdateUrlData("testApp", true, true, Guid.Empty, subscribingCompany, OfferSubscriptionStatusId.ACTIVE, new OfferUpdateUrlSubscriptionDetailData(detailId, clientClientId, oldUrl)));

        // Act
        async Task Act() => await _sut.UpdateTenantUrlAsync(appId, subscriptionId, data, _iamUserId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("Subscription url of single instance apps can't be changed");
        A.CallTo(() => _provisioningManager.UpdateClient(clientClientId, A<string>._, A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _notificationRepository.CreateNotification(A<Guid>._, A<NotificationTypeId>._, A<bool>._, A<Action<Notification>>._)).MustNotHaveHappened();
        A.CallTo(() => _notificationService.CreateNotifications(A<IDictionary<string, IEnumerable<string>>>._, A<Guid?>._, A<IEnumerable<(string? content, NotificationTypeId notificationTypeId)>>._, A<Guid>._, A<bool?>._)).MustNotHaveHappened();
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
        A.CallTo(() => _offerSubscriptionsRepository.GetUpdateUrlDataAsync(appId, subscriptionId, _iamUserId))
            .Returns(new OfferUpdateUrlData("testApp", false, false, Guid.Empty, subscribingCompany, OfferSubscriptionStatusId.ACTIVE, new OfferUpdateUrlSubscriptionDetailData(detailId, clientClientId, oldUrl)));

        // Act
        async Task Act() => await _sut.UpdateTenantUrlAsync(appId, subscriptionId, data, _iamUserId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be($"User {_iamUserId} is not part of the app's providing company");
        A.CallTo(() => _provisioningManager.UpdateClient(clientClientId, A<string>._, A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _notificationRepository.CreateNotification(A<Guid>._, A<NotificationTypeId>._, A<bool>._, A<Action<Notification>>._)).MustNotHaveHappened();
        A.CallTo(() => _notificationService.CreateNotifications(A<IDictionary<string, IEnumerable<string>>>._, A<Guid?>._, A<IEnumerable<(string? content, NotificationTypeId notificationTypeId)>>._, A<Guid>._, A<bool?>._)).MustNotHaveHappened();
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
        A.CallTo(() => _offerSubscriptionsRepository.GetUpdateUrlDataAsync(appId, subscriptionId, _iamUserId))
            .Returns(new OfferUpdateUrlData("testApp", false, true, Guid.Empty, subscribingCompany, OfferSubscriptionStatusId.PENDING, new OfferUpdateUrlSubscriptionDetailData(detailId, clientClientId, oldUrl)));
        
        // Act
        async Task Act() => await _sut.UpdateTenantUrlAsync(appId, subscriptionId, data, _iamUserId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"Subscription {subscriptionId} must be in status ACTIVE");
        A.CallTo(() => _provisioningManager.UpdateClient(clientClientId, A<string>._, A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _notificationRepository.CreateNotification(A<Guid>._, A<NotificationTypeId>._, A<bool>._, A<Action<Notification>>._)).MustNotHaveHappened();
        A.CallTo(() => _notificationService.CreateNotifications(A<IDictionary<string, IEnumerable<string>>>._, A<Guid?>._, A<IEnumerable<(string? content, NotificationTypeId notificationTypeId)>>._, A<Guid>._, A<bool?>._)).MustNotHaveHappened();
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
        A.CallTo(() => _offerSubscriptionsRepository.GetUpdateUrlDataAsync(appId, subscriptionId, _iamUserId))
            .Returns(new OfferUpdateUrlData("testApp", false, true, Guid.Empty, subscribingCompany, OfferSubscriptionStatusId.ACTIVE, null));
        
        // Act
        async Task Act() => await _sut.UpdateTenantUrlAsync(appId, subscriptionId, data, _iamUserId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"There is no subscription detail data configured for subscription {subscriptionId}");
        A.CallTo(() => _provisioningManager.UpdateClient(clientClientId, A<string>._, A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _notificationRepository.CreateNotification(A<Guid>._, A<NotificationTypeId>._, A<bool>._, A<Action<Notification>>._)).MustNotHaveHappened();
        A.CallTo(() => _notificationService.CreateNotifications(A<IDictionary<string, IEnumerable<string>>>._, A<Guid?>._, A<IEnumerable<(string? content, NotificationTypeId notificationTypeId)>>._, A<Guid>._, A<bool?>._)).MustNotHaveHappened();
        A.CallTo(() => _offerSubscriptionsRepository.AttachAndModifyAppSubscriptionDetail(A<Guid>._, subscriptionId, A<Action<AppSubscriptionDetail>>._, A<Action<AppSubscriptionDetail>>._)).MustNotHaveHappened();
    }

    #endregion
}
