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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library.Models;

public record BpdmIdentifier(
    BpdmIdentifierId Type,
    string Value,
    string? IssuingBody
);

public record BpdmState(
    DateTime ValidFrom,
    DateTime ValidTo,
    string Type
);

public record BpdmClassification(
    string Type,
    string Code,
    string Value
);

public record BpdmGeographicCoordinates(
    double Longitude,
    double Latitude,
    double Altitude
);

public record BpdmPutStreet(
    string? NamePrefix,
    string? AdditionalNamePrefix,
    string Name,
    string? NameSuffix,
    string? AdditionalNameSuffix,
    string? HouseNumber,
    string? HouseNumberSupplement,
    string? Milestone,
    string? Direction
);

public record BpdmPutPhysicalPostalAddress(
    BpdmGeographicCoordinates? GeographicCoordinates,
    string Country,
    string? AdministrativeAreaLevel1,
    string? AdministrativeAreaLevel2,
    string? AdministrativeAreaLevel3,
    string? PostalCode,
    string? City,
    string? District,
    BpdmPutStreet Street,
    string? CompanyPostalCode,
    string? IndustrialZone,
    string? Building,
    string? Floor,
    string? Door
);

public record BpdmPutAlternativePostalAddress(
    BpdmGeographicCoordinates? GeographicCoordinates,
    string Country,
    string AdministrativeAreaLevel1,
    string PostalCode,
    string City,
    string DeliveryServiceType,
    string DeliveryServiceQualifier,
    string DeliveryServiceNumber
);

public record BpdmAddress(
    string? AddressBpn,
    string? Name,
    string? AddressType,
    BpdmPutPhysicalPostalAddress PhysicalPostalAddress,
    BpdmPutAlternativePostalAddress? AlternativePostalAddress,
    IEnumerable<BpdmState> States
);

public record BpdmLegalEntity(
    string? LegalEntityBpn,
    string LegalName,
    string? ShortName,
    string? LegalForm,
    IEnumerable<BpdmState> States
);

public record BpdmSite(
    string SiteBpn,
    string Name,
    IEnumerable<BpdmState> States
);

public record BpdmLegalEntityData(
    string ExternalId,
    IEnumerable<string> NameParts,
    IEnumerable<BpdmIdentifier> Identifiers,
    IEnumerable<BpdmState> States,
    IEnumerable<string> Roles,
    BpdmLegalEntity LegalEntity,
    BpdmSite? Site,
    BpdmAddress Address,
    bool OwnCompanyData
);
