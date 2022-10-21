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
using System.Text;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Custodian;

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
        var stringContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var result = await _custodianHttpClient.PostAsync($"/api/wallets", stringContent, cancellationToken);
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
        var result = await _custodianHttpClient.GetAsync("/api/wallets").ConfigureAwait(false);
        if (result.IsSuccessStatusCode)
        {
            var wallets = JsonSerializer.Deserialize<List<GetWallets>>(await result.Content.ReadAsStringAsync().ConfigureAwait(false));
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
        var parameters = new Dictionary<string, string>();
        parameters.Add("username", _settings.Username);
        parameters.Add("password", _settings.Password);
        parameters.Add("client_id", _settings.ClientId);
        parameters.Add("grant_type", _settings.GrantType);
        parameters.Add("client_secret", _settings.ClientSecret);
        parameters.Add("scope", _settings.Scope);
        var content = new FormUrlEncodedContent(parameters);
        var response = await _custodianAuthHttpClient.PostAsync("", content);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Token could not be retrieved");
        }
        var responseObject = JsonSerializer.Deserialize<AuthResponse>(await response.Content.ReadAsStringAsync());
        return responseObject?.access_token;
    }
}
