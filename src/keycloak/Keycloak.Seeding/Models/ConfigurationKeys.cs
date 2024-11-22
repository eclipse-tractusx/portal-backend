/********************************************************************************
 * Copyright (c) 2024 Contributors to the Eclipse Foundation
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

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Models;

public static class ConfigurationKeys
{
    public const string RolesConfigKey = "ROLES";
    public const string LocalizationsConfigKey = "LOCALIZATIONS";
    public const string UserProfileConfigKey = "USERPROFILE";
    public const string ClientScopesConfigKey = "CLIENTSCOPES";
    public const string ClientsConfigKey = "CLIENTS";
    public const string IdentityProvidersConfigKey = "IDENTITYPROVIDERS";
    public const string IdentityProviderMappersConfigKey = "IDENTITYPROVIDERMAPPERS";
    public const string UsersConfigKey = "USERS";
    // TODO (PS): Clarify how to define the identity providers which should be skipped
    public const string FederatedIdentitiesConfigKeys = "FEDERATEDIDENTITIES";
    public const string ClientScopeMappersConfigKey = "CLIENTSCOPEMAPPERS";
    public const string ProtocolMappersConfigKey = "PROTOCOLMAPPERS";
    // TODO (PS): Clarify how to define the auth flows which should be skipped
    public const string AuthenticationFlowsConfigKey = "AUTHENTICATIONFLOWS";
    public const string ClientProtocolMapperConfigKey = "CLIENTPROTOCOLMAPPER";
    public const string ClientRolesConfigKey = "CLIENTROLES";
    public const string AuthenticationFlowExecutionConfigKey = "AUTHENTICATIONFLOWEXECUTION";
    public const string AuthenticatorConfigConfigKey = "AUTHENTICATORCONFIG";
}
