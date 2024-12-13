/********************************************************************************
 * Copyright (c) 2024 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Authentication;

internal static class KeycloakAccessTokenExtensions
{
    public static async Task<KeycloakAccessToken> GetAccessToken(this KeycloakAccessToken? token, Url url, string realm, string? userName, string password, string? clientSecret, string clientId, CancellationToken cancellationToken)
    {
        if (clientSecret == null && userName == null)
        {
            throw new ArgumentException($"{nameof(userName)} and {nameof(clientSecret)} must not all be null");
        }

        if (token is null)
        {
            return await GetToken(url, realm, userName, password, clientSecret, clientId, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        }

        if (token.ExpiresIn > DateTimeOffset.UtcNow)
        {
            return token;
        }

        return token.RefreshExpiresIn > DateTimeOffset.UtcNow ?
            await GetToken(url, realm, [
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("refresh_token", token.RefreshToken),
                new KeyValuePair<string, string>("client_id", clientId)
            ], cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None) :
            await GetToken(url, realm, userName, password, clientSecret, clientId, cancellationToken)
                .ConfigureAwait(ConfigureAwaitOptions.None);
    }

    private static async Task<KeycloakAccessToken> GetToken(Url url, string realm, string? userName, string password, string? clientSecret, string clientId, CancellationToken cancellationToken)
    {
        if (clientSecret != null)
        {
            return await GetToken(url, realm, [
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("client_id", clientId)
            ], cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        }

        if (userName != null)
        {
            return await GetToken(url, realm, [
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", userName),
                new KeyValuePair<string, string>("password", password),
                new KeyValuePair<string, string>("client_id", "admin-cli")
            ], cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        }

        throw new ArgumentException($"{nameof(userName)} and {nameof(clientSecret)} must not all be null");
    }

    private static async Task<KeycloakAccessToken> GetToken(Url url, string realm, List<KeyValuePair<string, string>> keyValues, CancellationToken cancellationToken)
    {
        var requestTime = DateTimeOffset.UtcNow;
        var result = await url
            .AppendPathSegment($"realms/{realm}/protocol/openid-connect/token")
            .WithHeader("Content-Type", "application/x-www-form-urlencoded")
            .PostUrlEncodedAsync(keyValues, cancellationToken: cancellationToken)
            .ReceiveJson<AccessTokenResponse>().ConfigureAwait(ConfigureAwaitOptions.None);

        if (result is null)
        {
            throw new ConflictException("result should never be null");
        }

        return new KeycloakAccessToken(result.AccessToken, requestTime.AddSeconds(result.ExpiresIn), requestTime.AddSeconds(result.RefreshExpiresIn), result.RefreshToken);
    }

    private record AccessTokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("expires_in")] int ExpiresIn,
        [property: JsonPropertyName("refresh_expires_in")] int RefreshExpiresIn,
        [property: JsonPropertyName("refresh_token")] string RefreshToken
    );
}
