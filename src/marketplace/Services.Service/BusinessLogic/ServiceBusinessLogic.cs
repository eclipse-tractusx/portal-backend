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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Service;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Services.Service.ViewModels;

namespace Org.Eclipse.TractusX.Portal.Backend.Services.Service.BusinessLogic;

/// <summary>
/// Implementation of <see cref="IServiceBusinessLogic"/>.
/// </summary>
public class ServiceBusinessLogic : IServiceBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly IOfferService _offerService;
    private readonly IOfferSubscriptionService _offerSubscriptionService;
    private readonly IOfferSetupService _offerSetupService;
    private readonly ServiceSettings _settings;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalRepositories">Factory to access the repositories</param>
    /// <param name="offerService">Access to the offer service</param>
    /// <param name="offerSubscriptionService">Service for Company to manage offer subscriptions</param>
    /// <param name="offerSetupService">Offer Setup Service</param>
    /// <param name="settings">Access to the settings</param>
    public ServiceBusinessLogic(
        IPortalRepositories portalRepositories,
        IOfferService offerService,
        IOfferSubscriptionService offerSubscriptionService,
        IOfferSetupService offerSetupService,
        IOptions<ServiceSettings> settings)
    {
        _portalRepositories = portalRepositories;
        _offerService = offerService;
        _offerSubscriptionService = offerSubscriptionService;
        _offerSetupService = offerSetupService;
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
    public async Task<ServiceDetailResponse> GetServiceDetailsAsync(Guid serviceId, string lang, string iamUserId)
    {        
        var result = await _portalRepositories.GetInstance<IOfferRepository>().GetServiceDetailByIdUntrackedAsync(serviceId, lang, iamUserId).ConfigureAwait(false);
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
            result.Price,
            result.OfferSubscriptionDetailData,
            result.ServiceTypeIds,
            result.Documents.GroupBy(doc => doc.documentTypeId).ToDictionary(d => d.Key, d => d.Select(x => new DocumentData(x.documentId, x.documentName)))
        );
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
    public IAsyncEnumerable<AgreementData> GetServiceAgreement(Guid serviceId) => 
        _offerService.GetOfferAgreementsAsync(serviceId, OfferTypeId.SERVICE);

    /// <inheritdoc />
    public Task<ConsentDetailData> GetServiceConsentDetailDataAsync(Guid serviceConsentId) =>
        _offerService.GetConsentDetailDataAsync(serviceConsentId, OfferTypeId.SERVICE);

    /// <inheritdoc />
    public Task<OfferAutoSetupResponseData> AutoSetupServiceAsync(OfferAutoSetupData data, string iamUserId) =>
        _offerSetupService.AutoSetupOfferAsync(data, _settings.ServiceAccountRoles, _settings.ITAdminRoles, iamUserId, OfferTypeId.SERVICE, _settings.UserManagementAddress);

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

        if (data.SalesManager.HasValue)
        {
            await _offerService.ValidateSalesManager(data.SalesManager.Value, iamUserId, _settings.SalesManagerRoles).ConfigureAwait(false);
        }

        var offerRepository = _portalRepositories.GetInstance<IOfferRepository>();
        offerRepository.AttachAndModifyOffer(
            serviceId,
            offer =>
            {
                offer.Name = data.Title;
                offer.SalesManagerId = data.SalesManager;
                offer.ContactEmail = data.ContactEmail;
            },
            offer =>
            {
                offer.SalesManagerId = serviceData.SalesManagerId;
            });

        _offerService.UpsertRemoveOfferDescription(serviceId, data.Descriptions, serviceData.Descriptions);
        _offerService.CreateOrUpdateOfferLicense(serviceId, data.Price, serviceData.OfferLicense);
        var newServiceTypes = data.ServiceTypeIds
            .Except(serviceData.ServiceTypeIds.Where(x => x.IsMatch).Select(x => x.ServiceTypeId))
            .Select(sti => (serviceId, sti, sti == ServiceTypeId.DATASPACE_SERVICE)); // TODO (PS): Must be refactored, customer needs to define whether the service needs a technical User
        var serviceTypeIdsToRemove = serviceData.ServiceTypeIds
            .Where(x => !x.IsMatch)
            .Select(sti => (serviceId, sti.ServiceTypeId));
        UpdateAssignedServiceTypes(
            newServiceTypes, 
            serviceTypeIdsToRemove,
            offerRepository);

        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }
    
    private static void UpdateAssignedServiceTypes(IEnumerable<(Guid serviceId, ServiceTypeId serviceTypeId, bool technicalUserNeeded)> newServiceTypes, IEnumerable<(Guid serviceId, ServiceTypeId serviceTypeId)> serviceTypeIdsToRemove, IOfferRepository appRepository)
    {
        appRepository.AddServiceAssignedServiceTypes(newServiceTypes);
        appRepository.RemoveServiceAssignedServiceTypes(serviceTypeIdsToRemove);
    }

    /// <inheritdoc/>
    public Task<Pagination.Response<OfferCompanySubscriptionStatusData>> GetCompanyProvidedServiceSubscriptionStatusesForUserAsync(int page, int size, string iamUserId, SubscriptionStatusSorting? sorting, OfferSubscriptionStatusId? statusId) =>
        Pagination.CreateResponseAsync(page, size, _settings.ApplicationsMaxPageSize, _portalRepositories.GetInstance<IOfferSubscriptionsRepository>()
            .GetOwnCompanyProvidedOfferSubscriptionStatusesUntrackedAsync(iamUserId, OfferTypeId.SERVICE, sorting, statusId ?? OfferSubscriptionStatusId.ACTIVE));

    /// <inheritdoc/>
    public Task SubmitServiceAsync(Guid serviceId, string iamUserId) => 
        _offerService.SubmitServiceAsync(serviceId, iamUserId, OfferTypeId.SERVICE, _settings.SubmitServiceNotificationTypeIds, _settings.CatenaAdminRoles);

    /// <inheritdoc/>
    public Task ApproveServiceRequestAsync(Guid appId, string iamUserId) =>
        _offerService.ApproveOfferRequestAsync(appId, iamUserId, OfferTypeId.SERVICE, _settings.ApproveServiceNotificationTypeIds, _settings.ApproveServiceUserRoles);

    /// <inheritdoc />
    public Task DeclineServiceRequestAsync(Guid serviceId, string iamUserId, OfferDeclineRequest data) => 
        _offerService.DeclineOfferAsync(serviceId, iamUserId, data, OfferTypeId.SERVICE, NotificationTypeId.SERVICE_RELEASE_REJECTION, _settings.ServiceManagerRoles, _settings.ServiceMarketplaceAddress);
    
    /// <inheritdoc />
    public Task CreateServiceDocumentAsync(Guid serviceId, DocumentTypeId documentTypeId, IFormFile document, string iamUserId, CancellationToken cancellationToken) =>
        UploadServiceDoc(serviceId, documentTypeId, document, iamUserId, OfferTypeId.SERVICE, cancellationToken);

    private Task UploadServiceDoc(Guid serviceId, DocumentTypeId documentTypeId, IFormFile document, string iamUserId, OfferTypeId offerTypeId, CancellationToken cancellationToken) =>
        _offerService.UploadDocumentAsync(serviceId, documentTypeId, document, iamUserId, offerTypeId, _settings.DocumentTypeIds, _settings.ContentTypeSettings, cancellationToken);
    
    /// <inheritdoc />
    public Task<(byte[] Content, string ContentType, string FileName)> GetServiceDocumentContentAsync(Guid serviceId, Guid documentId, CancellationToken cancellationToken) =>
        _offerService.GetOfferDocumentContentAsync(serviceId, documentId, _settings.ServiceImageDocumentTypeIds, OfferTypeId.SERVICE, cancellationToken);

    /// <inheritdoc/>
    public Task<Pagination.Response<AllOfferStatusData>> GetCompanyProvidedServiceStatusDataAsync(int page, int size, string userId, OfferSorting? sorting, string? offerName,  ServiceStatusIdFilter? statusId) =>
        Pagination.CreateResponseAsync(page, size, 15,
            _portalRepositories.GetInstance<IOfferRepository>()
                .GetCompanyProvidedServiceStatusDataAsync(GetOfferStatusIds(statusId), OfferTypeId.SERVICE, userId, sorting ?? OfferSorting.DateDesc, offerName));

    private static IEnumerable<OfferStatusId> GetOfferStatusIds(ServiceStatusIdFilter? serviceStatusIdFilter)
    {
        switch(serviceStatusIdFilter)
        {
            case ServiceStatusIdFilter.Active :
            {
               return new []{ OfferStatusId.ACTIVE };
            }
            case ServiceStatusIdFilter.Inactive :
            {
               return new []{ OfferStatusId.INACTIVE };
            }
            case ServiceStatusIdFilter.InReview:
            {
               return new []{ OfferStatusId.IN_REVIEW };
            }
            case ServiceStatusIdFilter.WIP:
            {
               return new []{ OfferStatusId.CREATED };
            }
            default :
            {
                return Enum.GetValues<OfferStatusId>();
            }
        }       
    }
}
