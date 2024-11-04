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

using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Users;

public class User
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    [JsonPropertyName("createdTimestamp")]
    public long? CreatedTimestamp { get; set; }
    [JsonPropertyName("username")]
    public string? UserName { get; set; } = default!;
    [JsonPropertyName("enabled")]
    public bool? Enabled { get; set; }
    [JsonPropertyName("totp")]
    public bool? Totp { get; set; }
    [JsonPropertyName("emailVerified")]
    public bool? EmailVerified { get; set; }
    [JsonPropertyName("firstName")]
    public string? FirstName { get; set; }
    [JsonPropertyName("lastName")]
    public string? LastName { get; set; }
    [JsonPropertyName("email")]
    public string? Email { get; set; }
    [JsonPropertyName("disableableCredentialTypes")]
    public IEnumerable<string>? DisableableCredentialTypes { get; set; }
    [JsonPropertyName("requiredActions")]
    public IEnumerable<string>? RequiredActions { get; set; }
    [JsonPropertyName("notBefore")]
    public int? NotBefore { get; set; }
    [JsonPropertyName("access")]
    public UserAccess? Access { get; set; }
    [JsonPropertyName("attributes")]
    public IDictionary<string, IEnumerable<string>?>? Attributes { get; set; }
    [JsonPropertyName("clientConsents")]
    public IEnumerable<UserConsent>? ClientConsents { get; set; }
    [JsonPropertyName("clientRoles")]
    public IDictionary<string, IEnumerable<string>>? ClientRoles { get; set; }
    [JsonPropertyName("credentials")]
    public IEnumerable<Credentials>? Credentials { get; set; }
    [JsonPropertyName("federatedIdentities")]
    public IEnumerable<FederatedIdentity>? FederatedIdentities { get; set; }
    [JsonPropertyName("federationLink")]
    public string? FederationLink { get; set; }
    [JsonPropertyName("groups")]
    public IEnumerable<string>? Groups { get; set; }
    [JsonPropertyName("origin")]
    public string? Origin { get; set; }
    [JsonPropertyName("realmRoles")]
    public IEnumerable<string>? RealmRoles { get; set; }
    [JsonPropertyName("self")]
    public string? Self { get; set; }
    [JsonPropertyName("serviceAccountClientId")]
    public string? ServiceAccountClientId { get; set; }
}
