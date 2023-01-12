/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Mailing.SendMail;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Service;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
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

        var sut = new AppsBusinessLogic(_portalRepositories, A.Fake<IOfferSubscriptionService>(), A.Fake<IOfferService>(), Options.Create(new AppsSettings()), A.Fake<MailingService>());

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

        var sut = new AppsBusinessLogic(_portalRepositories, A.Fake<IOfferSubscriptionService>(), A.Fake<IOfferService>(), Options.Create(new AppsSettings()), A.Fake<MailingService>());


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
        var results = _fixture.CreateMany<ValueTuple<Guid, string?, string, IEnumerable<string>, Guid, string?, string?>>(5);
        A.CallTo(() => _offerRepository.GetAllActiveAppsAsync(A<string>._)).Returns(results.ToAsyncEnumerable());

        var sut = new AppsBusinessLogic(_portalRepositories, null!, null!, A.Fake<IOptions<AppsSettings>>(), A.Fake<MailingService>());

        // Act
        var result = await sut.GetAllActiveAppsAsync().ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().HaveCount(5);
    }
    
    #endregion

    #region GetAllUserUserBusinessApps

    [Fact]
    public async Task GetAllUserUserBusinessAppsAsync_WithValidData_ReturnsExpectedData()
    {
        // Arrange
        var appData = _fixture.CreateMany<(Guid, string?, string, Guid, string)>(5);
        A.CallTo(() => _offerSubscriptionRepository.GetAllBusinessAppDataForUserIdAsync(A<string>._)).Returns(appData.ToAsyncEnumerable());
        var sut = new AppsBusinessLogic(_portalRepositories, A.Fake<IOfferSubscriptionService>(), A.Fake<IOfferService>(), Options.Create(new AppsSettings()), A.Fake<MailingService>());

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
        var sut = new AppsBusinessLogic(null!, null!, offerService, Options.Create(new AppsSettings()), A.Fake<MailingService>());

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
            .ReturnsLazily(() => offerSubscriptionId);
        var sut = new AppsBusinessLogic(A.Fake<IPortalRepositories>(), offerSubscriptionService , A.Fake<IOfferService>(), Options.Create(new AppsSettings()), A.Fake<MailingService>());

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
        var offerService = A.Fake<IOfferService>();
        A.CallTo(() => offerService.AutoSetupServiceAsync(A<OfferAutoSetupData>._, A<IDictionary<string, IEnumerable<string>>>._, A<IDictionary<string, IEnumerable<string>>>._, A<string>._, A<OfferTypeId>._, A<string>._))
            .ReturnsLazily(() => offerAutoSetupResponseData);
        var data = new OfferAutoSetupData(Guid.NewGuid(), "https://www.offer.com");

        var sut = new AppsBusinessLogic(null!, null!, offerService, _fixture.Create<IOptions<AppsSettings>>(), A.Fake<MailingService>());

        // Act
        var result = await sut.AutoSetupAppAsync(data, IamUserId).ConfigureAwait(false);

        // Assert
        result.Should().Be(offerAutoSetupResponseData);
    }

    #endregion

    #region GetCompanyProvidedAppSubscriptionStatusesForUser
    
    [Fact]
    public async Task GetCompanyProvidedAppSubscriptionStatusesForUserAsync_ReturnsExpectedCount()
    {
        // Arrange
        var iamUserId = _fixture.Create<string>();
        var data = _fixture.CreateMany<OfferCompanySubscriptionStatusData>(5);
        A.CallTo(() => _offerSubscriptionRepository.GetOwnCompanyProvidedOfferSubscriptionStatusesUntrackedAsync(iamUserId, OfferTypeId.APP, default, OfferSubscriptionStatusId.ACTIVE))
            .Returns((skip, take) => Task.FromResult(new Pagination.Source<OfferCompanySubscriptionStatusData>(data.Count(), data.Skip(skip).Take(take)))!);

        var appsSettings = new AppsSettings
        {
            ApplicationsMaxPageSize = 15
        };
        var sut = new AppsBusinessLogic(_portalRepositories, A.Fake<IOfferSubscriptionService>(), A.Fake<IOfferService>(), Options.Create(appsSettings), A.Fake<MailingService>());

        // Act
        var result = await sut.GetCompanyProvidedAppSubscriptionStatusesForUserAsync(0, 10, iamUserId, null, null).ConfigureAwait(false);

        // Assert
        result.Content.Should().HaveCount(5);
    }

    #endregion
    
    #region DeclineAppRequest
    
    [Fact]
    public async Task DeclineAppRequestAsync_CallsExpected()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var data = new OfferDeclineRequest("Just a test");
        var settings = new AppsSettings
        {
            ServiceManagerRoles = _fixture.Create<Dictionary<string, IEnumerable<string>>>(),
            BasePortalAddress = "test"
        };
        var sut = new AppsBusinessLogic(null!,null!, _offerService, Options.Create(settings), _mailingService);
     
        // Act
        await sut.DeclineAppRequestAsync(appId, IamUserId, data).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _offerService.DeclineOfferAsync(appId, IamUserId, data,
            OfferTypeId.APP, NotificationTypeId.APP_RELEASE_REJECTION,
            A<IDictionary<string, IEnumerable<string>>>._, A<string>._)).MustHaveHappenedOnceExactly();
    }
    
    #endregion
    
    #region ActivateOwnCompanyProvidedAppSubscription

    [Fact]
    public async Task ActivateOwnCompanyProvidedAppSubscriptionAsync_WithNotExistingApp_ThrowsNotFoundException()
    {
        // Arrange
        var notExistingAppId = _fixture.Create<Guid>();
        A.CallTo(() => _offerSubscriptionRepository.GetCompanyAssignedAppDataForProvidingCompanyUserAsync(notExistingAppId, A<Guid>._, IamUserId))
            .ReturnsLazily(() => new ValueTuple<Guid, OfferSubscriptionStatusId, Guid, string?, Guid, string?, string?>());
        
        var sut = new AppsBusinessLogic(_portalRepositories, null!, null!, _fixture.Create<IOptions<AppsSettings>>(), A.Fake<MailingService>());

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
            .ReturnsLazily(() => new ValueTuple<Guid, OfferSubscriptionStatusId, Guid, string?, Guid, string?, string?>(Guid.Empty, default, Guid.Empty, "app 1", Guid.Empty, null, null));
        
        var sut = new AppsBusinessLogic(_portalRepositories, null!, null!, _fixture.Create<IOptions<AppsSettings>>(), A.Fake<MailingService>());

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
            .ReturnsLazily(() => new ValueTuple<Guid, OfferSubscriptionStatusId, Guid, string?, Guid, string?, string?>(offerSubscription.Id, offerSubscription.OfferSubscriptionStatusId, offerSubscription.RequesterId, "app 1", Guid.NewGuid(), null, null));
        
        var sut = new AppsBusinessLogic(_portalRepositories, null!, null!, _fixture.Create<IOptions<AppsSettings>>(), A.Fake<MailingService>());

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
            .ReturnsLazily(() => new ValueTuple<Guid, OfferSubscriptionStatusId, Guid, string?, Guid, string?, string?>(offerSubscription.Id, offerSubscription.OfferSubscriptionStatusId, offerSubscription.RequesterId, "app 1", Guid.NewGuid(), null, null));
        
        var sut = new AppsBusinessLogic(_portalRepositories, null!, null!, _fixture.Create<IOptions<AppsSettings>>(), A.Fake<MailingService>());

        // Act
        await sut.ActivateOwnCompanyProvidedAppSubscriptionAsync(appId, Guid.NewGuid(), IamUserId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _notificationRepository.CreateNotification(A<Guid>._, NotificationTypeId.APP_SUBSCRIPTION_ACTIVATION, false, A<Action<PortalBackend.PortalEntities.Entities.Notification>?>._)).MustHaveHappenedOnceExactly();
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
            .ReturnsLazily(() => new ValueTuple<Guid, OfferSubscriptionStatusId, Guid, string?, Guid, string?, string?>(offerSubscription.Id, offerSubscription.OfferSubscriptionStatusId, offerSubscription.RequesterId, "app 1", Guid.NewGuid(), "test@email.com", "tony"));
        
        var sut = new AppsBusinessLogic(_portalRepositories, null!, null!, _fixture.Create<IOptions<AppsSettings>>(), _mailingService);

        // Act
        await sut.ActivateOwnCompanyProvidedAppSubscriptionAsync(appId, Guid.NewGuid(), IamUserId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _notificationRepository.CreateNotification(A<Guid>._, NotificationTypeId.APP_SUBSCRIPTION_ACTIVATION, false, A<Action<PortalBackend.PortalEntities.Entities.Notification>?>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _mailingService.SendMails("test@email.com", A<IDictionary<string, string>>._, A<IEnumerable<string>>.That.Matches(x => x.Count() == 1 && x.First() == "subscription-activation"))).MustHaveHappenedOnceExactly();
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
        var sut = new AppsBusinessLogic(null!,null!, _offerService, Options.Create(settings), _mailingService);
        
        // Act
        await sut.DeactivateOfferbyAppIdAsync(appId, IamUserId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _offerService.DeactivateOfferIdAsync(appId, IamUserId, OfferTypeId.APP)).MustHaveHappenedOnceExactly();
    }

    #endregion

    [Fact]
    public async Task GetAppImageDocumentContentAsync_ReturnsExpectedresult()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var documentId = _fixture.Create<Guid>();
        var data = _fixture.Create<byte[]>();
        var settings = new AppsSettings
        {
            AppImageDocumentTypeIds = _fixture.Create<IEnumerable<DocumentTypeId>>(),
        };
        A.CallTo(() => _documentRepository.GetOfferImageDocumentContentAsync(appId, documentId, settings.AppImageDocumentTypeIds, OfferTypeId.APP))
            .Returns((true, true, true, data, true));

        var sut = new AppsBusinessLogic(_portalRepositories, A.Fake<IOfferSubscriptionService>(), A.Fake<IOfferService>(), Options.Create(settings), A.Fake<MailingService>());

        // Act
        var result = await sut.GetAppImageDocumentContentAsync(appId, documentId).ConfigureAwait(false);

        // Assert
        result.Content.Should().BeSameAs(data);
        A.CallTo(() => _documentRepository.GetOfferImageDocumentContentAsync(appId, documentId, settings.AppImageDocumentTypeIds, OfferTypeId.APP)).MustHaveHappened();
    }

    [Fact]
    public async Task GetAppImageDocumentContentAsync_WithInvalidDocumentTypeId_ThrowsArgumentException()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var documentId = _fixture.Create<Guid>();
        var data = _fixture.Create<byte[]>();
        var settings = new AppsSettings
        {
            AppImageDocumentTypeIds = _fixture.Create<IEnumerable<DocumentTypeId>>(),
        };
        A.CallTo(() => _documentRepository.GetOfferImageDocumentContentAsync(appId, documentId, settings.AppImageDocumentTypeIds, OfferTypeId.APP))
            .Returns((false, true, true, null, true));

        var sut = new AppsBusinessLogic(_portalRepositories, A.Fake<IOfferSubscriptionService>(), A.Fake<IOfferService>(), Options.Create(settings), A.Fake<MailingService>());

        // Act
        async Task Act() => await sut.GetAppImageDocumentContentAsync(appId, documentId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be($"Document {documentId} can not get retrieved. Document type not supported.");
    }

    [Fact]
    public async Task GetAppImageDocumentContentAsync_WithOfferNotLinkToDocument_ThrowsArgumentException()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var documentId = _fixture.Create<Guid>();
        var data = _fixture.Create<byte[]>();
        var settings = new AppsSettings
        {
            AppImageDocumentTypeIds = _fixture.Create<IEnumerable<DocumentTypeId>>(),
        };
        A.CallTo(() => _documentRepository.GetOfferImageDocumentContentAsync(appId, documentId, settings.AppImageDocumentTypeIds, OfferTypeId.APP))
            .Returns((true, false, true, null, true));

        var sut = new AppsBusinessLogic(_portalRepositories, A.Fake<IOfferSubscriptionService>(), A.Fake<IOfferService>(), Options.Create(settings), A.Fake<MailingService>());

        // Act
        async Task Act() => await sut.GetAppImageDocumentContentAsync(appId, documentId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be($"Document {documentId} and app id {appId} do not match.");
    }

    #region GetCompanyProvidedAppsDataForUserAsync

    [Fact]
    public async Task GetCompanyProvidedAppsDataForUserAsync_ReturnsExpectedCount()
    {
        //Arrange
        var data = new AsyncEnumerableStub<AllOfferData>(_fixture.CreateMany<AllOfferData>(5));

        A.CallTo(() => _offerRepository.GetProvidedOffersData(A<OfferTypeId>._, A<string>._))
            .Returns(data.AsAsyncEnumerable());

        var sut = new AppsBusinessLogic(_portalRepositories, null!, null!, _fixture.Create<IOptions<AppsSettings>>(), _mailingService);

        //Act
        var result = await sut.GetCompanyProvidedAppsDataForUserAsync(IamUserId).ToListAsync().ConfigureAwait(false);

        //Assert
        result.Should().HaveSameCount(data);
    }

    #endregion
    
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
}
