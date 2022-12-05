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

using Org.CatenaX.Ng.Portal.Backend.Offers.Library.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.CatenaX.Ng.Portal.Backend.Offers.Library.Service;

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
    /// <param name="iamUserId">Id of the iam user</param>
    /// <param name="offerTypeId">Id of the offer type</param>
    /// <returns>Returns the id of the created consent</returns>
    Task<Guid> CreateOfferSubscriptionAgreementConsentAsync(Guid subscriptionId,
        Guid agreementId, ConsentStatusId consentStatusId, string iamUserId, OfferTypeId offerTypeId);

    /// <summary>
    /// Updates the existing consent offer subscriptions in the database and creates new entries for the non existing.
    /// </summary>
    /// <param name="subscriptionId">Id of the offer subscription</param>
    /// <param name="offerAgreementConsentData">List of the agreement and status of the consent</param>
    /// <param name="iamUserId">Id of the iam user</param>
    /// <param name="offerTypeId">Id of the offer type</param>
    Task CreateOrUpdateOfferSubscriptionAgreementConsentAsync(Guid subscriptionId,
        IEnumerable<OfferAgreementConsentData> offerAgreementConsentData,
        string iamUserId, OfferTypeId offerTypeId);

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
    IAsyncEnumerable<AgreementData> GetOfferTypeAgreementsAsync(OfferTypeId offerTypeId);
    
    /// <summary>
    /// Return Offer Agreement Consent
    /// </summary>
    /// <param name="offerId"></param>
    /// <param name="iamUserId"></param>
    /// <param name="offerTypeId">OfferTypeId the agreements are associated with</param>
    /// <returns></returns>
    Task<OfferAgreementConsent> GetProviderOfferAgreementConsentById(Guid offerId, string iamUserId, OfferTypeId offerTypeId);
    
    /// <summary>
    /// Create or Update consent to agreements associated with an offer
    /// </summary>
    /// <param name="offerId"></param>
    /// <param name="offerAgreementConsent"></param>
    /// <param name="iamUserId"></param>
    /// <param name="offerTypeId">OfferTypeId the agreements are associated with</param>
    /// <returns></returns>
    Task<int> CreaeteOrUpdateProviderOfferAgreementConsent(Guid offerId, OfferAgreementConsent offerAgreementConsent, string iamUserId, OfferTypeId offerTypeId);

    /// <summary>
    /// Auto setup the service.
    /// </summary>
    /// <param name="data">The offer subscription id and url for the service</param>
    /// <param name="serviceAccountRoles">Roles that will be assigned to the service account</param>
    /// <param name="companyAdminRoles">Roles that will be assigned to the company admin</param>
    /// <param name="iamUserId">Id of the iam user</param>
    /// <param name="offerTypeId">OfferTypeId of offer to be created</param>
    /// <param name="basePortalAddress">Address of the portal</param>
    /// <returns>Returns the response data</returns>
    Task<OfferAutoSetupResponseData> AutoSetupServiceAsync(OfferAutoSetupData data, IDictionary<string,IEnumerable<string>> serviceAccountRoles, IDictionary<string,IEnumerable<string>> companyAdminRoles, string iamUserId, OfferTypeId offerTypeId, string basePortalAddress);

    /// <summary>
    /// Creates a new service offering
    /// </summary>
    /// <param name="data">The data to create the service offering</param>
    /// <param name="iamUserId">the iamUser id</param>
    /// <param name="offerTypeId">Id of the offer type</param>
    /// <returns>The id of the newly created service</returns>
    Task<Guid> CreateServiceOfferingAsync(ServiceOfferingData data, string iamUserId, OfferTypeId offerTypeId);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="offerId"></param>
    /// <param name="userId"></param>
    /// <param name="offerTypeId"></param>
    /// <returns></returns>
    Task<OfferProviderResponse> GetProviderOfferDetailsForStatusAsync(Guid offerId, string userId, OfferTypeId offerTypeId);

    /// <summary>
    /// Checks whether the sales manager has the a sales manager role assigned and is in the same company as the user
    /// </summary>
    /// <param name="salesManagerId">Id of the sales manager</param>
    /// <param name="iamUserId">id of the current user</param>
    /// <param name="salesManagerRoles">the sales manager roles</param>
    /// <returns>Returns the company id of the user</returns>
    Task<Guid> ValidateSalesManager(Guid salesManagerId, string iamUserId, IDictionary<string, IEnumerable<string>> salesManagerRoles);
    
    void UpsertRemoveOfferDescription(Guid offerId, IEnumerable<Localization> updateDescriptions, IEnumerable<(string LanguageShortName, string DescriptionLong, string DescriptionShort)> existingDescriptions);

    void CreateOrUpdateOfferLicense(Guid offerId, string licenseText, (Guid OfferLicenseId, string LicenseText, bool AssignedToMultipleOffers) offerLicense);

    /// <summary>
    /// Declines the given offer
    /// </summary>
    /// <param name="offerId">Id of the offer that should be declined</param>
    /// <param name="iamUserId">Id of the iam User</param>
    /// <param name="data">The offer decline data</param>
    /// <param name="offerType">The offer type</param>
    /// <param name="notificationTypeId">Id of the notification that should be send</param>
    /// <param name="notificationRecipients">Recipients of the notifications</param>
    /// <param name="basePortalAddress">the base portal address</param>
    Task DeclineOfferAsync(Guid offerId, string iamUserId, OfferDeclineRequest data, OfferTypeId offerType, NotificationTypeId notificationTypeId, IDictionary<string,IEnumerable<string>> notificationRecipients, string basePortalAddress);
}
