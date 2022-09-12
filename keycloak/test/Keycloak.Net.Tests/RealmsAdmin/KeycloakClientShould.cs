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
