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
    public async Task<NotificationDetailData> CreateNotification(NotificationCreationData creationData, Guid companyUserId)
    {
        if (!await _portalRepositories.GetInstance<IUserRepository>().IsUserWithIdExisting(companyUserId))
            throw new ArgumentException("User does not exist", nameof(companyUserId));

        var notificationId = Guid.NewGuid();
        var (content, notificationTypeId, notificationStatusId, dueData, creatorUserId) =
            creationData;

        var notification = _portalRepositories.GetInstance<INotificationRepository>().Add(companyUserId, content, notificationTypeId, notificationStatusId);
        notification.DueDate = dueData;
        notification.CreatorUserId = creatorUserId;

        await _portalRepositories.SaveAsync().ConfigureAwait(false);
        return new NotificationDetailData(notificationId, content, dueData, notificationTypeId, notificationStatusId);
    }

    /// <inheritdoc />
    public async Task<IAsyncEnumerable<NotificationDetailData>> GetNotifications(string iamUserId,
        NotificationStatusId? statusId, NotificationTypeId? typeId)
    {
        var companyUserId = await _portalRepositories.GetInstance<IUserRepository>()
            .GetCompanyIdForIamUserUntrackedAsync(iamUserId)
            .ConfigureAwait(false);
        if (companyUserId == default) throw new ForbiddenException($"iamUserId {iamUserId} is not assigned");

        return _portalRepositories.GetInstance<INotificationRepository>()
            .GetAllAsDetailsByUserIdUntracked(companyUserId, statusId, typeId);
    }

    /// <inheritdoc />
    public async Task<int> GetNotificationCount(string userId, NotificationStatusId? statusId)
    {
        var companyUserId = await _portalRepositories.GetInstance<IUserRepository>()
            .GetCompanyIdForIamUserUntrackedAsync(userId)
            .ConfigureAwait(false);
        if (companyUserId == default) throw new ForbiddenException($"iamUserId {userId} is not assigned");

        return await _portalRepositories.GetInstance<INotificationRepository>()
            .GetNotificationCountAsync(companyUserId, statusId)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task SetNotificationToRead(string userId, Guid notificationId, NotificationStatusId notificationStatusId)
    {
        var companyUserId = await _portalRepositories.GetInstance<IUserRepository>()
            .GetCompanyIdForIamUserUntrackedAsync(userId)
            .ConfigureAwait(false);
        if (companyUserId == default) throw new ForbiddenException($"iamUserId {userId} is not assigned");

        if (!await _portalRepositories.GetInstance<INotificationRepository>()
                .CheckExistsByIdAndUserIdAsync(notificationId, companyUserId).ConfigureAwait(false))
        {
            throw new NotFoundException("Notification does not exist.");
        }

        var notification =_portalRepositories.Attach(new PortalBackend.PortalEntities.Entities.Notification(notificationId));
        notification.ReadStatusId = notificationStatusId;
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task DeleteNotification(string userId, Guid notificationId)
    {
        var companyUserId = await _portalRepositories.GetInstance<IUserRepository>()
            .GetCompanyIdForIamUserUntrackedAsync(userId)
            .ConfigureAwait(false);
        if (companyUserId == default) throw new ForbiddenException($"iamUserId {userId} is not assigned");

        var notificationRepository = _portalRepositories.GetInstance<INotificationRepository>();
        var exists = await notificationRepository.CheckExistsByIdAndUserIdAsync(notificationId, companyUserId).ConfigureAwait(false);

        if (!exists) throw new NotFoundException("Notification does not exist.");

        var notification = new PortalBackend.PortalEntities.Entities.Notification(notificationId);
        _portalRepositories.Remove(notification);
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }
}
