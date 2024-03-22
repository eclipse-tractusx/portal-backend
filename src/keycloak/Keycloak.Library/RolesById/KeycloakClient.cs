/********************************************************************************
 * MIT License
 *
 * Copyright (c) 2019 Luk Vermeulen
 * Copyright (c) 2021, 2023 Contributors to the Eclipse Foundation
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 ********************************************************************************/

using Flurl.Http;
using Flurl.Http.Content;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Common;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Roles;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library;

public partial class KeycloakClient
{
    public async Task<Role> GetRoleByIdAsync(string realm, string roleId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/roles-by-id/")
            .AppendPathSegment(roleId, true)
            .GetJsonAsync<Role>()
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task UpdateRoleByIdAsync(string realm, string roleId, Role role, CancellationToken cancellationToken = default) =>
        await (await GetBaseUrlAsync(realm, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/roles-by-id/")
            .AppendPathSegment(roleId, true)
            .PutJsonAsync(role, cancellationToken)
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task DeleteRoleByIdAsync(string realm, string roleId, CancellationToken cancellationToken = default) =>
        await (await GetBaseUrlAsync(realm, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/roles-by-id/")
            .AppendPathSegment(roleId, true)
            .DeleteAsync(cancellationToken)
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task MakeRoleCompositeAsync(string realm, string roleId, IEnumerable<Role> roles) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/roles-by-id/")
            .AppendPathSegment(roleId, true)
            .AppendPathSegment("/composites")
            .PostJsonAsync(roles)
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task<IEnumerable<Role>> GetRoleChildrenAsync(string realm, string roleId, CancellationToken cancellationToken = default) =>
        await (await GetBaseUrlAsync(realm, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/roles-by-id/")
            .AppendPathSegment(roleId, true)
            .AppendPathSegment("/composites")
            .GetJsonAsync<IEnumerable<Role>>(cancellationToken)
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task RemoveRolesFromCompositeAsync(string realm, string roleId, IEnumerable<Role> roles)
    {
        using var jsonContent = new CapturedJsonContent(_serializer.Serialize(roles));
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/roles-by-id/")
            .AppendPathSegment(roleId, true)
            .AppendPathSegment("/composites")
            .SendJsonAsync(HttpMethod.Delete, jsonContent)
            .ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public async Task<IEnumerable<Role>> GetClientRolesForCompositeByIdAsync(string realm, string roleId, string clientId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/roles-by-id/")
            .AppendPathSegment(roleId, true)
            .AppendPathSegment("/composites/clients/")
            .AppendPathSegment(clientId, true)
            .GetJsonAsync<IEnumerable<Role>>()
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task<IEnumerable<Role>> GetRealmRolesForCompositeByIdAsync(string realm, string roleId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/roles-by-id/")
            .AppendPathSegment(roleId, true)
            .AppendPathSegment("/composites/realm")
            .GetJsonAsync<IEnumerable<Role>>()
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task<ManagementPermission> GetRoleByIdAuthorizationPermissionsInitializedAsync(string realm, string roleId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/roles-by-id/")
            .AppendPathSegment(roleId, true)
            .AppendPathSegment("/management/permissions")
            .GetJsonAsync<ManagementPermission>()
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task<ManagementPermission> SetRoleByIdAuthorizationPermissionsInitializedAsync(string realm, string roleId, ManagementPermission managementPermission) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/roles-by-id/")
            .AppendPathSegment(roleId, true)
            .AppendPathSegment("/management/permissions")
            .PutJsonAsync(managementPermission)
            .ReceiveJson<ManagementPermission>()
            .ConfigureAwait(ConfigureAwaitOptions.None);
}
