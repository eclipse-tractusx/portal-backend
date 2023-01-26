/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
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

using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library.Models;

/// <summary>
/// Model used to request connector registration at sd factory.
/// </summary>
public record SdFactoryRequestModel(
    [property: JsonPropertyName("registrationNumber")] IEnumerable<RegistrationNumber> RegistrationNumber,
    [property: JsonPropertyName("headquarterAddress.country")] string HeadquarterCountry,
    [property: JsonPropertyName("legalAddress.country")] string LegalCountry,
    [property: JsonPropertyName("type"), JsonConverter(typeof(JsonStringEnumConverter))] SdFactoryRequestModelSdType Type,
    [property: JsonPropertyName("bpn")] string Bpn,
    [property: JsonPropertyName("holder")] string Holder,
    [property: JsonPropertyName("issuer")] string Issuer);

public record RegistrationNumber(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("value")] string Value
);

/// <summary>
/// Model used to request connector registration at sd factory.
/// </summary>
public record ConnectorSdFactoryRequestModel(
    [property: JsonPropertyName("type"), JsonConverter(typeof(JsonStringEnumConverter))] SdFactoryRequestModelSdType Type,
    [property: JsonPropertyName("providedBy")] string ProvidedBy,
    [property: JsonPropertyName("aggregationOf")] string? AggregationOf,
    [property: JsonPropertyName("termsAndConditions")] string? TermsAndConditions,
    [property: JsonPropertyName("policies")] string Policies,
    [property: JsonPropertyName("issuer")] string Issuer,
    [property: JsonPropertyName("holder")] string Holder
);

public enum SdFactoryRequestModelSdType
{
    [EnumMember(Value = "LegalPerson")]
    LegalPerson,

    [EnumMember(Value = "ServiceOffering")]
    ServiceOffering
}

public enum SdFactoryResponseModelTitle
{
    [EnumMember(Value = "Connector")]
    Connector,

    [EnumMember(Value = "LegalPerson")]
    LegalPerson
}
