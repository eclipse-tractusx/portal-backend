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

using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.Framework.Notifications;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.Notification.Service.BusinessLogic;

/// <inheritdoc />
public class NotificationBusinessLogic : INotificationBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly INotificationService _notificationService;

    /// <summary>
    ///     Creates a new instance of <see cref="NotificationBusinessLogic" />
    /// </summary>
    /// <param name="portalRepositories">Access to the repository factory.</param>
    /// <param name="notificationService">Access to the notification service.</param>
    public NotificationBusinessLogic(IPortalRepositories portalRepositories, INotificationService notificationService)
    {
        _portalRepositories = portalRepositories;
        _notificationService = notificationService;
    }

    /// <inheritdoc />
    public async Task<NotificationDetailData> CreateNotificationAsync(string iamUserId,
        NotificationCreationData creationData, Guid receiverId) =>
        await this._notificationService
            .CreateNotificationAsync(iamUserId, creationData, receiverId)
            .ConfigureAwait(false);

    /// <inheritdoc />
    public IAsyncEnumerable<NotificationDetailData> GetNotificationsAsync(string iamUserId,
        bool? isRead, NotificationTypeId? typeId) =>
        _portalRepositories.GetInstance<INotificationRepository>()
            .GetAllNotificationDetailsByIamUserIdUntracked(iamUserId, isRead, typeId);

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
    public async Task SetNotificationStatusAsync(string iamUserId, Guid notificationId, bool isRead)
    {
        await CheckNotificationExistsAndIamUserIsReceiver(notificationId, iamUserId).ConfigureAwait(false);

        _portalRepositories.Attach(new PortalBackend.PortalEntities.Entities.Notification(notificationId), notification =>
        {
            notification.IsRead = isRead;
        });
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task DeleteNotificationAsync(string iamUserId, Guid notificationId)
    {
        await CheckNotificationExistsAndIamUserIsReceiver(notificationId, iamUserId).ConfigureAwait(false);

        _portalRepositories.Remove(new PortalBackend.PortalEntities.Entities.Notification(notificationId));
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
