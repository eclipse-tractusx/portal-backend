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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Service;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.Services.Service.ViewModels;

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
        _offerService.GetOfferTypeAgreements(OfferTypeId.SERVICE);

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
            result.Descriptions,
            result.Documents.GroupBy(d => d.documentTypeId).ToDictionary(g => g.Key, g => g.Select(d => new DocumentData(d.documentId, d.documentName))),
            result.ProviderUri ?? Constants.ErrorString,
            result.ContactEmail,
            result.ContactNumber
        );
    }

    /// <inheritdoc />
    public IAsyncEnumerable<ServiceTypeData> GetServiceTypeDataAsync()=>
        _portalRepositories.GetInstance<IStaticDataRepository>().GetServiceTypeData();

    /// <inheritdoc/>
    public Task<OfferAgreementConsent> GetServiceAgreementConsentAsync(Guid serviceId, string iamUserId) => 
        _offerService.GetProviderOfferAgreementConsentById(serviceId,  iamUserId, OfferTypeId.SERVICE);

    public async Task<ServiceProviderResponse> GetServiceDetailsForStatusAsync(Guid serviceId, string userId)
    {
        var result = await _offerService.GetProviderOfferDetailsForStatusAsync(serviceId, userId, OfferTypeId.SERVICE).ConfigureAwait(false);
        if (result.ServiceTypeIds == null)
        {
            throw new UnexpectedConditionException("serviceTypeIds should never be null here");
        }
        return new ServiceProviderResponse(
            result.Title,
            result.LeadPictureId,
            result.Descriptions,
            result.Agreements,
            result.Price,
            result.Images,
            result.ProviderUri,
            result.ContactEmail,
            result.ContactNumber,
            result.Documents,
            result.SalesManagerId,
            result.ServiceTypeIds);
    }
}
