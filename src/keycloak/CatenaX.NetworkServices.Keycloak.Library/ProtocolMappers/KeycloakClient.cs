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

using CatenaX.NetworkServices.Keycloak.Library.Models.ProtocolMappers;
using Flurl.Http;

namespace CatenaX.NetworkServices.Keycloak.Library;

public partial class KeycloakClient
{
    public async Task<bool> CreateMultipleProtocolMappersAsync(string realm, string clientScopeId, IEnumerable<ProtocolMapper> protocolMapperRepresentations)
    {
        var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/client-scopes/")
            .AppendPathSegment(clientScopeId, true)
            .AppendPathSegment("/protocol-mappers/add-models")
            .PostJsonAsync(protocolMapperRepresentations)
            .ConfigureAwait(false);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> CreateProtocolMapperAsync(string realm, string clientScopeId, ProtocolMapper protocolMapperRepresentation)
    {
        var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/client-scopes/")
            .AppendPathSegment(clientScopeId, true)
            .AppendPathSegment("/protocol-mappers/models")
            .PostJsonAsync(protocolMapperRepresentation)
            .ConfigureAwait(false);
        return response.IsSuccessStatusCode;
    }

    public async Task<IEnumerable<ProtocolMapper>> GetProtocolMappersAsync(string realm, string clientScopeId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
        .AppendPathSegment("/admin/realms/")
        .AppendPathSegment(realm, true)
        .AppendPathSegment("/client-scopes/")
        .AppendPathSegment(clientScopeId, true)
        .AppendPathSegment("/protocol-mappers/models")
        .GetJsonAsync<IEnumerable<ProtocolMapper>>()
        .ConfigureAwait(false);

    public async Task<ProtocolMapper> GetProtocolMapperAsync(string realm, string clientScopeId, string protocolMapperId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
        .AppendPathSegment("/admin/realms/")
        .AppendPathSegment(realm, true)
        .AppendPathSegment("/client-scopes/")
        .AppendPathSegment(clientScopeId, true)
        .AppendPathSegment("/protocol-mappers/models/")
        .AppendPathSegment(protocolMapperId, true)
        .GetJsonAsync<ProtocolMapper>()
        .ConfigureAwait(false);

    public async Task<bool> UpdateProtocolMapperAsync(string realm, string clientScopeId, string protocolMapperId, ProtocolMapper protocolMapperRepresentation)
    {
        var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/client-scopes/")
            .AppendPathSegment(clientScopeId, true)
            .AppendPathSegment("/protocol-mappers/models/")
            .AppendPathSegment(protocolMapperId, true)
            .PutJsonAsync(protocolMapperRepresentation)
            .ConfigureAwait(false);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteProtocolMapperAsync(string realm, string clientScopeId, string protocolMapperId)
    {
        var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/client-scopes/")
            .AppendPathSegment(clientScopeId, true)
            .AppendPathSegment("/protocol-mappers/models/")
            .AppendPathSegment(protocolMapperId, true)
            .DeleteAsync()
            .ConfigureAwait(false);
        return response.IsSuccessStatusCode;
    }

    public async Task<IEnumerable<ProtocolMapper>> GetProtocolMappersByNameAsync(string realm, string clientScopeId, string protocol) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
        .AppendPathSegment("/admin/realms/")
        .AppendPathSegment(realm, true)
        .AppendPathSegment("/client-scopes/")
        .AppendPathSegment(clientScopeId, true)
        .AppendPathSegment("/protocol-mappers/protocol/")
        .AppendPathSegment(protocol, true)
        .GetJsonAsync<IEnumerable<ProtocolMapper>>()
        .ConfigureAwait(false);

    public async Task<bool> CreateClientProtocolMapperAsync(string realm, string clientId, ProtocolMapper protocolMapperRepresentation)
    {
        var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/protocol-mappers/models")
            .PostJsonAsync(protocolMapperRepresentation)
            .ConfigureAwait(false);
        return response.IsSuccessStatusCode;
    }
}        
