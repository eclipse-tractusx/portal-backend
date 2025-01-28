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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.HttpClientExtensions;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Token;
using System.Net.Http.Json;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library;

public class BpdmAccessService(ITokenService tokenService, IOptions<BpdmAccessSettings> options)
    : IBpdmAccessService
{
    private readonly BpdmAccessSettings _settings = options.Value;
    private static readonly JsonSerializerOptions Options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task<BpdmLegalEntityDto> FetchLegalEntityByBpn(string businessPartnerNumber, CancellationToken cancellationToken)
    {
        using var httpClient = await tokenService.GetAuthorizedClient<BpdmAccessService>(_settings, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        var result = await httpClient.GetAsync($"legal-entities/{Uri.EscapeDataString(businessPartnerNumber)}?idType=BPN", cancellationToken)
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
