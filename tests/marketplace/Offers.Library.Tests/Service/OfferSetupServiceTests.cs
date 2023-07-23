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

using Org.Eclipse.TractusX.Portal.Backend.Framework.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Mailing.SendMail;
using Org.Eclipse.TractusX.Portal.Backend.Notifications.Library;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.Library;
using Org.Eclipse.TractusX.Portal.Backend.Processes.OfferSubscription.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;
using PortalBackend.PortalEntities.Identity;

namespace Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Tests.Service;

public class OfferSetupServiceTests
{
    private const string Bpn = "CAXSDUMMYCATENAZZ";
    private const string IamUserId = "9aae7a3b-b188-4a42-b46b-fb2ea5f47668";
    private static readonly Guid CompanyUserCompanyId = new("395f955b-f11b-4a74-ab51-92a526c1973a");

    private readonly IdentityData _identity = new(IamUserId, Guid.NewGuid(), IdentityTypeId.COMPANY_USER, CompanyUserCompanyId);
    private readonly Guid _existingServiceId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47661");
    private readonly Guid _validSubscriptionId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47662");
    private readonly Guid _pendingSubscriptionId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47663");
    private readonly Guid _validOfferId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47664");
    private readonly Guid _offerIdWithoutClient = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47665");
    private readonly Guid _offerIdWithMultipleInstances = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47667");
    private readonly Guid _offerIdWithWithNoInstanceSetupIds = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47668");
    private readonly Guid _validInstanceSetupId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47666");
    private readonly Guid _technicalUserId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47998");
    private readonly Guid _salesManagerId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47999");
    private readonly Guid _companyUserWithoutMailId = new("9aae7a3b-b188-4a99-b46b-fb2ea5f47111");
    private readonly Guid _companyUserWithoutMailCompanyId = new("9aae7a3b-b188-4a99-b46b-fb2ea5f47112");

    private readonly IFixture _fixture;
    private readonly IAppInstanceRepository _appInstanceRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IAppSubscriptionDetailRepository _appSubscriptionDetailRepository;
    private readonly IOfferSubscriptionsRepository _offerSubscriptionsRepository;
    private readonly IOfferRepository _offerRepository;
    private readonly IUserRolesRepository _userRolesRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IProvisioningManager _provisioningManager;
    private readonly IServiceAccountCreation _serviceAccountCreation;
    private readonly INotificationService _notificationService;
    private readonly IMailingService _mailingService;
    private readonly OfferSetupService _sut;
    private readonly ITechnicalUserProfileService _technicalUserProfileService;
    private readonly IOfferSubscriptionProcessService _offerSubscriptionProcessService;

    public OfferSetupServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _portalRepositories = A.Fake<IPortalRepositories>();
        _appSubscriptionDetailRepository = A.Fake<IAppSubscriptionDetailRepository>();
        _appInstanceRepository = A.Fake<IAppInstanceRepository>();
        _clientRepository = A.Fake<IClientRepository>();
        _offerSubscriptionsRepository = A.Fake<IOfferSubscriptionsRepository>();
        _offerRepository = A.Fake<IOfferRepository>();
        _userRolesRepository = A.Fake<IUserRolesRepository>();
        _provisioningManager = A.Fake<IProvisioningManager>();
        _notificationRepository = A.Fake<INotificationRepository>();
        _serviceAccountCreation = A.Fake<IServiceAccountCreation>();
        _notificationService = A.Fake<INotificationService>();
        _mailingService = A.Fake<IMailingService>();
        _technicalUserProfileService = A.Fake<ITechnicalUserProfileService>();

        _offerSubscriptionProcessService = A.Fake<IOfferSubscriptionProcessService>();

        A.CallTo(() => _portalRepositories.GetInstance<IAppSubscriptionDetailRepository>()).Returns(_appSubscriptionDetailRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IAppInstanceRepository>()).Returns(_appInstanceRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IClientRepository>()).Returns(_clientRepository);
        A.CallTo(() => _portalRepositories.GetInstance<INotificationRepository>()).Returns(_notificationRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IOfferSubscriptionsRepository>()).Returns(_offerSubscriptionsRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IOfferRepository>()).Returns(_offerRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRolesRepository>()).Returns(_userRolesRepository);

        _sut = new OfferSetupService(_portalRepositories, _provisioningManager, _serviceAccountCreation, _notificationService, _offerSubscriptionProcessService, _mailingService, _technicalUserProfileService);
    }

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
        var companyServiceAccount = new CompanyServiceAccount(Guid.NewGuid(), "test", "test", CompanyServiceAccountTypeId.OWN);
        var createNotificationsEnumerator = SetupAutoSetup(offerTypeId, offerSubscription, isSingleInstance, companyServiceAccount);
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
        var serviceManagerRoles = new[]
        {
            new UserRoleConfig("Cl2-CX-Portal", new [] { "Service Manager" })
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
        var companyAdminRoles = new[]
        {
            new UserRoleConfig("Cl2-CX-Portal", new [] { "IT Admin" })
        };

        var data = new OfferAutoSetupData(_pendingSubscriptionId, "https://new-url.com/");

        // Act
        var result = await _sut.AutoSetupOfferAsync(data, companyAdminRoles, (_identity.UserId, _identity.CompanyId), offerTypeId, "https://base-address.com", serviceManagerRoles).ConfigureAwait(false);

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
            result.TechnicalUserInfo!.Should().Match<TechnicalUserInfoData>(info =>
                info.TechnicalUserId == _technicalUserId &&
                info.TechnicalUserSecret == "katze!1234" &&
                info.TechnicalUserPermissions.SequenceEqual(new[] { "role1", "role2" })
            );
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
                A<IEnumerable<UserRoleConfig>>._,
                A<IEnumerable<NotificationTypeId>>.That.Matches(x =>
                    x.Count() == 1 && x.Single() == notificationTypeId),
                _existingServiceId,
                A<IEnumerable<Guid>?>.That.Matches(x => x != null && x.Count() == 1 && x.Single() == _salesManagerId)))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => createNotificationsEnumerator.MoveNextAsync()).MustHaveHappened(2, Times.Exactly);
        offerSubscription.OfferSubscriptionStatusId.Should().Be(OfferSubscriptionStatusId.ACTIVE);
        if (!isSingleInstance)
        {
            A.CallTo(() => _mailingService.SendMails(A<string>._, A<Dictionary<string, string>>._, A<IEnumerable<string>>._)).MustHaveHappenedOnceExactly();
        }
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task AutoSetup_WithMultipleTechnicalUsers_ThrowsException()
    {
        // Arrange
        var offerSubscription = new OfferSubscription(Guid.NewGuid(), Guid.Empty, Guid.Empty, OfferSubscriptionStatusId.PENDING, Guid.Empty, Guid.Empty);
        var companyServiceAccount = new CompanyServiceAccount(Guid.NewGuid(), "test", "test", CompanyServiceAccountTypeId.OWN);
        SetupAutoSetup(OfferTypeId.APP, offerSubscription, false, companyServiceAccount);
        var clientId = Guid.NewGuid();
        var appInstanceId = Guid.NewGuid();
        var clients = new List<IamClient>();
        A.CallTo(() => _clientRepository.CreateClient(A<string>._))
            .Invokes((string clientName) =>
            {
                var client = new IamClient(clientId, clientName);
                clients.Add(client);
            })
            .Returns(new IamClient(clientId, "cl1"));
        A.CallTo(() => _technicalUserProfileService.GetTechnicalUserProfilesForOfferSubscription(A<Guid>._))
            .Returns(new ServiceAccountCreationInfo[]
            {
                new(Guid.NewGuid().ToString(), "test", IamClientAuthMethod.SECRET, new List<Guid>()),
                new(Guid.NewGuid().ToString(), "test1", IamClientAuthMethod.SECRET, new List<Guid>())
            });

        A.CallTo(() => _appInstanceRepository.CreateAppInstance(A<Guid>._, A<Guid>._))
            .Returns(new AppInstance(appInstanceId, _existingServiceId, clientId));

        var companyAdminRoles = new[]
        {
            new UserRoleConfig("Cl2-CX-Portal", new[] { "IT Admin" })
        };
        var serviceManagerAdminRoles = new[]
        {
            new UserRoleConfig("Cl2-CX-Portal", new[] { "Service Manager" })
        };

        var data = new OfferAutoSetupData(_pendingSubscriptionId, "https://new-url.com/");

        // Act
        async Task Act() => await _sut.AutoSetupOfferAsync(data, companyAdminRoles, (_identity.UserId, _identity.CompanyId), OfferTypeId.APP, "https://base-address.com", serviceManagerAdminRoles).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);
        ex.Message.Should().Be($"There should only be one or none technical user profile configured for {data.RequestId}");
    }

    [Fact]
    public async Task AutoSetup_WithValidDataAndUserWithoutMail_NoMailIsSend()
    {
        // Arrange
        SetupAutoSetup(OfferTypeId.APP);
        var companyAdminRoles = new[]
        {
            new UserRoleConfig("Cl2-CX-Portal", new[] { "IT Admin" })
        };
        var serviceManagerRoles = new[]
        {
            new UserRoleConfig("Cl2-CX-Portal", new [] { "Service Manager" })
        };

        var data = new OfferAutoSetupData(_pendingSubscriptionId, "https://new-url.com/");

        // Act
        var result = await _sut.AutoSetupOfferAsync(data, companyAdminRoles, (_companyUserWithoutMailId, _companyUserWithoutMailCompanyId), OfferTypeId.SERVICE, "https://base-address.com", serviceManagerRoles).ConfigureAwait(false);

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
        SetupAutoSetup(OfferTypeId.APP, isSingleInstance: true);
        var companyAdminRoles = new[]
        {
            new UserRoleConfig("Cl2-CX-Portal", new[] { "IT Admin" })
        };
        var serviceManagerRoles = new[]
        {
            new UserRoleConfig("Cl2-CX-Portal", new [] { "Service Manager" })
        };

        var data = new OfferAutoSetupData(_offerIdWithMultipleInstances, "https://new-url.com/");

        // Act
        async Task Act() => await _sut.AutoSetupOfferAsync(data, companyAdminRoles, (_identity.UserId, _identity.CompanyId), OfferTypeId.APP, "https://base-address.com", serviceManagerRoles).ConfigureAwait(false);

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
        SetupAutoSetup(OfferTypeId.APP);
        var data = new OfferAutoSetupData(Guid.NewGuid(), "https://new-url.com/");
        A.CallTo(() => _offerSubscriptionsRepository.GetOfferDetailsAndCheckProviderCompany(data.RequestId, _identity.CompanyId, OfferTypeId.SERVICE))
            .Returns((OfferSubscriptionTransferData?)null);

        // Act
        async Task Action() => await _sut.AutoSetupOfferAsync(data, Enumerable.Empty<UserRoleConfig>(), (_identity.UserId, _identity.CompanyId), OfferTypeId.SERVICE, "https://base-address.com", Enumerable.Empty<UserRoleConfig>());

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
        SetupAutoSetup(OfferTypeId.APP);
        var data = new OfferAutoSetupData(_validSubscriptionId, "https://new-url.com/");

        // Act
        async Task Action() => await _sut.AutoSetupOfferAsync(data, Enumerable.Empty<UserRoleConfig>(), (_identity.UserId, _identity.CompanyId), OfferTypeId.SERVICE, "https://base-address.com", Enumerable.Empty<UserRoleConfig>());

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Action);
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
        A.CallTo(() => _mailingService.SendMails(A<string>._, A<Dictionary<string, string>>._, A<List<string>>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task AutoSetup_WithUserNotFromProvidingCompany_ThrowsException()
    {
        // Arrange
        SetupAutoSetup(OfferTypeId.APP);
        var data = new OfferAutoSetupData(_pendingSubscriptionId, "https://new-url.com/");

        // Act
        async Task Action() => await _sut.AutoSetupOfferAsync(data, Enumerable.Empty<UserRoleConfig>(), (Guid.NewGuid(), Guid.NewGuid()), OfferTypeId.SERVICE, "https://base-address.com", Enumerable.Empty<UserRoleConfig>());

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
            .MustHaveHappened(2, Times.Exactly);
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
    public async Task ActivateSingleInstanceAppAsync_WithNoInstanceSetupId_ThrowsConflictException()
    {
        SetupCreateSingleInstance();

        async Task Act() => await _sut.ActivateSingleInstanceAppAsync(_offerIdWithWithNoInstanceSetupIds).ConfigureAwait(false);

        var ex = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);
        ex.Message.Should().Be($"There should always be exactly one instance defined for a single instance offer {_offerIdWithWithNoInstanceSetupIds}");
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
    public async Task ActivateSingleInstanceAppAsync_WithMultipleInstances_ThrowsConflictException()
    {
        SetupCreateSingleInstance();
        async Task Act() => await _sut.ActivateSingleInstanceAppAsync(_offerIdWithMultipleInstances).ConfigureAwait(false);

        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"offer {_offerIdWithMultipleInstances} is not set up as single instance app");
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
            .Returns(false);
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
            .Returns(true);

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
            .Returns(new[] { serviceAccountId }.ToAsyncEnumerable());

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

    #region StartAutoSetupAsync

    [Theory]
    [InlineData(OfferTypeId.APP)]
    [InlineData(OfferTypeId.SERVICE)]
    public async Task StartAutoSetupAsync_WithNotExistingOfferSubscription_ThrowsNotFoundException(OfferTypeId offerTypeId)
    {
        // Arrange
        var offerSubscriptionId = Guid.NewGuid();
        var data = new OfferAutoSetupData(offerSubscriptionId, "https://www.test.de");
        A.CallTo(() => _offerSubscriptionsRepository.GetOfferDetailsAndCheckProviderCompany(offerSubscriptionId, _identity.UserId, offerTypeId))
            .Returns((OfferSubscriptionTransferData?)null);

        // Act
        async Task Act() => await _sut.StartAutoSetupAsync(data, _identity.UserId, offerTypeId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"OfferSubscription {offerSubscriptionId} does not exist");
    }

    [Theory]
    [InlineData(OfferTypeId.APP)]
    [InlineData(OfferTypeId.SERVICE)]
    public async Task StartAutoSetupAsync_WithWrongStatue_ThrowsConflictException(OfferTypeId offerTypeId)
    {
        // Arrange
        var transferData = _fixture.Build<OfferSubscriptionTransferData>()
            .With(x => x.Status, OfferSubscriptionStatusId.ACTIVE)
            .Create();
        var offerSubscriptionId = Guid.NewGuid();
        var data = new OfferAutoSetupData(offerSubscriptionId, "https://www.test.de");
        A.CallTo(() => _offerSubscriptionsRepository.GetOfferDetailsAndCheckProviderCompany(offerSubscriptionId, _identity.UserId, offerTypeId))
            .Returns(transferData);

        // Act
        async Task Act() => await _sut.StartAutoSetupAsync(data, _identity.UserId, offerTypeId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("Status of the offer subscription must be pending");

    }

    [Theory]
    [InlineData(OfferTypeId.APP)]
    [InlineData(OfferTypeId.SERVICE)]
    public async Task StartAutoSetupAsync_WithNotProvidingCompany_ThrowsForbiddenException(OfferTypeId offerTypeId)
    {
        // Arrange
        var transferData = _fixture.Build<OfferSubscriptionTransferData>()
            .With(x => x.Status, OfferSubscriptionStatusId.PENDING)
            .With(x => x.IsProviderCompany, false)
            .Create();
        var offerSubscriptionId = Guid.NewGuid();
        var data = new OfferAutoSetupData(offerSubscriptionId, "https://www.test.de");
        A.CallTo(() => _offerSubscriptionsRepository.GetOfferDetailsAndCheckProviderCompany(offerSubscriptionId, _identity.UserId, offerTypeId))
            .Returns(transferData);

        // Act
        async Task Act() => await _sut.StartAutoSetupAsync(data, _identity.UserId, offerTypeId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be("Only the providing company can setup the service");
    }

    [Theory]
    [InlineData(OfferTypeId.APP)]
    [InlineData(OfferTypeId.SERVICE)]
    public async Task StartAutoSetupAsync_WithMultipleInstancesForSingleInstance_ThrowsConflictException(OfferTypeId offerTypeId)
    {
        // Arrange
        var transferData = _fixture.Build<OfferSubscriptionTransferData>()
            .With(x => x.Status, OfferSubscriptionStatusId.PENDING)
            .With(x => x.IsProviderCompany, true)
            .With(x => x.InstanceData, new ValueTuple<bool, string?>(true, null))
            .With(x => x.AppInstanceIds, new[] { Guid.NewGuid(), Guid.NewGuid() })
            .Create();
        var offerSubscriptionId = Guid.NewGuid();
        var data = new OfferAutoSetupData(offerSubscriptionId, "https://www.test.de");
        A.CallTo(() => _offerSubscriptionsRepository.GetOfferDetailsAndCheckProviderCompany(offerSubscriptionId, _identity.UserId, offerTypeId))
            .Returns(transferData);

        // Act
        async Task Act() => await _sut.StartAutoSetupAsync(data, _identity.UserId, offerTypeId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("There must only be one app instance for single instance apps");
    }

    [Theory]
    [InlineData(OfferTypeId.APP)]
    [InlineData(OfferTypeId.SERVICE)]
    public async Task StartAutoSetupAsync_WithValidSingleInstance_ThrowsConflictException(OfferTypeId offerTypeId)
    {
        // Arrange
        var transferData = _fixture.Build<OfferSubscriptionTransferData>()
            .With(x => x.Status, OfferSubscriptionStatusId.PENDING)
            .With(x => x.IsProviderCompany, true)
            .With(x => x.InstanceData, new ValueTuple<bool, string?>(true, "https://www.test.de"))
            .With(x => x.AppInstanceIds, new[] { Guid.NewGuid() })
            .Create();
        var offerSubscriptionId = Guid.NewGuid();
        var process = _fixture.Create<Process>();
        var data = new OfferAutoSetupData(offerSubscriptionId, "https://www.test.de");
        A.CallTo(() => _offerSubscriptionsRepository.GetOfferDetailsAndCheckProviderCompany(offerSubscriptionId, _identity.UserId, offerTypeId))
            .Returns(transferData);
        A.CallTo(() =>
                _offerSubscriptionProcessService.VerifySubscriptionAndProcessSteps(offerSubscriptionId,
                    ProcessStepTypeId.START_AUTOSETUP, null, false))
            .Returns(new ManualProcessStepData(
                ProcessStepTypeId.START_AUTOSETUP,
                process,
                new ProcessStep[]
                {
                    new (Guid.NewGuid(), ProcessStepTypeId.START_AUTOSETUP, ProcessStepStatusId.TODO, process.Id, DateTimeOffset.Now)
                },
                _portalRepositories));

        // Act
        async Task Act() => await _sut.StartAutoSetupAsync(data, _identity.UserId, offerTypeId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("This step is not eligible to run for single instance apps");
    }

    [Theory]
    [InlineData(OfferTypeId.APP)]
    [InlineData(OfferTypeId.SERVICE)]
    public async Task StartAutoSetupAsync_WithValidMultiInstance_ReturnsExpected(OfferTypeId offerTypeId)
    {
        // Arrange
        var transferData = _fixture.Build<OfferSubscriptionTransferData>()
            .With(x => x.Status, OfferSubscriptionStatusId.PENDING)
            .With(x => x.IsProviderCompany, true)
            .With(x => x.InstanceData, new ValueTuple<bool, string?>(false, null))
            .With(x => x.AppInstanceIds, new[] { Guid.NewGuid(), Guid.NewGuid() })
            .Create();
        var offerSubscriptionId = Guid.NewGuid();
        var process = _fixture.Create<Process>();
        var data = new OfferAutoSetupData(offerSubscriptionId, "https://www.test.de");
        A.CallTo(() => _offerSubscriptionsRepository.GetOfferDetailsAndCheckProviderCompany(offerSubscriptionId, _identity.UserId, offerTypeId))
            .Returns(transferData);
        A.CallTo(() =>
                _offerSubscriptionProcessService.VerifySubscriptionAndProcessSteps(offerSubscriptionId,
                    ProcessStepTypeId.START_AUTOSETUP, null, false))
            .Returns(new ManualProcessStepData(
                ProcessStepTypeId.START_AUTOSETUP,
                process,
                new ProcessStep[]
                {
                    new (Guid.NewGuid(), ProcessStepTypeId.START_AUTOSETUP, ProcessStepStatusId.TODO, process.Id, DateTimeOffset.Now)
                },
                _portalRepositories));

        // Act
        await _sut.StartAutoSetupAsync(data, _identity.UserId, offerTypeId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _offerSubscriptionsRepository.CreateOfferSubscriptionProcessData(offerSubscriptionId, "https://www.test.de"))
            .MustHaveHappenedOnceExactly();
        var nextStepId = offerTypeId == OfferTypeId.APP
            ? ProcessStepTypeId.OFFERSUBSCRIPTION_CLIENT_CREATION
            : ProcessStepTypeId.OFFERSUBSCRIPTION_TECHNICALUSER_CREATION;
        A.CallTo(() => _offerSubscriptionProcessService.FinalizeProcessSteps(
                A<ManualProcessStepData>._,
                A<IEnumerable<ProcessStepTypeId>>.That.Matches(x => x.Count() == 1 && x.Single() == nextStepId)))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    #endregion

    #region CreateSingleInstanceSubscriptionDetail

    [Fact]
    public async Task CreateSingleInstanceSubscriptionDetail_WithNotExistingOfferSubscription_ThrowsNotFoundException()
    {
        // Arrange
        var offerSubscriptionId = Guid.NewGuid();
        A.CallTo(() => _offerSubscriptionsRepository.GetSubscriptionActivationDataByIdAsync(offerSubscriptionId))
            .Returns((SubscriptionActivationData?)null);

        // Act
        async Task Act() => await _sut.CreateSingleInstanceSubscriptionDetail(offerSubscriptionId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"Offer Subscription {offerSubscriptionId} does not exist");
    }

    [Fact]
    public async Task CreateSingleInstanceSubscriptionDetail_WithMultipleInstancesForSingleInstance_ThrowsConflictException()
    {
        // Arrange
        var transferData = _fixture.Build<SubscriptionActivationData>()
            .With(x => x.Status, OfferSubscriptionStatusId.PENDING)
            .With(x => x.InstanceData, new ValueTuple<bool, string?>(true, "https://www.test.de"))
            .With(x => x.AppInstanceIds, new[] { Guid.NewGuid(), Guid.NewGuid() })
            .Create();
        var offerSubscriptionId = Guid.NewGuid();
        A.CallTo(() => _offerSubscriptionsRepository.GetSubscriptionActivationDataByIdAsync(offerSubscriptionId))
            .Returns(transferData);

        // Act
        async Task Act() => await _sut.CreateSingleInstanceSubscriptionDetail(offerSubscriptionId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("There must only be one app instance for single instance apps");
    }

    [Fact]
    public async Task CreateSingleInstanceSubscriptionDetail_WithMultipleInstance_ThrowsConflictException()
    {
        // Arrange
        var transferData = _fixture.Build<SubscriptionActivationData>()
            .With(x => x.Status, OfferSubscriptionStatusId.PENDING)
            .With(x => x.InstanceData, new ValueTuple<bool, string?>(false, "https://www.test.de"))
            .With(x => x.AppInstanceIds, new[] { Guid.NewGuid(), Guid.NewGuid() })
            .Create();
        var offerSubscriptionId = Guid.NewGuid();
        A.CallTo(() => _offerSubscriptionsRepository.GetSubscriptionActivationDataByIdAsync(offerSubscriptionId))
            .Returns(transferData);

        // Act
        async Task Act() => await _sut.CreateSingleInstanceSubscriptionDetail(offerSubscriptionId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("The process step is only executable for single instance apps");
    }
    [Fact]
    public async Task CreateSingleInstanceSubscriptionDetail_WithValidData_ReturnsExpected()
    {
        // Arrange
        var offerSubscriptionId = Guid.NewGuid();
        var transferData = _fixture.Build<SubscriptionActivationData>()
            .With(x => x.Status, OfferSubscriptionStatusId.PENDING)
            .With(x => x.InstanceData, new ValueTuple<bool, string?>(true, "https://www.test.de"))
            .With(x => x.AppInstanceIds, new[] { Guid.NewGuid() })
            .Create();
        var detail = new AppSubscriptionDetail(Guid.NewGuid(), offerSubscriptionId);
        A.CallTo(() => _offerSubscriptionsRepository.GetSubscriptionActivationDataByIdAsync(offerSubscriptionId))
            .Returns(transferData);
        A.CallTo(() => _appSubscriptionDetailRepository.CreateAppSubscriptionDetail(offerSubscriptionId, A<Action<AppSubscriptionDetail?>>._))
            .Invokes((Guid _, Action<AppSubscriptionDetail> setOptionalParameter) =>
            {
                setOptionalParameter.Invoke(detail);
            });

        // Act
        var result = await _sut.CreateSingleInstanceSubscriptionDetail(offerSubscriptionId).ConfigureAwait(false);

        // Assert
        detail.AppSubscriptionUrl.Should().Be("https://www.test.de");
        result.nextStepTypeIds.Should().ContainSingle().And
            .AllSatisfy(x => x.Should().Be(ProcessStepTypeId.ACTIVATE_SUBSCRIPTION));
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.modified.Should().BeTrue();
        result.processMessage.Should().BeNull();
    }

    #endregion

    #region CreateClient

    [Fact]
    public async Task CreateClient_WithNotExistingOfferSubscription_ThrowsNotFoundException()
    {
        // Arrange
        var offerSubscriptionId = Guid.NewGuid();
        A.CallTo(() => _offerSubscriptionsRepository.GetClientCreationData(offerSubscriptionId))
            .Returns((OfferSubscriptionClientCreationData?)null);

        // Act
        async Task Act() => await _sut.CreateClient(offerSubscriptionId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"offer subscription {offerSubscriptionId} does not exist");
    }

    [Fact]
    public async Task CreateClient_WithService_ThrowsConflictException()
    {
        // Arrange
        var clientCreationData = _fixture.Build<OfferSubscriptionClientCreationData>()
            .With(x => x.OfferType, OfferTypeId.SERVICE)
            .Create();
        var offerSubscriptionId = Guid.NewGuid();
        A.CallTo(() => _offerSubscriptionsRepository.GetClientCreationData(offerSubscriptionId))
            .Returns(clientCreationData);

        // Act
        async Task Act() => await _sut.CreateClient(offerSubscriptionId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"Offers without type {OfferTypeId.APP} are not eligible to run");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CreateClient_WithValidData_ReturnsExpected(bool technicalUserNeeded)
    {
        // Arrange
        const string clientId = "cl1";
        const string url = "https://www.test.de";
        var clients = new List<IamClient>();
        var offerSubscriptionId = Guid.NewGuid();
        var iamClientId = Guid.NewGuid();
        var data = new OfferSubscriptionClientCreationData(_validOfferId, OfferTypeId.APP, url, technicalUserNeeded);
        var detail = new AppSubscriptionDetail(Guid.NewGuid(), offerSubscriptionId);
        A.CallTo(() => _offerSubscriptionsRepository.GetClientCreationData(offerSubscriptionId))
            .Returns(data);
        A.CallTo(() => _userRolesRepository.GetUserRolesForOfferIdAsync(_validOfferId))
            .Returns(new List<string> { "TestRole" }.ToAsyncEnumerable());
        A.CallTo(() => _provisioningManager.SetupClientAsync($"{url}/*", url, A<IEnumerable<string>>._, true))
            .Returns(clientId);
        A.CallTo(() => _clientRepository.CreateClient(clientId))
            .Invokes((string paramClientId) =>
            {
                var client = new IamClient(iamClientId, paramClientId);
                clients.Add(client);
            })
            .Returns(new IamClient(iamClientId, clientId));
        A.CallTo(() => _appSubscriptionDetailRepository.CreateAppSubscriptionDetail(offerSubscriptionId, A<Action<AppSubscriptionDetail?>>._))
            .Invokes((Guid _, Action<AppSubscriptionDetail> setOptionalParameter) =>
            {
                setOptionalParameter.Invoke(detail);
            });

        // Act
        var result = await _sut.CreateClient(offerSubscriptionId).ConfigureAwait(false);

        // Assert
        detail.AppSubscriptionUrl.Should().Be("https://www.test.de");
        result.nextStepTypeIds.Should().ContainSingle().And
            .AllSatisfy(x => x.Should().Be(technicalUserNeeded ?
                ProcessStepTypeId.OFFERSUBSCRIPTION_TECHNICALUSER_CREATION :
                ProcessStepTypeId.ACTIVATE_APPLICATION));
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.modified.Should().BeTrue();
        result.processMessage.Should().BeNull();
        A.CallTo(() => _appInstanceRepository.CreateAppInstance(_validOfferId, iamClientId))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region CreateTechnicalUser

    [Fact]
    public async Task CreateTechnicalUser_WithNotExistingOfferSubscription_ThrowsNotFoundException()
    {
        // Arrange
        var offerSubscriptionId = Guid.NewGuid();
        A.CallTo(() => _offerSubscriptionsRepository.GetTechnicalUserCreationData(offerSubscriptionId))
            .Returns((OfferSubscriptionTechnicalUserCreationData?)null);

        // Act
        async Task Act() => await _sut.CreateTechnicalUser(offerSubscriptionId, null!).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"Offer subscription {offerSubscriptionId} does not exist");
    }

    [Fact]
    public async Task CreateTechnicalUser_WithTechnicalUserNotNeeded_ThrowsConflictException()
    {
        // Arrange
        var offerSubscriptionId = Guid.NewGuid();
        var data = new OfferSubscriptionTechnicalUserCreationData(false, "cl1", "Test App", "Stark Industries", CompanyUserCompanyId, Bpn, OfferTypeId.SERVICE);
        A.CallTo(() => _offerSubscriptionsRepository.GetTechnicalUserCreationData(offerSubscriptionId))
            .Returns(data);

        // Act
        async Task Act() => await _sut.CreateTechnicalUser(offerSubscriptionId, null!).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("Technical user is not needed");
    }

    [Theory]
    [InlineData("cl1")]
    [InlineData(null)]
    public async Task CreateTechnicalUser_WithTechnicalUserNeeded_ReturnsExpected(string? clientId)
    {
        // Arrange
        var offerSubscriptionId = Guid.NewGuid();
        var serviceAccountId = Guid.NewGuid();
        var userRoleId = Guid.NewGuid();
        var userRoleData = new List<UserRoleData> { new(userRoleId, "Client1", "TestRole") };
        var data = new OfferSubscriptionTechnicalUserCreationData(true, clientId, "Test App", "Stark Industries", CompanyUserCompanyId, Bpn, OfferTypeId.SERVICE);
        var serviceAccountData = _fixture.Create<ServiceAccountData>();
        var companyServiceAccount = _fixture.Build<CompanyServiceAccount>()
            .With(x => x.OfferSubscriptionId, (Guid?)null)
            .Create();
        A.CallTo(() => _offerSubscriptionsRepository.GetTechnicalUserCreationData(offerSubscriptionId))
            .Returns(data);
        A.CallTo(() => _userRolesRepository.GetUserRoleDataUntrackedAsync(A<IEnumerable<UserRoleConfig>>._))
            .Returns(userRoleData.ToAsyncEnumerable());
        A.CallTo(() => _technicalUserProfileService.GetTechnicalUserProfilesForOfferSubscription(A<Guid>._))
            .Returns(new ServiceAccountCreationInfo[] { new(Guid.NewGuid().ToString(), "test", IamClientAuthMethod.SECRET, new List<Guid>()) });
        A.CallTo(() => _serviceAccountCreation.CreateServiceAccountAsync(A<ServiceAccountCreationInfo>._, CompanyUserCompanyId, A<IEnumerable<string>>.That.Matches(x => x.Count() == 1 && x.Single() == Bpn), CompanyServiceAccountTypeId.MANAGED, false, A<Action<CompanyServiceAccount>>._))
            .Invokes((ServiceAccountCreationInfo _, Guid _, IEnumerable<string> _, CompanyServiceAccountTypeId _, bool _, Action<CompanyServiceAccount>? setOptionalParameter) =>
            {
                setOptionalParameter?.Invoke(companyServiceAccount);
            })
            .Returns(new ValueTuple<string, ServiceAccountData, Guid, List<UserRoleData>>(
                clientId ?? $"{data.OfferName}-{data.CompanyName}",
                serviceAccountData,
                serviceAccountId,
                userRoleData));
        var itAdminRoles = Enumerable.Repeat(new UserRoleConfig("Test", new[] { "AdminRoles" }), 1);

        // Act
        var result = await _sut.CreateTechnicalUser(offerSubscriptionId, itAdminRoles).ConfigureAwait(false);

        // Assert
        result.nextStepTypeIds.Should().ContainSingle().And
            .AllSatisfy(x => x.Should().Be(ProcessStepTypeId.ACTIVATE_SUBSCRIPTION));
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.modified.Should().BeTrue();
        result.processMessage.Should().BeNull();
        A.CallTo(() => _notificationService.CreateNotifications(A<IEnumerable<UserRoleConfig>>._, null, A<IEnumerable<(string?, NotificationTypeId)>>.That.Matches(x => x.Count() == 1 && x.Single().Item2 == NotificationTypeId.TECHNICAL_USER_CREATION), CompanyUserCompanyId, null))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region ActivateSubscription

    [Fact]
    public async Task ActivateSubscription_WithNotExistingOfferSubscription_ThrowsNotFoundException()
    {
        // Arrange
        var offerSubscriptionId = Guid.NewGuid();
        A.CallTo(() => _offerSubscriptionsRepository.GetSubscriptionActivationDataByIdAsync(offerSubscriptionId))
            .Returns((SubscriptionActivationData?)null);

        // Act
        async Task Act() => await _sut.ActivateSubscription(offerSubscriptionId, null!, null!, null!).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"Offer Subscription {offerSubscriptionId} does not exist");
    }

    [Theory]
    [InlineData(null, true, OfferTypeId.APP)]
    [InlineData(null, false, OfferTypeId.APP)]
    [InlineData("test@email.com", true, OfferTypeId.APP)]
    [InlineData("test@email.com", false, OfferTypeId.APP)]
    [InlineData(null, true, OfferTypeId.SERVICE)]
    [InlineData(null, false, OfferTypeId.SERVICE)]
    [InlineData("test@email.com", true, OfferTypeId.SERVICE)]
    [InlineData("test@email.com", false, OfferTypeId.SERVICE)]
    public async Task ActivateSubscription_WithValidData_ReturnsExpected(string? requesterEmail, bool isSingleInstance, OfferTypeId offerTypeId)
    {
        // Arrange
        var offerSubscription = new OfferSubscription(Guid.NewGuid(), _validOfferId, CompanyUserCompanyId, OfferSubscriptionStatusId.PENDING, _identity.UserId, _identity.UserId);
        var subscriptionProcessData = new List<OfferSubscriptionProcessData>
        {
            new(offerSubscription.Id, "https://www.test.de")
        };
        A.CallTo(() => _notificationService.CreateNotificationsWithExistenceCheck(A<IEnumerable<UserRoleConfig>>._, null, A<IEnumerable<(string?, NotificationTypeId)>>._, A<Guid>._, A<string>._, A<string>._, A<bool?>._))
            .Returns(new[] { Guid.NewGuid() }.AsFakeIAsyncEnumerable(out var createNotificationsEnumerator));
        A.CallTo(() => _offerSubscriptionsRepository.GetSubscriptionActivationDataByIdAsync(offerSubscription.Id))
            .Returns(new SubscriptionActivationData(_validOfferId, OfferSubscriptionStatusId.PENDING, offerTypeId, "Test App", "Stark Industries", _identity.CompanyId, requesterEmail, "Tony", "Stark", Guid.NewGuid(), new(isSingleInstance, null), new[] { Guid.NewGuid() }, true, Guid.NewGuid()));
        A.CallTo(() => _notificationRepository.CheckNotificationExistsForParam(A<Guid>._, A<NotificationTypeId>._, A<string>._, A<string>._))
            .Returns(false);
        A.CallTo(() => _offerSubscriptionsRepository.AttachAndModifyOfferSubscription(offerSubscription.Id, A<Action<OfferSubscription>>._))
            .Invokes((Guid _, Action<OfferSubscription> setOptionalParameter) =>
            {
                setOptionalParameter.Invoke(offerSubscription);
            });
        A.CallTo(() => _offerSubscriptionsRepository.RemoveOfferSubscriptionProcessData(offerSubscription.Id))
            .Invokes((Guid id) =>
            {
                subscriptionProcessData.Remove(subscriptionProcessData.Single(x => x.OfferSubscriptionId == id));
            });
        var itAdminRoles = Enumerable.Repeat(new UserRoleConfig("Test", new[] { "AdminRoles" }), 1);
        var serviceManagerRoles = Enumerable.Repeat(new UserRoleConfig("Test", new[] { "ServiceManagerRoles" }), 1);

        // Act
        var result = await _sut.ActivateSubscription(offerSubscription.Id, itAdminRoles, serviceManagerRoles, "https://portal-backend.dev.demo.catena-x.net/").ConfigureAwait(false);

        // Assert
        var notificationTypeId = offerTypeId == OfferTypeId.APP
            ? NotificationTypeId.APP_SUBSCRIPTION_ACTIVATION
            : NotificationTypeId.SERVICE_ACTIVATION;
        offerSubscription.OfferSubscriptionStatusId.Should().Be(OfferSubscriptionStatusId.ACTIVE);
        if (isSingleInstance)
        {
            result.nextStepTypeIds.Should().BeNull();
        }
        else
        {
            result.nextStepTypeIds.Should().ContainSingle().And
                .AllSatisfy(x => x.Should().Be(ProcessStepTypeId.TRIGGER_PROVIDER_CALLBACK));
        }
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.modified.Should().BeTrue();
        result.processMessage.Should().BeNull();
        A.CallTo(() => _notificationService.CreateNotificationsWithExistenceCheck(A<IEnumerable<UserRoleConfig>>._, null, A<IEnumerable<(string?, NotificationTypeId)>>.That.Matches(x => x.Count() == 1 && x.Single().Item2 == notificationTypeId), _identity.CompanyId, A<string>._, offerSubscription.Id.ToString(), null))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => createNotificationsEnumerator.MoveNextAsync()).MustHaveHappened(2, Times.Exactly);
        A.CallTo(() => _notificationRepository.CreateNotification(A<Guid>._, notificationTypeId, false, A<Action<Notification>>._))
            .MustHaveHappenedOnceExactly();
        if (string.IsNullOrWhiteSpace(requesterEmail))
        {
            A.CallTo(() => _mailingService.SendMails(A<string>._, A<IDictionary<string, string>>._, A<IEnumerable<string>>._)).MustNotHaveHappened();
        }
        else
        {
            A.CallTo(() => _mailingService.SendMails(requesterEmail, A<IDictionary<string, string>>._, A<IEnumerable<string>>._)).MustHaveHappenedOnceExactly();
        }
    }

    #endregion

    #region Setup

    private IAsyncEnumerator<Guid> SetupServices(CompanyServiceAccount? companyServiceAccount = null)
    {
        A.CallTo(() => _provisioningManager.SetupClientAsync(A<string>._, A<string>._, A<IEnumerable<string>?>._, A<bool>._))
            .Returns("cl1");

        A.CallTo(() => _serviceAccountCreation.CreateServiceAccountAsync(A<ServiceAccountCreationInfo>._, A<Guid>._, A<IEnumerable<string>>.That.Matches(x => x.Any(y => y == "CAXSDUMMYCATENAZZ")), CompanyServiceAccountTypeId.MANAGED, A<bool>._, A<Action<CompanyServiceAccount>?>._))
            .Invokes((ServiceAccountCreationInfo _, Guid _, IEnumerable<string> _, CompanyServiceAccountTypeId _, bool _, Action<CompanyServiceAccount>? setOptionalParameter) =>
            {
                if (companyServiceAccount != null)
                {
                    setOptionalParameter?.Invoke(companyServiceAccount);
                }
            })
            .Returns((
                "sa2",
                new ServiceAccountData(Guid.NewGuid().ToString(), "cl1", new ClientAuthData(IamClientAuthMethod.SECRET)
                {
                    Secret = "katze!1234"
                }),
                _technicalUserId,
                new UserRoleData[]
                {
                    new(Guid.NewGuid(), "client1", "role1"),
                    new(Guid.NewGuid(), "client1", "role2"),
                }));

        A.CallTo(() => _notificationService.CreateNotifications(A<IEnumerable<UserRoleConfig>>._,
                A<Guid>._, A<IEnumerable<(string?, NotificationTypeId)>>._, A<Guid>._, A<bool?>._))
            .Returns(new[] { Guid.NewGuid() }.AsFakeIAsyncEnumerable(out var createNotificationsEnumerator));

        return createNotificationsEnumerator;
    }

    private IAsyncEnumerator<Guid> SetupAutoSetup(OfferTypeId offerTypeId, OfferSubscription? offerSubscription = null, bool isSingleInstance = false, CompanyServiceAccount? companyServiceAccount = null)
    {
        var createNotificationsEnumerator = SetupServices(companyServiceAccount);

        if (offerSubscription != null)
        {
            A.CallTo(() =>
                    _offerSubscriptionsRepository.AttachAndModifyOfferSubscription(A<Guid>._,
                        A<Action<OfferSubscription>>._))
                .Invokes((Guid _, Action<OfferSubscription> modify) => { modify.Invoke(offerSubscription); });
        }

        A.CallTo(() => _offerSubscriptionsRepository.GetOfferDetailsAndCheckProviderCompany(
                _validSubscriptionId,
                _identity.CompanyId,
                A<OfferTypeId>._))
            .Returns(new OfferSubscriptionTransferData(OfferSubscriptionStatusId.ACTIVE, true, "Company",
                _identity.CompanyId, _identity.UserId, _existingServiceId, offerTypeId, "Test Service",
                Bpn, "user@email.com", "Tony", "Gilbert", (isSingleInstance, "https://test.de"),
                new[] { Guid.NewGuid() },
                _salesManagerId));
        A.CallTo(() => _offerSubscriptionsRepository.GetOfferDetailsAndCheckProviderCompany(
                _pendingSubscriptionId,
                _companyUserWithoutMailCompanyId,
                A<OfferTypeId>._))
            .Returns(new OfferSubscriptionTransferData(OfferSubscriptionStatusId.PENDING, true,
                "Company", _identity.CompanyId, _identity.UserId, _existingServiceId, offerTypeId, "Test Service",
                Bpn, null, null, null, (isSingleInstance, "https://test.de"),
                new[] { Guid.NewGuid() },
                _salesManagerId));
        A.CallTo(() => _offerSubscriptionsRepository.GetOfferDetailsAndCheckProviderCompany(
                _pendingSubscriptionId,
                _identity.CompanyId,
                A<OfferTypeId>._))
            .Returns(new OfferSubscriptionTransferData(OfferSubscriptionStatusId.PENDING, true, "Company", _identity.CompanyId,
                _identity.UserId,
                _existingServiceId, offerTypeId, "Test Service",
                Bpn, "user@email.com", "Tony", "Gilbert", (isSingleInstance, "https://test.de"),
                new[] { Guid.NewGuid() },
                _salesManagerId));
        A.CallTo(() => _offerSubscriptionsRepository.GetOfferDetailsAndCheckProviderCompany(
                _offerIdWithMultipleInstances,
                _identity.CompanyId,
                A<OfferTypeId>._))
            .Returns(new OfferSubscriptionTransferData(OfferSubscriptionStatusId.PENDING, true, "Company",
                _identity.CompanyId, _identity.UserId, _existingServiceId, offerTypeId, "Test Service",
                Bpn, "user@email.com", "Tony", "Gilbert", (isSingleInstance, null),
                Enumerable.Empty<Guid>(),
                null));
        A.CallTo(() => _offerSubscriptionsRepository.GetOfferDetailsAndCheckProviderCompany(
                A<Guid>.That.Not.Matches(x => x == _pendingSubscriptionId || x == _validSubscriptionId || x == _offerIdWithMultipleInstances),
                _identity.CompanyId,
                A<OfferTypeId>._))
            .Returns((OfferSubscriptionTransferData?)null);
        A.CallTo(() => _offerSubscriptionsRepository.GetOfferDetailsAndCheckProviderCompany(
                _pendingSubscriptionId,
                A<Guid>.That.Not.Matches(x => x == _identity.CompanyId || x == _companyUserWithoutMailCompanyId),
                A<OfferTypeId>._))
            .Returns(new OfferSubscriptionTransferData(OfferSubscriptionStatusId.PENDING, false, string.Empty,
                Guid.NewGuid(), Guid.NewGuid(), _existingServiceId, OfferTypeId.APP, "Test Service",
                Bpn, null, null, null, (isSingleInstance, "https://test.de"),
                new[] { Guid.NewGuid() },
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
            .Returns(new SingleInstanceOfferData(CompanyUserCompanyId, "app1", Bpn, true, new[] { (_validInstanceSetupId, "cl1") }));
        A.CallTo(() => _offerRepository.GetSingleInstanceOfferData(_offerIdWithoutClient, OfferTypeId.APP))
            .Returns(new SingleInstanceOfferData(CompanyUserCompanyId, "app1", Bpn, true, new[] { (_validInstanceSetupId, string.Empty) }));
        A.CallTo(() => _offerRepository.GetSingleInstanceOfferData(_offerIdWithMultipleInstances, OfferTypeId.APP))
            .Returns(new SingleInstanceOfferData(CompanyUserCompanyId, "app1", Bpn, false, Enumerable.Empty<(Guid, string)>()));
        A.CallTo(() => _offerRepository.GetSingleInstanceOfferData(_offerIdWithWithNoInstanceSetupIds, OfferTypeId.APP))
            .Returns(new SingleInstanceOfferData(CompanyUserCompanyId, "app1", Bpn, true, Enumerable.Empty<(Guid, string)>()));
        A.CallTo(() => _offerRepository.GetSingleInstanceOfferData(A<Guid>.That.Not.Matches(x => x == _offerIdWithoutClient || x == _validOfferId || x == _offerIdWithMultipleInstances || x == _offerIdWithWithNoInstanceSetupIds), OfferTypeId.APP))
            .Returns((SingleInstanceOfferData?)null);
    }

    #endregion
}
