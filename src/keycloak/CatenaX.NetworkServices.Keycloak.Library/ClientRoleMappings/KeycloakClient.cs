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

using CatenaX.NetworkServices.Keycloak.Library.Models.Roles;
using Flurl.Http;

namespace CatenaX.NetworkServices.Keycloak.Library;

public partial class KeycloakClient
{
    public async Task<bool> AddClientRoleMappingsToGroupAsync(string realm, string groupId, string clientId, IEnumerable<Role> roles)
    {
        var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/groups/")
            .AppendPathSegment(groupId, true)
            .AppendPathSegment("/role-mappings/clients/")
            .AppendPathSegment(clientId, true)
            .PostJsonAsync(roles)
            .ConfigureAwait(false);
        return response.IsSuccessStatusCode;
    }

    public async Task<IEnumerable<Role>> GetClientRoleMappingsForGroupAsync(string realm, string groupId, string clientId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
        .AppendPathSegment("/admin/realms/")
        .AppendPathSegment(realm, true)
        .AppendPathSegment("/groups/")
        .AppendPathSegment(groupId, true)
        .AppendPathSegment("/role-mappings/clients/")
        .AppendPathSegment(clientId, true)
        .GetJsonAsync<IEnumerable<Role>>()
        .ConfigureAwait(false);

    public async Task<bool> DeleteClientRoleMappingsFromGroupAsync(string realm, string groupId, string clientId, IEnumerable<Role> roles)
    {
        var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/groups/")
            .AppendPathSegment(groupId, true)
            .AppendPathSegment("/role-mappings/clients/")
            .AppendPathSegment(clientId, true)
            .SendJsonAsync(HttpMethod.Delete, roles)
            .ConfigureAwait(false);
        return response.IsSuccessStatusCode;
    }

    public async Task<IEnumerable<Role>> GetAvailableClientRoleMappingsForGroupAsync(string realm, string groupId, string clientId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
        .AppendPathSegment("/admin/realms/")
        .AppendPathSegment(realm, true)
        .AppendPathSegment("/groups/")
        .AppendPathSegment(groupId, true)
        .AppendPathSegment("/role-mappings/clients/")
        .AppendPathSegment(clientId, true)
        .AppendPathSegment("/available")
        .GetJsonAsync<IEnumerable<Role>>()
        .ConfigureAwait(false);

    public async Task<IEnumerable<Role>> GetEffectiveClientRoleMappingsForGroupAsync(string realm, string groupId, string clientId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
        .AppendPathSegment("/admin/realms/")
        .AppendPathSegment(realm, true)
        .AppendPathSegment("/groups/")
        .AppendPathSegment(groupId, true)
        .AppendPathSegment("/role-mappings/clients/")
        .AppendPathSegment(clientId, true)
        .AppendPathSegment("/composite")
        .GetJsonAsync<IEnumerable<Role>>()
        .ConfigureAwait(false);

    public async Task<bool> AddClientRoleMappingsToUserAsync(string realm, string userId, string clientId, IEnumerable<Role> roles)
    {
        var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/users/")
            .AppendPathSegment(userId, true)
            .AppendPathSegment("/role-mappings/clients/")
            .AppendPathSegment(clientId, true)
            .PostJsonAsync(roles)
            .ConfigureAwait(false);
        return response.IsSuccessStatusCode;
    }

    public async Task<IEnumerable<Role>> GetClientRoleMappingsForUserAsync(string realm, string userId, string clientId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
        .AppendPathSegment("/admin/realms/")
        .AppendPathSegment(realm, true)
        .AppendPathSegment("/users/")
        .AppendPathSegment(userId, true)
        .AppendPathSegment("/role-mappings/clients/")
        .AppendPathSegment(clientId, true)
        .GetJsonAsync<IEnumerable<Role>>()
        .ConfigureAwait(false);

    public async Task<bool> DeleteClientRoleMappingsFromUserAsync(string realm, string userId, string clientId, IEnumerable<Role> roles)
    {
        var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/users/")
            .AppendPathSegment(userId, true)
            .AppendPathSegment("/role-mappings/clients/")
            .AppendPathSegment(clientId, true)
            .SendJsonAsync(HttpMethod.Delete, roles)
            .ConfigureAwait(false);
        return response.IsSuccessStatusCode;
    }

    public async Task<IEnumerable<Role>> GetAvailableClientRoleMappingsForUserAsync(string realm, string userId, string clientId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
        .AppendPathSegment("/admin/realms/")
        .AppendPathSegment(realm, true)
        .AppendPathSegment("/users/")
        .AppendPathSegment(userId, true)
        .AppendPathSegment("/role-mappings/clients/")
        .AppendPathSegment(clientId, true)
        .AppendPathSegment("/available")
        .GetJsonAsync<IEnumerable<Role>>()
        .ConfigureAwait(false);

    public async Task<IEnumerable<Role>> GetEffectiveClientRoleMappingsForUserAsync(string realm, string userId, string clientId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
        .AppendPathSegment("/admin/realms/")
        .AppendPathSegment(realm, true)
        .AppendPathSegment("/users/")
        .AppendPathSegment(userId, true)
        .AppendPathSegment("/role-mappings/clients/")
        .AppendPathSegment(clientId, true)
        .AppendPathSegment("/composite")
        .GetJsonAsync<IEnumerable<Role>>()
        .ConfigureAwait(false);
}
