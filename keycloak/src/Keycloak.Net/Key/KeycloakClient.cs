using System.Threading.Tasks;
using Flurl.Http;
using Keycloak.Net.Models.Key;

namespace Keycloak.Net
{
    public partial class KeycloakClient
    {
        public async Task<KeysMetadata> GetKeysAsync(string realm) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/keys")
            .GetJsonAsync<KeysMetadata>()
            .ConfigureAwait(false);
    }
}
