/********************************************************************************
 * Copyright (c) 2021,2022 Microsoft and BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
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

namespace Org.CatenaX.Ng.Portal.Backend.Registration.Service.Bpn.Model;

public record BpdmLegalEntityData(
     [property: JsonPropertyName("externalId")] string ExternalId,
     [property: JsonPropertyName("bpn")] string Bpn,
     [property: JsonPropertyName("identifiers")] IEnumerable<BpdmIdentifiers> Identifiers,
     [property: JsonPropertyName("names")] IEnumerable<BpdmName> Names,
     [property: JsonPropertyName("legalForm")] string? LegalForm,
     [property: JsonPropertyName("status")] BpdmBusinessStatus? Status,
     [property: JsonPropertyName("profileClassifications")] IEnumerable<BpdmClassification> ProfileClassifications,
     [property: JsonPropertyName("types")] IEnumerable<string> Types,
     [property: JsonPropertyName("bankAccounts")] IEnumerable<BpdmBankAccounts> BankAccounts,
     [property: JsonPropertyName("legalAddress")] BpdmAddress LegalAddress
);

public record BpdmIdentifiers(
    [property: JsonPropertyName("value")] string Value,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("issuingBody")] string? IssuingBody,
    [property: JsonPropertyName("status")] string? Status);

public record BpdmName(
    [property: JsonPropertyName("value")] string? Value,
    [property: JsonPropertyName("shortName")] string? ShortName,
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("language")] string? Language);

public record BpdmBusinessStatus(
    [property: JsonPropertyName("officialDenotation")] string? OfficialDenotation,
    [property: JsonPropertyName("validFrom")] string? ValidFrom,
    [property: JsonPropertyName("validUntil")] string? ValidUntil,
    [property: JsonPropertyName("type")] string? Type);

public record BpdmClassification(
    [property: JsonPropertyName("value")] string Value,
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("type")] string Type);

public record BpdmBankAccounts(
    [property: JsonPropertyName("trustScores")] string? TrustScores,
    [property: JsonPropertyName("currency")] string? Currency,
    [property: JsonPropertyName("internationalBankAccountIdentifier")] string? InternationalBankAccountIdentifier,
    [property: JsonPropertyName("internationalBankIdentifier")] string? InternationalBankIdentifier,
    [property: JsonPropertyName("nationalBankAccountIdentifier")] string? NationalBankAccountIdentifier,
    [property: JsonPropertyName("nationalBankIdentifier")] string? NationalBankIdentifier);

public record BpdmAddress(
    [property: JsonPropertyName("version")] BpdmAddressVersion Version,
    [property: JsonPropertyName("careOf")] string? CareOf,
    [property: JsonPropertyName("contexts")] IEnumerable<string> Contexts,
    [property: JsonPropertyName("country")] string Country,
    [property: JsonPropertyName("administrativeAreas")] IEnumerable<BpdmAministrativeArea> AdministrativeAreas,
    [property: JsonPropertyName("postCodes")] IEnumerable<BpdmPostcode> PostCodes,
    [property: JsonPropertyName("localities")] IEnumerable<BpdmLocality> Localities,
    [property: JsonPropertyName("thoroughfares")] IEnumerable<BpdmThoroughfares> Thoroughfares,
    [property: JsonPropertyName("premises")] IEnumerable<BpdmPremises> Premises,
    [property: JsonPropertyName("postalDeliveryPoints")] IEnumerable<BpdmPostalDeliveryPoints> PostalDeliveryPoints,
    [property: JsonPropertyName("geographicCoordinates")] IEnumerable<BpdmGeoCoordinates> GeographicCoordinates,
    [property: JsonPropertyName("types")] IEnumerable<string> Types);

public record BpdmAddressVersion(
    [property: JsonPropertyName("characterSet")] string CharacterSet,
    [property: JsonPropertyName("language")] string Language);

public record BpdmAministrativeArea(
    [property: JsonPropertyName("value")] string Value,
    [property: JsonPropertyName("shortName")] string? ShortName,
    [property: JsonPropertyName("fipsCode")] string? FipsCode,
    [property: JsonPropertyName("type")] string? Type);

public record BpdmPostcode(
    [property: JsonPropertyName("value")] string Value,
    [property: JsonPropertyName("type")] string Type);

public record BpdmLocality(
    [property: JsonPropertyName("value")] string Value,
    [property: JsonPropertyName("shortName")] string? ShortName,
    [property: JsonPropertyName("type")] string Type);

public record BpdmThoroughfares(
    [property: JsonPropertyName("value")] string Value,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("shortName")] string? ShortName,
    [property: JsonPropertyName("number")] string? Number,
    [property: JsonPropertyName("direction")] string? Direction,
    [property: JsonPropertyName("type")] string Type);

public record BpdmPremises(
    [property: JsonPropertyName("value")] string Value,
    [property: JsonPropertyName("shortName")] string? ShortName,
    [property: JsonPropertyName("number")] string? Number,
    [property: JsonPropertyName("type")] string Type);

public record BpdmPostalDeliveryPoints(
    [property: JsonPropertyName("value")] string Value,
    [property: JsonPropertyName("shortName")] string? ShortName,
    [property: JsonPropertyName("number")] string? Number,
    [property: JsonPropertyName("type")] string Type);

public record BpdmGeoCoordinates(
    [property: JsonPropertyName("longitude")] float Longitude,
    [property: JsonPropertyName("latitude")] string? Latitude,
    [property: JsonPropertyName("altitude")] string? Altitude);
