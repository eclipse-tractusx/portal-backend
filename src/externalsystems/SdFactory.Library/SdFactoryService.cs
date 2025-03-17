/********************************************************************************
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Framework.HttpClientExtensions;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Token;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library.Models;
using System.Net.Http.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library;

/// <summary>
/// Service to handle communication with the connectors sd factory
/// </summary>
public class SdFactoryService(ITokenService tokenService, IOptions<SdFactorySettings> options) : ISdFactoryService
{
    private readonly SdFactorySettings _settings = options.Value;

    /// <inheritdoc />
    public async Task RegisterConnectorAsync(Guid connectorId, string selfDescriptionDocumentUrl, string businessPartnerNumber, CancellationToken cancellationToken)
    {
        var httpClient = await tokenService.GetAuthorizedClient<SdFactoryService>(_settings, cancellationToken)
            .ConfigureAwait(ConfigureAwaitOptions.None);
        var requestModel = new ConnectorSdFactoryRequestModel(
            connectorId.ToString(),
            SdFactoryRequestModelSdType.ServiceOffering,
            selfDescriptionDocumentUrl,
            businessPartnerNumber);

        await httpClient.PostAsJsonAsync(default(string?), requestModel, cancellationToken)
            .CatchingIntoServiceExceptionFor("sd-factory-connector-post", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task RegisterSelfDescriptionAsync(Guid externalId, string legalName, IEnumerable<(UniqueIdentifierId Id, string Value)> uniqueIdentifiers, string countryCode, string region, string businessPartnerNumber, CancellationToken cancellationToken)
    {
        var httpClient = await tokenService.GetAuthorizedClient<SdFactoryService>(_settings, cancellationToken)
            .ConfigureAwait(ConfigureAwaitOptions.None);
        var countrySubdivisionCode = string.Format("{0}-{1}", countryCode, region);
        var requestModel = new SdFactoryRequestModel(
            externalId.ToString(),
            legalName,
            uniqueIdentifiers.Select(x => new RegistrationNumber(x.Id.GetSdUniqueIdentifierValue(), x.Value.GetUniqueIdentifierValue(x.Id, countryCode))),
            countrySubdivisionCode,
            countrySubdivisionCode,
            SdFactoryRequestModelSdType.LegalParticipant,
            businessPartnerNumber);

        await httpClient.PostAsJsonAsync(default(string?), requestModel, cancellationToken)
            .CatchingIntoServiceExceptionFor("sd-factory-selfdescription-post", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE).ConfigureAwait(false);
    }
}
