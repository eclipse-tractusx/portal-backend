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

using System.Text.Json;
using Org.CatenaX.Ng.Portal.Backend.Framework.ErrorHandling;
using Org.CatenaX.Ng.Portal.Backend.Framework.Models;
using Org.CatenaX.Ng.Portal.Backend.Offers.Library.Models;
using Org.CatenaX.Ng.Portal.Backend.Offers.Library.Service;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.CatenaX.Ng.Portal.Backend.Services.Service.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Org.CatenaX.Ng.Portal.Backend.Services.Service.BusinessLogic;

/// <summary>
/// Implementation of <see cref="IServiceBusinessLogic"/>.
/// </summary>
public class ServiceBusinessLogic : IServiceBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly IOfferSetupService _offerSetupService;
    private readonly IOfferService _offerService;
    private readonly ServiceSettings _settings;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalRepositories">Factory to access the repositories</param>
    /// <param name="offerSetupService">SetupService for the 3rd Party Service Provider</param>
    /// <param name="offerService">Access to the offer service</param>
    /// <param name="settings">Access to the settings</param>
    public ServiceBusinessLogic(
        IPortalRepositories portalRepositories, 
        IOfferSetupService offerSetupService, 
        IOfferService offerService, 
        IOptions<ServiceSettings> settings)
    {
        _portalRepositories = portalRepositories;
        _offerSetupService = offerSetupService;
        _offerService = offerService;
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
    public async Task<Guid> AddServiceSubscription(Guid serviceId, string iamUserId)
    {
        var serviceDetails = await _portalRepositories.GetInstance<IOfferRepository>().GetOfferProviderDetailsAsync(serviceId, OfferTypeId.SERVICE).ConfigureAwait(false);
        if (serviceDetails == null)
        {
            throw new NotFoundException($"Service {serviceId} does not exist");
        }

        var (companyId, companyUserId, companyName, requesterEmail) = await _portalRepositories.GetInstance<IUserRepository>().GetOwnCompanAndCompanyUseryIdWithCompanyNameAndUserEmailAsync(iamUserId).ConfigureAwait(false);
        if (companyId == Guid.Empty)
        {
            throw new ControllerArgumentException($"User {iamUserId} has no company assigned", nameof(iamUserId));
        }
        
        if (companyUserId == Guid.Empty)
        {
            throw new ControllerArgumentException($"User {iamUserId} has no company user assigned", nameof(iamUserId));
        }

        var offerSubscription = _portalRepositories.GetInstance<IOfferSubscriptionsRepository>().CreateOfferSubscription(serviceId, companyId, OfferSubscriptionStatusId.PENDING, companyUserId, companyUserId);
        var autoSetupResult = string.Empty;
        if (!string.IsNullOrWhiteSpace(serviceDetails.AutoSetupUrl))
        {
            try
            {
                await _offerSetupService.AutoSetupOffer(offerSubscription.Id, iamUserId, serviceDetails.AutoSetupUrl).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                autoSetupResult = e.Message;
            }
        }

        if (serviceDetails.SalesManagerId.HasValue)
        {
            var notificationContent = new
            {
                serviceDetails.AppName,
                RequestorCompanyName = companyName,
                UserEmail = requesterEmail,
                AutoSetupExecuted = !string.IsNullOrWhiteSpace(serviceDetails.AutoSetupUrl),
                AutoSetupError = autoSetupResult
            };
            _portalRepositories.GetInstance<INotificationRepository>().CreateNotification(serviceDetails.SalesManagerId.Value, NotificationTypeId.APP_SUBSCRIPTION_REQUEST, false,
                notification =>
                {
                    notification.CreatorUserId = companyUserId;
                    notification.Content = JsonSerializer.Serialize(notificationContent);
                });
        }

        await _portalRepositories.SaveAsync().ConfigureAwait(false);
        return offerSubscription.Id;
    }

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
        _offerService.AutoSetupServiceAsync(data, _settings.ServiceAccountRoles, _settings.CompanyAdminRoles, iamUserId, OfferTypeId.APP);
}
