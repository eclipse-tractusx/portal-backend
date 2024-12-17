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
    public static async Task<KeycloakAccessToken> GetAccessToken(this KeycloakAccessToken? token, Url url, string realm, string? userName, string? password, string? clientSecret, string clientId, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        if (token != null && token.ExpiryTime > now)
        {
            return token;
        }

        var accessTokenResponse = await (token is null
                ? GetToken()
                : RefreshToken()).ConfigureAwait(ConfigureAwaitOptions.None) ?? throw new ConflictException("accessTokenResponse should never be null");

        return new KeycloakAccessToken(accessTokenResponse.AccessToken, now.AddSeconds(accessTokenResponse.ExpiresIn), accessTokenResponse.RefreshToken, now.AddSeconds(accessTokenResponse.RefreshExpiresIn));

        Task<AccessTokenResponse> GetToken()
        {
            if (clientSecret != null)
            {
                return RetrieveToken([
                    new("grant_type", "client_credentials"),
                    new("client_secret", clientSecret),
                    new("client_id", clientId)
                ]);
            }

            if (userName != null)
            {
                return RetrieveToken([
                    new("grant_type", "password"),
                    new("username", userName),
                    new("password", password ?? ""),
                    new("client_id", "admin-cli")
                ]);
            }

            throw new ArgumentException($"{nameof(userName)} and {nameof(clientSecret)} must not all be null");
        }

        Task<AccessTokenResponse> RefreshToken() =>
            token.RefreshExpiryTime > now
                ? RetrieveToken([
                    new("grant_type", "refresh_token"),
                    new("refresh_token", token.RefreshToken),
                    new("client_id", clientId)
                ])
                : GetToken();

        Task<AccessTokenResponse> RetrieveToken(IEnumerable<KeyValuePair<string, string>> keyValues) =>
            url
                .AppendPathSegments("realms", Url.Encode(realm), "protocol/openid-connect/token")
                .WithHeader("Content-Type", "application/x-www-form-urlencoded")
                .PostUrlEncodedAsync(keyValues, cancellationToken: cancellationToken)
                .ReceiveJson<AccessTokenResponse>();
    }

    private sealed record AccessTokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("expires_in")] int ExpiresIn,
        [property: JsonPropertyName("refresh_token")] string RefreshToken,
        [property: JsonPropertyName("refresh_expires_in")] int RefreshExpiresIn
    );
}
