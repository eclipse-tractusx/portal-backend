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

using AuthorizationResource = CatenaX.NetworkServices.Keycloak.Library.Models.AuthorizationResources.AuthorizationResource;
using CatenaX.NetworkServices.Keycloak.Library.Models.AuthorizationPermissions;
using CatenaX.NetworkServices.Keycloak.Library.Models.AuthorizationScopes;
using CatenaX.NetworkServices.Keycloak.Library.Models.Clients;
using Flurl.Http;

namespace CatenaX.NetworkServices.Keycloak.Library;

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
