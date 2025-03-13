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

public class AdministrationServiceAccountErrorMessageContainer : IErrorMessageContainer
{
    private static readonly IReadOnlyDictionary<int, string> _messageContainer = ImmutableDictionary.CreateRange<int, string>([
        new((int)AdministrationServiceAccountErrors.SERVICE_AUTH_SECRET_ARGUMENT, "other authenticationType values than SECRET are not supported yet , {authenticationType}"),
        new((int)AdministrationServiceAccountErrors.SERVICE_NAME_EMPTY_ARGUMENT, "name must not be empty, {name}"),
        new((int)AdministrationServiceAccountErrors.SERVICE_COMPANY_NOT_EXIST_CONFLICT, "company {companyId} does not exist"),
        new((int)AdministrationServiceAccountErrors.SERVICE_BPN_NOT_SET_CONFLICT, "bpn not set for company {companyId}"),
        new((int)AdministrationServiceAccountErrors.SERVICE_ROLES_NOT_ASSIGN_ARGUMENT, "The roles {unassignable} are not assignable to a service account, {userRoleIds}"),
        new((int)AdministrationServiceAccountErrors.SERVICE_ACCOUNT_NOT_FOUND, "serviceAccount {serviceAccountId} does not exist"),
        new((int)AdministrationServiceAccountErrors.SERVICE_USERID_ACTIVATION_PENDING_CONFLICT, "Technical User is linked to an active connector. Change the link or deactivate the connector to delete the technical user."),
        new((int)AdministrationServiceAccountErrors.SERVICE_USERID_ACTIVATION_ACTIVE_CONFLICT, "Technical User is linked to an active subscription. Deactivate the subscription to delete the technical user."),
        new((int)AdministrationServiceAccountErrors.SERVICE_UNDEFINED_CLIENTID_CONFLICT, "undefined clientId for serviceAccount {serviceAccountId}"),
        new((int)AdministrationServiceAccountErrors.SERVICE_ID_PATH_NOT_MATCH_ARGUMENT, "serviceAccountId {serviceAccountId} from path does not match the one in body {serviceAccountDetailsServiceAccountId}"),
        new((int)AdministrationServiceAccountErrors.SERVICE_INACTIVE_CONFLICT, "serviceAccount {serviceAccountId} is already INACTIVE"),
        new((int)AdministrationServiceAccountErrors.SERVICE_CLIENTID_NOT_NULL_CONFLICT, "clientClientId of serviceAccount {serviceAccountId} should not be null"),
        new((int)AdministrationServiceAccountErrors.SERVICE_ACCOUNT_NOT_LINKED_TO_PROCESS, "Service Account {serviceAccountId} is not linked to a process"),
        new((int)AdministrationServiceAccountErrors.SERVICE_ACCOUNT_PENDING_PROCESS_STEPS, "Service Account {serviceAccountId} has pending process steps {processStepTypeIds}"),
        new((int)AdministrationServiceAccountErrors.SERVICE_ACCOUNT_NOT_ACTIVE, "Service Account {serviceAccountId} is not status active"),
        new((int)AdministrationServiceAccountErrors.SERVICE_ACCOUNT_NO_PROVIDER_OR_OWNER, "Only provider or owner of the service account are allowed to delete it"),
        new((int)AdministrationServiceAccountErrors.TECHNICAL_USER_CREATION_IN_PROGRESS, "Technical user can't be deleted because the creation progress is still running")
    ]);

    public Type Type { get => typeof(AdministrationServiceAccountErrors); }
    public IReadOnlyDictionary<int, string> MessageContainer { get => _messageContainer; }
}

public enum AdministrationServiceAccountErrors
{
    SERVICE_AUTH_SECRET_ARGUMENT,
    SERVICE_NAME_EMPTY_ARGUMENT,
    SERVICE_COMPANY_NOT_EXIST_CONFLICT,
    SERVICE_BPN_NOT_SET_CONFLICT,
    SERVICE_ROLES_NOT_ASSIGN_ARGUMENT,
    SERVICE_ACCOUNT_NOT_FOUND,
    SERVICE_USERID_ACTIVATION_PENDING_CONFLICT,
    SERVICE_USERID_ACTIVATION_ACTIVE_CONFLICT,
    SERVICE_UNDEFINED_CLIENTID_CONFLICT,
    SERVICE_ID_PATH_NOT_MATCH_ARGUMENT,
    SERVICE_INACTIVE_CONFLICT,
    SERVICE_CLIENTID_NOT_NULL_CONFLICT,
    SERVICE_ACCOUNT_NOT_LINKED_TO_PROCESS,
    SERVICE_ACCOUNT_PENDING_PROCESS_STEPS,
    TECHNICAL_USER_CREATION_IN_PROGRESS,
    SERVICE_ACCOUNT_NOT_ACTIVE,
    SERVICE_ACCOUNT_NO_PROVIDER_OR_OWNER
}
