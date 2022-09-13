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

using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.Framework.Models;
using CatenaX.NetworkServices.Notification.Library;
using CatenaX.NetworkServices.Offers.Library.Service;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.Provisioning.Library;
using CatenaX.NetworkServices.Provisioning.Library.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CatenaX.NetworkServices.Services.Service.BusinessLogic;

/// <summary>
/// Implementation of <see cref="IServiceBusinessLogic"/>.
/// </summary>
public class ServiceBusinessLogic : IServiceBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly IOfferService _offerService;
    private readonly IProvisioningManager _provisioningManager;
    private readonly IServiceAccountCreation _serviceAccountCreation;
    private readonly INotificationService _notificationService;
    private readonly ServiceSettings _settings;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalRepositories">Factory to access the repositories</param>
    /// <param name="settings">Access to the settings</param>
    /// <param name="offerService">Access to the offer service</param>
    /// <param name="provisioningManager">Access to the provisioning manager</param>
    /// <param name="serviceAccountCreation">Access to the service account creation</param>
    /// <param name="notificationService">Access to the notification service</param>
    public ServiceBusinessLogic(
        IPortalRepositories portalRepositories, 
        IOptions<ServiceSettings> settings, 
        IOfferService offerService,
        IProvisioningManager provisioningManager,
        IServiceAccountCreation serviceAccountCreation,
        INotificationService notificationService)
    {
        _portalRepositories = portalRepositories;
        _offerService = offerService;
        _provisioningManager = provisioningManager;
        _serviceAccountCreation = serviceAccountCreation;
        _notificationService = notificationService;
        _settings = settings.Value;
    }

    /// <inheritdoc />
    public Task<Pagination.Response<ServiceDetailData>> GetAllActiveServicesAsync(int page, int size)
    {
        var services = _portalRepositories.GetInstance<IOfferRepository>().GetActiveServices();
        return Pagination.CreateResponseAsync(
            page,
            size,
            _settings.ApplicationsMaxPageSize,
            (skip, take) => new Pagination.AsyncSource<ServiceDetailData>(
                services.CountAsync(),
                services
                    .Skip(skip)
                    .Take(take)
                    .Select(s =>
                        new ServiceDetailData(
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
    public async Task<Guid> CreateServiceOffering(ServiceOfferingData data, string iamUserId)
    {
        var results = await _portalRepositories.GetInstance<IUserRepository>()
            .GetCompanyUserWithIamUserCheckAndCompanyShortName(iamUserId, data.SalesManager)
            .ToListAsync();

        if (!results.Any(x => x.IsIamUser))
            throw new ControllerArgumentException($"IamUser is not assignable to company user {iamUserId}", nameof(iamUserId));

        if (string.IsNullOrWhiteSpace(results.Single(x => x.IsIamUser).CompanyShortName))
            throw new ControllerArgumentException($"No matching company found for user {iamUserId}", nameof(iamUserId));

        if (results.All(x => x.CompanyUserId != data.SalesManager))
            throw new ControllerArgumentException("SalesManager does not exist", nameof(data.SalesManager));

        await CheckLanguageCodesExist(data.Descriptions.Select(x => x.LanguageCode)).ConfigureAwait(false);

        var offerRepository = _portalRepositories.GetInstance<IOfferRepository>();
        var service = offerRepository.CreateOffer(string.Empty, OfferTypeId.SERVICE, service =>
        {
            service.ContactEmail = data.ContactEmail;
            service.Name = data.Title;
            service.SalesManagerId = data.SalesManager;
            service.ThumbnailUrl = data.ThumbnailUrl;
            service.Provider = results.Single(x => x.IsIamUser).CompanyShortName;
            service.OfferStatusId = OfferStatusId.CREATED;
            service.ProviderCompanyId = results.Single(x => x.IsIamUser).CompanyId;
        });
        var licenseId = offerRepository.CreateOfferLicenses(data.Price).Id;
        offerRepository.CreateOfferAssignedLicense(service.Id, licenseId);
        offerRepository.AddOfferDescriptions(data.Descriptions.Select(d =>
            new ValueTuple<Guid, string, string, string>(service.Id, d.LanguageCode, string.Empty, d.Description)));

        await _portalRepositories.SaveAsync();
        return service.Id;
    }

    /// <inheritdoc />
    public async Task<Guid> AddServiceSubscription(Guid serviceId, string iamUserId)
    {
        if (!await _portalRepositories.GetInstance<IOfferRepository>().CheckServiceExistsById(serviceId).ConfigureAwait(false))
        {
            throw new NotFoundException($"Service {serviceId} does not exist");
        }

        var (companyId, companyUserId) = await _portalRepositories.GetInstance<IUserRepository>().GetOwnCompanAndCompanyUseryId(iamUserId).ConfigureAwait(false);
        if (companyId == Guid.Empty)
        {
            throw new ControllerArgumentException($"User {iamUserId} has no company assigned", nameof(iamUserId));
        }
        
        if (companyUserId == Guid.Empty)
        {
            throw new ControllerArgumentException($"User {iamUserId} has no company user assigned", nameof(iamUserId));
        }

        var offerSubscription = _portalRepositories.GetInstance<IOfferSubscriptionsRepository>().CreateOfferSubscription(serviceId, companyId, OfferSubscriptionStatusId.PENDING, companyUserId, companyUserId);
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
        return offerSubscription.Id;
    }

    /// <inheritdoc />
    public async Task<ServiceDetailData> GetServiceDetailsAsync(Guid serviceId, string lang)
    {        
        var serviceDetailData = await _portalRepositories.GetInstance<IOfferRepository>().GetServiceDetailByIdUntrackedAsync(serviceId, lang).ConfigureAwait(false);
        if (serviceDetailData == default)
        {
            throw new NotFoundException($"Service {serviceId} does not exist");
        }

        return new ServiceDetailData(
            serviceDetailData.Id,
            serviceDetailData.Title ?? Constants.ErrorString,
            serviceDetailData.Provider,
            serviceDetailData.LeadPictureUri ?? Constants.ErrorString,
            serviceDetailData.ContactEmail,
            serviceDetailData.Description ?? Constants.ErrorString,
            serviceDetailData.Price ?? Constants.ErrorString);
    }

    /// <inheritdoc />
    public async Task<SubscriptionDetailData> GetSubscriptionDetailAsync(Guid subscriptionId, string iamUserId)
    {
        var subscriptionDetailData = await _portalRepositories.GetInstance<IOfferSubscriptionsRepository>()
            .GetSubscriptionDetailDataForOwnUserAsync(subscriptionId, iamUserId).ConfigureAwait(false);
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
            serviceAgreementConsentData.ConsentStatusId, iamUserId, OfferTypeId.SERVICE, Enumerable.Repeat(AgreementCategoryId.SERVICE_CONTRACT, 1));

    /// <inheritdoc />
    public IAsyncEnumerable<AgreementData> GetServiceAgreement(string iamUserId) => 
        _offerService.GetOfferAgreement(iamUserId, OfferTypeId.SERVICE);

    /// <inheritdoc />
    public Task<ConsentDetailData> GetServiceConsentDetailDataAsync(Guid serviceConsentId) =>
        _offerService.GetConsentDetailDataAsync(serviceConsentId);

    /// <inheritdoc />
    public async Task<ServiceAutoSetupResponseData> AutoSetupService(ServiceAutoSetupData data, string iamUserId)
    {
        var offerDetails = await _portalRepositories.GetInstance<IOfferSubscriptionsRepository>()
            .GetOfferDetailsAndCheckUser(data.RequestId, iamUserId).ConfigureAwait(false);
        if (offerDetails == null)
        {
            throw new NotFoundException($"OfferSubscription {data.RequestId} does not exist");
        }

        if (offerDetails.Status is not OfferSubscriptionStatusId.PENDING)
        {
            throw new ControllerArgumentException("Status of the offer subscription must be pending", nameof(offerDetails.Status));
        }

        if (offerDetails.CompanyUserId == Guid.Empty)
        {
            throw new ControllerArgumentException("Only the providing company can setup the service", nameof(offerDetails.CompanyUserId));
        }

        var userRolesRepository = _portalRepositories.GetInstance<IUserRolesRepository>();
        var userRoles = await userRolesRepository.GetUserRolesForOfferIdAsync(offerDetails.OfferId).ConfigureAwait(false);
        var redirectUrl = data.OfferUrl.EndsWith("/") ? $"{data.OfferUrl}*" : $"{data.OfferUrl}/*";
        var clientId = await _provisioningManager.SetupClientAsync(redirectUrl, userRoles).ConfigureAwait(false);
        var iamClient = _portalRepositories.GetInstance<IClientRepository>().CreateClient(clientId);
        
        var appInstance = _portalRepositories.GetInstance<IAppInstanceRepository>().CreateAppInstance(offerDetails.OfferId, iamClient.Id);
        _portalRepositories.GetInstance<IAppSubscriptionDetailRepository>()
            .CreateAppSubscriptionDetail(data.RequestId, (appSubscriptionDetail) =>
            {
                appSubscriptionDetail.AppInstanceId = appInstance.Id;
                appSubscriptionDetail.AppSubscriptionUrl = data.OfferUrl;
            });
        
        var serviceAccountUserRoles = await userRolesRepository
            .GetUserRoleDataUntrackedAsync(_settings.ServiceAccountRoles)
            .ToListAsync()
            .ConfigureAwait(false);
        var description = $"Technical User for app {offerDetails.OfferName} - {string.Join(",", serviceAccountUserRoles.Select(x => x.UserRoleText))}";
        var (_, serviceAccountData, serviceAccountId, _) = await _serviceAccountCreation
            .CreateServiceAccountAsync(
                clientId, 
                description, 
                IamClientAuthMethod.SECRET, 
                serviceAccountUserRoles.Select(x => x.UserRoleId), 
                offerDetails.CompanyId, 
                Enumerable.Repeat(offerDetails.Bpn, 1))
            .ConfigureAwait(false);

        var offerSubscription = new OfferSubscription(data.RequestId);
        _portalRepositories.Attach(offerSubscription, (subscription =>
        {
            subscription.OfferSubscriptionStatusId = OfferSubscriptionStatusId.ACTIVE;
        }));
        
        await _notificationService.CreateNotifications(
            _settings.CompanyAdminRoles,
            offerDetails.CompanyUserId,
            new (string?, NotificationTypeId)[]
            {
                (null, NotificationTypeId.TECHNICAL_USER_CREATION),
                (null, NotificationTypeId.APP_SUBSCRIPTION_ACTIVATION)
            }).ConfigureAwait(false);
        
        _portalRepositories.GetInstance<INotificationRepository>().CreateNotification(offerDetails.RequesterId,
            NotificationTypeId.APP_SUBSCRIPTION_ACTIVATION, false);
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
        
        return new ServiceAutoSetupResponseData(serviceAccountId, serviceAccountData.AuthData.Secret);
    }

    private async Task CheckLanguageCodesExist(IEnumerable<string> languageCodes)
    {
        if (languageCodes.Any())
        {
            var foundLanguageCodes = await _portalRepositories.GetInstance<ILanguageRepository>()
                .GetLanguageCodesUntrackedAsync(languageCodes)
                .ToListAsync()
                .ConfigureAwait(false);
            var notFoundLanguageCodes = languageCodes.Except(foundLanguageCodes).ToList();
            if (notFoundLanguageCodes.Any())
            {
                throw new ControllerArgumentException(
                    $"Language code(s) {string.Join(",", notFoundLanguageCodes)} do(es) not exist",
                    nameof(languageCodes));
            }
        }
    }
}
