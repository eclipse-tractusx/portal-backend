/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
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
using Org.CatenaX.Ng.Portal.Backend.Apps.Service.BusinessLogic;
using Org.CatenaX.Ng.Portal.Backend.Apps.Service.Controllers;
using Org.CatenaX.Ng.Portal.Backend.Apps.Service.ViewModels;
using Org.CatenaX.Ng.Portal.Backend.Framework.Models;
using Org.CatenaX.Ng.Portal.Backend.Offers.Library.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.CatenaX.Ng.Portal.Backend.Tests.Shared;
using Org.CatenaX.Ng.Portal.Backend.Tests.Shared.Extensions;
using Xunit;

namespace Org.CatenaX.Ng.Portal.Backend.Apps.Service.Controllers.Tests;

public class AppsControllerTests
{
    private static readonly string IamUserId = "4C1A6851-D4E7-4E10-A011-3732CD045E8A";
    private readonly string _accessToken = "THISISTHEACCESSTOKEN";
    private readonly IAppsBusinessLogic _logic;
    private readonly AppsController _controller;
    private readonly IFixture _fixture;

    public AppsControllerTests()
    {
        _fixture = new Fixture();
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
    public async Task CreateApp_ReturnsExpectedId()
    {
        //Arrange
        var id = new Guid("d90995fe-1241-4b8d-9f5c-f3909acc6383");
        var appInputModel = _fixture.Create<AppInputModel>();
        A.CallTo(() => _logic.CreateAppAsync(A<AppInputModel>._))
            .Returns(id);

        //Act
        var result = await this._controller.CreateAppAsync(appInputModel).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.CreateAppAsync(appInputModel)).MustHaveHappenedOnceExactly();
        result.Value.Should().Be(id);
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
        var data = new AsyncEnumerableStub<AppCompanySubscriptionStatusData>(_fixture.CreateMany<AppCompanySubscriptionStatusData>(5));
        A.CallTo(() => _logic.GetCompanyProvidedAppSubscriptionStatusesForUserAsync(A<string>._))
            .Returns(data.AsAsyncEnumerable());

        //Act
        var result = await this._controller.GetCompanyProvidedAppSubscriptionStatusesForCurrentUserAsync().ToListAsync().ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.GetCompanyProvidedAppSubscriptionStatusesForUserAsync(IamUserId)).MustHaveHappenedOnceExactly();
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task AddCompanyAppSubscriptionAsync_ReturnsExpectedId()
    {
        //Arrange
        var offerSubscriptionId = Guid.NewGuid();
        A.CallTo(() => _logic.AddOwnCompanyAppSubscriptionAsync(A<Guid>._, IamUserId, _accessToken))
            .Returns(offerSubscriptionId);

        //Act
        var serviceId = Guid.NewGuid();
        var result = await this._controller.AddCompanyAppSubscriptionAsync(serviceId).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.AddOwnCompanyAppSubscriptionAsync(serviceId, IamUserId, _accessToken)).MustHaveHappenedOnceExactly();
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
        var data = new AsyncEnumerableStub<AllAppData>(_fixture.CreateMany<AllAppData>(5));
        A.CallTo(() => _logic.GetCompanyProvidedAppsDataForUserAsync(A<string>._))
            .Returns(data.AsAsyncEnumerable());

        //Act
        var result = await this._controller.GetAppDataAsync().ToListAsync().ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.GetCompanyProvidedAppsDataForUserAsync(IamUserId)).MustHaveHappenedOnceExactly();
        result.Should().HaveCount(5);
    }
    
    [Fact]
    public async Task ExecuteAppCreation_ReturnsExpectedId()
    {
        //Arrange
        var appId = _fixture.Create<Guid>();
        var data = _fixture.Create<AppRequestModel>();
        A.CallTo(() => _logic.AddAppAsync(A<AppRequestModel>._))
            .ReturnsLazily(() => appId);

        //Act
        var result = await this._controller.ExecuteAppCreation(data).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.AddAppAsync(data)).MustHaveHappenedOnceExactly();
        Assert.IsType<CreatedAtRouteResult>(result);
        result.Value.Should().Be(appId);
    }

    [Fact]
    public async Task GetAllInReviewStatusAppsAsync_ReturnsExpectedCount()
    {
        //Arrange
        var paginationResponse = new Pagination.Response<InReviewAppData>(new Pagination.Metadata(15, 1, 1, 15), _fixture.CreateMany<InReviewAppData>(5));
        A.CallTo(() => _logic.GetAllInReviewStatusAppsAsync(A<int>._, A<int>._))
            .ReturnsLazily(() => paginationResponse);

        //Act
        var result = await this._controller.GetAllInReviewStatusAppsAsync().ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.GetAllInReviewStatusAppsAsync(0, 15)).MustHaveHappenedOnceExactly();
        result.Content.Should().HaveCount(5);
    }

    [Fact]
    public async Task SubmitAppReleaseRequest_ReturnsExpectedCount()
    {
        //Arrange
        var appId = _fixture.Create<Guid>();
        A.CallTo(() => _logic.SubmitAppReleaseRequestAsync(appId, A<string>._))
            .ReturnsLazily(() => Task.CompletedTask);

        //Act
        var result = await this._controller.SubmitAppReleaseRequest(appId).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.SubmitAppReleaseRequestAsync(appId, IamUserId)).MustHaveHappenedOnceExactly();
        Assert.IsType<NoContentResult>(result);
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
        var result = await this._controller.AutoSetupService(data).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.AutoSetupAppAsync(data, IamUserId)).MustHaveHappenedOnceExactly();
        Assert.IsType<OfferAutoSetupResponseData>(result);
        result.Should().Be(responseData);
    }
}
