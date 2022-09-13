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

using CatenaX.NetworkServices.Keycloak.Library.Models.AuthorizationResources;
using Flurl.Http;

namespace CatenaX.NetworkServices.Keycloak.Library;

public partial class KeycloakClient
{
    public async Task CreateResourceAsync(string realm, string resourceServerId, AuthorizationResource resource) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(resourceServerId, true)
            .AppendPathSegment("/authz/resource-server/resource")
            .PostJsonAsync(resource)
            .ConfigureAwait(false);

    public async Task<IEnumerable<AuthorizationResource>> GetResourcesAsync(string realm, string? resourceServerId = null, 
        bool deep = false, int? first = null, int? max = null, string? name = null, string? owner = null,
        string? type = null, string? uri = null)
    {
        var queryParams = new Dictionary<string, object?>
        {
            [nameof(deep)] = deep,
            [nameof(first)] = first,
            [nameof(max)] = max,
            [nameof(name)] = name,
            [nameof(owner)] = owner,
            [nameof(type)] = type,
            [nameof(uri)] = uri
        };
        
        return await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(resourceServerId, true)
            .AppendPathSegment("/authz/resource-server/resource")
            .SetQueryParams(queryParams)
            .GetJsonAsync<IEnumerable<AuthorizationResource>>()
            .ConfigureAwait(false);
    }

    public async Task<AuthorizationResource> GetResourceAsync(string realm, string resourceServerId, string resourceId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(resourceServerId, true)
            .AppendPathSegment("/authz/resource-server/resource/")
            .AppendPathSegment(resourceId, true)
            .GetJsonAsync<AuthorizationResource>()
            .ConfigureAwait(false);

    public async Task UpdateResourceAsync(string realm, string resourceServerId, string resourceId, AuthorizationResource resource) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(resourceServerId, true)
            .AppendPathSegment("/authz/resource-server/resource/")
            .AppendPathSegment(resourceId, true)
            .PutJsonAsync(resource)
            .ConfigureAwait(false);

    public async Task DeleteResourceAsync(string realm, string resourceServerId, string resourceId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(resourceServerId, true)
            .AppendPathSegment("/authz/resource-server/resource/")
            .AppendPathSegment(resourceId, true)
            .DeleteAsync()
            .ConfigureAwait(false);
}
