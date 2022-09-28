/********************************************************************************
 * MIT License
 *
 * Copyright (c) 2019 Luk Vermeulen
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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

using Flurl;
using Flurl.Http;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Common.Extensions;

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

    private static async Task<string> GetAccessTokenWithClientIdAsync(string url, string realm, string clientSecret, string? clientId)
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

    public static async Task<IFlurlRequest> WithAuthenticationAsync(this IFlurlRequest request, Func<Task<string>>? getTokenAsync, string url, string realm, string? userName, string? password, string? clientSecret, string? clientId)
    {
        string? token = null;

        if (getTokenAsync != null)
        {
            token = await getTokenAsync().ConfigureAwait(false);
        }
        else if (clientSecret != null)
        {
            token = await GetAccessTokenWithClientIdAsync(url, realm, clientSecret, clientId).ConfigureAwait(false);
        }
        else if (userName != null)
        {
            token = await GetAccessTokenAsync(url, realm, userName, password ?? "").ConfigureAwait(false);
        }
        else
        {
            throw new ArgumentException($"{nameof(getTokenAsync)}, {nameof(userName)} and {nameof(clientSecret)} must not all be null");
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
