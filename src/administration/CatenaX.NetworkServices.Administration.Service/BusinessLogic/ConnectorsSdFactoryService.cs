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
using System.Security.Cryptography;
using CatenaX.NetworkServices.Administration.Service.Models;
using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using Microsoft.Extensions.Options;

namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic;

/// <summary>
/// Service to handle communication with the connectors sd factory
/// </summary>
public class ConnectorsSdFactoryService : IConnectorsSdFactoryService
{
    private readonly ConnectorsSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IPortalRepositories _portalRepositories;

    /// <summary>
    /// Creates a new instance of <see cref="ConnectorsSdFactoryService"/>
    /// </summary>
    /// <param name="options">The options</param>
    /// <param name="httpClientFactory">Factory to create httpClients</param>
    /// <param name="portalRepositories">Access to the portal Repositories</param>
    public ConnectorsSdFactoryService(IOptions<ConnectorsSettings> options, IHttpClientFactory httpClientFactory, IPortalRepositories portalRepositories)
    {
        _httpClientFactory = httpClientFactory;
        _portalRepositories = portalRepositories;
        _settings = options.Value;
    }

    /// <inheritdoc />
    public async Task RegisterConnectorAsync(ConnectorInputModel connectorInputModel, string accessToken, string bpn, Guid companyUserId)
    {
        using var httpClient =_httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        // The hardcoded values (headquarterCountry, legalCountry, sdType, issuer) will be fetched from the user input or db in future
        var requestModel = new ConnectorSdFactoryRequestModel(bpn, "DE", "DE", connectorInputModel.ConnectorUrl,
            "connector", bpn, bpn, "BPNL000000000000");
        var response = await httpClient.PostAsJsonAsync(_settings.SdFactoryUrl, requestModel).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw new ServiceException($"Access to SD factory failed with status code {response.StatusCode}", response.StatusCode);
        }
        
        var content = System.Text.Encoding.UTF8.GetBytes(await response.Content.ReadAsStringAsync());
        using var sha512Hash = SHA512.Create();
        using var ms = new MemoryStream(content, 0 , content.Length);
        var hash = await sha512Hash.ComputeHashAsync(ms);
        var documentContent = ms.GetBuffer();
        if (ms.Length != content.Length || documentContent.Length != content.Length)
        {
            throw new ControllerArgumentException($"document transmitted length {content.Length} doesn't match actual length {ms.Length}.");
        }
        
        _portalRepositories.GetInstance<IDocumentRepository>().CreateDocument(companyUserId, $"SelfDescription_{bpn}.json", documentContent, hash, DocumentTypeId.SELF_DESCRIPTION_EDC);
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }
}
