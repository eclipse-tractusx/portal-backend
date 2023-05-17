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
using System.ComponentModel.DataAnnotations;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

/// <summary>
/// Topic of a notification
/// </summary>
public class NotificationTopic
{
	/// <summary>
	/// Internal constructor, only for EF
	/// </summary>
	private NotificationTopic()
	{
		Label = null!;
		NotificationTypeAssignedTopics = new HashSet<NotificationTypeAssignedTopic>();
	}

	/// <summary>
	/// Creates a new instance of <see cref="NotificationTopicId"/> and initializes the id and label 
	/// </summary>
	/// <param name="notificationTopicId">The notification topic id</param>
	public NotificationTopic(NotificationTopicId notificationTopicId) : this()
	{
		Id = notificationTopicId;
		Label = notificationTopicId.ToString();
	}

	/// <summary>
	/// Id of the type
	/// </summary>
	public NotificationTopicId Id { get; private set; }

	/// <summary>
	/// The type as string 
	/// </summary>
	[MaxLength(255)]
	public string Label { get; private set; }

	// Navigation properties

	/// <summary>
	/// Mapped notification types
	/// </summary>
	public virtual ICollection<NotificationTypeAssignedTopic> NotificationTypeAssignedTopics { get; private set; }
}
