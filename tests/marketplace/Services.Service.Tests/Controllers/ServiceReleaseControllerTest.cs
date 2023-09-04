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
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;
using Xunit;

namespace Org.Eclipse.TractusX.Portal.Backend.Services.Service.Tests.Controllers;

public class ServiceReleaseControllerTest
{
    private const string AccessToken = "THISISTHEACCESSTOKEN";
    private readonly IdentityData _identity = new("4C1A6851-D4E7-4E10-A011-3732CD045E8A", Guid.NewGuid(), IdentityTypeId.COMPANY_USER, Guid.NewGuid());
    private static readonly Guid ServiceId = new("4C1A6851-D4E7-4E10-A011-3732CD045453");
    private readonly IFixture _fixture;
    private readonly IServiceReleaseBusinessLogic _logic;
    private readonly ServiceReleaseController _controller;
    public ServiceReleaseControllerTest()
    {
        _fixture = new Fixture();
        _logic = A.Fake<IServiceReleaseBusinessLogic>();
        this._controller = new ServiceReleaseController(_logic);
        _controller.AddControllerContextWithClaimAndBearer(AccessToken, _identity);
    }

    [Fact]
    public async Task GetServiceAgreementData_ReturnsExpectedResult()
    {
        //Arrange
        var data = _fixture.CreateMany<AgreementDocumentData>(5).ToAsyncEnumerable();
        A.CallTo(() => _logic.GetServiceAgreementDataAsync())
            .Returns(data);

        //Act
        var result = await this._controller.GetServiceAgreementDataAsync().ToListAsync().ConfigureAwait(false);

        // Assert 
        A.CallTo(() => _logic.GetServiceAgreementDataAsync()).MustHaveHappenedOnceExactly();
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetServiceDetailsByIdAsync_ReturnsExpectedResult()
    {
        //Arrange
        var data = _fixture.Create<ServiceData>();
        var serviceId = _fixture.Create<Guid>();
        A.CallTo(() => _logic.GetServiceDetailsByIdAsync(serviceId))
            .Returns(data);

        //Act
        var result = await this._controller.GetServiceDetailsByIdAsync(serviceId).ConfigureAwait(false);

        // Assert 
        A.CallTo(() => _logic.GetServiceDetailsByIdAsync(serviceId)).MustHaveHappenedOnceExactly();
        result.Should().BeOfType<ServiceData>();
    }

    [Fact]
    public async Task GetServiceTypeData_ReturnsExpectedResult()
    {
        //Arrange
        var data = _fixture.CreateMany<ServiceTypeData>(5).ToAsyncEnumerable();
        A.CallTo(() => _logic.GetServiceTypeDataAsync())
            .Returns(data);

        //Act
        var result = await this._controller.GetServiceTypeDataAsync().ToListAsync().ConfigureAwait(false);

        // Assert 
        A.CallTo(() => _logic.GetServiceTypeDataAsync()).MustHaveHappenedOnceExactly();
        result.Should().AllBeOfType<ServiceTypeData>();
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetServiceAgreementConsentByIdAsync_ReturnsExpectedResult()
    {
        //Arrange
        var serviceId = Guid.NewGuid();
        var data = _fixture.Create<OfferAgreementConsent>();
        A.CallTo(() => _logic.GetServiceAgreementConsentAsync(A<Guid>._))
            .Returns(data);

        //Act
        var result = await this._controller.GetServiceAgreementConsentByIdAsync(serviceId).ConfigureAwait(false);

        // Assert 
        result.Should().Be(data);
        A.CallTo(() => _logic.GetServiceAgreementConsentAsync(serviceId))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetServiceDetailsForStatusAsync_ReturnsExpectedResult()
    {
        //Arrange
        var serviceId = Guid.NewGuid();
        var data = _fixture.Create<ServiceProviderResponse>();
        A.CallTo(() => _logic.GetServiceDetailsForStatusAsync(A<Guid>._))
            .Returns(data);

        //Act
        var result = await this._controller.GetServiceDetailsForStatusAsync(serviceId).ConfigureAwait(false);

        // Assert 
        result.Should().Be(data);
        A.CallTo(() => _logic.GetServiceDetailsForStatusAsync(serviceId))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task SubmitOfferConsentToAgreementsAsync_ReturnsExpectedId()
    {
        //Arrange
        var serviceId = Guid.NewGuid();
        var agreementId = Guid.NewGuid();
        var consentStatusData = new ConsentStatusData(Guid.NewGuid(), ConsentStatusId.ACTIVE);
        var offerAgreementConsentData = new OfferAgreementConsent(new[] { new AgreementConsentStatus(agreementId, ConsentStatusId.ACTIVE) });
        A.CallTo(() => _logic.SubmitOfferConsentAsync(A<Guid>._, A<OfferAgreementConsent>._))
            .Returns(Enumerable.Repeat(consentStatusData, 1));

        //Act
        var result = await this._controller.SubmitOfferConsentToAgreementsAsync(serviceId, offerAgreementConsentData).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.SubmitOfferConsentAsync(serviceId, offerAgreementConsentData)).MustHaveHappenedOnceExactly();
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllInReviewStatusServiceAsync_ReturnsExpectedCount()
    {
        //Arrange
        var paginationResponse = new Pagination.Response<InReviewServiceData>(new Pagination.Metadata(15, 1, 1, 15), _fixture.CreateMany<InReviewServiceData>(5));
        A.CallTo(() => _logic.GetAllInReviewStatusServiceAsync(A<int>._, A<int>._, A<OfferSorting?>._, A<string>._, A<string>._, A<ServiceReleaseStatusIdFilter?>._))
            .Returns(paginationResponse);

        //Act
        var result = await this._controller.GetAllInReviewStatusServiceAsync().ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.GetAllInReviewStatusServiceAsync(0, 15, null, null, null, null)).MustHaveHappenedOnceExactly();
        result.Content.Should().HaveCount(5);
    }

    [Fact]
    public async Task DeleteServiceDocumentsAsync_ReturnsExpectedCount()
    {
        //Arrange
        var documentId = Guid.NewGuid();

        //Act
        var result = await this._controller.DeleteServiceDocumentsAsync(documentId).ConfigureAwait(false);

        // Assert 
        Assert.IsType<NoContentResult>(result);
        A.CallTo(() => _logic.DeleteServiceDocumentsAsync(documentId))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CreateServiceOffering_ReturnsExpectedId()
    {
        //Arrange
        var id = new Guid("d90995fe-1241-4b8d-9f5c-f3909acc6383");
        var serviceOfferingData = _fixture.Create<ServiceOfferingData>();
        A.CallTo(() => _logic.CreateServiceOfferingAsync(A<ServiceOfferingData>._))
            .Returns(id);

        //Act
        var result = await this._controller.CreateServiceOffering(serviceOfferingData).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.CreateServiceOfferingAsync(serviceOfferingData)).MustHaveHappenedOnceExactly();
        Assert.IsType<CreatedAtRouteResult>(result);
        result.Value.Should().Be(id);
    }

    [Fact]
    public async Task UpdateService_ReturnsExpected()
    {
        //Arrange
        var serviceId = _fixture.Create<Guid>();
        var data = _fixture.Create<ServiceUpdateRequestData>();
        A.CallTo(() => _logic.UpdateServiceAsync(A<Guid>._, A<ServiceUpdateRequestData>._, _identity.CompanyId))
            .Returns(Task.CompletedTask);

        //Act
        var result = await this._controller.UpdateService(serviceId, data).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.UpdateServiceAsync(serviceId, data, _identity.CompanyId)).MustHaveHappenedOnceExactly();
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task SubmitService_ReturnsExpectedCount()
    {
        //Act
        await this._controller.SubmitService(ServiceId).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.SubmitServiceAsync(ServiceId)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ApproveServiceRequest_ReturnsNoContent()
    {
        //Arrange
        var serviceId = _fixture.Create<Guid>();

        //Act
        var result = await this._controller.ApproveServiceRequest(serviceId).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.ApproveServiceRequestAsync(serviceId)).MustHaveHappenedOnceExactly();
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeclineServiceRequest_ReturnsNoContent()
    {
        //Arrange
        var serviceId = _fixture.Create<Guid>();
        var data = new OfferDeclineRequest("Just a test");

        //Act
        var result = await this._controller.DeclineServiceRequest(serviceId, data).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.DeclineServiceRequestAsync(serviceId, data)).MustHaveHappenedOnceExactly();
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task UpdateServiceDocumentAsync_CallExpected()
    {
        // Arrange
        var serviceId = _fixture.Create<Guid>();
        var file = FormFileHelper.GetFormFile("this is just a test", "superFile.pdf", "application/pdf");

        // Act
        await this._controller.UpdateServiceDocumentAsync(serviceId, DocumentTypeId.ADDITIONAL_DETAILS, file, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _logic.CreateServiceDocumentAsync(serviceId,
            DocumentTypeId.ADDITIONAL_DETAILS, file, A<ValueTuple<Guid, Guid>>.That.Matches(x => x.Item1 == _identity.UserId && x.Item2 == _identity.CompanyId), CancellationToken.None)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetTechnicalUserProfiles_ReturnsExpectedCount()
    {
        //Arrange
        var offerId = Guid.NewGuid();

        var data = _fixture.CreateMany<TechnicalUserProfileInformation>(5);
        A.CallTo(() => _logic.GetTechnicalUserProfilesForOffer(offerId))
            .Returns(data);

        //Act
        var result = await this._controller.GetTechnicalUserProfiles(offerId).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.GetTechnicalUserProfilesForOffer(offerId)).MustHaveHappenedOnceExactly();
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task UpdateTechnicalUserProfiles_ReturnsExpectedCount()
    {
        //Arrange
        var offerId = Guid.NewGuid();
        var data = _fixture.CreateMany<TechnicalUserProfileData>(5);

        //Act
        var result = await this._controller.CreateAndUpdateTechnicalUserProfiles(offerId, data).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.UpdateTechnicalUserProfiles(offerId, A<IEnumerable<TechnicalUserProfileData>>.That.Matches(x => x.Count() == 5))).MustHaveHappenedOnceExactly();
        result.Should().BeOfType<NoContentResult>();
    }
}
