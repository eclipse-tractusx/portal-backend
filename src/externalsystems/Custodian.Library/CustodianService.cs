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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Custodian.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.DateTimeProvider;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.HttpClientExtensions;
using Org.Eclipse.TractusX.Portal.Backend.Framework.IO;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Token;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Custodian.Library;

public class CustodianService : ICustodianService
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private readonly ITokenService _tokenService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly CustodianSettings _settings;

    public CustodianService(ITokenService tokenService, IDateTimeProvider dateTimeProvider, IOptions<CustodianSettings> settings)
    {
        _tokenService = tokenService;
        _dateTimeProvider = dateTimeProvider;
        _settings = settings.Value;
    }

    /// <inhertidoc />
    public async Task<WalletData> GetWalletByBpnAsync(string bpn, CancellationToken cancellationToken)
    {
        using var httpClient = await _tokenService.GetAuthorizedClient<CustodianService>(_settings, cancellationToken).ConfigureAwait(false);

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
        const string walletUrl = "/api/wallets";
        using var httpClient = await _tokenService.GetAuthorizedClient<CustodianService>(_settings, cancellationToken).ConfigureAwait(false);
        var requestBody = new { name = name, bpn = bpn };
        async ValueTask<(bool, string?)> CreateErrorMessage(HttpResponseMessage errorResponse) =>
            (false, (await errorResponse.Content.ReadFromJsonAsync<WalletErrorResponse>(Options, cancellationToken).ConfigureAwait(false))?.Message);

        var result = await httpClient.PostAsJsonAsync(walletUrl, requestBody, Options, cancellationToken)
            .CatchingIntoServiceExceptionFor("custodian-post", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE, CreateErrorMessage).ConfigureAwait(false);

        try
        {
            var walletResponse = await result.Content.ReadFromJsonAsync<WalletCreationResponse>(Options, cancellationToken).ConfigureAwait(false);
            if (walletResponse == null)
            {
                return "Service Response for custodian-post is null";
            }

            return JsonSerializer.Serialize(new WalletCreationLogData(walletResponse.Did, _dateTimeProvider.OffsetNow), Options);
        }
        catch (JsonException)
        {
            return "Service Response deSerialization failed for custodian-post";
        }
    }

    /// <inhertidoc />
    public async Task<string> SetMembership(string bpn, CancellationToken cancellationToken)
    {
        using var httpClient = await _tokenService.GetAuthorizedClient<CustodianService>(_settings, cancellationToken).ConfigureAwait(false);
        var requestBody = new { bpn = bpn };

        async ValueTask<(bool, string?)> CustomErrorHandling(HttpResponseMessage errorResponse) => (
            errorResponse.StatusCode == HttpStatusCode.Conflict &&
                (await errorResponse.Content.ReadFromJsonAsync<MembershipErrorResponse>(Options, cancellationToken).ConfigureAwait(false))?.Title.Trim() == _settings.MembershipErrorMessage,
            null);

        var result = await httpClient.PostAsJsonAsync("/api/credentials/issuer/membership", requestBody, Options, cancellationToken)
            .CatchingIntoServiceExceptionFor("custodian-membership-post",
                HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE, CustomErrorHandling).ConfigureAwait(false);

        return result.StatusCode == HttpStatusCode.Conflict ? $"{bpn} already has a membership" : "Membership Credential successfully created";
    }

    /// <inheritdoc />
    public async Task TriggerFrameworkAsync(string bpn, UseCaseDetailData useCaseDetailData, CancellationToken cancellationToken)
    {
        using var httpClient = await _tokenService.GetAuthorizedClient<CustodianService>(_settings, cancellationToken).ConfigureAwait(false);

        var requestBody = new CustodianFrameworkRequest
        (
            bpn,
            useCaseDetailData.VerifiedCredentialExternalTypeId,
            useCaseDetailData.Template,
            useCaseDetailData.Version
        );

        await httpClient.PostAsJsonAsync("/api/credentials/issuer/framework", requestBody, Options, cancellationToken)
            .CatchingIntoServiceExceptionFor("custodian-framework-post", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task TriggerDismantlerAsync(string bpn, VerifiedCredentialTypeId credentialTypeId, CancellationToken cancellationToken)
    {
        using var httpClient = await _tokenService.GetAuthorizedClient<CustodianService>(_settings, cancellationToken).ConfigureAwait(false);

        var requestBody = new CustodianDismantlerRequest
        (
            bpn,
            credentialTypeId
        );

        await httpClient.PostAsJsonAsync("/api/credentials/issuer/dismantler", requestBody, Options, cancellationToken)
            .CatchingIntoServiceExceptionFor("custodian-dismantler-post", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE).ConfigureAwait(false);
    }
}
