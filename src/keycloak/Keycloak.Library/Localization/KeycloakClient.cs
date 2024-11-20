/********************************************************************************
 * Copyright (c) 2024 Contributors to the Eclipse Foundation
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

using Flurl.Http;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Linq;
using System.Net.Http.Headers;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library;

public partial class KeycloakClient
{
    private const string AdminUrlSegment = "/admin/realms/";
    private const string LocalizationUrlSegment = "/localization/";

    public async Task<IEnumerable<string>> GetLocaleAsync(string realm, CancellationToken cancellationToken = default)
    {
        return await (await GetBaseUrlAsync(realm, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment(AdminUrlSegment)
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/localization")
            .GetJsonAsync<IEnumerable<string>>(cancellationToken: cancellationToken)
            .ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public async Task<IEnumerable<KeyValuePair<string, string>>> GetLocaleAsync(string realm, string locale, CancellationToken cancellationToken = default)
    {
        var response = await (await GetBaseUrlAsync(realm, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment(AdminUrlSegment)
            .AppendPathSegment(realm, true)
            .AppendPathSegment(LocalizationUrlSegment)
            .AppendPathSegment(locale, true)
            .GetJsonAsync<IDictionary<string, string>?>(cancellationToken: cancellationToken)
            .ConfigureAwait(ConfigureAwaitOptions.None);

        return response == null
            ? Enumerable.Empty<KeyValuePair<string, string>>()
            : response.FilterNotNull();
    }

    public async Task UpdateLocaleAsync(string realm, string locale, string key, string translation, CancellationToken cancellationToken)
    {
        using var content = new StringContent(translation, MediaTypeHeaderValue.Parse("text/plain"));
        await (await GetBaseUrlAsync(realm, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment(AdminUrlSegment)
            .AppendPathSegment(realm, true)
            .AppendPathSegment(LocalizationUrlSegment)
            .AppendPathSegment(locale, true)
            .AppendPathSegment(key, true)
            .PutAsync(content, cancellationToken: cancellationToken)
            .ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public async Task DeleteLocaleAsync(string realm, string locale, string key, CancellationToken cancellationToken) =>
        await (await GetBaseUrlAsync(realm, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment(AdminUrlSegment)
            .AppendPathSegment(realm, true)
            .AppendPathSegment(LocalizationUrlSegment)
            .AppendPathSegment(locale, true)
            .AppendPathSegment(key, true)
            .DeleteAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(ConfigureAwaitOptions.None);

    public async Task DeleteLocaleAsync(string realm, string locale, CancellationToken cancellationToken) =>
        await (await GetBaseUrlAsync(realm, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None))
            .AppendPathSegment(AdminUrlSegment)
            .AppendPathSegment(realm, true)
            .AppendPathSegment(LocalizationUrlSegment)
            .AppendPathSegment(locale, true)
            .DeleteAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(ConfigureAwaitOptions.None);
}
