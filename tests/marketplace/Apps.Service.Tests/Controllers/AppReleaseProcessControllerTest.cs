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
using Org.Eclipse.TractusX.Portal.Backend.Apps.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Apps.Service.ViewModels;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;
using Xunit;

namespace Org.Eclipse.TractusX.Portal.Backend.Apps.Service.Controllers.Tests;

public class AppReleaseProcessControllerTest
{
    private static readonly string IamUserId = "4C1A6851-D4E7-4E10-A011-3732CD045E8A";
    private readonly IdentityData _identity = new(IamUserId, Guid.NewGuid(), IdentityTypeId.COMPANY_USER, Guid.NewGuid());
    private readonly IFixture _fixture;
    private readonly AppReleaseProcessController _controller;
    private readonly IAppReleaseBusinessLogic _logic;

    public AppReleaseProcessControllerTest()
    {
        _fixture = new Fixture();
        _logic = A.Fake<IAppReleaseBusinessLogic>();
        this._controller = new AppReleaseProcessController(_logic);
        _controller.AddControllerContextWithClaim(IamUserId, _identity);
    }

    [Fact]
    public async Task UpdateApp_ReturnsNoContent()
    {
        // Arrange
        var appId = new Guid("5cf74ef8-e0b7-4984-a872-474828beb5d2");
        var data = new AppEditableDetail(
            new LocalizedDescription[]
            {
                new("en", "This is a long description", "description")
            },
            "https://test.provider.com",
            null,
            null);

        // Act
        var result = await this._controller.UpdateApp(appId, data).ConfigureAwait(false);

        // Assert
        Assert.IsType<NoContentResult>(result);
        A.CallTo(() => _logic.UpdateAppAsync(appId, data, _identity.CompanyId)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task UpdateAppDocument_ReturnsExpectedResult()
    {
        //Arrange
        var appId = new Guid("5cf74ef8-e0b7-4984-a872-474828beb5d2");
        var documentTypeId = DocumentTypeId.ADDITIONAL_DETAILS;
        var file = FormFileHelper.GetFormFile("this is just a test", "superFile.pdf", "application/pdf");

        //Act
        await this._controller.UpdateAppDocumentAsync(appId, documentTypeId, file, CancellationToken.None).ConfigureAwait(false);

        // Assert 
        A.CallTo(() => _logic.CreateAppDocumentAsync(appId, documentTypeId, file, A<ValueTuple<Guid, Guid>>.That.Matches(x => x.Item1 == _identity.UserId && x.Item2 == _identity.CompanyId), CancellationToken.None))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task AddAppUserRole_AndUserRoleDescriptionWith201StatusCode()
    {
        //Arrange
        var appId = new Guid("5cf74ef8-e0b7-4984-a872-474828beb5d2");
        var appUserRoles = _fixture.CreateMany<AppUserRole>(3);
        var appRoleData = _fixture.CreateMany<AppRoleData>(3);
        A.CallTo(() => _logic.AddAppUserRoleAsync(appId, appUserRoles, _identity.CompanyId))
            .Returns(appRoleData);

        //Act
        var result = await this._controller.AddAppUserRole(appId, appUserRoles).ConfigureAwait(false);
        foreach (var item in result)
        {
            //Assert
            A.CallTo(() => _logic.AddAppUserRoleAsync(appId, appUserRoles, _identity.CompanyId)).MustHaveHappenedOnceExactly();
            Assert.NotNull(item);
            Assert.IsType<AppRoleData>(item);
        }
    }

    [Fact]
    public async Task GetOfferAgreementData_ReturnsExpectedResult()
    {
        //Arrange
        var data = _fixture.CreateMany<AgreementDocumentData>(5).ToAsyncEnumerable();
        A.CallTo(() => _logic.GetOfferAgreementDataAsync())
            .Returns(data);

        //Act
        var result = await this._controller.GetOfferAgreementDataAsync().ToListAsync().ConfigureAwait(false);

        // Assert 
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetOfferAgreementConsentById_ReturnsExpectedResult()
    {
        //Arrange
        var appId = Guid.NewGuid();
        var data = _fixture.Create<OfferAgreementConsent>();
        A.CallTo(() => _logic.GetOfferAgreementConsentById(A<Guid>._, A<Guid>._))
            .Returns(data);

        //Act
        var result = await this._controller.GetOfferAgreementConsentById(appId).ConfigureAwait(false);

        // Assert 
        result.Should().Be(data);
        A.CallTo(() => _logic.GetOfferAgreementConsentById(appId, _identity.CompanyId))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task SubmitOfferConsentToAgreementsAsync_ReturnsExpectedResult()
    {
        //Arrange
        var appId = Guid.NewGuid();
        var data = _fixture.Create<OfferAgreementConsent>();
        var consentStatusData = new ConsentStatusData(Guid.NewGuid(), ConsentStatusId.ACTIVE);
        var identitydatas = (_identity.UserId, _identity.CompanyId);
        A.CallTo(() => _logic.SubmitOfferConsentAsync(A<Guid>._, A<OfferAgreementConsent>._, A<(Guid, Guid)>._))
            .Returns(Enumerable.Repeat(consentStatusData, 1));

        //Act
        var result = await this._controller.SubmitOfferConsentToAgreementsAsync(appId, data).ConfigureAwait(false);

        // Assert 
        result.Should().HaveCount(1);
        A.CallTo(() => _logic.SubmitOfferConsentAsync(appId, data, identitydatas))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetAppDetailsForStatusAsync_ReturnsExpectedResult()
    {
        //Arrange
        var appId = Guid.NewGuid();
        var data = _fixture.Create<AppProviderResponse>();
        A.CallTo(() => _logic.GetAppDetailsForStatusAsync(A<Guid>._, A<Guid>._))
            .Returns(data);

        //Act
        var result = await this._controller.GetAppDetailsForStatusAsync(appId).ConfigureAwait(false);

        // Assert 
        result.Should().Be(data);
        A.CallTo(() => _logic.GetAppDetailsForStatusAsync(appId, _identity.CompanyId))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DeleteAppRoleAsync_ReturnsExpectedResult()
    {
        //Arrange
        var appId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        //Act
        var result = await this._controller.DeleteAppRoleAsync(appId, roleId).ConfigureAwait(false);

        // Assert 
        Assert.IsType<NoContentResult>(result);
        A.CallTo(() => _logic.DeleteAppRoleAsync(appId, roleId, _identity.CompanyId))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetAppProviderSalesManagerAsync_ReturnsExpectedResult()
    {
        //Arrange
        var data = _fixture.CreateMany<CompanyUserNameData>(5).ToAsyncEnumerable();
        A.CallTo(() => _logic.GetAppProviderSalesManagersAsync(A<Guid>._))
            .Returns(data);

        //Act
        var result = await this._controller.GetAppProviderSalesManagerAsync().ToListAsync().ConfigureAwait(false);

        // Assert 
        result.Should().HaveCount(5);
        A.CallTo(() => _logic.GetAppProviderSalesManagersAsync(_identity.CompanyId))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExecuteAppCreation_ReturnsExpectedId()
    {
        //Arrange
        var appId = _fixture.Create<Guid>();
        var data = _fixture.Create<AppRequestModel>();
        A.CallTo(() => _logic.AddAppAsync(A<AppRequestModel>._, _identity.CompanyId))
            .Returns(appId);

        //Act
        var result = await this._controller.ExecuteAppCreation(data).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.AddAppAsync(data, _identity.CompanyId)).MustHaveHappenedOnceExactly();
        Assert.IsType<CreatedAtRouteResult>(result);
        result.Value.Should().Be(appId);
    }

    [Fact]
    public async Task UpdateAppRelease_ReturnsNoContent()
    {
        // Arrange
        var appId = new Guid("5cf74ef8-e0b7-4984-a872-474828beb5d2");
        var data = new AppRequestModel(
            "Test",
            "Test Provider",
            Guid.NewGuid(),
            new[]
            {
                Guid.NewGuid()
            },
            new LocalizedDescription[]
            {
                new("en", "This is a long description", "description")
            },
            new[]
            {
                "https://test.com/image.jpg"
            },
            "19€",
            new[]
            {
                PrivacyPolicyId.COMPANY_DATA
            },
            "https://test.provider.com",
            "test@gmail.com",
            "9456321678"
            );

        // Act
        var result = await this._controller.UpdateAppRelease(appId, data).ConfigureAwait(false);

        // Assert
        Assert.IsType<NoContentResult>(result);
        A.CallTo(() => _logic.UpdateAppReleaseAsync(appId, data, _identity.CompanyId)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetAllInReviewStatusAppsAsync_ReturnsExpectedCount()
    {
        //Arrange
        var paginationResponse = new Pagination.Response<InReviewAppData>(new Pagination.Metadata(15, 1, 1, 15), _fixture.CreateMany<InReviewAppData>(5));
        A.CallTo(() => _logic.GetAllInReviewStatusAppsAsync(A<int>._, A<int>._, A<OfferSorting?>._, null))
            .Returns(paginationResponse);

        //Act
        var result = await this._controller.GetAllInReviewStatusAppsAsync().ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.GetAllInReviewStatusAppsAsync(0, 15, null, null)).MustHaveHappenedOnceExactly();
        result.Content.Should().HaveCount(5);
    }

    [Fact]
    public async Task SubmitAppReleaseRequest_ReturnsExpectedCount()
    {
        //Arrange
        var appId = _fixture.Create<Guid>();

        //Act
        var result = await this._controller.SubmitAppReleaseRequest(appId).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.SubmitAppReleaseRequestAsync(appId, _identity.UserId)).MustHaveHappenedOnceExactly();
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task ApproveAppRequest_ReturnsExpectedCount()
    {
        //Arrange
        var appId = _fixture.Create<Guid>();

        //Act
        var result = await this._controller.ApproveAppRequest(appId).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.ApproveAppRequestAsync(appId, _identity.UserId)).MustHaveHappenedOnceExactly();
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeclineAppRequest_ReturnsNoContent()
    {
        //Arrange
        var appId = _fixture.Create<Guid>();
        var data = new OfferDeclineRequest("Just a test");

        //Act
        var result = await this._controller.DeclineAppRequest(appId, data).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.DeclineAppRequestAsync(appId, _identity.UserId, data)).MustHaveHappenedOnceExactly();
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task GetinReviewAppDetailsByIdAsync_ReturnsExpectedResult()
    {
        //Arrange
        var appId = _fixture.Create<Guid>();
        var data = _fixture.Create<InReviewAppDetails>();

        A.CallTo(() => _logic.GetInReviewAppDetailsByIdAsync(appId))
            .Returns(data);

        //Act
        var result = await this._controller.GetInReviewAppDetailsByIdAsync(appId);

        //Assert
        result.Should().NotBeNull();
        result.Title.Should().Be(data.Title);
        result.OfferStatusId.Should().Be(data.OfferStatusId);
    }

    [Fact]
    public async Task DeleteAppDocumentsAsync_ReturnsExpectedResult()
    {
        //Arrange
        var documentId = Guid.NewGuid();

        //Act
        var result = await this._controller.DeleteAppDocumentsAsync(documentId).ConfigureAwait(false);

        // Assert 
        Assert.IsType<NoContentResult>(result);
        A.CallTo(() => _logic.DeleteAppDocumentsAsync(documentId, _identity.CompanyId))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DeleteAppAsync_ReturnsExpectedResult()
    {
        //Arrange
        var appId = _fixture.Create<Guid>();

        //Act
        var result = await this._controller.DeleteAppAsync(appId).ConfigureAwait(false);

        // Assert 
        Assert.IsType<NoContentResult>(result);
        A.CallTo(() => _logic.DeleteAppAsync(appId, _identity.CompanyId))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task SetInstanceType_ReturnsExpectedResult()
    {
        var appId = _fixture.Create<Guid>();
        var data = new AppInstanceSetupData(true, "https://test.de");

        var result = await _controller.SetInstanceType(appId, data).ConfigureAwait(false);

        Assert.IsType<NoContentResult>(result);
        A.CallTo(() => _logic.SetInstanceType(appId, data, _identity.CompanyId))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetTechnicalUserProfiles_ReturnsExpectedCount()
    {
        //Arrange
        var offerId = Guid.NewGuid();

        var data = _fixture.CreateMany<TechnicalUserProfileInformation>(5);
        A.CallTo(() => _logic.GetTechnicalUserProfilesForOffer(offerId, _identity.CompanyId))
            .Returns(data);

        //Act
        var result = await this._controller.GetTechnicalUserProfiles(offerId).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.GetTechnicalUserProfilesForOffer(offerId, _identity.CompanyId)).MustHaveHappenedOnceExactly();
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
        A.CallTo(() => _logic.UpdateTechnicalUserProfiles(offerId, A<IEnumerable<TechnicalUserProfileData>>.That.Matches(x => x.Count() == 5), _identity.CompanyId)).MustHaveHappenedOnceExactly();
        result.Should().BeOfType<NoContentResult>();
    }
}
