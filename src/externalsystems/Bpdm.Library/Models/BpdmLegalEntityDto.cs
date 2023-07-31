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
    IEnumerable<BpdmIdentifierDto> Identifiers,
    BpdmLegalFormDto? LegalForm,
    IEnumerable<BpdmStatusDto> Status,
    IEnumerable<BpdmProfileClassificationDto> ProfileClassifications,
    IEnumerable<BpdmRelationDto> Relations,
    BpdmLegalEntityAddress LegalEntityAddress
);

public record BpdmIdentifierDto(
    string Value,
    BpdmTechnicalKey Type,
    string IssuingBody
);

public record BpdmLegalFormDto(
    string TechnicalKey,
    string Name,
    string Abbreviation
);

public record BpdmStatusDto(
    string OfficialDenotation,
    DateTimeOffset ValidFrom,
    DateTimeOffset ValidUntil,
    BpdmTechnicalKey Type
);

public record BpdmProfileClassificationDto(
    string Value,
    string Code,
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
    string StartBpn,
    string EndBpn,
    DateTimeOffset ValidFrom,
    DateTimeOffset ValidTo
);

public record BpdmLegalEntityAddress
(
    string Bpnl,
    string Name,
    string BpnLegalEntity,
    string BpnSite,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    bool IsLegalAddress,
    bool IsMainAddress,
    IEnumerable<BpdmLegalEntityAddressState> States,
    IEnumerable<BpdmLegalEntityAddressIdentifier> Identifiers,
    BpdmPhysicalPostalAddress? PhysicalPostalAddress,
    BpdmAlternativePostalAddress? AlternativePostalAddress
);

public record BpdmLegalEntityAddressState
(
    string Description,
    DateTimeOffset ValidFrom,
    DateTimeOffset ValidTo,
    BpdmTechnicalKey Type
);

public record BpdmLegalEntityAddressIdentifier
(
    string Value,
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
    string? Type,
    string? DeliveryServiceQualifier
);

public record BpdmAdministrativeAreaLevel(
    string? Name,
    string? RegionCode
);

public record BpdmStreet(
    string? NamePrefix,
    string? AdditionalNamePrefix,
    string Name,
    string? NameSuffix,
    string? AdditionalNameSuffix,
    string? HouseNumber,
    string? Milestone,
    string? Direction
);
