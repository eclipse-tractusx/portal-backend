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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Custodian.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.HttpClientExtensions;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Token;
using System.Text;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Custodian.Library;

public class CustodianService : ICustodianService
{
    private static readonly JsonSerializerOptions _options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private readonly ITokenService _tokenService;
    private readonly CustodianSettings _settings;

    public CustodianService(ITokenService tokenService, IOptions<CustodianSettings> settings)
    {
        _tokenService = tokenService;
        _settings = settings.Value;
    }

    /// <inhertidoc />
    public async Task<WalletData> GetWalletByBpnAsync(string bpn, CancellationToken cancellationToken)
    {
        var httpClient = await _tokenService.GetAuthorizedClient<CustodianService>(_settings, cancellationToken).ConfigureAwait(false);

        var result = await httpClient.GetAsync($"/api/wallets/{bpn}", cancellationToken)
            .CatchingIntoServiceExceptionFor("custodian-get", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE).ConfigureAwait(false);

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

    /// <inhertidoc />
    public async Task<string> CreateWalletAsync(string bpn, string name, CancellationToken cancellationToken)
    {
        var httpClient = await _tokenService.GetAuthorizedClient<CustodianService>(_settings, cancellationToken).ConfigureAwait(false);

        var requestBody = new { name = name, bpn = bpn };
        var json = JsonSerializer.Serialize(requestBody);
        var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
        const string walletUrl = "/api/wallets";

        async ValueTask<string?> CreateErrorMessage(HttpContent errorContent) =>
            (await JsonSerializer.DeserializeAsync<WalletErrorResponse>(errorContent.ReadAsStream(cancellationToken), _options, cancellationToken).ConfigureAwait(false))?.Message;

        var result = await httpClient.PostAsync(walletUrl, stringContent, cancellationToken)
            .CatchingIntoServiceExceptionFor("custodian-post", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE, CreateErrorMessage).ConfigureAwait(false);

        return await result.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inhertidoc />
    public async Task<string> SetMembership(string bpn, CancellationToken cancellationToken)
    {
        var httpClient = await _tokenService.GetAuthorizedClient<CustodianService>(_settings, cancellationToken).ConfigureAwait(false);

        var requestBody = new { bpn = bpn };
        var json = JsonSerializer.Serialize(requestBody);
        var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

        await httpClient.PostAsync("/api/credentials/issuer/membership", stringContent, cancellationToken)
            .CatchingIntoServiceExceptionFor("custodian-membership-post", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE).ConfigureAwait(false);

        return "Membership Credential successfully created";
    }
}
