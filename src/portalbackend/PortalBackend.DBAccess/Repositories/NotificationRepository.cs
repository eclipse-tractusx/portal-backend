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

using Microsoft.EntityFrameworkCore;
using Org.CatenaX.Ng.Portal.Backend.Framework.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Linq.Expressions;

namespace Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Repositories;

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
        bool isRead, Action<Notification>? setOptionalParameter = null)
    {
        var notification = new Notification(Guid.NewGuid(), receiverUserId, DateTimeOffset.UtcNow,
            notificationTypeId, isRead);
        setOptionalParameter?.Invoke(notification);

        return _dbContext.Add(notification).Entity;
    }

    public Notification AttachAndModifyNotification(Guid notificationId, Action<Notification>? setOptionalParameter = null)
    {
        var notification = _dbContext.Attach(new Notification(notificationId, Guid.Empty, default, default, default)).Entity;
        setOptionalParameter?.Invoke(notification);
        return notification;
    }

    public Notification DeleteNotification(Guid notificationId) =>
        _dbContext.Remove(new Notification(notificationId, Guid.Empty, default, default, default)).Entity;

    /// <inheritdoc />
    public Task<Pagination.Source<NotificationDetailData>?> GetAllNotificationDetailsByIamUserIdUntracked(string iamUserId, bool? isRead, NotificationTypeId? typeId, int skip, int take, NotificationSorting? sorting) =>
        Pagination.CreateSourceQueryAsync(
            skip,
            take,
            _dbContext.Notifications.AsNoTracking()
                .Where(notification =>
                    notification.Receiver!.IamUser!.UserEntityId == iamUserId
                    && (!isRead.HasValue || notification.IsRead == isRead.Value)
                    && (!typeId.HasValue || notification.NotificationTypeId == typeId.Value))
                .GroupBy(notification => notification.ReceiverUserId),
            sorting switch
            {
                NotificationSorting.DateAsc => (IEnumerable<Notification> notifications) => notifications.OrderBy(notification => notification.DateCreated),
                NotificationSorting.DateDesc => (IEnumerable<Notification> notifications) => notifications.OrderByDescending(notification => notification.DateCreated),
                NotificationSorting.ReadStatusAsc => (IEnumerable<Notification> notifications) => notifications.OrderBy(notification => notification.IsRead),
                NotificationSorting.ReadStatusDesc => (IEnumerable<Notification> notifications) => notifications.OrderByDescending(notification => notification.IsRead),
                _ => (Expression<Func<IEnumerable<Notification>,IOrderedEnumerable<Notification>>>?)null
            },
            notification => new NotificationDetailData(
                notification.Id,
                notification.DateCreated,
                notification.NotificationTypeId,
                notification.NotificationType!.NotificationTypeAssignedTopic!.NotificationTopicId,
                notification.IsRead,
                notification.Content,
                notification.DueDate))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<(bool IsUserReceiver, NotificationDetailData NotificationDetailData)> GetNotificationByIdAndIamUserIdUntrackedAsync(Guid notificationId, string iamUserId) =>
        _dbContext.Notifications
            .AsNoTracking()
            .Where(notification => notification.Id == notificationId)
            .Select(notification => new ValueTuple<bool, NotificationDetailData>(
                notification.Receiver!.IamUser!.UserEntityId == iamUserId,
                new NotificationDetailData(
                    notification.Id,
                    notification.DateCreated,
                    notification.NotificationTypeId,
                    notification.NotificationType!.NotificationTypeAssignedTopic!.NotificationTopicId,
                    notification.IsRead,
                    notification.Content,
                    notification.DueDate)))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<(bool IsUserExisting, int Count)> GetNotificationCountForIamUserAsync(string iamUserId, bool? isRead) =>
        _dbContext.CompanyUsers
            .AsNoTracking()
            .Where(companyUser => companyUser.IamUser!.UserEntityId == iamUserId)
            .Select(companyUser => new ValueTuple<bool, int>(
                true,
                companyUser.Notifications
                    .Count(notification => !isRead.HasValue || notification.IsRead == isRead.Value)))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public IAsyncEnumerable<(bool IsRead, NotificationTopicId NotificationTopicId, int Count)> GetCountDetailsForUserAsync(string iamUserId) =>
        _dbContext.Notifications
            .AsNoTracking()
            .Where(not => not.Receiver!.IamUser!.UserEntityId == iamUserId)
            .GroupBy(not => new { not.IsRead, not.NotificationType!.NotificationTypeAssignedTopic!.NotificationTopicId },
                (key, element) => new ValueTuple<bool,NotificationTopicId,int>(key.IsRead, key.NotificationTopicId, element.Count()))
            .AsAsyncEnumerable();

    /// <inheritdoc />
    public Task<(bool IsUserReceiver, bool IsNotificationExisting)> CheckNotificationExistsByIdAndIamUserIdAsync(Guid notificationId, string iamUserId) =>
        _dbContext.Notifications
            .AsNoTracking()
            .Where(notification => notification.Id == notificationId)
            .Select(notification => new ValueTuple<bool, bool>(
                notification.Receiver!.IamUser!.UserEntityId == iamUserId,
                true))
            .SingleOrDefaultAsync();
}
