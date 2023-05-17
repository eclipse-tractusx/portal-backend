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

using Flurl.Http;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Common;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Groups;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Roles;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Users;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library;

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

	public async Task CreateRoleAsync(string realm, Role role) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
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

	public async Task<Role> GetRoleByNameAsync(string realm, string roleName) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
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

	public async Task UpdateRoleByNameAsync(string realm, string roleName, Role role) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
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

	public async Task DeleteRoleByNameAsync(string realm, string roleName) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
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

	public async Task AddCompositesToRoleAsync(string realm, string roleName, IEnumerable<Role> roles) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
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

	public async Task<IEnumerable<Role>> GetRoleCompositesAsync(string realm, string roleName) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
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

	public async Task RemoveCompositesFromRoleAsync(string realm, string roleName, IEnumerable<Role> roles) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
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

	public async Task<ManagementPermission> GetRoleAuthorizationPermissionsInitializedAsync(string realm, string roleName) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
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
