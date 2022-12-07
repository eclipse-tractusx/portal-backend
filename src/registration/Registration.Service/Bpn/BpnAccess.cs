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

using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Bpn.Model;

namespace Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Bpn;

public class BpnAccess : IBpnAccess
{
    private readonly HttpClient _httpClient;

    public BpnAccess(IHttpClientFactory httpFactory)
    {
        _httpClient = httpFactory.CreateClient(nameof(BpnAccess));
    }

    public async IAsyncEnumerable<FetchBusinessPartnerDto> FetchBusinessPartner(string bpn, string token, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var result = await _httpClient.GetAsync($"api/catena/business-partner/{Uri.EscapeDataString(bpn)}", cancellationToken).ConfigureAwait(false);
        if (result.IsSuccessStatusCode)
        {
            await using var responseStream = await result.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            var businesPartnerDto = await JsonSerializer.DeserializeAsync<FetchBusinessPartnerDto>(responseStream, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (businesPartnerDto != null)
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
