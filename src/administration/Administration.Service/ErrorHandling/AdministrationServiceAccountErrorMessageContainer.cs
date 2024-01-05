/********************************************************************************
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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Library;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.ErrorHandling;

public class AdministrationServiceAccountErrorMessageContainer : IErrorMessageContainer
{
    private static readonly IReadOnlyDictionary<int, string> _messageContainer = new Dictionary<AdministrationServiceAccountErrors, string> {
                { AdministrationServiceAccountErrors.SERVICE_AUTH_SECRET_ARGUMENT, "other authenticationType values than SECRET are not supported yet , {authenticationType}" },
                { AdministrationServiceAccountErrors.SERVICE_NAME_EMPTY_ARGUMENT,"name must not be empty, {name}"},
                { AdministrationServiceAccountErrors.SERVICE_COMPANY_NOT_EXIST_CONFLICT,"company {companyId} does not exist"},
                { AdministrationServiceAccountErrors.SERVICE_BPN_NOT_SET_CONFLICT,"bpn not set for company {companyId}"},
                { AdministrationServiceAccountErrors.SERVICE_ROLES_NOT_ASSIGN_ARGUMENT,"The roles {unassignable} are not assignable to a service account, {userRoleIds}"},
                { AdministrationServiceAccountErrors.SERVICE_ACCOUNT_NOT_CONFLICT,"serviceAccount {serviceAccountId} not found for company {companyId}"},
                { AdministrationServiceAccountErrors.SERVICE_USERID_ACTIVATION_PENDING_CONFLICT,"Technical User is linked to an active connector. Change the link or deactivate the connector to delete the technical user."},
                { AdministrationServiceAccountErrors.SERVICE_USERID_ACTIVATION_ACTIVE_CONFLICT,"Technical User is linked to an active subscription. Deactivate the subscription to delete the technical user."},
                { AdministrationServiceAccountErrors.SERVICE_UNDEFINED_CLIENTID_CONFLICT,"undefined clientId for serviceAccount {serviceAccountId}"},
                { AdministrationServiceAccountErrors.SERVICE_ID_PATH_NOT_MATCH_ARGUMENT,"serviceAccountId {serviceAccountId} from path does not match the one in body {serviceAccountDetailsServiceAccountId}"},
                { AdministrationServiceAccountErrors.SERVICE_INACTIVE_CONFLICT,"serviceAccount {serviceAccountId} is already INACTIVE"},
                { AdministrationServiceAccountErrors.SERVICE_CLIENTID_NOT_NULL_CONFLICT,"clientClientId of serviceAccount {serviceAccountId} should not be null"}
            }.ToImmutableDictionary(x => (int)x.Key, x => x.Value);

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
    SERVICE_ACCOUNT_NOT_CONFLICT,
    SERVICE_USERID_ACTIVATION_PENDING_CONFLICT,
    SERVICE_USERID_ACTIVATION_ACTIVE_CONFLICT,
    SERVICE_UNDEFINED_CLIENTID_CONFLICT,
    SERVICE_ID_PATH_NOT_MATCH_ARGUMENT,
    SERVICE_INACTIVE_CONFLICT,
    SERVICE_CLIENTID_NOT_NULL_CONFLICT
}
