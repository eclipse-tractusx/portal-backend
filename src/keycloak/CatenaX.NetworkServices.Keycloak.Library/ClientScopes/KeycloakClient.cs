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

using CatenaX.NetworkServices.Keycloak.Library.Models.ClientScopes;
using Flurl.Http;

namespace CatenaX.NetworkServices.Keycloak.Library;

public partial class KeycloakClient
{
    public async Task<bool> CreateClientScopeAsync(string realm, ClientScope clientScope)
    {
        var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/client-scopes")
            .PostJsonAsync(clientScope)
            .ConfigureAwait(false);
        return response.IsSuccessStatusCode;
    }

    public async Task<IEnumerable<ClientScope>> GetClientScopesAsync(string realm) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
        .AppendPathSegment("/admin/realms/")
        .AppendPathSegment(realm, true)
        .AppendPathSegment("/client-scopes")
        .GetJsonAsync<IEnumerable<ClientScope>>()
        .ConfigureAwait(false);

    public async Task<ClientScope> GetClientScopeAsync(string realm, string clientScopeId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
        .AppendPathSegment("/admin/realms/")
        .AppendPathSegment(realm, true)
        .AppendPathSegment("/client-scopes/")
        .AppendPathSegment(clientScopeId, true)
        .GetJsonAsync<ClientScope>()
        .ConfigureAwait(false);

    public async Task<bool> UpdateClientScopeAsync(string realm, string clientScopeId, ClientScope clientScope)
    {
        var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/client-scopes/")
            .AppendPathSegment(clientScopeId, true)
            .PutJsonAsync(clientScope)
            .ConfigureAwait(false);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteClientScopeAsync(string realm, string clientScopeId)
    {
        var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/client-scopes/")
            .AppendPathSegment(clientScopeId, true)
            .DeleteAsync()
            .ConfigureAwait(false);
        return response.IsSuccessStatusCode;
    }
}
