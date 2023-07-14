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
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Mailing.SendMail;
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

public class AppBusinessLogicTests
{
    private const string IamUserId = "3e8343f7-4fe5-4296-8312-f33aa6dbde5d";
    private readonly IdentityData _identity = new(IamUserId, Guid.NewGuid(), IdentityTypeId.COMPANY_USER, Guid.NewGuid());

    private readonly IFixture _fixture;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IOfferRepository _offerRepository;
    private readonly IOfferSubscriptionsRepository _offerSubscriptionRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IMailingService _mailingService;
    private readonly IOfferService _offerService;

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
        _notificationRepository = A.Fake<INotificationRepository>();
        A.CallTo(() => _portalRepositories.GetInstance<IOfferRepository>()).Returns(_offerRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IOfferSubscriptionsRepository>()).Returns(_offerSubscriptionRepository);
        A.CallTo(() => _portalRepositories.GetInstance<INotificationRepository>()).Returns(_notificationRepository);
    }

    [Fact]
    public async Task AddFavouriteAppForUser_ExecutesSuccessfully()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();

        var sut = new AppsBusinessLogic(_portalRepositories, null!, null!, null!, Options.Create(new AppsSettings()), null!);

        // Act
        await sut.AddFavouriteAppForUserAsync(appId, _identity.UserId);

        // Assert
        A.CallTo(() => _offerRepository.CreateAppFavourite(A<Guid>.That.Matches(x => x == appId), A<Guid>.That.Matches(x => x == _identity.UserId))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RemoveFavouriteAppForUser_ExecutesSuccessfully()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();

        var sut = new AppsBusinessLogic(_portalRepositories, null!, null!, null!, Options.Create(new AppsSettings()), null!);

        // Act
        await sut.RemoveFavouriteAppForUserAsync(appId, _identity.UserId);

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
        A.CallTo(() => _offerSubscriptionRepository.GetAllBusinessAppDataForUserIdAsync(A<Guid>._)).Returns(appData.ToAsyncEnumerable());
        var sut = new AppsBusinessLogic(_portalRepositories, null!, null!, null!, Options.Create(new AppsSettings()), null!);

        // Act
        var result = await sut.GetAllUserUserBusinessAppsAsync(_fixture.Create<Guid>()).ToListAsync().ConfigureAwait(false);

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
        var identity = _fixture.Build<IdentityData>()
            .With(x => x.UserEntityId, "44638c72-690c-42e8-bd5e-c8ac3047ff82")
            .Create();

        var offerSubscriptionId = Guid.NewGuid();
        var offerSubscriptionService = A.Fake<IOfferSubscriptionService>();
        var consentData = _fixture.CreateMany<OfferAgreementConsentData>(2);
        A.CallTo(() => offerSubscriptionService.AddOfferSubscriptionAsync(A<Guid>._, A<IEnumerable<OfferAgreementConsentData>>._, A<ValueTuple<Guid, Guid>>._, A<OfferTypeId>._, A<string>._))
            .Returns(offerSubscriptionId);
        var sut = new AppsBusinessLogic(null!, offerSubscriptionService, null!, null!, Options.Create(new AppsSettings()), null!);

        // Act
        var result = await sut.AddOwnCompanyAppSubscriptionAsync(Guid.NewGuid(), consentData, (identity.UserId, identity.CompanyId)).ConfigureAwait(false);

        // Assert
        result.Should().Be(offerSubscriptionId);
        A.CallTo(() => offerSubscriptionService.AddOfferSubscriptionAsync(
                A<Guid>._,
                A<IEnumerable<OfferAgreementConsentData>>._,
                A<ValueTuple<Guid, Guid>>._,
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
        A.CallTo(() => offerSetupService.AutoSetupOfferAsync(A<OfferAutoSetupData>._, A<IEnumerable<UserRoleConfig>>._, A<ValueTuple<Guid, Guid>>._, A<OfferTypeId>._, A<string>._, A<IEnumerable<UserRoleConfig>>._))
            .Returns(offerAutoSetupResponseData);
        var data = new OfferAutoSetupData(Guid.NewGuid(), "https://www.offer.com");

        var sut = new AppsBusinessLogic(null!, null!, null!, offerSetupService, _fixture.Create<IOptions<AppsSettings>>(), A.Fake<MailingService>());

        // Act
        var result = await sut.AutoSetupAppAsync(data, (_identity.UserId, _identity.CompanyId)).ConfigureAwait(false);

        // Assert
        result.Should().Be(offerAutoSetupResponseData);
    }

    #endregion

    #region Start Auto setup service

    [Fact]
    public async Task StartAutoSetupService_ReturnsExcepted()
    {
        // Arrange
        var offerSetupService = A.Fake<IOfferSetupService>();
        var data = new OfferAutoSetupData(Guid.NewGuid(), "https://www.offer.com");

        var sut = new AppsBusinessLogic(null!, null!, null!, offerSetupService, _fixture.Create<IOptions<AppsSettings>>(), A.Fake<MailingService>());

        // Act
        await sut.StartAutoSetupAsync(data, _identity.CompanyId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => offerSetupService.StartAutoSetupAsync(A<OfferAutoSetupData>._, A<Guid>._, OfferTypeId.APP)).MustHaveHappenedOnceExactly();
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
        A.CallTo(() => _offerSubscriptionRepository.GetOwnCompanyProvidedOfferSubscriptionStatusesUntrackedAsync(A<Guid>._, A<OfferTypeId>._, A<SubscriptionStatusSorting?>._, A<IEnumerable<OfferSubscriptionStatusId>>._, A<Guid?>._))
            .Returns((skip, take) => Task.FromResult(new Pagination.Source<OfferCompanySubscriptionStatusData>(data.Length, data.Skip(skip).Take(take)))!);

        var appsSettings = new AppsSettings
        {
            ApplicationsMaxPageSize = 15
        };

        var sut = new AppsBusinessLogic(_portalRepositories, null!, null!, null!, Options.Create(appsSettings), null!);

        // Act
        var result = await sut.GetCompanyProvidedAppSubscriptionStatusesForUserAsync(0, 10, _identity.CompanyId, null, null, offerId).ConfigureAwait(false);

        // Assert
        result.Meta.NumberOfElements.Should().Be(5);
        result.Content.Should().HaveCount(5).And.Satisfy(
            x => x.OfferId == data[0].OfferId && x.OfferName == data[0].ServiceName && x.CompanySubscriptionStatuses.SequenceEqual(data[0].CompanySubscriptionStatuses) && x.Image == data[0].Image,
            x => x.OfferId == data[1].OfferId && x.OfferName == data[1].ServiceName && x.CompanySubscriptionStatuses.SequenceEqual(data[1].CompanySubscriptionStatuses) && x.Image == data[1].Image,
            x => x.OfferId == data[2].OfferId && x.OfferName == data[2].ServiceName && x.CompanySubscriptionStatuses.SequenceEqual(data[2].CompanySubscriptionStatuses) && x.Image == data[2].Image,
            x => x.OfferId == data[3].OfferId && x.OfferName == data[3].ServiceName && x.CompanySubscriptionStatuses.SequenceEqual(data[3].CompanySubscriptionStatuses) && x.Image == data[3].Image,
            x => x.OfferId == data[4].OfferId && x.OfferName == data[4].ServiceName && x.CompanySubscriptionStatuses.SequenceEqual(data[4].CompanySubscriptionStatuses) && x.Image == data[4].Image
        );
        A.CallTo(() => _offerSubscriptionRepository.GetOwnCompanyProvidedOfferSubscriptionStatusesUntrackedAsync(_identity.CompanyId, OfferTypeId.APP, default, A<IEnumerable<OfferSubscriptionStatusId>>.That.IsSameSequenceAs(new[] { OfferSubscriptionStatusId.PENDING, OfferSubscriptionStatusId.ACTIVE }), offerId))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetCompanyProvidedAppSubscriptionStatusesForUserAsync_EmptyImage_ReturnsExpected()
    {
        // Arrange
        var iamUserId = _fixture.Create<string>();
        var offerId = Guid.NewGuid();
        var data = new[] {
            _fixture.Build<OfferCompanySubscriptionStatusData>().With(x => x.Image, Guid.Empty).Create(),
            _fixture.Build<OfferCompanySubscriptionStatusData>().With(x => x.Image, Guid.NewGuid()).Create()
        };
        A.CallTo(() => _offerSubscriptionRepository.GetOwnCompanyProvidedOfferSubscriptionStatusesUntrackedAsync(A<Guid>._, A<OfferTypeId>._, A<SubscriptionStatusSorting?>._, A<IEnumerable<OfferSubscriptionStatusId>>._, A<Guid?>._))
            .Returns((skip, take) => Task.FromResult(new Pagination.Source<OfferCompanySubscriptionStatusData>(data.Length, data.Skip(skip).Take(take)))!);

        var appsSettings = new AppsSettings
        {
            ApplicationsMaxPageSize = 15
        };

        var sut = new AppsBusinessLogic(_portalRepositories, null!, null!, null!, Options.Create(appsSettings), null!);

        // Act
        var result = await sut.GetCompanyProvidedAppSubscriptionStatusesForUserAsync(0, 10, _identity.CompanyId, null, null, offerId).ConfigureAwait(false);

        // Assert
        result.Meta.NumberOfElements.Should().Be(2);
        result.Content.Should().HaveCount(2).And.Satisfy(
            x => x.OfferId == data[0].OfferId && x.OfferName == data[0].ServiceName && x.CompanySubscriptionStatuses.SequenceEqual(data[0].CompanySubscriptionStatuses) && x.Image == null,
            x => x.OfferId == data[1].OfferId && x.OfferName == data[1].ServiceName && x.CompanySubscriptionStatuses.SequenceEqual(data[1].CompanySubscriptionStatuses) && x.Image == data[1].Image
        );
        A.CallTo(() => _offerSubscriptionRepository.GetOwnCompanyProvidedOfferSubscriptionStatusesUntrackedAsync(_identity.CompanyId, OfferTypeId.APP, default, A<IEnumerable<OfferSubscriptionStatusId>>.That.IsSameSequenceAs(new[] { OfferSubscriptionStatusId.PENDING, OfferSubscriptionStatusId.ACTIVE }), offerId))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetCompanyProvidedAppSubscriptionStatusesForUserAsync_QueryNullResult_ReturnsExpected()
    {
        // Arrange
        var iamUserId = _fixture.Create<string>();
        var offerId = Guid.NewGuid();
        var data = new[] {
            _fixture.Build<OfferCompanySubscriptionStatusData>().With(x => x.Image, Guid.Empty).Create(),
            _fixture.Build<OfferCompanySubscriptionStatusData>().With(x => x.Image, Guid.NewGuid()).Create()
        };
        A.CallTo(() => _offerSubscriptionRepository.GetOwnCompanyProvidedOfferSubscriptionStatusesUntrackedAsync(A<Guid>._, A<OfferTypeId>._, A<SubscriptionStatusSorting?>._, A<IEnumerable<OfferSubscriptionStatusId>>._, A<Guid?>._))
            .Returns((skip, take) => Task.FromResult((Pagination.Source<OfferCompanySubscriptionStatusData>?)null));

        var appsSettings = new AppsSettings
        {
            ApplicationsMaxPageSize = 15
        };

        var sut = new AppsBusinessLogic(_portalRepositories, null!, null!, null!, Options.Create(appsSettings), null!);

        // Act
        var result = await sut.GetCompanyProvidedAppSubscriptionStatusesForUserAsync(0, 10, _identity.CompanyId, null, null, offerId).ConfigureAwait(false);

        // Assert
        result.Meta.NumberOfElements.Should().Be(0);
        result.Content.Should().BeEmpty();
        A.CallTo(() => _offerSubscriptionRepository.GetOwnCompanyProvidedOfferSubscriptionStatusesUntrackedAsync(_identity.CompanyId, OfferTypeId.APP, default, A<IEnumerable<OfferSubscriptionStatusId>>.That.IsSameSequenceAs(new[] { OfferSubscriptionStatusId.PENDING, OfferSubscriptionStatusId.ACTIVE }), offerId))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region ActivateOwnCompanyProvidedAppSubscription

    [Fact]
    public async Task ActivateOwnCompanyProvidedAppSubscriptionAsync_WithNotExistingApp_ThrowsNotFoundException()
    {
        // Arrange
        var notExistingAppId = _fixture.Create<Guid>();
        A.CallTo(() => _offerSubscriptionRepository.GetCompanyAssignedAppDataForProvidingCompanyUserAsync(notExistingAppId, A<Guid>._, _identity.CompanyId))
            .Returns(((Guid, OfferSubscriptionStatusId, Guid, string?, bool, RequesterData))default);

        var sut = new AppsBusinessLogic(_portalRepositories, null!, null!, null!, Options.Create(new AppsSettings()), null!);

        // Act
        async Task Act() => await sut.ActivateOwnCompanyProvidedAppSubscriptionAsync(notExistingAppId, Guid.NewGuid(), (_identity.UserId, _identity.CompanyId)).ConfigureAwait(false);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(Act);
    }

    [Fact]
    public async Task ActivateOwnCompanyProvidedAppSubscriptionAsync_IsNoMemberOfCompanyProvidingApp_ThrowsArgumentException()
    {
        // Arrange
        var identity = _fixture.Create<IdentityData>();
        var appId = _fixture.Create<Guid>();
        A.CallTo(() => _offerSubscriptionRepository.GetCompanyAssignedAppDataForProvidingCompanyUserAsync(appId, A<Guid>._, A<Guid>.That.Not.Matches(x => x == _identity.CompanyId)))
            .Returns((
                Guid.Empty,
                default,
                Guid.Empty,
                "app 1",
                false,
                new RequesterData(string.Empty, string.Empty, string.Empty)));

        var sut = new AppsBusinessLogic(_portalRepositories, null!, null!, null!, Options.Create(new AppsSettings()), null!);

        // Act
        async Task Act() => await sut.ActivateOwnCompanyProvidedAppSubscriptionAsync(appId, Guid.NewGuid(), (identity.UserId, identity.CompanyId)).ConfigureAwait(false);

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
        A.CallTo(() => _offerSubscriptionRepository.GetCompanyAssignedAppDataForProvidingCompanyUserAsync(appId, companyId, _identity.CompanyId))
            .Returns((
                offerSubscription.Id,
                offerSubscription.OfferSubscriptionStatusId,
                offerSubscription.RequesterId,
                "app 1",
                true,
                new RequesterData(string.Empty, string.Empty, string.Empty)));

        var sut = new AppsBusinessLogic(_portalRepositories, null!, null!, null!, Options.Create(new AppsSettings()), null!);

        // Act
        async Task Act() => await sut.ActivateOwnCompanyProvidedAppSubscriptionAsync(appId, companyId, (_identity.UserId, _identity.CompanyId)).ConfigureAwait(false);

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
        A.CallTo(() => _offerSubscriptionRepository.GetCompanyAssignedAppDataForProvidingCompanyUserAsync(appId, A<Guid>._, _identity.CompanyId))
            .Returns((
                offerSubscription.Id,
                offerSubscription.OfferSubscriptionStatusId,
                offerSubscription.RequesterId,
                "app 1",
                true,
                new RequesterData(string.Empty, string.Empty, string.Empty)));

        var sut = new AppsBusinessLogic(_portalRepositories, null!, null!, null!, Options.Create(new AppsSettings()), null!);

        // Act
        await sut.ActivateOwnCompanyProvidedAppSubscriptionAsync(appId, Guid.NewGuid(), (_identity.UserId, _identity.CompanyId)).ConfigureAwait(false);

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
        A.CallTo(() => _offerSubscriptionRepository.GetCompanyAssignedAppDataForProvidingCompanyUserAsync(appId, A<Guid>._, _identity.CompanyId))
            .Returns((
                offerSubscription.Id,
                offerSubscription.OfferSubscriptionStatusId,
                offerSubscription.RequesterId,
                "app 1",
                true,
                new RequesterData("test@email.com", "tony", "gilbert")));

        var sut = new AppsBusinessLogic(_portalRepositories, null!, null!, null!, Options.Create(new AppsSettings()), _mailingService);

        // Act
        await sut.ActivateOwnCompanyProvidedAppSubscriptionAsync(appId, Guid.NewGuid(), (_identity.UserId, _identity.CompanyId)).ConfigureAwait(false);

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
            AppImageDocumentTypeIds = _fixture.CreateMany<DocumentTypeId>(),
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

        A.CallTo(() => _offerRepository.GetProvidedOffersData(A<OfferTypeId>._, A<Guid>._))
            .Returns(data.AsAsyncEnumerable());

        var sut = new AppsBusinessLogic(_portalRepositories, null!, null!, null!, _fixture.Create<IOptions<AppsSettings>>(), _mailingService);

        //Act
        var result = await sut.GetCompanyProvidedAppsDataForUserAsync(_identity.CompanyId).ToListAsync().ConfigureAwait(false);

        //Assert
        result.Should().HaveSameCount(data);
    }

    #endregion

    #region GetCompanySubscribedAppSubscriptionStatusesForUserAsync

    [Fact]
    public async Task GetCompanySubscribedAppSubscriptionStatusesForUserAsync_ReturnsExpected()
    {
        // Arrange
        var identity = _fixture.Create<IdentityData>();
        var data = _fixture.CreateMany<OfferSubscriptionStatusDetailData>(5).ToImmutableArray();
        var pagination = new Pagination.Response<OfferSubscriptionStatusDetailData>(new Pagination.Metadata(data.Count(), 1, 0, data.Count()), data);
        A.CallTo(() => _offerService.GetCompanySubscribedOfferSubscriptionStatusesForUserAsync(A<int>._, A<int>._, A<Guid>._, A<OfferTypeId>._, A<DocumentTypeId>._))
            .Returns(pagination);

        var sut = new AppsBusinessLogic(_portalRepositories, null!, _offerService, null!, _fixture.Create<IOptions<AppsSettings>>(), _mailingService);

        // Act
        var result = await sut.GetCompanySubscribedAppSubscriptionStatusesForUserAsync(0, 10, identity.CompanyId).ConfigureAwait(false);

        // Assert
        result.Meta.NumberOfElements.Should().Be(5);
        result.Content.Should().HaveCount(5);
        A.CallTo(() => _offerService.GetCompanySubscribedOfferSubscriptionStatusesForUserAsync(0, 10, identity.CompanyId, OfferTypeId.APP, DocumentTypeId.APP_LEADIMAGE)).MustHaveHappenedOnceExactly();
    }

    #endregion

    #region GetSubscriptionDetailForProvider

    [Fact]
    public async Task GetSubscriptionDetailForProvider_ReturnsExpected()
    {
        // Arrange
        var appId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var data = _fixture.Create<AppProviderSubscriptionDetailData>();
        var settings = new AppsSettings
        {
            CompanyAdminRoles = new[]
            {
                new UserRoleConfig("ClientTest", new[] {"Test"})
            }
        };
        A.CallTo(() => _offerService.GetAppSubscriptionDetailsForProviderAsync(A<Guid>._, A<Guid>._, A<Guid>._, A<OfferTypeId>._, A<IEnumerable<UserRoleConfig>>._))
            .Returns(data);
        var sut = new AppsBusinessLogic(null!, null!, _offerService, null!, Options.Create(settings), null!);

        // Act
        var result = await sut.GetSubscriptionDetailForProvider(appId, subscriptionId, companyId).ConfigureAwait(false);

        // Assert
        result.Should().Be(data);
        A.CallTo(() => _offerService.GetAppSubscriptionDetailsForProviderAsync(appId, subscriptionId, companyId, OfferTypeId.APP, A<IEnumerable<UserRoleConfig>>._))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region GetSubscriptionDetailForSubscriber

    [Fact]
    public async Task GetSubscriptionDetailForSubscriber_ReturnsExpected()
    {
        // Arrange
        var appId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var settings = new AppsSettings
        {
            CompanyAdminRoles = new[]
            {
                new UserRoleConfig("ClientTest", new[] {"Test"})
            }
        };

        var data = _fixture.Create<SubscriberSubscriptionDetailData>();

        A.CallTo(() => _offerService.GetSubscriptionDetailsForSubscriberAsync(A<Guid>._, A<Guid>._, A<Guid>._, A<OfferTypeId>._, A<IEnumerable<UserRoleConfig>>._))
            .Returns(data);

        var sut = new AppsBusinessLogic(null!, null!, _offerService, null!, Options.Create(settings), null!);

        // Act
        var result = await sut.GetSubscriptionDetailForSubscriber(appId, subscriptionId, companyId).ConfigureAwait(false);

        // Assert
        result.Should().Be(data);
        A.CallTo(() => _offerService.GetSubscriptionDetailsForSubscriberAsync(appId, subscriptionId, companyId, OfferTypeId.APP, A<IEnumerable<UserRoleConfig>>._))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region GetAppDetailsByIdAsync

    [Fact]
    public async Task GetAppDetailsByIdAsync_ReturnsExpected()
    {
        // Arrange
        var appId = Guid.NewGuid();
        var language = "it";
        var documentData = new DocumentTypeData[]
        {
            new(DocumentTypeId.APP_CONTRACT, Guid.NewGuid(), _fixture.Create<string>()),
            new(DocumentTypeId.APP_TECHNICAL_INFORMATION, Guid.NewGuid(), _fixture.Create<string>()),
            new(DocumentTypeId.APP_CONTRACT, Guid.NewGuid(), _fixture.Create<string>()),
            new(DocumentTypeId.APP_TECHNICAL_INFORMATION, Guid.NewGuid(), _fixture.Create<string>()),
            new(DocumentTypeId.APP_CONTRACT, Guid.NewGuid(), _fixture.Create<string>()),
        };
        var technicalUserProfiles = new TechnicalUserRoleData[]
        {
            new(Guid.NewGuid(), _fixture.CreateMany<string>(3).ToImmutableArray()),
            new(Guid.NewGuid(), _fixture.CreateMany<string>(3).ToImmutableArray())
        };
        var data = _fixture.Build<OfferDetailsData>()
            .With(x => x.Documents, documentData)
            .With(x => x.TechnicalUserProfile, technicalUserProfiles)
            .Create();
        A.CallTo(() => _offerRepository.GetOfferDetailsByIdAsync(A<Guid>._, A<Guid>._, A<string?>._, A<string>._!, A<OfferTypeId>._))
            .Returns(data);

        var sut = new AppsBusinessLogic(_portalRepositories, null!, _offerService, null!, Options.Create(new AppsSettings()), null!);

        // Act
        var result = await sut.GetAppDetailsByIdAsync(appId, _identity.CompanyId, language).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _offerRepository.GetOfferDetailsByIdAsync(appId, _identity.CompanyId, language, Constants.DefaultLanguage, OfferTypeId.APP))
            .MustHaveHappenedOnceExactly();

        result.Id.Should().Be(data.Id);
        result.Title.Should().Be(data.Title);
        result.LeadPictureId.Should().Be(data.LeadPictureId);
        result.Images.Should().ContainInOrder(data.Images);
        result.ProviderUri.Should().Be(data.ProviderUri);
        result.Provider.Should().Be(data.Provider);
        result.ContactEmail.Should().Be(data.ContactEmail);
        result.ContactNumber.Should().Be(data.ContactNumber);
        result.UseCases.Should().ContainInOrder(data.UseCases);
        result.LongDescription.Should().Be(data.LongDescription);
        result.LicenseType.Should().Be(data.LicenseTypeId);
        result.Price.Should().Be(data.Price);
        result.Tags.Should().ContainInOrder(data.Tags);
        result.IsSubscribed.Should().Be(result.IsSubscribed);
        result.Languages.Should().ContainInOrder(data.Languages);
        result.Documents.Should().HaveCount(2).And.Satisfy(
            x => x.Key == DocumentTypeId.APP_CONTRACT &&
                x.Value.Count() == 3 &&
                x.Value.Contains(new DocumentData(documentData[0].DocumentId, documentData[0].DocumentName)) &&
                x.Value.Contains(new DocumentData(documentData[2].DocumentId, documentData[2].DocumentName)) &&
                x.Value.Contains(new DocumentData(documentData[4].DocumentId, documentData[4].DocumentName)),
            x => x.Key == DocumentTypeId.APP_TECHNICAL_INFORMATION &&
                x.Value.Count() == 2 &&
                x.Value.Contains(new DocumentData(documentData[1].DocumentId, documentData[1].DocumentName)) &&
                x.Value.Contains(new DocumentData(documentData[3].DocumentId, documentData[3].DocumentName))
        );
        result.PrivacyPolicies.Should().ContainInOrder(data.PrivacyPolicies);
        result.IsSingleInstance.Should().Be(data.IsSingleInstance);
        result.TechnicalUserProfile.Should().HaveCount(2).And.Satisfy(
            x => x.Key == technicalUserProfiles[0].TechnicalUserProfileId && x.Value.SequenceEqual(technicalUserProfiles[0].UserRoles),
            x => x.Key == technicalUserProfiles[1].TechnicalUserProfileId && x.Value.SequenceEqual(technicalUserProfiles[1].UserRoles)
        );
    }

    [Fact]
    public async Task GetAppDetailsByIdAsync_WithNullProperties_ReturnsExpected()
    {
        // Arrange
        var appId = Guid.NewGuid();
        var data = _fixture.Build<OfferDetailsData>()
            .With(x => x.Title, (string?)null)
            .With(x => x.ProviderUri, (string?)null)
            .With(x => x.LongDescription, (string?)null)
            .With(x => x.Price, (string?)null)
            .With(x => x.IsSubscribed, (OfferSubscriptionStatusId)default)
            .Create();
        A.CallTo(() => _offerRepository.GetOfferDetailsByIdAsync(A<Guid>._, A<Guid>._, A<string?>._, A<string>._!, A<OfferTypeId>._))
            .Returns(data);

        var sut = new AppsBusinessLogic(_portalRepositories, null!, _offerService, null!, Options.Create(new AppsSettings()), null!);

        // Act
        var result = await sut.GetAppDetailsByIdAsync(appId, _identity.CompanyId, null).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _offerRepository.GetOfferDetailsByIdAsync(appId, _identity.CompanyId, null, Constants.DefaultLanguage, OfferTypeId.APP))
            .MustHaveHappenedOnceExactly();

        result.Title.Should().Be(Constants.ErrorString);
        result.ProviderUri.Should().Be(Constants.ErrorString);
        result.LongDescription.Should().Be(Constants.ErrorString);
        result.Price.Should().Be(Constants.ErrorString);
        result.IsSubscribed.Should().BeNull();
    }

    #endregion
}
