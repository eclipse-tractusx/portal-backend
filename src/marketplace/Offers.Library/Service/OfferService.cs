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

using Microsoft.AspNetCore.Http;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Mailing.SendMail;
using Org.Eclipse.TractusX.Portal.Backend.Notifications.Library;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Security.Cryptography;
using System.Text.Json;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Extensions;

namespace Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Service;

public class OfferService : IOfferService
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly INotificationService _notificationService;
    private readonly IMailingService _mailingService;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalRepositories">Factory to access the repositories</param>
    /// <param name="notificationService">Creates notifications for the user</param>
    /// <param name="mailingService">Mailing service to send mails to the user</param>
    public OfferService(IPortalRepositories portalRepositories,
        INotificationService notificationService,
        IMailingService mailingService)
    {
        _portalRepositories = portalRepositories;
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

    public IAsyncEnumerable<AgreementDocumentData> GetOfferTypeAgreements(OfferTypeId offerTypeId)=>
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

    public async Task<IEnumerable<ConsentStatusData>> CreateOrUpdateProviderOfferAgreementConsent(Guid offerId, OfferAgreementConsent offerAgreementConsent, string iamUserId, OfferTypeId offerTypeId)
    {
        var (companyUserId, companyId, dbAgreements, requiredAgreementIds) = await GetProviderOfferAgreementConsent(offerId, iamUserId, OfferStatusId.CREATED, offerTypeId).ConfigureAwait(false);
        var invalidConsents = offerAgreementConsent.Agreements.ExceptBy(requiredAgreementIds, consent => consent.AgreementId);
        if (invalidConsents.Any())
        {
            throw new ControllerArgumentException($"agreements {string.Join(",", invalidConsents.Select(consent => consent.AgreementId))} are not valid for offer {offerId}", nameof(offerAgreementConsent));
        }

        return _portalRepositories.GetInstance<IConsentRepository>()
            .AddAttachAndModifyConsents(
                dbAgreements,
                offerAgreementConsent.Agreements,
                offerId,
                companyId,
                companyUserId,
                DateTimeOffset.UtcNow)
            .Select(consent => new ConsentStatusData(consent.Id, consent.ConsentStatusId));
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

    /// <inheritdoc />
    public async Task<Guid> CreateServiceOfferingAsync(ServiceOfferingData data, string iamUserId, OfferTypeId offerTypeId)
    {
        var results = await _portalRepositories.GetInstance<IUserRepository>()
            .GetCompanyUserWithIamUserCheckAndCompanyName(iamUserId, data.SalesManager)
            .ToListAsync().ConfigureAwait(false);

        var iamUserResult = results.Where(x => x.IsIamUser).Select(x => (x.CompanyName, x.CompanyId)).SingleOrDefault();

        if (iamUserResult == default)
            throw new ControllerArgumentException($"IamUser is not assignable to company user {iamUserId}", nameof(iamUserId));

        if (data.SalesManager.HasValue && results.All(x => x.CompanyUserId != data.SalesManager))
            throw new ControllerArgumentException("SalesManager does not exist", nameof(data.SalesManager));

        await CheckLanguageCodesExist(data.Descriptions.Select(x => x.LanguageCode)).ConfigureAwait(false);

        var offerRepository = _portalRepositories.GetInstance<IOfferRepository>();
        var service = offerRepository.CreateOffer(string.Empty, offerTypeId, service =>
        {
            service.ContactEmail = data.ContactEmail;
            service.Name = data.Title;
            service.SalesManagerId = data.SalesManager;
            service.Provider = iamUserResult.CompanyName;
            service.OfferStatusId = OfferStatusId.CREATED;
            service.ProviderCompanyId = iamUserResult.CompanyId;
        });
        var licenseId = offerRepository.CreateOfferLicenses(data.Price).Id;
        offerRepository.CreateOfferAssignedLicense(service.Id, licenseId);
        
        offerRepository.AddServiceAssignedServiceTypes(data.ServiceTypeIds.Select(id => (service.Id, id, id == ServiceTypeId.DATASPACE_SERVICE))); // TODO (PS): Must be refactored, customer needs to define whether the service needs a technical User
        offerRepository.AddOfferDescriptions(data.Descriptions.Select(d =>
            new ValueTuple<Guid, string, string, string>(service.Id, d.LanguageCode, d.LongDescription, d.ShortDescription)));

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
        if (offerDetail.OfferProviderData == null)
        {
            throw new UnexpectedConditionException("offerProviderData should never be null here");
        }

        var data = offerDetail.OfferProviderData;

        return new OfferProviderResponse(
            data.Title,
            data.Provider,
            data.LeadPictureId,
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
            data.SalesManagerId,
            data.PrivacyPolicies,
            data.ServiceTypeIds);
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
    
    public void UpsertRemoveOfferDescription(Guid offerId, IEnumerable<LocalizedDescription> updateDescriptions, IEnumerable<LocalizedDescription> existingDescriptions)
    {
        var offerRepository = _portalRepositories.GetInstance<IOfferRepository>();
        offerRepository.CreateUpdateDeleteOfferDescriptions(offerId, existingDescriptions, 
            updateDescriptions.Select(od => new ValueTuple<string, string, string>(od.LanguageCode, od.LongDescription, od.ShortDescription)));
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
    public async Task SubmitOfferAsync(Guid offerId, string iamUserId, OfferTypeId offerTypeId, IEnumerable<NotificationTypeId> notificationTypeIds, IDictionary<string,IEnumerable<string>> catenaAdminRoles, IEnumerable<DocumentTypeId> submitAppDocumentTypeIds)
    {
        var offerDetails = await GetOfferReleaseData(offerId, offerTypeId).ConfigureAwait(false);

        var isvalidDocumentType = submitAppDocumentTypeIds.All(x=> offerDetails.DocumentTypeIds.Contains(x));
        if (!isvalidDocumentType)
        {
            throw new ConflictException($"{string.Join(",", submitAppDocumentTypeIds)} are mandatory document types");
        }
       
        await SubmitAppServiceAsync(offerId, iamUserId, notificationTypeIds, catenaAdminRoles, offerDetails).ConfigureAwait(false);
    }
    
    /// <inheritdoc/>
    public async Task SubmitServiceAsync(Guid offerId, string iamUserId, OfferTypeId offerTypeId, IEnumerable<NotificationTypeId> notificationTypeIds, IDictionary<string,IEnumerable<string>> catenaAdminRoles)
    {
        var offerDetails = await GetOfferReleaseData(offerId, offerTypeId).ConfigureAwait(false);
        await SubmitAppServiceAsync(offerId, iamUserId, notificationTypeIds, catenaAdminRoles, offerDetails).ConfigureAwait(false);
    }

    private async Task<OfferReleaseData> GetOfferReleaseData(Guid offerId, OfferTypeId offerTypeId)
    {
        var offerRepository = _portalRepositories.GetInstance<IOfferRepository>();
        var offerDetails = await offerRepository.GetOfferReleaseDataByIdAsync(offerId, offerTypeId).ConfigureAwait(false);
        if (offerDetails == null)
        {
            throw new NotFoundException($"{offerTypeId} {offerId} does not exist");
        }
        return offerDetails;
    }

    private async Task SubmitAppServiceAsync(Guid offerId, string iamUserId, IEnumerable<NotificationTypeId> notificationTypeIds, IDictionary<string, IEnumerable<string>> catenaAdminRoles, OfferReleaseData offerDetails)
    {
         GetAndValidateOfferDetails(offerDetails);
        if(offerDetails.DocumentStatusDatas.Any())
        {
            var documentRepository = _portalRepositories.GetInstance<IDocumentRepository>();
            foreach(var documentStatusData in offerDetails.DocumentStatusDatas)
            {
                documentRepository.AttachAndModifyDocument(documentStatusData!.DocumentId,
                a => { a.DocumentStatusId = documentStatusData.StatusId; },
                a => { a.DocumentStatusId = DocumentStatusId.LOCKED; });
            }
        }
        var offerRepository = _portalRepositories.GetInstance<IOfferRepository>();
        offerRepository.AttachAndModifyOffer(offerId, offer =>
        {
            offer.OfferStatusId = OfferStatusId.IN_REVIEW;
            offer.DateLastChanged = DateTimeOffset.UtcNow;
        });

        var requesterId = await _portalRepositories.GetInstance<IUserRepository>()
            .GetCompanyUserIdForIamUserUntrackedAsync(iamUserId).ConfigureAwait(false);
        if (requesterId == Guid.Empty)
        {
            throw new ConflictException($"keycloak user {iamUserId} is not associated with any portal user");
        }            

        var notificationContent = new
        {
            offerId,
            RequestorCompanyName = offerDetails.CompanyName
        };
        
        var serializeNotificationContent = JsonSerializer.Serialize(notificationContent);
        var content = notificationTypeIds.Select(typeId => new ValueTuple<string?, NotificationTypeId>(serializeNotificationContent, typeId));
        await _notificationService.CreateNotifications(catenaAdminRoles, requesterId, content, offerDetails.ProviderCompanyId!.Value).ConfigureAwait(false);
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    private static void GetAndValidateOfferDetails(OfferReleaseData offerDetails)
    {
        if (offerDetails.Name is not null && 
            offerDetails.ProviderCompanyId is not null && 
            offerDetails is { IsDescriptionLongNotSet: false, IsDescriptionShortNotSet: false, HasUserRoles: true }) return;
        
        var nullProperties = new List<string>();
        if (offerDetails.Name is null)
        {
            nullProperties.Add($"{nameof(Offer)}.{nameof(offerDetails.Name)}");
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
        if (!offerDetails.HasUserRoles)
        {
            nullProperties.Add($"{nameof(Offer)}.{nameof(offerDetails.HasUserRoles)}");
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

        if (offerDetails.ProviderCompanyId == null)
        {
            throw new ConflictException($"Offer {offerId} providing company is not yet set.");
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
            offer.DateReleased = DateTime.UtcNow;
        });
        object notificationContent = offerTypeId switch
        {
            OfferTypeId.SERVICE => new
            {
                OfferId = offerId,
                ServiceName = offerDetails.OfferName
            },
            OfferTypeId.APP => new
                {
                    OfferId = offerId,
                    AppName = offerDetails.OfferName
                },
            _ => throw new UnexpectedConditionException($"offerTypeId {offerTypeId} is not implemented yet")
        };
        
        var serializeNotificationContent = JsonSerializer.Serialize(notificationContent);
        var content = notificationTypeIds.Select(typeId => new ValueTuple<string?, NotificationTypeId>(serializeNotificationContent, typeId));
        await _notificationService.CreateNotifications(approveOfferRoles, requesterId, content, offerDetails.ProviderCompanyId.Value).ConfigureAwait(false);
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
        await _notificationService.CreateNotifications(notificationRecipients, requesterId, content, declineData.CompanyId.Value).ConfigureAwait(false);
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
        
        await SendMail(notificationRecipients, declineData.OfferName, basePortalAddress, data.Message, declineData.CompanyId.Value);
    }

    private async Task SendMail(IDictionary<string,IEnumerable<string>> receiverUserRoles, string offerName, string basePortalAddress, string message, Guid companyId)
    {
        var userRolesRepository = _portalRepositories.GetInstance<IUserRolesRepository>();
        var roleData = await userRolesRepository
            .GetUserRoleIdsUntrackedAsync(receiverUserRoles)
            .ToListAsync()
            .ConfigureAwait(false);
        if (roleData.Count < receiverUserRoles.Sum(clientRoles => clientRoles.Value.Count()))
        {
            throw new ConfigurationException(
                $"invalid configuration, at least one of the configured roles does not exist in the database: {string.Join(", ", receiverUserRoles.Select(clientRoles => $"client: {clientRoles.Key}, roles: [{string.Join(", ", clientRoles.Value)}]"))}");
        }

        var companyUserWithRoleIdForCompany = _portalRepositories.GetInstance<IUserRepository>()
            .GetCompanyUserEmailForCompanyAndRoleId(roleData, companyId);
        await foreach (var (receiver, firstName, lastName) in companyUserWithRoleIdForCompany)
        {
            var userName = string.Join(" ", new[] { firstName, lastName }.Where(item => !string.IsNullOrWhiteSpace(item)));            

            var mailParams = new Dictionary<string, string>
            {
                { "offerName", offerName },
                { "url", basePortalAddress },
                { "declineMessage", message },
                { "offerProviderName", !string.IsNullOrWhiteSpace(userName) ? userName : "Service Manager"},
            };
            await _mailingService.SendMails(receiver, mailParams, new List<string> { "offer-request-decline" }).ConfigureAwait(false);
        }
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

    public async Task DeactivateOfferIdAsync(Guid appId, string iamUserId, OfferTypeId offerTypeId)
    {
        var appRepository = _portalRepositories.GetInstance<IOfferRepository>();
        var appdata =  await appRepository.GetOfferActiveStatusDataByIdAsync(appId, offerTypeId, iamUserId).ConfigureAwait(false);
        if(appdata == default)
        {
            throw new NotFoundException($"App {appId} does not exist.");
        }
        if(!appdata.IsUserCompanyProvider)
        {
            throw new ForbiddenException("Missing permission: The user's company does not provide the requested app so they cannot deactivate it.");
        }
        if(!appdata.IsStatusActive)
        {
            throw new ConflictException($"offerStatus is in Incorrect State");
        }
        appRepository.AttachAndModifyOffer(appId, offer => 
            offer.OfferStatusId = OfferStatusId.INACTIVE );
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    public async Task UploadDocumentAsync(Guid Id, DocumentTypeId documentTypeId, IFormFile document, string iamUserId, OfferTypeId offertypeId, IEnumerable<DocumentTypeId> documentTypeIdSettings, IEnumerable<string> contentTypeSettings, CancellationToken cancellationToken)
    {
        if (Id == Guid.Empty)
            throw new ControllerArgumentException($"{offertypeId}id should not be null");

        if (string.IsNullOrEmpty(document.FileName))
            throw new ControllerArgumentException("File name should not be null");

        if (!documentTypeIdSettings.Contains(documentTypeId))
            throw new ControllerArgumentException($"documentType must be either: {string.Join(",", documentTypeIdSettings)}");

        // Check if document is a pdf,jpeg and png file (also see https://www.rfc-editor.org/rfc/rfc3778.txt)
        var documentContentType = document.ContentType;
        if (!contentTypeSettings.Contains(documentContentType))
            throw new UnsupportedMediaTypeException($"Document type not supported. File with contentType :{string.Join(",", contentTypeSettings)} are allowed.");
        
        var offerRepository = _portalRepositories.GetInstance<IOfferRepository>();
        var result = await offerRepository.GetProviderCompanyUserIdForOfferUntrackedAsync(Id, iamUserId, OfferStatusId.CREATED, offertypeId).ConfigureAwait(false);

        if (result == default)
            throw new NotFoundException($"{offertypeId} {Id} does not exist");

        if(!result.IsStatusCreated)
            throw new ConflictException($"offerStatus is in Incorrect State");

        var companyUserId = result.CompanyUserId;
        if (companyUserId == Guid.Empty)
            throw new ForbiddenException($"user {iamUserId} is not a member of the providercompany of {offertypeId} {Id}");

        var documentName = document.FileName;
        using var sha512Hash = SHA512.Create();
        using var ms = new MemoryStream((int)document.Length);

        await document.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
        var hash = sha512Hash.ComputeHash(ms);
        var documentContent = ms.GetBuffer();
        if (ms.Length != document.Length || documentContent.Length != document.Length)
            throw new ControllerArgumentException($"document {document.FileName} transmitted length {document.Length} doesn't match actual length {ms.Length}.");
        
        var doc = _portalRepositories.GetInstance<IDocumentRepository>().CreateDocument(documentName, documentContent, hash, documentContentType.ParseMediaTypeId(), documentTypeId, x =>
        {
            x.CompanyUserId = companyUserId;
        });
        _portalRepositories.GetInstance<IOfferRepository>().CreateOfferAssignedDocument(Id, doc.Id);
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }
}
