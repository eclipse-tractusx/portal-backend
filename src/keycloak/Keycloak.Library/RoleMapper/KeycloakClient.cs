/********************************************************************************
 * MIT License
 *
 * Copyright (c) 2019 Luk Vermeulen
 * Copyright (c) 2021, 2023 BMW Group AG
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

using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Common;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Roles;
using Flurl.Http;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library;

public partial class KeycloakClient
{
    public async Task<Mapping> GetRoleMappingsForGroupAsync(string realm, string groupId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/groups/")
            .AppendPathSegment(groupId, true)
            .AppendPathSegment("/role-mappings")
            .GetJsonAsync<Mapping>()
            .ConfigureAwait(false);

    public async Task AddRealmRoleMappingsToGroupAsync(string realm, string groupId, IEnumerable<Role> roles) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/groups/")
            .AppendPathSegment(groupId, true)
            .AppendPathSegment("/role-mappings/realm")
            .PostJsonAsync(roles)
            .ConfigureAwait(false);

    public async Task<IEnumerable<Role>> GetRealmRoleMappingsForGroupAsync(string realm, string groupId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/groups/")
            .AppendPathSegment(groupId, true)
            .AppendPathSegment("/role-mappings/realm")
            .GetJsonAsync<IEnumerable<Role>>()
            .ConfigureAwait(false);

    public async Task DeleteRealmRoleMappingsFromGroupAsync(string realm, string groupId, IEnumerable<Role> roles) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/groups/")
            .AppendPathSegment(groupId, true)
            .AppendPathSegment("/role-mappings/realm")
            .SendJsonAsync(HttpMethod.Delete, roles)
            .ConfigureAwait(false);

    public async Task<IEnumerable<Role>> GetAvailableRealmRoleMappingsForGroupAsync(string realm, string groupId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/groups/")
            .AppendPathSegment(groupId, true)
            .AppendPathSegment("/role-mappings/realm/available")
            .GetJsonAsync<IEnumerable<Role>>()
            .ConfigureAwait(false);

    public async Task<IEnumerable<Role>> GetEffectiveRealmRoleMappingsForGroupAsync(string realm, string groupId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/groups/")
            .AppendPathSegment(groupId, true)
            .AppendPathSegment("/role-mappings/realm/composite")
            .GetJsonAsync<IEnumerable<Role>>()
            .ConfigureAwait(false);

    public async Task<Mapping> GetRoleMappingsForUserAsync(string realm, string userId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/users/")
            .AppendPathSegment(userId, true)
            .AppendPathSegment("/role-mappings")
            .GetJsonAsync<Mapping>()
            .ConfigureAwait(false);

    public async Task AddRealmRoleMappingsToUserAsync(string realm, string userId, IEnumerable<Role> roles) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/users/")
            .AppendPathSegment(userId, true)
            .AppendPathSegment("/role-mappings/realm")
            .PostJsonAsync(roles)
            .ConfigureAwait(false);

    public async Task<IEnumerable<Role>> GetRealmRoleMappingsForUserAsync(string realm, string userId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/users/")
            .AppendPathSegment(userId, true)
            .AppendPathSegment("/role-mappings/realm")
            .GetJsonAsync<IEnumerable<Role>>()
            .ConfigureAwait(false);

    public async Task DeleteRealmRoleMappingsFromUserAsync(string realm, string userId, IEnumerable<Role> roles) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/users/")
            .AppendPathSegment(userId, true)
            .AppendPathSegment("/role-mappings/realm")
            .SendJsonAsync(HttpMethod.Delete, roles)
            .ConfigureAwait(false);

    public async Task<IEnumerable<Role>> GetAvailableRealmRoleMappingsForUserAsync(string realm, string userId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/users/")
            .AppendPathSegment(userId, true)
            .AppendPathSegment("/role-mappings/realm/available")
            .GetJsonAsync<IEnumerable<Role>>()
            .ConfigureAwait(false);

    public async Task<IEnumerable<Role>> GetEffectiveRealmRoleMappingsForUserAsync(string realm, string userId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/users/")
            .AppendPathSegment(userId, true)
            .AppendPathSegment("/role-mappings/realm/composite")
            .GetJsonAsync<IEnumerable<Role>>()
            .ConfigureAwait(false);
}
