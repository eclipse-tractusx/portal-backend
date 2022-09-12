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
        public async Task GetRoleMappingsForGroupAsync(string realm)
        {
            var groups = await _client.GetGroupHierarchyAsync(realm).ConfigureAwait(false);
            string groupId = groups.FirstOrDefault()?.Id;
            if (groupId != null)
            {
                var result = await _client.GetRoleMappingsForGroupAsync(realm, groupId).ConfigureAwait(false);
                Assert.NotNull(result);
            }
        }

        [Theory]
        [InlineData("master")]
        public async Task GetRealmRoleMappingsForGroupAsync(string realm)
        {
            var groups = await _client.GetGroupHierarchyAsync(realm).ConfigureAwait(false);
            string groupId = groups.FirstOrDefault()?.Id;
            if (groupId != null)
            {
                var result = await _client.GetRealmRoleMappingsForGroupAsync(realm, groupId).ConfigureAwait(false);
                Assert.NotNull(result);
            }
        }

        [Theory]
        [InlineData("master")]
        public async Task GetAvailableRealmRoleMappingsForGroupAsync(string realm)
        {
            var groups = await _client.GetGroupHierarchyAsync(realm).ConfigureAwait(false);
            string groupId = groups.FirstOrDefault()?.Id;
            if (groupId != null)
            {
                var result = await _client.GetAvailableRealmRoleMappingsForGroupAsync(realm, groupId).ConfigureAwait(false);
                Assert.NotNull(result);
            }
        }

        [Theory]
        [InlineData("master")]
        public async Task GetEffectiveRealmRoleMappingsForGroupAsync(string realm)
        {
            var groups = await _client.GetGroupHierarchyAsync(realm).ConfigureAwait(false);
            string groupId = groups.FirstOrDefault()?.Id;
            if (groupId != null)
            {
                var result = await _client.GetEffectiveRealmRoleMappingsForGroupAsync(realm, groupId).ConfigureAwait(false);
                Assert.NotNull(result);
            }
        }

        [Theory]
        [InlineData("master")]
        public async Task GetRoleMappingsForUserAsync(string realm)
        {
            var users = await _client.GetUsersAsync(realm).ConfigureAwait(false);
            string userId = users.FirstOrDefault()?.Id;
            if (userId != null)
            {
                var result = await _client.GetRoleMappingsForUserAsync(realm, userId).ConfigureAwait(false);
                Assert.NotNull(result);
            }
        }

        [Theory]
        [InlineData("master")]
        public async Task GetRealmRoleMappingsForUserAsync(string realm)
        {
            var users = await _client.GetUsersAsync(realm).ConfigureAwait(false);
            string userId = users.FirstOrDefault()?.Id;
            if (userId != null)
            {
                var result = await _client.GetRealmRoleMappingsForUserAsync(realm, userId).ConfigureAwait(false);
                Assert.NotNull(result);
            }
        }

        [Theory]
        [InlineData("master")]
        public async Task GetAvailableRealmRoleMappingsForUserAsync(string realm)
        {
            var users = await _client.GetUsersAsync(realm).ConfigureAwait(false);
            string userId = users.FirstOrDefault()?.Id;
            if (userId != null)
            {
                var result = await _client.GetAvailableRealmRoleMappingsForUserAsync(realm, userId).ConfigureAwait(false);
                Assert.NotNull(result);
            }
        }

        [Theory]
        [InlineData("master")]
        public async Task GetEffectiveRealmRoleMappingsForUserAsync(string realm)
        {
            var users = await _client.GetUsersAsync(realm).ConfigureAwait(false);
            string userId = users.FirstOrDefault()?.Id;
            if (userId != null)
            {
                var result = await _client.GetEffectiveRealmRoleMappingsForUserAsync(realm, userId).ConfigureAwait(false);
                Assert.NotNull(result);
            }
        }
    }
}
