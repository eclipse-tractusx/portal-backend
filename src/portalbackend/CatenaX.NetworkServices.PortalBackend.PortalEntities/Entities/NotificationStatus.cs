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

using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

/// <summary>
/// Status of a notification
/// </summary>
public class NotificationStatus
{
    /// <summary>
    /// Internal constructor, only for EF
    /// </summary>
    private NotificationStatus()
    {
        Label = null!;
        Notifications = new HashSet<Notification>();
    }

    /// <summary>
    /// Creates a new instance of <see cref="NotificationStatus"/> and initializes the id and label 
    /// </summary>
    /// <param name="notificationStatusId">The NotificationStatusId</param>
    public NotificationStatus(NotificationStatusId notificationStatusId) : this()
    {
        Id = notificationStatusId;
        Label = notificationStatusId.ToString();
    }

    /// <summary>
    /// Id of the status
    /// </summary>
    public NotificationStatusId Id { get; private set; }

    /// <summary>
    /// The status as string 
    /// </summary>
    [MaxLength(255)]
    public string Label { get; private set; }

    // Navigation properties
    public virtual ICollection<Notification> Notifications { get; private set; }
}
