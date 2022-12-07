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
using System.ComponentModel.DataAnnotations;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

/// <summary>
/// Type of a notification
/// </summary>
public class NotificationType
{
    /// <summary>
    /// Internal constructor, only for EF
    /// </summary>
    private NotificationType()
    {
        Label = null!;
        Notifications = new HashSet<Notification>();
    }

    /// <summary>
    /// Creates a new instance of <see cref="NotificationType"/> and initializes the id and label 
    /// </summary>
    /// <param name="notificationTypeId">The NotificationTypesId</param>
    public NotificationType(NotificationTypeId notificationTypeId) : this()
    {
        Id = notificationTypeId;
        Label = notificationTypeId.ToString();
    }

    /// <summary>
    /// Id of the type
    /// </summary>
    public NotificationTypeId Id { get; private set; }

    /// <summary>
    /// The type as string 
    /// </summary>
    [MaxLength(255)]
    public string Label { get; private set; }

    // Navigation properties
    public virtual NotificationTypeAssignedTopic? NotificationTypeAssignedTopic { get; set; }

    public virtual ICollection<Notification> Notifications { get; private set; }
}
