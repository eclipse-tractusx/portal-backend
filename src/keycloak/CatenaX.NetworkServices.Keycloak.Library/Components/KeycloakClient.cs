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

using CatenaX.NetworkServices.Keycloak.Library.Models.Components;
using Flurl.Http;

namespace CatenaX.NetworkServices.Keycloak.Library;

public partial class KeycloakClient
{
    public async Task CreateComponentAsync(string realm, Component componentRepresentation) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/components")
            .PostJsonAsync(componentRepresentation)
            .ConfigureAwait(false);

    public async Task<IEnumerable<Component>> GetComponentsAsync(string realm, string? name = null, string? parent = null, string? type = null)
    {
        var queryParams = new Dictionary<string, object?>
        {
            [nameof(name)] = name,
            [nameof(parent)] = parent,
            [nameof(type)] = type
        };

        return await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/components")
            .SetQueryParams(queryParams)
            .GetJsonAsync<IEnumerable<Component>>()
            .ConfigureAwait(false);
    }

    public async Task<Component> GetComponentAsync(string realm, string componentId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/components/")
            .AppendPathSegment(componentId, true)
            .GetJsonAsync<Component>()
            .ConfigureAwait(false);

    public async Task UpdateComponentAsync(string realm, string componentId, Component componentRepresentation) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/components/")
            .AppendPathSegment(componentId, true)
            .PutJsonAsync(componentRepresentation)
            .ConfigureAwait(false);

    public async Task DeleteComponentAsync(string realm, string componentId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/components/")
            .AppendPathSegment(componentId, true)
            .DeleteAsync()
            .ConfigureAwait(false);

    public async Task<IEnumerable<ComponentType>> GetSubcomponentTypesAsync(string realm, string componentId, string? type = null)
    {
        var queryParams = new Dictionary<string, object?>
        {
            [nameof(type)] = type
        };

        var result = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/components/")
            .AppendPathSegment(componentId, true)
            .AppendPathSegment("/sub-component-types")
            .SetQueryParams(queryParams)
            .GetJsonAsync<IEnumerable<ComponentType>>()
            .ConfigureAwait(false);
        return result;
    }
}
