/********************************************************************************
 * MIT License
 *
 * Copyright (c) 2019 Luk Vermeulen
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
    public async Task<IEnumerable<IDictionary<string, object>>> GetAuthenticatorProvidersAsync(string realm) => await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
        .AppendPathSegment("/admin/realms/")
        .AppendPathSegment(realm, true)
        .AppendPathSegment("/authentication/authenticator-providers")
        .GetJsonAsync<IEnumerable<IDictionary<string, object>>>()
        .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task<IEnumerable<IDictionary<string, object>>> GetClientAuthenticatorProvidersAsync(string realm) => await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
        .AppendPathSegment("/admin/realms/")
        .AppendPathSegment(realm, true)
        .AppendPathSegment("/authentication/client-authenticator-providers")
        .GetJsonAsync<IEnumerable<IDictionary<string, object>>>()
        .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task<AuthenticatorConfigInfo> GetAuthenticatorProviderConfigurationDescriptionAsync(string realm, string providerId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
        .AppendPathSegment("/admin/realms/")
        .AppendPathSegment(realm, true)
        .AppendPathSegment("/authentication/config-description/")
        .AppendPathSegment(providerId, true)
        .GetJsonAsync<AuthenticatorConfigInfo>()
        .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task<AuthenticatorConfig> GetAuthenticatorConfigurationAsync(string realm, string configurationId, CancellationToken cancellationToken = default) =>
        await (await GetBaseUrlAsync(realm, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/config/")
            .AppendPathSegment(configurationId, true)
            .GetJsonAsync<AuthenticatorConfig>(cancellationToken)
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task UpdateAuthenticatorConfigurationAsync(string realm, string configurationId, AuthenticatorConfig authenticatorConfig, CancellationToken cancellationToken = default) =>
        await (await GetBaseUrlAsync(realm, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/config/")
            .AppendPathSegment(configurationId, true)
            .PutJsonAsync(authenticatorConfig, cancellationToken)
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task DeleteAuthenticatorConfigurationAsync(string realm, string configurationId, CancellationToken cancellationToken = default) =>
        await (await GetBaseUrlAsync(realm, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/config/")
            .AppendPathSegment(configurationId, true)
            .DeleteAsync(cancellationToken)
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task AddAuthenticationExecutionAsync(string realm, AuthenticationExecution authenticationExecution) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/executions")
            .PostJsonAsync(authenticationExecution)
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task<AuthenticationExecutionById> GetAuthenticationExecutionAsync(string realm, string executionId) =>
         await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/executions/")
            .AppendPathSegment(executionId, true)
            .GetJsonAsync<AuthenticationExecutionById>()
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task DeleteAuthenticationExecutionAsync(string realm, string executionId, CancellationToken cancellationToken = default) =>
        await (await GetBaseUrlAsync(realm, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/executions/")
            .AppendPathSegment(executionId, true)
            .DeleteAsync(cancellationToken)
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task CreateAuthenticationExecutionConfigurationAsync(string realm, string executionId, AuthenticatorConfig authenticatorConfig, CancellationToken cancellationToken = default) =>
        await (await GetBaseUrlAsync(realm, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/executions/")
            .AppendPathSegment(executionId, true)
            .AppendPathSegment("/config")
            .PostJsonAsync(authenticatorConfig, cancellationToken)
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task LowerAuthenticationExecutionPriorityAsync(string realm, string executionId)
    {
        using var stringContent = new StringContent("");
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/executions/")
            .AppendPathSegment(executionId, true)
            .AppendPathSegment("/lower-priority")
            .PostAsync(stringContent)
            .ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public async Task RaiseAuthenticationExecutionPriorityAsync(string realm, string executionId)
    {
        using var stringContent = new StringContent("");
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/executions/")
            .AppendPathSegment(executionId, true)
            .AppendPathSegment("/raise-priority")
            .PostAsync(stringContent)
            .ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public async Task CreateAuthenticationFlowAsync(string realm, AuthenticationFlow authenticationFlow, CancellationToken cancellationToken = default) =>
        await (await GetBaseUrlAsync(realm, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/flows")
            .PostJsonAsync(authenticationFlow, cancellationToken)
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task<IEnumerable<AuthenticationFlow>> GetAuthenticationFlowsAsync(string realm, CancellationToken cancellationToken = default) =>
        await (await GetBaseUrlAsync(realm, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/flows")
            .GetJsonAsync<IEnumerable<AuthenticationFlow>>(cancellationToken)
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task DuplicateAuthenticationFlowAsync(string realm, string flowAlias, string newName) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/flows/")
            .AppendPathSegment(flowAlias, true)
            .AppendPathSegment("/copy")
            .PostJsonAsync(new Dictionary<string, object> { [nameof(newName)] = newName })
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task<IEnumerable<AuthenticationFlowExecution>> GetAuthenticationFlowExecutionsAsync(string realm, string flowAlias, CancellationToken cancellationToken = default) =>
         await (await GetBaseUrlAsync(realm, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/flows/")
            .AppendPathSegment(flowAlias, true)
            .AppendPathSegment("/executions")
            .GetJsonAsync<IEnumerable<AuthenticationFlowExecution>>(cancellationToken)
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task UpdateAuthenticationFlowExecutionsAsync(string realm, string flowAlias, AuthenticationExecutionInfo authenticationExecutionInfo, CancellationToken cancellationToken = default) =>
        await (await GetBaseUrlAsync(realm, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/flows/")
            .AppendPathSegment(flowAlias, true)
            .AppendPathSegment("/executions")
            .PutJsonAsync(authenticationExecutionInfo, cancellationToken)
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public Task AddAuthenticationFlowExecutionAsync(string realm, string flowAlias, IDictionary<string, object> dataWithProvider, CancellationToken cancellationToken = default) =>
        InternalAddAuthenticationFlowExecutionAsync(realm, flowAlias, dataWithProvider, cancellationToken);

    public async Task<string?> AddAndRetrieveAuthenticationFlowExecutionIdAsync(string realm, string flowAlias, IDictionary<string, object> dataWithProvider, CancellationToken cancellationToken = default)
    {
        var response = await InternalAddAuthenticationFlowExecutionAsync(realm, flowAlias, dataWithProvider, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        var locationPathAndQuery = response.ResponseMessage.Headers.Location?.PathAndQuery;
        return locationPathAndQuery != null ? locationPathAndQuery.Substring(locationPathAndQuery.LastIndexOf("/", StringComparison.Ordinal) + 1) : null;
    }

    private async Task<IFlurlResponse> InternalAddAuthenticationFlowExecutionAsync(string realm, string flowAlias, IDictionary<string, object> dataWithProvider, CancellationToken cancellationToken) =>
        await (await GetBaseUrlAsync(realm, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/flows/")
            .AppendPathSegment(flowAlias, true)
            .AppendPathSegment("/executions/execution")
            .PostJsonAsync(dataWithProvider, cancellationToken)
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public Task AddAuthenticationFlowAndExecutionToAuthenticationFlowAsync(string realm, string flowAlias, IDictionary<string, object> dataWithAliasTypeProviderDescription, CancellationToken cancellationToken = default) =>
        InternalAddAuthenticationFlowAndExecutionToAuthenticationFlowAsync(realm, flowAlias, dataWithAliasTypeProviderDescription, cancellationToken);

    public async Task<string?> AddAndRetrieveAuthenticationFlowAndExecutionToAuthenticationFlowIdAsync(string realm, string flowAlias, IDictionary<string, object> dataWithAliasTypeProviderDescription, CancellationToken cancellationToken)
    {
        var response = await InternalAddAuthenticationFlowAndExecutionToAuthenticationFlowAsync(realm, flowAlias, dataWithAliasTypeProviderDescription, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        var locationPathAndQuery = response.ResponseMessage.Headers.Location?.PathAndQuery;
        return locationPathAndQuery != null ? locationPathAndQuery.Substring(locationPathAndQuery.LastIndexOf("/", StringComparison.Ordinal) + 1) : null;
    }

    private async Task<IFlurlResponse> InternalAddAuthenticationFlowAndExecutionToAuthenticationFlowAsync(string realm, string flowAlias, IDictionary<string, object> dataWithAliasTypeProviderDescription, CancellationToken cancellationToken) =>
        await (await GetBaseUrlAsync(realm, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/flows/")
            .AppendPathSegment(flowAlias, true)
            .AppendPathSegment("/executions/flow")
            .PostJsonAsync(dataWithAliasTypeProviderDescription, cancellationToken)
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task<AuthenticationFlow> GetAuthenticationFlowByIdAsync(string realm, string flowId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/flows/")
            .AppendPathSegment(flowId, true)
            .GetJsonAsync<AuthenticationFlow>()
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task UpdateAuthenticationFlowAsync(string realm, string flowId, AuthenticationFlow authenticationFlow, CancellationToken cancellationToken = default) =>
        await (await GetBaseUrlAsync(realm, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/flows/")
            .AppendPathSegment(flowId, true)
            .PutJsonAsync(authenticationFlow, cancellationToken)
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task DeleteAuthenticationFlowAsync(string realm, string flowId, CancellationToken cancellationToken = default) =>
        await (await GetBaseUrlAsync(realm, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/flows/")
            .AppendPathSegment(flowId, true)
            .DeleteAsync(cancellationToken)
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task<IEnumerable<IDictionary<string, object>>> GetFormActionProvidersAsync(string realm) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/form-action-providers")
            .GetJsonAsync<IEnumerable<IDictionary<string, object>>>()
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task<IEnumerable<IDictionary<string, object>>> GetFormProvidersAsync(string realm) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/form-providers")
            .GetJsonAsync<IEnumerable<IDictionary<string, object>>>()
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task<IDictionary<string, object>> GetConfigurationDescriptionsForAllClientsAsync(string realm) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/per-client-config-description")
            .GetJsonAsync<IDictionary<string, object>>()
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task RegisterRequiredActionAsync(string realm, IDictionary<string, object> dataWithProviderIdName) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/register-required-action")
            .PostJsonAsync(dataWithProviderIdName)
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task<IEnumerable<RequiredActionProvider>> GetRequiredActionsAsync(string realm) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/required-actions")
            .GetJsonAsync<IEnumerable<RequiredActionProvider>>()
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task<RequiredActionProvider> GetRequiredActionByAliasAsync(string realm, string requiredActionAlias) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/required-actions/")
            .AppendPathSegment(requiredActionAlias, true)
            .GetJsonAsync<RequiredActionProvider>()
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task UpdateRequiredActionAsync(string realm, string requiredActionAlias, RequiredActionProvider requiredActionProvider) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/required-actions/")
            .AppendPathSegment(requiredActionAlias, true)
            .PutJsonAsync(requiredActionProvider)
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task DeleteRequiredActionAsync(string realm, string requiredActionAlias) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/required-actions/")
            .AppendPathSegment(requiredActionAlias, true)
            .DeleteAsync()
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task LowerRequiredActionPriorityAsync(string realm, string requiredActionAlias)
    {
        using var stringContent = new StringContent("");
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/required-actions/")
            .AppendPathSegment(requiredActionAlias, true)
            .AppendPathSegment("/lower-priority")
            .PostAsync(stringContent)
            .ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public async Task RaiseRequiredActionPriorityAsync(string realm, string requiredActionAlias)
    {
        using var stringContent = new StringContent("");
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/required-actions/")
            .AppendPathSegment(requiredActionAlias, true)
            .AppendPathSegment("/raise-priority")
            .PostAsync(stringContent)
            .ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public async Task<IEnumerable<IDictionary<string, object>>> GetUnregisteredRequiredActionsAsync(string realm) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/unregistered-required-actions")
            .GetJsonAsync<IEnumerable<IDictionary<string, object>>>()
            .ConfigureAwait(ConfigureAwaitOptions.None);
}
