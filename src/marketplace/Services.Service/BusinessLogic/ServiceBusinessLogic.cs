/********************************************************************************
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
using Offers.Library.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Service;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;
using Org.Eclipse.TractusX.Portal.Backend.Services.Service.ViewModels;
using System.Text.RegularExpressions;

namespace Org.Eclipse.TractusX.Portal.Backend.Services.Service.BusinessLogic;

/// <summary>
/// Implementation of <see cref="IServiceBusinessLogic"/>.
/// </summary>
public class ServiceBusinessLogic : IServiceBusinessLogic
{
    private static readonly Regex Company = new(ValidationExpressions.Company, RegexOptions.Compiled, TimeSpan.FromSeconds(1));
    private readonly IPortalRepositories _portalRepositories;
    private readonly IOfferService _offerService;
    private readonly IOfferSubscriptionService _offerSubscriptionService;
    private readonly IOfferSetupService _offerSetupService;
    private readonly ServiceSettings _settings;
    private readonly IIdentityData _identityData;
    private readonly ILogger<ServiceBusinessLogic> _logger;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalRepositories">Factory to access the repositories</param>
    /// <param name="offerService">Access to the offer service</param>
    /// <param name="offerSubscriptionService">Service for Company to manage offer subscriptions</param>
    /// <param name="offerSetupService">Offer Setup Service</param>
    /// <param name="identityService">Access the identity of the user</param>
    /// <param name="settings">Access to the settings</param>
    /// <param name="logger">Access to the logger</param>
    public ServiceBusinessLogic(
        IPortalRepositories portalRepositories,
        IOfferService offerService,
        IOfferSubscriptionService offerSubscriptionService,
        IOfferSetupService offerSetupService,
        IIdentityService identityService,
        IOptions<ServiceSettings> settings,
        ILogger<ServiceBusinessLogic> logger)
    {
        _portalRepositories = portalRepositories;
        _offerService = offerService;
        _offerSubscriptionService = offerSubscriptionService;
        _offerSetupService = offerSetupService;
        _identityData = identityService.IdentityData;
        _logger = logger;
        _settings = settings.Value;
    }

    /// <inheritdoc />
    public Task<Pagination.Response<ServiceOverviewData>> GetAllActiveServicesAsync(int page, int size, ServiceOverviewSorting? sorting, ServiceTypeId? serviceTypeId) =>
        Pagination.CreateResponseAsync(
            page,
            size,
            _settings.ApplicationsMaxPageSize,
            _portalRepositories.GetInstance<IOfferRepository>().GetActiveServicesPaginationSource(sorting, serviceTypeId, Constants.DefaultLanguage));

    /// <inheritdoc />
    public Task<Guid> AddServiceSubscription(Guid serviceId, IEnumerable<OfferAgreementConsentData> offerAgreementConsentData) =>
        _offerSubscriptionService.AddOfferSubscriptionAsync(serviceId, offerAgreementConsentData, OfferTypeId.SERVICE, _settings.BasePortalAddress, _settings.SubscriptionManagerRoles, _settings.ServiceManagerRoles);

    /// <inheritdoc />
    public async Task<ServiceDetailResponse> GetServiceDetailsAsync(Guid serviceId, string lang)
    {
        var result = await _portalRepositories.GetInstance<IOfferRepository>().GetServiceDetailByIdUntrackedAsync(serviceId, lang, _identityData.CompanyId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (result == default)
        {
            throw new NotFoundException($"Service {serviceId} does not exist");
        }

        return new ServiceDetailResponse(
            result.Id,
            result.Title,
            result.Provider,
            result.ContactEmail,
            result.Description,
            result.LicenseTypeId,
            result.Price,
            result.OfferSubscriptionDetailData,
            result.ServiceTypeIds,
            result.Documents.GroupBy(doc => doc.DocumentTypeId).ToDictionary(d => d.Key, d => d.Select(x => new DocumentData(x.DocumentId, x.DocumentName))),
            result.TechnicalUserProfile.ToDictionary(g => g.TechnicalUserProfileId, g => g.UserRoles)
        );
    }

    /// <inheritdoc />
    public async Task<SubscriptionDetailData> GetSubscriptionDetailAsync(Guid subscriptionId)
    {
        var subscriptionDetailData = await _portalRepositories.GetInstance<IOfferSubscriptionsRepository>()
            .GetSubscriptionDetailDataForOwnUserAsync(subscriptionId, _identityData.CompanyId, OfferTypeId.SERVICE).ConfigureAwait(ConfigureAwaitOptions.None);
        if (subscriptionDetailData is null)
        {
            throw new NotFoundException($"Subscription {subscriptionId} does not exist");
        }

        return subscriptionDetailData;
    }

    /// <inheritdoc />
    public IAsyncEnumerable<AgreementData> GetServiceAgreement(Guid serviceId) =>
        _offerService.GetOfferAgreementsAsync(serviceId, OfferTypeId.SERVICE);

    /// <inheritdoc />
    public Task<ConsentDetailData> GetServiceConsentDetailDataAsync(Guid serviceConsentId) =>
        _offerService.GetConsentDetailDataAsync(serviceConsentId, OfferTypeId.SERVICE);

    /// <inheritdoc />
    public Task<OfferAutoSetupResponseData> AutoSetupServiceAsync(OfferAutoSetupData data) =>
        _offerSetupService.AutoSetupOfferAsync(data, _settings.ITAdminRoles, OfferTypeId.SERVICE, _settings.UserManagementAddress, _settings.ServiceManagerRoles);

    /// <inheritdoc/>
    public async Task<Pagination.Response<OfferCompanySubscriptionStatusResponse>> GetCompanyProvidedServiceSubscriptionStatusesForUserAsync(int page, int size, SubscriptionStatusSorting? sorting, OfferSubscriptionStatusId? statusId, Guid? offerId, string? companyName = null)
    {
        if (!string.IsNullOrEmpty(companyName) && !Company.IsMatch(companyName))
        {
            throw new ControllerArgumentException("CompanyName length must be 3-40 characters and *+=#%\\s not used as one of the first three characters in the company name");
        }
        async Task<Pagination.Source<OfferCompanySubscriptionStatusResponse>?> GetCompanyProvidedAppSubscriptionStatusData(int skip, int take)
        {
            var offerCompanySubscriptionResponse = await _portalRepositories.GetInstance<IOfferSubscriptionsRepository>()
                .GetOwnCompanyProvidedOfferSubscriptionStatusesUntrackedAsync(_identityData.CompanyId, OfferTypeId.SERVICE, sorting, OfferSubscriptionService.GetOfferSubscriptionFilterStatusIds(statusId), offerId, companyName)(skip, take).ConfigureAwait(ConfigureAwaitOptions.None);

            return offerCompanySubscriptionResponse == null
                ? null
                : new Pagination.Source<OfferCompanySubscriptionStatusResponse>(
                    offerCompanySubscriptionResponse.Count,
                    offerCompanySubscriptionResponse.Data.Select(item =>
                        new OfferCompanySubscriptionStatusResponse(
                            item.OfferId,
                            item.ServiceName,
                            item.CompanySubscriptionStatuses.Select(x => x.GetCompanySubscriptionStatus(item.OfferId, _logger)),
                            item.Image == Guid.Empty ? null : item.Image)));
        }
        return await Pagination.CreateResponseAsync(page, size, _settings.ApplicationsMaxPageSize, GetCompanyProvidedAppSubscriptionStatusData).ConfigureAwait(ConfigureAwaitOptions.None);
    }

    /// <inheritdoc />
    public Task<(byte[] Content, string ContentType, string FileName)> GetServiceDocumentContentAsync(Guid serviceId, Guid documentId, CancellationToken cancellationToken) =>
        _offerService.GetOfferDocumentContentAsync(serviceId, documentId, _settings.ServiceImageDocumentTypeIds, OfferTypeId.SERVICE, cancellationToken);

    /// <inheritdoc/>
    public async Task<Pagination.Response<AllOfferStatusData>> GetCompanyProvidedServiceStatusDataAsync(int page, int size, OfferSorting? sorting, string? offerName, ServiceStatusIdFilter? statusId)
    {
        async Task<Pagination.Source<AllOfferStatusData>?> GetCompanyProvidedServiceStatusData(int skip, int take)
        {
            var companyProvidedServiceStatusData = await _portalRepositories.GetInstance<IOfferRepository>()
                .GetCompanyProvidedServiceStatusDataAsync(GetOfferStatusIds(statusId), OfferTypeId.SERVICE, _identityData.CompanyId, sorting ?? OfferSorting.DateDesc, offerName)(skip, take).ConfigureAwait(ConfigureAwaitOptions.None);

            return companyProvidedServiceStatusData == null
                ? null
                : new Pagination.Source<AllOfferStatusData>(
                    companyProvidedServiceStatusData.Count,
                    companyProvidedServiceStatusData.Data.Select(item =>
                        item with
                        {
                            LeadPictureId = item.LeadPictureId == Guid.Empty ? null : item.LeadPictureId
                        }));
        }
        return await Pagination.CreateResponseAsync(page, size, 15, GetCompanyProvidedServiceStatusData).ConfigureAwait(ConfigureAwaitOptions.None);
    }

    private static IEnumerable<OfferStatusId> GetOfferStatusIds(ServiceStatusIdFilter? serviceStatusIdFilter)
    {
        switch (serviceStatusIdFilter)
        {
            case ServiceStatusIdFilter.Active:
                {
                    return new[] { OfferStatusId.ACTIVE };
                }
            case ServiceStatusIdFilter.Inactive:
                {
                    return new[] { OfferStatusId.INACTIVE };
                }
            case ServiceStatusIdFilter.InReview:
                {
                    return new[] { OfferStatusId.IN_REVIEW };
                }
            case ServiceStatusIdFilter.WIP:
                {
                    return new[] { OfferStatusId.CREATED };
                }
            default:
                {
                    return Enum.GetValues<OfferStatusId>();
                }
        }
    }

    /// <inheritdoc />
    public Task<ProviderSubscriptionDetailData> GetSubscriptionDetailForProvider(Guid serviceId, Guid subscriptionId) =>
        _offerService.GetSubscriptionDetailsForProviderAsync(serviceId, subscriptionId, OfferTypeId.SERVICE, _settings.CompanyAdminRoles);

    /// <inheritdoc />
    public Task<SubscriberSubscriptionDetailData> GetSubscriptionDetailForSubscriber(Guid serviceId, Guid subscriptionId) =>
        _offerService.GetSubscriptionDetailsForSubscriberAsync(serviceId, subscriptionId, OfferTypeId.SERVICE, _settings.SalesManagerRoles);

    /// <inheritdoc />
    public Task<Pagination.Response<OfferSubscriptionStatusDetailData>> GetCompanySubscribedServiceSubscriptionStatusesForUserAsync(int page, int size) =>
        _offerService.GetCompanySubscribedOfferSubscriptionStatusesForUserAsync(page, size, OfferTypeId.SERVICE, DocumentTypeId.SERVICE_LEADIMAGE);

    /// <inheritdoc />
    public Task StartAutoSetupAsync(OfferAutoSetupData data) =>
        _offerSetupService.StartAutoSetupAsync(data, OfferTypeId.SERVICE);

    /// <inheritdoc/>
    public Task UnsubscribeOwnCompanyServiceSubscriptionAsync(Guid subscriptionId) =>
        _offerService.UnsubscribeOwnCompanySubscriptionAsync(subscriptionId);

    /// <inheritdoc/>
    public Task TriggerActivateOfferSubscription(Guid subscriptionId) =>
        _offerSetupService.TriggerActivateSubscription(subscriptionId);
}
