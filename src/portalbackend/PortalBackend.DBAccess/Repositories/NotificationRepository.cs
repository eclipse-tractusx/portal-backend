/********************************************************************************
 * Copyright (c) 2022 BMW Group AG
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Framework.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

/// <inheritdoc />
public class NotificationRepository(PortalDbContext dbContext)
    : INotificationRepository
{
    /// <inheritdoc />
    public Notification CreateNotification(Guid receiverUserId, NotificationTypeId notificationTypeId,
        bool isRead, Action<Notification>? setOptionalParameters = null)
    {
        var notification = new Notification(Guid.NewGuid(), receiverUserId, DateTimeOffset.UtcNow,
            notificationTypeId, isRead);
        setOptionalParameters?.Invoke(notification);

        return dbContext.Add(notification).Entity;
    }

    public void AttachAndModifyNotification(Guid notificationId, Action<Notification>? initialize, Action<Notification> updateFields)
    {
        var notification = new Notification(notificationId, Guid.Empty, default, default, default);
        initialize?.Invoke(notification);
        dbContext.Attach(notification);
        updateFields.Invoke(notification);
    }

    public void AttachAndModifyNotifications(IEnumerable<Guid> notificationIds, Action<Notification> setOptionalParameters)
    {
        var notifications = notificationIds.Select(notificationId => new Notification(notificationId, Guid.Empty, default, default, default)).ToList();
        dbContext.AttachRange(notifications);
        notifications.ForEach(notification => setOptionalParameters.Invoke(notification));
    }

    public Notification DeleteNotification(Guid notificationId) =>
        dbContext.Remove(new Notification(notificationId, Guid.Empty, default, default, default)).Entity;

    /// <inheritdoc />
    public Func<int, int, Task<Pagination.Source<NotificationDetailData>?>> GetAllNotificationDetailsByReceiver(Guid receiverUserId, SearchSemanticTypeId semantic, bool? isRead, NotificationTypeId? typeId, NotificationTopicId? topicId, bool onlyDueDate, NotificationSorting? sorting, bool? doneState, IEnumerable<NotificationTypeId> searchTypeIds, string? searchQuery) =>
        (skip, take) => Pagination.CreateSourceQueryAsync(
            skip,
            take,
            dbContext.Notifications.AsNoTracking()
                .Where(notification =>
                    notification.ReceiverUserId == receiverUserId &&
                    (semantic == SearchSemanticTypeId.AND
                        ? ((!isRead.HasValue || notification.IsRead == isRead.Value) &&
                           (!typeId.HasValue || notification.NotificationTypeId == typeId.Value) &&
                           (!topicId.HasValue || notification.NotificationType!.NotificationTypeAssignedTopic!.NotificationTopicId == topicId.Value) &&
                           (!onlyDueDate || notification.DueDate.HasValue) &&
                           (!doneState.HasValue || notification.Done == doneState.Value) &&
                           (!searchTypeIds.Any() || searchTypeIds.Contains(notification.NotificationTypeId)) &&
                           (searchQuery == null || notification.Content != null && EF.Functions.ILike(notification.Content, $"%{searchQuery.EscapeForILike()}%")))
                        : ((isRead.HasValue && notification.IsRead == isRead.Value) ||
                           (typeId.HasValue && notification.NotificationTypeId == typeId.Value) ||
                           (topicId.HasValue && notification.NotificationType!.NotificationTypeAssignedTopic!.NotificationTopicId == topicId.Value) ||
                           (onlyDueDate && notification.DueDate.HasValue) ||
                           (doneState.HasValue && notification.Done == doneState.Value) ||
                           (searchTypeIds.Any() && searchTypeIds.Contains(notification.NotificationTypeId)) ||
                           (searchQuery != null && notification.Content != null && EF.Functions.ILike(notification.Content, $"%{searchQuery.EscapeForILike()}%")))))
                .GroupBy(notification => notification.ReceiverUserId),
            sorting switch
            {
                NotificationSorting.DateAsc => (IEnumerable<Notification> notifications) => notifications.OrderBy(notification => notification.DateCreated),
                NotificationSorting.DateDesc => (IEnumerable<Notification> notifications) => notifications.OrderByDescending(notification => notification.DateCreated),
                NotificationSorting.ReadStatusAsc => (IEnumerable<Notification> notifications) => notifications.OrderBy(notification => notification.IsRead),
                NotificationSorting.ReadStatusDesc => (IEnumerable<Notification> notifications) => notifications.OrderByDescending(notification => notification.IsRead),
                _ => null
            },
            notification => new NotificationDetailData(
                notification.Id,
                notification.DateCreated,
                notification.NotificationTypeId,
                notification.NotificationType!.NotificationTypeAssignedTopic!.NotificationTopicId,
                notification.IsRead,
                notification.Content,
                notification.DueDate,
                notification.Done))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<(bool IsUserReceiver, NotificationDetailData NotificationDetailData)> GetNotificationByIdAndValidateReceiverAsync(Guid notificationId, Guid companyUserId) =>
        dbContext.Notifications
            .AsNoTracking()
            .Where(notification => notification.Id == notificationId)
            .Select(notification => new ValueTuple<bool, NotificationDetailData>(
                notification.ReceiverUserId == companyUserId,
                new NotificationDetailData(
                    notification.Id,
                    notification.DateCreated,
                    notification.NotificationTypeId,
                    notification.NotificationType!.NotificationTypeAssignedTopic!.NotificationTopicId,
                    notification.IsRead,
                    notification.Content,
                    notification.DueDate,
                    notification.Done)))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<int> GetNotificationCountForUserAsync(Guid companyUserId, bool? isRead) =>
        dbContext.CompanyUsers
            .AsNoTracking()
            .Where(companyUser => companyUser.Id == companyUserId)
            .Select(companyUser => companyUser.Notifications.Count(notification => !isRead.HasValue || notification.IsRead == isRead.Value))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public IAsyncEnumerable<(bool IsRead, bool? Done, NotificationTopicId? NotificationTopicId, int Count)> GetCountDetailsForUserAsync(Guid companyUserId) =>
        dbContext.Notifications
            .AsNoTracking()
            .Where(not => not.ReceiverUserId == companyUserId)
            .GroupBy(not => new { not.IsRead, not.Done, not.NotificationType!.NotificationTypeAssignedTopic!.NotificationTopicId },
                (key, element) => new ValueTuple<bool, bool?, NotificationTopicId?, int>(key.IsRead, key.Done, key.NotificationTopicId, element.Count()))
            .AsAsyncEnumerable();

    /// <inheritdoc />
    public Task<bool> CheckNotificationExistsForParam(Guid receiverId, NotificationTypeId notificationTypeId, string searchParam, string searchValue) =>
        dbContext.Notifications
            .Where(n =>
                n.ReceiverUserId == receiverId &&
                n.NotificationTypeId == notificationTypeId &&
                EF.Functions.ILike(n.Content!, $"%\"{searchParam}\":\"{searchValue}\"%"))
            .AnyAsync();

    /// <inheritdoc />
    public IAsyncEnumerable<(NotificationTypeId NotificationTypeId, Guid ReceiverId)> CheckNotificationsExistsForParam(IEnumerable<Guid> receiverIds, IEnumerable<NotificationTypeId> notificationTypeIds, string searchParam, string searchValue) =>
        dbContext.Notifications
            .Where(n =>
                receiverIds.Contains(n.ReceiverUserId) &&
                notificationTypeIds.Contains(n.NotificationTypeId) &&
                EF.Functions.ILike(n.Content!, $"%\"{searchParam}\":\"{searchValue}\"%"))
            .Select(x => new ValueTuple<NotificationTypeId, Guid>(x.NotificationTypeId, x.ReceiverUserId))
            .ToAsyncEnumerable();

    /// <inheritdoc />
    public IAsyncEnumerable<Guid> GetNotificationUpdateIds(IEnumerable<Guid> userRoleIds, IEnumerable<Guid>? companyUserIds, IEnumerable<NotificationTypeId> notificationTypeIds, Guid offerId) =>
        dbContext.CompanyUsers
            .Where(x =>
                x.Identity!.UserStatusId == UserStatusId.ACTIVE &&
                (companyUserIds != null && companyUserIds.Any(cu => cu == x.Id)) || x.Identity!.IdentityAssignedRoles.Select(ur => ur.UserRole!).Any(ur => userRoleIds.Contains(ur.Id)))
            .SelectMany(x => x.Notifications
                .Where(n =>
                    notificationTypeIds.Contains(n.NotificationTypeId) &&
                    EF.Functions.ILike(n.Content!, $"%\"offerId\":\"{offerId}\"%")
                )
                .Select(n => n.Id))
            .ToAsyncEnumerable();

    /// <inheritdoc />
    public Task<(bool IsUserReceiver, bool IsNotificationExisting, bool isRead)> CheckNotificationExistsByIdAndValidateReceiverAsync(Guid notificationId, Guid companyUserId) =>
        dbContext.Notifications
            .AsNoTracking()
            .Where(notification => notification.Id == notificationId)
            .Select(notification => new ValueTuple<bool, bool, bool>(
                notification.ReceiverUserId == companyUserId,
                true,
                notification.IsRead))
            .SingleOrDefaultAsync();
}
