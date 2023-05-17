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
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Clients;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.ClientScopes;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Common;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Groups;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.RealmsAdmin;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library;

public partial class KeycloakClient
{
	public async Task ImportRealmAsync(string realm, Realm rep) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms")
			.PostJsonAsync(rep)
			.ConfigureAwait(false);

	public async Task<IEnumerable<Realm>> GetRealmsAsync(string realm) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms")
			.GetJsonAsync<IEnumerable<Realm>>()
			.ConfigureAwait(false);

	public async Task<Realm> GetRealmAsync(string realm) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.GetJsonAsync<Realm>()
			.ConfigureAwait(false);

	public async Task UpdateRealmAsync(string realm, Realm rep) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.PutJsonAsync(rep)
			.ConfigureAwait(false);

	public async Task DeleteRealmAsync(string realm) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.DeleteAsync()
			.ConfigureAwait(false);

	public async Task<IEnumerable<AdminEvent>> GetAdminEventsAsync(string realm, string? authClient = null, string? authIpAddress = null, string? authRealm = null, string? authUser = null,
		string? dateFrom = null, string? dateTo = null, int? first = null, int? max = null,
		IEnumerable<string>? operationTypes = null, string? resourcePath = null, IEnumerable<string>? resourceTypes = null)
	{
		var queryParams = new Dictionary<string, object?>
		{
			[nameof(authClient)] = authClient,
			[nameof(authIpAddress)] = authIpAddress,
			[nameof(authRealm)] = authRealm,
			[nameof(authUser)] = authUser,
			[nameof(dateFrom)] = dateFrom,
			[nameof(dateTo)] = dateTo,
			[nameof(first)] = first,
			[nameof(max)] = max,
			[nameof(operationTypes)] = operationTypes == null ? null : string.Join(",", operationTypes),
			[nameof(resourcePath)] = resourcePath,
			[nameof(resourceTypes)] = resourceTypes == null ? null : string.Join(",", resourceTypes)
		};

		return await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/admin-events")
			.SetQueryParams(queryParams)
			.GetJsonAsync<IEnumerable<AdminEvent>>()
			.ConfigureAwait(false);
	}

	public async Task DeleteAdminEventsAsync(string realm) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/admin-events")
			.DeleteAsync()
			.ConfigureAwait(false);

	public async Task ClearKeysCacheAsync(string realm) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/clear-keys-cache")
			.PostAsync(new StringContent(""))
			.ConfigureAwait(false);

	public async Task ClearRealmCacheAsync(string realm) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/clear-realm-cache")
			.PostAsync(new StringContent(""))
			.ConfigureAwait(false);

	public async Task ClearUserCacheAsync(string realm) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/clear-user-cache")
			.PostAsync(new StringContent(""))
			.ConfigureAwait(false);

	public async Task<Client> BasePathForImportingClientsAsync(string realm, string description) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/client-description-converter")
			.PostAsync(new StringContent(description))
			.ReceiveJson<Client>()
			.ConfigureAwait(false);

	public async Task<IEnumerable<IDictionary<string, object>>> GetClientSessionStatsAsync(string realm) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/client-session-stats")
			.GetJsonAsync<IEnumerable<IDictionary<string, object>>>()
			.ConfigureAwait(false);

	public async Task<IEnumerable<ClientScope>> GetRealmDefaultClientScopesAsync(string realm) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/default-default-client-scopes")
			.GetJsonAsync<IEnumerable<ClientScope>>()
			.ConfigureAwait(false);

	public async Task UpdateRealmDefaultClientScopeAsync(string realm, string clientScopeId) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/default-default-client-scopes/")
			.AppendPathSegment(clientScopeId, true)
			.PutAsync(new StringContent(""))
			.ConfigureAwait(false);

	public async Task DeleteRealmDefaultClientScopeAsync(string realm, string clientScopeId) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/default-default-client-scopes/")
			.AppendPathSegment(clientScopeId, true)
			.DeleteAsync()
			.ConfigureAwait(false);

	public async Task<IEnumerable<Group>> GetRealmGroupHierarchyAsync(string realm) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/default-groups")
			.GetJsonAsync<IEnumerable<Group>>()
			.ConfigureAwait(false);

	public async Task UpdateRealmGroupAsync(string realm, string groupId) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/default-groups/")
			.AppendPathSegment(groupId, true)
			.PutAsync(new StringContent(""))
			.ConfigureAwait(false);

	public async Task DeleteRealmGroupAsync(string realm, string groupId) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/default-groups/")
			.AppendPathSegment(groupId, true)
			.DeleteAsync()
			.ConfigureAwait(false);

	public async Task<IEnumerable<ClientScope>> GetRealmOptionalClientScopesAsync(string realm) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/default-optional-client-scopes")
			.GetJsonAsync<IEnumerable<ClientScope>>()
			.ConfigureAwait(false);

	public async Task UpdateRealmOptionalClientScopeAsync(string realm, string clientScopeId) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/default-optional-client-scopes/")
			.AppendPathSegment(clientScopeId, true)
			.PutAsync(new StringContent(""))
			.ConfigureAwait(false);

	public async Task DeleteRealmOptionalClientScopeAsync(string realm, string clientScopeId) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/default-optional-client-scopes/")
			.AppendPathSegment(clientScopeId, true)
			.DeleteAsync()
			.ConfigureAwait(false);

	public async Task<IEnumerable<Event>> GetEventsAsync(string realm, string? client = null, string? dateFrom = null, string? dateTo = null, int? first = null,
		string? ipAddress = null, int? max = null, string? type = null, string? user = null)
	{
		var queryParams = new Dictionary<string, object?>
		{
			[nameof(client)] = client,
			[nameof(dateFrom)] = dateFrom,
			[nameof(dateTo)] = dateTo,
			[nameof(first)] = first,
			[nameof(max)] = max,
			[nameof(ipAddress)] = ipAddress,
			[nameof(type)] = type,
			[nameof(user)] = user
		};

		return await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/events")
			.SetQueryParams(queryParams)
			.GetJsonAsync<IEnumerable<Event>>()
			.ConfigureAwait(false);
	}

	public async Task DeleteEventsAsync(string realm) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/events")
			.DeleteAsync()
			.ConfigureAwait(false);

	public async Task<RealmEventsConfig> GetRealmEventsProviderConfigurationAsync(string realm) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/events/config")
			.GetJsonAsync<RealmEventsConfig>()
			.ConfigureAwait(false);

	public async Task UpdateRealmEventsProviderConfigurationAsync(string realm, RealmEventsConfig rep) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/events/config")
			.PutJsonAsync(rep)
			.ConfigureAwait(false);

	public async Task<Group> GetRealmGroupByPathAsync(string realm, string path) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/group-by-path/")
			.AppendPathSegment(path, true)
			.GetJsonAsync<Group>()
			.ConfigureAwait(false);

	public async Task<GlobalRequestResult> RemoveUserSessionsAsync(string realm) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/logout-all")
			.PostAsync(new StringContent(""))
			.ReceiveJson<GlobalRequestResult>()
			.ConfigureAwait(false);

	public async Task<Realm> RealmPartialExportAsync(string realm, bool? exportClients = null, bool? exportGroupsAndRoles = null)
	{
		var queryParams = new Dictionary<string, object?>
		{
			[nameof(exportClients)] = exportClients,
			[nameof(exportGroupsAndRoles)] = exportGroupsAndRoles,
		};

		return await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/partial-export")
			.SetQueryParams(queryParams)
			.PostAsync(new StringContent(""))
			.ReceiveJson<Realm>()
			.ConfigureAwait(false);
	}

	public async Task RealmPartialImportAsync(string realm, PartialImport rep) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/partialImport")
			.PostJsonAsync(rep)
			.ConfigureAwait(false);

	public async Task<GlobalRequestResult> PushRealmRevocationPolicyAsync(string realm) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/push-revocation")
			.PostAsync(new StringContent(""))
			.ReceiveJson<GlobalRequestResult>()
			.ConfigureAwait(false);

	public async Task DeleteUserSessionAsync(string realm, string session) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/sessions/")
			.AppendPathSegment(session, true)
			.DeleteAsync()
			.ConfigureAwait(false);

	public async Task TestLdapConnectionAsync(string realm, string? action = null, string? bindCredential = null, string? bindDn = null,
		string? componentId = null, string? connectionTimeout = null, string? connectionUrl = null, string? useTruststoreSpi = null) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/testLDAPConnection")
			.PostMultipartAsync(content => content
				.AddString(nameof(action), action)
				.AddString(nameof(bindCredential), bindCredential)
				.AddString(nameof(bindDn), bindDn)
				.AddString(nameof(componentId), componentId)
				.AddString(nameof(connectionTimeout), connectionTimeout)
				.AddString(nameof(connectionUrl), connectionUrl)
				.AddString(nameof(useTruststoreSpi), useTruststoreSpi))
			.ConfigureAwait(false);

	public async Task TestSmtpConnectionAsync(string realm, string config) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/testSMTPConnection/")
			.AppendPathSegment(config, true)
			.PostAsync(new StringContent(""))
			.ConfigureAwait(false);

	public async Task<ManagementPermission> GetRealmUsersManagementPermissionsAsync(string realm) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/users-management-permissions")
			.GetJsonAsync<ManagementPermission>()
			.ConfigureAwait(false);

	public async Task<ManagementPermission> UpdateRealmUsersManagementPermissionsAsync(string realm, ManagementPermission managementPermission) =>
		await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
			.AppendPathSegment("/admin/realms/")
			.AppendPathSegment(realm, true)
			.AppendPathSegment("/users-management-permissions")
			.PutJsonAsync(managementPermission)
			.ReceiveJson<ManagementPermission>()
			.ConfigureAwait(false);
}
