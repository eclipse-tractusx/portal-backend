/********************************************************************************
 * MIT License
 *
 * Copyright (c) 2019 Luk Vermeulen
 * Copyright (c) 2021, 2023 BMW Group AG
 * Copyright (c) 2021, 2023 Contributors to the Eclipse Foundation
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

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Clients;

public class Client
{
	[JsonProperty("id")]
	public string Id { get; set; }
	[JsonProperty("clientId")]
	public string ClientId { get; set; }
	[JsonProperty("rootUrl")]
	public string RootUrl { get; set; }
	[JsonProperty("name")]
	public string Name { get; set; }
	[JsonProperty("baseUrl")]
	public string BaseUrl { get; set; }
	[JsonProperty("surrogateAuthRequired")]
	public bool? SurrogateAuthRequired { get; set; }
	[JsonProperty("enabled")]
	public bool? Enabled { get; set; }
	[JsonProperty("alwaysDisplayInConsole")]
	public bool? AlwaysDisplayInConsole { get; set; }
	[JsonProperty("clientAuthenticatorType")]
	public string ClientAuthenticatorType { get; set; }
	[JsonProperty("redirectUris")]
	public IEnumerable<string> RedirectUris { get; set; }
	[JsonProperty("webOrigins")]
	public IEnumerable<string> WebOrigins { get; set; }
	[JsonProperty("notBefore")]
	public int? NotBefore { get; set; }
	[JsonProperty("bearerOnly")]
	public bool? BearerOnly { get; set; }
	[JsonProperty("consentRequired")]
	public bool? ConsentRequired { get; set; }
	[JsonProperty("standardFlowEnabled")]
	public bool? StandardFlowEnabled { get; set; }
	[JsonProperty("implicitFlowEnabled")]
	public bool? ImplicitFlowEnabled { get; set; }
	[JsonProperty("directAccessGrantsEnabled")]
	public bool? DirectAccessGrantsEnabled { get; set; }
	[JsonProperty("serviceAccountsEnabled")]
	public bool? ServiceAccountsEnabled { get; set; }
	[JsonProperty("publicClient")]
	public bool? PublicClient { get; set; }
	[JsonProperty("frontchannelLogout")]
	public bool? FrontChannelLogout { get; set; }
	[JsonProperty("protocol")]
	public string Protocol { get; set; }
	[JsonProperty("attributes")]
	public IDictionary<string, string> Attributes { get; set; }
	[JsonProperty("authenticationFlowBindingOverrides")]
	public IDictionary<string, string> AuthenticationFlowBindingOverrides { get; set; }
	[JsonProperty("fullScopeAllowed")]
	public bool? FullScopeAllowed { get; set; }
	[JsonProperty("nodeReRegistrationTimeout")]
	public int? NodeReregistrationTimeout { get; set; }
	[JsonProperty("protocolMappers")]
	public IEnumerable<ClientProtocolMapper> ProtocolMappers { get; set; }
	[JsonProperty("defaultClientScopes")]
	public IEnumerable<string> DefaultClientScopes { get; set; }
	[JsonProperty("optionalClientScopes")]
	public IEnumerable<string> OptionalClientScopes { get; set; }
	[JsonProperty("access")]
	public ClientAccess Access { get; set; }
	[JsonProperty("secret")]
	public string Secret { get; set; }
	[JsonProperty("authorizationServicesEnabled")]
	public bool? AuthorizationServicesEnabled { get; set; }
}
