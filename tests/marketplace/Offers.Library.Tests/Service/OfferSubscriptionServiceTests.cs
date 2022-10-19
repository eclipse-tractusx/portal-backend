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
using Org.CatenaX.Ng.Portal.Backend.Framework.ErrorHandling;
using Org.CatenaX.Ng.Portal.Backend.Offers.Library.Service;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.CatenaX.Ng.Portal.Backend.Tests.Shared;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Org.CatenaX.Ng.Portal.Backend.Mailing.SendMail;
using PortalBackend.DBAccess.Models;
using Xunit;

namespace Org.CatenaX.Ng.Portal.Backend.Offers.Library.Tests.Service;

public class OfferSubscriptionServiceTests
{
    private const string BasePortalUrl = "http//base-url.com";
    private const string AccessToken = "THISISAACCESSTOKEN";
    private const string ClientId = "Client1";
    
    private readonly string _notAssignedCompanyIdUser;
    private readonly string _iamUserId;
    private readonly Guid _companyUserId;
    private readonly Guid _companyId;
    private readonly Guid _existingServiceId;
    private readonly Guid _existingServiceWithFailingAutoSetupId;
    private readonly Guid _validSubscriptionId;
    private readonly Guid _newOfferSubscriptionId;
    private readonly Guid _userRoleId;
    private readonly IFixture _fixture;
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
        _companyUserId = _fixture.Create<Guid>();
        _companyId = _fixture.Create<Guid>();
        _notAssignedCompanyIdUser = _fixture.Create<string>();
        _existingServiceId = _fixture.Create<Guid>();
        _existingServiceWithFailingAutoSetupId = _fixture.Create<Guid>();
        _validSubscriptionId = _fixture.Create<Guid>();
        _newOfferSubscriptionId = _fixture.Create<Guid>();
        _userRoleId = _fixture.Create<Guid>();
        _serviceManagerRoles = new Dictionary<string, IEnumerable<string>>
        {
            { ClientId, new[] { "Service Manager" } }
        };

        _portalRepositories = A.Fake<IPortalRepositories>();
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

    #region Add Service Subscription

    [Fact]
    public async Task AddServiceSubscription_NotAssignedCompany_ThrowsException()
    {
        // Arrange
        var sut = new OfferSubscriptionService(_portalRepositories, _offerSetupService, _mailingService, A.Fake<ILogger<OfferSubscriptionService>>());

        // Act
        async Task Action() => await sut.AddOfferSubscriptionAsync(_existingServiceId, _notAssignedCompanyIdUser, AccessToken, _serviceManagerRoles, OfferTypeId.SERVICE, BasePortalUrl);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("iamUserId");
    }

    [Fact]
    public async Task AddServiceSubscription_WithExistingId_CreatesServiceSubscription()
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
                var setOptionalParameter = x.Arguments.Get< Action<PortalBackend.PortalEntities.Entities.Notification>?>("setOptionalParameter");

                var notification = new PortalBackend.PortalEntities.Entities.Notification(notificationId, receiverUserId, DateTimeOffset.UtcNow, notificationTypeId, isRead);
                setOptionalParameter?.Invoke(notification);
                notifications.Add(notification);
            });        
        var sut = new OfferSubscriptionService(_portalRepositories, _offerSetupService, _mailingService, A.Fake<ILogger<OfferSubscriptionService>>());

        // Act
        await sut.AddOfferSubscriptionAsync(_existingServiceId, _iamUserId, AccessToken, _serviceManagerRoles, OfferTypeId.SERVICE, BasePortalUrl);

        // Assert
        companyAssignedApps.Should().HaveCount(1);
        notifications.Should().HaveCount(2);
        A.CallTo(() => _mailingService.SendMails(A<string>._, A<Dictionary<string, string>>._, A<List<string>>._)).MustHaveHappenedOnceExactly();
    }
    
    [Fact]
    public async Task AddServiceSubscription_WithFailingAutoSetup_ReturnsExpectedResult()
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
                var setOptionalParameter = x.Arguments.Get< Action<PortalBackend.PortalEntities.Entities.Notification>?>("setOptionalParameter");

                var notification = new PortalBackend.PortalEntities.Entities.Notification(notificationId, receiverUserId, DateTimeOffset.UtcNow, notificationTypeId, isRead);
                setOptionalParameter?.Invoke(notification);
                notifications.Add(notification);
            });
        var sut = new OfferSubscriptionService(_portalRepositories, _offerSetupService, _mailingService, A.Fake<ILogger<OfferSubscriptionService>>());

        // Act
        await sut.AddOfferSubscriptionAsync(_existingServiceWithFailingAutoSetupId, _iamUserId, AccessToken, _serviceManagerRoles, OfferTypeId.SERVICE, BasePortalUrl);

        // Assert
        notifications.Should().ContainSingle();
        notifications.First().Content.Should().Contain("Error occured");
    }

    [Fact]
    public async Task AddServiceSubscription_WithNotExistingId_ThrowsException()
    {
        // Arrange
        var notExistingServiceId = Guid.NewGuid();
        var sut = new OfferSubscriptionService(_portalRepositories, _offerSetupService, _mailingService, A.Fake<ILogger<OfferSubscriptionService>>());

        // Act
        async Task Action() => await sut.AddOfferSubscriptionAsync(notExistingServiceId, _iamUserId, AccessToken, _serviceManagerRoles, OfferTypeId.SERVICE, BasePortalUrl);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Action);
        ex.Message.Should().Be($"Service {notExistingServiceId} does not exist");
    }
    
    [Fact]
    public async Task AddServiceSubscription_NotAssignedCompanyUser_ThrowsException()
    {
        // Arrange
        var sut = new OfferSubscriptionService(_portalRepositories, _offerSetupService, _mailingService, A.Fake<ILogger<OfferSubscriptionService>>());

        // Act
        async Task Action() => await sut.AddOfferSubscriptionAsync(_existingServiceId, Guid.NewGuid().ToString(), AccessToken, _serviceManagerRoles, OfferTypeId.SERVICE, BasePortalUrl);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("iamUserId");
    }

    #endregion

    #region Setup

    private void SetupRepositories()
    {
        var serviceDetailData = new AsyncEnumerableStub<ValueTuple<Guid, string?, string, string?, string?, string?>>(_fixture.CreateMany<ValueTuple<Guid, string?, string, string?, string?, string?>>(5));
        var serviceDetail = _fixture.Build<OfferDetailData>()
            .With(x => x.Id, _existingServiceId)
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
        A.CallTo(() => _userRepository.GetOwnCompanyInformationWithCompanyUserIdAndEmailAsync(A<string>.That.Not.Matches(x => x == _iamUserId || x == _notAssignedCompanyIdUser)))
            .ReturnsLazily(() => (
                new CompanyInformationData(_companyId, "The Company", "DE", "BPM00000001"),
                Guid.Empty,
                "test@mail.de"));
        A.CallTo(() => _userRepository.GetServiceProviderCompanyUserWithRoleIdAsync(A<Guid>.That.Matches(x => x == _existingServiceId), A<List<Guid>>.That.Matches(x => x.Count == 1 && x.All(y => y == _userRoleId))))
            .Returns(new List<Guid> { _companyUserId }.ToAsyncEnumerable());
        A.CallTo(() => _userRolesRepository.GetUserRoleIdsUntrackedAsync(A<IDictionary<string, IEnumerable<string>>>.That.Matches(x => x[ClientId].First() == "Service Manager")))
            .Returns(new List<Guid> { _userRoleId }.ToAsyncEnumerable());
        A.CallTo(() => _userRolesRepository.GetUserRoleIdsUntrackedAsync(A<IDictionary<string, IEnumerable<string>>>.That.Not.Matches(x => x[ClientId].First() == "Service Manager")))
            .Returns(new List<Guid>().ToAsyncEnumerable());

        A.CallTo(() => _offerRepository.GetActiveServices())
            .Returns(serviceDetailData.AsQueryable());
        
        A.CallTo(() => _offerRepository.GetOfferDetailByIdUntrackedAsync(_existingServiceId, A<string>.That.Matches(x => x == "en"), A<string>._, A<OfferTypeId>._))
            .ReturnsLazily(() => serviceDetail with {OfferSubscriptionDetailData = new []
            {
                new OfferSubscriptionStateDetailData(Guid.NewGuid(), OfferSubscriptionStatusId.ACTIVE)
            }});
        A.CallTo(() => _offerRepository.GetOfferDetailByIdUntrackedAsync(A<Guid>.That.Not.Matches(x => x == _existingServiceId), A<string>._, A<string>._, A<OfferTypeId>._))
            .ReturnsLazily(() => (OfferDetailData?)null);
        
        A.CallTo(() => _offerRepository.GetOfferProviderDetailsAsync(A<Guid>.That.Matches(x => x == _existingServiceId), A<OfferTypeId>._))
            .ReturnsLazily(() => new OfferProviderDetailsData("Test Service", "Test Company", "provider@mail.de", new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"), "https://www.testurl.com"));
        A.CallTo(() => _offerRepository.GetOfferProviderDetailsAsync(A<Guid>.That.Matches(x => x == _existingServiceWithFailingAutoSetupId), A<OfferTypeId>._))
            .ReturnsLazily(() => new OfferProviderDetailsData("Test Service", "Test Company", "provider@mail.de", new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"), "https://www.fail.com"));
        A.CallTo(() => _offerRepository.GetOfferProviderDetailsAsync(A<Guid>.That.Not.Matches(x => x == _existingServiceId || x == _existingServiceWithFailingAutoSetupId), A<OfferTypeId>._))
            .ReturnsLazily(() => (OfferProviderDetailsData?)null);
        
        A.CallTo(() => _offerRepository.CheckServiceExistsById(_existingServiceId))
            .Returns(true);
        A.CallTo(() => _offerRepository.CheckServiceExistsById(A<Guid>.That.Not.Matches(x => x == _existingServiceId)))
            .Returns(false);
        
        var offerSubscription = _fixture.Create<OfferSubscription>();
        A.CallTo(() => _offerSubscriptionsRepository.GetSubscriptionDetailDataForOwnUserAsync(
                A<Guid>.That.Matches(x => x == _validSubscriptionId),
                A<string>.That.Matches(x => x == _iamUserId),
                A<OfferTypeId>._))
            .ReturnsLazily(() =>
                new SubscriptionDetailData(_existingServiceId, "Super Service", OfferSubscriptionStatusId.ACTIVE));
        A.CallTo(() => _offerSubscriptionsRepository.GetSubscriptionDetailDataForOwnUserAsync(
                A<Guid>.That.Not.Matches(x => x == _validSubscriptionId),
                A<string>.That.Matches(x => x == _iamUserId),
                A<OfferTypeId>._))
            .ReturnsLazily(() => (SubscriptionDetailData?)null);
        A.CallTo(() => _offerSubscriptionsRepository.GetCompanyIdWithAssignedOfferForCompanyUserAndSubscriptionAsync(
                A<Guid>.That.Matches(x => x == _existingServiceId), A<string>.That.Matches(x => x == _iamUserId), A<OfferTypeId>._))
            .ReturnsLazily(() => new ValueTuple<Guid, OfferSubscription?, Guid>(_companyId, offerSubscription, _companyUserId));
        A.CallTo(() => _offerSubscriptionsRepository.GetCompanyIdWithAssignedOfferForCompanyUserAndSubscriptionAsync(
                A<Guid>.That.Not.Matches(x => x == _existingServiceId), A<string>.That.Matches(x => x == _iamUserId),
                A<OfferTypeId>._))
            .ReturnsLazily(() => new ValueTuple<Guid, OfferSubscription?, Guid>(_companyId, (OfferSubscription?)null, _companyUserId));
        A.CallTo(() => _offerSubscriptionsRepository.GetCompanyIdWithAssignedOfferForCompanyUserAndSubscriptionAsync(
                A<Guid>.That.Matches(x => x == _existingServiceId), A<string>.That.Not.Matches(x => x == _iamUserId),
                A<OfferTypeId>._))
            .ReturnsLazily(() => ((Guid companyId, OfferSubscription? offerSubscription, Guid companyUserId))default);

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
