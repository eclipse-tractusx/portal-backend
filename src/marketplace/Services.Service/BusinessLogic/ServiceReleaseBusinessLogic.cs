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
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Web;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;
using Org.Eclipse.TractusX.Portal.Backend.Services.Service.ViewModels;

namespace Org.Eclipse.TractusX.Portal.Backend.Services.Service.BusinessLogic;

/// <summary>
/// Implementation of <see cref="IServiceReleaseBusinessLogic"/>.
/// </summary>
public class ServiceReleaseBusinessLogic : IServiceReleaseBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly IOfferService _offerService;
    private readonly IOfferDocumentService _offerDocumentService;
    private readonly IIdentityData _identityData;
    private readonly ServiceSettings _settings;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalRepositories">Factory to access the repositories</param>
    /// <param name="offerService">Access to the offer service</param>
    /// <param name="offerDocumentService">Access to the offer document service</param>
    /// <param name="identityService">Access to identity</param>
    /// <param name="settings">Access to the settings</param>
    public ServiceReleaseBusinessLogic(
        IPortalRepositories portalRepositories,
        IOfferService offerService,
        IOfferDocumentService offerDocumentService,
        IIdentityService identityService,
        IOptions<ServiceSettings> settings)
    {
        _portalRepositories = portalRepositories;
        _offerService = offerService;
        _offerDocumentService = offerDocumentService;
        _identityData = identityService.IdentityData;
        _settings = settings.Value;
    }

    public IAsyncEnumerable<AgreementDocumentData> GetServiceAgreementDataAsync() =>
        _offerService.GetOfferTypeAgreements(OfferTypeId.SERVICE);

    /// <inheritdoc />
    public async Task<ServiceData> GetServiceDetailsByIdAsync(Guid serviceId)
    {
        var result = await _portalRepositories.GetInstance<IOfferRepository>()
            .GetServiceDetailsByIdAsync(serviceId).ConfigureAwait(false);
        if (result == default)
        {
            throw new NotFoundException($"serviceId {serviceId} not found or Incorrect Status");
        }

        return new ServiceData(
            result.Id,
            result.Title ?? Constants.ErrorString,
            result.ServiceTypeIds,
            result.Provider,
            result.Descriptions,
            result.Documents.GroupBy(d => d.DocumentTypeId).ToDictionary(g => g.Key, g => g.Select(d => new DocumentData(d.DocumentId, d.DocumentName))),
            result.ProviderUri ?? Constants.ErrorString,
            result.ContactEmail,
            result.ContactNumber,
            result.LicenseTypeId,
            result.OfferStatusId,
            result.TechnicalUserProfile.ToDictionary(g => g.TechnicalUserProfileId, g => g.UserRoles)
        );
    }

    /// <inheritdoc />
    public IAsyncEnumerable<ServiceTypeData> GetServiceTypeDataAsync() =>
        _portalRepositories.GetInstance<IStaticDataRepository>().GetServiceTypeData();

    /// <inheritdoc/>
    public Task<OfferAgreementConsent> GetServiceAgreementConsentAsync(Guid serviceId) =>
        _offerService.GetProviderOfferAgreementConsentById(serviceId, OfferTypeId.SERVICE);

    public async Task<ServiceProviderResponse> GetServiceDetailsForStatusAsync(Guid serviceId)
    {
        var result = await _offerService.GetProviderOfferDetailsForStatusAsync(serviceId, OfferTypeId.SERVICE).ConfigureAwait(false);
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
            result.ServiceTypeIds,
            result.TechnicalUserProfile);
    }

    /// <inheritdoc/>
    public Task<IEnumerable<ConsentStatusData>> SubmitOfferConsentAsync(Guid serviceId, OfferAgreementConsent offerAgreementConsents)
    {
        if (serviceId == Guid.Empty)
        {
            throw new ControllerArgumentException("ServiceId must not be empty");
        }

        return _offerService.CreateOrUpdateProviderOfferAgreementConsent(serviceId, offerAgreementConsents, OfferTypeId.SERVICE);
    }

    /// <inheritdoc/>
    public Task<Pagination.Response<InReviewServiceData>> GetAllInReviewStatusServiceAsync(int page, int size, OfferSorting? sorting, string? serviceName, string? languageShortName, ServiceReleaseStatusIdFilter? statusId) =>
        Pagination.CreateResponseAsync(page, size, 15,
            _portalRepositories.GetInstance<IOfferRepository>()
                .GetAllInReviewStatusServiceAsync(GetOfferStatusIds(statusId), OfferTypeId.SERVICE, sorting ?? OfferSorting.DateDesc, serviceName, languageShortName ?? Constants.DefaultLanguage, Constants.DefaultLanguage));

    private IEnumerable<OfferStatusId> GetOfferStatusIds(ServiceReleaseStatusIdFilter? serviceStatusIdFilter)
    {
        switch (serviceStatusIdFilter)
        {
            case ServiceReleaseStatusIdFilter.InReview:
                {
                    return new[] { OfferStatusId.IN_REVIEW };
                }

            default:
                {
                    return _settings.OfferStatusIds;
                }
        }
    }

    /// <inheritdoc />
    public Task<Guid> CreateServiceOfferingAsync(ServiceOfferingData data) =>
        _offerService.CreateServiceOfferingAsync(data, OfferTypeId.SERVICE);

    /// <inheritdoc />
    public async Task UpdateServiceAsync(Guid serviceId, ServiceUpdateRequestData data)
    {
        var companyId = _identityData.CompanyId;
        var serviceData = await _portalRepositories
            .GetInstance<IOfferRepository>()
            .GetServiceUpdateData(serviceId, data.ServiceTypeIds, companyId)
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
            throw new ForbiddenException($"Company {companyId} is not the service provider.");
        }

        if (data.SalesManager.HasValue)
        {
            await _offerService.ValidateSalesManager(data.SalesManager.Value, _settings.SalesManagerRoles).ConfigureAwait(false);
        }

        var offerRepository = _portalRepositories.GetInstance<IOfferRepository>();
        offerRepository.AttachAndModifyOffer(
            serviceId,
            offer =>
            {
                offer.Name = data.Title;
                offer.SalesManagerId = data.SalesManager;
                offer.ContactEmail = data.ContactEmail;
                offer.MarketingUrl = data.ProviderUri;
            },
            offer =>
            {
                offer.SalesManagerId = serviceData.SalesManagerId;
                offer.DateLastChanged = DateTimeOffset.UtcNow;
            });

        _offerService.UpsertRemoveOfferDescription(serviceId, data.Descriptions, serviceData.Descriptions);
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
        if (data.ServiceTypeIds.All(x => x == ServiceTypeId.CONSULTANCY_SERVICE))
        {
            _portalRepositories.GetInstance<ITechnicalUserProfileRepository>()
                .RemoveTechnicalUserProfilesForOffer(serviceId);
        }

        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    private static void UpdateAssignedServiceTypes(IEnumerable<(Guid serviceId, ServiceTypeId serviceTypeId)> newServiceTypes, IEnumerable<(Guid serviceId, ServiceTypeId serviceTypeId)> serviceTypeIdsToRemove, IOfferRepository appRepository)
    {
        appRepository.AddServiceAssignedServiceTypes(newServiceTypes);
        appRepository.RemoveServiceAssignedServiceTypes(serviceTypeIdsToRemove);
    }

    /// <inheritdoc/>
    public Task SubmitServiceAsync(Guid serviceId) =>
        _offerService.SubmitServiceAsync(serviceId, OfferTypeId.SERVICE, _settings.SubmitServiceNotificationTypeIds, _settings.CatenaAdminRoles);

    /// <inheritdoc/>
    public Task ApproveServiceRequestAsync(Guid appId) =>
        _offerService.ApproveOfferRequestAsync(appId, OfferTypeId.SERVICE, _settings.ApproveServiceNotificationTypeIds, _settings.ApproveServiceUserRoles, _settings.SubmitServiceNotificationTypeIds, _settings.CatenaAdminRoles, (_settings.OfferSubscriptionAddress, _settings.OfferDetailAddress), _settings.ActivationUserRoles);

    /// <inheritdoc />
    public Task DeclineServiceRequestAsync(Guid serviceId, OfferDeclineRequest data) =>
        _offerService.DeclineOfferAsync(serviceId, data, OfferTypeId.SERVICE, NotificationTypeId.SERVICE_RELEASE_REJECTION, _settings.ServiceManagerRoles, _settings.ServiceOverviewAddress, _settings.SubmitServiceNotificationTypeIds, _settings.CatenaAdminRoles);

    /// <inheritdoc />
    public Task CreateServiceDocumentAsync(Guid serviceId, DocumentTypeId documentTypeId, IFormFile document, CancellationToken cancellationToken) =>
        _offerDocumentService.UploadDocumentAsync(serviceId, documentTypeId, document, OfferTypeId.SERVICE, _settings.UploadServiceDocumentTypeIds, OfferStatusId.CREATED, cancellationToken);

    /// <inheritdoc/>
    public Task DeleteServiceDocumentsAsync(Guid documentId) =>
        _offerService.DeleteDocumentsAsync(documentId, _settings.DeleteDocumentTypeIds, OfferTypeId.SERVICE);

    /// <inheritdoc />
    public Task<IEnumerable<TechnicalUserProfileInformation>> GetTechnicalUserProfilesForOffer(Guid offerId) =>
        _offerService.GetTechnicalUserProfilesForOffer(offerId, OfferTypeId.SERVICE);

    /// <inheritdoc />
    public Task UpdateTechnicalUserProfiles(Guid serviceId, IEnumerable<TechnicalUserProfileData> data) =>
        _offerService.UpdateTechnicalUserProfiles(serviceId, OfferTypeId.SERVICE, data, _settings.TechnicalUserProfileClient);
}
