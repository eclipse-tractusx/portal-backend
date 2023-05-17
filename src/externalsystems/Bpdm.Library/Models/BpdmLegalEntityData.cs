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
	string? Bpn,
	IEnumerable<BpdmIdentifier> Identifiers,
	IEnumerable<BpdmName> Names,
	string? LegalForm,
	BpdmStatus? Status,
	IEnumerable<BpdmProfileClassification> ProfileClassifications,
	IEnumerable<string> Types,
	IEnumerable<BpdmBankAccount>? BankAccounts,
	BpdmLegalAddress LegalAddress
);

public record BpdmIdentifier(
	string Value,
	BpdmIdentifierId Type,
	string? IssuingBody,
	string? Status
);

public record BpdmName(
	string Value,
	string? ShortName,
	string Type,
	string Language
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

public record BpdmBankAccount(
	IEnumerable<int> TrustScores,
	string Currency,
	string InternationalBankAccountIdentifier,
	string InternationalBankIdentifier,
	string NationalBankAccountIdentifier,
	string NationalBankIdentifier
);

public record BpdmLegalAddress(
	BpdmAddressVersion Version,
	string? CareOf,
	IEnumerable<string> Contexts,
	string Country,
	IEnumerable<BpdmAdministrativeArea> AdministrativeAreas,
	IEnumerable<BpdmPostcode> PostCodes,
	IEnumerable<BpdmLocality> Localities,
	IEnumerable<BpdmThoroughfare> Thoroughfares,
	IEnumerable<BpdmPremise> Premises,
	IEnumerable<BpdmPostalDeliveryPoint> PostalDeliveryPoints,
	BpdmGeographicCoordinates? GeographicCoordinates,
	IEnumerable<string> Types
);

public record BpdmAddressVersion(
	string CharacterSet,
	string Language
);

public record BpdmAdministrativeArea(
	string Value,
	string? ShortName,
	string? FipsCode,
	string Type
);

public record BpdmPostcode(
	string Value,
	string Type
);

public record BpdmLocality(
	string Value,
	string? ShortName,
	string Type
);

public record BpdmThoroughfare(
	string Value,
	string? Name,
	string? ShortName,
	string? Number,
	string? Direction,
	string Type
);

public record BpdmPremise(
	string Value,
	string? ShortName,
	string? Number,
	string Type
);

public record BpdmPostalDeliveryPoint(
	string Value,
	string? ShortName,
	string? Number,
	string Type
);

public record BpdmGeographicCoordinates(
	int Longitude,
	int Latitude,
	int Altitude
);
