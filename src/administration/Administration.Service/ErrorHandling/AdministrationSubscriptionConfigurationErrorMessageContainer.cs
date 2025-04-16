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

public class AdministrationSubscriptionConfigurationErrorMessageContainer : IErrorMessageContainer
{
    private static readonly IReadOnlyDictionary<int, string> _messageContainer = ImmutableDictionary.CreateRange<int, string>([
        new((int)AdministrationSubscriptionConfigurationErrors.SUBSCRIPTION_CONFLICT_COMPANY_NOT_FOUND, "Company {companyId} not found"),
        new((int)AdministrationSubscriptionConfigurationErrors.SUBSCRIPTION_CONFLICT_AUTO_SETUP_NOT_FOUND, "Company {companyId} does not have auto setup configured" ),
        new((int)AdministrationSubscriptionConfigurationErrors.SUBSCRIPTION_FORBIDDEN_COMPANY_NOT_PROVIDER, "Company {companyId} is not an App/Service provider"),
        new((int)AdministrationSubscriptionConfigurationErrors.SUBSCRIPTION_CONFLICT_CLIENT_MUST_SET, "Client id should not be null or empty"),
        new((int)AdministrationSubscriptionConfigurationErrors.SUBSCRIPTION_CONFLICT_SECRET_MUST_SET, "Client secret should not be null or empty"),
        new((int)AdministrationSubscriptionConfigurationErrors.SUBSCRIPTION_ARGUMENT_MAX_LENGTH_ALLOW_HUNDRED_CHAR, "the maximum allowed length is 100 characters")
    ]);

    public Type Type { get => typeof(AdministrationSubscriptionConfigurationErrors); }
    public IReadOnlyDictionary<int, string> MessageContainer { get => _messageContainer; }
}

public enum AdministrationSubscriptionConfigurationErrors
{
    SUBSCRIPTION_CONFLICT_COMPANY_NOT_FOUND,
    SUBSCRIPTION_CONFLICT_AUTO_SETUP_NOT_FOUND,
    SUBSCRIPTION_FORBIDDEN_COMPANY_NOT_PROVIDER,
    SUBSCRIPTION_CONFLICT_CLIENT_MUST_SET,
    SUBSCRIPTION_CONFLICT_SECRET_MUST_SET,
    SUBSCRIPTION_ARGUMENT_MAX_LENGTH_ALLOW_HUNDRED_CHAR

}
