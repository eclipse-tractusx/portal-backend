/********************************************************************************
 * Copyright (c) 2021, 2023 BMW Group AG
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
using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;

public record ClearinghouseData(
    CompanyApplicationStatusId ApplicationStatusId,
    ParticipantDetails ParticipantDetails,
    IEnumerable<UniqueIdData> UniqueIds);

public record ParticipantDetails(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("city")] string? City,
    [property: JsonPropertyName("street")] string Street,
    [property: JsonPropertyName("bpn")] string? Bpn,
    [property: JsonPropertyName("region")] string? Region,
    [property: JsonPropertyName("zipCode")] string? ZipCode,
    [property: JsonPropertyName("country")] string Country,
    [property: JsonPropertyName("countryAlpha2Code")] string CountryAlpha2Code
);

public record UniqueIdData(string Type, string Value);
