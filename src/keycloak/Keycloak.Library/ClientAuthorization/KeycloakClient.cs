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

using AuthorizationResource = Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.AuthorizationResources.AuthorizationResource;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.AuthorizationPermissions;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.AuthorizationScopes;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Clients;
using Flurl.Http;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library;

public partial class KeycloakClient
{
    #region Permissions
    public async Task<AuthorizationPermission> CreateAuthorizationPermissionAsync(string realm, string clientId, AuthorizationPermission permission) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/authz/resource-server/permission")
            .AppendPathSegment(permission.Type == AuthorizationPermissionType.Scope ? "/scope" : "/resource")
            .PostJsonAsync(permission)
            .ReceiveJson<AuthorizationPermission>()
            .ConfigureAwait(false);

    public async Task<AuthorizationPermission> GetAuthorizationPermissionByIdAsync(string realm, string clientId,
        AuthorizationPermissionType permissionType, string permissionId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/authz/resource-server/permission")
            .AppendPathSegment(permissionType == AuthorizationPermissionType.Scope ? "/scope/" : "/resource/")
            .AppendPathSegment(permissionId, true)
            .GetJsonAsync<AuthorizationPermission>()
            .ConfigureAwait(false);

    public async Task<IEnumerable<AuthorizationPermission>> GetAuthorizationPermissionsAsync(string realm, string clientId, AuthorizationPermissionType? ofPermissionType = null, 
        int? first = null, int? max = null, string? name = null, string? resource = null, string? scope = null)
    {
        var queryParams = new Dictionary<string, object?>
        {
            [nameof(first)] = first,
            [nameof(max)] = max,
            [nameof(name)] = name,
            [nameof(resource)] = resource,
            [nameof(scope)] = scope
        };

        var request = (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/authz/resource-server/permission");

        if (ofPermissionType.HasValue)
            request.AppendPathSegment(ofPermissionType.Value == AuthorizationPermissionType.Scope ? "/scope" : "/resource");
        
        return await request
            .SetQueryParams(queryParams)
            .GetJsonAsync<IEnumerable<AuthorizationPermission>>()
            .ConfigureAwait(false);
    }

    public async Task UpdateAuthorizationPermissionAsync(string realm, string clientId, AuthorizationPermission permission) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/authz/resource-server/permission")
            .AppendPathSegment(permission.Type == AuthorizationPermissionType.Scope ? "/scope/" : "/resource/")
            .AppendPathSegment(permission.Id, true)
            .PutJsonAsync(permission)
            .ConfigureAwait(false);

    public async Task DeleteAuthorizationPermissionAsync(string realm, string clientId, AuthorizationPermissionType permissionType,
        string permissionId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/authz/resource-server/permission")
            .AppendPathSegment(permissionType == AuthorizationPermissionType.Scope ? "/scope" : "/resource")
            .AppendPathSegment(permissionId, true)
            .DeleteAsync()
            .ConfigureAwait(false);
    
    public async Task<IEnumerable<Policy>> GetAuthorizationPermissionAssociatedPoliciesAsync(string realm, string clientId, string permissionId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/authz/resource-server/policy/")
            .AppendPathSegment(permissionId, true)
            .AppendPathSegment("/associatedPolicies")
            .GetJsonAsync<IEnumerable<Policy>>()
            .ConfigureAwait(false);

    public async Task<IEnumerable<AuthorizationScope>> GetAuthorizationPermissionAssociatedScopesAsync(string realm, string clientId, string permissionId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/authz/resource-server/policy/")
            .AppendPathSegment(permissionId, true)
            .AppendPathSegment("/scopes")
            .GetJsonAsync<IEnumerable<AuthorizationScope>>()
            .ConfigureAwait(false);

    public async Task<IEnumerable<AuthorizationResource>> GetAuthorizationPermissionAssociatedResourcesAsync(string realm, string clientId, string permissionId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/authz/resource-server/policy/")
            .AppendPathSegment(permissionId, true)
            .AppendPathSegment("/resources")
            .GetJsonAsync<IEnumerable<AuthorizationResource>>()
            .ConfigureAwait(false);

    #endregion 

    #region Policy
    public async Task<RolePolicy> CreateRolePolicyAsync(string realm, string clientId, RolePolicy policy) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
                .AppendPathSegment("/admin/realms/")
                .AppendPathSegment(realm, true)
                .AppendPathSegment("/clients/")
                .AppendPathSegment(clientId, true)
                .AppendPathSegment("/authz/resource-server/policy")
                .AppendPathSegment(policy.Type == PolicyType.Role ? "/role" : string.Empty)
                .PostJsonAsync(policy)
                .ReceiveJson<RolePolicy>()
                .ConfigureAwait(false);

    public async Task<RolePolicy> GetRolePolicyByIdAsync(string realm, string clientId, PolicyType policyType, string rolePolicyId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/authz/resource-server/policy")
            .AppendPathSegment(policyType == PolicyType.Role ? "/role" : string.Empty)
            .AppendPathSegment(rolePolicyId, true)
            .GetJsonAsync<RolePolicy>()
            .ConfigureAwait(false);

    public async Task<IEnumerable<Policy>> GetAuthorizationPoliciesAsync(string realm, string clientId, 
        int? first = null, int? max = null, 
        string? name = null, string? resource = null,
        string? scope = null, bool? permission = null)
    {
        var queryParams = new Dictionary<string, object?>
        {
            [nameof(first)] = first,
            [nameof(max)] = max,
            [nameof(name)] = name,
            [nameof(resource)] = resource,
            [nameof(scope)] = scope,
            [nameof(permission)] = permission
        };

        return await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/authz/resource-server/policy")
            .SetQueryParams(queryParams)
            .GetJsonAsync<IEnumerable<Policy>>()
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<RolePolicy>> GetRolePoliciesAsync(string realm, string clientId, 
        int? first = null, int? max = null, 
        string? name = null, string? resource = null,
        string? scope = null, bool? permission = null)
    {
        var queryParams = new Dictionary<string, object?>
        {
            [nameof(first)] = first,
            [nameof(max)] = max,
            [nameof(name)] = name,
            [nameof(resource)] = resource,
            [nameof(scope)] = scope,
            [nameof(permission)] = permission
        };

        return await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/authz/resource-server/policy/role")
            .SetQueryParams(queryParams)
            .GetJsonAsync<IEnumerable<RolePolicy>>()
            .ConfigureAwait(false);
    }

    public async Task UpdateRolePolicyAsync(string realm, string clientId, RolePolicy policy) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/authz/resource-server/policy")
            .AppendPathSegment(policy.Type == PolicyType.Role ? "/role" : string.Empty)
            .AppendPathSegment(policy.Id, true)
            .PutJsonAsync(policy)
            .ConfigureAwait(false);

    public async Task DeleteRolePolicyAsync(string realm, string clientId, PolicyType policyType, string rolePolicyId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/authz/resource-server/policy")
            .AppendPathSegment(policyType == PolicyType.Role ? "/role" : string.Empty)
            .AppendPathSegment(rolePolicyId, true)
            .DeleteAsync()
            .ConfigureAwait(false);

    #endregion
}
