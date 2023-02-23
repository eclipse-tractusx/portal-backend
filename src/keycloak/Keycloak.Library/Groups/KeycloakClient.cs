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

using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Common.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Common;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Groups;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Users;
using Flurl.Http;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library;

public partial class KeycloakClient
{
    public async Task CreateGroupAsync(string realm, Group group) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/groups")
            .PostJsonAsync(group)
            .ConfigureAwait(false);

    public async Task<IEnumerable<Group>> GetGroupHierarchyAsync(string realm, int? first = null, int? max = null, string? search = null)
    {
        var queryParams = new Dictionary<string, object?>
        {
            [nameof(first)] = first,
            [nameof(max)] = max,
            [nameof(search)] = search,
            ["briefRepresentation"] = false
        };

        return await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/groups")
            .SetQueryParams(queryParams)
            .GetJsonAsync<IEnumerable<Group>>()
            .ConfigureAwait(false);
    }

    public async Task<int> GetGroupsCountAsync(string realm, string? search = null, bool? top = null)
    {
        var queryParams = new Dictionary<string, object?>
        {
            [nameof(search)] = search,
            [nameof(top)] = top
        };

        var result = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/groups/count")
            .SetQueryParams(queryParams)
            .GetJsonAsync()
            .ConfigureAwait(false);

        return Convert.ToInt32(DynamicExtensions.GetFirstPropertyValue(result));
    }

    public async Task<Group> GetGroupAsync(string realm, string groupId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/groups/")
            .AppendPathSegment(groupId, true)
            .GetJsonAsync<Group>()
            .ConfigureAwait(false);

    public async Task UpdateGroupAsync(string realm, string groupId, Group group) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/groups/")
            .AppendPathSegment(groupId, true)
            .PutJsonAsync(group)
            .ConfigureAwait(false);

    public async Task DeleteGroupAsync(string realm, string groupId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/groups/")
            .AppendPathSegment(groupId, true)
            .DeleteAsync()
            .ConfigureAwait(false);

    public async Task SetOrCreateGroupChildAsync(string realm, string groupId, Group group) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/groups/")
            .AppendPathSegment(groupId, true)
            .AppendPathSegment("/children")
            .PostJsonAsync(group)
            .ConfigureAwait(false);

    public async Task<ManagementPermission> GetGroupClientAuthorizationPermissionsInitializedAsync(string realm, string groupId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/groups/")
            .AppendPathSegment(groupId, true)
            .AppendPathSegment("/management/permissions")
            .GetJsonAsync<ManagementPermission>()
            .ConfigureAwait(false);

    public async Task<ManagementPermission> SetGroupClientAuthorizationPermissionsInitializedAsync(string realm, string groupId, ManagementPermission managementPermission) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/groups/")
            .AppendPathSegment(groupId, true)
            .AppendPathSegment("/management/permissions")
            .PutJsonAsync(managementPermission)
            .ReceiveJson<ManagementPermission>()
            .ConfigureAwait(false);

    public async Task<IEnumerable<User>> GetGroupUsersAsync(string realm, string groupId, int? first = null, int? max = null) 
    {
        var queryParams = new Dictionary<string, object?>
        {
            [nameof(first)] = first,
            [nameof(max)] = max
        };

        return await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/groups/")
            .AppendPathSegment(groupId, true)
            .AppendPathSegment("/members")
            .SetQueryParams(queryParams)
            .GetJsonAsync<IEnumerable<User>>()
            .ConfigureAwait(false);
    }
}
