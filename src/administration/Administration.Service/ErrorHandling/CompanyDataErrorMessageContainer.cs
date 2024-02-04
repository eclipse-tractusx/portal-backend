/********************************************************************************
 * Copyright (c) 2021, 2024 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Library;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.ErrorHandling;

public class CompanyDataErrorMessageContainer : IErrorMessageContainer
{
    private static readonly IReadOnlyDictionary<int, string> _messageContainer = new Dictionary<CompanyDataErrors, string> {
        { CompanyDataErrors.INVALID_COMPANY, "company {companyId} is not a valid company" },
        { CompanyDataErrors.INVALID_COMPANY_STATUS, "Company Status is Incorrect" },
        { CompanyDataErrors.USE_CASE_NOT_FOUND, "UseCaseId {useCaseId} is not available" },
        { CompanyDataErrors.INVALID_LANGUAGECODE, "language {languageShortName} is not a valid languagecode" },
        { CompanyDataErrors.COMPANY_NOT_FOUND, "company {companyId} does not exist" },
        { CompanyDataErrors.COMPANY_ROLE_IDS_CONSENT_STATUS_NULL, "neither CompanyRoleIds nor ConsentStatusDetails should ever be null here" },
        { CompanyDataErrors.MISSING_AGREEMENTS, "All agreements need to get signed as Active or InActive. Missing consents: [{missingConsents}]" },
        { CompanyDataErrors.UNASSIGN_ALL_ROLES, "Company can't unassign from all roles, Atleast one Company role need to signed as active" },
        { CompanyDataErrors.AGREEMENTS_NOT_ASSIGNED_WITH_ROLES, "Agreements not associated with requested companyRoles: [{companyRoles}]" },
        { CompanyDataErrors.MULTIPLE_SSI_DETAIL, "There should only be one pending or active ssi detail be assigne" },
        { CompanyDataErrors.EXTERNAL_TYPE_DETAIL_NOT_FOUND, "VerifiedCredentialExternalTypeDetail {verifiedCredentialExternalTypeDetailId} does not exist" },
        { CompanyDataErrors.EXPIRY_DATE_IN_PAST, "The expiry date must not be in the past" },
        { CompanyDataErrors.CREDENTIAL_NO_CERTIFICATE, "{credentialTypeId} is not assigned to a certificate" },
        { CompanyDataErrors.EXTERNAL_TYPE_DETAIL_ID_NOT_SET, "The VerifiedCredentialExternalTypeDetailId must be set" },
        { CompanyDataErrors.CREDENTIAL_ALREADY_EXISTING, "Credential request already existing" },
        { CompanyDataErrors.CREDENTIAL_TYPE_NOT_FOUND, "VerifiedCredentialType {verifiedCredentialType} does not exists" },
        { CompanyDataErrors.SSI_DETAILS_NOT_FOUND, "CompanySsiDetail {credentialId} does not exists" },
        { CompanyDataErrors.CREDENTIAL_NOT_PENDING, "Credential {credentialId} must be {status}" },
        { CompanyDataErrors.BPN_NOT_SET, "Bpn should be set for company {companyName}" },
        { CompanyDataErrors.EXPIRY_DATE_NOT_SET, "Expiry date must always be set for use cases" },
        { CompanyDataErrors.EMPTY_VERSION, "External Detail Version must not be null" },
        { CompanyDataErrors.KIND_NOT_SUPPORTED, "{kind} is currently not supported"}
    }.ToImmutableDictionary(x => (int)x.Key, x => x.Value);

    public Type Type { get => typeof(CompanyDataErrors); }
    public IReadOnlyDictionary<int, string> MessageContainer { get => _messageContainer; }
}

public enum CompanyDataErrors
{
    INVALID_COMPANY,
    INVALID_COMPANY_STATUS,
    USE_CASE_NOT_FOUND,
    INVALID_LANGUAGECODE,
    COMPANY_NOT_FOUND,
    COMPANY_ROLE_IDS_CONSENT_STATUS_NULL,
    MISSING_AGREEMENTS,
    UNASSIGN_ALL_ROLES,
    AGREEMENTS_NOT_ASSIGNED_WITH_ROLES,
    MULTIPLE_SSI_DETAIL,
    EXTERNAL_TYPE_DETAIL_NOT_FOUND,
    EXPIRY_DATE_IN_PAST,
    CREDENTIAL_NO_CERTIFICATE,
    EXTERNAL_TYPE_DETAIL_ID_NOT_SET,
    CREDENTIAL_ALREADY_EXISTING,
    CREDENTIAL_TYPE_NOT_FOUND,
    SSI_DETAILS_NOT_FOUND,
    CREDENTIAL_NOT_PENDING,
    BPN_NOT_SET,
    EXPIRY_DATE_NOT_SET,
    EMPTY_VERSION,
    KIND_NOT_SUPPORTED
}
