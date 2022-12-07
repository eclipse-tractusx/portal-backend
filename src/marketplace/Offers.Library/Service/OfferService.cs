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
using Org.CatenaX.Ng.Portal.Backend.Mailing.SendMail;
using Org.CatenaX.Ng.Portal.Backend.Notifications.Library;
using Org.CatenaX.Ng.Portal.Backend.Offers.Library.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.CatenaX.Ng.Portal.Backend.Provisioning.Library;
using Org.CatenaX.Ng.Portal.Backend.Provisioning.Library.Enums;
using Org.CatenaX.Ng.Portal.Backend.Provisioning.Library.Service;

namespace Org.CatenaX.Ng.Portal.Backend.Offers.Library.Service;

public class OfferService : IOfferService
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly IProvisioningManager _provisioningManager;
    private readonly IServiceAccountCreation _serviceAccountCreation;
    private readonly INotificationService _notificationService;
    private readonly IMailingService _mailingService;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalRepositories">Factory to access the repositories</param>
    /// <param name="provisioningManager">Access to the provisioning manager</param>
    /// <param name="serviceAccountCreation">Access to the service account creation</param>
    /// <param name="notificationService">Creates notifications for the user</param>
    /// <param name="mailingService">Mailing service to send mails to the user</param>
    public OfferService(IPortalRepositories portalRepositories,
        IProvisioningManager provisioningManager,
        IServiceAccountCreation serviceAccountCreation,
        INotificationService notificationService,
        IMailingService mailingService)
    {
        _portalRepositories = portalRepositories;
        _provisioningManager = provisioningManager;
        _serviceAccountCreation = serviceAccountCreation;
        _notificationService = notificationService;
        _mailingService = mailingService;
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
    public async Task CreateOrUpdateOfferSubscriptionAgreementConsentAsync(Guid subscriptionId, IEnumerable<OfferAgreementConsentData> offerAgreementConsentData, string iamUserId, OfferTypeId offerTypeId)
    {
        var (companyId, offerSubscription, companyUserId) = await GetOfferSubscriptionCompanyAndUserAsync(subscriptionId, iamUserId, offerTypeId).ConfigureAwait(false);

        if (!await _portalRepositories
                .GetInstance<IAgreementRepository>()
                .CheckAgreementsExistsForSubscriptionAsync(offerAgreementConsentData.Select(x => x.AgreementId), subscriptionId, offerTypeId)
                .ConfigureAwait(false))
        {
            throw new ControllerArgumentException($"Invalid Agreements for subscription {subscriptionId}", nameof(offerAgreementConsentData));
        }

        var consentAssignedOfferSubscriptionRepository = _portalRepositories.GetInstance<IConsentAssignedOfferSubscriptionRepository>();
        var offerSubscriptionConsents = await consentAssignedOfferSubscriptionRepository
            .GetConsentAssignedOfferSubscriptionsForSubscriptionAsync(subscriptionId, offerAgreementConsentData.Select(x => x.AgreementId))
            .ToListAsync()
            .ConfigureAwait(false);

        foreach (var offerSubscriptionConsent in offerSubscriptionConsents)
        {
            var consent = new Consent(offerSubscriptionConsent.ConsentId)
                {
                    ConsentStatusId = offerSubscriptionConsent.ConsentStatusId
                };
            var dbConsent = _portalRepositories.Attach(consent);
            dbConsent.ConsentStatusId = offerAgreementConsentData.Single(x => x.AgreementId == offerSubscriptionConsent.AgreementId).ConsentStatusId;
        }
        
        foreach (var consentData in offerAgreementConsentData.ExceptBy(offerSubscriptionConsents.Select(x => x.AgreementId), consentData => consentData.AgreementId))
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

    public async Task<int> CreateOrUpdateProviderOfferAgreementConsent(Guid offerId, OfferAgreementConsent offerAgreementConsent, string iamUserId, OfferTypeId offerTypeId)
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

    public async Task<OfferAutoSetupResponseData> AutoSetupServiceAsync(OfferAutoSetupData data, IDictionary<string,IEnumerable<string>> serviceAccountRoles, IDictionary<string,IEnumerable<string>> companyAdminRoles, string iamUserId, OfferTypeId offerTypeId, string basePortalAddress)
    {
        var offerSubscriptionsRepository = _portalRepositories.GetInstance<IOfferSubscriptionsRepository>();
        var offerDetails = await offerSubscriptionsRepository
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

        offerSubscriptionsRepository.AttachAndModifyOfferSubscription(data.RequestId, subscription =>
        {
            subscription.OfferSubscriptionStatusId = OfferSubscriptionStatusId.ACTIVE;
            subscription.LastEditorId = offerDetails.CompanyUserId;
        });

        await CreateNotifications(companyAdminRoles, offerTypeId, offerDetails);
        await _portalRepositories.SaveAsync().ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(offerDetails.RequesterEmail))
        {
            var mailParams = new Dictionary<string, string>
            {
                { "offerRequesterName", offerDetails.RequesterFirstname ?? "User" },
                { "offerName", offerDetails.OfferName },
                { "url", basePortalAddress },
            };
            await _mailingService.SendMails(offerDetails.RequesterEmail, mailParams, new List<string> { "subscription-activation" }).ConfigureAwait(false);
        }
        return new OfferAutoSetupResponseData(
            new TechnicalUserInfoData(serviceAccountId, serviceAccountData.AuthData.Secret, technicalClientId),
            new ClientInfoData(clientId));
    }

    private async Task CreateNotifications(IDictionary<string, IEnumerable<string>> companyAdminRoles, OfferTypeId offerTypeId,
        OfferSubscriptionTransferData offerDetails)
    {
        var appSubscriptionActivation = offerTypeId == OfferTypeId.APP
            ? NotificationTypeId.APP_SUBSCRIPTION_ACTIVATION
            : NotificationTypeId.SERVICE_ACTIVATION;
        var notificationContent = JsonSerializer.Serialize(new
        {
            offerDetails.OfferId,
            offerDetails.CompanyName,
            offerDetails.OfferName
        });
        await _notificationService.CreateNotifications(
            companyAdminRoles,
            offerDetails.CompanyUserId != Guid.Empty ? offerDetails.CompanyUserId : null,
            new (string?, NotificationTypeId)[]
            {
                (null, NotificationTypeId.TECHNICAL_USER_CREATION),
                (notificationContent, appSubscriptionActivation)
            }).ConfigureAwait(false);

        _portalRepositories.GetInstance<INotificationRepository>().CreateNotification(offerDetails.RequesterId, appSubscriptionActivation, false, notification =>
            {
                notification.Content = notificationContent;
            });
    }

    /// <inheritdoc />
    public async Task<Guid> CreateServiceOfferingAsync(ServiceOfferingData data, string iamUserId, OfferTypeId offerTypeId)
    {
        var results = await _portalRepositories.GetInstance<IUserRepository>()
            .GetCompanyUserWithIamUserCheckAndCompanyShortName(iamUserId, data.SalesManager)
            .ToListAsync().ConfigureAwait(false);

        if (!results.Any(x => x.IsIamUser))
            throw new ControllerArgumentException($"IamUser is not assignable to company user {iamUserId}", nameof(iamUserId));

        if (string.IsNullOrWhiteSpace(results.Single(x => x.IsIamUser).CompanyShortName))
            throw new ControllerArgumentException($"No matching company found for user {iamUserId}", nameof(iamUserId));

        if (data.SalesManager.HasValue && results.All(x => x.CompanyUserId != data.SalesManager))
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
        
        offerRepository.AddServiceAssignedServiceTypes(data.ServiceTypeIds.Select(id => (service.Id, id)));
        offerRepository.AddOfferDescriptions(data.Descriptions.Select(d => (service.Id, d.LanguageCode, string.Empty, d.Description)));

        await _portalRepositories.SaveAsync();
        return service.Id;
    }
    
    /// <inheritdoc />
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

        var data = offerDetail.OfferProviderData;

        return new OfferProviderResponse(
            data.Title,
            data.Provider,
            data.LeadPictureUri,
            data.ProviderName,
            data.UseCase,
            data.Descriptions,
            data.Agreements.Select(a => new OfferAgreement(a.AgreementId, a.AgreementName, a.ConsentStatusId == null ? string.Empty : a.ConsentStatusId.ToString())),
            data.SupportedLanguageCodes,
            data.Price,
            data.Images,
            data.ProviderUri,
            data.ContactEmail,
            data.ContactNumber,
            data.Documents.GroupBy(d => d.documentTypeId).ToDictionary(g => g.Key, g => g.Select(d => new DocumentData(d.documentId, d.documentName))),
            data.SalesManagerId);
    }
    
    /// <inheritdoc />
    public async Task<Guid> ValidateSalesManager(Guid salesManagerId, string iamUserId, IDictionary<string, IEnumerable<string>> salesManagerRoles)
    {
        var userRoleIds = await _portalRepositories.GetInstance<IUserRolesRepository>()
            .GetUserRoleIdsUntrackedAsync(salesManagerRoles).ToListAsync().ConfigureAwait(false);
        var responseData = await _portalRepositories.GetInstance<IUserRepository>()
            .GetRolesAndCompanyMembershipUntrackedAsync(iamUserId, userRoleIds, salesManagerId)
            .ConfigureAwait(false);
        if (responseData == default)
        {
            throw new ControllerArgumentException($"invalid salesManagerId {salesManagerId}", nameof(salesManagerId));
        }

        if (!responseData.IsSameCompany)
        {
            throw new ForbiddenException($"user {iamUserId} is not a member of the company");
        }

        if (userRoleIds.Except(responseData.RoleIds).Any())
        {
            throw new ControllerArgumentException(
                $"User {salesManagerId} does not have sales Manager Role", nameof(salesManagerId));
        }

        return responseData.UserCompanyId;
    }
    
    public void UpsertRemoveOfferDescription(Guid offerId, IEnumerable<Localization> updateDescriptions, IEnumerable<(string LanguageShortName, string DescriptionLong, string DescriptionShort)> existingDescriptions)
    {
        var offerRepository = _portalRepositories.GetInstance<IOfferRepository>();
        offerRepository.AddOfferDescriptions(
            updateDescriptions.ExceptBy(existingDescriptions.Select(d => d.LanguageShortName), updateDescription => updateDescription.LanguageCode)
                .Select(updateDescription => (offerId, updateDescription.LanguageCode, updateDescription.LongDescription, updateDescription.ShortDescription))
        );

        offerRepository.RemoveOfferDescriptions(
            existingDescriptions.ExceptBy(updateDescriptions.Select(d => d.LanguageCode), existingDescription => existingDescription.LanguageShortName)
                .Select(existingDescription => (offerId, existingDescription.LanguageShortName))
        );

        foreach (var update
                 in updateDescriptions
                     .Where(update => existingDescriptions.Any(existing => 
                         existing.LanguageShortName == update.LanguageCode &&
                         (existing.DescriptionLong != update.LongDescription ||
                          existing.DescriptionShort != update.ShortDescription))))
        {
            offerRepository.AttachAndModifyOfferDescription(offerId, update.LanguageCode, offerDescription =>
            {
                offerDescription.DescriptionLong = update.LongDescription;
                offerDescription.DescriptionShort = update.ShortDescription;
            });
        }
    }

    public void CreateOrUpdateOfferLicense(Guid offerId, string licenseText, (Guid OfferLicenseId, string LicenseText, bool AssignedToMultipleOffers) offerLicense)
    {
        var offerRepository = _portalRepositories.GetInstance<IOfferRepository>();
        if (offerLicense == default || offerLicense.LicenseText == licenseText) return;
        
        if (!offerLicense.AssignedToMultipleOffers)
        {
            offerRepository.AttachAndModifyOfferLicense(offerLicense.OfferLicenseId, ol => ol.Licensetext = licenseText);
        }
        else
        {
            offerRepository.RemoveOfferAssignedLicense(offerId, offerLicense.OfferLicenseId);
            var licenseId = offerRepository.CreateOfferLicenses(licenseText).Id;
            offerRepository.CreateOfferAssignedLicense(offerId, licenseId);
        }
    }
    
    /// <inheritdoc/>
    public async Task SubmitOfferAsync(Guid offerId, string iamUserId, OfferTypeId offerTypeId, IEnumerable<NotificationTypeId> notificationTypeIds, IDictionary<string,IEnumerable<string>> companyAdminRoles)
    {
        var offerRepository = _portalRepositories.GetInstance<IOfferRepository>();
        var offerDetails = await offerRepository.GetOfferReleaseDataByIdAsync(offerId, offerTypeId).ConfigureAwait(false);
        if (offerDetails == null)
        {
            throw new NotFoundException($"{offerTypeId} {offerId} does not exist");
        }

        ValidateOfferDetails(offerDetails);

        offerRepository.AttachAndModifyOffer(offerId, offer =>
        {
            offer.OfferStatusId = OfferStatusId.IN_REVIEW;
            offer.DateLastChanged = DateTimeOffset.UtcNow;
        });

        var requesterId = await _portalRepositories.GetInstance<IUserRepository>()
            .GetCompanyUserIdForIamUserUntrackedAsync(iamUserId).ConfigureAwait(false);
        if (requesterId == Guid.Empty)
        {
            throw new ConflictException($"keycloak user ${iamUserId} is not associated with any portal user");
        }            

        var notificationContent = new
        {
            offerId,
            RequestorCompanyName = offerDetails.CompanyName
        };
        
        var serializeNotificationContent = JsonSerializer.Serialize(notificationContent);
        var content = notificationTypeIds.Select(typeId => new ValueTuple<string?, NotificationTypeId>(serializeNotificationContent, typeId));
        await _notificationService.CreateNotifications(companyAdminRoles, requesterId, content).ConfigureAwait(false);
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    private static void ValidateOfferDetails(OfferReleaseData offerDetails)
    {
        if (offerDetails.Name is not null && offerDetails.ThumbnailUrl is not null &&
            offerDetails.SalesManagerId is not null &&
            offerDetails.ProviderCompanyId is not null &&
            offerDetails is { IsDescriptionLongNotSet: false, IsDescriptionShortNotSet: false }) return;
        
        var nullProperties = new List<string>();
        if (offerDetails.Name is null)
        {
            nullProperties.Add($"{nameof(Offer)}.{nameof(offerDetails.Name)}");
        }

        if (offerDetails.ThumbnailUrl is null)
        {
            nullProperties.Add($"{nameof(Offer)}.{nameof(offerDetails.ThumbnailUrl)}");
        }

        if (offerDetails.SalesManagerId is null)
        {
            nullProperties.Add($"{nameof(Offer)}.{nameof(offerDetails.SalesManagerId)}");
        }

        if (offerDetails.ProviderCompanyId is null)
        {
            nullProperties.Add($"{nameof(Offer)}.{nameof(offerDetails.ProviderCompanyId)}");
        }

        if (offerDetails.IsDescriptionLongNotSet)
        {
            nullProperties.Add($"{nameof(Offer)}.{nameof(offerDetails.IsDescriptionLongNotSet)}");
        }

        if (offerDetails.IsDescriptionShortNotSet)
        {
            nullProperties.Add($"{nameof(Offer)}.{nameof(offerDetails.IsDescriptionShortNotSet)}");
        }

        throw new ConflictException($"Missing  : {string.Join(", ", nullProperties)}");
    }

    /// <inheritdoc/>
    public async Task ApproveOfferRequestAsync(Guid offerId, string iamUserId, OfferTypeId offerTypeId, IEnumerable<NotificationTypeId> notificationTypeIds, IDictionary<string,IEnumerable<string>> approveOfferRoles)
    {
        var offerRepository = _portalRepositories.GetInstance<IOfferRepository>();
        var offerDetails = await offerRepository.GetOfferStatusDataByIdAsync(offerId, offerTypeId).ConfigureAwait(false);
        if (offerDetails == default)
        {
            throw new NotFoundException($"Offer {offerId} not found. Either Not Existing or incorrect offer type");
        }
        
        if (!offerDetails.IsStatusInReview)
        {
            throw new ConflictException($"Offer {offerId} is in InCorrect Status");
        }

        if (offerDetails.OfferName is null)
        {
            throw new ConflictException($"Offer {offerId} Name is not yet set.");
        }

        var requesterId = await _portalRepositories.GetInstance<IUserRepository>()
            .GetCompanyUserIdForIamUserUntrackedAsync(iamUserId).ConfigureAwait(false);
        if (requesterId == Guid.Empty)
        {
            throw new ConflictException($"keycloak user ${iamUserId} is not associated with any portal user");
        }

        offerRepository.AttachAndModifyOffer(offerId, offer =>
        {
            offer.OfferStatusId = OfferStatusId.ACTIVE;
        });
        var notificationContent = offerTypeId switch
        {
            OfferTypeId.SERVICE => (object) new
                {
                    OfferId = offerId,
                    ServiceName = offerDetails.OfferName
                },
            OfferTypeId.APP => (object) new
                {
                    OfferId = offerId,
                    AppName = offerDetails.OfferName
                },
            _ => throw new UnexpectedConditionException($"offerTypeId {offerTypeId} is not implemented yet")
        };
        
        var serializeNotificationContent = JsonSerializer.Serialize(notificationContent);
        var content = notificationTypeIds.Select(typeId => new ValueTuple<string?, NotificationTypeId>(serializeNotificationContent, typeId));
        await _notificationService.CreateNotifications(approveOfferRoles, requesterId, content).ConfigureAwait(false);
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task DeclineOfferAsync(Guid offerId, string iamUserId, OfferDeclineRequest data, OfferTypeId offerType, NotificationTypeId notificationTypeId, IDictionary<string,IEnumerable<string>> notificationRecipients, string basePortalAddress)
    {
        var offerRepository = _portalRepositories.GetInstance<IOfferRepository>();
        var declineData = await offerRepository.GetOfferDeclineDataAsync(offerId, iamUserId, offerType).ConfigureAwait(false);

        if (declineData == default)
        {
            throw new NotFoundException($"{offerType} {offerId} does not exist");
        }

        if (!declineData.IsUserOfProvider)
        {
            throw new ForbiddenException($"{offerType} not found. Either not existing or no permission for change.");
        }

        if (declineData.OfferStatus != OfferStatusId.IN_REVIEW)
        {
            throw new ConflictException($"{offerType} must be in status {OfferStatusId.IN_REVIEW}");
        }

        if (string.IsNullOrWhiteSpace(declineData.OfferName))
        {
            throw new ConflictException($"{offerType} name is not set");
        }
        
        if (declineData.CompanyId == null)
        {
            throw new ConflictException($"{offerType} providing company is not set");
        }
        
        offerRepository.AttachAndModifyOffer(offerId, offer =>
        {
            offer.OfferStatusId = OfferStatusId.CREATED;
            offer.DateLastChanged = DateTime.UtcNow;
        });
        
        var requesterId = await _portalRepositories.GetInstance<IUserRepository>()
            .GetCompanyUserIdForIamUserUntrackedAsync(iamUserId).ConfigureAwait(false);
        var notificationContent = new
        {
            declineData.OfferName,
            OfferId = offerId,
            DeclineMessage= data.Message
        };
        
        var serializeNotificationContent = JsonSerializer.Serialize(notificationContent);
        var content = Enumerable.Repeat(notificationTypeId, 1).Select(typeId => new ValueTuple<string?, NotificationTypeId>(serializeNotificationContent, typeId));
        await _notificationService.CreateNotifications(notificationRecipients, requesterId, content).ConfigureAwait(false);
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
        
        var mailParams = new Dictionary<string, string>
        {
            { "offerName", declineData.OfferName },
            { "url", basePortalAddress },
            { "declineMessage", data.Message }
        };
        await _mailingService.SendMails("test@email.com", mailParams, new List<string> { "offer-request-decline" }).ConfigureAwait(false);
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
