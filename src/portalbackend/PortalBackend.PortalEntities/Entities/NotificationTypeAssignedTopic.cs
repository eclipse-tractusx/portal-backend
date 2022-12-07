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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

/// <summary>
/// Value table for app subscription statuses.
/// </summary>
public class NotificationTypeAssignedTopic
{
    /// <summary>
    /// Constructor.
    /// </summary>
    private NotificationTypeAssignedTopic()
    {
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="notificationTypeId">Id of the notification type.</param>
    /// <param name="notificationTopicId">Id of the notification topic.</param>
    public NotificationTypeAssignedTopic(NotificationTypeId notificationTypeId, NotificationTopicId notificationTopicId) : this()
    {
        NotificationTypeId = notificationTypeId;
        NotificationTopicId = notificationTopicId;
    }

    /// <summary>
    /// Id of the notification type.
    /// </summary>
    public NotificationTypeId NotificationTypeId { get; private set; }

    /// <summary>
    /// Id of the notification topic.
    /// </summary>
    public NotificationTopicId NotificationTopicId { get; private set; }

    // Navigation properties

    /// <summary>
    /// All AppSubscriptions currently with this status.
    /// </summary>
    public virtual NotificationType? NotificationType { get; private set; }
    
    /// <summary>
    /// All AppSubscriptions currently with this status.
    /// </summary>
    public virtual NotificationTopic? NotificationTopic { get; private set; }
}
