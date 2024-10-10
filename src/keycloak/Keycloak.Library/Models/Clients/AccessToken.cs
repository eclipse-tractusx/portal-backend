/********************************************************************************
 * MIT License
 *
 * Copyright (c) 2019 Luk Vermeulen
 * Copyright (c) 2022 BMW Group AG
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 ********************************************************************************/

using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Clients;

public class AccessToken
{
    [JsonPropertyName("acr")]
    public string Acr { get; set; }
    [JsonPropertyName("address")]
    public AddressClaimSet Address { get; set; }
    [JsonPropertyName("allowed-origins")]
    public IEnumerable<string> AllowedOrigins { get; set; }
    [JsonPropertyName("at_hash")]
    public string AtHash { get; set; }
    [JsonPropertyName("auth_time")]
    public int? AuthTime { get; set; }
    [JsonPropertyName("authorization")]
    public AccessTokenAuthorization Authorization { get; set; }
    [JsonPropertyName("azp")]
    public string Azp { get; set; }
    [JsonPropertyName("birthdate")]
    public string Birthdate { get; set; }
    [JsonPropertyName("c_hash")]
    public string CHash { get; set; }
    [JsonPropertyName("category")]
    [JsonConverter(typeof(EnumMemberConverter<AccessTokenCategories>))]
    public AccessTokenCategories Category { get; set; }
    [JsonPropertyName("claims_locales")]
    public string ClaimsLocales { get; set; }
    [JsonPropertyName("cnf")]
    public AccessTokenCertConf Cnf { get; set; }
    [JsonPropertyName("email")]
    public string Email { get; set; }
    [JsonPropertyName("email_verified")]
    public bool? EmailVerified { get; set; }
    [JsonPropertyName("exp")]
    public int? Exp { get; set; }
    [JsonPropertyName("family_name")]
    public string FamilyName { get; set; }
    [JsonPropertyName("gender")]
    public string Gender { get; set; }
    [JsonPropertyName("given_name")]
    public string GivenName { get; set; }
    [JsonPropertyName("iat")]
    public int? Iat { get; set; }
    [JsonPropertyName("iss")]
    public string Iss { get; set; }
    [JsonPropertyName("jti")]
    public string Jti { get; set; }
    [JsonPropertyName("locale")]
    public string Locale { get; set; }
    [JsonPropertyName("middle_name")]
    public string MiddleName { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("nickname")]
    public string Nickname { get; set; }
    [JsonPropertyName("nonce")]
    public string Nonce { get; set; }
    [JsonPropertyName("otherClaims")]
    public IDictionary<string, object> OtherClaims { get; set; }
    [JsonPropertyName("phone_number")]
    public string PhoneNumber { get; set; }
    [JsonPropertyName("phone_number_verified")]
    public bool? PhoneNumberVerified { get; set; }
    [JsonPropertyName("picture")]
    public string Picture { get; set; }
    [JsonPropertyName("preferred_username")]
    public string PreferredUsername { get; set; }
    [JsonPropertyName("profile")]
    public string Profile { get; set; }
    [JsonPropertyName("realm_access")]
    public AccessTokenAccess RealmAccess { get; set; }
    [JsonPropertyName("s_hash")]
    public string Hash { get; set; }
    [JsonPropertyName("scope")]
    public string Scope { get; set; }
    [JsonPropertyName("session_state")]
    public string SessionState { get; set; }
    [JsonPropertyName("sub")]
    public string Sub { get; set; }
    [JsonPropertyName("trusted-certs")]
    public IEnumerable<string> TrustedCerts { get; set; }
    [JsonPropertyName("typ")]
    public string Typ { get; set; }
    [JsonPropertyName("updated_at")]
    public long UpdatedAt { get; set; }
    [JsonPropertyName("website")]
    public string Website { get; set; }
    [JsonPropertyName("zoneinfo")]
    public string Zoneinfo { get; set; }
}
