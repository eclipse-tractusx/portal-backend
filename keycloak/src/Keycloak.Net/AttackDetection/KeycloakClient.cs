using System.Threading.Tasks;
using Flurl.Http;
using Keycloak.Net.Models.AttackDetection;

namespace Keycloak.Net
{
    public partial class KeycloakClient
    {
        public async Task<bool> ClearUserLoginFailuresAsync(string realm)
        {
            var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
                .AppendPathSegment("/admin/realms/")
                .AppendPathSegment(realm, true)
                .AppendPathSegment("/attack-detection/brute-force/users")
                .DeleteAsync()
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ClearUserLoginFailuresAsync(string realm, string userId)
        {
            var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
                .AppendPathSegment("/admin/realms/")
                .AppendPathSegment(realm, true)
                .AppendPathSegment("/attack-detection/brute-force/users/")
                .AppendPathSegment(userId, true)
                .DeleteAsync()
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        public async Task<UserNameStatus> GetUserNameStatusInBruteForceDetectionAsync(string realm, string userId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/attack-detection/brute-force/users/")
            .AppendPathSegment(userId, true)
            .GetJsonAsync<UserNameStatus>()
            .ConfigureAwait(false);
    }
}
