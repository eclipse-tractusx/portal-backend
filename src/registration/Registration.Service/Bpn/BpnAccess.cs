/********************************************************************************
 * Copyright (c) 2021, 2023 Microsoft and BMW Group AG
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

using Org.Eclipse.TractusX.Portal.Backend.Framework.Async;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Bpn.Model;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text.Json;

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

    public async Task<BpdmLegalEntityDto> FetchLegalEntityByBpn(string businessPartnerNumber, string token, CancellationToken cancellationToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var uri = new UriBuilder()
        {
            Path = $"api/catena/legal-entities/{Uri.EscapeDataString(businessPartnerNumber)}",
            Query = "idType=BPN"
        }.Uri;
        var result = await _httpClient.GetAsync(uri.PathAndQuery, cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccessStatusCode)
        {
            throw new ServiceException($"Access to external system bpdm failed with Status Code {result.StatusCode}", result.StatusCode);
        }
        await using var responseStream = await result.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var legalEntityResponse = await JsonSerializer.DeserializeAsync<BpdmLegalEntityDto>(
                responseStream,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase },
                cancellationToken: cancellationToken).ConfigureAwait(false);
            if (legalEntityResponse?.Bpn == null)
            {
                throw new ServiceException("Access to external system bpdm did not return a valid legal entity response");
            }
            return legalEntityResponse;
        }
        catch(JsonException je)
        {
            throw new ServiceException($"Access to external system bpdm did not return a valid json response: {je.Message}");
        }
    }

    public async IAsyncEnumerable<BpdmLegalEntityAddressDto> FetchLegalEntityAddressByBpn(string businessPartnerNumber, string token, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var uri = new UriBuilder()
        {
            Path = "api/catena/legal-entities/legal-addresses/search"
        }.Uri;
        var json = new [] { businessPartnerNumber };
        var result = await _httpClient.PostAsJsonAsync(uri.PathAndQuery, json, cancellationToken).ConfigureAwait(false);
        if (result.IsSuccessStatusCode)
        {
            await using var responseStream = await result.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

            await foreach (var address in JsonSerializer
                    .DeserializeAsyncEnumerable<BpdmLegalEntityAddressDto>(
                        responseStream,
                        new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase },
                        cancellationToken: cancellationToken)
                    .CatchingAsync(
                        ex => {
                            throw new ServiceException($"Access to external system bpdm did not return a valid json response: {ex.Message}");
                        },
                        cancellationToken)
                    .ConfigureAwait(false))
            {
                if (address?.LegalAddress == null || address.LegalEntity == null)
                {
                    throw new ServiceException("Access to external system bpdm did not return a valid legal entity address response");
                }
                yield return address;
            }
        }
        else
        {
            throw new ServiceException($"Access to external system bpdm failed with Status Code {result.StatusCode}", result.StatusCode);
        }
    }
}
