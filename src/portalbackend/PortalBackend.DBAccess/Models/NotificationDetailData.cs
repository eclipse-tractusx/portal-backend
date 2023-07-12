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

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;

/// <summary>
/// Detail data of a notification
/// </summary>
/// <param name="Id">The Id of the notification</param>
/// <param name="Created">Date of the notification creation</param>
/// <param name="TypeId">The notifications type id</param>
/// <param name="IsRead"><c>true</c> if the notification is read, otherwise <c>false</c></param>
/// <param name="Content">The notifications content</param>
/// <param name="DueDate">Optional: The notifications dueDate</param>
/// <param name="Done"><c>true</c> if the notification is an action notification and done, otherwise <c>false</c></param>
public record NotificationDetailData(
    Guid Id,
    DateTimeOffset Created,
    NotificationTypeId TypeId,
    NotificationTopicId? NotificationTopic,
    bool IsRead,
    string? Content,
    DateTimeOffset? DueDate,
    bool? Done);
