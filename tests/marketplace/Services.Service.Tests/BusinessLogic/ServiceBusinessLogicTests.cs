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

using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Service;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Services.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Services.Service.ViewModels;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using Xunit;

namespace Org.Eclipse.TractusX.Portal.Backend.Services.Service.Tests.BusinessLogic;

public class ServiceBusinessLogicTests
{
    private readonly Guid _companyUserCompanyId = new("395f955b-f11b-4a74-ab51-92a526c1973a");
    private readonly string _notAssignedCompanyIdUser = "395f955b-f11b-4a74-ab51-92a526c1973c";
    private readonly Guid _existingServiceId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47661");
    private readonly Guid _existingServiceWithFailingAutoSetupId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47662");
    private readonly Guid _validSubscriptionId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47662");
    private readonly Guid _existingAgreementId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47664");
    private readonly Guid _validConsentId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47665");
    private readonly Guid _notExistingServiceId = Guid.NewGuid();
    private readonly Guid _activeServiceId = Guid.NewGuid();
    private readonly Guid _differentCompanyServiceId = Guid.NewGuid();
    private readonly CompanyUser _companyUser;
    private readonly IFixture _fixture;
    private readonly IamUser _iamUser;
    private readonly IAgreementRepository _agreementRepository;
    private readonly IConsentRepository _consentRepository;
    private readonly IOfferRepository _offerRepository;
    private readonly IOfferSubscriptionsRepository _offerSubscriptionsRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IUserRepository _userRepository;
    private readonly IUserRolesRepository _userRolesRepository;
    private readonly IOfferSubscriptionService _offerSubscriptionService;
    private readonly IOfferService _offerService;
    private readonly IOptions<ServiceSettings> _options;

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
        _consentRepository = A.Fake<IConsentRepository>();
        _offerRepository = A.Fake<IOfferRepository>();
        _offerSubscriptionsRepository = A.Fake<IOfferSubscriptionsRepository>();
        _notificationRepository = A.Fake<INotificationRepository>();
        _userRepository = A.Fake<IUserRepository>();
        _userRolesRepository = A.Fake<IUserRolesRepository>();

        _offerSubscriptionService = A.Fake<IOfferSubscriptionService>();
        _offerService = A.Fake<IOfferService>();

        SetupRepositories();

        var serviceSettings = new ServiceSettings
        {
            ApplicationsMaxPageSize = 15, 
            ServiceAccountRoles = new Dictionary<string, IEnumerable<string>>
            {
                {"Test", new[] {"Technical User"}}
            }, 
            CompanyAdminRoles = new Dictionary<string, IEnumerable<string>>
            {
                {"CatenaX", new[] {"Company Admin"}}
            },
            SubmitServiceNotificationTypeIds = new List<NotificationTypeId>
            {
                NotificationTypeId.SERVICE_RELEASE_REQUEST
            }
        };
        _options = Options.Create(serviceSettings);
        _fixture.Inject(_options);
    }

    #region Create Service

    [Fact]
    public async Task CreateServiceOffering_WithValidDataAndEmptyDescriptions_ReturnsCorrectDetails()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var offerService = A.Fake<IOfferService>();
        _fixture.Inject(offerService);
        A.CallTo(() => offerService.CreateServiceOfferingAsync(A<ServiceOfferingData>._, A<string>._, A<OfferTypeId>._)).ReturnsLazily(() => serviceId);
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        var result = await sut.CreateServiceOfferingAsync(new ServiceOfferingData("Newest Service", "42", "mail@test.de", _companyUser.Id, new List<LocalizedDescription>(), new List<ServiceTypeId>()), _iamUser.UserEntityId);

        // Assert
        result.Should().Be(serviceId);
    }

    #endregion

    #region Get Active Services

    [Fact]
    public async Task GetAllActiveServicesAsync_WithDefaultRequest_GetsExpectedEntries()
    {
        // Arrange
        SetupPagination();
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        var result = await sut.GetAllActiveServicesAsync(0, 5, null, null);

        // Assert
        result.Content.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetAllActiveServicesAsync_WithSmallSize_GetsExpectedEntries()
    {
        // Arrange
        const int expectedCount = 3;
        SetupPagination(expectedCount);
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        var result = await sut.GetAllActiveServicesAsync(0, expectedCount, null, null);

        // Assert
        result.Content.Should().HaveCount(expectedCount);
    }

    #endregion

    #region Add Service Subscription

    [Fact]
    public async Task AddServiceSubscription_ReturnsCorrectId()
    {
        // Arrange
        var offerSubscriptionId = Guid.NewGuid();
        var consentData = _fixture.CreateMany<OfferAgreementConsentData>(2);
        A.CallTo(() => _offerSubscriptionService.AddOfferSubscriptionAsync(A<Guid>._, A<IEnumerable<OfferAgreementConsentData>>._, A<string>._, A<string>._, A<IDictionary<string, IEnumerable<string>>>._, A<OfferTypeId>._, A<string>._))
            .ReturnsLazily(() => offerSubscriptionId);
        var serviceSettings = new ServiceSettings
        {
            ServiceManagerRoles = new Dictionary<string, IEnumerable<string>>
            {
                { "portal", new [] { "ServiceManager" }}
            },
            BasePortalAddress = "https://base-portal-address-test.de"
        };
        var sut = new ServiceBusinessLogic(A.Fake<IPortalRepositories>(), A.Fake<IOfferService>(), _offerSubscriptionService, Options.Create(serviceSettings));

        // Act
        var result = await sut.AddServiceSubscription(_existingServiceId, consentData, _iamUser.UserEntityId, "THISISAACCESSTOKEN");

        // Assert
        result.Should().Be(offerSubscriptionId);
        A.CallTo(() => _offerSubscriptionService.AddOfferSubscriptionAsync(
            A<Guid>._,
            A<IEnumerable<OfferAgreementConsentData>>._,
            A<string>._,
            A<string>._,
            A<IDictionary<string, IEnumerable<string>>>._,
            A<OfferTypeId>.That.Matches(x => x == OfferTypeId.SERVICE),
            A<string>._))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region GetCompanyProvidedServiceSubscriptionStatusesForUser
    
    [Fact]
    public async Task GetCompanyProvidedServiceSubscriptionStatusesForUserAsync_ReturnsExpectedCount()
    {
        // Arrange
        var (_, iamUser) = CreateTestUserPair();

        var data = _fixture.CreateMany<OfferCompanySubscriptionStatusData>(5);
        A.CallTo(() => _offerSubscriptionsRepository.GetOwnCompanyProvidedOfferSubscriptionStatusesUntrackedAsync(iamUser.UserEntityId, OfferTypeId.SERVICE, null, OfferSubscriptionStatusId.ACTIVE))
            .Returns((skip, take) => Task.FromResult(new Pagination.Source<OfferCompanySubscriptionStatusData>(data.Count(), data.Skip(skip).Take(take)))!);

        var serviceSettings = new ServiceSettings
        {
            ApplicationsMaxPageSize = 15
        };
        var sut = new ServiceBusinessLogic(_portalRepositories, A.Fake<IOfferService>(), A.Fake<IOfferSubscriptionService>(), Options.Create(serviceSettings));

        // Act
        var result = await sut.GetCompanyProvidedServiceSubscriptionStatusesForUserAsync(0, 10, iamUser.UserEntityId, null, null).ConfigureAwait(false);

        // Assert
        result.Content.Should().HaveCount(5);
    }

    #endregion

    #region Get Service Detail Data

    [Fact]
    public async Task GetServiceDetailsAsync_WithExistingServiceAndLanguageCode_ReturnsServiceDetailData()
    {
        // Arrange
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        var result = await sut.GetServiceDetailsAsync(_existingServiceId, "en", _iamUser.UserEntityId);

        // Assert
        result.Id.Should().Be(_existingServiceId);
        result.Documents.Keys.Should().Contain(DocumentTypeId.ADDITIONAL_DETAILS);
        Assert.IsType<ServiceDetailResponse>(result);
    }

    [Fact]
    public async Task GetServiceDetailsAsync_WithoutExistingService_ThrowsException()
    {
        // Arrange
        var notExistingServiceId = Guid.NewGuid();
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
        var sut = new ServiceBusinessLogic(A.Fake<IPortalRepositories>(), offerService, A.Fake<IOfferSubscriptionService>(), Options.Create(new ServiceSettings()));

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
        var sut = new ServiceBusinessLogic(A.Fake<IPortalRepositories>(),offerService, A.Fake<IOfferSubscriptionService>(), Options.Create(new ServiceSettings()));

        // Act
        var offerAgreementConsentData = new OfferAgreementConsentData(_existingAgreementId, ConsentStatusId.ACTIVE);
        var result = await sut.CreateServiceAgreementConsentAsync(_existingServiceId, offerAgreementConsentData, _iamUser.UserEntityId);

        // Assert
        result.Should().Be(consentId);
    }

    [Fact]
    public async Task CreateOrUpdateServiceAgreementConsentAsync_RunsSuccessfull()
    {
        // Arrange
        var offerService = A.Fake<IOfferService>();
        A.CallTo(() => offerService.CreateOrUpdateOfferSubscriptionAgreementConsentAsync(A<Guid>._, A<IEnumerable<OfferAgreementConsentData>>._, A<string>._, A<OfferTypeId>._))
            .ReturnsLazily(() => Task.CompletedTask);
        var sut = new ServiceBusinessLogic(A.Fake<IPortalRepositories>(), offerService, A.Fake<IOfferSubscriptionService>(), Options.Create(new ServiceSettings()));

        // Act
        await sut.CreateOrUpdateServiceAgreementConsentAsync(_existingServiceId, new List<OfferAgreementConsentData>
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
        var sut = new ServiceBusinessLogic(A.Fake<IPortalRepositories>(),offerService, A.Fake<IOfferSubscriptionService>(), Options.Create(new ServiceSettings()));

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
        var sut = new ServiceBusinessLogic(A.Fake<IPortalRepositories>(), offerService, A.Fake<IOfferSubscriptionService>(), Options.Create(new ServiceSettings()));

        // Act
        async Task Action() => await sut.GetServiceConsentDetailDataAsync(invalidConsentId).ConfigureAwait(false);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(Action);
    }

    #endregion

    #region Auto setup service

    [Fact]
    public async Task AutoSetupService_ReturnsExcepted()
    {
        // Arrange
        var offerService = A.Fake<IOfferService>();
        A.CallTo(() => offerService.AutoSetupServiceAsync(A<OfferAutoSetupData>._, A<IDictionary<string, IEnumerable<string>>>._, A<IDictionary<string, IEnumerable<string>>>._, A<string>._, A<OfferTypeId>._, A<string>._))
            .ReturnsLazily(() => new OfferAutoSetupResponseData(new TechnicalUserInfoData(Guid.NewGuid(), "abcSecret", "sa1"), new ClientInfoData(Guid.NewGuid().ToString())));
        var data = new OfferAutoSetupData(Guid.NewGuid(), "https://www.offer.com");
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        var result = await sut.AutoSetupServiceAsync(data, _iamUser.UserEntityId).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region UpdateServiceAsync
    
    [Fact]
    public async Task UpdateServiceAsync_WithoutService_ThrowsException()
    {
        // Arrange
        SetupUpdateService();
        var data = new ServiceUpdateRequestData("test", new List<LocalizedDescription>(), new List<ServiceTypeId>(), "123","test@email.com", Guid.NewGuid());
        var settings = new ServiceSettings();
        var sut = new ServiceBusinessLogic(_portalRepositories, _offerService, _offerSubscriptionService, Options.Create(settings));
     
        // Act
        async Task Act() => await sut.UpdateServiceAsync(_notExistingServiceId, data, _iamUser.UserEntityId).ConfigureAwait(false);

        // Assert
        var error = await Assert.ThrowsAsync<NotFoundException>(Act).ConfigureAwait(false);
        error.Message.Should().Be($"Service {_notExistingServiceId} does not exists");
    }
    
    [Fact]
    public async Task UpdateServiceAsync_WithActiveService_ThrowsException()
    {
        // Arrange
        SetupUpdateService();
        var data = new ServiceUpdateRequestData("test", new List<LocalizedDescription>(), new List<ServiceTypeId>(), "123","test@email.com", Guid.NewGuid());
        var settings = new ServiceSettings();
        var sut = new ServiceBusinessLogic(_portalRepositories, _offerService, _offerSubscriptionService, Options.Create(settings));
     
        // Act
        async Task Act() => await sut.UpdateServiceAsync(_activeServiceId, data, _iamUser.UserEntityId).ConfigureAwait(false);

        // Assert
        var error = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);
        error.Message.Should().Be("Service in State ACTIVE can't be updated");
    }

    [Fact]
    public async Task UpdateServiceAsync_WithInvalidUser_ThrowsException()
    {
        // Arrange
        SetupUpdateService();
        var data = new ServiceUpdateRequestData("test", new List<LocalizedDescription>(), new List<ServiceTypeId>(), "123","test@email.com", Guid.NewGuid());
        var settings = new ServiceSettings();
        var sut = new ServiceBusinessLogic(_portalRepositories, _offerService, _offerSubscriptionService, Options.Create(settings));

        // Act
        async Task Act() => await sut.UpdateServiceAsync(_differentCompanyServiceId, data, _iamUser.UserEntityId).ConfigureAwait(false);

        // Assert
        var error = await Assert.ThrowsAsync<ForbiddenException>(Act).ConfigureAwait(false);
        error.Message.Should().Be($"User {_iamUser.UserEntityId} is not allowed to change the service.");
    }

    [Fact]
    public async Task UpdateServiceAsync_WithValidData_ReturnsExpected()
    {
        // Arrange
        SetupUpdateService();
        var data = new ServiceUpdateRequestData(
            "test", 
            new List<LocalizedDescription>
            {
                new("de", "Long description", "desc") 
            }, 
            new List<ServiceTypeId>
            {
                ServiceTypeId.CONSULTANCE_SERVICE
            }, 
            "43",
            "test@email.com",
            _companyUser.Id);
        var settings = new ServiceSettings
        {
            SalesManagerRoles = new Dictionary<string, IEnumerable<string>>
            {
                { "portal", new[] { "SalesManager" } }
            }
        };
        var existingOffer = _fixture.Create<Offer>();
        A.CallTo(() => _offerRepository.AttachAndModifyOffer(A<Guid>._, A<Action<Offer>>._, A<Action<Offer>>._))
            .Invokes((Guid _, Action<Offer> setOptionalParameters, Action<Offer>? initializeParameters) =>
            {
                initializeParameters?.Invoke(existingOffer);
                setOptionalParameters(existingOffer);
            });
        var sut = new ServiceBusinessLogic(_portalRepositories, _offerService, _offerSubscriptionService, Options.Create(settings));

        // Act
        await sut.UpdateServiceAsync(_existingServiceId, data, _iamUser.UserEntityId).ConfigureAwait(false);
        
        // Assert
        A.CallTo(() => _offerRepository.AttachAndModifyOffer(A<Guid>._, A<Action<Offer>>._, A<Action<Offer>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerService.UpsertRemoveOfferDescription(A<Guid>._, A<IEnumerable<Localization>>._, A<IEnumerable<(string LanguageShortName, string DescriptionLong, string DescriptionShort)>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerService.CreateOrUpdateOfferLicense(A<Guid>._, A<string>._, A<(Guid offerLicenseId, string price, bool assignedToMultipleOffers)>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.AddServiceAssignedServiceTypes(A<IEnumerable<(Guid serviceId, ServiceTypeId serviceTypeId)>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.RemoveServiceAssignedServiceTypes(A<IEnumerable<(Guid serviceId, ServiceTypeId serviceTypeId)>>._))
            .MustHaveHappenedOnceExactly();
        existingOffer.Name.Should().Be("test");
    }

    #endregion
    
    #region SubmitServiceAsync

    [Fact]
    public async Task SubmitServiceAsync_CallsOfferService()
    {
        // Arrange
        var sut = new ServiceBusinessLogic(null!, _offerService, null!, _options);

        // Act
        await sut.SubmitServiceAsync(_existingServiceId, _iamUser.UserEntityId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => 
                _offerService.SubmitOfferAsync(
                    _existingServiceId,
                    _iamUser.UserEntityId,
                    OfferTypeId.SERVICE,
                    A<IEnumerable<NotificationTypeId>>._,
                    A<IDictionary<string, IEnumerable<string>>>._))
            .MustHaveHappenedOnceExactly();
    }

    #endregion
    
    #region DeclineServiceRequest
    
    [Fact]
    public async Task DeclineServiceRequestAsync_CallsExpected()
    {
        // Arrange
        var data = new OfferDeclineRequest("Just a test");
        var settings = new ServiceSettings
        {
            ServiceManagerRoles = _fixture.Create<Dictionary<string, IEnumerable<string>>>(),
            BasePortalAddress = "test"
        };
        var sut = new ServiceBusinessLogic(null!, _offerService, null!, Options.Create(settings));
     
        // Act
        await sut.DeclineServiceRequestAsync(_existingServiceId, _iamUser.UserEntityId, data).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _offerService.DeclineOfferAsync(_existingServiceId, _iamUser.UserEntityId, data,
            OfferTypeId.SERVICE, NotificationTypeId.SERVICE_RELEASE_REJECTION,
            A<IDictionary<string, IEnumerable<string>>>._, A<string>._)).MustHaveHappenedOnceExactly();
    }
    
    #endregion

    #region Create Service Document
    [Fact]
    public async Task CreateServiceDocument_ExecutesSuccessfully()
    {
        // Arrange
        var serviceId = _fixture.Create<Guid>();
        var file = FormFileHelper.GetFormFile("this is just a test", "superFile.pdf", "application/pdf");
        var settings = new ServiceSettings()
        {
            ContentTypeSettings = new[] { "application/pdf" },
            DocumentTypeIds = new[] { DocumentTypeId.ADDITIONAL_DETAILS }
        };
        var sut = new ServiceBusinessLogic(_portalRepositories, _offerService, null!, Options.Create(settings));

        // Act
        await sut.CreateServiceDocumentAsync(serviceId, DocumentTypeId.ADDITIONAL_DETAILS, file, _iamUser.UserEntityId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _offerService.UploadDocumentAsync(serviceId, DocumentTypeId.ADDITIONAL_DETAILS, file, _iamUser.UserEntityId, OfferTypeId.SERVICE, settings.DocumentTypeIds, settings.ContentTypeSettings, CancellationToken.None)).MustHaveHappenedOnceExactly();
    }

    #endregion
    
    #region Setup

    private void SetupUpdateService()
    {
        A.CallTo(() => _offerRepository.GetServiceUpdateData(_notExistingServiceId, A<IEnumerable<ServiceTypeId>>._, _iamUser.UserEntityId))
            .ReturnsLazily(() => (ServiceUpdateData?)null);
        A.CallTo(() => _offerRepository.GetServiceUpdateData(_activeServiceId, A<IEnumerable<ServiceTypeId>>._, _iamUser.UserEntityId))
            .ReturnsLazily(() => new ServiceUpdateData(OfferStatusId.ACTIVE, false, Array.Empty<(ServiceTypeId serviceTypeId, bool IsMatch)>(), new ValueTuple<Guid, string, bool>(), Array.Empty<(string, string, string)>(), null));
        A.CallTo(() => _offerRepository.GetServiceUpdateData(_differentCompanyServiceId, A<IEnumerable<ServiceTypeId>>._, _iamUser.UserEntityId))
            .ReturnsLazily(() => new ServiceUpdateData(OfferStatusId.CREATED, false, Array.Empty<(ServiceTypeId serviceTypeId, bool IsMatch)>(), new ValueTuple<Guid, string, bool>(), Array.Empty<(string, string, string)>(), null));
        A.CallTo(() => _offerRepository.GetServiceUpdateData(_existingServiceId, A<IEnumerable<ServiceTypeId>>._, _iamUser.UserEntityId))
            .ReturnsLazily(() => new ServiceUpdateData(OfferStatusId.CREATED, true, Enumerable.Repeat(new ValueTuple<ServiceTypeId, bool>(ServiceTypeId.DATASPACE_SERVICE, false), 1), new ValueTuple<Guid, string, bool>(Guid.NewGuid(), "123", false), Array.Empty<(string, string, string)>(), Guid.NewGuid()));
        A.CallTo(() => _offerService.ValidateSalesManager(A<Guid>._, A<string>._, A<IDictionary<string, IEnumerable<string>>>._)).Returns(_companyUser.CompanyId);
    }

    private void SetupPagination(int count = 5)
    {
        var serviceDetailData = _fixture.CreateMany<ServiceOverviewData>(count);
        var paginationResult = (int skip, int take) => Task.FromResult(new Pagination.Source<ServiceOverviewData>(serviceDetailData.Count(), serviceDetailData.Skip(skip).Take(take)));
        
        A.CallTo(() => _offerRepository.GetActiveServicesPaginationSource(A<ServiceOverviewSorting?>._, A<ServiceTypeId?>._))
            .Returns(paginationResult);
        
        A.CallTo(() => _portalRepositories.GetInstance<IOfferRepository>()).Returns(_offerRepository);
    }

    private void SetupRepositories()
    {
        var serviceDetail = _fixture.Build<ServiceDetailData>()
            .With(x => x.Id, _existingServiceId)
            .Create();
        A.CallTo(() => _userRepository.GetCompanyUserWithIamUserCheckAndCompanyName(_iamUser.UserEntityId, _companyUser.Id))
            .ReturnsLazily(() => new List<(Guid CompanyUserId, bool IsIamUser, string CompanyUserName, Guid CompanyId)>{new (_companyUser.Id, true, "COMPANYBPN", _companyUserCompanyId), new (_companyUser.Id, false, "OTHERCOMPANYBPN", _companyUserCompanyId)}.ToAsyncEnumerable());
        A.CallTo(() => _userRepository.GetCompanyUserWithIamUserCheckAndCompanyName(_iamUser.UserEntityId, A<Guid>.That.Not.Matches(x => x == _companyUser.Id)))
            .ReturnsLazily(() => new List<(Guid CompanyUserId, bool IsIamUser, string CompanyUserName, Guid CompanyId)>{new (_companyUser.Id, true, "COMPANYBPN", _companyUserCompanyId)}.ToAsyncEnumerable());
        A.CallTo(() => _userRepository.GetCompanyUserWithIamUserCheckAndCompanyName(A<string>.That.Not.Matches(x => x == _iamUser.UserEntityId), _companyUser.Id))
            .ReturnsLazily(() => new List<(Guid CompanyUserId, bool IsIamUser, string CompanyUserName, Guid CompanyId)>{new (_companyUser.Id, false, "OTHERCOMPANYBPN", _companyUserCompanyId)}.ToAsyncEnumerable());
        A.CallTo(() => _userRepository.GetCompanyUserWithIamUserCheckAndCompanyName(A<string>.That.Not.Matches(x => x == _iamUser.UserEntityId), A<Guid>.That.Not.Matches(x => x == _companyUser.Id)))
            .ReturnsLazily(() => new List<(Guid CompanyUserId, bool IsIamUser, string CompanyUserName, Guid CompanyId)>().ToAsyncEnumerable());

        A.CallTo(() => _userRepository.GetOwnCompanyAndCompanyUserId(_iamUser.UserEntityId))
            .ReturnsLazily(() => (_companyUser.Id, _companyUser.CompanyId));
        A.CallTo(() => _userRepository.GetOwnCompanyAndCompanyUserId(_notAssignedCompanyIdUser))
            .ReturnsLazily(() => (_companyUser.Id, Guid.Empty));
        A.CallTo(() => _userRepository.GetOwnCompanyAndCompanyUserId(A<string>.That.Not.Matches(x => x == _iamUser.UserEntityId || x == _notAssignedCompanyIdUser)))
            .ReturnsLazily(() => (Guid.Empty, _companyUser.CompanyId));

        A.CallTo(() => _userRepository.GetOwnCompanyInformationWithCompanyUserIdAndEmailAsync(_iamUser.UserEntityId))
            .ReturnsLazily(() => (new CompanyInformationData(_companyUser.CompanyId, "The Company", "DE", "BPN00000001"), _companyUser.Id, "test@mail.de"));
        A.CallTo(() => _userRepository.GetOwnCompanyInformationWithCompanyUserIdAndEmailAsync(_notAssignedCompanyIdUser))
            .ReturnsLazily(() => (new CompanyInformationData(Guid.Empty, "The Company", "DE", "BPN00000001"), _companyUser.Id, "test@mail.de"));
        A.CallTo(() => _userRepository.GetOwnCompanyInformationWithCompanyUserIdAndEmailAsync(A<string>.That.Not.Matches(x => x == _iamUser.UserEntityId || x == _notAssignedCompanyIdUser)))
            .ReturnsLazily(() => (new CompanyInformationData(_companyUser.CompanyId, "The Company", "DE", "BPN00000001"), Guid.Empty, "test@mail.de"));
        
        A.CallTo(() => _offerRepository.GetServiceDetailByIdUntrackedAsync(_existingServiceId, A<string>.That.Matches(x => x == "en"), A<string>._))
            .ReturnsLazily(() => serviceDetail with {OfferSubscriptionDetailData = new []
            {
                new OfferSubscriptionStateDetailData(Guid.NewGuid(), OfferSubscriptionStatusId.ACTIVE)
            }, Documents = new [] {
                new DocumentTypeData(DocumentTypeId.ADDITIONAL_DETAILS,Guid.NewGuid(), "testDocument")
            } });
        A.CallTo(() => _offerRepository.GetServiceDetailByIdUntrackedAsync(A<Guid>.That.Not.Matches(x => x == _existingServiceId), A<string>._, A<string>._))
            .ReturnsLazily(() => (ServiceDetailData?)null);

        A.CallTo(() => _offerRepository.GetOfferProviderDetailsAsync(A<Guid>.That.Matches(x => x == _existingServiceId), A<OfferTypeId>._))
            .ReturnsLazily(() => new OfferProviderDetailsData("Test Service", "Test Company", "provider@mail.de", new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"), "https://www.testurl.com"));
        A.CallTo(() => _offerRepository.GetOfferProviderDetailsAsync(A<Guid>.That.Matches(x => x == _existingServiceWithFailingAutoSetupId), A<OfferTypeId>._))
            .ReturnsLazily(() => new OfferProviderDetailsData("Test Service", "Test Company", "provider@mail.de", new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"), "https://www.fail.com"));
        A.CallTo(() => _offerRepository.GetOfferProviderDetailsAsync(A<Guid>.That.Not.Matches(x => x == _existingServiceId || x == _existingServiceWithFailingAutoSetupId), A<OfferTypeId>._))
            .ReturnsLazily(() => (OfferProviderDetailsData?)null);

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
                A<string>.That.Matches(x => x == _iamUser.UserEntityId),
                A<OfferTypeId>._))
            .ReturnsLazily(() =>
                new SubscriptionDetailData(_existingServiceId, "Super Service", OfferSubscriptionStatusId.ACTIVE));
        A.CallTo(() => _offerSubscriptionsRepository.GetSubscriptionDetailDataForOwnUserAsync(
                A<Guid>.That.Not.Matches(x => x == _validSubscriptionId),
                A<string>.That.Matches(x => x == _iamUser.UserEntityId),
                A<OfferTypeId>._))
            .ReturnsLazily(() => (SubscriptionDetailData?)null);
        A.CallTo(() => _offerSubscriptionsRepository.GetCompanyIdWithAssignedOfferForCompanyUserAndSubscriptionAsync(
                A<Guid>.That.Matches(x => x == _existingServiceId), A<string>.That.Matches(x => x == _iamUser.UserEntityId), A<OfferTypeId>._))
            .ReturnsLazily(() => new ValueTuple<Guid, OfferSubscription?, Guid>(_companyUser.CompanyId, offerSubscription, _companyUser.Id));
        A.CallTo(() => _offerSubscriptionsRepository.GetCompanyIdWithAssignedOfferForCompanyUserAndSubscriptionAsync(
                A<Guid>.That.Not.Matches(x => x == _existingServiceId), A<string>.That.Matches(x => x == _iamUser.UserEntityId),
                A<OfferTypeId>._))
            .ReturnsLazily(() => new ValueTuple<Guid, OfferSubscription?, Guid>(_companyUser.CompanyId, null, _companyUser.Id));
        A.CallTo(() => _offerSubscriptionsRepository.GetCompanyIdWithAssignedOfferForCompanyUserAndSubscriptionAsync(
                A<Guid>.That.Matches(x => x == _existingServiceId), A<string>.That.Not.Matches(x => x == _iamUser.UserEntityId),
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

        var userRoleData = _fixture.CreateMany<UserRoleData>(3);
        A.CallTo(
                () => _userRolesRepository.GetUserRoleDataUntrackedAsync(A<IDictionary<string, IEnumerable<string>>>._))
            .ReturnsLazily(() => userRoleData.ToAsyncEnumerable());

        A.CallTo(() => _userRolesRepository.GetUserRolesForOfferIdAsync(A<Guid>.That.Matches(x => x == _existingServiceId)))
            .ReturnsLazily(() => new List<string> { "Buyer", "Supplier" });
        A.CallTo(() => _portalRepositories.GetInstance<IAgreementRepository>()).Returns(_agreementRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IConsentRepository>()).Returns(_consentRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IOfferRepository>()).Returns(_offerRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IOfferSubscriptionsRepository>()).Returns(_offerSubscriptionsRepository);
        A.CallTo(() => _portalRepositories.GetInstance<INotificationRepository>()).Returns(_notificationRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRolesRepository>()).Returns(_userRolesRepository);
        _fixture.Inject(_portalRepositories);
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
