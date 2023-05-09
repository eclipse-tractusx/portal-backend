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
using System.Collections.Immutable;
using Xunit;

namespace Org.Eclipse.TractusX.Portal.Backend.Services.Service.Tests.BusinessLogic;

public class ServiceReleaseBusinessLogicTest
{
    private readonly Guid _notExistingServiceId = Guid.NewGuid();
    private readonly Guid _companyUserId = Guid.NewGuid();
    private readonly Guid _companyUserCompanyId = Guid.NewGuid();
    private readonly Guid _existingServiceId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47661");
    private readonly Guid _activeServiceId = Guid.NewGuid();
    private readonly Guid _differentCompanyServiceId = Guid.NewGuid();
    private readonly string _iamUserId = Guid.NewGuid().ToString();
    private readonly IFixture _fixture;
    private readonly IOfferRepository _offerRepository;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IOfferService _offerService;
    private readonly IStaticDataRepository _staticDataRepository;
    private readonly ITechnicalUserProfileRepository _technicalUserProfileRepository;
    private readonly ServiceReleaseBusinessLogic _sut;
    private readonly IOptions<ServiceSettings> _options;
    public ServiceReleaseBusinessLogicTest()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _portalRepositories = A.Fake<IPortalRepositories>();
        _offerRepository = A.Fake<IOfferRepository>();
        _offerService = A.Fake<IOfferService>();
        _staticDataRepository = A.Fake<IStaticDataRepository>();
        _technicalUserProfileRepository = A.Fake<ITechnicalUserProfileRepository>();

        SetupRepositories();
        var serviceSettings = new ServiceSettings
        {
            ApplicationsMaxPageSize = 15, 
            ITAdminRoles = new Dictionary<string, IEnumerable<string>>
            {
                {"Cl2-CX-Portal", new[] {"IT Admin"}}
            },
            SubmitServiceNotificationTypeIds = new List<NotificationTypeId>
            {
                NotificationTypeId.SERVICE_RELEASE_REQUEST
            },
            OfferStatusIds = new List<OfferStatusId>
            {
                OfferStatusId.ACTIVE , 
                OfferStatusId.IN_REVIEW
            }
        };
        _options = Options.Create(serviceSettings);
        _fixture.Inject(_options);
        _sut = new ServiceReleaseBusinessLogic(_portalRepositories, _offerService, _options);
    }

    [Fact]
    public async Task GetServiceAgreementData_ReturnsExpectedResult()
    {
        //Arrange
        var data = _fixture.CreateMany<AgreementDocumentData>(5).ToAsyncEnumerable();
        A.CallTo(() => _offerService.GetOfferTypeAgreements(OfferTypeId.SERVICE))
            .Returns(data);

        //Act
        var result = await _sut.GetServiceAgreementDataAsync().ToListAsync().ConfigureAwait(false);
        
        // Assert 
        A.CallTo(() => _offerService.GetOfferTypeAgreements(OfferTypeId.SERVICE))
            .MustHaveHappenedOnceExactly();
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetServiceDetailsByIdAsync_ReturnsExpectedResult()
    {
        //Arrange
        var data = _fixture.Build<ServiceDetailsData>()
                           .With(x=>x.OfferStatusId, OfferStatusId.IN_REVIEW)
                           .Create();
        var serviceId = _fixture.Create<Guid>();
       
        A.CallTo(() => _offerRepository.GetServiceDetailsByIdAsync(serviceId))
            .Returns(data);

        //Act
        var result = await _sut.GetServiceDetailsByIdAsync(serviceId).ConfigureAwait(false);
        
        // Assert 
        A.CallTo(() => _offerRepository.GetServiceDetailsByIdAsync(A<Guid>._))
            .MustHaveHappenedOnceExactly();
        result.Should().BeOfType<ServiceData>();
        result.Title.Should().NotBeNull().And.Be(data.Title);
        result.Provider.Should().Be(data.Provider);
        result.ProviderUri.Should().NotBeNull().And.Be(data.ProviderUri);
        result.ContactEmail.Should().NotBeNull().And.Be(data.ContactEmail);
        result.ContactNumber.Should().NotBeNull().And.Be(data.ContactNumber);
        result.OfferStatus.Should().Be(data.OfferStatusId);
        result.TechnicalUserProfile.Should().HaveSameCount(data.TechnicalUserProfile).And.AllSatisfy(
            x => data.TechnicalUserProfile.Should().ContainSingle(d => d.TechnicalUserProfileId == x.Key).Which.UserRoles.Should().ContainInOrder(x.Value)
        );
    }

    [Fact]
    public async Task GetServiceDetailsByIdWillNullPropertiesAsync_ReturnsExpectedResult()
    {
        //Arrange
        var data = _fixture.Build<ServiceDetailsData>()
                           .With(x => x.OfferStatusId, OfferStatusId.IN_REVIEW)
                           .With(x => x.Title, (string?)null)
                           .With(x => x.ProviderUri, (string?)null)
                           .With(x => x.ContactEmail, (string?)null)
                           .With(x => x.ContactNumber, (string?)null)
                           .Create();
        var serviceId = _fixture.Create<Guid>();
       
        A.CallTo(() => _offerRepository.GetServiceDetailsByIdAsync(serviceId))
            .Returns(data);

        //Act
        var result = await _sut.GetServiceDetailsByIdAsync(serviceId).ConfigureAwait(false);
        
        // Assert 
        A.CallTo(() => _offerRepository.GetServiceDetailsByIdAsync(A<Guid>._))
            .MustHaveHappenedOnceExactly();
        result.Should().BeOfType<ServiceData>();
        result.Title.Should().Be(Constants.ErrorString);
        result.ProviderUri.Should().Be(Constants.ErrorString);
        result.ContactEmail.Should().BeNull();
        result.ContactNumber.Should().BeNull();
    }

    [Fact]
    public async Task GetServiceDetailsByIdAsync_WithInvalidServiceId_ThrowsException()
    {
        // Arrange
        var invalidServiceId = Guid.NewGuid();
        A.CallTo(() => _offerRepository.GetServiceDetailsByIdAsync(invalidServiceId))
           .Returns((ServiceDetailsData?)null);

        // Act
        async Task Act() => await _sut.GetServiceDetailsByIdAsync(invalidServiceId).ConfigureAwait(false);

        // Assert
        var error = await Assert.ThrowsAsync<NotFoundException>(Act).ConfigureAwait(false);
        error.Message.Should().Be($"serviceId {invalidServiceId} not found or Incorrect Status");
    }

    [Fact]
    public async Task GetServiceTypeData_ReturnExpectedResult()
    {
        // Arrange
        var data = _fixture.Build<ServiceTypeData>()
                            .With(x=>x.ServiceTypeId, 1)
                            .With(x=>x.Name, ServiceTypeId.CONSULTANCE_SERVICE.ToString())
                            .CreateMany()
                            .ToAsyncEnumerable();
       
        A.CallTo(() => _staticDataRepository.GetServiceTypeData())
            .Returns(data);
        var sut = _fixture.Create<ServiceReleaseBusinessLogic>();

        // Act
        var result = await sut.GetServiceTypeDataAsync().ToListAsync().ConfigureAwait(false);

        // Assert
        A.CallTo(() => _staticDataRepository.GetServiceTypeData())
            .MustHaveHappenedOnceExactly();
        result.Should().BeOfType<List<ServiceTypeData>>();
        result.FirstOrDefault()!.ServiceTypeId.Should().Be(1);
        result.FirstOrDefault()!.Name.Should().Be(ServiceTypeId.CONSULTANCE_SERVICE.ToString());
    }
    
    [Fact]
    public async Task GetServiceAgreementConsentAsync_ReturnsExpectedResult()
    {
        //Arrange
        var data = _fixture.Create<OfferAgreementConsent>();
        var serviceId = Guid.NewGuid();
        var iamUserId = _fixture.Create<string>();
        var offerService = A.Fake<IOfferService>();
        _fixture.Inject(offerService);
        A.CallTo(() => offerService.GetProviderOfferAgreementConsentById(A<Guid>._, A<string>._, OfferTypeId.SERVICE))
            .Returns(data);

        //Act
        var sut = _fixture.Create<ServiceReleaseBusinessLogic>();
        var result = await sut.GetServiceAgreementConsentAsync(serviceId, iamUserId).ConfigureAwait(false);
        
        // Assert 
        A.CallTo(() => offerService.GetProviderOfferAgreementConsentById(serviceId, iamUserId, OfferTypeId.SERVICE))
            .MustHaveHappenedOnceExactly();
        result.Should().BeOfType<OfferAgreementConsent>();
    } 

    [Fact]
    public async Task GetServiceTypeDataAsync_ReturnsExpected()
    {
        var serviceId = Guid.NewGuid();
        var iamUserId = Guid.NewGuid().ToString();
        var data = _fixture.Build<OfferProviderResponse>()
            .With(x => x.Title, "test title")
            .With(x => x.ContactEmail, "info@test.de")
            .With(x => x.UseCase, (IEnumerable<AppUseCaseData>?) null)
            .With(x => x.ServiceTypeIds, new [] {ServiceTypeId.DATASPACE_SERVICE, ServiceTypeId.CONSULTANCE_SERVICE})
            .Create();

        A.CallTo(() => _offerService.GetProviderOfferDetailsForStatusAsync(serviceId, iamUserId, OfferTypeId.SERVICE))
            .Returns(data);

        var result = await _sut.GetServiceDetailsForStatusAsync(serviceId, iamUserId).ConfigureAwait(false);

        result.Should().NotBeNull();
        result.Title.Should().Be("test title");
        result.ContactEmail.Should().Be("info@test.de");
        result.ServiceTypeIds.Should().HaveCount(2);
        result.TechnicalUserProfile.Should().HaveSameCount(data.TechnicalUserProfile).And.AllSatisfy(
            x => data.TechnicalUserProfile.Should().ContainSingle(d => d.Key == x.Key).Which.Value.Should().ContainInOrder(x.Value)
        );
    }

    #region GetAllInReviewStatusApps

    [Theory]
    [InlineData(ServiceReleaseStatusIdFilter.All, true)]
    [InlineData(ServiceReleaseStatusIdFilter.InReview, false)]
    [InlineData(null, true)]
    public async Task GetAllInReviewStatusServiceAsync_ReturnsExpected(ServiceReleaseStatusIdFilter? filter, bool isOptions)
    {
        // Arrange
        var inReviewData = _fixture.CreateMany<InReviewServiceData>(15).ToImmutableArray();
        var paginationResult = (int skip, int take) => Task.FromResult(new Pagination.Source<InReviewServiceData>(15, inReviewData.Skip(skip).Take(take)));
        A.CallTo(() => _offerRepository.GetAllInReviewStatusServiceAsync(A<IEnumerable<OfferStatusId>>._, A<OfferTypeId>._, A<OfferSorting>._, A<string>._, A<string>._, A<string>._))
            .Returns(paginationResult);

        // Act
        var result = await _sut.GetAllInReviewStatusServiceAsync(1, 5, OfferSorting.DateAsc, null, "en", filter).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _offerRepository.GetAllInReviewStatusServiceAsync(
            A<IEnumerable<OfferStatusId>>.That.Matches(x =>
                x.SequenceEqual(isOptions
                    ? _options.Value.OfferStatusIds
                    : new [] { OfferStatusId.IN_REVIEW })),
            OfferTypeId.SERVICE, A<OfferSorting>._, A<string>._, A<string>._, A<string>._)).MustHaveHappenedOnceExactly();

        result.Should().BeOfType<Pagination.Response<InReviewServiceData>>()
            .Which.Meta.Should().Match<Pagination.Metadata>(x =>
                x.NumberOfElements == 15 &&
                x.NumberOfPages == 3 &&
                x.Page == 1 &&
                x.PageSize == 5);
        result.Content.Should().HaveCount(5).And.Satisfy(
            x => x == inReviewData[5],
            x => x == inReviewData[6],
            x => x == inReviewData[7],
            x => x == inReviewData[8],
            x => x == inReviewData[9]
        );
    }

    #endregion

    #region SubmitOfferConsentAsync

    [Fact]
    public async Task SubmitOfferConsentAsync_WithValidData_ReturnsExpected()
    {
        var serviceId = Guid.NewGuid();
        var iamUserId = Guid.NewGuid().ToString();
        var data = new OfferAgreementConsent(new List<AgreementConsentStatus>());

        A.CallTo(() => _offerService.CreateOrUpdateProviderOfferAgreementConsent(serviceId, data, iamUserId, OfferTypeId.SERVICE))
            .ReturnsLazily(() => new []{ new ConsentStatusData(Guid.NewGuid(), ConsentStatusId.ACTIVE)});

        var result = await _sut.SubmitOfferConsentAsync(serviceId, data, iamUserId).ConfigureAwait(false);

        result.Should().ContainSingle().Which.ConsentStatus.Should().Be(ConsentStatusId.ACTIVE);
    }

    [Fact]
    public async Task SubmitOfferConsentAsync_WithEmptyGuid_ThrowsControllerArgumentException()
    {
        var data = new OfferAgreementConsent(new List<AgreementConsentStatus>());
        async Task Act() => await _sut.SubmitOfferConsentAsync(Guid.Empty, data,  Guid.NewGuid().ToString()).ConfigureAwait(false);

        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);
        ex.Message.Should().Be("ServiceId must not be empty");
    }

    #endregion

    #region DeleteServiceDocument

    [Fact]
    public async Task DeleteServiceDocumentsAsync_ReturnsExpected()
    {
        // Arrange
        var documentId = _fixture.Create<Guid>();
        var IamUserId = _fixture.Create<Guid>().ToString();

        // Act
        await _sut.DeleteServiceDocumentsAsync(documentId, IamUserId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _offerService.DeleteDocumentsAsync(documentId, IamUserId, A<IEnumerable<DocumentTypeId>>._, OfferTypeId.SERVICE)).MustHaveHappenedOnceExactly();

    }

    #endregion

    #region Create Service

    [Fact]
    public async Task CreateServiceOffering_WithValidDataAndEmptyDescriptions_ReturnsCorrectDetails()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var offerService = A.Fake<IOfferService>();
        _fixture.Inject(offerService);
        A.CallTo(() => offerService.CreateServiceOfferingAsync(A<ServiceOfferingData>._, A<string>._, A<OfferTypeId>._)).ReturnsLazily(() => serviceId);
        var sut = _fixture.Create<ServiceReleaseBusinessLogic>();

        // Act
        var result = await sut.CreateServiceOfferingAsync(new ServiceOfferingData("Newest Service", "42", "mail@test.de", _companyUserId, new List<LocalizedDescription>(), new List<ServiceTypeId>(), null), _iamUserId);

        // Assert
        result.Should().Be(serviceId);
    }

    #endregion

    #region UpdateServiceAsync
    
    [Fact]
    public async Task UpdateServiceAsync_WithoutService_ThrowsException()
    {
        // Arrange
        SetupUpdateService();
        var data = new ServiceUpdateRequestData("test", new List<LocalizedDescription>(), new List<ServiceTypeId>(), "123","test@email.com", Guid.NewGuid(), null);
        var sut = new ServiceReleaseBusinessLogic(_portalRepositories, _offerService, Options.Create(new ServiceSettings()));
     
        // Act
        async Task Act() => await sut.UpdateServiceAsync(_notExistingServiceId, data, _iamUserId).ConfigureAwait(false);

        // Assert
        var error = await Assert.ThrowsAsync<NotFoundException>(Act).ConfigureAwait(false);
        error.Message.Should().Be($"Service {_notExistingServiceId} does not exists");
    }
    
    [Fact]
    public async Task UpdateServiceAsync_WithActiveService_ThrowsException()
    {
        // Arrange
        SetupUpdateService();
        var data = new ServiceUpdateRequestData("test", new List<LocalizedDescription>(), new List<ServiceTypeId>(), "123","test@email.com", Guid.NewGuid(), null);
        var sut = new ServiceReleaseBusinessLogic(_portalRepositories, _offerService, Options.Create(new ServiceSettings()));
     
        // Act
        async Task Act() => await sut.UpdateServiceAsync(_activeServiceId, data, _iamUserId).ConfigureAwait(false);

        // Assert
        var error = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);
        error.Message.Should().Be("Service in State ACTIVE can't be updated");
    }

    [Fact]
    public async Task UpdateServiceAsync_WithInvalidUser_ThrowsException()
    {
        // Arrange
        SetupUpdateService();
        var data = new ServiceUpdateRequestData("test", new List<LocalizedDescription>(), new List<ServiceTypeId>(), "123","test@email.com", Guid.NewGuid(), null);
        var sut = new ServiceReleaseBusinessLogic(_portalRepositories, _offerService, Options.Create(new ServiceSettings()));

        // Act
        async Task Act() => await sut.UpdateServiceAsync(_differentCompanyServiceId, data, _iamUserId).ConfigureAwait(false);

        // Assert
        var error = await Assert.ThrowsAsync<ForbiddenException>(Act).ConfigureAwait(false);
        error.Message.Should().Be($"User {_iamUserId} is not allowed to change the service.");
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
            _companyUserId,
            null);
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
        var sut = new ServiceReleaseBusinessLogic(_portalRepositories, _offerService, Options.Create(settings));

        // Act
        await sut.UpdateServiceAsync(_existingServiceId, data, _iamUserId).ConfigureAwait(false);
        
        // Assert
        A.CallTo(() => _offerRepository.AttachAndModifyOffer(A<Guid>._, A<Action<Offer>>._, A<Action<Offer>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerService.UpsertRemoveOfferDescription(A<Guid>._, A<IEnumerable<LocalizedDescription>>._, A<IEnumerable<LocalizedDescription>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerService.CreateOrUpdateOfferLicense(A<Guid>._, A<string>._, A<(Guid offerLicenseId, string price, bool assignedToMultipleOffers)>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.AddServiceAssignedServiceTypes(A<IEnumerable<(Guid serviceId, ServiceTypeId serviceTypeId)>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _offerRepository.RemoveServiceAssignedServiceTypes(A<IEnumerable<(Guid serviceId, ServiceTypeId serviceTypeId)>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _technicalUserProfileRepository.RemoveTechnicalUserProfilesForOffer(_existingServiceId)).MustHaveHappenedOnceExactly();
        existingOffer.Name.Should().Be("test");
    }

    #endregion
    
    #region SubmitServiceAsync

    [Fact]
    public async Task SubmitServiceAsync_CallsOfferService()
    {
        // Arrange
        var sut = new ServiceReleaseBusinessLogic(null!, _offerService, _options);

        // Act
        await sut.SubmitServiceAsync(_existingServiceId, _iamUserId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => 
                _offerService.SubmitServiceAsync(
                    _existingServiceId,
                    _iamUserId,
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
        var sut = new ServiceReleaseBusinessLogic(null!, _offerService, Options.Create(settings));
     
        // Act
        await sut.DeclineServiceRequestAsync(_existingServiceId, _iamUserId, data).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _offerService.DeclineOfferAsync(_existingServiceId, _iamUserId, data,
            OfferTypeId.SERVICE, NotificationTypeId.SERVICE_RELEASE_REJECTION,
            A<IDictionary<string, IEnumerable<string>>>._, A<string>._,
            A<IEnumerable<NotificationTypeId>>._, A<IDictionary<string, IEnumerable<string>>>._)).MustHaveHappenedOnceExactly();
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
            UploadServiceDocumentTypeIds = new Dictionary<DocumentTypeId, IEnumerable<string>>{
                { DocumentTypeId.ADDITIONAL_DETAILS, new []{ "application/pdf" } }}
        };
        var sut = new ServiceReleaseBusinessLogic(_portalRepositories, _offerService, Options.Create(settings));

        // Act
        await sut.CreateServiceDocumentAsync(serviceId, DocumentTypeId.ADDITIONAL_DETAILS, file, _iamUserId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _offerService.UploadDocumentAsync(serviceId, DocumentTypeId.ADDITIONAL_DETAILS, file, _iamUserId, OfferTypeId.SERVICE, settings.UploadServiceDocumentTypeIds, CancellationToken.None)).MustHaveHappenedOnceExactly();
    }

    #endregion

    #region ApproveServiceRequestAsync

    [Fact]
    public async Task ApproveServiceRequestAsync_WithValid_CallsExpected()
    {
        // Arrange
        var appId = Guid.NewGuid();
        var sut = new ServiceReleaseBusinessLogic(_portalRepositories, _offerService, Options.Create(new ServiceSettings()));
        
        // Act
        await sut.ApproveServiceRequestAsync(appId, _iamUserId).ConfigureAwait(false);
        
        // Assert
        A.CallTo(() => _offerService.ApproveOfferRequestAsync(appId, _iamUserId, OfferTypeId.SERVICE,
            A<IEnumerable<NotificationTypeId>>._, A<IDictionary<string, IEnumerable<string>>>._,
            A<IEnumerable<NotificationTypeId>>._, A<IDictionary<string, IEnumerable<string>>>._)).MustHaveHappenedOnceExactly();
    }

    #endregion

    private void SetupUpdateService()
    {
        A.CallTo(() => _offerRepository.GetServiceUpdateData(_notExistingServiceId, A<IEnumerable<ServiceTypeId>>._, _iamUserId))
            .ReturnsLazily(() => (ServiceUpdateData?)null);
        A.CallTo(() => _offerRepository.GetServiceUpdateData(_activeServiceId, A<IEnumerable<ServiceTypeId>>._, _iamUserId))
            .ReturnsLazily(() => new ServiceUpdateData(OfferStatusId.ACTIVE, false, Array.Empty<(ServiceTypeId serviceTypeId, bool IsMatch)>(), new ValueTuple<Guid, string, bool>(), Array.Empty<LocalizedDescription>(), null));
        A.CallTo(() => _offerRepository.GetServiceUpdateData(_differentCompanyServiceId, A<IEnumerable<ServiceTypeId>>._, _iamUserId))
            .ReturnsLazily(() => new ServiceUpdateData(OfferStatusId.CREATED, false, Array.Empty<(ServiceTypeId serviceTypeId, bool IsMatch)>(), new ValueTuple<Guid, string, bool>(), Array.Empty<LocalizedDescription>(), null));
        A.CallTo(() => _offerRepository.GetServiceUpdateData(_existingServiceId, A<IEnumerable<ServiceTypeId>>._, _iamUserId))
            .ReturnsLazily(() => new ServiceUpdateData(OfferStatusId.CREATED, true, Enumerable.Repeat(new ValueTuple<ServiceTypeId, bool>(ServiceTypeId.DATASPACE_SERVICE, false), 1), new ValueTuple<Guid, string, bool>(Guid.NewGuid(), "123", false), Array.Empty<LocalizedDescription>(), Guid.NewGuid()));
        A.CallTo(() => _offerService.ValidateSalesManager(A<Guid>._, A<string>._, A<IDictionary<string, IEnumerable<string>>>._)).Returns(_companyUserCompanyId);
    }

    private void SetupRepositories()
    {
        A.CallTo(() => _portalRepositories.GetInstance<IOfferRepository>()).Returns(_offerRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IStaticDataRepository>()).Returns(_staticDataRepository);
        A.CallTo(() => _portalRepositories.GetInstance<ITechnicalUserProfileRepository>()).Returns(_technicalUserProfileRepository);
        _fixture.Inject(_portalRepositories);
    }
}
