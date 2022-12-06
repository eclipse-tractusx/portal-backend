/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
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

using Org.CatenaX.Ng.Portal.Backend.Framework.ErrorHandling;
using System.Text.Json;

namespace Org.CatenaX.Ng.Portal.Backend.Framework.Token;

public class TokenService : ITokenService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public TokenService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<string?> GetTokenAsync(GetTokenSettings settings, CancellationToken cancellationToken)
    {
        try
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
            var response = await _httpClientFactory.CreateClient(settings.HttpClientName).PostAsync("", content, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                throw new ServiceException($"Get Token Call for {settings.HttpClientName} was not successful", response.StatusCode);
            }

            using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            var responseObject = await JsonSerializer.DeserializeAsync<AuthResponse>(responseStream, cancellationToken: cancellationToken).ConfigureAwait(false);
            return responseObject?.AccessToken;
        }
        catch (Exception ex)
        {
            if (ex is ServiceException)
            {
                throw;
            }
            throw new ServiceException($"Get Token Call for {settings.HttpClientName} threw exception", ex);
        }
    }
}
