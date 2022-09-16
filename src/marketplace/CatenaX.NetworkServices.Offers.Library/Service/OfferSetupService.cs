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

using System.Net.Http.Json;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using Microsoft.Extensions.Logging;

namespace CatenaX.NetworkServices.Offers.Library.Service;
   
public class OfferSetupService : IOfferSetupService
{
    private readonly ILogger _logger;

    public OfferSetupService(ILogger logger)
    {
        _logger = logger;
    }
    
    /// <inheritdoc />
    public async Task<bool> AutoSetupOffer(Guid serviceId, string serviceDetailsAutoSetupUrl)
    {
        var httpClient = new HttpClient();
        
        var requestModel = new OfferAutoSetupData(new CustomerData("", "", ""), new PropertyData("", new Guid(), new Guid()));
        var response = await httpClient.PostAsJsonAsync(serviceDetailsAutoSetupUrl, requestModel).ConfigureAwait(false);

        return response.IsSuccessStatusCode;
    }
}
