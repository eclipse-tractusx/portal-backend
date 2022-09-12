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

using Flurl;
using Flurl.Http;

namespace CatenaX.NetworkServices.Keycloak.Library.Common.Extensions;

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
