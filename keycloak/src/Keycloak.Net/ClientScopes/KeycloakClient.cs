using System.Collections.Generic;
using System.Threading.Tasks;
using Flurl.Http;
using Keycloak.Net.Models.ClientScopes;

namespace Keycloak.Net
{
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
}
