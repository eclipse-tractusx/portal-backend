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

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;

/// <summary>
/// Basic model for user role data needed to display user roles.
/// </summary>
public record UserRoleData(
        [property: JsonPropertyName("roleId")] Guid UserRoleId,
        [property: JsonPropertyName("clientId")] string ClientClientId,
        [property: JsonPropertyName("roleName")] string UserRoleText);

/// <summary>
/// Basic model for user role data needed to display user roles with description.
/// </summary>
public record UserRoleWithDescription(
        [property: JsonPropertyName("roleId")] Guid UserRoleId,
        [property: JsonPropertyName("roleName")] string UserRoleText,
        [property: JsonPropertyName("roleDescription")] string RoleDescription);

public record UserRoleInformation(
    [property: JsonPropertyName("roleId")] Guid UserRoleId,
    [property: JsonPropertyName("roleName")] string UserRoleText);
