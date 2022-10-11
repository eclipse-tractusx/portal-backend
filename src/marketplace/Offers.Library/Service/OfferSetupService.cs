/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
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
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Org.CatenaX.Ng.Portal.Backend.Framework.ErrorHandling;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Repositories;

namespace Org.CatenaX.Ng.Portal.Backend.Offers.Library.Service;
   
public class OfferSetupService : IOfferSetupService
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<OfferSetupService> _logger;

    public OfferSetupService(IPortalRepositories portalRepositories, IHttpClientFactory httpClientFactory, ILogger<OfferSetupService> logger)
    {
        _portalRepositories = portalRepositories;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }
    
    /// <inheritdoc />
    public async Task AutoSetupOffer(Guid serviceSubscriptionId, string iamUserId, string accessToken, string serviceDetailsAutoSetupUrl)
    {
        _logger.LogInformation("AutoSetup started");
        using var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var result = await _portalRepositories.GetInstance<IOfferSubscriptionsRepository>().GetThirdPartyAutoSetupDataAsync(serviceSubscriptionId, iamUserId).ConfigureAwait(false);
        if (result == default)
        {
            throw new NotFoundException($"serviceSubscription {serviceSubscriptionId} does not exist");
        }
        var (autoSetupData, isUsersCompany) = result;
        if (!isUsersCompany)
        {
            throw new ForbiddenException($"IamUser {iamUserId} company is not associated with serviceSubscription");
        }
        if (autoSetupData.OfferThirdPartyAutoSetupProperties.BpnNumber == null)
        {
            throw new ConflictException($"company {autoSetupData.OfferThirdPartyAutoSetupCustomer.OrganizationName} has no BusinessPartnerNumber assigned");
        }

        try
        {
            _logger.LogInformation("OfferSetupService was called with the following url: {serviceDetailsAutoSetupUrl}", serviceDetailsAutoSetupUrl);
            var response = await httpClient.PostAsJsonAsync(serviceDetailsAutoSetupUrl, autoSetupData).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                throw new ServiceException(
                    response.ReasonPhrase ?? $"Request failed with StatusCode: {response.StatusCode}",
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
