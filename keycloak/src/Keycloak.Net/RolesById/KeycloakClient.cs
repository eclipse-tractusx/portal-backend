using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl.Http;
using Flurl.Http.Content;
using Keycloak.Net.Models.Common;
using Keycloak.Net.Models.Roles;

namespace Keycloak.Net
{
    public partial class KeycloakClient
    {
        public async Task<Role> GetRoleByIdAsync(string realm, string roleId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/roles-by-id/")
            .AppendPathSegment(roleId, true)
            .GetJsonAsync<Role>()
            .ConfigureAwait(false);

        public async Task<bool> UpdateRoleByIdAsync(string realm, string roleId, Role role)
        {
            var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
                .AppendPathSegment("/admin/realms/")
                .AppendPathSegment(realm, true)
                .AppendPathSegment("/roles-by-id/")
                .AppendPathSegment(roleId, true)
                .PutJsonAsync(role)
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteRoleByIdAsync(string realm, string roleId)
        {
            var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
                .AppendPathSegment("/admin/realms/")
                .AppendPathSegment(realm, true)
                .AppendPathSegment("/roles-by-id/")
                .AppendPathSegment(roleId, true)
                .DeleteAsync()
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> MakeRoleCompositeAsync(string realm, string roleId, IEnumerable<Role> roles)
        {
            var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
                .AppendPathSegment("/admin/realms/")
                .AppendPathSegment(realm, true)
                .AppendPathSegment("/roles-by-id/")
                .AppendPathSegment(roleId, true)
                .AppendPathSegment("/composites")
                .PostJsonAsync(roles)
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        public async Task<IEnumerable<Role>> GetRoleChildrenAsync(string realm, string roleId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/roles-by-id/")
            .AppendPathSegment(roleId, true)
            .AppendPathSegment("/composites")
            .GetJsonAsync<IEnumerable<Role>>()
            .ConfigureAwait(false);

        public async Task<bool> RemoveRolesFromCompositeAsync(string realm, string roleId, IEnumerable<Role> roles)
        {
            var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
                .AppendPathSegment("/admin/realms/")
                .AppendPathSegment(realm, true)
                .AppendPathSegment("/roles-by-id/")
                .AppendPathSegment(roleId, true)
                .AppendPathSegment("/composites")
                .SendJsonAsync(HttpMethod.Delete, new CapturedJsonContent(_serializer.Serialize(roles)))
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        public async Task<IEnumerable<Role>> GetClientRolesForCompositeByIdAsync(string realm, string roleId, string clientId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/roles-by-id/")
            .AppendPathSegment(roleId, true)
            .AppendPathSegment("/composites/clients/")
            .AppendPathSegment(clientId, true)
            .GetJsonAsync<IEnumerable<Role>>()
            .ConfigureAwait(false);

        public async Task<IEnumerable<Role>> GetRealmRolesForCompositeByIdAsync(string realm, string roleId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/roles-by-id/")
            .AppendPathSegment(roleId, true)
            .AppendPathSegment("/composites/realm")
            .GetJsonAsync<IEnumerable<Role>>()
            .ConfigureAwait(false);

        public async Task<ManagementPermission> GetRoleByIdAuthorizationPermissionsInitializedAsync(string realm, string roleId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/roles-by-id/")
            .AppendPathSegment(roleId, true)
            .AppendPathSegment("/management/permissions")
            .GetJsonAsync<ManagementPermission>()
            .ConfigureAwait(false);

        public async Task<ManagementPermission> SetRoleByIdAuthorizationPermissionsInitializedAsync(string realm, string roleId, ManagementPermission managementPermission) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/roles-by-id/")
            .AppendPathSegment(roleId, true)
            .AppendPathSegment("/management/permissions")
            .PutJsonAsync(managementPermission)
            .ReceiveJson<ManagementPermission>()
            .ConfigureAwait(false);
    }
}
