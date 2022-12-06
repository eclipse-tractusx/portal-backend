/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Custodian.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;

using Microsoft.Extensions.Options;

using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Custodian;

public class CustodianService : ICustodianService
{
    private readonly CustodianSettings _settings;
    private readonly HttpClient _custodianHttpClient;
    private readonly HttpClient _custodianAuthHttpClient;

    public CustodianService(IHttpClientFactory httpFactory, IOptions<CustodianSettings> settings)
    {
        _settings = settings.Value;
        _custodianHttpClient = httpFactory.CreateClient("custodian");
        _custodianAuthHttpClient = httpFactory.CreateClient("custodianAuth");
    }


    public async Task CreateWalletAsync(string bpn, string name, CancellationToken cancellationToken)
    {
        var token = await GetTokenAsync(cancellationToken).ConfigureAwait(false);
        _custodianHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var requestBody = new { name = name, bpn = bpn };
        var json = JsonSerializer.Serialize(requestBody);
        var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
        const string walletUrl = "/api/wallets";

        var result = await _custodianHttpClient.PostAsync(walletUrl, stringContent, cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccessStatusCode)
        {
            throw new ServiceException("Access to Custodian Failed with Status Code {StatusCode}", result.StatusCode);
        }
    }

    public async IAsyncEnumerable<GetWallets> GetWalletsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var token = await GetTokenAsync(cancellationToken).ConfigureAwait(false);
        _custodianHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        const string url = "/api/wallets";
        var result = await _custodianHttpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
        
        if (result.IsSuccessStatusCode)
        {
            using var responseStream = await result.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            await foreach (var wallet in JsonSerializer.DeserializeAsyncEnumerable<GetWallets>(responseStream, cancellationToken: cancellationToken).ConfigureAwait(false))
            {
                if (wallet != null)
                {
                    yield return wallet;
                }
            }
        }
        else
        {
            throw new ServiceException("Error on retrieving Wallets HTTP Response Code {StatusCode}", result.StatusCode);
        }
    }

    public async Task<string?> GetTokenAsync(CancellationToken cancellationToken)
    {
        var parameters = new Dictionary<string, string>
        {
            {"username", _settings.Username},
            {"password", _settings.Password},
            {"client_id", _settings.ClientId},
            {"grant_type", _settings.GrantType},
            {"client_secret", _settings.ClientSecret},
            {"scope", _settings.Scope}
        };
        var content = new FormUrlEncodedContent(parameters);
        var response = await _custodianAuthHttpClient.PostAsync("", content, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw new ServiceException("Token could not be retrieved");
        }
        using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        var responseObject = await JsonSerializer.DeserializeAsync<AuthResponse>(responseStream, cancellationToken: cancellationToken).ConfigureAwait(false);
        return responseObject?.access_token;
    }
}
