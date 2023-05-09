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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Mailing.SendMail;
using Org.Eclipse.TractusX.Portal.Backend.Notifications.Library;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;
using System.Net;

namespace Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Tests.Service;

public class OfferSetupServiceTests
{
    private const string AccessToken = "THISISAACCESSTOKEN";
    private const string Bpn = "CAXSDUMMYCATENAZZ";
    private const string IamUserId = "9aae7a3b-b188-4a42-b46b-fb2ea5f47668";
    private const string IamUserIdWithoutMail = "9aae7a3b-b188-4a42-b46b-fb2ea5f47669";

    private readonly Guid _companyUserCompanyId = new("395f955b-f11b-4a74-ab51-92a526c1973a");
    private readonly Guid _existingServiceId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47661");
    private readonly Guid _validSubscriptionId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47662");
    private readonly Guid _pendingSubscriptionId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47663");
    private readonly Guid _validOfferId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47664");
    private readonly Guid _offerIdWithoutClient = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47665");
    private readonly Guid _offerIdWithInstanceNotSet = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47667");
    private readonly Guid _validInstanceSetupId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47666");
    private readonly Guid _technicalUserId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47998");
    private readonly Guid _salesManagerId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47999");

    private readonly CompanyUser _companyUser;
    private readonly IFixture _fixture;
    private readonly IAppInstanceRepository _appInstanceRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IAppSubscriptionDetailRepository _appSubscriptionDetailRepository;
    private readonly IOfferSubscriptionsRepository _offerSubscriptionsRepository;
    private readonly IOfferRepository _offerRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IProvisioningManager _provisioningManager;
    private readonly IServiceAccountCreation _serviceAccountCreation;
    private readonly INotificationService _notificationService;
    private readonly IMailingService _mailingService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly OfferSetupService _sut;
    private readonly ITechnicalUserProfileService _technicalUserProfileService;

    public OfferSetupServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _companyUser = _fixture.Build<CompanyUser>()
            .Without(u => u.IamUser)
            .With(u => u.CompanyId, _companyUserCompanyId)
            .Create();
        _portalRepositories = A.Fake<IPortalRepositories>();
        _appSubscriptionDetailRepository = A.Fake<IAppSubscriptionDetailRepository>();
        _appInstanceRepository = A.Fake<IAppInstanceRepository>();
        _clientRepository = A.Fake<IClientRepository>();
        _offerSubscriptionsRepository = A.Fake<IOfferSubscriptionsRepository>();
        _offerRepository = A.Fake<IOfferRepository>();
        _provisioningManager = A.Fake<IProvisioningManager>();
        _notificationRepository = A.Fake<INotificationRepository>();
        _serviceAccountCreation = A.Fake<IServiceAccountCreation>();
        _notificationService = A.Fake<INotificationService>();
        _mailingService = A.Fake<IMailingService>();
        _httpClientFactory = A.Fake<IHttpClientFactory>();
        _technicalUserProfileService = A.Fake<ITechnicalUserProfileService>();
        
        A.CallTo(() => _portalRepositories.GetInstance<IAppSubscriptionDetailRepository>()).Returns(_appSubscriptionDetailRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IAppInstanceRepository>()).Returns(_appInstanceRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IClientRepository>()).Returns(_clientRepository);
        A.CallTo(() => _portalRepositories.GetInstance<INotificationRepository>()).Returns(_notificationRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IOfferSubscriptionsRepository>()).Returns(_offerSubscriptionsRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IOfferRepository>()).Returns(_offerRepository);

        _sut = new OfferSetupService(_portalRepositories, _provisioningManager, _serviceAccountCreation, _notificationService, _mailingService, _httpClientFactory, _technicalUserProfileService);
    }

    #region CallThirdPartyAutoSetupOfferAsync

    [Fact]
    public async Task CallThirdPartyAutoSetupOfferAsync_WithNonSuccessfullyClientCall_ThrowsServiceException()
    {
        // Arrange
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.BadRequest);
        A.CallTo(() => _httpClientFactory.CreateClient(nameof(OfferSetupService)))
            .Returns(new HttpClient(httpMessageHandlerMock));

        // Act
        async Task Action() => await _sut.AutoSetupOfferSubscription(_fixture.Create<OfferThirdPartyAutoSetupData>(), AccessToken, "https://www.superservice.com").ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Action);
        ex.Message.Should().Be("call to external system autosetup-offer-subscription failed with statuscode 400");
    }

    [Fact]
    public async Task CallThirdPartyAutoSetupOfferAsync_WithDnsError_ReturnsServiceException()
    {
        // Arrange
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.BadRequest, ex: new HttpRequestException ("DNS Error"));
        A.CallTo(() => _httpClientFactory.CreateClient(nameof(OfferSetupService)))
            .Returns(new HttpClient(httpMessageHandlerMock));

        // Act
        async Task Action() => await _sut.AutoSetupOfferSubscription(_fixture.Create<OfferThirdPartyAutoSetupData>(), AccessToken, "https://www.superservice.com").ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Action);
        ex.Message.Should().Be("call to external system autosetup-offer-subscription failed");
    }

    [Fact]
    public async Task CallThirdPartyAutoSetupOfferAsync_WithTimeout_ReturnsServiceException()
    {
        // Arrange
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.BadRequest, ex: new TaskCanceledException("Timed out"));
        A.CallTo(() => _httpClientFactory.CreateClient(nameof(OfferSetupService)))
            .Returns(new HttpClient(httpMessageHandlerMock));

        // Act
        async Task Action() => await _sut.AutoSetupOfferSubscription(_fixture.Create<OfferThirdPartyAutoSetupData>(), AccessToken, "https://www.superservice.com").ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Action);
        ex.Message.Should().Be("call to external system autosetup-offer-subscription failed due to timeout");
    }

    #endregion
    
    #region AutoSetupServiceAsync

    [Theory]
    [InlineData(OfferTypeId.APP, true, true)]
    [InlineData(OfferTypeId.APP, true, false)]
    [InlineData(OfferTypeId.SERVICE, true, false)]
    [InlineData(OfferTypeId.SERVICE, false, false)]
    public async Task AutoSetup_WithValidData_ReturnsExpectedNotificationAndSecret(OfferTypeId offerTypeId, bool technicalUserRequired, bool isSingleInstance)
    {
        // Arrange
        var offerSubscription = new OfferSubscription(Guid.NewGuid(), Guid.Empty, Guid.Empty, OfferSubscriptionStatusId.PENDING, Guid.Empty, Guid.Empty);
        var companyServiceAccount = new CompanyServiceAccount(Guid.NewGuid(), Guid.Empty, CompanyServiceAccountStatusId.ACTIVE, "test", "test", DateTimeOffset.UtcNow, CompanyServiceAccountTypeId.OWN);
        var createNotificationsEnumerator = SetupAutoSetup(technicalUserRequired, offerSubscription, isSingleInstance, companyServiceAccount);
        var clientId = Guid.NewGuid();
        var appInstanceId = Guid.NewGuid();
        var appSubscriptionDetailId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();
        var clients = new List<IamClient>();
        var appInstances = new List<AppInstance>();
        var appSubscriptionDetails = new List<AppSubscriptionDetail>();
        var notifications = new List<Notification>();
        A.CallTo(() => _clientRepository.CreateClient(A<string>._))
            .Invokes((string clientName) =>
            {
                var client = new IamClient(clientId, clientName);
                clients.Add(client);
            })
            .Returns(new IamClient(clientId, "cl1"));
        if (technicalUserRequired)
        {
            A.CallTo(() => _technicalUserProfileService.GetTechnicalUserProfilesForOfferSubscription(A<Guid>._))
                .Returns(new ServiceAccountCreationInfo[] { new(Guid.NewGuid().ToString(), "test", IamClientAuthMethod.SECRET, Enumerable.Empty<Guid>()) });
        }
        var serviceManagerRoles = new Dictionary<string, IEnumerable<string>>
        {
            { "Cl2-CX-Portal", new [] { "Service Manager" } }
        };
        
        A.CallTo(() => _appInstanceRepository.CreateAppInstance(A<Guid>._, A<Guid>._))
            .Invokes((Guid appId, Guid iamClientId) =>
            {
                var appInstance = new AppInstance(appInstanceId, appId, iamClientId);
                appInstances.Add(appInstance);
            })
            .Returns(new AppInstance(appInstanceId, _existingServiceId, clientId));
        A.CallTo(() => _appSubscriptionDetailRepository.CreateAppSubscriptionDetail(A<Guid>._, A<Action<AppSubscriptionDetail>?>._))
            .Invokes((Guid offerSubscriptionId, Action<AppSubscriptionDetail>? updateOptionalFields) =>
            {
                var appDetail = new AppSubscriptionDetail(appSubscriptionDetailId, offerSubscriptionId);
                updateOptionalFields?.Invoke(appDetail);
                appSubscriptionDetails.Add(appDetail);
            })
            .Returns(new AppSubscriptionDetail(appSubscriptionDetailId, _validSubscriptionId));
        A.CallTo(() => _notificationRepository.CreateNotification(A<Guid>._, A<NotificationTypeId>._, A<bool>._,
                A<Action<Notification>?>._))
            .Invokes((Guid receiverUserId, NotificationTypeId notificationTypeId, bool isRead, Action<Notification>? setOptionalParameters) =>
            {
                var notification = new Notification(notificationId, receiverUserId, DateTimeOffset.UtcNow, notificationTypeId, isRead);
                setOptionalParameters?.Invoke(notification);
                notifications.Add(notification);
            });
        var companyAdminRoles = new Dictionary<string, IEnumerable<string>>
        {
            { "Cl2-CX-Portal", new [] { "IT Admin" } }
        };

        var data = new OfferAutoSetupData(_pendingSubscriptionId, "https://new-url.com/");

        // Act
        var result = await _sut.AutoSetupOfferAsync(data, companyAdminRoles, IamUserId, offerTypeId, "https://base-address.com", serviceManagerRoles).ConfigureAwait(false);
        
        // Assert
        result.Should().NotBeNull();
        if (!technicalUserRequired || isSingleInstance)
        {
            result.TechnicalUserInfo.Should().BeNull();
            result.ClientInfo.Should().BeNull();
            clients.Should().BeEmpty();
        }
        else
        {
            result.TechnicalUserInfo.Should().NotBeNull();
            result.TechnicalUserInfo!.TechnicalUserId.Should().Be(_technicalUserId);
            result.TechnicalUserInfo.TechnicalUserSecret.Should().Be("katze!1234");
            companyServiceAccount.OfferSubscriptionId.Should().Be(_pendingSubscriptionId);
        }

        if (isSingleInstance)
        {
            appInstances.Should().BeEmpty();
            appSubscriptionDetails.Should().ContainSingle();
        }
        else
        {
            if (offerTypeId == OfferTypeId.SERVICE)
            {
                appInstances.Should().BeEmpty();
                appSubscriptionDetails.Should().BeEmpty();
            }
            else
            {
                appInstances.Should().ContainSingle();
                appSubscriptionDetails.Should().ContainSingle();
                clients.Should().ContainSingle();
            }
        }

        var notificationTypeId = offerTypeId == OfferTypeId.APP
            ? NotificationTypeId.APP_SUBSCRIPTION_REQUEST
            : NotificationTypeId.SERVICE_REQUEST;
        notifications.Should().HaveCount(1);
        A.CallTo(() => _notificationService.SetNotificationsForOfferToDone(
                A<IDictionary<string, IEnumerable<string>>>._,
                A<IEnumerable<NotificationTypeId>>.That.Matches(x =>
                    x.Count() == 1 && x.Single() == notificationTypeId),
                _existingServiceId,
                A<IEnumerable<Guid>?>.That.Matches(x => x != null && x.Count() == 1 && x.Single() == _salesManagerId)))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => createNotificationsEnumerator.MoveNextAsync()).MustHaveHappened(2, Times.Exactly);
        offerSubscription.OfferSubscriptionStatusId.Should().Be(OfferSubscriptionStatusId.ACTIVE);
        if (!isSingleInstance)
        {
            A.CallTo(() => _mailingService.SendMails(A<string>._, A<Dictionary<string, string>>._, A<List<string>>._)).MustHaveHappenedOnceExactly();
        }
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task AutoSetup_WithValidDataAndUserWithoutMail_NoMailIsSend()
    {
        // Arrange
        SetupAutoSetup();
        var companyAdminRoles = new Dictionary<string, IEnumerable<string>>
        {
            { "Cl2-CX-Portal", new [] { "IT Admin" } }
        };
        var serviceManagerRoles = new Dictionary<string, IEnumerable<string>>
        {
            { "Cl2-CX-Portal", new [] { "Service Manager" } }
        };

        var data = new OfferAutoSetupData(_pendingSubscriptionId, "https://new-url.com/");

        // Act
        var result = await _sut.AutoSetupOfferAsync(data, companyAdminRoles, IamUserIdWithoutMail, OfferTypeId.SERVICE, "https://base-address.com", serviceManagerRoles).ConfigureAwait(false);
        
        // Assert
        result.Should().NotBeNull();
        result.TechnicalUserInfo.Should().BeNull();
        result.ClientInfo.Should().BeNull();
        A.CallTo(() => _mailingService.SendMails(A<string>._, A<Dictionary<string, string>>._, A<List<string>>._)).MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task AutoSetup_WithoutAppInstanceSetForSingleInstanceApp_ThrowsConflictException()
    {
        // Arrange
        SetupAutoSetup(isSingleInstance: true);
        var companyAdminRoles = new Dictionary<string, IEnumerable<string>>
        {
            { "Cl2-CX-Portal", new [] { "IT Admin" } }
        };
        var serviceManagerRoles = new Dictionary<string, IEnumerable<string>>
        {
            { "Cl2-CX-Portal", new [] { "Service Manager" } }
        };

        var data = new OfferAutoSetupData(_offerIdWithInstanceNotSet, "https://new-url.com/");

        // Act
        async Task Act() => await _sut.AutoSetupOfferAsync(data, companyAdminRoles, IamUserId, OfferTypeId.APP, "https://base-address.com", serviceManagerRoles).ConfigureAwait(false);
        
        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("There must only be one app instance for single instance apps");
        A.CallTo(() => _mailingService.SendMails(A<string>._, A<Dictionary<string, string>>._, A<List<string>>._)).MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
    }

    [Fact]
    public async Task AutoSetup_WithNotExistingOfferSubscriptionId_ThrowsException()
    {
        // Arrange
        SetupAutoSetup();
        var data = new OfferAutoSetupData(Guid.NewGuid(), "https://new-url.com/");

        // Act
        async Task Action() => await _sut.AutoSetupOfferAsync(data, new Dictionary<string, IEnumerable<string>>(), IamUserId, OfferTypeId.SERVICE, "https://base-address.com", new Dictionary<string, IEnumerable<string>>());
        
        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Action);
        ex.Message.Should().Be($"OfferSubscription {data.RequestId} does not exist");
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        A.CallTo(() => _mailingService.SendMails(A<string>._, A<Dictionary<string, string>>._, A<List<string>>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task AutoSetup_WithActiveSubscription_ThrowsException()
    {
        // Arrange
        SetupAutoSetup();
        var data = new OfferAutoSetupData(_validSubscriptionId, "https://new-url.com/");

        // Act
        async Task Action() => await _sut.AutoSetupOfferAsync(data, new Dictionary<string, IEnumerable<string>>(), IamUserId, OfferTypeId.SERVICE, "https://base-address.com", new Dictionary<string, IEnumerable<string>>());
        
        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Action);
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        A.CallTo(() => _mailingService.SendMails(A<string>._, A<Dictionary<string, string>>._, A<List<string>>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task AutoSetup_WithUserNotFromProvidingCompany_ThrowsException()
    {
        // Arrange
        SetupAutoSetup();
        var data = new OfferAutoSetupData(_pendingSubscriptionId, "https://new-url.com/");

        // Act
        async Task Action() => await _sut.AutoSetupOfferAsync(data, new Dictionary<string, IEnumerable<string>>(), Guid.NewGuid().ToString(), OfferTypeId.SERVICE, "https://base-address.com", new Dictionary<string, IEnumerable<string>>());
        
        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Action);
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        A.CallTo(() => _mailingService.SendMails(A<string>._, A<Dictionary<string, string>>._, A<List<string>>._)).MustNotHaveHappened();
    }

    #endregion
    
    #region ActivateSingleInstanceAppAsync
    
    [Fact]
    public async Task ActivateSingleInstanceAppAsync_WithValidData_ReturnsExpected()
    {
        // Arrange
        var appInstanceId = Guid.NewGuid();
        var appInstance = new AppInstance(appInstanceId, _validOfferId, default);
        SetupCreateSingleInstance(appInstance);
        A.CallTo(() => _technicalUserProfileService.GetTechnicalUserProfilesForOffer(_validOfferId, A<OfferTypeId>._))
            .Returns(new ServiceAccountCreationInfo[] { new(Guid.NewGuid().ToString(), "test", IamClientAuthMethod.SECRET, Enumerable.Empty<Guid>()) }.AsFakeIEnumerable(out var enumerator));
        
        // Act
        var result = await _sut.ActivateSingleInstanceAppAsync(_validOfferId).ConfigureAwait(false);
        
        // Assert
        result.Should().NotBeNull();
        appInstance.ServiceAccounts.Should().HaveCount(1);
        A.CallTo(() => _provisioningManager.EnableClient(A<string>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => enumerator.MoveNext())
            .MustHaveHappened(2,Times.Exactly);
    }

    [Fact]
    public async Task ActivateSingleInstanceAppAsync_WithNotExistingApp_ThrowsConflictException()
    {
        var offerId = Guid.NewGuid();
        SetupCreateSingleInstance();
        
        async Task Act() => await _sut.ActivateSingleInstanceAppAsync(offerId).ConfigureAwait(false);

        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"App {offerId} does not exist.");
    }

    [Fact]
    public async Task ActivateSingleInstanceAppAsync_WithNoClientSet_ThrowsConflictException()
    {
        SetupCreateSingleInstance();
        async Task Act() => await _sut.ActivateSingleInstanceAppAsync(_offerIdWithoutClient).ConfigureAwait(false);

        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"clientId must not be empty for single instance offer {_offerIdWithoutClient}");
    }

    [Fact]
    public async Task ActivateSingleInstanceAppAsync_WithInstanceNotSet_ThrowsConflictException()
    {
        SetupCreateSingleInstance();
        async Task Act() => await _sut.ActivateSingleInstanceAppAsync(_offerIdWithInstanceNotSet).ConfigureAwait(false);

        var ex = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);
        ex.Message.Should().Be($"There should always be exactly one instance defined for a single instance offer {_offerIdWithInstanceNotSet}");
    }

    #endregion
    
    #region SetupSingleInstance
    
    [Fact]
    public async Task SetupSingleInstance_WithValidData_ReturnsExpected()
    {
        // Arrange
        SetupServices();
        var clientId = Guid.NewGuid();
        var appInstanceId = Guid.NewGuid();
        var offerId = Guid.NewGuid();
        var clients = new List<IamClient>();
        var appInstances = new List<AppInstance>();
        A.CallTo(() => _appInstanceRepository.CheckInstanceExistsForOffer(offerId))
            .ReturnsLazily(() => true);
        A.CallTo(() => _clientRepository.CreateClient(A<string>._))
            .Invokes((string clientName) =>
            {
                var client = new IamClient(clientId, clientName);
                clients.Add(client);
            })
            .Returns(new IamClient(clientId, "cl1"));

        A.CallTo(() => _appInstanceRepository.CreateAppInstance(A<Guid>._, A<Guid>._))
            .Invokes((Guid appId, Guid iamClientId) =>
            {
                var appInstance = new AppInstance(appInstanceId, appId, iamClientId);
                appInstances.Add(appInstance);
            })
            .Returns(new AppInstance(appInstanceId, _existingServiceId, clientId));

        // Act
        await _sut.SetupSingleInstance(offerId, "https://base-address.com").ConfigureAwait(false);
        
        // Assert
        appInstances.Should().ContainSingle();
        clients.Should().ContainSingle();
    }
    
    [Fact]
    public async Task SetupSingleInstance_WithExistingAppInstance_ThrowsConflictException()
    {
        // Arrange
        SetupServices();
        var offerId = Guid.NewGuid();
        A.CallTo(() => _appInstanceRepository.CheckInstanceExistsForOffer(offerId))
            .ReturnsLazily(() => false);

        // Act
        async Task Act() => await _sut.SetupSingleInstance(offerId, "https://base-address.com").ConfigureAwait(false);
        
        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"The app instance for offer {offerId} already exist");
    }

    #endregion
    
    #region UpdateSingleInstance

    [Fact]
    public async Task UpdateSingleInstance_CallsExpected()
    {
        // Arrange
        const string url = "https://test.de";
        
        // Act
        await _sut.UpdateSingleInstance("test", url).ConfigureAwait(false);
        
        // Assert
        A.CallTo(() => _provisioningManager.UpdateClient("test", url, $"{url}/*"))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region DeleteSingleInstance
    
    [Fact]
    public async Task DeleteSingleInstance_WithExistingServiceAccountsAssigned_CallsExpected()
    {
        // Arrange
        var appInstanceId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var clientClientId = Guid.NewGuid().ToString();
        var serviceAccountId = Guid.NewGuid();
        A.CallTo(() => _appInstanceRepository.CheckInstanceHasAssignedSubscriptions(appInstanceId))
            .Returns(false);
        A.CallTo(() => _appInstanceRepository.GetAssignedServiceAccounts(appInstanceId))
            .Returns(new []{ serviceAccountId }.ToAsyncEnumerable());

        // Act
        await _sut.DeleteSingleInstance(appInstanceId, clientId, clientClientId).ConfigureAwait(false);
        
        // Assert
        A.CallTo(() => _appInstanceRepository.CheckInstanceHasAssignedSubscriptions(appInstanceId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.DeleteCentralClientAsync(clientClientId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _clientRepository.RemoveClient(clientId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _appInstanceRepository.RemoveAppInstance(appInstanceId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _appInstanceRepository.RemoveAppInstanceAssignedServiceAccounts(appInstanceId, A<IEnumerable<Guid>>.That.Matches(x => x.Count() == 1 && x.Single() == serviceAccountId)))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DeleteSingleInstance_WithNonExistingServiceAccounts_CallsExpected()
    {
        // Arrange
        var appInstanceId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var clientClientId = Guid.NewGuid().ToString();
        A.CallTo(() => _appInstanceRepository.CheckInstanceHasAssignedSubscriptions(appInstanceId))
            .Returns(false);
        A.CallTo(() => _appInstanceRepository.GetAssignedServiceAccounts(appInstanceId))
            .Returns(Enumerable.Empty<Guid>().ToAsyncEnumerable());

        // Act
        await _sut.DeleteSingleInstance(appInstanceId, clientId, clientClientId).ConfigureAwait(false);
        
        // Assert
        A.CallTo(() => _appInstanceRepository.CheckInstanceHasAssignedSubscriptions(appInstanceId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _provisioningManager.DeleteCentralClientAsync(clientClientId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _clientRepository.RemoveClient(clientId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _appInstanceRepository.RemoveAppInstance(appInstanceId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _appInstanceRepository.RemoveAppInstanceAssignedServiceAccounts(appInstanceId, A<IEnumerable<Guid>>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task DeleteSingleInstance_WithExistingSubscriptions_Throws()
    {
        // Arrange
        var appInstanceId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var clientClientId = Guid.NewGuid().ToString();
        A.CallTo(() => _appInstanceRepository.CheckInstanceHasAssignedSubscriptions(appInstanceId))
            .Returns(true);

        var Act = () => _sut.DeleteSingleInstance(appInstanceId, clientId, clientClientId);

        // Act
        var result = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _provisioningManager.DeleteCentralClientAsync(A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _clientRepository.RemoveClient(A<Guid>._)).MustNotHaveHappened();
        A.CallTo(() => _appInstanceRepository.RemoveAppInstance(A<Guid>._)).MustNotHaveHappened();
        A.CallTo(() => _appInstanceRepository.RemoveAppInstanceAssignedServiceAccounts(A<Guid>._, A<IEnumerable<Guid>>._))
            .MustNotHaveHappened();

        result.Message.Should().Be($"The app instance {appInstanceId} is associated with exiting subscriptions");
    }

    #endregion
    
    #region Setup

    private IAsyncEnumerator<Guid> SetupServices(CompanyServiceAccount? companyServiceAccount = null)
    {
        A.CallTo(() => _provisioningManager.SetupClientAsync(A<string>._, A<string>._, A<IEnumerable<string>?>._, A<bool>._))
            .ReturnsLazily(() => "cl1");
        
        A.CallTo(() => _serviceAccountCreation.CreateServiceAccountAsync(A<ServiceAccountCreationInfo>._, A<Guid>._, A<IEnumerable<string>>.That.Matches(x => x.Any(y => y == "CAXSDUMMYCATENAZZ")), CompanyServiceAccountTypeId.MANAGED, A<bool>._, A<Action<CompanyServiceAccount>?>._))
            .Invokes((ServiceAccountCreationInfo _, Guid _, IEnumerable<string> _, CompanyServiceAccountTypeId _, bool _, Action<CompanyServiceAccount>? setOptionalParameter) =>
            {
                if (companyServiceAccount != null)
                {
                    setOptionalParameter?.Invoke(companyServiceAccount);
                }
            })
            .ReturnsLazily(() => new ValueTuple<string, ServiceAccountData, Guid, List<UserRoleData>>(
                "sa2",
                new ServiceAccountData(Guid.NewGuid().ToString(), "cl1", new ClientAuthData(IamClientAuthMethod.SECRET)
                {
                    Secret = "katze!1234"
                }),
                _technicalUserId, 
                new List<UserRoleData>()));

        A.CallTo(() => _notificationService.CreateNotifications(A<IDictionary<string, IEnumerable<string>>>._,
                A<Guid>._, A<IEnumerable<(string?, NotificationTypeId)>>._, A<Guid>._, A<bool?>._))
            .Returns(new List<Guid>{Guid.NewGuid()}.AsFakeIAsyncEnumerable(out var createNotificationsEnumerator));

        return createNotificationsEnumerator;
    }

    private IAsyncEnumerator<Guid> SetupAutoSetup(bool technicalUserRequired = false, OfferSubscription? offerSubscription = null, bool isSingleInstance = false, CompanyServiceAccount? companyServiceAccount = null)
    {
        var createNotificationsEnumerator = SetupServices(companyServiceAccount);

        if (offerSubscription != null)
        {
            A.CallTo(() =>
                    _offerSubscriptionsRepository.AttachAndModifyOfferSubscription(A<Guid>._,
                        A<Action<OfferSubscription>>._))
                .Invokes((Guid _, Action<OfferSubscription> modify) => { modify.Invoke(offerSubscription); });
        }

        A.CallTo(() => _offerSubscriptionsRepository.GetOfferDetailsAndCheckUser(
                A<Guid>.That.Matches(x => x == _validSubscriptionId),
                A<string>.That.Matches(x => x == IamUserId),
                A<OfferTypeId>._))
            .ReturnsLazily(() => new OfferSubscriptionTransferData(OfferSubscriptionStatusId.ACTIVE, _companyUser.Id,
                Guid.Empty,
                _companyUser.Company!.Name, _companyUser.CompanyId, _companyUser.Id, _existingServiceId, "Test Service",
                Bpn, "user@email.com", "Tony", "Gilbert", (isSingleInstance, "https://test.de"),
                new[] {Guid.NewGuid()},
                _salesManagerId));
        A.CallTo(() => _offerSubscriptionsRepository.GetOfferDetailsAndCheckUser(
                A<Guid>.That.Matches(x => x == _pendingSubscriptionId),
                A<string>.That.Matches(x => x == IamUserIdWithoutMail),
                A<OfferTypeId>._))
            .ReturnsLazily(() => new OfferSubscriptionTransferData(OfferSubscriptionStatusId.PENDING, _companyUser.Id,
                Guid.Empty,
                _companyUser.Company!.Name, _companyUser.CompanyId, _companyUser.Id, _existingServiceId, "Test Service",
                Bpn, null, null, null, (isSingleInstance, "https://test.de"),
                new[] {Guid.NewGuid()},
                _salesManagerId));
        A.CallTo(() => _offerSubscriptionsRepository.GetOfferDetailsAndCheckUser(
                A<Guid>.That.Matches(x => x == _pendingSubscriptionId),
                A<string>.That.Matches(x => x == IamUserId),
                A<OfferTypeId>._))
            .ReturnsLazily(() => new OfferSubscriptionTransferData(OfferSubscriptionStatusId.PENDING, _companyUser.Id,
                Guid.Empty,
                string.Empty, _companyUser.CompanyId, _companyUser.Id, _existingServiceId, "Test Service",
                Bpn, "user@email.com", "Tony", "Gilbert", (isSingleInstance, "https://test.de"),
                new[] {Guid.NewGuid()},
                _salesManagerId));
        A.CallTo(() => _offerSubscriptionsRepository.GetOfferDetailsAndCheckUser(
                A<Guid>.That.Matches(x => x == _offerIdWithInstanceNotSet),
                A<string>.That.Matches(x => x == IamUserId),
                A<OfferTypeId>._))
            .ReturnsLazily(() => new OfferSubscriptionTransferData(OfferSubscriptionStatusId.PENDING, _companyUser.Id,
                Guid.Empty,
                string.Empty, _companyUser.CompanyId, _companyUser.Id, _existingServiceId, "Test Service",
                Bpn, "user@email.com", "Tony", "Gilbert", (isSingleInstance, null),
                new List<Guid>(),
                null));
        A.CallTo(() => _offerSubscriptionsRepository.GetOfferDetailsAndCheckUser(
                A<Guid>.That.Not.Matches(x =>
                    x == _pendingSubscriptionId || x == _validSubscriptionId || x == _offerIdWithInstanceNotSet),
                A<string>.That.Matches(x => x == IamUserId),
                A<OfferTypeId>._))
            .ReturnsLazily(() => (OfferSubscriptionTransferData?) null);
        A.CallTo(() => _offerSubscriptionsRepository.GetOfferDetailsAndCheckUser(
                A<Guid>.That.Matches(x => x == _pendingSubscriptionId),
                A<string>.That.Not.Matches(x => x == IamUserId || x == IamUserIdWithoutMail),
                A<OfferTypeId>._))
            .ReturnsLazily(() => new OfferSubscriptionTransferData(OfferSubscriptionStatusId.PENDING, Guid.Empty,
                Guid.Empty,
                string.Empty, _companyUser.CompanyId, _companyUser.Id, _existingServiceId, "Test Service",
                Bpn, null, null, null, (isSingleInstance, "https://test.de"),
                new[] {Guid.NewGuid()},
                null));

        return createNotificationsEnumerator;
    }

    private void SetupCreateSingleInstance(AppInstance? appInstance = null)
    {
        SetupServices();

        if (appInstance != null)
        {
            A.CallTo(() => _appInstanceRepository.CreateAppInstanceAssignedServiceAccounts(A<IEnumerable<(Guid, Guid)>>._))
                .Invokes((IEnumerable<(Guid AppInstanceId, Guid CompanyServiceAccountId)> instanceAccounts) =>
                {
                    foreach (var i in instanceAccounts.Select(x =>
                                 new AppInstanceAssignedCompanyServiceAccount(x.AppInstanceId,
                                     x.CompanyServiceAccountId)))
                    {
                        appInstance.ServiceAccounts.Add(i);
                    }
                });
        }

        A.CallTo(() => _offerRepository.GetSingleInstanceOfferData(_validOfferId, OfferTypeId.APP))
            .Returns(new SingleInstanceOfferData(_companyUserCompanyId, "app1", Bpn, true, new [] { (_validInstanceSetupId, "cl1") }));
        A.CallTo(() => _offerRepository.GetSingleInstanceOfferData(_offerIdWithoutClient, OfferTypeId.APP))
            .Returns(new SingleInstanceOfferData(_companyUserCompanyId, "app1", Bpn, true, new [] { (_validInstanceSetupId, string.Empty) }));
        A.CallTo(() => _offerRepository.GetSingleInstanceOfferData(_offerIdWithInstanceNotSet, OfferTypeId.APP))
            .Returns(new SingleInstanceOfferData(_companyUserCompanyId, "app1", Bpn, true, Enumerable.Empty<(Guid,string)>()));
        A.CallTo(() => _offerRepository.GetSingleInstanceOfferData(A<Guid>.That.Not.Matches(x => x == _offerIdWithoutClient || x == _validOfferId || x == _offerIdWithInstanceNotSet), OfferTypeId.APP))
            .ReturnsLazily(() => (SingleInstanceOfferData?)null);
    }

    #endregion
}
