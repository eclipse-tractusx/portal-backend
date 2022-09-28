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

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;

/// <summary>
/// Model used to request connector registration at sd factory.
/// </summary>
public class ConnectorSdFactoryRequestModel
{
    public ConnectorSdFactoryRequestModel(string companyNumber, string headquarterCountry, string legalCountry, string serviceProvider, string sdType, string bpn, string holder, string issuer)
    {
        CompanyNumber = companyNumber;
        HeadquarterCountry = headquarterCountry;
        LegalCountry = legalCountry;
        ServiceProvider = serviceProvider;
        SdType = sdType;
        Bpn = bpn;
        Holder = holder;
        Issuer = issuer;
    }

    [JsonPropertyName("company_number")]
    public string CompanyNumber { get; set; }

    [JsonPropertyName("headquarter_country")]
    public string HeadquarterCountry { get; set; }

    [JsonPropertyName("legal_country")]
    public string LegalCountry { get; set; }

    [JsonPropertyName("service_provider")]
    public string ServiceProvider { get; set; }

    [JsonPropertyName("sd_type")]
    public string SdType { get; set; }

    [JsonPropertyName("bpn")]
    public string Bpn { get; set; }

    [JsonPropertyName("holder")]
    public string Holder { get; set; }

    [JsonPropertyName("issuer")]
    public string Issuer { get; set; }
}
