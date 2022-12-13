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

using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;

public record CompanyWithAddress(
    [property: JsonPropertyName("companyId")] Guid CompanyId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("city")] string City,
    [property: JsonPropertyName("streetName")] string StreetName,
    [property: JsonPropertyName("countryAlpha2Code")] string CountryAlpha2Code)
{
    [JsonPropertyName("bpn")]
    public string? BusinessPartnerNumber { get; set; }

    [JsonPropertyName("shortName")]
    public string? Shortname { get; set; }

    [JsonPropertyName("region")]
    public string? Region { get; set; }

    [JsonPropertyName("streetAdditional")]
    public string? Streetadditional { get; set; }

    [JsonPropertyName("streetNumber")]
    public string? Streetnumber { get; set; }

    [JsonPropertyName("zipCode")]
    public string? Zipcode { get; set; }

    [JsonPropertyName("countryDe")]
    public string? CountryDe { get; set; }

    [JsonPropertyName("taxId")]
    public string? TaxId { get; set; }
}