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

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Token;

public record AuthResponse(
	[property:JsonPropertyName("access_token")]
	string?  AccessToken,

	[property:JsonPropertyName("expires_in")]
	int  ExpiresIn,

	[property:JsonPropertyName("refresh_expires_in")]
	int RefreshExpiresIn,

	[property:JsonPropertyName("refresh_token")]
	string? RefreshToken,

	[property:JsonPropertyName("token_type")]
	string? TokenType,

	[property:JsonPropertyName("id_token")]
	string? IdToken,

	[property:JsonPropertyName("notbeforepolicy")]
	int NotBeforePolicy,

	[property:JsonPropertyName("session_state")]
	string? SessionState,

	[property:JsonPropertyName("scope")]
	string? Scope
);
