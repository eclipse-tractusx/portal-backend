/********************************************************************************
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Identity;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Linq;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Notifications.Library;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.Mailing.Library;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Service;

public class OfferService(
    IPortalRepositories portalRepositories,
    INotificationService notificationService,
    IMailingProcessCreation mailingProcessCreation,
    IIdentityService identityService,
    IOfferSetupService offerSetupService) : IOfferService
{
    private readonly IIdentityData _identityData = identityService.IdentityData;

    /// <inheritdoc />
    public async Task<Guid> CreateOfferSubscriptionAgreementConsentAsync(Guid subscriptionId, Guid agreementId, ConsentStatusId consentStatusId, OfferTypeId offerTypeId)
    {
        var (companyId, offerSubscriptionId, companyUserId) = await GetOfferSubscriptionCompanyAndUserAsync(subscriptionId, offerTypeId).ConfigureAwait(ConfigureAwaitOptions.None);

        if (!await portalRepositories.GetInstance<IAgreementRepository>()
                .CheckAgreementExistsForSubscriptionAsync(agreementId, subscriptionId, offerTypeId).ConfigureAwait(ConfigureAwaitOptions.None))
        {
            throw new ControllerArgumentException($"Invalid Agreement {agreementId} for subscription {subscriptionId}", nameof(agreementId));
        }

        var consent = portalRepositories.GetInstance<IConsentRepository>().CreateConsent(agreementId, companyId, companyUserId, consentStatusId);
        portalRepositories.GetInstance<IConsentAssignedOfferSubscriptionRepository>().CreateConsentAssignedOfferSubscription(consent.Id, offerSubscriptionId);

        await portalRepositories.SaveAsync();
        return consent.Id;
    }

    /// <inheritdoc />
    public async Task CreateOrUpdateOfferSubscriptionAgreementConsentAsync(Guid subscriptionId, IEnumerable<OfferAgreementConsentData> offerAgreementConsentData, OfferTypeId offerTypeId)
    {
        var (companyId, offerSubscriptionId, companyUserId) = await GetOfferSubscriptionCompanyAndUserAsync(subscriptionId, offerTypeId).ConfigureAwait(ConfigureAwaitOptions.None);

        if (!await portalRepositories
                .GetInstance<IAgreementRepository>()
                .CheckAgreementsExistsForSubscriptionAsync(offerAgreementConsentData.Select(x => x.AgreementId), subscriptionId, offerTypeId)
                .ConfigureAwait(ConfigureAwaitOptions.None))
        {
            throw new ControllerArgumentException($"Invalid Agreements for subscription {subscriptionId}", nameof(offerAgreementConsentData));
        }

        var consentAssignedOfferSubscriptionRepository = portalRepositories.GetInstance<IConsentAssignedOfferSubscriptionRepository>();
        var offerSubscriptionConsents = await consentAssignedOfferSubscriptionRepository
            .GetConsentAssignedOfferSubscriptionsForSubscriptionAsync(subscriptionId, offerAgreementConsentData.Select(x => x.AgreementId))
            .ToListAsync()
            .ConfigureAwait(false);

        foreach (var offerSubscriptionConsent in offerSubscriptionConsents)
        {
            var consent = new Consent(offerSubscriptionConsent.ConsentId, Guid.Empty, Guid.Empty, Guid.Empty, offerSubscriptionConsent.ConsentStatusId, default);
            var dbConsent = portalRepositories.Attach(consent);
            dbConsent.ConsentStatusId = offerAgreementConsentData.Single(x => x.AgreementId == offerSubscriptionConsent.AgreementId).ConsentStatusId;
        }

        foreach (var consentData in offerAgreementConsentData.ExceptBy(offerSubscriptionConsents.Select(x => x.AgreementId), consentData => consentData.AgreementId))
        {
            var consent = portalRepositories.GetInstance<IConsentRepository>().CreateConsent(consentData.AgreementId, companyId, companyUserId, consentData.ConsentStatusId);
            consentAssignedOfferSubscriptionRepository.CreateConsentAssignedOfferSubscription(consent.Id, offerSubscriptionId);
        }
    }

    private async Task<(Guid CompanyId, Guid OfferSubscriptionId, Guid CompanyUserId)> GetOfferSubscriptionCompanyAndUserAsync(Guid subscriptionId, OfferTypeId offerTypeId)
    {
        var result = await portalRepositories.GetInstance<IOfferSubscriptionsRepository>()
            .GetCompanyIdWithAssignedOfferForCompanyUserAndSubscriptionAsync(subscriptionId, _identityData.IdentityId, offerTypeId)
            .ConfigureAwait(ConfigureAwaitOptions.None);
        if (result == default)
        {
            throw new ControllerArgumentException("Company or CompanyUser not assigned correctly.", nameof(_identityData.IdentityId));
        }
        if (!result.IsValidOfferSubscription)
        {
            throw new NotFoundException($"Invalid OfferSubscription {subscriptionId} for OfferType {offerTypeId}");
        }
        return (result.CompanyId, subscriptionId, _identityData.IdentityId);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<AgreementData> GetOfferAgreementsAsync(Guid offerId, OfferTypeId offerTypeId) =>
        portalRepositories.GetInstance<IAgreementRepository>().GetOfferAgreementDataForOfferId(offerId, offerTypeId);

    /// <inheritdoc />
    public async Task<ConsentDetailData> GetConsentDetailDataAsync(Guid consentId, OfferTypeId offerTypeId)
    {
        var consentDetails = await portalRepositories.GetInstance<IConsentRepository>()
            .GetConsentDetailData(consentId, offerTypeId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (consentDetails is null)
        {
            throw new NotFoundException($"Consent {consentId} does not exist");
        }

        return consentDetails;
    }

    public IAsyncEnumerable<AgreementDocumentData> GetOfferTypeAgreements(OfferTypeId offerTypeId) =>
        portalRepositories.GetInstance<IAgreementRepository>().GetAgreementDataForOfferType(offerTypeId);

    public async Task<OfferAgreementConsent> GetProviderOfferAgreementConsentById(Guid offerId, OfferTypeId offerTypeId)
    {
        var companyId = _identityData.CompanyId;
        var result = await portalRepositories.GetInstance<IAgreementRepository>().GetOfferAgreementConsentById(offerId, companyId, offerTypeId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (result == default)
        {
            throw new NotFoundException($"offer {offerId}, offertype {offerTypeId} does not exist");
        }
        if (!result.IsProviderCompany)
        {
            throw new ForbiddenException($"Company {companyId} is not assigned with Offer {offerId}");
        }
        return result.OfferAgreementConsent;
    }

    public async Task<IEnumerable<ConsentStatusData>> CreateOrUpdateProviderOfferAgreementConsent(Guid offerId, OfferAgreementConsent offerAgreementConsent, OfferTypeId offerTypeId)
    {
        var (dbAgreements, requiredAgreements) = await GetProviderOfferAgreementConsent(offerId, OfferStatusId.CREATED, offerTypeId).ConfigureAwait(ConfigureAwaitOptions.None);

        offerAgreementConsent.Agreements
            .ExceptBy(
                requiredAgreements.Select(x => x.AgreementId),
                consent => consent.AgreementId)
            .IfAny(invalidConsents =>
                throw new ControllerArgumentException($"agreements {string.Join(",", invalidConsents.Select(consent => consent.AgreementId))} are not valid for offer {offerId}", nameof(offerAgreementConsent)));

        // ignore consents refering to inactive agreements
        var activeAgreements = offerAgreementConsent.Agreements
            .ExceptBy(
                requiredAgreements.Where(x => x.AgreementStatusId == AgreementStatusId.INACTIVE).Select(x => x.AgreementId),
                consent => consent.AgreementId);

        var ConsentStatusdata = portalRepositories.GetInstance<IConsentRepository>()
            .AddAttachAndModifyOfferConsents(
                dbAgreements,
                activeAgreements,
                offerId,
                _identityData.CompanyId,
                _identityData.IdentityId,
                DateTimeOffset.UtcNow)
            .Select(consent => new ConsentStatusData(consent.AgreementId, consent.ConsentStatusId));

        portalRepositories.GetInstance<IOfferRepository>().AttachAndModifyOffer(offerId, offer =>
            offer.DateLastChanged = DateTimeOffset.UtcNow);

        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
        return ConsentStatusdata;
    }

    private async Task<OfferAgreementConsentUpdate> GetProviderOfferAgreementConsent(Guid offerId, OfferStatusId statusId, OfferTypeId offerTypeId)
    {
        var companyId = _identityData.CompanyId;
        var result = await portalRepositories.GetInstance<IAgreementRepository>().GetOfferAgreementConsent(offerId, companyId, statusId, offerTypeId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (result == default)
        {
            throw new NotFoundException($"offer {offerId}, offertype {offerTypeId}, offerStatus {statusId} does not exist");
        }
        if (!result.IsProviderCompany)
        {
            throw new ForbiddenException($"Company {companyId} is not assigned with Offer {offerId}");
        }
        return result.OfferAgreementConsentUpdate;
    }

    /// <inheritdoc />
    public async Task<Guid> CreateServiceOfferingAsync(ServiceOfferingData data, OfferTypeId offerTypeId)
    {
        if (!data.ServiceTypeIds.Any())
        {
            throw new ControllerArgumentException("ServiceTypeIds must be specified", nameof(data.ServiceTypeIds));
        }
        if (data.Title.Length < 3)
        {
            throw new ControllerArgumentException("Title should be at least three character long", nameof(data.Title));
        }
        if (data.SalesManager.HasValue && _identityData.IdentityId != data.SalesManager.Value)
            throw new ControllerArgumentException("SalesManager does not exist", nameof(data.SalesManager));

        await CheckLanguageCodesExist(data.Descriptions.Select(x => x.LanguageCode)).ConfigureAwait(ConfigureAwaitOptions.None);

        var offerRepository = portalRepositories.GetInstance<IOfferRepository>();
        var service = offerRepository.CreateOffer(offerTypeId, _identityData.CompanyId, service =>
        {
            service.ContactEmail = data.ContactEmail;
            service.Name = data.Title;
            service.SalesManagerId = data.SalesManager;
            service.OfferStatusId = OfferStatusId.CREATED;
            service.ProviderCompanyId = _identityData.CompanyId;
            service.MarketingUrl = data.ProviderUri;
            service.LicenseTypeId = LicenseTypeId.COTS;
            service.DateLastChanged = DateTimeOffset.UtcNow;
        });
        var licenseId = offerRepository.CreateOfferLicenses(data.Price).Id;
        offerRepository.CreateOfferAssignedLicense(service.Id, licenseId);

        offerRepository.AddServiceAssignedServiceTypes(data.ServiceTypeIds.Select(id => (service.Id, id)));
        offerRepository.AddOfferDescriptions(data.Descriptions.Select(d =>
            new ValueTuple<Guid, string, string, string>(service.Id, d.LanguageCode, d.LongDescription, d.ShortDescription)));

        await portalRepositories.SaveAsync();
        return service.Id;
    }

    /// <inheritdoc />
    public async Task<OfferProviderResponse> GetProviderOfferDetailsForStatusAsync(Guid offerId, OfferTypeId offerTypeId, DocumentTypeId documentTypeId)
    {
        var companyId = _identityData.CompanyId;
        var offerDetail = await portalRepositories.GetInstance<IOfferRepository>().GetProviderOfferDataWithConsentStatusAsync(offerId, companyId, offerTypeId, documentTypeId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (offerDetail == default)
        {
            throw new NotFoundException($"Offer {offerId} does not exist");
        }
        if (!offerDetail.IsProviderCompanyUser)
        {
            throw new ForbiddenException($"Company {companyId} is not associated with provider-company of offer {offerId}");
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
    public async Task ValidateSalesManager(Guid salesManagerId, IEnumerable<UserRoleConfig> salesManagerRoles)
    {
        var companyId = _identityData.CompanyId;
        var userRoleIds = await portalRepositories.GetInstance<IUserRolesRepository>()
            .GetUserRoleIdsUntrackedAsync(salesManagerRoles).ToListAsync().ConfigureAwait(false);
        var responseData = await portalRepositories.GetInstance<IUserRepository>()
            .GetRolesForCompanyUser(companyId, userRoleIds, salesManagerId)
            .ConfigureAwait(ConfigureAwaitOptions.None);

        if (responseData == default)
        {
            throw new ControllerArgumentException($"invalid salesManagerId {salesManagerId}", nameof(salesManagerId));
        }

        if (!responseData.IsSameCompany)
        {
            throw new ForbiddenException($"SalesManger is not a member of the company {companyId}");
        }

        if (userRoleIds.Except(responseData.RoleIds).Any())
        {
            throw new ControllerArgumentException(
                $"User {salesManagerId} does not have sales Manager Role", nameof(salesManagerId));
        }
    }

    public void UpsertRemoveOfferDescription(Guid offerId, IEnumerable<LocalizedDescription> updateDescriptions, IEnumerable<LocalizedDescription> existingDescriptions)
    {
        var offerRepository = portalRepositories.GetInstance<IOfferRepository>();
        offerRepository.CreateUpdateDeleteOfferDescriptions(offerId, existingDescriptions,
            updateDescriptions.Select(od => (od.LanguageCode, od.LongDescription, od.ShortDescription)));
    }

    public void CreateOrUpdateOfferLicense(Guid offerId, string licenseText, (Guid OfferLicenseId, string LicenseText, bool AssignedToMultipleOffers) offerLicense)
    {
        var offerRepository = portalRepositories.GetInstance<IOfferRepository>();
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
    public async Task SubmitOfferAsync(Guid offerId, OfferTypeId offerTypeId, IEnumerable<NotificationTypeId> notificationTypeIds, IEnumerable<UserRoleConfig> catenaAdminRoles, IEnumerable<DocumentTypeId> submitAppDocumentTypeIds)
    {
        var offerDetails = await GetOfferReleaseData(offerId, offerTypeId).ConfigureAwait(ConfigureAwaitOptions.None);

        var missingDocumentTypes = submitAppDocumentTypeIds.Except(offerDetails.DocumentDatas.Select(data => data.DocumentTypeId));
        if (missingDocumentTypes.Any())
        {
            throw new ConflictException($"{string.Join(", ", submitAppDocumentTypeIds)} are mandatory document types, ({string.Join(", ", missingDocumentTypes)} are missing)");
        }
        if (!offerDetails.HasUserRoles)
        {
            throw new ConflictException("The app has no roles assigned");
        }
        if (!offerDetails.HasPrivacyPolicies)
        {
            throw new ConflictException("PrivacyPolicies is missing for the app");
        }

        await SubmitAppServiceAsync(offerId, notificationTypeIds, catenaAdminRoles, offerDetails).ConfigureAwait(ConfigureAwaitOptions.None);
    }

    /// <inheritdoc/>
    public async Task SubmitServiceAsync(Guid offerId, OfferTypeId offerTypeId, IEnumerable<NotificationTypeId> notificationTypeIds, IEnumerable<UserRoleConfig> catenaAdminRoles)
    {
        var offerDetails = await GetOfferReleaseData(offerId, offerTypeId).ConfigureAwait(ConfigureAwaitOptions.None);
        await SubmitAppServiceAsync(offerId, notificationTypeIds, catenaAdminRoles, offerDetails).ConfigureAwait(ConfigureAwaitOptions.None);
    }

    private async Task<OfferReleaseData> GetOfferReleaseData(Guid offerId, OfferTypeId offerTypeId)
    {
        var offerRepository = portalRepositories.GetInstance<IOfferRepository>();
        var offerDetails = await offerRepository.GetOfferReleaseDataByIdAsync(offerId, offerTypeId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (offerDetails == null)
        {
            throw new NotFoundException($"{offerTypeId} {offerId} does not exist");
        }
        return offerDetails;
    }

    private async Task SubmitAppServiceAsync(Guid offerId, IEnumerable<NotificationTypeId> notificationTypeIds, IEnumerable<UserRoleConfig> catenaAdminRoles, OfferReleaseData offerDetails)
    {
        GetAndValidateOfferDetails(offerDetails);
        var pendingDocuments = offerDetails.DocumentDatas.Where(data => data.StatusId == DocumentStatusId.PENDING);
        if (pendingDocuments.Any())
        {
            portalRepositories.GetInstance<IDocumentRepository>()
                .AttachAndModifyDocuments(
                    pendingDocuments.Select(x => new ValueTuple<Guid, Action<Document>?, Action<Document>>(
                        x.DocumentId,
                        document => document.DocumentStatusId = x.StatusId,
                        document => document.DocumentStatusId = DocumentStatusId.LOCKED)));
        }
        var offerRepository = portalRepositories.GetInstance<IOfferRepository>();
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
        await notificationService.CreateNotifications(catenaAdminRoles, _identityData.IdentityId, content, false).ConfigureAwait(ConfigureAwaitOptions.None);
        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    private static void GetAndValidateOfferDetails(OfferReleaseData offerDetails)
    {
        if (offerDetails.Name is not null &&
            offerDetails is { IsDescriptionLongNotSet: false, IsDescriptionShortNotSet: false })
        {
            return;
        }

        var nullProperties = new List<string>();
        if (offerDetails.Name is null)
        {
            nullProperties.Add($"{nameof(Offer)}.{nameof(offerDetails.Name)}");
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
    public async Task ApproveOfferRequestAsync(Guid offerId, OfferTypeId offerTypeId, IEnumerable<NotificationTypeId> approveOfferNotificationTypeIds, IEnumerable<UserRoleConfig> approveOfferRoles, IEnumerable<NotificationTypeId> submitOfferNotificationTypeIds, IEnumerable<UserRoleConfig> catenaAdminRoles, (string SubscriptionUrl, string DetailUrl) mailParams, IEnumerable<UserRoleConfig> notificationRecipients)
    {
        var offerRepository = portalRepositories.GetInstance<IOfferRepository>();
        var offerDetails = await offerRepository.GetOfferStatusDataByIdAsync(offerId, offerTypeId).ConfigureAwait(ConfigureAwaitOptions.None);
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
            ? await offerSetupService.ActivateSingleInstanceAppAsync(offerId).ConfigureAwait(ConfigureAwaitOptions.None)
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
        await notificationService.CreateNotifications(approveOfferRoles, _identityData.IdentityId, content, offerDetails.ProviderCompanyId.Value).AwaitAll().ConfigureAwait(false);
        await notificationService.SetNotificationsForOfferToDone(catenaAdminRoles, submitOfferNotificationTypeIds, offerId).ConfigureAwait(ConfigureAwaitOptions.None);

        await mailingProcessCreation.RoleBaseSendMail(
            notificationRecipients,
            [
                ("offerName", offerDetails.OfferName),
                ("offerSubscriptionUrl", mailParams.SubscriptionUrl),
                ("offerDetailUrl", $"{mailParams.DetailUrl}/{offerId}"),
                (offerTypeId == OfferTypeId.APP ? "appId" : "serviceId", offerId.ToString())
            ],
            ("offerProviderName", "User"),
            [
                $"{offerTypeId.ToString().ToLower()}-release-activation"
            ],
            offerDetails.ProviderCompanyId.Value).ConfigureAwait(ConfigureAwaitOptions.None);

        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    /// <inheritdoc />
    public async Task DeclineOfferAsync(Guid offerId, OfferDeclineRequest data, OfferTypeId offerType, NotificationTypeId notificationTypeId, IEnumerable<UserRoleConfig> notificationRecipients, string basePortalAddress, IEnumerable<NotificationTypeId> submitOfferNotificationTypeIds, IEnumerable<UserRoleConfig> catenaAdminRoles)
    {
        var offerRepository = portalRepositories.GetInstance<IOfferRepository>();
        var declineData = await offerRepository.GetOfferDeclineDataAsync(offerId, offerType).ConfigureAwait(ConfigureAwaitOptions.None);

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
            portalRepositories.GetInstance<IDocumentRepository>()
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

        var content = new (string?, NotificationTypeId)[]
        {
            (JsonSerializer.Serialize(notificationContent), notificationTypeId)
        };

        await notificationService.CreateNotifications(notificationRecipients, _identityData.IdentityId, content, declineData.CompanyId.Value).AwaitAll().ConfigureAwait(false);
        await notificationService.SetNotificationsForOfferToDone(catenaAdminRoles, submitOfferNotificationTypeIds, offerId).ConfigureAwait(ConfigureAwaitOptions.None);

        await mailingProcessCreation.RoleBaseSendMail(
            notificationRecipients,
            [
                ("offerName", declineData.OfferName),
                ("url", basePortalAddress),
                ("declineMessage", data.Message),
            ],
            ("offerProviderName", "Service Manager"),
            [
                $"{offerType.ToString().ToLower()}-request-decline"
            ],
            declineData.CompanyId.Value).ConfigureAwait(ConfigureAwaitOptions.None);

        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    private async Task CheckLanguageCodesExist(IEnumerable<string> languageCodes)
    {
        if (languageCodes.Any())
        {
            var foundLanguageCodes = await portalRepositories.GetInstance<ILanguageRepository>()
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

    public async Task DeactivateOfferIdAsync(Guid offerId, OfferTypeId offerTypeId)
    {
        var offerRepository = portalRepositories.GetInstance<IOfferRepository>();
        var offerData = await offerRepository.GetOfferActiveStatusDataByIdAsync(offerId, offerTypeId, _identityData.CompanyId).ConfigureAwait(ConfigureAwaitOptions.None);
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
        {
            offer.OfferStatusId = OfferStatusId.INACTIVE;
            offer.DateReleased = DateTime.UtcNow;
        });
        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    /// <inheritdoc/>
    public async Task<(byte[] Content, string ContentType, string FileName)> GetOfferDocumentContentAsync(Guid offerId, Guid documentId, IEnumerable<DocumentTypeId> documentTypeIdSettings, OfferTypeId offerTypeId, CancellationToken cancellationToken)
    {
        var documentRepository = portalRepositories.GetInstance<IDocumentRepository>();
        var result = await documentRepository.GetOfferDocumentContentAsync(offerId, documentId, documentTypeIdSettings, offerTypeId, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
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
    public async Task DeleteDocumentsAsync(Guid documentId, IEnumerable<DocumentTypeId> documentTypeIdSettings, OfferTypeId offerTypeId)
    {
        var companyId = _identityData.CompanyId;
        var result = await portalRepositories.GetInstance<IDocumentRepository>().GetOfferDocumentsAsync(documentId, companyId, documentTypeIdSettings, offerTypeId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (result == default)
        {
            throw new NotFoundException($"Document {documentId} does not exist");
        }

        if (!result.IsProviderCompanyUser)
        {
            throw new ForbiddenException($"Company {companyId} is not the same company of document {documentId}");
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

        portalRepositories.GetInstance<IOfferRepository>().RemoveOfferAssignedDocument(offer.OfferId, documentId);
        portalRepositories.GetInstance<IDocumentRepository>().RemoveDocument(documentId);
        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public async Task<IEnumerable<TechnicalUserProfileInformation>> GetTechnicalUserProfilesForOffer(Guid offerId, OfferTypeId offerTypeId, IEnumerable<UserRoleConfig> externalUserRoles)
    {
        var companyId = _identityData.CompanyId;
        var userRoles = await portalRepositories.GetInstance<IUserRolesRepository>().GetUserRoleIdsUntrackedAsync(externalUserRoles)
            .ToListAsync()
            .ConfigureAwait(false);
        var result = await portalRepositories.GetInstance<ITechnicalUserProfileRepository>()
            .GetTechnicalUserProfileInformation(offerId, companyId, offerTypeId, userRoles).ConfigureAwait(ConfigureAwaitOptions.None);
        if (result == default)
        {
            throw new NotFoundException($"Offer {offerId} does not exist");
        }

        if (!result.IsUserOfProvidingCompany)
        {
            throw new ForbiddenException($"Company {companyId} is not the providing company");
        }

        return result.Information
            .Select(x => new TechnicalUserProfileInformation(
                x.TechnicalUserProfileId,
                x.UserRoles
                    .Select(ur => new UserRoleInformation(
                        ur.UserRoleId,
                        ur.UserRoleText,
                        ur.External ? UserRoleType.External : UserRoleType.Internal))
            ));
    }

    /// <inheritdoc />
    public async Task UpdateTechnicalUserProfiles(Guid offerId, OfferTypeId offerTypeId, IEnumerable<TechnicalUserProfileData> data, string technicalUserProfileClient)
    {
        var companyId = _identityData.CompanyId;
        if (data.Any(x => x.TechnicalUserProfileId == null && !x.UserRoleIds.Any()))
        {
            throw new ControllerArgumentException("Technical User Profiles and Role IDs both should not be empty.");
        }
        var technicalUserProfileRepository = portalRepositories.GetInstance<ITechnicalUserProfileRepository>();
        var offerProfileData = await technicalUserProfileRepository.GetOfferProfileData(offerId, offerTypeId, companyId).ConfigureAwait(ConfigureAwaitOptions.None);
        var roles = await portalRepositories.GetInstance<IUserRolesRepository>()
            .GetRolesForClient(technicalUserProfileClient)
            .ToListAsync()
            .ConfigureAwait(false);

        if (offerProfileData == null)
        {
            throw new NotFoundException($"Offer {offerTypeId} {offerId} does not exist");
        }

        if (!offerProfileData.IsProvidingCompanyUser)
        {
            throw new ForbiddenException($"Company {companyId} is not the providing company");
        }

        if (offerProfileData.ServiceTypeIds?.All(x => x == ServiceTypeId.CONSULTANCY_SERVICE) ?? false)
        {
            throw new ConflictException("Technical User Profiles can't be set for CONSULTANCY_SERVICE");
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
                    data.Where(x => x.TechnicalUserProfileId != null && x.UserRoleIds.Any())
                        .Select(x => x.TechnicalUserProfileId!.Value),
                    x => x.TechnicalUserProfileId)
                .Select(x => x.TechnicalUserProfileId));

        portalRepositories.GetInstance<IOfferRepository>().AttachAndModifyOffer(offerId, offer =>
            offer.DateLastChanged = DateTimeOffset.UtcNow);

        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    /// <inheritdoc />
    public Task<ProviderSubscriptionDetailData> GetSubscriptionDetailsForProviderAsync(Guid offerId, Guid subscriptionId, OfferTypeId offerTypeId, IEnumerable<UserRoleConfig> contactUserRoles) =>
        GetOfferSubscriptionDetailsInternal(offerId, subscriptionId, offerTypeId, contactUserRoles, OfferCompanyRole.Provider, portalRepositories.GetInstance<IOfferSubscriptionsRepository>().GetSubscriptionDetailsForProviderAsync);

    /// <inheritdoc />
    public async Task<AppProviderSubscriptionDetailData> GetAppSubscriptionDetailsForProviderAsync(Guid offerId, Guid subscriptionId, OfferTypeId offerTypeId, IEnumerable<UserRoleConfig> contactUserRoles, WalletConfigData walletData)
    {
        var data = await GetOfferSubscriptionDetailsInternal(offerId, subscriptionId, offerTypeId, contactUserRoles, OfferCompanyRole.Provider, portalRepositories.GetInstance<IOfferSubscriptionsRepository>().GetAppSubscriptionDetailsForProviderAsync)
            .ConfigureAwait(ConfigureAwaitOptions.None);

        return new AppProviderSubscriptionDetailData(
            data.Id,
            data.OfferSubscriptionStatus,
            data.Name,
            data.Customer,
            data.Bpn,
            data.Contact,
            data.TechnicalUserData,
            data.ConnectorData,
            data.TenantUrl,
            data.AppInstanceId,
            data.ProcessSteps.GetProcessStepTypeId(data.Id),
            new SubscriptionExternalServiceData(
                walletData.IssuerDid,
                data.ExternalServiceData?.ParticipantId,
                data.ExternalServiceData == null || data.ExternalServiceData.TrustedIssuer.EndsWith(":holder-iatp") ? data.ExternalServiceData?.TrustedIssuer : $"{data.ExternalServiceData.TrustedIssuer}:holder-iatp",
                walletData.BpnDidResolverUrl,
                walletData.DecentralIdentityManagementAuthUrl,
                data.ExternalServiceData?.DecentralIdentityManagementServiceUrl));
    }

    /// <inheritdoc />
    public Task<SubscriberSubscriptionDetailData> GetSubscriptionDetailsForSubscriberAsync(Guid offerId, Guid subscriptionId, OfferTypeId offerTypeId, IEnumerable<UserRoleConfig> contactUserRoles) =>
        GetOfferSubscriptionDetailsInternal(offerId, subscriptionId, offerTypeId, contactUserRoles, OfferCompanyRole.Subscriber, portalRepositories.GetInstance<IOfferSubscriptionsRepository>().GetSubscriptionDetailsForSubscriberAsync);

    private async Task<R> GetOfferSubscriptionDetailsInternal<R>(Guid offerId, Guid subscriptionId,
        OfferTypeId offerTypeId, IEnumerable<UserRoleConfig> contactUserRoles, OfferCompanyRole offerCompanyRole, Func<Guid, Guid, Guid, OfferTypeId, IEnumerable<Guid>, Task<(bool, bool, R?)>> query)
    {
        var companyId = _identityData.CompanyId;
        var userRoleIds = await ValidateRoleData(contactUserRoles).ConfigureAwait(ConfigureAwaitOptions.None);

        var (exists, isUserOfCompany, details) = await query(offerId, subscriptionId, companyId, offerTypeId, userRoleIds)
            .ConfigureAwait(ConfigureAwaitOptions.None);

        if (!exists)
        {
            throw new NotFoundException($"subscription {subscriptionId} for offer {offerId} of type {offerTypeId} does not exist");
        }

        if (!isUserOfCompany)
        {
            throw new ForbiddenException($"Company {companyId} is not part of the {offerCompanyRole} company");
        }

        return details ?? throw new UnexpectedConditionException($"details for offer {offerId}, subscription {subscriptionId}, company {companyId}, offerType {offerTypeId}, should never be null here");
    }

    private async Task<IEnumerable<Guid>> ValidateRoleData(IEnumerable<UserRoleConfig> userRoles)
    {
        var userRolesRepository = portalRepositories.GetInstance<IUserRolesRepository>();
        var roleData = await userRolesRepository
            .GetUserRoleIdsUntrackedAsync(userRoles)
            .ToListAsync()
            .ConfigureAwait(false);
        if (roleData.Count < userRoles.Select(x => x.UserRoleNames).Sum(clientRoles => clientRoles.Count()))
        {
            throw new ConfigurationException($"invalid configuration, at least one of the configured roles does not exist in the database: {string.Join(", ", userRoles.Select(clientRoles => $"client: {clientRoles.ClientId}, roles: [{string.Join(", ", clientRoles.UserRoleNames)}]"))}");
        }

        return roleData;
    }

    /// <inheritdoc/>
    public async Task<Pagination.Response<OfferSubscriptionStatusDetailData>> GetCompanySubscribedOfferSubscriptionStatusesForUserAsync(int page, int size, OfferTypeId offerTypeId, DocumentTypeId documentTypeId, OfferSubscriptionStatusId? statusId, string? name)
    {
        async Task<Pagination.Source<OfferSubscriptionStatusDetailData>?> GetCompanySubscribedOfferSubscriptionStatusesData(int skip, int take)
        {
            var offerCompanySubscriptionResponse = await portalRepositories.GetInstance<IOfferSubscriptionsRepository>()
                .GetOwnCompanySubscribedOfferSubscriptionStatusAsync(_identityData.CompanyId, offerTypeId, documentTypeId, statusId, name)(skip, take).ConfigureAwait(ConfigureAwaitOptions.None);

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
                            item.OfferSubscriptionId,
                            item.DocumentId == Guid.Empty ? null : item.DocumentId)));
        }
        return await Pagination.CreateResponseAsync(page, size, 15, GetCompanySubscribedOfferSubscriptionStatusesData).ConfigureAwait(ConfigureAwaitOptions.None);
    }
    /// <inheritdoc/>
    public async Task UnsubscribeOwnCompanySubscriptionAsync(Guid subscriptionId)
    {
        var companyId = _identityData.CompanyId;
        var offerSubscriptionsRepository = portalRepositories.GetInstance<IOfferSubscriptionsRepository>();
        var connectorsRepository = portalRepositories.GetInstance<IConnectorsRepository>();
        var userRepository = portalRepositories.GetInstance<IUserRepository>();
        var assignedOfferSubscriptionData = await offerSubscriptionsRepository.GetCompanyAssignedOfferSubscriptionDataForCompanyUserAsync(subscriptionId, companyId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (assignedOfferSubscriptionData == default)
        {
            throw new NotFoundException($"Subscription {subscriptionId} does not exist.");
        }

        var (status, isSubscribingCompany, _, connectorIds, serviceAccounts) = assignedOfferSubscriptionData;

        if (!isSubscribingCompany)
        {
            throw new ForbiddenException("the calling user does not belong to the subscribing company");
        }

        if (status != OfferSubscriptionStatusId.ACTIVE && status != OfferSubscriptionStatusId.PENDING)
        {
            throw new ConflictException($"There is no active or pending subscription for company '{companyId}' and subscriptionId '{subscriptionId}'");
        }

        offerSubscriptionsRepository.AttachAndModifyOfferSubscription(subscriptionId, os =>
        {
            os.OfferSubscriptionStatusId = OfferSubscriptionStatusId.INACTIVE;
        });

        foreach (var cid in connectorIds)
        {
            connectorsRepository.AttachAndModifyConnector(cid, null, con =>
            {
                con.StatusId = ConnectorStatusId.INACTIVE;
            });
        }

        foreach (var sid in serviceAccounts)
        {
            userRepository.AttachAndModifyIdentity(sid, null, iden =>
            {
                iden.UserStatusId = UserStatusId.INACTIVE;
            });
        }

        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }
}
