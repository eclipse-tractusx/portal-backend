// /********************************************************************************
//  * Copyright (c) 2021,2022 BMW Group AG
//  * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
//  *
//  * See the NOTICE file(s) distributed with this work for additional
//  * information regarding copyright ownership.
//  *
//  * This program and the accompanying materials are made available under the
//  * terms of the Apache License, Version 2.0 which is available at
//  * https://www.apache.org/licenses/LICENSE-2.0.
//  *
//  * Unless required by applicable law or agreed to in writing, software
//  * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
//  * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
//  * License for the specific language governing permissions and limitations
//  * under the License.
//  *
//  * SPDX-License-Identifier: Apache-2.0
//  ********************************************************************************/

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
    public async Task<NotificationDetailData> CreateNotification(NotificationCreationData creationData,
        Guid companyUserId)
    {
        if (!await _portalRepositories.GetInstance<IUserRepository>().IsUserWithIdExisting(companyUserId))
            throw new ArgumentException("User does not exist", nameof(companyUserId));

        var notificationId = Guid.NewGuid();
        var (dateTimeOffset, title, message, notificationTypeId, notificationStatusId, appId, dueData, creatorUserId) =
            creationData;
        CheckEnumValues(notificationTypeId, notificationStatusId);
        _portalRepositories.GetInstance<INotificationRepository>().Add(
            new PortalBackend.PortalEntities.Entities.Notification(notificationId, companyUserId, dateTimeOffset, title,
                message, notificationTypeId, notificationStatusId)
            {
                DueDate = dueData,
                AppId = appId,
                CreatorUserId = creatorUserId
            });
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
        return new NotificationDetailData(notificationId, title, message);
    }

    /// <inheritdoc />
    public async Task<IAsyncEnumerable<NotificationDetailData>> GetNotifications(string iamUserId)
    {
        var companyUserId = await _portalRepositories.GetInstance<IUserRepository>()
            .GetCompanyUserIdForIamUserIdUntrackedAsync(iamUserId)
            .ConfigureAwait(false);
        if (companyUserId == default) throw new ForbiddenException($"iamUserId {iamUserId} is not assigned");

        return _portalRepositories.GetInstance<INotificationRepository>()
            .GetAllAsDetailsByUserIdUntracked(companyUserId, NotificationStatusId.UNREAD);
    }

    /// <inheritdoc />
    public async Task<NotificationDetailData> GetNotification(Guid notficationId, string iamUserId)
    {
        var companyUserId = await _portalRepositories.GetInstance<IUserRepository>()
            .GetCompanyUserIdForIamUserIdUntrackedAsync(iamUserId)
            .ConfigureAwait(false);
        if (companyUserId == default) throw new ForbiddenException($"iamUserId {iamUserId} is not assigned");

        var notificationDetails = await _portalRepositories.GetInstance<INotificationRepository>()
            .GetByIdAndUserIdUntrackedAsync(notficationId, companyUserId)
            .ConfigureAwait(false);
        if (notificationDetails is null) throw new NotFoundException("Notification does not exist.");

        return notificationDetails;
    }

    /// <summary>
    ///     Check the enum values if the api receives a int value which is not in the range of the valid values
    /// </summary>
    /// <param name="notificationTypeId">The notification type</param>
    /// <param name="notificationStatusId">The notification status</param>
    private static void CheckEnumValues(NotificationTypeId notificationTypeId,
        NotificationStatusId notificationStatusId)
    {
        if (!Enum.IsDefined(typeof(NotificationTypeId), notificationTypeId))
            throw new ArgumentException("notificationType does not exist.", nameof(notificationTypeId));

        if (!Enum.IsDefined(typeof(NotificationStatusId), notificationStatusId.ToString()))
            throw new ArgumentException("notificationStatus does not exist.", nameof(notificationStatusId));
    }
}