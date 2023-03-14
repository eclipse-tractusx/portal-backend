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

using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Service;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Services.Service.ViewModels;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Services.Service.BusinessLogic;

/// <summary>
/// Implementation of <see cref="IServiceReleaseBusinessLogic"/>.
/// </summary>
public class ServiceReleaseBusinessLogic : IServiceReleaseBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly IOfferService _offerService;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalRepositories">Factory to access the repositories</param>
    /// <param name="offerService">Access to the offer service</param>
    public ServiceReleaseBusinessLogic(
        IPortalRepositories portalRepositories,
        IOfferService offerService)
    {
        _portalRepositories = portalRepositories;
        _offerService = offerService;
    }

    public IAsyncEnumerable<AgreementDocumentData> GetServiceAgreementDataAsync()=>
        _offerService.GetOfferTypeAgreementsAsync(OfferTypeId.SERVICE);

    /// <inheritdoc />
    public async Task<ServiceData> GetServiceDetailsByIdAsync(Guid serviceId)
    {
        var result = await _portalRepositories.GetInstance<IOfferRepository>()
            .GetServiceDetailsByIdAsync(serviceId).ConfigureAwait(false);
        if (result == null)
        {
            throw new NotFoundException($"serviceId {serviceId} does not exist");
        }
        if (result.OfferStatusId != OfferStatusId.IN_REVIEW)
        {
            throw new ConflictException($"serviceId {serviceId} is incorrect status");
        }
        return new ServiceData(
            result.Id,
            result.Title ?? Constants.ErrorString,
            result.ServiceTypeIds,
            result.Provider,
            result.Descriptions.Select(x => new LocalizedDescription(x.languageCode, x.longDescription, x.shortDescription)),
            result.Documents.GroupBy(d => d.documentTypeId).ToDictionary(g => g.Key, g => g.Select(d => new DocumentData(d.documentId, d.documentName))),
            result.ProviderUri ?? Constants.ErrorString,
            result.ContactEmail,
            result.ContactNumber
        );
    }
    
    /// <inheritdoc />
    public IAsyncEnumerable<ServiceTypeData> GetServiceTypeDataAsync()=>
        _portalRepositories.GetInstance<IStaticDataRepository>().GetServiceTypeData();
}
