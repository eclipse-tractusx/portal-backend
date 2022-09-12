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

﻿using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Keycloak.Net.Tests
{
    public partial class KeycloakClientShould
    {
        [Theory]
        [InlineData("master")]
        public async Task GetIdentityProviderInstancesAsync(string realm)
        {
            var result = await _client.GetIdentityProviderInstancesAsync(realm).ConfigureAwait(false);
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData("master")]
        public async Task GetIdentityProviderAsync(string realm)
        {
            var identityProviderInstances = await _client.GetIdentityProviderInstancesAsync(realm).ConfigureAwait(false);
            string identityProviderAlias = identityProviderInstances.FirstOrDefault()?.Alias;
            if (identityProviderAlias != null)
            {
                var result = await _client.GetIdentityProviderAsync(realm, identityProviderAlias).ConfigureAwait(false);
                Assert.NotNull(result);
            }
        }

        //[Theory]
        //[InlineData("Insurance")]
        //public async Task GetIdentityProviderTokenAsync(string realm)
        //{
        //    var token = await _client.GetIdentityProviderTokenAsync(realm).ConfigureAwait(false);
        //    Assert.NotNull(token);
        //}

        [Theory]
        [InlineData("master")]
        public async Task GetIdentityProviderAuthorizationPermissionsInitializedAsync(string realm)
        {
            var identityProviderInstances = await _client.GetIdentityProviderInstancesAsync(realm).ConfigureAwait(false);
            string identityProviderAlias = identityProviderInstances.FirstOrDefault()?.Alias;
            if (identityProviderAlias != null)
            {
                var result = await _client.GetIdentityProviderAuthorizationPermissionsInitializedAsync(realm, identityProviderAlias).ConfigureAwait(false);
                Assert.NotNull(result);
            }
        }

        [Theory(Skip = "500 Internal server error")]
        [InlineData("master")]
        public async Task GetIdentityProviderMapperTypesAsync(string realm)
        {
            var identityProviderInstances = await _client.GetIdentityProviderInstancesAsync(realm).ConfigureAwait(false);
            string identityProviderAlias = identityProviderInstances.FirstOrDefault()?.Alias;
            if (identityProviderAlias != null)
            {
                var result = await _client.GetIdentityProviderMapperTypesAsync(realm, identityProviderAlias).ConfigureAwait(false);
                Assert.NotNull(result);
            }
        }

        [Theory]
        [InlineData("master")]
        public async Task GetIdentityProviderMappersAsync(string realm)
        {
            var identityProviderInstances = await _client.GetIdentityProviderInstancesAsync(realm).ConfigureAwait(false);
            string identityProviderAlias = identityProviderInstances.FirstOrDefault()?.Alias;
            if (identityProviderAlias != null)
            {
                var result = await _client.GetIdentityProviderMappersAsync(realm, identityProviderAlias).ConfigureAwait(false);
                Assert.NotNull(result);
            }
        }

        [Theory]
        [InlineData("master")]
        public async Task GetIdentityProviderMapperByIdAsync(string realm)
        {
            var identityProviderInstances = await _client.GetIdentityProviderInstancesAsync(realm).ConfigureAwait(false);
            string identityProviderAlias = identityProviderInstances.FirstOrDefault()?.Alias;
            if (identityProviderAlias != null)
            {
                var mappers = await _client.GetIdentityProviderMappersAsync(realm, identityProviderAlias).ConfigureAwait(false);
                string mapperId = mappers.FirstOrDefault()?.Id;
                if (mapperId != null)
                {
                    var result = await _client.GetIdentityProviderMapperByIdAsync(realm, identityProviderAlias, mapperId).ConfigureAwait(false);
                    Assert.NotNull(result);
                }
            }
        }

        [Theory]
        [InlineData("master")]
        public async Task GetIdentityProviderByProviderIdAsync(string realm)
        {
            var identityProviderInstances = await _client.GetIdentityProviderInstancesAsync(realm).ConfigureAwait(false);
            string identityProviderId = identityProviderInstances.FirstOrDefault()?.ProviderId;
            if (identityProviderId != null)
            {
                var result = await _client.GetIdentityProviderByProviderIdAsync(realm, identityProviderId).ConfigureAwait(false);
                Assert.NotNull(result);
            }
        }
    }
}
