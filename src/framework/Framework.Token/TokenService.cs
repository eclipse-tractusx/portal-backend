/********************************************************************************
 * Copyright (c) 2021, 2023 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.Framework.HttpClientExtensions;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Token;

public class TokenService : ITokenService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public TokenService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<HttpClient> GetAuthorizedClient<T>(KeyVaultAuthSettings settings, CancellationToken cancellationToken)
    {
        var tokenParameters = new GetTokenSettings(
            $"{typeof(T).Name}Auth",
            settings.Username,
            settings.Password,
            settings.ClientId,
            settings.GrantType,
            settings.ClientSecret,
            settings.Scope,
            settings.TokenAddress);

        var token = await GetTokenAsync(tokenParameters, cancellationToken).ConfigureAwait(false);

        var httpClient = _httpClientFactory.CreateClient(typeof(T).Name);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return httpClient;
    }

    private async Task<string?> GetTokenAsync(GetTokenSettings settings, CancellationToken cancellationToken)
    {
        var formParameters = new Dictionary<string, string>
        {
            {"username", settings.Username},
            {"password", settings.Password},
            {"client_id", settings.ClientId},
            {"grant_type", settings.GrantType},
            {"client_secret", settings.ClientSecret},
            {"scope", settings.Scope}
        };
        using var content = new FormUrlEncodedContent(formParameters);
        using var httpClient = _httpClientFactory.CreateClient(settings.HttpClientName);
        using var response = await httpClient.PostAsync(settings.TokenUrl, content, cancellationToken)
            .CatchingIntoServiceExceptionFor("token-post", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE).ConfigureAwait(false);

        using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        var responseObject = await JsonSerializer.DeserializeAsync<AuthResponse>(responseStream, cancellationToken: cancellationToken).ConfigureAwait(false);
        return responseObject?.AccessToken;
    }

    public async Task<HttpClient> GetBasicAuthorizedClient<T>(BasicAuthSettings settings, CancellationToken cancellationToken)
    {
        var tokenParameters = new GetBasicTokenSettings(
            $"{typeof(T).Name}Auth",
            settings.ClientId,
            settings.ClientSecret,
            settings.TokenAddress,
            settings.GrantType);

        var token = await GetBasicTokenAsync(tokenParameters, cancellationToken).ConfigureAwait(false);

        var httpClient = _httpClientFactory.CreateClient(typeof(T).Name);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return httpClient;
    }

    private async Task<string?> GetBasicTokenAsync(GetBasicTokenSettings settings, CancellationToken cancellationToken)
    {
        var formParameters = new Dictionary<string, string>
        {
            { "grant_type", settings.GrantType }
        };
        var content = new FormUrlEncodedContent(formParameters);
        var authClient = _httpClientFactory.CreateClient(settings.HttpClientName);
        var authenticationString = $"{settings.ClientId}:{settings.ClientSecret}";
        var base64String = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(authenticationString));

        authClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64String);

        var response = await authClient.PostAsync(settings.TokenAddress, content, cancellationToken)
            .CatchingIntoServiceExceptionFor("token-post", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE).ConfigureAwait(false);

        using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        var responseObject = await JsonSerializer.DeserializeAsync<BasicAuthResponse>(responseStream, cancellationToken: cancellationToken).ConfigureAwait(false);
        return responseObject?.AccessToken;
    }
}
