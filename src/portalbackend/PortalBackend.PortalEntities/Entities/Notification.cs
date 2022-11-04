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

using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Extensions;

namespace Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Entities;

/// <summary>
/// Notification
/// </summary>
public class Notification
{
    /// <summary>
    /// Only needed for ef core
    /// </summary>
    private Notification()
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="Notification"/> and sets the required values.
    /// </summary>
    /// <param name="id">Id of the notification</param>
    /// <param name="receiverUserId">Mapping to the company user who should receive the message</param>
    /// <param name="dateCreated">The creation date</param>
    /// <param name="notificationTypeId">id of the notification type</param>
    /// <param name="notificationTopicId">id of the notification topic</param>
    /// <param name="isRead"><c>true</c> if the notification is read, otherwise <c>false</c></param>
    public Notification(Guid id, Guid receiverUserId, DateTimeOffset dateCreated, NotificationTypeId notificationTypeId, NotificationTopicId notificationTopicId, bool isRead)
    {
        Id = id;
        ReceiverUserId = receiverUserId;
        DateCreated = dateCreated;
        NotificationTypeId = notificationTypeId;
        NotificationTopicId = notificationTopicId;
        IsRead = isRead;
    }

    public Guid Id { get; private set; }

    public Guid ReceiverUserId { get; private set; }
    
    public DateTimeOffset DateCreated { get; private set; }

    public string? Content { get; set; }

    public NotificationTypeId NotificationTypeId { get; private set; }

    public NotificationTopicId NotificationTopicId { get; private set; }
    
    public bool IsRead { get; set; }

    public DateTimeOffset? DueDate { get; set; }

    public Guid? CreatorUserId { get; set; }

    // Navigation properties
    public virtual CompanyUser? Receiver { get; set; }
    public virtual NotificationType? NotificationType { get; private set; }
    public virtual NotificationTopic? NotificationTopic { get; private set; }
    public virtual CompanyUser? Creator { get; private set; }
}
