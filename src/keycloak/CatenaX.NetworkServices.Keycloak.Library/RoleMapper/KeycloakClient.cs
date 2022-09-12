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

using CatenaX.NetworkServices.Keycloak.Library.Models.Common;
using CatenaX.NetworkServices.Keycloak.Library.Models.Roles;
using Flurl.Http;

namespace CatenaX.NetworkServices.Keycloak.Library;

public partial class KeycloakClient
{
    public async Task<Mapping> GetRoleMappingsForGroupAsync(string realm, string groupId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
        .AppendPathSegment("/admin/realms/")
        .AppendPathSegment(realm, true)
        .AppendPathSegment("/groups/")
        .AppendPathSegment(groupId, true)
        .AppendPathSegment("/role-mappings")
        .GetJsonAsync<Mapping>()
        .ConfigureAwait(false);

    public async Task<bool> AddRealmRoleMappingsToGroupAsync(string realm, string groupId, IEnumerable<Role> roles)
    {
        var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/groups/")
            .AppendPathSegment(groupId, true)
            .AppendPathSegment("/role-mappings/realm")
            .PostJsonAsync(roles)
            .ConfigureAwait(false);
        return response.IsSuccessStatusCode;
    }

    public async Task<IEnumerable<Role>> GetRealmRoleMappingsForGroupAsync(string realm, string groupId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
        .AppendPathSegment("/admin/realms/")
        .AppendPathSegment(realm, true)
        .AppendPathSegment("/groups/")
        .AppendPathSegment(groupId, true)
        .AppendPathSegment("/role-mappings/realm")
        .GetJsonAsync<IEnumerable<Role>>()
        .ConfigureAwait(false);

    public async Task<bool> DeleteRealmRoleMappingsFromGroupAsync(string realm, string groupId, IEnumerable<Role> roles)
    {
        var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/groups/")
            .AppendPathSegment(groupId, true)
            .AppendPathSegment("/role-mappings/realm")
            .SendJsonAsync(HttpMethod.Delete, roles)
            .ConfigureAwait(false);
        return response.IsSuccessStatusCode;
    }

    public async Task<IEnumerable<Role>> GetAvailableRealmRoleMappingsForGroupAsync(string realm, string groupId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
        .AppendPathSegment("/admin/realms/")
        .AppendPathSegment(realm, true)
        .AppendPathSegment("/groups/")
        .AppendPathSegment(groupId, true)
        .AppendPathSegment("/role-mappings/realm/available")
        .GetJsonAsync<IEnumerable<Role>>()
        .ConfigureAwait(false);

    public async Task<IEnumerable<Role>> GetEffectiveRealmRoleMappingsForGroupAsync(string realm, string groupId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
        .AppendPathSegment("/admin/realms/")
        .AppendPathSegment(realm, true)
        .AppendPathSegment("/groups/")
        .AppendPathSegment(groupId, true)
        .AppendPathSegment("/role-mappings/realm/composite")
        .GetJsonAsync<IEnumerable<Role>>()
        .ConfigureAwait(false);

    public async Task<Mapping> GetRoleMappingsForUserAsync(string realm, string userId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
        .AppendPathSegment("/admin/realms/")
        .AppendPathSegment(realm, true)
        .AppendPathSegment("/users/")
        .AppendPathSegment(userId, true)
        .AppendPathSegment("/role-mappings")
        .GetJsonAsync<Mapping>()
        .ConfigureAwait(false);

    public async Task<bool> AddRealmRoleMappingsToUserAsync(string realm, string userId, IEnumerable<Role> roles)
    {
        var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/users/")
            .AppendPathSegment(userId, true)
            .AppendPathSegment("/role-mappings/realm")
            .PostJsonAsync(roles)
            .ConfigureAwait(false);
        return response.IsSuccessStatusCode;
    }

    public async Task<IEnumerable<Role>> GetRealmRoleMappingsForUserAsync(string realm, string userId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
        .AppendPathSegment("/admin/realms/")
        .AppendPathSegment(realm, true)
        .AppendPathSegment("/users/")
        .AppendPathSegment(userId, true)
        .AppendPathSegment("/role-mappings/realm")
        .GetJsonAsync<IEnumerable<Role>>()
        .ConfigureAwait(false);

    public async Task<bool> DeleteRealmRoleMappingsFromUserAsync(string realm, string userId, IEnumerable<Role> roles)
    {
        var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/users/")
            .AppendPathSegment(userId, true)
            .AppendPathSegment("/role-mappings/realm")
            .SendJsonAsync(HttpMethod.Delete, roles)
            .ConfigureAwait(false);
        return response.IsSuccessStatusCode;
    }

    public async Task<IEnumerable<Role>> GetAvailableRealmRoleMappingsForUserAsync(string realm, string userId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
        .AppendPathSegment("/admin/realms/")
        .AppendPathSegment(realm, true)
        .AppendPathSegment("/users/")
        .AppendPathSegment(userId, true)
        .AppendPathSegment("/role-mappings/realm/available")
        .GetJsonAsync<IEnumerable<Role>>()
        .ConfigureAwait(false);

    public async Task<IEnumerable<Role>> GetEffectiveRealmRoleMappingsForUserAsync(string realm, string userId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
        .AppendPathSegment("/admin/realms/")
        .AppendPathSegment(realm, true)
        .AppendPathSegment("/users/")
        .AppendPathSegment(userId, true)
        .AppendPathSegment("/role-mappings/realm/composite")
        .GetJsonAsync<IEnumerable<Role>>()
        .ConfigureAwait(false);
}
