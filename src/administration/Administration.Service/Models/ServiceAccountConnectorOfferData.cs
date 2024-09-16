/********************************************************************************
 * Copyright (c) 2023 BMW Group AG
 * Copyright (c) 2023 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Enums;
using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;

public record ServiceAccountConnectorOfferData(
    [property: JsonPropertyName("serviceAccountId")] Guid ServiceAccountId,
    [property: JsonPropertyName("clientId")] string? ClientId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("authenticationType")] IamClientAuthMethod? IamClientAuthMethod,
    [property: JsonPropertyName("roles")] IEnumerable<UserRoleData> UserRoleDatas,
    [property: JsonPropertyName("companyServiceAccountTypeId")] CompanyServiceAccountTypeId CompanyServiceAccountTypeId,
    [property: JsonPropertyName("usertype")] CompanyServiceAccountKindId CompanyServiceAccountKindId,
    [property: JsonPropertyName("authenticationServiceUrl")] string AuthenticationServiceUrl,
    [property: JsonPropertyName("status")] UserStatusId UserStatusId,
    [property: JsonPropertyName("secret")] string? Secret,
    [property: JsonPropertyName("connector")] ConnectorResponseData? Connector,
    [property: JsonPropertyName("offer")] OfferResponseData? Offer,
    [property: JsonPropertyName("LastEditorName")] string? LastName,
    [property: JsonPropertyName("LastEditorCompanyName")] string? CompanyName,
    [property: JsonPropertyName("subscriptionId")] Guid? SubscriptionId = null

);
