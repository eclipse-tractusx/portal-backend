/********************************************************************************
 * MIT License
 *
 * Copyright (c) 2019 Luk Vermeulen
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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

using Newtonsoft.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Users;

public class User
{
    [JsonProperty("id")]
    public string? Id { get; set; }
    [JsonProperty("createdTimestamp")]
    public long? CreatedTimestamp { get; set; }
    [JsonProperty("username")]
    public string UserName { get; set; } = default!;
    [JsonProperty("enabled")]
    public bool? Enabled { get; set; }
    [JsonProperty("totp")]
    public bool? Totp { get; set; }
    [JsonProperty("emailVerified")]
    public bool? EmailVerified { get; set; }
    [JsonProperty("firstName")]
    public string? FirstName { get; set; }
    [JsonProperty("lastName")]
    public string? LastName { get; set; }
    [JsonProperty("email")]
    public string? Email { get; set; }
    [JsonProperty("disableableCredentialTypes")]
    public IEnumerable<string>? DisableableCredentialTypes { get; set; }
    [JsonProperty("requiredActions")]
    public IEnumerable<string>? RequiredActions { get; set; }
    [JsonProperty("notBefore")]
    public int? NotBefore { get; set; }
    [JsonProperty("access")]
    public UserAccess? Access { get; set; }
    [JsonProperty("attributes")]
    public IDictionary<string, IEnumerable<string>>? Attributes { get; set; }
    [JsonProperty("clientConsents")]
    public IEnumerable<UserConsent>? ClientConsents { get; set; }
    [JsonProperty("clientRoles")]
    public IDictionary<string, object>? ClientRoles { get; set; }
    [JsonProperty("credentials")]
    public IEnumerable<Credentials>? Credentials { get; set; }
    [JsonProperty("federatedIdentities")]
    public IEnumerable<FederatedIdentity>? FederatedIdentities { get; set; }
    [JsonProperty("federationLink")]
    public string? FederationLink { get; set; }
    [JsonProperty("groups")]
    public IEnumerable<string>? Groups { get; set; }
    [JsonProperty("origin")]
    public string? Origin { get; set; }
    [JsonProperty("realmRoles")]
    public IEnumerable<string>? RealmRoles { get; set; }
    [JsonProperty("self")]
    public string? Self { get; set; }
    [JsonProperty("serviceAccountClientId")]
    public string? ServiceAccountClientId { get; set; }
}
