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

namespace CatenaX.NetworkServices.Framework.Notifications;

/// <summary>
///     Business logic to work with the notifications
/// </summary>
public interface INotificationService
{
    /// <summary>
    ///     Creates a new internal triggered Notification.
    /// </summary>
    /// <param name="receiverId">Id of the company user that should receive the notifications</param>
    /// <returns>Returns information of the created notification</returns>
    Task CreateWelcomeNotifications(Guid receiverId);

    /// <summary>
    ///     Creates a new Notification with the given data
    /// </summary>
    /// <param name="iamUserId">Id of the iamUser</param>
    /// <param name="creationData">The data for the creation of the notification.</param>
    /// <param name="receiverId">Id of the company user the notification is intended for.</param>
    Task<NotificationDetailData> CreateNotificationAsync(string iamUserId, NotificationCreationData creationData,
        Guid receiverId);

}
