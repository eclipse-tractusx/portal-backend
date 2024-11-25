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
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Models;

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
            await CheckAndExecuteUpdater(ConfigurationKeys.Localizations, realm.InstanceName, localizationsUpdater.UpdateLocalizations, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
            await CheckAndExecuteUpdater(ConfigurationKeys.UserProfile, realm.InstanceName, userProfileUpdater.UpdateUserProfile, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
            await CheckAndExecuteUpdater(ConfigurationKeys.Roles, realm.InstanceName, rolesUpdater.UpdateRealmRoles, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
            await CheckAndExecuteUpdater(ConfigurationKeys.ClientScopes, realm.InstanceName, clientScopesUpdater.UpdateClientScopes, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
            await CheckAndExecuteUpdater(ConfigurationKeys.Clients, realm.InstanceName, clientsUpdater.UpdateClients, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
            await CheckAndExecuteUpdater(ConfigurationKeys.ClientRoles, realm.InstanceName, rolesUpdater.UpdateClientRoles, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
            await CheckAndExecuteUpdater(ConfigurationKeys.Roles, realm.InstanceName, rolesUpdater.UpdateCompositeRoles, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
            await CheckAndExecuteUpdater(ConfigurationKeys.IdentityProviders, realm.InstanceName, identityProvidersUpdater.UpdateIdentityProviders, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
            await CheckAndExecuteUpdater(ConfigurationKeys.Users, realm.InstanceName, usersUpdater.UpdateUsers, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
            await CheckAndExecuteUpdater(ConfigurationKeys.ClientScopeMappers, realm.InstanceName, clientScopeMapperUpdater.UpdateClientScopeMapper, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
            await CheckAndExecuteUpdater(ConfigurationKeys.AuthenticationFlows, realm.InstanceName, authenticationFlowsUpdater.UpdateAuthenticationFlows, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        }
    }

    private async Task CheckAndExecuteUpdater(ConfigurationKeys configKey, string instanceName, Func<string, CancellationToken, Task> updaterExecution, CancellationToken cancellationToken)
    {
        if (!seedDataHandler.Configuration.ModificationAllowed(configKey))
        {
            return;
        }

        await updaterExecution(instanceName, cancellationToken).ConfigureAwait(false);
    }
}
