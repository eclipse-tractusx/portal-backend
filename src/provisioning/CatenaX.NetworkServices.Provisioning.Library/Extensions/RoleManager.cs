/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
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

using CatenaX.NetworkServices.Keycloak.ErrorHandling;
using Keycloak.Net.Models.Roles;
using Keycloak.Net.Models.Clients;

namespace CatenaX.NetworkServices.Provisioning.Library;

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

    public async Task<IDictionary<string, IEnumerable<string>>> AssignClientRolesToCentralUserAsync(string centralUserId, IDictionary<string, IEnumerable<string>> clientRoleNames) =>
        (await Task.WhenAll(clientRoleNames.Select(async x =>
            {
                var (client, roleNames) = x;
                var (clientId, roles) = await GetCentralClientIdRolesAsync(client, roleNames).ConfigureAwait(false);
                if (clientId != null && roles.Any() &&
                    await _CentralIdp.AddClientRoleMappingsToUserAsync(_Settings.CentralRealm, centralUserId, clientId, roles).ConfigureAwait(false))
                {
                    return (client: client, rolesList: roles.Select(role => role.Name));
                }
                return (client: client, rolesList: Enumerable.Empty<string>());
            }
        )).ConfigureAwait(false))
        .ToDictionary(clientRole => clientRole.client, clientRole => clientRole.rolesList);
}
