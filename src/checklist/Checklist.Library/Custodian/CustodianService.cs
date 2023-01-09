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

using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Checklist.Library.Custodian.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Token;

namespace Org.Eclipse.TractusX.Portal.Backend.Checklist.Library.Custodian;

public class CustodianService : ICustodianService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ITokenService _tokenService;
    private readonly CustodianSettings _settings;

    public CustodianService(IHttpClientFactory httpClientFactory, ITokenService tokenService, IOptions<CustodianSettings> settings)
    {
        _httpClientFactory = httpClientFactory;
        _tokenService = tokenService;
        _settings = settings.Value;
    }

    public async Task<string> CreateWalletAsync(string bpn, string name, CancellationToken cancellationToken)
    {
        var httpClient = await GetCustodianHttpClient(cancellationToken).ConfigureAwait(false);

        var requestBody = new { name = name, bpn = bpn };
        var json = JsonSerializer.Serialize(requestBody);
        var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
        const string walletUrl = "/api/wallets";

        var result = await httpClient.PostAsync(walletUrl, stringContent, cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccessStatusCode)
        {
            throw new ServiceException("Access to Custodian Failed with Status Code {StatusCode}", result.StatusCode);
        }

        return await result.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    }

    public async IAsyncEnumerable<GetWallets> GetWalletsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var httpClient = await GetCustodianHttpClient(cancellationToken).ConfigureAwait(false);

        const string url = "/api/wallets";
        var result = await httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
        
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

    private async Task<HttpClient> GetCustodianHttpClient(CancellationToken cancellationToken)
    {
        var tokenParameters = new GetTokenSettings(
                "custodianAuth",
                _settings.Username,
                _settings.Password,
                _settings.ClientId,
                _settings.GrantType,
                _settings.ClientSecret,
                _settings.Scope);

        var token = await _tokenService.GetTokenAsync(tokenParameters, cancellationToken).ConfigureAwait(false);

        var httpClient = _httpClientFactory.CreateClient("custodian");
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return httpClient;
    }
}