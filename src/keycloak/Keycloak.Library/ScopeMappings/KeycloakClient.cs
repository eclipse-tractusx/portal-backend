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
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Roles;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library;

public partial class KeycloakClient
{
    public async Task<Mapping> GetScopeMappingsAsync(string realm, string clientScopeId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/client-scopes/")
            .AppendPathSegment(clientScopeId, true)
            .AppendPathSegment("/scope-mappings")
            .GetJsonAsync<Mapping>()
            .ConfigureAwait(false);

    public async Task AddClientRolesToClientScopeAsync(string realm, string clientScopeId, string clientId, IEnumerable<Role> roles) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/client-scopes/")
            .AppendPathSegment(clientScopeId, true)
            .AppendPathSegment("/scope-mappings/clients/")
            .AppendPathSegment(clientId, true)
            .PostJsonAsync(roles)
            .ConfigureAwait(false);

    public async Task<IEnumerable<Role>> GetClientRolesForClientScopeAsync(string realm, string clientScopeId, string clientId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/client-scopes/")
            .AppendPathSegment(clientScopeId, true)
            .AppendPathSegment("/scope-mappings/clients/")
            .AppendPathSegment(clientId, true)
            .GetJsonAsync<IEnumerable<Role>>()
            .ConfigureAwait(false);

    public async Task RemoveClientRolesFromClientScopeAsync(string realm, string clientScopeId, string clientId, IEnumerable<Role> roles) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/client-scopes/")
            .AppendPathSegment(clientScopeId, true)
            .AppendPathSegment("/scope-mappings/clients/")
            .AppendPathSegment(clientId, true)
            .SendJsonAsync(HttpMethod.Delete, roles)
            .ConfigureAwait(false);

    public async Task<IEnumerable<Role>> GetAvailableClientRolesForClientScopeAsync(string realm, string clientScopeId, string clientId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/client-scopes/")
            .AppendPathSegment(clientScopeId, true)
            .AppendPathSegment("/scope-mappings/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/available")
            .GetJsonAsync<IEnumerable<Role>>()
            .ConfigureAwait(false);

    public async Task<IEnumerable<Role>> GetEffectiveClientRolesForClientScopeAsync(string realm, string clientScopeId, string clientId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/client-scopes/")
            .AppendPathSegment(clientScopeId, true)
            .AppendPathSegment("/scope-mappings/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/composite")
            .GetJsonAsync<IEnumerable<Role>>()
            .ConfigureAwait(false);

    public async Task AddRealmRolesToClientScopeAsync(string realm, string clientScopeId, IEnumerable<Role> roles) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/client-scopes/")
            .AppendPathSegment(clientScopeId, true)
            .AppendPathSegment("/scope-mappings/realm")
            .PostJsonAsync(roles)
            .ConfigureAwait(false);

    public async Task<IEnumerable<Role>> GetRealmRolesForClientScopeAsync(string realm, string clientScopeId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/client-scopes/")
            .AppendPathSegment(clientScopeId, true)
            .AppendPathSegment("/scope-mappings/realm")
            .GetJsonAsync<IEnumerable<Role>>()
            .ConfigureAwait(false);

    public async Task RemoveRealmRolesFromClientScopeAsync(string realm, string clientScopeId, IEnumerable<Role> roles) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/client-scopes/")
            .AppendPathSegment(clientScopeId, true)
            .AppendPathSegment("/scope-mappings/realm")
            .SendJsonAsync(HttpMethod.Delete, roles)
            .ConfigureAwait(false);

    public async Task<IEnumerable<Role>> GetAvailableRealmRolesForClientScopeAsync(string realm, string clientScopeId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/client-scopes/")
            .AppendPathSegment(clientScopeId, true)
            .AppendPathSegment("/scope-mappings/realm/available")
            .GetJsonAsync<IEnumerable<Role>>()
            .ConfigureAwait(false);

    public async Task<IEnumerable<Role>> GetEffectiveRealmRolesForClientScopeAsync(string realm, string clientScopeId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/client-scopes/")
            .AppendPathSegment(clientScopeId, true)
            .AppendPathSegment("/scope-mappings/realm/composite")
            .GetJsonAsync<IEnumerable<Role>>()
            .ConfigureAwait(false);

    public async Task<Mapping> GetScopeMappingsForClientAsync(string realm, string clientId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/scope-mappings")
            .GetJsonAsync<Mapping>()
            .ConfigureAwait(false);

    public async Task AddClientRolesScopeMappingToClientAsync(string realm, string clientId, string scopeClientId, IEnumerable<Role> roles, CancellationToken cancellationToken = default) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/scope-mappings/clients/")
            .AppendPathSegment(scopeClientId, true)
            .PostJsonAsync(roles, cancellationToken)
            .ConfigureAwait(false);

    public async Task<IEnumerable<Role>> GetClientRolesScopeMappingsForClientAsync(string realm, string clientId, string scopeClientId, CancellationToken cancellationToken = default) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/scope-mappings/clients/")
            .AppendPathSegment(scopeClientId, true)
            .GetJsonAsync<IEnumerable<Role>>(cancellationToken)
            .ConfigureAwait(false);

    public async Task RemoveClientRolesFromClientScopeForClientAsync(string realm, string clientId, string scopeClientId, IEnumerable<Role> roles, CancellationToken cancellationToken = default) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/scope-mappings/clients/")
            .AppendPathSegment(scopeClientId, true)
            .SendJsonAsync(HttpMethod.Delete, roles, cancellationToken)
            .ConfigureAwait(false);

    public async Task<IEnumerable<Role>> GetAvailableClientRolesForClientScopeForClientAsync(string realm, string clientId, string scopeClientId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/scope-mappings/clients/")
            .AppendPathSegment(scopeClientId, true)
            .AppendPathSegment("/available")
            .GetJsonAsync<IEnumerable<Role>>()
            .ConfigureAwait(false);

    public async Task<IEnumerable<Role>> GetEffectiveClientRolesForClientScopeForClientAsync(string realm, string clientId, string scopeClientId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/scope-mappings/clients/")
            .AppendPathSegment(scopeClientId, true)
            .AppendPathSegment("/composite")
            .GetJsonAsync<IEnumerable<Role>>()
            .ConfigureAwait(false);

    public async Task AddRealmRolesScopeMappingToClientAsync(string realm, string clientId, IEnumerable<Role> roles) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/scope-mappings/realm")
            .PostJsonAsync(roles)
            .ConfigureAwait(false);

    public async Task<IEnumerable<Role>> GetRealmRolesScopeMappingsForClientAsync(string realm, string clientId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/scope-mappings/realm")
            .GetJsonAsync<IEnumerable<Role>>()
            .ConfigureAwait(false);

    public async Task RemoveRealmRolesFromClientScopeForClientAsync(string realm, string clientId, IEnumerable<Role> roles) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/scope-mappings/realm")
            .SendJsonAsync(HttpMethod.Delete, roles)
            .ConfigureAwait(false);

    public async Task<IEnumerable<Role>> GetAvailableRealmRolesForClientScopeForClientAsync(string realm, string clientId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/scope-mappings/realm/available")
            .GetJsonAsync<IEnumerable<Role>>()
            .ConfigureAwait(false);

    public async Task<IEnumerable<Role>> GetEffectiveRealmRolesForClientScopeForClientAsync(string realm, string clientId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/scope-mappings/realm/composite")
            .GetJsonAsync<IEnumerable<Role>>()
            .ConfigureAwait(false);
}
