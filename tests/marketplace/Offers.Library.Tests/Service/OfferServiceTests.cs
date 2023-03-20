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
using Org.Eclipse.TractusX.Portal.Backend.Notifications.Library;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Mailing.SendMail;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;

namespace Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Tests.Service;

public class OfferServiceTests
{
    private readonly Guid _companyUserCompanyId = new("395f955b-f11b-4a74-ab51-92a526c1973a");
    private readonly Guid _existingServiceId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47661");
    private readonly Guid _existingAgreementId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47664");
    private readonly Guid _validConsentId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47664");
    private readonly Guid _existingAgreementForSubscriptionId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47665");
    private readonly Guid _differentCompanyUserId = Guid.NewGuid();
    private readonly Guid _noSalesManagerUserId = Guid.NewGuid();

    private readonly CompanyUser _companyUser;
    private readonly IFixture _fixture;
    private readonly string _iamUserId;
    private readonly IAgreementRepository _agreementRepository;
    private readonly IConsentRepository _consentRepository;
    private readonly IOfferSubscriptionsRepository _offerSubscriptionsRepository;
    private readonly IConsentAssignedOfferSubscriptionRepository _consentAssignedOfferSubscriptionRepository;
    private readonly IPortalRepositories _portalRepositories;
    private readonly INotificationService _notificationService;
    private readonly IOfferRepository _offerRepository;
    private readonly IUserRepository _userRepository;    
    private readonly IUserRolesRepository _userRolesRepository;
    private readonly ILanguageRepository _languageRepository;
    private readonly IMailingService _mailingService;
    private readonly IamUser _iamUser;
    private readonly IDocumentRepository _documentRepository;
    private readonly OfferService _sut;

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
        _portalRepositories = A.Fake<IPortalRepositories>();
        _agreementRepository = A.Fake<IAgreementRepository>();
        _consentRepository = A.Fake<IConsentRepository>();
        _offerSubscriptionsRepository = A.Fake<IOfferSubscriptionsRepository>();
        _consentAssignedOfferSubscriptionRepository = A.Fake<IConsentAssignedOfferSubscriptionRepository>();
        _offerRepository = A.Fake<IOfferRepository>();
        _userRepository = A.Fake<IUserRepository>();
        _userRolesRepository = A.Fake<IUserRolesRepository>();
        _languageRepository = A.Fake<ILanguageRepository>();
        _notificationService = A.Fake<INotificationService>();
        _mailingService = A.Fake<IMailingService>();
        _iamUser = _fixture.Build<IamUser>()
            .With(u => u.CompanyUser, _companyUser)
            .Create();
        _documentRepository = A.Fake<IDocumentRepository>();

        _sut = new OfferService(_portalRepositories, _notificationService, _mailingService);

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

        // Act
        var result = await _sut.CreateServiceOfferingAsync(new ServiceOfferingData("Newest Service", "42", "mail@test.de", _companyUser.Id, new List<LocalizedDescription>(), new List<ServiceTypeId>()), _iamUserId, OfferTypeId.SERVICE);

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

        // Act
        var serviceOfferingData = new ServiceOfferingData("Newest Service", "42", "mail@test.de", _companyUser.Id, new List<LocalizedDescription>
        {
            new ("en", "That's a description with a valid language code", "Short description")
        },
        new[]
        {
            ServiceTypeId.DATASPACE_SERVICE
        });
        var result = await _sut.CreateServiceOfferingAsync(serviceOfferingData, _iamUserId, OfferTypeId.SERVICE);

        // Assert
        result.Should().Be(serviceId);
        apps.Should().HaveCount(1);
        A.CallTo(() => _offerRepository.AddServiceAssignedServiceTypes(A<IEnumerable<(Guid serviceId, ServiceTypeId serviceTypeId, bool technicalUserNeeded)>>.That.Matches(s => s.Any(x => x.serviceId == serviceId)))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.AddOfferDescriptions(A< IEnumerable<(Guid offerId, string languageShortName, string descriptionLong, string descriptionShort)>>._)).MustHaveHappenedOnceExactly();
    }
    
    [Fact]
    public async Task CreateServiceOffering_WithWrongIamUser_ThrowsException()
    {
        // Act
        async Task Action() => await _sut.CreateServiceOfferingAsync(new ServiceOfferingData("Newest Service", "42", "mail@test.de", _companyUser.Id, new List<LocalizedDescription>(), new List<ServiceTypeId>()), Guid.NewGuid().ToString(), OfferTypeId.SERVICE);
        
        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("iamUserId");
    }

    [Fact]
    public async Task CreateServiceOffering_WithInvalidLanguage_ThrowsException()
    {
        // Act
        var serviceOfferingData = new ServiceOfferingData("Newest Service", "42", "mail@test.de", _companyUser.Id, new List<LocalizedDescription>
        {
            new ("gg", "That's a description with incorrect language short code", "Short description")
        }, new List<ServiceTypeId>());
        async Task Action() => await _sut.CreateServiceOfferingAsync(serviceOfferingData, _iamUserId, OfferTypeId.SERVICE);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("languageCodes");
    }

    [Fact]
    public async Task CreateServiceOffering_WithoutCompanyUser_ThrowsException()
    {
        // Act
        async Task Action() => await _sut.CreateServiceOfferingAsync(new ServiceOfferingData("Newest Service", "42", "mail@test.de", Guid.NewGuid(), new List<LocalizedDescription>(), new List<ServiceTypeId>()), _iamUserId, OfferTypeId.SERVICE);
        
        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("SalesManager");
    }

    #endregion

    #region Get Service Agreement

    [Fact]
    public async Task GetOfferAgreement_WithUserId_ReturnsServiceDetailData()
    {
        // Act
        var result = await _sut.GetOfferAgreementsAsync(_existingServiceId, OfferTypeId.SERVICE).ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().ContainSingle();
    }

    [Fact]
    public async Task GetOfferAgreement_WithoutExistingService_ThrowsException()
    {
        // Act
        var agreementData = await _sut.GetOfferAgreementsAsync(Guid.NewGuid(), OfferTypeId.SERVICE).ToListAsync().ConfigureAwait(false);

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

        // Act
        var result = await _sut.CreateOfferSubscriptionAgreementConsentAsync(_existingServiceId, _existingAgreementId, statusId, _iamUserId, OfferTypeId.SERVICE);

        // Assert
        result.Should().Be(consentId);
        consents.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateOfferAgreementConsentAsync_WithNotExistingAgreement_ThrowsException()
    {
        // Arrange
        var nonExistingAgreementId = Guid.NewGuid();

        // Act
        async Task Action() => await _sut.CreateOfferSubscriptionAgreementConsentAsync(_existingServiceId, nonExistingAgreementId, ConsentStatusId.ACTIVE, _iamUserId, OfferTypeId.SERVICE);
        
        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.Message.Should().Be($"Invalid Agreement {nonExistingAgreementId} for subscription {_existingServiceId} (Parameter 'agreementId')");
    }

    [Fact]
    public async Task CreateOfferAgreementConsentAsync_WithWrongUser_ThrowsException()
    {
        // Act
        async Task Action() => await _sut.CreateOfferSubscriptionAgreementConsentAsync(_existingServiceId, _existingAgreementId, ConsentStatusId.ACTIVE, Guid.NewGuid().ToString(), OfferTypeId.SERVICE);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("iamUserId");
    }

    [Fact]
    public async Task CreateOfferAgreementConsentAsync_WithNotExistingService_ThrowsException()
    {
        // Arrange
        var notExistingServiceId = Guid.NewGuid();

        // Act
        async Task Action() => await _sut.CreateOfferSubscriptionAgreementConsentAsync(notExistingServiceId, _existingAgreementId, ConsentStatusId.ACTIVE, _iamUserId, OfferTypeId.SERVICE);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Action);
        ex.Message.Should().Be($"Invalid OfferSubscription {notExistingServiceId} for OfferType SERVICE");
    }

    [Fact]
    public async Task CreateOfferAgreementConsentAsync_WithInvalidOfferType_ThrowsException()
    {
        // Act
        async Task Action() => await _sut.CreateOfferSubscriptionAgreementConsentAsync(_existingServiceId, _existingAgreementId, ConsentStatusId.ACTIVE,
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

        // Act
        await _sut.CreateOrUpdateOfferSubscriptionAgreementConsentAsync(_existingServiceId, data, _iamUserId, OfferTypeId.SERVICE);

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

        // Act
        async Task Action() => await _sut.CreateOrUpdateOfferSubscriptionAgreementConsentAsync(_existingServiceId, data, _iamUserId, OfferTypeId.SERVICE);
        
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

        // Act
        async Task Action() => await _sut.CreateOrUpdateOfferSubscriptionAgreementConsentAsync(_existingServiceId, data, Guid.NewGuid().ToString(), OfferTypeId.SERVICE);

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

        // Act
        async Task Action() => await _sut.CreateOrUpdateOfferSubscriptionAgreementConsentAsync(notExistingServiceId, data,
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

        // Act
        async Task Action() => await _sut.CreateOrUpdateOfferSubscriptionAgreementConsentAsync(_existingServiceId, data,
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
        // Act
        var result = await _sut.GetConsentDetailDataAsync(_validConsentId, OfferTypeId.SERVICE).ConfigureAwait(false);

        // Assert
        result.Id.Should().Be(_validConsentId);
        result.CompanyName.Should().Be("The Company");
    }

    [Fact]
    public async Task GetServiceConsentDetailData_WithInvalidId_ThrowsException()
    {
        // Arrange
        var notExistingId = Guid.NewGuid();

        // Act
        async Task Action() => await _sut.GetConsentDetailDataAsync(notExistingId, OfferTypeId.SERVICE).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Action);
        ex.Message.Should().Be($"Consent {notExistingId} does not exist");
    }

    #endregion

    #region Validate SalesManager
    
    [Fact]
    public async Task AddAppAsync_WithInvalidSalesManager_ThrowsException()
    {
        // Arrange
        SetupValidateSalesManager();

        //null Act
        async Task Act() => await _sut.ValidateSalesManager(Guid.NewGuid(), _iamUserId, new Dictionary<string, IEnumerable<string>>()).ConfigureAwait(false);

        // Assert
        var error = await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);
        error.ParamName.Should().Be("salesManagerId");
    }

    [Fact]
    public async Task AddAppAsync_WithUserFromOtherCompany_ThrowsException()
    {
        // Arrange
        SetupValidateSalesManager();

        // Act
        async Task Act() => await _sut.ValidateSalesManager(_differentCompanyUserId, _iamUserId, new Dictionary<string, IEnumerable<string>>()).ConfigureAwait(false);

        // Assert
        var error = await Assert.ThrowsAsync<ForbiddenException>(Act).ConfigureAwait(false);
        error.Message.Should().Contain("is not a member of the company");
    }

    [Fact]
    public async Task AddAppAsync_WithUserWithoutSalesManagerRole_ThrowsException()
    {
        // Arrange
        SetupValidateSalesManager();
     
        // Act
        async Task Act() => await _sut.ValidateSalesManager(_noSalesManagerUserId, _iamUserId, new Dictionary<string, IEnumerable<string>>()).ConfigureAwait(false);

        // Assert
        var error = await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);
        error.ParamName.Should().Be("salesManagerId");
    }
    
    #endregion
    
    #region UpsertRemoveOfferDescription

    [Fact]
    public void UpsertRemoveOfferDescription_ReturnsExpected()
    {
        // Arrange
        var seedOfferId = _fixture.Create<Guid>();
        var seed = new Dictionary<(Guid,string),OfferDescription>() {
            {(seedOfferId, "de"), new OfferDescription(seedOfferId, "de", _fixture.Create<string>(), _fixture.Create<string>())},
            {(seedOfferId, "en"), new OfferDescription(seedOfferId, "en", _fixture.Create<string>(), _fixture.Create<string>())},
            {(seedOfferId, "fr"), new OfferDescription(seedOfferId, "fr", _fixture.Create<string>(), _fixture.Create<string>())},
            {(seedOfferId, "cz"), new OfferDescription(seedOfferId, "cz", _fixture.Create<string>(), _fixture.Create<string>())},
            {(seedOfferId, "it"), new OfferDescription(seedOfferId, "it", _fixture.Create<string>(), _fixture.Create<string>())},
        };

        var updateDescriptions = new [] {
            new LocalizedDescription("de", _fixture.Create<string>(), _fixture.Create<string>()),
            new LocalizedDescription("fr", _fixture.Create<string>(), _fixture.Create<string>()),
            new LocalizedDescription("sk", _fixture.Create<string>(), _fixture.Create<string>()),
            new LocalizedDescription("se", _fixture.Create<string>(), _fixture.Create<string>()),
            new LocalizedDescription("it", null!,null!)
        };

        var existingDescriptions = seed.Select((x) => x.Value).Select(y => new LocalizedDescription(y.LanguageShortName, y.DescriptionLong, y.DescriptionShort)).ToList();

        A.CallTo(() => _offerRepository.CreateUpdateDeleteOfferDescriptions(seedOfferId, existingDescriptions, 
            updateDescriptions.Select(x => new ValueTuple<string, string, string>(x.LanguageCode, x.LongDescription, x.ShortDescription))));

        _sut.UpsertRemoveOfferDescription(seedOfferId, updateDescriptions, existingDescriptions);

        // Assert
        A.CallTo(() => _offerRepository.CreateUpdateDeleteOfferDescriptions(A<Guid>._,A<IEnumerable<LocalizedDescription>>._
            ,A<IEnumerable<(string, string, string)>>._)).MustHaveHappened();
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

        _sut.CreateOrUpdateOfferLicense(offerId, price, offerLicense);

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

        _sut.CreateOrUpdateOfferLicense(offerId, price, offerLicense);

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
    public async Task SubmitOffer_WithNotExistingOffer_ThrowsNotFoundException(OfferTypeId offerType)
    {
        // Arrange
        var notExistingOffer = _fixture.Create<Guid>();
        A.CallTo(() => _offerRepository.GetOfferReleaseDataByIdAsync(notExistingOffer, offerType)).ReturnsLazily(() => (OfferReleaseData?)null);

        // Act
        async Task Act() => await _sut.SubmitOfferAsync(notExistingOffer, _iamUserId, offerType, new [] { NotificationTypeId.APP_SUBSCRIPTION_REQUEST }, _fixture.Create<IDictionary<string, IEnumerable<string>>>(),new [] { DocumentTypeId.CONFORMITY_APPROVAL_BUSINESS_APPS }).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act).ConfigureAwait(false);
        ex.Message.Should().Be($"{offerType} {notExistingOffer} does not exist");
    }

    [Theory]
    [InlineData(null, "c8d4d854-8ac6-425f-bc5a-dbf457670732", false, false, true)]
    [InlineData("name", null, false, false, true)]
    [InlineData("name", "c8d4d854-8ac6-425f-bc5a-dbf457670732", true, false, true)]
    [InlineData("name", "c8d4d854-8ac6-425f-bc5a-dbf457670732", false, true, true)]
    [InlineData("name", "c8d4d854-8ac6-425f-bc5a-dbf457670732", false, false, false)]
    public async Task SubmitOffer_WithInvalidOffer_ThrowsConflictException(string? name, string? providerCompanyId, bool isDescriptionLongNotSet, bool isDescriptionShortNotSet, bool hasUserRoles)
    {
        // Arrange
        A.CallTo(() => _offerRepository.GetOfferReleaseDataByIdAsync(A<Guid>._,A<OfferTypeId>._)).Returns(new OfferReleaseData(name, providerCompanyId == null ? null : new Guid(providerCompanyId), _fixture.Create<string>(), isDescriptionLongNotSet, isDescriptionShortNotSet, hasUserRoles, null!, new [] { DocumentTypeId.CONFORMITY_APPROVAL_BUSINESS_APPS }));
        A.CallTo(() => _userRepository.GetCompanyUserIdForIamUserUntrackedAsync(A<string>._)).Returns(Guid.NewGuid());

        // Act
        async Task Act() => await _sut.SubmitOfferAsync(Guid.NewGuid(), _iamUserId, _fixture.Create<OfferTypeId>(), _fixture.CreateMany<NotificationTypeId>(1), _fixture.Create<IDictionary<string, IEnumerable<string>>>(), new [] { DocumentTypeId.CONFORMITY_APPROVAL_BUSINESS_APPS }).ConfigureAwait(false);

        // Assert
        var result = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);
        result.Message.Should().StartWith("Missing  : ");
    }

    [Fact]
    public async Task SubmitOffer_WithInvalidRequester_ThrowsConflictException()
    {
        // Arrange
        var data = _fixture.Build<OfferReleaseData>()
            .With(x => x.IsDescriptionLongNotSet, false)
            .With(x => x.IsDescriptionShortNotSet, false)
            .With(x => x.DocumentTypeIds, new [] { DocumentTypeId.CONFORMITY_APPROVAL_BUSINESS_APPS })
            .Create();
        A.CallTo(() => _offerRepository.GetOfferReleaseDataByIdAsync(A<Guid>._,A<OfferTypeId>._)).Returns(data);
        A.CallTo(() => _userRepository.GetCompanyUserIdForIamUserUntrackedAsync(A<string>._)).Returns(Guid.Empty);

        // Act
        async Task Act() => await _sut.SubmitOfferAsync(Guid.NewGuid(), _iamUserId, _fixture.Create<OfferTypeId>(), _fixture.CreateMany<NotificationTypeId>(1), _fixture.Create<IDictionary<string, IEnumerable<string>>>(),  new [] { DocumentTypeId.CONFORMITY_APPROVAL_BUSINESS_APPS }).ConfigureAwait(false);

        // Assert
        var result = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);
        result.Message.Should().StartWith($"keycloak user {_iamUserId} is not associated with any portal user");
    }
    
    [Fact]
    public async Task SubmitOffer_WithInvalidDocumentType_ThrowsConflictException()
    {
        // Arrange
        var data = _fixture.Build<OfferReleaseData>()
            .With(x => x.IsDescriptionLongNotSet, false)
            .With(x => x.IsDescriptionShortNotSet, false)
            .With(x => x.DocumentTypeIds, new [] { DocumentTypeId.SELF_DESCRIPTION })
            .Create();
        var submitAppDocumentTypeIds = new [] { DocumentTypeId.APP_IMAGE,DocumentTypeId.APP_LEADIMAGE,DocumentTypeId.CONFORMITY_APPROVAL_BUSINESS_APPS };
        var userId = _fixture.Create<Guid>();
        A.CallTo(() => _offerRepository.GetOfferReleaseDataByIdAsync(A<Guid>._,A<OfferTypeId>._)).Returns(data);
        A.CallTo(() => _userRepository.GetCompanyUserIdForIamUserUntrackedAsync(A<string>._)).Returns(Guid.Empty);
        var sut = new OfferService(_portalRepositories, null!, null!);

        // Act
        async Task Act() => await sut.SubmitOfferAsync(Guid.NewGuid(), _iamUserId, _fixture.Create<OfferTypeId>(), _fixture.CreateMany<NotificationTypeId>(1), _fixture.Create<IDictionary<string, IEnumerable<string>>>(),  new [] { DocumentTypeId.APP_IMAGE,DocumentTypeId.APP_LEADIMAGE,DocumentTypeId.CONFORMITY_APPROVAL_BUSINESS_APPS }).ConfigureAwait(false);

        // Assert
        var result = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);
        result.Message.Should().StartWith($"{string.Join(",", submitAppDocumentTypeIds)} are mandatory document types");
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
            .With(x => x.DocumentStatusDatas,new[]{
                new DocumentStatusData(Guid.NewGuid(), DocumentStatusId.PENDING),
                new DocumentStatusData(Guid.NewGuid(), DocumentStatusId.INACTIVE)})
            .With(x=> x.DocumentTypeIds, new [] { DocumentTypeId.CONFORMITY_APPROVAL_BUSINESS_APPS, DocumentTypeId.APP_LEADIMAGE, DocumentTypeId.APP_IMAGE})
            .Create();
        var userId = _fixture.Create<Guid>();
        A.CallTo(() => _offerRepository.GetOfferReleaseDataByIdAsync(offerId, offerType)).ReturnsLazily(() => data);
        A.CallTo(() => _userRepository.GetCompanyUserIdForIamUserUntrackedAsync(A<string>._)).ReturnsLazily(() => userId);
        A.CallTo(() => _documentRepository.AttachAndModifyDocument(A<Guid>._,A<Action<Document>>._, A<Action<Document>>._))
            .Invokes((Guid DocId, Action<Document>? initialize, Action<Document> modify)
                => {
                        var document = new Document(DocId, null!, null!, null!, default, default, default, default);
                        initialize?.Invoke(document);
                        modify(document);
                    });
        A.CallTo(() => _offerRepository.AttachAndModifyOffer(offerId, A<Action<Offer>>._, A<Action<Offer>?>._)).Invokes(
            (Guid _, 
                Action<Offer> setOptionalParameters, 
                Action<Offer>? initializeParemeters) =>
            {
                initializeParemeters?.Invoke(offer);
                setOptionalParameters.Invoke(offer);
            });

        // Act
        await _sut.SubmitOfferAsync(offerId, _iamUserId, offerType, new[] { NotificationTypeId.APP_SUBSCRIPTION_REQUEST }, _fixture.Create<IDictionary<string, IEnumerable<string>>>(), new[] { DocumentTypeId.CONFORMITY_APPROVAL_BUSINESS_APPS, DocumentTypeId.APP_LEADIMAGE, DocumentTypeId.APP_IMAGE }).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _notificationService.CreateNotifications(A<IDictionary<string, IEnumerable<string>>>._, userId, A<IEnumerable<(string? content, NotificationTypeId notifcationTypeId)>>._, A<Guid>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _documentRepository.AttachAndModifyDocument(A<Guid>._, A<Action<Document>>._, A<Action<Document>>._)).MustHaveHappenedTwiceExactly();
        A.CallTo(() => _offerRepository.AttachAndModifyOffer(offerId, A<Action<Offer>>._, A<Action<Offer>?>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    #endregion

    #region ApproveOfferRequest

    [Fact]
    public async Task ApproveOfferRequestAsync_ExecutesSuccessfully()
    {
        //Arrange
        var offer = _fixture.Build<Offer>().With(o => o.OfferStatusId, OfferStatusId.IN_REVIEW).Create();
        var requesterId = _fixture.Create<Guid>();
        var companyId = _fixture.Create<Guid>();
        var iamUserId = _fixture.Create<string>();
       
        A.CallTo(() => _offerRepository.GetOfferStatusDataByIdAsync(offer.Id, OfferTypeId.APP))
            .ReturnsLazily(() => (true, offer.Name, companyId));
        A.CallTo(() => _userRepository.GetCompanyUserIdForIamUserUntrackedAsync(iamUserId))
            .ReturnsLazily(() => (requesterId));
        A.CallTo(() => _offerRepository.AttachAndModifyOffer(offer.Id, A<Action<Offer>>._, A<Action<Offer>?>._))
            .Invokes((Guid _, Action<Offer> setOptionalParameters, Action<Offer>? initializeParemeters) => 
            {
                initializeParemeters?.Invoke(offer);
                setOptionalParameters(offer);
            });

        var approveAppNotificationTypeIds = new []
        {
            NotificationTypeId.APP_RELEASE_APPROVAL
        };
        var approveAppUserRoles = new Dictionary<string, IEnumerable<string>>
        {
            { "catenax-portal", new [] { "Sales Manager" } }
        };

        //Act
        await _sut.ApproveOfferRequestAsync(offer.Id, iamUserId, OfferTypeId.APP, approveAppNotificationTypeIds, approveAppUserRoles).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _offerRepository.GetOfferStatusDataByIdAsync(offer.Id, OfferTypeId.APP)).MustHaveHappened();
        A.CallTo(() => _offerRepository.AttachAndModifyOffer(A<Guid>._, A<Action<Offer>>._, A<Action<Offer>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _userRepository.GetCompanyUserIdForIamUserUntrackedAsync(iamUserId)).MustHaveHappened();
        A.CallTo(() => _notificationService.CreateNotifications(A<IDictionary<string, IEnumerable<string>>>._, A<Guid>._, A<IEnumerable<(string? content, NotificationTypeId notificationTypeId)>>._, A<Guid>._)).MustHaveHappened();
        offer.OfferStatusId.Should().Be(OfferStatusId.ACTIVE);
        offer.DateReleased.Should().NotBeNull();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ApproveOfferRequestAsync_WithAppNameNotSet_ThrowsConflictException()
    {
        //Arrange
        var offerId = _fixture.Create<Guid>();
        var iamUserId = _fixture.Create<string>();
        var companyId = _fixture.Create<Guid>();

        A.CallTo(() => _offerRepository.GetOfferStatusDataByIdAsync(offerId, OfferTypeId.APP))
            .ReturnsLazily(() => (true, null, companyId));

        var approveAppNotificationTypeIds = new []
        {
            NotificationTypeId.APP_RELEASE_APPROVAL
        };
        var approveAppUserRoles = new Dictionary<string, IEnumerable<string>>
        {
            { "catenax-portal", new [] { "Sales Manager" } }
        };

        //Act
        Task Act() => _sut.ApproveOfferRequestAsync(offerId, iamUserId, OfferTypeId.APP, approveAppNotificationTypeIds, approveAppUserRoles);

        //Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);
        ex.Message.Should().Be($"Offer {offerId} Name is not yet set.");
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
    }

    [Fact]
    public async Task ApproveOfferRequestAsync_WithProviderCompanyIdNotSet_ThrowsConflictException()
    {
        //Arrange
        var offerId = _fixture.Create<Guid>();
        var iamUserId = _fixture.Create<string>();

        A.CallTo(() => _offerRepository.GetOfferStatusDataByIdAsync(offerId, OfferTypeId.APP))
            .ReturnsLazily(() => (true, "The name", null));

        var approveAppNotificationTypeIds = new []
        {
            NotificationTypeId.APP_RELEASE_APPROVAL
        };
        var approveAppUserRoles = new Dictionary<string, IEnumerable<string>>
        {
            { "catenax-portal", new [] { "Sales Manager" } }
        };

        //Act
        Task Act() => _sut.ApproveOfferRequestAsync(offerId, iamUserId, OfferTypeId.APP, approveAppNotificationTypeIds, approveAppUserRoles);

        //Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);
        ex.Message.Should().Be($"Offer {offerId} providing company is not yet set.");
        A.CallTo(() => _portalRepositories.SaveAsync()).MustNotHaveHappened();
    }

    #endregion
    
    #region SubmitService
    
    [Theory]
    [InlineData(OfferTypeId.SERVICE)]
    public async Task SubmitService_WithNotExistingOffer_ThrowsNotFoundException(OfferTypeId offerType)
    {
        // Arrange
        var notExistingOffer = _fixture.Create<Guid>();
        A.CallTo(() => _offerRepository.GetOfferReleaseDataByIdAsync(notExistingOffer, offerType)).ReturnsLazily(() => (OfferReleaseData?)null);

        var sut = new OfferService(_portalRepositories, null!, null!);

        // Act
        async Task Act() => await sut.SubmitServiceAsync(notExistingOffer, _iamUserId, offerType, new [] { NotificationTypeId.APP_SUBSCRIPTION_REQUEST }, _fixture.Create<IDictionary<string, IEnumerable<string>>>()).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act).ConfigureAwait(false);
        ex.Message.Should().Be($"{offerType} {notExistingOffer} does not exist");
    }

    [Theory]
    [InlineData(null, "c8d4d854-8ac6-425f-bc5a-dbf457670732", false, false, true)]
    [InlineData("name", null, false, false, true)]
    [InlineData("name", "c8d4d854-8ac6-425f-bc5a-dbf457670732", true, false, true)]
    [InlineData("name", "c8d4d854-8ac6-425f-bc5a-dbf457670732", false, true, true)]
    [InlineData("name", "c8d4d854-8ac6-425f-bc5a-dbf457670732", false, false, false)]
    public async Task SubmitService_WithInvalidOffer_ThrowsConflictException(string? name, string? providerCompanyId, bool isDescriptionLongNotSet, bool isDescriptionShortNotSet, bool hasUserRoles)
    {
        // Arrange
        A.CallTo(() => _offerRepository.GetOfferReleaseDataByIdAsync(A<Guid>._,A<OfferTypeId>._)).Returns(new OfferReleaseData(name, providerCompanyId == null ? null : new Guid(providerCompanyId), _fixture.Create<string>(), isDescriptionLongNotSet, isDescriptionShortNotSet, hasUserRoles, null!, new [] { DocumentTypeId.CONFORMITY_APPROVAL_BUSINESS_APPS }));
        A.CallTo(() => _userRepository.GetCompanyUserIdForIamUserUntrackedAsync(A<string>._)).Returns(Guid.NewGuid());

        var sut = new OfferService(_portalRepositories, null!, null!);

        // Act
        async Task Act() => await sut.SubmitServiceAsync(Guid.NewGuid(), _iamUserId, _fixture.Create<OfferTypeId>(), _fixture.CreateMany<NotificationTypeId>(1), _fixture.Create<IDictionary<string, IEnumerable<string>>>()).ConfigureAwait(false);

        // Assert
        var result = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);
        result.Message.Should().StartWith("Missing  : ");
    }
    
     [Fact]
    public async Task SubmitService_WithInvalidRequester_ThrowsConflictException()
    {
        // Arrange
        var data = _fixture.Build<OfferReleaseData>()
            .With(x => x.IsDescriptionLongNotSet, false)
            .With(x => x.IsDescriptionShortNotSet, false)
            .With(x => x.DocumentTypeIds, new [] { DocumentTypeId.CONFORMITY_APPROVAL_BUSINESS_APPS })
            .Create();
        var userId = _fixture.Create<Guid>();
        A.CallTo(() => _offerRepository.GetOfferReleaseDataByIdAsync(A<Guid>._,A<OfferTypeId>._)).Returns(data);
        A.CallTo(() => _userRepository.GetCompanyUserIdForIamUserUntrackedAsync(A<string>._)).Returns(Guid.Empty);
        var sut = new OfferService(_portalRepositories, null!, null!);

        // Act
        async Task Act() => await sut.SubmitServiceAsync(Guid.NewGuid(), _iamUserId, _fixture.Create<OfferTypeId>(), _fixture.CreateMany<NotificationTypeId>(1), _fixture.Create<IDictionary<string, IEnumerable<string>>>()).ConfigureAwait(false);

        // Assert
        var result = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);
        result.Message.Should().StartWith($"keycloak user {_iamUserId} is not associated with any portal user");
    }

    [Theory]
    [InlineData(OfferTypeId.SERVICE)]
    public async Task SubmitService_WithValidOfferData_UpdatesAppAndSendsNotification(OfferTypeId offerType)
    {
        // Arrange
        var offer = _fixture.Create<Offer>();
        var offerId = _fixture.Create<Guid>();
        var data = _fixture.Build<OfferReleaseData>()
            .With(x => x.IsDescriptionLongNotSet, false)
            .With(x => x.IsDescriptionShortNotSet, false)
            .With(x => x.DocumentStatusDatas,new[]{
                new DocumentStatusData(Guid.NewGuid(), DocumentStatusId.PENDING),
                new DocumentStatusData(Guid.NewGuid(), DocumentStatusId.INACTIVE)})
            .With(x=> x.DocumentTypeIds, new [] { DocumentTypeId.CONFORMITY_APPROVAL_BUSINESS_APPS })
            .Create();
        var userId = _fixture.Create<Guid>();
        A.CallTo(() => _offerRepository.GetOfferReleaseDataByIdAsync(offerId, offerType)).ReturnsLazily(() => data);
        A.CallTo(() => _userRepository.GetCompanyUserIdForIamUserUntrackedAsync(A<string>._)).ReturnsLazily(() => userId);
        A.CallTo(() => _documentRepository.AttachAndModifyDocument(A<Guid>._,A<Action<Document>>._, A<Action<Document>>._))
            .Invokes((Guid DocId, Action<Document>? initialize, Action<Document> modify)
                => {
                        var document = new Document(DocId, null!, null!, null!, default, default, default, default);
                        initialize?.Invoke(document);
                        modify(document);
                    });
        A.CallTo(() => _offerRepository.AttachAndModifyOffer(offerId, A<Action<Offer>>._, A<Action<Offer>?>._)).Invokes(
            (Guid _, 
                Action<Offer> setOptionalParameters, 
                Action<Offer>? initializeParemeters) =>
            {
                initializeParemeters?.Invoke(offer);
                setOptionalParameters.Invoke(offer);
            });

        // Act
        await _sut.SubmitServiceAsync(offerId, _iamUserId, offerType, new [] { NotificationTypeId.APP_SUBSCRIPTION_REQUEST }, _fixture.Create<IDictionary<string, IEnumerable<string>>>()).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _notificationService.CreateNotifications(A<IDictionary<string, IEnumerable<string>>>._, userId, A<IEnumerable<(string? content, NotificationTypeId notifcationTypeId)>>._, A<Guid>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _documentRepository.AttachAndModifyDocument(A<Guid>._, A<Action<Document>>._, A<Action<Document>>._)).MustHaveHappenedTwiceExactly();
        A.CallTo(() => _offerRepository.AttachAndModifyOffer(offerId, A<Action<Offer>>._, A<Action<Offer>?>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    #endregion

    #region DeclineOfferAsync

    [Theory]
    [InlineData(OfferTypeId.APP)]
    [InlineData(OfferTypeId.SERVICE)]
    public async Task DeclineOfferAsync_WithNotExistingOffer_ThrowsForbiddenExceptionException(OfferTypeId offerTypeId)
    {
        // Arrange
        var notExistingOffer = _fixture.Create<Guid>();
        A.CallTo(() => _offerRepository.GetOfferDeclineDataAsync(notExistingOffer, _iamUserId, offerTypeId))
            .ReturnsLazily(() => new ValueTuple<string? , OfferStatusId, Guid?>());

        // Act
        async Task Act() => await _sut.DeclineOfferAsync(notExistingOffer, _iamUserId, new OfferDeclineRequest("Test"), offerTypeId, NotificationTypeId.SERVICE_RELEASE_REJECTION, new Dictionary<string, IEnumerable<string>>(), string.Empty).ConfigureAwait(false);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(Act).ConfigureAwait(false);
    }

    [Theory]
    [InlineData(OfferTypeId.APP)]
    [InlineData(OfferTypeId.SERVICE)]
    public async Task DeclineOfferAsync_WithWrongStatus_ThrowsConflictException(OfferTypeId offerTypeId)
    {
        // Arrange
        var notExistingOffer = _fixture.Create<Guid>();
        A.CallTo(() => _offerRepository.GetOfferDeclineDataAsync(notExistingOffer, _iamUserId, offerTypeId))
            .ReturnsLazily(() => new ValueTuple<string?, OfferStatusId, Guid?>("test", OfferStatusId.CREATED, Guid.NewGuid()));

        // Act
        async Task Act() => await _sut.DeclineOfferAsync(notExistingOffer, _iamUserId, new OfferDeclineRequest("Test"), offerTypeId, NotificationTypeId.SERVICE_RELEASE_REJECTION, new Dictionary<string, IEnumerable<string>>(), string.Empty).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);
        ex.Message.Should().Be($"{offerTypeId} must be in status {OfferStatusId.IN_REVIEW}");
    }

    [Theory]
    [InlineData(OfferTypeId.APP)]
    [InlineData(OfferTypeId.SERVICE)]
    public async Task DeclineOfferAsync_WithOfferNameNotSet_ThrowsConflictException(OfferTypeId offerTypeId)
    {
        // Arrange
        var notExistingOffer = _fixture.Create<Guid>();
        A.CallTo(() => _offerRepository.GetOfferDeclineDataAsync(notExistingOffer, _iamUserId, offerTypeId))
            .ReturnsLazily(() => new ValueTuple<string?, OfferStatusId, Guid?>(null, OfferStatusId.IN_REVIEW, Guid.NewGuid()));

        // Act
        async Task Act() => await _sut.DeclineOfferAsync(notExistingOffer, _iamUserId, new OfferDeclineRequest("Test"), offerTypeId, NotificationTypeId.SERVICE_RELEASE_REJECTION, new Dictionary<string, IEnumerable<string>>(), string.Empty).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);
        ex.Message.Should().Be($"{offerTypeId} name is not set");
    }

    [Theory]
    [InlineData(OfferTypeId.APP)]
    [InlineData(OfferTypeId.SERVICE)]
    public async Task DeclineOfferAsync_WithProvidingCompanyNotSet_ThrowsConflictException(OfferTypeId offerTypeId)
    {
        // Arrange
        var notExistingOffer = _fixture.Create<Guid>();
        A.CallTo(() => _offerRepository.GetOfferDeclineDataAsync(notExistingOffer, _iamUserId, offerTypeId))
            .ReturnsLazily(() => new ValueTuple<string?, OfferStatusId, Guid?>("test", OfferStatusId.IN_REVIEW, null));

        // Act
        async Task Act() => await _sut.DeclineOfferAsync(notExistingOffer, _iamUserId, new OfferDeclineRequest("Test"), offerTypeId, NotificationTypeId.SERVICE_RELEASE_REJECTION, new Dictionary<string, IEnumerable<string>>(), string.Empty).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);
        ex.Message.Should().Be($"{offerTypeId} providing company is not set");
    }

    [Theory]
    [InlineData(OfferTypeId.APP)]
    [InlineData(OfferTypeId.SERVICE)]
    public async Task DeclineOfferAsync_WithValidData_CallsExpected(OfferTypeId offerTypeId)
    {
        // Arrange
        var offer = _fixture.Create<Offer>();
        var offerId = _fixture.Create<Guid>();
        var recipients = new Dictionary<string, IEnumerable<string>>(){{"Test", new []{"Abc"}}};
        var roleIds = _fixture.Create<IEnumerable<Guid>>();
        A.CallTo(() => _offerRepository.GetOfferDeclineDataAsync(offerId, _iamUserId, offerTypeId))
            .ReturnsLazily(() => new ValueTuple<string?, OfferStatusId, Guid?>("test", OfferStatusId.IN_REVIEW, Guid.NewGuid()));
        A.CallTo(() => _offerRepository.AttachAndModifyOffer(offerId, A<Action<Offer>>._, A<Action<Offer>?>._))
            .Invokes((Guid _, Action<Offer> setOptionalParameters, Action<Offer>? initializeParemeters) =>
            {
                initializeParemeters?.Invoke(offer);
                setOptionalParameters.Invoke(offer);
            });
        A.CallTo(() => _userRolesRepository.GetUserRoleIdsUntrackedAsync(A<IDictionary<string, IEnumerable<string>>>._))
            .Returns(roleIds.ToAsyncEnumerable());
        A.CallTo(() => _userRepository.GetCompanyUserEmailForCompanyAndRoleId(A<IEnumerable<Guid>>._, A<Guid>._))
            .Returns(new (string Email, string? Firstname, string? Lastname)[] {new ("test@email.com", "Test User 1", "cx-user-2")}.ToAsyncEnumerable());

        // Act
        await _sut.DeclineOfferAsync(offerId, _iamUserId, new OfferDeclineRequest("Test"), offerTypeId, NotificationTypeId.SERVICE_RELEASE_REJECTION, recipients, string.Empty).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _offerRepository.AttachAndModifyOffer(offerId, A<Action<Offer>>._, A<Action<Offer>?>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _notificationService.CreateNotifications(A<IDictionary<string, IEnumerable<string>>>._, A<Guid>._, A<IEnumerable<(string? content, NotificationTypeId notificationTypeId)>>._, A<Guid>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _mailingService.SendMails(A<string>._, A<IDictionary<string, string>>._, A<IEnumerable<string>>._)).MustHaveHappenedOnceExactly();
        offer.OfferStatusId.Should().Be(OfferStatusId.CREATED);
    }

    #endregion

    #region DeactivateOfferStatusId

    [Theory]
    [InlineData(OfferTypeId.APP)]
    public async Task DeactivateOfferStatusIdAsync_WithoutExistingAppId_ThrowsForbiddenExceptionException(OfferTypeId offerTypeId)
    {
        // Arrange
        var notExistingId = _fixture.Create<Guid>();
        A.CallTo(() => _offerRepository.GetOfferActiveStatusDataByIdAsync(notExistingId, offerTypeId, _iamUserId))
            .ReturnsLazily(() => new ValueTuple<bool, bool>());
    
        // Act
        async Task Act() => await _sut.DeactivateOfferIdAsync(notExistingId,_iamUserId, offerTypeId).ConfigureAwait(false);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(Act).ConfigureAwait(false);
    }

    [Theory]
    [InlineData(OfferTypeId.APP)]
    public async Task DeactivateOfferStatusIdAsync_WithNotAssignedUser_ThrowsForbiddenException(OfferTypeId offerTypeId)
    {
        // Arrange
        var appid = _fixture.Create<Guid>();
        A.CallTo(() => _offerRepository.GetOfferActiveStatusDataByIdAsync(appid, offerTypeId, _iamUserId))
            .ReturnsLazily(() => new ValueTuple<bool, bool>(true, false));
    
        // Act
        async Task Act() => await _sut.DeactivateOfferIdAsync(appid, _iamUserId, offerTypeId).ConfigureAwait(false);
        
        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act).ConfigureAwait(false);
        ex.Message.Should().Be("Missing permission: The user's company does not provide the requested app so they cannot deactivate it.");

    }

    [Theory]
    [InlineData(OfferTypeId.APP)]
    public async Task DeactivateOfferStatusIdAsync_WithNotOfferStatusId_ThrowsConflictException(OfferTypeId offerTypeId)
    {
        // Arrange
        var appid = _fixture.Create<Guid>();
        A.CallTo(() => _offerRepository.GetOfferActiveStatusDataByIdAsync(appid, offerTypeId, _iamUserId))
            .ReturnsLazily(() => new ValueTuple<bool, bool>(false, true));
    
        // Act
        async Task Act() => await _sut.DeactivateOfferIdAsync(appid,_iamUserId, offerTypeId).ConfigureAwait(false);
        
        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);
        ex.Message.Should().Be("offerStatus is in Incorrect State");

    }

    [Theory]
    [InlineData(OfferTypeId.APP)]
    public async Task DeactivateOfferStatusIdAsync_WithValidData_CallsExpected(OfferTypeId offerTypeId)
    {
        // Arrange
        var offer = _fixture.Create<Offer>();
        var appid = _fixture.Create<Guid>();
        A.CallTo(() => _offerRepository.GetOfferActiveStatusDataByIdAsync(appid, offerTypeId, _iamUserId))
            .ReturnsLazily(() => new ValueTuple<bool, bool>(true, true));
        
        A.CallTo(() => _offerRepository.AttachAndModifyOffer(appid, A<Action<Offer>>._, A<Action<Offer>?>._))
            .Invokes((Guid _, Action<Offer> setOptionalParameters, Action<Offer>? initializeParemeters) =>
            {
                initializeParemeters?.Invoke(offer);
                setOptionalParameters.Invoke(offer);
            });

        // Act
        await _sut.DeactivateOfferIdAsync(appid,_iamUserId, offerTypeId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _offerRepository.AttachAndModifyOffer(appid, A<Action<Offer>>._, A<Action<Offer>?>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        offer.OfferStatusId.Should().Be(OfferStatusId.INACTIVE);
    }
    
    #endregion

    #region UploadDocument
    
    [Theory]
    [InlineData(OfferTypeId.APP, DocumentTypeId.APP_CONTRACT, new[] { DocumentTypeId.APP_CONTRACT }, new [] { "application/pdf" })]
    [InlineData(OfferTypeId.SERVICE, DocumentTypeId.ADDITIONAL_DETAILS, new[] { DocumentTypeId.ADDITIONAL_DETAILS }, new [] { "application/pdf" })]
    public async Task UploadDocumentAsync_WithValidData_CallsExpected(OfferTypeId offerTypeId, DocumentTypeId documentTypeId, IEnumerable<DocumentTypeId> documentTypeIdSettings, IEnumerable<string> contentTypeSettings)
    {
        // Arrange
        var id = _fixture.Create<Guid>();
        var documentId = _fixture.Create<Guid>();
        var file = FormFileHelper.GetFormFile("this is just a test", "superFile.pdf", "application/pdf");
        var documents = new List<Document>();
        var offerAssignedDocuments = new List<OfferAssignedDocument>();
        SetupCreateDocument(id, offerTypeId);
        A.CallTo(() => _documentRepository.CreateDocument(A<string>._, A<byte[]>._, A<byte[]>._, A<MediaTypeId>._, A<DocumentTypeId>._,A<Action<Document>?>._))
            .Invokes((string documentName, byte[] documentContent, byte[] hash, MediaTypeId mediaTypeId, DocumentTypeId documentTypeId, Action<Document>? setupOptionalFields) =>
            {
                var document = new Document(documentId, documentContent, hash, documentName, mediaTypeId, DateTimeOffset.UtcNow, DocumentStatusId.PENDING, documentTypeId);
                setupOptionalFields?.Invoke(document);
                documents.Add(document);
            });
        A.CallTo(() => _offerRepository.CreateOfferAssignedDocument(A<Guid>._, A<Guid>._))
            .Invokes((Guid offerId, Guid docId) =>
            {
                var offerAssignedDocument = new OfferAssignedDocument(offerId, docId);
                offerAssignedDocuments.Add(offerAssignedDocument);
            });

        // Act
        await _sut.UploadDocumentAsync(id, documentTypeId, file, _iamUser.UserEntityId, offerTypeId, documentTypeIdSettings, contentTypeSettings, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        documents.Should().HaveCount(1);
        offerAssignedDocuments.Should().HaveCount(1);
    }

    [Theory]
    [InlineData(OfferTypeId.APP, DocumentTypeId.APP_CONTRACT, new[] { DocumentTypeId.APP_CONTRACT }, new [] { "application/pdf" })]
    [InlineData(OfferTypeId.SERVICE, DocumentTypeId.ADDITIONAL_DETAILS, new[] { DocumentTypeId.ADDITIONAL_DETAILS }, new [] { "application/pdf" })]
    public async Task UploadDocumentAsync_InValidData_ThrowsNotFoundException(OfferTypeId offerTypeId, DocumentTypeId documentTypeId, IEnumerable<DocumentTypeId> documentTypeIdSettings, IEnumerable<string> contentTypeSettings)
    {
        // Arrange
        var id = _fixture.Create<Guid>();
        var file = FormFileHelper.GetFormFile("this is just a test", "superFile.pdf", "application/pdf");
        A.CallTo(() => _offerRepository.GetProviderCompanyUserIdForOfferUntrackedAsync(id, _iamUser.UserEntityId, OfferStatusId.CREATED, offerTypeId))
            .ReturnsLazily(() => new ValueTuple<bool,bool,Guid>());

        // Act
        async Task Act() => await _sut.UploadDocumentAsync(id, documentTypeId, file, _iamUser.UserEntityId, offerTypeId, documentTypeIdSettings, contentTypeSettings, CancellationToken.None).ConfigureAwait(false);

        // Arrange
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act).ConfigureAwait(false);
        ex.Message.Should().Be($"{offerTypeId} {id} does not exist");
    }

    [Theory]
    [InlineData(OfferTypeId.APP, DocumentTypeId.APP_CONTRACT, new[] { DocumentTypeId.APP_CONTRACT }, new [] { "application/pdf" })]
    [InlineData(OfferTypeId.SERVICE, DocumentTypeId.ADDITIONAL_DETAILS, new[] { DocumentTypeId.ADDITIONAL_DETAILS }, new [] { "application/pdf" })]
    public async Task UploadDocumentAsync_InValidData_ThrowsForbiddenException(OfferTypeId offerTypeId, DocumentTypeId documentTypeId, IEnumerable<DocumentTypeId> documentTypeIdSettings, IEnumerable<string> contentTypeSettings)
    {
        // Arrange
        var id = _fixture.Create<Guid>();
        var file = FormFileHelper.GetFormFile("this is just a test", "superFile.pdf", "application/pdf");
        A.CallTo(() => _offerRepository.GetProviderCompanyUserIdForOfferUntrackedAsync(id, _iamUser.UserEntityId, OfferStatusId.CREATED, offerTypeId))
            .ReturnsLazily(() => (true, true, Guid.Empty));

        // Act
        async Task Act() => await _sut.UploadDocumentAsync(id, documentTypeId, file, _iamUser.UserEntityId, offerTypeId, documentTypeIdSettings, contentTypeSettings, CancellationToken.None).ConfigureAwait(false);

        // Arrange
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act).ConfigureAwait(false);
        ex.Message.Should().Be($"user {_iamUser.UserEntityId} is not a member of the providercompany of {offerTypeId} {id}");
    }

    [Theory]
    [InlineData(OfferTypeId.APP, DocumentTypeId.APP_CONTRACT, new[] { DocumentTypeId.APP_CONTRACT }, new [] { "application/pdf" })]
    [InlineData(OfferTypeId.SERVICE, DocumentTypeId.ADDITIONAL_DETAILS, new[] { DocumentTypeId.ADDITIONAL_DETAILS }, new [] { "application/pdf" })]
    public async Task UploadDocumentAsync_EmptyId_ThrowsControllerArgumentException(OfferTypeId offerTypeId, DocumentTypeId documentTypeId, IEnumerable<DocumentTypeId> documentTypeIdSettings, IEnumerable<string> contentTypeSettings)
    {
        // Arrange
        var file = FormFileHelper.GetFormFile("this is just a test", "superFile.pdf", "application/pdf");

        // Act
        async Task Act() => await _sut.UploadDocumentAsync(Guid.Empty, documentTypeId, file, _iamUser.UserEntityId, offerTypeId, documentTypeIdSettings, contentTypeSettings, CancellationToken.None).ConfigureAwait(false);

        // Arrange
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);
        ex.Message.Should().Be($"{offerTypeId}id should not be null");
    }

    [Theory]
    [InlineData(OfferTypeId.APP, DocumentTypeId.APP_CONTRACT, new[] { DocumentTypeId.APP_CONTRACT }, new [] { "application/pdf" })]
    [InlineData(OfferTypeId.SERVICE, DocumentTypeId.ADDITIONAL_DETAILS, new[] { DocumentTypeId.ADDITIONAL_DETAILS }, new [] { "application/pdf" })]
    public async Task UploadDocumentAsync_EmptyFileName_ThrowsControllerArgumentException(OfferTypeId offerTypeId, DocumentTypeId documentTypeId, IEnumerable<DocumentTypeId> documentTypeIdSettings, IEnumerable<string> contentTypeSettings)
    {
        // Arrange
        var id = _fixture.Create<Guid>();
        var file = FormFileHelper.GetFormFile("this is just a test", "", "application/pdf");

        // Act
        async Task Act() => await _sut.UploadDocumentAsync(id, documentTypeId, file, _iamUser.UserEntityId, offerTypeId, documentTypeIdSettings, contentTypeSettings, CancellationToken.None).ConfigureAwait(false);

        // Arrange
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);
        ex.Message.Should().Be($"File name should not be null");
    }

    [Theory]
    [InlineData(OfferTypeId.APP, DocumentTypeId.APP_CONTRACT, new[] { DocumentTypeId.APP_CONTRACT }, new [] { "application/pdf" })]
    [InlineData(OfferTypeId.SERVICE, DocumentTypeId.ADDITIONAL_DETAILS, new[] { DocumentTypeId.ADDITIONAL_DETAILS }, new [] { "application/pdf" })]
    public async Task UploadDocumentAsync_contentType_ThrowsUnsupportedMediaTypeException(OfferTypeId offerTypeId, DocumentTypeId documentTypeId, IEnumerable<DocumentTypeId> documentTypeIdSettings, IEnumerable<string> contentTypeSettings)
    {
        // Arrange
        var id = _fixture.Create<Guid>();
        var file = FormFileHelper.GetFormFile("this is just a test", "TestFile.txt", "text/csv");

        // Act
        async Task Act() => await _sut.UploadDocumentAsync(id, documentTypeId, file, _iamUser.UserEntityId, offerTypeId, documentTypeIdSettings, contentTypeSettings, CancellationToken.None).ConfigureAwait(false);

        // Arrange
        var ex = await Assert.ThrowsAsync<UnsupportedMediaTypeException>(Act).ConfigureAwait(false);
        ex.Message.Should().Be($"Document type not supported. File with contentType :{string.Join(",", contentTypeSettings)} are allowed.");
    }

    [Theory]
    [InlineData(OfferTypeId.APP, DocumentTypeId.SELF_DESCRIPTION, new[] { DocumentTypeId.APP_CONTRACT }, new [] { "application/pdf" })]
    [InlineData(OfferTypeId.SERVICE, DocumentTypeId.APP_TECHNICAL_INFORMATION, new[] { DocumentTypeId.ADDITIONAL_DETAILS }, new [] { "application/pdf" })]
    public async Task UploadDocumentAsync_documentType_ThrowsControllerArgumentException(OfferTypeId offerTypeId, DocumentTypeId documentTypeId, IEnumerable<DocumentTypeId> documentTypeIdSettings, IEnumerable<string> contentTypeSettings)
    {
        // Arrange
        var id = _fixture.Create<Guid>();
        var file = FormFileHelper.GetFormFile("this is just a test", "superFile.pdf", "application/pdf");

        // Act
        async Task Act() => await _sut.UploadDocumentAsync(id, documentTypeId, file, _iamUser.UserEntityId, offerTypeId, documentTypeIdSettings, contentTypeSettings, CancellationToken.None).ConfigureAwait(false);

        // Arrange
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);
        ex.Message.Should().Be($"documentType must be either: {string.Join(",", documentTypeIdSettings)}");
    }

    [Theory]
    [InlineData(OfferTypeId.APP, DocumentTypeId.APP_CONTRACT, new[] { DocumentTypeId.APP_CONTRACT }, new [] { "application/pdf" })]
    [InlineData(OfferTypeId.SERVICE, DocumentTypeId.ADDITIONAL_DETAILS, new[] { DocumentTypeId.ADDITIONAL_DETAILS }, new [] { "application/pdf" })]
    public async Task UploadDocumentAsync_isStatusCreated_ThrowsConflictException(OfferTypeId offerTypeId, DocumentTypeId documentTypeId, IEnumerable<DocumentTypeId> documentTypeIdSettings, IEnumerable<string> contentTypeSettings)
    {
        // Arrange
        var id = _fixture.Create<Guid>();
        var file = FormFileHelper.GetFormFile("this is just a test", "superFile.pdf", "application/pdf");
        A.CallTo(() => _offerRepository.GetProviderCompanyUserIdForOfferUntrackedAsync(id, _iamUser.UserEntityId, OfferStatusId.CREATED, offerTypeId))
            .ReturnsLazily(() => (true, false, Guid.NewGuid()));

        // Act
        async Task Act() => await _sut.UploadDocumentAsync(id, documentTypeId, file, _iamUser.UserEntityId, offerTypeId, documentTypeIdSettings, contentTypeSettings, CancellationToken.None).ConfigureAwait(false);

        // Arrange
        var ex = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);
        ex.Message.Should().Be($"offerStatus is in Incorrect State");
    }

    #endregion
    
    #region GetProviderOfferAgreementConsentById_ReturnExpectedResult
    
    [Fact]
    public async Task GetProviderOfferAgreementConsentById_ReturnExpectedResult()
    {
        //Arrange
        var data = _fixture.Create<OfferAgreementConsent>();
        var serviceId = Guid.NewGuid();
        A.CallTo(() => _agreementRepository.GetOfferAgreementConsentById(A<Guid>._,A<string>._,OfferTypeId.SERVICE))
            .Returns((data,true));

        // Act
        var result = await _sut.GetProviderOfferAgreementConsentById(serviceId, _iamUserId, OfferTypeId.SERVICE).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _agreementRepository.GetOfferAgreementConsentById(serviceId, _iamUserId, OfferTypeId.SERVICE))
            .MustHaveHappenedOnceExactly();
        result.Should().Be(data);
    }

    [Fact]
    public async Task GetProviderOfferAgreementConsentById_WithInvalidUserProviderCompany_ThrowsForbiddenException()
    {
        //Arrange
        var data = _fixture.Create<OfferAgreementConsent>();
        var serviceId = Guid.NewGuid();
        A.CallTo(() => _agreementRepository.GetOfferAgreementConsentById(A<Guid>._,A<string>._,OfferTypeId.SERVICE))
            .Returns((data,false));

         // Act
        async Task Act() => await _sut.GetProviderOfferAgreementConsentById(serviceId, _iamUserId, OfferTypeId.SERVICE).ConfigureAwait(false);

        // Arrange
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act).ConfigureAwait(false);
        ex.Message.Should().Be($"UserId {_iamUserId} is not assigned with Offer {serviceId}");
    }
    
    [Fact]
    public async Task GetProviderOfferAgreementConsentById_WithInvalidOfferId_ThrowsNotFoundException()
    {
        //Arrange
        var serviceId = Guid.NewGuid();

        A.CallTo(() => _agreementRepository.GetOfferAgreementConsentById(A<Guid>._,A<string>._,OfferTypeId.SERVICE))
            .Returns(new ValueTuple<OfferAgreementConsent, bool>());

         // Act
        async Task Act() => await _sut.GetProviderOfferAgreementConsentById(serviceId, _iamUserId, OfferTypeId.SERVICE).ConfigureAwait(false);

        // Arrange
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act).ConfigureAwait(false);
        ex.Message.Should().Be($"offer {serviceId}, offertype {OfferTypeId.SERVICE} does not exist");
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
        
        A.CallTo(() => _userRepository.GetCompanyUserWithIamUserCheckAndCompanyName(_iamUserId, _companyUser.Id))
            .ReturnsLazily(() => new List<(Guid CompanyUserId, bool IsIamUser, string CompanyUserName, Guid CompanyId)>{new (this._companyUser.Id, true, "COMPANYBPN", _companyUserCompanyId), new (this._companyUser.Id, false, "OTHERCOMPANYBPN", _companyUserCompanyId)}.ToAsyncEnumerable());
        A.CallTo(() => _userRepository.GetCompanyUserWithIamUserCheckAndCompanyName(_iamUserId, A<Guid>.That.Not.Matches(x => x == _companyUser.Id)))
            .ReturnsLazily(() => new List<(Guid CompanyUserId, bool IsIamUser, string CompanyUserName, Guid CompanyId)>{new (this._companyUser.Id, true, "COMPANYBPN", _companyUserCompanyId)}.ToAsyncEnumerable());
        A.CallTo(() => _userRepository.GetCompanyUserWithIamUserCheckAndCompanyName(A<string>.That.Not.Matches(x => x == _iamUserId), _companyUser.Id))
            .ReturnsLazily(() => new List<(Guid CompanyUserId, bool IsIamUser, string CompanyUserName, Guid CompanyId)>{new (this._companyUser.Id, false, "OTHERCOMPANYBPN", _companyUserCompanyId)}.ToAsyncEnumerable());
        A.CallTo(() => _userRepository.GetCompanyUserWithIamUserCheckAndCompanyName(A<string>.That.Not.Matches(x => x == _iamUserId), A<Guid>.That.Not.Matches(x => x == _companyUser.Id)))
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
        
        A.CallTo(() => _languageRepository.GetLanguageCodesUntrackedAsync(A<IEnumerable<string>>.That.Matches(x => x.Count() == 1 && x.All(y => y == "en"))))
            .Returns(new List<string> { "en" }.ToAsyncEnumerable());
        A.CallTo(() => _languageRepository.GetLanguageCodesUntrackedAsync(A<IEnumerable<string>>.That.Matches(x => x.Count() == 1 && x.All(y => y == "gg"))))
            .Returns(new List<string>().ToAsyncEnumerable());

        A.CallTo(() => _portalRepositories.GetInstance<IAgreementRepository>()).Returns(_agreementRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IConsentRepository>()).Returns(_consentRepository);        
        A.CallTo(() => _portalRepositories.GetInstance<IOfferRepository>()).Returns(_offerRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IOfferSubscriptionsRepository>()).Returns(_offerSubscriptionsRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IConsentAssignedOfferSubscriptionRepository>()).Returns(_consentAssignedOfferSubscriptionRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRolesRepository>()).Returns(_userRolesRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ILanguageRepository>()).Returns(_languageRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IDocumentRepository>()).Returns(_documentRepository);
        _fixture.Inject(_portalRepositories);
    }

    private void SetupServices()
    {
        A.CallTo(() => _notificationService.CreateNotifications(A<IDictionary<string, IEnumerable<string>>>._,
                A<Guid>._, A<IEnumerable<(string?, NotificationTypeId)>>._, A<Guid>._))
            .ReturnsLazily(() => Task.CompletedTask);
    }

    private void SetupCreateDocument(Guid appId, OfferTypeId offerTypeId)
    {
        A.CallTo(() => _offerRepository.GetProviderCompanyUserIdForOfferUntrackedAsync(appId, _iamUser.UserEntityId, OfferStatusId.CREATED, offerTypeId))
            .ReturnsLazily(() => (true, true, _companyUser.Id));
        A.CallTo(() => _portalRepositories.GetInstance<IOfferRepository>()).Returns(_offerRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IDocumentRepository>()).Returns(_documentRepository);
    }

    #endregion
}
