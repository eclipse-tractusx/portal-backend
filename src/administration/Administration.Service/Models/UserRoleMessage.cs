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

using System.Text.Json.Serialization;
namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;

/// <summary>
/// model to specify Message for Adding User Role
/// </summary>
public class UserRoleMessage
{
	public UserRoleMessage(IEnumerable<Message> success, IEnumerable<Message> warning)
	{
		Success = success;
		Warning = warning;
	}

	/// <summary>
	/// Success Message
	/// </summary>
	[JsonPropertyName("success")]
	public IEnumerable<Message> Success { get; set; }

	/// <summary>
	/// Warning Message
	/// </summary>
	[JsonPropertyName("warning")]
	public IEnumerable<Message> Warning { get; set; }

	/// <summary>
	/// model to specify Message
	/// </summary>
	public class Message
	{
		public Message(string name, Detail info)
		{
			Name = name;
			Info = info;
		}

		/// <summary>
		/// Name of the Role
		/// </summary>
		[JsonPropertyName("name")]
		public string Name { get; set; }

		/// <summary>
		/// Message Description
		/// </summary>
		[JsonPropertyName("info")]
		public Detail Info { get; set; }
	}

	public enum Detail
	{
		ROLE_DOESNT_EXIST,
		ROLE_ADDED,
		ROLE_ALREADY_ADDED
	}
}
