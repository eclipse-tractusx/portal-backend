/********************************************************************************
 * MIT License
 *
 * Copyright (c) 2019 Luk Vermeulen
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
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
