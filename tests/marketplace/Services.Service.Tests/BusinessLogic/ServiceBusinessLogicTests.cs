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
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Web;
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

public class ServiceBusinessLogicTests
{
    private const string IamUserId = "502dabcf-01c7-47d9-a88e-0be4279097b5";

    private readonly Guid _companyUserCompanyId = new("395f955b-f11b-4a74-ab51-92a526c1973a");
    private readonly string _notAssignedCompanyIdUser = "395f955b-f11b-4a74-ab51-92a526c1973c";
    private readonly Guid _existingServiceId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47661");
    private readonly Guid _existingServiceWithFailingAutoSetupId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47662");
    private readonly Guid _validSubscriptionId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47662");
    private readonly Guid _existingAgreementId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47664");
    private readonly Guid _validConsentId = new("9aae7a3b-b188-4a42-b46b-fb2ea5f47665");
    private readonly CompanyUser _companyUser;
    private readonly IFixture _fixture;
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
    private readonly IOfferDocumentService _offerDocumentService;
    private readonly IOptions<ServiceSettings> _options;

    public ServiceBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _companyUser = _fixture.Build<CompanyUser>()
            .With(u => u.UserEntityId, IamUserId)
            .With(u => u.CompanyId, _companyUserCompanyId)
            .Create();

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
        _offerDocumentService = A.Fake<IOfferDocumentService>();

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
            }
        };
        _options = Options.Create(serviceSettings);
        _fixture.Inject(_options);
    }

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
        A.CallTo(() => _offerSubscriptionService.AddOfferSubscriptionAsync(A<Guid>._, A<IEnumerable<OfferAgreementConsentData>>._, A<string>._, A<OfferTypeId>._, A<string>._))
            .Returns(offerSubscriptionId);
        var serviceSettings = new ServiceSettings
        {
            ServiceManagerRoles = new Dictionary<string, IEnumerable<string>>
            {
                { "portal", new [] { "ServiceManager" }}
            },
            BasePortalAddress = "https://base-portal-address-test.de"
        };
        var sut = new ServiceBusinessLogic(null!, null!, null!, _offerSubscriptionService, null!, Options.Create(serviceSettings));

        // Act
        var result = await sut.AddServiceSubscription(_existingServiceId, consentData, IamUserId, "THISISAACCESSTOKEN");

        // Assert
        result.Should().Be(offerSubscriptionId);
        A.CallTo(() => _offerSubscriptionService.AddOfferSubscriptionAsync(
            A<Guid>._,
            A<IEnumerable<OfferAgreementConsentData>>._,
            A<string>._,
            A<OfferTypeId>.That.Matches(x => x == OfferTypeId.SERVICE),
            A<string>._))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region GetCompanyProvidedServiceSubscriptionStatusesForUser

    [Theory]
    [InlineData(null)]
    [InlineData("c714b905-9d2a-4cf3-b9f7-10be4eeddfc8")]
    public async Task GetCompanyProvidedServiceSubscriptionStatusesForUserAsync_ReturnsExpected(string? offerIdTxt)
    {
        // Arrange
        Guid? offerId = offerIdTxt == null ? null : new Guid(offerIdTxt);
        var data = _fixture.CreateMany<OfferCompanySubscriptionStatusData>(5).ToImmutableArray();
        A.CallTo(() => _offerSubscriptionsRepository.GetOwnCompanyProvidedOfferSubscriptionStatusesUntrackedAsync(IamUserId, OfferTypeId.SERVICE, null, OfferSubscriptionStatusId.ACTIVE, A<Guid?>._))
            .Returns((skip, take) => Task.FromResult(new Pagination.Source<OfferCompanySubscriptionStatusData>(data.Length, data.Skip(skip).Take(take)))!);

        var serviceSettings = new ServiceSettings
        {
            ApplicationsMaxPageSize = 15
        };
        var sut = new ServiceBusinessLogic(_portalRepositories, null!, null!, null!, null!, Options.Create(serviceSettings));

        // Act
        var result = await sut.GetCompanyProvidedServiceSubscriptionStatusesForUserAsync(0, 10, IamUserId, null, null, offerId).ConfigureAwait(false);

        // Assert
        result.Meta.NumberOfElements.Should().Be(5);
        result.Content.Should().HaveCount(5).And.Satisfy(
            x => x.OfferId == data[0].OfferId && x.OfferName == data[0].ServiceName && x.CompanySubscriptionStatuses.SequenceEqual(data[0].CompanySubscriptionStatuses) && x.Image == data[0].Image,
            x => x.OfferId == data[1].OfferId && x.OfferName == data[1].ServiceName && x.CompanySubscriptionStatuses.SequenceEqual(data[1].CompanySubscriptionStatuses) && x.Image == data[1].Image,
            x => x.OfferId == data[2].OfferId && x.OfferName == data[2].ServiceName && x.CompanySubscriptionStatuses.SequenceEqual(data[2].CompanySubscriptionStatuses) && x.Image == data[2].Image,
            x => x.OfferId == data[3].OfferId && x.OfferName == data[3].ServiceName && x.CompanySubscriptionStatuses.SequenceEqual(data[3].CompanySubscriptionStatuses) && x.Image == data[3].Image,
            x => x.OfferId == data[4].OfferId && x.OfferName == data[4].ServiceName && x.CompanySubscriptionStatuses.SequenceEqual(data[4].CompanySubscriptionStatuses) && x.Image == data[4].Image
        );
        A.CallTo(() => _offerSubscriptionsRepository.GetOwnCompanyProvidedOfferSubscriptionStatusesUntrackedAsync(IamUserId, OfferTypeId.SERVICE, null, OfferSubscriptionStatusId.ACTIVE, offerId))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetCompanyProvidedServiceSubscriptionStatusesForUserAsync_EmptyImage_ReturnsExpected()
    {
        // Arrange
        var offerId = Guid.NewGuid();
        var data = new[] {
            _fixture.Build<OfferCompanySubscriptionStatusData>().With(x => x.Image, Guid.Empty).Create(),
            _fixture.Build<OfferCompanySubscriptionStatusData>().With(x => x.Image, Guid.NewGuid()).Create()
        };
        A.CallTo(() => _offerSubscriptionsRepository.GetOwnCompanyProvidedOfferSubscriptionStatusesUntrackedAsync(IamUserId, OfferTypeId.SERVICE, null, OfferSubscriptionStatusId.ACTIVE, A<Guid?>._))
            .Returns((skip, take) => Task.FromResult(new Pagination.Source<OfferCompanySubscriptionStatusData>(data.Length, data.Skip(skip).Take(take)))!);

        var serviceSettings = new ServiceSettings
        {
            ApplicationsMaxPageSize = 15
        };
        var sut = new ServiceBusinessLogic(_portalRepositories, null!, null!, null!, null!, Options.Create(serviceSettings));

        // Act
        var result = await sut.GetCompanyProvidedServiceSubscriptionStatusesForUserAsync(0, 10, IamUserId, null, null, offerId).ConfigureAwait(false);

        // Assert
        result.Meta.NumberOfElements.Should().Be(2);
        result.Content.Should().HaveCount(2).And.Satisfy(
            x => x.OfferId == data[0].OfferId && x.OfferName == data[0].ServiceName && x.CompanySubscriptionStatuses.SequenceEqual(data[0].CompanySubscriptionStatuses) && x.Image == null,
            x => x.OfferId == data[1].OfferId && x.OfferName == data[1].ServiceName && x.CompanySubscriptionStatuses.SequenceEqual(data[1].CompanySubscriptionStatuses) && x.Image == data[1].Image
        );
        A.CallTo(() => _offerSubscriptionsRepository.GetOwnCompanyProvidedOfferSubscriptionStatusesUntrackedAsync(IamUserId, OfferTypeId.SERVICE, null, OfferSubscriptionStatusId.ACTIVE, offerId))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetCompanyProvidedServiceSubscriptionStatusesForUserAsync_QueryResultNull_ReturnsExpected()
    {
        // Arrange
        var offerId = Guid.NewGuid();
        var data = new[] {
            _fixture.Build<OfferCompanySubscriptionStatusData>().With(x => x.Image, Guid.Empty).Create(),
            _fixture.Build<OfferCompanySubscriptionStatusData>().With(x => x.Image, Guid.NewGuid()).Create()
        };
        A.CallTo(() => _offerSubscriptionsRepository.GetOwnCompanyProvidedOfferSubscriptionStatusesUntrackedAsync(IamUserId, OfferTypeId.SERVICE, null, OfferSubscriptionStatusId.ACTIVE, A<Guid?>._))
            .Returns((skip, take) => Task.FromResult((Pagination.Source<OfferCompanySubscriptionStatusData>?)null));

        var serviceSettings = new ServiceSettings
        {
            ApplicationsMaxPageSize = 15
        };
        var sut = new ServiceBusinessLogic(_portalRepositories, null!, null!, null!, null!, Options.Create(serviceSettings));

        // Act
        var result = await sut.GetCompanyProvidedServiceSubscriptionStatusesForUserAsync(0, 10, IamUserId, null, null, offerId).ConfigureAwait(false);

        // Assert
        result.Meta.NumberOfElements.Should().Be(0);
        result.Content.Should().BeEmpty();
        A.CallTo(() => _offerSubscriptionsRepository.GetOwnCompanyProvidedOfferSubscriptionStatusesUntrackedAsync(IamUserId, OfferTypeId.SERVICE, null, OfferSubscriptionStatusId.ACTIVE, offerId))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region Get Service Detail Data

    [Fact]
    public async Task GetServiceDetailsAsync_WithExistingServiceAndLanguageCode_ReturnsServiceDetailData()
    {
        // Arrange
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        var result = await sut.GetServiceDetailsAsync(_existingServiceId, "en", IamUserId);

        // Assert
        result.Should().BeOfType<ServiceDetailResponse>();
        result.Id.Should().Be(_existingServiceId);
        result.Documents.Should().ContainSingle().Which.Should().Match<KeyValuePair<DocumentTypeId, IEnumerable<DocumentData>>>(
            x => x.Key == DocumentTypeId.ADDITIONAL_DETAILS && x.Value.Count() == 1 && x.Value.Any(y => y.DocumentName == "testDocument"));
        result.TechnicalUserProfile.Should().ContainSingle().Which.Should().Match<KeyValuePair<Guid, IEnumerable<string>>>(
            x => x.Value.SequenceEqual(new[] { "role1", "role2" }));
    }

    [Fact]
    public async Task GetServiceDetailsAsync_WithoutExistingService_ThrowsException()
    {
        // Arrange
        var notExistingServiceId = Guid.NewGuid();
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        async Task Action() => await sut.GetServiceDetailsAsync(notExistingServiceId, "en", IamUserId);

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
        var sut = new ServiceBusinessLogic(null!, offerService, null!, null!, null!, Options.Create(new ServiceSettings()));

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
        var result = await sut.GetSubscriptionDetailAsync(_validSubscriptionId, IamUserId).ConfigureAwait(false);

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
        async Task Action() => await sut.GetSubscriptionDetailAsync(notExistingId, IamUserId).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Action);
        ex.Message.Should().Be($"Subscription {notExistingId} does not exist");
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
            .Returns(data);
        var sut = new ServiceBusinessLogic(null!, offerService, null!, null!, null!, Options.Create(new ServiceSettings()));

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
        var sut = new ServiceBusinessLogic(null!, offerService, null!, null!, null!, Options.Create(new ServiceSettings()));

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
        var offerSetupService = A.Fake<IOfferSetupService>();
        var userRoleData = new[]
        {
            "Sales Manager",
            "IT Manager"
        };
        var responseData = new OfferAutoSetupResponseData(new TechnicalUserInfoData(Guid.NewGuid(), userRoleData, "abcSecret", "sa1"), new ClientInfoData(_fixture.Create<string>(), "http://www.google.com"));
        A.CallTo(() => offerSetupService.AutoSetupOfferAsync(A<OfferAutoSetupData>._, A<IDictionary<string, IEnumerable<string>>>._, A<string>._, A<OfferTypeId>._, A<string>._, A<IDictionary<string, IEnumerable<string>>>._))
            .Returns(responseData);
        var data = new OfferAutoSetupData(Guid.NewGuid(), "https://www.offer.com");
        var settings = _fixture.Create<ServiceSettings>();
        var sut = new ServiceBusinessLogic(null!, null!, null!, null!, offerSetupService, Options.Create(settings));

        // Act
        var result = await sut.AutoSetupServiceAsync(data, IamUserId).ConfigureAwait(false);

        // Assert
        result.Should().Be(responseData);
        A.CallTo(() => offerSetupService.AutoSetupOfferAsync(data, settings.ITAdminRoles, IamUserId, OfferTypeId.SERVICE, settings.UserManagementAddress, settings.ServiceManagerRoles))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region Start Auto setup

    [Fact]
    public async Task StartAutoSetupAsync_ReturnsExcepted()
    {
        // Arrange
        var offerSetupService = A.Fake<IOfferSetupService>();
        _fixture.Inject(offerSetupService);
        var data = new OfferAutoSetupData(Guid.NewGuid(), "https://www.offer.com");
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        await sut.StartAutoSetupAsync(data, IamUserId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => offerSetupService.StartAutoSetupAsync(A<OfferAutoSetupData>._, A<string>._, OfferTypeId.SERVICE)).MustHaveHappenedOnceExactly();
    }

    #endregion

    #region SubmitServiceAsync

    [Fact]
    public async Task SubmitServiceAsync_CallsOfferService()
    {
        // Arrange
        var sut = new ServiceBusinessLogic(null!, _offerService, null!, null!, null!, _options);

        // Act
        await sut.SubmitServiceAsync(_existingServiceId, IamUserId).ConfigureAwait(false);

        // Assert
        A.CallTo(() =>
                _offerService.SubmitServiceAsync(
                    _existingServiceId,
                    IamUserId,
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
        var sut = new ServiceBusinessLogic(null!, _offerService, null!, null!, null!, Options.Create(settings));

        // Act
        await sut.DeclineServiceRequestAsync(_existingServiceId, IamUserId, data).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _offerService.DeclineOfferAsync(_existingServiceId, IamUserId, data,
            OfferTypeId.SERVICE, NotificationTypeId.SERVICE_RELEASE_REJECTION,
            A<IDictionary<string, IEnumerable<string>>>._,
            A<string>._,
            A<IEnumerable<NotificationTypeId>>._,
            A<IDictionary<string, IEnumerable<string>>>._)).MustHaveHappenedOnceExactly();
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
        var sut = new ServiceBusinessLogic(_portalRepositories, _offerService, _offerDocumentService, null!, null!, Options.Create(settings));

        // Act
        await sut.CreateServiceDocumentAsync(serviceId, DocumentTypeId.ADDITIONAL_DETAILS, file, IamUserId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _offerDocumentService.UploadDocumentAsync(serviceId, DocumentTypeId.ADDITIONAL_DETAILS, file, IamUserId, OfferTypeId.SERVICE, settings.UploadServiceDocumentTypeIds, CancellationToken.None)).MustHaveHappenedOnceExactly();
    }

    #endregion

    #region GetServiceDocumentContentAsync

    [Fact]
    public async Task GetServiceDocumentContentAsync_ReturnsExpectedCalls()
    {
        // Arrange
        var serviceId = _fixture.Create<Guid>();
        var documentId = _fixture.Create<Guid>();
        var settings = new ServiceSettings()
        {
            ServiceImageDocumentTypeIds = new[] {
                DocumentTypeId.ADDITIONAL_DETAILS,
                DocumentTypeId.CONFORMITY_APPROVAL_SERVICES,
                DocumentTypeId.SERVICE_LEADIMAGE
                }
        };
        var sut = new ServiceBusinessLogic(_portalRepositories, _offerService, null!, null!, null!, Options.Create(settings));

        // Act
        await sut.GetServiceDocumentContentAsync(serviceId, documentId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _offerService.GetOfferDocumentContentAsync(serviceId, documentId, settings.ServiceImageDocumentTypeIds, OfferTypeId.SERVICE, CancellationToken.None)).MustHaveHappenedOnceExactly();
    }
    #endregion

    #region GetCompanyProvidedServiceStatusDataAsync

    [Theory]
    [InlineData(ServiceStatusIdFilter.Active, new[] { OfferStatusId.ACTIVE })]
    [InlineData(ServiceStatusIdFilter.Inactive, new[] { OfferStatusId.INACTIVE })]
    [InlineData(ServiceStatusIdFilter.InReview, new[] { OfferStatusId.IN_REVIEW })]
    [InlineData(ServiceStatusIdFilter.WIP, new[] { OfferStatusId.CREATED })]
    [InlineData((ServiceStatusIdFilter)default, new[] { OfferStatusId.CREATED, OfferStatusId.IN_REVIEW, OfferStatusId.ACTIVE, OfferStatusId.INACTIVE })]
    public async Task GetCompanyProvidedServiceStatusDataAsync_InActiveRequest(ServiceStatusIdFilter serviceStatusIdFilter, IEnumerable<OfferStatusId> offerStatusIds)
    {
        // Arrange
        var serviceDetailData = _fixture.CreateMany<AllOfferStatusData>(10).ToImmutableArray();
        var paginationResult = (int skip, int take) => Task.FromResult(new Pagination.Source<AllOfferStatusData>(serviceDetailData.Length, serviceDetailData.Skip(skip).Take(take)));
        var user = _fixture.Create<string>();
        var sorting = _fixture.Create<OfferSorting>();
        var name = _fixture.Create<string>();

        A.CallTo(() => _offerRepository.GetCompanyProvidedServiceStatusDataAsync(A<IEnumerable<OfferStatusId>>._, A<OfferTypeId>._, A<string>._, A<OfferSorting>._, A<string>._))
            .Returns(paginationResult);

        A.CallTo(() => _portalRepositories.GetInstance<IOfferRepository>()).Returns(_offerRepository);
        var sut = _fixture.Create<ServiceBusinessLogic>();

        // Act
        var result = await sut.GetCompanyProvidedServiceStatusDataAsync(2, 3, user, sorting, name, serviceStatusIdFilter).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _offerRepository.GetCompanyProvidedServiceStatusDataAsync(A<IEnumerable<OfferStatusId>>
            .That.IsSameSequenceAs(offerStatusIds), OfferTypeId.SERVICE, user, sorting, name)).MustHaveHappenedOnceExactly();
        result.Content.Should().HaveCount(3)
            .And.ContainInOrder(serviceDetailData.Skip(6).Take(3));
    }

    #endregion

    #region ApproveServiceRequestAsync

    [Fact]
    public async Task ApproveServiceRequestAsync_WithValid_CallsExpected()
    {
        // Arrange
        var appId = Guid.NewGuid();
        var sut = new ServiceBusinessLogic(_portalRepositories, _offerService, null!, null!, null!, Options.Create(new ServiceSettings()));

        // Act
        await sut.ApproveServiceRequestAsync(appId, IamUserId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _offerService.ApproveOfferRequestAsync(appId, IamUserId, OfferTypeId.SERVICE,
            A<IEnumerable<NotificationTypeId>>._,
            A<IDictionary<string, IEnumerable<string>>>._,
            A<IEnumerable<NotificationTypeId>>._,
            A<IDictionary<string, IEnumerable<string>>>._)).MustHaveHappenedOnceExactly();
    }

    #endregion

    #region GetSubscriptionDetailForProvider

    [Fact]
    public async Task GetSubscriptionDetailForProvider_WithNotMatchingUserRoles_ThrowsException()
    {
        // Arrange
        var offerId = _fixture.Create<Guid>();
        var subscriptionId = _fixture.Create<Guid>();
        var data = _fixture.Create<ProviderSubscriptionDetailData>();
        var settings = new ServiceSettings
        {
            CompanyAdminRoles = new Dictionary<string, IEnumerable<string>>
            {
                {"ClientTest", new[] {"Test"}}
            }
        };
        A.CallTo(() => _offerService.GetSubscriptionDetailsForProviderAsync(offerId, subscriptionId, IamUserId, OfferTypeId.SERVICE, A<IDictionary<string, IEnumerable<string>>>._))
            .Returns(data);
        var sut = new ServiceBusinessLogic(null!, _offerService, null!, null!, null!, Options.Create(settings));

        // Act
        var result = await sut.GetSubscriptionDetailForProvider(offerId, subscriptionId, IamUserId).ConfigureAwait(false);

        // Assert
        result.Should().Be(data);
    }

    #endregion

    #region GetSubscriptionDetailForSubscriber

    [Fact]
    public async Task GetSubscriptionDetailForSubscriber_WithNotMatchingUserRoles_ThrowsException()
    {
        // Arrange
        var offerId = _fixture.Create<Guid>();
        var subscriptionId = _fixture.Create<Guid>();
        var data = _fixture.Create<SubscriberSubscriptionDetailData>();
        var settings = new ServiceSettings
        {
            CompanyAdminRoles = new Dictionary<string, IEnumerable<string>>
            {
                {"ClientTest", new[] {"Test"}}
            }
        };
        A.CallTo(() => _offerService.GetSubscriptionDetailsForSubscriberAsync(offerId, subscriptionId, IamUserId, OfferTypeId.SERVICE, A<IDictionary<string, IEnumerable<string>>>._))
            .Returns(data);
        var sut = new ServiceBusinessLogic(null!, _offerService, null!, null!, null!, Options.Create(settings));

        // Act
        var result = await sut.GetSubscriptionDetailForSubscriber(offerId, subscriptionId, IamUserId).ConfigureAwait(false);

        // Assert
        result.Should().Be(data);
    }

    #endregion

    #region GetCompanySubscribedServiceSubscriptionStatusesForUserAsync

    [Fact]
    public async Task GetCompanySubscribedServiceSubscriptionStatusesForUserAsync_ReturnsExpected()
    {
        // Arrange
        var iamUserId = _fixture.Create<Guid>().ToString();
        var data = _fixture.CreateMany<OfferSubscriptionStatusDetailData>(5).ToImmutableArray();
        var paginationResponse = new Pagination.Response<OfferSubscriptionStatusDetailData>(new Pagination.Metadata(data.Count(), 1, 0, data.Count()), data);
        A.CallTo(() => _offerService.GetCompanySubscribedOfferSubscriptionStatusesForUserAsync(A<int>._, A<int>._, A<string>._, A<OfferTypeId>._, A<DocumentTypeId>._))
            .Returns(paginationResponse);

        var sut = new ServiceBusinessLogic(null!, _offerService, null!, null!, null!, Options.Create(new ServiceSettings()));

        // Act
        var result = await sut.GetCompanySubscribedServiceSubscriptionStatusesForUserAsync(0, 10, iamUserId).ConfigureAwait(false);

        // Assert
        result.Meta.NumberOfElements.Should().Be(5);
        result.Content.Should().HaveCount(5);
        A.CallTo(() => _offerService.GetCompanySubscribedOfferSubscriptionStatusesForUserAsync(0, 10, iamUserId, OfferTypeId.SERVICE, DocumentTypeId.SERVICE_LEADIMAGE))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region Setup

    private void SetupPagination(int count = 5)
    {
        var serviceDetailData = _fixture.CreateMany<ServiceOverviewData>(count);
        var paginationResult = (int skip, int take) => Task.FromResult(new Pagination.Source<ServiceOverviewData>(serviceDetailData.Count(), serviceDetailData.Skip(skip).Take(take)));

        A.CallTo(() => _offerRepository.GetActiveServicesPaginationSource(A<ServiceOverviewSorting?>._, A<ServiceTypeId?>._, A<string>._))
            .Returns(paginationResult);

        A.CallTo(() => _portalRepositories.GetInstance<IOfferRepository>()).Returns(_offerRepository);
    }

    private void SetupRepositories()
    {
        var serviceDetail = _fixture.Build<ServiceDetailData>()
            .With(x => x.Id, _existingServiceId)
            .Create();
        A.CallTo(() => _userRepository.GetCompanyUserWithIamUserCheckAndCompanyName(IamUserId, _companyUser.Id))
            .Returns(new[] { (_companyUser.Id, true, "COMPANYBPN", _companyUserCompanyId), (_companyUser.Id, false, "OTHERCOMPANYBPN", _companyUserCompanyId) }.ToAsyncEnumerable());
        A.CallTo(() => _userRepository.GetCompanyUserWithIamUserCheckAndCompanyName(IamUserId, A<Guid>.That.Not.Matches(x => x == _companyUser.Id)))
            .Returns(new[] { (_companyUser.Id, true, "COMPANYBPN", _companyUserCompanyId) }.ToAsyncEnumerable());
        A.CallTo(() => _userRepository.GetCompanyUserWithIamUserCheckAndCompanyName(A<string>.That.Not.Matches(x => x == IamUserId), _companyUser.Id))
            .Returns(new[] { (_companyUser.Id, false, "OTHERCOMPANYBPN", _companyUserCompanyId) }.ToAsyncEnumerable());
        A.CallTo(() => _userRepository.GetCompanyUserWithIamUserCheckAndCompanyName(A<string>.That.Not.Matches(x => x == IamUserId), A<Guid>.That.Not.Matches(x => x == _companyUser.Id)))
            .Returns(Enumerable.Empty<(Guid CompanyUserId, bool IsIamUser, string CompanyUserName, Guid CompanyId)>().ToAsyncEnumerable());

        A.CallTo(() => _userRepository.GetOwnCompanyAndCompanyUserId(IamUserId))
            .Returns((_companyUser.Id, _companyUser.CompanyId));
        A.CallTo(() => _userRepository.GetOwnCompanyAndCompanyUserId(_notAssignedCompanyIdUser))
            .Returns((_companyUser.Id, Guid.Empty));
        A.CallTo(() => _userRepository.GetOwnCompanyAndCompanyUserId(A<string>.That.Not.Matches(x => x == IamUserId || x == _notAssignedCompanyIdUser)))
            .Returns((Guid.Empty, _companyUser.CompanyId));

        A.CallTo(() => _userRepository.GetOwnCompanyInformationWithCompanyUserIdAndEmailAsync(IamUserId))
            .Returns((new CompanyInformationData(_companyUser.CompanyId, "The Company", "DE", "BPN00000001"), _companyUser.Id, "test@mail.de"));
        A.CallTo(() => _userRepository.GetOwnCompanyInformationWithCompanyUserIdAndEmailAsync(_notAssignedCompanyIdUser))
            .Returns((new CompanyInformationData(Guid.Empty, "The Company", "DE", "BPN00000001"), _companyUser.Id, "test@mail.de"));
        A.CallTo(() => _userRepository.GetOwnCompanyInformationWithCompanyUserIdAndEmailAsync(A<string>.That.Not.Matches(x => x == IamUserId || x == _notAssignedCompanyIdUser)))
            .Returns((new CompanyInformationData(_companyUser.CompanyId, "The Company", "DE", "BPN00000001"), Guid.Empty, "test@mail.de"));

        A.CallTo(() => _offerRepository.GetServiceDetailByIdUntrackedAsync(_existingServiceId, A<string>.That.Matches(x => x == "en"), A<string>._))
            .Returns(serviceDetail with
            {
                OfferSubscriptionDetailData = new[] { new OfferSubscriptionStateDetailData(Guid.NewGuid(), OfferSubscriptionStatusId.ACTIVE) },
                Documents = new[] { new DocumentTypeData(DocumentTypeId.ADDITIONAL_DETAILS, Guid.NewGuid(), "testDocument") },
                TechnicalUserProfile = new[] { new TechnicalUserRoleData(Guid.NewGuid(), new[] { "role1", "role2" }) }
            });
        A.CallTo(() => _offerRepository.GetServiceDetailByIdUntrackedAsync(A<Guid>.That.Not.Matches(x => x == _existingServiceId), A<string>._, A<string>._))
            .Returns((ServiceDetailData?)null);

        A.CallTo(() => _offerRepository.GetOfferProviderDetailsAsync(A<Guid>.That.Matches(x => x == _existingServiceId), A<OfferTypeId>._))
            .Returns(new OfferProviderDetailsData("Test Service", "Test Company", "provider@mail.de", new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"), "https://www.testurl.com", false));
        A.CallTo(() => _offerRepository.GetOfferProviderDetailsAsync(A<Guid>.That.Matches(x => x == _existingServiceWithFailingAutoSetupId), A<OfferTypeId>._))
            .Returns(new OfferProviderDetailsData("Test Service", "Test Company", "provider@mail.de", new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"), "https://www.fail.com", false));
        A.CallTo(() => _offerRepository.GetOfferProviderDetailsAsync(A<Guid>.That.Not.Matches(x => x == _existingServiceId || x == _existingServiceWithFailingAutoSetupId), A<OfferTypeId>._))
            .Returns((OfferProviderDetailsData?)null);

        var agreementData = _fixture.CreateMany<AgreementData>(1);
        A.CallTo(() => _agreementRepository.GetOfferAgreementDataForOfferId(A<Guid>.That.Matches(x => x == _existingServiceId), A<OfferTypeId>._))
            .Returns(agreementData.ToAsyncEnumerable());
        A.CallTo(() => _agreementRepository.GetOfferAgreementDataForOfferId(A<Guid>.That.Not.Matches(x => x == _existingServiceId), A<OfferTypeId>._))
            .Returns(Enumerable.Empty<AgreementData>().ToAsyncEnumerable());
        A.CallTo(() => _agreementRepository.CheckAgreementExistsForSubscriptionAsync(A<Guid>.That.Matches(x => x == _existingAgreementId), A<Guid>._, A<OfferTypeId>.That.Matches(x => x == OfferTypeId.SERVICE)))
            .Returns(true);
        A.CallTo(() => _agreementRepository.CheckAgreementExistsForSubscriptionAsync(A<Guid>.That.Not.Matches(x => x == _existingAgreementId), A<Guid>._, A<OfferTypeId>._))
            .Returns(false);
        A.CallTo(() => _agreementRepository.CheckAgreementExistsForSubscriptionAsync(A<Guid>._, A<Guid>._, A<OfferTypeId>.That.Not.Matches(x => x == OfferTypeId.SERVICE)))
            .Returns(false);

        var offerSubscription = _fixture.Create<OfferSubscription>();
        A.CallTo(() => _offerSubscriptionsRepository.GetSubscriptionDetailDataForOwnUserAsync(
                A<Guid>.That.Matches(x => x == _validSubscriptionId),
                A<string>.That.Matches(x => x == IamUserId),
                A<OfferTypeId>._))
            .Returns(new SubscriptionDetailData(_existingServiceId, "Super Service", OfferSubscriptionStatusId.ACTIVE));
        A.CallTo(() => _offerSubscriptionsRepository.GetSubscriptionDetailDataForOwnUserAsync(
                A<Guid>.That.Not.Matches(x => x == _validSubscriptionId),
                A<string>.That.Matches(x => x == IamUserId),
                A<OfferTypeId>._))
            .Returns((SubscriptionDetailData?)null);
        A.CallTo(() => _offerSubscriptionsRepository.GetCompanyIdWithAssignedOfferForCompanyUserAndSubscriptionAsync(
                A<Guid>.That.Matches(x => x == _existingServiceId), A<string>.That.Matches(x => x == IamUserId), A<OfferTypeId>._))
            .Returns((_companyUser.CompanyId, offerSubscription, _companyUser.Id));
        A.CallTo(() => _offerSubscriptionsRepository.GetCompanyIdWithAssignedOfferForCompanyUserAndSubscriptionAsync(
                A<Guid>.That.Not.Matches(x => x == _existingServiceId), A<string>.That.Matches(x => x == IamUserId),
                A<OfferTypeId>._))
            .Returns((_companyUser.CompanyId, (OfferSubscription?)null, _companyUser.Id));
        A.CallTo(() => _offerSubscriptionsRepository.GetCompanyIdWithAssignedOfferForCompanyUserAndSubscriptionAsync(
                A<Guid>.That.Matches(x => x == _existingServiceId), A<string>.That.Not.Matches(x => x == IamUserId),
                A<OfferTypeId>._))
            .Returns(((Guid companyId, OfferSubscription? offerSubscription, Guid companyUserId))default);

        A.CallTo(() => _consentRepository.GetConsentDetailData(A<Guid>.That.Matches(x => x == _validConsentId), A<OfferTypeId>.That.Matches(x => x == OfferTypeId.SERVICE)))
            .Returns(new ConsentDetailData(_validConsentId, "The Company", _companyUser.Id, ConsentStatusId.ACTIVE, "Agreed"));
        A.CallTo(() => _consentRepository.GetConsentDetailData(A<Guid>._, A<OfferTypeId>.That.Not.Matches(x => x == OfferTypeId.SERVICE)))
            .Returns((ConsentDetailData?)null);
        A.CallTo(() => _consentRepository.GetConsentDetailData(A<Guid>.That.Not.Matches(x => x == _validConsentId), A<OfferTypeId>._))
            .Returns((ConsentDetailData?)null);

        var userRoleData = _fixture.CreateMany<UserRoleData>(3);
        A.CallTo(
                () => _userRolesRepository.GetUserRoleDataUntrackedAsync(A<IDictionary<string, IEnumerable<string>>>._))
            .Returns(userRoleData.ToAsyncEnumerable());

        A.CallTo(() => _userRolesRepository.GetUserRolesForOfferIdAsync(A<Guid>.That.Matches(x => x == _existingServiceId)))
            .Returns(new[] { "Buyer", "Supplier" }.ToAsyncEnumerable());
        A.CallTo(() => _portalRepositories.GetInstance<IAgreementRepository>()).Returns(_agreementRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IConsentRepository>()).Returns(_consentRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IOfferRepository>()).Returns(_offerRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IOfferSubscriptionsRepository>()).Returns(_offerSubscriptionsRepository);
        A.CallTo(() => _portalRepositories.GetInstance<INotificationRepository>()).Returns(_notificationRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRolesRepository>()).Returns(_userRolesRepository);
        _fixture.Inject(_portalRepositories);
    }

    #endregion
}
