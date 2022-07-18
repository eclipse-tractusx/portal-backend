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
    public Notification Add(Guid receiverUserId, string content, NotificationTypeId notificationTypeId,
        NotificationStatusId readStatusId, Action<Notification>? setOptionalParameter = null)
    {
        var notification = new Notification(Guid.NewGuid(), receiverUserId, DateTimeOffset.UtcNow, content,
            notificationTypeId, readStatusId);
        setOptionalParameter?.Invoke(notification);

        return _dbContext.Add(notification).Entity;
    }

    /// <inheritdoc />
    public IAsyncEnumerable<NotificationDetailData> GetAllAsDetailsByUserIdUntracked(Guid companyUserId,
        NotificationStatusId? statusId, NotificationTypeId? typeId)
    {
        var query = _dbContext.Notifications
            .Where(x =>
                x.ReceiverUserId == companyUserId);
        if (statusId.HasValue)
        {
            query = query.Where(x => x.ReadStatusId == statusId.Value);
        }

        if (typeId.HasValue)
        {
            query = query.Where(x => x.NotificationTypeId == typeId.Value);
        }

        return query
            .Select(x => new NotificationDetailData(x.Id, x.Content, x.DueDate, x.NotificationTypeId, x.ReadStatusId))
            .AsAsyncEnumerable();
    }

    /// <inheritdoc />
    public async Task<NotificationDetailData?> GetByIdAndUserIdUntrackedAsync(Guid notificationId, Guid companyUserId)
    {
        return await _dbContext.Notifications
            .Where(x => x.Id == notificationId && x.ReceiverUserId == companyUserId)
            .Select(x => new NotificationDetailData(x.Id, x.Content, x.DueDate, x.NotificationTypeId, x.ReadStatusId))
            .SingleOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<int> GetNotificationCountAsync(Guid companyUserId, NotificationStatusId? statusId) =>
        await _dbContext.Notifications.CountAsync(x =>
            x.ReceiverUserId == companyUserId &&
            statusId.HasValue ? x.ReadStatusId == statusId.Value : true);

    /// <inheritdoc />
    public async Task<bool> CheckExistsByIdAndUserIdAsync(Guid notificationId, Guid companyUserId) =>
        await _dbContext.Notifications
            .AnyAsync(x => x.Id == notificationId && x.ReceiverUserId == companyUserId);
}
