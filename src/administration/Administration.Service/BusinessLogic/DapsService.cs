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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.IO;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Token;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

public class DapsService : IDapsService
{
    private const string BaseSecurityProfile = "BASE_SECURITY_PROFILE";
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ITokenService _tokenService;
    private readonly DapsSettings _settings;

    /// <summary>
    /// Creates a new instance of <see cref="DapsService"/>
    /// </summary>
    /// <param name="httpClientFactory">Factory to create httpClients</param>
    /// <param name="tokenService"></param>
    /// <param name="options"></param>
    public DapsService(IHttpClientFactory httpClientFactory, ITokenService tokenService, IOptions<DapsSettings> options)
    {
        _httpClientFactory = httpClientFactory;
        _tokenService = tokenService;
        _settings = options.Value;
    }

    /// <inheritdoc />
    public Task<bool> EnableDapsAuthAsync(string clientName, string connectorUrl, string businessPartnerNumber, IFormFile formFile, CancellationToken cancellationToken)
    {
        UrlHelper.ValidateHttpUrl(connectorUrl, () => nameof(connectorUrl));
        return HandleRequest(clientName, connectorUrl, businessPartnerNumber, formFile, cancellationToken);
    }

    private async Task<bool> HandleRequest(string clientName, string connectorUrl, string businessPartnerNumber,
        IFormFile formFile, CancellationToken cancellationToken)
    {
        var httpClient = await GetDapsHttpClient(cancellationToken).ConfigureAwait(false);

        try
        {
            using var stream = formFile.OpenReadStream();

            var multiPartStream = new MultipartFormDataContent();
            multiPartStream.Add(new StreamContent(stream), "file", formFile.FileName);
            multiPartStream.Add(new StringContent(clientName), "clientName");
            multiPartStream.Add(new StringContent(BaseSecurityProfile), "securityProfile");
            multiPartStream.Add(new StringContent(UrlHelper.AppendToPathEncoded(connectorUrl, businessPartnerNumber)), "referringConnector");

            var response = await httpClient.PostAsync(string.Empty, multiPartStream, cancellationToken)
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
    
    private async Task<HttpClient> GetDapsHttpClient(CancellationToken cancellationToken)
    {
        var tokenParameters = new GetTokenSettings(
            $"{nameof(DapsService)}Auth",
            _settings.Username,
            _settings.Password,
            _settings.ClientId,
            _settings.GrantType,
            _settings.ClientSecret,
            _settings.Scope);

        var token = await _tokenService.GetTokenAsync(tokenParameters, cancellationToken).ConfigureAwait(false);

        var httpClient = _httpClientFactory.CreateClient(nameof(DapsService));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return httpClient;
    }
}
