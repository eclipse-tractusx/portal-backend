/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
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

using Microsoft.Extensions.Logging;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Service;
   
public class OfferSetupService : IOfferSetupService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<OfferSetupService> _logger;

    public OfferSetupService(IHttpClientFactory httpClientFactory, ILogger<OfferSetupService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }
    
    /// <inheritdoc />
    public async Task AutoSetupOffer(OfferThirdPartyAutoSetupData autoSetupData, string iamUserId, string accessToken, string serviceDetailsAutoSetupUrl)
    {
        _logger.LogInformation("AutoSetup started");
        using var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        try
        {
            // TODO: Remove the autosetupdata from logging after testing.
            _logger.LogInformation("OfferSetupService was called with the following url: {ServiceDetailsAutoSetupUrl} and following data: {AutoSetupData}", serviceDetailsAutoSetupUrl, JsonSerializer.Serialize(autoSetupData));
            var response = await httpClient.PostAsJsonAsync(serviceDetailsAutoSetupUrl, autoSetupData).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                throw new ServiceException(
                    response.ReasonPhrase ?? $"Request failed with StatusCode: {response.StatusCode} and Message: {await response.Content.ReadAsStringAsync()}",
                    response.StatusCode);

            _logger.LogInformation("OfferSetupService AutoSetup was successfully executed.");
        }
        catch (InvalidOperationException e)
        {
            throw new ServiceException("The requestUri must be an absolute URI or BaseAddress must be set.", e);
        }
        catch (HttpRequestException e)
        {
            throw new ServiceException("The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.", e);
        }
        catch (TaskCanceledException e)
        {
            throw new ServiceException("The request failed due to timeout.", e);
        }
        catch (Exception e)
        {
            throw new ServiceException("Request failed", e);
        }
    }
}
