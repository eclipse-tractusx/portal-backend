/********************************************************************************
 * Copyright (c) 2021, 2023 BMW Group AG
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

        var token = await this.GetTokenAsync(tokenParameters, cancellationToken).ConfigureAwait(false);

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
        var content = new FormUrlEncodedContent(formParameters);
        var response = await _httpClientFactory.CreateClient(settings.HttpClientName).PostAsync(settings.TokenUrl, content, cancellationToken)
            .CatchingIntoServiceExceptionFor("token-post", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE).ConfigureAwait(false);

        using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        var responseObject = await JsonSerializer.DeserializeAsync<AuthResponse>(responseStream, cancellationToken: cancellationToken).ConfigureAwait(false);
        return responseObject?.AccessToken;
    }
}
