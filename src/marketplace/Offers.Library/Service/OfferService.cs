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

using Org.Eclipse.TractusX.Portal.Backend.Framework.Async;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Mailing.SendMail;
using Org.Eclipse.TractusX.Portal.Backend.Notifications.Library;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Service;

public class OfferService : IOfferService
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly INotificationService _notificationService;
    private readonly IMailingService _mailingService;
    private readonly IOfferSetupService _offerSetupService;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalRepositories">Factory to access the repositories</param>
    /// <param name="notificationService">Creates notifications for the user</param>
    /// <param name="mailingService">Mailing service to send mails to the user</param>
    /// <param name="offerSetupService">The offer Setup Service</param>
    public OfferService(IPortalRepositories portalRepositories,
        INotificationService notificationService,
        IMailingService mailingService,
        IOfferSetupService offerSetupService)
    {
        _portalRepositories = portalRepositories;
        _notificationService = notificationService;
        _mailingService = mailingService;
        _offerSetupService = offerSetupService;
    }

    /// <inheritdoc />
    public async Task<Guid> CreateOfferSubscriptionAgreementConsentAsync(Guid subscriptionId,
        Guid agreementId, ConsentStatusId consentStatusId, IdentityData identity, OfferTypeId offerTypeId)
    {
        var (companyId, offerSubscription, companyUserId) = await GetOfferSubscriptionCompanyAndUserAsync(subscriptionId, identity, offerTypeId).ConfigureAwait(false);

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
    public async Task CreateOrUpdateOfferSubscriptionAgreementConsentAsync(Guid subscriptionId, IEnumerable<OfferAgreementConsentData> offerAgreementConsentData, IdentityData identity, OfferTypeId offerTypeId)
    {
        var (companyId, offerSubscription, companyUserId) = await GetOfferSubscriptionCompanyAndUserAsync(subscriptionId, identity, offerTypeId).ConfigureAwait(false);

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
            var consent = new Consent(offerSubscriptionConsent.ConsentId, Guid.Empty, Guid.Empty, Guid.Empty, offerSubscriptionConsent.ConsentStatusId, default);
            var dbConsent = _portalRepositories.Attach(consent);
            dbConsent.ConsentStatusId = offerAgreementConsentData.Single(x => x.AgreementId == offerSubscriptionConsent.AgreementId).ConsentStatusId;
        }

        foreach (var consentData in offerAgreementConsentData.ExceptBy(offerSubscriptionConsents.Select(x => x.AgreementId), consentData => consentData.AgreementId))
        {
            var consent = _portalRepositories.GetInstance<IConsentRepository>().CreateConsent(consentData.AgreementId, companyId, companyUserId, consentData.ConsentStatusId);
            consentAssignedOfferSubscriptionRepository.CreateConsentAssignedOfferSubscription(consent.Id, offerSubscription.Id);
        }
    }

    private async Task<(Guid CompanyId, OfferSubscription OfferSubscription, Guid CompanyUserId)> GetOfferSubscriptionCompanyAndUserAsync(Guid subscriptionId, IdentityData identity, OfferTypeId offerTypeId)
    {
        var result = await _portalRepositories.GetInstance<IOfferSubscriptionsRepository>()
            .GetCompanyIdWithAssignedOfferForCompanyUserAndSubscriptionAsync(subscriptionId, identity.CompanyUserId, offerTypeId)
            .ConfigureAwait(false);
        if (result == default)
        {
            throw new ControllerArgumentException("Company or CompanyUser not assigned correctly.", nameof(identity.UserEntityId));
        }
        var (companyId, offerSubscription) = result;
        if (offerSubscription is null)
        {
            throw new NotFoundException($"Invalid OfferSubscription {subscriptionId} for OfferType {offerTypeId}");
        }
        return (companyId, offerSubscription, identity.CompanyUserId);
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

    public IAsyncEnumerable<AgreementDocumentData> GetOfferTypeAgreements(OfferTypeId offerTypeId) =>
        _portalRepositories.GetInstance<IAgreementRepository>().GetAgreementDataForOfferType(offerTypeId);

    public async Task<OfferAgreementConsent> GetProviderOfferAgreementConsentById(Guid offerId, IdentityData identity, OfferTypeId offerTypeId)
    {
        var result = await _portalRepositories.GetInstance<IAgreementRepository>().GetOfferAgreementConsentById(offerId, identity.CompanyId, offerTypeId).ConfigureAwait(false);
        if (result == default)
        {
            throw new NotFoundException($"offer {offerId}, offertype {offerTypeId} does not exist");
        }
        if (!result.IsProviderCompany)
        {
            throw new ForbiddenException($"UserId {identity.UserEntityId} is not assigned with Offer {offerId}");
        }
        return result.OfferAgreementConsent;
    }

    public async Task<IEnumerable<ConsentStatusData>> CreateOrUpdateProviderOfferAgreementConsent(Guid offerId, OfferAgreementConsent offerAgreementConsent, IdentityData identity, OfferTypeId offerTypeId)
    {
        var (companyUserId, companyId, dbAgreements, requiredAgreementIds) = await GetProviderOfferAgreementConsent(offerId, identity, OfferStatusId.CREATED, offerTypeId).ConfigureAwait(false);
        var invalidConsents = offerAgreementConsent.Agreements.ExceptBy(requiredAgreementIds, consent => consent.AgreementId);
        if (invalidConsents.Any())
        {
            throw new ControllerArgumentException($"agreements {string.Join(",", invalidConsents.Select(consent => consent.AgreementId))} are not valid for offer {offerId}", nameof(offerAgreementConsent));
        }

        var ConsentStatusdata = _portalRepositories.GetInstance<IConsentRepository>()
            .AddAttachAndModifyOfferConsents(
                dbAgreements,
                offerAgreementConsent.Agreements,
                offerId,
                companyId,
                companyUserId,
                DateTimeOffset.UtcNow)
            .Select(consent => new ConsentStatusData(consent.AgreementId, consent.ConsentStatusId));
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
        return ConsentStatusdata;
    }

    private async Task<OfferAgreementConsentUpdate> GetProviderOfferAgreementConsent(Guid offerId, IdentityData identity, OfferStatusId statusId, OfferTypeId offerTypeId)
    {
        var result = await _portalRepositories.GetInstance<IAgreementRepository>().GetOfferAgreementConsent(offerId, identity.CompanyId, statusId, offerTypeId).ConfigureAwait(false);
        if (result == default)
        {
            throw new NotFoundException($"offer {offerId}, offertype {offerTypeId}, offerStatus {statusId} does not exist");
        }
        if (!result.IsProviderCompany)
        {
            throw new ForbiddenException($"UserId {identity.UserEntityId} is not assigned with Offer {offerId}");
        }
        return result.OfferAgreementConsentUpdate;
    }

    /// <inheritdoc />
    public async Task<Guid> CreateServiceOfferingAsync(ServiceOfferingData data, IdentityData identity, OfferTypeId offerTypeId)
    {
        if (!data.ServiceTypeIds.Any())
        {
            throw new ControllerArgumentException("ServiceTypeIds must be specified", nameof(data.ServiceTypeIds));
        }
        if (data.Title.Length < 3)
        {
            throw new ControllerArgumentException("Title should be at least three character long", nameof(data.Title));
        }

        var result = await _portalRepositories.GetInstance<ICompanyRepository>().GetCompanyNameUntrackedAsync(identity.CompanyId).ConfigureAwait(false);
        if (!result.Exists)
        {
            throw new ControllerArgumentException($"No company {identity.CompanyId} found");
        }

        if (data.SalesManager.HasValue && identity.CompanyUserId != data.SalesManager.Value)
            throw new ControllerArgumentException("SalesManager does not exist", nameof(data.SalesManager));

        await CheckLanguageCodesExist(data.Descriptions.Select(x => x.LanguageCode)).ConfigureAwait(false);

        var offerRepository = _portalRepositories.GetInstance<IOfferRepository>();
        var service = offerRepository.CreateOffer(string.Empty, offerTypeId, service =>
        {
            service.ContactEmail = data.ContactEmail;
            service.Name = data.Title;
            service.SalesManagerId = data.SalesManager;
            service.Provider = result.CompanyName;
            service.OfferStatusId = OfferStatusId.CREATED;
            service.ProviderCompanyId = identity.CompanyId;
            service.MarketingUrl = data.ProviderUri;
            service.LicenseTypeId = LicenseTypeId.COTS;
        });
        var licenseId = offerRepository.CreateOfferLicenses(data.Price).Id;
        offerRepository.CreateOfferAssignedLicense(service.Id, licenseId);

        offerRepository.AddServiceAssignedServiceTypes(data.ServiceTypeIds.Select(id => (service.Id, id)));
        offerRepository.AddOfferDescriptions(data.Descriptions.Select(d =>
            new ValueTuple<Guid, string, string, string>(service.Id, d.LanguageCode, d.LongDescription, d.ShortDescription)));

        await _portalRepositories.SaveAsync();
        return service.Id;
    }

    /// <inheritdoc />
    public async Task<OfferProviderResponse> GetProviderOfferDetailsForStatusAsync(Guid offerId, IdentityData identity, OfferTypeId offerTypeId)
    {
        var offerDetail = await _portalRepositories.GetInstance<IOfferRepository>().GetProviderOfferDataWithConsentStatusAsync(offerId, identity.CompanyId, offerTypeId).ConfigureAwait(false);
        if (offerDetail == default)
        {
            throw new NotFoundException($"Offer {offerId} does not exist");
        }
        if (!offerDetail.IsProviderCompanyUser)
        {
            throw new ForbiddenException($"userId {identity.UserEntityId} is not associated with provider-company of offer {offerId}");
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
            data.Documents.GroupBy(d => d.DocumentTypeId).ToDictionary(g => g.Key, g => g.Select(d => new DocumentData(d.DocumentId, d.DocumentName))),
            data.SalesManagerId,
            data.PrivacyPolicies,
            data.ServiceTypeIds,
            data.TechnicalUserProfile.ToDictionary(g => g.TechnicalUserProfileId, g => g.UserRoles));
    }

    /// <inheritdoc />
    public async Task<Guid> ValidateSalesManager(Guid salesManagerId, IdentityData identity, IDictionary<string, IEnumerable<string>> salesManagerRoles)
    {
        var userRoleIds = await _portalRepositories.GetInstance<IUserRolesRepository>()
            .GetUserRoleIdsUntrackedAsync(salesManagerRoles).ToListAsync().ConfigureAwait(false);
        var responseData = await _portalRepositories.GetInstance<IUserRepository>()
            .GetRolesAndCompanyMembershipUntrackedAsync(identity.CompanyId, userRoleIds, salesManagerId)
            .ConfigureAwait(false);
        if (responseData == default)
        {
            throw new ControllerArgumentException($"invalid salesManagerId {salesManagerId}", nameof(salesManagerId));
        }

        if (!responseData.IsSameCompany)
        {
            throw new ForbiddenException($"user {identity.UserEntityId} is not a member of the company");
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
        if (offerLicense == default || offerLicense.LicenseText == licenseText)
            return;

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
    public async Task SubmitOfferAsync(Guid offerId, IdentityData identity, OfferTypeId offerTypeId, IEnumerable<NotificationTypeId> notificationTypeIds, IDictionary<string, IEnumerable<string>> catenaAdminRoles, IEnumerable<DocumentTypeId> submitAppDocumentTypeIds)
    {
        var offerDetails = await GetOfferReleaseData(offerId, offerTypeId).ConfigureAwait(false);

        var missingDocumentTypes = submitAppDocumentTypeIds.Except(offerDetails.DocumentDatas.Select(data => data.DocumentTypeId));
        if (missingDocumentTypes.Any())
        {
            throw new ConflictException($"{string.Join(", ", submitAppDocumentTypeIds)} are mandatory document types, ({string.Join(", ", missingDocumentTypes)} are missing)");
        }
        if (!offerDetails.HasUserRoles)
        {
            throw new ConflictException("The app has no roles assigned");
        }
        if (!offerDetails.HasTechnicalUserProfiles)
        {
            throw new ConflictException("Technical user profile setup is missing for the app");
        }
        if (!offerDetails.HasPrivacyPolicies)
        {
            throw new ConflictException("PrivacyPolicies is missing for the app");
        }

        await SubmitAppServiceAsync(offerId, identity, notificationTypeIds, catenaAdminRoles, offerDetails).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task SubmitServiceAsync(Guid offerId, IdentityData identity, OfferTypeId offerTypeId, IEnumerable<NotificationTypeId> notificationTypeIds, IDictionary<string, IEnumerable<string>> catenaAdminRoles)
    {
        var offerDetails = await GetOfferReleaseData(offerId, offerTypeId).ConfigureAwait(false);
        await SubmitAppServiceAsync(offerId, identity, notificationTypeIds, catenaAdminRoles, offerDetails).ConfigureAwait(false);
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

    private async Task SubmitAppServiceAsync(Guid offerId, IdentityData identity, IEnumerable<NotificationTypeId> notificationTypeIds, IDictionary<string, IEnumerable<string>> catenaAdminRoles, OfferReleaseData offerDetails)
    {
        GetAndValidateOfferDetails(offerDetails);
        var pendingDocuments = offerDetails.DocumentDatas.Where(data => data.StatusId == DocumentStatusId.PENDING);
        if (pendingDocuments.Any())
        {
            _portalRepositories.GetInstance<IDocumentRepository>()
                .AttachAndModifyDocuments(
                    pendingDocuments.Select(x => new ValueTuple<Guid, Action<Document>?, Action<Document>>(
                        x.DocumentId,
                        document => document.DocumentStatusId = x.StatusId,
                        document => document.DocumentStatusId = DocumentStatusId.LOCKED)));
        }
        var offerRepository = _portalRepositories.GetInstance<IOfferRepository>();
        offerRepository.AttachAndModifyOffer(offerId, offer =>
        {
            offer.OfferStatusId = OfferStatusId.IN_REVIEW;
            offer.DateLastChanged = DateTimeOffset.UtcNow;
        });

        var notificationContent = new
        {
            offerId,
            RequestorCompanyName = offerDetails.CompanyName,
            OfferName = offerDetails.Name
        };

        var serializeNotificationContent = JsonSerializer.Serialize(notificationContent);
        var content = notificationTypeIds.Select(typeId => new ValueTuple<string?, NotificationTypeId>(serializeNotificationContent, typeId));
        await _notificationService.CreateNotifications(catenaAdminRoles, identity.CompanyUserId, content, false).ConfigureAwait(false);
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    private static void GetAndValidateOfferDetails(OfferReleaseData offerDetails)
    {
        if (offerDetails.Name is not null &&
            offerDetails.ProviderCompanyId is not null &&
            offerDetails is { IsDescriptionLongNotSet: false, IsDescriptionShortNotSet: false })
        {
            return;
        }

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

        throw new ConflictException($"Missing  : {string.Join(", ", nullProperties)}");
    }

    /// <inheritdoc/>
    public async Task ApproveOfferRequestAsync(Guid offerId, IdentityData identity, OfferTypeId offerTypeId, IEnumerable<NotificationTypeId> approveOfferNotificationTypeIds, IDictionary<string, IEnumerable<string>> approveOfferRoles, IEnumerable<NotificationTypeId> submitOfferNotificationTypeIds, IDictionary<string, IEnumerable<string>> catenaAdminRoles)
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

        offerRepository.AttachAndModifyOffer(offerId, offer =>
        {
            offer.OfferStatusId = OfferStatusId.ACTIVE;
            offer.DateReleased = DateTime.UtcNow;
        });

        var technicalUserIds = offerTypeId == OfferTypeId.APP && offerDetails.IsSingleInstance
            ? await _offerSetupService.ActivateSingleInstanceAppAsync(offerId).ConfigureAwait(false)
            : null;

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
                AppName = offerDetails.OfferName,
                TechnicalUserIds = technicalUserIds
            },
            _ => throw new UnexpectedConditionException($"offerTypeId {offerTypeId} is not implemented yet")
        };

        var serializeNotificationContent = JsonSerializer.Serialize(notificationContent);
        var content = approveOfferNotificationTypeIds.Select(typeId => new ValueTuple<string?, NotificationTypeId>(serializeNotificationContent, typeId));
        await _notificationService.CreateNotifications(approveOfferRoles, identity.CompanyUserId, content, offerDetails.ProviderCompanyId.Value).AwaitAll().ConfigureAwait(false);
        await _notificationService.SetNotificationsForOfferToDone(catenaAdminRoles, submitOfferNotificationTypeIds, offerId).ConfigureAwait(false);

        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task DeclineOfferAsync(Guid offerId, IdentityData identity, OfferDeclineRequest data, OfferTypeId offerType, NotificationTypeId notificationTypeId, IDictionary<string, IEnumerable<string>> notificationRecipients, string basePortalAddress, IEnumerable<NotificationTypeId> submitOfferNotificationTypeIds, IDictionary<string, IEnumerable<string>> catenaAdminRoles)
    {
        var offerRepository = _portalRepositories.GetInstance<IOfferRepository>();
        var declineData = await offerRepository.GetOfferDeclineDataAsync(offerId, offerType).ConfigureAwait(false);

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

        if (declineData.ActiveDocumentStatusDatas.Any())
        {
            _portalRepositories.GetInstance<IDocumentRepository>()
                .AttachAndModifyDocuments(
                    declineData.ActiveDocumentStatusDatas.Select(x => new ValueTuple<Guid, Action<Document>?, Action<Document>>(
                        x.DocumentId,
                        document => document.DocumentStatusId = x.StatusId,
                        document => document.DocumentStatusId = DocumentStatusId.PENDING)));
        }
        var notificationContent = new
        {
            declineData.OfferName,
            OfferId = offerId,
            DeclineMessage = data.Message
        };

        var serializeNotificationContent = JsonSerializer.Serialize(notificationContent);
        var content = Enumerable.Repeat(notificationTypeId, 1).Select(typeId => new ValueTuple<string?, NotificationTypeId>(serializeNotificationContent, typeId));
        await _notificationService.CreateNotifications(notificationRecipients, identity.CompanyUserId, content, declineData.CompanyId.Value).AwaitAll().ConfigureAwait(false);
        await _notificationService.SetNotificationsForOfferToDone(catenaAdminRoles, submitOfferNotificationTypeIds, offerId).ConfigureAwait(false);

        await _portalRepositories.SaveAsync().ConfigureAwait(false);

        await SendMail(notificationRecipients, declineData.OfferName, basePortalAddress, data.Message, declineData.CompanyId.Value);
    }

    private async Task SendMail(IDictionary<string, IEnumerable<string>> receiverUserRoles, string offerName, string basePortalAddress, string message, Guid companyId)
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

    public async Task DeactivateOfferIdAsync(Guid offerId, IdentityData identity, OfferTypeId offerTypeId)
    {
        var offerRepository = _portalRepositories.GetInstance<IOfferRepository>();
        var offerData = await offerRepository.GetOfferActiveStatusDataByIdAsync(offerId, offerTypeId, identity.CompanyId).ConfigureAwait(false);
        if (offerData == default)
        {
            throw new NotFoundException($"{offerTypeId} {offerId} does not exist.");
        }
        if (!offerData.IsUserCompanyProvider)
        {
            throw new ForbiddenException("Missing permission: The user's company does not provide the requested app so they cannot deactivate it.");
        }
        if (!offerData.IsStatusActive)
        {
            throw new ConflictException($"offerStatus is in Incorrect State");
        }
        offerRepository.AttachAndModifyOffer(offerId, offer =>
            offer.OfferStatusId = OfferStatusId.INACTIVE);
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<(byte[] Content, string ContentType, string FileName)> GetOfferDocumentContentAsync(Guid offerId, Guid documentId, IEnumerable<DocumentTypeId> documentTypeIdSettings, OfferTypeId offerTypeId, CancellationToken cancellationToken)
    {
        var documentRepository = _portalRepositories.GetInstance<IDocumentRepository>();
        var result = await documentRepository.GetOfferDocumentContentAsync(offerId, documentId, documentTypeIdSettings, offerTypeId, cancellationToken).ConfigureAwait(false);
        if (result is null)
        {
            throw new NotFoundException($"document {documentId} does not exist");
        }
        if (!result.IsValidDocumentType)
        {
            throw new ControllerArgumentException($"Document {documentId} can not get retrieved. Document type not supported.");
        }
        if (!result.IsValidOfferType)
        {
            throw new ControllerArgumentException($"offer {offerId} is not an {offerTypeId}");
        }
        if (!result.IsDocumentLinkedToOffer)
        {
            throw new ControllerArgumentException($"Document {documentId} and {offerTypeId} id {offerId} do not match.");
        }
        if (result.IsInactive)
        {
            throw new ConflictException($"Document {documentId} is in status INACTIVE");
        }
        if (result.Content == null)
        {
            throw new UnexpectedConditionException($"document content should never be null");
        }
        return (result.Content, result.MediaTypeId.MapToMediaType(), result.FileName);
    }

    /// <inheritdoc/>
    public async Task DeleteDocumentsAsync(Guid documentId, IdentityData identity, IEnumerable<DocumentTypeId> documentTypeIdSettings, OfferTypeId offerTypeId)
    {
        var result = await _portalRepositories.GetInstance<IDocumentRepository>().GetOfferDocumentsAsync(documentId, identity.CompanyId, documentTypeIdSettings, offerTypeId).ConfigureAwait(false);
        if (result == default)
        {
            throw new NotFoundException($"Document {documentId} does not exist");
        }

        if (!result.IsProviderCompanyUser)
        {
            throw new ForbiddenException($"user {identity.UserEntityId} is not a member of the same company of document {documentId}");
        }

        if (!result.OfferData.Any())
        {
            throw new ControllerArgumentException($"Document {documentId} is not assigned to an {offerTypeId}");
        }

        if (result.OfferData.Count() > 1)
        {
            throw new ConflictException($"Document {documentId} is assigned to more than one {offerTypeId}");
        }

        var offer = result.OfferData.Single();
        if (!offer.IsOfferType)
        {
            throw new ConflictException($"Document {documentId} is not assigned to an {offerTypeId}");
        }

        if (offer.OfferStatusId != OfferStatusId.CREATED)
        {
            throw new ConflictException($"{offerTypeId} {offer.OfferId} is in locked state");
        }

        if (!result.IsDocumentTypeMatch)
        {
            throw new ControllerArgumentException($"Document {documentId} can not get retrieved. Document type not supported");
        }

        if (result.DocumentStatusId == DocumentStatusId.LOCKED)
        {
            throw new ConflictException($"Document in State {result.DocumentStatusId} can't be deleted");
        }

        _portalRepositories.GetInstance<IOfferRepository>().RemoveOfferAssignedDocument(offer.OfferId, documentId);
        _portalRepositories.GetInstance<IDocumentRepository>().RemoveDocument(documentId);
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    public async Task<IEnumerable<TechnicalUserProfileInformation>> GetTechnicalUserProfilesForOffer(Guid offerId, IdentityData identity, OfferTypeId offerTypeId)
    {
        var result = await _portalRepositories.GetInstance<ITechnicalUserProfileRepository>()
            .GetTechnicalUserProfileInformation(offerId, identity.CompanyId, offerTypeId).ConfigureAwait(false);
        if (result == default)
        {
            throw new NotFoundException($"Offer {offerId} does not exist");
        }

        if (!result.IsUserOfProvidingCompany)
        {
            throw new ForbiddenException($"User {identity.UserEntityId} is not in providing company");
        }

        return result.Information;
    }

    /// <inheritdoc />
    public async Task UpdateTechnicalUserProfiles(Guid offerId, OfferTypeId offerTypeId, IEnumerable<TechnicalUserProfileData> data, IdentityData identity, string technicalUserProfileClient)
    {
        var technicalUserProfileRepository = _portalRepositories.GetInstance<ITechnicalUserProfileRepository>();
        var offerProfileData = await technicalUserProfileRepository.GetOfferProfileData(offerId, offerTypeId, identity.CompanyId).ConfigureAwait(false);
        var roles = await _portalRepositories.GetInstance<IUserRolesRepository>()
            .GetRolesForClient(technicalUserProfileClient)
            .ToListAsync()
            .ConfigureAwait(false);

        if (offerProfileData == null)
        {
            throw new NotFoundException($"Offer {offerTypeId} {offerId} does not exist");
        }

        if (!offerProfileData.IsProvidingCompanyUser)
        {
            throw new ForbiddenException($"User {identity.UserEntityId} is not in providing company");
        }

        if (offerProfileData.ServiceTypeIds?.All(x => x == ServiceTypeId.CONSULTANCE_SERVICE) ?? false)
        {
            throw new ConflictException("Technical User Profiles can't be set for CONSULTANCE_SERVICE");
        }

        var notExistingRoles = data.SelectMany(ur => ur.UserRoleIds).Except(roles);
        if (notExistingRoles.Any())
        {
            throw new ConflictException($"Roles {string.Join(",", notExistingRoles)} do not exist");
        }

        technicalUserProfileRepository.CreateDeleteTechnicalUserProfileAssignedRoles(
            offerProfileData.ProfileData.SelectMany(profileData => profileData.UserRoleIds.Select(roleId => (profileData.TechnicalUserProfileId, roleId))).ToList(),
            data.Select(profileData => (
                    TechnicalUserProfileId: profileData.TechnicalUserProfileId == null
                        ? technicalUserProfileRepository.CreateTechnicalUserProfile(Guid.NewGuid(), offerId).Id
                        : profileData.TechnicalUserProfileId.Value,
                    profileData.UserRoleIds))
                .SelectMany(data => data.UserRoleIds.Select(roleId => (data.TechnicalUserProfileId, roleId))).ToList());

        technicalUserProfileRepository.RemoveTechnicalUserProfiles(
            offerProfileData.ProfileData
                .ExceptBy(
                    data.Where(x => x.TechnicalUserProfileId != null)
                        .Select(x => x.TechnicalUserProfileId!.Value),
                    x => x.TechnicalUserProfileId)
                .Select(x => x.TechnicalUserProfileId));

        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<ProviderSubscriptionDetailData> GetSubscriptionDetailsForProviderAsync(Guid offerId, Guid subscriptionId, IdentityData identity, OfferTypeId offerTypeId, IDictionary<string, IEnumerable<string>> contactUserRoles)
    {
        var details = await GetOfferSubscriptionDetailsInternal(offerId, subscriptionId, identity, offerTypeId, contactUserRoles, OfferCompanyRole.Provider);
        return new ProviderSubscriptionDetailData(
            details.Id,
            details.OfferSubscriptionStatus,
            details.Name,
            details.CompanyName,
            details.Bpn,
            details.Contact,
            details.TechnicalUserData);
    }

    /// <inheritdoc />
    public async Task<SubscriberSubscriptionDetailData> GetSubscriptionDetailsForSubscriberAsync(Guid offerId, Guid subscriptionId, IdentityData identity, OfferTypeId offerTypeId, IDictionary<string, IEnumerable<string>> contactUserRoles)
    {
        var details = await GetOfferSubscriptionDetailsInternal(offerId, subscriptionId, identity, offerTypeId, contactUserRoles, OfferCompanyRole.Subscriber);
        return new SubscriberSubscriptionDetailData(
            details.Id,
            details.OfferSubscriptionStatus,
            details.Name,
            details.CompanyName,
            details.Contact,
            details.TechnicalUserData);
    }

    private async Task<OfferSubscriptionDetailData> GetOfferSubscriptionDetailsInternal(Guid offerId, Guid subscriptionId, IdentityData identity,
        OfferTypeId offerTypeId, IDictionary<string, IEnumerable<string>> contactUserRoles, OfferCompanyRole offerCompanyRole)
    {
        var userRoleIds = await ValidateRoleData(contactUserRoles).ConfigureAwait(false);

        var (exists, isUserOfCompany, details) = await _portalRepositories.GetInstance<IOfferSubscriptionsRepository>()
            .GetSubscriptionDetailsAsync(offerId, subscriptionId, identity.CompanyId, offerTypeId, userRoleIds, offerCompanyRole == OfferCompanyRole.Provider)
            .ConfigureAwait(false);

        if (!exists)
        {
            throw new NotFoundException($"subscription {subscriptionId} for offer {offerId} of type {offerTypeId} does not exist");
        }

        if (!isUserOfCompany)
        {
            throw new ForbiddenException($"User {identity.UserEntityId} is not part of the {offerCompanyRole} company");
        }

        return details;
    }

    private async Task<IEnumerable<Guid>> ValidateRoleData(IDictionary<string, IEnumerable<string>> userRoles)
    {
        var userRolesRepository = _portalRepositories.GetInstance<IUserRolesRepository>();
        var roleData = await userRolesRepository
            .GetUserRoleIdsUntrackedAsync(userRoles)
            .ToListAsync()
            .ConfigureAwait(false);
        if (roleData.Count < userRoles.Sum(clientRoles => clientRoles.Value.Count()))
        {
            throw new ConfigurationException($"invalid configuration, at least one of the configured roles does not exist in the database: {string.Join(", ", userRoles.Select(clientRoles => $"client: {clientRoles.Key}, roles: [{string.Join(", ", clientRoles.Value)}]"))}");
        }
        return roleData;
    }

    /// <inheritdoc/>
    public async Task<Pagination.Response<OfferSubscriptionStatusDetailData>> GetCompanySubscribedOfferSubscriptionStatusesForUserAsync(int page, int size, IdentityData identity, OfferTypeId offerTypeId, DocumentTypeId documentTypeId)
    {
        async Task<Pagination.Source<OfferSubscriptionStatusDetailData>?> GetCompanySubscribedOfferSubscriptionStatusesData(int skip, int take)
        {
            var offerCompanySubscriptionResponse = await _portalRepositories.GetInstance<IOfferSubscriptionsRepository>()
                .GetOwnCompanySubscribedOfferSubscriptionStatusesUntrackedAsync(identity.CompanyId, offerTypeId, documentTypeId)(skip, take).ConfigureAwait(false);

            return offerCompanySubscriptionResponse == null
                ? null
                : new Pagination.Source<OfferSubscriptionStatusDetailData>(
                    offerCompanySubscriptionResponse.Count,
                    offerCompanySubscriptionResponse.Data.Select(item =>
                        new OfferSubscriptionStatusDetailData(
                            item.OfferId,
                            item.OfferName,
                            item.Provider,
                            item.OfferSubscriptionStatusId,
                            item.DocumentId == Guid.Empty ? null : item.DocumentId)));
        }
        return await Pagination.CreateResponseAsync(page, size, 15, GetCompanySubscribedOfferSubscriptionStatusesData).ConfigureAwait(false);
    }
}
