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
