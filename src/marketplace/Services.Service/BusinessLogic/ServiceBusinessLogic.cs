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

using Microsoft.Extensions.Options;
using Org.CatenaX.Ng.Portal.Backend.Framework.ErrorHandling;
using Org.CatenaX.Ng.Portal.Backend.Framework.Models;
using Org.CatenaX.Ng.Portal.Backend.Offers.Library.Models;
using Org.CatenaX.Ng.Portal.Backend.Offers.Library.Service;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.CatenaX.Ng.Portal.Backend.Services.Service.ViewModels;

namespace Org.CatenaX.Ng.Portal.Backend.Services.Service.BusinessLogic;

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
    public Task<Pagination.Response<ServiceOverviewData>> GetAllActiveServicesAsync(int page, int size, ServiceOverviewSorting? sorting, ServiceTypeId? serviceTypeId) =>
        Pagination.CreateResponseAsync(
            page,
            size,
            _settings.ApplicationsMaxPageSize,
            _portalRepositories.GetInstance<IOfferRepository>().GetActiveServicesPaginationSource(sorting, serviceTypeId));

    /// <inheritdoc />
    public Task<Guid> CreateServiceOfferingAsync(ServiceOfferingData data, string iamUserId) =>
        _offerService.CreateServiceOfferingAsync(data, iamUserId, OfferTypeId.SERVICE);

    /// <inheritdoc />
    public Task<Guid> AddServiceSubscription(Guid serviceId, IEnumerable<OfferAgreementConsentData> offerAgreementConsentData, string iamUserId, string accessToken) =>
        _offerSubscriptionService.AddOfferSubscriptionAsync(serviceId, offerAgreementConsentData, iamUserId, accessToken, _settings.ServiceManagerRoles, OfferTypeId.SERVICE, _settings.BasePortalAddress);

    /// <inheritdoc />
    public async Task<ServiceDetailData> GetServiceDetailsAsync(Guid serviceId, string lang, string iamUserId)
    {        
        var serviceDetailData = await _portalRepositories.GetInstance<IOfferRepository>().GetServiceDetailByIdUntrackedAsync(serviceId, lang, iamUserId).ConfigureAwait(false);
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
        OfferAgreementConsentData offerAgreementConsentData, string iamUserId) =>
        _offerService.CreateOfferSubscriptionAgreementConsentAsync(subscriptionId, offerAgreementConsentData.AgreementId,
            offerAgreementConsentData.ConsentStatusId, iamUserId, OfferTypeId.SERVICE);

    /// <inheritdoc />
    public IAsyncEnumerable<AgreementData> GetServiceAgreement(Guid serviceId) => 
        _offerService.GetOfferAgreementsAsync(serviceId, OfferTypeId.SERVICE);

    /// <inheritdoc />
    public Task<ConsentDetailData> GetServiceConsentDetailDataAsync(Guid serviceConsentId) =>
        _offerService.GetConsentDetailDataAsync(serviceConsentId, OfferTypeId.SERVICE);

    /// <inheritdoc />
    public Task CreateOrUpdateServiceAgreementConsentAsync(Guid subscriptionId,
        IEnumerable<OfferAgreementConsentData> offerAgreementConsentData,
        string iamUserId) =>
        _offerService.CreateOrUpdateOfferSubscriptionAgreementConsentAsync(subscriptionId, offerAgreementConsentData, iamUserId, OfferTypeId.SERVICE);

    /// <inheritdoc />
    public Task<OfferAutoSetupResponseData> AutoSetupServiceAsync(OfferAutoSetupData data, string iamUserId) =>
        _offerService.AutoSetupServiceAsync(data, _settings.ServiceAccountRoles, _settings.CompanyAdminRoles, iamUserId, OfferTypeId.SERVICE, _settings.BasePortalAddress);

    /// <inheritdoc />
    public async Task UpdateServiceAsync(Guid serviceId, ServiceUpdateRequestData data, string iamUserId)
    {
        var serviceData = await _portalRepositories
            .GetInstance<IOfferRepository>()
            .GetServiceUpdateData(serviceId, data.ServiceTypeIds, iamUserId)
            .ConfigureAwait(false);
        if (serviceData is null)
        {
            throw new NotFoundException($"Service {serviceId} does not exists");
        }

        if (serviceData.OfferState != OfferStatusId.CREATED)
        {
            throw new ConflictException($"Service in State {serviceData.OfferState} can't be updated");
        }

        if (!serviceData.IsUserOfProvider)
        {
            throw new ForbiddenException($"User {iamUserId} is not allowed to change the service.");
        }

        await _offerService.ValidateSalesManager(data.SalesManager, iamUserId, _settings.SalesManagerRoles).ConfigureAwait(false);

        var offerRepository = _portalRepositories.GetInstance<IOfferRepository>();
        offerRepository.AttachAndModifyOffer(serviceId, offer =>
        {
            offer.Name = data.Title;
            offer.SalesManagerId = data.SalesManager;
            offer.ContactEmail = data.ContactEmail;
        });

        _offerService.UpsertRemoveOfferDescription(serviceId, data.Descriptions.Select(x => new Localization(x.LanguageCode, x.LongDescription, x.ShortDescription)), serviceData.Descriptions);
        _offerService.CreateOrUpdateOfferLicense(serviceId, data.Price, serviceData.OfferLicense);
        var newServiceTypes = data.ServiceTypeIds
            .Except(serviceData.ServiceTypeIds.Where(x => x.IsMatch).Select(x => x.ServiceTypeId))
            .Select(sti => (serviceId, sti));
        var serviceTypeIdsToRemove = serviceData.ServiceTypeIds
            .Where(x => !x.IsMatch)
            .Select(sti => (serviceId, sti.ServiceTypeId));
        UpdateAssignedServiceTypes(
            newServiceTypes, 
            serviceTypeIdsToRemove,
            offerRepository);

        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }
    
    private static void UpdateAssignedServiceTypes(IEnumerable<(Guid serviceId, ServiceTypeId serviceTypeId)> newServiceTypes, IEnumerable<(Guid serviceId, ServiceTypeId serviceTypeId)> serviceTypeIdsToRemove, IOfferRepository appRepository)
    {
        appRepository.AddServiceAssignedServiceTypes(newServiceTypes);
        appRepository.RemoveServiceAssignedServiceTypes(serviceTypeIdsToRemove);
    }

    /// <inheritdoc/>
    public Task<Pagination.Response<OfferCompanySubscriptionStatusData>> GetCompanyProvidedServiceSubscriptionStatusesForUserAsync(int page, int size, string iamUserId, SubscriptionStatusSorting? sorting, OfferSubscriptionStatusId? statusId) =>
        Pagination.CreateResponseAsync(page, size, _settings.ApplicationsMaxPageSize, _portalRepositories.GetInstance<IOfferSubscriptionsRepository>()
            .GetOwnCompanyProvidedOfferSubscriptionStatusesUntrackedAsync(iamUserId, OfferTypeId.SERVICE, sorting, statusId));

}
