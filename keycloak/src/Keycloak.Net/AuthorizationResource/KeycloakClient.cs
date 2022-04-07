using Flurl.Http;
using Keycloak.Net.Models.AuthorizationResources;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Keycloak.Net
{
    public partial class KeycloakClient
    {
        public async Task<bool> CreateResourceAsync(string realm, string resourceServerId, AuthorizationResource resource)
        {
            var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
                .AppendPathSegment("/admin/realms/")
                .AppendPathSegment(realm, true)
                .AppendPathSegment("/clients/")
                .AppendPathSegment(resourceServerId, true)
                .AppendPathSegment("/authz/resource-server/resource")
                .PostJsonAsync(resource)
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        public async Task<IEnumerable<AuthorizationResource>> GetResourcesAsync(string realm, string resourceServerId = null, 
            bool deep = false, int? first = null, int? max = null, string name = null, string owner = null,
            string type = null, string uri = null)
        {
            var queryParams = new Dictionary<string, object>
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

        public async Task<AuthorizationResource> GetResourceAsync(string realm, string resourceServerId, string resourceId) => await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
            .AppendPathSegment("/admin/realms/")
            .AppendPathSegment(realm, true)
            .AppendPathSegment("/clients/")
            .AppendPathSegment(resourceServerId, true)
            .AppendPathSegment("/authz/resource-server/resource/")
            .AppendPathSegment(resourceId, true)
            .GetJsonAsync<AuthorizationResource>()
            .ConfigureAwait(false);

        public async Task<bool> UpdateResourceAsync(string realm, string resourceServerId, string resourceId, AuthorizationResource resource)
        {
            var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
                .AppendPathSegment("/admin/realms/")
                .AppendPathSegment(realm, true)
                .AppendPathSegment("/clients/")
                .AppendPathSegment(resourceServerId, true)
                .AppendPathSegment("/authz/resource-server/resource/")
                .AppendPathSegment(resourceId, true)
                .PutJsonAsync(resource)
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteResourceAsync(string realm, string resourceServerId, string resourceId)
        {
            var response = await (await GetBaseUrlAsync(realm).ConfigureAwait(false))
                .AppendPathSegment("/admin/realms/")
                .AppendPathSegment(realm, true)
                .AppendPathSegment("/clients/")
                .AppendPathSegment(resourceServerId, true)
                .AppendPathSegment("/authz/resource-server/resource/")
                .AppendPathSegment(resourceId, true)
                .DeleteAsync()
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }
    }
}
