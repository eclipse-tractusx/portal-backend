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

namespace Org.Eclipse.TractusX.Portal.Backend.Registration.Service.ErrorHandling;

public class RegistrationErrorMessageContainer : IErrorMessageContainer
{
    private static readonly IReadOnlyDictionary<int, string> _messageContainer = ImmutableDictionary.CreateRange<int, string>([
       new((int)RegistrationErrors.REGISTRATION_ARG_BPN_NOT_HAVING_SIXTEEN_LENGTH, "BPN must contain exactly 16 digits or letters."),
        new((int)RegistrationErrors.REGISTRATION_CONFLICT_BPDM_RETURN_INCORRECT_PIN, "Bpdm did return incorrect bpn legal-entity-data"),
        new((int)RegistrationErrors.REGISTRATION_CONFLICT_LEGAL_INVALID_COUNTRY_IN_LEGAL_ENTITY, "Legal-entity-data did not contain a valid country identifier"),
        new((int)RegistrationErrors.REGISTRATION_CONFLICT_LEGAL_INVALID_COUNTRY_IN_ADDRESS_DATA, "Bpdm did return invalid country {country} in address-data"),
        new((int)RegistrationErrors.REGISTRATION_ARG_FILE_NAME_NOT_NULL, "File name is must not be null"),
        new((int)RegistrationErrors.REGISTRATION_UNSUPPORTED_MEDIA_ONLY_PDF_ALLOWED, "Only .pdf files are allowed."),
        new((int)RegistrationErrors.REGISTRATION_ARG_CHECK_DOCUMENT_TYPE, "documentType must be either: {documentType}"),
        new((int)RegistrationErrors.REGISTRATION_FORBIDDEN_USER_COMPANY_NOT_ASSIGNED_APPLICATION_ID, "The users company is not assigned with application {applicationId}"),
        new((int)RegistrationErrors.REGISTRATION_DOCUMENT_NOT_EXIST, "document {documentId} does not exist."),
        new((int)RegistrationErrors.REGISTRATION_FORBIDDEN_NOT_PERMITTED_DOCUMENT_ACCESS, "The user is not permitted to access document {documentId}."),
        new((int)RegistrationErrors.REGISTRATION_FORBIDDEN_DOCUMENT_ACCESSIBLE_AFTER_ONBOARDING_PROCESS, "Documents not accessible as onboarding process finished {documentId}."),
        new((int)RegistrationErrors.REGISTRATION_COMPANY_APPLICATION_NOT_FOUND, "CompanyApplication {applicationId} not found"),
        new((int)RegistrationErrors.REGISTRATION_COMPANY_APPLICATION_FOR_COMPANY_ID_NOT_FOUND, "CompanyApplication {applicationId} for CompanyId {companyId} not found"),
        new((int)RegistrationErrors.REGISTRATION_FORBIDDEN_USER_APPLICATION_NOT_ASSIGN_WITH_COMP_APPLICATION, "users company is not assigned with CompanyApplication {applicationId}"),
        new((int)RegistrationErrors.REGISTRATION_ARGUMENT_EMAIL_MUST_NOT_EMPTY, "email must not be empty"),
        new((int)RegistrationErrors.REGISTRATION_ARGUMENT_EMAIL_ALREADY_EXIST, "user with email {email} does already exist"),
        new((int)RegistrationErrors.REGISTRATION_ARG_STATUS_NOT_NULL, "status must not be null"),
        new((int)RegistrationErrors.REGISTRATION_APPLICATION_NOT_EXIST, "application {applicationId} does not exist"),
        new((int)RegistrationErrors.REGISTRATION_ARG_INVALID_COMPANY_ROLES, "invalid companyRole: {companyRoles}"),
        new((int)RegistrationErrors.REGISTRATION_ARG_CONSENT_MUST_GIVEN_ALL_ASSIGNED_AGREEMENTS, "consent must be given to all CompanyRole assigned agreements"),
        new((int)RegistrationErrors.REGISTRATION_ARG_AGREEMENTS_NOT_ASSOCIATED_COMPANY_ROLES, "Agreements which not associated with requested companyRoles: {agreementId}"),
        new((int)RegistrationErrors.REGISTRATION_UNEXPECTED_UPDATE_STATUS_SHOULD_SUBMITTED, "updateStatus should allways be SUBMITTED here"),
        new((int)RegistrationErrors.REGISTRATION_FORBIDDEN_USER_ID_NOT_ASSOCIATED_WITH_COMPANY_APPLICATION, "userId {userId} is not associated with CompanyApplication {applicationId}"),
        new((int)RegistrationErrors.REGISTRATION_CONFLICT_COMPANY_NAME_NOT_EMPTY, "Company Name must not be empty"),
        new((int)RegistrationErrors.REGISTRATION_CONFLICT_ADDRESS_NOT_EMPTY, "Address must not be empty"),
        new((int)RegistrationErrors.REGISTRATION_CONFLICT_STREET_NOT_EMPTY, "Street Name must not be empty"),
        new((int)RegistrationErrors.REGISTRATION_CONFLICT_CITY_NOT_EMPTY, "City must not be empty"),
        new((int)RegistrationErrors.REGISTRATION_CONFLICT_COUNTRY_NOT_EMPTY, "Country must not be empty"),
        new((int)RegistrationErrors.REGISTRATION_CONFLICT_AGREE_CONSENT_NOT_EMPTY, "Agreement and Consent must not be empty"),
        new((int)RegistrationErrors.REGISTRATION_CONFLICT_COMPANY_IDENTIFIERS_NOT_EMPTY, "Company Identifiers {uniqueIds} must not be empty"),
        new((int)RegistrationErrors.REGISTRATION_CONFLICT_COMPANY_ASSIGNED_ROLE_NOT_EMPTY, "Company assigned role {companyRoleIds} must not be empty"),
        new((int)RegistrationErrors.REGISTRATION_CONFLICT_AT_LEAST_ONE_DOCUMENT_ID_AVAILABLE, "At least one Document type Id must be {docTypeIds}"),
        new((int)RegistrationErrors.REGISTRATION_FORBIDDEN_USER_NOT_ASSOCIATED_APPLICATION, "The user is not associated with application {applicationId}"),
        new((int)RegistrationErrors.REGISTRATION_FORBIDDEN_USER_NOT_ASSOCIATED_INVITATION, "user is not associated with invitation"),
        new((int)RegistrationErrors.REGISTRATION_UNEXPECT_REGISTER_DATA_NOT_NULL_APPLICATION, "registrationData should never be null for application {applicationId}"),
        new((int)RegistrationErrors.REGISTRATION_ARG_INVALID_STATUS_REQUEST_APPLICATION_STATUS, "invalid status update requested {status}, current status is {statusId}, possible values are: {applicationStatus}"),
        new((int)RegistrationErrors.REGISTRATION_FORBIDDEN_STATUS_NOT_FITTING_PRE_REQUITISE, "Application status is not fitting to the pre-requisite"),
        new((int)RegistrationErrors.REGISTRATION_FORBIDDEN_APPLICATION_ALREADY_CLOSED, "Application is already closed"),
        new((int)RegistrationErrors.REGISTRATION_ARG_DOCUMENT_ID_NOT_EMPTY, "documentId must not be empty"),
        new((int)RegistrationErrors.REGISTRATION_CONFLICT_DOCUMENT_DELETION_NOT_ALLOWED, "Document deletion is not allowed. DocumentType must be either :{DocumentTypeIds}"),
        new((int)RegistrationErrors.REGISTRATION_CONFLICT_DOCUMENT_DELETION_NOT_ALLOWED_APP_CLOSED, "Document deletion is not allowed. Application is already closed."),
        new((int)RegistrationErrors.REGISTRATION_FORBIDDEN_USER_NOT_ALLOWED_DELETE_DOCUMENT, "User is not allowed to delete this document"),
        new((int)RegistrationErrors.REGISTRATION_CONFLICT_DELETION_NOT_ALLOWED_LOCKED, "Document deletion is not allowed. The document is locked."),
        new((int)RegistrationErrors.REGISTRATION_NOT_INVALID_COUNTRY_CODE, "invalid country code {alpha2Code}"),
        new((int)RegistrationErrors.REGISTRATION_FORBIDDEN_USER_NOT_ALLOWED_TO_DECLINED_APP, "User is not allowed to decline this application"),
        new((int)RegistrationErrors.REGISTRATION_UNEXPECT_APP_DECLINED_DATA_NOT_NULL, "ApplicationDeclineData should never be null here"),
        new((int)RegistrationErrors.REGISTRATION_UNEXPECT_IDENTITY_PROVIDER_TYPE_ID_SHARED_OR_OWN, "IdentityProviderTypeId should allways be shared or own here")

   ]);

    public Type Type { get => typeof(RegistrationErrors); }
    public IReadOnlyDictionary<int, string> MessageContainer { get => _messageContainer; }
}

public enum RegistrationErrors
{
    REGISTRATION_ARG_BPN_NOT_HAVING_SIXTEEN_LENGTH,
    REGISTRATION_CONFLICT_BPDM_RETURN_INCORRECT_PIN,
    REGISTRATION_CONFLICT_LEGAL_INVALID_COUNTRY_IN_LEGAL_ENTITY,
    REGISTRATION_CONFLICT_LEGAL_INVALID_COUNTRY_IN_ADDRESS_DATA,
    REGISTRATION_ARG_FILE_NAME_NOT_NULL,
    REGISTRATION_UNSUPPORTED_MEDIA_ONLY_PDF_ALLOWED,
    REGISTRATION_ARG_CHECK_DOCUMENT_TYPE,
    REGISTRATION_FORBIDDEN_USER_COMPANY_NOT_ASSIGNED_APPLICATION_ID,
    REGISTRATION_DOCUMENT_NOT_EXIST,
    REGISTRATION_FORBIDDEN_NOT_PERMITTED_DOCUMENT_ACCESS,
    REGISTRATION_FORBIDDEN_DOCUMENT_ACCESSIBLE_AFTER_ONBOARDING_PROCESS,
    REGISTRATION_COMPANY_APPLICATION_NOT_FOUND,
    REGISTRATION_COMPANY_APPLICATION_FOR_COMPANY_ID_NOT_FOUND,
    REGISTRATION_FORBIDDEN_USER_APPLICATION_NOT_ASSIGN_WITH_COMP_APPLICATION,
    REGISTRATION_ARGUMENT_EMAIL_MUST_NOT_EMPTY,
    REGISTRATION_ARGUMENT_EMAIL_ALREADY_EXIST,
    REGISTRATION_ARG_STATUS_NOT_NULL,
    REGISTRATION_APPLICATION_NOT_EXIST,
    REGISTRATION_ARG_INVALID_COMPANY_ROLES,
    REGISTRATION_ARG_CONSENT_MUST_GIVEN_ALL_ASSIGNED_AGREEMENTS,
    REGISTRATION_ARG_AGREEMENTS_NOT_ASSOCIATED_COMPANY_ROLES,
    REGISTRATION_UNEXPECTED_UPDATE_STATUS_SHOULD_SUBMITTED,
    REGISTRATION_FORBIDDEN_USER_ID_NOT_ASSOCIATED_WITH_COMPANY_APPLICATION,
    REGISTRATION_CONFLICT_COMPANY_NAME_NOT_EMPTY,
    REGISTRATION_CONFLICT_ADDRESS_NOT_EMPTY,
    REGISTRATION_CONFLICT_STREET_NOT_EMPTY,
    REGISTRATION_CONFLICT_CITY_NOT_EMPTY,
    REGISTRATION_CONFLICT_COUNTRY_NOT_EMPTY,
    REGISTRATION_CONFLICT_AGREE_CONSENT_NOT_EMPTY,
    REGISTRATION_CONFLICT_COMPANY_IDENTIFIERS_NOT_EMPTY,
    REGISTRATION_CONFLICT_COMPANY_ASSIGNED_ROLE_NOT_EMPTY,
    REGISTRATION_CONFLICT_AT_LEAST_ONE_DOCUMENT_ID_AVAILABLE,
    REGISTRATION_FORBIDDEN_USER_NOT_ASSOCIATED_APPLICATION,
    REGISTRATION_FORBIDDEN_USER_NOT_ASSOCIATED_INVITATION,
    REGISTRATION_UNEXPECT_REGISTER_DATA_NOT_NULL_APPLICATION,
    REGISTRATION_ARG_INVALID_STATUS_REQUEST_APPLICATION_STATUS,
    REGISTRATION_FORBIDDEN_STATUS_NOT_FITTING_PRE_REQUITISE,
    REGISTRATION_FORBIDDEN_APPLICATION_ALREADY_CLOSED,
    REGISTRATION_ARG_DOCUMENT_ID_NOT_EMPTY,
    REGISTRATION_CONFLICT_DOCUMENT_DELETION_NOT_ALLOWED,
    REGISTRATION_CONFLICT_DOCUMENT_DELETION_NOT_ALLOWED_APP_CLOSED,
    REGISTRATION_FORBIDDEN_USER_NOT_ALLOWED_DELETE_DOCUMENT,
    REGISTRATION_CONFLICT_DELETION_NOT_ALLOWED_LOCKED,
    REGISTRATION_NOT_INVALID_COUNTRY_CODE,
    REGISTRATION_FORBIDDEN_USER_NOT_ALLOWED_TO_DECLINED_APP,
    REGISTRATION_UNEXPECT_APP_DECLINED_DATA_NOT_NULL,
    REGISTRATION_UNEXPECT_IDENTITY_PROVIDER_TYPE_ID_SHARED_OR_OWN

}
