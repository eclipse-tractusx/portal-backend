/********************************************************************************
 * MIT License
 *
 * Copyright (c) 2019 Luk Vermeulen
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

using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.ProtocolMappers;
using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Clients;

public class Client
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    [JsonPropertyName("clientId")]
    public string? ClientId { get; set; }
    [JsonPropertyName("rootUrl")]
    public string? RootUrl { get; set; }
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    [JsonPropertyName("baseUrl")]
    public string? BaseUrl { get; set; }
    [JsonPropertyName("surrogateAuthRequired")]
    public bool? SurrogateAuthRequired { get; set; }
    [JsonPropertyName("enabled")]
    public bool? Enabled { get; set; }
    [JsonPropertyName("alwaysDisplayInConsole")]
    public bool? AlwaysDisplayInConsole { get; set; }
    [JsonPropertyName("clientAuthenticatorType")]
    public string? ClientAuthenticatorType { get; set; }
    [JsonPropertyName("redirectUris")]
    public IEnumerable<string>? RedirectUris { get; set; }
    [JsonPropertyName("webOrigins")]
    public IEnumerable<string>? WebOrigins { get; set; }
    [JsonPropertyName("notBefore")]
    public int? NotBefore { get; set; }
    [JsonPropertyName("bearerOnly")]
    public bool? BearerOnly { get; set; }
    [JsonPropertyName("consentRequired")]
    public bool? ConsentRequired { get; set; }
    [JsonPropertyName("standardFlowEnabled")]
    public bool? StandardFlowEnabled { get; set; }
    [JsonPropertyName("implicitFlowEnabled")]
    public bool? ImplicitFlowEnabled { get; set; }
    [JsonPropertyName("directAccessGrantsEnabled")]
    public bool? DirectAccessGrantsEnabled { get; set; }
    [JsonPropertyName("serviceAccountsEnabled")]
    public bool? ServiceAccountsEnabled { get; set; }
    [JsonPropertyName("publicClient")]
    public bool? PublicClient { get; set; }
    [JsonPropertyName("frontchannelLogout")]
    public bool? FrontChannelLogout { get; set; }
    [JsonPropertyName("protocol")]
    public string? Protocol { get; set; }
    [JsonPropertyName("attributes")]
    public IDictionary<string, string>? Attributes { get; set; }
    [JsonPropertyName("authenticationFlowBindingOverrides")]
    public IDictionary<string, string>? AuthenticationFlowBindingOverrides { get; set; }
    [JsonPropertyName("fullScopeAllowed")]
    public bool? FullScopeAllowed { get; set; }
    [JsonPropertyName("nodeReRegistrationTimeout")]
    public int? NodeReregistrationTimeout { get; set; }
    [JsonPropertyName("protocolMappers")]
    public IEnumerable<ProtocolMapper>? ProtocolMappers { get; set; }
    [JsonPropertyName("defaultClientScopes")]
    public IEnumerable<string>? DefaultClientScopes { get; set; }
    [JsonPropertyName("optionalClientScopes")]
    public IEnumerable<string>? OptionalClientScopes { get; set; }
    [JsonPropertyName("access")]
    public ClientAccess? Access { get; set; }
    [JsonPropertyName("secret")]
    public string? Secret { get; set; }
    [JsonPropertyName("authorizationServicesEnabled")]
    public bool? AuthorizationServicesEnabled { get; set; }
    [JsonPropertyName("adminUrl")]
    public string? AdminUrl { get; set; }
}
