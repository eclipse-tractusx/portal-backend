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
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Common.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Clients;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.ClientScopes;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Common;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Roles;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Users;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library;

public partial class KeycloakClient
{
    public async Task CreateClientAsync(string realm, Client client) =>
        await InternalCreateClientAsync(realm, client).ConfigureAwait(false);

    public async Task<string?> CreateClientAndRetrieveClientIdAsync(string realm, Client client)
    {
        var response = await InternalCreateClientAsync(realm, client).ConfigureAwait(false);

        var locationPathAndQuery = response.ResponseMessage.Headers.Location?.PathAndQuery;
        return locationPathAndQuery?.Substring(locationPathAndQuery.LastIndexOf("/", StringComparison.Ordinal) + 1);
    }

    private async Task<IFlurlResponse> InternalCreateClientAsync(string realm, Client client) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients")
            .PostJsonAsync(client)
            .ConfigureAwait(false);

    public async Task<IEnumerable<Client>> GetClientsAsync(string realm, string? clientId = null, bool? viewableOnly = null)
    {
        var queryParams = new Dictionary<string, object?>
        {
            [nameof(clientId)] = clientId,
            [nameof(viewableOnly)] = viewableOnly
        };

        return await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients")
            .SetQueryParams(queryParams)
            .GetJsonAsync<IEnumerable<Client>>()
            .ConfigureAwait(false);
    }

    public async Task<Client> GetClientAsync(string realm, string clientId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .GetJsonAsync<Client>()
            .ConfigureAwait(false);

    public async Task UpdateClientAsync(string realm, string clientId, Client client) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .PutJsonAsync(client)
            .ConfigureAwait(false);

    public async Task DeleteClientAsync(string realm, string clientId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .DeleteAsync()
            .ConfigureAwait(false);

    public async Task<Credentials> GenerateClientSecretAsync(string realm, string clientId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/client-secret")
            .PostJsonAsync(new StringContent(""))
            .ReceiveJson<Credentials>()
            .ConfigureAwait(false);

    public async Task<Credentials> GetClientSecretAsync(string realm, string clientId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/client-secret")
            .GetJsonAsync<Credentials>()
            .ConfigureAwait(false);

    public async Task<IEnumerable<ClientScope>> GetDefaultClientScopesAsync(string realm, string clientId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/default-client-scopes")
            .GetJsonAsync<IEnumerable<ClientScope>>()
            .ConfigureAwait(false);

    public async Task UpdateDefaultClientScopeAsync(string realm, string clientId, string clientScopeId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/default-client-scopes/")
            .AppendPathSegment(clientScopeId, true)
            .PutAsync(new StringContent(""))
            .ConfigureAwait(false);

    public async Task DeleteDefaultClientScopeAsync(string realm, string clientId, string clientScopeId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/default-client-scopes/")
            .AppendPathSegment(clientScopeId, true)
            .DeleteAsync()
            .ConfigureAwait(false);

    [Obsolete("Not working yet")]
    public async Task<AccessToken> GenerateClientExampleAccessTokenAsync(string realm, string clientId, string? scope = null, string? userId = null)
    {
        var queryParams = new Dictionary<string, object?>
        {
            [nameof(scope)] = scope,
            [nameof(userId)] = userId
        };

        return await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/evaluate-scopes/generate-example-access-token")
            .SetQueryParams(queryParams)
            .GetJsonAsync<AccessToken>()
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<ClientScopeEvaluateResourceProtocolMapperEvaluation>> GetProtocolMappersInTokenGenerationAsync(string realm, string clientId, string? scope = null)
    {
        var queryParams = new Dictionary<string, object?>
        {
            [nameof(scope)] = scope
        };

        return await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/evaluate-scopes/protocol-mappers")
            .SetQueryParams(queryParams)
            .GetJsonAsync<IEnumerable<ClientScopeEvaluateResourceProtocolMapperEvaluation>>()
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<Role>> GetClientGrantedScopeMappingsAsync(string realm, string clientId, string roleContainerId, string? scope = null)
    {
        var queryParams = new Dictionary<string, object?>
        {
            [nameof(scope)] = scope
        };

        return await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/evaluate-scopes/scope-mappings/")
            .AppendPathSegment(roleContainerId, true)
            .AppendPathSegment("/granted")
            .SetQueryParams(queryParams)
            .GetJsonAsync<IEnumerable<Role>>()
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<Role>> GetClientNotGrantedScopeMappingsAsync(string realm, string clientId, string roleContainerId, string? scope = null)
    {
        var queryParams = new Dictionary<string, object?>
        {
            [nameof(scope)] = scope
        };

        return await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/evaluate-scopes/scope-mappings/")
            .AppendPathSegment(roleContainerId, true)
            .AppendPathSegment("/not-granted")
            .SetQueryParams(queryParams)
            .GetJsonAsync<IEnumerable<Role>>()
            .ConfigureAwait(false);
    }

    [Obsolete("Not working yet")]
    public async Task<string> GetClientProviderAsync(string realm, string clientId, string providerId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/installation/providers/")
            .AppendPathSegment(providerId, true)
            .GetStringAsync()
            .ConfigureAwait(false);

    public async Task<ManagementPermission> GetClientAuthorizationPermissionsInitializedAsync(string realm, string clientId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/management/permissions")
            .GetJsonAsync<ManagementPermission>()
            .ConfigureAwait(false);

    public async Task<ManagementPermission> SetClientAuthorizationPermissionsInitializedAsync(string realm, string clientId, ManagementPermission managementPermission) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/management/permissions")
            .PutJsonAsync(managementPermission)
            .ReceiveJson<ManagementPermission>()
            .ConfigureAwait(false);

    public async Task RegisterClientClusterNodeAsync(string realm, string clientId, IDictionary<string, object> formParams) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/nodes")
            .PostJsonAsync(formParams)
            .ConfigureAwait(false);

    public async Task UnregisterClientClusterNodeAsync(string realm, string clientId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/nodes")
            .DeleteAsync()
            .ConfigureAwait(false);

    public async Task<int> GetClientOfflineSessionCountAsync(string realm, string clientId)
    {
        var result = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/offline-session-count")
            .GetJsonAsync()
            .ConfigureAwait(false);

        return Convert.ToInt32(DynamicExtensions.GetFirstPropertyValue(result));
    }

    public async Task<IEnumerable<UserSession>> GetClientOfflineSessionsAsync(string realm, string clientId, int? first = null, int? max = null)
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
            .AppendPathSegment("/offline-sessions")
            .SetQueryParams(queryParams)
            .GetJsonAsync<IEnumerable<UserSession>>()
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<ClientScope>> GetOptionalClientScopesAsync(string realm, string clientId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/optional-client-scopes")
            .GetJsonAsync<IEnumerable<ClientScope>>()
            .ConfigureAwait(false);

    public async Task UpdateOptionalClientScopeAsync(string realm, string clientId, string clientScopeId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/optional-client-scopes/")
            .AppendPathSegment(clientScopeId, true)
            .PutAsync(new StringContent(""))
            .ConfigureAwait(false);

    public async Task DeleteOptionalClientScopeAsync(string realm, string clientId, string clientScopeId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/optional-client-scopes/")
            .AppendPathSegment(clientScopeId, true)
            .DeleteAsync()
            .ConfigureAwait(false);

    public async Task<GlobalRequestResult> PushClientRevocationPolicyAsync(string realm, string clientId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/push-revocation")
            .PostAsync(new StringContent(""))
            .ReceiveJson<GlobalRequestResult>()
            .ConfigureAwait(false);

    public async Task<Client> GenerateClientRegistrationAccessTokenAsync(string realm, string clientId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/registration-access-token")
            .PostJsonAsync(new StringContent(""))
            .ReceiveJson<Client>()
            .ConfigureAwait(false);

    // [Obsolete("Not working yet")] - seems to work fine?
    public async Task<User> GetUserForServiceAccountAsync(string realm, string clientId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/service-account-user")
            .GetJsonAsync<User>()
            .ConfigureAwait(false);

    public async Task<int> GetClientSessionCountAsync(string realm, string clientId)
    {
        var result = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/session-count")
            .GetJsonAsync()
            .ConfigureAwait(false);

        return Convert.ToInt32(DynamicExtensions.GetFirstPropertyValue(result));
    }

    public async Task<GlobalRequestResult> TestClientClusterNodesAvailableAsync(string realm, string clientId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/test-nodes-available")
            .GetJsonAsync<GlobalRequestResult>()
            .ConfigureAwait(false);

    public async Task<IEnumerable<UserSession>> GetClientUserSessionsAsync(string realm, string clientId, int? first = null, int? max = null)
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
            .AppendPathSegment("/user-sessions")
            .SetQueryParams(queryParams)
            .GetJsonAsync<IEnumerable<UserSession>>()
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<Resource>> GetResourcesOwnedByClientAsync(string realm, string clientId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/protocol/openid-connect/token")
            .PostUrlEncodedAsync(new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "urn:ietf:params:oauth:grant-type:uma-ticket"),
                new KeyValuePair<string, string>("response_mode", "permissions"),
                new KeyValuePair<string, string>("audience", clientId)
            })
            .ReceiveJson<IEnumerable<Resource>>()
            .ConfigureAwait(false);
}
