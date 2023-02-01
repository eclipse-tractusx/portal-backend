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

    Notification DeleteNotification(Guid notificationId);

    /// <summary>
    ///     Gets all Notifications for a specific user
    /// </summary>
    /// <param name="iamUserId">Id of the user</param>
    /// <param name="isRead">OPTIONAL: filter read or unread notifications</param>
    /// <param name="typeId">OPTIONAL: The type of the notifications</param>
    /// <param name="topicId">OPTIONAL: The topic of the notifications</param>
    /// <param name="sorting"></param>
    /// <returns>Returns a collection of NotificationDetailData</returns>
    Func<int,int,Task<Pagination.Source<NotificationDetailData>?>> GetAllNotificationDetailsByIamUserIdUntracked(string iamUserId, bool? isRead, NotificationTypeId? typeId, NotificationTopicId? topicId, NotificationSorting? sorting);

    /// <summary>
    ///     Returns a notification for the given id and given user if it exists in the persistence layer, otherwise null
    /// </summary>
    /// <param name="notificationId">Id of the notification</param>
    /// <param name="iamUserId">Id of the receiver</param>
    /// <returns>Returns a notification for the given id and given user if it exists in the persistence layer, otherwise null</returns>
    Task<(bool IsUserReceiver, NotificationDetailData NotificationDetailData)> GetNotificationByIdAndIamUserIdUntrackedAsync(Guid notificationId, string iamUserId);

    /// <summary>
    /// Checks if a notification exists for the given id and companyUserId
    /// </summary>
    /// <param name="notificationId">Id of the notification</param>
    /// <param name="iamUserId">Id of the receiver</param>
    /// <returns><c>true</c> if the notification exists, <c>false</c> if it doesn't exist</returns>
    Task<(bool IsUserReceiver, bool IsNotificationExisting)> CheckNotificationExistsByIdAndIamUserIdAsync(Guid notificationId, string iamUserId);

    /// <summary>
    /// Gets the count of the notifications for the given user and optional status
    /// </summary>
    /// <param name="iamUserId">Id of the iam User</param>
    /// <param name="isRead">OPTIONAL: filter read or unread notifications</param>
    /// <returns>Returns the count of the notifications</returns>
    Task<(bool IsUserExisting, int Count)> GetNotificationCountForIamUserAsync(string iamUserId, bool? isRead);

    /// <summary>
    /// Gets the count details of the notifications for the given user
    /// </summary>
    /// <param name="iamUserId">id of the iam user</param>
    /// <returns>Returns the notification count details</returns>
    IAsyncEnumerable<(bool IsRead, NotificationTopicId NotificationTopicId, int Count)> GetCountDetailsForUserAsync(string iamUserId);
}
