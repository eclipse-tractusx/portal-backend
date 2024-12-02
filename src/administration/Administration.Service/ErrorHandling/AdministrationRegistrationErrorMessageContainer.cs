/********************************************************************************
 * Copyright (c) 2023 Contributors to the Eclipse Foundation
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

public class AdministrationRegistrationErrorMessageContainer : IErrorMessageContainer
{
    private static readonly IReadOnlyDictionary<int, string> _messageContainer = new Dictionary<AdministrationRegistrationErrors, string> {
                { AdministrationRegistrationErrors.APPLICATION_NOT_FOUND, "application {applicationId} does not exist" },
                { AdministrationRegistrationErrors.REGISTRATION_ARGUMENT_BPN_MUST_SIXTEEN_CHAR_LONG, "BPN must contain exactly 16 characters long."},
                { AdministrationRegistrationErrors.REGISTRATION_ARGUMENT_BPNL_PREFIXED_BPNL, "businessPartnerNumbers must prefixed with BPNL"},
                { AdministrationRegistrationErrors.REGISTRATION_NOT_APPLICATION_FOUND, "application {applicationId} not found"},
                { AdministrationRegistrationErrors.REGISTRATION_BPN_ASSIGN_TO_OTHER_COMP, "BusinessPartnerNumber is already assigned to a different company"},
                { AdministrationRegistrationErrors.REGISTRATION_CONFLICT_APPLICATION_FOR_COMPANY_NOT_PENDING, "application {applicationId} for company {companyId} is not pending"},
                { AdministrationRegistrationErrors.REGISTRATION_CONFLICT_BPN_OF_COMPANY_SET, "BusinessPartnerNumber of company {companyId} has already been set."},
                { AdministrationRegistrationErrors.REGISTRATION_NOT_COMP_APP_BPN_STATUS_SUBMIT, "No companyApplication for BPN {businessPartnerNumber} is not in status SUBMITTED"},
                { AdministrationRegistrationErrors.REGISTRATION_CONFLICT_APP_STATUS_STATUS_SUBMIT_FOUND_BPN, "more than one companyApplication in status SUBMITTED found for BPN {businessPartnerNumber}"},
                { AdministrationRegistrationErrors.REGISTRATION_ARGUMENT_PROCEES_TYPID_NOT_TRIGERABLE, "The processStep {processStepTypeId} is not retriggerable"},
                { AdministrationRegistrationErrors.REGISTRATION_UNEXPECT_PROCESS_TYPID_CONFIGURED_TRIGERABLE, "While the processStep {processStepTypeId} is configured to be retriggerable there is no next step configured"},
                { AdministrationRegistrationErrors.REGISTRATION_NOT_COMPANY_EXTERNAL_APP_NOT_FOUND, "companyApplication {externalId} not found"},
                { AdministrationRegistrationErrors.REGISTRATION_NOT_COMPANY_EXTERNAL_NOT_STATUS_SUBMIT, "companyApplication {externalId} is not in status SUBMITTED"},
                { AdministrationRegistrationErrors.REGISTRATION_CONFLICT_COMMENT_NOT_SET, "No comment set."},
                { AdministrationRegistrationErrors.REGISTRATION_ARGUMENT_COMP_APP_STATUS_NOTSUBMITTED, "CompanyApplication {applicationId} is not in status SUBMITTED"},
                { AdministrationRegistrationErrors.REGISTRATION_CONFLICT_EMAIL_NOT_ASSIGN_TO_USERNAME, "user {userName} has no assigned email"},
                { AdministrationRegistrationErrors.REGISTRATION_NOT_DOC_NOT_EXIST, "Document {documentId} does not exist"},
            }.ToImmutableDictionary(x => (int)x.Key, x => x.Value);

    public Type Type { get => typeof(AdministrationRegistrationErrors); }
    public IReadOnlyDictionary<int, string> MessageContainer { get => _messageContainer; }
}

public enum AdministrationRegistrationErrors
{
    APPLICATION_NOT_FOUND,
    REGISTRATION_ARGUMENT_BPN_MUST_SIXTEEN_CHAR_LONG,
    REGISTRATION_ARGUMENT_BPNL_PREFIXED_BPNL,
    REGISTRATION_NOT_APPLICATION_FOUND,
    REGISTRATION_BPN_ASSIGN_TO_OTHER_COMP,
    REGISTRATION_CONFLICT_APPLICATION_FOR_COMPANY_NOT_PENDING,
    REGISTRATION_CONFLICT_BPN_OF_COMPANY_SET,
    REGISTRATION_NOT_COMP_APP_BPN_STATUS_SUBMIT,
    REGISTRATION_CONFLICT_APP_STATUS_STATUS_SUBMIT_FOUND_BPN,
    REGISTRATION_ARGUMENT_PROCEES_TYPID_NOT_TRIGERABLE,
    REGISTRATION_UNEXPECT_PROCESS_TYPID_CONFIGURED_TRIGERABLE,
    REGISTRATION_NOT_COMPANY_EXTERNAL_APP_NOT_FOUND,
    REGISTRATION_NOT_COMPANY_EXTERNAL_NOT_STATUS_SUBMIT,
    REGISTRATION_CONFLICT_COMMENT_NOT_SET,
    REGISTRATION_ARGUMENT_COMP_APP_STATUS_NOTSUBMITTED,
    REGISTRATION_CONFLICT_EMAIL_NOT_ASSIGN_TO_USERNAME,
    REGISTRATION_NOT_DOC_NOT_EXIST

}
