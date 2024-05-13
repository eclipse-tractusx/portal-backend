/********************************************************************************
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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Roles;

namespace Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;

public partial class ProvisioningManager
{
    private async Task<(string? ClientId, IEnumerable<Role> RoleNames)> GetCentralClientIdRolesAsync(string clientName, IEnumerable<string> roleNames)
    {
        var count = roleNames.Count();
        string? idOfClient = null;
        try
        {
            idOfClient = await GetIdOfCentralClientAsync(clientName).ConfigureAwait(ConfigureAwaitOptions.None);
            return count switch
            {
                0 => (idOfClient, Enumerable.Empty<Role>()),
                1 => (idOfClient, Enumerable.Repeat(await _centralIdp.GetRoleByNameAsync(_settings.CentralRealm, idOfClient, roleNames.Single()).ConfigureAwait(ConfigureAwaitOptions.None), 1)),
                _ => (idOfClient, (await _centralIdp.GetRolesAsync(_settings.CentralRealm, idOfClient).ConfigureAwait(ConfigureAwaitOptions.None)).Where(x => roleNames.Contains(x.Name))),
            };
        }
        catch (KeycloakEntityNotFoundException)
        {
            return (idOfClient, Enumerable.Empty<Role>());
        }
    }

    public async Task DeleteClientRolesFromCentralUserAsync(string centralUserId, IDictionary<string, IEnumerable<string>> clientRoleNames) =>
        await Task.WhenAll(clientRoleNames.Select(async x =>
            {
                var (client, roleNames) = x;
                var (clientId, roles) = await GetCentralClientIdRolesAsync(client, roleNames).ConfigureAwait(ConfigureAwaitOptions.None);
                if (clientId == null || !roles.Any())
                    return;

                await _centralIdp.DeleteClientRoleMappingsFromUserAsync(_settings.CentralRealm, centralUserId, clientId, roles).ConfigureAwait(ConfigureAwaitOptions.None);
            }
        )).ConfigureAwait(ConfigureAwaitOptions.None);

    public async IAsyncEnumerable<(string Client, IEnumerable<string> Roles, Exception? Error)> AssignClientRolesToCentralUserAsync(string centralUserId, IDictionary<string, IEnumerable<string>> clientRoleNames)
    {
        foreach (var (client, roleNames) in clientRoleNames)
        {
            var (clientId, roles) = await GetCentralClientIdRolesAsync(client, roleNames).ConfigureAwait(ConfigureAwaitOptions.None);
            if (clientId == null || !roles.Any())
            {
                yield return (Client: client, Roles: Enumerable.Empty<string>(), null);
                continue;
            }

            IEnumerable<string> assigned;
            Exception? error;
            try
            {
                await _centralIdp.AddClientRoleMappingsToUserAsync(_settings.CentralRealm, centralUserId, clientId, roles).ConfigureAwait(ConfigureAwaitOptions.None);
                assigned = roles.Select(role => role.Name ?? throw new KeycloakInvalidResponseException("name of role is null"));
                error = null;
            }
            catch (Exception e)
            {
                assigned = Enumerable.Empty<string>();
                error = e;
            }
            yield return (Client: client, Roles: assigned, Error: error);
        }
    }

    public async Task AddRolesToClientAsync(string clientName, IEnumerable<string> roleNames)
    {
        var result = await GetCentralClientIdRolesAsync(clientName, roleNames);
        if (result.ClientId == null)
        {
            throw new ConflictException($"Client {clientName} does not exist");
        }

        foreach (var role in roleNames.Except(result.RoleNames.Select(x => x.Name))
            .Select(roleName =>
                new Role
                {
                    Name = roleName,
                    ClientRole = true
                }))
        {
            await _centralIdp.CreateRoleAsync(_settings.CentralRealm, result.ClientId, role).ConfigureAwait(ConfigureAwaitOptions.None);
        }
    }

    private Task AssignClientRolesToClient(string clientId, IEnumerable<string> clientRoleNames) =>
        Task.WhenAll(clientRoleNames.Select(x => new Role
        {
            Name = x,
            ClientRole = true
        }).Select(async role =>
            {
                await _centralIdp.CreateRoleAsync(_settings.CentralRealm, clientId, role).ConfigureAwait(ConfigureAwaitOptions.None);
            }
        ));
}
