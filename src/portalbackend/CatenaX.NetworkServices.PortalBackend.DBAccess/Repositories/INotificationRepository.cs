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
    /// <param name="notification">The notification that should be added to the persistence layer.</param>
    void Add(Notification notification);

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
}
