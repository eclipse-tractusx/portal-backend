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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

/// <summary>
/// Service to handle communication with the connectors sd factory
/// </summary>
public class SdFactoryService : ISdFactoryService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IPortalRepositories _portalRepositories;
    private readonly ILogger<SdFactoryService> _logger;
    private readonly SdFactorySettings _settings;

    /// <summary>
    /// Creates a new instance of <see cref="SdFactoryService"/>
    /// </summary>
    /// <param name="httpClientFactory">Factory to create httpClients</param>
    /// <param name="options">The options</param>
    /// <param name="portalRepositories">Access to the portalRepositories</param>
    /// <param name="logger"></param>
    public SdFactoryService(IOptions<SdFactorySettings> options, IHttpClientFactory httpClientFactory,
        IPortalRepositories portalRepositories, ILogger<SdFactoryService> logger)
    {
        _settings = options.Value;
        _httpClientFactory = httpClientFactory;
        _portalRepositories = portalRepositories;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Guid> RegisterConnectorAsync(ConnectorInputModel connectorInputModel, string accessToken, string businessPartnerNumber, CancellationToken cancellationToken)
    {
        using var httpClient =_httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        // The hardcoded values (headquarterCountry, legalCountry, sdType, issuer) will be fetched from the user input or db in future
        var requestModel = new ConnectorSdFactoryRequestModel(
            "ServiceOffering",
            connectorInputModel.ConnectorUrl,
            string.Empty,
            string.Empty,
            string.Empty,
            _settings.SdFactoryIssuerBpn,
            businessPartnerNumber);

        var response = await httpClient.PostAsJsonAsync(_settings.SdFactoryUrl, requestModel, cancellationToken).ConfigureAwait(false);

        return await ProcessResponse(businessPartnerNumber, response, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Guid> RegisterSelfDescriptionAsync(string accessToken, Guid applicationId, string countryCode, string businessPartnerNumber, CancellationToken cancellationToken)
    {
        using var httpClient =_httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var requestModel = new SdFactoryRequestModel(
            applicationId.ToString(),
            countryCode,
            countryCode,
            SdFactoryRequestModelSdType.LegalPerson,
            businessPartnerNumber,
            businessPartnerNumber,
            _settings.SdFactoryIssuerBpn);

        // TODO: Please remove after testing
        _logger.LogInformation("SdFactory RegisterSelfDescriptionAsync was called with the following url: {ServiceDetailsAutoSetupUrl} and following data: {AutoSetupData}", _settings.SdFactoryUrl, JsonSerializer.Serialize(requestModel));
        var response = await httpClient.PostAsJsonAsync(_settings.SdFactoryUrl, requestModel, cancellationToken).ConfigureAwait(false);

        return await ProcessResponse(businessPartnerNumber, response, cancellationToken).ConfigureAwait(false);
    }

    private async Task<Guid> ProcessResponse(string businessPartnerNumber, HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            throw new ServiceException($"Access to SD factory failed with status code {response.StatusCode}",
                response.StatusCode);
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var sha512Hash = SHA512.Create();
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms, cancellationToken);
        var hash = await sha512Hash.ComputeHashAsync(ms, cancellationToken);
        var documentContent = ms.GetBuffer();
        if (ms.Length != stream.Length || documentContent.Length != stream.Length)
        {
            throw new ControllerArgumentException(
                $"document transmitted length {stream.Length} doesn't match actual length {ms.Length}.");
        }

        var document = _portalRepositories.GetInstance<IDocumentRepository>().CreateDocument($"SelfDescription_{businessPartnerNumber}.json", documentContent, hash, DocumentTypeId.SELF_DESCRIPTION_EDC, null);

        await _portalRepositories.SaveAsync().ConfigureAwait(false);
        return document.Id;
    }
}
