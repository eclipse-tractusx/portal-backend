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
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.Notification.Service.BusinessLogic;

/// <inheritdoc />
public class NotificationBusinessLogic : INotificationBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;

    /// <summary>
    ///     Creates a new instance of <see cref="NotificationBusinessLogic" />
    /// </summary>
    /// <param name="portalRepositories">Access to the repository factory.</param>
    public NotificationBusinessLogic(IPortalRepositories portalRepositories)
    {
        _portalRepositories = portalRepositories;
    }

    /// <inheritdoc />
    public async Task<NotificationDetailData> CreateNotificationAsync(string iamUserId,
        NotificationCreationData creationData, Guid companyUserId)
    {
        var users = await _portalRepositories.GetInstance<IUserRepository>().GetCompanyUserWithIamUserCheck(iamUserId, companyUserId).ToListAsync().ConfigureAwait(false);

        if (users.All(x => x.CompanyUserId != companyUserId))
            throw new ArgumentException("User does not exist", nameof(companyUserId));

        var (content, notificationTypeId, notificationStatusId, dueDate) =
            creationData;

        var notification = _portalRepositories.GetInstance<INotificationRepository>().Create(
            companyUserId,
            notificationTypeId,
            notificationStatusId,
            notification => 
            {
                notification.DueDate = dueDate;
                notification.CreatorUserId = users.Single(x => x.iamUser).CompanyUserId;
                notification.Content = content;
            });

        await _portalRepositories.SaveAsync().ConfigureAwait(false);
        return new NotificationDetailData(notification.Id, content, dueDate, notificationTypeId, notificationStatusId);
    }

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
        var result = await _portalRepositories.GetInstance<INotificationRepository>().CheckNotificationExistsByIdAndIamUserIdAsync(notificationId, iamUserId).ConfigureAwait(false);
        if (result == default || !result.IsNotificationExisting)
        {
            throw new NotFoundException($"Notification {notificationId} does not exist.");
        }
        if (!result.IsUserReceiver)
        {
            throw new ForbiddenException($"iamUserId {iamUserId} is not the receiver of the notification");
        }
        var notification =_portalRepositories.Attach(new PortalBackend.PortalEntities.Entities.Notification(notificationId));
        notification.IsRead = isRead;
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task DeleteNotificationAsync(string iamUserId, Guid notificationId)
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

        var notification = new PortalBackend.PortalEntities.Entities.Notification(notificationId);
        _portalRepositories.Remove(notification);
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }
}
