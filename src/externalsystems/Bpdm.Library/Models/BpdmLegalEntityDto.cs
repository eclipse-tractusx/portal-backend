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

public record BpdmLegalEntityDto(
    [property: JsonPropertyName("bpnl")] string Bpn,
    [property: JsonPropertyName("legalName")] string? LegalName,
    [property: JsonPropertyName("legalShortName")] string? LegalShortName,
    [property: JsonPropertyName("currentness")] DateTimeOffset Currentness,
    [property: JsonPropertyName("createdAt")] DateTimeOffset CreatedAt,
    [property: JsonPropertyName("updatedAt")] DateTimeOffset UpdatedAt,
    [property: JsonPropertyName("identifiers")] IEnumerable<BpdmIdentifierDto> Identifiers,
    [property: JsonPropertyName("legalForm")] BpdmLegalFormDto? LegalForm,
    [property: JsonPropertyName("states")] IEnumerable<BpdmStatusDto> States,
    [property: JsonPropertyName("confidenceCriteria")] BpdmConfidenceCriteria? ConfidenceCriteria,
    [property: JsonPropertyName("isCatenaXMemberData")] bool IsCatenaXMemberData,
    [property: JsonPropertyName("relations")] IEnumerable<BpdmRelationDto> Relations,
    [property: JsonPropertyName("legalAddress")] BpdmLegalEntityAddress? LegalEntityAddress
);

public record BpdmIdentifierDto(
    string Value,
    BpdmTechnicalKey Type,
    string? IssuingBody
);

public record BpdmLegalFormDto(
    string? TechnicalKey,
    string? Name,
    string? Abbreviation
);

public record BpdmStatusDto(
    DateTimeOffset? ValidFrom,
    DateTimeOffset? ValidTo,
    BpdmTechnicalKey Type
);

public record BpdmDataDto(
    string TechnicalKey,
    string Name
);

public record BpdmTechnicalKey(
    string TechnicalKey,
    string Name
);

public record BpdmRelationDto(
    BpdmTechnicalKey Type,
    string? StartBpnl,
    string? EndBpnl,
    DateTimeOffset? ValidFrom,
    DateTimeOffset? ValidTo
);

public record BpdmLegalEntityAddress
(
    string? Bpna,
    string? Name,
    string? BpnLegalEntity,
    string? BpnSite,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string AddressType,
    IEnumerable<BpdmLegalEntityAddressState> States,
    IEnumerable<BpdmLegalEntityAddressIdentifier> Identifiers,
    BpdmPhysicalPostalAddress? PhysicalPostalAddress,
    BpdmAlternativePostalAddress? AlternativePostalAddress,
    [property: JsonPropertyName("confidenceCriteria")] BpdmConfidenceCriteria? ConfidenceCriteria,
    [property: JsonPropertyName("isCatenaXMemberData")] bool IsCatenaXMemberData
);

public record BpdmLegalEntityAddressState
(
    string? Description,
    DateTimeOffset? ValidFrom,
    DateTimeOffset? ValidTo,
    BpdmTechnicalKey Type
);

public record BpdmLegalEntityAddressIdentifier
(
    string? Value,
    BpdmTechnicalKey Type
);

public record BpdmPhysicalPostalAddress(
    BpdmGeographicCoordinatesDto? GeographicCoordinates,
    BpdmCountry? Country,
    string? PostalCode,
    string? City,
    BpdmStreet? Street,
    BpdmAdministrativeAreaLevel? AdministrativeAreaLevel1,
    string? AdministrativeAreaLevel2,
    string? AdministrativeAreaLevel3,
    string? District,
    string? CompanyPostalCode,
    string? IndustrialZone,
    string? Building,
    string? Floor,
    string? Door
);

public record BpdmAlternativePostalAddress(
    BpdmGeographicCoordinatesDto? GeographicCoordinates,
    BpdmCountry? Country,
    string? PostalCode,
    string? City,
    BpdmAdministrativeAreaLevel? AdministrativeAreaLevel1,
    string? DeliveryServiceNumber,
    string? DeliveryServiceType,
    string? DeliveryServiceQualifier
);

public record BpdmAdministrativeAreaLevel(
    string? CountryCode,
    string? RegionName,
    string? RegionCode
);

public record BpdmStreet(
    string? Name,
    string? HouseNumber,
    string? Milestone,
    string? Direction
);
