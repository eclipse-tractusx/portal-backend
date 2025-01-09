/********************************************************************************
 * Copyright (c) 2023 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Token;
using Org.Eclipse.TractusX.Portal.Backend.OfferProvider.Library.Models;
using System.Net.Http.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.OfferProvider.Library;

public class OfferProviderService : IOfferProviderService
{
    private readonly ITokenService _tokenService;

    /// <summary>
    /// Creates a new instance of <see cref="OfferProviderService"/>
    /// </summary>
    /// <param name="tokenService"></param>
    /// <param name="options"></param>
    public OfferProviderService(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    /// <inheritdoc />
    public async Task<bool> TriggerOfferProvider(OfferThirdPartyAutoSetupData autoSetupData, string autoSetupUrl, string authUrl, string clientId, string clientSecret, CancellationToken cancellationToken)
    {
        var settings = new KeyVaultAuthSettings
        {
            TokenAddress = authUrl,
            ClientId = clientId,
            ClientSecret = clientSecret
        };
        using var httpClient = await _tokenService.GetAuthorizedClient<OfferProviderService>(settings, cancellationToken)
            .ConfigureAwait(ConfigureAwaitOptions.None);
        await httpClient.PostAsJsonAsync(autoSetupUrl, autoSetupData, cancellationToken)
            .CatchingIntoServiceExceptionFor("trigger-offer-provider")
            .ConfigureAwait(false);

        return true;
    }

    /// <inheritdoc />
    public async Task<bool> TriggerOfferProviderCallback(OfferProviderCallbackData callbackData, string callbackUrl, string authUrl, string clientId, string clientSecret, CancellationToken cancellationToken)
    {
        var settings = new KeyVaultAuthSettings
        {
            TokenAddress = authUrl,
            ClientId = clientId,
            ClientSecret = clientSecret
        };
        using var httpClient = await _tokenService.GetAuthorizedClient<OfferProviderService>(settings, cancellationToken)
            .ConfigureAwait(ConfigureAwaitOptions.None);
        await httpClient.PostAsJsonAsync(callbackUrl, callbackData, cancellationToken)
            .CatchingIntoServiceExceptionFor("trigger-offer-provider-callback")
            .ConfigureAwait(false);

        return true;
    }
}
