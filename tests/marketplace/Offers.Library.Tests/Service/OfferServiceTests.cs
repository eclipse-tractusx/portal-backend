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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Mailing.SendMail;
using Org.Eclipse.TractusX.Portal.Backend.Notifications.Library;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Tests.Service;

public class OfferServiceTests
{
    private const string IamUserId = "3e8343f7-4fe5-4296-8312-f33aa6dbde5d";
    private static readonly Guid CompanyUserCompanyId = new("395f955b-f11b-4a74-ab51-92a526c1973a");
    private readonly IdentityData _identity = new(IamUserId, Guid.NewGuid(), IdentityTypeId.COMPANY_USER, CompanyUserCompanyId);
    private readonly Guid _existingServiceId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47661");
    private readonly Guid _existingAgreementId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47664");
    private readonly Guid _validConsentId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47664");
    private readonly Guid _validUserRoleId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47665");
    private readonly Guid _existingAgreementForSubscriptionId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47665");
    private readonly Guid _differentCompanyUserId = Guid.NewGuid();
    private readonly Guid _noSalesManagerUserId = Guid.NewGuid();
    private readonly Guid _validDocumentId = Guid.NewGuid();

    private readonly CompanyUser _companyUser;
    private readonly IFixture _fixture;
    private readonly IAgreementRepository _agreementRepository;
    private readonly IConsentRepository _consentRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IOfferSubscriptionsRepository _offerSubscriptionsRepository;
    private readonly IConsentAssignedOfferSubscriptionRepository _consentAssignedOfferSubscriptionRepository;
    private readonly IPortalRepositories _portalRepositories;
    private readonly INotificationService _notificationService;
    private readonly IAsyncEnumerator<Guid> _createNotificationsEnumerator;
    private readonly IOfferRepository _offerRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserRolesRepository _userRolesRepository;
    private readonly ILanguageRepository _languageRepository;
    private readonly IMailingService _mailingService;
    private readonly IDocumentRepository _documentRepository;
    private readonly OfferService _sut;
    private readonly IOfferSetupService _offerSetupService;
    private readonly ITechnicalUserProfileRepository _technicalUserProfileRepository;

    public OfferServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        var identity = new Identity(Guid.NewGuid(), DateTimeOffset.UtcNow, CompanyUserCompanyId, UserStatusId.ACTIVE, IdentityTypeId.COMPANY_USER)
        {
            UserEntityId = IamUserId
        };

        _companyUser = _fixture.Build<CompanyUser>()
            .With(u => u.Identity, identity)
            .Create();
        _portalRepositories = A.Fake<IPortalRepositories>();
        _agreementRepository = A.Fake<IAgreementRepository>();
        _consentRepository = A.Fake<IConsentRepository>();
        _companyRepository = A.Fake<ICompanyRepository>();
        _offerSubscriptionsRepository = A.Fake<IOfferSubscriptionsRepository>();
        _consentAssignedOfferSubscriptionRepository = A.Fake<IConsentAssignedOfferSubscriptionRepository>();
        _offerRepository = A.Fake<IOfferRepository>();
        _userRepository = A.Fake<IUserRepository>();
        _userRolesRepository = A.Fake<IUserRolesRepository>();
        _languageRepository = A.Fake<ILanguageRepository>();
        _technicalUserProfileRepository = A.Fake<ITechnicalUserProfileRepository>();
        _notificationService = A.Fake<INotificationService>();
        _mailingService = A.Fake<IMailingService>();
        _documentRepository = A.Fake<IDocumentRepository>();
        _offerSetupService = A.Fake<IOfferSetupService>();

        _sut = new OfferService(_portalRepositories, _notificationService, _mailingService, _offerSetupService);

        SetupRepositories();
        _createNotificationsEnumerator = SetupServices();
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
        var result = await _sut.CreateServiceOfferingAsync(new ServiceOfferingData("Newest Service", "42", "mail@test.de", _identity.UserId, Enumerable.Empty<LocalizedDescription>(), new[] { ServiceTypeId.DATASPACE_SERVICE }, "http://google.com"), (_identity.UserId, _identity.CompanyId), OfferTypeId.SERVICE);

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
        var serviceOfferingData = new ServiceOfferingData("Newest Service", "42", "mail@test.de", _identity.UserId, new LocalizedDescription[]
        {
            new ("en", "That's a description with a valid language code", "Short description")
        },
        new[]
        {
            ServiceTypeId.DATASPACE_SERVICE
        }, "http://google.com");
        var result = await _sut.CreateServiceOfferingAsync(serviceOfferingData, (_identity.UserId, _identity.CompanyId), OfferTypeId.SERVICE);

        // Assert
        result.Should().Be(serviceId);
        apps.Should().HaveCount(1);
        A.CallTo(() => _offerRepository.AddServiceAssignedServiceTypes(A<IEnumerable<(Guid serviceId, ServiceTypeId serviceTypeId)>>.That.Matches(s => s.Any(x => x.serviceId == serviceId)))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.AddOfferDescriptions(A<IEnumerable<(Guid offerId, string languageShortName, string descriptionLong, string descriptionShort)>>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CreateServiceOffering_WithoutServiceTypeId_ThrowsException()
    {
        // Act
        var serviceTypeIds = Enumerable.Empty<ServiceTypeId>();
        async Task Action() => await _sut.CreateServiceOfferingAsync(new ServiceOfferingData("Newest Service", "42", "mail@test.de", _companyUser.Id, Enumerable.Empty<LocalizedDescription>(), serviceTypeIds, "http://google.com"), _fixture.Create<ValueTuple<Guid, Guid>>(), OfferTypeId.SERVICE);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("ServiceTypeIds");
    }

    [Fact]
    public async Task CreateServiceOffering_WithoutTitle_ThrowsException()
    {
        // Act
        async Task Action() => await _sut.CreateServiceOfferingAsync(new ServiceOfferingData("", "42", "mail@test.de", _companyUser.Id, Enumerable.Empty<LocalizedDescription>(), new[] { ServiceTypeId.DATASPACE_SERVICE }, "http://google.com"), _fixture.Create<ValueTuple<Guid, Guid>>(), OfferTypeId.SERVICE);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("Title");
    }

    [Fact]
    public async Task CreateServiceOffering_WithInvalidLanguage_ThrowsException()
    {
        // Act
        var serviceOfferingData = new ServiceOfferingData("Newest Service", "42", "mail@test.de", _identity.UserId, new LocalizedDescription[]
        {
            new ("gg", "That's a description with incorrect language short code", "Short description")
        }, new[] { ServiceTypeId.DATASPACE_SERVICE }, "http://google.com");
        async Task Action() => await _sut.CreateServiceOfferingAsync(serviceOfferingData, (_identity.UserId, _identity.CompanyId), OfferTypeId.SERVICE);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("languageCodes");
    }

    [Fact]
    public async Task CreateServiceOffering_WithoutCompanyUser_ThrowsException()
    {
        // Act
        async Task Action() => await _sut.CreateServiceOfferingAsync(new ServiceOfferingData("Newest Service", "42", "mail@test.de", Guid.NewGuid(), Enumerable.Empty<LocalizedDescription>(), new[] { ServiceTypeId.DATASPACE_SERVICE }, "http://google.com"), (_identity.UserId, _identity.CompanyId), OfferTypeId.SERVICE);

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
            .Returns(new Consent(consentId, Guid.Empty, Guid.Empty, Guid.Empty, default, default)
            {
                ConsentStatusId = statusId
            });

        // Act
        var result = await _sut.CreateOfferSubscriptionAgreementConsentAsync(_existingServiceId, _existingAgreementId, statusId, _identity, OfferTypeId.SERVICE);

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
        async Task Action() => await _sut.CreateOfferSubscriptionAgreementConsentAsync(_existingServiceId, nonExistingAgreementId, ConsentStatusId.ACTIVE, _identity, OfferTypeId.SERVICE);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.Message.Should().Be($"Invalid Agreement {nonExistingAgreementId} for subscription {_existingServiceId} (Parameter 'agreementId')");
    }

    [Fact]
    public async Task CreateOfferAgreementConsentAsync_WithWrongUser_ThrowsException()
    {
        var identity = _fixture.Create<IdentityData>();

        // Act
        async Task Action() => await _sut.CreateOfferSubscriptionAgreementConsentAsync(_existingServiceId, _existingAgreementId, ConsentStatusId.ACTIVE, identity, OfferTypeId.SERVICE);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("UserEntityId");
    }

    [Fact]
    public async Task CreateOfferAgreementConsentAsync_WithNotExistingService_ThrowsException()
    {
        // Arrange
        var notExistingServiceId = Guid.NewGuid();

        // Act
        async Task Action() => await _sut.CreateOfferSubscriptionAgreementConsentAsync(notExistingServiceId, _existingAgreementId, ConsentStatusId.ACTIVE, _identity, OfferTypeId.SERVICE);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Action);
        ex.Message.Should().Be($"Invalid OfferSubscription {notExistingServiceId} for OfferType SERVICE");
    }

    [Fact]
    public async Task CreateOfferAgreementConsentAsync_WithInvalidOfferType_ThrowsException()
    {
        // Act
        async Task Action() => await _sut.CreateOfferSubscriptionAgreementConsentAsync(_existingServiceId, _existingAgreementId, ConsentStatusId.ACTIVE, _identity, OfferTypeId.APP);

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
        var data = new OfferAgreementConsentData[]
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
            .Returns(new Consent(consentId, Guid.Empty, Guid.Empty, Guid.Empty, default, default)
            {
                ConsentStatusId = statusId
            });

        // Act
        await _sut.CreateOrUpdateOfferSubscriptionAgreementConsentAsync(_existingServiceId, data, _identity, OfferTypeId.SERVICE);

        // Assert
        consents.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateOrUpdateServiceAgreementConsentAsync_WithNotExistingAgreement_ThrowsException()
    {
        // Arrange
        var nonExistingAgreementId = Guid.NewGuid();
        var data = new OfferAgreementConsentData[]
        {
            new(nonExistingAgreementId, ConsentStatusId.ACTIVE)
        };

        // Act
        async Task Action() => await _sut.CreateOrUpdateOfferSubscriptionAgreementConsentAsync(_existingServiceId, data, _identity, OfferTypeId.SERVICE);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.Message.Should().Be($"Invalid Agreements for subscription {_existingServiceId} (Parameter 'offerAgreementConsentData')");
    }

    [Fact]
    public async Task CreateOrUpdateServiceAgreementConsentAsync_WithWrongUser_ThrowsException()
    {
        // Arrange
        var identity = _fixture.Create<IdentityData>();
        var data = new OfferAgreementConsentData[]
        {
            new(_existingAgreementId, ConsentStatusId.ACTIVE)
        };

        // Act
        async Task Action() => await _sut.CreateOrUpdateOfferSubscriptionAgreementConsentAsync(_existingServiceId, data, identity, OfferTypeId.SERVICE);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("UserEntityId");
    }

    [Fact]
    public async Task CreateOrUpdateServiceAgreementConsentAsync_WithNotExistingService_ThrowsException()
    {
        // Arrange
        var notExistingServiceId = Guid.NewGuid();
        var data = new OfferAgreementConsentData[]
        {
            new(_existingAgreementId, ConsentStatusId.ACTIVE)
        };

        // Act
        async Task Action() => await _sut.CreateOrUpdateOfferSubscriptionAgreementConsentAsync(notExistingServiceId, data,
            _identity, OfferTypeId.SERVICE);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Action);
        ex.Message.Should().Be($"Invalid OfferSubscription {notExistingServiceId} for OfferType SERVICE");
    }

    [Fact]
    public async Task CreateOrUpdateServiceAgreementConsentAsync_WithInvalidOfferType_ThrowsException()
    {
        // Arrange
        var data = new OfferAgreementConsentData[]
        {
            new(_existingAgreementId, ConsentStatusId.ACTIVE)
        };

        // Act
        async Task Action() => await _sut.CreateOrUpdateOfferSubscriptionAgreementConsentAsync(_existingServiceId, data,
            _identity, OfferTypeId.APP);

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
        async Task Act() => await _sut.ValidateSalesManager(Guid.NewGuid(), _identity.CompanyId, Enumerable.Empty<UserRoleConfig>()).ConfigureAwait(false);

        // Assert
        var error = await Assert.ThrowsAsync<ForbiddenException>(Act).ConfigureAwait(false);
        error.Message.Should().Be($"SalesManger is not a member of the company {_identity.CompanyId}");
    }

    [Fact]
    public async Task AddAppAsync_WithUserFromOtherCompany_ThrowsException()
    {
        // Arrange
        SetupValidateSalesManager();

        // Act
        async Task Act() => await _sut.ValidateSalesManager(_differentCompanyUserId, _identity.CompanyId, Enumerable.Empty<UserRoleConfig>()).ConfigureAwait(false);

        // Assert
        var error = await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);
        error.Message.Should().Be($"User {_differentCompanyUserId} does not have sales Manager Role (Parameter 'salesManagerId')");
    }

    [Fact]
    public async Task AddAppAsync_WithUserWithoutSalesManagerRole_ThrowsException()
    {
        // Arrange
        SetupValidateSalesManager();

        // Act
        async Task Act() => await _sut.ValidateSalesManager(_noSalesManagerUserId, _identity.CompanyId, Enumerable.Empty<UserRoleConfig>()).ConfigureAwait(false);

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
        var seed = new Dictionary<(Guid, string), OfferDescription>() {
            {(seedOfferId, "de"), new OfferDescription(seedOfferId, "de", _fixture.Create<string>(), _fixture.Create<string>())},
            {(seedOfferId, "en"), new OfferDescription(seedOfferId, "en", _fixture.Create<string>(), _fixture.Create<string>())},
            {(seedOfferId, "fr"), new OfferDescription(seedOfferId, "fr", _fixture.Create<string>(), _fixture.Create<string>())},
            {(seedOfferId, "cz"), new OfferDescription(seedOfferId, "cz", _fixture.Create<string>(), _fixture.Create<string>())},
            {(seedOfferId, "it"), new OfferDescription(seedOfferId, "it", _fixture.Create<string>(), _fixture.Create<string>())},
        };

        var updateDescriptions = new[] {
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
        A.CallTo(() => _offerRepository.CreateUpdateDeleteOfferDescriptions(A<Guid>._, A<IEnumerable<LocalizedDescription>>._
            , A<IEnumerable<(string, string, string)>>._)).MustHaveHappened();
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
        A.CallTo(() => _offerRepository.GetOfferReleaseDataByIdAsync(notExistingOffer, offerType)).Returns((OfferReleaseData?)null);

        // Act
        async Task Act() => await _sut.SubmitOfferAsync(notExistingOffer, _identity.UserId, offerType, new[] { NotificationTypeId.APP_SUBSCRIPTION_REQUEST }, Enumerable.Empty<UserRoleConfig>(), new[] { DocumentTypeId.CONFORMITY_APPROVAL_BUSINESS_APPS }).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act).ConfigureAwait(false);
        ex.Message.Should().Be($"{offerType} {notExistingOffer} does not exist");
    }

    [Theory]
    [InlineData(null, "c8d4d854-8ac6-425f-bc5a-dbf457670732", false, false, true)]
    [InlineData("name", null, false, false, true)]
    [InlineData("name", "c8d4d854-8ac6-425f-bc5a-dbf457670732", true, false, true)]
    [InlineData("name", "c8d4d854-8ac6-425f-bc5a-dbf457670732", false, true, true)]
    [InlineData("name", "c8d4d854-8ac6-425f-bc5a-dbf457670732", true, true, false)]
    public async Task SubmitOffer_WithInvalidOffer_ThrowsConflictException(string? name, string? providerCompanyId, bool isDescriptionLongNotSet, bool isDescriptionShortNotSet, bool hasUserRoles)
    {
        // Arrange
        A.CallTo(() => _offerRepository.GetOfferReleaseDataByIdAsync(A<Guid>._, A<OfferTypeId>._)).Returns(new OfferReleaseData(name, providerCompanyId == null ? null : new Guid(providerCompanyId), _fixture.Create<string>(), isDescriptionLongNotSet, isDescriptionShortNotSet, hasUserRoles, true, true, new[] { (Guid.NewGuid(), DocumentStatusId.PENDING, DocumentTypeId.CONFORMITY_APPROVAL_BUSINESS_APPS) }));

        // Act
        async Task Act() => await _sut.SubmitOfferAsync(Guid.NewGuid(), _identity.UserId, _fixture.Create<OfferTypeId>(), _fixture.CreateMany<NotificationTypeId>(1), _fixture.CreateMany<UserRoleConfig>(), new[] { DocumentTypeId.CONFORMITY_APPROVAL_BUSINESS_APPS }).ConfigureAwait(false);

        // Assert
        var result = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);
        if (hasUserRoles)
        {
            result.Message.Should().StartWith("Missing ");
        }
        else
        {
            result.Message.Should().Be("The app has no roles assigned");
        }
    }

    [Fact]
    public async Task SubmitOffer_WithInvalidDocumentType_ThrowsConflictException()
    {
        // Arrange
        var data = _fixture.Build<OfferReleaseData>()
            .With(x => x.IsDescriptionLongNotSet, false)
            .With(x => x.IsDescriptionShortNotSet, false)
            .With(x => x.DocumentDatas, new[] { (Guid.NewGuid(), DocumentStatusId.PENDING, DocumentTypeId.CONFORMITY_APPROVAL_BUSINESS_APPS) })
            .Create();
        var submitAppDocumentTypeIds = new[] { DocumentTypeId.APP_IMAGE, DocumentTypeId.APP_LEADIMAGE, DocumentTypeId.CONFORMITY_APPROVAL_BUSINESS_APPS };
        var missingAppDocumentTypeIds = new[] { DocumentTypeId.APP_IMAGE, DocumentTypeId.APP_LEADIMAGE };
        A.CallTo(() => _offerRepository.GetOfferReleaseDataByIdAsync(A<Guid>._, A<OfferTypeId>._)).Returns(data);

        // Act
        async Task Act() => await _sut.SubmitOfferAsync(Guid.NewGuid(), _identity.UserId, _fixture.Create<OfferTypeId>(), _fixture.CreateMany<NotificationTypeId>(1), _fixture.CreateMany<UserRoleConfig>(), submitAppDocumentTypeIds).ConfigureAwait(false);

        // Assert
        var result = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);
        result.Message.Should().StartWith($"{string.Join(", ", submitAppDocumentTypeIds)} are mandatory document types, ({string.Join(", ", missingAppDocumentTypeIds)} are missing)");
    }

    [Fact]
    public async Task SubmitOffer_WithInvalidUserRole_ThrowsConflictException()
    {
        // Arrange
        var data = _fixture.Build<OfferReleaseData>()
            .With(x => x.IsDescriptionLongNotSet, false)
            .With(x => x.IsDescriptionShortNotSet, false)
            .With(x => x.HasUserRoles, false)
            .With(x => x.DocumentDatas, new[] { (Guid.NewGuid(), DocumentStatusId.PENDING, DocumentTypeId.CONFORMITY_APPROVAL_BUSINESS_APPS) })
            .Create();

        A.CallTo(() => _offerRepository.GetOfferReleaseDataByIdAsync(A<Guid>._, A<OfferTypeId>._)).Returns(data);

        // Act
        async Task Act() => await _sut.SubmitOfferAsync(Guid.NewGuid(), _identity.UserId, _fixture.Create<OfferTypeId>(), _fixture.CreateMany<NotificationTypeId>(1), _fixture.CreateMany<UserRoleConfig>(), new[] { DocumentTypeId.CONFORMITY_APPROVAL_BUSINESS_APPS }).ConfigureAwait(false);

        // Assert
        var result = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);
        result.Message.Should().StartWith("The app has no roles assigned");
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
            .With(x => x.HasPrivacyPolicies, true)
            .With(x => x.HasTechnicalUserProfiles, true)
            .With(x => x.DocumentDatas, new[] {
                (Guid.NewGuid(), DocumentStatusId.PENDING, DocumentTypeId.CONFORMITY_APPROVAL_BUSINESS_APPS),
                (Guid.NewGuid(), DocumentStatusId.INACTIVE, DocumentTypeId.APP_LEADIMAGE),
                (Guid.NewGuid(), DocumentStatusId.LOCKED, DocumentTypeId.APP_IMAGE) })
            .With(x => x.HasUserRoles, true)
            .Create();
        A.CallTo(() => _offerRepository.GetOfferReleaseDataByIdAsync(offerId, offerType)).Returns(data);

        IEnumerable<Document>? initial = null;
        IEnumerable<Document>? modified = null;

        A.CallTo(() => _documentRepository.AttachAndModifyDocuments(A<IEnumerable<(Guid DocumentId, Action<Document>? Initialize, Action<Document> Modify)>>._))
            .Invokes((IEnumerable<(Guid DocumentId, Action<Document>? Initialize, Action<Document> Modify)> data) =>
            {
                initial = data.Select(x =>
                    {
                        var document = new Document(x.DocumentId, null!, null!, null!, default, default, default, default);
                        x.Initialize?.Invoke(document);
                        return document;
                    }
                ).ToImmutableArray();
                modified = data.Select(x =>
                    {
                        var document = new Document(x.DocumentId, null!, null!, null!, default, default, default, default);
                        x.Modify(document);
                        return document;
                    }
                ).ToImmutableArray();
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
        await _sut.SubmitOfferAsync(offerId, _identity.UserId, offerType, new[] { NotificationTypeId.APP_SUBSCRIPTION_REQUEST }, _fixture.CreateMany<UserRoleConfig>(), new[] { DocumentTypeId.CONFORMITY_APPROVAL_BUSINESS_APPS, DocumentTypeId.APP_LEADIMAGE, DocumentTypeId.APP_IMAGE }).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _notificationService.CreateNotifications(A<IEnumerable<UserRoleConfig>>._, _identity.UserId, A<IEnumerable<(string? content, NotificationTypeId notifcationTypeId)>>._, false)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _documentRepository.AttachAndModifyDocuments(A<IEnumerable<(Guid, Action<Document>?, Action<Document>)>>._)).MustHaveHappenedOnceExactly();
        initial.Should().NotBeNull().And.HaveCount(1).And.Satisfy(x => x.Id == data.DocumentDatas.ElementAt(0).DocumentId && x.DocumentStatusId == DocumentStatusId.PENDING);
        modified.Should().NotBeNull().And.HaveCount(1).And.Satisfy(x => x.Id == data.DocumentDatas.ElementAt(0).DocumentId && x.DocumentStatusId == DocumentStatusId.LOCKED);
        A.CallTo(() => _offerRepository.AttachAndModifyOffer(offerId, A<Action<Offer>>._, A<Action<Offer>?>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task SubmitOffer_WithInvalidTechnicalUserProfiles_ThrowsConflictException()
    {
        // Arrange
        var data = _fixture.Build<OfferReleaseData>()
            .With(x => x.IsDescriptionLongNotSet, false)
            .With(x => x.IsDescriptionShortNotSet, false)
            .With(x => x.HasUserRoles, true)
            .With(x => x.HasTechnicalUserProfiles, false)
            .With(x => x.DocumentDatas, new[] { (Guid.NewGuid(), DocumentStatusId.PENDING, DocumentTypeId.CONFORMITY_APPROVAL_BUSINESS_APPS) })
            .Create();

        A.CallTo(() => _offerRepository.GetOfferReleaseDataByIdAsync(A<Guid>._, A<OfferTypeId>._)).Returns(data);
        var sut = new OfferService(_portalRepositories, null!, null!, _offerSetupService);

        // Act
        async Task Act() => await sut.SubmitOfferAsync(Guid.NewGuid(), _identity.UserId, _fixture.Create<OfferTypeId>(), _fixture.CreateMany<NotificationTypeId>(1), _fixture.CreateMany<UserRoleConfig>(), new[] { DocumentTypeId.CONFORMITY_APPROVAL_BUSINESS_APPS }).ConfigureAwait(false);

        // Assert
        var result = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);
        result.Message.Should().StartWith("Technical user profile setup is missing for the app");
    }

    [Fact]
    public async Task SubmitOffer_WithInvalidPrivacyPolicies_ThrowsConflictException()
    {
        // Arrange
        var data = _fixture.Build<OfferReleaseData>()
            .With(x => x.IsDescriptionLongNotSet, false)
            .With(x => x.IsDescriptionShortNotSet, false)
            .With(x => x.HasUserRoles, true)
            .With(x => x.HasTechnicalUserProfiles, true)
            .With(x => x.HasPrivacyPolicies, false)
            .With(x => x.DocumentDatas, new[] { (Guid.NewGuid(), DocumentStatusId.PENDING, DocumentTypeId.CONFORMITY_APPROVAL_BUSINESS_APPS) })
            .Create();

        A.CallTo(() => _offerRepository.GetOfferReleaseDataByIdAsync(A<Guid>._, A<OfferTypeId>._)).Returns(data);
        var sut = new OfferService(_portalRepositories, null!, null!, _offerSetupService);

        // Act
        async Task Act() => await sut.SubmitOfferAsync(Guid.NewGuid(), _identity.UserId, _fixture.Create<OfferTypeId>(), _fixture.CreateMany<NotificationTypeId>(1), _fixture.CreateMany<UserRoleConfig>(), new[] { DocumentTypeId.CONFORMITY_APPROVAL_BUSINESS_APPS }).ConfigureAwait(false);

        // Assert
        var result = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);
        result.Message.Should().StartWith("PrivacyPolicies is missing for the app");
    }

    #endregion

    #region ApproveOfferRequest

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ApproveOfferRequestAsync_ExecutesSuccessfully(bool isSingleInstance)
    {
        //Arrange
        var offer = _fixture.Build<Offer>().With(o => o.OfferStatusId, OfferStatusId.IN_REVIEW).Create();
        var companyId = _fixture.Create<Guid>();
        var instances = isSingleInstance
            ? new[]
            {
                (Guid.NewGuid(),Guid.NewGuid().ToString())
            }
            : new[]
            {
                (Guid.NewGuid(),Guid.NewGuid().ToString()),
                (Guid.NewGuid(),Guid.NewGuid().ToString())
            };
        A.CallTo(() => _offerRepository.GetOfferStatusDataByIdAsync(offer.Id, OfferTypeId.APP))
            .Returns((true, offer.Name, companyId, isSingleInstance, instances));
        A.CallTo(() => _offerRepository.AttachAndModifyOffer(offer.Id, A<Action<Offer>>._, A<Action<Offer>?>._))
            .Invokes((Guid _, Action<Offer> setOptionalParameters, Action<Offer>? initializeParemeters) =>
            {
                initializeParemeters?.Invoke(offer);
                setOptionalParameters(offer);
            });

        var approveAppNotificationTypeIds = new[]
        {
            NotificationTypeId.APP_RELEASE_APPROVAL
        };
        var approveAppUserRoles = new[]
        {
            new UserRoleConfig("catenax-portal", new [] { "Sales Manager" })
        };

        //Act
        await _sut.ApproveOfferRequestAsync(offer.Id, _identity.UserId, OfferTypeId.APP, approveAppNotificationTypeIds, approveAppUserRoles, new[] { NotificationTypeId.APP_SUBSCRIPTION_REQUEST }, _fixture.CreateMany<UserRoleConfig>()).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _offerRepository.GetOfferStatusDataByIdAsync(offer.Id, OfferTypeId.APP)).MustHaveHappened();
        A.CallTo(() => _offerRepository.AttachAndModifyOffer(A<Guid>._, A<Action<Offer>>._, A<Action<Offer>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _notificationService.CreateNotifications(A<IEnumerable<UserRoleConfig>>._, A<Guid>._, A<IEnumerable<(string? content, NotificationTypeId notificationTypeId)>>._, A<Guid>._, A<bool?>._)).MustHaveHappened();
        A.CallTo(() => _createNotificationsEnumerator.MoveNextAsync()).MustHaveHappened(2, Times.Exactly);
        offer.OfferStatusId.Should().Be(OfferStatusId.ACTIVE);
        offer.DateReleased.Should().NotBeNull();
        if (isSingleInstance)
        {
            A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
            A.CallTo(() => _offerSetupService.ActivateSingleInstanceAppAsync(offer.Id))
                .MustHaveHappenedOnceExactly();
        }
        else
        {
            A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
            A.CallTo(() => _offerSetupService.ActivateSingleInstanceAppAsync(offer.Id))
                .MustNotHaveHappened();
        }
    }

    [Fact]
    public async Task ApproveOfferRequestAsync_WithAppNameNotSet_ThrowsConflictException()
    {
        //Arrange
        var offerId = _fixture.Create<Guid>();
        var companyId = _fixture.Create<Guid>();

        A.CallTo(() => _offerRepository.GetOfferStatusDataByIdAsync(offerId, OfferTypeId.APP))
            .Returns((true, null, companyId, false, Enumerable.Empty<(Guid, string)>()));

        var approveAppNotificationTypeIds = new[]
        {
            NotificationTypeId.APP_RELEASE_APPROVAL
        };
        var approveAppUserRoles = new[]
        {
            new UserRoleConfig("catenax-portal", new [] { "Sales Manager" })
        };

        //Act
        Task Act() => _sut.ApproveOfferRequestAsync(offerId, _identity.UserId, OfferTypeId.APP, approveAppNotificationTypeIds, approveAppUserRoles, new[] { NotificationTypeId.APP_SUBSCRIPTION_REQUEST }, _fixture.CreateMany<UserRoleConfig>());

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

        A.CallTo(() => _offerRepository.GetOfferStatusDataByIdAsync(offerId, OfferTypeId.APP))
            .Returns((true, "The name", null, false, Enumerable.Empty<(Guid, string)>()));

        var approveAppNotificationTypeIds = new[]
        {
            NotificationTypeId.APP_RELEASE_APPROVAL
        };
        var approveAppUserRoles = new[]
        {
            new UserRoleConfig("catenax-portal", new [] { "Sales Manager" })
        };

        //Act
        Task Act() => _sut.ApproveOfferRequestAsync(offerId, _identity.UserId, OfferTypeId.APP, approveAppNotificationTypeIds, approveAppUserRoles, new[] { NotificationTypeId.APP_SUBSCRIPTION_REQUEST }, _fixture.CreateMany<UserRoleConfig>());

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
        A.CallTo(() => _offerRepository.GetOfferReleaseDataByIdAsync(notExistingOffer, offerType)).Returns((OfferReleaseData?)null);

        // Act
        async Task Act() => await _sut.SubmitServiceAsync(notExistingOffer, _identity.UserId, offerType, new[] { NotificationTypeId.APP_SUBSCRIPTION_REQUEST }, _fixture.CreateMany<UserRoleConfig>()).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act).ConfigureAwait(false);
        ex.Message.Should().Be($"{offerType} {notExistingOffer} does not exist");
    }

    [Theory]
    [InlineData(null, "c8d4d854-8ac6-425f-bc5a-dbf457670732", false, false, true)]
    [InlineData("name", null, false, false, true)]
    [InlineData("name", "c8d4d854-8ac6-425f-bc5a-dbf457670732", true, false, true)]
    [InlineData("name", "c8d4d854-8ac6-425f-bc5a-dbf457670732", false, true, true)]
    public async Task SubmitService_WithInvalidOffer_ThrowsConflictException(string? name, string? providerCompanyId, bool isDescriptionLongNotSet, bool isDescriptionShortNotSet, bool hasUserRoles)
    {
        // Arrange
        A.CallTo(() => _offerRepository.GetOfferReleaseDataByIdAsync(A<Guid>._, A<OfferTypeId>._)).Returns(new OfferReleaseData(name, providerCompanyId == null ? null : new Guid(providerCompanyId), _fixture.Create<string>(), isDescriptionLongNotSet, isDescriptionShortNotSet, hasUserRoles, true, true, new[] { (Guid.NewGuid(), DocumentStatusId.PENDING, DocumentTypeId.CONFORMITY_APPROVAL_BUSINESS_APPS) }));

        // Act
        async Task Act() => await _sut.SubmitServiceAsync(Guid.NewGuid(), _identity.UserId, _fixture.Create<OfferTypeId>(), _fixture.CreateMany<NotificationTypeId>(1), _fixture.CreateMany<UserRoleConfig>()).ConfigureAwait(false);

        // Assert
        var result = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);
        result.Message.Should().StartWith("Missing  : ");
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
            .With(x => x.DocumentDatas, new[]{
                (Guid.NewGuid(), DocumentStatusId.PENDING, DocumentTypeId.CONFORMITY_APPROVAL_BUSINESS_APPS),
                (Guid.NewGuid(), DocumentStatusId.INACTIVE, DocumentTypeId.CONFORMITY_APPROVAL_BUSINESS_APPS) })
            .Create();
        A.CallTo(() => _offerRepository.GetOfferReleaseDataByIdAsync(offerId, offerType)).Returns(data);

        A.CallTo(() => _documentRepository.AttachAndModifyDocument(A<Guid>._, A<Action<Document>>._, A<Action<Document>>._))
            .Invokes((Guid docId, Action<Document>? initialize, Action<Document> modify)
                =>
            {
                var document = new Document(docId, null!, null!, null!, default, default, default, default);
                initialize?.Invoke(document);
                modify(document);
            });

        IEnumerable<Document>? initial = null;
        IEnumerable<Document>? modified = null;

        A.CallTo(() => _documentRepository.AttachAndModifyDocuments(A<IEnumerable<(Guid DocumentId, Action<Document>? Initialize, Action<Document> Modify)>>._))
            .Invokes((IEnumerable<(Guid DocumentId, Action<Document>? Initialize, Action<Document> Modify)> data) =>
            {
                initial = data.Select(x =>
                    {
                        var document = new Document(x.DocumentId, null!, null!, null!, default, default, default, default);
                        x.Initialize?.Invoke(document);
                        return document;
                    }
                ).ToImmutableArray();
                modified = data.Select(x =>
                    {
                        var document = new Document(x.DocumentId, null!, null!, null!, default, default, default, default);
                        x.Modify(document);
                        return document;
                    }
                ).ToImmutableArray();
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
        await _sut.SubmitServiceAsync(offerId, _identity.UserId, offerType, new[] { NotificationTypeId.APP_SUBSCRIPTION_REQUEST }, _fixture.CreateMany<UserRoleConfig>()).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _notificationService.CreateNotifications(A<IEnumerable<UserRoleConfig>>._, _identity.UserId, A<IEnumerable<(string? content, NotificationTypeId notifcationTypeId)>>._, A<bool?>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _documentRepository.AttachAndModifyDocuments(A<IEnumerable<(Guid, Action<Document>?, Action<Document>)>>._)).MustHaveHappenedOnceExactly();
        initial.Should().NotBeNull().And.HaveCount(1).And.Satisfy(x => x.Id == data.DocumentDatas.ElementAt(0).DocumentId && x.DocumentStatusId == DocumentStatusId.PENDING);
        modified.Should().NotBeNull().And.HaveCount(1).And.Satisfy(x => x.Id == data.DocumentDatas.ElementAt(0).DocumentId && x.DocumentStatusId == DocumentStatusId.LOCKED);
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
        A.CallTo(() => _offerRepository.GetOfferDeclineDataAsync(notExistingOffer, offerTypeId))
            .Returns(((string?, OfferStatusId, Guid?, IEnumerable<DocumentStatusData>))default);

        // Act
        async Task Act() => await _sut.DeclineOfferAsync(notExistingOffer, _identity.UserId, new OfferDeclineRequest("Test"), offerTypeId, NotificationTypeId.SERVICE_RELEASE_REJECTION, Enumerable.Empty<UserRoleConfig>(), string.Empty, new[] { NotificationTypeId.APP_SUBSCRIPTION_REQUEST }, _fixture.CreateMany<UserRoleConfig>()).ConfigureAwait(false);

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
        var documentStatusData = _fixture.CreateMany<DocumentStatusData>();
        A.CallTo(() => _offerRepository.GetOfferDeclineDataAsync(notExistingOffer, offerTypeId))
            .Returns(("test", OfferStatusId.CREATED, Guid.NewGuid(), documentStatusData));

        // Act
        async Task Act() => await _sut.DeclineOfferAsync(notExistingOffer, _identity.UserId, new OfferDeclineRequest("Test"), offerTypeId, NotificationTypeId.SERVICE_RELEASE_REJECTION, Enumerable.Empty<UserRoleConfig>(), string.Empty, new[] { NotificationTypeId.APP_SUBSCRIPTION_REQUEST }, _fixture.CreateMany<UserRoleConfig>()).ConfigureAwait(false);

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
        var documentStatusDatas = _fixture.CreateMany<DocumentStatusData>();
        A.CallTo(() => _offerRepository.GetOfferDeclineDataAsync(notExistingOffer, offerTypeId))
            .Returns(((string?)null, OfferStatusId.IN_REVIEW, Guid.NewGuid(), documentStatusDatas));

        // Act
        async Task Act() => await _sut.DeclineOfferAsync(notExistingOffer, _identity.UserId, new OfferDeclineRequest("Test"), offerTypeId, NotificationTypeId.SERVICE_RELEASE_REJECTION, Enumerable.Empty<UserRoleConfig>(), string.Empty, new[] { NotificationTypeId.APP_SUBSCRIPTION_REQUEST }, _fixture.CreateMany<UserRoleConfig>()).ConfigureAwait(false);

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
        var documentStatusDatas = _fixture.CreateMany<DocumentStatusData>();
        A.CallTo(() => _offerRepository.GetOfferDeclineDataAsync(notExistingOffer, offerTypeId))
            .Returns(("test", OfferStatusId.IN_REVIEW, null, documentStatusDatas));

        // Act
        async Task Act() => await _sut.DeclineOfferAsync(notExistingOffer, _identity.UserId, new OfferDeclineRequest("Test"), offerTypeId, NotificationTypeId.SERVICE_RELEASE_REJECTION, Enumerable.Empty<UserRoleConfig>(), string.Empty, new[] { NotificationTypeId.APP_SUBSCRIPTION_REQUEST }, _fixture.CreateMany<UserRoleConfig>()).ConfigureAwait(false);

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
        var recipients = new[] { new UserRoleConfig("Test", new[] { "Abc" }) };
        var roleIds = _fixture.CreateMany<Guid>();
        var documentStatusDatas = new DocumentStatusData[]
        {
            new(Guid.NewGuid(), DocumentStatusId.LOCKED),
            new(Guid.NewGuid(), DocumentStatusId.PENDING),
        };
        A.CallTo(() => _offerRepository.GetOfferDeclineDataAsync(offerId, offerTypeId))
            .Returns(("test", OfferStatusId.IN_REVIEW, Guid.NewGuid(), documentStatusDatas));
        A.CallTo(() => _offerRepository.AttachAndModifyOffer(offerId, A<Action<Offer>>._, A<Action<Offer>?>._))
            .Invokes((Guid _, Action<Offer> setOptionalParameters, Action<Offer>? initializeParemeters) =>
            {
                initializeParemeters?.Invoke(offer);
                setOptionalParameters.Invoke(offer);
            });
        A.CallTo(() => _userRolesRepository.GetUserRoleIdsUntrackedAsync(A<IEnumerable<UserRoleConfig>>._))
            .Returns(roleIds.ToAsyncEnumerable());
        A.CallTo(() => _userRepository.GetCompanyUserEmailForCompanyAndRoleId(A<IEnumerable<Guid>>._, A<Guid>._))
            .Returns(new (string Email, string? Firstname, string? Lastname)[] { new("test@email.com", "Test User 1", "cx-user-2") }.ToAsyncEnumerable());

        IEnumerable<Document>? initial = null;
        IEnumerable<Document>? modified = null;
        A.CallTo(() => _documentRepository.AttachAndModifyDocuments(A<IEnumerable<(Guid DocumentId, Action<Document>? Initialize, Action<Document> Modify)>>._))
            .Invokes((IEnumerable<(Guid DocumentId, Action<Document>? Initialize, Action<Document> Modify)> data) =>
            {
                initial = data.Select(x =>
                    {
                        var document = new Document(x.DocumentId, null!, null!, null!, default, default, default, default);
                        x.Initialize?.Invoke(document);
                        return document;
                    }
                ).ToImmutableArray();
                modified = data.Select(x =>
                    {
                        var document = new Document(x.DocumentId, null!, null!, null!, default, default, default, default);
                        x.Modify(document);
                        return document;
                    }
                ).ToImmutableArray();
            });

        // Act
        await _sut.DeclineOfferAsync(offerId, _identity.UserId, new OfferDeclineRequest("Test"), offerTypeId, NotificationTypeId.SERVICE_RELEASE_REJECTION, recipients, string.Empty, new[] { NotificationTypeId.APP_SUBSCRIPTION_REQUEST }, _fixture.CreateMany<UserRoleConfig>()).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _offerRepository.AttachAndModifyOffer(offerId, A<Action<Offer>>._, A<Action<Offer>?>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _notificationService.CreateNotifications(A<IEnumerable<UserRoleConfig>>._, A<Guid>._, A<IEnumerable<(string? content, NotificationTypeId notificationTypeId)>>._, A<Guid>._, A<bool?>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _createNotificationsEnumerator.MoveNextAsync()).MustHaveHappened(2, Times.Exactly);
        A.CallTo(() => _mailingService.SendMails(A<string>._, A<IDictionary<string, string>>._, A<IEnumerable<string>>._)).MustHaveHappenedOnceExactly();
        offer.OfferStatusId.Should().Be(OfferStatusId.CREATED);
        A.CallTo(() => _documentRepository.AttachAndModifyDocuments(A<IEnumerable<(Guid, Action<Document>?, Action<Document>)>>._)).MustHaveHappenedOnceExactly();
        initial.Should().HaveCount(2).And.Satisfy(
            x => x.Id == documentStatusDatas.ElementAt(0).DocumentId && x.DocumentStatusId == DocumentStatusId.LOCKED,
            x => x.Id == documentStatusDatas.ElementAt(1).DocumentId && x.DocumentStatusId == DocumentStatusId.PENDING);
        modified.Should().HaveCount(2).And.Satisfy(
            x => x.Id == documentStatusDatas.ElementAt(0).DocumentId && x.DocumentStatusId == DocumentStatusId.PENDING,
            x => x.Id == documentStatusDatas.ElementAt(1).DocumentId && x.DocumentStatusId == DocumentStatusId.PENDING);
    }

    #endregion

    #region DeactivateOfferStatusId

    [Theory]
    [InlineData(OfferTypeId.APP)]
    [InlineData(OfferTypeId.SERVICE)]
    public async Task DeactivateOfferStatusIdAsync_WithoutExistingAppId_ThrowsForbiddenExceptionException(OfferTypeId offerTypeId)
    {
        // Arrange
        var notExistingId = _fixture.Create<Guid>();
        A.CallTo(() => _offerRepository.GetOfferActiveStatusDataByIdAsync(notExistingId, offerTypeId, _identity.CompanyId))
            .Returns(((bool, bool))default);

        // Act
        async Task Act() => await _sut.DeactivateOfferIdAsync(notExistingId, _identity.CompanyId, offerTypeId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act).ConfigureAwait(false);
        ex.Message.Should().Be($"{offerTypeId} {notExistingId} does not exist.");
    }

    [Theory]
    [InlineData(OfferTypeId.APP)]
    [InlineData(OfferTypeId.SERVICE)]
    public async Task DeactivateOfferStatusIdAsync_WithNotAssignedUser_ThrowsForbiddenException(OfferTypeId offerTypeId)
    {
        // Arrange
        var offerId = _fixture.Create<Guid>();
        A.CallTo(() => _offerRepository.GetOfferActiveStatusDataByIdAsync(offerId, offerTypeId, _identity.CompanyId))
            .Returns((true, false));

        // Act
        async Task Act() => await _sut.DeactivateOfferIdAsync(offerId, _identity.CompanyId, offerTypeId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act).ConfigureAwait(false);
        ex.Message.Should().Be("Missing permission: The user's company does not provide the requested app so they cannot deactivate it.");

    }

    [Theory]
    [InlineData(OfferTypeId.APP)]
    [InlineData(OfferTypeId.SERVICE)]
    public async Task DeactivateOfferStatusIdAsync_WithNotOfferStatusId_ThrowsConflictException(OfferTypeId offerTypeId)
    {
        // Arrange
        var offerId = _fixture.Create<Guid>();
        A.CallTo(() => _offerRepository.GetOfferActiveStatusDataByIdAsync(offerId, offerTypeId, _identity.CompanyId))
            .Returns((false, true));

        // Act
        async Task Act() => await _sut.DeactivateOfferIdAsync(offerId, _identity.CompanyId, offerTypeId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);
        ex.Message.Should().Be("offerStatus is in Incorrect State");

    }

    [Theory]
    [InlineData(OfferTypeId.APP)]
    [InlineData(OfferTypeId.SERVICE)]
    public async Task DeactivateOfferStatusIdAsync_WithValidData_CallsExpected(OfferTypeId offerTypeId)
    {
        // Arrange
        var offer = _fixture.Create<Offer>();
        var offerId = _fixture.Create<Guid>();
        A.CallTo(() => _offerRepository.GetOfferActiveStatusDataByIdAsync(offerId, offerTypeId, _identity.CompanyId))
            .Returns(new ValueTuple<bool, bool>(true, true));

        A.CallTo(() => _offerRepository.AttachAndModifyOffer(offerId, A<Action<Offer>>._, A<Action<Offer>?>._))
            .Invokes((Guid _, Action<Offer> setOptionalParameters, Action<Offer>? initializeParemeters) =>
            {
                initializeParemeters?.Invoke(offer);
                setOptionalParameters.Invoke(offer);
            });

        // Act
        await _sut.DeactivateOfferIdAsync(offerId, _identity.CompanyId, offerTypeId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _offerRepository.AttachAndModifyOffer(offerId, A<Action<Offer>>._, A<Action<Offer>?>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        offer.OfferStatusId.Should().Be(OfferStatusId.INACTIVE);
    }

    #endregion

    #region GetProviderOfferAgreementConsentById_ReturnExpectedResult

    [Fact]
    public async Task GetProviderOfferAgreementConsentById_ReturnExpectedResult()
    {
        //Arrange
        var data = _fixture.Create<OfferAgreementConsent>();
        var serviceId = Guid.NewGuid();
        A.CallTo(() => _agreementRepository.GetOfferAgreementConsentById(A<Guid>._, A<Guid>._, OfferTypeId.SERVICE))
            .Returns((data, true));

        // Act
        var result = await _sut.GetProviderOfferAgreementConsentById(serviceId, _identity.CompanyId, OfferTypeId.SERVICE).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _agreementRepository.GetOfferAgreementConsentById(serviceId, _identity.CompanyId, OfferTypeId.SERVICE))
            .MustHaveHappenedOnceExactly();
        result.Should().Be(data);
    }

    [Fact]
    public async Task GetProviderOfferAgreementConsentById_WithInvalidUserProviderCompany_ThrowsForbiddenException()
    {
        //Arrange
        var data = _fixture.Create<OfferAgreementConsent>();
        var serviceId = Guid.NewGuid();
        A.CallTo(() => _agreementRepository.GetOfferAgreementConsentById(A<Guid>._, A<Guid>._, OfferTypeId.SERVICE))
            .Returns((data, false));

        // Act
        async Task Act() => await _sut.GetProviderOfferAgreementConsentById(serviceId, _identity.CompanyId, OfferTypeId.SERVICE).ConfigureAwait(false);

        // Arrange
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act).ConfigureAwait(false);
        ex.Message.Should().Be($"Company {_identity.CompanyId} is not assigned with Offer {serviceId}");
    }

    [Fact]
    public async Task GetProviderOfferAgreementConsentById_WithInvalidOfferId_ThrowsNotFoundException()
    {
        //Arrange
        var serviceId = Guid.NewGuid();

        A.CallTo(() => _agreementRepository.GetOfferAgreementConsentById(A<Guid>._, A<Guid>._, OfferTypeId.SERVICE))
            .Returns(((OfferAgreementConsent, bool))default);

        // Act
        async Task Act() => await _sut.GetProviderOfferAgreementConsentById(serviceId, _identity.CompanyId, OfferTypeId.SERVICE).ConfigureAwait(false);

        // Arrange
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act).ConfigureAwait(false);
        ex.Message.Should().Be($"offer {serviceId}, offertype {OfferTypeId.SERVICE} does not exist");
    }

    #endregion

    #region CreateOrUpdateProviderOfferAgreementConsent

    [Theory]
    [InlineData(OfferTypeId.SERVICE)]
    [InlineData(OfferTypeId.APP)]
    public async Task CreateOrUpdateProviderOfferAgreementConsent_WithNoService_ThrowsNotFoundException(OfferTypeId offerTypeId)
    {
        //Arrange
        var offerId = Guid.NewGuid();
        var agreementId = Guid.NewGuid();
        var identitytestdata = (_identity.UserId, _identity.CompanyId);
        var consentData = new OfferAgreementConsent(new[] { new AgreementConsentStatus(agreementId, ConsentStatusId.ACTIVE) });
        A.CallTo(() => _agreementRepository.GetOfferAgreementConsent(offerId, _identity.CompanyId, OfferStatusId.CREATED, offerTypeId))
            .Returns(((OfferAgreementConsentUpdate, bool))default);

        // Act
        async Task Act() => await _sut.CreateOrUpdateProviderOfferAgreementConsent(offerId, consentData, identitytestdata, offerTypeId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"offer {offerId}, offertype {offerTypeId}, offerStatus {OfferStatusId.CREATED} does not exist");
    }

    [Theory]
    [InlineData(OfferTypeId.SERVICE)]
    [InlineData(OfferTypeId.APP)]
    public async Task CreateOrUpdateProviderOfferAgreementConsent_WithUserNotInProviderCompany_ThrowsNotFoundException(OfferTypeId offerTypeId)
    {
        //Arrange
        var offerId = Guid.NewGuid();
        var agreementId = Guid.NewGuid();
        var identitytestdata = (_identity.UserId, _identity.CompanyId);
        var consentData = new OfferAgreementConsent(new[] { new AgreementConsentStatus(agreementId, ConsentStatusId.ACTIVE) });
        A.CallTo(() => _agreementRepository.GetOfferAgreementConsent(offerId, _identity.CompanyId, OfferStatusId.CREATED, offerTypeId))
            .Returns((new OfferAgreementConsentUpdate(Enumerable.Empty<AppAgreementConsentStatus>(), Enumerable.Empty<Guid>()), false));

        // Act
        async Task Act() => await _sut.CreateOrUpdateProviderOfferAgreementConsent(offerId, consentData, identitytestdata, offerTypeId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be($"Company {_identity.CompanyId} is not assigned with Offer {offerId}");
    }

    [Theory]
    [InlineData(OfferTypeId.SERVICE)]
    [InlineData(OfferTypeId.APP)]
    public async Task CreateOrUpdateProviderOfferAgreementConsent_WithInvalidData_Throws(OfferTypeId offerTypeId)
    {
        //Arrange
        var offerId = Guid.NewGuid();
        var agreementId = Guid.NewGuid();
        var additionalAgreementId = Guid.NewGuid();
        var consentId = Guid.NewGuid();
        var identitytestdata = (_identity.UserId, _identity.CompanyId);
        var consentData = new OfferAgreementConsent(new[] { new AgreementConsentStatus(agreementId, ConsentStatusId.ACTIVE), new AgreementConsentStatus(additionalAgreementId, ConsentStatusId.ACTIVE) });
        var offerAgreementConsent = new OfferAgreementConsentUpdate(
            new[]
            {
                new AppAgreementConsentStatus(agreementId, consentId, ConsentStatusId.INACTIVE)
            },
            new[]
            {
                agreementId
            });
        A.CallTo(() => _agreementRepository.GetOfferAgreementConsent(offerId, _identity.CompanyId, OfferStatusId.CREATED, offerTypeId))
            .Returns((offerAgreementConsent, true));

        // Act
        async Task Act() => await _sut.CreateOrUpdateProviderOfferAgreementConsent(offerId, consentData, identitytestdata, offerTypeId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);
        ex.Message.Should().Be($"agreements {additionalAgreementId} are not valid for offer {offerId} (Parameter 'offerAgreementConsent')");
        A.CallTo(() => _consentRepository.AddAttachAndModifyOfferConsents(A<IEnumerable<AppAgreementConsentStatus>>._, A<IEnumerable<AgreementConsentStatus>>._, A<Guid>._, A<Guid>._, A<Guid>._, A<DateTimeOffset>._))
            .MustNotHaveHappened();
    }

    [Theory]
    [InlineData(OfferTypeId.SERVICE)]
    [InlineData(OfferTypeId.APP)]
    public async Task CreateOrUpdateProviderOfferAgreementConsent_WithValidData_ReturnsExpected(OfferTypeId offerTypeId)
    {
        //Arrange
        var offerId = Guid.NewGuid();
        var agreementId = Guid.NewGuid();
        var additionalAgreementId = Guid.NewGuid();
        var consentId = Guid.NewGuid();
        var newCreatedConsentId = Guid.NewGuid();
        var utcNow = DateTimeOffset.UtcNow;
        var identitytestdata = (_identity.UserId, _identity.CompanyId);
        var consentData = new OfferAgreementConsent(new[] { new AgreementConsentStatus(agreementId, ConsentStatusId.ACTIVE), new AgreementConsentStatus(additionalAgreementId, ConsentStatusId.ACTIVE) });
        var offerAgreementConsent = new OfferAgreementConsentUpdate(
            new[]
            {
                new AppAgreementConsentStatus(agreementId, consentId, ConsentStatusId.INACTIVE)
            },
            new[]
            {
                agreementId,
                additionalAgreementId
            });
        A.CallTo(() => _agreementRepository.GetOfferAgreementConsent(offerId, _identity.CompanyId, OfferStatusId.CREATED, offerTypeId))
            .Returns((offerAgreementConsent, true));
        A.CallTo(() => _consentRepository.AddAttachAndModifyOfferConsents(A<IEnumerable<AppAgreementConsentStatus>>._, A<IEnumerable<AgreementConsentStatus>>._, A<Guid>._, A<Guid>._, A<Guid>._, A<DateTimeOffset>._))
            .Returns(new Consent[] {
                new(consentId, agreementId, CompanyUserCompanyId, _companyUser.Id, ConsentStatusId.ACTIVE, utcNow),
                new(newCreatedConsentId, additionalAgreementId, CompanyUserCompanyId, _companyUser.Id, ConsentStatusId.ACTIVE, utcNow)
            });
        var existingOffer = _fixture.Create<Offer>();
        existingOffer.DateLastChanged = DateTimeOffset.UtcNow;
        A.CallTo(() => _offerRepository.AttachAndModifyOffer(offerId, A<Action<Offer>>._, A<Action<Offer>?>._))
            .Invokes((Guid _, Action<Offer> setOptionalParameters, Action<Offer>? initializeParemeters) =>
            {
                initializeParemeters?.Invoke(existingOffer);
                setOptionalParameters(existingOffer);
            });
        // Act
        var result = await _sut.CreateOrUpdateProviderOfferAgreementConsent(offerId, consentData, identitytestdata, offerTypeId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _consentRepository.AddAttachAndModifyOfferConsents(A<IEnumerable<AppAgreementConsentStatus>>._, A<IEnumerable<AgreementConsentStatus>>._, offerId, _identity.CompanyId, _identity.UserId, A<DateTimeOffset>._))
            .MustHaveHappenedOnceExactly();
        result.Should()
            .HaveCount(2)
            .And.Satisfy(
                x => x.AgreementId == agreementId && x.ConsentStatus == ConsentStatusId.ACTIVE,
                x => x.AgreementId == additionalAgreementId && x.ConsentStatus == ConsentStatusId.ACTIVE
            );
        A.CallTo(() => _offerRepository.AttachAndModifyOffer(offerId, A<Action<Offer>>._, A<Action<Offer>?>._)).MustHaveHappenedOnceExactly();
    }

    #endregion

    #region GetOfferDocumentContentAsync

    [Theory]
    [InlineData(OfferTypeId.APP, new[] { DocumentTypeId.APP_LEADIMAGE, DocumentTypeId.APP_IMAGE })]
    [InlineData(OfferTypeId.SERVICE, new[] { DocumentTypeId.ADDITIONAL_DETAILS })]
    public async Task GetOfferDocumentContentAsync_ReturnsExpectedResult(OfferTypeId offerTypeId, IEnumerable<DocumentTypeId> documentTypeIdSettings)
    {
        // Arrange
        var offerId = _fixture.Create<Guid>();
        var documentId = _fixture.Create<Guid>();
        var data = _fixture.Create<byte[]>();
        var fileName = _fixture.Create<string>() + ".jpeg";

        var documentContentData = new OfferDocumentContentData(true, true, true, false, data, fileName, MediaTypeId.JPEG);
        A.CallTo(() => _documentRepository.GetOfferDocumentContentAsync(offerId, documentId, documentTypeIdSettings, offerTypeId, A<CancellationToken>._))
            .Returns(documentContentData);

        // Act
        var result = await _sut.GetOfferDocumentContentAsync(offerId, documentId, documentTypeIdSettings, offerTypeId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.Content.Should().BeSameAs(data);
        result.ContentType.Should().Be("image/jpeg");
        A.CallTo(() => _documentRepository.GetOfferDocumentContentAsync(offerId, documentId, documentTypeIdSettings, offerTypeId, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData(OfferTypeId.APP, new[] { DocumentTypeId.APP_LEADIMAGE, DocumentTypeId.APP_IMAGE })]
    [InlineData(OfferTypeId.SERVICE, new[] { DocumentTypeId.ADDITIONAL_DETAILS })]
    public async Task GetOfferDocumentContentAsync_ForDocumentIdNotExist_ThrowsArgumentException(OfferTypeId offerTypeId, IEnumerable<DocumentTypeId> documentTypeIdSettings)
    {
        // Arrange
        var offerId = _fixture.Create<Guid>();
        var documentId = _fixture.Create<Guid>();

        A.CallTo(() => _documentRepository.GetOfferDocumentContentAsync(offerId, documentId, documentTypeIdSettings, offerTypeId, A<CancellationToken>._))
            .Returns((OfferDocumentContentData?)null);

        // Act
        async Task Act() => await _sut.GetOfferDocumentContentAsync(offerId, documentId, documentTypeIdSettings, offerTypeId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"document {documentId} does not exist");
    }

    [Theory]
    [InlineData(OfferTypeId.APP, new[] { DocumentTypeId.APP_LEADIMAGE, DocumentTypeId.APP_IMAGE })]
    [InlineData(OfferTypeId.SERVICE, new[] { DocumentTypeId.ADDITIONAL_DETAILS })]
    public async Task GetOfferDocumentContentAsync_WithInvalidDocumentType_ThrowsArgumentException(OfferTypeId offerTypeId, IEnumerable<DocumentTypeId> documentTypeIdSettings)
    {
        // Arrange
        var offerId = _fixture.Create<Guid>();
        var documentId = _fixture.Create<Guid>();
        var fileName = _fixture.Create<string>() + ".jpeg";

        var documentContentData = new OfferDocumentContentData(false, true, true, false, null, fileName, MediaTypeId.JPEG);
        A.CallTo(() => _documentRepository.GetOfferDocumentContentAsync(offerId, documentId, documentTypeIdSettings, offerTypeId, A<CancellationToken>._))
            .Returns(documentContentData);

        // Act
        async Task Act() => await _sut.GetOfferDocumentContentAsync(offerId, documentId, documentTypeIdSettings, offerTypeId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be($"Document {documentId} can not get retrieved. Document type not supported.");
    }

    [Theory]
    [InlineData(OfferTypeId.APP, new[] { DocumentTypeId.APP_LEADIMAGE, DocumentTypeId.APP_IMAGE })]
    [InlineData(OfferTypeId.SERVICE, new[] { DocumentTypeId.ADDITIONAL_DETAILS })]
    public async Task GetOfferDocumentContentAsync_WithInvalidOfferType_ThrowsArgumentException(OfferTypeId offerTypeId, IEnumerable<DocumentTypeId> documentTypeIdSettings)
    {
        // Arrange
        var offerId = _fixture.Create<Guid>();
        var documentId = _fixture.Create<Guid>();
        var fileName = _fixture.Create<string>() + ".jpeg";

        var documentContentData = new OfferDocumentContentData(true, true, false, false, null, fileName, MediaTypeId.JPEG);
        A.CallTo(() => _documentRepository.GetOfferDocumentContentAsync(offerId, documentId, documentTypeIdSettings, offerTypeId, A<CancellationToken>._))
            .Returns(documentContentData);

        // Act
        async Task Act() => await _sut.GetOfferDocumentContentAsync(offerId, documentId, documentTypeIdSettings, offerTypeId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be($"offer {offerId} is not an {offerTypeId}");
    }

    [Theory]
    [InlineData(OfferTypeId.APP, new[] { DocumentTypeId.APP_LEADIMAGE, DocumentTypeId.APP_IMAGE })]
    [InlineData(OfferTypeId.SERVICE, new[] { DocumentTypeId.ADDITIONAL_DETAILS })]
    public async Task GetOfferDocumentContentAsync_WithOfferNotLinkToDocument_ThrowsArgumentException(OfferTypeId offerTypeId, IEnumerable<DocumentTypeId> documentTypeIdSettings)
    {
        // Arrange
        var offerId = _fixture.Create<Guid>();
        var documentId = _fixture.Create<Guid>();
        var fileName = _fixture.Create<string>() + ".jpeg";

        var documentContentData = new OfferDocumentContentData(true, false, true, false, null, fileName, MediaTypeId.JPEG);
        A.CallTo(() => _documentRepository.GetOfferDocumentContentAsync(offerId, documentId, documentTypeIdSettings, offerTypeId, A<CancellationToken>._))
            .Returns(documentContentData);

        // Act
        async Task Act() => await _sut.GetOfferDocumentContentAsync(offerId, documentId, documentTypeIdSettings, offerTypeId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be($"Document {documentId} and {offerTypeId} id {offerId} do not match.");
    }

    [Theory]
    [InlineData(OfferTypeId.APP, new[] { DocumentTypeId.APP_LEADIMAGE, DocumentTypeId.APP_IMAGE })]
    [InlineData(OfferTypeId.SERVICE, new[] { DocumentTypeId.ADDITIONAL_DETAILS })]
    public async Task GetOfferDocumentContentAsync_WithInvalidStatus_ThrowsConflictException(OfferTypeId offerTypeId, IEnumerable<DocumentTypeId> documentTypeIdSettings)
    {
        // Arrange
        var offerId = _fixture.Create<Guid>();
        var documentId = _fixture.Create<Guid>();
        var fileName = _fixture.Create<string>() + ".jpeg";

        var documentContentData = new OfferDocumentContentData(true, true, true, true, null, fileName, MediaTypeId.JPEG);
        A.CallTo(() => _documentRepository.GetOfferDocumentContentAsync(offerId, documentId, documentTypeIdSettings, offerTypeId, A<CancellationToken>._))
            .Returns(documentContentData);

        // Act
        async Task Act() => await _sut.GetOfferDocumentContentAsync(offerId, documentId, documentTypeIdSettings, offerTypeId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"Document {documentId} is in status INACTIVE");
    }

    [Theory]
    [InlineData(OfferTypeId.APP, new[] { DocumentTypeId.APP_LEADIMAGE, DocumentTypeId.APP_IMAGE })]
    [InlineData(OfferTypeId.SERVICE, new[] { DocumentTypeId.ADDITIONAL_DETAILS })]
    public async Task GetAppImageDocumentContentAsync_WithContentNull_ThrowsUnexpectedConditionException(OfferTypeId offerTypeId, IEnumerable<DocumentTypeId> documentTypeIdSettings)
    {
        // Arrange
        var offerId = _fixture.Create<Guid>();
        var documentId = _fixture.Create<Guid>();
        var fileName = _fixture.Create<string>() + ".jpeg";

        var documentContentData = new OfferDocumentContentData(true, true, true, false, null, fileName, MediaTypeId.JPEG);
        A.CallTo(() => _documentRepository.GetOfferDocumentContentAsync(offerId, documentId, documentTypeIdSettings, offerTypeId, A<CancellationToken>._))
            .Returns(documentContentData);

        // Act
        async Task Act() => await _sut.GetOfferDocumentContentAsync(offerId, documentId, documentTypeIdSettings, offerTypeId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);
        ex.Message.Should().Be($"document content should never be null");
    }

    #endregion

    #region  DeleteDocument

    [Theory]
    [InlineData(OfferTypeId.APP, new[] { DocumentTypeId.APP_CONTRACT })]
    [InlineData(OfferTypeId.SERVICE, new[] { DocumentTypeId.ADDITIONAL_DETAILS })]
    public async Task DeleteDocumentsAsync_ReturnsExpectedResult(OfferTypeId offerTypeId, IEnumerable<DocumentTypeId> documentTypeIdSettings)
    {
        //Arrange
        var offerId = Guid.NewGuid();

        A.CallTo(() => _documentRepository.GetOfferDocumentsAsync(_validDocumentId, _identity.CompanyId, documentTypeIdSettings, offerTypeId))
            .Returns((new[] { (OfferStatusId.CREATED, offerId, true) }, true, DocumentStatusId.PENDING, true));

        //Act
        await _sut.DeleteDocumentsAsync(_validDocumentId, _identity.CompanyId, documentTypeIdSettings, offerTypeId).ConfigureAwait(false);

        // Assert 
        A.CallTo(() => _documentRepository.GetOfferDocumentsAsync(_validDocumentId, _identity.CompanyId, documentTypeIdSettings, offerTypeId))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.RemoveOfferAssignedDocument(offerId, _validDocumentId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _documentRepository.RemoveDocument(_validDocumentId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData(OfferTypeId.APP, new[] { DocumentTypeId.APP_CONTRACT })]
    [InlineData(OfferTypeId.SERVICE, new[] { DocumentTypeId.ADDITIONAL_DETAILS })]
    public async Task DeleteDocumentsAsync_WithNoDocument_ThrowsNotFoundException(OfferTypeId offerTypeId, IEnumerable<DocumentTypeId> documentTypeIdSettings)
    {
        //Arrange
        A.CallTo(() => _documentRepository.GetOfferDocumentsAsync(_validDocumentId, _identity.CompanyId, documentTypeIdSettings, offerTypeId))
            .Returns(((IEnumerable<(OfferStatusId, Guid, bool)>, bool, DocumentStatusId, bool))default);

        //Act
        async Task Act() => await _sut.DeleteDocumentsAsync(_validDocumentId, _identity.CompanyId, documentTypeIdSettings, offerTypeId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"Document {_validDocumentId} does not exist");
    }

    [Theory]
    [InlineData(OfferTypeId.APP, new[] { DocumentTypeId.APP_CONTRACT })]
    [InlineData(OfferTypeId.SERVICE, new[] { DocumentTypeId.ADDITIONAL_DETAILS })]
    public async Task DeleteDocumentsAsync_WithNoAssignedOfferDocument_ThrowsConflictException(OfferTypeId offerTypeId, IEnumerable<DocumentTypeId> documentTypeIdSettings)
    {
        //Arrange
        A.CallTo(() => _documentRepository.GetOfferDocumentsAsync(_validDocumentId, _identity.CompanyId, documentTypeIdSettings, offerTypeId))
            .Returns((new[] { ((OfferStatusId, Guid, bool))default }, true, DocumentStatusId.PENDING, true));

        //Act
        async Task Act() => await _sut.DeleteDocumentsAsync(_validDocumentId, _identity.CompanyId, documentTypeIdSettings, offerTypeId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"Document {_validDocumentId} is not assigned to an {offerTypeId}");
    }

    [Theory]
    [InlineData(OfferTypeId.APP, new[] { DocumentTypeId.APP_CONTRACT })]
    [InlineData(OfferTypeId.SERVICE, new[] { DocumentTypeId.ADDITIONAL_DETAILS })]
    public async Task DeleteDocumentsAsync_WithMultipleDocumentsAssigned_ThrowsConflictException(OfferTypeId offerTypeId, IEnumerable<DocumentTypeId> documentTypeIdSettings)
    {
        //Arrange
        A.CallTo(() => _documentRepository.GetOfferDocumentsAsync(_validDocumentId, _identity.CompanyId, documentTypeIdSettings, offerTypeId))
            .Returns((
                new[]
                {
                    (OfferStatusId.CREATED, Guid.NewGuid(), true),
                    (OfferStatusId.CREATED, Guid.NewGuid(), true)
                },
                true,
                DocumentStatusId.PENDING,
                true));

        //Act
        async Task Act() => await _sut.DeleteDocumentsAsync(_validDocumentId, _identity.CompanyId, documentTypeIdSettings, offerTypeId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"Document {_validDocumentId} is assigned to more than one {offerTypeId}");
    }

    [Theory]
    [InlineData(OfferTypeId.APP, new[] { DocumentTypeId.APP_CONTRACT })]
    [InlineData(OfferTypeId.SERVICE, new[] { DocumentTypeId.ADDITIONAL_DETAILS })]
    public async Task DeleteDocumentsAsync_WithDocumentAssignedToService_ThrowsConflictException(OfferTypeId offerTypeId, IEnumerable<DocumentTypeId> documentTypeIdSettings)
    {
        //Arrange
        var offerId = Guid.NewGuid();

        A.CallTo(() => _documentRepository.GetOfferDocumentsAsync(_validDocumentId, _identity.CompanyId, documentTypeIdSettings, offerTypeId))
            .Returns((new[] { (OfferStatusId.CREATED, offerId, false) }, true, DocumentStatusId.PENDING, true));

        //Act
        async Task Act() => await _sut.DeleteDocumentsAsync(_validDocumentId, _identity.CompanyId, documentTypeIdSettings, offerTypeId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"Document {_validDocumentId} is not assigned to an {offerTypeId}");
    }

    [Theory]
    [InlineData(OfferTypeId.APP, new[] { DocumentTypeId.APP_CONTRACT })]
    [InlineData(OfferTypeId.SERVICE, new[] { DocumentTypeId.ADDITIONAL_DETAILS })]
    public async Task DeleteDocumentsAsync_WithInvalidProviderCompanyUser_ThrowsForbiddenException(OfferTypeId offerTypeId, IEnumerable<DocumentTypeId> documentTypeIdSettings)
    {
        //Arrange
        var offerId = Guid.NewGuid();

        A.CallTo(() => _documentRepository.GetOfferDocumentsAsync(_validDocumentId, _identity.CompanyId, documentTypeIdSettings, offerTypeId))
            .Returns((new[] { (OfferStatusId.CREATED, offerId, true) }, true, DocumentStatusId.PENDING, false));

        //Act
        async Task Act() => await _sut.DeleteDocumentsAsync(_validDocumentId, _identity.CompanyId, documentTypeIdSettings, offerTypeId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be($"Company {_identity.CompanyId} is not the same company of document {_validDocumentId}");
    }

    [Theory]
    [InlineData(OfferTypeId.APP, new[] { DocumentTypeId.APP_CONTRACT })]
    [InlineData(OfferTypeId.SERVICE, new[] { DocumentTypeId.ADDITIONAL_DETAILS })]
    public async Task DeleteDocumentsAsync_WithInvalidOfferStatus_ThrowsConflictException(OfferTypeId offerTypeId, IEnumerable<DocumentTypeId> documentTypeIdSettings)
    {
        //Arrange
        var offerId = Guid.NewGuid();

        A.CallTo(() => _documentRepository.GetOfferDocumentsAsync(_validDocumentId, _identity.CompanyId, documentTypeIdSettings, offerTypeId))
            .Returns((new[] { (OfferStatusId.ACTIVE, offerId, true) }, true, DocumentStatusId.PENDING, true));

        //Act
        async Task Act() => await _sut.DeleteDocumentsAsync(_validDocumentId, _identity.CompanyId, documentTypeIdSettings, offerTypeId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"{offerTypeId} {offerId} is in locked state");
    }

    [Theory]
    [InlineData(OfferTypeId.APP, new[] { DocumentTypeId.APP_CONTRACT })]
    [InlineData(OfferTypeId.SERVICE, new[] { DocumentTypeId.ADDITIONAL_DETAILS })]
    public async Task DeleteDocumentsAsync_WithInvalidDocumentType_ThrowsArgumentException(OfferTypeId offerTypeId, IEnumerable<DocumentTypeId> documentTypeIdSettings)
    {
        //Arrange
        var offerId = Guid.NewGuid();

        A.CallTo(() => _documentRepository.GetOfferDocumentsAsync(_validDocumentId, _identity.CompanyId, documentTypeIdSettings, offerTypeId))
            .Returns((new[] { (OfferStatusId.CREATED, offerId, true) }, false, DocumentStatusId.PENDING, true));

        //Act
        async Task Act() => await _sut.DeleteDocumentsAsync(_validDocumentId, _identity.CompanyId, documentTypeIdSettings, offerTypeId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be($"Document {_validDocumentId} can not get retrieved. Document type not supported");
    }

    [Theory]
    [InlineData(OfferTypeId.APP, new[] { DocumentTypeId.APP_CONTRACT })]
    [InlineData(OfferTypeId.SERVICE, new[] { DocumentTypeId.ADDITIONAL_DETAILS })]
    public async Task DeleteDocumentsAsync_WithInvalidDocumentStatus_ThrowsConflictException(OfferTypeId offerTypeId, IEnumerable<DocumentTypeId> documentTypeIdSettings)
    {
        //Arrange
        var offerId = Guid.NewGuid();

        A.CallTo(() => _documentRepository.GetOfferDocumentsAsync(_validDocumentId, _identity.CompanyId, documentTypeIdSettings, offerTypeId))
            .Returns((new[] { (OfferStatusId.CREATED, offerId, true) }, true, DocumentStatusId.LOCKED, true));
        //Act
        async Task Act() => await _sut.DeleteDocumentsAsync(_validDocumentId, _identity.CompanyId, documentTypeIdSettings, offerTypeId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"Document in State {DocumentStatusId.LOCKED} can't be deleted");
    }

    #endregion

    #region GetTechnicalUserProfileData

    [Theory]
    [InlineData(OfferTypeId.APP)]
    [InlineData(OfferTypeId.SERVICE)]
    public async Task GetTechnicalUserProfileData_ReturnsExpectedResult(OfferTypeId offerTypeId)
    {
        // Arrange
        var offerId = _fixture.Create<Guid>();
        var data = _fixture.CreateMany<TechnicalUserProfileInformation>(5);
        A.CallTo(() => _technicalUserProfileRepository.GetTechnicalUserProfileInformation(offerId, _identity.CompanyId, offerTypeId))
            .Returns((true, data));

        // Act
        var result = await _sut.GetTechnicalUserProfilesForOffer(offerId, _identity.CompanyId, offerTypeId).ConfigureAwait(false);

        // Assert
        result.Should().HaveCount(5);
        A.CallTo(() => _technicalUserProfileRepository.GetTechnicalUserProfileInformation(offerId, _identity.CompanyId, offerTypeId)).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData(OfferTypeId.APP)]
    [InlineData(OfferTypeId.SERVICE)]
    public async Task GetTechnicalUserProfileData_WithoutOffer_ThrowsNotFoundException(OfferTypeId offerTypeId)
    {
        // Arrange
        var offerId = _fixture.Create<Guid>();
        var data = _fixture.CreateMany<TechnicalUserProfileInformation>(5);
        A.CallTo(() => _technicalUserProfileRepository.GetTechnicalUserProfileInformation(offerId, _identity.CompanyId, offerTypeId))
            .Returns(((bool, IEnumerable<TechnicalUserProfileInformation>))default);

        // Act
        async Task Act() => await _sut.GetTechnicalUserProfilesForOffer(offerId, _identity.CompanyId, offerTypeId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"Offer {offerId} does not exist");
        A.CallTo(() => _technicalUserProfileRepository.GetTechnicalUserProfileInformation(offerId, _identity.CompanyId, offerTypeId)).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData(OfferTypeId.APP)]
    [InlineData(OfferTypeId.SERVICE)]
    public async Task GetTechnicalUserProfileData_WithUserNotInProvidingCompany_ThrowsForbiddenException(OfferTypeId offerTypeId)
    {
        // Arrange
        var offerId = _fixture.Create<Guid>();
        A.CallTo(() => _technicalUserProfileRepository.GetTechnicalUserProfileInformation(offerId, _identity.CompanyId, offerTypeId))
            .Returns((false, Enumerable.Empty<TechnicalUserProfileInformation>()));

        // Act
        async Task Act() => await _sut.GetTechnicalUserProfilesForOffer(offerId, _identity.CompanyId, offerTypeId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be($"Company {_identity.CompanyId} is not the providing company");
        A.CallTo(() => _technicalUserProfileRepository.GetTechnicalUserProfileInformation(offerId, _identity.CompanyId, offerTypeId)).MustHaveHappenedOnceExactly();
    }

    #endregion

    #region UpdateTechnicalUserProfiles

    [Theory]
    [InlineData(OfferTypeId.APP)]
    [InlineData(OfferTypeId.SERVICE)]
    public async Task UpdateTechnicalUserProfiles_ReturnsExpectedResult(OfferTypeId offerTypeId)
    {
        // Arrange
        var offerId = _fixture.Create<Guid>();
        var technicalUserProfile1 = _fixture.Create<Guid>();
        var technicalUserProfile2 = _fixture.Create<Guid>();
        var technicalUserProfile3 = _fixture.Create<Guid>();
        var newProfileId = _fixture.Create<Guid>();
        var userRole1Id = _fixture.Create<Guid>();
        var userRole2Id = _fixture.Create<Guid>();
        var data = new[]
        {
            new TechnicalUserProfileData(null, new []                     // to create
            {
                userRole1Id,
                userRole2Id
            }),
            new TechnicalUserProfileData(technicalUserProfile1, new []
            {
                userRole2Id
            }),
            new TechnicalUserProfileData(technicalUserProfile3, Enumerable.Empty<Guid>())
        };
        var profileData = new (Guid, IEnumerable<Guid>)[]
        {
            (technicalUserProfile1, new[] {userRole1Id}),                 // to update
            (technicalUserProfile2, new[] {userRole1Id, userRole2Id}),    // to delete
            (technicalUserProfile3, Enumerable.Empty<Guid>())
        };
        A.CallTo(() => _technicalUserProfileRepository.GetOfferProfileData(offerId, offerTypeId, _identity.CompanyId))
            .Returns(new OfferProfileData(true, new[] { ServiceTypeId.DATASPACE_SERVICE }, profileData));
        A.CallTo(() => _userRolesRepository.GetRolesForClient("cl1"))
            .Returns(new Guid[] { userRole1Id, userRole2Id }.ToAsyncEnumerable());
        A.CallTo(() => _technicalUserProfileRepository.CreateTechnicalUserProfile(A<Guid>._, offerId))
            .Returns(new TechnicalUserProfile(newProfileId, offerId));
        var existingOffer = _fixture.Create<Offer>();
        existingOffer.DateLastChanged = DateTimeOffset.UtcNow;
        A.CallTo(() => _offerRepository.AttachAndModifyOffer(offerId, A<Action<Offer>>._, A<Action<Offer>?>._))
            .Invokes((Guid _, Action<Offer> setOptionalParameters, Action<Offer>? initializeParemeters) =>
            {
                initializeParemeters?.Invoke(existingOffer);
                setOptionalParameters(existingOffer);
            });
        // Act
        await _sut.UpdateTechnicalUserProfiles(offerId, offerTypeId, data, _identity.CompanyId, "cl1").ConfigureAwait(false);

        // Assert
        A.CallTo(() => _technicalUserProfileRepository.CreateTechnicalUserProfile(A<Guid>._, offerId))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _technicalUserProfileRepository.CreateDeleteTechnicalUserProfileAssignedRoles(
                A<IEnumerable<(Guid, Guid)>>.That.Matches(
                    x => x.Count() == 3 &&
                    x.Any(y => y.Item1 == technicalUserProfile1 && y.Item2 == userRole1Id) &&
                    x.Any(y => y.Item1 == technicalUserProfile2 && y.Item2 == userRole1Id) &&
                    x.Any(y => y.Item1 == technicalUserProfile2 && y.Item2 == userRole2Id)),
                A<IEnumerable<(Guid, Guid)>>.That.Matches(
                    x => x.Count() == 3 &&
                    x.Any(y => y.Item1 == newProfileId && y.Item2 == userRole1Id) &&
                    x.Any(y => y.Item1 == newProfileId && y.Item2 == userRole2Id) &&
                    x.Any(y => y.Item1 == technicalUserProfile1 && y.Item2 == userRole2Id))))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _technicalUserProfileRepository.RemoveTechnicalUserProfiles(A<IEnumerable<Guid>>.That.Matches(x => x.Count() == 2 && x.Contains(technicalUserProfile2) && x.Contains(technicalUserProfile3))))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.AttachAndModifyOffer(offerId, A<Action<Offer>>._, A<Action<Offer>?>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData(OfferTypeId.APP)]
    [InlineData(OfferTypeId.SERVICE)]
    public async Task UpdateTechnicalUserProfiles_WithNotExistingRoles_ThrowsException(OfferTypeId offerTypeId)
    {
        // Arrange
        var offerId = _fixture.Create<Guid>();
        var missingRoleId = _fixture.Create<Guid>();
        var userRole1Id = _fixture.Create<Guid>();
        var userRole2Id = _fixture.Create<Guid>();
        var data = new[]
        {
            new TechnicalUserProfileData(null, new []
            {
                userRole1Id,
                userRole2Id,
                missingRoleId
            }),
        };
        A.CallTo(() => _technicalUserProfileRepository.GetOfferProfileData(offerId, offerTypeId, _identity.CompanyId))
            .Returns(new OfferProfileData(true, new[] { ServiceTypeId.DATASPACE_SERVICE }, Enumerable.Empty<(Guid TechnicalUserProfileId, IEnumerable<Guid> UserRoleIds)>()));
        A.CallTo(() => _userRolesRepository.GetRolesForClient("cl1"))
            .Returns(new Guid[] { userRole1Id, userRole2Id }.ToAsyncEnumerable());

        // Act
        async Task Act() => await _sut.UpdateTechnicalUserProfiles(offerId, offerTypeId, data, _identity.CompanyId, "cl1").ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be($"Roles {missingRoleId} do not exist");
    }

    [Theory]
    [InlineData(OfferTypeId.APP)]
    [InlineData(OfferTypeId.SERVICE)]
    public async Task UpdateTechnicalUserProfiles_ForConsultancyService_ThrowsException(OfferTypeId offerTypeId)
    {
        // Arrange
        var offerId = _fixture.Create<Guid>();
        var userRole1Id = _fixture.Create<Guid>();
        var userRole2Id = _fixture.Create<Guid>();
        var data = new[]
        {
            new TechnicalUserProfileData(null, new []
            {
                userRole1Id,
                userRole2Id,
            }),
        };
        A.CallTo(() => _technicalUserProfileRepository.GetOfferProfileData(offerId, offerTypeId, _identity.CompanyId))
            .Returns(new OfferProfileData(true, new[] { ServiceTypeId.CONSULTANCE_SERVICE }, Enumerable.Empty<(Guid TechnicalUserProfileId, IEnumerable<Guid> UserRoleIds)>()));
        A.CallTo(() => _userRolesRepository.GetRolesForClient("cl1"))
            .Returns(new Guid[] { userRole1Id, userRole2Id }.ToAsyncEnumerable());

        // Act
        async Task Act() => await _sut.UpdateTechnicalUserProfiles(offerId, offerTypeId, data, _identity.CompanyId, "cl1").ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be("Technical User Profiles can't be set for CONSULTANCE_SERVICE");
    }

    [Theory]
    [InlineData(OfferTypeId.APP)]
    [InlineData(OfferTypeId.SERVICE)]
    public async Task UpdateTechnicalUserProfiles_WithUserNotInProvidingCompany_ThrowsException(OfferTypeId offerTypeId)
    {
        // Arrange
        var offerId = _fixture.Create<Guid>();
        var userRole1Id = _fixture.Create<Guid>();
        var userRole2Id = _fixture.Create<Guid>();
        var data = new[]
        {
            new TechnicalUserProfileData(null, new []
            {
                userRole1Id,
                userRole2Id,
            }),
        };
        A.CallTo(() => _technicalUserProfileRepository.GetOfferProfileData(offerId, offerTypeId, _identity.CompanyId))
            .Returns(new OfferProfileData(false, new[] { ServiceTypeId.DATASPACE_SERVICE }, Enumerable.Empty<(Guid TechnicalUserProfileId, IEnumerable<Guid> UserRoleIds)>()));
        A.CallTo(() => _userRolesRepository.GetRolesForClient("cl1"))
            .Returns(new Guid[] { userRole1Id, userRole2Id }.ToAsyncEnumerable());

        // Act
        async Task Act() => await _sut.UpdateTechnicalUserProfiles(offerId, offerTypeId, data, _identity.CompanyId, "cl1").ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be($"Company {_identity.CompanyId} is not the providing company");
    }

    [Theory]
    [InlineData(OfferTypeId.APP)]
    [InlineData(OfferTypeId.SERVICE)]
    public async Task UpdateTechnicalUserProfiles_WithoutOffer_ThrowsException(OfferTypeId offerTypeId)
    {
        // Arrange
        var offerId = _fixture.Create<Guid>();
        var userRole1Id = _fixture.Create<Guid>();
        var userRole2Id = _fixture.Create<Guid>();
        var data = new[]
        {
            new TechnicalUserProfileData(null, new []
            {
                userRole1Id,
                userRole2Id,
            }),
        };
        A.CallTo(() => _technicalUserProfileRepository.GetOfferProfileData(offerId, offerTypeId, _identity.CompanyId))
            .Returns((OfferProfileData?)null);
        A.CallTo(() => _userRolesRepository.GetRolesForClient("cl1"))
            .Returns(new Guid[] { userRole1Id, userRole2Id }.ToAsyncEnumerable());

        // Act
        async Task Act() => await _sut.UpdateTechnicalUserProfiles(offerId, offerTypeId, data, _identity.CompanyId, "cl1").ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"Offer {offerTypeId} {offerId} does not exist");
    }

    [Theory]
    [InlineData(OfferTypeId.APP)]
    [InlineData(OfferTypeId.SERVICE)]
    public async Task UpdateTechnicalUserProfiles_WithoutTechnicalUserProfileAndUserRole_ThrowsException(OfferTypeId offerTypeId)
    {
        // Arrange
        var offerId = _fixture.Create<Guid>();
        var data = new[]
        {
            new TechnicalUserProfileData(null, Enumerable.Empty<Guid>())
        };

        // Act
        async Task Act() => await _sut.UpdateTechnicalUserProfiles(offerId, offerTypeId, data, _identity.CompanyId, "cl1").ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be("Technical User Profiles and Role IDs both should not be empty.");
    }
    #endregion

    #region GetSubscriptionDetailForProvider

    [Fact]
    public async Task GetSubscriptionDetailForProvider_WithNotMatchingUserRoles_ThrowsConfigurationException()
    {
        // Arrange
        var offerId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        var companyAdminRoles = _fixture.CreateMany<UserRoleConfig>().ToImmutableArray();

        SetupGetSubscriptionDetailForProvider();

        // Act
        async Task Act() => await _sut.GetSubscriptionDetailsForProviderAsync(offerId, subscriptionId, _identity.CompanyId, OfferTypeId.SERVICE, companyAdminRoles).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConfigurationException>(Act);
        ex.Message.Should().Contain("invalid configuration, at least one of the configured roles does not exist in the database:");
        A.CallTo(() => _userRolesRepository.GetUserRoleIdsUntrackedAsync(A<IEnumerable<UserRoleConfig>>.That.IsSameSequenceAs(companyAdminRoles)))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerSubscriptionsRepository.GetSubscriptionDetailsForProviderAsync(A<Guid>._, A<Guid>._, A<Guid>._, A<OfferTypeId>._, A<IEnumerable<Guid>>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task GetSubscriptionDetailForProvider_WithNotExistingOffer_ThrowsNotFoundException()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var companyAdminRoles = new[]
            {
                new UserRoleConfig("ClientTest", new[] {"Test"})
            };
        SetupGetSubscriptionDetailForProvider();

        A.CallTo(() => _offerSubscriptionsRepository.GetSubscriptionDetailsForProviderAsync(A<Guid>._, A<Guid>._, A<Guid>._, A<OfferTypeId>._, A<IEnumerable<Guid>>._))
            .Returns(((bool, bool, ProviderSubscriptionDetailData))default);

        // Act
        async Task Act() => await _sut.GetSubscriptionDetailsForProviderAsync(serviceId, subscriptionId, companyId, OfferTypeId.SERVICE, companyAdminRoles).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Contain($"subscription {subscriptionId} for offer {serviceId} of type {OfferTypeId.SERVICE} does not exist");
        A.CallTo(() => _userRolesRepository.GetUserRoleIdsUntrackedAsync(A<IEnumerable<UserRoleConfig>>.That.IsSameSequenceAs(companyAdminRoles)))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerSubscriptionsRepository.GetSubscriptionDetailsForProviderAsync(serviceId, subscriptionId, companyId, OfferTypeId.SERVICE, A<IEnumerable<Guid>>.That.IsSameSequenceAs(new[] { _validUserRoleId })))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetSubscriptionDetailForProvider_WithUserNotInProvidingCompany_ThrowsForbiddenException()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var companyAdminRoles = new[]
        {
            new UserRoleConfig("ClientTest", new[] {"Test"})
        };
        SetupGetSubscriptionDetailForProvider();

        A.CallTo(() => _offerSubscriptionsRepository.GetSubscriptionDetailsForProviderAsync(A<Guid>._, A<Guid>._, A<Guid>._, A<OfferTypeId>._, A<IEnumerable<Guid>>._))
            .Returns((true, false, _fixture.Create<ProviderSubscriptionDetailData>()));

        // Act
        async Task Act() => await _sut.GetSubscriptionDetailsForProviderAsync(serviceId, subscriptionId, companyId, OfferTypeId.SERVICE, companyAdminRoles).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Contain($"Company {companyId} is not part of the Provider company");
        A.CallTo(() => _userRolesRepository.GetUserRoleIdsUntrackedAsync(A<IEnumerable<UserRoleConfig>>.That.IsSameSequenceAs(companyAdminRoles)))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerSubscriptionsRepository.GetSubscriptionDetailsForProviderAsync(serviceId, subscriptionId, companyId, OfferTypeId.SERVICE, A<IEnumerable<Guid>>.That.IsSameSequenceAs(new[] { _validUserRoleId })))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetSubscriptionDetailForProvider_WithValidData_ReturnsExpected()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var companyAdminRoles = new[]
        {
            new UserRoleConfig("ClientTest", new[] {"Test"})
        };
        SetupGetSubscriptionDetailForProvider();

        var data = _fixture.Create<ProviderSubscriptionDetailData>();

        A.CallTo(() => _offerSubscriptionsRepository.GetSubscriptionDetailsForProviderAsync(A<Guid>._, A<Guid>._, A<Guid>._, A<OfferTypeId>._, A<IEnumerable<Guid>>._))
            .Returns((true, true, data));
        // Act
        var result = await _sut.GetSubscriptionDetailsForProviderAsync(serviceId, subscriptionId, companyId, OfferTypeId.SERVICE, companyAdminRoles).ConfigureAwait(false);

        // Assert
        result.Should().Be(data);
        A.CallTo(() => _userRolesRepository.GetUserRoleIdsUntrackedAsync(A<IEnumerable<UserRoleConfig>>.That.IsSameSequenceAs(companyAdminRoles)))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerSubscriptionsRepository.GetSubscriptionDetailsForProviderAsync(serviceId, subscriptionId, companyId, OfferTypeId.SERVICE, A<IEnumerable<Guid>>.That.IsSameSequenceAs(new[] { _validUserRoleId })))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region GetAppSubscriptionDetailForProvider

    [Fact]
    public async Task GetAppSubscriptionDetailForProvider_WithNotMatchingUserRoles_ThrowsConfigurationException()
    {
        // Arrange
        var offerId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var companyAdminRoles = _fixture.CreateMany<UserRoleConfig>().ToImmutableArray();

        SetupGetSubscriptionDetailForProvider();

        // Act
        async Task Act() => await _sut.GetAppSubscriptionDetailsForProviderAsync(offerId, subscriptionId, companyId, OfferTypeId.APP, companyAdminRoles).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConfigurationException>(Act);
        ex.Message.Should().Contain("invalid configuration, at least one of the configured roles does not exist in the database:");
        A.CallTo(() => _userRolesRepository.GetUserRoleIdsUntrackedAsync(A<IEnumerable<UserRoleConfig>>.That.IsSameSequenceAs(companyAdminRoles)))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerSubscriptionsRepository.GetSubscriptionDetailsForProviderAsync(A<Guid>._, A<Guid>._, A<Guid>._, A<OfferTypeId>._, A<IEnumerable<Guid>>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task GetAppSubscriptionDetailForProvider_WithNotExistingOffer_ThrowsNotFoundException()
    {
        // Arrange
        var appId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var companyAdminRoles = new[]
        {
            new UserRoleConfig("ClientTest", new[] {"Test"})
        };
        SetupGetSubscriptionDetailForProvider();

        A.CallTo(() => _offerSubscriptionsRepository.GetAppSubscriptionDetailsForProviderAsync(A<Guid>._, A<Guid>._, A<Guid>._, A<OfferTypeId>._, A<IEnumerable<Guid>>._))
            .Returns(((bool, bool, AppProviderSubscriptionDetailData))default);

        // Act
        async Task Act() => await _sut.GetAppSubscriptionDetailsForProviderAsync(appId, subscriptionId, companyId, OfferTypeId.APP, companyAdminRoles).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Contain($"subscription {subscriptionId} for offer {appId} of type {OfferTypeId.APP} does not exist");
        A.CallTo(() => _userRolesRepository.GetUserRoleIdsUntrackedAsync(A<IEnumerable<UserRoleConfig>>.That.IsSameSequenceAs(companyAdminRoles)))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerSubscriptionsRepository.GetAppSubscriptionDetailsForProviderAsync(appId, subscriptionId, companyId, OfferTypeId.APP, A<IEnumerable<Guid>>.That.IsSameSequenceAs(new[] { _validUserRoleId })))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetAppSubscriptionDetailForProvider_WithUserNotInProvidingCompany_ThrowsForbiddenException()
    {
        // Arrange
        var appId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var companyAdminRoles = new[]
        {
            new UserRoleConfig("ClientTest", new[] {"Test"})
        };
        SetupGetSubscriptionDetailForProvider();

        A.CallTo(() => _offerSubscriptionsRepository.GetAppSubscriptionDetailsForProviderAsync(A<Guid>._, A<Guid>._, A<Guid>._, A<OfferTypeId>._, A<IEnumerable<Guid>>._))
            .Returns((true, false, _fixture.Create<AppProviderSubscriptionDetailData>()));

        // Act
        async Task Act() => await _sut.GetAppSubscriptionDetailsForProviderAsync(appId, subscriptionId, companyId, OfferTypeId.APP, companyAdminRoles).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Contain($"Company {companyId} is not part of the Provider company");
        A.CallTo(() => _userRolesRepository.GetUserRoleIdsUntrackedAsync(A<IEnumerable<UserRoleConfig>>.That.IsSameSequenceAs(companyAdminRoles)))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerSubscriptionsRepository.GetAppSubscriptionDetailsForProviderAsync(appId, subscriptionId, companyId, OfferTypeId.APP, A<IEnumerable<Guid>>.That.IsSameSequenceAs(new[] { _validUserRoleId })))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetAppSubscriptionDetailForProvider_WithValidData_ReturnsExpected()
    {
        // Arrange
        var appId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var companyAdminRoles = new[]
        {
            new UserRoleConfig("ClientTest", new[] {"Test"})
        };
        SetupGetSubscriptionDetailForProvider();

        var data = _fixture.Create<AppProviderSubscriptionDetailData>();

        A.CallTo(() => _offerSubscriptionsRepository.GetAppSubscriptionDetailsForProviderAsync(A<Guid>._, A<Guid>._, A<Guid>._, A<OfferTypeId>._, A<IEnumerable<Guid>>._))
            .Returns((true, true, data));

        // Act
        var result = await _sut.GetAppSubscriptionDetailsForProviderAsync(appId, subscriptionId, companyId, OfferTypeId.APP, companyAdminRoles).ConfigureAwait(false);

        // Assert
        result.Should().Be(data);
        A.CallTo(() => _userRolesRepository.GetUserRoleIdsUntrackedAsync(A<IEnumerable<UserRoleConfig>>.That.IsSameSequenceAs(companyAdminRoles)))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerSubscriptionsRepository.GetAppSubscriptionDetailsForProviderAsync(appId, subscriptionId, companyId, OfferTypeId.APP, A<IEnumerable<Guid>>.That.IsSameSequenceAs(new[] { _validUserRoleId })))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region GetSubscriptionDetailsForSubscriber

    [Fact]
    public async Task GetSubscriptionDetailsForSubscriber_WithNotMatchingUserRoles_ThrowsConfigurationException()
    {
        // Arrange
        var offerId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var companyAdminRoles = _fixture.CreateMany<UserRoleConfig>().ToImmutableArray();

        SetupGetSubscriptionDetailForProvider();

        // Act
        async Task Act() => await _sut.GetSubscriptionDetailsForSubscriberAsync(offerId, subscriptionId, companyId, OfferTypeId.APP, companyAdminRoles).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ConfigurationException>(Act);
        ex.Message.Should().Contain("invalid configuration, at least one of the configured roles does not exist in the database:");
        A.CallTo(() => _userRolesRepository.GetUserRoleIdsUntrackedAsync(A<IEnumerable<UserRoleConfig>>.That.IsSameSequenceAs(companyAdminRoles)))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerSubscriptionsRepository.GetSubscriptionDetailsForSubscriberAsync(A<Guid>._, A<Guid>._, A<Guid>._, A<OfferTypeId>._, A<IEnumerable<Guid>>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task GetSubscriptionDetailsForSubscriber_WithNotExistingOffer_ThrowsNotFoundException()
    {
        // Arrange
        var appId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var companyAdminRoles = new[]
        {
            new UserRoleConfig("ClientTest", new[] {"Test"})
        };
        SetupGetSubscriptionDetailForProvider();

        A.CallTo(() => _offerSubscriptionsRepository.GetSubscriptionDetailsForSubscriberAsync(A<Guid>._, A<Guid>._, A<Guid>._, A<OfferTypeId>._, A<IEnumerable<Guid>>._))
            .Returns(((bool, bool, SubscriberSubscriptionDetailData))default);

        // Act
        async Task Act() => await _sut.GetSubscriptionDetailsForSubscriberAsync(appId, subscriptionId, companyId, OfferTypeId.APP, companyAdminRoles).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Contain($"subscription {subscriptionId} for offer {appId} of type {OfferTypeId.APP} does not exist");
        A.CallTo(() => _userRolesRepository.GetUserRoleIdsUntrackedAsync(A<IEnumerable<UserRoleConfig>>.That.IsSameSequenceAs(companyAdminRoles)))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerSubscriptionsRepository.GetSubscriptionDetailsForSubscriberAsync(appId, subscriptionId, companyId, OfferTypeId.APP, A<IEnumerable<Guid>>.That.IsSameSequenceAs(new[] { _validUserRoleId })))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetSubscriptionDetailsForSubscriber_WithUserNotInProvidingCompany_ThrowsForbiddenException()
    {
        // Arrange
        var appId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var companyAdminRoles = new[]
        {
            new UserRoleConfig("ClientTest", new[] {"Test"})
        };
        SetupGetSubscriptionDetailForProvider();

        A.CallTo(() => _offerSubscriptionsRepository.GetSubscriptionDetailsForSubscriberAsync(A<Guid>._, A<Guid>._, A<Guid>._, A<OfferTypeId>._, A<IEnumerable<Guid>>._))
            .Returns((true, false, _fixture.Create<SubscriberSubscriptionDetailData>()));

        // Act
        async Task Act() => await _sut.GetSubscriptionDetailsForSubscriberAsync(appId, subscriptionId, companyId, OfferTypeId.APP, companyAdminRoles).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Contain($"Company {companyId} is not part of the Subscriber company");
        A.CallTo(() => _userRolesRepository.GetUserRoleIdsUntrackedAsync(A<IEnumerable<UserRoleConfig>>.That.IsSameSequenceAs(companyAdminRoles)))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerSubscriptionsRepository.GetSubscriptionDetailsForSubscriberAsync(appId, subscriptionId, companyId, OfferTypeId.APP, A<IEnumerable<Guid>>.That.IsSameSequenceAs(new[] { _validUserRoleId })))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetSubscriptionDetailsForSubscriber_WithValidData_ReturnsExpected()
    {
        // Arrange
        var appId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var companyAdminRoles = new[]
        {
            new UserRoleConfig("ClientTest", new[] {"Test"})
        };
        SetupGetSubscriptionDetailForProvider();

        var data = _fixture.Create<SubscriberSubscriptionDetailData>();

        A.CallTo(() => _offerSubscriptionsRepository.GetSubscriptionDetailsForSubscriberAsync(A<Guid>._, A<Guid>._, A<Guid>._, A<OfferTypeId>._, A<IEnumerable<Guid>>._))
            .Returns((true, true, data));

        // Act
        var result = await _sut.GetSubscriptionDetailsForSubscriberAsync(appId, subscriptionId, companyId, OfferTypeId.APP, companyAdminRoles).ConfigureAwait(false);

        // Assert
        result.Should().Be(data);
        A.CallTo(() => _userRolesRepository.GetUserRoleIdsUntrackedAsync(A<IEnumerable<UserRoleConfig>>.That.IsSameSequenceAs(companyAdminRoles)))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerSubscriptionsRepository.GetSubscriptionDetailsForSubscriberAsync(appId, subscriptionId, companyId, OfferTypeId.APP, A<IEnumerable<Guid>>.That.IsSameSequenceAs(new[] { _validUserRoleId })))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region  GetCompanySubscribedOfferSubscriptionStatusesForUser

    [Theory]
    [InlineData(OfferTypeId.SERVICE, DocumentTypeId.SERVICE_LEADIMAGE)]
    public async Task GetCompanySubscribedOfferSubscriptionStatusesForUserAsync_ReturnsExpected(OfferTypeId offerTypeId, DocumentTypeId documentTypeId)
    {
        // Arrange
        var data = _fixture.CreateMany<OfferSubscriptionStatusData>(5).ToImmutableArray();
        A.CallTo(() => _offerSubscriptionsRepository.GetOwnCompanySubscribedOfferSubscriptionStatusesUntrackedAsync(A<Guid>._, A<OfferTypeId>._, A<DocumentTypeId>._))
            .Returns((skip, take) => Task.FromResult(new Pagination.Source<OfferSubscriptionStatusData>(data.Length, data.Skip(skip).Take(take)))!);

        // Act
        var result = await _sut.GetCompanySubscribedOfferSubscriptionStatusesForUserAsync(0, 10, _identity.CompanyId, offerTypeId, documentTypeId).ConfigureAwait(false);

        // Assert
        result.Meta.NumberOfElements.Should().Be(5);
        result.Content.Should().HaveCount(5).And.Satisfy(
            x => x.OfferId == data[0].OfferId && x.OfferName == data[0].OfferName && x.Provider == data[0].Provider && x.OfferSubscriptionStatusId == data[0].OfferSubscriptionStatusId && x.DocumentId == data[0].DocumentId,
            x => x.OfferId == data[1].OfferId && x.OfferName == data[1].OfferName && x.Provider == data[1].Provider && x.OfferSubscriptionStatusId == data[1].OfferSubscriptionStatusId && x.DocumentId == data[1].DocumentId,
            x => x.OfferId == data[2].OfferId && x.OfferName == data[2].OfferName && x.Provider == data[2].Provider && x.OfferSubscriptionStatusId == data[2].OfferSubscriptionStatusId && x.DocumentId == data[2].DocumentId,
            x => x.OfferId == data[3].OfferId && x.OfferName == data[3].OfferName && x.Provider == data[3].Provider && x.OfferSubscriptionStatusId == data[3].OfferSubscriptionStatusId && x.DocumentId == data[3].DocumentId,
            x => x.OfferId == data[4].OfferId && x.OfferName == data[4].OfferName && x.Provider == data[4].Provider && x.OfferSubscriptionStatusId == data[4].OfferSubscriptionStatusId && x.DocumentId == data[4].DocumentId
        );
        A.CallTo(() => _offerSubscriptionsRepository.GetOwnCompanySubscribedOfferSubscriptionStatusesUntrackedAsync(_identity.CompanyId, offerTypeId, documentTypeId))
            .MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData(OfferTypeId.SERVICE, DocumentTypeId.SERVICE_LEADIMAGE)]
    public async Task GetCompanySubscribedOfferSubscriptionStatusesForUserAsync_WithQueryNullResult_ReturnsExpected(OfferTypeId offerTypeId, DocumentTypeId documentTypeId)
    {
        // Arrange
        A.CallTo(() => _offerSubscriptionsRepository.GetOwnCompanySubscribedOfferSubscriptionStatusesUntrackedAsync(A<Guid>._, A<OfferTypeId>._, A<DocumentTypeId>._))
            .Returns((skip, take) => Task.FromResult((Pagination.Source<OfferSubscriptionStatusData>?)null));

        // Act
        var result = await _sut.GetCompanySubscribedOfferSubscriptionStatusesForUserAsync(0, 10, _identity.CompanyId, offerTypeId, documentTypeId).ConfigureAwait(false);

        // Assert
        result.Meta.NumberOfElements.Should().Be(0);
        result.Content.Should().BeEmpty();
        A.CallTo(() => _offerSubscriptionsRepository.GetOwnCompanySubscribedOfferSubscriptionStatusesUntrackedAsync(_identity.CompanyId, offerTypeId, documentTypeId))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region Setup

    private void SetupValidateSalesManager()
    {
        var roleIds = _fixture.CreateMany<Guid>(2);
        A.CallTo(() => _userRolesRepository.GetUserRoleIdsUntrackedAsync(A<IEnumerable<UserRoleConfig>>._))
            .Returns(roleIds.ToAsyncEnumerable());
        A.CallTo(() => _userRepository.GetRolesForCompanyUser(A<Guid>._, A<IEnumerable<Guid>>._, A<Guid>.That.Matches(x => x == _companyUser.Id)))
            .Returns((true, roleIds));
        A.CallTo(() => _userRepository.GetRolesForCompanyUser(A<Guid>._, A<IEnumerable<Guid>>._, A<Guid>.That.Matches(x => x == _differentCompanyUserId)))
            .Returns((true, Enumerable.Repeat(roleIds.First(), 1)));
        A.CallTo(() => _userRepository.GetRolesForCompanyUser(A<Guid>._, A<IEnumerable<Guid>>._, A<Guid>.That.Matches(x => x == _noSalesManagerUserId)))
            .Returns((true, Enumerable.Repeat(roleIds.First(), 1)));
    }

    private void SetupRepositories()
    {
        A.CallTo(() => _agreementRepository.CheckAgreementExistsForSubscriptionAsync(A<Guid>.That.Matches(x => x == _existingAgreementId), A<Guid>._, A<OfferTypeId>.That.Matches(x => x == OfferTypeId.SERVICE)))
            .Returns(true);
        A.CallTo(() => _agreementRepository.CheckAgreementExistsForSubscriptionAsync(A<Guid>._, A<Guid>._, A<OfferTypeId>.That.Not.Matches(x => x == OfferTypeId.SERVICE)))
            .Returns(false);
        A.CallTo(() => _agreementRepository.CheckAgreementExistsForSubscriptionAsync(A<Guid>.That.Not.Matches(x => x == _existingAgreementId), A<Guid>._, A<OfferTypeId>._))
            .Returns(false);

        A.CallTo(() => _agreementRepository.CheckAgreementsExistsForSubscriptionAsync(A<IEnumerable<Guid>>.That.Matches(x => x.Any(y => y == _existingAgreementId)), A<Guid>._, A<OfferTypeId>._))
            .Returns(true);
        A.CallTo(() => _agreementRepository.CheckAgreementsExistsForSubscriptionAsync(A<IEnumerable<Guid>>.That.Matches(x => x.All(y => y != _existingAgreementId)), A<Guid>._, A<OfferTypeId>._))
            .Returns(false);

        var offerSubscription = _fixture.Create<OfferSubscription>();
        A.CallTo(() => _offerSubscriptionsRepository.GetCompanyIdWithAssignedOfferForCompanyUserAndSubscriptionAsync(
                A<Guid>.That.Matches(x => x == _existingServiceId), _identity.UserId, A<OfferTypeId>.That.Matches(x => x == OfferTypeId.SERVICE)))
            .Returns((_identity.CompanyId, offerSubscription));
        A.CallTo(() => _offerSubscriptionsRepository.GetCompanyIdWithAssignedOfferForCompanyUserAndSubscriptionAsync(
                A<Guid>.That.Matches(x => x == _existingServiceId), _identity.UserId, A<OfferTypeId>.That.Not.Matches(x => x == OfferTypeId.SERVICE)))
            .Returns((_identity.CompanyId, (OfferSubscription?)null));
        A.CallTo(() => _offerSubscriptionsRepository.GetCompanyIdWithAssignedOfferForCompanyUserAndSubscriptionAsync(A<Guid>.That.Not.Matches(x => x == _existingServiceId), _identity.UserId, A<OfferTypeId>._))
            .Returns((_identity.CompanyId, (OfferSubscription?)null));
        A.CallTo(() => _offerSubscriptionsRepository.GetCompanyIdWithAssignedOfferForCompanyUserAndSubscriptionAsync(
                A<Guid>.That.Matches(x => x == _existingServiceId), A<Guid>.That.Not.Matches(x => x == _identity.UserId),
                A<OfferTypeId>._))
            .Returns(((Guid companyId, OfferSubscription? offerSubscription))default);

        var agreementData = _fixture.CreateMany<AgreementData>(1);
        A.CallTo(() => _agreementRepository.GetOfferAgreementDataForOfferId(A<Guid>.That.Matches(x => x == _existingServiceId), A<OfferTypeId>._))
            .Returns(agreementData.ToAsyncEnumerable());
        A.CallTo(() => _agreementRepository.GetOfferAgreementDataForOfferId(A<Guid>.That.Not.Matches(x => x == _existingServiceId), A<OfferTypeId>._))
            .Returns(Enumerable.Empty<AgreementData>().ToAsyncEnumerable());

        A.CallTo(() => _consentRepository.GetConsentDetailData(A<Guid>.That.Matches(x => x == _validConsentId), A<OfferTypeId>.That.Matches(x => x == OfferTypeId.SERVICE)))
            .Returns(new ConsentDetailData(_validConsentId, "The Company", this._companyUser.Id, ConsentStatusId.ACTIVE, "Agreed"));
        A.CallTo(() => _consentRepository.GetConsentDetailData(A<Guid>.That.Not.Matches(x => x == _validConsentId), A<OfferTypeId>._))
            .Returns((ConsentDetailData?)null);
        A.CallTo(() => _consentRepository.GetConsentDetailData(A<Guid>._, A<OfferTypeId>.That.Not.Matches(x => x == OfferTypeId.SERVICE)))
            .Returns((ConsentDetailData?)null);

        A.CallTo(() => _companyRepository.GetCompanyNameUntrackedAsync(_identity.CompanyId))
            .Returns((true, "the company"));
        A.CallTo(() => _companyRepository.GetCompanyNameUntrackedAsync(A<Guid>.That.Not.Matches(x => x == _identity.CompanyId)))
            .Returns(new ValueTuple<bool, string>());

        A.CallTo(() => _consentAssignedOfferSubscriptionRepository.GetConsentAssignedOfferSubscriptionsForSubscriptionAsync(A<Guid>._, A<IEnumerable<Guid>>.That.Not.Matches(x => x.Any(y => y == _existingAgreementForSubscriptionId))))
            .Returns(Enumerable.Empty<(Guid ConsentId, Guid AgreementId, ConsentStatusId ConsentStatusId)>().ToAsyncEnumerable());

        A.CallTo(() => _languageRepository.GetLanguageCodesUntrackedAsync(A<IEnumerable<string>>.That.Matches(x => x.Count() == 1 && x.All(y => y == "en"))))
            .Returns(new[] { "en" }.ToAsyncEnumerable());
        A.CallTo(() => _languageRepository.GetLanguageCodesUntrackedAsync(A<IEnumerable<string>>.That.Matches(x => x.Count() == 1 && x.All(y => y == "gg"))))
            .Returns(Enumerable.Empty<string>().ToAsyncEnumerable());

        A.CallTo(() => _portalRepositories.GetInstance<IAgreementRepository>()).Returns(_agreementRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IConsentRepository>()).Returns(_consentRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ICompanyRepository>()).Returns(_companyRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IOfferRepository>()).Returns(_offerRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IOfferSubscriptionsRepository>()).Returns(_offerSubscriptionsRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IConsentAssignedOfferSubscriptionRepository>()).Returns(_consentAssignedOfferSubscriptionRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRolesRepository>()).Returns(_userRolesRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ILanguageRepository>()).Returns(_languageRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IDocumentRepository>()).Returns(_documentRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ITechnicalUserProfileRepository>()).Returns(_technicalUserProfileRepository);
        _fixture.Inject(_portalRepositories);
    }

    private IAsyncEnumerator<Guid> SetupServices()
    {
        A.CallTo(() => _notificationService.CreateNotifications(A<IEnumerable<UserRoleConfig>>._,
                A<Guid>._, A<IEnumerable<(string?, NotificationTypeId)>>._, A<Guid>._, A<bool?>._))
            .Returns(new[] { _companyUser.Id }.AsFakeIAsyncEnumerable(out var createNotificationsResultAsyncEnumerator));

        return createNotificationsResultAsyncEnumerator;
    }

    private void SetupGetSubscriptionDetailForProvider()
    {
        A.CallTo(() => _userRolesRepository.GetUserRoleIdsUntrackedAsync(A<IEnumerable<UserRoleConfig>>.That.Matches(x => x.Any(y => y.ClientId == "ClientTest"))))
            .Returns(new[] { _validUserRoleId }.ToAsyncEnumerable());

        A.CallTo(() => _portalRepositories.GetInstance<IOfferSubscriptionsRepository>()).Returns(_offerSubscriptionsRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRolesRepository>()).Returns(_userRolesRepository);
    }

    #endregion
}
