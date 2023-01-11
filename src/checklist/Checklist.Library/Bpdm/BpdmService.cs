/********************************************************************************
 * Copyright (c) 2021,2022 Microsoft and BMW Group AG
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
using Org.Eclipse.TractusX.Portal.Backend.Checklist.Library.Bpdm.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Token;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Checklist.Library.Bpdm;

public class BpdmService : IBpdmService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ITokenService _tokenService;
    private readonly BpdmServiceSettings _settings;

    public BpdmService(IHttpClientFactory httpClientFactory, ITokenService tokenService, IOptions<BpdmServiceSettings> options)
    {
        _httpClientFactory = httpClientFactory;
        _tokenService = tokenService;
        _settings = options.Value;
    }

    /// <inheritdoc />
    public async Task<bool> TriggerBpnDataPush(BpdmTransferData data, CancellationToken cancellationToken)
    {
        var httpClient = await GetBpdmHttpClient(cancellationToken).ConfigureAwait(false);

        try
        {
            var requestData = new BpdmLegalEntityData[]
            {
                new(
                    "registrationnumber",
                    new[]
                    {
                        new BpdmIdentifiers("DE216038746", "EU_VAT_ID_DE")
                    },
                    new[]
                    {
                        new BpdmName(data.CompanyName, "REGISTERED", "de")
                    },

                    new BpdmAddress(
                        new BpdmAddressVersion("WESTERN_LATIN_STANDARD", "de"),
                        data.AlphaCode2,
                        new[]
                        {
                            new BpdmPostcode(data.ZipCode, "REGULAR")
                        },
                        new[]
                        {
                            new BpdmLocality(data.City, "CITY")
                        },
                        new[]
                        {
                            new BpdmThoroughfares(data.Street, "STREET")
                        })
                )
            };
        
            var result = await httpClient.PutAsJsonAsync("api/catena/input/legal-entities", requestData, cancellationToken).ConfigureAwait(false);
            if (result.IsSuccessStatusCode)
                return true;

            throw new ServiceException("Bpdm Service Call failed with StatusCode", result.StatusCode);
        }
        catch (Exception ex)
        {
            if (ex is ServiceException)
            {
                throw;
            }
            throw new ServiceException("Bpdm Service Call failed.", ex);
        }
    }

    private async Task<HttpClient> GetBpdmHttpClient(CancellationToken cancellationToken)
    {
        var tokenParameters = new GetTokenSettings(
                $"{nameof(BpdmService)}Auth",
                _settings.Username,
                _settings.Password,
                _settings.ClientId,
                _settings.GrantType,
                _settings.ClientSecret,
                _settings.Scope);

        var token = await _tokenService.GetTokenAsync(tokenParameters, cancellationToken).ConfigureAwait(false);

        var httpClient = _httpClientFactory.CreateClient(nameof(BpdmService));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return httpClient;
    }
}
