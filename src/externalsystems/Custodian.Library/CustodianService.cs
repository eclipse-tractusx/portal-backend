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
using Org.Eclipse.TractusX.Portal.Backend.Framework.IO;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Token;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Custodian.Library;

public class CustodianService : ICustodianService
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
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

        var result = await httpClient.GetAsync("/api/wallets".AppendToPathEncoded(bpn), cancellationToken)
            .CatchingIntoServiceExceptionFor("custodian-get", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE).ConfigureAwait(false);

        try
        {
            var walletData = await result.Content.ReadFromJsonAsync<WalletData>(Options, cancellationToken).ConfigureAwait(false);

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

        async ValueTask<(bool, string?)> CreateErrorMessage(HttpResponseMessage errorResponse) =>
            (false, (await errorResponse.Content.ReadFromJsonAsync<WalletErrorResponse>(Options, cancellationToken).ConfigureAwait(false))?.Message);

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

        async ValueTask<(bool, string?)> CustomErrorHandling(HttpResponseMessage errorResponse) => (
            errorResponse.StatusCode == HttpStatusCode.Conflict &&
                (await errorResponse.Content.ReadFromJsonAsync<MembershipErrorResponse>(Options, cancellationToken).ConfigureAwait(false))?.Title.Trim() == _settings.MembershipErrorMessage,
            null);

        var result = await httpClient.PostAsync("/api/credentials/issuer/membership", stringContent, cancellationToken)
            .CatchingIntoServiceExceptionFor("custodian-membership-post",
                HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE, CustomErrorHandling).ConfigureAwait(false);

        return result.StatusCode == HttpStatusCode.Conflict ? $"{bpn} already has a membership" : "Membership Credential successfully created";
    }
}
