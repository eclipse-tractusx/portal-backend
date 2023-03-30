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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Org.Eclipse.TractusX.Portal.Backend.Apps.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Apps.Service.ViewModels;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;
using Xunit;

namespace Org.Eclipse.TractusX.Portal.Backend.Apps.Service.Controllers.Tests;

public class AppsControllerTests
{
    private static readonly string IamUserId = "4C1A6851-D4E7-4E10-A011-3732CD045E8A";
    private readonly string _accessToken = "THISISTHEACCESSTOKEN";
    private readonly IFixture _fixture;
    private readonly IAppsBusinessLogic _logic;
    private readonly AppsController _controller;

    public AppsControllerTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _logic = A.Fake<IAppsBusinessLogic>();
        this._controller = new AppsController(_logic);
        _controller.AddControllerContextWithClaimAndBearer(IamUserId, _accessToken);
    }

    [Fact]
    public async Task GetAllActiveAppsAsync_ReturnsExpectedCount()
    {
        //Arrange
        var data = new AsyncEnumerableStub<AppData>(_fixture.CreateMany<AppData>(5));
        A.CallTo(() => _logic.GetAllActiveAppsAsync(A<string?>._))
            .Returns(data.AsAsyncEnumerable());

        //Act
        var result = await this._controller.GetAllActiveAppsAsync().ToListAsync().ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.GetAllActiveAppsAsync(null)).MustHaveHappenedOnceExactly();
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetAllBusinessAppsForCurrentUserAsync_ReturnsExpectedCount()
    {
        //Arrange
        var data = new AsyncEnumerableStub<BusinessAppData>(_fixture.CreateMany<BusinessAppData>(5));
        A.CallTo(() => _logic.GetAllUserUserBusinessAppsAsync(A<string>._))
            .Returns(data.AsAsyncEnumerable());

        //Act
        var result = await this._controller.GetAllBusinessAppsForCurrentUserAsync().ToListAsync().ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.GetAllUserUserBusinessAppsAsync(IamUserId)).MustHaveHappenedOnceExactly();
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetAppDetailsByIdAsync_ReturnsExpectedCount()
    {
        //Arrange
        var appId = _fixture.Create<Guid>();
        var data = _fixture.Create<AppDetailResponse>();
        A.CallTo(() => _logic.GetAppDetailsByIdAsync(A<Guid>._, A<string>._, A<string?>._))
            .ReturnsLazily(() => data);

        //Act
        var result = await this._controller.GetAppDetailsByIdAsync(appId).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.GetAppDetailsByIdAsync(appId, IamUserId, null)).MustHaveHappenedOnceExactly();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllFavouriteAppsForCurrentUser_ReturnsExpectedCount()
    {
        //Arrange
        var ids = new AsyncEnumerableStub<Guid>(_fixture.CreateMany<Guid>(5));
        A.CallTo(() => _logic.GetAllFavouriteAppsForUserAsync(A<string>._))
            .Returns(ids.AsAsyncEnumerable());

        //Act
        var result = await this._controller.GetAllFavouriteAppsForCurrentUserAsync().ToListAsync().ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.GetAllFavouriteAppsForUserAsync(IamUserId)).MustHaveHappenedOnceExactly();
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task AddFavouriteAppForCurrentUserAsync_ReturnsBusinessLogic()
    {
        //Arrange
        var id = _fixture.Create<Guid>();
        A.CallTo(() => _logic.AddFavouriteAppForUserAsync(A<Guid>._, A<string>._))
            .ReturnsLazily(() => Task.CompletedTask);

        //Act
        var result = await this._controller.AddFavouriteAppForCurrentUserAsync(id).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.AddFavouriteAppForUserAsync(id, IamUserId)).MustHaveHappenedOnceExactly();
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task RemoveFavouriteAppForCurrentUserAsync_CallsBusinessLogic()
    {
        //Arrange
        var id = _fixture.Create<Guid>();
        A.CallTo(() => _logic.RemoveFavouriteAppForUserAsync(A<Guid>._, A<string>._))
            .ReturnsLazily(() => Task.CompletedTask);

        //Act
        var result = await this._controller.RemoveFavouriteAppForCurrentUserAsync(id).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.RemoveFavouriteAppForUserAsync(id, IamUserId)).MustHaveHappenedOnceExactly();
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task GetCompanySubscribedAppSubscriptionStatusesForCurrentUserAsync_ReturnsExpectedCount()
    {
        //Arrange
        var data = new AsyncEnumerableStub<AppWithSubscriptionStatus>(_fixture.CreateMany<AppWithSubscriptionStatus>(5));
        A.CallTo(() => _logic.GetCompanySubscribedAppSubscriptionStatusesForUserAsync(A<string>._))
            .Returns(data.AsAsyncEnumerable());

        //Act
        var result = await this._controller.GetCompanySubscribedAppSubscriptionStatusesForCurrentUserAsync().ToListAsync().ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.GetCompanySubscribedAppSubscriptionStatusesForUserAsync(IamUserId)).MustHaveHappenedOnceExactly();
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetCompanyProvidedAppSubscriptionStatusesForCurrentUserAsync_ReturnsExpectedCount()
    {
        //Arrange
        var data = _fixture.CreateMany<OfferCompanySubscriptionStatusData>(5);
        var pagination = new Pagination.Response<OfferCompanySubscriptionStatusData>(new Pagination.Metadata(data.Count(), 1, 0, data.Count()), data);
        A.CallTo(() => _logic.GetCompanyProvidedAppSubscriptionStatusesForUserAsync(A<int>._, A<int>._, A<string>._, A<SubscriptionStatusSorting?>._, A<OfferSubscriptionStatusId?>._))
            .ReturnsLazily(() => pagination);

        //Act
        var result = await this._controller.GetCompanyProvidedAppSubscriptionStatusesForCurrentUserAsync().ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.GetCompanyProvidedAppSubscriptionStatusesForUserAsync(0, 15, IamUserId, null, null)).MustHaveHappenedOnceExactly();
        result.Content.Should().HaveCount(5);
    }

    [Fact]
    public async Task AddAppSubscriptionWithConsent_ReturnsExpectedId()
    {
        //Arrange
        var offerSubscriptionId = Guid.NewGuid();
        var consentData = _fixture.CreateMany<OfferAgreementConsentData>(2);
        A.CallTo(() => _logic.AddOwnCompanyAppSubscriptionAsync(A<Guid>._, A<IEnumerable<OfferAgreementConsentData>>._, IamUserId, _accessToken))
            .Returns(offerSubscriptionId);

        //Act
        var serviceId = Guid.NewGuid();
        var result = await this._controller.AddCompanyAppSubscriptionAsync(serviceId, consentData).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.AddOwnCompanyAppSubscriptionAsync(serviceId, consentData, IamUserId, _accessToken)).MustHaveHappenedOnceExactly();
        Assert.IsType<NoContentResult>(result);
    }
    
    [Fact]
    public async Task ActivateCompanyAppSubscriptionAsync_ReturnsNoContent()
    {
        //Arrange
        var appId = _fixture.Create<Guid>();
        var companyId = _fixture.Create<Guid>();
        A.CallTo(() => _logic.ActivateOwnCompanyProvidedAppSubscriptionAsync(A<Guid>._, A<Guid>._, A<string>._))
            .ReturnsLazily(() => Task.CompletedTask);

        //Act
        var result = await this._controller.ActivateCompanyAppSubscriptionAsync(appId, companyId).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.ActivateOwnCompanyProvidedAppSubscriptionAsync(appId, companyId, IamUserId)).MustHaveHappenedOnceExactly();
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task UnsubscribeCompanyAppSubscription_ReturnsNoContent()
    {
        //Arrange
        var appId = _fixture.Create<Guid>();
        A.CallTo(() => _logic.UnsubscribeOwnCompanyAppSubscriptionAsync(A<Guid>._, A<string>._))
            .ReturnsLazily(() => Task.CompletedTask);

        //Act
        var result = await this._controller.UnsubscribeCompanyAppSubscriptionAsync(appId).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.UnsubscribeOwnCompanyAppSubscriptionAsync(appId, IamUserId)).MustHaveHappenedOnceExactly();
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task GetAppDataAsync_ReturnsExpectedCount()
    {
        //Arrange
        var data = new AsyncEnumerableStub<AllOfferData>(_fixture.CreateMany<AllOfferData>(5));
        A.CallTo(() => _logic.GetCompanyProvidedAppsDataForUserAsync(A<string>._))
            .Returns(data.AsAsyncEnumerable());

        //Act
        var result = await this._controller.GetAppDataAsync().ToListAsync().ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.GetCompanyProvidedAppsDataForUserAsync(IamUserId)).MustHaveHappenedOnceExactly();
        result.Should().HaveSameCount(data);
    }
    
    [Fact]
    public async Task GetServiceAgreement_ReturnsExpected()
    {
        //Arrange
        var appId = _fixture.Create<Guid>();
        var agreementData = _fixture.CreateMany<AgreementData>(5).ToAsyncEnumerable();
        A.CallTo(() => _logic.GetAppAgreement(A<Guid>._))
            .Returns(agreementData);

        //Act
        var result = await this._controller.GetAppAgreement(appId).ToListAsync().ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.GetAppAgreement(appId)).MustHaveHappenedOnceExactly();
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task AutoSetupService_ReturnsExpected()
    {
        //Arrange
        var offerSubscriptionId = Guid.NewGuid();
        var data = new OfferAutoSetupData(offerSubscriptionId, "https://test.de");
        var responseData = new OfferAutoSetupResponseData(
            new TechnicalUserInfoData(Guid.NewGuid(), "abcPW", "sa1"),
            new ClientInfoData(Guid.NewGuid().ToString())
        );
        A.CallTo(() => _logic.AutoSetupAppAsync(A<OfferAutoSetupData>._, A<string>.That.Matches(x => x== IamUserId)))
            .Returns(responseData);

        //Act
        var result = await this._controller.AutoSetupApp(data).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.AutoSetupAppAsync(data, IamUserId)).MustHaveHappenedOnceExactly();
        Assert.IsType<OfferAutoSetupResponseData>(result);
        result.Should().Be(responseData);
    }

    [Fact]
    public async Task DeactivateApp_ReturnsNoContent()
    {
        //Arrange
        var appId = _fixture.Create<Guid>();
        A.CallTo(() => _logic.DeactivateOfferByAppIdAsync(A<Guid>._, A<string>._))
            .ReturnsLazily(() => Task.CompletedTask);
        
        //Act
        var result = await this._controller.DeactivateApp(appId).ConfigureAwait(false);
        
        //Assert
        A.CallTo(() => _logic.DeactivateOfferByAppIdAsync(appId, IamUserId)).MustHaveHappenedOnceExactly();
        result.Should().BeOfType<NoContentResult>(); 
    }

    [Fact]
    public async Task GetAppImageDocumentContentAsync_ReturnsExpected()
    {
        //Arrange
        var appId = _fixture.Create<Guid>();
        var documentId = _fixture.Create<Guid>();
        var content = _fixture.Create<byte[]>();
        var fileName = _fixture.Create<string>();
        
        A.CallTo(() => _logic.GetAppDocumentContentAsync(A<Guid>._ , A<Guid>._, A<CancellationToken>._))
            .ReturnsLazily(() => (content,"image/png",fileName));

        //Act
        var result = await this._controller.GetAppDocumentContentAsync(appId,documentId,CancellationToken.None).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.GetAppDocumentContentAsync(A<Guid>._ , A<Guid>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetAppUpdateDescriptionsAsync_ReturnsExpected()
    {
        //Arrange
        var appId = _fixture.Create<Guid>();
        var offerDescriptionData = _fixture.CreateMany<LocalizedDescription>(3);

        A.CallTo(() => _logic.GetAppUpdateDescriptionByIdAsync(A<Guid>._, A<string>._))
            .ReturnsLazily(() => offerDescriptionData);
        
        //Act
        var result = await this._controller.GetAppUpdateDescriptionsAsync(appId).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.GetAppUpdateDescriptionByIdAsync(A<Guid>._, A<string>._)).MustHaveHappened();
    }

    [Fact]
    public async Task CreateOrUpdateAppDescriptionsAsync_ReturnsExpected()
    {
        //Arrange
        var appId = _fixture.Create<Guid>();
        var offerDescriptionData = _fixture.CreateMany<LocalizedDescription>(3);

        A.CallTo(() => _logic.CreateOrUpdateAppDescriptionByIdAsync(A<Guid>._, A<string>._, A<IEnumerable<LocalizedDescription>>._))
            .ReturnsLazily(() => Task.CompletedTask);
        
        //Act
        var result = await this._controller.CreateOrUpdateAppDescriptionsByIdAsync(appId,offerDescriptionData).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.CreateOrUpdateAppDescriptionByIdAsync(A<Guid>._, A<string>._, A<IEnumerable<LocalizedDescription>>._)).MustHaveHappened();
        result.Should().BeOfType<NoContentResult>(); 
    }

    [Fact]
    public async Task GetAppDocumentTypePdfContentAsync_ReturnsExpected()
    {
        //Arrange
        var appId = _fixture.Create<Guid>();
        var documentId = _fixture.Create<Guid>();
        var content = _fixture.Create<byte[]>();
        var fileName = _fixture.Create<string>();

        A.CallTo(() => _logic.GetAppDocumentContentAsync(A<Guid>._, A<Guid>._, A<CancellationToken>._))
            .ReturnsLazily(() => (content, "application/pdf", fileName));

        //Act
        var result = await this._controller.GetAppDocumentContentAsync(appId, documentId, CancellationToken.None).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.GetAppDocumentContentAsync(A<Guid>._, A<Guid>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CreatOfferAssignedAppLeadImageDocumentByIdAsync_ReturnsExpected()
    {
        // Arrange
        var appId = _fixture.Create<Guid>();
        var file = FormFileHelper.GetFormFile("Test Image", "TestImage.jpeg", "image/jpeg");
        A.CallTo(() => _logic.CreatOfferAssignedAppLeadImageDocumentByIdAsync(A<Guid>._, A<string>._, A<IFormFile>._, CancellationToken.None))
            .ReturnsLazily(() => Task.CompletedTask);

        // Act
        var result = await this._controller.CreatOfferAssignedAppLeadImageDocumentByIdAsync(appId, file, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _logic.CreatOfferAssignedAppLeadImageDocumentByIdAsync(appId, IamUserId, file, CancellationToken.None)).MustHaveHappenedOnceExactly();
        result.Should().BeOfType<NoContentResult>();
    }
}
