/********************************************************************************
 * Copyright (c) 2021,2022 Microsoft and BMW Group AG
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

using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Org.CatenaX.Ng.Portal.Backend.Administration.Service.Custodian.Models;
using Org.CatenaX.Ng.Portal.Backend.Administration.Service.Models;
using Org.CatenaX.Ng.Portal.Backend.Framework.ErrorHandling;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;

namespace Org.CatenaX.Ng.Portal.Backend.Administration.Service.BusinessLogic;

public class BpdmService : IBpdmService
{
    private readonly HttpClient _httpClient;
    private readonly HttpClient _authHttpClient;
    private readonly BpdmServiceSettings _settings;

    public BpdmService(IHttpClientFactory httpFactory, IOptions<BpdmServiceSettings> options)
    {
        _httpClient = httpFactory.CreateClient(nameof(BpdmService));
        _authHttpClient = httpFactory.CreateClient($"{nameof(BpdmService)}Auth");
        _settings = options.Value;
    }

    /// <inheritdoc />
    public async Task<bool> TriggerBpnDataPush(BpdmData data, CancellationToken cancellationToken)
    {
        var token = await GetTokenAsync(cancellationToken).ConfigureAwait(false);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        try
        {
            var requestData = new BpdmLegalEntityData(
                "registrationnumber",
                new []
                {
                    new BpdmIdentifiers("DE216038746", "EU_VAT_ID_DE")
                },
                new []
                {
                    new BpdmName(data.CompanyName, "REGISTERED", "de")
                },
                
                new BpdmAddress(
                    new BpdmAddressVersion("WESTERN_LATIN_STANDARD", "de"),
                    data.AlphaCode2,
                    new []
                    {
                        new BpdmPostcode(data.ZipCode, "REGULAR")
                    },
                    new []
                    {
                        new BpdmLocality(data.City, null, "CITY")
                    },
                    new []
                    {
                        new BpdmThoroughfares(data.Street, null, null, null, null, "STREET")
                    })
                );
            var result = await _httpClient.PutAsJsonAsync("api/catena/input/legal-entities", requestData, cancellationToken).ConfigureAwait(false);
            if (result.IsSuccessStatusCode)
                return true;

            throw new ServiceException("Bpdm Service Call failed with StatusCode", result.StatusCode);
        }
        catch (Exception ex)
        {
            throw new ServiceException("Bpdm Service Call failed.", ex);
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
        var response = await _authHttpClient.PostAsync("", content, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw new ServiceException("Token could not be retrieved");
        }

        using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        var responseObject = await JsonSerializer.DeserializeAsync<AuthResponse>(responseStream, cancellationToken: cancellationToken).ConfigureAwait(false);
        return responseObject?.access_token;
    }
}