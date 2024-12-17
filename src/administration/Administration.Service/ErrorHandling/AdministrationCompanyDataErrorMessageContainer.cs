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

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.ErrorHandling;

public class AdministrationCompanyDataErrorMessageContainer : IErrorMessageContainer
{
    private static readonly IReadOnlyDictionary<int, string> _messageContainer = ImmutableDictionary.CreateRange<int, string>([
        new((int)AdministrationCompanyDataErrors.COMPANY_DATA_CONFLICT_INVALID_COMPANY, "company {companyId} is not a valid company"),
        new((int)AdministrationCompanyDataErrors.COMPANY_DATA_CONFLICT_INCORR_COMPANY_STATUS, "Company Status is Incorrect"),
        new((int)AdministrationCompanyDataErrors.COMPANY_DATA_CONFLICT_USECASEID_NOT_AVAL, "UseCaseId {useCaseId} is not available"),
        new((int)AdministrationCompanyDataErrors.COMPANY_DATA_ARGUMENT_LANG_CODE_NOT_VALID, "language {languageShortName} is not a valid languagecode"),
        new((int)AdministrationCompanyDataErrors.COMPANY_DATA_NOT_COMPANY_NOT_EXIST, "company {companyId} does not exist"),
        new((int)AdministrationCompanyDataErrors.COMPANY_DATA_UNEXP_COMP_ROLES_NOR_DETAILS_NULL, "neither CompanyRoleIds nor ConsentStatusDetails should ever be null here"),
        new((int)AdministrationCompanyDataErrors.COMPANY_DATA_ARGUMENT_AGREEMENT_ACTIVE_INACTIVE_MISSING, "All agreements need to get signed as Active or InActive. Missing consents: { consentType }"),
        new((int)AdministrationCompanyDataErrors.COMPANY_DATA_CONFLICT_NOT_UNASSIGN_ALL_ROLES_ATLEAST_ONE_ACTIVE_NEEDED, "Company can't unassign from all roles, Atleast one Company role need to signed as active"),
        new((int)AdministrationCompanyDataErrors.COMPANY_DATA_ARGUMENT_AGREEMENT_NOT_ASSOCIATE_COMPANY_ROLES, "Agreements not associated with requested companyRoles: {companyRoles}"),
        new((int)AdministrationCompanyDataErrors.COMPANY_DATA_ARGUMENT_EXTER_CERT_APLHA_LENGTH, "ExternalCertificateNumber must be alphanumeric and length should not be greater than 36"),
        new((int)AdministrationCompanyDataErrors.COMPANY_DATA_ARGUMENT_PREFIXED_BPNS_SIXTEEN_CHAR, "BPN must contain exactly 16 characters and must be prefixed with BPNS"),
        new((int)AdministrationCompanyDataErrors.COMPANY_DATA_ARGUMENT_NOT_GREATER_CURR_DATE, "ValidFrom date should not be greater than current date"),
        new((int)AdministrationCompanyDataErrors.COMPANY_DATA_ARGUMENT_SHOULD_GREATER_THAN_CURR_DATE, "ValidTill date should be greater than current date"),
        new((int)AdministrationCompanyDataErrors.COMPANY_DATA_ARGUMENT_CERT_TYPE_NOT_ASSIGN_CERTIFICATE, "{certificateType} is not assigned to a certificate"),
        new((int)AdministrationCompanyDataErrors.COMPANY_DATA_ARGUMENT_BPN_NOT_EMPTY, "businessPartnerNumber must not be empty"),
        new((int)AdministrationCompanyDataErrors.COMPANY_DATA_ARGUMENT_COMP_NOT_EXISTS_FOR_BPN, "company does not exist for {businessPartnerNumber}"),
        new((int)AdministrationCompanyDataErrors.COMPANY_DATA_CONFLICT_MULTIPLE_ACTIVE_CERT_NOT_ALLOWED_ONE_DOC, "There must not be multiple active certificates for document {documentId}"),
        new((int)AdministrationCompanyDataErrors.COMPANY_DATA_NOT_DOC_NOT_EXIST, "Document is not existing"),
        new((int)AdministrationCompanyDataErrors.COMPANY_DATA_FORBIDDEN_USER_NOT_ALLOW_DEL_DOC, "User is not allowed to delete this document"),
        new((int)AdministrationCompanyDataErrors.COMPANY_DATA_NOT_COMP_CERT_DOC_NOT_EXIST, "Company certificate document {documentId} does not exist"),
        new((int)AdministrationCompanyDataErrors.COMPANY_DATA_FORBIDDEN_DOC_STATUS_NOT_LOCKED, "Document {documentId} status is not locked"),
        new((int)AdministrationCompanyDataErrors.COMPANY_DATA_NOT_PROCESSID_NOT_EXIST, "process {processId} does not exist")
    ]);

    public Type Type { get => typeof(AdministrationCompanyDataErrors); }

    public IReadOnlyDictionary<int, string> MessageContainer { get => _messageContainer; }
}

public enum AdministrationCompanyDataErrors
{
    COMPANY_DATA_CONFLICT_INVALID_COMPANY,
    COMPANY_DATA_CONFLICT_INCORR_COMPANY_STATUS,
    COMPANY_DATA_CONFLICT_USECASEID_NOT_AVAL,
    COMPANY_DATA_ARGUMENT_LANG_CODE_NOT_VALID,
    COMPANY_DATA_NOT_COMPANY_NOT_EXIST,
    COMPANY_DATA_UNEXP_COMP_ROLES_NOR_DETAILS_NULL,
    COMPANY_DATA_ARGUMENT_AGREEMENT_ACTIVE_INACTIVE_MISSING,
    COMPANY_DATA_CONFLICT_NOT_UNASSIGN_ALL_ROLES_ATLEAST_ONE_ACTIVE_NEEDED,
    COMPANY_DATA_ARGUMENT_AGREEMENT_NOT_ASSOCIATE_COMPANY_ROLES,
    COMPANY_DATA_ARGUMENT_EXTER_CERT_APLHA_LENGTH,
    COMPANY_DATA_ARGUMENT_PREFIXED_BPNS_SIXTEEN_CHAR,
    COMPANY_DATA_ARGUMENT_NOT_GREATER_CURR_DATE,
    COMPANY_DATA_ARGUMENT_SHOULD_GREATER_THAN_CURR_DATE,
    COMPANY_DATA_ARGUMENT_CERT_TYPE_NOT_ASSIGN_CERTIFICATE,
    COMPANY_DATA_ARGUMENT_BPN_NOT_EMPTY,
    COMPANY_DATA_ARGUMENT_COMP_NOT_EXISTS_FOR_BPN,
    COMPANY_DATA_CONFLICT_MULTIPLE_ACTIVE_CERT_NOT_ALLOWED_ONE_DOC,
    COMPANY_DATA_NOT_DOC_NOT_EXIST,
    COMPANY_DATA_FORBIDDEN_USER_NOT_ALLOW_DEL_DOC,
    COMPANY_DATA_NOT_COMP_CERT_DOC_NOT_EXIST,
    COMPANY_DATA_FORBIDDEN_DOC_STATUS_NOT_LOCKED,
    COMPANY_DATA_NOT_PROCESSID_NOT_EXIST
}
