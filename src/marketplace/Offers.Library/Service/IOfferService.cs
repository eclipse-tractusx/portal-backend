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

using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Service;

/// <summary>
/// Business logic for handling offer-related operations. Includes persistence layer access.
/// </summary>
public interface IOfferService
{
    /// <summary>
    /// Creates new offer agreement consents with the given data for the given offer
    /// </summary>
    /// <param name="subscriptionId">Id of the offer to create the consents for.</param>
    /// <param name="agreementId">id of the agreement the consent is created for</param>
    /// <param name="consentStatusId">consent status</param>
    /// <param name="offerTypeId">Id of the offer type</param>
    /// <returns>Returns the id of the created consent</returns>
    Task<Guid> CreateOfferSubscriptionAgreementConsentAsync(Guid subscriptionId,
        Guid agreementId, ConsentStatusId consentStatusId, OfferTypeId offerTypeId);

    /// <summary>
    /// Updates the existing consent offer subscriptions in the database and creates new entries for the non existing.
    /// </summary>
    /// <param name="subscriptionId">Id of the offer subscription</param>
    /// <param name="offerAgreementConsentData">List of the agreement and status of the consent</param>
    /// <param name="offerTypeId">Id of the offer type</param>
    Task CreateOrUpdateOfferSubscriptionAgreementConsentAsync(Guid subscriptionId,
        IEnumerable<OfferAgreementConsentData> offerAgreementConsentData, OfferTypeId offerTypeId);

    /// <summary>
    /// Gets the offer agreement data
    /// </summary>
    /// <param name="offerId">Id of the offer to get the agreements for</param>
    /// <param name="offerTypeId">Id of the offer type</param>
    /// <returns>Returns IAsyncEnumerable of agreement data</returns>
    IAsyncEnumerable<AgreementData> GetOfferAgreementsAsync(Guid offerId, OfferTypeId offerTypeId);

    /// <summary>
    /// Gets the offer consent detail data
    /// </summary>
    /// <param name="consentId">Id of the offer consent</param>
    /// <param name="offerTypeId"></param>
    /// <returns>Returns the details</returns>
    Task<ConsentDetailData> GetConsentDetailDataAsync(Guid consentId, OfferTypeId offerTypeId);

    /// <summary>
    /// Return Agreements for App_Contract Category
    /// </summary>
    /// <param name="offerTypeId">OfferTypeId the agreement is associated with</param>
    /// <returns></returns>
    IAsyncEnumerable<AgreementDocumentData> GetOfferTypeAgreements(OfferTypeId offerTypeId);

    /// <summary>
    /// Return Offer Agreement Consent
    /// </summary>
    /// <param name="offerId"></param>
    /// <param name="offerTypeId">OfferTypeId the agreements are associated with</param>
    /// <returns></returns>
    Task<OfferAgreementConsent> GetProviderOfferAgreementConsentById(Guid offerId, OfferTypeId offerTypeId);

    /// <summary>
    /// Create or Update consent to agreements associated with an offer
    /// </summary>
    /// <param name="offerId"></param>
    /// <param name="offerAgreementConsent"></param>
    /// <param name="offerTypeId">OfferTypeId the agreements are associated with</param>
    /// <returns></returns>
    Task<IEnumerable<ConsentStatusData>> CreateOrUpdateProviderOfferAgreementConsent(Guid offerId, OfferAgreementConsent offerAgreementConsent, OfferTypeId offerTypeId);

    /// <summary>
    /// Creates a new service offering
    /// </summary>
    /// <param name="data">The data to create the service offering</param>
    /// <param name="offerTypeId">Id of the offer type</param>
    /// <returns>The id of the newly created service</returns>
    Task<Guid> CreateServiceOfferingAsync(ServiceOfferingData data, OfferTypeId offerTypeId);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="offerId"></param>
    /// <param name="offerTypeId"></param>
    /// <returns></returns>
    Task<OfferProviderResponse> GetProviderOfferDetailsForStatusAsync(Guid offerId, OfferTypeId offerTypeId);

    /// <summary>
    /// Checks whether the sales manager has the a sales manager role assigned and is in the same company as the user
    /// </summary>
    /// <param name="salesManagerId">Id of the sales manager</param>
    /// <param name="salesManagerRoles">the sales manager roles</param>
    /// <returns>Returns the company id of the user</returns>
    Task ValidateSalesManager(Guid salesManagerId, IEnumerable<UserRoleConfig> salesManagerRoles);

    void UpsertRemoveOfferDescription(Guid offerId, IEnumerable<LocalizedDescription> updateDescriptions, IEnumerable<LocalizedDescription> existingDescriptions);

    void CreateOrUpdateOfferLicense(Guid offerId, string licenseText, (Guid OfferLicenseId, string LicenseText, bool AssignedToMultipleOffers) offerLicense);

    /// <summary>
    /// Approve App Status from IN_Review to Active
    /// </summary>
    /// <param name="offerId"></param>
    /// <param name="offerTypeId"></param>
    /// <param name="approveOfferNotificationTypeIds"></param>
    /// <param name="approveOfferRoles"></param>
    /// <param name="submitOfferNotificationTypeIds"></param>
    /// <param name="catenaAdminRoles"></param>
    /// <param name="mailParams"></param>
    /// <param name="notificationRecipients"></param>
    /// <returns></returns>
    Task ApproveOfferRequestAsync(Guid offerId, OfferTypeId offerTypeId, IEnumerable<NotificationTypeId> approveOfferNotificationTypeIds, IEnumerable<UserRoleConfig> approveOfferRoles, IEnumerable<NotificationTypeId> submitOfferNotificationTypeIds, IEnumerable<UserRoleConfig> catenaAdminRoles, (string SubscriptionUrl, string DetailUrl) mailParams, IEnumerable<UserRoleConfig> notificationRecipients);

    /// <summary>
    /// Update offer status and create notification for App 
    /// </summary>
    /// <param name="offerId">Id of the offer that should be submitted</param>
    /// <param name="offerTypeId">Type of the offer</param>
    /// <param name="notificationTypeIds">Ids for the notifications that are created</param>
    /// <param name="catenaAdminRoles">Company Admin Roles</param>
    /// <param name="submitAppDocumentTypeIds">Document Type Id</param>
    /// <returns></returns>
    Task SubmitOfferAsync(Guid offerId, OfferTypeId offerTypeId, IEnumerable<NotificationTypeId> notificationTypeIds, IEnumerable<UserRoleConfig> catenaAdminRoles, IEnumerable<DocumentTypeId> submitAppDocumentTypeIds);

    /// <summary>
    /// Declines the given offer
    /// </summary>
    /// <param name="offerId">Id of the offer that should be declined</param>
    /// <param name="data">The offer decline data</param>
    /// <param name="offerType">The offer type</param>
    /// <param name="notificationTypeId">Id of the notification that should be send</param>
    /// <param name="notificationRecipients">Recipients of the notifications</param>
    /// <param name="basePortalAddress">the base portal address</param>
    /// <param name="submitOfferNotificationTypeIds">the submit notification notification type ids</param>
    /// <param name="catenaAdminRoles">The catena x admin roles</param>
    Task DeclineOfferAsync(Guid offerId, OfferDeclineRequest data, OfferTypeId offerType, NotificationTypeId notificationTypeId, IEnumerable<UserRoleConfig> notificationRecipients, string basePortalAddress, IEnumerable<NotificationTypeId> submitOfferNotificationTypeIds, IEnumerable<UserRoleConfig> catenaAdminRoles);

    /// <summary>
    /// Deactivate the given offerStatus by offerId and offerType
    /// </summary>
    /// <param name="offerId">Id of the offer that should be Deactivate</param>
    /// <param name="offerTypeId">Type of the offer</param>
    Task DeactivateOfferIdAsync(Guid offerId, OfferTypeId offerTypeId);

    /// <summary>
    /// Update offer status and create notification for Service
    /// </summary>
    /// <param name="offerId"></param>
    /// <param name="offerTypeId"></param>
    /// <param name="notificationTypeIds"></param>
    /// <param name="catenaAdminRoles"></param>
    /// <returns></returns>
    Task SubmitServiceAsync(Guid offerId, OfferTypeId offerTypeId, IEnumerable<NotificationTypeId> notificationTypeIds, IEnumerable<UserRoleConfig> catenaAdminRoles);

    /// <summary>
    /// Get offer Document Content for given offertypeId by Id
    /// </summary>
    /// <param name="offerId"></param>
    /// <param name="documentId"></param>
    /// <param name="documentTypeIdSettings"></param>
    /// <param name="offerTypeId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<(byte[] Content, string ContentType, string FileName)> GetOfferDocumentContentAsync(Guid offerId, Guid documentId, IEnumerable<DocumentTypeId> documentTypeIdSettings, OfferTypeId offerTypeId, CancellationToken cancellationToken);

    /// <summary>
    /// Get offer Document Content for given offertypeId by Id
    /// </summary>
    /// <param name="documentId"></param>
    /// <param name="documentTypeIdSettings"></param>
    /// <param name="offerTypeId"></param>
    /// <returns></returns>
    Task DeleteDocumentsAsync(Guid documentId, IEnumerable<DocumentTypeId> documentTypeIdSettings, OfferTypeId offerTypeId);

    /// <summary>
    /// Get technical user profiles for a specific offer
    /// </summary>
    /// <param name="offerId">Id of the offer</param>
    /// <param name="offerTypeId">Id of the offer type</param>
    /// <returns>IEnumerable with the technical user profile information</returns>
    Task<IEnumerable<TechnicalUserProfileInformation>> GetTechnicalUserProfilesForOffer(Guid offerId, OfferTypeId offerTypeId);

    /// <summary>
    /// Creates or updates the technical user profiles
    /// </summary>
    /// <param name="offerId">Id of the offer</param>
    /// <param name="offerTypeId">The OfferTypeId of the offer</param>
    /// <param name="data">The technical user profiles</param>
    /// <param name="technicalUserProfileClient">Client to get the technicalUserProfiles</param>
    Task UpdateTechnicalUserProfiles(Guid offerId, OfferTypeId offerTypeId, IEnumerable<TechnicalUserProfileData> data, string technicalUserProfileClient);

    /// <summary>
    /// Gets the information for the subscription for the provider
    /// </summary>
    /// <param name="offerId">Id of the offer</param>
    /// <param name="subscriptionId">Id of the subscription</param>
    /// <param name="offerTypeId">Offer type</param>
    /// <param name="contactUserRoles">The roles of the users that will be listed as contact</param>
    /// <returns>Returns the details of the subscription</returns>
    Task<ProviderSubscriptionDetailData> GetSubscriptionDetailsForProviderAsync(Guid offerId, Guid subscriptionId, OfferTypeId offerTypeId, IEnumerable<UserRoleConfig> contactUserRoles);

    /// <summary>
    /// Gets the information for the subscription for the subscriber
    /// </summary>
    /// <param name="offerId">Id of the offer</param>
    /// <param name="subscriptionId">Id of the subscription</param>
    /// <param name="offerTypeId">Offer type</param>
    /// <param name="contactUserRoles">The roles of the users that will be listed as contact</param>
    /// <returns>Returns the details of the subscription</returns>
    Task<SubscriberSubscriptionDetailData> GetSubscriptionDetailsForSubscriberAsync(Guid offerId, Guid subscriptionId, OfferTypeId offerTypeId, IEnumerable<UserRoleConfig> contactUserRoles);

    /// <summary>
    /// Gets the information of company Subscribed, Subscription Status for user by OfferType
    /// </summary>
    /// <param name="page"></param>
    /// <param name="size"></param>
    /// <param name="offerTypeId"></param>
    /// <param name="documentTypeId"></param>
    /// <returns>Returns the details of the subscription status for user by OfferType</returns>
    Task<Pagination.Response<OfferSubscriptionStatusDetailData>> GetCompanySubscribedOfferSubscriptionStatusesForUserAsync(int page, int size, OfferTypeId offerTypeId, DocumentTypeId documentTypeId);

    /// <summary>
    /// Gets the information for the subscription for the provider
    /// </summary>
    /// <param name="offerId">Id of the offer</param>
    /// <param name="subscriptionId">Id of the subscription</param>
    /// <param name="offerTypeId">Offer type</param>
    /// <param name="contactUserRoles">The roles of the users that will be listed as contact</param>
    /// <returns>Returns the details of the subscription</returns>
    Task<AppProviderSubscriptionDetailData> GetAppSubscriptionDetailsForProviderAsync(Guid offerId, Guid subscriptionId, OfferTypeId offerTypeId, IEnumerable<UserRoleConfig> contactUserRoles);

    /// <summary>
    /// Unsubscribe the Offer subscription by subscriptionId
    /// </summary>
    /// <param name="subscriptionId">Id of the subscription</param>
    Task UnsubscribeOwnCompanySubscriptionAsync(Guid subscriptionId);
}
