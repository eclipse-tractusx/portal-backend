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

using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using Microsoft.EntityFrameworkCore;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

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
    public Notification Create(Guid receiverUserId, NotificationTypeId notificationTypeId,
        bool isRead, Action<Notification>? setOptionalParameter = null)
    {
        var notification = new Notification(Guid.NewGuid(), receiverUserId, DateTimeOffset.UtcNow,
            notificationTypeId, isRead);
        setOptionalParameter?.Invoke(notification);

        return _dbContext.Add(notification).Entity;
    }

    /// <inheritdoc />
    public IAsyncEnumerable<NotificationDetailData> GetAllNotificationDetailsByIamUserIdUntracked(string iamUserId, bool? isRead, NotificationTypeId? typeId) =>
        _dbContext.Notifications
            .AsNoTracking()
            .Where(notification =>
                (notification.Receiver!.IamUser!.UserEntityId == iamUserId)
                && (isRead.HasValue ? notification.IsRead == isRead.Value : true)
                && (typeId.HasValue ? notification.NotificationTypeId == typeId.Value : true))
            .Select(notification => new NotificationDetailData(
                notification.Id,
                notification.DateCreated,
                notification.NotificationTypeId,
                notification.IsRead,
                notification.Content,
                notification.DueDate))
            .ToAsyncEnumerable();

    /// <inheritdoc />
    public Task<(bool IsUserReceiver, NotificationDetailData NotificationDetailData)> GetNotificationByIdAndIamUserIdUntrackedAsync(Guid notificationId, string iamUserId) =>
        _dbContext.Notifications
            .AsNoTracking()
            .Where(notification => notification.Id == notificationId)
            .Select(notification => ((bool IsUserReceiver, NotificationDetailData NotificationDetailData)) new (
                notification.Receiver!.IamUser!.UserEntityId == iamUserId,
                new NotificationDetailData(
                    notification.Id,
                    notification.DateCreated,
                    notification.NotificationTypeId,
                    notification.IsRead,
                    notification.Content,
                    notification.DueDate)))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<(bool IsUserExisting, int Count)> GetNotificationCountForIamUserAsync(string iamUserId, bool? isRead) =>
        _dbContext.CompanyUsers
            .AsNoTracking()
            .Where(companyUser => companyUser.IamUser!.UserEntityId == iamUserId)
            .Select(companyUser => ((bool IsUserExisting, int Count)) new (
                true,
                companyUser.Notifications
                    .Count(notification => isRead.HasValue ? notification.IsRead == isRead.Value : true)))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<(bool IsUserReceiver, bool IsNotificationExisting)> CheckNotificationExistsByIdAndIamUserIdAsync(Guid notificationId, string iamUserId) =>
        _dbContext.Notifications
            .AsNoTracking()
            .Where(notification => notification.Id == notificationId)
            .Select(notification => ((bool IsUserReceiver, bool IsNotificationExisting)) new (
                notification.Receiver!.IamUser!.UserEntityId == iamUserId,
                true))
            .SingleOrDefaultAsync();
}
