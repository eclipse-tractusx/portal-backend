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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Factory;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.BusinessLogic;

public class KeycloakSeeder
{
    private readonly KeycloakSeederSettings _settings;
    private readonly ISeedDataHandler _seedData;
    private readonly IRealmUpdater _realmUpdater;
    private readonly IRolesUpdater _rolesUpdater;
    private readonly IClientsUpdater _clientsUpdater;
    private readonly IIdentityProvidersUpdater _identityProvidersUpdater;
    private readonly IUsersUpdater _usersUpdater;
    private readonly IAuthenticationFlowsUpdater _authenticationFlowsUpdater;
    public KeycloakSeeder(ISeedDataHandler seedDataHandler, IRealmUpdater realmUpdater, IRolesUpdater rolesUpdater, IClientsUpdater clientsUpdater, IIdentityProvidersUpdater identityProvidersUpdater, IUsersUpdater usersUpdater, IAuthenticationFlowsUpdater authenticationFlowsUpdater, IOptions<KeycloakSeederSettings> options)
    {
        _seedData = seedDataHandler;
        _realmUpdater = realmUpdater;
        _rolesUpdater = rolesUpdater;
        _clientsUpdater = clientsUpdater;
        _identityProvidersUpdater = identityProvidersUpdater;
        _usersUpdater = usersUpdater;
        _authenticationFlowsUpdater = authenticationFlowsUpdater;
        _settings = options.Value;
    }

    public async Task Seed()
    {
        await _seedData.Import(_settings.DataPath).ConfigureAwait(false);
        await _realmUpdater.UpdateRealm(_settings.KeycloakInstanceName).ConfigureAwait(false);
        await _rolesUpdater.UpdateRealmRoles(_settings.KeycloakInstanceName).ConfigureAwait(false);
        await _clientsUpdater.UpdateClients(_settings.KeycloakInstanceName).ConfigureAwait(false);
        await _rolesUpdater.UpdateClientRoles(_settings.KeycloakInstanceName).ConfigureAwait(false);
        await _rolesUpdater.UpdateCompositeRoles(_settings.KeycloakInstanceName).ConfigureAwait(false);
        await _identityProvidersUpdater.UpdateIdentityProviders(_settings.KeycloakInstanceName).ConfigureAwait(false);
        await _usersUpdater.UpdateUsers(_settings.KeycloakInstanceName).ConfigureAwait(false);
        await _authenticationFlowsUpdater.UpdateAuthenticationFlows(_settings.KeycloakInstanceName).ConfigureAwait(false);
    }
}
