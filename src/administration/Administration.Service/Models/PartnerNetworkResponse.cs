/********************************************************************************
 * Copyright (c) 2022 BMW Group AG
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;

public record PartnerNetworkResponse(
    IEnumerable<PartnerNetworkData> Content,
    int ContentSize,
    int Page,
    int TotalElements,
    int TotalPages
);

public record PartnerNetworkData(
    string Bpn,
    string? LegalName,
    string? LegalShortName,
    DateTimeOffset Currentness,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IEnumerable<BpdmIdentifierData> Identifiers,
    BpdmLegalFormData? LegalForm,
    IEnumerable<BpdmStatusData> States,
    BpdmConfidenceCriteriaData? ConfidenceCriteria,
    bool IsCatenaXMemberData,
    IEnumerable<BpdmRelationData> Relations,
    BpdmLegalEntityAddressData? LegalEntityAddress
);

public record BpdmIdentifierData(
    string Value,
    BpdmTechnicalKeyData Type,
    string? IssuingBody
);

public record BpdmTechnicalKeyData(
    string TechnicalKey,
    string Name
);

public record BpdmLegalFormData(
    string? TechnicalKey,
    string? Name,
    string? Abbreviation
);

public record BpdmStatusData(
    DateTimeOffset? ValidFrom,
    DateTimeOffset? ValidTo,
    BpdmTechnicalKeyData Type
);

public record BpdmConfidenceCriteriaData(
    bool SharedByOwner,
    bool CheckedByExternalDataSource,
    int NumberOfSharingMembers,
    DateTime LastConfidenceCheckAt,
    DateTime NextConfidenceCheckAt,
    int ConfidenceLevel
);

public record BpdmRelationData(
    BpdmTechnicalKeyData Type,
    string? StartBpnl,
    string? EndBpnl,
    DateTimeOffset? ValidFrom,
    DateTimeOffset? ValidTo
);

public record BpdmLegalEntityAddressData
(
    string? Bpna,
    string? Name,
    string? BpnLegalEntity,
    string? BpnSite,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string AddressType,
    IEnumerable<BpdmLegalEntityAddressStateData> States,
    IEnumerable<BpdmLegalEntityAddressIdentifierData> Identifiers,
    BpdmPhysicalPostalAddressData? PhysicalPostalAddress,
    BpdmAlternativePostalAddressData? AlternativePostalAddress,
    BpdmConfidenceCriteriaData? ConfidenceCriteria,
    bool IsCatenaXMemberData
);

public record BpdmLegalEntityAddressStateData
(
    string? Description,
    DateTimeOffset? ValidFrom,
    DateTimeOffset? ValidTo,
    BpdmTechnicalKeyData Type
);

public record BpdmLegalEntityAddressIdentifierData
(
    string? Value,
    BpdmTechnicalKeyData Type
);

public record BpdmPhysicalPostalAddressData(
    BpdmGeographicCoordinatesData? GeographicCoordinates,
    BpdmCountryData? Country,
    string? PostalCode,
    string? City,
    BpdmStreetData? Street,
    BpdmAdministrativeAreaLevelData? AdministrativeAreaLevel1,
    string? AdministrativeAreaLevel2,
    string? AdministrativeAreaLevel3,
    string? District,
    string? CompanyPostalCode,
    string? IndustrialZone,
    string? Building,
    string? Floor,
    string? Door
);

public record BpdmAlternativePostalAddressData(
    BpdmGeographicCoordinatesData? GeographicCoordinates,
    BpdmCountryData? Country,
    string? PostalCode,
    string? City,
    BpdmAdministrativeAreaLevelData? AdministrativeAreaLevel1,
    string? DeliveryServiceNumber,
    string? DeliveryServiceType,
    string? DeliveryServiceQualifier
);

public record BpdmGeographicCoordinatesData(
    double Longitude,
    double Latitude,
    double? Altitude
);

public record BpdmCountryData
(
    string TechnicalKey,
    string Name
);

public record BpdmStreetData(
    string? Name,
    string? HouseNumber,
    string? Milestone,
    string? Direction
);

public record BpdmAdministrativeAreaLevelData(
    string? CountryCode,
    string? RegionName,
    string? RegionCode
);
