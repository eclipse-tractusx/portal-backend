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

using CatenaX.NetworkServices.Keycloak.Library.Common.Converters;
using Newtonsoft.Json;

namespace CatenaX.NetworkServices.Keycloak.Library.Models.Clients;

public class AccessToken
{
    [JsonProperty("acr")]
    public string Acr { get; set; }
    [JsonProperty("address")]
    public AddressClaimSet Address { get; set; }
    [JsonProperty("allowed-origins")]
    public IEnumerable<string> AllowedOrigins { get; set; }
    [JsonProperty("at_hash")]
    public string AtHash { get; set; }
    [JsonProperty("auth_time")]
    public int? AuthTime { get; set; }
    [JsonProperty("authorization")]
    public AccessTokenAuthorization Authorization { get; set; }
    [JsonProperty("azp")]
    public string Azp { get; set; }
    [JsonProperty("birthdate")]
    public string Birthdate { get; set; }
    [JsonProperty("c_hash")]
    public string CHash { get; set; }
    [JsonProperty("category")]
    [JsonConverter(typeof(AccessTokenCategoriesConverter))]
    public AccessTokenCategories Category { get; set; }
    [JsonProperty("claims_locales")]
    public string ClaimsLocales { get; set; }
    [JsonProperty("cnf")]
    public AccessTokenCertConf Cnf { get; set; }
    [JsonProperty("email")]
    public string Email { get; set; }
    [JsonProperty("email_verified")]
    public bool? EmailVerified { get; set; }
    [JsonProperty("exp")]
    public int? Exp { get; set; }
    [JsonProperty("family_name")]
    public string FamilyName { get; set; }
    [JsonProperty("gender")]
    public string Gender { get; set; }
    [JsonProperty("given_name")]
    public string GivenName { get; set; }
    [JsonProperty("iat")]
    public int? Iat { get; set; }
    [JsonProperty("iss")]
    public string Iss { get; set; }
    [JsonProperty("jti")]
    public string Jti { get; set; }
    [JsonProperty("locale")]
    public string Locale { get; set; }
    [JsonProperty("middle_name")]
    public string MiddleName { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("nickname")]
    public string Nickname { get; set; }
    [JsonProperty("nonce")]
    public string Nonce { get; set; }
    [JsonProperty("otherClaims")]
    public IDictionary<string, object> OtherClaims { get; set; }
    [JsonProperty("phone_number")]
    public string PhoneNumber { get; set; }
    [JsonProperty("phone_number_verified")]
    public bool? PhoneNumberVerified { get; set; }
    [JsonProperty("picture")]
    public string Picture { get; set; }
    [JsonProperty("preferred_username")]
    public string PreferredUsername { get; set; }
    [JsonProperty("profile")]
    public string Profile { get; set; }
    [JsonProperty("realm_access")]
    public AccessTokenAccess RealmAccess { get; set; }
    [JsonProperty("s_hash")]
    public string Hash { get; set; }
    [JsonProperty("scope")]
    public string Scope { get; set; }
    [JsonProperty("session_state")]
    public string SessionState { get; set; }
    [JsonProperty("sub")]
    public string Sub { get; set; }
    [JsonProperty("trusted-certs")]
    public IEnumerable<string> TrustedCerts { get; set; }
    [JsonProperty("typ")]
    public string Typ { get; set; }
    [JsonProperty("updated_at")]
    public long UpdatedAt { get; set; }
    [JsonProperty("website")]
    public string Website { get; set; }
    [JsonProperty("zoneinfo")]
    public string Zoneinfo { get; set; }
}
