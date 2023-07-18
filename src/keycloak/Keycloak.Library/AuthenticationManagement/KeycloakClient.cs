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
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.AuthenticationManagement;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library;

public partial class KeycloakClient
{
    public async Task<IEnumerable<IDictionary<string, object>>> GetAuthenticatorProvidersAsync(string realm) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
        .AppendPathSegment("/admin/realms/")
        .AppendPathSegment(realm, true)
        .AppendPathSegment("/authentication/authenticator-providers")
        .GetJsonAsync<IEnumerable<IDictionary<string, object>>>()
        .ConfigureAwait(false);

    public async Task<IEnumerable<IDictionary<string, object>>> GetClientAuthenticatorProvidersAsync(string realm) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
        .AppendPathSegment("/admin/realms/")
        .AppendPathSegment(realm, true)
        .AppendPathSegment("/authentication/client-authenticator-providers")
        .GetJsonAsync<IEnumerable<IDictionary<string, object>>>()
        .ConfigureAwait(false);

    public async Task<AuthenticatorConfigInfo> GetAuthenticatorProviderConfigurationDescriptionAsync(string realm, string providerId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
        .AppendPathSegment("/admin/realms/")
        .AppendPathSegment(realm, true)
        .AppendPathSegment("/authentication/config-description/")
        .AppendPathSegment(providerId, true)
        .GetJsonAsync<AuthenticatorConfigInfo>()
        .ConfigureAwait(false);

    [Obsolete("Not working yet")]
    public async Task<AuthenticatorConfig> GetAuthenticatorConfigurationAsync(string realm, string configurationId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
        .AppendPathSegment("/admin/realms/")
        .AppendPathSegment(realm, true)
        .AppendPathSegment("/authentication/config/")
        .AppendPathSegment(configurationId, true)
        .GetJsonAsync<AuthenticatorConfig>()
        .ConfigureAwait(false);

    public async Task UpdateAuthenticatorConfigurationAsync(string realm, string configurationId, AuthenticatorConfig authenticatorConfig) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/config/")
            .AppendPathSegment(configurationId, true)
            .PutJsonAsync(authenticatorConfig)
            .ConfigureAwait(false);

    public async Task DeleteAuthenticatorConfigurationAsync(string realm, string configurationId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/config/")
            .AppendPathSegment(configurationId, true)
            .DeleteAsync()
            .ConfigureAwait(false);

    public async Task AddAuthenticationExecutionAsync(string realm, AuthenticationExecution authenticationExecution) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/executions")
            .PostJsonAsync(authenticationExecution)
            .ConfigureAwait(false);

    public async Task<AuthenticationExecutionById> GetAuthenticationExecutionAsync(string realm, string executionId) =>
         await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/executions/")
            .AppendPathSegment(executionId, true)
            .GetJsonAsync<AuthenticationExecutionById>()
            .ConfigureAwait(false);

    public async Task DeleteAuthenticationExecutionAsync(string realm, string executionId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/executions/")
            .AppendPathSegment(executionId, true)
            .DeleteAsync()
            .ConfigureAwait(false);

    public async Task UpdateAuthenticationExecutionConfigurationAsync(string realm, string executionId, AuthenticatorConfig authenticatorConfig) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/executions/")
            .AppendPathSegment(executionId, true)
            .AppendPathSegment("/config")
            .PostJsonAsync(authenticatorConfig)
            .ConfigureAwait(false);

    public async Task LowerAuthenticationExecutionPriorityAsync(string realm, string executionId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/executions/")
            .AppendPathSegment(executionId, true)
            .AppendPathSegment("/lower-priority")
            .PostAsync(new StringContent(""))
            .ConfigureAwait(false);

    public async Task RaiseAuthenticationExecutionPriorityAsync(string realm, string executionId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/executions/")
            .AppendPathSegment(executionId, true)
            .AppendPathSegment("/raise-priority")
            .PostAsync(new StringContent(""))
            .ConfigureAwait(false);

    public async Task CreateAuthenticationFlowAsync(string realm, AuthenticationFlow authenticationFlow) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/flows")
            .PostJsonAsync(authenticationFlow)
            .ConfigureAwait(false);

    public async Task<IEnumerable<AuthenticationFlow>> GetAuthenticationFlowsAsync(string realm) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/flows")
            .GetJsonAsync<IEnumerable<AuthenticationFlow>>()
            .ConfigureAwait(false);

    public async Task DuplicateAuthenticationFlowAsync(string realm, string flowAlias, string newName) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/flows/")
            .AppendPathSegment(flowAlias, true)
            .AppendPathSegment("/copy")
            .PostJsonAsync(new Dictionary<string, object> { [nameof(newName)] = newName })
            .ConfigureAwait(false);

    public async Task<IEnumerable<AuthenticationFlowExecution>> GetAuthenticationFlowExecutionsAsync(string realm, string flowAlias) =>
         await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/flows/")
            .AppendPathSegment(flowAlias, true)
            .AppendPathSegment("/executions")
            .GetJsonAsync<IEnumerable<AuthenticationFlowExecution>>()
            .ConfigureAwait(false);

    public async Task UpdateAuthenticationFlowExecutionsAsync(string realm, string flowAlias, AuthenticationExecutionInfo authenticationExecutionInfo) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/flows/")
            .AppendPathSegment(flowAlias, true)
            .AppendPathSegment("/executions")
            .PutJsonAsync(authenticationExecutionInfo)
            .ConfigureAwait(false);

    public Task AddAuthenticationFlowExecutionAsync(string realm, string flowAlias, IDictionary<string, object> dataWithProvider) =>
        InternalAddAuthenticationFlowExecutionAsync(realm, flowAlias, dataWithProvider);

    public async Task<string?> AddAndRetrieveAuthenticationFlowExecutionIdAsync(string realm, string flowAlias, IDictionary<string, object> dataWithProvider)
    {
        var response = await InternalAddAuthenticationFlowExecutionAsync(realm, flowAlias, dataWithProvider).ConfigureAwait(false);
        var locationPathAndQuery = response.ResponseMessage.Headers.Location?.PathAndQuery;
        return locationPathAndQuery != null ? locationPathAndQuery.Substring(locationPathAndQuery.LastIndexOf("/", StringComparison.Ordinal) + 1) : null;
    }

    private async Task<IFlurlResponse> InternalAddAuthenticationFlowExecutionAsync(string realm, string flowAlias, IDictionary<string, object> dataWithProvider) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/flows/")
            .AppendPathSegment(flowAlias, true)
            .AppendPathSegment("/executions/execution")
            .PostJsonAsync(dataWithProvider)
            .ConfigureAwait(false);

    public Task AddAuthenticationFlowAndExecutionToAuthenticationFlowAsync(string realm, string flowAlias, IDictionary<string, object> dataWithAliasTypeProviderDescription) =>
        InternalAddAuthenticationFlowAndExecutionToAuthenticationFlowAsync(realm, flowAlias, dataWithAliasTypeProviderDescription);

    public async Task<string?> AddAndRetrieveAuthenticationFlowAndExecutionToAuthenticationFlowIdAsync(string realm, string flowAlias, IDictionary<string, object> dataWithAliasTypeProviderDescription)
    {
        var response = await InternalAddAuthenticationFlowAndExecutionToAuthenticationFlowAsync(realm, flowAlias, dataWithAliasTypeProviderDescription).ConfigureAwait(false);
        var locationPathAndQuery = response.ResponseMessage.Headers.Location?.PathAndQuery;
        return locationPathAndQuery != null ? locationPathAndQuery.Substring(locationPathAndQuery.LastIndexOf("/", StringComparison.Ordinal) + 1) : null;
    }

    private async Task<IFlurlResponse> InternalAddAuthenticationFlowAndExecutionToAuthenticationFlowAsync(string realm, string flowAlias, IDictionary<string, object> dataWithAliasTypeProviderDescription) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/flows/")
            .AppendPathSegment(flowAlias, true)
            .AppendPathSegment("/executions/flow")
            .PostJsonAsync(dataWithAliasTypeProviderDescription)
            .ConfigureAwait(false);

    public async Task<AuthenticationFlow> GetAuthenticationFlowByIdAsync(string realm, string flowId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/flows/")
            .AppendPathSegment(flowId, true)
            .GetJsonAsync<AuthenticationFlow>()
            .ConfigureAwait(false);

    public async Task UpdateAuthenticationFlowAsync(string realm, string flowId, AuthenticationFlow authenticationFlow) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/flows/")
            .AppendPathSegment(flowId, true)
            .PutJsonAsync(authenticationFlow)
            .ConfigureAwait(false);

    public async Task DeleteAuthenticationFlowAsync(string realm, string flowId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/flows/")
            .AppendPathSegment(flowId, true)
            .DeleteAsync()
            .ConfigureAwait(false);

    public async Task<IEnumerable<IDictionary<string, object>>> GetFormActionProvidersAsync(string realm) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/form-action-providers")
            .GetJsonAsync<IEnumerable<IDictionary<string, object>>>()
            .ConfigureAwait(false);

    public async Task<IEnumerable<IDictionary<string, object>>> GetFormProvidersAsync(string realm) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/form-providers")
            .GetJsonAsync<IEnumerable<IDictionary<string, object>>>()
            .ConfigureAwait(false);

    public async Task<IDictionary<string, object>> GetConfigurationDescriptionsForAllClientsAsync(string realm) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/per-client-config-description")
            .GetJsonAsync<IDictionary<string, object>>()
            .ConfigureAwait(false);

    public async Task RegisterRequiredActionAsync(string realm, IDictionary<string, object> dataWithProviderIdName) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/register-required-action")
            .PostJsonAsync(dataWithProviderIdName)
            .ConfigureAwait(false);

    public async Task<IEnumerable<RequiredActionProvider>> GetRequiredActionsAsync(string realm) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/required-actions")
            .GetJsonAsync<IEnumerable<RequiredActionProvider>>()
            .ConfigureAwait(false);

    public async Task<RequiredActionProvider> GetRequiredActionByAliasAsync(string realm, string requiredActionAlias) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/required-actions/")
            .AppendPathSegment(requiredActionAlias, true)
            .GetJsonAsync<RequiredActionProvider>()
            .ConfigureAwait(false);

    public async Task UpdateRequiredActionAsync(string realm, string requiredActionAlias, RequiredActionProvider requiredActionProvider) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/required-actions/")
            .AppendPathSegment(requiredActionAlias, true)
            .PutJsonAsync(requiredActionProvider)
            .ConfigureAwait(false);

    public async Task DeleteRequiredActionAsync(string realm, string requiredActionAlias) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/required-actions/")
            .AppendPathSegment(requiredActionAlias, true)
            .DeleteAsync()
            .ConfigureAwait(false);

    public async Task LowerRequiredActionPriorityAsync(string realm, string requiredActionAlias) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/required-actions/")
            .AppendPathSegment(requiredActionAlias, true)
            .AppendPathSegment("/lower-priority")
            .PostAsync(new StringContent(""))
            .ConfigureAwait(false);

    public async Task RaiseRequiredActionPriorityAsync(string realm, string requiredActionAlias) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/required-actions/")
            .AppendPathSegment(requiredActionAlias, true)
            .AppendPathSegment("/raise-priority")
            .PostAsync(new StringContent(""))
            .ConfigureAwait(false);

    public async Task<IEnumerable<IDictionary<string, object>>> GetUnregisteredRequiredActionsAsync(string realm) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/unregistered-required-actions")
            .GetJsonAsync<IEnumerable<IDictionary<string, object>>>()
            .ConfigureAwait(false);
}
