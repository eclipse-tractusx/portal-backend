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

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Root;

public class ComponentTypes
{
	[JsonProperty("org.keycloak.authentication.ClientAuthenticator")]
	public List<KeycloakAuthenticationAuthenticator> OrgKeycloakAuthenticationClientAuthenticator { get; set; }

	[JsonProperty("org.keycloak.services.clientregistration.policy.ClientRegistrationPolicy")]
	public List<KeycloakAuthenticationAuthenticator> OrgKeycloakServicesClientregistrationPolicyClientRegistrationPolicy { get; set; }

	[JsonProperty("org.keycloak.authentication.FormAction")]
	public List<KeycloakAuthenticationFormAction> OrgKeycloakAuthenticationFormAction { get; set; }

	[JsonProperty("org.keycloak.authentication.Authenticator")]
	public List<KeycloakAuthenticationAuthenticator> OrgKeycloakAuthenticationAuthenticator { get; set; }

	[JsonProperty("org.keycloak.storage.UserStorageProvider")]
	public List<KeycloakStorageUserStorageProvider> OrgKeycloakStorageUserStorageProvider { get; set; }

	[JsonProperty("org.keycloak.keys.KeyProvider")]
	public List<KeycloakAuthenticationFormAction> OrgKeycloakKeysKeyProvider { get; set; }

	[JsonProperty("org.keycloak.storage.ldap.mappers.LDAPStorageMapper")]
	public List<KeycloakStorageLdapMappersLdapStorageMapper> OrgKeycloakStorageLdapMappersLdapStorageMapper { get; set; }

	[JsonProperty("org.keycloak.authentication.FormAuthenticator")]
	public List<KeycloakAuthenticationAuthenticator> OrgKeycloakAuthenticationFormAuthenticator { get; set; }

	[JsonProperty("org.keycloak.protocol.ProtocolMapper")]
	public List<KeycloakAuthenticationAuthenticator> OrgKeycloakProtocolProtocolMapper { get; set; }

	[JsonProperty("org.keycloak.broker.provider.IdentityProviderMapper")]
	public List<KeycloakAuthenticationFormAction> OrgKeycloakBrokerProviderIdentityProviderMapper { get; set; }
}
