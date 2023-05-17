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

using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;

public record UserCreationInfo(

	[property:JsonPropertyName("userName")]
	string? userName,

	[RegularExpression(ValidationExpressions.Email, ErrorMessage = "Invalid email", MatchTimeoutInMilliseconds = 500)]
	[property:JsonPropertyName("email")]
	string eMail,

	[RegularExpression(ValidationExpressions.Name, ErrorMessage = "Invalid firstName", MatchTimeoutInMilliseconds = 500)]
	[property:JsonPropertyName("firstName")]
	string? firstName,

	[RegularExpression(ValidationExpressions.Name, ErrorMessage = "Invalid lastName", MatchTimeoutInMilliseconds = 500)]
	[property:JsonPropertyName("lastName")]
	string? lastName,

	[property:JsonPropertyName("roles")]
	IEnumerable<string> Roles
);

public record UserCreationInfoWithMessage(

	[property:JsonPropertyName("userName")]
	string? userName,

	[property:JsonPropertyName("email")]
	string eMail,

	[property:JsonPropertyName("firstName")]
	string? firstName,

	[property:JsonPropertyName("lastName")]
	string? lastName,

	[property:JsonPropertyName("roles")]
	IEnumerable<string> Roles,

	[property:JsonPropertyName("message")]
	string? Message
);
