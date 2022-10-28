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

using Org.CatenaX.Ng.Portal.Backend.Registration.Service.BPN.Model;
using Org.CatenaX.Ng.Portal.Backend.Framework.ErrorHandling;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Org.CatenaX.Ng.Portal.Backend.Registration.Service.BPN;

public class BpnAccess : IBpnAccess
{
    private readonly HttpClient _httpClient;

    public BpnAccess(IHttpClientFactory httpFactory)
    {
        _httpClient = httpFactory.CreateClient("bpn");
    }

    public async IAsyncEnumerable<FetchBusinessPartnerDto> FetchBusinessPartner(string bpn, string token, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var response = new List<FetchBusinessPartnerDto>();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var result = await _httpClient.GetAsync($"api/catena/business-partner/{bpn}").ConfigureAwait(false);
        if (result.IsSuccessStatusCode)
        {
            using var responseStream = await result.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            var businesPartnerDto = await JsonSerializer.DeserializeAsync<FetchBusinessPartnerDto>(responseStream, cancellationToken: cancellationToken).ConfigureAwait(false);
            if(businesPartnerDto != null)
            {
                yield return businesPartnerDto;
            }
        }
        else
        {
            throw new ServiceException($"Access to BPN Failed with Status Code {result.StatusCode}", result.StatusCode);
        }
    }
}
