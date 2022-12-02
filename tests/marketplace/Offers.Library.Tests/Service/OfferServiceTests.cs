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
using Org.CatenaX.Ng.Portal.Backend.Framework.ErrorHandling;
using Org.CatenaX.Ng.Portal.Backend.Notifications.Library;
using Org.CatenaX.Ng.Portal.Backend.Offers.Library.Models;
using Org.CatenaX.Ng.Portal.Backend.Offers.Library.Service;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.CatenaX.Ng.Portal.Backend.Provisioning.Library;
using Org.CatenaX.Ng.Portal.Backend.Provisioning.Library.Enums;
using Org.CatenaX.Ng.Portal.Backend.Provisioning.Library.Models;
using Org.CatenaX.Ng.Portal.Backend.Provisioning.Library.Service;
using Org.CatenaX.Ng.Portal.Backend.Mailing.SendMail;
using Xunit;

namespace Org.CatenaX.Ng.Portal.Backend.Offers.Library.Tests.Service;

public class OfferServiceTests
{
    private const string Bpn = "CAXSDUMMYCATENAZZ";

    private readonly Guid _companyUserCompanyId = new("395f955b-f11b-4a74-ab51-92a526c1973a");
    private readonly Guid _existingServiceId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47661");
    private readonly Guid _validSubscriptionId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47662");
    private readonly Guid _pendingSubscriptionId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47663");
    private readonly Guid _existingAgreementId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47664");
    private readonly Guid _validConsentId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47664");
    private readonly Guid _existingAgreementForSubscriptionId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47665");
    private readonly Guid _technicalUserId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47999");
    private readonly Guid _differentCompanyUserId = Guid.NewGuid();
    private readonly Guid _noSalesManagerUserId = Guid.NewGuid();

    private readonly CompanyUser _companyUser;
    private readonly IFixture _fixture;
    private readonly string _iamUserId;
    private readonly string _iamUserIdWithoutMail;
    private readonly IAppInstanceRepository _appInstanceRepository;
    private readonly IAgreementRepository _agreementRepository;
    private readonly IConsentRepository _consentRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IAppSubscriptionDetailRepository _appSubscriptionDetailRepository;
    private readonly IOfferSubscriptionsRepository _offerSubscriptionsRepository;
    private readonly IConsentAssignedOfferSubscriptionRepository _consentAssignedOfferSubscriptionRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IProvisioningManager _provisioningManager;
    private readonly IServiceAccountCreation _serviceAccountCreation;
    private readonly INotificationService _notificationService;
    private readonly IOfferRepository _offerRepository;
    private readonly IUserRepository _userRepository;    
    private readonly IUserRolesRepository _userRolesRepository;
    private readonly ILanguageRepository _languageRepository;

    public OfferServiceTests()
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
        _agreementRepository = A.Fake<IAgreementRepository>();
        _appSubscriptionDetailRepository = A.Fake<IAppSubscriptionDetailRepository>();
        _appInstanceRepository = A.Fake<IAppInstanceRepository>();
        _clientRepository = A.Fake<IClientRepository>();
        _consentRepository = A.Fake<IConsentRepository>();
        _offerSubscriptionsRepository = A.Fake<IOfferSubscriptionsRepository>();
        _consentAssignedOfferSubscriptionRepository = A.Fake<IConsentAssignedOfferSubscriptionRepository>();
        _offerRepository = A.Fake<IOfferRepository>();
        _userRepository = A.Fake<IUserRepository>();
        _userRolesRepository = A.Fake<IUserRolesRepository>();
        _languageRepository = A.Fake<ILanguageRepository>();
        _provisioningManager = A.Fake<IProvisioningManager>();
        _notificationRepository = A.Fake<INotificationRepository>();
        _serviceAccountCreation = A.Fake<IServiceAccountCreation>();
        _notificationService = A.Fake<INotificationService>();

        SetupRepositories();
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
            .Invokes((string provider, OfferTypeId offerType, Action<Offer>? setOptionalParameters) =>
            {
                var app = new Offer(serviceId, provider, DateTimeOffset.UtcNow, offerType);
                setOptionalParameters?.Invoke(app);
                apps.Add(app);
            })
            .Returns(new Offer(serviceId, null!, default, default)
            {
                OfferTypeId = OfferTypeId.SERVICE 
            });
        var sut = new OfferService(_portalRepositories, _provisioningManager, _serviceAccountCreation, _notificationService, A.Fake<IMailingService>());

        // Act
        var result = await sut.CreateServiceOfferingAsync(new ServiceOfferingData("Newest Service", "42", "img/thumbnail.png", "mail@test.de", _companyUser.Id, new List<OfferingDescription>(), new List<ServiceTypeId>()), _iamUserId, OfferTypeId.SERVICE);

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
            .Invokes((string provider, OfferTypeId offerType, Action<Offer>? setOptionalParameters) =>
            {
                var app = new Offer(serviceId, provider, DateTimeOffset.UtcNow, offerType);
                setOptionalParameters?.Invoke(app);
                apps.Add(app);
            })
            .Returns(new Offer(serviceId, null!, default, default)
            {
                OfferTypeId = OfferTypeId.SERVICE 
            });
        var sut = new OfferService(_portalRepositories, _provisioningManager, _serviceAccountCreation, _notificationService, A.Fake<IMailingService>());

        // Act
        var serviceOfferingData = new ServiceOfferingData("Newest Service", "42", "img/thumbnail.png", "mail@test.de", _companyUser.Id, new List<OfferingDescription>
        {
            new ("en", "That's a description with a valid language code")
        },
        new[]
        {
            ServiceTypeId.DATASPACE_SERVICE
        });
        var result = await sut.CreateServiceOfferingAsync(serviceOfferingData, _iamUserId, OfferTypeId.SERVICE);

        // Assert
        result.Should().Be(serviceId);
        apps.Should().HaveCount(1);
        A.CallTo(() => _offerRepository.AddServiceAssignedServiceTypes(A<IEnumerable<(Guid serviceId, ServiceTypeId serviceTypeId)>>.That.Matches(s => s.Any(x => x.serviceId == serviceId)))).MustHaveHappenedOnceExactly();
    }
    
    [Fact]
    public async Task CreateServiceOffering_WithWrongIamUser_ThrowsException()
    {
        // Arrange
        var sut = new OfferService(_portalRepositories, _provisioningManager, _serviceAccountCreation, _notificationService, A.Fake<IMailingService>());

        // Act
        async Task Action() => await sut.CreateServiceOfferingAsync(new ServiceOfferingData("Newest Service", "42", "img/thumbnail.png", "mail@test.de", _companyUser.Id, new List<OfferingDescription>(), new List<ServiceTypeId>()), Guid.NewGuid().ToString(), OfferTypeId.SERVICE);
        
        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("iamUserId");
    }

    [Fact]
    public async Task CreateServiceOffering_WithInvalidLanguage_ThrowsException()
    {
        // Arrange
        var sut = new OfferService(_portalRepositories, _provisioningManager, _serviceAccountCreation, _notificationService, A.Fake<IMailingService>());

        // Act
        var serviceOfferingData = new ServiceOfferingData("Newest Service", "42", "img/thumbnail.png", "mail@test.de", _companyUser.Id, new List<OfferingDescription>
        {
            new ("gg", "That's a description with incorrect language short code")
        }, new List<ServiceTypeId>());
        async Task Action() => await sut.CreateServiceOfferingAsync(serviceOfferingData, _iamUserId, OfferTypeId.SERVICE);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("languageCodes");
    }

    [Fact]
    public async Task CreateServiceOffering_WithoutCompanyUser_ThrowsException()
    {
        // Arrange
        var sut = new OfferService(_portalRepositories, _provisioningManager, _serviceAccountCreation, _notificationService, A.Fake<IMailingService>());

        // Act
        async Task Action() => await sut.CreateServiceOfferingAsync(new ServiceOfferingData("Newest Service", "42", "img/thumbnail.png", "mail@test.de", Guid.NewGuid(), new List<OfferingDescription>(), new List<ServiceTypeId>()), _iamUserId, OfferTypeId.SERVICE);
        
        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("SalesManager");
    }

    #endregion

    #region Get Service Agreement

    [Fact]
    public async Task GetOfferAgreement_WithUserId_ReturnsServiceDetailData()
    {
        // Arrange
        var sut = new OfferService(_portalRepositories, _provisioningManager, _serviceAccountCreation, _notificationService, A.Fake<IMailingService>());

        // Act
        var result = await sut.GetOfferAgreementsAsync(_existingServiceId, OfferTypeId.SERVICE).ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().ContainSingle();
    }

    [Fact]
    public async Task GetOfferAgreement_WithoutExistingService_ThrowsException()
    {
        // Arrange
        var sut = new OfferService(_portalRepositories, _provisioningManager, _serviceAccountCreation, _notificationService, A.Fake<IMailingService>());

        // Act
        var agreementData = await sut.GetOfferAgreementsAsync(Guid.NewGuid(), OfferTypeId.SERVICE).ToListAsync().ConfigureAwait(false);

        // Assert
        agreementData.Should().BeEmpty();
    }

    #endregion

    #region Create Offer Agreement Consent

    [Fact]
    public async Task CreateOfferAgreementConsentAsync_WithValidDataAndEmptyDescriptions_ReturnsCorrectDetails()
    {
        // Arrange
        var consentId = Guid.NewGuid();
        var statusId = ConsentStatusId.ACTIVE;

        var consents = new List<Consent>();
        A.CallTo(() => _consentRepository.CreateConsent(A<Guid>._, A<Guid>._, A<Guid>._, A<ConsentStatusId>._, A<Action<Consent>?>._))
            .Invokes((Guid agreementId, Guid companyId, Guid companyUserId, ConsentStatusId consentStatusId, Action<Consent>? setupOptionalFields) =>
            {
                var consent = new Consent(consentId, agreementId, companyId, companyUserId, consentStatusId, DateTimeOffset.UtcNow);
                setupOptionalFields?.Invoke(consent);
                consents.Add(consent);
            })
            .Returns(new Consent(consentId)
            {
                ConsentStatusId = statusId
            });
        var sut = new OfferService(_portalRepositories, _provisioningManager, _serviceAccountCreation, _notificationService, A.Fake<IMailingService>());

        // Act
        var result = await sut.CreateOfferSubscriptionAgreementConsentAsync(_existingServiceId, _existingAgreementId, statusId, _iamUserId, OfferTypeId.SERVICE);

        // Assert
        result.Should().Be(consentId);
        consents.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateOfferAgreementConsentAsync_WithNotExistingAgreement_ThrowsException()
    {
        // Arrange
        var nonExistingAgreementId = Guid.NewGuid();
        var sut = new OfferService(_portalRepositories, _provisioningManager, _serviceAccountCreation, _notificationService, A.Fake<IMailingService>());

        // Act
        async Task Action() => await sut.CreateOfferSubscriptionAgreementConsentAsync(_existingServiceId, nonExistingAgreementId, ConsentStatusId.ACTIVE, _iamUserId, OfferTypeId.SERVICE);
        
        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.Message.Should().Be($"Invalid Agreement {nonExistingAgreementId} for subscription {_existingServiceId} (Parameter 'agreementId')");
    }

    [Fact]
    public async Task CreateOfferAgreementConsentAsync_WithWrongUser_ThrowsException()
    {
        // Arrange
        var sut = new OfferService(_portalRepositories, _provisioningManager, _serviceAccountCreation, _notificationService, A.Fake<IMailingService>());

        // Act
        async Task Action() => await sut.CreateOfferSubscriptionAgreementConsentAsync(_existingServiceId, _existingAgreementId, ConsentStatusId.ACTIVE, Guid.NewGuid().ToString(), OfferTypeId.SERVICE);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("iamUserId");
    }

    [Fact]
    public async Task CreateOfferAgreementConsentAsync_WithNotExistingService_ThrowsException()
    {
        // Arrange
        var notExistingServiceId = Guid.NewGuid();
        var sut = new OfferService(_portalRepositories, _provisioningManager, _serviceAccountCreation, _notificationService, A.Fake<IMailingService>());

        // Act
        async Task Action() => await sut.CreateOfferSubscriptionAgreementConsentAsync(notExistingServiceId, _existingAgreementId, ConsentStatusId.ACTIVE,
            _iamUserId, OfferTypeId.SERVICE);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Action);
        ex.Message.Should().Be($"Invalid OfferSubscription {notExistingServiceId} for OfferType SERVICE");
    }

    [Fact]
    public async Task CreateOfferAgreementConsentAsync_WithInvalidOfferType_ThrowsException()
    {
        // Arrange
        var sut = new OfferService(_portalRepositories, _provisioningManager, _serviceAccountCreation, _notificationService, A.Fake<IMailingService>());

        // Act
        async Task Action() => await sut.CreateOfferSubscriptionAgreementConsentAsync(_existingServiceId, _existingAgreementId, ConsentStatusId.ACTIVE,
            _iamUserId, OfferTypeId.APP);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Action);
        ex.Message.Should().Be($"Invalid OfferSubscription {_existingServiceId} for OfferType APP");
    }

    #endregion

    #region Create Offer Agreement Consent

    [Fact]
    public async Task CreateOrUpdateServiceAgreementConsentAsync_WithValidDataAndEmptyDescriptions_ReturnsCorrectDetails()
    {
        // Arrange
        var consentId = Guid.NewGuid();
        var statusId = ConsentStatusId.ACTIVE;
        var data = new List<OfferAgreementConsentData>
        {
            new(_existingAgreementId, statusId)
        };

        var consents = new List<Consent>();
        A.CallTo(() => _consentRepository.CreateConsent(A<Guid>._, A<Guid>._, A<Guid>._, A<ConsentStatusId>._, A<Action<Consent>?>._))
            .Invokes((Guid agreementId, Guid companyId, Guid companyUserId, ConsentStatusId consentStatusId, Action<Consent>? setupOptionalFields) =>
            {
                var consent = new Consent(consentId, agreementId, companyId, companyUserId, consentStatusId, DateTimeOffset.UtcNow);
                setupOptionalFields?.Invoke(consent);
                consents.Add(consent);
            })
            .Returns(new Consent(consentId)
            {
                ConsentStatusId = statusId
            });
        var sut = new OfferService(_portalRepositories, _provisioningManager, _serviceAccountCreation, _notificationService, A.Fake<IMailingService>());

        // Act
        await sut.CreateOrUpdateOfferSubscriptionAgreementConsentAsync(_existingServiceId, data, _iamUserId, OfferTypeId.SERVICE);

        // Assert
        consents.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateOrUpdateServiceAgreementConsentAsync_WithNotExistingAgreement_ThrowsException()
    {
        // Arrange
        var nonExistingAgreementId = Guid.NewGuid();
        var data = new List<OfferAgreementConsentData>
        {
            new(nonExistingAgreementId, ConsentStatusId.ACTIVE)
        };
        var sut = new OfferService(_portalRepositories, _provisioningManager, _serviceAccountCreation, _notificationService, A.Fake<IMailingService>());

        // Act
        async Task Action() => await sut.CreateOrUpdateOfferSubscriptionAgreementConsentAsync(_existingServiceId, data, _iamUserId, OfferTypeId.SERVICE);
        
        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.Message.Should().Be($"Invalid Agreements for subscription {_existingServiceId} (Parameter 'offerAgreementConsentData')");
    }

    [Fact]
    public async Task CreateOrUpdateServiceAgreementConsentAsync_WithWrongUser_ThrowsException()
    {
        // Arrange
        var data = new List<OfferAgreementConsentData>
        {
            new(_existingAgreementId, ConsentStatusId.ACTIVE)
        };
        var sut = new OfferService(_portalRepositories, _provisioningManager, _serviceAccountCreation, _notificationService, A.Fake<IMailingService>());

        // Act
        async Task Action() => await sut.CreateOrUpdateOfferSubscriptionAgreementConsentAsync(_existingServiceId, data, Guid.NewGuid().ToString(), OfferTypeId.SERVICE);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("iamUserId");
    }

    [Fact]
    public async Task CreateOrUpdateServiceAgreementConsentAsync_WithNotExistingService_ThrowsException()
    {
        // Arrange
        var notExistingServiceId = Guid.NewGuid();
        var data = new List<OfferAgreementConsentData>
        {
            new(_existingAgreementId, ConsentStatusId.ACTIVE)
        };
        var sut = new OfferService(_portalRepositories, _provisioningManager, _serviceAccountCreation, _notificationService, A.Fake<IMailingService>());

        // Act
        async Task Action() => await sut.CreateOrUpdateOfferSubscriptionAgreementConsentAsync(notExistingServiceId, data,
            _iamUserId, OfferTypeId.SERVICE);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Action);
        ex.Message.Should().Be($"Invalid OfferSubscription {notExistingServiceId} for OfferType SERVICE");
    }

    [Fact]
    public async Task CreateOrUpdateServiceAgreementConsentAsync_WithInvalidOfferType_ThrowsException()
    {
        // Arrange
        var data = new List<OfferAgreementConsentData>
        {
            new(_existingAgreementId, ConsentStatusId.ACTIVE)
        };
        var sut = new OfferService(_portalRepositories, _provisioningManager, _serviceAccountCreation, _notificationService, A.Fake<IMailingService>());

        // Act
        async Task Action() => await sut.CreateOrUpdateOfferSubscriptionAgreementConsentAsync(_existingServiceId, data,
            _iamUserId, OfferTypeId.APP);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Action);
        ex.Message.Should().Be($"Invalid OfferSubscription {_existingServiceId} for OfferType APP");
    }

    #endregion

    #region Get Consent Detail Data

    [Fact]
    public async Task GetServiceConsentDetailData_WithValidId_ReturnsServiceConsentDetailData()
    {
        // Arrange
        var sut = new OfferService(_portalRepositories, _provisioningManager, _serviceAccountCreation, _notificationService, A.Fake<IMailingService>());

        // Act
        var result = await sut.GetConsentDetailDataAsync(_validConsentId, OfferTypeId.SERVICE).ConfigureAwait(false);

        // Assert
        result.Id.Should().Be(_validConsentId);
        result.CompanyName.Should().Be("The Company");
    }

    [Fact]
    public async Task GetServiceConsentDetailData_WithInvalidId_ThrowsException()
    {
        // Arrange
        var notExistingId = Guid.NewGuid();
        var sut = new OfferService(_portalRepositories, _provisioningManager, _serviceAccountCreation, _notificationService, A.Fake<IMailingService>());

        // Act
        async Task Action() => await sut.GetConsentDetailDataAsync(notExistingId, OfferTypeId.SERVICE).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Action);
        ex.Message.Should().Be($"Consent {notExistingId} does not exist");
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
        var notifications = new List<Notification>();
        A.CallTo(() => _clientRepository.CreateClient(A<string>._))
            .Invokes(x =>
            {
                var clientName = x.Arguments.Get<string>("clientId");

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
        _fixture.Inject(_provisioningManager);
        _fixture.Inject(_serviceAccountCreation);
        _fixture.Inject(_notificationService);
        var serviceAccountRoles = new Dictionary<string, IEnumerable<string>>
        {
            { "technical_roles_management", new [] { "Digital Twin Management" } }
        };
        var companyAdminRoles = new Dictionary<string, IEnumerable<string>>
        {
            { "Cl2-CX-Portal", new [] { "IT Admin" } }
        };

        var data = new OfferAutoSetupData(_pendingSubscriptionId, "https://new-url.com/");
        var mailingService = A.Fake<IMailingService>();
        var sut = new OfferService(_portalRepositories, _provisioningManager, _serviceAccountCreation, _notificationService, mailingService);

        // Act
        var result = await sut.AutoSetupServiceAsync(data, serviceAccountRoles, companyAdminRoles, _iamUserId, OfferTypeId.SERVICE, "https://base-address.com").ConfigureAwait(false);
        
        // Assert
        result.TechnicalUserInfo.TechnicalUserId.Should().Be(_technicalUserId);
        result.TechnicalUserInfo.TechnicalUserSecret.Should().Be("katze!1234");
        clients.Should().HaveCount(1);
        appInstances.Should().HaveCount(1);
        appSubscriptionDetails.Should().HaveCount(1);
        notifications.Should().HaveCount(1);
        A.CallTo(() => mailingService.SendMails(A<string>._, A<Dictionary<string, string>>._, A<List<string>>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task AutoSetup_WithValidDataAndUserWithoutMail_NoMailIsSend()
    {
        // Arrange
        _fixture.Inject(_provisioningManager);
        _fixture.Inject(_serviceAccountCreation);
        _fixture.Inject(_notificationService);
        var serviceAccountRoles = new Dictionary<string, IEnumerable<string>>
        {
            { "technical_roles_management", new [] { "Digital Twin Management" } }
        };
        var companyAdminRoles = new Dictionary<string, IEnumerable<string>>
        {
            { "Cl2-CX-Portal", new [] { "IT Admin" } }
        };

        var data = new OfferAutoSetupData(_pendingSubscriptionId, "https://new-url.com/");
        var mailingService = A.Fake<IMailingService>();
        var sut = new OfferService(_portalRepositories, _provisioningManager, _serviceAccountCreation, _notificationService, mailingService);

        // Act
        var result = await sut.AutoSetupServiceAsync(data, serviceAccountRoles, companyAdminRoles, _iamUserIdWithoutMail, OfferTypeId.SERVICE, "https://base-address.com").ConfigureAwait(false);
        
        // Assert
        result.TechnicalUserInfo.TechnicalUserId.Should().Be(_technicalUserId);
        result.TechnicalUserInfo.TechnicalUserSecret.Should().Be("katze!1234");
        A.CallTo(() => mailingService.SendMails(A<string>._, A<Dictionary<string, string>>._, A<List<string>>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task AutoSetup_WithNotExistingOfferSubscriptionId_ThrowsException()
    {
        // Arrange
        var data = new OfferAutoSetupData(Guid.NewGuid(), "https://new-url.com/");
        var sut = new OfferService(_portalRepositories, _provisioningManager, _serviceAccountCreation, _notificationService, A.Fake<IMailingService>());

        // Act
        async Task Action() => await sut.AutoSetupServiceAsync(data, new Dictionary<string, IEnumerable<string>>(), new Dictionary<string, IEnumerable<string>>(), _iamUserId, OfferTypeId.SERVICE, "https://base-address.com");
        
        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Action);
        ex.Message.Should().Be($"OfferSubscription {data.RequestId} does not exist");
    }

    [Fact]
    public async Task AutoSetup_WithActiveSubscription_ThrowsException()
    {
        // Arrange
        var data = new OfferAutoSetupData(_validSubscriptionId, "https://new-url.com/");
        var sut = new OfferService(_portalRepositories, _provisioningManager, _serviceAccountCreation, _notificationService, A.Fake<IMailingService>());

        // Act
        async Task Action() => await sut.AutoSetupServiceAsync(data, new Dictionary<string, IEnumerable<string>>(), new Dictionary<string, IEnumerable<string>>(), _iamUserId, OfferTypeId.SERVICE, "https://base-address.com");
        
        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("Status");
    }

    [Fact]
    public async Task AutoSetup_WithUserNotFromProvidingCompany_ThrowsException()
    {
        // Arrange
        var data = new OfferAutoSetupData(_pendingSubscriptionId, "https://new-url.com/");
        var sut = new OfferService(_portalRepositories, _provisioningManager, _serviceAccountCreation, _notificationService, A.Fake<IMailingService>());

        // Act
        async Task Action() => await sut.AutoSetupServiceAsync(data, new Dictionary<string, IEnumerable<string>>(), new Dictionary<string, IEnumerable<string>>(), Guid.NewGuid().ToString(), OfferTypeId.SERVICE, "https://base-address.com");
        
        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("CompanyUserId");
    }

    #endregion

    #region Validate SalesManager
    
    [Fact]
    public async Task AddAppAsync_WithInvalidSalesManager_ThrowsException()
    {
        // Arrange
        SetupValidateSalesManager();
        var sut = new OfferService(_portalRepositories, null!, null!, null!, null!);

        //null Act
        async Task Act() => await sut.ValidateSalesManager(Guid.NewGuid(), _iamUserId, new Dictionary<string, IEnumerable<string>>()).ConfigureAwait(false);

        // Assert
        var error = await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);
        error.ParamName.Should().Be("salesManagerId");
    }

    [Fact]
    public async Task AddAppAsync_WithUserFromOtherCompany_ThrowsException()
    {
        // Arrange
        SetupValidateSalesManager();
        var sut = new OfferService(_portalRepositories, null!, null!, null!, null!);

        // Act
        async Task Act() => await sut.ValidateSalesManager(_differentCompanyUserId, _iamUserId, new Dictionary<string, IEnumerable<string>>()).ConfigureAwait(false);

        // Assert
        var error = await Assert.ThrowsAsync<ForbiddenException>(Act).ConfigureAwait(false);
        error.Message.Should().Contain("is not a member of the company");
    }

    [Fact]
    public async Task AddAppAsync_WithUserWithoutSalesManagerRole_ThrowsException()
    {
        // Arrange
        SetupValidateSalesManager();
        var sut = new OfferService(_portalRepositories, null!, null!, null!, null!);
     
        // Act
        async Task Act() => await sut.ValidateSalesManager(_noSalesManagerUserId, _iamUserId, new Dictionary<string, IEnumerable<string>>()).ConfigureAwait(false);

        // Assert
        var error = await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);
        error.ParamName.Should().Be("salesManagerId");
    }
    
    #endregion
    
    #region UpsertRemoveOfferDescription

    [Fact]
    public void UpsertRemoveOfferDescription_ReturnsExpected()
    {
        var seedOfferId = _fixture.Create<Guid>();
        var seed = new Dictionary<(Guid,string),OfferDescription>() {
            {(seedOfferId, "de"), new OfferDescription(seedOfferId, "de", _fixture.Create<string>(), _fixture.Create<string>())},
            {(seedOfferId, "en"), new OfferDescription(seedOfferId, "en", _fixture.Create<string>(), _fixture.Create<string>())},
            {(seedOfferId, "fr"), new OfferDescription(seedOfferId, "fr", _fixture.Create<string>(), _fixture.Create<string>())},
            {(seedOfferId, "cz"), new OfferDescription(seedOfferId, "cz", _fixture.Create<string>(), _fixture.Create<string>())},
            {(seedOfferId, "it"), new OfferDescription(seedOfferId, "it", _fixture.Create<string>(), _fixture.Create<string>())},
        };

        var updateDescriptions = new [] {
            new Localization("de", _fixture.Create<string>(), _fixture.Create<string>()),
            new Localization("fr", _fixture.Create<string>(), _fixture.Create<string>()),
            new Localization("sk", _fixture.Create<string>(), _fixture.Create<string>()),
            new Localization("se", _fixture.Create<string>(), _fixture.Create<string>()),
        };

        var existingDescriptions = seed.Select((x) => x.Value).Select(y => (y.LanguageShortName, y.DescriptionLong, y.DescriptionShort)).ToList();

        A.CallTo(() => _offerRepository.AddOfferDescriptions(A<IEnumerable<(Guid offerId, string languageShortName, string descriptionLong, string descriptionShort)>>._))
            .Invokes((IEnumerable<(Guid offerId, string languageShortName, string descriptionLong, string descriptionShort)> offerDescriptions) =>
                {
                    foreach (var x in offerDescriptions)
                    {
                        seed[(x.offerId, x.languageShortName)] = new OfferDescription(x.offerId, x.languageShortName, x.descriptionLong, x.descriptionShort);
                    }
                });

        A.CallTo(() => _offerRepository.RemoveOfferDescriptions(A<IEnumerable<(Guid offerId, string languageShortName)>>._))
            .Invokes((IEnumerable<(Guid offerId, string languageShortName)> offerDescriptionIds) =>
            {
                foreach (var x in offerDescriptionIds)
                {
                    seed.Remove((x.offerId, x.languageShortName));
                }
            });

        A.CallTo(() => _offerRepository.AttachAndModifyOfferDescription(A<Guid>._, A<string>._, A<Action<OfferDescription>>._)) 
            .Invokes((Guid offerId, string languageShortName, Action<OfferDescription> setOptionalParameters) => 
            {
                if (!seed.TryGetValue((offerId, languageShortName), out var offerDescription))
                {
                    offerDescription = new OfferDescription(offerId, languageShortName, null!, null!);
                    seed[(offerId, languageShortName)] = offerDescription;
                }
               
                setOptionalParameters.Invoke(offerDescription);
            });

        var sut = new OfferService(_portalRepositories, null!, null!, null!, null!);

        sut.UpsertRemoveOfferDescription(seedOfferId, updateDescriptions, existingDescriptions);

        A.CallTo(() => _offerRepository.AddOfferDescriptions(A<IEnumerable<(Guid offerId, string languageShortName, string descriptionLong, string descriptionShort)>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.RemoveOfferDescriptions(A<IEnumerable<(Guid offerId, string languageShortName)>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.AttachAndModifyOfferDescription(A<Guid>._, A<string>._, A<Action<OfferDescription>>._)) 
            .MustHaveHappenedTwiceExactly();

        seed.Should().HaveSameCount(updateDescriptions);
        updateDescriptions.Should().AllSatisfy(x => seed.Should().ContainKey((seedOfferId, x.LanguageCode)));
        updateDescriptions.Should().AllSatisfy(x => seed[(seedOfferId, x.LanguageCode)].DescriptionLong.Should().BeSameAs(x.LongDescription));
        updateDescriptions.Should().AllSatisfy(x => seed[(seedOfferId, x.LanguageCode)].DescriptionShort.Should().BeSameAs(x.ShortDescription));
    }

    #endregion

    #region CreateOrUpdateOfferLicense

    [Fact]
    public void CreateOrUpdateOfferLicense_AssignedToMultipleOffers_ReturnsExpected()
    {
        var offerId = _fixture.Create<Guid>();
        var price = _fixture.Create<string>();
        var offerLicense = _fixture.Build<(Guid OfferLicenseId, string LicenseText, bool AssignedToMultipleOffers)>().With(x => x.AssignedToMultipleOffers, true).Create();
        var offerLicenseId = _fixture.Create<Guid>();

        A.CallTo(() => _offerRepository.CreateOfferLicenses(A<string>._)).ReturnsLazily((string p) => new OfferLicense(offerLicenseId, p));

        var sut = new OfferService(_portalRepositories, null!, null!, null!, null!);

        sut.CreateOrUpdateOfferLicense(offerId, price, offerLicense);

        A.CallTo(() => _offerRepository.AttachAndModifyOfferLicense(A<Guid>._, A<Action<OfferLicense>>._)).MustNotHaveHappened();

        A.CallTo(() => _offerRepository.CreateOfferLicenses(price)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.RemoveOfferAssignedLicense(offerId, offerLicense.OfferLicenseId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.CreateOfferAssignedLicense(offerId, offerLicenseId)).MustHaveHappened();
    }

    [Fact]
    public void CreateOrUpdateOfferLicense_NotAssignedToMultipleOffers_ReturnsExpected()
    {
        var offerId = _fixture.Create<Guid>();
        var price = _fixture.Create<string>();
        var offerLicense = _fixture.Build<(Guid OfferLicenseId, string LicenseText, bool AssignedToMultipleOffers)>().With(x => x.AssignedToMultipleOffers, false).Create();
        OfferLicense? modifiedOfferLicense = null;

        A.CallTo(() => _offerRepository.AttachAndModifyOfferLicense(offerLicense.OfferLicenseId, A<Action<OfferLicense>>._))
            .Invokes((Guid offerLicenseId, Action<OfferLicense> setOptionalParameters) =>
            {
                modifiedOfferLicense = new OfferLicense(offerLicenseId, null!);
                setOptionalParameters.Invoke(modifiedOfferLicense);
            });

        var sut = new OfferService(_portalRepositories, null!, null!, null!, null!);

        sut.CreateOrUpdateOfferLicense(offerId, price, offerLicense);

        A.CallTo(() => _offerRepository.CreateOfferLicenses(A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _offerRepository.RemoveOfferAssignedLicense(A<Guid>._, A<Guid>._)).MustNotHaveHappened();
        A.CallTo(() => _offerRepository.CreateOfferAssignedLicense(A<Guid>._, A<Guid>._)).MustNotHaveHappened();

        A.CallTo(() => _offerRepository.AttachAndModifyOfferLicense(A<Guid>._, A<Action<OfferLicense>>._)).MustHaveHappenedOnceExactly();
        modifiedOfferLicense.Should().NotBeNull();
        modifiedOfferLicense!.Licensetext.Should().NotBeNull(); 
        modifiedOfferLicense!.Licensetext.Should().BeSameAs(price);
    }

    #endregion

    #region SubmitOffer

    [Theory]
    [InlineData(OfferTypeId.APP)]
    [InlineData(OfferTypeId.SERVICE)]
    public async Task SubmitOffer_WithNotExistingOffer_ThrowsNotFoundException(OfferTypeId offerType)
    {
        // Arrange
        var notExistingOffer = _fixture.Create<Guid>();
        A.CallTo(() => _offerRepository.GetOfferReleaseDataByIdAsync(notExistingOffer, offerType)).ReturnsLazily(() => (OfferReleaseData?)null);

        var sut = new OfferService(_portalRepositories, null!, null!, null!, null!);

        // Act
        async Task Act() => await sut.SubmitOfferAsync(notExistingOffer, _iamUserId, offerType, new [] { NotificationTypeId.APP_SUBSCRIPTION_REQUEST }, _fixture.Create<IDictionary<string, IEnumerable<string>>>()).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act).ConfigureAwait(false);
        ex.Message.Should().Be($"{offerType.ToString()} {notExistingOffer} does not exist");
    }

    [Theory]
    [InlineData(OfferTypeId.APP)]
    [InlineData(OfferTypeId.SERVICE)]
    public async Task SubmitOffer_WithInvalidOffer_ThrowsConflictException(OfferTypeId offerType)
    {
        // Arrange
        var notExistingOffer = _fixture.Create<Guid>();
        A.CallTo(() => _offerRepository.GetOfferReleaseDataByIdAsync(notExistingOffer, offerType)).ReturnsLazily(() => new OfferReleaseData(null, null, null, null, "company", true, true));

        var sut = new OfferService(_portalRepositories, null!, null!, null!, null!);

        // Act
        async Task Act() => await sut.SubmitOfferAsync(notExistingOffer, _iamUserId, offerType, new [] { NotificationTypeId.APP_SUBSCRIPTION_REQUEST }, _fixture.Create<IDictionary<string, IEnumerable<string>>>()).ConfigureAwait(false);

        // Assert
        await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);
    }

    [Theory]
    [InlineData(OfferTypeId.APP)]
    [InlineData(OfferTypeId.SERVICE)]
    public async Task SubmitOffer_WithValidOfferData_UpdatesAppAndSendsNotification(OfferTypeId offerType)
    {
        // Arrange
        var offer = _fixture.Create<Offer>();
        var offerId = _fixture.Create<Guid>();
        var data = _fixture.Build<OfferReleaseData>()
            .With(x => x.IsDescriptionLongNotSet, false)
            .With(x => x.IsDescriptionShortNotSet, false)
            .Create();
        var userId = _fixture.Create<Guid>();
        A.CallTo(() => _offerRepository.GetOfferReleaseDataByIdAsync(offerId, offerType)).ReturnsLazily(() => data);
        A.CallTo(() => _userRepository.GetCompanyUserIdForIamUserUntrackedAsync(A<string>._)).ReturnsLazily(() => userId);
        A.CallTo(() => _offerRepository.AttachAndModifyOffer(offerId, A<Action<Offer>>._, A<Action<Offer>?>._)).Invokes(
            x =>
            {
                var setOptionalParameters = x.Arguments.Get<Action<Offer>>("setOptionalParameters")!;
                var initializeParemeters = x.Arguments.Get<Action<Offer>?>("initializeParemeters")!;

                initializeParemeters?.Invoke(offer);
                setOptionalParameters.Invoke(offer);
            });
        var sut = new OfferService(_portalRepositories, null!, null!, _notificationService, null!);

        // Act
        await sut.SubmitOfferAsync(offerId, _iamUserId, offerType, new [] { NotificationTypeId.APP_SUBSCRIPTION_REQUEST }, _fixture.Create<IDictionary<string, IEnumerable<string>>>()).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _notificationService.CreateNotifications(A<IDictionary<string, IEnumerable<string>>>._, userId, A<IEnumerable<(string? content, NotificationTypeId notifcationTypeId)>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.AttachAndModifyOffer(offerId, A<Action<Offer>>._, A<Action<Offer>?>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    #endregion

    #region Setup

    private void SetupValidateSalesManager()
    {
        var roleIds = _fixture.CreateMany<Guid>(2);
        A.CallTo(() => _userRolesRepository.GetUserRoleIdsUntrackedAsync(A<IDictionary<string, IEnumerable<string>>>._))
            .Returns(roleIds.ToAsyncEnumerable());
        A.CallTo(() => _userRepository.GetRolesAndCompanyMembershipUntrackedAsync(A<string>._, A<IEnumerable<Guid>>._, A<Guid>.That.Matches(x => x == _companyUser.Id)))
            .ReturnsLazily(() => new ValueTuple<IEnumerable<Guid>, bool, Guid>(roleIds, true, _companyUser.CompanyId));
        A.CallTo(() => _userRepository.GetRolesAndCompanyMembershipUntrackedAsync(A<string>._, A<IEnumerable<Guid>>._, A<Guid>.That.Matches(x => x == _differentCompanyUserId)))
            .ReturnsLazily(() => new ValueTuple<IEnumerable<Guid>, bool, Guid>(Enumerable.Repeat(roleIds.First(), 1), false, Guid.NewGuid()));
        A.CallTo(() => _userRepository.GetRolesAndCompanyMembershipUntrackedAsync(A<string>._, A<IEnumerable<Guid>>._, A<Guid>.That.Matches(x => x == _noSalesManagerUserId)))
            .ReturnsLazily(() => new ValueTuple<IEnumerable<Guid>, bool, Guid>(Enumerable.Repeat(roleIds.First(), 1), true, _companyUser.CompanyId));
        A.CallTo(() => _userRepository.GetRolesAndCompanyMembershipUntrackedAsync(A<string>._, A<IEnumerable<Guid>>._, A<Guid>.That.Not.Matches(x => x == _companyUser.Id || x == _differentCompanyUserId || x == _noSalesManagerUserId)))
            .ReturnsLazily(() => new ValueTuple<IEnumerable<Guid>, bool, Guid>());
        A.CallTo(() => _userRepository.GetOwnCompanyId(A<string>.That.IsEqualTo(_iamUserId))).Returns(_companyUser.CompanyId);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRolesRepository>()).Returns(_userRolesRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ILanguageRepository>()).Returns(_languageRepository);
    }

    private void SetupRepositories()
    {
        A.CallTo(() => _agreementRepository.CheckAgreementExistsForSubscriptionAsync(A<Guid>.That.Matches(x => x == _existingAgreementId), A<Guid>._, A<OfferTypeId>.That.Matches(x => x == OfferTypeId.SERVICE)))
            .ReturnsLazily(() => true);
        A.CallTo(() => _agreementRepository.CheckAgreementExistsForSubscriptionAsync(A<Guid>._, A<Guid>._, A<OfferTypeId>.That.Not.Matches(x => x == OfferTypeId.SERVICE)))
            .ReturnsLazily(() => false);
        A.CallTo(() => _agreementRepository.CheckAgreementExistsForSubscriptionAsync(A<Guid>.That.Not.Matches(x => x == _existingAgreementId), A<Guid>._, A<OfferTypeId>._))
            .ReturnsLazily(() => false);
        
        A.CallTo(() => _agreementRepository.CheckAgreementsExistsForSubscriptionAsync(A<IEnumerable<Guid>>.That.Matches(x => x.Any(y => y == _existingAgreementId)), A<Guid>._, A<OfferTypeId>._))
            .ReturnsLazily(() => true);
        A.CallTo(() => _agreementRepository.CheckAgreementsExistsForSubscriptionAsync(A<IEnumerable<Guid>>.That.Matches(x => x.All(y => y != _existingAgreementId)), A<Guid>._, A<OfferTypeId>._))
            .ReturnsLazily(() => false);
        
        A.CallTo(() => _userRepository.GetCompanyUserWithIamUserCheckAndCompanyShortName(_iamUserId, _companyUser.Id))
            .ReturnsLazily(() => new List<(Guid CompanyUserId, bool IsIamUser, string CompanyUserName, Guid CompanyId)>{new (this._companyUser.Id, true, "COMPANYBPN", _companyUserCompanyId), new (this._companyUser.Id, false, "OTHERCOMPANYBPN", _companyUserCompanyId)}.ToAsyncEnumerable());
        A.CallTo(() => _userRepository.GetCompanyUserWithIamUserCheckAndCompanyShortName(_iamUserId, A<Guid>.That.Not.Matches(x => x == _companyUser.Id)))
            .ReturnsLazily(() => new List<(Guid CompanyUserId, bool IsIamUser, string CompanyUserName, Guid CompanyId)>{new (this._companyUser.Id, true, "COMPANYBPN", _companyUserCompanyId)}.ToAsyncEnumerable());
        A.CallTo(() => _userRepository.GetCompanyUserWithIamUserCheckAndCompanyShortName(A<string>.That.Not.Matches(x => x == _iamUserId), _companyUser.Id))
            .ReturnsLazily(() => new List<(Guid CompanyUserId, bool IsIamUser, string CompanyUserName, Guid CompanyId)>{new (this._companyUser.Id, false, "OTHERCOMPANYBPN", _companyUserCompanyId)}.ToAsyncEnumerable());
        A.CallTo(() => _userRepository.GetCompanyUserWithIamUserCheckAndCompanyShortName(A<string>.That.Not.Matches(x => x == _iamUserId), A<Guid>.That.Not.Matches(x => x == _companyUser.Id)))
            .ReturnsLazily(() => new List<(Guid CompanyUserId, bool IsIamUser, string CompanyUserName, Guid CompanyId)>().ToAsyncEnumerable());

        var offerSubscription = _fixture.Create<OfferSubscription>();
        A.CallTo(() => _offerSubscriptionsRepository.GetCompanyIdWithAssignedOfferForCompanyUserAndSubscriptionAsync(
                A<Guid>.That.Matches(x => x == _existingServiceId), A<string>.That.Matches(x => x == _iamUserId), A<OfferTypeId>.That.Matches(x => x == OfferTypeId.SERVICE)))
            .ReturnsLazily(() => new ValueTuple<Guid, OfferSubscription?, Guid>(this._companyUser.CompanyId, offerSubscription, this._companyUser.Id));
        A.CallTo(() => _offerSubscriptionsRepository.GetCompanyIdWithAssignedOfferForCompanyUserAndSubscriptionAsync(
                A<Guid>.That.Matches(x => x == _existingServiceId), A<string>.That.Matches(x => x == _iamUserId), A<OfferTypeId>.That.Not.Matches(x => x == OfferTypeId.SERVICE)))
            .ReturnsLazily(() => new ValueTuple<Guid, OfferSubscription?, Guid>(this._companyUser.CompanyId, null, this._companyUser.Id));
        A.CallTo(() => _offerSubscriptionsRepository.GetCompanyIdWithAssignedOfferForCompanyUserAndSubscriptionAsync(
                A<Guid>.That.Not.Matches(x => x == _existingServiceId), A<string>.That.Matches(x => x == _iamUserId),
                A<OfferTypeId>._))
            .ReturnsLazily(() => new ValueTuple<Guid, OfferSubscription?, Guid>(this._companyUser.CompanyId, null, this._companyUser.Id));
        A.CallTo(() => _offerSubscriptionsRepository.GetCompanyIdWithAssignedOfferForCompanyUserAndSubscriptionAsync(
                A<Guid>.That.Matches(x => x == _existingServiceId), A<string>.That.Not.Matches(x => x == _iamUserId),
                A<OfferTypeId>._))
            .ReturnsLazily(() => ((Guid companyId, OfferSubscription? offerSubscription, Guid companyUserId))default);

        var agreementData = _fixture.CreateMany<AgreementData>(1);
        A.CallTo(() => _agreementRepository.GetOfferAgreementDataForOfferId(A<Guid>.That.Matches(x => x == _existingServiceId), A<OfferTypeId>._))
            .Returns(agreementData.ToAsyncEnumerable());
        A.CallTo(() => _agreementRepository.GetOfferAgreementDataForOfferId(A<Guid>.That.Not.Matches(x => x == _existingServiceId), A<OfferTypeId>._))
            .Returns(new List<AgreementData>().ToAsyncEnumerable());

        A.CallTo(() => _consentRepository.GetConsentDetailData(A<Guid>.That.Matches(x => x == _validConsentId), A<OfferTypeId>.That.Matches(x => x == OfferTypeId.SERVICE)))
            .ReturnsLazily(() =>
                new ConsentDetailData(_validConsentId, "The Company", this._companyUser.Id, ConsentStatusId.ACTIVE,
                    "Agreed"));
        A.CallTo(() => _consentRepository.GetConsentDetailData(A<Guid>.That.Not.Matches(x => x == _validConsentId), A<OfferTypeId>._))
            .ReturnsLazily(() => (ConsentDetailData?)null);
        A.CallTo(() => _consentRepository.GetConsentDetailData(A<Guid>._, A<OfferTypeId>.That.Not.Matches(x => x == OfferTypeId.SERVICE)))
            .ReturnsLazily(() => (ConsentDetailData?)null);

        A.CallTo(() => _consentAssignedOfferSubscriptionRepository.GetConsentAssignedOfferSubscriptionsForSubscriptionAsync(A<Guid>._, A<IEnumerable<Guid>>.That.Not.Matches(x => x.Any(y => y ==_existingAgreementForSubscriptionId))))
            .ReturnsLazily(() => new List<(Guid ConsentId, Guid AgreementId, ConsentStatusId ConsentStatusId)>().ToAsyncEnumerable());
        
        A.CallTo(() => _offerSubscriptionsRepository.GetOfferDetailsAndCheckUser(
                A<Guid>.That.Matches(x => x == _validSubscriptionId),
                A<string>.That.Matches(x => x == _iamUserId),
                A<OfferTypeId>._))
            .ReturnsLazily(() => new OfferSubscriptionTransferData(OfferSubscriptionStatusId.ACTIVE, _companyUser.Id, Guid.Empty,
                _companyUser.Company!.Name, _companyUser.CompanyId, _companyUser.Id, _existingServiceId, "Test Service",
                Bpn, "user@email.com", "Tony"));
        A.CallTo(() => _offerSubscriptionsRepository.GetOfferDetailsAndCheckUser(
                A<Guid>.That.Matches(x => x == _pendingSubscriptionId),
                A<string>.That.Matches(x => x == _iamUserIdWithoutMail),
                A<OfferTypeId>._))
            .ReturnsLazily(() => new OfferSubscriptionTransferData(OfferSubscriptionStatusId.PENDING, _companyUser.Id, Guid.Empty,
                _companyUser.Company!.Name, _companyUser.CompanyId, _companyUser.Id, _existingServiceId, "Test Service",
                Bpn, null, null));
        A.CallTo(() => _offerSubscriptionsRepository.GetOfferDetailsAndCheckUser(
                A<Guid>.That.Matches(x => x == _pendingSubscriptionId),
                A<string>.That.Matches(x => x == _iamUserId),
                A<OfferTypeId>._))
            .ReturnsLazily(() => new OfferSubscriptionTransferData(OfferSubscriptionStatusId.PENDING, _companyUser.Id, Guid.Empty,
                string.Empty, _companyUser.CompanyId, _companyUser.Id, _existingServiceId, "Test Service",
                Bpn, "user@email.com", "Tony"));
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
                Bpn, null, null));

        A.CallTo(() => _languageRepository.GetLanguageCodesUntrackedAsync(A<IEnumerable<string>>.That.Matches(x => x.Count() == 1 && x.All(y => y == "en"))))
            .Returns(new List<string> { "en" }.ToAsyncEnumerable());
        A.CallTo(() => _languageRepository.GetLanguageCodesUntrackedAsync(A<IEnumerable<string>>.That.Matches(x => x.Count() == 1 && x.All(y => y == "gg"))))
            .Returns(new List<string>().ToAsyncEnumerable());

        A.CallTo(() => _portalRepositories.GetInstance<IAgreementRepository>()).Returns(_agreementRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IAppSubscriptionDetailRepository>()).Returns(_appSubscriptionDetailRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IAppInstanceRepository>()).Returns(_appInstanceRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IClientRepository>()).Returns(_clientRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IConsentRepository>()).Returns(_consentRepository);        
        A.CallTo(() => _portalRepositories.GetInstance<INotificationRepository>()).Returns(_notificationRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IOfferRepository>()).Returns(_offerRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IOfferSubscriptionsRepository>()).Returns(_offerSubscriptionsRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IConsentAssignedOfferSubscriptionRepository>()).Returns(_consentAssignedOfferSubscriptionRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ILanguageRepository>()).Returns(_languageRepository);
        _fixture.Inject(_portalRepositories);
    }

    private void SetupServices()
    {
        A.CallTo(() => _provisioningManager.SetupClientAsync(A<string>._, A<IEnumerable<string>?>._))
            .ReturnsLazily(() => "cl1");
        
        A.CallTo(() => _serviceAccountCreation.CreateServiceAccountAsync(A<string>._, A<string>._,
                A<IamClientAuthMethod>._, A<IEnumerable<Guid>>._, A<Guid>._, A<IEnumerable<string>>.That.Matches(x => x.Any(y => y == "CAXSDUMMYCATENAZZ"))))
            .ReturnsLazily(() => new ValueTuple<string, ServiceAccountData, Guid, List<UserRoleData>>(
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

    #endregion
}
