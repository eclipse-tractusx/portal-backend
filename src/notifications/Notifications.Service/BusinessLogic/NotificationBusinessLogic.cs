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
    public async Task<Guid> CreateNotificationAsync(string iamUserId,
        NotificationCreationData creationData, Guid receiverId)
    {
        var users = await _portalRepositories.GetInstance<IUserRepository>().GetCompanyUserWithIamUserCheck(iamUserId, receiverId).ToListAsync().ConfigureAwait(false);

        if (users.All(x => x.CompanyUserId != receiverId))
            throw new ArgumentException("User does not exist", nameof(receiverId));

        var (content, notificationTypeId, notificationStatusId, dueDate) =
            creationData;

        var notification = _portalRepositories.GetInstance<INotificationRepository>().CreateNotification(
            receiverId,
            notificationTypeId,
            notificationStatusId,
            notification =>
            {
                notification.DueDate = dueDate;
                notification.CreatorUserId = users.Single(x => x.IsIamUser).CompanyUserId;
                notification.Content = content;
            });

        await _portalRepositories.SaveAsync().ConfigureAwait(false);
        return notification.Id;
    }

    /// <inheritdoc />
    public Task<Pagination.Response<NotificationDetailData>> GetNotificationsAsync(int page, int size, string iamUserId,
        bool? isRead, NotificationTypeId? typeId, NotificationTopicId? topicId,
        bool onlyDueDate,
        NotificationSorting? sorting) =>
        Pagination.CreateResponseAsync(page, size, _settings.MaxPageSize,
            _portalRepositories.GetInstance<INotificationRepository>()
                .GetAllNotificationDetailsByIamUserIdUntracked(iamUserId, isRead, typeId, topicId, onlyDueDate, sorting ?? NotificationSorting.DateDesc));

    /// <inheritdoc />
    public async Task<NotificationDetailData> GetNotificationDetailDataAsync(string iamUserId, Guid notificationId)
    {
        var result = await _portalRepositories.GetInstance<INotificationRepository>().GetNotificationByIdAndIamUserIdUntrackedAsync(notificationId, iamUserId).ConfigureAwait(false);
        if (result == default)
        {
            throw new NotFoundException($"Notification {notificationId} does not exist.");
        }
        if (!result.IsUserReceiver)
        {
            throw new ForbiddenException($"iamUserId {iamUserId} is not the receiver of the notification");
        }
        return result.NotificationDetailData;
    }

    /// <inheritdoc />
    public async Task<int> GetNotificationCountAsync(string iamUserId, bool? isRead)
    {
        var result = await _portalRepositories.GetInstance<INotificationRepository>().GetNotificationCountForIamUserAsync(iamUserId, isRead).ConfigureAwait(false);
        if (result == default)
        {
            throw new ForbiddenException($"iamUserId {iamUserId} is not assigned");
        }
        return result.Count;
    }

    /// <inheritdoc />
    public async Task<NotificationCountDetails> GetNotificationCountDetailsAsync(string iamUserId)
    {
        var details = await _portalRepositories.GetInstance<INotificationRepository>().GetCountDetailsForUserAsync(iamUserId).ToListAsync().ConfigureAwait(false);
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
    public async Task SetNotificationStatusAsync(string iamUserId, Guid notificationId, bool isRead)
    {
        await CheckNotificationExistsAndIamUserIsReceiver(notificationId, iamUserId).ConfigureAwait(false);

        _portalRepositories.GetInstance<INotificationRepository>().AttachAndModifyNotification(notificationId, notification =>
        {
            notification.IsRead = isRead;
        });
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task DeleteNotificationAsync(string iamUserId, Guid notificationId)
    {
        await CheckNotificationExistsAndIamUserIsReceiver(notificationId, iamUserId).ConfigureAwait(false);

        _portalRepositories.GetInstance<INotificationRepository>().DeleteNotification(notificationId);
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    private async Task CheckNotificationExistsAndIamUserIsReceiver(Guid notificationId, string iamUserId)
    {
        var result = await _portalRepositories.GetInstance<INotificationRepository>().CheckNotificationExistsByIdAndIamUserIdAsync(notificationId, iamUserId).ConfigureAwait(false);
        if (result == default || !result.IsNotificationExisting)
        {
            throw new NotFoundException($"Notification {notificationId} does not exist.");
        }
        if (!result.IsUserReceiver)
        {
            throw new ForbiddenException($"iamUserId {iamUserId} is not the receiver of the notification");
        }
    }
}
