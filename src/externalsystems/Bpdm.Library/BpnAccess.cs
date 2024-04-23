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

using Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.HttpClientExtensions;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library;

public class BpnAccess : IBpnAccess
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions Options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public BpnAccess(IHttpClientFactory httpFactory)
    {
        _httpClient = httpFactory.CreateClient(nameof(BpnAccess));
    }

    public async Task<BpdmLegalEntityDto> FetchLegalEntityByBpn(string businessPartnerNumber, string token, CancellationToken cancellationToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var uri = new UriBuilder
        {
            Path = $"legal-entities/{Uri.EscapeDataString(businessPartnerNumber)}",
            Query = "idType=BPN"
        }.Uri;
        var result = await _httpClient.GetAsync(uri.PathAndQuery.TrimStart('/'), cancellationToken)
            .CatchingIntoServiceExceptionFor("bpn-fetch-legal-entity")
            .ConfigureAwait(false);
        try
        {
            var legalEntityResponse = await result.Content.ReadFromJsonAsync<BpdmLegalEntityDto>(Options, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
            if (legalEntityResponse?.Bpn == null)
            {
                throw new ServiceException("Access to external system bpdm did not return a valid legal entity response");
            }
            return legalEntityResponse;
        }
        catch (JsonException je)
        {
            throw new ServiceException($"Access to external system bpdm did not return a valid json response: {je.Message}");
        }
    }
}
