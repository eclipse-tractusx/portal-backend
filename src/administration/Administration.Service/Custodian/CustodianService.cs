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

using Org.CatenaX.Ng.Portal.Backend.Administration.Service.Custodian.Models;
using Org.CatenaX.Ng.Portal.Backend.Framework.ErrorHandling;

using Microsoft.Extensions.Options;

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Org.CatenaX.Ng.Portal.Backend.Administration.Service.Custodian;

public class CustodianService : ICustodianService
{
    private readonly CustodianSettings _settings;
    private readonly HttpClient _custodianHttpClient;
    private readonly HttpClient _custodianAuthHttpClient;
    private readonly ILogger<CustodianService> _logger;

    public CustodianService(ILogger<CustodianService> logger, IHttpClientFactory httpFactory, IOptions<CustodianSettings> settings)
    {
        _settings = settings.Value;
        _custodianHttpClient = httpFactory.CreateClient("custodian");
        _custodianAuthHttpClient = httpFactory.CreateClient("custodianAuth");
        _logger = logger;
    }


    public async Task CreateWallet(string bpn, string name, CancellationToken cancellationToken)
    {
        var token = await GetToken();
        _custodianHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var requestBody = new { name = name, bpn = bpn };
        var json = JsonSerializer.Serialize(requestBody);
        var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
        const string walletUrl = "/api/wallets";

        _logger.LogDebug("CreateWallet was called with the following url: {Url} and following data: {Data}", walletUrl, json);
        var result = await _custodianHttpClient.PostAsync(walletUrl, stringContent, cancellationToken);
        _logger.LogDebug("Responded with StatusCode: {StatusCode} and the following content {Content}", result.StatusCode, await result.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));

        if (!result.IsSuccessStatusCode)
        {
            _logger.LogError($"Error on creating Wallet HTTP Response Code {result.StatusCode}");
            throw new ServiceException($"Access to Custodian Failed with Status Code {result.StatusCode}", result.StatusCode);
        }
    }

    public async Task<List<GetWallets>> GetWallets()
    {
        var response = new List<GetWallets>();
        var token = await GetToken().ConfigureAwait(false);
        _custodianHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        const string url = "/api/wallets";
        _logger.LogDebug("GetWallets was called with the following url: {Url}", url);
        var result = await _custodianHttpClient.GetAsync(url).ConfigureAwait(false);
        
        var responseContent = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
        _logger.LogDebug("Responded with StatusCode: {StatusCode} and the following content {Content}", result.StatusCode, responseContent);

        if (result.IsSuccessStatusCode)
        {
            var wallets = JsonSerializer.Deserialize<List<GetWallets>>(responseContent);
            if (wallets != null)
            {
                response.AddRange(wallets);
            }
        }
        else
        {
            _logger.LogInformation($"Error on retrieveing Wallets HTTP Response Code {result.StatusCode}");
        }
        return response;
    }

    public async Task<string?> GetToken()
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
        _logger.LogDebug("GetToken for Wallet was called with the following data: {Data}", JsonSerializer.Serialize(parameters));
        var response = await _custodianAuthHttpClient.PostAsync("", content);
        var responseContent = await response.Content.ReadAsStringAsync();
        _logger.LogDebug("Responded with StatusCode: {StatusCode} and the following content {Content}", response.StatusCode, responseContent);
        if (!response.IsSuccessStatusCode)
        {
            throw new ServiceException("Token could not be retrieved");
        }
        var responseObject = JsonSerializer.Deserialize<AuthResponse>(responseContent);
        return responseObject?.access_token;
    }
}
