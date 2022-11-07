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
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Tests.Setup;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Extensions;
using Xunit;
using Xunit.Extensions.AssemblyFixture;

namespace Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Tests;

/// <summary>
/// Tests the functionality of the <see cref="NotificationRepository"/>
/// </summary>
public class NotificationRepositoryTests : IAssemblyFixture<TestDbFixture>
{
    private const string IamUserId = "3d8142f1-860b-48aa-8c2b-1ccb18699f65";
    private readonly TestDbFixture _dbTestDbFixture;

    public NotificationRepositoryTests(TestDbFixture testDbFixture)
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }

    #region GetNotificationTopic

    [Theory]
    [InlineData(NotificationTypeId.INFO, NotificationTopicId.INFO)]
    [InlineData(NotificationTypeId.TECHNICAL_USER_CREATION, NotificationTopicId.INFO)]
    [InlineData(NotificationTypeId.CONNECTOR_REGISTERED, NotificationTopicId.INFO)]
    [InlineData(NotificationTypeId.WELCOME_SERVICE_PROVIDER, NotificationTopicId.INFO)]
    [InlineData(NotificationTypeId.WELCOME_CONNECTOR_REGISTRATION, NotificationTopicId.INFO)]
    [InlineData(NotificationTypeId.WELCOME, NotificationTopicId.INFO)]
    [InlineData(NotificationTypeId.WELCOME_USE_CASES, NotificationTopicId.INFO)]
    [InlineData(NotificationTypeId.WELCOME_APP_MARKETPLACE, NotificationTopicId.INFO)]
    [InlineData(NotificationTypeId.ACTION, NotificationTopicId.ACTION)]
    [InlineData(NotificationTypeId.APP_SUBSCRIPTION_REQUEST, NotificationTopicId.ACTION)]
    [InlineData(NotificationTypeId.SERVICE_REQUEST, NotificationTopicId.ACTION)]
    [InlineData(NotificationTypeId.APP_SUBSCRIPTION_ACTIVATION, NotificationTopicId.OFFER)]
    [InlineData(NotificationTypeId.APP_RELEASE_REQUEST, NotificationTopicId.OFFER)]
    [InlineData(NotificationTypeId.SERVICE_ACTIVATION, NotificationTopicId.OFFER)]
    public void GetNotificationTopicId_GetsCorrectTopicId(NotificationTypeId type, NotificationTopicId expectedResult)
    {
        // Arrange
        
        // Act
        var topic = type.GetNotificationTopic();
        
        // Assert
        topic.Should().Be(expectedResult);
    }
    
    [Fact]
    public void GetNotificationTopicId_WithInvalidNotificationType_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var notificationTypeId = (NotificationTypeId)0;

        // Act
        try
        {
            notificationTypeId.GetNotificationTopic();
        }
        catch (ArgumentOutOfRangeException ex)
        {
            // Assert
            ex.ParamName.Should().Be("typeId");
            return;
        }

        // Should never been hit.
        true.Should().BeFalse();
    }

    #endregion
    
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
    public async Task GetAllAsDetailsByUserIdUntracked_WithUnreadStatus_ReturnsExpectedNotificationDetailData()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut
            .GetAllNotificationDetailsByIamUserIdUntracked(IamUserId, false, null)
            .ToListAsync()
            .ConfigureAwait(false);

        // Assert
        results.Should().NotBeNullOrEmpty();
        results.Should().HaveCount(3);
        results.Should().AllBeOfType<NotificationDetailData>();
    }

    [Fact]
    public async Task GetAllAsDetailsByUserIdUntracked_WithReadStatus_ReturnsExpectedNotificationDetailData()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut
            .GetAllNotificationDetailsByIamUserIdUntracked(IamUserId, true, null)
            .ToListAsync()
            .ConfigureAwait(false);

        // Assert
        results.Should().NotBeNullOrEmpty();
        results.Should().HaveCount(3);
        results.Should().AllBeOfType<NotificationDetailData>();
    }

    [Fact]
    public async Task GetAllAsDetailsByUserIdUntracked_WithReadStatusAndInfoType_ReturnsExpectedNotificationDetailData()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut.GetAllNotificationDetailsByIamUserIdUntracked(IamUserId, true, NotificationTypeId.INFO).ToListAsync();

        // Assert
        results.Should().NotBeNullOrEmpty();
        results.Should().ContainSingle();
        results.Should().AllBeOfType<NotificationDetailData>();
    }

    [Fact]
    public async Task GetAllAsDetailsByUserIdUntracked_WithReadStatusAndActionType_ReturnsExpectedNotificationDetailData()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut.GetAllNotificationDetailsByIamUserIdUntracked(IamUserId, true, NotificationTypeId.ACTION).ToListAsync();

        // Assert
        results.Should().NotBeNullOrEmpty();
        results.Should().HaveCount(2);
        results.Should().AllBeOfType<NotificationDetailData>();
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
            .GetNotificationByIdAndIamUserIdUntrackedAsync(new Guid("19AFFED7-13F0-4868-9A23-E77C23D8C889"), IamUserId)
            .ConfigureAwait(false);
        
        // Assert
        result.Should().NotBeNull();
        result.IsUserReceiver.Should().BeTrue();
        result.NotificationDetailData.IsRead.Should().BeFalse();
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
            .GetNotificationCountForIamUserAsync(IamUserId, true)
            .ConfigureAwait(false);

        // Assert
        results.Count.Should().Be(3);
    }

    [Fact]
    public async Task GetNotificationCountAsync_WithoutStatus_ReturnsExpectedCount()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var results = await sut
            .GetNotificationCountForIamUserAsync(IamUserId, null)
            .ConfigureAwait(false);

        // Assert
        results.Count.Should().Be(6);
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
            .GetCountDetailsForUserAsync(IamUserId)
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
            .CheckNotificationExistsByIdAndIamUserIdAsync(new Guid("19AFFED7-13F0-4868-9A23-E77C23D8C889"), IamUserId)
            .ConfigureAwait(false);

        // Assert
        results.IsUserReceiver.Should().BeTrue();
        results.IsNotificationExisting.Should().BeTrue();
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
