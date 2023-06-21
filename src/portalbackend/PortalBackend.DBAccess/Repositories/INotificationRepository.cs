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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

/// <summary>
///     Provides functionality to create, modify and get notifications from the persistence layer
/// </summary>
public interface INotificationRepository
{
    /// <summary>
    ///     Adds the given notification to the persistence layer.
    /// </summary>
    /// <param name="receiverUserId">Mapping to the company user who should receive the message</param>
    /// <param name="notificationTypeId">id of the notification type</param>
    /// <param name="isRead"><c>true</c> if the notification is read, otherwise <c>false</c></param>
    /// <param name="setOptionalParameters">Optional Action to set the notifications optional properties</param>
    Notification CreateNotification(Guid receiverUserId, NotificationTypeId notificationTypeId,
        bool isRead, Action<Notification>? setOptionalParameters = null);

    void AttachAndModifyNotification(Guid notificationId, Action<Notification> setOptionalParameters);

    void AttachAndModifyNotifications(IEnumerable<Guid> notificationIds, Action<Notification> setOptionalParameters);

    Notification DeleteNotification(Guid notificationId);

    /// <summary>
    ///     Gets all Notifications for a specific user
    /// </summary>
    /// <param name="receiverUserId">Id of the user</param>
    /// <param name="isRead">OPTIONAL: filter read or unread notifications</param>
    /// <param name="typeId">OPTIONAL: The type of the notifications</param>
    /// <param name="topicId">OPTIONAL: The topic of the notifications</param>
    /// <param name="onlyDueDate">OPTIONAL: If true only notifications with a due date will be returned</param>
    /// <param name="sorting"></param>
    /// <returns>Returns a collection of NotificationDetailData</returns>
    Func<int, int, Task<Pagination.Source<NotificationDetailData>?>> GetAllNotificationDetailsByReceiver(Guid receiverUserId, bool? isRead, NotificationTypeId? typeId, NotificationTopicId? topicId, bool onlyDueDate, NotificationSorting? sorting);

    /// <summary>
    ///     Returns a notification for the given id and given user if it exists in the persistence layer, otherwise null
    /// </summary>
    /// <param name="notificationId">Id of the notification</param>
    /// <param name="companyUserId">Id of the receiver</param>
    /// <returns>Returns a notification for the given id and given user if it exists in the persistence layer, otherwise null</returns>
    Task<(bool IsUserReceiver, NotificationDetailData NotificationDetailData)> GetNotificationByIdAndValidateReceiverAsync(Guid notificationId, Guid companyUserId);

    /// <summary>
    /// Checks if a notification exists for the given id and companyUserId
    /// </summary>
    /// <param name="notificationId">Id of the notification</param>
    /// <param name="companyUserId">Id of the receiver</param>
    /// <returns><c>true</c> if the notification exists, <c>false</c> if it doesn't exist</returns>
    Task<(bool IsUserReceiver, bool IsNotificationExisting)> CheckNotificationExistsByIdAndValidateReceiverAsync(Guid notificationId, Guid companyUserId);

    /// <summary>
    /// Gets the count of the notifications for the given user and optional status
    /// </summary>
    /// <param name="companyUserId">Id of the User</param>
    /// <param name="isRead">OPTIONAL: filter read or unread notifications</param>
    /// <returns>Returns the count of the notifications</returns>
    Task<int> GetNotificationCountForUserAsync(Guid companyUserId, bool? isRead);

    /// <summary>
    /// Gets the count details of the notifications for the given user
    /// </summary>
    /// <param name="companyUserId">id of the user</param>
    /// <returns>Returns the notification count details</returns>
    IAsyncEnumerable<(bool IsRead, bool? Done, NotificationTopicId NotificationTopicId, int Count)> GetCountDetailsForUserAsync(Guid companyUserId);

    /// <summary>
    /// Gets the notification ids that should be updated
    /// </summary>
    /// <param name="userRoleIds">ids of the user roles</param>
    /// <param name="companyUserIds">(optional) ids of companyUsers</param>
    /// <param name="notificationTypeIds">notification type ids</param>
    /// <param name="offerId">id of the offer to get the notifications for</param>
    /// <returns>List of the notification ids that should be updated</returns>
    IAsyncEnumerable<Guid> GetNotificationUpdateIds(IEnumerable<Guid> userRoleIds, IEnumerable<Guid>? companyUserIds, IEnumerable<NotificationTypeId> notificationTypeIds, Guid offerId);

    /// <summary>
    /// Checks if a notification is existing for a company user and a specific search param
    /// </summary>
    /// <param name="receiverId">Id of the receiver</param>
    /// <param name="notificationTypeId">Id of the notification type</param>
    /// <param name="searchParam">The value of the notifications content that should be searched</param>
    /// <param name="searchValue">The value of the search Param</param>
    /// <returns>Returns true if a notification exists</returns>
    Task<bool> CheckNotificationExistsForParam(Guid receiverId, NotificationTypeId notificationTypeId, string searchParam, string searchValue);

    /// <summary>
    /// Checks if notifications are existing for company users, notificationTypeIds and a specific search param
    /// </summary>
    /// <param name="receiverIds">Id of the receivers</param>
    /// <param name="notificationTypeIds">Id of the notification types</param>
    /// <param name="searchParam">The value of the notifications content that should be searched</param>
    /// <param name="searchValue">The value of the search Param</param>
    /// <returns>Returns the existing notification typeIds for the receiver</returns>
    IAsyncEnumerable<(NotificationTypeId NotificationTypeId, Guid ReceiverId)> CheckNotificationsExistsForParam(IEnumerable<Guid> receiverIds, IEnumerable<NotificationTypeId> notificationTypeIds, string searchParam, string searchValue);
}
