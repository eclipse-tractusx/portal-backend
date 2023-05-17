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
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.ProtocolMappers;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library;

public partial class KeycloakClient
{
    public async Task CreateMultipleProtocolMappersAsync(string realm, string clientScopeId, IEnumerable<ProtocolMapper> protocolMapperRepresentations) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/client-scopes/")
            .AppendPathSegment(clientScopeId, true)
            .AppendPathSegment("/protocol-mappers/add-models")
            .PostJsonAsync(protocolMapperRepresentations)
            .ConfigureAwait(false);

    public async Task CreateProtocolMapperAsync(string realm, string clientScopeId, ProtocolMapper protocolMapperRepresentation) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/client-scopes/")
            .AppendPathSegment(clientScopeId, true)
            .AppendPathSegment("/protocol-mappers/models")
            .PostJsonAsync(protocolMapperRepresentation)
            .ConfigureAwait(false);

    public async Task<IEnumerable<ProtocolMapper>> GetProtocolMappersAsync(string realm, string clientScopeId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/client-scopes/")
            .AppendPathSegment(clientScopeId, true)
            .AppendPathSegment("/protocol-mappers/models")
            .GetJsonAsync<IEnumerable<ProtocolMapper>>()
            .ConfigureAwait(false);

    public async Task<ProtocolMapper> GetProtocolMapperAsync(string realm, string clientScopeId, string protocolMapperId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/client-scopes/")
            .AppendPathSegment(clientScopeId, true)
            .AppendPathSegment("/protocol-mappers/models/")
            .AppendPathSegment(protocolMapperId, true)
            .GetJsonAsync<ProtocolMapper>()
            .ConfigureAwait(false);

    public async Task UpdateProtocolMapperAsync(string realm, string clientScopeId, string protocolMapperId, ProtocolMapper protocolMapperRepresentation) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/client-scopes/")
            .AppendPathSegment(clientScopeId, true)
            .AppendPathSegment("/protocol-mappers/models/")
            .AppendPathSegment(protocolMapperId, true)
            .PutJsonAsync(protocolMapperRepresentation)
            .ConfigureAwait(false);

    public async Task DeleteProtocolMapperAsync(string realm, string clientScopeId, string protocolMapperId) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/client-scopes/")
            .AppendPathSegment(clientScopeId, true)
            .AppendPathSegment("/protocol-mappers/models/")
            .AppendPathSegment(protocolMapperId, true)
            .DeleteAsync()
            .ConfigureAwait(false);

    public async Task<IEnumerable<ProtocolMapper>> GetProtocolMappersByNameAsync(string realm, string clientScopeId, string protocol) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/client-scopes/")
            .AppendPathSegment(clientScopeId, true)
            .AppendPathSegment("/protocol-mappers/protocol/")
            .AppendPathSegment(protocol, true)
            .GetJsonAsync<IEnumerable<ProtocolMapper>>()
            .ConfigureAwait(false);

    public async Task CreateClientProtocolMapperAsync(string realm, string clientId, ProtocolMapper protocolMapperRepresentation) =>
        await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(clientId, true)
            .AppendPathSegment("/protocol-mappers/models")
            .PostJsonAsync(protocolMapperRepresentation)
            .ConfigureAwait(false);
}
