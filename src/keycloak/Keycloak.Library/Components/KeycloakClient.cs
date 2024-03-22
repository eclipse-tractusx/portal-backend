/********************************************************************************
 * MIT License
 *
 * Copyright (c) 2019 Luk Vermeulen
 * Copyright (c) 2021, 2023 BMW Group AG
 * Copyright (c) 2021, 2023 Contributors to the Eclipse Foundation
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

using Flurl.Http;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Components;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library;

public partial class KeycloakClient
{
    public async Task CreateComponentAsync(string realm, Component componentRepresentation) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/components")
            .PostJsonAsync(componentRepresentation)
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task<IEnumerable<Component>> GetComponentsAsync(string realm, string? name = null, string? parent = null, string? type = null)
    {
        var queryParams = new Dictionary<string, object?>
        {
            [nameof(name)] = name,
            [nameof(parent)] = parent,
            [nameof(type)] = type
        };

        return await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/components")
            .SetQueryParams(queryParams)
            .GetJsonAsync<IEnumerable<Component>>()
            .ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public async Task<Component> GetComponentAsync(string realm, string componentId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/components/")
            .AppendPathSegment(componentId, true)
            .GetJsonAsync<Component>()
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task UpdateComponentAsync(string realm, string componentId, Component componentRepresentation) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/components/")
            .AppendPathSegment(componentId, true)
            .PutJsonAsync(componentRepresentation)
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task DeleteComponentAsync(string realm, string componentId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/components/")
            .AppendPathSegment(componentId, true)
            .DeleteAsync()
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task<IEnumerable<ComponentType>> GetSubcomponentTypesAsync(string realm, string componentId, string? type = null)
    {
        var queryParams = new Dictionary<string, object?>
        {
            [nameof(type)] = type
        };

        var result = await (await GetBaseUrlAsync(realm).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/components/")
            .AppendPathSegment(componentId, true)
            .AppendPathSegment("/sub-component-types")
            .SetQueryParams(queryParams)
            .GetJsonAsync<IEnumerable<ComponentType>>()
            .ConfigureAwait(ConfigureAwaitOptions.None);
        return result;
    }
}
