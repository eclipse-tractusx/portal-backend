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

public class AdministrationNetworkErrorMessageContainer : IErrorMessageContainer
{
    private static readonly IReadOnlyDictionary<int, string> _messageContainer = new Dictionary<AdministrationNetworkErrors, string> {
            { AdministrationNetworkErrors.NETWORK_SERVICE_ERROR_SAVED_USERS, "Errors occured while saving the users: {errors}"},
            { AdministrationNetworkErrors.NETWORK_ARGUMENT_LEAST_ONE_COMP_ROLE_SELECT, "At least one company role must be selected {companyRoles}"},
            { AdministrationNetworkErrors.NETWORK_ARGUMENT_EXTERNALID_BET_SIX_TO_THIRTYSIX, "ExternalId must be between 6 and 36 characters"},
            { AdministrationNetworkErrors.NETWORK_ARGUMENT_EXTERNALID_EXISTS, "ExternalId {ExternalId} already exists"},
            { AdministrationNetworkErrors.NETWORK_CONFLICT_NO_MANAGED_PROVIDER, "company {ownerCompanyId} has no managed identityProvider"},
            { AdministrationNetworkErrors.NETWORK_ARGUMENT_IDENTIFIER_SET_FOR_ALL_USERS, "Company {ownerCompanyId} has more than one identity provider linked, therefore identityProviderId must be set for all users"},
            { AdministrationNetworkErrors.NETWORK_CONFLICT_IDENTITY_PROVIDER_AS_NO_ALIAS, "identityProvider {identityProviderId} has no alias"},
            { AdministrationNetworkErrors.NETWORK_ARGUMENT_IDPS_NOT_EXIST, "Idps {invalidIds} do not exist"},
            { AdministrationNetworkErrors.NETWORK_ARGUMENT_MAIL_NOT_EMPTY_WITH_VALID_FORMAT, "Mail {email} must not be empty and have valid format"},
            { AdministrationNetworkErrors.NETWORK_ARGUMENT_FIRST_NAME_NOT_MATCH_FORMAT, "Firstname does not match expected format"},
            { AdministrationNetworkErrors.NETWORK_ARGUMENT_LAST_NAME_NOT_MATCH_FORMAT, "Lastname does not match expected format"},
            }.ToImmutableDictionary(x => (int)x.Key, x => x.Value);
    public Type Type { get => typeof(AdministrationNetworkErrors); }

    public IReadOnlyDictionary<int, string> MessageContainer { get => _messageContainer; }
}

public enum AdministrationNetworkErrors
{
    NETWORK_SERVICE_ERROR_SAVED_USERS,
    NETWORK_ARGUMENT_LEAST_ONE_COMP_ROLE_SELECT,
    NETWORK_ARGUMENT_EXTERNALID_BET_SIX_TO_THIRTYSIX,
    NETWORK_ARGUMENT_EXTERNALID_EXISTS,
    NETWORK_CONFLICT_NO_MANAGED_PROVIDER,
    NETWORK_ARGUMENT_IDENTIFIER_SET_FOR_ALL_USERS,
    NETWORK_CONFLICT_IDENTITY_PROVIDER_AS_NO_ALIAS,
    NETWORK_ARGUMENT_IDPS_NOT_EXIST,
    NETWORK_ARGUMENT_MAIL_NOT_EMPTY_WITH_VALID_FORMAT,
    NETWORK_ARGUMENT_FIRST_NAME_NOT_MATCH_FORMAT,
    NETWORK_ARGUMENT_LAST_NAME_NOT_MATCH_FORMAT

}
