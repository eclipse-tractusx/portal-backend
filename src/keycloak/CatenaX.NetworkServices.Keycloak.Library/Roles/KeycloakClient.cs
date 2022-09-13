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
using CatenaX.NetworkServices.Keycloak.Library.Models.Groups;
using CatenaX.NetworkServices.Keycloak.Library.Models.Roles;
using CatenaX.NetworkServices.Keycloak.Library.Models.Users;
using Flurl.Http;

namespace CatenaX.NetworkServices.Keycloak.Library;

public partial class KeycloakClient
{
    public async Task CreateRoleAsync(string realm, string clientId, Role role) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/roles")
            .PostJsonAsync(role)
            .ConfigureAwait(false);

    public async Task<IEnumerable<Role>> GetRolesAsync(string realm, string clientId, int? first = null, int? max = null, string? search = null)
    {
        var queryParams = new Dictionary<string, object?>
        {
            [nameof(first)] = first,
            [nameof(max)] = max,
            [nameof(search)] = search
        };

        return await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/roles")
            .SetQueryParams(queryParams)
            .GetJsonAsync<IEnumerable<Role>>()
            .ConfigureAwait(false);
    }

    public async Task<Role> GetRoleByNameAsync(string realm, string clientId, string roleName) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/roles/")
            .AppendPathSegment(roleName, true)
            .GetJsonAsync<Role>()
            .ConfigureAwait(false);
    
    public async Task UpdateRoleByNameAsync(string realm, string clientId, string roleName, Role role) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/roles/")
            .AppendPathSegment(roleName, true)
            .PutJsonAsync(role)
            .ConfigureAwait(false);

    public async Task DeleteRoleByNameAsync(string realm, string clientId, string roleName) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/roles/")
            .AppendPathSegment(roleName, true)
            .DeleteAsync()
            .ConfigureAwait(false);

    public async Task AddCompositesToRoleAsync(string realm, string clientId, string roleName, IEnumerable<Role> roles) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/roles/")
            .AppendPathSegment(roleName, true)
            .AppendPathSegment("/composites")
            .PostJsonAsync(roles)
            .ConfigureAwait(false);

    public async Task<IEnumerable<Role>> GetRoleCompositesAsync(string realm, string clientId, string roleName) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/roles/")
            .AppendPathSegment(roleName, true)
            .AppendPathSegment("/composites")
            .GetJsonAsync<IEnumerable<Role>>()
            .ConfigureAwait(false);

    public async Task RemoveCompositesFromRoleAsync(string realm, string clientId, string roleName, IEnumerable<Role> roles) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/roles/")
            .AppendPathSegment(roleName, true)
            .AppendPathSegment("/composites")
            .SendJsonAsync(HttpMethod.Delete, roles)
            .ConfigureAwait(false);

    public async Task<IEnumerable<Role>> GetApplicationRolesForCompositeAsync(string realm, string clientId, string roleName, string forClientId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/roles/")
            .AppendPathSegment(roleName, true)
            .AppendPathSegment("/composites/clients/")
            .AppendPathSegment(forClientId, true)
            .GetJsonAsync<IEnumerable<Role>>()
            .ConfigureAwait(false);

    public async Task<IEnumerable<Role>> GetRealmRolesForCompositeAsync(string realm, string clientId, string roleName) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/roles/")
            .AppendPathSegment(roleName, true)
            .AppendPathSegment("/composites/realm")
            .GetJsonAsync<IEnumerable<Role>>()
            .ConfigureAwait(false);

    [Obsolete("Not working yet")]
    public async Task<IEnumerable<Group>> GetGroupsWithRoleNameAsync(string realm, string clientId, string roleName, int? first = null, bool? full = null, int? max = null)
    {
        var queryParams = new Dictionary<string, object?>
        {
            [nameof(first)] = first,
            [nameof(full)] = full,
            [nameof(max)] = max
        };

        return await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/roles/")
            .AppendPathSegment(roleName, true)
            .AppendPathSegment("/groups")
            .SetQueryParams(queryParams)
            .GetJsonAsync<IEnumerable<Group>>()
            .ConfigureAwait(false);
    }

    public async Task<ManagementPermission> GetRoleAuthorizationPermissionsInitializedAsync(string realm, string clientId, string roleName) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/roles/")
            .AppendPathSegment(roleName, true)
            .AppendPathSegment("/management/permissions")
            .GetJsonAsync<ManagementPermission>()
            .ConfigureAwait(false);

    public async Task<ManagementPermission> SetRoleAuthorizationPermissionsInitializedAsync(string realm, string clientId, string roleName, ManagementPermission managementPermission) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/roles/")
            .AppendPathSegment(roleName, true)
            .AppendPathSegment("/management/permissions")
            .PutJsonAsync(managementPermission)
            .ReceiveJson<ManagementPermission>()
            .ConfigureAwait(false);

    public async Task<IEnumerable<User>> GetUsersWithRoleNameAsync(string realm, string clientId, string roleName, int? first = null, int? max = null)
    {
        var queryParams = new Dictionary<string, object?>
        {
            [nameof(first)] = first,
            [nameof(max)] = max
        };

        return await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/roles/")
            .AppendPathSegment(roleName, true)
            .AppendPathSegment("/users")
            .SetQueryParams(queryParams)
            .GetJsonAsync<IEnumerable<User>>()
            .ConfigureAwait(false);
    }

    public async Task CreateRoleAsync(string realm, Role role) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/roles")
            .PostJsonAsync(role)
            .ConfigureAwait(false);

    public async Task<IEnumerable<Role>> GetRolesAsync(string realm, int? first = null, int? max = null, string? search = null)
    {
        var queryParams = new Dictionary<string, object?>
        {
            [nameof(first)] = first,
            [nameof(max)] = max,
            [nameof(search)] = search
        };

        return await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/roles")
            .SetQueryParams(queryParams)
            .GetJsonAsync<IEnumerable<Role>>()
            .ConfigureAwait(false);
    }

    public async Task<Role> GetRoleByNameAsync(string realm, string roleName) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/roles/")
            .AppendPathSegment(roleName, true)
            .GetJsonAsync<Role>()
            .ConfigureAwait(false);
    
    public async Task UpdateRoleByNameAsync(string realm, string roleName, Role role) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/roles/")
            .AppendPathSegment(roleName, true)
            .PutJsonAsync(role)
            .ConfigureAwait(false);

    public async Task DeleteRoleByNameAsync(string realm, string roleName) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/roles/")
            .AppendPathSegment(roleName, true)
            .DeleteAsync()
            .ConfigureAwait(false);

    public async Task AddCompositesToRoleAsync(string realm, string roleName, IEnumerable<Role> roles) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/roles/")
            .AppendPathSegment(roleName, true)
            .AppendPathSegment("/composites")
            .PostJsonAsync(roles)
            .ConfigureAwait(false);

    public async Task<IEnumerable<Role>> GetRoleCompositesAsync(string realm, string roleName) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/roles/")
            .AppendPathSegment(roleName, true)
            .AppendPathSegment("/composites")
            .GetJsonAsync<IEnumerable<Role>>()
            .ConfigureAwait(false);

    public async Task RemoveCompositesFromRoleAsync(string realm, string roleName, IEnumerable<Role> roles) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/roles/")
            .AppendPathSegment(roleName, true)
            .AppendPathSegment("/composites")
            .SendJsonAsync(HttpMethod.Delete, roles)
            .ConfigureAwait(false);

    public async Task<IEnumerable<Role>> GetApplicationRolesForCompositeAsync(string realm, string roleName, string forClientId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/roles/")
            .AppendPathSegment(roleName, true)
            .AppendPathSegment("/composites/clients/")
            .AppendPathSegment(forClientId, true)
            .GetJsonAsync<IEnumerable<Role>>()
            .ConfigureAwait(false);

    public async Task<IEnumerable<Role>> GetRealmRolesForCompositeAsync(string realm, string roleName) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/roles/")
            .AppendPathSegment(roleName, true)
            .AppendPathSegment("/composites/realm")
            .GetJsonAsync<IEnumerable<Role>>()
            .ConfigureAwait(false);

    [Obsolete("Not working yet")]
    public async Task<IEnumerable<Group>> GetGroupsWithRoleNameAsync(string realm, string roleName, int? first = null, bool? full = null, int? max = null)
    {
        var queryParams = new Dictionary<string, object?>
        {
            [nameof(first)] = first,
            [nameof(full)] = full,
            [nameof(max)] = max
        };

        return await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/roles/")
            .AppendPathSegment(roleName, true)
            .AppendPathSegment("/groups")
            .SetQueryParams(queryParams)
            .GetJsonAsync<IEnumerable<Group>>()
            .ConfigureAwait(false);
    }

    public async Task<ManagementPermission> GetRoleAuthorizationPermissionsInitializedAsync(string realm, string roleName) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/roles/")
            .AppendPathSegment(roleName, true)
            .AppendPathSegment("/management/permissions")
            .GetJsonAsync<ManagementPermission>()
            .ConfigureAwait(false);

    public async Task<ManagementPermission> SetRoleAuthorizationPermissionsInitializedAsync(string realm, string roleName, ManagementPermission managementPermission) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/roles/")
            .AppendPathSegment(roleName, true)
            .AppendPathSegment("/management/permissions")
            .PutJsonAsync(managementPermission)
            .ReceiveJson<ManagementPermission>()
            .ConfigureAwait(false);

    public async Task<IEnumerable<User>> GetUsersWithRoleNameAsync(string realm, string roleName, int? first = null, int? max = null)
    {
        var queryParams = new Dictionary<string, object?>
        {
            [nameof(first)] = first,
            [nameof(max)] = max
        };

        return await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/roles/")
            .AppendPathSegment(roleName, true)
            .AppendPathSegment("/users")
            .SetQueryParams(queryParams)
            .GetJsonAsync<IEnumerable<User>>()
            .ConfigureAwait(false);
    }
}
