/********************************************************************************
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

using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.Clearinghouse.Library.Models;

public record ClearinghouseTransferData(
    [property: JsonPropertyName("legalEntity")] LegalEntity LegalEntity,
    [property: JsonPropertyName("validationMode")] string ValidationMode,
    [property: JsonPropertyName("callback")] CallBack Callback
);

public record LegalEntity(
    [property: JsonPropertyName("legalName")] string LegalName,
    [property: JsonPropertyName("address")] LegalAddress Address,
    [property: JsonPropertyName("identifiers")] IEnumerable<UniqueIdData> Identifiers
);

public record LegalAddress(
    [property: JsonPropertyName("country")] string CountryAlpha2Code,
    [property: JsonPropertyName("region")] string Region,
    [property: JsonPropertyName("locality")] string City,
    [property: JsonPropertyName("postalCode")] string? ZipCode,
    [property: JsonPropertyName("addressLine")] string AddressLine
);

public record UniqueIdData(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("value")] string Value
);

public record CallBack(
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("headers")] IReadOnlyDictionary<string, string> Headers
);
