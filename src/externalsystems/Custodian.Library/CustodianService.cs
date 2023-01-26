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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Checklist.Library.Custodian.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Token;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Custodian.Library;

public class CustodianService : ICustodianService
{
    private readonly ITokenService _tokenService;
    private readonly CustodianSettings _settings;

    public CustodianService(ITokenService tokenService, IOptions<CustodianSettings> settings)
    {
        _tokenService = tokenService;
        _settings = settings.Value;
    }

    /// <inhertidoc />
    public async Task<string> CreateWalletAsync(string bpn, string name, CancellationToken cancellationToken)
    {
        var httpClient = await _tokenService.GetAuthorizedClient<CustodianService>(_settings, cancellationToken).ConfigureAwait(false);

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

    /// <inhertidoc />
    public async Task<WalletData> GetWalletByBpnAsync(string bpn, CancellationToken cancellationToken)
    {
        var httpClient = await _tokenService.GetAuthorizedClient<CustodianService>(_settings, cancellationToken).ConfigureAwait(false);

        var result = await httpClient.GetAsync($"/api/wallets/{bpn}", cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccessStatusCode)
        {
            throw new ServiceException("Error on retrieving Wallets HTTP Response Code {StatusCode}",
                result.StatusCode);
        }

        using var responseStream = await result.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var walletData = await JsonSerializer
                .DeserializeAsync<WalletData>(responseStream, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            if (walletData == null)
            {
                throw new ServiceException("Couldn't resolve wallet data from the service");
            }

            return walletData;
        }
        catch (JsonException)
        {
            throw new ServiceException("Couldn't resolve wallet data");
        }
    }
}
