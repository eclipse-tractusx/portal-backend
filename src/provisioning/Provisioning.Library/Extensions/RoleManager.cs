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

using Org.Eclipse.TractusX.Portal.Backend.Keycloak.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Roles;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Clients;

namespace Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;

public partial class ProvisioningManager
{
    private async Task<(string?, IEnumerable<Role>)> GetCentralClientIdRolesAsync(string clientName, IEnumerable<string> roleNames)
    {
        var count = roleNames.Count();
        Client? client = null;
        try
        {
            client = await GetCentralClientViewableAsync(clientName).ConfigureAwait(false);
            if (client == null)
            {
                return (null, Enumerable.Empty<Role>());
            }
            switch (count)
            {
                case 0:
                    return (client.Id, Enumerable.Empty<Role>());
                case 1:
                    return (client.Id, Enumerable.Repeat<Role>(await _CentralIdp.GetRoleByNameAsync(_Settings.CentralRealm, client.Id, roleNames.Single()).ConfigureAwait(false), 1));
                default:
                    return (client.Id, (await _CentralIdp.GetRolesAsync(_Settings.CentralRealm, client.Id).ConfigureAwait(false)).Where(x => roleNames.Contains(x.Name)));
            }
        }
        catch(KeycloakEntityNotFoundException)
        {
            return (client?.Id, Enumerable.Empty<Role>());
        }
    }

    public async Task DeleteClientRolesFromCentralUserAsync(string centralUserId, IDictionary<string, IEnumerable<string>> clientRoleNames) =>
        await Task.WhenAll(clientRoleNames.Select(async x =>
            {
                var (client, roleNames) = x;
                var (clientId, roles) = await GetCentralClientIdRolesAsync(client, roleNames).ConfigureAwait(false);
                if (clientId == null || !roles.Any()) return;
                
                await _CentralIdp.DeleteClientRoleMappingsFromUserAsync(_Settings.CentralRealm, centralUserId, clientId, roles).ConfigureAwait(false);
            }
        )).ConfigureAwait(false);

    public async IAsyncEnumerable<(string Client, IEnumerable<string> Roles)> AssignClientRolesToCentralUserAsync(string centralUserId, IDictionary<string, IEnumerable<string>> clientRoleNames)
    {
        foreach (var (client, roleNames) in clientRoleNames)
        {
            var (clientId, roles) = await GetCentralClientIdRolesAsync(client, roleNames).ConfigureAwait(false);
            if (clientId == null || !roles.Any())
            {
                yield return (Client: client, Roles: Enumerable.Empty<string>());
            }
            
            IEnumerable<string> assigned;
            try
            {
                await _CentralIdp.AddClientRoleMappingsToUserAsync(_Settings.CentralRealm, centralUserId, clientId!, roles).ConfigureAwait(false);
                assigned = roles.Select(role => role.Name);
            }
            catch (Exception)
            {
                assigned = Enumerable.Empty<string>();
            }
            yield return (Client: client, Roles: assigned);
        }
    }

    private Task AssignClientRolesToClient(string clientId, IEnumerable<string> clientRoleNames) =>
        Task.WhenAll(clientRoleNames.Select(x => new Role
        {
            Name = x,
            ClientRole = true
        }).Select(async role =>
            {
                await _CentralIdp.CreateRoleAsync(_Settings.CentralRealm, clientId, role).ConfigureAwait(false);
            }
        ));
}
