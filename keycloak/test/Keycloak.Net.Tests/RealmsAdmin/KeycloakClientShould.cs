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
	    public async Task GetRealmsAsync(string realm)
	    {
		    var result = await _client.GetRealmsAsync(realm).ConfigureAwait(false);
		    Assert.NotNull(result);
	    }

        [Theory]
        [InlineData("master")]
        public async Task GetRealmAsync(string realm)
        {
            var result = await _client.GetRealmAsync(realm).ConfigureAwait(false);
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData("master")]
        public async Task GetAdminEventsAsync(string realm)
        {
            var result = await _client.GetAdminEventsAsync(realm).ConfigureAwait(false);
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData("master")]
        public async Task GetClientSessionStatsAsync(string realm)
        {
            var result = await _client.GetClientSessionStatsAsync(realm).ConfigureAwait(false);
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData("master")]
        public async Task GetRealmDefaultClientScopesAsync(string realm)
        {
            var result = await _client.GetRealmDefaultClientScopesAsync(realm).ConfigureAwait(false);
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData("master")]
        public async Task GetRealmGroupHierarchyAsync(string realm)
        {
            var result = await _client.GetRealmGroupHierarchyAsync(realm).ConfigureAwait(false);
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData("master")]
        public async Task GetRealmOptionalClientScopesAsync(string realm)
        {
            var result = await _client.GetRealmOptionalClientScopesAsync(realm).ConfigureAwait(false);
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData("master")]
        public async Task GetEventsAsync(string realm)
        {
            var result = await _client.GetEventsAsync(realm).ConfigureAwait(false);
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData("master")]
        public async Task GetRealmEventsProviderConfigurationAsync(string realm)
        {
            var result = await _client.GetRealmEventsProviderConfigurationAsync(realm).ConfigureAwait(false);
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData("master")]
        public async Task GetRealmGroupByPathAsync(string realm)
        {
            var groups = await _client.GetRealmGroupHierarchyAsync(realm).ConfigureAwait(false);
            string path = groups.FirstOrDefault()?.Path;
            if (path != null)
            {
                var result = await _client.GetRealmGroupByPathAsync(realm, path).ConfigureAwait(false);
                Assert.NotNull(result);
            }
        }

        [Theory]
        [InlineData("master")]
        public async Task GetRealmUsersManagementPermissionsAsync(string realm)
        {
            var result = await _client.GetRealmUsersManagementPermissionsAsync(realm).ConfigureAwait(false);
            Assert.NotNull(result);
        }
    }
}
