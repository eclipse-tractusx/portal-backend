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
public async Task<Mapping> GetScopeMappingsAsync(string realm, string clientScopeId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
	.AppendPathSegment("/admin/realms/")
	.AppendPathSegment(realm, true)
	.AppendPathSegment("/client-scopes/")
	.AppendPathSegment(clientScopeId, true)
	.AppendPathSegment("/scope-mappings")
	.GetJsonAsync<Mapping>()
	.ConfigureAwait(false);

public async Task<bool> AddClientRolesToClientScopeAsync(string realm, string clientScopeId, string clientId, IEnumerable<Role> roles)
{
	var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
		.AppendPathSegment("/admin/realms/")
		.AppendPathSegment(realm, true)
		.AppendPathSegment("/client-scopes/")
		.AppendPathSegment(clientScopeId, true)
		.AppendPathSegment("/scope-mappings/clients/")
		.AppendPathSegment(clientId, true)
		.PostJsonAsync(roles)
		.ConfigureAwait(false);
	return response.IsSuccessStatusCode;
}

public async Task<IEnumerable<Role>> GetClientRolesForClientScopeAsync(string realm, string clientScopeId, string clientId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
	.AppendPathSegment("/admin/realms/")
	.AppendPathSegment(realm, true)
	.AppendPathSegment("/client-scopes/")
	.AppendPathSegment(clientScopeId, true)
	.AppendPathSegment("/scope-mappings/clients/")
	.AppendPathSegment(clientId, true)
	.GetJsonAsync<IEnumerable<Role>>()
	.ConfigureAwait(false);

public async Task<bool> RemoveClientRolesFromClientScopeAsync(string realm, string clientScopeId, string clientId, IEnumerable<Role> roles)
{
	var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
		.AppendPathSegment("/admin/realms/")
		.AppendPathSegment(realm, true)
		.AppendPathSegment("/client-scopes/")
		.AppendPathSegment(clientScopeId, true)
		.AppendPathSegment("/scope-mappings/clients/")
		.AppendPathSegment(clientId, true)
		.SendJsonAsync(HttpMethod.Delete, roles)
		.ConfigureAwait(false);
	return response.IsSuccessStatusCode;
}

public async Task<IEnumerable<Role>> GetAvailableClientRolesForClientScopeAsync(string realm, string clientScopeId, string clientId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
	.AppendPathSegment("/admin/realms/")
	.AppendPathSegment(realm, true)
	.AppendPathSegment("/client-scopes/")
	.AppendPathSegment(clientScopeId, true)
	.AppendPathSegment("/scope-mappings/clients/")
	.AppendPathSegment(clientId, true)
	.AppendPathSegment("/available")
	.GetJsonAsync<IEnumerable<Role>>()
	.ConfigureAwait(false);

public async Task<IEnumerable<Role>> GetEffectiveClientRolesForClientScopeAsync(string realm, string clientScopeId, string clientId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
	.AppendPathSegment("/admin/realms/")
	.AppendPathSegment(realm, true)
	.AppendPathSegment("/client-scopes/")
	.AppendPathSegment(clientScopeId, true)
	.AppendPathSegment("/scope-mappings/clients/")
	.AppendPathSegment(clientId, true)
	.AppendPathSegment("/composite")
	.GetJsonAsync<IEnumerable<Role>>()
	.ConfigureAwait(false);

public async Task<bool> AddRealmRolesToClientScopeAsync(string realm, string clientScopeId, IEnumerable<Role> roles)
{
	var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
		.AppendPathSegment("/admin/realms/")
		.AppendPathSegment(realm, true)
		.AppendPathSegment("/client-scopes/")
		.AppendPathSegment(clientScopeId, true)
		.AppendPathSegment("/scope-mappings/realm")
		.PostJsonAsync(roles)
		.ConfigureAwait(false);
	return response.IsSuccessStatusCode;
}

public async Task<IEnumerable<Role>> GetRealmRolesForClientScopeAsync(string realm, string clientScopeId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
	.AppendPathSegment("/admin/realms/")
	.AppendPathSegment(realm, true)
	.AppendPathSegment("/client-scopes/")
	.AppendPathSegment(clientScopeId, true)
	.AppendPathSegment("/scope-mappings/realm")
	.GetJsonAsync<IEnumerable<Role>>()
	.ConfigureAwait(false);

public async Task<bool> RemoveRealmRolesFromClientScopeAsync(string realm, string clientScopeId, IEnumerable<Role> roles)
{
	var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
		.AppendPathSegment("/admin/realms/")
		.AppendPathSegment(realm, true)
		.AppendPathSegment("/client-scopes/")
		.AppendPathSegment(clientScopeId, true)
		.AppendPathSegment("/scope-mappings/realm")
		.SendJsonAsync(HttpMethod.Delete, roles)
		.ConfigureAwait(false);
	return response.IsSuccessStatusCode;
}

public async Task<IEnumerable<Role>> GetAvailableRealmRolesForClientScopeAsync(string realm, string clientScopeId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
	.AppendPathSegment("/admin/realms/")
	.AppendPathSegment(realm, true)
	.AppendPathSegment("/client-scopes/")
	.AppendPathSegment(clientScopeId, true)
	.AppendPathSegment("/scope-mappings/realm/available")
	.GetJsonAsync<IEnumerable<Role>>()
	.ConfigureAwait(false);

public async Task<IEnumerable<Role>> GetEffectiveRealmRolesForClientScopeAsync(string realm, string clientScopeId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
	.AppendPathSegment("/admin/realms/")
	.AppendPathSegment(realm, true)
	.AppendPathSegment("/client-scopes/")
	.AppendPathSegment(clientScopeId, true)
	.AppendPathSegment("/scope-mappings/realm/composite")
	.GetJsonAsync<IEnumerable<Role>>()
	.ConfigureAwait(false);

public async Task<Mapping> GetScopeMappingsForClientAsync(string realm, string clientId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
	.AppendPathSegment("/admin/realms/")
	.AppendPathSegment(realm, true)
	.AppendPathSegment("/clients/")
	.AppendPathSegment(clientId, true)
	.AppendPathSegment("/scope-mappings")
	.GetJsonAsync<Mapping>()
	.ConfigureAwait(false);

public async Task<bool> AddClientRolesScopeMappingToClientAsync(string realm, string clientId, string scopeClientId, IEnumerable<Role> roles)
{
	var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
		.AppendPathSegment("/admin/realms/")
		.AppendPathSegment(realm, true)
		.AppendPathSegment("/clients/")
		.AppendPathSegment(clientId, true)
		.AppendPathSegment("/scope-mappings/clients/")
		.AppendPathSegment(scopeClientId, true)
		.PostJsonAsync(roles)
		.ConfigureAwait(false);
	return response.IsSuccessStatusCode;
}

public async Task<IEnumerable<Role>> GetClientRolesScopeMappingsForClientAsync(string realm, string clientId, string scopeClientId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
	.AppendPathSegment("/admin/realms/")
	.AppendPathSegment(realm, true)
	.AppendPathSegment("/clients/")
	.AppendPathSegment(clientId, true)
	.AppendPathSegment("/scope-mappings/clients/")
	.AppendPathSegment(scopeClientId, true)
	.GetJsonAsync<IEnumerable<Role>>()
	.ConfigureAwait(false);

public async Task<bool> RemoveClientRolesFromClientScopeForClientAsync(string realm, string clientId, string scopeClientId, IEnumerable<Role> roles)
{
	var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
		.AppendPathSegment("/admin/realms/")
		.AppendPathSegment(realm, true)
		.AppendPathSegment("/clients/")
		.AppendPathSegment(clientId, true)
		.AppendPathSegment("/scope-mappings/clients/")
		.AppendPathSegment(scopeClientId, true)
		.SendJsonAsync(HttpMethod.Delete, roles)
		.ConfigureAwait(false);
	return response.IsSuccessStatusCode;
}

public async Task<IEnumerable<Role>> GetAvailableClientRolesForClientScopeForClientAsync(string realm, string clientId, string scopeClientId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
	.AppendPathSegment("/admin/realms/")
	.AppendPathSegment(realm, true)
	.AppendPathSegment("/clients/")
	.AppendPathSegment(clientId, true)
	.AppendPathSegment("/scope-mappings/clients/")
	.AppendPathSegment(scopeClientId, true)
	.AppendPathSegment("/available")
	.GetJsonAsync<IEnumerable<Role>>()
	.ConfigureAwait(false);

public async Task<IEnumerable<Role>> GetEffectiveClientRolesForClientScopeForClientAsync(string realm, string clientId, string scopeClientId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
	.AppendPathSegment("/admin/realms/")
	.AppendPathSegment(realm, true)
	.AppendPathSegment("/clients/")
	.AppendPathSegment(clientId, true)
	.AppendPathSegment("/scope-mappings/clients/")
	.AppendPathSegment(scopeClientId, true)
	.AppendPathSegment("/composite")
	.GetJsonAsync<IEnumerable<Role>>()
	.ConfigureAwait(false);

public async Task<bool> AddRealmRolesScopeMappingToClientAsync(string realm, string clientId, IEnumerable<Role> roles)
{
	var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
		.AppendPathSegment("/admin/realms/")
		.AppendPathSegment(realm, true)
		.AppendPathSegment("/clients/")
		.AppendPathSegment(clientId, true)
		.AppendPathSegment("/scope-mappings/realm")
		.PostJsonAsync(roles)
		.ConfigureAwait(false);
	return response.IsSuccessStatusCode;
}

public async Task<IEnumerable<Role>> GetRealmRolesScopeMappingsForClientAsync(string realm, string clientId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
	.AppendPathSegment("/admin/realms/")
	.AppendPathSegment(realm, true)
	.AppendPathSegment("/clients/")
	.AppendPathSegment(clientId, true)
	.AppendPathSegment("/scope-mappings/realm")
	.GetJsonAsync<IEnumerable<Role>>()
	.ConfigureAwait(false);

public async Task<bool> RemoveRealmRolesFromClientScopeForClientAsync(string realm, string clientId, IEnumerable<Role> roles)
{
	var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
		.AppendPathSegment("/admin/realms/")
		.AppendPathSegment(realm, true)
		.AppendPathSegment("/clients/")
		.AppendPathSegment(clientId, true)
		.AppendPathSegment("/scope-mappings/realm")
		.SendJsonAsync(HttpMethod.Delete, roles)
		.ConfigureAwait(false);
	return response.IsSuccessStatusCode;
}

public async Task<IEnumerable<Role>> GetAvailableRealmRolesForClientScopeForClientAsync(string realm, string clientId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
	.AppendPathSegment("/admin/realms/")
	.AppendPathSegment(realm, true)
	.AppendPathSegment("/clients/")
	.AppendPathSegment(clientId, true)
	.AppendPathSegment("/scope-mappings/realm/available")
	.GetJsonAsync<IEnumerable<Role>>()
	.ConfigureAwait(false);

public async Task<IEnumerable<Role>> GetEffectiveRealmRolesForClientScopeForClientAsync(string realm, string clientId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
	.AppendPathSegment("/admin/realms/")
	.AppendPathSegment(realm, true)
	.AppendPathSegment("/clients/")
	.AppendPathSegment(clientId, true)
	.AppendPathSegment("/scope-mappings/realm/composite")
	.GetJsonAsync<IEnumerable<Role>>()
	.ConfigureAwait(false);
}
