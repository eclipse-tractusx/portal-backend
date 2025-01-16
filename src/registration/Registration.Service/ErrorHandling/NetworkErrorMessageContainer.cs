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

public class NetworkErrorMessageContainer : IErrorMessageContainer
{
    private static readonly IReadOnlyDictionary<int, string> _messageContainer = ImmutableDictionary.CreateRange<int, string>([
       new((int)NetworkErrors.NETWORK_COMPANY_NOT_FOUND, "Company {companyId} not found"),
        new((int)NetworkErrors.NETWORK_CONFLICT_ONLY_ONE_APPLICATION_PER_COMPANY, "Company {companyId} has no or more than one application"),
        new((int)NetworkErrors.NETWORK_CONFLICT_PROCESS_MUST_EXIST, "There must be an process"),
        new((int)NetworkErrors.NETWORK_CONFLICT_APP_NOT_CREATED_STATE, "Application {companyApplicationId} is not in state CREATED"),
        new((int)NetworkErrors.NETWORK_ARG_NOT_ACTIVE_AGREEMENTS, "All agreements must be agreed to. Agreements that are not active: {agreementId}"),
        new((int)NetworkErrors.NETWORK_ARG_COMPANY_ROLES_MISSING, "CompanyRoles {companyRoleId} are missing"),
        new((int)NetworkErrors.NETWORK_ARG_ALL_AGREEMNTS_COMPANY_SHOULD_AGREED, "All Agreements for the company roles must be agreed to, missing agreementIds: {agreementIds}"),
        new((int)NetworkErrors.NETWORK_COMPANY_APPLICATION_NOT_EXIST, "CompanyApplication {applicationId} does not exist"),
        new((int)NetworkErrors.NETWORK_FORBIDDEN_USER_NOT_ALLOWED_DECLINE_APPLICATION, "User is not allowed to decline application {applicationId}"),
        new((int)NetworkErrors.NETWORK_CONFLICT_EXTERNAL_REGISTRATIONS_DECLINED, "Only external registrations can be declined"),
        new((int)NetworkErrors.NETWORK_CONFLICT_CHECK_APPLICATION_STATUS, "The status of the application {applicationId} must be one of the following: {validStatus}"),
    ]);

    public Type Type { get => typeof(NetworkErrors); }
    public IReadOnlyDictionary<int, string> MessageContainer { get => _messageContainer; }
}

public enum NetworkErrors
{
    NETWORK_COMPANY_NOT_FOUND,
    NETWORK_CONFLICT_ONLY_ONE_APPLICATION_PER_COMPANY,
    NETWORK_CONFLICT_PROCESS_MUST_EXIST,
    NETWORK_CONFLICT_APP_NOT_CREATED_STATE,
    NETWORK_ARG_NOT_ACTIVE_AGREEMENTS,
    NETWORK_ARG_COMPANY_ROLES_MISSING,
    NETWORK_ARG_ALL_AGREEMNTS_COMPANY_SHOULD_AGREED,
    NETWORK_COMPANY_APPLICATION_NOT_EXIST,
    NETWORK_FORBIDDEN_USER_NOT_ALLOWED_DECLINE_APPLICATION,
    NETWORK_CONFLICT_EXTERNAL_REGISTRATIONS_DECLINED,
    NETWORK_CONFLICT_CHECK_APPLICATION_STATUS,
}
