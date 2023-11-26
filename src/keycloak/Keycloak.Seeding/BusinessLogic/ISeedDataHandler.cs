/********************************************************************************
 * Copyright (c) 2021, 2023 BMW Group AG
 * Copyright (c) 2021, 2023 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.BusinessLogic;

public interface ISeedDataHandler
{
    Task Import(string path, CancellationToken cancellationToken);

    string Realm { get; }

    KeycloakRealm KeycloakRealm { get; }

    IEnumerable<ClientModel> Clients { get; }

    IReadOnlyDictionary<string, IEnumerable<RoleModel>> ClientRoles { get; }

    IEnumerable<RoleModel> RealmRoles { get; }

    IEnumerable<IdentityProviderModel> IdentityProviders { get; }

    IEnumerable<IdentityProviderMapperModel> IdentityProviderMappers { get; }

    IEnumerable<UserModel> Users { get; }

    IEnumerable<AuthenticationFlowModel> TopLevelCustomAuthenticationFlows { get; }

    IEnumerable<ClientScopeModel> ClientScopes { get; }

    IReadOnlyDictionary<string, string> ClientsDictionary { get; }

    IReadOnlyDictionary<string, IEnumerable<ClientScopeMappingModel>> ClientScopeMappings { get; }

    Task SetClientInternalIds(IAsyncEnumerable<(string ClientId, string Id)> clientInternalIds);

    string GetIdOfClient(string clientId);

    AuthenticationFlowModel GetAuthenticationFlow(string? alias);

    IEnumerable<AuthenticationExecutionModel> GetAuthenticationExecutions(string? alias);

    AuthenticatorConfigModel GetAuthenticatorConfig(string? alias);
}
