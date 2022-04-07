using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;

namespace Keycloak.Net.Common.Extensions
{
    public static class FlurlRequestExtensions
    {
        private static async Task<string> GetAccessTokenAsync(string url, string realm, string userName, string password)
        {
            var result = await url
                .AppendPathSegment($"/auth/realms/{realm}/protocol/openid-connect/token")
                .WithHeader("Accept", "application/json")
                .PostUrlEncodedAsync(new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("username", userName),
                    new KeyValuePair<string, string>("password", password),
                    new KeyValuePair<string, string>("client_id", "admin-cli")
                })
                .ReceiveJson().ConfigureAwait(false);

            string accessToken = result
                .access_token.ToString();

            return accessToken;
        }

        private static async Task<string> GetAccessTokenWithClientIdAsync(string url, string realm, string clientSecret, string clientId)
        {
            var result = await url
                .AppendPathSegment($"/auth/realms/{realm}/protocol/openid-connect/token")
                .WithHeader("Content-Type", "application/x-www-form-urlencoded")
                .PostUrlEncodedAsync(new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("client_secret", clientSecret),
                    new KeyValuePair<string, string>("client_id", clientId ?? "admin-cli")
                })
                .ReceiveJson().ConfigureAwait(false);

            string accessToken = result
                .access_token.ToString();

            return accessToken;
        }

        public static async Task<IFlurlRequest> WithAuthenticationAsync(this IFlurlRequest request, Func<Task<string>> getTokenAsync, string url, string realm, string userName, string password, string clientSecret, string clientId)
        {
            string token = null;

            if (getTokenAsync != null)
            {
                token = await getTokenAsync().ConfigureAwait(false);
            }
            else if (clientSecret != null)
            {
                token = await GetAccessTokenWithClientIdAsync(url, realm, clientSecret, clientId).ConfigureAwait(false);
            }
            else
            {
                token = await GetAccessTokenAsync(url, realm, userName, password).ConfigureAwait(false);
            }

            return request.WithOAuthBearerToken(token);
        }

        public static IFlurlRequest WithForwardedHttpHeaders(this IFlurlRequest request, ForwardedHttpHeaders forwardedHeaders)
        {
            if (!string.IsNullOrEmpty(forwardedHeaders?.forwardedFor))
            {
	            request = request.WithHeader("X-Forwarded-For", forwardedHeaders.forwardedFor);
            }

            if (!string.IsNullOrEmpty(forwardedHeaders?.forwardedProto))
            {
	            request = request.WithHeader("X-Forwarded-Proto", forwardedHeaders.forwardedProto);
            }

            if (!string.IsNullOrEmpty(forwardedHeaders?.forwardedHost))
            {
	            request = request.WithHeader("X-Forwarded-Host", forwardedHeaders.forwardedHost);
            }

            return request;
        }
    }
}
