/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Notifications.Service.BusinessLogic;

/// <summary>
///     Business logic to work with the notifications
/// </summary>
public interface INotificationBusinessLogic
{
    /// <summary>
    ///     Creates a new Notification with the given data
    /// </summary>
    /// <param name="iamUserId">Id of the iamUser</param>
    /// <param name="creationData">The data for the creation of the notification.</param>
    /// <param name="receiverId">Id of the company user the notification is intended for.</param>
    Task<Guid> CreateNotificationAsync(string iamUserId, NotificationCreationData creationData,
        Guid receiverId);

    /// <summary>
    ///     Gets all unread notification for the given user.
    /// </summary>
    /// <param name="page">the requested page</param>
    /// <param name="size">the requested size</param>
    /// <param name="iamUserId">The id of the current user</param>
    /// <param name="isRead">OPTIONAL: filter for read or unread notifications</param>
    /// <param name="typeId">OPTIONAL: The type of the notifications</param>
    /// <param name="topicId">OPTIONAL: The topic of the notifications</param>
    /// <param name="sorting">Kind of sorting for the notifications</param>
    /// <returns>Returns a collection of the users notification</returns>
    Task<Pagination.Response<NotificationDetailData>> GetNotificationsAsync(int page, int size, string iamUserId,
        bool? isRead = null, NotificationTypeId? typeId = null, NotificationTopicId? topicId = null,
        NotificationSorting? sorting = null);

    /// <summary>
    ///     Gets a specific notification for the given user.
    /// </summary>
    /// <param name="iamUserId">The id of the current user</param>
    /// <param name="notificationId">The id of the notification</param>
    /// <returns>Returns a notification</returns>
    Task<NotificationDetailData> GetNotificationDetailDataAsync(string iamUserId, Guid notificationId);        

    /// <summary>
    /// Gets the notification account for the given user
    /// </summary>
    /// <param name="iamUserId">Id of the current user</param>
    /// <param name="isRead">OPTIONAL: filter for read or unread notifications</param>
    /// <returns>Returns the count of the notifications</returns>
    Task<int> GetNotificationCountAsync(string iamUserId, bool? isRead);

    /// <summary>
    /// Gets the count details of the notifications for the given user
    /// </summary>
    /// <param name="iamUserId">Id of the current user</param>
    /// <returns>Returns the count details of the notifications</returns>
    Task<NotificationCountDetails> GetNotificationCountDetailsAsync(string iamUserId);

    /// <summary>
    /// Sets the status of the notification with the given id to read
    /// </summary>
    /// <param name="iamUserId">Id of the notification receiver</param>
    /// <param name="notificationId">Id of the notification</param>
    /// <param name="isRead">Read or unread</param>
    Task SetNotificationStatusAsync(string iamUserId, Guid notificationId, bool isRead);

    /// <summary>
    /// Deletes the given notification
    /// </summary>
    /// <param name="iamUserId">Id of the notification receiver</param>
    /// <param name="notificationId">Id of the notification that should be deleted</param>
    Task DeleteNotificationAsync(string iamUserId, Guid notificationId);
}
