/********************************************************************************
 * Copyright (c) 2021, 2024 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Notifications.Service.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Notifications.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;

namespace Org.Eclipse.TractusX.Portal.Backend.Notifications.Service.BusinessLogic;

/// <inheritdoc />
public class NotificationBusinessLogic : INotificationBusinessLogic
{
    private static readonly IEnumerable<NotificationTypeId> ValidNotificationTypes =
        [NotificationTypeId.CREDENTIAL_APPROVAL,
        NotificationTypeId.CREDENTIAL_REJECTED,
        NotificationTypeId.CREDENTIAL_EXPIRY];

    private readonly IPortalRepositories _portalRepositories;
    private readonly IIdentityData _identityData;
    private readonly NotificationSettings _settings;

    /// <summary>
    ///     Creates a new instance of <see cref="NotificationBusinessLogic" />
    /// </summary>
    /// <param name="portalRepositories">Access to the repository factory.</param>
    /// <param name="identityService">Access to the identity</param>
    /// <param name="settings">Access to the notifications options</param>
    public NotificationBusinessLogic(IPortalRepositories portalRepositories, IIdentityService identityService, IOptions<NotificationSettings> settings)
    {
        _portalRepositories = portalRepositories;
        _identityData = identityService.IdentityData;
        _settings = settings.Value;
    }

    /// <inheritdoc />
    public Task<Pagination.Response<NotificationDetailData>> GetNotificationsAsync(int page, int size, NotificationFilters filters, SearchSemanticTypeId semantic) =>
        Pagination.CreateResponseAsync(page, size, _settings.MaxPageSize, _portalRepositories.GetInstance<INotificationRepository>()
                .GetAllNotificationDetailsByReceiver(_identityData.IdentityId, semantic, filters.IsRead, filters.TypeId, filters.TopicId, filters.OnlyDueDate, filters.Sorting ?? NotificationSorting.DateDesc, filters.DoneState, filters.SearchTypeIds, filters.SearchQuery));

    /// <inheritdoc />
    public async Task<NotificationDetailData> GetNotificationDetailDataAsync(Guid notificationId)
    {
        var result = await _portalRepositories.GetInstance<INotificationRepository>().GetNotificationByIdAndValidateReceiverAsync(notificationId, _identityData.IdentityId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (result == default)
        {
            throw NotFoundException.Create(NotificationErrors.NOTIFICATION_NOT_FOUND, [new("notificationId", notificationId.ToString())]);
        }

        if (!result.IsUserReceiver)
        {
            throw ForbiddenException.Create(NotificationErrors.USER_NOT_RECEIVER);
        }

        return result.NotificationDetailData;
    }

    /// <inheritdoc />
    public Task<int> GetNotificationCountAsync(bool? isRead) =>
        _portalRepositories.GetInstance<INotificationRepository>().GetNotificationCountForUserAsync(_identityData.IdentityId, isRead);

    /// <inheritdoc />
    public async Task<NotificationCountDetails> GetNotificationCountDetailsAsync()
    {
        var details = await _portalRepositories.GetInstance<INotificationRepository>().GetCountDetailsForUserAsync(_identityData.IdentityId).ToListAsync().ConfigureAwait(false);
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
    public async Task SetNotificationStatusAsync(Guid notificationId, bool isRead)
    {
        var isReadFlag = await CheckNotificationExistsAndValidateReceiver(notificationId).ConfigureAwait(ConfigureAwaitOptions.None);

        _portalRepositories.GetInstance<INotificationRepository>().AttachAndModifyNotification(notificationId, notification =>
            {
                notification.IsRead = isReadFlag;
            },
            notification =>
            {
                notification.IsRead = isRead;
            });

        await _portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    /// <inheritdoc />
    public async Task DeleteNotificationAsync(Guid notificationId)
    {
        await CheckNotificationExistsAndValidateReceiver(notificationId).ConfigureAwait(ConfigureAwaitOptions.None);

        _portalRepositories.GetInstance<INotificationRepository>().DeleteNotification(notificationId);
        await _portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }
    private async Task<bool> CheckNotificationExistsAndValidateReceiver(Guid notificationId)
    {
        var result = await _portalRepositories.GetInstance<INotificationRepository>().CheckNotificationExistsByIdAndValidateReceiverAsync(notificationId, _identityData.IdentityId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (result == default || !result.IsNotificationExisting)
        {
            throw NotFoundException.Create(NotificationErrors.NOTIFICATION_NOT_FOUND, [new("notificationId", notificationId.ToString())]);
        }

        if (!result.IsUserReceiver)
        {
            throw ForbiddenException.Create(NotificationErrors.USER_NOT_RECEIVER);
        }

        return result.isRead;
    }

    /// <inheritdoc />
    public async Task CreateNotification(NotificationRequest data)
    {
        if (!ValidNotificationTypes.Contains(data.NotificationTypeId))
        {
            throw ConflictException.Create(NotificationErrors.INVALID_NOTIFICATION_TYPE, [new("notificationTypeId", data.NotificationTypeId.ToString())]);
        }

        var userExists = await _portalRepositories.GetInstance<IUserRepository>().CheckUserExists(data.Requester).ConfigureAwait(ConfigureAwaitOptions.None);
        if (!userExists)
        {
            throw NotFoundException.Create(NotificationErrors.USER_NOT_FOUND, [new("userId", data.Requester.ToString())]);
        }

        _portalRepositories.GetInstance<INotificationRepository>().CreateNotification(data.Requester, data.NotificationTypeId, false, n =>
        {
            n.CreatorUserId = _identityData.IdentityId;
            n.Content = data.Content;
        });
        await _portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }
}
