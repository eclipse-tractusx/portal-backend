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

using System.Net;
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

namespace Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Tests.Service;

public class OfferSetupServiceTests
{
    private const string AccessToken = "THISISAACCESSTOKEN";
    private const string Bpn = "CAXSDUMMYCATENAZZ";

    private readonly Guid _companyUserCompanyId = new("395f955b-f11b-4a74-ab51-92a526c1973a");
    private readonly Guid _existingServiceId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47661");
    private readonly Guid _validSubscriptionId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47662");
    private readonly Guid _pendingSubscriptionId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47663");
    private readonly Guid _technicalUserId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47999");

    private readonly CompanyUser _companyUser;
    private readonly IFixture _fixture;
    private readonly string _iamUserId;
    private readonly string _iamUserIdWithoutMail;
    private readonly IAppInstanceRepository _appInstanceRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IAppSubscriptionDetailRepository _appSubscriptionDetailRepository;
    private readonly IOfferSubscriptionsRepository _offerSubscriptionsRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IProvisioningManager _provisioningManager;
    private readonly IServiceAccountCreation _serviceAccountCreation;
    private readonly INotificationService _notificationService;
    private readonly IMailingService _mailingService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly OfferSetupService _sut;

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
        _iamUserId = _fixture.Create<string>();
        _iamUserIdWithoutMail = _fixture.Create<string>();
        _portalRepositories = A.Fake<IPortalRepositories>();
        _appSubscriptionDetailRepository = A.Fake<IAppSubscriptionDetailRepository>();
        _appInstanceRepository = A.Fake<IAppInstanceRepository>();
        _clientRepository = A.Fake<IClientRepository>();
        _offerSubscriptionsRepository = A.Fake<IOfferSubscriptionsRepository>();
        _provisioningManager = A.Fake<IProvisioningManager>();
        _notificationRepository = A.Fake<INotificationRepository>();
        _serviceAccountCreation = A.Fake<IServiceAccountCreation>();
        _notificationService = A.Fake<INotificationService>();
        _mailingService = A.Fake<IMailingService>();
        _httpClientFactory = A.Fake<IHttpClientFactory>();
        
        A.CallTo(() => _portalRepositories.GetInstance<IAppSubscriptionDetailRepository>()).Returns(_appSubscriptionDetailRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IAppInstanceRepository>()).Returns(_appInstanceRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IClientRepository>()).Returns(_clientRepository);
        A.CallTo(() => _portalRepositories.GetInstance<INotificationRepository>()).Returns(_notificationRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IOfferSubscriptionsRepository>()).Returns(_offerSubscriptionsRepository);

        _sut = new OfferSetupService(_portalRepositories, _provisioningManager, _serviceAccountCreation, _notificationService, _mailingService, _httpClientFactory);
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
    [InlineData(OfferTypeId.APP, true)]
    [InlineData(OfferTypeId.SERVICE, true)]
    [InlineData(OfferTypeId.SERVICE, false)]
    public async Task AutoSetup_WithValidData_ReturnsExpectedNotificationAndSecret(OfferTypeId offerTypeId, bool technicalUserRequired)
    {
        // Arrange
        var offerSubscription = new OfferSubscription(Guid.NewGuid(), Guid.Empty, Guid.Empty, OfferSubscriptionStatusId.PENDING, Guid.Empty, Guid.Empty);
        var createNotificationsEnumerator = Setup(technicalUserRequired, offerSubscription);
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
                var client = new IamClient(clientId, clientName!);
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
        var serviceAccountRoles = new Dictionary<string, IEnumerable<string>>
        {
            { "technical_roles_management", new [] { "Digital Twin Management" } }
        };
        var companyAdminRoles = new Dictionary<string, IEnumerable<string>>
        {
            { "Cl2-CX-Portal", new [] { "IT Admin" } }
        };

        var data = new OfferAutoSetupData(_pendingSubscriptionId, "https://new-url.com/");

        // Act
        var result = await _sut.AutoSetupOfferAsync(data, serviceAccountRoles, companyAdminRoles, _iamUserId, offerTypeId, "https://base-address.com").ConfigureAwait(false);
        
        // Assert
        result.Should().NotBeNull();
        if (technicalUserRequired)
        {
            result.TechnicalUserInfo.Should().NotBeNull();
            result.TechnicalUserInfo!.TechnicalUserId.Should().Be(_technicalUserId);
            result.TechnicalUserInfo.TechnicalUserSecret.Should().Be("katze!1234");
        }
        else
        {
            result.TechnicalUserInfo.Should().BeNull();
            result.ClientInfo.Should().BeNull();
            clients.Should().BeEmpty();
        }

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

        notifications.Should().HaveCount(1);
        offerSubscription.OfferSubscriptionStatusId.Should().Be(OfferSubscriptionStatusId.ACTIVE);
        A.CallTo(() => createNotificationsEnumerator.MoveNextAsync()).MustHaveHappened(2, Times.Exactly);
        A.CallTo(() => _mailingService.SendMails(A<string>._, A<Dictionary<string, string>>._, A<List<string>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task AutoSetup_WithValidDataAndUserWithoutMail_NoMailIsSend()
    {
        // Arrange
        Setup();
        var serviceAccountRoles = new Dictionary<string, IEnumerable<string>>
        {
            { "technical_roles_management", new [] { "Digital Twin Management" } }
        };
        var companyAdminRoles = new Dictionary<string, IEnumerable<string>>
        {
            { "Cl2-CX-Portal", new [] { "IT Admin" } }
        };

        var data = new OfferAutoSetupData(_pendingSubscriptionId, "https://new-url.com/");

        // Act
        var result = await _sut.AutoSetupOfferAsync(data, serviceAccountRoles, companyAdminRoles, _iamUserIdWithoutMail, OfferTypeId.SERVICE, "https://base-address.com").ConfigureAwait(false);
        
        // Assert
        result.Should().NotBeNull();
        result.TechnicalUserInfo.Should().BeNull();
        result.ClientInfo.Should().BeNull();
        A.CallTo(() => _mailingService.SendMails(A<string>._, A<Dictionary<string, string>>._, A<List<string>>._)).MustNotHaveHappened();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task AutoSetup_WithNotExistingOfferSubscriptionId_ThrowsException()
    {
        // Arrange
        Setup();
        var data = new OfferAutoSetupData(Guid.NewGuid(), "https://new-url.com/");

        // Act
        async Task Action() => await _sut.AutoSetupOfferAsync(data, new Dictionary<string, IEnumerable<string>>(), new Dictionary<string, IEnumerable<string>>(), _iamUserId, OfferTypeId.SERVICE, "https://base-address.com");
        
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
        Setup();
        var data = new OfferAutoSetupData(_validSubscriptionId, "https://new-url.com/");

        // Act
        async Task Action() => await _sut.AutoSetupOfferAsync(data, new Dictionary<string, IEnumerable<string>>(), new Dictionary<string, IEnumerable<string>>(), _iamUserId, OfferTypeId.SERVICE, "https://base-address.com");
        
        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Action);
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        A.CallTo(() => _mailingService.SendMails(A<string>._, A<Dictionary<string, string>>._, A<List<string>>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task AutoSetup_WithUserNotFromProvidingCompany_ThrowsException()
    {
        // Arrange
        Setup();
        var data = new OfferAutoSetupData(_pendingSubscriptionId, "https://new-url.com/");

        // Act
        async Task Action() => await _sut.AutoSetupOfferAsync(data, new Dictionary<string, IEnumerable<string>>(), new Dictionary<string, IEnumerable<string>>(), Guid.NewGuid().ToString(), OfferTypeId.SERVICE, "https://base-address.com");
        
        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Action);
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        A.CallTo(() => _mailingService.SendMails(A<string>._, A<Dictionary<string, string>>._, A<List<string>>._)).MustNotHaveHappened();
    }

    #endregion
    
    #region Setup

    private IAsyncEnumerator<Guid> SetupServices()
    {
        A.CallTo(() => _provisioningManager.SetupClientAsync(A<string>._, A<string>._, A<IEnumerable<string>?>._))
            .ReturnsLazily(() => "cl1");
        
        A.CallTo(() => _serviceAccountCreation.CreateServiceAccountAsync(A<ServiceAccountCreationInfo>._, A<Guid>._, A<IEnumerable<string>>.That.Matches(x => x.Any(y => y == "CAXSDUMMYCATENAZZ")), CompanyServiceAccountTypeId.MANAGED, A<bool>._, A<Action<CompanyServiceAccount>?>._))
            .ReturnsLazily(() => new ValueTuple<string, ServiceAccountData, Guid, List<UserRoleData>>(
                "sa2",
                new ServiceAccountData(Guid.NewGuid().ToString(), "cl1", new ClientAuthData(IamClientAuthMethod.SECRET)
                {
                    Secret = "katze!1234"
                }),
                _technicalUserId, 
                new List<UserRoleData>()));

        A.CallTo(() => _notificationService.CreateNotifications(A<IDictionary<string, IEnumerable<string>>>._,
                A<Guid>._, A<IEnumerable<(string?, NotificationTypeId)>>._, A<Guid>._))
            .Returns(new List<Guid>{Guid.NewGuid()}.AsFakeIAsyncEnumerable(out var createNotificationsEnumerator));

        return createNotificationsEnumerator;
    }

    private IAsyncEnumerator<Guid> Setup(bool technicalUserRequired = false, OfferSubscription? offerSubscription = null)
    {
        var createNotificationsEnumerator = SetupServices();

        if (offerSubscription != null)
        {
            A.CallTo(() => _offerSubscriptionsRepository.AttachAndModifyOfferSubscription(A<Guid>._, A<Action<OfferSubscription>>._))
                .Invokes((Guid _, Action<OfferSubscription> modify) =>
                {
                    modify.Invoke(offerSubscription);
                });
        }

        A.CallTo(() => _offerSubscriptionsRepository.GetOfferDetailsAndCheckUser(
                A<Guid>.That.Matches(x => x == _validSubscriptionId),
                A<string>.That.Matches(x => x == _iamUserId),
                A<OfferTypeId>._))
            .ReturnsLazily(() => new OfferSubscriptionTransferData(OfferSubscriptionStatusId.ACTIVE, _companyUser.Id, Guid.Empty,
                _companyUser.Company!.Name, _companyUser.CompanyId, _companyUser.Id, _existingServiceId, "Test Service",
                Bpn, "user@email.com", "Tony", "Gilbert", technicalUserRequired));
        A.CallTo(() => _offerSubscriptionsRepository.GetOfferDetailsAndCheckUser(
                A<Guid>.That.Matches(x => x == _pendingSubscriptionId),
                A<string>.That.Matches(x => x == _iamUserIdWithoutMail),
                A<OfferTypeId>._))
            .ReturnsLazily(() => new OfferSubscriptionTransferData(OfferSubscriptionStatusId.PENDING, _companyUser.Id, Guid.Empty,
                _companyUser.Company!.Name, _companyUser.CompanyId, _companyUser.Id, _existingServiceId, "Test Service",
                Bpn, null, null, null, technicalUserRequired));
        A.CallTo(() => _offerSubscriptionsRepository.GetOfferDetailsAndCheckUser(
                A<Guid>.That.Matches(x => x == _pendingSubscriptionId),
                A<string>.That.Matches(x => x == _iamUserId),
                A<OfferTypeId>._))
            .ReturnsLazily(() => new OfferSubscriptionTransferData(OfferSubscriptionStatusId.PENDING, _companyUser.Id, Guid.Empty,
                string.Empty, _companyUser.CompanyId, _companyUser.Id, _existingServiceId, "Test Service",
                Bpn, "user@email.com", "Tony", "Gilbert", technicalUserRequired));
        A.CallTo(() => _offerSubscriptionsRepository.GetOfferDetailsAndCheckUser(
                A<Guid>.That.Not.Matches(x => x == _pendingSubscriptionId || x == _validSubscriptionId),
                A<string>.That.Matches(x => x == _iamUserId),
                A<OfferTypeId>._))
            .ReturnsLazily(() => (OfferSubscriptionTransferData?)null);
        A.CallTo(() => _offerSubscriptionsRepository.GetOfferDetailsAndCheckUser(
                A<Guid>.That.Matches(x => x == _pendingSubscriptionId),
                A<string>.That.Not.Matches(x => x == _iamUserId || x == _iamUserIdWithoutMail),
                A<OfferTypeId>._))
            .ReturnsLazily(() =>new OfferSubscriptionTransferData(OfferSubscriptionStatusId.PENDING, Guid.Empty, Guid.Empty,
                string.Empty, _companyUser.CompanyId, _companyUser.Id, _existingServiceId, "Test Service",
                Bpn, null, null, null, technicalUserRequired));

        return createNotificationsEnumerator;
    }

    #endregion
}
