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

namespace Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library.Models;

public record PageOutputResponseBpdmLegalEntityData(
    IEnumerable<BpdmLegalEntityOutputData> Content,
    IEnumerable<BpdmErrorInfo> Errors
);

public record BpdmLegalEntityOutputData(
    string ExternalId,
    string? Bpn,
    IEnumerable<BpdmIdentifier> Identifiers,
    IEnumerable<BpdmName> Names,
    BpdmLegalForm? LegalForm,
    BpdmStatus? Status,
    IEnumerable<BpdmProfileClassification> ProfileClassifications,
    IEnumerable<BpdmType> Types,
    IEnumerable<BpdmBankAccount>? BankAccounts,
    IEnumerable<BpdmRoles> Roles,
    IEnumerable<BpdmReations> Relations,
    BpdmLegalAddressResponse LegalAddress
);

public record BpdmLegalAddressResponse(
    BpdmAddressVersionResponse Version,
    string? CareOf,
    IEnumerable<string> Contexts,
    BpdmCountry Country,
    IEnumerable<BpdmAdministrativeAreaResponse> AdministrativeAreas,
    IEnumerable<BpdmPostcodeResponse> PostCodes,
    IEnumerable<BpdmLocalityResponse> Localities,
    IEnumerable<BpdmThoroughfareResponse> Thoroughfares,
    IEnumerable<BpdmPremiseResponse> Premises,
    IEnumerable<BpdmPostalDeliveryPointResponse> PostalDeliveryPoints,
    BpdmGeographicCoordinates? GeographicCoordinates,
    IEnumerable<BpdmType> Types
);

public record BpdmAddressVersionResponse(
    BpdmCharacterSet CharacterSet,
    BpdmLanguage Language
);

public record BpdmCharacterSet
(
    string TechnicalKey,
    string Name
);

public record BpdmCountry
(
    string TechnicalKey,
    string Name
);

public record BpdmAdministrativeAreaResponse(
    string Value,
    string? ShortName,
    string? FipsCode,
    BpdmType Type,
    BpdmLanguage Language
);

public record BpdmPostcodeResponse(
    string Value,
    BpdmType Type
);

public record BpdmLocalityResponse(
    string Value,
    string? ShortName,
    BpdmType Type,
    BpdmLanguage Language
);

public record BpdmThoroughfareResponse(
    string Value,
    string? Name,
    string? ShortName,
    string? Number,
    string? Direction,
    BpdmType Type,
    BpdmLanguage Language
);

public record BpdmPremiseResponse(
    string Value,
    string? ShortName,
    string? Number,
    BpdmType Type,
    BpdmLanguage Language
);

public record BpdmPostalDeliveryPointResponse(
    string Value,
    string? ShortName,
    string? Number,
    BpdmType Type,
    BpdmLanguage Language
);

public record BpdmErrorInfo(
    string ErrorCode,
    string Message,
    string EntityKey
);
