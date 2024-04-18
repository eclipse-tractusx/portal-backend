/********************************************************************************
 * Copyright (c) 2021, 2023 Microsoft and BMW Group AG
 * Copyright (c) 2021, 2024 Contributors to the Eclipse Foundation
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
using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library.Models;

public record PageOutputResponseBpdmLegalEntityData(
    IEnumerable<BpdmLegalEntityOutputData>? Content
);

public record BpdmLegalEntityOutputData(
    [property: JsonPropertyName("externalId")] string? ExternalId,
    [property: JsonPropertyName("nameParts")] IEnumerable<string> NameParts,
    [property: JsonPropertyName("identifiers")] IEnumerable<BpdmIdentifier> Identifiers,
    [property: JsonPropertyName("states")] IEnumerable<BpdmStatus> States,
    [property: JsonPropertyName("roles")] IEnumerable<string> Roles,
    [property: JsonPropertyName("isOwnCompanyData")] bool IsOwnCompanyData,
    [property: JsonPropertyName("legalEntity")] BpdmLegelEntityData? LegalEntity,
    [property: JsonPropertyName("site")] BpdmLegalEntitySite? Site,
    [property: JsonPropertyName("address")] BpdmLegalAddressResponse Address
);

public record BpdmLegelEntityData(
    [property: JsonPropertyName("legalEntityBpn")] string? Bpnl,
    [property: JsonPropertyName("legalName")] string? LegalName,
    [property: JsonPropertyName("shortName")] string? ShortName,
    [property: JsonPropertyName("legalForm")] string? LegalForm,
    [property: JsonPropertyName("confidenceCriteria")] BpdmConfidenceCriteria ConfidenceCriteria,
    [property: JsonPropertyName("states")] IEnumerable<BpdmStatus> States
);

public record BpdmLegalEntitySite(
    string SiteBpn,
    string Name,
    [property: JsonPropertyName("confidenceCriteria")] BpdmConfidenceCriteria ConfidenceCriteria,
    IEnumerable<BpdmState> States
);

public record BpdmConfidenceCriteria(
    [property: JsonPropertyName("sharedByOwner")] bool SharedByOwner,
    [property: JsonPropertyName("checkedByExternalDataSource")] bool CheckedByExternalDataSource,
    [property: JsonPropertyName("numberOfSharingMembers")] int NumberOfSharingMembers,
    [property: JsonPropertyName("lastConfidenceCheckAt")] DateTime LastConfidenceCheckAt,
    [property: JsonPropertyName("nextConfidenceCheckAt")] DateTime NextConfidenceCheckAt,
    [property: JsonPropertyName("confidenceLevel")] int ConfidenceLevel
);

public record BpdmLegalAddressResponse(
    [property: JsonPropertyName("addressBpn")] string? Bpna,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("addressType")] string? AddressType,
    [property: JsonPropertyName("physicalPostalAddress")] BpdmAddressPhysicalPostalAddress? PhysicalPostalAddress,
    [property: JsonPropertyName("alternativePostalAddress")] BpdmAddressAlternativePostalAddress? AlternativePostalAddress,
    [property: JsonPropertyName("confidenceCriteria")] BpdmConfidenceCriteria ConfidenceCriteria,
    [property: JsonPropertyName("states")] IEnumerable<BpdmStatus> States
);

public record BpdmStatus(
    DateTimeOffset ValidFrom,
    DateTimeOffset ValidTo,
    string Type
);

public record BpdmCountry
(
    string TechnicalKey,
    string Name
);

public record BpdmProfileClassification(
    string Value,
    string Code,
    string Type
);

public record BpdmAddressState(
    string Description,
    DateTimeOffset? ValidFrom,
    DateTimeOffset? ValidTo,
    string Type
);

public record BpdmAddressIdentifier(
    string Value,
    BpdmIdentifierId Type
);

public record BpdmAddressPhysicalPostalAddress(
    BpdmGeographicCoordinatesDto? GeographicCoordinates,
    string? Country,
    string? AdministrativeAreaLevel1,
    string? AdministrativeAreaLevel2,
    string? AdministrativeAreaLevel3,
    string? PostalCode,
    string? City,
    string? District,
    BpdmLegalEntityStreet? Street,
    string? CompanyPostalCode,
    string? IndustrialZone,
    string? Building,
    string? Floor,
    string? Door
);

public record BpdmLegalEntityStreet(
    string? NamePrefix,
    string? AdditionalNamePrefix,
    string Name,
    string? NameSuffix,
    string? AdditionalNameSuffix,
    string? HouseNumber,
    string? Milestone,
    string? Direction
);

public record BpdmAddressAlternativePostalAddress(
    BpdmGeographicCoordinatesDto? GeographicCoordinates,
    string? Country,
    string? PostalCode,
    string? City,
    string? AdministrativeAreaLevel1,
    string? DeliveryServiceNumber,
    string? DeliveryServiceType,
    string? DeliveryServiceQualifier
);
