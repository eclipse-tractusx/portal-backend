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
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;
using Org.Eclipse.TractusX.Portal.Backend.Services.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Services.Service.Controllers;
using Org.Eclipse.TractusX.Portal.Backend.Services.Service.ViewModels;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;
using System.Collections.Immutable;
using Xunit;

namespace Org.Eclipse.TractusX.Portal.Backend.Services.Service.Tests.Controllers;

public class ServiceControllerTest
{
    private const string AccessToken = "THISISTHEACCESSTOKEN";
    private static readonly Guid ServiceId = new("4C1A6851-D4E7-4E10-A011-3732CD045453");
    private readonly IdentityData _identity = new("4C1A6851-D4E7-4E10-A011-3732CD045E8A", Guid.NewGuid(), IdentityTypeId.COMPANY_USER, Guid.NewGuid());
    private readonly IFixture _fixture;
    private readonly IServiceBusinessLogic _logic;
    private readonly ServicesController _controller;

    public ServiceControllerTest()
    {
        _fixture = new Fixture();
        _logic = A.Fake<IServiceBusinessLogic>();
        this._controller = new ServicesController(_logic);
        _controller.AddControllerContextWithClaimAndBearer(AccessToken, _identity);
    }

    [Fact]
    public async Task GetAllActiveServicesAsync_ReturnsExpectedId()
    {
        //Arrange
        var paginationResponse = new Pagination.Response<ServiceOverviewData>(new Pagination.Metadata(15, 1, 1, 15), _fixture.CreateMany<ServiceOverviewData>(5));
        A.CallTo(() => _logic.GetAllActiveServicesAsync(0, 15, null, null))
            .Returns(paginationResponse);

        //Act
        var result = await this._controller.GetAllActiveServicesAsync().ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.GetAllActiveServicesAsync(0, 15, null, null)).MustHaveHappenedOnceExactly();
        Assert.IsType<Pagination.Response<ServiceOverviewData>>(result);
        result.Content.Should().HaveCount(5);
    }

    [Fact]
    public async Task AddServiceSubscriptionWithConsent_ReturnsExpectedId()
    {
        //Arrange
        var offerSubscriptionId = Guid.NewGuid();
        var consentData = _fixture.CreateMany<OfferAgreementConsentData>(2);
        A.CallTo(() => _logic.AddServiceSubscription(A<Guid>._, A<IEnumerable<OfferAgreementConsentData>>._))
            .Returns(offerSubscriptionId);

        //Act
        var serviceId = Guid.NewGuid();
        var result = await this._controller.AddServiceSubscription(serviceId, consentData).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.AddServiceSubscription(serviceId, consentData)).MustHaveHappenedOnceExactly();
        Assert.IsType<CreatedAtRouteResult>(result);
        result.Value.Should().Be(offerSubscriptionId);
    }

    [Fact]
    public async Task GetServiceDetails_ReturnsExpectedId()
    {
        //Arrange
        var serviceId = Guid.NewGuid();
        var serviceDetailData = _fixture.Create<ServiceDetailResponse>();
        A.CallTo(() => _logic.GetServiceDetailsAsync(serviceId, A<string>._))
            .Returns(serviceDetailData);

        //Act
        var result = await this._controller.GetServiceDetails(serviceId).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.GetServiceDetailsAsync(serviceId, "en")).MustHaveHappenedOnceExactly();
        Assert.IsType<ServiceDetailResponse>(result);
        result.Should().Be(serviceDetailData);
    }

    [Fact]
    public async Task GetSubscriptionDetail_ReturnsExpectedId()
    {
        //Arrange
        var subscriptionId = Guid.NewGuid();
        var detailData = new SubscriptionDetailData(subscriptionId, "Service", OfferSubscriptionStatusId.ACTIVE);
        A.CallTo(() => _logic.GetSubscriptionDetailAsync(subscriptionId))
            .Returns(detailData);

        //Act
        var result = await this._controller.GetSubscriptionDetail(subscriptionId).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.GetSubscriptionDetailAsync(subscriptionId)).MustHaveHappenedOnceExactly();
        Assert.IsType<SubscriptionDetailData>(result);
        result.Should().Be(detailData);
    }

    [Fact]
    public async Task GetServiceAgreement_ReturnsExpected()
    {
        //Arrange
        var agreementData = _fixture.CreateMany<AgreementData>(5).ToAsyncEnumerable();
        A.CallTo(() => _logic.GetServiceAgreement(A<Guid>._))
            .Returns(agreementData);

        //Act
        var result = await this._controller.GetServiceAgreement(ServiceId).ToListAsync().ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.GetServiceAgreement(ServiceId)).MustHaveHappenedOnceExactly();
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetServiceAgreementConsentDetail_ReturnsExpected()
    {
        //Arrange
        var consentId = Guid.NewGuid();
        var consentDetailData = new ConsentDetailData(consentId, "Test Company", Guid.NewGuid(), ConsentStatusId.ACTIVE, "Aggred");
        A.CallTo(() => _logic.GetServiceConsentDetailDataAsync(A<Guid>._))
            .Returns(consentDetailData);

        //Act
        var result = await this._controller.GetServiceAgreementConsentDetail(consentId).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.GetServiceConsentDetailDataAsync(consentId)).MustHaveHappenedOnceExactly();
        result.CompanyName.Should().Be("Test Company");
    }

    [Fact]
    public async Task AutoSetupService_ReturnsExpected()
    {
        //Arrange
        var offerSubscriptionId = Guid.NewGuid();
        var userRoleData = new List<string>();
        userRoleData.Add("Sales Manager");
        userRoleData.Add("IT Manager");
        var data = new OfferAutoSetupData(offerSubscriptionId, "https://test.de");
        var responseData = new OfferAutoSetupResponseData(
            new TechnicalUserInfoData(Guid.NewGuid(), userRoleData, "abcPW", "sa1"),
            new ClientInfoData(Guid.NewGuid().ToString(), "http://www.google.com")
        );
        A.CallTo(() => _logic.AutoSetupServiceAsync(A<OfferAutoSetupData>._))
            .Returns(responseData);

        //Act
        var result = await this._controller.AutoSetupService(data).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.AutoSetupServiceAsync(data)).MustHaveHappenedOnceExactly();
        Assert.IsType<OfferAutoSetupResponseData>(result);
        result.Should().Be(responseData);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("c714b905-9d2a-4cf3-b9f7-10be4eeddfc8")]
    public async Task GetCompanyProvidedServiceSubscriptionStatusesForCurrentUserAsync_ReturnsExpectedCount(string? offerIdTxt)
    {
        //Arrange
        Guid? offerId = offerIdTxt == null ? null : new Guid(offerIdTxt);
        var data = _fixture.CreateMany<OfferCompanySubscriptionStatusResponse>(5);
        var pagination = new Pagination.Response<OfferCompanySubscriptionStatusResponse>(new Pagination.Metadata(data.Count(), 1, 0, data.Count()), data);
        A.CallTo(() => _logic.GetCompanyProvidedServiceSubscriptionStatusesForUserAsync(A<int>._, A<int>._, A<SubscriptionStatusSorting?>._, A<OfferSubscriptionStatusId?>._, A<Guid?>._))
            .Returns(pagination);

        //Act
        var result = await this._controller.GetCompanyProvidedServiceSubscriptionStatusesForCurrentUserAsync(offerId: offerId).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.GetCompanyProvidedServiceSubscriptionStatusesForUserAsync(0, 15, null, null, offerId)).MustHaveHappenedOnceExactly();
        result.Content.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetServiceDocumentContentAsync_ReturnsExpected()
    {
        //Arrange
        var serviceId = _fixture.Create<Guid>();
        var documentId = _fixture.Create<Guid>();
        var content = _fixture.Create<byte[]>();
        var fileName = _fixture.Create<string>();

        A.CallTo(() => _logic.GetServiceDocumentContentAsync(A<Guid>._, A<Guid>._, A<CancellationToken>._))
            .Returns((content, "image/png", fileName));

        //Act
        var result = await this._controller.GetServiceDocumentContentAsync(serviceId, documentId, CancellationToken.None).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.GetServiceDocumentContentAsync(A<Guid>._, A<Guid>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        result.ContentType.Should().Be("image/png");
        result.FileDownloadName.Should().Be(fileName);
        result.Should().BeOfType<FileContentResult>();
    }

    [Fact]
    public async Task GetCompanyProvidedServiceStatusDataAsync_ReturnsExpectedCount()
    {
        //Arrange
        var data = _fixture.CreateMany<AllOfferStatusData>(5);
        var paginationResponse = new Pagination.Response<AllOfferStatusData>(new Pagination.Metadata(data.Count(), 1, 0, data.Count()), data);
        A.CallTo(() => _logic.GetCompanyProvidedServiceStatusDataAsync(0, 15, null, null, null))
            .Returns(paginationResponse);

        //Act
        var result = await this._controller.GetCompanyProvidedServiceStatusDataAsync().ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.GetCompanyProvidedServiceStatusDataAsync(0, 15, null, null, null)).MustHaveHappenedOnceExactly();
        result.Content.Should().HaveCount(5);
    }

    [Fact]
    public async Task StartAutoSetupProcess_ReturnsExpected()
    {
        //Arrange
        var offerSubscriptionId = Guid.NewGuid();
        var data = new OfferAutoSetupData(offerSubscriptionId, "https://test.de");

        //Act
        var result = await this._controller.StartAutoSetupServiceProcess(data).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.StartAutoSetupAsync(data)).MustHaveHappenedOnceExactly();
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task GetSubscriptionDetailForProvider_ReturnsExpected()
    {
        // Arrange
        var serviceId = _fixture.Create<Guid>();
        var subscriptionId = _fixture.Create<Guid>();
        var data = _fixture.Create<ProviderSubscriptionDetailData>();
        A.CallTo(() => _logic.GetSubscriptionDetailForProvider(serviceId, subscriptionId))
            .Returns(data);

        // Act
        var result = await this._controller.GetSubscriptionDetailForProvider(serviceId, subscriptionId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _logic.GetSubscriptionDetailForProvider(serviceId, subscriptionId)).MustHaveHappenedOnceExactly();
        result.Should().Be(data);
    }

    [Fact]
    public async Task GetSubscriptionDetailForSubscriber_ReturnsExpected()
    {
        // Arrange
        var serviceId = _fixture.Create<Guid>();
        var subscriptionId = _fixture.Create<Guid>();
        var data = _fixture.Create<SubscriberSubscriptionDetailData>();
        A.CallTo(() => _logic.GetSubscriptionDetailForSubscriber(serviceId, subscriptionId))
            .Returns(data);

        // Act
        var result = await this._controller.GetSubscriptionDetailForSubscriber(serviceId, subscriptionId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _logic.GetSubscriptionDetailForSubscriber(serviceId, subscriptionId)).MustHaveHappenedOnceExactly();
        result.Should().Be(data);
    }

    #region GetCompanySubscribedServiceSubscriptionStatusesForCurrentUserAsync

    [Fact]
    public async Task GetCompanySubscribedServiceSubscriptionStatusesForCurrentUserAsync_ReturnsExpectedCount()
    {
        //Arrange
        var data = _fixture.CreateMany<OfferSubscriptionStatusDetailData>(3).ToImmutableArray();
        var pagination = new Pagination.Response<OfferSubscriptionStatusDetailData>(new Pagination.Metadata(data.Count(), 1, 0, data.Count()), data);
        A.CallTo(() => _logic.GetCompanySubscribedServiceSubscriptionStatusesForUserAsync(A<int>._, A<int>._))
            .Returns(pagination);

        //Act
        var result = await this._controller.GetCompanySubscribedServiceSubscriptionStatusesForUserAsync().ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.GetCompanySubscribedServiceSubscriptionStatusesForUserAsync(0, 15)).MustHaveHappenedOnceExactly();
        result.Content.Should().HaveCount(3).And.ContainInOrder(data);
    }

    #endregion

    [Fact]
    public async Task UnsubscribeCompanyServiceSubscription_ReturnsNoContent()
    {
        //Arrange
        var serviceId = _fixture.Create<Guid>();

        //Act
        var result = await this._controller.UnsubscribeCompanyServiceSubscriptionAsync(serviceId).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.UnsubscribeOwnCompanyServiceSubscriptionAsync(serviceId)).MustHaveHappenedOnceExactly();
        Assert.IsType<NoContentResult>(result);
    }
}
