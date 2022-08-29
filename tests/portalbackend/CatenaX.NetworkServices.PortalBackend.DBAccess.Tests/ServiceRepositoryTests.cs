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
using AutoFixture.AutoFakeItEasy;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;
using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.Tests.Shared.DatabaseRelatedTests;
using CatenaX.NetworkServices.Tests.Shared.TestSeeds;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Tests;

/// <summary>
/// Tests the functionality of the <see cref="ServiceRepositoryTests"/>
/// </summary>
public class ServiceRepositoryTests : IClassFixture<TestDbFixture>
{
    private const string IamUserId = "3d8142f1-860b-48aa-8c2b-1ccb18699f65";
    private readonly IFixture _fixture;
    private readonly ICollection<App> _apps;
    private readonly TestDbFixture _dbTestDbFixture;

    public ServiceRepositoryTests(TestDbFixture testDbFixture)
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));

        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }

    #region GetAllAsDetailsByUserIdUntracked

    [Fact]
    public async Task GetActiveServices_ReturnsExpectedAppCount()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var results = await sut.GetActiveServices().ToListAsync();

        // Assert
        results.Should().NotBeNullOrEmpty();
        results.Should().HaveCount(1);
    }
    //
    // [Fact]
    // public async Task GetAllAsDetailsByUserIdUntracked_WithReadStatus_ReturnsExpectedNotificationDetailData()
    // {
    //     // Arrange
    //     var sut = CreateSut();
    //
    //     // Act
    //     var results = await sut.GetAllNotificationDetailsByIamUserIdUntracked(IamUserId, true, null).ToListAsync();
    //
    //     var readNotificationIds = _readNotifications.Select(notification => notification.Id).ToList();
    //     // Assert
    //     results.Should().NotBeNullOrEmpty();
    //     results.Should().HaveCount(_readNotifications.Count);
    //     results.Should().AllBeOfType<NotificationDetailData>();
    //     results.Select(x => x.Id).Should().BeEquivalentTo(readNotificationIds);
    // }
    //
    // [Fact]
    // public async Task GetAllAsDetailsByUserIdUntracked_WithReadStatusAndInfoType_ReturnsExpectedNotificationDetailData()
    // {
    //     // Arrange
    //     var sut = CreateSut();
    //
    //     // Act
    //     var results = await sut.GetAllNotificationDetailsByIamUserIdUntracked(IamUserId, true, NotificationTypeId.INFO).ToListAsync();
    //
    //     // Assert
    //     var readNotificationIds = _readNotifications
    //         .Where(x => x.NotificationTypeId == NotificationTypeId.INFO)
    //         .Select(x => x.Id)
    //         .ToList();
    //     results.Should().NotBeNullOrEmpty();
    //     results.Should().HaveCount(1);
    //     results.Should().AllBeOfType<NotificationDetailData>();
    //     results.Select(x => x.Id).Should().BeEquivalentTo(readNotificationIds);
    // }
    //
    // [Fact]
    // public async Task GetAllAsDetailsByUserIdUntracked_WithReadStatusAndActionType_ReturnsExpectedNotificationDetailData()
    // {
    //     // Arrange
    //     var sut = CreateSut();
    //
    //     // Act
    //     var results = await sut.GetAllNotificationDetailsByIamUserIdUntracked(IamUserId, true, NotificationTypeId.ACTION).ToListAsync();
    //
    //     // Assert
    //     var readNotificationIds = _readNotifications
    //         .Where(x => x.NotificationTypeId == NotificationTypeId.ACTION)
    //         .Select(x => x.Id).ToList();
    //     results.Should().NotBeNullOrEmpty();
    //     results.Should().HaveCount(2);
    //     results.Should().AllBeOfType<NotificationDetailData>();
    //     results.Select(x => x.Id).Should().BeEquivalentTo(readNotificationIds);
    // }
    //
    // #endregion
    //
    // #region GetNotificationCount
    //
    // [Fact]
    // public async Task GetNotificationCountAsync_WithReadStatus_ReturnsExpectedCount()
    // {
    //     // Arrange
    //     var sut = CreateSut();
    //
    //     // Act
    //     var results = await sut.GetNotificationCountForIamUserAsync(IamUserId, true);
    //
    //     // Assert
    //     results.Count.Should().Be(_readNotifications.Count);
    // }
    //
    // [Fact]
    // public async Task GetNotificationCountAsync_WithoutStatus_ReturnsExpectedCount()
    // {
    //     // Arrange
    //     var sut = CreateSut();
    //
    //     // Act
    //     var results = await sut.GetNotificationCountForIamUserAsync(IamUserId, null);
    //
    //     // Assert
    //     results.Count.Should().Be(_notifications.Count);
    // }

    private AppRepository CreateSut()
    {
        var context = _dbTestDbFixture.GetPortalDbContext();
        _fixture.Inject(context);
        var sut = _fixture.Create<AppRepository>();
        return sut;
    }

    #endregion
}
