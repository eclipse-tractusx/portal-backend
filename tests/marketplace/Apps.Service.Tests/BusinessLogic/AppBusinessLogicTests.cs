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
// 
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Mailing.SendMail;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Service;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using System.Collections.Immutable;
using Xunit;

namespace Org.Eclipse.TractusX.Portal.Backend.Apps.Service.BusinessLogic.Tests;

public class AppBusinessLogicTests
{
    private const string IamUserId = "3e8343f7-4fe5-4296-8312-f33aa6dbde5d";

    private readonly IFixture _fixture;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IOfferRepository _offerRepository;
    private readonly IOfferSubscriptionsRepository _offerSubscriptionRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IMailingService _mailingService;
    private readonly IOfferService _offerService;
    private readonly IDocumentRepository _documentRepository;

    public AppBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _mailingService = A.Fake<IMailingService>();

        _offerService = A.Fake<IOfferService>();
        _portalRepositories = A.Fake<IPortalRepositories>();
        _offerRepository = A.Fake<IOfferRepository>();
        _offerSubscriptionRepository = A.Fake<IOfferSubscriptionsRepository>();
        _userRepository = A.Fake<IUserRepository>();
        _notificationRepository = A.Fake<INotificationRepository>();
        _documentRepository = A.Fake<IDocumentRepository>();
        A.CallTo(() => _portalRepositories.GetInstance<IOfferRepository>()).Returns(_offerRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
        A.CallTo(() => _portalRepositories.GetInstance<INotificationRepository>()).Returns(_notificationRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IOfferSubscriptionsRepository>()).Returns(_offerSubscriptionRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IDocumentRepository>()).Returns(_documentRepository);
    }

    [Fact]
    public async Task AddFavouriteAppForUser_ExecutesSuccessfully()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var (companyUser, iamUser) = CreateTestUserPair();
        A.CallTo(() => _userRepository.GetCompanyUserIdForIamUserUntrackedAsync(iamUser.UserEntityId))
            .Returns(companyUser.Id);

        var sut = new AppsBusinessLogic(_portalRepositories, null!, null!, null!, Options.Create(new AppsSettings()), null!);

        // Act
        await sut.AddFavouriteAppForUserAsync(appId, iamUser.UserEntityId);

        // Assert
        A.CallTo(() => _offerRepository.CreateAppFavourite(A<Guid>.That.Matches(x => x == appId), A<Guid>.That.Matches(x => x == companyUser.Id))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RemoveFavouriteAppForUser_ExecutesSuccessfully()
    {
        // Arrange
        var (companyUser, iamUser) = CreateTestUserPair();

        var appId = _fixture.Create<Guid>();
        A.CallTo(() => _userRepository.GetCompanyUserIdForIamUserUntrackedAsync(iamUser.UserEntityId))
            .Returns(companyUser.Id);

        var sut = new AppsBusinessLogic(_portalRepositories, null!, null!, null!, Options.Create(new AppsSettings()), null!);

        // Act
        await sut.RemoveFavouriteAppForUserAsync(appId, iamUser.UserEntityId);

        // Assert
        A.CallTo(() => _portalRepositories.Remove(A<CompanyUserAssignedAppFavourite>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    #region GetAllActiveApps
    
    [Fact]
    public async Task GetAllActiveAppsAsync_ExecutesSuccessfully()
    {
        // Arrange
        var results = _fixture.CreateMany<ActiveAppData>(5);
        A.CallTo(() => _offerRepository.GetAllActiveAppsAsync(A<string>._, A<string>._)).Returns(results.ToAsyncEnumerable());

        var sut = new AppsBusinessLogic(_portalRepositories, null!, null!, null!, Options.Create(new AppsSettings()), null!);

        // Act
        var result = await sut.GetAllActiveAppsAsync(null).ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().HaveCount(5);
    }
    
    #endregion

    #region GetAllUserUserBusinessApps

    [Fact]
    public async Task GetAllUserUserBusinessAppsAsync_WithValidData_ReturnsExpectedData()
    {
        // Arrange
        var appData = _fixture.CreateMany<(Guid, Guid, string?, string, Guid, string)>(5);
        A.CallTo(() => _offerSubscriptionRepository.GetAllBusinessAppDataForUserIdAsync(A<string>._)).Returns(appData.ToAsyncEnumerable());
        var sut = new AppsBusinessLogic(_portalRepositories, null!, null!, null!, Options.Create(new AppsSettings()), null!);

        // Act
        var result = await sut.GetAllUserUserBusinessAppsAsync(_fixture.Create<string>()).ToListAsync().ConfigureAwait(false);
        
        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().HaveCount(5);
    }

    #endregion
    
    #region Get App Agreement

    [Fact]
    public async Task GetAppAgreement_WithUserId_ReturnsAgreementData()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var offerService = A.Fake<IOfferService>();
        var data = _fixture.CreateMany<AgreementData>(1);
        A.CallTo(() => offerService.GetOfferAgreementsAsync(A<Guid>.That.Matches(x => x == appId), A<OfferTypeId>._))
            .Returns(data.ToAsyncEnumerable());
        var sut = new AppsBusinessLogic(null!, null!, offerService, null!, Options.Create(new AppsSettings()), null!);

        // Act
        var result = await sut.GetAppAgreement(appId).ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().ContainSingle();
    }

    #endregion

    #region Add Service Subscription

    [Fact]
    public async Task AddServiceSubscription_ReturnsCorrectId()
    {
        // Arrange
        var offerSubscriptionId = Guid.NewGuid();
        var offerSubscriptionService = A.Fake<IOfferSubscriptionService>();
        var consentData = _fixture.CreateMany<OfferAgreementConsentData>(2);
        A.CallTo(() => offerSubscriptionService.AddOfferSubscriptionAsync(A<Guid>._, A<IEnumerable<OfferAgreementConsentData>>._, A<string>._, A<string>._, A<IDictionary<string, IEnumerable<string>>>._, A<OfferTypeId>._, A<string>._))
            .Returns(offerSubscriptionId);
        var sut = new AppsBusinessLogic(null!, offerSubscriptionService, null!, null!, Options.Create(new AppsSettings()), null!);

        // Act
        var result = await sut.AddOwnCompanyAppSubscriptionAsync(Guid.NewGuid(), consentData, "44638c72-690c-42e8-bd5e-c8ac3047ff82", "THISISAACCESSTOKEN").ConfigureAwait(false);

        // Assert
        result.Should().Be(offerSubscriptionId);
        A.CallTo(() => offerSubscriptionService.AddOfferSubscriptionAsync(
                A<Guid>._,
                A<IEnumerable<OfferAgreementConsentData>>._,
                A<string>._,
                A<string>._,
                A<IDictionary<string, IEnumerable<string>>>._,
                A<OfferTypeId>.That.Matches(x => x == OfferTypeId.APP),
                A<string>._))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region Auto setup service

    [Fact]
    public async Task AutoSetupService_ReturnsExcepted()
    {
        var offerAutoSetupResponseData = _fixture.Create<OfferAutoSetupResponseData>();
        // Arrange
        var offerSetupService = A.Fake<IOfferSetupService>();
        A.CallTo(() => offerSetupService.AutoSetupOfferAsync(A<OfferAutoSetupData>._, A<IDictionary<string, IEnumerable<string>>>._, A<string>._, A<OfferTypeId>._, A<string>._, A<IDictionary<string, IEnumerable<string>>>._))
            .Returns(offerAutoSetupResponseData);
        var data = new OfferAutoSetupData(Guid.NewGuid(), "https://www.offer.com");

        var sut = new AppsBusinessLogic(null!, null!, null!, offerSetupService, _fixture.Create<IOptions<AppsSettings>>(), A.Fake<MailingService>());

        // Act
        var result = await sut.AutoSetupAppAsync(data, IamUserId).ConfigureAwait(false);

        // Assert
        result.Should().Be(offerAutoSetupResponseData);
    }

    #endregion

    #region GetCompanyProvidedAppSubscriptionStatusesForUser
    
    [Theory]
    [InlineData(null)]
    [InlineData("c714b905-9d2a-4cf3-b9f7-10be4eeddfc8")]
    public async Task GetCompanyProvidedAppSubscriptionStatusesForUserAsync_ReturnsExpected(string? offerIdTxt)
    {
        // Arrange
        var iamUserId = _fixture.Create<string>();
        Guid? offerId = offerIdTxt == null ? null : new Guid(offerIdTxt);
        var data = _fixture.CreateMany<OfferCompanySubscriptionStatusData>(5).ToImmutableArray();
        A.CallTo(() => _offerSubscriptionRepository.GetOwnCompanyProvidedOfferSubscriptionStatusesUntrackedAsync(iamUserId, OfferTypeId.APP, default, OfferSubscriptionStatusId.ACTIVE, A<Guid?>._))
            .Returns((skip, take) => Task.FromResult(new Pagination.Source<OfferCompanySubscriptionStatusData>(data.Length, data.Skip(skip).Take(take)))!);

        var appsSettings = new AppsSettings
        {
            ApplicationsMaxPageSize = 15
        };
        
        var sut = new AppsBusinessLogic(_portalRepositories, null!, null!, null!, Options.Create(appsSettings), null!);

        // Act
        var result = await sut.GetCompanyProvidedAppSubscriptionStatusesForUserAsync(0, 10, iamUserId, null, null, offerId).ConfigureAwait(false);

        // Assert
        result.Meta.NumberOfElements.Should().Be(5);
        result.Content.Should().HaveCount(5).And.Satisfy(
            x => x.OfferId == data[0].OfferId && x.ServiceName == data[0].ServiceName && x.CompanySubscriptionStatuses.SequenceEqual(data[0].CompanySubscriptionStatuses) && x.Image == data[0].Image,
            x => x.OfferId == data[1].OfferId && x.ServiceName == data[1].ServiceName && x.CompanySubscriptionStatuses.SequenceEqual(data[1].CompanySubscriptionStatuses) && x.Image == data[1].Image,
            x => x.OfferId == data[2].OfferId && x.ServiceName == data[2].ServiceName && x.CompanySubscriptionStatuses.SequenceEqual(data[2].CompanySubscriptionStatuses) && x.Image == data[2].Image,
            x => x.OfferId == data[3].OfferId && x.ServiceName == data[3].ServiceName && x.CompanySubscriptionStatuses.SequenceEqual(data[3].CompanySubscriptionStatuses) && x.Image == data[3].Image,
            x => x.OfferId == data[4].OfferId && x.ServiceName == data[4].ServiceName && x.CompanySubscriptionStatuses.SequenceEqual(data[4].CompanySubscriptionStatuses) && x.Image == data[4].Image
        );
        A.CallTo(() => _offerSubscriptionRepository.GetOwnCompanyProvidedOfferSubscriptionStatusesUntrackedAsync(iamUserId, OfferTypeId.APP, default, OfferSubscriptionStatusId.ACTIVE, offerId))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetCompanyProvidedAppSubscriptionStatusesForUserAsync_EmptyImage_ReturnsExpected()
    {
        // Arrange
        var iamUserId = _fixture.Create<string>();
        var offerId = Guid.NewGuid();
        var data = new [] {
            _fixture.Build<OfferCompanySubscriptionStatusData>().With(x => x.Image, Guid.Empty).Create(),
            _fixture.Build<OfferCompanySubscriptionStatusData>().With(x => x.Image, Guid.NewGuid()).Create()
        };
        A.CallTo(() => _offerSubscriptionRepository.GetOwnCompanyProvidedOfferSubscriptionStatusesUntrackedAsync(iamUserId, OfferTypeId.APP, default, OfferSubscriptionStatusId.ACTIVE, A<Guid?>._))
            .Returns((skip, take) => Task.FromResult(new Pagination.Source<OfferCompanySubscriptionStatusData>(data.Length, data.Skip(skip).Take(take)))!);

        var appsSettings = new AppsSettings
        {
            ApplicationsMaxPageSize = 15
        };
        
        var sut = new AppsBusinessLogic(_portalRepositories, null!, null!, null!, Options.Create(appsSettings), null!);

        // Act
        var result = await sut.GetCompanyProvidedAppSubscriptionStatusesForUserAsync(0, 10, iamUserId, null, null, offerId).ConfigureAwait(false);

        // Assert
        result.Meta.NumberOfElements.Should().Be(2);
        result.Content.Should().HaveCount(2).And.Satisfy(
            x => x.OfferId == data[0].OfferId && x.ServiceName == data[0].ServiceName && x.CompanySubscriptionStatuses.SequenceEqual(data[0].CompanySubscriptionStatuses) && x.Image == null,
            x => x.OfferId == data[1].OfferId && x.ServiceName == data[1].ServiceName && x.CompanySubscriptionStatuses.SequenceEqual(data[1].CompanySubscriptionStatuses) && x.Image == data[1].Image
        );
        A.CallTo(() => _offerSubscriptionRepository.GetOwnCompanyProvidedOfferSubscriptionStatusesUntrackedAsync(iamUserId, OfferTypeId.APP, default, OfferSubscriptionStatusId.ACTIVE, offerId))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetCompanyProvidedAppSubscriptionStatusesForUserAsync_QueryNullResult_ReturnsExpected()
    {
        // Arrange
        var iamUserId = _fixture.Create<string>();
        var offerId = Guid.NewGuid();
        var data = new [] {
            _fixture.Build<OfferCompanySubscriptionStatusData>().With(x => x.Image, Guid.Empty).Create(),
            _fixture.Build<OfferCompanySubscriptionStatusData>().With(x => x.Image, Guid.NewGuid()).Create()
        };
        A.CallTo(() => _offerSubscriptionRepository.GetOwnCompanyProvidedOfferSubscriptionStatusesUntrackedAsync(iamUserId, OfferTypeId.APP, default, OfferSubscriptionStatusId.ACTIVE, A<Guid?>._))
            .Returns((skip, take) => Task.FromResult((Pagination.Source<OfferCompanySubscriptionStatusData>?)null));

        var appsSettings = new AppsSettings
        {
            ApplicationsMaxPageSize = 15
        };
        
        var sut = new AppsBusinessLogic(_portalRepositories, null!, null!, null!, Options.Create(appsSettings), null!);

        // Act
        var result = await sut.GetCompanyProvidedAppSubscriptionStatusesForUserAsync(0, 10, iamUserId, null, null, offerId).ConfigureAwait(false);

        // Assert
        result.Meta.NumberOfElements.Should().Be(0);
        result.Content.Should().BeEmpty();
        A.CallTo(() => _offerSubscriptionRepository.GetOwnCompanyProvidedOfferSubscriptionStatusesUntrackedAsync(iamUserId, OfferTypeId.APP, default, OfferSubscriptionStatusId.ACTIVE, offerId))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region ActivateOwnCompanyProvidedAppSubscription

    [Fact]
    public async Task ActivateOwnCompanyProvidedAppSubscriptionAsync_WithNotExistingApp_ThrowsNotFoundException()
    {
        // Arrange
        var notExistingAppId = _fixture.Create<Guid>();
        A.CallTo(() => _offerSubscriptionRepository.GetCompanyAssignedAppDataForProvidingCompanyUserAsync(notExistingAppId, A<Guid>._, IamUserId))
            .Returns(((Guid, OfferSubscriptionStatusId, Guid, string?, Guid, RequesterData))default);
        
        var sut = new AppsBusinessLogic(_portalRepositories, null!, null!, null!, Options.Create(new AppsSettings()), null!);

        // Act
        async Task Act() => await sut.ActivateOwnCompanyProvidedAppSubscriptionAsync(notExistingAppId, Guid.NewGuid(), IamUserId).ConfigureAwait(false);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(Act);
    }

    [Fact]
    public async Task ActivateOwnCompanyProvidedAppSubscriptionAsync_IsNoMemberOfCompanyProvidingApp_ThrowsArgumentException()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        A.CallTo(() => _offerSubscriptionRepository.GetCompanyAssignedAppDataForProvidingCompanyUserAsync(appId, A<Guid>._, A<string>.That.Not.Matches(x => x == IamUserId)))
            .Returns((
                Guid.Empty,
                default,
                Guid.Empty,
                "app 1",
                Guid.Empty,
                new RequesterData(string.Empty, string.Empty, string.Empty)));
        
        var sut = new AppsBusinessLogic(_portalRepositories, null!, null!, null!, Options.Create(new AppsSettings()), null!);

        // Act
        async Task Act() => await sut.ActivateOwnCompanyProvidedAppSubscriptionAsync(appId, Guid.NewGuid(), Guid.NewGuid().ToString()).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be("Missing permission: The user's company does not provide the requested app so they cannot activate it.");
    }

    [Fact]
    public async Task ActivateOwnCompanyProvidedAppSubscriptionAsync_WithActiveApp_ThrowsArgumentException()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var offerSubscription = _fixture.Create<OfferSubscription>();
        var companyId = _fixture.Create<Guid>();
        offerSubscription.OfferSubscriptionStatusId = OfferSubscriptionStatusId.ACTIVE;
        A.CallTo(() => _offerSubscriptionRepository.GetCompanyAssignedAppDataForProvidingCompanyUserAsync(appId, companyId, IamUserId))
            .Returns((
                offerSubscription.Id,
                offerSubscription.OfferSubscriptionStatusId,
                offerSubscription.RequesterId,
                "app 1",
                Guid.NewGuid(),
                new RequesterData(string.Empty, string.Empty, string.Empty)));
        
        var sut = new AppsBusinessLogic(_portalRepositories, null!, null!, null!, Options.Create(new AppsSettings()), null!);

        // Act
        async Task Act() => await sut.ActivateOwnCompanyProvidedAppSubscriptionAsync(appId, companyId, IamUserId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be($"subscription for app {appId}, company {companyId} is not in status PENDING");
    }

    [Fact]
    public async Task ActivateOwnCompanyProvidedAppSubscriptionAsync_WithProviderNEmailNotSet_DoesntSendMail()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var offerSubscription = _fixture.Create<OfferSubscription>();
        offerSubscription.OfferSubscriptionStatusId = OfferSubscriptionStatusId.PENDING;
        A.CallTo(() => _offerSubscriptionRepository.GetCompanyAssignedAppDataForProvidingCompanyUserAsync(appId, A<Guid>._, IamUserId))
            .Returns((
                offerSubscription.Id,
                offerSubscription.OfferSubscriptionStatusId,
                offerSubscription.RequesterId,
                "app 1",
                Guid.NewGuid(),
                new RequesterData(string.Empty, string.Empty, string.Empty)));
        
        var sut = new AppsBusinessLogic(_portalRepositories, null!, null!, null!, Options.Create(new AppsSettings()), null!);

        // Act
        await sut.ActivateOwnCompanyProvidedAppSubscriptionAsync(appId, Guid.NewGuid(), IamUserId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _notificationRepository.CreateNotification(A<Guid>._, NotificationTypeId.APP_SUBSCRIPTION_ACTIVATION, false, A<Action<Notification>?>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _mailingService.SendMails(A<string>._, A<IDictionary<string, string>>._, A<IEnumerable<string>>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task ActivateOwnCompanyProvidedAppSubscriptionAsync_WithProviderEmailSet_SendsMail()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var offerSubscription = _fixture.Create<OfferSubscription>();
        offerSubscription.OfferSubscriptionStatusId = OfferSubscriptionStatusId.PENDING;
        A.CallTo(() => _offerSubscriptionRepository.GetCompanyAssignedAppDataForProvidingCompanyUserAsync(appId, A<Guid>._, IamUserId))
            .Returns((
                offerSubscription.Id,
                offerSubscription.OfferSubscriptionStatusId,
                offerSubscription.RequesterId,
                "app 1",
                Guid.NewGuid(),
                new RequesterData("test@email.com", "tony", "gilbert")));
        
        var sut = new AppsBusinessLogic(_portalRepositories, null!, null!, null!, Options.Create(new AppsSettings()), _mailingService);

        // Act
        await sut.ActivateOwnCompanyProvidedAppSubscriptionAsync(appId, Guid.NewGuid(), IamUserId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _notificationRepository.CreateNotification(A<Guid>._, NotificationTypeId.APP_SUBSCRIPTION_ACTIVATION, false, A<Action<Notification>?>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _mailingService.SendMails("test@email.com", A<IDictionary<string, string>>._, A<IEnumerable<string>>.That.Matches(x => x.Count() == 1 && x.First() == "subscription-activation"))).MustHaveHappenedOnceExactly();
    }

    #endregion

    #region GetAppDocumentContentAsync

    [Fact]
    public async Task GetAppDocumentContentAsync_ReturnsExpectedResult()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var documentId = _fixture.Create<Guid>();
        var settings = new AppsSettings
        {
            AppImageDocumentTypeIds = _fixture.Create<IEnumerable<DocumentTypeId>>(),
        };

        var sut = new AppsBusinessLogic(_portalRepositories, null!, _offerService, null!, Options.Create(settings), null!);

        // Act
        var result = await sut.GetAppDocumentContentAsync(appId, documentId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _offerService.GetOfferDocumentContentAsync(appId, documentId, settings.AppImageDocumentTypeIds, OfferTypeId.APP, A<CancellationToken>._)).MustHaveHappened();
    }

    #endregion

    #region GetCompanyProvidedAppsDataForUserAsync

    [Fact]
    public async Task GetCompanyProvidedAppsDataForUserAsync_ReturnsExpectedCount()
    {
        //Arrange
        var data = new AsyncEnumerableStub<AllOfferData>(_fixture.CreateMany<AllOfferData>(5));

        A.CallTo(() => _offerRepository.GetProvidedOffersData(A<OfferTypeId>._, A<string>._))
            .Returns(data.AsAsyncEnumerable());

        var sut = new AppsBusinessLogic(_portalRepositories, null!, null!, null!, _fixture.Create<IOptions<AppsSettings>>(), _mailingService);

        //Act
        var result = await sut.GetCompanyProvidedAppsDataForUserAsync(IamUserId).ToListAsync().ConfigureAwait(false);

        //Assert
        result.Should().HaveSameCount(data);
    }

    #endregion

    #region GetCompanySubscribedAppSubscriptionStatusesForUserAsync

    [Fact]
    public async Task GetCompanySubscribedAppSubscriptionStatusesForUserAsync_ReturnsExpected()
    {
        // Arrange
        var iamUserId = _fixture.Create<string>();
        var data = _fixture.CreateMany<(Guid AppId, OfferSubscriptionStatusId OfferSubscriptionStatusId, string? Name, string Provider, Guid Image)>(3).ToImmutableArray();
        A.CallTo(() => _offerSubscriptionRepository.GetOwnCompanySubscribedAppSubscriptionStatusesUntrackedAsync(iamUserId))
            .Returns(data.ToAsyncEnumerable());

        var sut = new AppsBusinessLogic(_portalRepositories, null!, null!,  null!, _fixture.Create<IOptions<AppsSettings>>(), null!);

        // Act
        var result = await sut.GetCompanySubscribedAppSubscriptionStatusesForUserAsync(iamUserId).ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().HaveCount(3).And.Satisfy(
            x => x.AppId == data[0].AppId && x.Name == data[0].Name && x.OfferSubscriptionStatus == data[0].OfferSubscriptionStatusId && x.Provider == data[0].Provider && x.Image == data[0].Image,
            x => x.AppId == data[1].AppId && x.Name == data[1].Name && x.OfferSubscriptionStatus == data[1].OfferSubscriptionStatusId && x.Provider == data[1].Provider && x.Image == data[1].Image,
            x => x.AppId == data[2].AppId && x.Name == data[2].Name && x.OfferSubscriptionStatus == data[2].OfferSubscriptionStatusId && x.Provider == data[2].Provider && x.Image == data[2].Image
        );
    }

    [Fact]
    public async Task GetCompanySubscribedAppSubscriptionStatusesForUserAsync_NullableProperties_ReturnsExpected()
    {
        // Arrange
        var iamUserId = _fixture.Create<string>();
        var data = new (Guid AppId, OfferSubscriptionStatusId OfferSubscriptionStatusId, string? Name, string Provider, Guid Image) [] {
            ( Guid.NewGuid(), OfferSubscriptionStatusId.ACTIVE, null, _fixture.Create<string>(), Guid.Empty )
        };

        _fixture.CreateMany<(Guid AppId, OfferSubscriptionStatusId OfferSubscriptionStatusId, string? Name, string Provider, Guid Image)>(3).ToImmutableArray();
        A.CallTo(() => _offerSubscriptionRepository.GetOwnCompanySubscribedAppSubscriptionStatusesUntrackedAsync(iamUserId))
            .Returns(data.ToAsyncEnumerable());

        var sut = new AppsBusinessLogic(_portalRepositories, null!, null!,  null!, _fixture.Create<IOptions<AppsSettings>>(), null!);

        // Act
        var result = await sut.GetCompanySubscribedAppSubscriptionStatusesForUserAsync(iamUserId).ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().HaveCount(1).And.Satisfy(
            x => x.AppId == data[0].AppId && x.Name == null && x.Image == null
        );
    }

    #endregion

    #region GetSubscriptionDetailForProvider

    [Fact]
    public async Task GetSubscriptionDetailForProvider_WithNotMatchingUserRoles_ThrowsException()
    {
        // Arrange
        var offerId = _fixture.Create<Guid>();
        var subscriptionId = _fixture.Create<Guid>();
        var data = _fixture.Create<ProviderSubscriptionDetailData>();
        var settings = new AppsSettings
        {
            CompanyAdminRoles = new Dictionary<string, IEnumerable<string>>
            {
                {"ClientTest", new[] {"Test"}}
            }
        };
        A.CallTo(() => _offerService.GetSubscriptionDetailsForProviderAsync(offerId, subscriptionId, IamUserId, OfferTypeId.APP, A<IDictionary<string, IEnumerable<string>>>._))
            .Returns(data);
        var sut = new AppsBusinessLogic(null!, null!, _offerService,  null!, Options.Create(settings), null!);

        // Act
        var result = await sut.GetSubscriptionDetailForProvider(offerId, subscriptionId, IamUserId).ConfigureAwait(false);

        // Assert
        result.Should().Be(data);
    }

    #endregion

    #region GetSubscriptionDetailForSubscriber

    [Fact]
    public async Task GetSubscriptionDetailForSubscriber_WithNotMatchingUserRoles_ThrowsException()
    {
        // Arrange
        var offerId = _fixture.Create<Guid>();
        var subscriptionId = _fixture.Create<Guid>();
        var data = _fixture.Create<SubscriberSubscriptionDetailData>();
        var settings = new AppsSettings
        {
            CompanyAdminRoles = new Dictionary<string, IEnumerable<string>>
            {
                {"ClientTest", new[] {"Test"}}
            }
        };
        A.CallTo(() => _offerService.GetSubscriptionDetailsForSubscriberAsync(offerId, subscriptionId, IamUserId, OfferTypeId.APP, A<IDictionary<string, IEnumerable<string>>>._))
            .Returns(data);
        var sut = new AppsBusinessLogic(null!, null!, _offerService,  null!, Options.Create(settings), null!);

        // Act
        var result = await sut.GetSubscriptionDetailForSubscriber(offerId, subscriptionId, IamUserId).ConfigureAwait(false);

        // Assert
        result.Should().Be(data);
    }

    #endregion

    #region Setup
    
    private (CompanyUser, IamUser) CreateTestUserPair()
    {
        var companyUser = _fixture.Build<CompanyUser>()
            .Without(u => u.IamUser)
            .Create();
        var iamUser = _fixture.Build<IamUser>()
            .With(u => u.CompanyUser, companyUser)
            .Create();
        companyUser.IamUser = iamUser;
        return (companyUser, iamUser);
    }

    #endregion
}
