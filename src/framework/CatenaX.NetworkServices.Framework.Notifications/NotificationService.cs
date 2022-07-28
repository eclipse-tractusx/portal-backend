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

namespace CatenaX.NetworkServices.Framework.Notifications;

/// <inheritdoc />
public class NotificationService : INotificationService
{
    private readonly IPortalRepositories _portalRepositories;

    /// <summary>
    ///     Creates a new instance of <see cref="NotificationService" />
    /// </summary>
    /// <param name="portalRepositories">Access to the repository factory.</param>
    public NotificationService(IPortalRepositories portalRepositories)
    {
        _portalRepositories = portalRepositories;
    }

    /// <inheritdoc />
    public async Task CreateWelcomeNotificationsForCompanyAsync(Guid companyId)
    {
        var userIds = await _portalRepositories.GetInstance<IUserRepository>().GetCatenaAndCompanyAdminIdAsync(companyId).ToListAsync().ConfigureAwait(false);
        if (userIds.All(x => !x.IsCatenaXAdmin))
        {
            return;
        }

        if (!userIds.Any(x => !x.IsCatenaXAdmin && x.IsCompanyAdmin))
        {
            return;
        }

        foreach (var receiverUserId in userIds.Where(x => !x.IsCatenaXAdmin && x.IsCompanyAdmin).Select(x => x.CompanyUserId))
        {
            foreach (var typeId in new[] {
                         NotificationTypeId.WELCOME,
                         NotificationTypeId.WELCOME_USE_CASES,
                         NotificationTypeId.WELCOME_SERVICE_PROVIDER,
                         NotificationTypeId.WELCOME_CONNECTOR_REGISTRATION,
                         NotificationTypeId.WELCOME_APP_MARKETPLACE,
                     })
            {
                _portalRepositories.GetInstance<INotificationRepository>().Create(receiverUserId, typeId, false,
                    notification =>
                    {
                        notification.CreatorUserId = userIds.Single(x => x.IsCatenaXAdmin).CompanyUserId;
                    });
            }
        }


        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<NotificationDetailData> CreateNotificationAsync(string iamUserId,
        NotificationCreationData creationData, Guid receiverId)
    {
        var users = await _portalRepositories.GetInstance<IUserRepository>().GetCompanyUserWithIamUserCheck(iamUserId, receiverId).ToListAsync().ConfigureAwait(false);

        if (users.All(x => x.CompanyUserId != receiverId))
            throw new ArgumentException("User does not exist", nameof(receiverId));

        var (content, notificationTypeId, notificationStatusId, dueDate) =
            creationData;

        var notification = _portalRepositories.GetInstance<INotificationRepository>().Create(
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
        return new NotificationDetailData(notification.Id, notification.DateCreated, notificationTypeId, notificationStatusId, content, dueDate);
    }
}
