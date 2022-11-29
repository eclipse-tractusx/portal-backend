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

using Org.CatenaX.Ng.Portal.Backend.Framework.ErrorHandling;
using Org.CatenaX.Ng.Portal.Backend.Framework.IO;
using System.Net.Http.Headers;

namespace Org.CatenaX.Ng.Portal.Backend.Administration.Service.BusinessLogic;

public class DapsService : IDapsService
{
    private const string BaseSecurityProfile = "BASE_SECURITY_PROFILE";
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Creates a new instance of <see cref="DapsService"/>
    /// </summary>
    /// <param name="httpClientFactory">Factory to create httpClients</param>
    public DapsService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient(nameof(DapsService));
    }

    /// <inheritdoc />
    public Task<bool> EnableDapsAuthAsync(string clientName, string accessToken, string connectorUrl, string businessPartnerNumber, IFormFile formFile, CancellationToken cancellationToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        UrlHelper.ValidateHttpUrl(connectorUrl, () => nameof(connectorUrl));

        return HandleRequest(clientName, connectorUrl, businessPartnerNumber, formFile, cancellationToken);
    }

    private async Task<bool> HandleRequest(string clientName, string connectorUrl, string businessPartnerNumber,
        IFormFile formFile, CancellationToken cancellationToken)
    {
        try
        {
            using var stream = formFile.OpenReadStream();

            var multiPartStream = new MultipartFormDataContent();
            multiPartStream.Add(new StreamContent(stream), "file", formFile.FileName);
            multiPartStream.Add(new StringContent(clientName), "clientName");
            multiPartStream.Add(new StringContent(BaseSecurityProfile), "securityProfile");
            multiPartStream.Add(new StringContent(UrlHelper.AppendToPathEncoded(connectorUrl, businessPartnerNumber)), "referringConnector");

            var response = await _httpClient.PostAsync(string.Empty, multiPartStream, cancellationToken)
                .ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                throw new ServiceException("Daps Service Call failed", response.StatusCode);
            }

            return true;
        }
        catch (Exception ex)
        {
            throw new ServiceException("Daps Service Call failed", ex);
        }
    }
}
