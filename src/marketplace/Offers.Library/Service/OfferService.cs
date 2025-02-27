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
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.ErrorHandling;
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
            throw ControllerArgumentException.Create(OfferServiceErrors.INVALID_AGREEMENT, new ErrorParameter[] { new("agreementId", agreementId.ToString()), new("subscriptionId", subscriptionId.ToString()) });
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
            throw ControllerArgumentException.Create(OfferServiceErrors.INVALID_AGREEMENTS, new ErrorParameter[] { new(nameof(subscriptionId), subscriptionId.ToString()) });
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
            throw ControllerArgumentException.Create(OfferServiceErrors.COMPANY_OR_USER_NOT_ASSIGNED, nameof(_identityData.IdentityId));
        }
        if (!result.IsValidOfferSubscription)
        {
            throw NotFoundException.Create(OfferServiceErrors.INVALID_OFFERSUBSCRIPTION, new ErrorParameter[] { new("subscriptionId", subscriptionId.ToString()), new("offerTypeId", offerTypeId.ToString()) });
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
            throw NotFoundException.Create(OfferServiceErrors.CONSENT_NOT_EXIST, new ErrorParameter[] { new(nameof(consentId), consentId.ToString()) });
        }

        return consentDetails;
    }

    public IAsyncEnumerable<AgreementDocumentData> GetOfferTypeAgreements(OfferTypeId offerTypeId, string languageShortName) =>
        portalRepositories.GetInstance<IAgreementRepository>().GetAgreementDataForOfferType(offerTypeId, languageShortName);

    public async Task<OfferAgreementConsent> GetProviderOfferAgreementConsentById(Guid offerId, OfferTypeId offerTypeId)
    {
        var companyId = _identityData.CompanyId;
        var result = await portalRepositories.GetInstance<IAgreementRepository>().GetOfferAgreementConsentById(offerId, companyId, offerTypeId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (result == default)
        {
            throw NotFoundException.Create(OfferServiceErrors.OFFER_OR_OFFERTYPE_NOT_EXIST, new ErrorParameter[] { new("offerId", offerId.ToString()), new("offerTypeId", offerTypeId.ToString()) });
        }
        if (!result.IsProviderCompany)
        {
            throw ForbiddenException.Create(OfferServiceErrors.COMPANY_NOT_ASSIGNED_WITH_OFFER, new ErrorParameter[] { new("companyId", companyId.ToString()), new("offerId", offerId.ToString()) });
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
            throw ControllerArgumentException.Create(OfferServiceErrors.AGREEMENTS_NOT_VALID_FOR_OFFER, new ErrorParameter[] { new ErrorParameter("agreementId", string.Join(",", invalidConsents.Select(consent => consent.AgreementId))), new ErrorParameter("offerId", offerId.ToString()) }, nameof(offerAgreementConsent)));

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
            throw NotFoundException.Create(OfferServiceErrors.OFFER_STATUS_NOT_EXIST, new ErrorParameter[] { new("offerId", offerId.ToString()), new("offerTypeId", offerTypeId.ToString()), new("statusId", statusId.ToString()) });
        }
        if (!result.IsProviderCompany)
        {
            throw ForbiddenException.Create(OfferServiceErrors.COMPANY_NOT_ASSIGNED_WITH_OFFER, new ErrorParameter[] { new("companyId", companyId.ToString()), new("offerId", offerId.ToString()) });
        }
        return result.OfferAgreementConsentUpdate;
    }

    /// <inheritdoc />
    public async Task<Guid> CreateServiceOfferingAsync(ServiceOfferingData data, OfferTypeId offerTypeId)
    {
        if (!data.ServiceTypeIds.Any())
        {
            throw ControllerArgumentException.Create(OfferServiceErrors.SERVICETYPEIDS_NOT_SPECIFIED, nameof(data.ServiceTypeIds));
        }
        if (data.Title.Length < 3)
        {
            throw ControllerArgumentException.Create(OfferServiceErrors.TITLE_TOO_SHORT, nameof(data.Title));
        }
        if (data.SalesManager.HasValue && _identityData.IdentityId != data.SalesManager.Value)
            throw ControllerArgumentException.Create(OfferServiceErrors.SALESMANAGER_NOT_EXIST, nameof(data.SalesManager));

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
    public async Task<OfferProviderResponse> GetProviderOfferDetailsForStatusAsync(Guid offerId, OfferTypeId offerTypeId, DocumentTypeId documentTypeId, string languageShortName)
    {
        var companyId = _identityData.CompanyId;
        var offerDetail = await portalRepositories.GetInstance<IOfferRepository>().GetProviderOfferDataWithConsentStatusAsync(offerId, companyId, offerTypeId, documentTypeId, languageShortName).ConfigureAwait(ConfigureAwaitOptions.None);
        if (offerDetail == default)
        {
            throw NotFoundException.Create(OfferServiceErrors.OFFER_NOT_EXIST, new ErrorParameter[] { new("offerId", offerId.ToString()) });
        }
        if (!offerDetail.IsProviderCompanyUser)
        {
            throw ForbiddenException.Create(OfferServiceErrors.COMPANY_NOT_ASSOCIATED_WITH_PROVIDER, new ErrorParameter[] { new("companyId", companyId.ToString()), new("offerId", offerId.ToString()) });
        }
        if (offerDetail.OfferProviderData == null)
        {
            throw UnexpectedConditionException.Create(OfferServiceErrors.OFFERPROVIDERDATA_NULL);
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
            throw ControllerArgumentException.Create(OfferServiceErrors.INVALID_SALESMANAGERID, new ErrorParameter[] { new(nameof(salesManagerId), salesManagerId.ToString()) });
        }

        if (!responseData.IsSameCompany)
        {
            throw ForbiddenException.Create(OfferServiceErrors.SALESMANAGER_NOT_MEMBER_OF_COMPANY, new ErrorParameter[] { new("companyId", companyId.ToString()) });
        }

        if (userRoleIds.Except(responseData.RoleIds).Any())
        {
            throw ControllerArgumentException.Create(OfferServiceErrors.USER_NOT_SALESMANAGER, new ErrorParameter[] { new(nameof(salesManagerId), salesManagerId.ToString()) });
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
            throw ConflictException.Create(OfferServiceErrors.MANDATORY_DOCUMENT_TYPES_MISSING, new ErrorParameter[] { new("submitAppDocumentTypeIds", string.Join(", ", submitAppDocumentTypeIds)), new("missingDocumentTypes", string.Join(", ", missingDocumentTypes)) });
        }
        if (!offerDetails.HasUserRoles)
        {
            throw ConflictException.Create(OfferServiceErrors.APP_NO_ROLES_ASSIGNED);
        }
        if (!offerDetails.HasPrivacyPolicies)
        {
            throw ConflictException.Create(OfferServiceErrors.PRIVACYPOLICIES_MISSING);
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
            throw NotFoundException.Create(OfferServiceErrors.OFFER_DOES_NOT_EXIST, new ErrorParameter[] { new("offerType", offerTypeId.ToString()), new("offerId", offerId.ToString()) });
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

        throw ConflictException.Create(OfferServiceErrors.MISSING_PROPERTIES, new ErrorParameter[] { new ErrorParameter("nullProperties", string.Join(", ", nullProperties)) });
    }

    /// <inheritdoc/>
    public async Task ApproveOfferRequestAsync(Guid offerId, OfferTypeId offerTypeId, IEnumerable<NotificationTypeId> approveOfferNotificationTypeIds, IEnumerable<UserRoleConfig> approveOfferRoles, IEnumerable<NotificationTypeId> submitOfferNotificationTypeIds, IEnumerable<UserRoleConfig> catenaAdminRoles, (string SubscriptionUrl, string DetailUrl) mailParams, IEnumerable<UserRoleConfig> notificationRecipients)
    {
        var offerRepository = portalRepositories.GetInstance<IOfferRepository>();
        var offerDetails = await offerRepository.GetOfferStatusDataByIdAsync(offerId, offerTypeId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (offerDetails == default)
        {
            throw NotFoundException.Create(OfferServiceErrors.OFFER_INCORRECT_STATUS, new ErrorParameter[] { new("offerId", offerId.ToString()) });
        }

        if (!offerDetails.IsStatusInReview)
        {
            throw ConflictException.Create(OfferServiceErrors.OFFER_IS_INCORRECT_STATUS, new ErrorParameter[] { new("offerId", offerId.ToString()) });
        }

        if (offerDetails.OfferName is null)
        {
            throw ConflictException.Create(OfferServiceErrors.OFFERID_NAME_NOT_SET, new ErrorParameter[] { new("offerId", offerId.ToString()) });
        }

        if (offerDetails.ProviderCompanyId == null)
        {
            throw ConflictException.Create(OfferServiceErrors.OFFERID_PROVIDING_COMPANY_NOT_SET, new ErrorParameter[] { new("offerId", offerId.ToString()) });
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
            _ => throw UnexpectedConditionException.Create(OfferServiceErrors.OFFER_TYPE_NOT_IMPLEMENTED, new ErrorParameter[] { new("offerTypeId", offerTypeId.ToString()) }),
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
            throw NotFoundException.Create(OfferServiceErrors.OFFER_DOES_NOT_EXIST, new ErrorParameter[] { new("offerType", offerType.ToString()), new("offerId", offerId.ToString()) });
        }

        if (declineData.OfferStatus != OfferStatusId.IN_REVIEW)
        {
            throw ConflictException.Create(OfferServiceErrors.OFFER_STATUS_IN_REVIEW, new ErrorParameter[] { new("offerType", offerType.ToString()), new("offerStatusId_IN_REVIEW", OfferStatusId.IN_REVIEW.ToString()) });
        }

        if (string.IsNullOrWhiteSpace(declineData.OfferName))
        {
            throw ConflictException.Create(OfferServiceErrors.OFFER_NAME_NOT_SET, new ErrorParameter[] { new("offerType", offerType.ToString()) });
        }

        if (declineData.CompanyId == null)
        {
            throw ConflictException.Create(OfferServiceErrors.OFFER_PROVIDING_COMPANY_NOT_SET, new ErrorParameter[] { new("offerType", offerType.ToString()) });
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
                throw ControllerArgumentException.Create(OfferServiceErrors.LANGUAGE_CODES_NOT_EXIST, new ErrorParameter[] { new ErrorParameter(nameof(languageCodes), string.Join(",", notFoundLanguageCodes)) });
            }
        }
    }

    public async Task DeactivateOfferIdAsync(Guid offerId, OfferTypeId offerTypeId)
    {
        var offerRepository = portalRepositories.GetInstance<IOfferRepository>();
        var offerData = await offerRepository.GetOfferActiveStatusDataByIdAsync(offerId, offerTypeId, _identityData.CompanyId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (offerData == default)
        {
            throw NotFoundException.Create(OfferServiceErrors.OFFER_DOES_NOT_EXIST, new ErrorParameter[] { new("offerType", offerTypeId.ToString()), new("offerId", offerId.ToString()) });
        }
        if (!offerData.IsUserCompanyProvider)
        {
            throw ForbiddenException.Create(OfferServiceErrors.MISSING_PERMISSION);
        }
        if (!offerData.IsStatusActive)
        {
            throw ConflictException.Create(OfferServiceErrors.OFFERSTATUS_INCORRECT_STATE);
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
            throw NotFoundException.Create(OfferServiceErrors.DOCUMENT_NOT_EXIST, new ErrorParameter[] { new("documentId", documentId.ToString()) });
        }
        if (!result.IsValidDocumentType)
        {
            throw ControllerArgumentException.Create(OfferServiceErrors.DOCUMENTSTATUS_RETRIEVED_NOT_ALLOWED, new ErrorParameter[] { new("documentId", documentId.ToString()) });
        }
        if (!result.IsValidOfferType)
        {
            throw ControllerArgumentException.Create(OfferServiceErrors.DOCUMENT_TYPE_NOT_SUPPORTED, new ErrorParameter[] { new("offerId", offerId.ToString()), new("offerTypeId", offerTypeId.ToString()) });
        }
        if (!result.IsDocumentLinkedToOffer)
        {
            throw ControllerArgumentException.Create(OfferServiceErrors.DOCUMENT_ID_MISMATCH, new ErrorParameter[] { new("documentId", documentId.ToString()), new("offerTypeId", offerTypeId.ToString()), new("offerId", offerId.ToString()) });
        }
        if (result.IsInactive)
        {
            throw ConflictException.Create(OfferServiceErrors.DOCUMENT_INACTIVE, new ErrorParameter[] { new("documentId", documentId.ToString()) });
        }
        if (result.Content == null)
        {
            throw UnexpectedConditionException.Create(OfferServiceErrors.DOCUMENT_CONTENT_NULL);
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
            throw NotFoundException.Create(OfferServiceErrors.DOCUMENT_NOT_EXIST, new ErrorParameter[] { new("documentId", documentId.ToString()) });
        }

        if (!result.IsProviderCompanyUser)
        {
            throw ForbiddenException.Create(OfferServiceErrors.COMPANY_NOT_SAME_AS_DOCUMENT_COMPANY, new ErrorParameter[] { new("companyId", companyId.ToString()), new("documentId", documentId.ToString()) });
        }

        if (!result.OfferData.Any())
        {
            throw ControllerArgumentException.Create(OfferServiceErrors.DOCUMENT_NOT_ASSIGNED_TO_OFFER, new ErrorParameter[] { new("documentId", documentId.ToString()), new("offerTypeId", offerTypeId.ToString()) });
        }

        if (result.OfferData.Count() > 1)
        {
            throw ConflictException.Create(OfferServiceErrors.DOCUMENT_ASSIGNED_TO_MULTIPLE_OFFERS, new ErrorParameter[] { new("documentId", documentId.ToString()), new("offerTypeId", offerTypeId.ToString()) });
        }

        var offer = result.OfferData.Single();
        if (!offer.IsOfferType)
        {
            throw ConflictException.Create(OfferServiceErrors.DOCUMENT_NOT_ASSIGNED_TO_OFFER, new ErrorParameter[] { new("documentId", documentId.ToString()), new("offerTypeId", offerTypeId.ToString()) });
        }

        if (offer.OfferStatusId != OfferStatusId.CREATED)
        {
            throw ConflictException.Create(OfferServiceErrors.OFFER_LOCKED_STATE, new ErrorParameter[] { new("offerTypeId", offerTypeId.ToString()), new("OfferId", offer.OfferId.ToString()) });
        }

        if (!result.IsDocumentTypeMatch)
        {
            throw ControllerArgumentException.Create(OfferServiceErrors.DOCUMENTSTATUS_RETRIEVED_NOT_ALLOWED, new ErrorParameter[] { new("documentId", documentId.ToString()) });
        }

        if (result.DocumentStatusId == DocumentStatusId.LOCKED)
        {
            throw ConflictException.Create(OfferServiceErrors.DOCUMENTSTATUS_DELETION_NOT_ALLOWED, new ErrorParameter[] { new("documentStatusId", result.DocumentStatusId.ToString()) });
        }

        portalRepositories.GetInstance<IOfferRepository>().RemoveOfferAssignedDocument(offer.OfferId, documentId);
        portalRepositories.GetInstance<IDocumentRepository>().RemoveDocument(documentId);
        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public async Task<IEnumerable<TechnicalUserProfileInformation>> GetTechnicalUserProfilesForOffer(Guid offerId, OfferTypeId offerTypeId, IEnumerable<UserRoleConfig> externalUserRolesConfig, IEnumerable<UserRoleConfig> userRolesAccessibleByProviderOnly)
    {
        var companyId = _identityData.CompanyId;
        var result = await portalRepositories.GetInstance<ITechnicalUserProfileRepository>()
            .GetTechnicalUserProfileInformation(offerId, companyId, offerTypeId, externalUserRolesConfig, userRolesAccessibleByProviderOnly).ConfigureAwait(ConfigureAwaitOptions.None);
        if (result == default)
        {
            throw NotFoundException.Create(OfferServiceErrors.OFFER_NOTFOUND, [new("offerId", offerId.ToString())]);
        }

        if (!result.IsUserOfProvidingCompany)
        {
            throw ForbiddenException.Create(OfferServiceErrors.COMPANY_NOT_PROVIDER, [new("companyId", companyId.ToString())]);
        }

        return result.Information
            .Select(x => new TechnicalUserProfileInformation(
                x.TechnicalUserProfileId,
                x.UserRoles
                    .Select(ur => new UserRoleInformation(
                        ur.UserRoleId,
                        ur.UserRoleText,
                        ur.IsExternal ? UserRoleType.External : UserRoleType.Internal,
                        ur.IsProviderOnly))));
    }

    /// <inheritdoc />
    public async Task UpdateTechnicalUserProfiles(Guid offerId, OfferTypeId offerTypeId, IEnumerable<TechnicalUserProfileData> data, string technicalUserProfileClient, IEnumerable<UserRoleConfig> userRolesAccessibleByProviderOnly)
    {
        var companyId = _identityData.CompanyId;
        if (data.Any(x => x.TechnicalUserProfileId == null && !x.UserRoleIds.Any()))
        {
            throw ControllerArgumentException.Create(OfferServiceErrors.NOT_EMPTY_ROLES_AND_PROFILES);
        }

        var technicalUserProfileRepository = portalRepositories.GetInstance<ITechnicalUserProfileRepository>();
        var offerProfileData = await technicalUserProfileRepository.GetOfferProfileData(offerId, offerTypeId, companyId).ConfigureAwait(ConfigureAwaitOptions.None);

        if (offerProfileData == null)
        {
            throw NotFoundException.Create(OfferServiceErrors.OFFER_NOTFOUND, [new("offerId", offerId.ToString())]);
        }

        if (!offerProfileData.IsProvidingCompanyUser)
        {
            throw ForbiddenException.Create(OfferServiceErrors.COMPANY_NOT_PROVIDER, [new("companyId", companyId.ToString())]);
        }

        if (offerProfileData.ServiceTypeIds?.All(x => x == ServiceTypeId.CONSULTANCY_SERVICE) ?? false)
        {
            throw ConflictException.Create(OfferServiceErrors.TECHNICAL_USERS_FOR_CONSULTANCY);
        }

        var roles = await portalRepositories.GetInstance<IUserRolesRepository>()
            .GetRolesForClient(technicalUserProfileClient)
            .ToListAsync()
            .ConfigureAwait(false);

        data.SelectMany(ur => ur.UserRoleIds).Except(roles).IfAny(notExistingRoles =>
            throw ConflictException.Create(OfferServiceErrors.ROLES_DOES_NOT_EXIST, [new("roleIds", string.Join(",", notExistingRoles))]));

        var providerOnlyUserRoles = await portalRepositories.GetInstance<IUserRolesRepository>().GetUserRoleIdsUntrackedAsync(userRolesAccessibleByProviderOnly)
            .ToListAsync()
            .ConfigureAwait(false);

        if (providerOnlyUserRoles.Except(data.SelectMany(ur => ur.UserRoleIds)).Any())
        {
            throw ConflictException.Create(OfferServiceErrors.ROLES_MISSMATCH);
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
    public async Task<OfferProviderSubscriptionDetailData> GetOfferSubscriptionDetailsForProviderAsync(Guid offerId, Guid subscriptionId, OfferTypeId offerTypeId, IEnumerable<UserRoleConfig> contactUserRoles, WalletConfigData walletData)
    {
        var data = await GetOfferSubscriptionDetailsInternal(offerId, subscriptionId, offerTypeId, contactUserRoles, OfferCompanyRole.Provider, portalRepositories.GetInstance<IOfferSubscriptionsRepository>().GetOfferSubscriptionDetailsForProviderAsync)
            .ConfigureAwait(ConfigureAwaitOptions.None);

        return new OfferProviderSubscriptionDetailData(
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
            throw NotFoundException.Create(OfferServiceErrors.SUBSCRIPTION_NOT_FOUND_FOR_OFFER, new ErrorParameter[] { new("subscriptionId", subscriptionId.ToString()), new("offerId", offerId.ToString()), new("offerTypeId", offerTypeId.ToString()) });
        }

        if (!isUserOfCompany)
        {
            throw ForbiddenException.Create(OfferServiceErrors.COMPANY_NOT_PART_OF_ROLE, new ErrorParameter[] { new("companyId", companyId.ToString()), new("offerCompanyRole", offerCompanyRole.ToString()) });
        }

        return details ?? throw UnexpectedConditionException.Create(OfferServiceErrors.DETAILS_SHOULD_NOT_BE_NULL, new ErrorParameter[] { new("offerId", offerId.ToString()), new("subscriptionId", subscriptionId.ToString()), new("companyId", companyId.ToString()), new("offerTypeId", offerTypeId.ToString()) });
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
            throw ConfigurationException.Create(OfferServiceErrors.INVALID_CONFIGURATION_ROLES_NOT_EXIST, new ErrorParameter[] { new ErrorParameter("userRoles", string.Join(", ", userRoles.Select(clientRoles => $"client: {clientRoles.ClientId}, roles: [{string.Join(", ", clientRoles.UserRoleNames)}]"))) });
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
            throw NotFoundException.Create(OfferServiceErrors.SUBSCRIPTION_NOT_EXIST, new ErrorParameter[] { new("subscriptionId", subscriptionId.ToString()) });
        }

        var (status, isSubscribingCompany, _, connectorIds, serviceAccounts) = assignedOfferSubscriptionData;

        if (!isSubscribingCompany)
        {
            throw ForbiddenException.Create(OfferServiceErrors.USER_NOT_BELONG_TO_COMPANY);
        }

        if (status != OfferSubscriptionStatusId.ACTIVE && status != OfferSubscriptionStatusId.PENDING)
        {
            throw ConflictException.Create(OfferServiceErrors.NO_ACTIVE_OR_PENDING_SUBSCRIPTION, new ErrorParameter[] { new("companyId", companyId.ToString()), new("subscriptionId", subscriptionId.ToString()) });
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
