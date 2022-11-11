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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Service;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Services.Service.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Org.Eclipse.TractusX.Portal.Backend.Services.Service.BusinessLogic;

/// <summary>
/// Implementation of <see cref="IServiceBusinessLogic"/>.
/// </summary>
public class ServiceBusinessLogic : IServiceBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly IOfferService _offerService;
    private readonly IOfferSubscriptionService _offerSubscriptionService;
    private readonly ServiceSettings _settings;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalRepositories">Factory to access the repositories</param>
    /// <param name="offerService">Access to the offer service</param>
    /// <param name="offerSubscriptionService">Service for Company to manage offer subscriptions</param>
    /// <param name="settings">Access to the settings</param>
    public ServiceBusinessLogic(
        IPortalRepositories portalRepositories,
        IOfferService offerService,
        IOfferSubscriptionService offerSubscriptionService,
        IOptions<ServiceSettings> settings)
    {
        _portalRepositories = portalRepositories;
        _offerService = offerService;
        _offerSubscriptionService = offerSubscriptionService;
        _settings = settings.Value;
    }

    /// <inheritdoc />
    public Task<Pagination.Response<ServiceOverviewData>> GetAllActiveServicesAsync(int page, int size)
    {
        var services = _portalRepositories.GetInstance<IOfferRepository>().GetActiveServices();
        return Pagination.CreateResponseAsync(
            page,
            size,
            _settings.ApplicationsMaxPageSize,
            (skip, take) => new Pagination.AsyncSource<ServiceOverviewData>(
                services.CountAsync(),
                services
                    .Skip(skip)
                    .Take(take)
                    .Select(s =>
                        new ServiceOverviewData(
                            s.id,
                            s.name ?? Constants.ErrorString,
                            s.provider,
                            s.thumbnailUrl ?? Constants.ErrorString,
                            s.contactEmail,
                            null,
                            s.price ?? Constants.ErrorString))
                    .AsAsyncEnumerable()));
    }

    /// <inheritdoc />
    public Task<Guid> CreateServiceOfferingAsync(OfferingData data, string iamUserId) =>
        _offerService.CreateServiceOfferingAsync(data, iamUserId, OfferTypeId.SERVICE);

    /// <inheritdoc />
    public Task<Guid> AddServiceSubscription(Guid serviceId, string iamUserId, string accessToken) =>
        _offerSubscriptionService.AddServiceSubscription(serviceId, iamUserId, accessToken, OfferTypeId.SERVICE);

    /// <inheritdoc />
    public async Task<OfferDetailData> GetServiceDetailsAsync(Guid serviceId, string lang, string iamUserId)
    {        
        var serviceDetailData = await _portalRepositories.GetInstance<IOfferRepository>().GetOfferDetailByIdUntrackedAsync(serviceId, lang, iamUserId, OfferTypeId.SERVICE).ConfigureAwait(false);
        if (serviceDetailData == default)
        {
            throw new NotFoundException($"Service {serviceId} does not exist");
        }

        return serviceDetailData;
    }

    /// <inheritdoc />
    public async Task<SubscriptionDetailData> GetSubscriptionDetailAsync(Guid subscriptionId, string iamUserId)
    {
        var subscriptionDetailData = await _portalRepositories.GetInstance<IOfferSubscriptionsRepository>()
            .GetSubscriptionDetailDataForOwnUserAsync(subscriptionId, iamUserId, OfferTypeId.SERVICE).ConfigureAwait(false);
        if (subscriptionDetailData is null)
        {
            throw new NotFoundException($"Subscription {subscriptionId} does not exist");
        }

        return subscriptionDetailData;
    }

    /// <inheritdoc />
    public Task<Guid> CreateServiceAgreementConsentAsync(Guid subscriptionId,
        ServiceAgreementConsentData serviceAgreementConsentData, string iamUserId) =>
        _offerService.CreateOfferSubscriptionAgreementConsentAsync(subscriptionId, serviceAgreementConsentData.AgreementId,
            serviceAgreementConsentData.ConsentStatusId, iamUserId, OfferTypeId.SERVICE);

    /// <inheritdoc />
    public IAsyncEnumerable<AgreementData> GetServiceAgreement(Guid serviceId) => 
        _offerService.GetOfferAgreementsAsync(serviceId, OfferTypeId.SERVICE);

    /// <inheritdoc />
    public Task<ConsentDetailData> GetServiceConsentDetailDataAsync(Guid serviceConsentId) =>
        _offerService.GetConsentDetailDataAsync(serviceConsentId, OfferTypeId.SERVICE);

    /// <inheritdoc />
    public Task CreateOrUpdateServiceAgreementConsentAsync(Guid subscriptionId,
        IEnumerable<ServiceAgreementConsentData> serviceAgreementConsentDatas,
        string iamUserId) =>
        _offerService.CreateOrUpdateOfferSubscriptionAgreementConsentAsync(subscriptionId, serviceAgreementConsentDatas, iamUserId, OfferTypeId.SERVICE);

    /// <inheritdoc />
    public Task<OfferAutoSetupResponseData> AutoSetupServiceAsync(OfferAutoSetupData data, string iamUserId) =>
        _offerService.AutoSetupServiceAsync(data, _settings.ServiceAccountRoles, _settings.CompanyAdminRoles, iamUserId, OfferTypeId.SERVICE);
}
