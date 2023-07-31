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

public record BpdmLegalEntityData(
    string ExternalId,
    IEnumerable<string> LegalNameParts,
    string? LegalShortName,
    string? LegalForm,
    IEnumerable<BpdmIdentifier> Identifiers,
    IEnumerable<BpdmStatus> States,
    IEnumerable<BpdmProfileClassification> Classifications,
    IEnumerable<string> Roles,
    BpdmLegalAddress LegalAddress
);

public record BpdmIdentifier(
    string Value,
    BpdmIdentifierId Type,
    string? IssuingBody
);

public record BpdmStatus(
    string OfficialDenotation,
    DateTimeOffset ValidFrom,
    DateTimeOffset ValidUntil,
    string Type
);

public record BpdmProfileClassification(
    string Value,
    string Code,
    string Type
);

public record BpdmLegalAddress(
    IEnumerable<string> NameParts,
    IEnumerable<BpdmAddressState> States,
    IEnumerable<BpdmAddressIdentifier> Identifiers,
    BpdmAddressPhysicalPostalAddress PhysicalPostalAddress,
    BpdmAddressAlternativePostalAddress? AlternativePostalAddress,
    IEnumerable<string> Roles
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
    string? PostalCode,
    string? City,
    BpdmStreet? Street,
    string? AdministrativeAreaLevel1,
    string? AdministrativeAreaLevel2,
    string? AdministrativeAreaLevel3,
    string? District,
    string? CompanyPostalCode,
    string? IndustrialZone,
    string? Building,
    string? Floor,
    string? Door
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
