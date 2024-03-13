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

using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Notifications.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Notifications.Service.BusinessLogic;

/// <summary>
///     Business logic to work with the notifications
/// </summary>
public interface INotificationBusinessLogic
{
    /// <summary>
    ///     Gets all unread notification for the given user.
    /// </summary>
    /// <param name="page">the requested page</param>
    /// <param name="size">the requested size</param>
    /// <param name="filters">additional filters to query notifications</param>
    /// <param name="semantic">use AND or OR semantic</param>
    /// <returns>Returns a collection of the users notification</returns>
    Task<Pagination.Response<NotificationDetailData>> GetNotificationsAsync(int page, int size, NotificationFilters filters, SearchSemanticTypeId semantic);

    /// <summary>
    ///     Gets a specific notification for the given user.
    /// </summary>
    /// <param name="notificationId">The id of the notification</param>
    /// <returns>Returns a notification</returns>
    Task<NotificationDetailData> GetNotificationDetailDataAsync(Guid notificationId);

    /// <summary>
    /// Gets the notification account for the given user
    /// </summary>
    /// <param name="isRead">OPTIONAL: filter for read or unread notifications</param>
    /// <returns>Returns the count of the notifications</returns>
    Task<int> GetNotificationCountAsync(bool? isRead);

    /// <summary>
    /// Gets the count details of the notifications for the given user
    /// </summary>
    /// <returns>Returns the count details of the notifications</returns>
    Task<NotificationCountDetails> GetNotificationCountDetailsAsync();

    /// <summary>
    /// Sets the status of the notification with the given id to read
    /// </summary>
    /// <param name="notificationId">Id of the notification</param>
    /// <param name="isRead">Read or unread</param>
    Task SetNotificationStatusAsync(Guid notificationId, bool isRead);

    /// <summary>
    /// Deletes the given notification
    /// </summary>
    /// <param name="notificationId">Id of the notification that should be deleted</param>
    Task DeleteNotificationAsync(Guid notificationId);

    /// <summary>
    /// Creates a notification with the given data
    /// </summary>
    /// <param name="data">The notification request data</param>
    Task CreateNotification(NotificationRequest data);
}
