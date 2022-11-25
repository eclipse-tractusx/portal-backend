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
using Microsoft.Extensions.Logging;
using Org.CatenaX.Ng.Portal.Backend.Framework.ErrorHandling;
using Org.CatenaX.Ng.Portal.Backend.Framework.Models;
using Org.CatenaX.Ng.Portal.Backend.Mailing.SendMail;
using Org.CatenaX.Ng.Portal.Backend.Offers.Library.Service;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Xunit;

namespace Org.CatenaX.Ng.Portal.Backend.Offers.Library.Tests.Service;

public class OfferSubscriptionServiceTests
{
    private const string BasePortalUrl = "http//base-url.com";
    private const string AccessToken = "THISISAACCESSTOKEN";
    private const string ClientId = "Client1";
    
    private readonly string _notAssignedCompanyIdUser;
    private readonly string _noBpnSetUserId;
    private readonly string _iamUserId;
    private readonly string _existingActiveSubscriptionUserId;
    private readonly string _existingInactiveSubscriptionUserId;
    private readonly Guid _companyUserId;
    private readonly Guid _companyId;
    private readonly Guid _existingActiveSubscriptionCompanyId;
    private readonly Guid _existingInactiveSubscriptionCompanyId;
    private readonly Guid _existingOfferId;
    private readonly Guid _existingOfferWithFailingAutoSetupId;
    private readonly Guid _existingOfferWithoutDetailsFilled;
    private readonly Guid _validSubscriptionId;
    private readonly Guid _newOfferSubscriptionId;
    private readonly Guid _userRoleId;
    private readonly IFixture _fixture;
    private readonly IAgreementRepository _agreementRepository;
    private readonly IConsentRepository _consentRepository;
    private readonly IConsentAssignedOfferSubscriptionRepository _consentAssignedOfferSubscriptionRepository;
    private readonly IOfferRepository _offerRepository;
    private readonly IOfferSubscriptionsRepository _offerSubscriptionsRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IUserRepository _userRepository;
    private readonly IUserRolesRepository _userRolesRepository;
    private readonly IOfferSetupService _offerSetupService;
    private readonly IMailingService _mailingService;
    private readonly Dictionary<string,IEnumerable<string>> _serviceManagerRoles;

    public OfferSubscriptionServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _iamUserId = _fixture.Create<string>();
        _existingActiveSubscriptionUserId = _fixture.Create<string>();
        _existingInactiveSubscriptionUserId = _fixture.Create<string>();
        _companyUserId = _fixture.Create<Guid>();
        _companyId = _fixture.Create<Guid>();
        _existingActiveSubscriptionCompanyId = _fixture.Create<Guid>();
        _existingInactiveSubscriptionCompanyId = _fixture.Create<Guid>();
        _notAssignedCompanyIdUser = _fixture.Create<string>();
        _noBpnSetUserId = _fixture.Create<string>();
        _existingOfferId = _fixture.Create<Guid>();
        _existingOfferWithFailingAutoSetupId = _fixture.Create<Guid>();
        _existingOfferWithoutDetailsFilled = _fixture.Create<Guid>();
        _validSubscriptionId = _fixture.Create<Guid>();
        _newOfferSubscriptionId = _fixture.Create<Guid>();
        _userRoleId = _fixture.Create<Guid>();
        _serviceManagerRoles = new Dictionary<string, IEnumerable<string>>
        {
            { ClientId, new[] { "Service Manager" } }
        };

        _portalRepositories = A.Fake<IPortalRepositories>();
        _agreementRepository = A.Fake<IAgreementRepository>();
        _consentRepository = A.Fake<IConsentRepository>();
        _consentAssignedOfferSubscriptionRepository = A.Fake<IConsentAssignedOfferSubscriptionRepository>();
        _offerRepository = A.Fake<IOfferRepository>();
        _offerSubscriptionsRepository = A.Fake<IOfferSubscriptionsRepository>();
        _notificationRepository = A.Fake<INotificationRepository>();
        _userRepository = A.Fake<IUserRepository>();
        _userRolesRepository = A.Fake<IUserRolesRepository>();
        _offerSetupService = A.Fake<IOfferSetupService>();
        _mailingService = A.Fake<IMailingService>();

        SetupRepositories();
        SetupServices();

        _fixture.Inject(_offerSetupService);
    }

    #region Add Offer Subscription

    [Theory]
    [InlineData(OfferTypeId.SERVICE)]
    [InlineData(OfferTypeId.APP)]
    public async Task AddOfferSubscription_WithExistingId_CreatesServiceSubscription(OfferTypeId offerTypeId)
    {
        // Arrange 
        var companyAssignedApps = new List<OfferSubscription>();
        A.CallTo(() => _offerSubscriptionsRepository.CreateOfferSubscription(A<Guid>._, A<Guid>._, A<OfferSubscriptionStatusId>._, A<Guid>._, A<Guid>._))
            .Invokes(x =>
            {
                var appId = x.Arguments.Get<Guid>("offerId");
                var companyId = x.Arguments.Get<Guid>("companyId");
                var appSubscriptionStatusId = x.Arguments.Get<OfferSubscriptionStatusId>("offerSubscriptionStatusId");
                var requesterId = x.Arguments.Get<Guid>("requesterId");
                var creatorId = x.Arguments.Get<Guid>("creatorId");

                var companyAssignedApp = new OfferSubscription(_newOfferSubscriptionId, appId, companyId, appSubscriptionStatusId, requesterId, creatorId);
                companyAssignedApps.Add(companyAssignedApp);
            });
        var notificationId = Guid.NewGuid();
        var notifications = new List<PortalBackend.PortalEntities.Entities.Notification>(); 
        A.CallTo(() => _notificationRepository.CreateNotification(A<Guid>._, A<NotificationTypeId>._, A<bool>._, A<Action<PortalBackend.PortalEntities.Entities.Notification>?>._))
            .Invokes(x =>
            {
                var receiverUserId = x.Arguments.Get<Guid>("receiverUserId");
                var notificationTypeId = x.Arguments.Get<NotificationTypeId>("notificationTypeId");
                var isRead = x.Arguments.Get<bool>("isRead");
                var setOptionalParameters = x.Arguments.Get< Action<PortalBackend.PortalEntities.Entities.Notification>?>("setOptionalParameters");

                var notification = new PortalBackend.PortalEntities.Entities.Notification(notificationId, receiverUserId, DateTimeOffset.UtcNow, notificationTypeId, isRead);
                setOptionalParameters?.Invoke(notification);
                notifications.Add(notification);
            });        
        var sut = new OfferSubscriptionService(_portalRepositories, _offerSetupService, _mailingService, A.Fake<ILogger<OfferSubscriptionService>>());

        // Act
        await sut.AddOfferSubscriptionAsync(_existingOfferId, new List<OfferAgreementConsentData>(), _iamUserId, AccessToken, _serviceManagerRoles, offerTypeId, BasePortalUrl).ConfigureAwait(false);

        // Assert
        companyAssignedApps.Should().HaveCount(1);
        notifications.Should().HaveCount(2);
        A.CallTo(() => _mailingService.SendMails(A<string>._, A<Dictionary<string, string>>._, A<List<string>>._)).MustHaveHappenedOnceExactly();
    }
    
    [Theory]
    [InlineData(OfferTypeId.SERVICE)]
    [InlineData(OfferTypeId.APP)]
    public async Task AddOfferSubscription_WithFailingAutoSetup_ReturnsExpectedResult(OfferTypeId offerTypeId)
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        var notifications = new List<PortalBackend.PortalEntities.Entities.Notification>(); 
        A.CallTo(() => _notificationRepository.CreateNotification(A<Guid>._, A<NotificationTypeId>._, A<bool>._, A<Action<PortalBackend.PortalEntities.Entities.Notification>?>._))
            .Invokes(x =>
            {
                var receiverUserId = x.Arguments.Get<Guid>("receiverUserId");
                var notificationTypeId = x.Arguments.Get<NotificationTypeId>("notificationTypeId");
                var isRead = x.Arguments.Get<bool>("isRead");
                var setOptionalParameters = x.Arguments.Get< Action<PortalBackend.PortalEntities.Entities.Notification>?>("setOptionalParameters");

                var notification = new PortalBackend.PortalEntities.Entities.Notification(notificationId, receiverUserId, DateTimeOffset.UtcNow, notificationTypeId, isRead);
                setOptionalParameters?.Invoke(notification);
                notifications.Add(notification);
            });
        var sut = new OfferSubscriptionService(_portalRepositories, _offerSetupService, _mailingService, A.Fake<ILogger<OfferSubscriptionService>>());

        // Act
        await sut.AddOfferSubscriptionAsync(_existingOfferWithFailingAutoSetupId, new List<OfferAgreementConsentData>(), _iamUserId, AccessToken, _serviceManagerRoles, offerTypeId, BasePortalUrl).ConfigureAwait(false);

        // Assert
        notifications.Should().ContainSingle();
        notifications.First().Content.Should().Contain("Error occured");
    }

    [Theory]
    [InlineData(OfferTypeId.SERVICE)]
    [InlineData(OfferTypeId.APP)]
    public async Task AddOfferSubscription_NotAssignedCompany_ThrowsException(OfferTypeId offerTypeId)
    {
        // Arrange
        var sut = new OfferSubscriptionService(_portalRepositories, _offerSetupService, _mailingService, A.Fake<ILogger<OfferSubscriptionService>>());

        // Act
        async Task Action() => await sut.AddOfferSubscriptionAsync(_existingOfferId, new List<OfferAgreementConsentData>(), _notAssignedCompanyIdUser, AccessToken, _serviceManagerRoles, offerTypeId, BasePortalUrl).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("iamUserId");
    }

    [Theory]
    [InlineData(OfferTypeId.SERVICE)]
    [InlineData(OfferTypeId.APP)]
    public async Task AddOfferSubscription_WithNotExistingId_ThrowsException(OfferTypeId offerTypeId)
    {
        // Arrange
        var notExistingServiceId = Guid.NewGuid();
        var sut = new OfferSubscriptionService(_portalRepositories, _offerSetupService, _mailingService, A.Fake<ILogger<OfferSubscriptionService>>());

        // Act
        async Task Action() => await sut.AddOfferSubscriptionAsync(notExistingServiceId, new List<OfferAgreementConsentData>(), _iamUserId, AccessToken, _serviceManagerRoles, offerTypeId, BasePortalUrl).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Action);
        ex.Message.Should().Be($"Offer {notExistingServiceId} does not exist");
    }
    
    [Theory]
    [InlineData(OfferTypeId.SERVICE)]
    [InlineData(OfferTypeId.APP)]
    public async Task AddOfferSubscription_NotAssignedCompanyUser_ThrowsException(OfferTypeId offerTypeId)
    {
        // Arrange
        var sut = new OfferSubscriptionService(_portalRepositories, _offerSetupService, _mailingService, A.Fake<ILogger<OfferSubscriptionService>>());

        // Act
        async Task Action() => await sut.AddOfferSubscriptionAsync(_existingOfferId, new List<OfferAgreementConsentData>(), Guid.NewGuid().ToString(), AccessToken, _serviceManagerRoles, offerTypeId, BasePortalUrl).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("iamUserId");
    }

    [Theory]
    [InlineData(OfferTypeId.SERVICE)]
    [InlineData(OfferTypeId.APP)]
    public async Task AddOfferSubscription_WithoutOfferProviderDetails_ThrowsException(OfferTypeId offerTypeId)
    {
        // Arrange
        var sut = new OfferSubscriptionService(_portalRepositories, _offerSetupService, _mailingService, A.Fake<ILogger<OfferSubscriptionService>>());

        // Act
        async Task Action() => await sut.AddOfferSubscriptionAsync(_existingOfferWithoutDetailsFilled, new List<OfferAgreementConsentData>(), _iamUserId, AccessToken, _serviceManagerRoles, offerTypeId, BasePortalUrl).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Action);
        ex.Message.Should().Be("The offer name has not been configured properly");
    }

    [Theory]
    [InlineData(OfferTypeId.SERVICE)]
    [InlineData(OfferTypeId.APP)]
    public async Task AddOfferSubscription_WithoutInvalidConsentData_ThrowsControllerArgumentException(OfferTypeId offerTypeId)
    {
        // Arrange
        var consentData = new List<OfferAgreementConsentData>
        {
            new(_fixture.Create<Guid>(), ConsentStatusId.ACTIVE)
        };
        var sut = new OfferSubscriptionService(_portalRepositories, _offerSetupService, _mailingService, A.Fake<ILogger<OfferSubscriptionService>>());

        // Act
        async Task Action() => await sut.AddOfferSubscriptionAsync(_existingOfferId, consentData, _iamUserId, AccessToken, _serviceManagerRoles, offerTypeId, BasePortalUrl).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("offerAgreementConsentData");
    }

    [Theory]
    [InlineData(OfferTypeId.SERVICE)]
    [InlineData(OfferTypeId.APP)]
    public async Task AddOfferSubscription_WithoutBuisnessPartnerNumber_ThrowsConflictException(OfferTypeId offerTypeId)
    {
        // Arrange
        var sut = new OfferSubscriptionService(_portalRepositories, _offerSetupService, _mailingService, A.Fake<ILogger<OfferSubscriptionService>>());

        // Act
        async Task Action() => await sut.AddOfferSubscriptionAsync(_existingOfferId, new List<OfferAgreementConsentData>(), _noBpnSetUserId, AccessToken, _serviceManagerRoles, offerTypeId, BasePortalUrl).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Action);
        ex.Message.Should().Contain("has no BusinessPartnerNumber assigned");
    }

    #endregion

    #region APP - Specialcases
    
    [Fact]
    public async Task AddOfferSubscription_WithExistingActiveSubscription_ThrowsConflictException()
    {
        // Arrange 
        A.CallTo(() => _offerSubscriptionsRepository.GetOfferSubscriptionStateForCompanyAsync(_existingOfferId, _existingActiveSubscriptionCompanyId, A<OfferTypeId>._))
            .ReturnsLazily(() => new ValueTuple<Guid, OfferSubscriptionStatusId>(Guid.NewGuid(), OfferSubscriptionStatusId.ACTIVE));
        var sut = new OfferSubscriptionService(_portalRepositories, _offerSetupService, _mailingService, A.Fake<ILogger<OfferSubscriptionService>>());

        // Act
        async Task Act() => await sut.AddOfferSubscriptionAsync(_existingOfferId, new List<OfferAgreementConsentData>(), _existingActiveSubscriptionUserId, AccessToken, _serviceManagerRoles, OfferTypeId.APP, BasePortalUrl).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Contain(" is already subscribed to ");
    }
    
    [Fact]
    public async Task AddOfferSubscription_WithExistingInactiveSubscription_UpdatesState()
    {
        // Arrange 
        A.CallTo(() => _offerSubscriptionsRepository.GetOfferSubscriptionStateForCompanyAsync(_existingOfferId, _existingInactiveSubscriptionCompanyId, A<OfferTypeId>._))
            .ReturnsLazily(() => new ValueTuple<Guid, OfferSubscriptionStatusId>(Guid.NewGuid(), OfferSubscriptionStatusId.INACTIVE));
        var sut = new OfferSubscriptionService(_portalRepositories, _offerSetupService, _mailingService, A.Fake<ILogger<OfferSubscriptionService>>());

        // Act
        await sut.AddOfferSubscriptionAsync(_existingOfferId, new List<OfferAgreementConsentData>(), _existingInactiveSubscriptionUserId, AccessToken, _serviceManagerRoles, OfferTypeId.APP, BasePortalUrl).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _mailingService.SendMails(A<string>._, A<Dictionary<string, string>>._, A<List<string>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerSubscriptionsRepository.AttachAndModifyOfferSubscription(A<Guid>._, A<Action<OfferSubscription>>._)).MustHaveHappenedOnceExactly();
    }
    
    #endregion
    
    #region Setup

    private void SetupRepositories()
    {
        var offerDetailData = _fixture.CreateMany<ServiceOverviewData>(5);
        var paginationResult = (int skip, int take) => Task.FromResult(new Pagination.Source<ServiceOverviewData>(offerDetailData.Count(), offerDetailData.Skip(skip).Take(take)));
        var offerDetail = _fixture.Build<OfferDetailData>()
            .With(x => x.Id, _existingOfferId)
            .Create();

        A.CallTo(() => _userRepository.GetOwnCompanyInformationWithCompanyUserIdAndEmailAsync(_iamUserId))
            .ReturnsLazily(() => (
                new CompanyInformationData(_companyId, "The Company", "DE", "BPM00000001"),
                _companyUserId,
                "test@mail.de"));
        A.CallTo(() => _userRepository.GetOwnCompanyInformationWithCompanyUserIdAndEmailAsync(_notAssignedCompanyIdUser))
            .ReturnsLazily(() => (
                new CompanyInformationData(Guid.Empty, "The Company", "DE", "BPM00000001"),
                _companyUserId,
                "test@mail.de"));
        A.CallTo(() => _userRepository.GetOwnCompanyInformationWithCompanyUserIdAndEmailAsync(_noBpnSetUserId))
            .ReturnsLazily(() => (
                new CompanyInformationData(_companyId, "The Company", "DE", null),
                _companyUserId,
                "test@mail.de"));
        A.CallTo(() => _userRepository.GetOwnCompanyInformationWithCompanyUserIdAndEmailAsync(_existingActiveSubscriptionUserId))
            .ReturnsLazily(() => (
                new CompanyInformationData(_existingActiveSubscriptionCompanyId, "The Company", "DE", "BPM00000001"),
                _companyUserId,
                "test@mail.de"));
        A.CallTo(() => _userRepository.GetOwnCompanyInformationWithCompanyUserIdAndEmailAsync(_existingInactiveSubscriptionUserId))
            .ReturnsLazily(() => (
                new CompanyInformationData(_existingInactiveSubscriptionCompanyId, "The Company", "DE", "BPM00000001"),
                _companyUserId,
                "test@mail.de"));
        A.CallTo(() => _userRepository.GetOwnCompanyInformationWithCompanyUserIdAndEmailAsync(A<string>.That.Not.Matches(x => x == _iamUserId || x == _notAssignedCompanyIdUser || x == _noBpnSetUserId || x == _existingActiveSubscriptionUserId || x == _existingInactiveSubscriptionUserId)))
            .ReturnsLazily(() => (
                new CompanyInformationData(_companyId, "The Company", "DE", "BPM00000001"),
                Guid.Empty,
                "test@mail.de"));
        A.CallTo(() => _userRepository.GetServiceProviderCompanyUserWithRoleIdAsync(A<Guid>.That.Matches(x => x == _existingOfferId), A<List<Guid>>.That.Matches(x => x.Count == 1 && x.All(y => y == _userRoleId))))
            .Returns(new List<Guid> { _companyUserId }.ToAsyncEnumerable());
        A.CallTo(() => _userRolesRepository.GetUserRoleIdsUntrackedAsync(A<IDictionary<string, IEnumerable<string>>>.That.Matches(x => x[ClientId].First() == "Service Manager")))
            .Returns(new List<Guid> { _userRoleId }.ToAsyncEnumerable());
        A.CallTo(() => _userRolesRepository.GetUserRoleIdsUntrackedAsync(A<IDictionary<string, IEnumerable<string>>>.That.Not.Matches(x => x[ClientId].First() == "Service Manager")))
            .Returns(new List<Guid>().ToAsyncEnumerable());

        
        A.CallTo(() => _offerRepository.GetActiveServicesPaginationSource(A<ServiceOverviewSorting>._, A<ServiceTypeId>._))
            .Returns(paginationResult);
        A.CallTo(() => _offerRepository.GetOfferDetailByIdUntrackedAsync(_existingOfferId, A<string>.That.Matches(x => x == "en"), A<string>._, A<OfferTypeId>._))
            .ReturnsLazily(() => offerDetail with {OfferSubscriptionDetailData = new []
            {
                new OfferSubscriptionStateDetailData(Guid.NewGuid(), OfferSubscriptionStatusId.ACTIVE)
            }});
        A.CallTo(() => _offerRepository.GetOfferDetailByIdUntrackedAsync(A<Guid>.That.Not.Matches(x => x == _existingOfferId), A<string>._, A<string>._, A<OfferTypeId>._))
            .ReturnsLazily(() => (OfferDetailData?)null);
        
        A.CallTo(() => _offerRepository.GetOfferProviderDetailsAsync(A<Guid>.That.Matches(x => x == _existingOfferId), A<OfferTypeId>._))
            .ReturnsLazily(() => new OfferProviderDetailsData("Test Offer", "Test Company", "provider@mail.de", new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"), "https://www.testurl.com"));
        A.CallTo(() => _offerRepository.GetOfferProviderDetailsAsync(A<Guid>.That.Matches(x => x == _existingOfferWithFailingAutoSetupId), A<OfferTypeId>._))
            .ReturnsLazily(() => new OfferProviderDetailsData("Test Offer", "Test Company", "provider@mail.de", new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"), "https://www.fail.com"));
        A.CallTo(() => _offerRepository.GetOfferProviderDetailsAsync(A<Guid>.That.Matches(x => x == _existingOfferWithoutDetailsFilled), A<OfferTypeId>._))
            .ReturnsLazily(() => new OfferProviderDetailsData(null, "Test Company", null, new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"), "https://www.fail.com"));
        A.CallTo(() => _offerRepository.GetOfferProviderDetailsAsync(A<Guid>.That.Not.Matches(x => x == _existingOfferId || x == _existingOfferWithFailingAutoSetupId || x == _existingOfferWithoutDetailsFilled), A<OfferTypeId>._))
            .ReturnsLazily(() => (OfferProviderDetailsData?)null);
        
        var offerSubscription = _fixture.Create<OfferSubscription>();
        A.CallTo(() => _offerSubscriptionsRepository.GetSubscriptionDetailDataForOwnUserAsync(
                A<Guid>.That.Matches(x => x == _validSubscriptionId),
                A<string>.That.Matches(x => x == _iamUserId),
                A<OfferTypeId>._))
            .ReturnsLazily(() =>
                new SubscriptionDetailData(_existingOfferId, "Super Offer", OfferSubscriptionStatusId.ACTIVE));
        A.CallTo(() => _offerSubscriptionsRepository.GetSubscriptionDetailDataForOwnUserAsync(
                A<Guid>.That.Not.Matches(x => x == _validSubscriptionId),
                A<string>.That.Matches(x => x == _iamUserId),
                A<OfferTypeId>._))
            .ReturnsLazily(() => (SubscriptionDetailData?)null);
        A.CallTo(() => _offerSubscriptionsRepository.GetCompanyIdWithAssignedOfferForCompanyUserAndSubscriptionAsync(
                A<Guid>.That.Matches(x => x == _existingOfferId), A<string>.That.Matches(x => x == _iamUserId), A<OfferTypeId>._))
            .ReturnsLazily(() => new ValueTuple<Guid, OfferSubscription?, Guid>(_companyId, offerSubscription, _companyUserId));
        A.CallTo(() => _offerSubscriptionsRepository.GetCompanyIdWithAssignedOfferForCompanyUserAndSubscriptionAsync(
                A<Guid>.That.Not.Matches(x => x == _existingOfferId), A<string>.That.Matches(x => x == _iamUserId),
                A<OfferTypeId>._))
            .ReturnsLazily(() => new ValueTuple<Guid, OfferSubscription?, Guid>(_companyId, null, _companyUserId));
        A.CallTo(() => _offerSubscriptionsRepository.GetCompanyIdWithAssignedOfferForCompanyUserAndSubscriptionAsync(
                A<Guid>.That.Matches(x => x == _existingOfferId), A<string>.That.Not.Matches(x => x == _iamUserId),
                A<OfferTypeId>._))
            .ReturnsLazily(() => ((Guid companyId, OfferSubscription? offerSubscription, Guid companyUserId))default);

        A.CallTo(() => _portalRepositories.GetInstance<IAgreementRepository>().CheckAgreementsExistsForOfferAsync(
            A<IEnumerable<Guid>>._,
            A<Guid>._,
            A<OfferTypeId>._))
            .ReturnsLazily(() => true);

        A.CallTo(() => _portalRepositories.GetInstance<IAgreementRepository>()).Returns(_agreementRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IConsentRepository>()).Returns(_consentRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IConsentAssignedOfferSubscriptionRepository>()).Returns(_consentAssignedOfferSubscriptionRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IOfferRepository>()).Returns(_offerRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IOfferSubscriptionsRepository>()).Returns(_offerSubscriptionsRepository);
        A.CallTo(() => _portalRepositories.GetInstance<INotificationRepository>()).Returns(_notificationRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRolesRepository>()).Returns(_userRolesRepository);
        _fixture.Inject(_portalRepositories);
    }

    private void SetupServices()
    {
        A.CallTo(() => _offerSetupService.AutoSetupOffer(A<OfferThirdPartyAutoSetupData>._, A<string>.That.Matches(x => x == _iamUserId), A<string>._, A<string>.That.Matches(x => x == "https://www.testurl.com")))
            .ReturnsLazily(() => Task.CompletedTask);
        A.CallTo(() => _offerSetupService.AutoSetupOffer(A<OfferThirdPartyAutoSetupData>._, A<string>.That.Matches(x => x == _iamUserId), A<string>._, A<string>.That.Matches(x => x == "https://www.fail.com")))
            .ThrowsAsync(() => new ServiceException("Error occured"));
    }

    #endregion
}
