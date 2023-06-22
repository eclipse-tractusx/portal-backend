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

using Microsoft.EntityFrameworkCore;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests.Setup;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Collections.Immutable;
using Xunit.Extensions.AssemblyFixture;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests;

/// <summary>
/// Tests the functionality of the <see cref="NotificationRepository"/>
/// </summary>
public class NotificationRepositoryTests : IAssemblyFixture<TestDbFixture>
{
    private readonly Guid _companyUserId = new("ac1cf001-7fbc-1f2f-817f-bce058020001");
    private readonly TestDbFixture _dbTestDbFixture;
    private readonly IFixture _fixture;

    public NotificationRepositoryTests(TestDbFixture testDbFixture)
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));

        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }

    #region Create Notification

    [Fact]
    public async Task CreateNotification_ReturnsExpectedResult()
    {
        // Arrange
        var notificationDueDate = DateTimeOffset.Now;
        var (sut, context) = await CreateSutWithContext().ConfigureAwait(false);

        // Act
        var results = sut.CreateNotification(Guid.NewGuid(), NotificationTypeId.INFO, false, notification =>
        {
            notification.DueDate = notificationDueDate;
        });

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        results.DueDate.Should().Be(notificationDueDate);
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        changedEntries.Single().Entity.Should().BeOfType<Notification>().Which.DueDate.Should().Be(notificationDueDate);
    }

    #endregion

    #region AttachAndModifyNotification

    [Fact]
    public async Task AttachAndModifyNotification_WithExistingNotification_UpdatesStatus()
    {
        // Arrange
        var (sut, dbContext) = await CreateSutWithContext().ConfigureAwait(false);

        // Act
        sut.AttachAndModifyNotification(new Guid("19AFFED7-13F0-4868-9A23-E77C23D8C889"), notification =>
        {
            notification.IsRead = true;
        });

        // Assert
        var changeTracker = dbContext.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        var changedEntity = changedEntries.Single();
        changedEntity.State.Should().Be(EntityState.Modified);
        changedEntity.Entity.Should().BeOfType<Notification>().Which.IsRead.Should().Be(true);
    }

    #endregion

    #region AttachAndModifyNotifications

    [Fact]
    public async Task AttachAndModifyNotifications_WithExistingNotification_UpdatesStatus()
    {
        // Arrange
        var (sut, dbContext) = await CreateSutWithContext().ConfigureAwait(false);
        var notificationIds = _fixture.CreateMany<Guid>().ToImmutableArray();

        // Act
        sut.AttachAndModifyNotifications(notificationIds, notification =>
        {
            notification.IsRead = true;
        });

        // Assert
        var changeTracker = dbContext.ChangeTracker;
        changeTracker.HasChanges().Should().BeTrue();
        changeTracker.Entries()
            .Should().HaveSameCount(notificationIds)
            .And.AllSatisfy(x => x.State.Should().Be(EntityState.Modified));
        changeTracker.Entries().Select(entry => entry.Entity)
            .Should().HaveSameCount(notificationIds)
            .And.AllBeOfType<Notification>()
            .And.AllSatisfy(x => ((Notification)x).IsRead.Should().BeTrue());
    }

    #endregion

    #region Delete Notification

    [Fact]
    public async Task DeleteNotification_WithExistingNotification_RemovesNotification()
    {
        // Arrange
        var (sut, dbContext) = await CreateSutWithContext().ConfigureAwait(false);

        // Act
        sut.DeleteNotification(new Guid("19AFFED7-13F0-4868-9A23-E77C23D8C889"));

        // Assert
        var changeTracker = dbContext.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        var changedEntity = changedEntries.Single();
        changedEntity.State.Should().Be(EntityState.Deleted);
    }

    #endregion

    #region GetAllAsDetailsByUserIdUntracked

    [Fact]
    public async Task GetAllAsDetailsByUserIdUntracked_ReturnsExpectedNotificationDetailData()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut.GetAllNotificationDetailsByReceiver(_companyUserId, null, null, null, false, null)(0, 15).ConfigureAwait(false);

        // Assert
        results.Should().NotBeNull();
        results!.Count.Should().Be(6);
        results.Data.Count().Should().Be(6);
        results.Data.Should().AllBeOfType<NotificationDetailData>();
    }

    [Fact]
    public async Task GetAllAsDetailsByUserIdUntracked_SortedByDateAsc_ReturnsExpectedNotificationDetailData()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut.GetAllNotificationDetailsByReceiver(_companyUserId, null, null, null, false, NotificationSorting.DateAsc)(0, 15).ConfigureAwait(false);

        // Assert
        results.Should().NotBeNull();
        results!.Data.Count().Should().Be(6);
        results.Data.Should().BeInAscendingOrder(detailData => detailData.Created);
    }

    [Fact]
    public async Task GetAllAsDetailsByUserIdUntracked_SortedByDateDesc_ReturnsExpectedNotificationDetailData()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut.GetAllNotificationDetailsByReceiver(_companyUserId, null, null, null, false, NotificationSorting.DateDesc)(0, 15).ConfigureAwait(false);

        // Assert
        results.Should().NotBeNull();
        results!.Data.Count().Should().Be(6);
        results.Data.Should().BeInDescendingOrder(detailData => detailData.Created);
    }

    [Fact]
    public async Task GetAllAsDetailsByUserIdUntracked_SortedByReadStatusAsc_ReturnsExpectedNotificationDetailData()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut.GetAllNotificationDetailsByReceiver(_companyUserId, null, null, null, false, NotificationSorting.ReadStatusAsc)(0, 15).ConfigureAwait(false);

        // Assert
        results.Should().NotBeNull();
        results!.Data.Count().Should().Be(6);
        results.Data.Should().BeInAscendingOrder(detailData => detailData.IsRead);
    }

    [Fact]
    public async Task GetAllAsDetailsByUserIdUntracked_SortedByReadStatusDesc_ReturnsExpectedNotificationDetailData()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut.GetAllNotificationDetailsByReceiver(_companyUserId, null, null, null, false, NotificationSorting.ReadStatusDesc)(0, 15).ConfigureAwait(false);

        // Assert
        results.Should().NotBeNull();
        results!.Data.Count().Should().Be(6);
        results.Data.Should().BeInDescendingOrder(detailData => detailData.IsRead);
    }

    [Fact]
    public async Task GetAllAsDetailsByUserIdUntracked_WithUnreadStatus_ReturnsExpectedNotificationDetailData()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut
            .GetAllNotificationDetailsByReceiver(_companyUserId, false, null, null, false, null)(0, 15)
            .ConfigureAwait(false);

        // Assert
        results.Should().NotBeNull();
        results!.Data.Count().Should().Be(3);
        results.Data.Should().AllSatisfy(detailData => detailData.Should().Match<NotificationDetailData>(x => x.IsRead == false));
    }

    [Fact]
    public async Task GetAllAsDetailsByUserIdUntracked_WithReadStatus_ReturnsExpectedNotificationDetailData()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut
            .GetAllNotificationDetailsByReceiver(_companyUserId, true, null, null, false, null)(0, 15)
            .ConfigureAwait(false);

        // Assert
        results.Should().NotBeNull();
        results!.Data.Count().Should().Be(3);
        results.Data.Should().AllSatisfy(detailData => detailData.Should().Match<NotificationDetailData>(x => x.IsRead == true));
    }

    [Fact]
    public async Task GetAllAsDetailsByUserIdUntracked_WithReadStatusAndInfoType_ReturnsExpectedNotificationDetailData()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut.GetAllNotificationDetailsByReceiver(_companyUserId, true, NotificationTypeId.INFO, null, false, NotificationSorting.ReadStatusDesc)(0, 15).ConfigureAwait(false);

        // Assert
        results.Should().NotBeNull();
        results!.Data.Count().Should().Be(1);
        results.Data.Should().AllSatisfy(detailData => detailData.Should().Match<NotificationDetailData>(x => x.IsRead == true && x.TypeId == NotificationTypeId.INFO));
    }

    [Fact]
    public async Task GetAllAsDetailsByUserIdUntracked_WithReadStatusAndActionType_ReturnsExpectedNotificationDetailData()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut.GetAllNotificationDetailsByReceiver(_companyUserId, true, NotificationTypeId.ACTION, null, false, NotificationSorting.ReadStatusAsc)(0, 15).ConfigureAwait(false);

        // Assert
        results.Should().NotBeNull();
        results!.Data.Count().Should().Be(1);
        results.Data.Should().AllSatisfy(detailData => detailData.Should().Match<NotificationDetailData>(x => x.IsRead == true && x.TypeId == NotificationTypeId.ACTION));
    }

    [Fact]
    public async Task GetAllAsDetailsByUserIdUntracked_WithTopic_ReturnsExpectedNotificationDetailData()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut.GetAllNotificationDetailsByReceiver(_companyUserId, null, null, NotificationTopicId.INFO, false, NotificationSorting.ReadStatusAsc)(0, 15).ConfigureAwait(false);

        // Assert
        results.Should().NotBeNull();
        results!.Data.Count().Should().Be(4);
        results.Data.Should().AllSatisfy(detailData => detailData.Should().Match<NotificationDetailData>(x => x.NotificationTopic == NotificationTopicId.INFO));
    }

    #endregion

    #region GetNotificationByIdAndIamUserId

    [Fact]
    public async Task GetNotificationByIdAndIamUserId_ForExistingUser_GetsExpectedNotification()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut
            .GetNotificationByIdAndValidateReceiverAsync(new Guid("500E4D2C-9919-4CA8-B75B-D523FBC99259"), _companyUserId)
            .ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.IsUserReceiver.Should().BeTrue();
        result.NotificationDetailData.IsRead.Should().BeTrue();
    }

    #endregion

    #region GetNotificationCount

    [Fact]
    public async Task GetNotificationCountAsync_WithReadStatus_ReturnsExpectedCount()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut
            .GetNotificationCountForUserAsync(_companyUserId, true)
            .ConfigureAwait(false);

        // Assert
        results.Should().Be(3);
    }

    [Fact]
    public async Task GetNotificationCountAsync_WithoutStatus_ReturnsExpectedCount()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut
            .GetNotificationCountForUserAsync(_companyUserId, null)
            .ConfigureAwait(false);

        // Assert
        results.Should().Be(6);
    }

    #endregion

    #region GetCountDetailsForUser

    [Fact]
    public async Task GetCountDetailsForUser_ReturnsExpectedCount()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut
            .GetCountDetailsForUserAsync(_companyUserId).ToListAsync()
            .ConfigureAwait(false);

        // Assert
        results.Count.Should().Be(4);
    }

    #endregion

    #region CheckNotificationExistsByIdAndIamUserId

    [Fact]
    public async Task CheckNotificationExistsByIdAndIamUserId_ReturnsExpectedCount()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut
            .CheckNotificationExistsByIdAndValidateReceiverAsync(new Guid("500E4D2C-9919-4CA8-B75B-D523FBC99259"), _companyUserId)
            .ConfigureAwait(false);

        // Assert
        results.IsUserReceiver.Should().BeTrue();
        results.IsNotificationExisting.Should().BeTrue();
    }

    #endregion

    #region GetUpdateData

    [Theory]
    [InlineData(new[] { "efc20368-9e82-46ff-b88f-6495b9810253" }, null)]
    [InlineData(new[] { "efc20368-9e82-46ff-b88f-6495b9810253" }, new[] { "ac1cf001-7fbc-1f2f-817f-bce058020001" })]
    [InlineData(new string[] { }, new[] { "ac1cf001-7fbc-1f2f-817f-bce058020001" })]
    public async Task GetUpdateData_ReturnsExpectedCount(IEnumerable<string> roleIds, IEnumerable<string>? userIds)
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut
            .GetNotificationUpdateIds(roleIds.Select(x => new Guid(x)), userIds == null ? null : userIds.Select(x => new Guid(x)), new[] { NotificationTypeId.APP_RELEASE_REQUEST }, new Guid("0fc768e5-d4cf-4d3d-a0db-379efedd60f5"))
            .ToListAsync()
            .ConfigureAwait(false);

        // Assert
        results.Should().HaveCount(1)
            .And.Contain(new Guid("1836bbf6-b067-4126-9745-a22a098d3486"));
    }

    #endregion

    #region CheckNotificationsExistsForParam

    [Fact]
    public async Task CheckNotificationsExistsForParam_WithCorrectSearch_ReturnsTrue()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut
            .CheckNotificationExistsForParam(new("ac1cf001-7fbc-1f2f-817f-bce058020001"), NotificationTypeId.APP_RELEASE_REQUEST, "offerId", "0fc768e5-d4cf-4d3d-a0db-379efedd60f5")
            .ConfigureAwait(false);

        // Assert
        results.Should().BeTrue();
    }

    [Fact]
    public async Task CheckNotificationsExistsForParam_WithWrongOfferId_ReturnsFalse()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut
            .CheckNotificationExistsForParam(new("ac1cf001-7fbc-1f2f-817f-bce058020001"), NotificationTypeId.APP_RELEASE_REQUEST, "offerId", "0fc768e5-d4cf-4d3d-a0db-379efedd6123")
            .ConfigureAwait(false);

        // Assert
        results.Should().BeFalse();
    }

    [Fact]
    public async Task CheckNotificationsExistsForParam_WithMatchingReceiver_ReturnsReceiver()
    {
        // Arrange
        var receiver = new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001");
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut
            .CheckNotificationsExistsForParam(new[] { receiver }, new[] { NotificationTypeId.APP_RELEASE_REQUEST }, "offerId", "0fc768e5-d4cf-4d3d-a0db-379efedd60f5")
            .ToListAsync()
            .ConfigureAwait(false);

        // Assert
        results.Should().ContainSingle();
        results.Single().ReceiverId.Should().Be(receiver);
    }

    #endregion

    #region Setup

    private async Task<NotificationRepository> CreateSut()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
        var sut = new NotificationRepository(context);
        return sut;
    }

    private async Task<(NotificationRepository repo, PortalDbContext context)> CreateSutWithContext()
    {
        var context = await _dbTestDbFixture.GetPortalDbContext().ConfigureAwait(false);
        var sut = new NotificationRepository(context);
        return (sut, context);
    }

    #endregion
}
