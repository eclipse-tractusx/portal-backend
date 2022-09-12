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

using CatenaX.NetworkServices.Keycloak.Library.Models.Clients;
using CatenaX.NetworkServices.Keycloak.Library.Models.ClientScopes;
using CatenaX.NetworkServices.Keycloak.Library.Models.Common;
using CatenaX.NetworkServices.Keycloak.Library.Models.Groups;
using CatenaX.NetworkServices.Keycloak.Library.Models.RealmsAdmin;
using Flurl.Http;

namespace CatenaX.NetworkServices.Keycloak.Library;

public partial class KeycloakClient
{
    public async Task<bool> ImportRealmAsync(string realm, Realm rep)
    {
        var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms")
            .PostJsonAsync(rep)
            .ConfigureAwait(false);
        return response.IsSuccessStatusCode;
    }

    public async Task<IEnumerable<Realm>> GetRealmsAsync(string realm) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
        .AppendPathSegment("/admin/realms")
        .GetJsonAsync<IEnumerable<Realm>>()
        .ConfigureAwait(false);

    public async Task<Realm> GetRealmAsync(string realm) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
        .AppendPathSegment("/admin/realms/")
        .AppendPathSegment(realm, true)
        .GetJsonAsync<Realm>()
        .ConfigureAwait(false);
    
    public async Task<bool> UpdateRealmAsync(string realm, Realm rep)
    {
        var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .PutJsonAsync(rep)
            .ConfigureAwait(false);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteRealmAsync(string realm)
    {
        var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .DeleteAsync()
            .ConfigureAwait(false);
        return response.IsSuccessStatusCode;
    }

    public async Task<IEnumerable<AdminEvent>> GetAdminEventsAsync(string realm, string authClient = null, string authIpAddress = null, string authRealm = null, string authUser = null,
        string dateFrom = null, string dateTo = null, int? first = null, int? max = null, 
        IEnumerable<string> operationTypes = null, string resourcePath = null, IEnumerable<string> resourceTypes = null)
    {
        var queryParams = new Dictionary<string, object>
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

    public async Task<bool> DeleteAdminEventsAsync(string realm)
    {
        var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/admin-events")
            .DeleteAsync()
            .ConfigureAwait(false);
        return response.IsSuccessStatusCode;
    }
    
    public async Task<bool> ClearKeysCacheAsync(string realm)
    {
        var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clear-keys-cache")
            .PostAsync(new StringContent(""))
            .ConfigureAwait(false);
        return response.IsSuccessStatusCode;
    }
    
    public async Task<bool> ClearRealmCacheAsync(string realm)
    {
        var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clear-realm-cache")
            .PostAsync(new StringContent(""))
            .ConfigureAwait(false);
        return response.IsSuccessStatusCode;
    }
    
    public async Task<bool> ClearUserCacheAsync(string realm)
    {
        var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clear-user-cache")
            .PostAsync(new StringContent(""))
            .ConfigureAwait(false);
        return response.IsSuccessStatusCode;
    }
    
    public async Task<Client> BasePathForImportingClientsAsync(string realm, string description) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
        .AppendPathSegment("/admin/realms/")
        .AppendPathSegment(realm, true)
        .AppendPathSegment("/client-description-converter")
        .PostAsync(new StringContent(description))
        .ReceiveJson<Client>()
        .ConfigureAwait(false);

    public async Task<IEnumerable<IDictionary<string, object>>> GetClientSessionStatsAsync(string realm) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
        .AppendPathSegment("/admin/realms/")
        .AppendPathSegment(realm, true)
        .AppendPathSegment("/client-session-stats")
        .GetJsonAsync<IEnumerable<IDictionary<string, object>>>()
        .ConfigureAwait(false);

    public async Task<IEnumerable<ClientScope>> GetRealmDefaultClientScopesAsync(string realm) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
        .AppendPathSegment("/admin/realms/")
        .AppendPathSegment(realm, true)
        .AppendPathSegment("/default-default-client-scopes")
        .GetJsonAsync<IEnumerable<ClientScope>>()
        .ConfigureAwait(false);
    
    public async Task<bool> UpdateRealmDefaultClientScopeAsync(string realm, string clientScopeId)
    {
        var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/default-default-client-scopes/")
            .AppendPathSegment(clientScopeId, true)
            .PutAsync(new StringContent(""))
            .ConfigureAwait(false);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteRealmDefaultClientScopeAsync(string realm, string clientScopeId)
    {
        var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/default-default-client-scopes/")
            .AppendPathSegment(clientScopeId, true)
            .DeleteAsync()
            .ConfigureAwait(false);
        return response.IsSuccessStatusCode;
    }

    public async Task<IEnumerable<Group>> GetRealmGroupHierarchyAsync(string realm) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
        .AppendPathSegment("/admin/realms/")
        .AppendPathSegment(realm, true)
        .AppendPathSegment("/default-groups")
        .GetJsonAsync<IEnumerable<Group>>()
        .ConfigureAwait(false);
    
    public async Task<bool> UpdateRealmGroupAsync(string realm, string groupId)
    {
        var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/default-groups/")
            .AppendPathSegment(groupId, true)
            .PutAsync(new StringContent(""))
            .ConfigureAwait(false);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteRealmGroupAsync(string realm, string groupId)
    {
        var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/default-groups/")
            .AppendPathSegment(groupId, true)
            .DeleteAsync()
            .ConfigureAwait(false);
        return response.IsSuccessStatusCode;
    }

    public async Task<IEnumerable<ClientScope>> GetRealmOptionalClientScopesAsync(string realm) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
        .AppendPathSegment("/admin/realms/")
        .AppendPathSegment(realm, true)
        .AppendPathSegment("/default-optional-client-scopes")
        .GetJsonAsync<IEnumerable<ClientScope>>()
        .ConfigureAwait(false);
    
    public async Task<bool> UpdateRealmOptionalClientScopeAsync(string realm, string clientScopeId)
    {
        var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/default-optional-client-scopes/")
            .AppendPathSegment(clientScopeId, true)
            .PutAsync(new StringContent(""))
            .ConfigureAwait(false);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteRealmOptionalClientScopeAsync(string realm, string clientScopeId)
    {
        var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/default-optional-client-scopes/")
            .AppendPathSegment(clientScopeId, true)
            .DeleteAsync()
            .ConfigureAwait(false);
        return response.IsSuccessStatusCode;
    }

    public async Task<IEnumerable<Event>> GetEventsAsync(string realm, string client = null, string dateFrom = null, string dateTo = null, int? first = null, 
        string ipAddress = null, int? max = null, string type = null, string user = null)
    {
        var queryParams = new Dictionary<string, object>
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

    public async Task<bool> DeleteEventsAsync(string realm)
    {
        var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/events")
            .DeleteAsync()
            .ConfigureAwait(false);
        return response.IsSuccessStatusCode;
    }

    public async Task<RealmEventsConfig> GetRealmEventsProviderConfigurationAsync(string realm) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
        .AppendPathSegment("/admin/realms/")
        .AppendPathSegment(realm, true)
        .AppendPathSegment("/events/config")
        .GetJsonAsync<RealmEventsConfig>()
        .ConfigureAwait(false);

    public async Task<bool> UpdateRealmEventsProviderConfigurationAsync(string realm, RealmEventsConfig rep)
    {
        var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/events/config")
            .PutJsonAsync(rep)
            .ConfigureAwait(false);
        return response.IsSuccessStatusCode;
    }

    public async Task<Group> GetRealmGroupByPathAsync(string realm, string path) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
        .AppendPathSegment("/admin/realms/")
        .AppendPathSegment(realm, true)
        .AppendPathSegment("/group-by-path/")
        .AppendPathSegment(path, true)
        .GetJsonAsync<Group>()
        .ConfigureAwait(false);

    public async Task<GlobalRequestResult> RemoveUserSessionsAsync(string realm) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
        .AppendPathSegment("/admin/realms/")
        .AppendPathSegment(realm, true)
        .AppendPathSegment("/logout-all")
        .PostAsync(new StringContent(""))
        .ReceiveJson<GlobalRequestResult>()
        .ConfigureAwait(false);

    public async Task<Realm> RealmPartialExportAsync(string realm, bool? exportClients = null, bool? exportGroupsAndRoles = null)
    {
        var queryParams = new Dictionary<string, object>
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

    public async Task<bool> RealmPartialImportAsync(string realm, PartialImport rep)
    {
        var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/partialImport")
            .PostJsonAsync(rep)
            .ConfigureAwait(false);
        return response.IsSuccessStatusCode;
    }

    public async Task<GlobalRequestResult> PushRealmRevocationPolicyAsync(string realm) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
        .AppendPathSegment("/admin/realms/")
        .AppendPathSegment(realm, true)
        .AppendPathSegment("/push-revocation")
        .PostAsync(new StringContent(""))
        .ReceiveJson<GlobalRequestResult>()
        .ConfigureAwait(false);

    public async Task<bool> DeleteUserSessionAsync(string realm, string session)
    {
        var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/sessions/")
            .AppendPathSegment(session, true)
            .DeleteAsync()
            .ConfigureAwait(false);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> TestLdapConnectionAsync(string realm, string action = null, string bindCredential = null, string bindDn = null, 
        string componentId = null, string connectionTimeout = null, string connectionUrl = null, string useTruststoreSpi = null)
    {
        var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
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
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> TestSmtpConnectionAsync(string realm, string config)
    {
        var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/testSMTPConnection/")
            .AppendPathSegment(config, true)
            .PostAsync(new StringContent(""))
            .ConfigureAwait(false);
        return response.IsSuccessStatusCode;
    }

    public async Task<ManagementPermission> GetRealmUsersManagementPermissionsAsync(string realm) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
        .AppendPathSegment("/admin/realms/")
        .AppendPathSegment(realm, true)
        .AppendPathSegment("/users-management-permissions")
        .GetJsonAsync<ManagementPermission>()
        .ConfigureAwait(false);

    public async Task<ManagementPermission> UpdateRealmUsersManagementPermissionsAsync(string realm, ManagementPermission managementPermission)
    {
        return await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/users-management-permissions")
            .PutJsonAsync(managementPermission)
            .ReceiveJson<ManagementPermission>()
            .ConfigureAwait(false);
    }
}
