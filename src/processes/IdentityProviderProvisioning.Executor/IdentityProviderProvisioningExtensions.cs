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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.IdentityProviderProvisioning.Executor;

public static class IdentityProviderProvisioningExtensions
{
    public static IEnumerable<ProcessStepTypeId>? GetIdentityProviderProvisioningRetriggerStep(this ProcessStepTypeId processStepTypeId) =>
        processStepTypeId switch
        {
            ProcessStepTypeId.DELETE_IDP_SHARED_REALM => [ProcessStepTypeId.RETRIGGER_DELETE_IDP_SHARED_REALM],
            ProcessStepTypeId.DELETE_IDP_SHARED_SERVICEACCOUNT => [ProcessStepTypeId.RETRIGGER_DELETE_IDP_SHARED_SERVICEACCOUNT],
            ProcessStepTypeId.DELETE_CENTRAL_IDENTITY_PROVIDER => [ProcessStepTypeId.RETRIGGER_DELETE_CENTRAL_IDENTITY_PROVIDER],
            _ => throw new UnexpectedConditionException($"ProcessStepTypeId {processStepTypeId} is not supported for Process IdentityProviderProvisioning")
        };
}
