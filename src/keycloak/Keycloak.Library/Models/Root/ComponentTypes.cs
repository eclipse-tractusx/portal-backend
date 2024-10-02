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

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Root;

public class ComponentTypes
{
    [JsonPropertyName("org.keycloak.authentication.ClientAuthenticator")]
    public List<KeycloakAuthenticationAuthenticator> OrgKeycloakAuthenticationClientAuthenticator { get; set; }

    [JsonPropertyName("org.keycloak.services.clientregistration.policy.ClientRegistrationPolicy")]
    public List<KeycloakAuthenticationAuthenticator> OrgKeycloakServicesClientregistrationPolicyClientRegistrationPolicy { get; set; }

    [JsonPropertyName("org.keycloak.authentication.FormAction")]
    public List<KeycloakAuthenticationFormAction> OrgKeycloakAuthenticationFormAction { get; set; }

    [JsonPropertyName("org.keycloak.authentication.Authenticator")]
    public List<KeycloakAuthenticationAuthenticator> OrgKeycloakAuthenticationAuthenticator { get; set; }

    [JsonPropertyName("org.keycloak.storage.UserStorageProvider")]
    public List<KeycloakStorageUserStorageProvider> OrgKeycloakStorageUserStorageProvider { get; set; }

    [JsonPropertyName("org.keycloak.keys.KeyProvider")]
    public List<KeycloakAuthenticationFormAction> OrgKeycloakKeysKeyProvider { get; set; }

    [JsonPropertyName("org.keycloak.storage.ldap.mappers.LDAPStorageMapper")]
    public List<KeycloakStorageLdapMappersLdapStorageMapper> OrgKeycloakStorageLdapMappersLdapStorageMapper { get; set; }

    [JsonPropertyName("org.keycloak.authentication.FormAuthenticator")]
    public List<KeycloakAuthenticationAuthenticator> OrgKeycloakAuthenticationFormAuthenticator { get; set; }

    [JsonPropertyName("org.keycloak.protocol.ProtocolMapper")]
    public List<KeycloakAuthenticationAuthenticator> OrgKeycloakProtocolProtocolMapper { get; set; }

    [JsonPropertyName("org.keycloak.broker.provider.IdentityProviderMapper")]
    public List<KeycloakAuthenticationFormAction> OrgKeycloakBrokerProviderIdentityProviderMapper { get; set; }
}
