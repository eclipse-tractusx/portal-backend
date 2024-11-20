/********************************************************************************
 * Copyright (c) 2023 Contributors to the Eclipse Foundation
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

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.BusinessLogic;

public class KeycloakSeeder(
    ISeedDataHandler seedDataHandler,
    IRealmUpdater realmUpdater,
    IRolesUpdater rolesUpdater,
    IClientsUpdater clientsUpdater,
    IIdentityProvidersUpdater identityProvidersUpdater,
    IUsersUpdater usersUpdater,
    IClientScopesUpdater clientScopesUpdater,
    IAuthenticationFlowsUpdater authenticationFlowsUpdater,
    IClientScopeMapperUpdater clientScopeMapperUpdater,
    ILocalizationsUpdater localizationsUpdater,
    IUserProfileUpdater userProfileUpdater,
    IOptions<KeycloakSeederSettings> options)
    : IKeycloakSeeder
{
    private readonly KeycloakSeederSettings _settings = options.Value;

    public async Task Seed(CancellationToken cancellationToken)
    {
        foreach (var realm in _settings.Realms)
        {
            await seedDataHandler.Import(realm, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
            await realmUpdater.UpdateRealm(realm.InstanceName, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
            await localizationsUpdater.UpdateLocalizations(realm.InstanceName, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
            await userProfileUpdater.UpdateUserProfile(realm.InstanceName, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
            await rolesUpdater.UpdateRealmRoles(realm.InstanceName, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
            await clientScopesUpdater.UpdateClientScopes(realm.InstanceName, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
            await clientsUpdater.UpdateClients(realm.InstanceName, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
            await rolesUpdater.UpdateClientRoles(realm.InstanceName, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
            await rolesUpdater.UpdateCompositeRoles(realm.InstanceName, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
            await identityProvidersUpdater.UpdateIdentityProviders(realm.InstanceName, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
            await usersUpdater.UpdateUsers(realm.InstanceName, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
            await clientScopeMapperUpdater.UpdateClientScopeMapper(realm.InstanceName, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
            await authenticationFlowsUpdater.UpdateAuthenticationFlows(realm.InstanceName, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        }
    }
}
