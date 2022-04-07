using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl.Http;
using Keycloak.Net.Models.AuthenticationManagement;

namespace Keycloak.Net
{
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

        public async Task<bool> UpdateAuthenticatorConfigurationAsync(string realm, string configurationId, AuthenticatorConfig authenticatorConfig)
        {
            var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
                .AppendPathSegment("/admin/realms/")
                .AppendPathSegment(realm, true)
                .AppendPathSegment("/authentication/config/")
                .AppendPathSegment(configurationId, true)
                .PutJsonAsync(authenticatorConfig)
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAuthenticatorConfigurationAsync(string realm, string configurationId)
        {
            var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
                .AppendPathSegment("/admin/realms/")
                .AppendPathSegment(realm, true)
                .AppendPathSegment("/authentication/config/")
                .AppendPathSegment(configurationId, true)
                .DeleteAsync()
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> AddAuthenticationExecutionAsync(string realm, AuthenticationExecution authenticationExecution)
        {
            var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
                .AppendPathSegment("/admin/realms/")
                .AppendPathSegment(realm, true)
                .AppendPathSegment("/authentication/executions")
                .PostJsonAsync(authenticationExecution)
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        public async Task<AuthenticationExecutionById> GetAuthenticationExecutionAsync(string realm, string executionId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/executions/")
            .AppendPathSegment(executionId, true)
            .GetJsonAsync<AuthenticationExecutionById>()
            .ConfigureAwait(false);

        public async Task<bool> DeleteAuthenticationExecutionAsync(string realm, string executionId)
        {
            var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
                .AppendPathSegment("/admin/realms/")
                .AppendPathSegment(realm, true)
                .AppendPathSegment("/authentication/executions/")
                .AppendPathSegment(executionId, true)
                .DeleteAsync()
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAuthenticationExecutionConfigurationAsync(string realm, string executionId, AuthenticatorConfig authenticatorConfig)
        {
            var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
                .AppendPathSegment("/admin/realms/")
                .AppendPathSegment(realm, true)
                .AppendPathSegment("/authentication/executions/")
                .AppendPathSegment(executionId, true)
                .AppendPathSegment("/config")
                .PostJsonAsync(authenticatorConfig)
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> LowerAuthenticationExecutionPriorityAsync(string realm, string executionId)
        {
            var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
                .AppendPathSegment("/admin/realms/")
                .AppendPathSegment(realm, true)
                .AppendPathSegment("/authentication/executions/")
                .AppendPathSegment(executionId, true)
                .AppendPathSegment("/lower-priority")
                .PostAsync(new StringContent(""))
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> RaiseAuthenticationExecutionPriorityAsync(string realm, string executionId)
        {
            var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
                .AppendPathSegment("/admin/realms/")
                .AppendPathSegment(realm, true)
                .AppendPathSegment("/authentication/executions/")
                .AppendPathSegment(executionId, true)
                .AppendPathSegment("/raise-priority")
                .PostAsync(new StringContent(""))
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> CreateAuthenticationFlowAsync(string realm, AuthenticationFlow authenticationFlow)
        {
            var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
                .AppendPathSegment("/admin/realms/")
                .AppendPathSegment(realm, true)
                .AppendPathSegment("/authentication/flows")
                .PostJsonAsync(authenticationFlow)
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        public async Task<IEnumerable<AuthenticationFlow>> GetAuthenticationFlowsAsync(string realm) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/flows")
            .GetJsonAsync<IEnumerable<AuthenticationFlow>>()
            .ConfigureAwait(false);

        public async Task<bool> DuplicateAuthenticationFlowAsync(string realm, string flowAlias, string newName)
        {
            var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
                .AppendPathSegment("/admin/realms/")
                .AppendPathSegment(realm, true)
                .AppendPathSegment("/authentication/flows/")
                .AppendPathSegment(flowAlias, true)
                .AppendPathSegment("/copy")
                .PostJsonAsync(new Dictionary<string, object> { [nameof(newName)] = newName })
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        public async Task<IEnumerable<AuthenticationFlowExecution>> GetAuthenticationFlowExecutionsAsync(string realm, string flowAlias) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/flows/")
            .AppendPathSegment(flowAlias, true)
            .AppendPathSegment("/executions")
            .GetJsonAsync<IEnumerable<AuthenticationFlowExecution>>()
            .ConfigureAwait(false);

        public async Task<bool> UpdateAuthenticationFlowExecutionsAsync(string realm, string flowAlias, AuthenticationExecutionInfo authenticationExecutionInfo)
        {
            var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
                .AppendPathSegment("/admin/realms/")
                .AppendPathSegment(realm, true)
                .AppendPathSegment("/authentication/flows/")
                .AppendPathSegment(flowAlias, true)
                .AppendPathSegment("/executions")
                .PutJsonAsync(authenticationExecutionInfo)
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> AddAuthenticationFlowExecutionAsync(string realm, string flowAlias, IDictionary<string, object> dataWithProvider)
        {
            var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
                .AppendPathSegment("/admin/realms/")
                .AppendPathSegment(realm, true)
                .AppendPathSegment("/authentication/flows/")
                .AppendPathSegment(flowAlias, true)
                .AppendPathSegment("/executions/execution")
                .PostJsonAsync(dataWithProvider)
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> AddAuthenticationFlowAndExecutionToAuthenticationFlowAsync(string realm, string flowAlias, IDictionary<string, object> dataWithAliasTypeProviderDescription)
        {
            var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
                .AppendPathSegment("/admin/realms/")
                .AppendPathSegment(realm, true)
                .AppendPathSegment("/authentication/flows/")
                .AppendPathSegment(flowAlias, true)
                .AppendPathSegment("/executions/flow")
                .PostJsonAsync(dataWithAliasTypeProviderDescription)
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        public async Task<AuthenticationFlow> GetAuthenticationFlowByIdAsync(string realm, string flowId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/flows/")
            .AppendPathSegment(flowId, true)
            .GetJsonAsync<AuthenticationFlow>()
            .ConfigureAwait(false);

        public async Task<bool> UpdateAuthenticationFlowAsync(string realm, string flowId, AuthenticationFlow authenticationFlow)
        {
            var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
                .AppendPathSegment("/admin/realms/")
                .AppendPathSegment(realm, true)
                .AppendPathSegment("/authentication/flows/")
                .AppendPathSegment(flowId, true)
                .PutJsonAsync(authenticationFlow)
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAuthenticationFlowAsync(string realm, string flowId)
        {
            var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
                .AppendPathSegment("/admin/realms/")
                .AppendPathSegment(realm, true)
                .AppendPathSegment("/authentication/flows/")
                .AppendPathSegment(flowId, true)
                .DeleteAsync()
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        public async Task<IEnumerable<IDictionary<string, object>>> GetFormActionProvidersAsync(string realm) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/form-action-providers")
            .GetJsonAsync<IEnumerable<IDictionary<string, object>>>()
            .ConfigureAwait(false);

        public async Task<IEnumerable<IDictionary<string, object>>> GetFormProvidersAsync(string realm) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/form-providers")
            .GetJsonAsync<IEnumerable<IDictionary<string, object>>>()
            .ConfigureAwait(false);

        public async Task<IDictionary<string, object>> GetConfigurationDescriptionsForAllClientsAsync(string realm) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/per-client-config-description")
            .GetJsonAsync<IDictionary<string, object>>()
            .ConfigureAwait(false);

        public async Task<bool> RegisterRequiredActionAsync(string realm, IDictionary<string, object> dataWithProviderIdName)
        {
            var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
                .AppendPathSegment("/admin/realms/")
                .AppendPathSegment(realm, true)
                .AppendPathSegment("/authentication/register-required-action")
                .PostJsonAsync(dataWithProviderIdName)
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        public async Task<IEnumerable<RequiredActionProvider>> GetRequiredActionsAsync(string realm) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/required-actions")
            .GetJsonAsync<IEnumerable<RequiredActionProvider>>()
            .ConfigureAwait(false);

        public async Task<RequiredActionProvider> GetRequiredActionByAliasAsync(string realm, string requiredActionAlias) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/required-actions/")
            .AppendPathSegment(requiredActionAlias, true)
            .GetJsonAsync<RequiredActionProvider>()
            .ConfigureAwait(false);

        public async Task<bool> UpdateRequiredActionAsync(string realm, string requiredActionAlias, RequiredActionProvider requiredActionProvider)
        {
            var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
                .AppendPathSegment("/admin/realms/")
                .AppendPathSegment(realm, true)
                .AppendPathSegment("/authentication/required-actions/")
                .AppendPathSegment(requiredActionAlias, true)
                .PutJsonAsync(requiredActionProvider)
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteRequiredActionAsync(string realm, string requiredActionAlias)
        {
            var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
                .AppendPathSegment("/admin/realms/")
                .AppendPathSegment(realm, true)
                .AppendPathSegment("/authentication/required-actions/")
                .AppendPathSegment(requiredActionAlias, true)
                .DeleteAsync()
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> LowerRequiredActionPriorityAsync(string realm, string requiredActionAlias)
        {
            var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
                .AppendPathSegment("/admin/realms/")
                .AppendPathSegment(realm, true)
                .AppendPathSegment("/authentication/required-actions/")
                .AppendPathSegment(requiredActionAlias, true)
                .AppendPathSegment("/lower-priority")
                .PostAsync(new StringContent(""))
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> RaiseRequiredActionPriorityAsync(string realm, string requiredActionAlias)
        {
            var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
                .AppendPathSegment("/admin/realms/")
                .AppendPathSegment(realm, true)
                .AppendPathSegment("/authentication/required-actions/")
                .AppendPathSegment(requiredActionAlias, true)
                .AppendPathSegment("/raise-priority")
                .PostAsync(new StringContent(""))
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        public async Task<IEnumerable<IDictionary<string, object>>> GetUnregisteredRequiredActionsAsync(string realm) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/authentication/unregistered-required-actions")
            .GetJsonAsync<IEnumerable<IDictionary<string, object>>>()
            .ConfigureAwait(false);
    }
}
