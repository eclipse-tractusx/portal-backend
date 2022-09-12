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

namespace CatenaX.NetworkServices.Keycloak.Library.Models.Root;

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
