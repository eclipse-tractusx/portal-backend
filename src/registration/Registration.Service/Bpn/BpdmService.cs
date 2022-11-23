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
using System.Runtime.CompilerServices;
using System.Text.Json;
using Org.CatenaX.Ng.Portal.Backend.Framework.ErrorHandling;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.CatenaX.Ng.Portal.Backend.Registration.Service.Bpn.Model;

namespace Org.CatenaX.Ng.Portal.Backend.Registration.Service.Bpn;

public class BpdmService : IBpdmService
{
    private readonly HttpClient _httpClient;

    public BpdmService(IHttpClientFactory httpFactory)
    {
        _httpClient = httpFactory.CreateClient(nameof(BpnAccess));
    }

    /// <inheritdoc />
    public async Task<bool> TriggerBpnDataPush(BpdmData data, string accessToken, CancellationToken cancellationToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        try
        {
            BpdmLegalEntityData requestData = new BpdmLegalEntityData(
                data.Bpn,
                data.Bpn,
                new []
                {
                    new BpdmIdentifiers("not yet available", "not yet available", null, null)
                },
                new []
                {
                    new BpdmName(data.CompanyName, null, "REGISTERED", "de")
                },
                null,
                null,
                new []
                {
                    new BpdmClassification("", "", "")
                },
                new []
                {
                    "ORGANIZATIONAL_UNIT"
                },
                new List<BpdmBankAccounts>(),
                new BpdmAddress(
                    new BpdmAddressVersion("WESTERN_LATIN_STANDARD", "de"),
                    null,
                    new List<string>(),
                    data.AlphaCode2,
                    new List<BpdmAministrativeArea>(),
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
                    },
                    new List<BpdmPremises>(),
                    new List<BpdmPostalDeliveryPoints>(),
                    new List<BpdmGeoCoordinates>(),
                    new List<string>()
                )
                );
            var result = await _httpClient.PutAsJsonAsync("api/catena/input/legal-entities", new {}, cancellationToken).ConfigureAwait(false);
            return result.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            return false;
        }
    }
}