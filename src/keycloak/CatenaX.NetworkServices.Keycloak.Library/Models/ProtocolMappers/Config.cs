/********************************************************************************
 * Copyright (c) 2021,2022 Contributors to https://github.com/lvermeulen/Keycloak.Net.git and BMW Group AG
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

using Newtonsoft.Json;

namespace CatenaX.NetworkServices.Keycloak.Library.Models.ProtocolMappers;

public class Config
{
    [JsonProperty("single")]
    public string Single { get; set; }
    [JsonProperty("attributenameformat")]
    public string AttributeNameFormat { get; set; }
    [JsonProperty("attributename")]
    public string AttributeName { get; set; }
    [JsonProperty("userinfo.token.claim")]
    public string UserInfoTokenClaim { get; set; }
    [JsonProperty("user.attribute")]
    public string UserAttribute { get; set; }
    [JsonProperty("id.token.claim")]
    public string IdTokenClaim { get; set; }
    [JsonProperty("access.token.claim")]
    public string AccessTokenClaim { get; set; }
    [JsonProperty("claim.name")]
    public string ClaimName { get; set; }
    [JsonProperty("jsonType.label")]
    public string JsonTypelabel { get; set; }
    [JsonProperty("userattributeformatted")]
    public string UserAttributeFormatted { get; set; }
    [JsonProperty("userattributecountry")]
    public string UserAttributeCountry { get; set; }
    [JsonProperty("userattributepostal_code")]
    public string UserAttributePostalCode { get; set; }
    [JsonProperty("userattributestreet")]
    public string UserAttributeStreet { get; set; }
    [JsonProperty("userattributeregion")]
    public string UserAttributeRegion { get; set; }
    [JsonProperty("userattributelocality")]
    public string UserAttributeLocality { get; set; }
    [JsonProperty("included.client.audience")]
    public string IncludedClientAudience {get; set; }
    [JsonProperty("multivalued")]
    public string Multivalued { get; set; }
}
