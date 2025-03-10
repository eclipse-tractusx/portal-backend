/********************************************************************************
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Identity;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Notifications.Library;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.Mailing.Library;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Tests.Service;

public class OfferServiceTests
{
    private static readonly Guid CompanyUserCompanyId = new("395f955b-f11b-4a74-ab51-92a526c1973a");
    private readonly IIdentityData _identity;
    private readonly Guid _companyUserId = Guid.NewGuid();
    private readonly Guid _companyId = Guid.NewGuid();
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
    private readonly IMailingProcessCreation _mailingProcessCreation;
    private readonly IDocumentRepository _documentRepository;
    private readonly OfferService _sut;
    private readonly IOfferSetupService _offerSetupService;
    private readonly ITechnicalUserProfileRepository _technicalUserProfileRepository;
    private readonly IConnectorsRepository _connectorsRepository;
    private readonly IIdentityService _identityService;

    public OfferServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.ConfigureFixture();

        var identity = new Identity(Guid.NewGuid(), DateTimeOffset.UtcNow, CompanyUserCompanyId, UserStatusId.ACTIVE, IdentityTypeId.COMPANY_USER);

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
        _mailingProcessCreation = A.Fake<IMailingProcessCreation>();
        _documentRepository = A.Fake<IDocumentRepository>();
        _offerSetupService = A.Fake<IOfferSetupService>();
        _connectorsRepository = A.Fake<IConnectorsRepository>();
        _identity = A.Fake<IIdentityData>();
        _identityService = A.Fake<IIdentityService>();
        A.CallTo(() => _identity.IdentityId).Returns(_companyUserId);
        A.CallTo(() => _identity.IdentityTypeId).Returns(IdentityTypeId.COMPANY_USER);
        A.CallTo(() => _identity.CompanyId).Returns(_companyId);
        A.CallTo(() => _identityService.IdentityData).Returns(_identity);

        _sut = new OfferService(_portalRepositories, _notificationService, _mailingProcessCreation, _identityService, _offerSetupService);

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
        A.CallTo(() => _offerRepository.CreateOffer(A<OfferTypeId>._, A<Guid>._, A<Action<Offer?>>._))
            .ReturnsLazily((OfferTypeId offerType, Guid providerCompanyId, Action<Offer>? setOptionalParameters) =>
            {
                var app = new Offer(serviceId, providerCompanyId, DateTimeOffset.UtcNow, offerType);
                setOptionalParameters?.Invoke(app);
                apps.Add(app);
                return app;
            });

        // Act
        var result = await _sut.CreateServiceOfferingAsync(new ServiceOfferingData("Newest Service", "42", "mail@test.de", _companyUserId, Enumerable.Empty<LocalizedDescription>(), new[] { ServiceTypeId.DATASPACE_SERVICE }, "http://google.com"), OfferTypeId.SERVICE);

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
        A.CallTo(() => _offerRepository.CreateOffer(A<OfferTypeId>._, A<Guid>._, A<Action<Offer?>>._))
            .ReturnsLazily((OfferTypeId offerType, Guid providerCompanyId, Action<Offer>? setOptionalParameters) =>
            {
                var app = new Offer(serviceId, providerCompanyId, DateTimeOffset.UtcNow, offerType);
                setOptionalParameters?.Invoke(app);
                apps.Add(app);
                return app;
            });

        // Act
        var serviceOfferingData = new ServiceOfferingData("Newest Service", "42", "mail@test.de", _companyUserId, new LocalizedDescription[]
        {
            new ("en", "That's a description with a valid language code", "Short description")
        },
        [
            ServiceTypeId.DATASPACE_SERVICE
        ], "http://google.com");
        var result = await _sut.CreateServiceOfferingAsync(serviceOfferingData, OfferTypeId.SERVICE);

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
        async Task Action() => await _sut.CreateServiceOfferingAsync(new ServiceOfferingData("Newest Service", "42", "mail@test.de", _companyUser.Id, Enumerable.Empty<LocalizedDescription>(), serviceTypeIds, "http://google.com"), OfferTypeId.SERVICE);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("ServiceTypeIds");
    }

    [Fact]
    public async Task CreateServiceOffering_WithoutTitle_ThrowsException()
    {
        // Act
        async Task Action() => await _sut.CreateServiceOfferingAsync(new ServiceOfferingData("", "42", "mail@test.de", _companyUser.Id, Enumerable.Empty<LocalizedDescription>(), new[] { ServiceTypeId.DATASPACE_SERVICE }, "http://google.com"), OfferTypeId.SERVICE);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("Title");
    }

    [Fact]
    public async Task CreateServiceOffering_WithInvalidLanguage_ThrowsException()
    {
        // Act
        var serviceOfferingData = new ServiceOfferingData("Newest Service", "42", "mail@test.de", _companyUserId, new LocalizedDescription[]
        {
            new ("gg", "That's a description with incorrect language short code", "Short description")
        }, new[] { ServiceTypeId.DATASPACE_SERVICE }, "http://google.com");
        async Task Action() => await _sut.CreateServiceOfferingAsync(serviceOfferingData, OfferTypeId.SERVICE);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.Parameters.First().Name.Should().Be("languageCodes");
    }

    [Fact]
    public async Task CreateServiceOffering_WithoutCompanyUser_ThrowsException()
    {
        // Act
        async Task Action() => await _sut.CreateServiceOfferingAsync(new ServiceOfferingData("Newest Service", "42", "mail@test.de", Guid.NewGuid(), Enumerable.Empty<LocalizedDescription>(), new[] { ServiceTypeId.DATASPACE_SERVICE }, "http://google.com"), OfferTypeId.SERVICE);

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
        var result = await _sut.GetOfferAgreementsAsync(_existingServiceId, OfferTypeId.SERVICE).ToListAsync();

        // Assert
        result.Should().ContainSingle();
    }

    [Fact]
    public async Task GetOfferAgreement_WithoutExistingService_ThrowsException()
    {
        // Act
        var agreementData = await _sut.GetOfferAgreementsAsync(Guid.NewGuid(), OfferTypeId.SERVICE).ToListAsync();

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
        var result = await _sut.CreateOfferSubscriptionAgreementConsentAsync(_existingServiceId, _existingAgreementId, statusId, OfferTypeId.SERVICE);

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
        async Task Action() => await _sut.CreateOfferSubscriptionAgreementConsentAsync(_existingServiceId, nonExistingAgreementId, ConsentStatusId.ACTIVE, OfferTypeId.SERVICE);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.Message.Should().Be(OfferServiceErrors.INVALID_AGREEMENT.ToString());
    }

    [Fact]
    public async Task CreateOfferAgreementConsentAsync_WithWrongUser_ThrowsException()
    {
        A.CallTo(() => _identity.IdentityId).Returns(Guid.NewGuid());

        // Act
        async Task Action() => await _sut.CreateOfferSubscriptionAgreementConsentAsync(_existingServiceId, _existingAgreementId, ConsentStatusId.ACTIVE, OfferTypeId.SERVICE);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("IdentityId");
    }

    [Fact]
    public async Task CreateOfferAgreementConsentAsync_WithNotExistingService_ThrowsException()
    {
        // Arrange
        var notExistingServiceId = Guid.NewGuid();

        // Act
        async Task Action() => await _sut.CreateOfferSubscriptionAgreementConsentAsync(notExistingServiceId, _existingAgreementId, ConsentStatusId.ACTIVE, OfferTypeId.SERVICE);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Action);
        ex.Message.Should().Be(OfferServiceErrors.INVALID_OFFERSUBSCRIPTION.ToString());
    }

    [Fact]
    public async Task CreateOfferAgreementConsentAsync_WithInvalidOfferType_ThrowsException()
    {
        // Act
        async Task Action() => await _sut.CreateOfferSubscriptionAgreementConsentAsync(_existingServiceId, _existingAgreementId, ConsentStatusId.ACTIVE, OfferTypeId.APP);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Action);
        ex.Message.Should().Be(OfferServiceErrors.INVALID_OFFERSUBSCRIPTION.ToString());
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
        await _sut.CreateOrUpdateOfferSubscriptionAgreementConsentAsync(_existingServiceId, data, OfferTypeId.SERVICE);

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
        async Task Action() => await _sut.CreateOrUpdateOfferSubscriptionAgreementConsentAsync(_existingServiceId, data, OfferTypeId.SERVICE);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.Message.Should().Be(OfferServiceErrors.INVALID_AGREEMENTS.ToString());
    }

    [Fact]
    public async Task CreateOrUpdateServiceAgreementConsentAsync_WithWrongUser_ThrowsException()
    {
        // Arrange
        var data = new OfferAgreementConsentData[]
        {
            new(_existingAgreementId, ConsentStatusId.ACTIVE)
        };
        A.CallTo(() => _identity.IdentityId).Returns(Guid.NewGuid());

        // Act
        async Task Action() => await _sut.CreateOrUpdateOfferSubscriptionAgreementConsentAsync(_existingServiceId, data, OfferTypeId.SERVICE);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Action);
        ex.ParamName.Should().Be("IdentityId");
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
        async Task Action() => await _sut.CreateOrUpdateOfferSubscriptionAgreementConsentAsync(notExistingServiceId, data, OfferTypeId.SERVICE);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Action);
        ex.Message.Should().Be(OfferServiceErrors.INVALID_OFFERSUBSCRIPTION.ToString());
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
        async Task Action() => await _sut.CreateOrUpdateOfferSubscriptionAgreementConsentAsync(_existingServiceId, data, OfferTypeId.APP);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Action);
        ex.Message.Should().Be(OfferServiceErrors.INVALID_OFFERSUBSCRIPTION.ToString());
    }

    #endregion

    #region Get Consent Detail Data

    [Fact]
    public async Task GetServiceConsentDetailData_WithValidId_ReturnsServiceConsentDetailData()
    {
        // Act
        var result = await _sut.GetConsentDetailDataAsync(_validConsentId, OfferTypeId.SERVICE);

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
        async Task Action() => await _sut.GetConsentDetailDataAsync(notExistingId, OfferTypeId.SERVICE);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Action);
        ex.Message.Should().Be(OfferServiceErrors.CONSENT_NOT_EXIST.ToString());
    }

    #endregion

    #region Validate SalesManager

    [Fact]
    public async Task AddAppAsync_WithInvalidSalesManager_ThrowsException()
    {
        // Arrange
        SetupValidateSalesManager();

        //null Act
        async Task Act() => await _sut.ValidateSalesManager(Guid.NewGuid(), Enumerable.Empty<UserRoleConfig>());

        // Assert
        var error = await Assert.ThrowsAsync<ForbiddenException>(Act);
        error.Message.Should().Be(OfferServiceErrors.SALESMANAGER_NOT_MEMBER_OF_COMPANY.ToString());
    }

    [Fact]
    public async Task AddAppAsync_WithUserFromOtherCompany_ThrowsException()
    {
        // Arrange
        SetupValidateSalesManager();

        // Act
        async Task Act() => await _sut.ValidateSalesManager(_differentCompanyUserId, Enumerable.Empty<UserRoleConfig>());

        // Assert
        var error = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        error.Message.Should().Be(OfferServiceErrors.USER_NOT_SALESMANAGER.ToString());
    }

    [Fact]
    public async Task AddAppAsync_WithUserWithoutSalesManagerRole_ThrowsException()
    {
        // Arrange
        SetupValidateSalesManager();

        // Act
        async Task Act() => await _sut.ValidateSalesManager(_noSalesManagerUserId, Enumerable.Empty<UserRoleConfig>());

        // Assert
        var error = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        error.Parameters.First().Name.Should().Be("salesManagerId");
    }

    #endregion

    #region UpsertRemoveOfferDescription

    [Fact]
    public void UpsertRemoveOfferDescription_ReturnsExpected()
    {
        // Arrange
        var seedOfferId = _fixture.Create<Guid>();
        var seed = new Dictionary<(Guid, string), OfferDescription>() {
            {(seedOfferId, "de"), new(seedOfferId, "de", _fixture.Create<string>(), _fixture.Create<string>())},
            {(seedOfferId, "en"), new(seedOfferId, "en", _fixture.Create<string>(), _fixture.Create<string>())},
            {(seedOfferId, "fr"), new(seedOfferId, "fr", _fixture.Create<string>(), _fixture.Create<string>())},
            {(seedOfferId, "cz"), new(seedOfferId, "cz", _fixture.Create<string>(), _fixture.Create<string>())},
            {(seedOfferId, "it"), new(seedOfferId, "it", _fixture.Create<string>(), _fixture.Create<string>())},
        };

        var updateDescriptions = new LocalizedDescription[] {
            new("de", _fixture.Create<string>(), _fixture.Create<string>()),
            new("fr", _fixture.Create<string>(), _fixture.Create<string>()),
            new("sk", _fixture.Create<string>(), _fixture.Create<string>()),
            new("se", _fixture.Create<string>(), _fixture.Create<string>()),
            new("it", null!,null!)
        };

        var transformedUpdateDescriptions = updateDescriptions.Select(x => (x.LanguageCode, x.LongDescription, x.ShortDescription));

        var existingDescriptions = seed.Select((x) => x.Value).Select(y => new LocalizedDescription(y.LanguageShortName, y.DescriptionLong, y.DescriptionShort)).ToList();

        _sut.UpsertRemoveOfferDescription(seedOfferId, updateDescriptions, existingDescriptions);

        // Assert
        A.CallTo(() => _offerRepository.CreateUpdateDeleteOfferDescriptions(seedOfferId, A<IEnumerable<LocalizedDescription>>.That.IsSameSequenceAs(existingDescriptions),
            A<IEnumerable<(string, string, string)>>.That.IsSameSequenceAs(transformedUpdateDescriptions))).MustHaveHappenedOnceExactly();
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
        A.CallTo(() => _offerRepository.GetOfferReleaseDataByIdAsync(notExistingOffer, offerType)).Returns<OfferReleaseData?>(null);

        // Act
        async Task Act() => await _sut.SubmitOfferAsync(notExistingOffer, offerType, new[] { NotificationTypeId.APP_SUBSCRIPTION_REQUEST }, Enumerable.Empty<UserRoleConfig>(), new[] { DocumentTypeId.CONFORMITY_APPROVAL_BUSINESS_APPS });

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be(OfferServiceErrors.OFFER_DOES_NOT_EXIST.ToString());
    }

    [Theory]
    [InlineData(null, false, false, true)]
    [InlineData("name", true, false, true)]
    [InlineData("name", false, true, true)]
    [InlineData("name", true, true, false)]
    public async Task SubmitOffer_WithInvalidOffer_ThrowsConflictException(string? name, bool isDescriptionLongNotSet, bool isDescriptionShortNotSet, bool hasUserRoles)
    {
        // Arrange
        A.CallTo(() => _offerRepository.GetOfferReleaseDataByIdAsync(A<Guid>._, A<OfferTypeId>._)).Returns(new OfferReleaseData(name, _fixture.Create<string>(), isDescriptionLongNotSet, isDescriptionShortNotSet, hasUserRoles, true, new[] { (Guid.NewGuid(), DocumentStatusId.PENDING, DocumentTypeId.CONFORMITY_APPROVAL_BUSINESS_APPS) }));

        // Act
        async Task Act() => await _sut.SubmitOfferAsync(Guid.NewGuid(), _fixture.Create<OfferTypeId>(), _fixture.CreateMany<NotificationTypeId>(1), _fixture.CreateMany<UserRoleConfig>(), new[] { DocumentTypeId.CONFORMITY_APPROVAL_BUSINESS_APPS });

        // Assert
        var result = await Assert.ThrowsAsync<ConflictException>(Act);
        if (hasUserRoles)
        {
            result.Message.Should().StartWith(OfferServiceErrors.MISSING_PROPERTIES.ToString());
        }
        else
        {
            result.Message.Should().Be(OfferServiceErrors.APP_NO_ROLES_ASSIGNED.ToString());
        }
    }

    [Fact]
    public async Task SubmitOffer_WithInvalidDocumentType_ThrowsConflictException()
    {
        // Arrange
        var data = _fixture.Build<OfferReleaseData>()
            .With(x => x.IsDescriptionLongNotSet, false)
            .With(x => x.IsDescriptionShortNotSet, false)
            .With(x => x.DocumentDatas, [(Guid.NewGuid(), DocumentStatusId.PENDING, DocumentTypeId.CONFORMITY_APPROVAL_BUSINESS_APPS)])
            .Create();
        var submitAppDocumentTypeIds = new[] { DocumentTypeId.APP_IMAGE, DocumentTypeId.APP_LEADIMAGE, DocumentTypeId.CONFORMITY_APPROVAL_BUSINESS_APPS };
        var missingAppDocumentTypeIds = new[] { DocumentTypeId.APP_IMAGE, DocumentTypeId.APP_LEADIMAGE };
        A.CallTo(() => _offerRepository.GetOfferReleaseDataByIdAsync(A<Guid>._, A<OfferTypeId>._)).Returns(data);

        // Act
        async Task Act() => await _sut.SubmitOfferAsync(Guid.NewGuid(), _fixture.Create<OfferTypeId>(), _fixture.CreateMany<NotificationTypeId>(1), _fixture.CreateMany<UserRoleConfig>(), submitAppDocumentTypeIds);

        // Assert
        var result = await Assert.ThrowsAsync<ConflictException>(Act);
        result.Message.Should().StartWith(OfferServiceErrors.MANDATORY_DOCUMENT_TYPES_MISSING.ToString());
    }

    [Fact]
    public async Task SubmitOffer_WithInvalidUserRole_ThrowsConflictException()
    {
        // Arrange
        var data = _fixture.Build<OfferReleaseData>()
            .With(x => x.IsDescriptionLongNotSet, false)
            .With(x => x.IsDescriptionShortNotSet, false)
            .With(x => x.HasUserRoles, false)
            .With(x => x.DocumentDatas, [(Guid.NewGuid(), DocumentStatusId.PENDING, DocumentTypeId.CONFORMITY_APPROVAL_BUSINESS_APPS)])
            .Create();

        A.CallTo(() => _offerRepository.GetOfferReleaseDataByIdAsync(A<Guid>._, A<OfferTypeId>._)).Returns(data);

        // Act
        async Task Act() => await _sut.SubmitOfferAsync(Guid.NewGuid(), _fixture.Create<OfferTypeId>(), _fixture.CreateMany<NotificationTypeId>(1), _fixture.CreateMany<UserRoleConfig>(), new[] { DocumentTypeId.CONFORMITY_APPROVAL_BUSINESS_APPS });

        // Assert
        var result = await Assert.ThrowsAsync<ConflictException>(Act);
        result.Message.Should().StartWith(OfferServiceErrors.APP_NO_ROLES_ASSIGNED.ToString());
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
            .With(x => x.DocumentDatas, [
                (Guid.NewGuid(), DocumentStatusId.PENDING, DocumentTypeId.CONFORMITY_APPROVAL_BUSINESS_APPS),
                (Guid.NewGuid(), DocumentStatusId.INACTIVE, DocumentTypeId.APP_LEADIMAGE),
                (Guid.NewGuid(), DocumentStatusId.LOCKED, DocumentTypeId.APP_IMAGE)])
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
                        var document = new Document(x.DocumentId, null!, null!, null!, default, default, default, default, default);
                        x.Initialize?.Invoke(document);
                        return document;
                    }
                ).ToImmutableArray();
                modified = data.Select(x =>
                    {
                        var document = new Document(x.DocumentId, null!, null!, null!, default, default, default, default, default);
                        x.Modify(document);
                        return document;
                    }
                ).ToImmutableArray();
            });

        A.CallTo(() => _offerRepository.AttachAndModifyOffer(offerId, A<Action<Offer>>._, A<Action<Offer>?>._)).Invokes(
            (Guid _,
                Action<Offer> setOptionalParameters,
                Action<Offer>? initializeParemeter) =>
            {
                initializeParemeter?.Invoke(offer);
                setOptionalParameters.Invoke(offer);
            });

        // Act
        await _sut.SubmitOfferAsync(offerId, offerType, new[] { NotificationTypeId.APP_SUBSCRIPTION_REQUEST }, _fixture.CreateMany<UserRoleConfig>(), new[] { DocumentTypeId.CONFORMITY_APPROVAL_BUSINESS_APPS, DocumentTypeId.APP_LEADIMAGE, DocumentTypeId.APP_IMAGE });

        // Assert
        A.CallTo(() => _notificationService.CreateNotifications(A<IEnumerable<UserRoleConfig>>._, _companyUserId, A<IEnumerable<(string? content, NotificationTypeId notifcationTypeId)>>._, false)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _documentRepository.AttachAndModifyDocuments(A<IEnumerable<(Guid, Action<Document>?, Action<Document>)>>._)).MustHaveHappenedOnceExactly();
        initial.Should().NotBeNull().And.HaveCount(1).And.Satisfy(x => x.Id == data.DocumentDatas.ElementAt(0).DocumentId && x.DocumentStatusId == DocumentStatusId.PENDING);
        modified.Should().NotBeNull().And.HaveCount(1).And.Satisfy(x => x.Id == data.DocumentDatas.ElementAt(0).DocumentId && x.DocumentStatusId == DocumentStatusId.LOCKED);
        A.CallTo(() => _offerRepository.AttachAndModifyOffer(offerId, A<Action<Offer>>._, A<Action<Offer>?>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task SubmitOffer_WithInvalidPrivacyPolicies_ThrowsConflictException()
    {
        // Arrange
        var data = _fixture.Build<OfferReleaseData>()
            .With(x => x.IsDescriptionLongNotSet, false)
            .With(x => x.IsDescriptionShortNotSet, false)
            .With(x => x.HasUserRoles, true)
            .With(x => x.HasPrivacyPolicies, false)
            .With(x => x.DocumentDatas, [(Guid.NewGuid(), DocumentStatusId.PENDING, DocumentTypeId.CONFORMITY_APPROVAL_BUSINESS_APPS)])
            .Create();

        A.CallTo(() => _offerRepository.GetOfferReleaseDataByIdAsync(A<Guid>._, A<OfferTypeId>._)).Returns(data);
        var sut = new OfferService(_portalRepositories, null!, null!, _identityService, _offerSetupService);

        // Act
        async Task Act() => await sut.SubmitOfferAsync(Guid.NewGuid(), _fixture.Create<OfferTypeId>(), _fixture.CreateMany<NotificationTypeId>(1), _fixture.CreateMany<UserRoleConfig>(), new[] { DocumentTypeId.CONFORMITY_APPROVAL_BUSINESS_APPS });

        // Assert
        var result = await Assert.ThrowsAsync<ConflictException>(Act);
        result.Message.Should().StartWith(OfferServiceErrors.PRIVACYPOLICIES_MISSING.ToString());
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
        var recipients = new[]
        {
            new UserRoleConfig("catenax-portal", new [] { "Sales Manager", "App Manager" })
        };
        var subscriptionUrl = _fixture.Create<string>();
        var detailUrl = _fixture.Create<string>();

        var mailParameters = new[]
        {
            ("offerName", offer.Name),
            ("offerSubscriptionUrl", subscriptionUrl),
            ("offerDetailUrl", $"{detailUrl}/{offer.Id}"),
            ("appId", offer.Id.ToString())
        };
        var userNameParameter = ("offerProviderName", "User");
        var template = new[]
        {
            "app-release-activation"
        };

        //Act
        await _sut.ApproveOfferRequestAsync(offer.Id, OfferTypeId.APP, approveAppNotificationTypeIds, approveAppUserRoles, new[] { NotificationTypeId.APP_SUBSCRIPTION_REQUEST }, _fixture.CreateMany<UserRoleConfig>(), (subscriptionUrl, detailUrl), recipients);

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
        A.CallTo(() => _mailingProcessCreation.RoleBaseSendMail(
            A<IEnumerable<UserRoleConfig>>.That.IsSameSequenceAs(recipients),
            A<IEnumerable<(string, string)>>.That.IsSameSequenceAs(mailParameters),
            userNameParameter,
            A<IEnumerable<string>>.That.IsSameSequenceAs(template),
            companyId)).MustHaveHappenedOnceExactly();
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
        var recipients = new[]
        {
            new UserRoleConfig("catenax-portal", new [] { "Sales Manager", "App Manager" })
        };
        var subscriptionUrl = _fixture.Create<string>();
        var detailUrl = _fixture.Create<string>();

        //Act
        Task Act() => _sut.ApproveOfferRequestAsync(offerId, OfferTypeId.APP, approveAppNotificationTypeIds, approveAppUserRoles, new[] { NotificationTypeId.APP_SUBSCRIPTION_REQUEST }, _fixture.CreateMany<UserRoleConfig>(), (subscriptionUrl, detailUrl), recipients);

        //Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be(OfferServiceErrors.OFFERID_NAME_NOT_SET.ToString());
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
        var recipients = new[]
        {
            new UserRoleConfig("catenax-portal", new [] { "Sales Manager", "App Manager" })
        };
        var subscriptionUrl = _fixture.Create<string>();
        var detailUrl = _fixture.Create<string>();

        //Act
        Task Act() => _sut.ApproveOfferRequestAsync(offerId, OfferTypeId.APP, approveAppNotificationTypeIds, approveAppUserRoles, new[] { NotificationTypeId.APP_SUBSCRIPTION_REQUEST }, _fixture.CreateMany<UserRoleConfig>(), (subscriptionUrl, detailUrl), recipients);

        //Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be(OfferServiceErrors.OFFERID_PROVIDING_COMPANY_NOT_SET.ToString());
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
        A.CallTo(() => _offerRepository.GetOfferReleaseDataByIdAsync(notExistingOffer, offerType)).Returns<OfferReleaseData?>(null);

        // Act
        async Task Act() => await _sut.SubmitServiceAsync(notExistingOffer, offerType, new[] { NotificationTypeId.APP_SUBSCRIPTION_REQUEST }, _fixture.CreateMany<UserRoleConfig>());

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be(OfferServiceErrors.OFFER_DOES_NOT_EXIST.ToString());
    }

    [Theory]
    [InlineData(null, false, false, true)]
    [InlineData("name", true, false, true)]
    [InlineData("name", false, true, true)]
    public async Task SubmitService_WithInvalidOffer_ThrowsConflictException(string? name, bool isDescriptionLongNotSet, bool isDescriptionShortNotSet, bool hasUserRoles)
    {
        // Arrange
        A.CallTo(() => _offerRepository.GetOfferReleaseDataByIdAsync(A<Guid>._, A<OfferTypeId>._)).Returns(new OfferReleaseData(name, _fixture.Create<string>(), isDescriptionLongNotSet, isDescriptionShortNotSet, hasUserRoles, true, new[] { (Guid.NewGuid(), DocumentStatusId.PENDING, DocumentTypeId.CONFORMITY_APPROVAL_BUSINESS_APPS) }));

        // Act
        async Task Act() => await _sut.SubmitServiceAsync(Guid.NewGuid(), _fixture.Create<OfferTypeId>(), _fixture.CreateMany<NotificationTypeId>(1), _fixture.CreateMany<UserRoleConfig>());

        // Assert
        var result = await Assert.ThrowsAsync<ConflictException>(Act);
        result.Message.Should().StartWith(OfferServiceErrors.MISSING_PROPERTIES.ToString());
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
            .With(x => x.DocumentDatas, [
                (Guid.NewGuid(), DocumentStatusId.PENDING, DocumentTypeId.CONFORMITY_APPROVAL_BUSINESS_APPS),
                (Guid.NewGuid(), DocumentStatusId.INACTIVE, DocumentTypeId.CONFORMITY_APPROVAL_BUSINESS_APPS)])
            .Create();
        A.CallTo(() => _offerRepository.GetOfferReleaseDataByIdAsync(offerId, offerType)).Returns(data);

        A.CallTo(() => _documentRepository.AttachAndModifyDocument(A<Guid>._, A<Action<Document>>._, A<Action<Document>>._))
            .Invokes((Guid docId, Action<Document>? initialize, Action<Document> modify)
                =>
            {
                var document = new Document(docId, null!, null!, null!, default, default, default, default, default);
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
                        var document = new Document(x.DocumentId, null!, null!, null!, default, default, default, default, default);
                        x.Initialize?.Invoke(document);
                        return document;
                    }
                ).ToImmutableArray();
                modified = data.Select(x =>
                    {
                        var document = new Document(x.DocumentId, null!, null!, null!, default, default, default, default, default);
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
        await _sut.SubmitServiceAsync(offerId, offerType, new[] { NotificationTypeId.APP_SUBSCRIPTION_REQUEST }, _fixture.CreateMany<UserRoleConfig>());

        // Assert
        A.CallTo(() => _notificationService.CreateNotifications(A<IEnumerable<UserRoleConfig>>._, _companyUserId, A<IEnumerable<(string? content, NotificationTypeId notifcationTypeId)>>._, A<bool?>._)).MustHaveHappenedOnceExactly();
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
            .Returns<(string?, OfferStatusId, Guid?, IEnumerable<DocumentStatusData>)>(default);

        // Act
        async Task Act() => await _sut.DeclineOfferAsync(notExistingOffer, new OfferDeclineRequest("Test"), offerTypeId, NotificationTypeId.SERVICE_RELEASE_REJECTION, Enumerable.Empty<UserRoleConfig>(), string.Empty, new[] { NotificationTypeId.APP_SUBSCRIPTION_REQUEST }, _fixture.CreateMany<UserRoleConfig>());

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(Act);
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
        async Task Act() => await _sut.DeclineOfferAsync(notExistingOffer, new OfferDeclineRequest("Test"), offerTypeId, NotificationTypeId.SERVICE_RELEASE_REJECTION, Enumerable.Empty<UserRoleConfig>(), string.Empty, new[] { NotificationTypeId.APP_SUBSCRIPTION_REQUEST }, _fixture.CreateMany<UserRoleConfig>());

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be(OfferServiceErrors.OFFER_STATUS_IN_REVIEW.ToString());
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
            .Returns((null, OfferStatusId.IN_REVIEW, Guid.NewGuid(), documentStatusDatas));

        // Act
        async Task Act() => await _sut.DeclineOfferAsync(notExistingOffer, new OfferDeclineRequest("Test"), offerTypeId, NotificationTypeId.SERVICE_RELEASE_REJECTION, Enumerable.Empty<UserRoleConfig>(), string.Empty, new[] { NotificationTypeId.APP_SUBSCRIPTION_REQUEST }, _fixture.CreateMany<UserRoleConfig>());

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be(OfferServiceErrors.OFFER_NAME_NOT_SET.ToString());
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
        async Task Act() => await _sut.DeclineOfferAsync(notExistingOffer, new OfferDeclineRequest("Test"), offerTypeId, NotificationTypeId.SERVICE_RELEASE_REJECTION, Enumerable.Empty<UserRoleConfig>(), string.Empty, new[] { NotificationTypeId.APP_SUBSCRIPTION_REQUEST }, _fixture.CreateMany<UserRoleConfig>());

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be(OfferServiceErrors.OFFER_PROVIDING_COMPANY_NOT_SET.ToString());
    }

    [Theory]
    [InlineData(OfferTypeId.APP)]
    [InlineData(OfferTypeId.SERVICE)]
    public async Task DeclineOfferAsync_WithValidData_CallsExpected(OfferTypeId offerTypeId)
    {
        // Arrange
        var offer = _fixture.Create<Offer>();
        var offerId = _fixture.Create<Guid>();
        var recipients = new[]
        {
            new UserRoleConfig("Test", new[] { "Abc" })
        };
        var roleIds = _fixture.CreateMany<Guid>();
        var documentStatusDatas = new DocumentStatusData[]
        {
            new(Guid.NewGuid(), DocumentStatusId.LOCKED),
            new(Guid.NewGuid(), DocumentStatusId.PENDING),
        };
        var companyId = Guid.NewGuid();
        var offerName = _fixture.Create<string>();
        A.CallTo(() => _offerRepository.GetOfferDeclineDataAsync(offerId, offerTypeId))
            .Returns((offerName, OfferStatusId.IN_REVIEW, companyId, documentStatusDatas));
        A.CallTo(() => _offerRepository.AttachAndModifyOffer(offerId, A<Action<Offer>>._, A<Action<Offer>?>._))
            .Invokes((Guid _, Action<Offer> setOptionalParameters, Action<Offer>? initializeParemeters) =>
            {
                initializeParemeters?.Invoke(offer);
                setOptionalParameters.Invoke(offer);
            });
        A.CallTo(() => _userRolesRepository.GetUserRoleIdsUntrackedAsync(A<IEnumerable<UserRoleConfig>>._))
            .Returns(roleIds.ToAsyncEnumerable());
        A.CallTo(() => _userRepository.GetCompanyUserEmailForCompanyAndRoleId(A<IEnumerable<Guid>>._, A<Guid>._))
            .Returns(new (string, string?, string?)[] { ("test@email.com", "Test User 1", "cx-user-2") }.ToAsyncEnumerable());

        IEnumerable<Document>? initial = null;
        IEnumerable<Document>? modified = null;
        A.CallTo(() => _documentRepository.AttachAndModifyDocuments(A<IEnumerable<(Guid DocumentId, Action<Document>? Initialize, Action<Document> Modify)>>._))
            .Invokes((IEnumerable<(Guid DocumentId, Action<Document>? Initialize, Action<Document> Modify)> data) =>
            {
                initial = data.Select(x =>
                    {
                        var document = new Document(x.DocumentId, null!, null!, null!, default, default, default, default, default);
                        x.Initialize?.Invoke(document);
                        return document;
                    }
                ).ToImmutableArray();
                modified = data.Select(x =>
                    {
                        var document = new Document(x.DocumentId, null!, null!, null!, default, default, default, default, default);
                        x.Modify(document);
                        return document;
                    }
                ).ToImmutableArray();
            });

        var declineMessage = _fixture.Create<string>();
        var basePortalAddress = _fixture.Create<string>();

        var mailParameters = new[]
        {
            ("offerName", offerName),
            ("url", basePortalAddress),
            ("declineMessage", declineMessage)
        };
        var userNameParameter = ("offerProviderName", "Service Manager");
        var template = new[]
        {
            $"{offerTypeId.ToString().ToLower()}-request-decline"
        };

        // Act
        await _sut.DeclineOfferAsync(offerId, new OfferDeclineRequest(declineMessage), offerTypeId, NotificationTypeId.SERVICE_RELEASE_REJECTION, recipients, basePortalAddress, new[] { NotificationTypeId.APP_SUBSCRIPTION_REQUEST }, _fixture.CreateMany<UserRoleConfig>());

        // Assert
        A.CallTo(() => _offerRepository.AttachAndModifyOffer(offerId, A<Action<Offer>>._, A<Action<Offer>?>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _notificationService.CreateNotifications(A<IEnumerable<UserRoleConfig>>._, A<Guid>._, A<IEnumerable<(string? content, NotificationTypeId notificationTypeId)>>._, A<Guid>._, A<bool?>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _createNotificationsEnumerator.MoveNextAsync()).MustHaveHappened(2, Times.Exactly);
        A.CallTo(() => _mailingProcessCreation.RoleBaseSendMail(
            A<IEnumerable<UserRoleConfig>>.That.IsSameSequenceAs(recipients),
            A<IEnumerable<(string, string)>>.That.IsSameSequenceAs(mailParameters),
            userNameParameter,
            A<IEnumerable<string>>.That.IsSameSequenceAs(template),
            companyId)).MustHaveHappenedOnceExactly();
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
        A.CallTo(() => _offerRepository.GetOfferActiveStatusDataByIdAsync(notExistingId, offerTypeId, _companyId))
            .Returns<(bool, bool)>(default);

        // Act
        async Task Act() => await _sut.DeactivateOfferIdAsync(notExistingId, offerTypeId);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be(OfferServiceErrors.OFFER_DOES_NOT_EXIST.ToString());
    }

    [Theory]
    [InlineData(OfferTypeId.APP)]
    [InlineData(OfferTypeId.SERVICE)]
    public async Task DeactivateOfferStatusIdAsync_WithNotAssignedUser_ThrowsForbiddenException(OfferTypeId offerTypeId)
    {
        // Arrange
        var offerId = _fixture.Create<Guid>();
        A.CallTo(() => _offerRepository.GetOfferActiveStatusDataByIdAsync(offerId, offerTypeId, _companyId))
            .Returns((true, false));

        // Act
        async Task Act() => await _sut.DeactivateOfferIdAsync(offerId, offerTypeId);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be(OfferServiceErrors.MISSING_PERMISSION.ToString());

    }

    [Theory]
    [InlineData(OfferTypeId.APP)]
    [InlineData(OfferTypeId.SERVICE)]
    public async Task DeactivateOfferStatusIdAsync_WithNotOfferStatusId_ThrowsConflictException(OfferTypeId offerTypeId)
    {
        // Arrange
        var offerId = _fixture.Create<Guid>();
        A.CallTo(() => _offerRepository.GetOfferActiveStatusDataByIdAsync(offerId, offerTypeId, _companyId))
            .Returns((false, true));

        // Act
        async Task Act() => await _sut.DeactivateOfferIdAsync(offerId, offerTypeId);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be(OfferServiceErrors.OFFERSTATUS_INCORRECT_STATE.ToString());

    }

    [Theory]
    [InlineData(OfferTypeId.APP)]
    [InlineData(OfferTypeId.SERVICE)]
    public async Task DeactivateOfferStatusIdAsync_WithValidData_CallsExpected(OfferTypeId offerTypeId)
    {
        // Arrange
        var offer = _fixture.Create<Offer>();
        var offerId = _fixture.Create<Guid>();
        A.CallTo(() => _offerRepository.GetOfferActiveStatusDataByIdAsync(offerId, offerTypeId, _companyId))
            .Returns((true, true));

        A.CallTo(() => _offerRepository.AttachAndModifyOffer(offerId, A<Action<Offer>>._, A<Action<Offer>?>._))
            .Invokes((Guid _, Action<Offer> setOptionalParameters, Action<Offer>? initializeParemeters) =>
            {
                initializeParemeters?.Invoke(offer);
                setOptionalParameters.Invoke(offer);
            });

        // Act
        await _sut.DeactivateOfferIdAsync(offerId, offerTypeId);

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
        var result = await _sut.GetProviderOfferAgreementConsentById(serviceId, OfferTypeId.SERVICE);

        // Assert
        A.CallTo(() => _agreementRepository.GetOfferAgreementConsentById(serviceId, _companyId, OfferTypeId.SERVICE))
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
        async Task Act() => await _sut.GetProviderOfferAgreementConsentById(serviceId, OfferTypeId.SERVICE);

        // Arrange
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be(OfferServiceErrors.COMPANY_NOT_ASSIGNED_WITH_OFFER.ToString());
    }

    [Fact]
    public async Task GetProviderOfferAgreementConsentById_WithInvalidOfferId_ThrowsNotFoundException()
    {
        //Arrange
        var serviceId = Guid.NewGuid();

        A.CallTo(() => _agreementRepository.GetOfferAgreementConsentById(A<Guid>._, A<Guid>._, OfferTypeId.SERVICE))
            .Returns<(OfferAgreementConsent, bool)>(default);

        // Act
        async Task Act() => await _sut.GetProviderOfferAgreementConsentById(serviceId, OfferTypeId.SERVICE);

        // Arrange
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be(OfferServiceErrors.OFFER_OR_OFFERTYPE_NOT_EXIST.ToString());
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
        var consentData = new OfferAgreementConsent(new[] { new AgreementConsentStatus(agreementId, ConsentStatusId.ACTIVE) });
        A.CallTo(() => _agreementRepository.GetOfferAgreementConsent(offerId, _companyId, OfferStatusId.CREATED, offerTypeId))
            .Returns<(OfferAgreementConsentUpdate, bool)>(default);

        // Act
        async Task Act() => await _sut.CreateOrUpdateProviderOfferAgreementConsent(offerId, consentData, offerTypeId);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be(OfferServiceErrors.OFFER_STATUS_NOT_EXIST.ToString());
    }

    [Theory]
    [InlineData(OfferTypeId.SERVICE)]
    [InlineData(OfferTypeId.APP)]
    public async Task CreateOrUpdateProviderOfferAgreementConsent_WithUserNotInProviderCompany_ThrowsNotFoundException(OfferTypeId offerTypeId)
    {
        //Arrange
        var offerId = Guid.NewGuid();
        var agreementId = Guid.NewGuid();
        var consentData = new OfferAgreementConsent(new[] { new AgreementConsentStatus(agreementId, ConsentStatusId.ACTIVE) });
        A.CallTo(() => _agreementRepository.GetOfferAgreementConsent(offerId, _companyId, OfferStatusId.CREATED, offerTypeId))
            .Returns((new OfferAgreementConsentUpdate(Enumerable.Empty<AppAgreementConsentStatus>(), Enumerable.Empty<AgreementStatusData>()), false));

        // Act
        async Task Act() => await _sut.CreateOrUpdateProviderOfferAgreementConsent(offerId, consentData, offerTypeId);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be(OfferServiceErrors.COMPANY_NOT_ASSIGNED_WITH_OFFER.ToString());
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
        var consentData = new OfferAgreementConsent(new[] { new AgreementConsentStatus(agreementId, ConsentStatusId.ACTIVE), new AgreementConsentStatus(additionalAgreementId, ConsentStatusId.ACTIVE) });
        var offerAgreementConsent = new OfferAgreementConsentUpdate(
            new[]
            {
                new AppAgreementConsentStatus(agreementId, consentId, ConsentStatusId.INACTIVE)
            },
            new[]
            {

                new AgreementStatusData(agreementId, AgreementStatusId.ACTIVE)
            });
        A.CallTo(() => _agreementRepository.GetOfferAgreementConsent(offerId, _companyId, OfferStatusId.CREATED, offerTypeId))
            .Returns((offerAgreementConsent, true));

        // Act
        async Task Act() => await _sut.CreateOrUpdateProviderOfferAgreementConsent(offerId, consentData, offerTypeId);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be(OfferServiceErrors.AGREEMENTS_NOT_VALID_FOR_OFFER.ToString());
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
        var additionalAgreementId1 = Guid.NewGuid();
        var consentId = Guid.NewGuid();
        var consentId1 = Guid.NewGuid();
        var newCreatedConsentId = Guid.NewGuid();
        var utcNow = DateTimeOffset.UtcNow;
        var consentData = new OfferAgreementConsent(new[] { new AgreementConsentStatus(agreementId, ConsentStatusId.ACTIVE), new AgreementConsentStatus(additionalAgreementId, ConsentStatusId.ACTIVE) });
        var offerAgreementConsent = new OfferAgreementConsentUpdate(
            new[]
            {
                new AppAgreementConsentStatus(agreementId, consentId, ConsentStatusId.INACTIVE),
                new AppAgreementConsentStatus(additionalAgreementId1, consentId1, ConsentStatusId.ACTIVE)
            },
            new[]
            {
                new AgreementStatusData(agreementId, AgreementStatusId.ACTIVE),
                new AgreementStatusData(additionalAgreementId, AgreementStatusId.ACTIVE),
                new AgreementStatusData(additionalAgreementId1, AgreementStatusId.INACTIVE),
            });
        A.CallTo(() => _agreementRepository.GetOfferAgreementConsent(offerId, _companyId, OfferStatusId.CREATED, offerTypeId))
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
        var result = await _sut.CreateOrUpdateProviderOfferAgreementConsent(offerId, consentData, offerTypeId);

        // Assert
        A.CallTo(() => _consentRepository.AddAttachAndModifyOfferConsents(A<IEnumerable<AppAgreementConsentStatus>>._, A<IEnumerable<AgreementConsentStatus>>._, offerId, _companyId, _companyUserId, A<DateTimeOffset>._))
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
        var result = await _sut.GetOfferDocumentContentAsync(offerId, documentId, documentTypeIdSettings, offerTypeId, CancellationToken.None);

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
            .Returns<OfferDocumentContentData?>(null);

        // Act
        async Task Act() => await _sut.GetOfferDocumentContentAsync(offerId, documentId, documentTypeIdSettings, offerTypeId, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be(OfferServiceErrors.DOCUMENT_NOT_EXIST.ToString());
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
        async Task Act() => await _sut.GetOfferDocumentContentAsync(offerId, documentId, documentTypeIdSettings, offerTypeId, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be(OfferServiceErrors.DOCUMENTSTATUS_RETRIEVED_NOT_ALLOWED.ToString());
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
        async Task Act() => await _sut.GetOfferDocumentContentAsync(offerId, documentId, documentTypeIdSettings, offerTypeId, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be(OfferServiceErrors.DOCUMENT_TYPE_NOT_SUPPORTED.ToString());
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
        async Task Act() => await _sut.GetOfferDocumentContentAsync(offerId, documentId, documentTypeIdSettings, offerTypeId, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be(OfferServiceErrors.DOCUMENT_ID_MISMATCH.ToString());
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
        async Task Act() => await _sut.GetOfferDocumentContentAsync(offerId, documentId, documentTypeIdSettings, offerTypeId, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be(OfferServiceErrors.DOCUMENT_INACTIVE.ToString());
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
        async Task Act() => await _sut.GetOfferDocumentContentAsync(offerId, documentId, documentTypeIdSettings, offerTypeId, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);
        ex.Message.Should().Be(OfferServiceErrors.DOCUMENT_CONTENT_NULL.ToString());
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

        A.CallTo(() => _documentRepository.GetOfferDocumentsAsync(_validDocumentId, _companyId, documentTypeIdSettings, offerTypeId))
            .Returns((new[] { (OfferStatusId.CREATED, offerId, true) }, true, DocumentStatusId.PENDING, true));

        //Act
        await _sut.DeleteDocumentsAsync(_validDocumentId, documentTypeIdSettings, offerTypeId);

        // Assert 
        A.CallTo(() => _documentRepository.GetOfferDocumentsAsync(_validDocumentId, _companyId, documentTypeIdSettings, offerTypeId))
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
        A.CallTo(() => _documentRepository.GetOfferDocumentsAsync(_validDocumentId, _companyId, documentTypeIdSettings, offerTypeId))
            .Returns<(IEnumerable<(OfferStatusId, Guid, bool)>, bool, DocumentStatusId, bool)>(default);

        //Act
        async Task Act() => await _sut.DeleteDocumentsAsync(_validDocumentId, documentTypeIdSettings, offerTypeId);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be(OfferServiceErrors.DOCUMENT_NOT_EXIST.ToString());
    }

    [Theory]
    [InlineData(OfferTypeId.APP, new[] { DocumentTypeId.APP_CONTRACT })]
    [InlineData(OfferTypeId.SERVICE, new[] { DocumentTypeId.ADDITIONAL_DETAILS })]
    public async Task DeleteDocumentsAsync_WithNoAssignedOfferDocument_ThrowsConflictException(OfferTypeId offerTypeId, IEnumerable<DocumentTypeId> documentTypeIdSettings)
    {
        //Arrange
        A.CallTo(() => _documentRepository.GetOfferDocumentsAsync(_validDocumentId, _companyId, documentTypeIdSettings, offerTypeId))
            .Returns((new[] { default((OfferStatusId, Guid, bool)) }, true, DocumentStatusId.PENDING, true));

        //Act
        async Task Act() => await _sut.DeleteDocumentsAsync(_validDocumentId, documentTypeIdSettings, offerTypeId);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be(OfferServiceErrors.DOCUMENT_NOT_ASSIGNED_TO_OFFER.ToString());
    }

    [Theory]
    [InlineData(OfferTypeId.APP, new[] { DocumentTypeId.APP_CONTRACT })]
    [InlineData(OfferTypeId.SERVICE, new[] { DocumentTypeId.ADDITIONAL_DETAILS })]
    public async Task DeleteDocumentsAsync_WithMultipleDocumentsAssigned_ThrowsConflictException(OfferTypeId offerTypeId, IEnumerable<DocumentTypeId> documentTypeIdSettings)
    {
        //Arrange
        A.CallTo(() => _documentRepository.GetOfferDocumentsAsync(_validDocumentId, _companyId, documentTypeIdSettings, offerTypeId))
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
        async Task Act() => await _sut.DeleteDocumentsAsync(_validDocumentId, documentTypeIdSettings, offerTypeId);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be(OfferServiceErrors.DOCUMENT_ASSIGNED_TO_MULTIPLE_OFFERS.ToString());
    }

    [Theory]
    [InlineData(OfferTypeId.APP, new[] { DocumentTypeId.APP_CONTRACT })]
    [InlineData(OfferTypeId.SERVICE, new[] { DocumentTypeId.ADDITIONAL_DETAILS })]
    public async Task DeleteDocumentsAsync_WithDocumentAssignedToService_ThrowsConflictException(OfferTypeId offerTypeId, IEnumerable<DocumentTypeId> documentTypeIdSettings)
    {
        //Arrange
        var offerId = Guid.NewGuid();

        A.CallTo(() => _documentRepository.GetOfferDocumentsAsync(_validDocumentId, _companyId, documentTypeIdSettings, offerTypeId))
            .Returns((new[] { (OfferStatusId.CREATED, offerId, false) }, true, DocumentStatusId.PENDING, true));

        //Act
        async Task Act() => await _sut.DeleteDocumentsAsync(_validDocumentId, documentTypeIdSettings, offerTypeId);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be(OfferServiceErrors.DOCUMENT_NOT_ASSIGNED_TO_OFFER.ToString());
    }

    [Theory]
    [InlineData(OfferTypeId.APP, new[] { DocumentTypeId.APP_CONTRACT })]
    [InlineData(OfferTypeId.SERVICE, new[] { DocumentTypeId.ADDITIONAL_DETAILS })]
    public async Task DeleteDocumentsAsync_WithInvalidProviderCompanyUser_ThrowsForbiddenException(OfferTypeId offerTypeId, IEnumerable<DocumentTypeId> documentTypeIdSettings)
    {
        //Arrange
        var offerId = Guid.NewGuid();

        A.CallTo(() => _documentRepository.GetOfferDocumentsAsync(_validDocumentId, _companyId, documentTypeIdSettings, offerTypeId))
            .Returns((new[] { (OfferStatusId.CREATED, offerId, true) }, true, DocumentStatusId.PENDING, false));

        //Act
        async Task Act() => await _sut.DeleteDocumentsAsync(_validDocumentId, documentTypeIdSettings, offerTypeId);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be(OfferServiceErrors.COMPANY_NOT_SAME_AS_DOCUMENT_COMPANY.ToString());
    }

    [Theory]
    [InlineData(OfferTypeId.APP, new[] { DocumentTypeId.APP_CONTRACT })]
    [InlineData(OfferTypeId.SERVICE, new[] { DocumentTypeId.ADDITIONAL_DETAILS })]
    public async Task DeleteDocumentsAsync_WithInvalidOfferStatus_ThrowsConflictException(OfferTypeId offerTypeId, IEnumerable<DocumentTypeId> documentTypeIdSettings)
    {
        //Arrange
        var offerId = Guid.NewGuid();

        A.CallTo(() => _documentRepository.GetOfferDocumentsAsync(_validDocumentId, _companyId, documentTypeIdSettings, offerTypeId))
            .Returns((new[] { (OfferStatusId.ACTIVE, offerId, true) }, true, DocumentStatusId.PENDING, true));

        //Act
        async Task Act() => await _sut.DeleteDocumentsAsync(_validDocumentId, documentTypeIdSettings, offerTypeId);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be(OfferServiceErrors.OFFER_LOCKED_STATE.ToString());
    }

    [Theory]
    [InlineData(OfferTypeId.APP, new[] { DocumentTypeId.APP_CONTRACT })]
    [InlineData(OfferTypeId.SERVICE, new[] { DocumentTypeId.ADDITIONAL_DETAILS })]
    public async Task DeleteDocumentsAsync_WithInvalidDocumentType_ThrowsArgumentException(OfferTypeId offerTypeId, IEnumerable<DocumentTypeId> documentTypeIdSettings)
    {
        //Arrange
        var offerId = Guid.NewGuid();

        A.CallTo(() => _documentRepository.GetOfferDocumentsAsync(_validDocumentId, _companyId, documentTypeIdSettings, offerTypeId))
            .Returns((new[] { (OfferStatusId.CREATED, offerId, true) }, false, DocumentStatusId.PENDING, true));

        //Act
        async Task Act() => await _sut.DeleteDocumentsAsync(_validDocumentId, documentTypeIdSettings, offerTypeId);

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be(OfferServiceErrors.DOCUMENTSTATUS_RETRIEVED_NOT_ALLOWED.ToString());
    }

    [Theory]
    [InlineData(OfferTypeId.APP, new[] { DocumentTypeId.APP_CONTRACT })]
    [InlineData(OfferTypeId.SERVICE, new[] { DocumentTypeId.ADDITIONAL_DETAILS })]
    public async Task DeleteDocumentsAsync_WithInvalidDocumentStatus_ThrowsConflictException(OfferTypeId offerTypeId, IEnumerable<DocumentTypeId> documentTypeIdSettings)
    {
        //Arrange
        var offerId = Guid.NewGuid();

        A.CallTo(() => _documentRepository.GetOfferDocumentsAsync(_validDocumentId, _companyId, documentTypeIdSettings, offerTypeId))
            .Returns((new[] { (OfferStatusId.CREATED, offerId, true) }, true, DocumentStatusId.LOCKED, true));
        //Act
        async Task Act() => await _sut.DeleteDocumentsAsync(_validDocumentId, documentTypeIdSettings, offerTypeId);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be(OfferServiceErrors.DOCUMENTSTATUS_DELETION_NOT_ALLOWED.ToString());
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
        var data = _fixture.CreateMany<TechnicalUserProfileInformationTransferData>(5);
        A.CallTo(() => _technicalUserProfileRepository.GetTechnicalUserProfileInformation(A<Guid>._, A<Guid>._, A<OfferTypeId>._, A<IEnumerable<UserRoleConfig>>._, A<IEnumerable<UserRoleConfig>>._))
            .Returns((true, data));

        // Act
        var result = await _sut.GetTechnicalUserProfilesForOffer(offerId, offerTypeId, Enumerable.Empty<UserRoleConfig>(), Enumerable.Empty<UserRoleConfig>());

        // Assert
        result.Should().HaveCount(5);
        A.CallTo(() => _technicalUserProfileRepository.GetTechnicalUserProfileInformation(offerId, _companyId, offerTypeId, A<IEnumerable<UserRoleConfig>>._, A<IEnumerable<UserRoleConfig>>._)).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData(OfferTypeId.APP)]
    [InlineData(OfferTypeId.SERVICE)]
    public async Task GetTechnicalUserProfileData_WithoutOffer_ThrowsNotFoundException(OfferTypeId offerTypeId)
    {
        // Arrange
        var offerId = _fixture.Create<Guid>();
        A.CallTo(() => _technicalUserProfileRepository.GetTechnicalUserProfileInformation(A<Guid>._, A<Guid>._, A<OfferTypeId>._, A<IEnumerable<UserRoleConfig>>._, A<IEnumerable<UserRoleConfig>>._))
            .Returns<(bool, IEnumerable<TechnicalUserProfileInformationTransferData>)>(default);

        // Act
        async Task Act() => await _sut.GetTechnicalUserProfilesForOffer(offerId, offerTypeId, Enumerable.Empty<UserRoleConfig>(), Enumerable.Empty<UserRoleConfig>());

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be(OfferServiceErrors.OFFER_NOTFOUND.ToString());
        A.CallTo(() => _technicalUserProfileRepository.GetTechnicalUserProfileInformation(offerId, _companyId, offerTypeId, A<IEnumerable<UserRoleConfig>>._, A<IEnumerable<UserRoleConfig>>._)).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData(OfferTypeId.APP)]
    [InlineData(OfferTypeId.SERVICE)]
    public async Task GetTechnicalUserProfileData_WithUserNotInProvidingCompany_ThrowsForbiddenException(OfferTypeId offerTypeId)
    {
        // Arrange
        var offerId = _fixture.Create<Guid>();
        A.CallTo(() => _technicalUserProfileRepository.GetTechnicalUserProfileInformation(A<Guid>._, A<Guid>._, A<OfferTypeId>._, A<IEnumerable<UserRoleConfig>>._, A<IEnumerable<UserRoleConfig>>._))
            .Returns((false, Enumerable.Empty<TechnicalUserProfileInformationTransferData>()));

        // Act
        async Task Act() => await _sut.GetTechnicalUserProfilesForOffer(offerId, offerTypeId, Enumerable.Empty<UserRoleConfig>(), Enumerable.Empty<UserRoleConfig>());

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be(OfferServiceErrors.COMPANY_NOT_PROVIDER.ToString());
        A.CallTo(() => _technicalUserProfileRepository.GetTechnicalUserProfileInformation(offerId, _companyId, offerTypeId, A<IEnumerable<UserRoleConfig>>._, A<IEnumerable<UserRoleConfig>>._)).MustHaveHappenedOnceExactly();
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
        A.CallTo(() => _technicalUserProfileRepository.GetOfferProfileData(offerId, offerTypeId, _companyId))
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
        await _sut.UpdateTechnicalUserProfiles(offerId, offerTypeId, data, "cl1", Enumerable.Empty<UserRoleConfig>());

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
        A.CallTo(() => _technicalUserProfileRepository.GetOfferProfileData(offerId, offerTypeId, _companyId))
            .Returns(new OfferProfileData(true, new[] { ServiceTypeId.DATASPACE_SERVICE }, Enumerable.Empty<(Guid TechnicalUserProfileId, IEnumerable<Guid> UserRoleIds)>()));
        A.CallTo(() => _userRolesRepository.GetRolesForClient("cl1"))
            .Returns(new Guid[] { userRole1Id, userRole2Id }.ToAsyncEnumerable());

        // Act
        async Task Act() => await _sut.UpdateTechnicalUserProfiles(offerId, offerTypeId, data, "cl1", Enumerable.Empty<UserRoleConfig>());

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be(OfferServiceErrors.ROLES_DOES_NOT_EXIST.ToString());
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
        A.CallTo(() => _technicalUserProfileRepository.GetOfferProfileData(offerId, offerTypeId, _companyId))
            .Returns(new OfferProfileData(true, new[] { ServiceTypeId.CONSULTANCY_SERVICE }, Enumerable.Empty<(Guid TechnicalUserProfileId, IEnumerable<Guid> UserRoleIds)>()));
        A.CallTo(() => _userRolesRepository.GetRolesForClient("cl1"))
            .Returns(new Guid[] { userRole1Id, userRole2Id }.ToAsyncEnumerable());

        // Act
        async Task Act() => await _sut.UpdateTechnicalUserProfiles(offerId, offerTypeId, data, "cl1", Enumerable.Empty<UserRoleConfig>());

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be(OfferServiceErrors.TECHNICAL_USERS_FOR_CONSULTANCY.ToString());
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
        A.CallTo(() => _technicalUserProfileRepository.GetOfferProfileData(offerId, offerTypeId, _companyId))
            .Returns(new OfferProfileData(false, new[] { ServiceTypeId.DATASPACE_SERVICE }, Enumerable.Empty<(Guid TechnicalUserProfileId, IEnumerable<Guid> UserRoleIds)>()));
        A.CallTo(() => _userRolesRepository.GetRolesForClient("cl1"))
            .Returns(new Guid[] { userRole1Id, userRole2Id }.ToAsyncEnumerable());

        // Act
        async Task Act() => await _sut.UpdateTechnicalUserProfiles(offerId, offerTypeId, data, "cl1", Enumerable.Empty<UserRoleConfig>());

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be(OfferServiceErrors.COMPANY_NOT_PROVIDER.ToString());
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
        A.CallTo(() => _technicalUserProfileRepository.GetOfferProfileData(offerId, offerTypeId, _companyId))
            .Returns<OfferProfileData?>(null);
        A.CallTo(() => _userRolesRepository.GetRolesForClient("cl1"))
            .Returns(new Guid[] { userRole1Id, userRole2Id }.ToAsyncEnumerable());

        // Act
        async Task Act() => await _sut.UpdateTechnicalUserProfiles(offerId, offerTypeId, data, "cl1", Enumerable.Empty<UserRoleConfig>());

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be(OfferServiceErrors.OFFER_NOTFOUND.ToString());
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
        async Task Act() => await _sut.UpdateTechnicalUserProfiles(offerId, offerTypeId, data, "cl1", Enumerable.Empty<UserRoleConfig>());

        // Assert
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be(OfferServiceErrors.NOT_EMPTY_ROLES_AND_PROFILES.ToString());
    }
    #endregion

    #region GetOfferSubscriptionDetailForProvider

    [Fact]
    public async Task GetOfferSubscriptionDetailForProvider_WithNotMatchingUserRoles_ThrowsConfigurationException()
    {
        // Arrange
        var offerId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        var companyAdminRoles = _fixture.CreateMany<UserRoleConfig>().ToImmutableArray();
        var walletData = _fixture.Create<WalletConfigData>();

        SetupGetSubscriptionDetailForProvider();

        // Act
        async Task Act() => await _sut.GetOfferSubscriptionDetailsForProviderAsync(offerId, subscriptionId, OfferTypeId.APP, companyAdminRoles, walletData);

        // Assert
        var ex = await Assert.ThrowsAsync<ConfigurationException>(Act);
        ex.Message.Should().Contain(OfferServiceErrors.INVALID_CONFIGURATION_ROLES_NOT_EXIST.ToString());
        A.CallTo(() => _userRolesRepository.GetUserRoleIdsUntrackedAsync(A<IEnumerable<UserRoleConfig>>.That.IsSameSequenceAs(companyAdminRoles)))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerSubscriptionsRepository.GetOfferSubscriptionDetailsForProviderAsync(A<Guid>._, A<Guid>._, A<Guid>._, A<OfferTypeId>._, A<IEnumerable<Guid>>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task GetOfferSubscriptionDetailForProvider_WithNotExistingOffer_ThrowsNotFoundException()
    {
        // Arrange
        var appId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        var companyAdminRoles = new[]
        {
            new UserRoleConfig("ClientTest", new[] {"Test"})
        };
        var walletData = _fixture.Create<WalletConfigData>();
        SetupGetSubscriptionDetailForProvider();

        A.CallTo(() => _offerSubscriptionsRepository.GetOfferSubscriptionDetailsForProviderAsync(A<Guid>._, A<Guid>._, A<Guid>._, A<OfferTypeId>._, A<IEnumerable<Guid>>._))
            .Returns<(bool, bool, OfferProviderSubscriptionDetail?)>(default);

        // Act
        async Task Act() => await _sut.GetOfferSubscriptionDetailsForProviderAsync(appId, subscriptionId, OfferTypeId.APP, companyAdminRoles, walletData);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Contain(OfferServiceErrors.SUBSCRIPTION_NOT_FOUND_FOR_OFFER.ToString());
        A.CallTo(() => _userRolesRepository.GetUserRoleIdsUntrackedAsync(A<IEnumerable<UserRoleConfig>>.That.IsSameSequenceAs(companyAdminRoles)))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerSubscriptionsRepository.GetOfferSubscriptionDetailsForProviderAsync(appId, subscriptionId, _companyId, OfferTypeId.APP, A<IEnumerable<Guid>>.That.IsSameSequenceAs(new[] { _validUserRoleId })))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetOfferSubscriptionDetailForProvider_WithUserNotInProvidingCompany_ThrowsForbiddenException()
    {
        // Arrange
        var appId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        var companyAdminRoles = new[]
        {
            new UserRoleConfig("ClientTest", new[] {"Test"})
        };
        var walletData = _fixture.Create<WalletConfigData>();
        SetupGetSubscriptionDetailForProvider();

        A.CallTo(() => _offerSubscriptionsRepository.GetOfferSubscriptionDetailsForProviderAsync(A<Guid>._, A<Guid>._, A<Guid>._, A<OfferTypeId>._, A<IEnumerable<Guid>>._))
            .Returns((true, false, _fixture.Create<OfferProviderSubscriptionDetail>()));

        // Act
        async Task Act() => await _sut.GetOfferSubscriptionDetailsForProviderAsync(appId, subscriptionId, OfferTypeId.APP, companyAdminRoles, walletData);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Contain(OfferServiceErrors.COMPANY_NOT_PART_OF_ROLE.ToString());
        A.CallTo(() => _userRolesRepository.GetUserRoleIdsUntrackedAsync(A<IEnumerable<UserRoleConfig>>.That.IsSameSequenceAs(companyAdminRoles)))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerSubscriptionsRepository.GetOfferSubscriptionDetailsForProviderAsync(appId, subscriptionId, _companyId, OfferTypeId.APP, A<IEnumerable<Guid>>.That.IsSameSequenceAs(new[] { _validUserRoleId })))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetOfferSubscriptionDetailForProvider_WithValidData_ReturnsExpected()
    {
        // Arrange
        var appId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        var companyAdminRoles = new[]
        {
            new UserRoleConfig("ClientTest", new[] {"Test"})
        };
        var walletData = _fixture.Create<WalletConfigData>();
        SetupGetSubscriptionDetailForProvider();

        var data = _fixture.Create<OfferProviderSubscriptionDetail>();

        A.CallTo(() => _offerSubscriptionsRepository.GetOfferSubscriptionDetailsForProviderAsync(A<Guid>._, A<Guid>._, A<Guid>._, A<OfferTypeId>._, A<IEnumerable<Guid>>._))
            .Returns((true, true, data));

        // Act
        var result = await _sut.GetOfferSubscriptionDetailsForProviderAsync(appId, subscriptionId, OfferTypeId.APP, companyAdminRoles, walletData);

        // Assert
        result.Id.Should().Be(data.Id);
        result.Bpn.Should().Be(data.Bpn);
        result.Customer.Should().Be(data.Customer);
        result.Name.Should().Be(data.Name);
        result.TenantUrl.Should().Be(data.TenantUrl);
        result.AppInstanceId.Should().Be(data.AppInstanceId);
        result.OfferSubscriptionStatus.Should().Be(data.OfferSubscriptionStatus);
        A.CallTo(() => _userRolesRepository.GetUserRoleIdsUntrackedAsync(A<IEnumerable<UserRoleConfig>>.That.IsSameSequenceAs(companyAdminRoles)))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerSubscriptionsRepository.GetOfferSubscriptionDetailsForProviderAsync(appId, subscriptionId, _companyId, OfferTypeId.APP, A<IEnumerable<Guid>>.That.IsSameSequenceAs(new[] { _validUserRoleId })))
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
        var companyAdminRoles = _fixture.CreateMany<UserRoleConfig>().ToImmutableArray();

        SetupGetSubscriptionDetailForProvider();

        // Act
        async Task Act() => await _sut.GetSubscriptionDetailsForSubscriberAsync(offerId, subscriptionId, OfferTypeId.APP, companyAdminRoles);

        // Assert
        var ex = await Assert.ThrowsAsync<ConfigurationException>(Act);
        ex.Message.Should().Contain(OfferServiceErrors.INVALID_CONFIGURATION_ROLES_NOT_EXIST.ToString());
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
        var companyAdminRoles = new[]
        {
            new UserRoleConfig("ClientTest", new[] {"Test"})
        };
        SetupGetSubscriptionDetailForProvider();

        A.CallTo(() => _offerSubscriptionsRepository.GetSubscriptionDetailsForSubscriberAsync(A<Guid>._, A<Guid>._, A<Guid>._, A<OfferTypeId>._, A<IEnumerable<Guid>>._))
            .Returns<(bool, bool, SubscriberSubscriptionDetailData?)>(default);

        // Act
        async Task Act() => await _sut.GetSubscriptionDetailsForSubscriberAsync(appId, subscriptionId, OfferTypeId.APP, companyAdminRoles);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Contain(OfferServiceErrors.SUBSCRIPTION_NOT_FOUND_FOR_OFFER.ToString());
        A.CallTo(() => _userRolesRepository.GetUserRoleIdsUntrackedAsync(A<IEnumerable<UserRoleConfig>>.That.IsSameSequenceAs(companyAdminRoles)))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerSubscriptionsRepository.GetSubscriptionDetailsForSubscriberAsync(appId, subscriptionId, _companyId, OfferTypeId.APP, A<IEnumerable<Guid>>.That.IsSameSequenceAs(new[] { _validUserRoleId })))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetSubscriptionDetailsForSubscriber_WithUserNotInProvidingCompany_ThrowsForbiddenException()
    {
        // Arrange
        var appId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        var companyAdminRoles = new[]
        {
            new UserRoleConfig("ClientTest", new[] {"Test"})
        };
        SetupGetSubscriptionDetailForProvider();

        A.CallTo(() => _offerSubscriptionsRepository.GetSubscriptionDetailsForSubscriberAsync(A<Guid>._, A<Guid>._, A<Guid>._, A<OfferTypeId>._, A<IEnumerable<Guid>>._))
            .Returns((true, false, _fixture.Create<SubscriberSubscriptionDetailData>()));

        // Act
        async Task Act() => await _sut.GetSubscriptionDetailsForSubscriberAsync(appId, subscriptionId, OfferTypeId.APP, companyAdminRoles);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Contain(OfferServiceErrors.COMPANY_NOT_PART_OF_ROLE.ToString());
        A.CallTo(() => _userRolesRepository.GetUserRoleIdsUntrackedAsync(A<IEnumerable<UserRoleConfig>>.That.IsSameSequenceAs(companyAdminRoles)))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerSubscriptionsRepository.GetSubscriptionDetailsForSubscriberAsync(appId, subscriptionId, _companyId, OfferTypeId.APP, A<IEnumerable<Guid>>.That.IsSameSequenceAs(new[] { _validUserRoleId })))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetSubscriptionDetailsForSubscriber_WithValidData_ReturnsExpected()
    {
        // Arrange
        var appId = Guid.NewGuid();
        var subscriptionId = Guid.NewGuid();
        var companyAdminRoles = new[]
        {
            new UserRoleConfig("ClientTest", new[] {"Test"})
        };
        SetupGetSubscriptionDetailForProvider();

        var data = _fixture.Create<SubscriberSubscriptionDetailData>();

        A.CallTo(() => _offerSubscriptionsRepository.GetSubscriptionDetailsForSubscriberAsync(A<Guid>._, A<Guid>._, A<Guid>._, A<OfferTypeId>._, A<IEnumerable<Guid>>._))
            .Returns((true, true, data));

        // Act
        var result = await _sut.GetSubscriptionDetailsForSubscriberAsync(appId, subscriptionId, OfferTypeId.APP, companyAdminRoles);

        // Assert
        result.Should().Be(data);
        A.CallTo(() => _userRolesRepository.GetUserRoleIdsUntrackedAsync(A<IEnumerable<UserRoleConfig>>.That.IsSameSequenceAs(companyAdminRoles)))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerSubscriptionsRepository.GetSubscriptionDetailsForSubscriberAsync(appId, subscriptionId, _companyId, OfferTypeId.APP, A<IEnumerable<Guid>>.That.IsSameSequenceAs(new[] { _validUserRoleId })))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region  GetCompanySubscribedOfferSubscriptionStatusesForUser

    [Theory]
    [InlineData(OfferTypeId.APP, DocumentTypeId.APP_LEADIMAGE)]
    [InlineData(OfferTypeId.SERVICE, DocumentTypeId.SERVICE_LEADIMAGE)]
    public async Task GetCompanySubscribedOfferSubscriptionStatusesForUserAsync_ReturnsExpected(OfferTypeId offerTypeId, DocumentTypeId documentTypeId)
    {
        // Arrange
        var data = _fixture.CreateMany<OfferSubscriptionStatusData>(5).ToImmutableArray();
        A.CallTo(() => _offerSubscriptionsRepository.GetOwnCompanySubscribedOfferSubscriptionStatusAsync(A<Guid>._, A<OfferTypeId>._, A<DocumentTypeId>._, A<OfferSubscriptionStatusId?>._, A<string?>._))
            .Returns((skip, take) => Task.FromResult(new Pagination.Source<OfferSubscriptionStatusData>(data.Length, data.Skip(skip).Take(take)))!);

        // Act
        var result = await _sut.GetCompanySubscribedOfferSubscriptionStatusesForUserAsync(0, 10, offerTypeId, documentTypeId, null, null);

        // Assert
        result.Meta.NumberOfElements.Should().Be(5);
        result.Content.Should().HaveCount(5).And.Satisfy(
            x => x.OfferId == data[0].OfferId && x.OfferName == data[0].OfferName && x.Provider == data[0].Provider && x.OfferSubscriptionStatusId == data[0].OfferSubscriptionStatusId && x.OfferSubscriptionId == data[0].OfferSubscriptionId && x.DocumentId == data[0].DocumentId,
            x => x.OfferId == data[1].OfferId && x.OfferName == data[1].OfferName && x.Provider == data[1].Provider && x.OfferSubscriptionStatusId == data[1].OfferSubscriptionStatusId && x.OfferSubscriptionId == data[1].OfferSubscriptionId && x.DocumentId == data[1].DocumentId,
            x => x.OfferId == data[2].OfferId && x.OfferName == data[2].OfferName && x.Provider == data[2].Provider && x.OfferSubscriptionStatusId == data[2].OfferSubscriptionStatusId && x.OfferSubscriptionId == data[2].OfferSubscriptionId && x.DocumentId == data[2].DocumentId,
            x => x.OfferId == data[3].OfferId && x.OfferName == data[3].OfferName && x.Provider == data[3].Provider && x.OfferSubscriptionStatusId == data[3].OfferSubscriptionStatusId && x.OfferSubscriptionId == data[3].OfferSubscriptionId && x.DocumentId == data[3].DocumentId,
            x => x.OfferId == data[4].OfferId && x.OfferName == data[4].OfferName && x.Provider == data[4].Provider && x.OfferSubscriptionStatusId == data[4].OfferSubscriptionStatusId && x.OfferSubscriptionId == data[4].OfferSubscriptionId && x.DocumentId == data[4].DocumentId
        );
        A.CallTo(() => _offerSubscriptionsRepository.GetOwnCompanySubscribedOfferSubscriptionStatusAsync(_companyId, offerTypeId, documentTypeId, null, null))
            .MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData(OfferTypeId.APP, DocumentTypeId.APP_LEADIMAGE)]
    [InlineData(OfferTypeId.SERVICE, DocumentTypeId.SERVICE_LEADIMAGE)]
    public async Task GetCompanySubscribedOfferSubscriptionStatusesForUserAsync_WithQueryNullResult_ReturnsExpected(OfferTypeId offerTypeId, DocumentTypeId documentTypeId)
    {
        // Arrange
        A.CallTo(() => _offerSubscriptionsRepository.GetOwnCompanySubscribedOfferSubscriptionStatusAsync(A<Guid>._, A<OfferTypeId>._, A<DocumentTypeId>._, A<OfferSubscriptionStatusId?>._, A<string?>._))
            .Returns((skip, take) => Task.FromResult<Pagination.Source<OfferSubscriptionStatusData>?>(null));

        // Act
        var result = await _sut.GetCompanySubscribedOfferSubscriptionStatusesForUserAsync(0, 10, offerTypeId, documentTypeId, null, null);

        // Assert
        result.Meta.NumberOfElements.Should().Be(0);
        result.Content.Should().BeEmpty();
        A.CallTo(() => _offerSubscriptionsRepository.GetOwnCompanySubscribedOfferSubscriptionStatusAsync(_companyId, offerTypeId, documentTypeId, null, null))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region UnsubscribeOwnCompanyAppSubscriptionAsync

    [Fact]
    public async Task UnsubscribeOwnCompanySubscriptionAsync_WithNotExistingApp_ThrowsNotFoundException()
    {
        // Arrange
        var notExistingSubscriptionId = _fixture.Create<Guid>();
        A.CallTo(() => _offerSubscriptionsRepository.GetCompanyAssignedOfferSubscriptionDataForCompanyUserAsync(A<Guid>._, A<Guid>._))
            .Returns<(OfferSubscriptionStatusId, bool, bool, IEnumerable<Guid>, IEnumerable<Guid>)>(default);

        // Act
        async Task Act() => await _sut.UnsubscribeOwnCompanySubscriptionAsync(notExistingSubscriptionId);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be(OfferServiceErrors.SUBSCRIPTION_NOT_EXIST.ToString());
        A.CallTo(() => _offerSubscriptionsRepository.GetCompanyAssignedOfferSubscriptionDataForCompanyUserAsync(notExistingSubscriptionId, _companyId))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task UnsubscribeOwnCompanySubscriptionAsync_IsNoMemberOfCompanyProvidingApp_ThrowsArgumentException()
    {
        // Arrange
        var subscriptionId = _fixture.Create<Guid>();
        A.CallTo(() => _offerSubscriptionsRepository.GetCompanyAssignedOfferSubscriptionDataForCompanyUserAsync(A<Guid>._, A<Guid>._))
            .Returns((OfferSubscriptionStatusId.ACTIVE, false, true, _fixture.CreateMany<Guid>(), _fixture.CreateMany<Guid>()));

        // Act
        async Task Act() => await _sut.UnsubscribeOwnCompanySubscriptionAsync(subscriptionId);

        // Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);
        ex.Message.Should().Be(OfferServiceErrors.USER_NOT_BELONG_TO_COMPANY.ToString());
        A.CallTo(() => _offerSubscriptionsRepository.GetCompanyAssignedOfferSubscriptionDataForCompanyUserAsync(subscriptionId, _companyId))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task UnsubscribeOwnCompanySubscriptionAsync_WithInactiveApp_ThrowsArgumentException()
    {
        // Arrange
        var offerSubscriptionId = _fixture.Create<Guid>();
        A.CallTo(() => _offerSubscriptionsRepository.GetCompanyAssignedOfferSubscriptionDataForCompanyUserAsync(A<Guid>._, A<Guid>._))
            .Returns((
                OfferSubscriptionStatusId.INACTIVE,
                true,
                true, Enumerable.Empty<Guid>(), Enumerable.Empty<Guid>()));

        // Act
        async Task Act() => await _sut.UnsubscribeOwnCompanySubscriptionAsync(offerSubscriptionId);

        // Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);
        ex.Message.Should().Be(OfferServiceErrors.NO_ACTIVE_OR_PENDING_SUBSCRIPTION.ToString());
        A.CallTo(() => _offerSubscriptionsRepository.GetCompanyAssignedOfferSubscriptionDataForCompanyUserAsync(offerSubscriptionId, _companyId))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task UnsubscribeOwnCompanySubscriptionAsync_CallsExpected()
    {
        // Arrange
        var offerSubscription = _fixture.Build<OfferSubscription>()
            .With(x => x.OfferSubscriptionStatusId, OfferSubscriptionStatusId.PENDING)
            .Create();
        var identities = _fixture.Build<Identity>()
            .With(x => x.UserStatusId, UserStatusId.ACTIVE).CreateMany(2);
        var connectors = _fixture.Build<Connector>()
            .With(x => x.StatusId, ConnectorStatusId.PENDING).CreateMany(2);
        A.CallTo(() => _offerSubscriptionsRepository.GetCompanyAssignedOfferSubscriptionDataForCompanyUserAsync(A<Guid>._, A<Guid>._))
            .Returns((OfferSubscriptionStatusId.ACTIVE, true, true, connectors.Select(con => con.Id), identities.Select(iden => iden.Id)));
        A.CallTo(() => _offerSubscriptionsRepository.AttachAndModifyOfferSubscription(A<Guid>._, A<Action<OfferSubscription>>._))
            .Invokes((Guid _, Action<OfferSubscription> setFields) =>
            {
                setFields.Invoke(offerSubscription);
            });

        foreach (var identity in identities)
        {
            A.CallTo(() => _userRepository.AttachAndModifyIdentity(identity.Id, A<Action<Identity>>._, A<Action<Identity>>._))
                .Invokes((Guid _, Action<Identity>? initialize, Action<Identity> setFields) =>
                {
                    initialize?.Invoke(identity);
                    setFields.Invoke(identity);
                });
        }
        foreach (var connector in connectors)
        {
            A.CallTo(() => _connectorsRepository.AttachAndModifyConnector(connector.Id, A<Action<Connector>>._, A<Action<Connector>>._))
                .Invokes((Guid _, Action<Connector>? initialize, Action<Connector> setOptionalFields) =>
                {
                    initialize?.Invoke(connector);
                    setOptionalFields.Invoke(connector);
                });
        }
        // Act
        await _sut.UnsubscribeOwnCompanySubscriptionAsync(offerSubscription.Id);

        // Assert
        offerSubscription.OfferSubscriptionStatusId.Should().Be(OfferSubscriptionStatusId.INACTIVE);
        A.CallTo(() => _portalRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerSubscriptionsRepository.GetCompanyAssignedOfferSubscriptionDataForCompanyUserAsync(offerSubscription.Id, _companyId))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerSubscriptionsRepository.AttachAndModifyOfferSubscription(offerSubscription.Id, A<Action<OfferSubscription>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _connectorsRepository.AttachAndModifyConnector(A<Guid>._, A<Action<Connector>>._, A<Action<Connector>>._))
            .MustHaveHappenedTwiceExactly();
        A.CallTo(() => _userRepository.AttachAndModifyIdentity(A<Guid>._, A<Action<Identity>>._, A<Action<Identity>>._))
            .MustHaveHappenedTwiceExactly();

        connectors.Should().AllSatisfy(x => x.StatusId.Should().Be(ConnectorStatusId.INACTIVE));
        identities.Should().AllSatisfy(x => x.UserStatusId.Should().Be(UserStatusId.INACTIVE));
        offerSubscription.OfferSubscriptionStatusId.Should().Be(OfferSubscriptionStatusId.INACTIVE);

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

        A.CallTo(() => _offerSubscriptionsRepository.GetCompanyIdWithAssignedOfferForCompanyUserAndSubscriptionAsync(
                A<Guid>.That.Matches(x => x == _existingServiceId), _companyUserId, A<OfferTypeId>.That.Matches(x => x == OfferTypeId.SERVICE)))
            .Returns((_companyId, true));
        A.CallTo(() => _offerSubscriptionsRepository.GetCompanyIdWithAssignedOfferForCompanyUserAndSubscriptionAsync(
                A<Guid>.That.Matches(x => x == _existingServiceId), _companyUserId, A<OfferTypeId>.That.Not.Matches(x => x == OfferTypeId.SERVICE)))
            .Returns((_companyId, false));
        A.CallTo(() => _offerSubscriptionsRepository.GetCompanyIdWithAssignedOfferForCompanyUserAndSubscriptionAsync(A<Guid>.That.Not.Matches(x => x == _existingServiceId), _companyUserId, A<OfferTypeId>._))
            .Returns((_companyId, false));
        A.CallTo(() => _offerSubscriptionsRepository.GetCompanyIdWithAssignedOfferForCompanyUserAndSubscriptionAsync(
                A<Guid>.That.Matches(x => x == _existingServiceId), A<Guid>.That.Not.Matches(x => x == _companyUserId),
                A<OfferTypeId>._))
            .Returns<(Guid, bool)>(default);

        var agreementData = _fixture.CreateMany<AgreementData>(1);
        A.CallTo(() => _agreementRepository.GetOfferAgreementDataForOfferId(A<Guid>.That.Matches(x => x == _existingServiceId), A<OfferTypeId>._))
            .Returns(agreementData.ToAsyncEnumerable());
        A.CallTo(() => _agreementRepository.GetOfferAgreementDataForOfferId(A<Guid>.That.Not.Matches(x => x == _existingServiceId), A<OfferTypeId>._))
            .Returns(Enumerable.Empty<AgreementData>().ToAsyncEnumerable());

        A.CallTo(() => _consentRepository.GetConsentDetailData(A<Guid>.That.Matches(x => x == _validConsentId), A<OfferTypeId>.That.Matches(x => x == OfferTypeId.SERVICE)))
            .Returns(new ConsentDetailData(_validConsentId, "The Company", _companyUser.Id, ConsentStatusId.ACTIVE, "Agreed"));
        A.CallTo(() => _consentRepository.GetConsentDetailData(A<Guid>.That.Not.Matches(x => x == _validConsentId), A<OfferTypeId>._))
            .Returns<ConsentDetailData?>(null);
        A.CallTo(() => _consentRepository.GetConsentDetailData(A<Guid>._, A<OfferTypeId>.That.Not.Matches(x => x == OfferTypeId.SERVICE)))
            .Returns<ConsentDetailData?>(null);

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
        A.CallTo(() => _portalRepositories.GetInstance<IConnectorsRepository>()).Returns(_connectorsRepository);
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
