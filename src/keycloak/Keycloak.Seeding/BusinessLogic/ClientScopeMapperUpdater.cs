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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Linq;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Factory;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Roles;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.BusinessLogic;

public class ClientScopeMapperUpdater(IKeycloakFactory keycloakFactory, ISeedDataHandler seedDataHandler)
    : IClientScopeMapperUpdater
{
    public async Task UpdateClientScopeMapper(string instanceName, CancellationToken cancellationToken)
    {
        var keycloak = keycloakFactory.CreateKeycloakClient(instanceName);
        var realm = seedDataHandler.Realm;
        var seederConfig = seedDataHandler.GetSpecificConfiguration(ConfigurationKey.ClientScopes);

        var clients = await keycloak.GetClientsAsync(realm, null, true, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        foreach (var (clientName, mappingModels) in seedDataHandler.ClientScopeMappings)
        {
            var client = clients.SingleOrDefault(x => x.ClientId == clientName);
            if (client?.Id is null)
            {
                throw new ConflictException($"No client id found with name {clientName}");
            }

            var roles = await keycloak.GetRolesAsync(realm, client.Id, cancellationToken: cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
            foreach (var mappingModel in mappingModels)
            {
                var clientScope = clients.SingleOrDefault(x => x.ClientId == mappingModel.Client);
                if (clientScope?.Id is null)
                {
                    throw new ConflictException($"No client id found with name {clientName}");
                }
                var clientRoles = await keycloak.GetClientRolesScopeMappingsForClientAsync(realm, clientScope.Id, client.Id, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
                var mappingModelRoles = mappingModel.Roles?.Select(roleName => roles.SingleOrDefault(r => r.Name == roleName) ?? throw new ConflictException($"No role with name {roleName} found")) ?? Enumerable.Empty<Role>();
                await AddAndDeleteRoles(keycloak, realm, clientScope.Id, client.Id, clientRoles, mappingModelRoles, seederConfig, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
            }
        }
    }

    private static async Task AddAndDeleteRoles(KeycloakClient keycloak, string realm, string clientScopeId, string clientId, IEnumerable<Role> roles, IEnumerable<Role> updateRoles, KeycloakSeederConfigModel seederConfig, CancellationToken cancellationToken)
    {
        await updateRoles
            .Where(x => seederConfig.ModificationAllowed(ModificationType.Create, x.Name))
            .ExceptBy(roles.Select(role => role.Name), roleModel => roleModel.Name)
            .IfAnyAwait(rolesToAdd =>
                keycloak.AddClientRolesScopeMappingToClientAsync(realm, clientScopeId, clientId, rolesToAdd, cancellationToken)).ConfigureAwait(false);

        await roles
            .Where(x => seederConfig.ModificationAllowed(ModificationType.Delete, x.Name))
            .ExceptBy(updateRoles.Select(roleModel => roleModel.Name), role => role.Name)
            .IfAnyAwait(rolesToDelete =>
                keycloak.RemoveClientRolesFromClientScopeForClientAsync(realm, clientScopeId, clientId, rolesToDelete, cancellationToken)).ConfigureAwait(false);
    }
}
