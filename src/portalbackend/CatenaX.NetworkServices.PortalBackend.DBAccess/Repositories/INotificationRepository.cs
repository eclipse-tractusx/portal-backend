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

using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

/// <summary>
///     Provides functionality to create, modify and get notifications from the persistence layer
/// </summary>
public interface INotificationRepository
{
    /// <summary>
    ///     Adds the given notification to the persistence layer.
    /// </summary>
    /// <param name="receiverUserId">Mapping to the company user who should receive the message</param>
    /// <param name="content">Contains the message content. The Content is a deserialized json object</param>
    /// <param name="notificationTypeId">id of the notification type</param>
    /// <param name="readStatusId">id of the notification status</param>
    /// <param name="setOptionalParameter">Optional Action to set the notifications optional properties</param>
    Notification Add(Guid receiverUserId, string content, NotificationTypeId notificationTypeId,
        NotificationStatusId readStatusId, Action<Notification>? setOptionalParameter = null);

    /// <summary>
    ///     Gets all Notifications for a specific user
    /// </summary>
    /// <param name="companyUserId">Id of the user</param>
    /// <param name="statusId">OPTIONAL: The status of the notifications</param>
    /// <param name="typeId">OPTIONAL: The type of the notifications</param>
    /// <returns>Returns a collection of NotificationDetailData</returns>
    IAsyncEnumerable<NotificationDetailData> GetAllAsDetailsByUserIdUntracked(Guid companyUserId,
        NotificationStatusId? statusId, NotificationTypeId? typeId);

    /// <summary>
    ///     Returns a notification for the given id and given user if it exists in the persistence layer, otherwise null
    /// </summary>
    /// <param name="notificationId">Id of the notification</param>
    /// <param name="companyUserId">Id of the receiver</param>
    /// <returns>Returns a notification for the given id and given user if it exists in the persistence layer, otherwise null</returns>
    Task<NotificationDetailData?> GetByIdAndUserIdUntrackedAsync(Guid notificationId, Guid companyUserId);

    /// <summary>
    /// Checks if a notification exists for the given id and companyUserId
    /// </summary>
    /// <param name="notificationId">Id of the notification</param>
    /// <param name="companyUserId">Id of the receiver</param>
    /// <returns><c>true</c> if the notification exists, <c>false</c> if it doesn't exist</returns>
    Task<bool> CheckExistsByIdAndUserIdAsync(Guid notificationId, Guid companyUserId);

    /// <summary>
    /// Gets the count of the notifications for the given user and optional status
    /// </summary>
    /// <param name="companyUserId">Id of the company user </param>
    /// <param name="statusId">OPTIONAL: Status of the notifications that should be considered</param>
    /// <returns>Returns the count of the notifications</returns>
    Task<int> GetNotificationCountAsync(Guid companyUserId, NotificationStatusId? statusId);
}
