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

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;

/// <summary>
/// Detail count data of the notifications for a specific user
/// </summary>
/// <param name="Read">Count of all read messages</param>
/// <param name="Unread">Count of all unread messages</param>
/// <param name="InfoUnread">Count of all unread messages from type info</param>
/// <param name="OfferUnread">Count of all unread messages from type offer</param>
/// <param name="ActionRequired">Count of all messages from type action that are not done yet</param>
/// <param name="UnreadActionRequired">Count of all unread messages from type action that are not done yet</param>
public record NotificationCountDetails(
    int Read,
    int Unread,
    int InfoUnread,
    int OfferUnread,
    int ActionRequired,
    int UnreadActionRequired
);
