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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests.Setup;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.TestSeeds;
using FluentAssertions;
using Xunit;
using Xunit.Extensions.AssemblyFixture;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests;

/// <summary>
/// Tests the functionality of the <see cref="NotificationRepository"/>
/// </summary>
/// [Collection("Database collection")]
public class NotificationRepositoryTests : IAssemblyFixture<TestDbFixture>
{
    private const string IamUserId = "3d8142f1-860b-48aa-8c2b-1ccb18699f65";
    private readonly IFixture _fixture;
    private readonly ICollection<Notification> _readNotifications;
    private readonly ICollection<Notification> _unreadNotifications;
    private readonly ICollection<Notification> _notifications;
    private readonly TestDbFixture _dbTestDbFixture;

    public NotificationRepositoryTests(TestDbFixture testDbFixture)
    {
        var companyUserId = new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001");
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));

        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _readNotifications = new List<Notification>();
        _unreadNotifications = new List<Notification>();
        for (var i = 0; i < 3; i++)
        {
            _readNotifications.Add(new Notification(Guid.NewGuid(), companyUserId, DateTimeOffset.UtcNow, i % 2 == 0 ? NotificationTypeId.ACTION : NotificationTypeId.INFO, true));
        }

        for (var i = 0; i < 2; i++)
        {
            _unreadNotifications.Add(new Notification(Guid.NewGuid(), companyUserId, DateTimeOffset.UtcNow, i % 2 == 0 ? NotificationTypeId.ACTION : NotificationTypeId.INFO, false));
        }

        _notifications = _readNotifications.Concat(_unreadNotifications).ToList();
        _dbTestDbFixture = testDbFixture;
    }

    #region GetAllAsDetailsByUserIdUntracked

    [Fact]
    public async Task GetAllAsDetailsByUserIdUntracked_WithUnreadStatus_ReturnsExpectedNotificationDetailData()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut.GetAllNotificationDetailsByIamUserIdUntracked(IamUserId, false, null).ToListAsync();

        // Assert
        var unreadNotificationIds = _unreadNotifications.Select(notification => notification.Id).ToList();
        results.Should().NotBeNullOrEmpty();
        results.Should().HaveCount(_unreadNotifications.Count);
        results.Should().AllBeOfType<NotificationDetailData>();
        results.Select(x => x.Id).Should().BeEquivalentTo(unreadNotificationIds);
    }

    [Fact]
    public async Task GetAllAsDetailsByUserIdUntracked_WithReadStatus_ReturnsExpectedNotificationDetailData()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut.GetAllNotificationDetailsByIamUserIdUntracked(IamUserId, true, null).ToListAsync();

        var readNotificationIds = _readNotifications.Select(notification => notification.Id).ToList();
        // Assert
        results.Should().NotBeNullOrEmpty();
        results.Should().HaveCount(_readNotifications.Count);
        results.Should().AllBeOfType<NotificationDetailData>();
        results.Select(x => x.Id).Should().BeEquivalentTo(readNotificationIds);
    }

    [Fact]
    public async Task GetAllAsDetailsByUserIdUntracked_WithReadStatusAndInfoType_ReturnsExpectedNotificationDetailData()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut.GetAllNotificationDetailsByIamUserIdUntracked(IamUserId, true, NotificationTypeId.INFO).ToListAsync();

        // Assert
        var readNotificationIds = _readNotifications
            .Where(x => x.NotificationTypeId == NotificationTypeId.INFO)
            .Select(x => x.Id)
            .ToList();
        results.Should().NotBeNullOrEmpty();
        results.Should().HaveCount(1);
        results.Should().AllBeOfType<NotificationDetailData>();
        results.Select(x => x.Id).Should().BeEquivalentTo(readNotificationIds);
    }

    [Fact]
    public async Task GetAllAsDetailsByUserIdUntracked_WithReadStatusAndActionType_ReturnsExpectedNotificationDetailData()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut.GetAllNotificationDetailsByIamUserIdUntracked(IamUserId, true, NotificationTypeId.ACTION).ToListAsync();

        // Assert
        var readNotificationIds = _readNotifications
            .Where(x => x.NotificationTypeId == NotificationTypeId.ACTION)
            .Select(x => x.Id).ToList();
        results.Should().NotBeNullOrEmpty();
        results.Should().HaveCount(2);
        results.Should().AllBeOfType<NotificationDetailData>();
        results.Select(x => x.Id).Should().BeEquivalentTo(readNotificationIds);
    }

    #endregion

    #region GetNotificationCount

    [Fact]
    public async Task GetNotificationCountAsync_WithReadStatus_ReturnsExpectedCount()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut.GetNotificationCountForIamUserAsync(IamUserId, true);

        // Assert
        results.Count.Should().Be(_readNotifications.Count);
    }

    [Fact]
    public async Task GetNotificationCountAsync_WithoutStatus_ReturnsExpectedCount()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut.GetNotificationCountForIamUserAsync(IamUserId, null);

        // Assert
        results.Count.Should().Be(_notifications.Count);
    }

    private async Task<NotificationRepository> CreateSut()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext(dbContext =>
            {
                dbContext.Notifications.RemoveRange(dbContext.Notifications.ToList());
            },
            SeedExtensions.SeedNotification(_notifications.ToArray())
        ).ConfigureAwait(false);
        _fixture.Inject(context);
        var sut = _fixture.Create<NotificationRepository>();
        return sut;
    }

    #endregion
}
