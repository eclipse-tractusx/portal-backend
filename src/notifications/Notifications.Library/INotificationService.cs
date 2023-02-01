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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Notifications.Library;

/// <summary>
/// Provides methods to create notifications
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Creates notifications for the given notification type ids with the given content.
    /// The receiver of the notification will be retrieved by the given roles for the given clients.
    /// </summary>
    /// <param name="receiverUserRoles">UserRoles for specified clients</param>
    /// <param name="creatorId">ID of the creator company user</param>
    /// <param name="notifications">combination of notification types with content of the notification</param>
    /// <param name="companyId">Id of the company to select the receiver users from</param>
    Task CreateNotifications(IDictionary<string, IEnumerable<string>> receiverUserRoles, Guid? creatorId, IEnumerable<(string? content, NotificationTypeId notificationTypeId)> notifications, Guid companyId);
}
