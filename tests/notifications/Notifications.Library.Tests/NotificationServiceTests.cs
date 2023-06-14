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
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Collections.Immutable;
using System.Text.Json;
using Xunit;

namespace Org.Eclipse.TractusX.Portal.Backend.Notifications.Library.Tests;

public class NotificationServiceTests
{
    private const string ClientId = "catenax-portal";
    private static readonly Guid UserId1 = new("857b93b1-8fcb-4141-81b0-ae81950d489e");
    private static readonly Guid UserId2 = new("857b93b1-8fcb-4141-81b0-ae81950d489f");
    private static readonly Guid UserId3 = new("857b93b1-8fcb-4141-81b0-ae81950d48af");
    private static readonly Guid CxAdminRoleId = new("607818be-4978-41f4-bf63-fa8d2de51155");
    private static readonly Guid CxAdminUserId = new("6bc51706-9a30-4eb9-9e60-77fdd6d9cd6f");
    private static readonly Guid UserRoleId = new("607818be-4978-41f4-bf63-fa8d2de51154");

    private readonly IFixture _fixture;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IUserRolesRepository _rolesRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly List<Notification> _notifications = new();
    private readonly INotificationService _sut;

    public NotificationServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _portalRepositories = A.Fake<IPortalRepositories>();
        _rolesRepository = A.Fake<IUserRolesRepository>();
        _userRepository = A.Fake<IUserRepository>();
        _notificationRepository = A.Fake<INotificationRepository>();

        A.CallTo(() => _portalRepositories.GetInstance<IUserRepository>()).Returns(_userRepository);
        A.CallTo(() => _portalRepositories.GetInstance<IUserRolesRepository>()).Returns(_rolesRepository);
        A.CallTo(() => _portalRepositories.GetInstance<INotificationRepository>()).Returns(_notificationRepository);

        SetupFakes();

        _sut = new NotificationService(_portalRepositories);
    }

    #region Create Notification

    [Fact]
    public async Task CreateNotifications_WithSingleUserRoleAndOneNotificationTypeId_CreatesOneNotification()
    {
        // Arrange
        var userRoles = new[]
        {
            new UserRoleConfig(ClientId, new []{ "CX Admin" })
        };
        var notificationContent = new
        {
            appId = "5cf74ef8-e0b7-4984-a872-474828beb5d2",
            companyName = "Shared Idp test"
        };
        var content = new (string?, NotificationTypeId)[]
        {
            new (JsonSerializer.Serialize(notificationContent), NotificationTypeId.INFO)
        };

        // Act
        var userIds = await _sut.CreateNotifications(userRoles, Guid.NewGuid(), content.AsEnumerable(), Guid.NewGuid()).ToListAsync().ConfigureAwait(false);

        // Assert
        _notifications.Should().HaveCount(1);
        _notifications.Single().NotificationTypeId.Should().Be(NotificationTypeId.INFO);
        userIds.Should().HaveCount(1).And.ContainSingle(x => x == new Guid("6bc51706-9a30-4eb9-9e60-77fdd6d9cd6f"));
    }

    [Fact]
    public async Task CreateNotifications_WithSingleUserRoleAndMultipleNotificationTypeId_CreatesFiveNotification()
    {
        // Arrange
        var userRoles = new[]
        {
            new UserRoleConfig(ClientId, new []{ "CX Admin" })
        };

        // Act
        var notificationContent = new
        {
            appId = "5cf74ef8-e0b7-4984-a872-474828beb5d2",
            companyName = "Shared Idp test"
        };
        var content = new (string?, NotificationTypeId)[]
        {
            new(JsonSerializer.Serialize(notificationContent), NotificationTypeId.WELCOME),
            new(JsonSerializer.Serialize(notificationContent), NotificationTypeId.WELCOME_USE_CASES),
            new(JsonSerializer.Serialize(notificationContent), NotificationTypeId.WELCOME_APP_MARKETPLACE),
            new(JsonSerializer.Serialize(notificationContent), NotificationTypeId.WELCOME_SERVICE_PROVIDER),
            new(JsonSerializer.Serialize(notificationContent), NotificationTypeId.WELCOME_CONNECTOR_REGISTRATION)
        };

        var userIds = await _sut.CreateNotifications(userRoles, Guid.NewGuid(), content.AsEnumerable(), Guid.NewGuid()).ToListAsync().ConfigureAwait(false);

        // Assert
        _notifications.Should().HaveCount(5);
        _notifications.Should().ContainSingle(x => x.NotificationTypeId == NotificationTypeId.WELCOME);
        _notifications.Should().ContainSingle(x => x.NotificationTypeId == NotificationTypeId.WELCOME_USE_CASES);
        _notifications.Should().ContainSingle(x => x.NotificationTypeId == NotificationTypeId.WELCOME_APP_MARKETPLACE);
        _notifications.Should().ContainSingle(x => x.NotificationTypeId == NotificationTypeId.WELCOME_SERVICE_PROVIDER);
        _notifications.Should().ContainSingle(x => x.NotificationTypeId == NotificationTypeId.WELCOME_CONNECTOR_REGISTRATION);
        userIds.Should().HaveCount(1).And.ContainSingle(x => x == new Guid("6bc51706-9a30-4eb9-9e60-77fdd6d9cd6f"));
    }

    [Fact]
    public async Task CreateNotifications_WithMultipleUserRoleAndOneNotificationTypeId_CreatesThreeNotification()
    {
        // Arrange
        var userRoles = new[]
        {
            new UserRoleConfig(ClientId, new []{ "Company Admin" })
        };
        var notificationContent = new
        {
            appId = "5cf74ef8-e0b7-4984-a872-474828beb5d2",
            companyName = "Shared Idp test"
        };
        var content = new (string?, NotificationTypeId)[]
        {
            new(JsonSerializer.Serialize(notificationContent), NotificationTypeId.INFO)
        };

        // Act
        var userIds = await _sut.CreateNotifications(userRoles, Guid.NewGuid(), content.AsEnumerable(), Guid.NewGuid()).ToListAsync().ConfigureAwait(false);

        // Assert
        _notifications.Should().HaveCount(3);
        _notifications.Should().ContainSingle(x => x.ReceiverUserId == UserId1);
        _notifications.Should().ContainSingle(x => x.ReceiverUserId == UserId2);
        _notifications.Should().ContainSingle(x => x.ReceiverUserId == UserId3);
        _notifications.Should().NotContain(x => x.ReceiverUserId == CxAdminUserId);
        userIds.Should().HaveCount(3).And
            .Contain(new Guid("857b93b1-8fcb-4141-81b0-ae81950d489e")).And
            .Contain(new Guid("857b93b1-8fcb-4141-81b0-ae81950d489f")).And
            .Contain(new Guid("857b93b1-8fcb-4141-81b0-ae81950d48af"));
    }

    [Fact]
    public async Task CreateNotifications_WithNotExistingRole_ThrowsException()
    {
        // Arrange
        var userRoles = new[]
        {
            new UserRoleConfig(ClientId, new []{ "Not Existing" })
        };
        var notificationContent = new
        {
            appId = "5cf74ef8-e0b7-4984-a872-474828beb5d2",
            companyName = "Shared Idp test"
        };
        var content = new (string?, NotificationTypeId)[]
        {
            new (JsonSerializer.Serialize(notificationContent), NotificationTypeId.INFO)
        };

        // Act
        async Task Action() => await _sut.CreateNotifications(userRoles, Guid.NewGuid(), content.AsEnumerable(), Guid.NewGuid()).ToListAsync().ConfigureAwait(false);

        // Assert
        await Assert.ThrowsAsync<ConfigurationException>(Action);
    }

    #endregion

    #region overload method

    [Fact]
    public async Task CreateNotificationsoverloadedmethod_WithSingleUserRoleAndOneNotificationTypeId_CreatesOneNotification()
    {
        // Arrange
        var userRoles = new[]
        {
            new UserRoleConfig(ClientId, new []{ "CX Admin" })
        };
        var notificationContent = new
        {
            appId = "5cf74ef8-e0b7-4984-a872-474828beb5d2",
            companyName = "Shared Idp test"
        };
        var content = new (string?, NotificationTypeId)[]
        {
            new (JsonSerializer.Serialize(notificationContent), NotificationTypeId.APP_RELEASE_REQUEST)
        };

        // Act
        await _sut.CreateNotifications(userRoles, Guid.NewGuid(), content.AsEnumerable()).ConfigureAwait(false);

        // Assert
        _notifications.Should().HaveCount(1);
        _notifications.Single().NotificationTypeId.Should().Be(NotificationTypeId.APP_RELEASE_REQUEST);
    }

    [Fact]
    public async Task CreateNotificationsoverloadedmethod_WithSingleUserRoleAndMultipleNotificationTypeId_CreatesFiveNotification()
    {
        // Arrange
        var userRoles = new[]
        {
            new UserRoleConfig(ClientId, new []{ "CX Admin" })
        };

        // Act
        var notificationContent = new
        {
            appId = "5cf74ef8-e0b7-4984-a872-474828beb5d2",
            companyName = "Shared Idp test"
        };
        var content = new (string?, NotificationTypeId)[]
        {
            new(JsonSerializer.Serialize(notificationContent), NotificationTypeId.WELCOME),
            new(JsonSerializer.Serialize(notificationContent), NotificationTypeId.WELCOME_USE_CASES),
            new(JsonSerializer.Serialize(notificationContent), NotificationTypeId.WELCOME_APP_MARKETPLACE),
            new(JsonSerializer.Serialize(notificationContent), NotificationTypeId.WELCOME_SERVICE_PROVIDER),
            new(JsonSerializer.Serialize(notificationContent), NotificationTypeId.WELCOME_CONNECTOR_REGISTRATION)
        };

        await _sut.CreateNotifications(userRoles, Guid.NewGuid(), content.AsEnumerable()).ConfigureAwait(false);

        // Assert
        _notifications.Should().HaveCount(5);
        _notifications.Should().ContainSingle(x => x.NotificationTypeId == NotificationTypeId.WELCOME);
        _notifications.Should().ContainSingle(x => x.NotificationTypeId == NotificationTypeId.WELCOME_USE_CASES);
        _notifications.Should().ContainSingle(x => x.NotificationTypeId == NotificationTypeId.WELCOME_APP_MARKETPLACE);
        _notifications.Should().ContainSingle(x => x.NotificationTypeId == NotificationTypeId.WELCOME_SERVICE_PROVIDER);
        _notifications.Should().ContainSingle(x => x.NotificationTypeId == NotificationTypeId.WELCOME_CONNECTOR_REGISTRATION);
    }

    [Fact]
    public async Task CreateNotificationsoverloadedmethod_WithMultipleUserRoleAndOneNotificationTypeId_CreatesThreeNotification()
    {
        // Arrange
        var userRoles = new[]
        {
            new UserRoleConfig(ClientId, new []{ "Company Admin" })
        };
        var notificationContent = new
        {
            appId = "5cf74ef8-e0b7-4984-a872-474828beb5d2",
            companyName = "Shared Idp test"
        };
        var content = new (string?, NotificationTypeId)[]
        {
            new(JsonSerializer.Serialize(notificationContent), NotificationTypeId.INFO)
        };

        // Act
        await _sut.CreateNotifications(userRoles, Guid.NewGuid(), content.AsEnumerable()).ConfigureAwait(false);

        // Assert
        _notifications.Should().HaveCount(3);
        _notifications.Should().ContainSingle(x => x.ReceiverUserId == UserId1);
        _notifications.Should().ContainSingle(x => x.ReceiverUserId == UserId2);
        _notifications.Should().ContainSingle(x => x.ReceiverUserId == UserId3);
        _notifications.Should().NotContain(x => x.ReceiverUserId == CxAdminUserId);
    }

    [Fact]
    public async Task CreateNotificationsoverloadedmethod_WithNotExistingRole_ThrowsException()
    {
        // Arrange
        var userRoles = new[]
        {
            new UserRoleConfig(ClientId, new []{ "Not Existing" })
        };
        var notificationContent = new
        {
            appId = "5cf74ef8-e0b7-4984-a872-474828beb5d2",
            companyName = "Shared Idp test"
        };
        var content = new (string?, NotificationTypeId)[]
        {
            new (JsonSerializer.Serialize(notificationContent), NotificationTypeId.INFO)
        };

        // Act
        async Task Action() => await _sut.CreateNotifications(userRoles, Guid.NewGuid(), content.AsEnumerable()).ConfigureAwait(false);

        // Assert
        await Assert.ThrowsAsync<ConfigurationException>(Action);
    }

    #endregion

    #region SetNotificationsForOfferToDone

    [Fact]
    public async Task SetNotificationsForOfferToDone_WithAdditionalUsersAndMultipleNotifications_UpdatesThreeNotification()
    {
        // Arrange
        var userIds = _fixture.CreateMany<Guid>(3).ToImmutableArray();
        var notifications = _fixture.CreateMany<Guid>(3).ToImmutableArray();

        var userRoles = new[]
        {
            new UserRoleConfig(ClientId, new []{ "Company Admin" })
        };
        var appId = new Guid("5cf74ef8-e0b7-4984-a872-474828beb5d2");
        A.CallTo(() => _notificationRepository.GetNotificationUpdateIds(A<IEnumerable<Guid>>._, A<IEnumerable<Guid>?>._, A<IEnumerable<NotificationTypeId>>._, A<Guid>._))
            .Returns(notifications.ToAsyncEnumerable());

        IEnumerable<Notification>? modified = null;

        A.CallTo(() => _notificationRepository.AttachAndModifyNotifications(A<IEnumerable<Guid>>._, A<Action<Notification>>._))
            .Invokes((IEnumerable<Guid> notificationIds, Action<Notification> setOptionalFields) =>
            {
                modified = notificationIds.Select(id => new Notification(id, Guid.Empty, default, default, false)).ToImmutableArray();
                foreach (var notification in modified)
                {
                    setOptionalFields.Invoke(notification);
                }
            });

        // Act
        await _sut.SetNotificationsForOfferToDone(userRoles, Enumerable.Repeat(NotificationTypeId.APP_RELEASE_REQUEST, 1), appId, userIds).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _notificationRepository.GetNotificationUpdateIds(
                A<IEnumerable<Guid>>.That.IsSameSequenceAs(new[] { UserRoleId }),
                A<IEnumerable<Guid>>.That.IsSameSequenceAs(userIds),
                A<IEnumerable<NotificationTypeId>>.That.IsSameSequenceAs(new[] { NotificationTypeId.APP_RELEASE_REQUEST }),
                appId))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _notificationRepository.AttachAndModifyNotifications(A<IEnumerable<Guid>>._, A<Action<Notification>>._)).MustHaveHappenedOnceExactly();
        modified.Should().NotBeNull()
            .And.HaveCount(3)
            .And.Satisfy(
                x => x.Id == notifications[0] && x.Done != null && x.Done.Value,
                x => x.Id == notifications[1] && x.Done != null && x.Done.Value,
                x => x.Id == notifications[2] && x.Done != null && x.Done.Value
            );
    }

    [Fact]
    public async Task SetNotificationsForOfferToDone_WithMultipleUserRoleAndOneNotificationTypeId_UpdatesAllNotification()
    {
        // Arrange
        var notifications = _fixture.CreateMany<Guid>(3).ToImmutableArray();
        var userRoles = new[]
        {
            new UserRoleConfig(ClientId, new []{ "Company Admin" })
        };
        var appId = new Guid("5cf74ef8-e0b7-4984-a872-474828beb5d2");
        A.CallTo(() => _notificationRepository.GetNotificationUpdateIds(A<IEnumerable<Guid>>._, A<IEnumerable<Guid>?>._, A<IEnumerable<NotificationTypeId>>._, A<Guid>._))
            .Returns(notifications.ToAsyncEnumerable());

        IEnumerable<Notification>? modified = null;

        A.CallTo(() => _notificationRepository.AttachAndModifyNotifications(A<IEnumerable<Guid>>._, A<Action<Notification>>._))
            .Invokes((IEnumerable<Guid> notificationIds, Action<Notification> setOptionalFields) =>
            {
                modified = notificationIds.Select(id => new Notification(id, Guid.Empty, default, default, false)).ToImmutableArray();
                foreach (var notification in modified)
                {
                    setOptionalFields.Invoke(notification);
                }
            });

        // Act
        await _sut.SetNotificationsForOfferToDone(userRoles, Enumerable.Repeat(NotificationTypeId.APP_RELEASE_REQUEST, 1), appId).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _notificationRepository.GetNotificationUpdateIds(
                A<IEnumerable<Guid>>.That.IsSameSequenceAs(new[] { UserRoleId }),
                (IEnumerable<Guid>?)null,
                A<IEnumerable<NotificationTypeId>>.That.IsSameSequenceAs(new[] { NotificationTypeId.APP_RELEASE_REQUEST }),
                appId))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _notificationRepository.AttachAndModifyNotifications(A<IEnumerable<Guid>>._, A<Action<Notification>>._)).MustHaveHappenedOnceExactly();
        modified.Should().NotBeNull()
            .And.HaveCount(3)
            .And.Satisfy(
                x => x.Id == notifications[0] && x.Done != null && x.Done.Value,
                x => x.Id == notifications[1] && x.Done != null && x.Done.Value,
                x => x.Id == notifications[2] && x.Done != null && x.Done.Value
            );
    }

    [Fact]
    public async Task SetNotificationsForOfferToDone_WithNotExistingRole_ThrowsException()
    {
        // Arrange
        var userRoles = new[]
        {
            new UserRoleConfig(ClientId, new []{ "Not Existing" })
        };
        var appId = new Guid("5cf74ef8-e0b7-4984-a872-474828beb5d2");

        // Act
        async Task Action() => await _sut.SetNotificationsForOfferToDone(userRoles, Enumerable.Repeat(NotificationTypeId.APP_RELEASE_REQUEST, 1), appId).ConfigureAwait(false);

        // Assert
        await Assert.ThrowsAsync<ConfigurationException>(Action);
    }

    #endregion

    #region Setup

    private void SetupFakes()
    {
        A.CallTo(() => _rolesRepository.GetUserRoleIdsUntrackedAsync(A<IEnumerable<UserRoleConfig>>.That.Matches(x => x.Single(y => y.ClientId == ClientId).UserRoleNames.First() == "Company Admin")))
            .Returns(new List<Guid> { UserRoleId }.ToAsyncEnumerable());
        A.CallTo(() => _rolesRepository.GetUserRoleIdsUntrackedAsync(A<IEnumerable<UserRoleConfig>>.That.Matches(x => x.Single(y => y.ClientId == ClientId).UserRoleNames.First() == "CX Admin")))
            .Returns(new List<Guid> { CxAdminRoleId }.ToAsyncEnumerable());
        A.CallTo(() => _rolesRepository.GetUserRoleIdsUntrackedAsync(A<IEnumerable<UserRoleConfig>>.That.Not.Matches(x => x.Single(y => y.ClientId == ClientId).UserRoleNames.First() == "CX Admin" || x.First(y => y.ClientId == ClientId).UserRoleNames.First() == "Company Admin")))
            .Returns(new List<Guid>().ToAsyncEnumerable());

        A.CallTo(() => _userRepository.GetCompanyUserWithRoleIdForCompany(A<List<Guid>>.That.Matches(x => x.Count == 1 && x.All(y => y == UserRoleId)), A<Guid>._))
            .Returns(new List<Guid> { UserId1, UserId2, UserId3 }.ToAsyncEnumerable());
        A.CallTo(() => _userRepository.GetCompanyUserWithRoleIdForCompany(A<List<Guid>>.That.Matches(x => x.Count == 1 && x.All(y => y == CxAdminRoleId)), A<Guid>._))
            .Returns(new List<Guid> { CxAdminUserId }.ToAsyncEnumerable());
        A.CallTo(() => _userRepository.GetCompanyUserWithRoleIdForCompany(A<List<Guid>>.That.Not.Matches(x => x.Contains(CxAdminRoleId) || x.Contains(UserRoleId)), A<Guid>._))
            .Returns(new List<Guid>().ToAsyncEnumerable());

        A.CallTo(() => _userRepository.GetCompanyUserWithRoleId(A<List<Guid>>.That.Matches(x => x.Count == 1 && x.All(y => y == UserRoleId))))
            .Returns(new List<Guid> { UserId1, UserId2, UserId3 }.ToAsyncEnumerable());
        A.CallTo(() => _userRepository.GetCompanyUserWithRoleId(A<List<Guid>>.That.Matches(x => x.Count == 1 && x.All(y => y == CxAdminRoleId))))
            .Returns(new List<Guid> { CxAdminUserId }.ToAsyncEnumerable());
        A.CallTo(() => _userRepository.GetCompanyUserWithRoleId(A<List<Guid>>.That.Not.Matches(x => x.Contains(CxAdminRoleId) || x.Contains(UserRoleId))))
            .Returns(new List<Guid>().ToAsyncEnumerable());

        A.CallTo(() => _notificationRepository.CreateNotification(A<Guid>._, A<NotificationTypeId>._, A<bool>._, A<Action<Notification>?>._))
            .Invokes((Guid receiverUserId, NotificationTypeId notificationTypeId, bool isRead, Action<Notification>? setOptionalParameters) =>
            {
                var notification = new Notification(Guid.NewGuid(), receiverUserId,
                    DateTimeOffset.UtcNow, notificationTypeId, isRead);
                setOptionalParameters?.Invoke(notification);
                _notifications.Add(notification);
            });

        A.CallTo(() => _portalRepositories.SaveAsync()).Returns(1);
    }

    #endregion
}
