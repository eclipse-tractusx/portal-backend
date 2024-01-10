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

using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.Custodian.Library.Models;

public record CustodianFrameworkRequest(
    [property: JsonPropertyName("holderIdentifier")] string HolderIdentifier,
    [property: JsonPropertyName("type"), JsonConverter(typeof(EnumMemberConverter<VerifiedCredentialExternalTypeId>))] VerifiedCredentialExternalTypeId Type,
    [property: JsonPropertyName("contract-template")] string? Template,
    [property: JsonPropertyName("contract-version")] string Version,
    [property: JsonPropertyName("expiry")] DateTimeOffset Expiry
);

public record CustodianDismantlerRequest(
    [property: JsonPropertyName("bpn")] string Bpn,
    [property: JsonPropertyName("activityType"), JsonConverter(typeof(EnumMemberConverter<VerifiedCredentialTypeId>))] VerifiedCredentialTypeId Type,
    [property: JsonPropertyName("expiry")] DateTimeOffset Expiry
);
