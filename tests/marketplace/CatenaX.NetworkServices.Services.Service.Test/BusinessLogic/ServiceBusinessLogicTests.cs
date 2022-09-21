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
using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.Notification.Library;
using CatenaX.NetworkServices.Offers.Library.Service;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.Provisioning.Library;
using CatenaX.NetworkServices.Provisioning.Library.Enums;
using CatenaX.NetworkServices.Provisioning.Library.Models;
using CatenaX.NetworkServices.Provisioning.Library.Service;
using CatenaX.NetworkServices.Services.Service.BusinessLogic;
using CatenaX.NetworkServices.Tests.Shared;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace CatenaX.NetworkServices.Services.Service.Test.BusinessLogic;

public class ServiceBusinessLogicTests
{
    private readonly Guid _companyUserCompanyId = new("395f955b-f11b-4a74-ab51-92a526c1973a");
    private readonly string _notAssignedCompanyIdUser = "395f955b-f11b-4a74-ab51-92a526c1973c";
    private readonly Guid _existingServiceId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47661");
    private readonly Guid _validSubscriptionId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47662");
    private readonly Guid _pendingSubscriptionId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47663");
    private readonly Guid _existingAgreementId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47664");
    private readonly Guid _validConsentId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47665");
    private readonly Guid _technicalUserId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47999");
    private readonly string _bpn = "CAXSDUMMYCATENAZZ";
    private readonly CompanyUser _companyUser;
    private readonly IFixture _fixture;
    private readonly IamUser _iamUser;
    private readonly IAgreementRepository _agreementRepository;
    private readonly IAppInstanceRepository _appInstanceRepository;
    private readonly IAppSubscriptionDetailRepository _appSubscriptionDetailRepository;
    private readonly IConsentRepository _consentRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IOfferRepository _offerRepository;
    private readonly IOfferSubscriptionsRepository _offerSubscriptionsRepository;
    private readonly ILanguageRepository _languageRepository;
    private readonly INotificationRepository _notificationReposiotry;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IUserRepository _userRepository;
    private readonly IUserRolesRepository _userRolesRepository;
    private readonly IProvisioningManager _provisioningManager;
    private readonly IServiceAccountCreation _serviceAccountCreation;
    private readonly INotificationService _notificationService;

    public ServiceBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        var (companyUser, iamUser) = CreateTestUserPair();
        _companyUser = companyUser;
        _iamUser = iamUser;

        _portalRepositories = A.Fake<IPortalRepositories>();
        _agreementRepository = A.Fake<IAgreementRepository>();
        _appInstanceRepository = A.Fake<IAppInstanceRepository>();
        _appSubscriptionDetailRepository = A.Fake<IAppSubscriptionDetailRepository>();
        _consentRepository = A.Fake<IConsentRepository>();
        _clientRepository = A.Fake<IClientRepository>();
        _offerRepository = A.Fake<IOfferRepository>();
        _offerSubscriptionsRepository = A.Fake<IOfferSubscriptionsRepository>();
        _languageRepository = A.Fake<ILanguageRepository>();
        _notificationReposiotry = A.Fake<INotificationRepository>();
        _userRepository = A.Fake<IUserRepository>();
        _userRolesRepository = A.Fake<IUserRolesRepository>();
        _provisioningManager = A.Fake<IProvisioningManager>();
        _serviceAccountCreation = A.Fake<IServiceAccountCreation>();
        _notificationService = A.Fake<INotificationService>();
        _notificationService = A.Fake<INotificationService>();

        _fixture.Inject(Options.Create(new ServiceSettings { ApplicationsMaxPageSize = 15 }));
        
        SetupRepositories(companyUser, iamUser);
        SetupServices();
    }

    #region Create Service

    [Fact]
    public async Task CreateServiceOffering_WithValidDataAndEmptyDescriptions_ReturnsCorrectDetails()
    {
        // Arrange
        var serviceId = Guid.NewGuid();

        var apps = new List<Offer>();
        A.CallTo(() => _offerRepository.CreateOffer(A<string>._, A<OfferTypeId>._, A<Action<Offer?>>._))
            .Invokes(x =>
            {
                var provider = x.Arguments.Get<string>("provider");
                var appTypeId = x.Arguments.Get<OfferTypeId>("offerType");
                var action = x.Arguments.Get<Action<Offer?>>("setOptionalParameters");

                var app = new Offer(serviceId, provider!, DateTimeOffset.UtcNow, appTypeId);
                action?.Invoke(app);
                apps.Add(app);
            })
            .Returns(new Offer(serviceId)
            {
                OfferTypeId = OfferTypeId.SERVICE 
            });
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        var result = await sut.CreateServiceOffering(new ServiceOfferingData("Newest Service", "42", "img/thumbnail.png", "mail@test.de", _companyUser.Id, new List<ServiceDescription>()), _iamUser.UserEntityId);

        // Assert
        result.Should().Be(serviceId);
        apps.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateServiceOffering_WithValidDataAndDescription_ReturnsCorrectDetails()
    {
        // Arrange
        var serviceId = Guid.NewGuid();

        var apps = new List<Offer>();
        A.CallTo(() => _offerRepository.CreateOffer(A<string>._, A<OfferTypeId>._, A<Action<Offer?>>._))
            .Invokes(x =>
            {
                var provider = x.Arguments.Get<string>("provider");
                var appTypeId = x.Arguments.Get<OfferTypeId>("offerType");
                var action = x.Arguments.Get<Action<Offer?>>("setOptionalParameters");

                var app = new Offer(serviceId, provider!, DateTimeOffset.UtcNow, appTypeId);
                action?.Invoke(app);
                apps.Add(app);
            })
            .Returns(new Offer(serviceId)
            {
                OfferTypeId = OfferTypeId.SERVICE 
            });
        
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        var serviceOfferingData = new ServiceOfferingData("Newest Service", "42", "img/thumbnail.png", "mail@test.de", _companyUser.Id, new List<ServiceDescription>
        {
            new ("en", "That's a description with a valid language code")
        });
        var result = await sut.CreateServiceOffering(serviceOfferingData, _iamUser.UserEntityId);

        // Assert
        result.Should().Be(serviceId);
        apps.Should().HaveCount(1);
    }
    
    [Fact]
    public async Task CreateServiceOffering_WithWrongIamUser_ThrowsException()
    {
        // Arrange
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        async Task Action() => await sut.CreateServiceOffering(new ServiceOfferingData("Newest Service", "42", "img/thumbnail.png", "mail@test.de", _companyUser.Id, new List<ServiceDescription>()), Guid.NewGuid().ToString());
        
        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("iamUserId");
    }

    [Fact]
    public async Task CreateServiceOffering_WithInvalidLanguage_ThrowsException()
    {
        // Arrange
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        var serviceOfferingData = new ServiceOfferingData("Newest Service", "42", "img/thumbnail.png", "mail@test.de", _companyUser.Id, new List<ServiceDescription>
        {
            new ("gg", "That's a description with incorrect language short code")
        });
        async Task Action() => await sut.CreateServiceOffering(serviceOfferingData, _iamUser.UserEntityId);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("languageCodes");
    }

    [Fact]
    public async Task CreateServiceOffering_WithoutCompanyUser_ThrowsException()
    {
        // Arrange
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        async Task Action() => await sut.CreateServiceOffering(new ServiceOfferingData("Newest Service", "42", "img/thumbnail.png", "mail@test.de", Guid.NewGuid(), new List<ServiceDescription>()), _iamUser.UserEntityId);
        
        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("SalesManager");
    }

    #endregion

    #region Get Active Services

    [Fact]
    public async Task GetAllActiveServicesAsync_WithDefaultRequest_GetsExpectedEntries()
    {
        // Arrange
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        var result = await sut.GetAllActiveServicesAsync(0, 5);

        // Assert
        result.Content.Should().HaveCount(5);
    }

    
    [Fact]
    public async Task GetAllActiveServicesAsync_WithSmallSize_GetsExpectedEntries()
    {
        // Arrange
        const int expectedCount = 3;
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        var result = await sut.GetAllActiveServicesAsync(0, expectedCount);

        // Assert
        result.Content.Should().HaveCount(expectedCount);
    }

    #endregion

    #region Add Service Subscription

    [Fact]
    public async Task AddServiceSubscription_WithExistingId_CreatesServiceSubscription()
    {
        // Arrange
        var companyAssignedAppId = Guid.NewGuid(); 
        var companyAssignedApps = new List<OfferSubscription>();
        A.CallTo(() => _offerSubscriptionsRepository.CreateOfferSubscription(A<Guid>._, A<Guid>._, A<OfferSubscriptionStatusId>._, A<Guid>._, A<Guid>._))
            .Invokes(x =>
            {
                var appId = x.Arguments.Get<Guid>("offerId");
                var companyId = x.Arguments.Get<Guid>("companyId");
                var appSubscriptionStatusId = x.Arguments.Get<OfferSubscriptionStatusId>("offerSubscriptionStatusId");
                var requesterId = x.Arguments.Get<Guid>("requesterId");
                var creatorId = x.Arguments.Get<Guid>("creatorId");

                var companyAssignedApp = new OfferSubscription(companyAssignedAppId, appId, companyId, appSubscriptionStatusId, requesterId, creatorId);
                companyAssignedApps.Add(companyAssignedApp);
            });
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        await sut.AddServiceSubscription(_existingServiceId, _iamUser.UserEntityId);

        // Assert
        companyAssignedApps.Should().HaveCount(1);
    }
    
    [Fact]
    public async Task AddServiceSubscription_WithNotExistingId_ThrowsException()
    {
        // Arrange
        var notExistingServiceId = Guid.NewGuid();
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        async Task Action() => await sut.AddServiceSubscription(notExistingServiceId, _iamUser.UserEntityId);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Action);
        ex.Message.Should().Be($"Service {notExistingServiceId} does not exist");
    }
    
    [Fact]
    public async Task AddServiceSubscription_NotAssignedCompany_ThrowsException()
    {
        // Arrange
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        async Task Action() => await sut.AddServiceSubscription(_existingServiceId, _notAssignedCompanyIdUser);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("iamUserId");
    }

    [Fact]
    public async Task AddServiceSubscription_NotAssignedCompanyUser_ThrowsException()
    {
        // Arrange
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        async Task Action() => await sut.AddServiceSubscription(_existingServiceId, Guid.NewGuid().ToString());

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("iamUserId");
    }

    #endregion

    #region Get Service Detail Data

    [Fact]
    public async Task GetServiceDetailsAsync_WithExistingServiceAndLanguageCode_ReturnsServiceDetailData()
    {
        // Arrange
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        var result = await sut.GetServiceDetailsAsync(_existingServiceId, "en", _iamUser.UserEntityId);

        // Assert
        result.Id.Should().Be(_existingServiceId);
    }

    [Fact]
    public async Task GetServiceDetailsAsync_WithoutExistingService_ThrowsException()
    {
        // Arrange
        var notExistingServiceId = Guid.NewGuid();
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        async Task Action() => await sut.GetServiceDetailsAsync(notExistingServiceId, "en", _iamUser.UserEntityId);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Action);
        ex.Message.Should().Be($"Service {notExistingServiceId} does not exist");
    }

    #endregion

    #region Get Service Agreement

    [Fact]
    public async Task GetServiceAgreement_WithUserId_ReturnsServiceDetailData()
    {
        // Arrange
        var offerService = A.Fake<IOfferService>();
        var data = _fixture.CreateMany<AgreementData>(1);
        A.CallTo(() => offerService.GetOfferAgreementsAsync(A<Guid>.That.Matches(x => x == _existingServiceId), A<OfferTypeId>._))
            .Returns(data.ToAsyncEnumerable());
        var sut = new ServiceBusinessLogic(A.Fake<IPortalRepositories>(), Options.Create(new ServiceSettings()), offerService, A.Fake<IProvisioningManager>(), A.Fake<IServiceAccountCreation>(), A.Fake<INotificationService>());

        // Act
        var result = await sut.GetServiceAgreement(_existingServiceId).ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().ContainSingle();
    }

    #endregion

    #region Get Subscription Details

    [Fact]
    public async Task GetSubscriptionDetails_WithValidId_ReturnsSubscriptionDetailData()
    {
        // Arrange
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        var result = await sut.GetSubscriptionDetailAsync(_validSubscriptionId, _iamUser.UserEntityId).ConfigureAwait(false);

        // Assert
        result.OfferId.Should().Be(_existingServiceId);
    }

    [Fact]
    public async Task GetSubscriptionDetails_WithInvalidId_ThrowsException()
    {
        // Arrange
        var notExistingId = Guid.NewGuid();
        _fixture.Inject(_portalRepositories);
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        async Task Action() => await sut.GetSubscriptionDetailAsync(notExistingId, _iamUser.UserEntityId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Action);
        ex.Message.Should().Be($"Subscription {notExistingId} does not exist");
    }

    #endregion

    #region Create Service Agreement Consent

    [Fact]
    public async Task CreateServiceAgreementConsent_ReturnsCorrectId()
    {
        // Arrange
        var consentId = Guid.NewGuid();
        var offerService = A.Fake<IOfferService>();
        A.CallTo(() => offerService.CreateOfferSubscriptionAgreementConsentAsync(A<Guid>._, A<Guid>._, A<ConsentStatusId>._, A<string>._, A<OfferTypeId>._))
            .ReturnsLazily(() => consentId);
        var sut = new ServiceBusinessLogic(A.Fake<IPortalRepositories>(), Options.Create(new ServiceSettings()), offerService, A.Fake<IProvisioningManager>(), A.Fake<IServiceAccountCreation>(), A.Fake<INotificationService>());

        // Act
        var serviceAgreementConsentData = new ServiceAgreementConsentData(_existingAgreementId, ConsentStatusId.ACTIVE);
        var result = await sut.CreateServiceAgreementConsentAsync(_existingServiceId, serviceAgreementConsentData, _iamUser.UserEntityId);

        // Assert
        result.Should().Be(consentId);
    }

    [Fact]
    public async Task CreateOrUpdateServiceAgreementConsentAsync_RunsSuccessfull()
    {
        // Arrange
        var offerService = A.Fake<IOfferService>();
        A.CallTo(() => offerService.CreateOrUpdateOfferSubscriptionAgreementConsentAsync(A<Guid>._, A<IEnumerable<ServiceAgreementConsentData>>._, A<string>._, A<OfferTypeId>._))
            .ReturnsLazily(() => Task.CompletedTask);
        var sut = new ServiceBusinessLogic(A.Fake<IPortalRepositories>(), Options.Create(new ServiceSettings()), offerService, A.Fake<IProvisioningManager>(), A.Fake<IServiceAccountCreation>(), A.Fake<INotificationService>());

        // Act
        await sut.CreateOrUpdateServiceAgreementConsentAsync(_existingServiceId, new List<ServiceAgreementConsentData>
        {
            new(_existingAgreementId, ConsentStatusId.ACTIVE)
        }, _iamUser.UserEntityId);

        // Assert
        true.Should().BeTrue();
    }

    #endregion

    #region Get Service Consent Detail Data

    [Fact]
    public async Task GetServiceConsentDetailData_WithValidId_ReturnsServiceConsentDetailData()
    {
        // Arrange
        var data = new ConsentDetailData(_validConsentId, "The Company", Guid.NewGuid(), ConsentStatusId.ACTIVE, "Agreed");
        var offerService = A.Fake<IOfferService>();
        A.CallTo(() => offerService.GetConsentDetailDataAsync(A<Guid>.That.Matches(x => x == _validConsentId), A<OfferTypeId>._))
            .ReturnsLazily(() => data);
        var sut = new ServiceBusinessLogic(A.Fake<IPortalRepositories>(), Options.Create(new ServiceSettings()), offerService, A.Fake<IProvisioningManager>(), A.Fake<IServiceAccountCreation>(), A.Fake<INotificationService>());

        // Act
        var result = await sut.GetServiceConsentDetailDataAsync(_validConsentId).ConfigureAwait(false);

        // Assert
        result.Id.Should().Be(_validConsentId);
        result.CompanyName.Should().Be("The Company");
    }

    [Fact]
    public async Task GetServiceConsentDetailData_WithInValidId_ReturnsServiceConsentDetailData()
    {
        // Arrange
        var offerService = A.Fake<IOfferService>();
        var invalidConsentId = Guid.NewGuid();
        A.CallTo(() => offerService.GetConsentDetailDataAsync(A<Guid>.That.Not.Matches(x => x == _validConsentId), A<OfferTypeId>._))
            .Throws(() => new NotFoundException("Test"));
        var sut = new ServiceBusinessLogic(A.Fake<IPortalRepositories>(), Options.Create(new ServiceSettings()), offerService, A.Fake<IProvisioningManager>(), A.Fake<IServiceAccountCreation>(), A.Fake<INotificationService>());

        // Act
        async Task Action() => await sut.GetServiceConsentDetailDataAsync(invalidConsentId).ConfigureAwait(false);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(Action);
    }

    #endregion
    
    #region Auto Setup

    [Fact]
    public async Task AutoSetup_WithValidData_ReturnsExpectedNotificationAndSecret()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var appInstanceId = Guid.NewGuid();
        var appSubscriptionDetailId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();
        var clients = new List<IamClient>();
        var appInstances = new List<AppInstance>();
        var appSubscriptionDetails = new List<AppSubscriptionDetail>();
        var notifications = new List<PortalBackend.PortalEntities.Entities.Notification>();
        A.CallTo(() => _clientRepository.CreateClient(A<string>._))
            .Invokes(x =>
            {
                var clientName = x.Arguments.Get<string>("clientId");

                var client = new IamClient(clientId, clientName!);
                clients.Add(client);
            })
            .Returns(new IamClient(clientId, "cl1"));

        A.CallTo(() => _appInstanceRepository.CreateAppInstance(A<Guid>._, A<Guid>._))
            .Invokes(x =>
            {
                var appId = x.Arguments.Get<Guid>("appId");
                var iamClientId = x.Arguments.Get<Guid>("iamClientId");

                var appInstance = new AppInstance(appInstanceId, appId, iamClientId);
                appInstances.Add(appInstance);
            })
            .Returns(new AppInstance(appInstanceId, _existingServiceId, clientId));
        A.CallTo(() => _appSubscriptionDetailRepository.CreateAppSubscriptionDetail(A<Guid>._, A<Action<AppSubscriptionDetail>?>._))
            .Invokes(x =>
            {
                var offerSubscriptionId = x.Arguments.Get<Guid>("offerSubscriptionId");
                var action = x.Arguments.Get<Action<AppSubscriptionDetail>?>("updateOptionalFields");

                var appDetail = new AppSubscriptionDetail(appSubscriptionDetailId, offerSubscriptionId);
                action?.Invoke(appDetail);
                appSubscriptionDetails.Add(appDetail);
            })
            .Returns(new AppSubscriptionDetail(appSubscriptionDetailId, _validSubscriptionId));
        A.CallTo(() => _notificationReposiotry.CreateNotification(A<Guid>._, A<NotificationTypeId>._, A<bool>._,
                A<Action<PortalBackend.PortalEntities.Entities.Notification>?>._))
            .Invokes(x =>
            {
                var receiverUserId = x.Arguments.Get<Guid>("receiverUserId");
                var notificationTypeId = x.Arguments.Get<NotificationTypeId>("notificationTypeId");
                var isRead = x.Arguments.Get<bool>("isRead");
                var action = x.Arguments.Get<Action<PortalBackend.PortalEntities.Entities.Notification>?>("setOptionalParameter");

                var notification = new PortalBackend.PortalEntities.Entities.Notification(notificationId, receiverUserId, DateTimeOffset.UtcNow, notificationTypeId, isRead);
                action?.Invoke(notification);
                notifications.Add(notification);
            });
        _fixture.Inject(_portalRepositories);
        _fixture.Inject(_provisioningManager);
        _fixture.Inject(_serviceAccountCreation);
        _fixture.Inject(_notificationService);
        
        var data = new ServiceAutoSetupData(_pendingSubscriptionId, "https://new-url.com/");
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        var result = await sut.AutoSetupService(data, _iamUser.UserEntityId).ConfigureAwait(false);
        
        // Assert
        result.TechnicalUserId.Should().Be(_technicalUserId);
        result.TechnicalUserSecret.Should().Be("katze!1234");
        clients.Should().HaveCount(1);
        appInstances.Should().HaveCount(1);
        appSubscriptionDetails.Should().HaveCount(1);
        notifications.Should().HaveCount(1);
    }

    [Fact]
    public async Task AutoSetup_WithNotExistingOfferSubscriptionId_ThrowsException()
    {
        // Arrange
        _fixture.Inject(_portalRepositories);
        var data = new ServiceAutoSetupData(Guid.NewGuid(), "https://new-url.com/");
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        async Task Action() => await sut.AutoSetupService(data, _iamUser.UserEntityId);
        
        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Action);
        ex.Message.Should().Be($"OfferSubscription {data.RequestId} does not exist");
    }

    [Fact]
    public async Task AutoSetup_WithActiveSubscription_ThrowsException()
    {
        // Arrange
        _fixture.Inject(_portalRepositories);
        var data = new ServiceAutoSetupData(_validSubscriptionId, "https://new-url.com/");
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        async Task Action() => await sut.AutoSetupService(data, _iamUser.UserEntityId);
        
        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("Status");
    }

    [Fact]
    public async Task AutoSetup_WithUserNotFromProvidingCompany_ThrowsException()
    {
        // Arrange
        _fixture.Inject(_portalRepositories);
        var data = new ServiceAutoSetupData(_pendingSubscriptionId, "https://new-url.com/");
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        async Task Action() => await sut.AutoSetupService(data, Guid.NewGuid().ToString());
        
        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("CompanyUserId");
    }

    #endregion

    #region Setup

    private void SetupRepositories(CompanyUser companyUser, IamUser iamUser)
    {
        var serviceDetailData = new AsyncEnumerableStub<ValueTuple<Guid, string?, string, string?, string?, string?>>(_fixture.CreateMany<ValueTuple<Guid, string?, string, string?, string?, string?>>(5));
        var serviceDetail = _fixture.Build<OfferDetailData>()
            .With(x => x.Id, _existingServiceId)
            .Create();
        
        A.CallTo(() => _userRepository.GetCompanyUserWithIamUserCheckAndCompanyShortName(iamUser.UserEntityId, companyUser.Id))
            .ReturnsLazily(() => new List<(Guid CompanyUserId, bool IsIamUser, string CompanyUserName, Guid CompanyId)>{new (_companyUser.Id, true, "COMPANYBPN", _companyUserCompanyId), new (_companyUser.Id, false, "OTHERCOMPANYBPN", _companyUserCompanyId)}.ToAsyncEnumerable());
        A.CallTo(() => _userRepository.GetCompanyUserWithIamUserCheckAndCompanyShortName(iamUser.UserEntityId, A<Guid>.That.Not.Matches(x => x == companyUser.Id)))
            .ReturnsLazily(() => new List<(Guid CompanyUserId, bool IsIamUser, string CompanyUserName, Guid CompanyId)>{new (_companyUser.Id, true, "COMPANYBPN", _companyUserCompanyId)}.ToAsyncEnumerable());
        A.CallTo(() => _userRepository.GetCompanyUserWithIamUserCheckAndCompanyShortName(A<string>.That.Not.Matches(x => x == iamUser.UserEntityId), companyUser.Id))
            .ReturnsLazily(() => new List<(Guid CompanyUserId, bool IsIamUser, string CompanyUserName, Guid CompanyId)>{new (_companyUser.Id, false, "OTHERCOMPANYBPN", _companyUserCompanyId)}.ToAsyncEnumerable());
        A.CallTo(() => _userRepository.GetCompanyUserWithIamUserCheckAndCompanyShortName(A<string>.That.Not.Matches(x => x == iamUser.UserEntityId), A<Guid>.That.Not.Matches(x => x == companyUser.Id)))
            .ReturnsLazily(() => new List<(Guid CompanyUserId, bool IsIamUser, string CompanyUserName, Guid CompanyId)>().ToAsyncEnumerable());

        A.CallTo(() => _userRepository.GetOwnCompanAndCompanyUseryId(iamUser.UserEntityId))
            .ReturnsLazily(() => (_companyUser.Id, _companyUser.CompanyId));
        A.CallTo(() => _userRepository.GetOwnCompanAndCompanyUseryId(_notAssignedCompanyIdUser))
            .ReturnsLazily(() => (_companyUser.Id, Guid.Empty));
        A.CallTo(() => _userRepository.GetOwnCompanAndCompanyUseryId(A<string>.That.Not.Matches(x => x == iamUser.UserEntityId || x == _notAssignedCompanyIdUser)))
            .ReturnsLazily(() => (Guid.Empty, _companyUser.CompanyId));
        
        A.CallTo(() => _offerRepository.GetActiveServices())
            .Returns(serviceDetailData.AsQueryable());
        
        A.CallTo(() => _offerRepository.GetOfferDetailByIdUntrackedAsync(_existingServiceId, A<string>.That.Matches(x => x == "en"), A<string>._, A<OfferTypeId>._))
            .ReturnsLazily(() => serviceDetail with {OfferSubscriptionDetailData = new []
            {
                new OfferSubscriptionStateDetailData(Guid.NewGuid(), OfferSubscriptionStatusId.ACTIVE)
            }});
        A.CallTo(() => _offerRepository.GetOfferDetailByIdUntrackedAsync(A<Guid>.That.Not.Matches(x => x == _existingServiceId), A<string>._, A<string>._, A<OfferTypeId>._))
            .ReturnsLazily(() => (OfferDetailData?)null);

        A.CallTo(() => _languageRepository.GetLanguageCodesUntrackedAsync(A<IEnumerable<string>>.That.Matches(x => x.Count() == 1 && x.All(y => y == "en"))))
            .Returns(new List<string> { "en" }.ToAsyncEnumerable());
        A.CallTo(() => _languageRepository.GetLanguageCodesUntrackedAsync(A<IEnumerable<string>>.That.Matches(x => x.Count() == 1 && x.All(y => y == "gg"))))
            .Returns(new List<string>().ToAsyncEnumerable());
        
        A.CallTo(() => _offerRepository.CheckServiceExistsById(_existingServiceId))
            .Returns(true);
        A.CallTo(() => _offerRepository.CheckServiceExistsById(A<Guid>.That.Not.Matches(x => x == _existingServiceId)))
            .Returns(false);
        
        var agreementData = _fixture.CreateMany<AgreementData>(1);
        A.CallTo(() => _agreementRepository.GetOfferAgreementDataForOfferId(A<Guid>.That.Matches(x => x == _existingServiceId), A<OfferTypeId>._))
            .Returns(agreementData.ToAsyncEnumerable());
        A.CallTo(() => _agreementRepository.GetOfferAgreementDataForOfferId(A<Guid>.That.Not.Matches(x => x == _existingServiceId), A<OfferTypeId>._))
            .Returns(new List<AgreementData>().ToAsyncEnumerable());
        A.CallTo(() => _agreementRepository.CheckAgreementExistsForSubscriptionAsync(A<Guid>.That.Matches(x => x == _existingAgreementId), A<Guid>._, A<OfferTypeId>.That.Matches(x => x == OfferTypeId.SERVICE)))
            .ReturnsLazily(() => true);
        A.CallTo(() => _agreementRepository.CheckAgreementExistsForSubscriptionAsync(A<Guid>.That.Not.Matches(x => x == _existingAgreementId), A<Guid>._, A<OfferTypeId>._))
            .ReturnsLazily(() => false);
        A.CallTo(() => _agreementRepository.CheckAgreementExistsForSubscriptionAsync(A<Guid>._, A<Guid>._, A<OfferTypeId>.That.Not.Matches(x => x == OfferTypeId.SERVICE)))
            .ReturnsLazily(() => false);

        var offerSubscription = _fixture.Create<OfferSubscription>();
        A.CallTo(() => _offerSubscriptionsRepository.GetSubscriptionDetailDataForOwnUserAsync(
                A<Guid>.That.Matches(x => x == _validSubscriptionId),
                A<string>.That.Matches(x => x == iamUser.UserEntityId)))
            .ReturnsLazily(() =>
                new SubscriptionDetailData(_existingServiceId, "Super Service", OfferSubscriptionStatusId.ACTIVE));
        A.CallTo(() => _offerSubscriptionsRepository.GetSubscriptionDetailDataForOwnUserAsync(
                A<Guid>.That.Not.Matches(x => x == _validSubscriptionId),
                A<string>.That.Matches(x => x == iamUser.UserEntityId)))
            .ReturnsLazily(() => (SubscriptionDetailData?)null);
        A.CallTo(() => _offerSubscriptionsRepository.GetCompanyIdWithAssignedOfferForCompanyUserAndSubscriptionAsync(
                A<Guid>.That.Matches(x => x == _existingServiceId), A<string>.That.Matches(x => x == iamUser.UserEntityId), A<OfferTypeId>._))
            .ReturnsLazily(() => new ValueTuple<Guid, OfferSubscription?, Guid>(_companyUser.CompanyId, offerSubscription, _companyUser.Id));
        A.CallTo(() => _offerSubscriptionsRepository.GetCompanyIdWithAssignedOfferForCompanyUserAndSubscriptionAsync(
                A<Guid>.That.Not.Matches(x => x == _existingServiceId), A<string>.That.Matches(x => x == iamUser.UserEntityId),
                A<OfferTypeId>._))
            .ReturnsLazily(() => new ValueTuple<Guid, OfferSubscription?, Guid>(_companyUser.CompanyId, (OfferSubscription?)null, _companyUser.Id));
        A.CallTo(() => _offerSubscriptionsRepository.GetCompanyIdWithAssignedOfferForCompanyUserAndSubscriptionAsync(
                A<Guid>.That.Matches(x => x == _existingServiceId), A<string>.That.Not.Matches(x => x == iamUser.UserEntityId),
                A<OfferTypeId>._))
            .ReturnsLazily(() => ((Guid companyId, OfferSubscription? offerSubscription, Guid companyUserId))default);

        A.CallTo(() => _consentRepository.GetConsentDetailData(A<Guid>.That.Matches(x => x == _validConsentId), A<OfferTypeId>.That.Matches(x => x == OfferTypeId.SERVICE)))
            .ReturnsLazily(() =>
                new ConsentDetailData(_validConsentId, "The Company", _companyUser.Id, ConsentStatusId.ACTIVE,
                    "Agreed"));
        A.CallTo(() => _consentRepository.GetConsentDetailData(A<Guid>._, A<OfferTypeId>.That.Not.Matches(x => x == OfferTypeId.SERVICE)))
            .ReturnsLazily(() => (ConsentDetailData?)null);
        A.CallTo(() => _consentRepository.GetConsentDetailData(A<Guid>.That.Not.Matches(x => x == _validConsentId), A<OfferTypeId>._))
            .ReturnsLazily(() => (ConsentDetailData?)null);

        A.CallTo(() => _offerSubscriptionsRepository.GetOfferDetailsAndCheckUser(
                A<Guid>.That.Matches(x => x == _validSubscriptionId),
                A<string>.That.Matches(x => x == _iamUser.UserEntityId)))
            .ReturnsLazily(() => new OfferSubscriptionDetailData(OfferSubscriptionStatusId.ACTIVE, companyUser.Id,
                companyUser.Company!.Name, companyUser.CompanyId, companyUser.Id, _existingServiceId, "Test Service",
                _bpn));
        A.CallTo(() => _offerSubscriptionsRepository.GetOfferDetailsAndCheckUser(
                A<Guid>.That.Matches(x => x == _pendingSubscriptionId),
                A<string>.That.Matches(x => x == _iamUser.UserEntityId)))
            .ReturnsLazily(() => new OfferSubscriptionDetailData(OfferSubscriptionStatusId.PENDING, companyUser.Id,
                string.Empty, companyUser.CompanyId, companyUser.Id, _existingServiceId, "Test Service",
                _bpn));
        A.CallTo(() => _offerSubscriptionsRepository.GetOfferDetailsAndCheckUser(
                A<Guid>.That.Not.Matches(x => x == _pendingSubscriptionId || x == _validSubscriptionId),
                A<string>.That.Matches(x => x == _iamUser.UserEntityId)))
            .ReturnsLazily(() => (OfferSubscriptionDetailData?)null);

        A.CallTo(() => _offerSubscriptionsRepository.GetOfferDetailsAndCheckUser(
                A<Guid>.That.Matches(x => x == _pendingSubscriptionId),
                A<string>.That.Not.Matches(x => x == _iamUser.UserEntityId)))
            .ReturnsLazily(() =>new OfferSubscriptionDetailData(OfferSubscriptionStatusId.PENDING, Guid.Empty,
                string.Empty, companyUser.CompanyId, companyUser.Id, _existingServiceId, "Test Service",
                _bpn));

        var userRoleData = _fixture.CreateMany<UserRoleData>(3);
        A.CallTo(
                () => _userRolesRepository.GetUserRoleDataUntrackedAsync(A<IDictionary<string, IEnumerable<string>>>._))
            .ReturnsLazily(() => userRoleData.ToAsyncEnumerable());

        A.CallTo(() => _userRolesRepository.GetUserRolesForOfferIdAsync(A<Guid>.That.Matches(x => x == _existingServiceId)))
            .ReturnsLazily(() => new List<string> { "Buyer", "Supplier" });
        A.CallTo(() => _portalRepositories.GetInstance<IAgreementRepository>()).Returns(_agreementRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IAppInstanceRepository>()).Returns(_appInstanceRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IAppSubscriptionDetailRepository>()).Returns(_appSubscriptionDetailRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IConsentRepository>()).Returns(_consentRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IClientRepository>()).Returns(_clientRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IOfferRepository>()).Returns(_offerRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IOfferSubscriptionsRepository>()).Returns(_offerSubscriptionsRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ILanguageRepository>()).Returns(_languageRepository);
        A.CallTo(() => _portalRepositories.GetInstance<INotificationRepository>()).Returns(_notificationReposiotry);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRolesRepository>()).Returns(_userRolesRepository);
    }

    private void SetupServices()
    {
        A.CallTo(() => _provisioningManager.SetupClientAsync(A<string>._, A<IEnumerable<string>?>._))
            .ReturnsLazily(() => "cl1");
        
        A.CallTo(() => _serviceAccountCreation.CreateServiceAccountAsync(A<string>._, A<string>._,
                A<IamClientAuthMethod>._, A<IEnumerable<Guid>>._, A<Guid>._, A<IEnumerable<string>>.That.Matches(x => x.Any(y => y == "CAXSDUMMYCATENAZZ"))))
            .ReturnsLazily(() => 
                new ValueTuple<string, ServiceAccountData, Guid, List<UserRoleData>>(
                    "sa2", 
                    new ServiceAccountData(Guid.NewGuid().ToString(), "cl1", new ClientAuthData(IamClientAuthMethod.SECRET)
                    {
                        Secret = "katze!1234"
                    }),
                    _technicalUserId, 
                    new List<UserRoleData>()));

        A.CallTo(() => _notificationService.CreateNotifications(A<IDictionary<string, IEnumerable<string>>>._,
                A<Guid>._, A<IEnumerable<(string?, NotificationTypeId)>>._))
            .ReturnsLazily(() => Task.CompletedTask);
    }

    private (CompanyUser, IamUser) CreateTestUserPair()
    {
        var companyUser = _fixture.Build<CompanyUser>()
            .Without(u => u.IamUser)
            .With(u => u.CompanyId, _companyUserCompanyId)
            .Create();
        var iamUser = _fixture.Build<IamUser>()
            .With(u => u.CompanyUser, companyUser)
            .Create();
        companyUser.IamUser = iamUser;
        companyUser.Company = new Company(Guid.NewGuid(), "The Company", CompanyStatusId.ACTIVE, DateTimeOffset.UtcNow);
        return (companyUser, iamUser);
    }

    #endregion
}
