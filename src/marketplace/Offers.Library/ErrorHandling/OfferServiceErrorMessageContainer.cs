/********************************************************************************
 * Copyright (c) 2024 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Service;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Offers.Library.ErrorHandling;

public class OfferServiceErrorMessageContainer : IErrorMessageContainer
{
    private static readonly IReadOnlyDictionary<int, string> _messageContainer = ImmutableDictionary.CreateRange<int, string>([
        new((int)OfferServiceErrors.OFFER_NOTFOUND, "Offer {offerId} does not exist"),
        new((int)OfferServiceErrors.COMPANY_NOT_PROVIDER, "Company {companyId} is not the providing company"),
        new((int)OfferServiceErrors.NOT_EMPTY_ROLES_AND_PROFILES, "Technical User Profiles and Role IDs both should not be empty."),
        new((int)OfferServiceErrors.TECHNICAL_USERS_FOR_CONSULTANCY, "Technical User Profiles can't be set for CONSULTANCY_SERVICE"),
        new((int)OfferServiceErrors.ROLES_DOES_NOT_EXIST, "Roles {roleIds} do not exist"),
        new((int)OfferServiceErrors.ROLES_MISSMATCH, "Roles must either be provider only or visible for provider and subscriber"),
        new((int)OfferServiceErrors.INVALID_AGREEMENT, "Invalid Agreement {agreementId} for subscription {subscriptionId}"),
        new((int)OfferServiceErrors.INVALID_AGREEMENTS, "Invalid Agreements for subscription {subscriptionId}"),
        new((int)OfferServiceErrors.COMPANY_OR_USER_NOT_ASSIGNED, "Company or CompanyUser not assigned correctly."),
        new((int)OfferServiceErrors.INVALID_OFFERSUBSCRIPTION, "Invalid OfferSubscription {subscriptionId} for OfferType {offerTypeId}"),
        new((int)OfferServiceErrors.CONSENT_NOT_EXIST, "Consent {consentId} does not exist"),
        new((int)OfferServiceErrors.OFFER_OR_OFFERTYPE_NOT_EXIST, "offer {offerId}, offertype {offerTypeId} does not exist"),
        new((int)OfferServiceErrors.AGREEMENTS_NOT_VALID_FOR_OFFER, "agreements {agreementId} are not valid for offer {offerId}"),
        new((int)OfferServiceErrors.OFFER_STATUS_NOT_EXIST, "offer {offerId}, offertype {offerTypeId}, offerStatus {statusId} does not exist"),
        new((int)OfferServiceErrors.COMPANY_NOT_ASSIGNED_WITH_OFFER, "Company {companyId} is not assigned with Offer {offerId}"),
        new((int)OfferServiceErrors.SERVICETYPEIDS_NOT_SPECIFIED, "ServiceTypeIds must be specified"),
        new((int)OfferServiceErrors.TITLE_TOO_SHORT, "Title should be at least three characters long"),
        new((int)OfferServiceErrors.SALESMANAGER_NOT_EXIST, "SalesManager does not exist"),
        new((int)OfferServiceErrors.OFFER_NOT_EXIST, "Offer {offerId} does not exist"),
        new((int)OfferServiceErrors.COMPANY_NOT_ASSOCIATED_WITH_PROVIDER, "Company {companyId} is not associated with provider-company of offer {offerId}"),
        new((int)OfferServiceErrors.OFFERPROVIDERDATA_NULL, "OfferProviderData should never be null here"),
        new((int)OfferServiceErrors.INVALID_SALESMANAGERID, "invalid salesManagerId {salesManagerId}"),
        new((int)OfferServiceErrors.SALESMANAGER_NOT_MEMBER_OF_COMPANY, "SalesManger is not a member of the company {companyId}"),
        new((int)OfferServiceErrors.USER_NOT_SALESMANAGER, "User {salesManagerId} does not have sales Manager Role"),
        new((int)OfferServiceErrors.MANDATORY_DOCUMENT_TYPES_MISSING, "{submitAppDocumentTypeIds} are mandatory document types, ({missingDocumentTypes} are missing)"),
        new((int)OfferServiceErrors.APP_NO_ROLES_ASSIGNED, "The app has no roles assigned"),
        new((int)OfferServiceErrors.PRIVACYPOLICIES_MISSING, "PrivacyPolicies is missing for the app"),
        new((int)OfferServiceErrors.OFFER_DOES_NOT_EXIST, "{offerType} {offerId} does not exist"),
        new((int)OfferServiceErrors.MISSING_PROPERTIES, "Missing: {nullProperties}"),
        new((int)OfferServiceErrors.OFFER_IS_INCORRECT_STATUS, "Offer {offerId} is in InCorrect Status"),
        new((int)OfferServiceErrors.OFFER_INCORRECT_STATUS, "Offer {offerId} not found. Either Not Existing or incorrect offer type"),
        new((int)OfferServiceErrors.OFFER_STATUS_IN_REVIEW, "{offerType} must be in status IN_REVIEW"),
        new((int)OfferServiceErrors.OFFERID_PROVIDING_COMPANY_NOT_SET, "Offer {offerId} providing company is not yet set."),
        new((int)OfferServiceErrors.OFFERID_NAME_NOT_SET, "Offer {offerId} Name is not yet set."),
        new((int)OfferServiceErrors.OFFER_NAME_NOT_SET, "{offerType} name is not set"),
        new((int)OfferServiceErrors.OFFER_PROVIDING_COMPANY_NOT_SET, "{offerType} providing company is not set"),
        new((int)OfferServiceErrors.OFFER_TYPE_NOT_IMPLEMENTED, "offerTypeId {offerTypeId} is not implemented yet"),
        new((int)OfferServiceErrors.LANGUAGE_CODES_NOT_EXIST, "Language code(s) {languageCodes} do(es) not exist"),
        new((int)OfferServiceErrors.MISSING_PERMISSION, "Missing permission: The user's company does not provide the requested app so they cannot deactivate it."),
        new((int)OfferServiceErrors.OFFERSTATUS_INCORRECT_STATE, "OfferStatus is in Incorrect State"),
        new((int)OfferServiceErrors.DOCUMENT_NOT_EXIST, "Document {documentId} does not exist"),
        new((int)OfferServiceErrors.DOCUMENT_TYPE_NOT_SUPPORTED, "offer {offerId} is not an {offerTypeId}"),
        new((int)OfferServiceErrors.DOCUMENT_ID_MISMATCH, "Document {documentId} and {offerTypeId} id {offerId} do not match."),
        new((int)OfferServiceErrors.DOCUMENT_INACTIVE, "Document {documentId} is in status INACTIVE"),
        new((int)OfferServiceErrors.DOCUMENT_CONTENT_NULL, "Document content should never be null"),
        new((int)OfferServiceErrors.COMPANY_NOT_SAME_AS_DOCUMENT_COMPANY, "Company {companyId} is not the same company of document {documentId}"),
        new((int)OfferServiceErrors.DOCUMENT_NOT_ASSIGNED_TO_OFFER, "Document {documentId} is not assigned to an {offerTypeId}"),
        new((int)OfferServiceErrors.DOCUMENT_ASSIGNED_TO_MULTIPLE_OFFERS, "Document {documentId} is assigned to more than one {offerTypeId}"),
        new((int)OfferServiceErrors.OFFER_LOCKED_STATE, "{offerTypeId} {offerId} is in locked state"),
        new((int)OfferServiceErrors.DOCUMENTSTATUS_RETRIEVED_NOT_ALLOWED, "Document {documentId} can not get retrieved. Document type not supported"),
        new((int)OfferServiceErrors.DOCUMENTSTATUS_DELETION_NOT_ALLOWED, "Document in State {documentStatusId} can't be deleted"),
        new((int)OfferServiceErrors.SUBSCRIPTION_NOT_FOUND_FOR_OFFER, "subscription {subscriptionId} for offer {offerId} of type {offerTypeId} does not exist"),
        new((int)OfferServiceErrors.COMPANY_NOT_PART_OF_ROLE, "Company {companyId} is not part of the {offerCompanyRole} company"),
        new((int)OfferServiceErrors.DETAILS_SHOULD_NOT_BE_NULL, "details for offer {offerId}, subscription {subscriptionId}, company {companyId}, offerType {offerTypeId}, should never be null here"),
        new((int)OfferServiceErrors.INVALID_CONFIGURATION_ROLES_NOT_EXIST, "invalid configuration, at least one of the configured roles does not exist in the database: {userRoles}"),
        new((int)OfferServiceErrors.SUBSCRIPTION_NOT_EXIST, "Subscription {subscriptionId} does not exist."),
        new((int)OfferServiceErrors.USER_NOT_BELONG_TO_COMPANY, "The calling user does not belong to the subscribing company"),
        new((int)OfferServiceErrors.NO_ACTIVE_OR_PENDING_SUBSCRIPTION, "There is no active or pending subscription for company {companyId} and subscriptionId {subscriptionId}")
    ]);

    public Type Type { get => typeof(OfferServiceErrors); }
    public IReadOnlyDictionary<int, string> MessageContainer { get => _messageContainer; }
}

public enum OfferServiceErrors
{
    OFFER_NOTFOUND,
    COMPANY_NOT_PROVIDER,
    NOT_EMPTY_ROLES_AND_PROFILES,
    TECHNICAL_USERS_FOR_CONSULTANCY,
    ROLES_DOES_NOT_EXIST,
    ROLES_MISSMATCH,
    INVALID_AGREEMENT,
    INVALID_AGREEMENTS,
    OFFER_STATUS_IN_REVIEW,
    OFFER_DOES_NOT_EXIST,
    COMPANY_OR_USER_NOT_ASSIGNED,
    INVALID_OFFERSUBSCRIPTION,
    CONSENT_NOT_EXIST,
    OFFER_OR_OFFERTYPE_NOT_EXIST,
    COMPANY_NOT_ASSIGNED_WITH_OFFER,
    AGREEMENTS_NOT_VALID_FOR_OFFER,
    OFFER_STATUS_NOT_EXIST,
    SERVICETYPEIDS_NOT_SPECIFIED,
    TITLE_TOO_SHORT,
    SALESMANAGER_NOT_EXIST,
    OFFER_NOT_EXIST,
    COMPANY_NOT_ASSOCIATED_WITH_PROVIDER,
    OFFERPROVIDERDATA_NULL,
    INVALID_SALESMANAGERID,
    SALESMANAGER_NOT_MEMBER_OF_COMPANY,
    USER_NOT_SALESMANAGER,
    MANDATORY_DOCUMENT_TYPES_MISSING,
    APP_NO_ROLES_ASSIGNED,
    PRIVACYPOLICIES_MISSING,
    MISSING_PROPERTIES,
    OFFER_IS_INCORRECT_STATUS,
    OFFER_INCORRECT_STATUS,
    OFFERID_NAME_NOT_SET,
    OFFER_NAME_NOT_SET,
    OFFER_PROVIDING_COMPANY_NOT_SET,
    OFFERID_PROVIDING_COMPANY_NOT_SET,
    OFFER_TYPE_NOT_IMPLEMENTED,
    LANGUAGE_CODES_NOT_EXIST,
    MISSING_PERMISSION,
    OFFERSTATUS_INCORRECT_STATE,
    DOCUMENT_NOT_EXIST,
    DOCUMENT_TYPE_NOT_SUPPORTED,
    DOCUMENT_ID_MISMATCH,
    DOCUMENT_INACTIVE,
    DOCUMENT_CONTENT_NULL,
    COMPANY_NOT_SAME_AS_DOCUMENT_COMPANY,
    DOCUMENT_NOT_ASSIGNED_TO_OFFER,
    DOCUMENT_ASSIGNED_TO_MULTIPLE_OFFERS,
    OFFER_LOCKED_STATE,
    DOCUMENTSTATUS_RETRIEVED_NOT_ALLOWED,
    DOCUMENTSTATUS_DELETION_NOT_ALLOWED,
    SUBSCRIPTION_NOT_FOUND_FOR_OFFER,
    SUBSCRIPTION_NOT_EXIST,
    COMPANY_NOT_PART_OF_ROLE,
    DETAILS_SHOULD_NOT_BE_NULL,
    INVALID_CONFIGURATION_ROLES_NOT_EXIST,
    USER_NOT_BELONG_TO_COMPANY,
    NO_ACTIVE_OR_PENDING_SUBSCRIPTION
}
