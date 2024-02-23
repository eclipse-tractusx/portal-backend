/********************************************************************************
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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;

public record CompanyUserDetails(
    [property: JsonPropertyName("companyUserId")] Guid CompanyUserId,
    [property: JsonPropertyName("created")] DateTimeOffset CreatedAt,
    [property: JsonPropertyName("bpn")] IEnumerable<string> BusinessPartnerNumbers,
    [property: JsonPropertyName("company")] string CompanyName,
    [property: JsonPropertyName("status")] UserStatusId UserStatusId,
    [property: JsonPropertyName("assignedRoles")] IEnumerable<CompanyUserAssignedRoleDetails> AssignedRoles,
    [property: JsonPropertyName("firstName")] string? FirstName,
    [property: JsonPropertyName("lastName")] string? LastName,
    [property: JsonPropertyName("email")] string? Email);

public record CompanyUserDetailData(
    [property: JsonPropertyName("companyUserId")] Guid CompanyUserId,
    [property: JsonPropertyName("created")] DateTimeOffset CreatedAt,
    [property: JsonPropertyName("bpn")] IEnumerable<string> BusinessPartnerNumbers,
    [property: JsonPropertyName("company")] string CompanyName,
    [property: JsonPropertyName("status")] UserStatusId UserStatusId,
    [property: JsonPropertyName("assignedRoles")] IEnumerable<CompanyUserAssignedRoleDetails> AssignedRoles,
    [property: JsonPropertyName("idpUserIds")] IEnumerable<IdpUserId> IdpUserIds,
    [property: JsonPropertyName("firstName")] string? FirstName,
    [property: JsonPropertyName("lastName")] string? LastName,
    [property: JsonPropertyName("email")] string? Email);

public record CompanyOwnUserDetails(
    [property: JsonPropertyName("companyUserId")] Guid CompanyUserId,
    [property: JsonPropertyName("created")] DateTimeOffset CreatedAt,
    [property: JsonPropertyName("bpn")] IEnumerable<string> BusinessPartnerNumbers,
    [property: JsonPropertyName("company")] string CompanyName,
    [property: JsonPropertyName("status")] UserStatusId UserStatusId,
    [property: JsonPropertyName("assignedRoles")] IEnumerable<CompanyUserAssignedRoleDetails> AssignedRoles,
    [property: JsonPropertyName("admin")] IEnumerable<CompanyUserAdminDetails> AdminDetails,
    [property: JsonPropertyName("idpUserIds")] IEnumerable<IdpUserId> IdpUserIds,
    [property: JsonPropertyName("firstName")] string? FirstName,
    [property: JsonPropertyName("lastName")] string? LastName,
    [property: JsonPropertyName("email")] string? Email);

public record IdpUserId(
    [property: JsonPropertyName("idpDisplayName")] string IdpDisplayName,
    [property: JsonPropertyName("idpAlias")] string IdpAlias,
    [property: JsonPropertyName("userId")] string UserId);
