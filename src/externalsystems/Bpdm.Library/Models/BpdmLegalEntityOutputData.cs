/********************************************************************************
 * Copyright (c) 2021, 2023 Microsoft and BMW Group AG
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

namespace Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library.Models;

public record PageOutputResponseBpdmLegalEntityData(
    IEnumerable<BpdmLegalEntityOutputData>? Content
);

public record BpdmLegalEntityOutputData(
    [property: JsonPropertyName("externalId")] string? ExternalId,
    [property: JsonPropertyName("bpnl")] string? Bpn,
    [property: JsonPropertyName("legalShortName")] string? LegalShortName,
    [property: JsonPropertyName("legalForm")] string? LegalForm,
    [property: JsonPropertyName("identifiers")] IEnumerable<BpdmIdentifier> Identifiers,
    [property: JsonPropertyName("states")] IEnumerable<BpdmStatus> States,
    [property: JsonPropertyName("classifications")] IEnumerable<BpdmProfileClassification> Classifications,
    [property: JsonPropertyName("legalNameParts")] IEnumerable<string> LegalNameParts,
    [property: JsonPropertyName("roles")] IEnumerable<string> Roles,
    [property: JsonPropertyName("legalAddress")] BpdmLegalAddressResponse LegalAddress
);

public record BpdmLegalAddressResponse(
    [property: JsonPropertyName("externalId")] string ExternalId,
    [property: JsonPropertyName("legalEntityExternalId")] string LegalEntityExternalId,
    [property: JsonPropertyName("siteExternalId")] string SiteExternalId,
    [property: JsonPropertyName("bpna")] string Bpn,
    [property: JsonPropertyName("nameParts")] IEnumerable<string> NameParts,
    [property: JsonPropertyName("states")] IEnumerable<BpdmAddressState> States,
    [property: JsonPropertyName("identifiers")] IEnumerable<BpdmAddressIdentifier> Identifiers,
    [property: JsonPropertyName("physicalPostalAddress")] BpdmAddressPhysicalPostalAddress? PhysicalPostalAddress,
    [property: JsonPropertyName("alternativePostalAddress")] BpdmAddressAlternativePostalAddress? AlternativePostalAddress,
    [property: JsonPropertyName("roles")] IEnumerable<string> Roles
);

public record BpdmCountry
(
    string TechnicalKey,
    string Name
);
