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

using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.AuthorizationResources;
using Flurl.Http;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library;

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
