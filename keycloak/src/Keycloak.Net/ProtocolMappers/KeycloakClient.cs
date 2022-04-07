using System.Collections.Generic;
using System.Threading.Tasks;
using Flurl.Http;
using Keycloak.Net.Models.ProtocolMappers;

namespace Keycloak.Net
{
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
}
