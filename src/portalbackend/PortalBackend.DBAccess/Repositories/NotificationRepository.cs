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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Linq.Expressions;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

/// <inheritdoc />
public class NotificationRepository : INotificationRepository
{
    private readonly PortalDbContext _dbContext;

    /// <summary>
    ///     Creates a new instance of <see cref="NotificationRepository" />
    /// </summary>
    /// <param name="dbContext">Access to the database</param>
    public NotificationRepository(PortalDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public Notification CreateNotification(Guid receiverUserId, NotificationTypeId notificationTypeId,
        bool isRead, Action<Notification>? setOptionalParameters = null)
    {
        var notification = new Notification(Guid.NewGuid(), receiverUserId, DateTimeOffset.UtcNow,
            notificationTypeId, isRead);
        setOptionalParameters?.Invoke(notification);

        return _dbContext.Add(notification).Entity;
    }

    public void AttachAndModifyNotification(Guid notificationId, Action<Notification> setOptionalParameters)
    {
        var notification = _dbContext.Attach(new Notification(notificationId, Guid.Empty, default, default, default)).Entity;
        setOptionalParameters.Invoke(notification);
    }

    public void AttachAndModifyNotifications(IEnumerable<Guid> notificationIds, Action<Notification> setOptionalParameters)
    {
        var notifications = notificationIds.Select(notificationId => new Notification(notificationId, Guid.Empty, default, default, default)).ToList();
        _dbContext.AttachRange(notifications);
        notifications.ForEach(notification => setOptionalParameters.Invoke(notification));
    }

    public Notification DeleteNotification(Guid notificationId) =>
        _dbContext.Remove(new Notification(notificationId, Guid.Empty, default, default, default)).Entity;

    /// <inheritdoc />
    public Func<int, int, Task<Pagination.Source<NotificationDetailData>?>> GetAllNotificationDetailsByReceiver(Guid receiverUserId, bool? isRead, NotificationTypeId? typeId, NotificationTopicId? topicId, bool onlyDueDate, NotificationSorting? sorting) =>
        (skip, take) => Pagination.CreateSourceQueryAsync(
            skip,
            take,
            _dbContext.Notifications.AsNoTracking()
                .Where(notification =>
                    notification.ReceiverUserId == receiverUserId &&
                    (!isRead.HasValue || notification.IsRead == isRead.Value) &&
                    (!typeId.HasValue || notification.NotificationTypeId == typeId.Value) &&
                    (!topicId.HasValue || notification.NotificationType!.NotificationTypeAssignedTopic!.NotificationTopicId == topicId.Value) &&
                    (!onlyDueDate || notification.DueDate.HasValue))
                .GroupBy(notification => notification.ReceiverUserId),
            sorting switch
            {
                NotificationSorting.DateAsc => (IEnumerable<Notification> notifications) => notifications.OrderBy(notification => notification.DateCreated),
                NotificationSorting.DateDesc => (IEnumerable<Notification> notifications) => notifications.OrderByDescending(notification => notification.DateCreated),
                NotificationSorting.ReadStatusAsc => (IEnumerable<Notification> notifications) => notifications.OrderBy(notification => notification.IsRead),
                NotificationSorting.ReadStatusDesc => (IEnumerable<Notification> notifications) => notifications.OrderByDescending(notification => notification.IsRead),
                _ => (Expression<Func<IEnumerable<Notification>, IOrderedEnumerable<Notification>>>?)null
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
        _dbContext.Notifications
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
        _dbContext.CompanyUsers
            .AsNoTracking()
            .Where(companyUser => companyUser.Id == companyUserId)
            .Select(companyUser => companyUser.Notifications.Count(notification => !isRead.HasValue || notification.IsRead == isRead.Value))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public IAsyncEnumerable<(bool IsRead, bool? Done, NotificationTopicId NotificationTopicId, int Count)> GetCountDetailsForUserAsync(Guid companyUserId) =>
        _dbContext.Notifications
            .AsNoTracking()
            .Where(not => not.ReceiverUserId == companyUserId)
            .GroupBy(not => new { not.IsRead, not.Done, not.NotificationType!.NotificationTypeAssignedTopic!.NotificationTopicId },
                (key, element) => new ValueTuple<bool, bool?, NotificationTopicId, int>(key.IsRead, key.Done, key.NotificationTopicId, element.Count()))
            .AsAsyncEnumerable();

    /// <inheritdoc />
    public IAsyncEnumerable<Guid> GetNotificationUpdateIds(IEnumerable<Guid> userRoleIds, IEnumerable<Guid>? companyUserIds, IEnumerable<NotificationTypeId> notificationTypeIds, Guid offerId) =>
        _dbContext.CompanyUsers
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
    public Task<(bool IsUserReceiver, bool IsNotificationExisting)> CheckNotificationExistsByIdAndValidateReceiverAsync(Guid notificationId, Guid companyUserId) =>
        _dbContext.Notifications
            .AsNoTracking()
            .Where(notification => notification.Id == notificationId)
            .Select(notification => new ValueTuple<bool, bool>(
                notification.ReceiverUserId == companyUserId,
                true))
            .SingleOrDefaultAsync();
}
