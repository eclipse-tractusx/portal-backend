/********************************************************************************
 * Copyright (c) 2021, 2023 BMW Group AG
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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Token;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library.Models;
using System.Net.Http.Json;
using System.Security.Cryptography;

namespace Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library;

/// <summary>
/// Service to handle communication with the connectors sd factory
/// </summary>
public class SdFactoryService : ISdFactoryService
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly ITokenService _tokenService;
    private readonly SdFactorySettings _settings;

    /// <summary>
    /// Creates a new instance of <see cref="SdFactoryService"/>
    /// </summary>
    /// <param name="tokenService">Access to the token service</param>
    /// <param name="options">The options</param>
    /// <param name="portalRepositories">Access to the portalRepositories</param>
    public SdFactoryService(
        IPortalRepositories portalRepositories,
        ITokenService tokenService,
        IOptions<SdFactorySettings> options)
    {
        _settings = options.Value;
        _portalRepositories = portalRepositories;
        _tokenService = tokenService;
    }

    /// <inheritdoc />
    public async Task<Guid> RegisterConnectorAsync(string connectorUrl, string businessPartnerNumber, CancellationToken cancellationToken)
    {
        var httpClient = await _tokenService.GetAuthorizedClient<SdFactoryService>(_settings, cancellationToken)
            .ConfigureAwait(false);
        // TODO The hardcoded values (headquarterCountry, legalCountry, sdType, issuer) will be fetched from the user input or db in future
        var requestModel = new ConnectorSdFactoryRequestModel(
            SdFactoryRequestModelSdType.ServiceOffering,
            connectorUrl,
            string.Empty,
            string.Empty,
            string.Empty,
            _settings.SdFactoryIssuerBpn,
            businessPartnerNumber);

        var response = await httpClient.PostAsJsonAsync((string?)null, requestModel, cancellationToken).ConfigureAwait(false);

        return await ProcessResponse(SdFactoryResponseModelTitle.Connector, response, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Guid> RegisterSelfDescriptionAsync(IEnumerable<(UniqueIdentifierId Id, string Value)> uniqueIdentifiers, string countryCode, string businessPartnerNumber, CancellationToken cancellationToken)
    {
        var httpClient = await _tokenService.GetAuthorizedClient<SdFactoryService>(_settings, cancellationToken)
            .ConfigureAwait(false);
        var requestModel = new SdFactoryRequestModel(
            uniqueIdentifiers.Select(x => new RegistrationNumber(x.Id.GetSdUniqueIdentifierValue(), x.Value)),
            countryCode,
            countryCode,
            SdFactoryRequestModelSdType.LegalPerson,
            businessPartnerNumber,
            businessPartnerNumber,
            _settings.SdFactoryIssuerBpn);

        var response = await httpClient.PostAsJsonAsync((string?)null, requestModel, cancellationToken).ConfigureAwait(false);

        return await ProcessResponse(SdFactoryResponseModelTitle.LegalPerson, response, cancellationToken).ConfigureAwait(false);
    }

    private async Task<Guid> ProcessResponse(SdFactoryResponseModelTitle docTitle, HttpResponseMessage response, CancellationToken cancellationToken)
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

        var document = _portalRepositories.GetInstance<IDocumentRepository>().CreateDocument(
            $"SelfDescription_{docTitle}.json", 
            documentContent, 
            hash, 
            DocumentTypeId.SELF_DESCRIPTION,
            doc =>
            {
                doc.DocumentStatusId = DocumentStatusId.LOCKED;
            });

        await _portalRepositories.SaveAsync().ConfigureAwait(false);
        return document.Id;
    }
}
