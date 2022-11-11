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
using Org.Eclipse.TractusX.Portal.Backend.Notification.Library;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;

namespace Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Service;

public class OfferService : IOfferService
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly IProvisioningManager _provisioningManager;
    private readonly IServiceAccountCreation _serviceAccountCreation;
    private readonly INotificationService _notificationService;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalRepositories">Factory to access the repositories</param>
    public OfferService(IPortalRepositories portalRepositories,
        IProvisioningManager provisioningManager,
        IServiceAccountCreation serviceAccountCreation,
        INotificationService notificationService)
    {
        _portalRepositories = portalRepositories;
        _provisioningManager = provisioningManager;
        _serviceAccountCreation = serviceAccountCreation;
        _notificationService = notificationService;
    }

    /// <inheritdoc />
    public async Task<Guid> CreateOfferSubscriptionAgreementConsentAsync(Guid subscriptionId,
        Guid agreementId, ConsentStatusId consentStatusId, string iamUserId, OfferTypeId offerTypeId)
    {
        var (companyId, offerSubscription, companyUserId) = await GetOfferSubscriptionCompanyAndUserAsync(subscriptionId, iamUserId, offerTypeId).ConfigureAwait(false);

        if (!await _portalRepositories.GetInstance<IAgreementRepository>()
                .CheckAgreementExistsForSubscriptionAsync(agreementId, subscriptionId, offerTypeId).ConfigureAwait(false))
        {
            throw new ControllerArgumentException($"Invalid Agreement {agreementId} for subscription {subscriptionId}", nameof(agreementId));
        }

        var consent = _portalRepositories.GetInstance<IConsentRepository>().CreateConsent(agreementId, companyId, companyUserId, consentStatusId);
        _portalRepositories.GetInstance<IConsentAssignedOfferSubscriptionRepository>().CreateConsentAssignedOfferSubscription(consent.Id, offerSubscription.Id);
        
        await _portalRepositories.SaveAsync();
        return consent.Id;
    }

    /// <inheritdoc />
    public async Task CreateOrUpdateOfferSubscriptionAgreementConsentAsync(Guid subscriptionId, IEnumerable<ServiceAgreementConsentData> offerAgreementConsentDatas,
        string iamUserId, OfferTypeId offerTypeId)
    {
        var (companyId, offerSubscription, companyUserId) = await GetOfferSubscriptionCompanyAndUserAsync(subscriptionId, iamUserId, offerTypeId).ConfigureAwait(false);

        if (!await _portalRepositories
                .GetInstance<IAgreementRepository>()
                .CheckAgreementsExistsForSubscriptionAsync(offerAgreementConsentDatas.Select(x => x.AgreementId), subscriptionId, offerTypeId)
                .ConfigureAwait(false))
        {
            throw new ControllerArgumentException($"Invalid Agreements for subscription {subscriptionId}", nameof(offerAgreementConsentDatas));
        }

        var consentAssignedOfferSubscriptionRepository = _portalRepositories.GetInstance<IConsentAssignedOfferSubscriptionRepository>();
        var offerSubscriptionConsents = await consentAssignedOfferSubscriptionRepository
            .GetConsentAssignedOfferSubscriptionsForSubscriptionAsync(subscriptionId, offerAgreementConsentDatas.Select(x => x.AgreementId))
            .ToListAsync()
            .ConfigureAwait(false);

        foreach (var offerSubscriptionConsent in offerSubscriptionConsents)
        {
            var consent = new Consent(offerSubscriptionConsent.ConsentId)
                {
                    ConsentStatusId = offerSubscriptionConsent.ConsentStatusId
                };
            var dbConsent = _portalRepositories.Attach(consent);
            dbConsent.ConsentStatusId = offerAgreementConsentDatas.Single(x => x.AgreementId == offerSubscriptionConsent.AgreementId).ConsentStatusId;
        }
        
        foreach (var consentData in offerAgreementConsentDatas.ExceptBy(offerSubscriptionConsents.Select(x => x.AgreementId), consentData => consentData.AgreementId))
        {
            var consent = _portalRepositories.GetInstance<IConsentRepository>().CreateConsent(consentData.AgreementId, companyId, companyUserId, consentData.ConsentStatusId);
            consentAssignedOfferSubscriptionRepository.CreateConsentAssignedOfferSubscription(consent.Id, offerSubscription.Id);
        }
    }

    private async Task<(Guid CompanyId, OfferSubscription OfferSubscription, Guid CompanyUserId)> GetOfferSubscriptionCompanyAndUserAsync(Guid subscriptionId, string iamUserId, OfferTypeId offerTypeId)
    {
        var result = await _portalRepositories.GetInstance<IOfferSubscriptionsRepository>()
            .GetCompanyIdWithAssignedOfferForCompanyUserAndSubscriptionAsync(subscriptionId, iamUserId, offerTypeId)
            .ConfigureAwait(false);
        if (result == default)
        {
            throw new ControllerArgumentException("Company or CompanyUser not assigned correctly.", nameof(iamUserId));
        }
        var (companyId, offerSubscription, companyUserId) = result;
        if (offerSubscription is null)
        {
            throw new NotFoundException($"Invalid OfferSubscription {subscriptionId} for OfferType {offerTypeId}");
        }
        return (companyId, offerSubscription, companyUserId);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<AgreementData> GetOfferAgreementsAsync(Guid offerId, OfferTypeId offerTypeId) => 
        _portalRepositories.GetInstance<IAgreementRepository>().GetOfferAgreementDataForOfferId(offerId, offerTypeId);

    /// <inheritdoc />
    public async Task<ConsentDetailData> GetConsentDetailDataAsync(Guid consentId, OfferTypeId offerTypeId)
    {
        var consentDetails = await _portalRepositories.GetInstance<IConsentRepository>()
            .GetConsentDetailData(consentId, offerTypeId).ConfigureAwait(false);
        if (consentDetails is null)
        {
            throw new NotFoundException($"Consent {consentId} does not exist");
        }

        return consentDetails;
    }

    public IAsyncEnumerable<AgreementData> GetOfferTypeAgreementsAsync(OfferTypeId offerTypeId)=>
        _portalRepositories.GetInstance<IAgreementRepository>().GetAgreementDataForOfferType(offerTypeId);

    public async Task<OfferAgreementConsent> GetProviderOfferAgreementConsentById(Guid offerId, string iamUserId, OfferTypeId offerTypeId)
    {
        var result = await _portalRepositories.GetInstance<IAgreementRepository>().GetOfferAgreementConsentById(offerId, iamUserId, offerTypeId).ConfigureAwait(false);
        if (result == default)
        {
            throw new NotFoundException($"offer {offerId}, offertype {offerTypeId} does not exist");
        }
        if (!result.IsProviderCompany)
        {
            throw new ForbiddenException($"UserId {iamUserId} is not assigned with Offer {offerId}");
        }
        return result.OfferAgreementConsent;
    }

    public async Task<int> CreaeteOrUpdateProviderOfferAgreementConsent(Guid offerId, OfferAgreementConsent offerAgreementConsent, string iamUserId, OfferTypeId offerTypeId)
    {
        var consentRepository = _portalRepositories.GetInstance<IConsentRepository>();

        var (companyUserId, companyId, dbAgreements) = await GetProviderOfferAgreementConsent(offerId, iamUserId, OfferStatusId.CREATED, offerTypeId).ConfigureAwait(false);

        foreach (var agreementId in offerAgreementConsent.Agreements
                .ExceptBy(dbAgreements.Select(db => db.AgreementId), input => input.AgreementId)
                .Select(input => input.AgreementId))
        {
            var consent = consentRepository.CreateConsent(agreementId, companyId, companyUserId, ConsentStatusId.ACTIVE);
            consentRepository.CreateConsentAssignedOffer(consent.Id, offerId);
        }
        foreach (var (agreementId, consentStatus) in offerAgreementConsent.Agreements
                .IntersectBy(dbAgreements.Select(d => d.AgreementId), input => input.AgreementId)
                .Select(input => (input.AgreementId, input.ConsentStatusId)))
        {
            var existing = dbAgreements.First(d => d.AgreementId == agreementId);
            _portalRepositories.Attach(new Consent(existing.ConsentId), consent =>
            {
                if (consentStatus != existing.ConsentStatusId)
                {
                    consent.ConsentStatusId = consentStatus;
                }
            });
        }

        return await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    private async Task<OfferAgreementConsentUpdate> GetProviderOfferAgreementConsent(Guid offerId, string iamUserId, OfferStatusId statusId, OfferTypeId offerTypeId)
    {
        var result = await _portalRepositories.GetInstance<IAgreementRepository>().GetOfferAgreementConsent(offerId, iamUserId, statusId, offerTypeId).ConfigureAwait(false);
        if (result == default)
        {
            throw new NotFoundException($"offer {offerId}, offertype {offerTypeId}, offerStatus {statusId} does not exist");
        }
        if (!result.IsProviderCompany)
        {
            throw new ForbiddenException($"UserId {iamUserId} is not assigned with Offer {offerId}");
        }
        return result.OfferAgreementConsentUpdate;
    }

    public async Task<OfferAutoSetupResponseData> AutoSetupServiceAsync(OfferAutoSetupData data, IDictionary<string,IEnumerable<string>> serviceAccountRoles, IDictionary<string,IEnumerable<string>> companyAdminRoles, string iamUserId, OfferTypeId offerTypeId)
    {
        var offerDetails = await _portalRepositories.GetInstance<IOfferSubscriptionsRepository>()
            .GetOfferDetailsAndCheckUser(data.RequestId, iamUserId, offerTypeId).ConfigureAwait(false);
        if (offerDetails == null)
        {
            throw new NotFoundException($"OfferSubscription {data.RequestId} does not exist");
        }

        if (offerDetails.Status is not OfferSubscriptionStatusId.PENDING)
        {
            throw new ControllerArgumentException("Status of the offer subscription must be pending", nameof(offerDetails.Status));
        }

        if (offerDetails.CompanyUserId == Guid.Empty && offerDetails.TechnicalUserId == Guid.Empty)
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
            .GetUserRoleDataUntrackedAsync(serviceAccountRoles)
            .ToListAsync()
            .ConfigureAwait(false);
        var description = $"Technical User for app {offerDetails.OfferName} - {string.Join(",", serviceAccountUserRoles.Select(x => x.UserRoleText))}";
        var (technicalClientId, serviceAccountData, serviceAccountId, _) = await _serviceAccountCreation
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
            companyAdminRoles,
            offerDetails.CompanyUserId != Guid.Empty ? offerDetails.CompanyUserId : null,
            new (string?, NotificationTypeId)[]
            {
                (null, NotificationTypeId.TECHNICAL_USER_CREATION),
                (null, NotificationTypeId.APP_SUBSCRIPTION_ACTIVATION)
            }).ConfigureAwait(false);
        
        _portalRepositories.GetInstance<INotificationRepository>().CreateNotification(offerDetails.RequesterId,
            NotificationTypeId.APP_SUBSCRIPTION_ACTIVATION, false);
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
        
        return new OfferAutoSetupResponseData(
            new TechnicalUserInfoData(serviceAccountId, serviceAccountData.AuthData.Secret, technicalClientId),
            new ClientInfoData(clientId));
    }

    /// <inheritdoc />
    public async Task<Guid> CreateServiceOfferingAsync(OfferingData data, string iamUserId, OfferTypeId offerTypeId)
    {
        var results = await _portalRepositories.GetInstance<IUserRepository>()
            .GetCompanyUserWithIamUserCheckAndCompanyShortName(iamUserId, data.SalesManager)
            .ToListAsync().ConfigureAwait(false);

        if (!results.Any(x => x.IsIamUser))
            throw new ControllerArgumentException($"IamUser is not assignable to company user {iamUserId}", nameof(iamUserId));

        if (string.IsNullOrWhiteSpace(results.Single(x => x.IsIamUser).CompanyShortName))
            throw new ControllerArgumentException($"No matching company found for user {iamUserId}", nameof(iamUserId));

        if (results.All(x => x.CompanyUserId != data.SalesManager))
            throw new ControllerArgumentException("SalesManager does not exist", nameof(data.SalesManager));

        await CheckLanguageCodesExist(data.Descriptions.Select(x => x.LanguageCode)).ConfigureAwait(false);

        var offerRepository = _portalRepositories.GetInstance<IOfferRepository>();
        var service = offerRepository.CreateOffer(string.Empty, offerTypeId, service =>
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

    public async Task<OfferProviderResponse> GetProviderOfferDetailsForStatusAsync(Guid offerId, string userId, OfferTypeId offerTypeId)
    {
        var offerDetail = await _portalRepositories.GetInstance<IOfferRepository>().GetProviderOfferDataWithConsentStatusAsync(offerId, userId, offerTypeId).ConfigureAwait(false);
        if (offerDetail == default)
        {
            throw new NotFoundException($"Offer {offerId} does not exist");
        }
        if (!offerDetail.IsProviderCompanyUser)
        {
            throw new ForbiddenException($"userId {userId} is not associated with provider-company of offer {offerId}");
        }
        return new OfferProviderResponse(
            offerDetail.OfferProviderData.Title,
            offerDetail.OfferProviderData.Provider,
            offerDetail.OfferProviderData.LeadPictureUri,
            offerDetail.OfferProviderData.ProviderName,
            offerDetail.OfferProviderData.UseCase,
            offerDetail.OfferProviderData.Descriptions,
            offerDetail.OfferProviderData.Agreements,
            offerDetail.OfferProviderData.SupportedLanguageCodes,
            offerDetail.OfferProviderData.Price,
            offerDetail.OfferProviderData.Images,
            offerDetail.OfferProviderData.ProviderUri,
            offerDetail.OfferProviderData.ContactEmail,
            offerDetail.OfferProviderData.ContactNumber,
            offerDetail.OfferProviderData.Documents.GroupBy(d => d.documentTypeId).ToDictionary(g => g.Key, g => g.Select(d => new DocumentData(d.documentId, d.documentName))));
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
