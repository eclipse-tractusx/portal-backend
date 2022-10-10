/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
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

namespace Org.CatenaX.Ng.Portal.Backend.Administration.Service.Models;

/// <summary>
/// Model used to request connector registration at sd factory.
/// </summary>
public record SdFactoryRequestModel(
    [property: JsonPropertyName("registrationNumber")] string RegistrationNumber,
    [property: JsonPropertyName("headquarterAddress.country")] string HeadquarterCountry,
    [property: JsonPropertyName("legalAddress.country")] string LegalCountry,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("bpn")] string Bpn,
    [property: JsonPropertyName("holder")] string Holder,
    [property: JsonPropertyName("issuer")] string Issuer);
