/********************************************************************************
 * Copyright (c) 2021,2022 Microsoft and BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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

namespace Org.Eclipse.TractusX.Portal.Backend.Checklist.Library.Bpdm.Models;

public record BpdmLegalEntityData(
     [property: JsonPropertyName("externalId")] string ExternalId,
     [property: JsonPropertyName("identifiers")] IEnumerable<BpdmIdentifiers> Identifiers,
     [property: JsonPropertyName("names")] IEnumerable<BpdmName> Names,
     [property: JsonPropertyName("legalAddress")] BpdmAddress LegalAddress
);

public record BpdmIdentifiers(
    [property: JsonPropertyName("value")] string Value,
    [property: JsonPropertyName("type")] string Type);

public record BpdmName(
    [property: JsonPropertyName("value")] string Value,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("language")] string Language);

public record BpdmAddress(
    [property: JsonPropertyName("version")] BpdmAddressVersion Version,
    [property: JsonPropertyName("country")] string Country,
    [property: JsonPropertyName("postCodes")] IEnumerable<BpdmPostcode> PostCodes,
    [property: JsonPropertyName("localities")] IEnumerable<BpdmLocality> Localities,
    [property: JsonPropertyName("thoroughfares")] IEnumerable<BpdmThoroughfares> Thoroughfares);

public record BpdmAddressVersion(
    [property: JsonPropertyName("characterSet")] string CharacterSet,
    [property: JsonPropertyName("language")] string Language);

public record BpdmPostcode(
    [property: JsonPropertyName("value")] string Value,
    [property: JsonPropertyName("type")] string Type);

public record BpdmLocality(
    [property: JsonPropertyName("value")] string Value,
    [property: JsonPropertyName("type")] string Type);

public record BpdmThoroughfares(
    [property: JsonPropertyName("value")] string Value,
    [property: JsonPropertyName("type")] string Type);
