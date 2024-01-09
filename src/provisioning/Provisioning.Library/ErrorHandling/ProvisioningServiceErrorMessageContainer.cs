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

namespace Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.ErrorHandling;

public class ProvisioningServiceErrorMessageContainer : IErrorMessageContainer
{
    private static readonly IReadOnlyDictionary<int, string> _messageContainer = new Dictionary<ProvisioningServiceErrors, string> {
                { ProvisioningServiceErrors.USER_CREATION_USERNAME_NULL, "userName is {userName} when trying to create user in realm {realm}" },
                { ProvisioningServiceErrors.USER_CREATION_CONFLICT, "userName {userName} already exists in realm {realm}" },
                { ProvisioningServiceErrors.USER_CREATION_NOTFOUND, "realm {realm} not found to create userName {userName}" },
                { ProvisioningServiceErrors.USER_CREATION_ARGUMENT, "invalid realm {realm} or userName {userName} for usercreation" },
                { ProvisioningServiceErrors.USER_CREATION_FAILURE, "unexpected error while creating userName {userName} in realm {realm}" },
                { ProvisioningServiceErrors.USER_CREATION_RETURNS_NULL, "creation of userName {userName} in realm {realm} returns null" },
                { ProvisioningServiceErrors.USER_NOT_VALID_USERROLEID, "{missingRoleIds} are not a valid UserRoleIds"}
            }.ToImmutableDictionary(x => (int)x.Key, x => x.Value);

    public Type Type { get => typeof(ProvisioningServiceErrors); }
    public IReadOnlyDictionary<int, string> MessageContainer { get => _messageContainer; }
}

public enum ProvisioningServiceErrors
{
    USER_CREATION_USERNAME_NULL,
    USER_CREATION_CONFLICT,
    USER_CREATION_NOTFOUND,
    USER_CREATION_ARGUMENT,
    USER_CREATION_FAILURE,
    USER_CREATION_RETURNS_NULL,
    USER_NOT_VALID_USERROLEID
}
