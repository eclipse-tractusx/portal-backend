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

﻿using Flurl.Http;
using Keycloak.Net.Models.AuthorizationScopes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Keycloak.Net
{
    public partial class KeycloakClient
    {
        public async Task<bool> CreateAuthorizationScopeAsync(string realm, string resourceServerId, AuthorizationScope scope)
        {
            var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
                .AppendPathSegment("/admin/realms/")
                .AppendPathSegment(realm, true)
                .AppendPathSegment("/clients/")
                .AppendPathSegment(resourceServerId, true)
                .AppendPathSegment("/authz/resource-server/scope")
                .PostJsonAsync(scope)
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        public async Task<IEnumerable<AuthorizationScope>> GetAuthorizationScopesAsync(string realm, string resourceServerId = null, 
            bool deep = false, int? first = null, int? max = null, string name = null)
        {
            var queryParams = new Dictionary<string, object>
            {
                [nameof(deep)] = deep,
                [nameof(first)] = first,
                [nameof(max)] = max,
                [nameof(name)] = name,
            };
            
            return await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
                .AppendPathSegment("/admin/realms/")
                .AppendPathSegment(realm, true)
                .AppendPathSegment("/clients/")
                .AppendPathSegment(resourceServerId, true)
                .AppendPathSegment("/authz/resource-server/scope")
                .SetQueryParams(queryParams)
                .GetJsonAsync<IEnumerable<AuthorizationScope>>()
                .ConfigureAwait(false);
        }

        public async Task<AuthorizationScope> GetAuthorizationScopeAsync(string realm, string resourceServerId, string scopeId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(resourceServerId, true)
            .AppendPathSegment("/authz/resource-server/scope/")
            .AppendPathSegment(scopeId, true)
            .GetJsonAsync<AuthorizationScope>()
            .ConfigureAwait(false);

        public async Task<bool> UpdateAuthorizationScopeAsync(string realm, string resourceServerId, string scopeId, AuthorizationScope scope)
        {
            var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
                .AppendPathSegment("/admin/realms/")
                .AppendPathSegment(realm, true)
                .AppendPathSegment("/clients/")
                .AppendPathSegment(resourceServerId, true)
                .AppendPathSegment("/authz/resource-server/scope/")
                .AppendPathSegment(scopeId, true)
                .PutJsonAsync(scope)
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAuthorizationScopeAsync(string realm, string resourceServerId, string scopeId)
        {
            var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
                .AppendPathSegment("/admin/realms/")
                .AppendPathSegment(realm, true)
                .AppendPathSegment("/clients/")
                .AppendPathSegment(resourceServerId, true)
                .AppendPathSegment("/authz/resource-server/scope/")
                .AppendPathSegment(scopeId, true)
                .DeleteAsync()
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }
    }
}
