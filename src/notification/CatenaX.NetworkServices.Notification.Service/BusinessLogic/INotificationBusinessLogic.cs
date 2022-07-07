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
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.Notification.Service.BusinessLogic;

/// <summary>
///     Business logic to work with the notifications
/// </summary>
public interface INotificationBusinessLogic
{
    /// <summary>
    ///     Creates a new Notification with the given data
    /// </summary>
    /// <param name="creationData">The data for the creation of the notification.</param>
    /// <param name="companyUserId">Id of the company user the notification is intended for.</param>
    Task<NotificationDetailData> CreateNotification(NotificationCreationData creationData, Guid companyUserId);

    /// <summary>
    ///     Gets all unread notification for the given user.
    /// </summary>
    /// <param name="iamUserId">The id of the current user</param>
    /// <param name="statusId">OPTIONAL: The status of the notifications</param>
    /// <param name="typeId">OPTIONAL: The type of the notifications</param>
    /// <returns>Returns a collection of the users notification</returns>
    Task<IAsyncEnumerable<NotificationDetailData>> GetNotifications(string iamUserId,
        NotificationStatusId? statusId, NotificationTypeId? typeId);

    /// <summary>
    ///     Gets a specific notification from the database
    /// </summary>
    /// <param name="notificationId">the notification that should be returned</param>
    /// <param name="iamUserId">the id of the current user</param>
    /// <returns>Returns detail data for the given notification</returns>
    Task<NotificationDetailData> GetNotification(Guid notificationId, string iamUserId);
}
