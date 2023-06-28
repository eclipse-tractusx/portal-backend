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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Notifications.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Notifications.Service.BusinessLogic;

/// <inheritdoc />
public class NotificationBusinessLogic : INotificationBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly NotificationSettings _settings;

    /// <summary>
    ///     Creates a new instance of <see cref="NotificationBusinessLogic" />
    /// </summary>
    /// <param name="portalRepositories">Access to the repository factory.</param>
    /// <param name="settings">Access to the notifications options</param>
    public NotificationBusinessLogic(IPortalRepositories portalRepositories, IOptions<NotificationSettings> settings)
    {
        _portalRepositories = portalRepositories;
        _settings = settings.Value;
    }

    /// <inheritdoc />
    public Task<Pagination.Response<NotificationDetailData>> GetNotificationsAsync(int page, int size, Guid receiverUserId, NotificationFilters filters) =>
        Pagination.CreateResponseAsync(page, size, _settings.MaxPageSize, _portalRepositories.GetInstance<INotificationRepository>()
                .GetAllNotificationDetailsByReceiver(receiverUserId, filters.IsRead, filters.TypeId, filters.TopicId, filters.OnlyDueDate, filters.Sorting ?? NotificationSorting.DateDesc, filters.DoneState));

    /// <inheritdoc />
    public async Task<NotificationDetailData> GetNotificationDetailDataAsync(Guid userId, Guid notificationId)
    {
        var result = await _portalRepositories.GetInstance<INotificationRepository>().GetNotificationByIdAndValidateReceiverAsync(notificationId, userId).ConfigureAwait(false);
        if (result == default)
        {
            throw new NotFoundException($"Notification {notificationId} does not exist.");
        }
        if (!result.IsUserReceiver)
        {
            throw new ForbiddenException("The user is not the receiver of the notification");
        }
        return result.NotificationDetailData;
    }

    /// <inheritdoc />
    public Task<int> GetNotificationCountAsync(Guid userId, bool? isRead) =>
        _portalRepositories.GetInstance<INotificationRepository>().GetNotificationCountForUserAsync(userId, isRead);

    /// <inheritdoc />
    public async Task<NotificationCountDetails> GetNotificationCountDetailsAsync(Guid userId)
    {
        var details = await _portalRepositories.GetInstance<INotificationRepository>().GetCountDetailsForUserAsync(userId).ToListAsync().ConfigureAwait(false);
        var unreadNotifications = details.Where(x => !x.IsRead);
        return new NotificationCountDetails(
            details.Where(x => x.IsRead).Sum(x => x.Count),
            unreadNotifications.Sum(x => x.Count),
            unreadNotifications.Where(x => x.NotificationTopicId == NotificationTopicId.INFO).Sum(x => x.Count),
            unreadNotifications.Where(x => x.NotificationTopicId == NotificationTopicId.OFFER).Sum(x => x.Count),
            details.Where(x => x is { NotificationTopicId: NotificationTopicId.ACTION, Done: null or false }).Sum(x => x.Count),
            unreadNotifications.Where(x => x is { NotificationTopicId: NotificationTopicId.ACTION }).Sum(x => x.Count));
    }

    /// <inheritdoc />
    public async Task SetNotificationStatusAsync(Guid userId, Guid notificationId, bool isRead)
    {
        await CheckNotificationExistsAndValidateReceiver(notificationId, userId).ConfigureAwait(false);

        _portalRepositories.GetInstance<INotificationRepository>().AttachAndModifyNotification(notificationId, notification =>
        {
            notification.IsRead = isRead;
        });
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task DeleteNotificationAsync(Guid userId, Guid notificationId)
    {
        await CheckNotificationExistsAndValidateReceiver(notificationId, userId).ConfigureAwait(false);

        _portalRepositories.GetInstance<INotificationRepository>().DeleteNotification(notificationId);
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    private async Task CheckNotificationExistsAndValidateReceiver(Guid notificationId, Guid userId)
    {
        var result = await _portalRepositories.GetInstance<INotificationRepository>().CheckNotificationExistsByIdAndValidateReceiverAsync(notificationId, userId).ConfigureAwait(false);
        if (result == default || !result.IsNotificationExisting)
        {
            throw new NotFoundException($"Notification {notificationId} does not exist.");
        }
        if (!result.IsUserReceiver)
        {
            throw new ForbiddenException("The user is not the receiver of the notification");
        }
    }
}
