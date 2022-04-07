using System.Threading.Tasks;
using Flurl.Http;
using Keycloak.Net.Models.OpenIDConfiguration;

namespace Keycloak.Net
{
    public partial class KeycloakClient
    {
        public async Task<OpenIDConfiguration> GetOpenIDConfigurationAsync(string realm) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/.well-known/openid-configuration")
            .GetJsonAsync<OpenIDConfiguration>()
            .ConfigureAwait(false);
    }
}
