/********************************************************************************
 * MIT License
 *
 * Copyright (c) 2019 Luk Vermeulen
 * Copyright (c) 2021,2022 BMW Group AG
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

﻿using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Keycloak.Net.Tests
{
    public partial class KeycloakClientShould
    {
        [Theory]
        [InlineData("master")]
        public async Task GetAuthenticatorProvidersAsync(string realm)
        {
            var result = await _client.GetAuthenticatorProvidersAsync(realm).ConfigureAwait(false);
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData("master")]
        public async Task GetClientAuthenticatorProvidersAsync(string realm)
        {
            var result = await _client.GetClientAuthenticatorProvidersAsync(realm).ConfigureAwait(false);
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData("master")]
        public async Task GetAuthenticatorProviderConfigurationDescriptionAsync(string realm)
        {
            var providers = await _client.GetAuthenticatorProvidersAsync(realm).ConfigureAwait(false);
            string providerId = providers.FirstOrDefault()?.FirstOrDefault(x => x.Key == "id").Value.ToString();
            if (providerId != null)
            {
                var result = await _client.GetAuthenticatorProviderConfigurationDescriptionAsync(realm, providerId).ConfigureAwait(false);
                Assert.NotNull(result);
            }
        }

        [Theory(Skip = "Not working yet")]
        [InlineData("master")]
        public async Task GetAuthenticatorConfigurationAsync(string realm)
        {
            string configurationId = ""; //TODO
            if (configurationId != null)
            {
                var result = await _client.GetAuthenticatorConfigurationAsync(realm, configurationId).ConfigureAwait(false);
                Assert.NotNull(result);
            }
        }

        [Theory]
        [InlineData("master")]
        public async Task GetAuthenticationExecutionAsync(string realm)
        {
            var flows = await _client.GetAuthenticationFlowsAsync(realm).ConfigureAwait(false);
            string flowAlias = flows.FirstOrDefault()?.Alias;
            if (flowAlias != null)
            {
                var executions = await _client.GetAuthenticationFlowExecutionsAsync(realm, flowAlias).ConfigureAwait(false);
                string executionId = executions.FirstOrDefault()?.Id;
                if (executionId != null)
                {
                    var result = await _client.GetAuthenticationExecutionAsync(realm, executionId).ConfigureAwait(false);
                    Assert.NotNull(result);
                }
            }
        }

        [Theory]
        [InlineData("master")]
        public async Task GetAuthenticationFlowsAsync(string realm)
        {
            var result = await _client.GetAuthenticationFlowsAsync(realm).ConfigureAwait(false);
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData("master")]
        public async Task GetAuthenticationFlowExecutionsAsync(string realm)
        {
            var flows = await _client.GetAuthenticationFlowsAsync(realm).ConfigureAwait(false);
            string flowAlias = flows.FirstOrDefault()?.Alias;
            if (flowAlias != null)
            {
                var result = await _client.GetAuthenticationFlowExecutionsAsync(realm, flowAlias).ConfigureAwait(false);
                Assert.NotNull(result);
            }
        }

        [Theory]
        [InlineData("master")]
        public async Task GetAuthenticationFlowByIdAsync(string realm)
        {
            var flows = await _client.GetAuthenticationFlowsAsync(realm).ConfigureAwait(false);
            string flowId = flows.FirstOrDefault()?.Id;
            if (flowId != null)
            {
                var result = await _client.GetAuthenticationFlowByIdAsync(realm, flowId).ConfigureAwait(false);
                Assert.NotNull(result);
            }
        }

        [Theory]
        [InlineData("master")]
        public async Task GetFormActionProvidersAsync(string realm)
        {
            var result = await _client.GetFormActionProvidersAsync(realm).ConfigureAwait(false);
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData("master")]
        public async Task GetFormProvidersAsync(string realm)
        {
            var result = await _client.GetFormProvidersAsync(realm).ConfigureAwait(false);
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData("master")]
        public async Task GetConfigurationDescriptionsForAllClientsAsync(string realm)
        {
            var result = await _client.GetConfigurationDescriptionsForAllClientsAsync(realm).ConfigureAwait(false);
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData("master")]
        public async Task GetRequiredActionsAsync(string realm)
        {
            var result = await _client.GetRequiredActionsAsync(realm).ConfigureAwait(false);
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData("master")]
        public async Task GetRequiredActionByAliasAsync(string realm)
        {
            var requiredActions = await _client.GetRequiredActionsAsync(realm).ConfigureAwait(false);
            string requiredActionAlias = requiredActions.FirstOrDefault()?.Alias;
            if (requiredActionAlias != null)
            {
                var result = await _client.GetRequiredActionByAliasAsync(realm, requiredActionAlias).ConfigureAwait(false);
                Assert.NotNull(result);
            }
        }

        [Theory]
        [InlineData("master")]
        public async Task GetUnregisteredRequiredActionsAsync(string realm)
        {
            var result = await _client.GetUnregisteredRequiredActionsAsync(realm).ConfigureAwait(false);
            Assert.NotNull(result);
        }
    }
}
