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

namespace CatenaX.NetworkServices.Keycloak.Library.Models.Clients;

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
