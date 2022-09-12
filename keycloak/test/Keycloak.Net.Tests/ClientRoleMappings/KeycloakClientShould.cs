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
        [InlineData("Insurance", "insurance-product")]
        public async Task GetClientRoleMappingsForGroupAsync(string realm, string clientId)
        {
            var groups = await _client.GetGroupHierarchyAsync(realm).ConfigureAwait(false);
            string groupId = groups.FirstOrDefault()?.Id;
            if (groupId != null)
            {
                var clients = await _client.GetClientsAsync(realm).ConfigureAwait(false);
                string clientsId = clients.FirstOrDefault(x => x.ClientId == clientId)?.Id;
                if (clientId != null)
                {
                    var result = await _client.GetClientRoleMappingsForGroupAsync(realm, groupId, clientsId).ConfigureAwait(false);
                    Assert.NotNull(result);
                }
            }
        }

        [Theory]
        [InlineData("Insurance", "insurance-product")]
        public async Task GetAvailableClientRoleMappingsForGroupAsync(string realm, string clientId)
        {
            var groups = await _client.GetGroupHierarchyAsync(realm).ConfigureAwait(false);
            string groupId = groups.FirstOrDefault()?.Id;
            if (groupId != null)
            {
                var clients = await _client.GetClientsAsync(realm).ConfigureAwait(false);
                string clientsId = clients.FirstOrDefault(x => x.ClientId == clientId)?.Id;
                if (clientId != null)
                {
                    var result = await _client.GetAvailableClientRoleMappingsForGroupAsync(realm, groupId, clientsId).ConfigureAwait(false);
                    Assert.NotNull(result);
                }
            }
        }

        [Theory]
        [InlineData("Insurance", "insurance-product")]
        public async Task GetEffectiveClientRoleMappingsForGroupAsync(string realm, string clientId)
        {
            var groups = await _client.GetGroupHierarchyAsync(realm).ConfigureAwait(false);
            string groupId = groups.FirstOrDefault()?.Id;
            if (groupId != null)
            {
                var clients = await _client.GetClientsAsync(realm).ConfigureAwait(false);
                string clientsId = clients.FirstOrDefault(x => x.ClientId == clientId)?.Id;
                if (clientId != null)
                {
                    var result = await _client.GetEffectiveClientRoleMappingsForGroupAsync(realm, groupId, clientsId).ConfigureAwait(false);
                    Assert.NotNull(result);
                }
            }
        }

        [Theory]
        [InlineData("Insurance", "insurance-product")]
        public async Task GetClientRoleMappingsForUserAsync(string realm, string clientId)
        {
            var users = await _client.GetUsersAsync(realm).ConfigureAwait(false);
            string userId = users.FirstOrDefault()?.Id;
            if (userId != null)
            {
                var clients = await _client.GetClientsAsync(realm).ConfigureAwait(false);
                string clientsId = clients.FirstOrDefault(x => x.ClientId == clientId)?.Id;
                if (clientId != null)
                {
                    var result = await _client.GetClientRoleMappingsForUserAsync(realm, userId, clientsId).ConfigureAwait(false);
                    Assert.NotNull(result);
                }
            }
        }

        [Theory]
        [InlineData("Insurance", "insurance-product")]
        public async Task GetAvailableClientRoleMappingsForUserAsync(string realm, string clientId)
        {
            var users = await _client.GetUsersAsync(realm).ConfigureAwait(false);
            string userId = users.FirstOrDefault()?.Id;
            if (userId != null)
            {
                var clients = await _client.GetClientsAsync(realm).ConfigureAwait(false);
                string clientsId = clients.FirstOrDefault(x => x.ClientId == clientId)?.Id;
                if (clientId != null)
                {
                    var result = await _client.GetAvailableClientRoleMappingsForUserAsync(realm, userId, clientsId).ConfigureAwait(false);
                    Assert.NotNull(result);
                }
            }
        }

        [Theory]
        [InlineData("Insurance", "insurance-product")]
        public async Task GetEffectiveClientRoleMappingsForUserAsync(string realm, string clientId)
        {
            var users = await _client.GetUsersAsync(realm).ConfigureAwait(false);
            string userId = users.FirstOrDefault()?.Id;
            if (userId != null)
            {
                var clients = await _client.GetClientsAsync(realm).ConfigureAwait(false);
                string clientsId = clients.FirstOrDefault(x => x.ClientId == clientId)?.Id;
                if (clientId != null)
                {
                    var result = await _client.GetEffectiveClientRoleMappingsForUserAsync(realm, userId, clientsId).ConfigureAwait(false);
                    Assert.NotNull(result);
                }
            }
        }
    }
}
