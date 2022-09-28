/********************************************************************************
 * MIT License
 *
 * Copyright (c) 2019 Luk Vermeulen
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Roles;
using Flurl.Http;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library;

public partial class KeycloakClient
{
    public async Task AddClientRoleMappingsToGroupAsync(string realm, string groupId, string clientId, IEnumerable<Role> roles) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/groups/")
            .AppendPathSegment(groupId, true)
            .AppendPathSegment("/role-mappings/clients/")
            .AppendPathSegment(clientId, true)
            .PostJsonAsync(roles)
            .ConfigureAwait(false);

    public async Task<IEnumerable<Role>> GetClientRoleMappingsForGroupAsync(string realm, string groupId, string clientId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/groups/")
            .AppendPathSegment(groupId, true)
            .AppendPathSegment("/role-mappings/clients/")
            .AppendPathSegment(clientId, true)
            .GetJsonAsync<IEnumerable<Role>>()
            .ConfigureAwait(false);

    public async Task DeleteClientRoleMappingsFromGroupAsync(string realm, string groupId, string clientId, IEnumerable<Role> roles) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/groups/")
            .AppendPathSegment(groupId, true)
            .AppendPathSegment("/role-mappings/clients/")
            .AppendPathSegment(clientId, true)
            .SendJsonAsync(HttpMethod.Delete, roles)
            .ConfigureAwait(false);

    public async Task<IEnumerable<Role>> GetAvailableClientRoleMappingsForGroupAsync(string realm, string groupId, string clientId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/groups/")
            .AppendPathSegment(groupId, true)
            .AppendPathSegment("/role-mappings/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/available")
            .GetJsonAsync<IEnumerable<Role>>()
            .ConfigureAwait(false);

    public async Task<IEnumerable<Role>> GetEffectiveClientRoleMappingsForGroupAsync(string realm, string groupId, string clientId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/groups/")
            .AppendPathSegment(groupId, true)
            .AppendPathSegment("/role-mappings/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/composite")
            .GetJsonAsync<IEnumerable<Role>>()
            .ConfigureAwait(false);

    public async Task AddClientRoleMappingsToUserAsync(string realm, string userId, string clientId, IEnumerable<Role> roles) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/users/")
            .AppendPathSegment(userId, true)
            .AppendPathSegment("/role-mappings/clients/")
            .AppendPathSegment(clientId, true)
            .PostJsonAsync(roles)
            .ConfigureAwait(false);

    public async Task<IEnumerable<Role>> GetClientRoleMappingsForUserAsync(string realm, string userId, string clientId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/users/")
            .AppendPathSegment(userId, true)
            .AppendPathSegment("/role-mappings/clients/")
            .AppendPathSegment(clientId, true)
            .GetJsonAsync<IEnumerable<Role>>()
            .ConfigureAwait(false);

    public async Task DeleteClientRoleMappingsFromUserAsync(string realm, string userId, string clientId, IEnumerable<Role> roles) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/users/")
            .AppendPathSegment(userId, true)
            .AppendPathSegment("/role-mappings/clients/")
            .AppendPathSegment(clientId, true)
            .SendJsonAsync(HttpMethod.Delete, roles)
            .ConfigureAwait(false);

    public async Task<IEnumerable<Role>> GetAvailableClientRoleMappingsForUserAsync(string realm, string userId, string clientId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/users/")
            .AppendPathSegment(userId, true)
            .AppendPathSegment("/role-mappings/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/available")
            .GetJsonAsync<IEnumerable<Role>>()
            .ConfigureAwait(false);

    public async Task<IEnumerable<Role>> GetEffectiveClientRoleMappingsForUserAsync(string realm, string userId, string clientId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
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
