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
using Org.CatenaX.Ng.Portal.Backend.Framework.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.CatenaX.Ng.Portal.Backend.Tests.Shared.Extensions;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Org.CatenaX.Ng.Portal.Backend.Notification.Service.BusinessLogic;
using Org.CatenaX.Ng.Portal.Backend.Notification.Service.Controllers;
using Xunit;

namespace Org.CatenaX.Ng.Portal.Backend.Notification.Service.Tests.Controllers;

public class ServiceControllerTest
{
    private static readonly string IamUserId = "4C1A6851-D4E7-4E10-A011-3732CD045E8A";
    private readonly string _accessToken = "THISISTHEACCESSTOKEN";
    private readonly IFixture _fixture;
    private readonly INotificationBusinessLogic _logic;
    private readonly NotificationController _controller;

    public ServiceControllerTest()
    {
        _fixture = new Fixture();
        _logic = A.Fake<INotificationBusinessLogic>();
        this._controller = new NotificationController(_logic);
        _controller.AddControllerContextWithClaimAndBearer(IamUserId, _accessToken);
    }

    [Fact]
    public async Task CreateNotification_ReturnsExpectedResult()
    {
        //Arrange
        var data = _fixture.Create<NotificationDetailData>();
        var creationData = _fixture.Create<NotificationCreationData>();
        var companyUserId = Guid.NewGuid();
        A.CallTo(() => _logic.CreateNotificationAsync(IamUserId, A<NotificationCreationData>._, A<Guid>._))
            .ReturnsLazily(() => data.Id);

        //Act
        var result = await this._controller.CreateNotification(companyUserId, creationData).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.CreateNotificationAsync(IamUserId, creationData, companyUserId)).MustHaveHappenedOnceExactly();
        Assert.IsType<CreatedAtRouteResult>(result);
        result.Value.Should().Be(data.Id);
    }

    [Fact]
    public async Task GetNotifications_ReturnsExpectedCount()
    {
        //Arrange
        var paginationResponse = new Pagination.Response<NotificationDetailData>(new Pagination.Metadata(15, 1, 1, 15), _fixture.CreateMany<NotificationDetailData>(5));
        A.CallTo(() => _logic.GetNotificationsAsync(A<int>._, A<int>._, A<string>._, A<bool?>._, A<NotificationTypeId?>._, A<NotificationSorting?>._))
            .ReturnsLazily(()=> paginationResponse);

        //Act
        var result = await this._controller.GetNotifications().ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.GetNotificationsAsync(0, 15, A<string>._, A<bool?>._, A<NotificationTypeId?>._, A<NotificationSorting?>._)).MustHaveHappenedOnceExactly();
        Assert.IsType<Pagination.Response<NotificationDetailData>>(result);
        result.Content.Should().HaveCount(5);
    }
        
    [Fact]
    public async Task GetNotification_ReturnsExpectedData()
    {
        //Arrange
        var data = _fixture.Create<NotificationDetailData>();
        var notificationId = _fixture.Create<Guid>();
        A.CallTo(() => _logic.GetNotificationDetailDataAsync(IamUserId, A<Guid>._))
            .ReturnsLazily(() => data);

        //Act
        var result = await this._controller.GetNotification(notificationId).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.GetNotificationDetailDataAsync(IamUserId, notificationId)).MustHaveHappenedOnceExactly();
        result.Should().Be(data);
    }
        
    [Fact]
    public async Task NotificationCountDetails_ReturnsExpectedData()
    {
        //Arrange
        var data = _fixture.Create<NotificationCountDetails>();
        A.CallTo(() => _logic.GetNotificationCountDetailsAsync(IamUserId))
            .ReturnsLazily(() => data);

        //Act
        var result = await this._controller.NotificationCountDetails().ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.GetNotificationCountDetailsAsync(IamUserId)).MustHaveHappenedOnceExactly();
        Assert.IsType<NotificationCountDetails>(result);
        result.Should().Be(data);
    }
        
    [Fact]
    public async Task SetNotificationToRead_ReturnsNoContent()
    {
        //Arrange
        var notificationId = Guid.NewGuid();
        A.CallTo(() => _logic.SetNotificationStatusAsync(IamUserId, notificationId, A<bool>._))
            .ReturnsLazily(() => Task.CompletedTask);

        //Act
        var result = await this._controller.SetNotificationToRead(notificationId).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.SetNotificationStatusAsync(IamUserId, notificationId, true)).MustHaveHappenedOnceExactly();
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteNotification_ReturnsNoContent()
    {
        //Arrange
        var notificationId = Guid.NewGuid();
        A.CallTo(() => _logic.DeleteNotificationAsync(IamUserId, notificationId))
            .ReturnsLazily(() => Task.CompletedTask);

        //Act
        var result = await this._controller.DeleteNotification(notificationId).ConfigureAwait(false);

        //Assert
        A.CallTo(() => _logic.DeleteNotificationAsync(IamUserId, notificationId)).MustHaveHappenedOnceExactly();
        Assert.IsType<NoContentResult>(result);
    }
}
