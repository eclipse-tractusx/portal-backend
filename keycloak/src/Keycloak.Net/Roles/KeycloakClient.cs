using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl.Http;
using Keycloak.Net.Models.Common;
using Keycloak.Net.Models.Groups;
using Keycloak.Net.Models.Roles;
using Keycloak.Net.Models.Users;

namespace Keycloak.Net
{
    public partial class KeycloakClient
    {
        public async Task<bool> CreateRoleAsync(string realm, string clientId, Role role)
        {
            var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
                .AppendPathSegment("/admin/realms/")
                .AppendPathSegment(realm, true)
                .AppendPathSegment("/clients/")
                .AppendPathSegment(clientId, true)
                .AppendPathSegment("/roles")
                .PostJsonAsync(role)
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        public async Task<IEnumerable<Role>> GetRolesAsync(string realm, string clientId, int? first = null, int? max = null, string search = null)
        {
            var queryParams = new Dictionary<string, object>
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

        public async Task<Role> GetRoleByNameAsync(string realm, string clientId, string roleName) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/roles/")
            .AppendPathSegment(roleName, true)
            .GetJsonAsync<Role>()
            .ConfigureAwait(false);
        
        public async Task<bool> UpdateRoleByNameAsync(string realm, string clientId, string roleName, Role role)
        {
            var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
                .AppendPathSegment("/admin/realms/")
                .AppendPathSegment(realm, true)
                .AppendPathSegment("/clients/")
                .AppendPathSegment(clientId, true)
                .AppendPathSegment("/roles/")
                .AppendPathSegment(roleName, true)
                .PutJsonAsync(role)
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteRoleByNameAsync(string realm, string clientId, string roleName)
        {
            var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
                .AppendPathSegment("/admin/realms/")
                .AppendPathSegment(realm, true)
                .AppendPathSegment("/clients/")
                .AppendPathSegment(clientId, true)
                .AppendPathSegment("/roles/")
                .AppendPathSegment(roleName, true)
                .DeleteAsync()
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> AddCompositesToRoleAsync(string realm, string clientId, string roleName, IEnumerable<Role> roles)
        {
            var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
                .AppendPathSegment("/admin/realms/")
                .AppendPathSegment(realm, true)
                .AppendPathSegment("/clients/")
                .AppendPathSegment(clientId, true)
                .AppendPathSegment("/roles/")
                .AppendPathSegment(roleName, true)
                .AppendPathSegment("/composites")
                .PostJsonAsync(roles)
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        public async Task<IEnumerable<Role>> GetRoleCompositesAsync(string realm, string clientId, string roleName) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/roles/")
            .AppendPathSegment(roleName, true)
            .AppendPathSegment("/composites")
            .GetJsonAsync<IEnumerable<Role>>()
            .ConfigureAwait(false);

        public async Task<bool> RemoveCompositesFromRoleAsync(string realm, string clientId, string roleName, IEnumerable<Role> roles)
        {
            var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
                .AppendPathSegment("/admin/realms/")
                .AppendPathSegment(realm, true)
                .AppendPathSegment("/clients/")
                .AppendPathSegment(clientId, true)
                .AppendPathSegment("/roles/")
                .AppendPathSegment(roleName, true)
                .AppendPathSegment("/composites")
                .SendJsonAsync(HttpMethod.Delete, roles)
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        public async Task<IEnumerable<Role>> GetApplicationRolesForCompositeAsync(string realm, string clientId, string roleName, string forClientId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
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

        public async Task<IEnumerable<Role>> GetRealmRolesForCompositeAsync(string realm, string clientId, string roleName) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
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
            var queryParams = new Dictionary<string, object>
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

        public async Task<ManagementPermission> GetRoleAuthorizationPermissionsInitializedAsync(string realm, string clientId, string roleName) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/roles/")
            .AppendPathSegment(roleName, true)
            .AppendPathSegment("/management/permissions")
            .GetJsonAsync<ManagementPermission>()
            .ConfigureAwait(false);

        public async Task<ManagementPermission> SetRoleAuthorizationPermissionsInitializedAsync(string realm, string clientId, string roleName, ManagementPermission managementPermission) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
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
            var queryParams = new Dictionary<string, object>
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

        public async Task<bool> CreateRoleAsync(string realm, Role role)
        {
            var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
                .AppendPathSegment("/admin/realms/")
                .AppendPathSegment(realm, true)
                .AppendPathSegment("/roles")
                .PostJsonAsync(role)
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        public async Task<IEnumerable<Role>> GetRolesAsync(string realm, int? first = null, int? max = null, string search = null)
        {
            var queryParams = new Dictionary<string, object>
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

        public async Task<Role> GetRoleByNameAsync(string realm, string roleName) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/roles/")
            .AppendPathSegment(roleName, true)
            .GetJsonAsync<Role>()
            .ConfigureAwait(false);
        
        public async Task<bool> UpdateRoleByNameAsync(string realm, string roleName, Role role)
        {
            var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
                .AppendPathSegment("/admin/realms/")
                .AppendPathSegment(realm, true)
                .AppendPathSegment("/roles/")
                .AppendPathSegment(roleName, true)
                .PutJsonAsync(role)
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteRoleByNameAsync(string realm, string roleName)
        {
            var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
                .AppendPathSegment("/admin/realms/")
                .AppendPathSegment(realm, true)
                .AppendPathSegment("/roles/")
                .AppendPathSegment(roleName, true)
                .DeleteAsync()
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> AddCompositesToRoleAsync(string realm, string roleName, IEnumerable<Role> roles)
        {
            var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
                .AppendPathSegment("/admin/realms/")
                .AppendPathSegment(realm, true)
                .AppendPathSegment("/roles/")
                .AppendPathSegment(roleName, true)
                .AppendPathSegment("/composites")
                .PostJsonAsync(roles)
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        public async Task<IEnumerable<Role>> GetRoleCompositesAsync(string realm, string roleName) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/roles/")
            .AppendPathSegment(roleName, true)
            .AppendPathSegment("/composites")
            .GetJsonAsync<IEnumerable<Role>>()
            .ConfigureAwait(false);

        public async Task<bool> RemoveCompositesFromRoleAsync(string realm, string roleName, IEnumerable<Role> roles)
        {
            var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
                .AppendPathSegment("/admin/realms/")
                .AppendPathSegment(realm, true)
                .AppendPathSegment("/roles/")
                .AppendPathSegment(roleName, true)
                .AppendPathSegment("/composites")
                .SendJsonAsync(HttpMethod.Delete, roles)
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        public async Task<IEnumerable<Role>> GetApplicationRolesForCompositeAsync(string realm, string roleName, string forClientId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/roles/")
            .AppendPathSegment(roleName, true)
            .AppendPathSegment("/composites/clients/")
            .AppendPathSegment(forClientId, true)
            .GetJsonAsync<IEnumerable<Role>>()
            .ConfigureAwait(false);

        public async Task<IEnumerable<Role>> GetRealmRolesForCompositeAsync(string realm, string roleName) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
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
            var queryParams = new Dictionary<string, object>
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

        public async Task<ManagementPermission> GetRoleAuthorizationPermissionsInitializedAsync(string realm, string roleName) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/roles/")
            .AppendPathSegment(roleName, true)
            .AppendPathSegment("/management/permissions")
            .GetJsonAsync<ManagementPermission>()
            .ConfigureAwait(false);

        public async Task<ManagementPermission> SetRoleAuthorizationPermissionsInitializedAsync(string realm, string roleName, ManagementPermission managementPermission) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
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
            var queryParams = new Dictionary<string, object>
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
}
