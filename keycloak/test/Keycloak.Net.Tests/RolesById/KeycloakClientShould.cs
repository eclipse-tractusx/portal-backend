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
        public async Task GetRoleByIdAsync(string realm)
        {
            var roles = await _client.GetRolesAsync(realm).ConfigureAwait(false);
            string roleId = roles.FirstOrDefault()?.Id;
            if (roleId != null)
            {
                var result = await _client.GetRoleByIdAsync(realm, roleId).ConfigureAwait(false);
                Assert.NotNull(result);
            }
        }

        [Theory]
        [InlineData("master")]
        public async Task GetRoleChildrenAsync(string realm)
        {
            var roles = await _client.GetRolesAsync(realm).ConfigureAwait(false);
            string roleId = roles.FirstOrDefault()?.Id;
            if (roleId != null)
            {
                var result = await _client.GetRoleChildrenAsync(realm, roleId).ConfigureAwait(false);
                Assert.NotNull(result);
            }
        }

        [Theory]
        [InlineData("Insurance", "insurance-product")]
        public async Task GetClientRolesForCompositeByIdAsync(string realm, string clientId)
        {
            var roles = await _client.GetRolesAsync(realm).ConfigureAwait(false);
            string roleId = roles.FirstOrDefault()?.Id;
            if (roleId != null)
            {
                var clients = await _client.GetClientsAsync(realm).ConfigureAwait(false);
                string clientsId = clients.FirstOrDefault(x => x.ClientId == clientId)?.Id;
                if (clientsId != null)
                {
                    var result = await _client.GetClientRolesForCompositeByIdAsync(realm, roleId, clientsId).ConfigureAwait(false);
                    Assert.NotNull(result);
                }
            }
        }

        [Theory]
        [InlineData("master")]
        public async Task GetRealmRolesForCompositeByIdAsync(string realm)
        {
            var roles = await _client.GetRolesAsync(realm).ConfigureAwait(false);
            string roleId = roles.FirstOrDefault()?.Id;
            if (roleId != null)
            {
                var result = await _client.GetRealmRolesForCompositeByIdAsync(realm, roleId).ConfigureAwait(false);
                Assert.NotNull(result);
            }
        }

        [Theory]
        [InlineData("master")]
        public async Task GetRoleByIdAuthorizationPermissionsInitializedAsync(string realm)
        {
            var roles = await _client.GetRolesAsync(realm).ConfigureAwait(false);
            string roleId = roles.FirstOrDefault()?.Id;
            if (roleId != null)
            {
                var result = await _client.GetRoleByIdAuthorizationPermissionsInitializedAsync(realm, roleId).ConfigureAwait(false);
                Assert.NotNull(result);
            }
        }
    }
}
