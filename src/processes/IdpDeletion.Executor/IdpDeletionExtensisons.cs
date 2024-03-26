/********************************************************************************
 * Copyright (c) 2021, 2023 BMW Group AG
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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.IdpDeletion.Executor;

public static class IdpDeletionExtensisons
{
    public static IEnumerable<ProcessStepTypeId>? GetIdpDeletionRetriggerStep(this ProcessStepTypeId processStepTypeId) =>
        processStepTypeId switch
        {
            ProcessStepTypeId.TRIGGER_DELETE_IDP_SHARED_REALM => new[] { ProcessStepTypeId.RETRIGGER_DELETE_IDP_SHARED_REALM },
            ProcessStepTypeId.TRIGGER_DELETE_IDP_SHARED_SERVICEACCOUNT => new[] { ProcessStepTypeId.RETRIGGER_DELETE_IDP_SHARED_SERVICEACCOUNT },
            ProcessStepTypeId.TRIGGER_DELETE_IDENTITY_LINKED_USERS => new[] { ProcessStepTypeId.RETRIGGER_DELETE_IDENTITY_LINKED_USERS },
            ProcessStepTypeId.TRIGGER_DELETE_CENTRAL_IDENTITY_PROVIDER => new[] { ProcessStepTypeId.RETRIGGER_DELETE_CENTRAL_IDENTITY_PROVIDER },
            ProcessStepTypeId.TRIGGER_DELETE_IDENTITY_PROVIDER => new[] { ProcessStepTypeId.RETRIGGER_DELETE_IDENTITY_PROVIDER },
            _ => throw new UnexpectedConditionException($"ProcessStepTypeId {processStepTypeId} is not supported for Process IdpDeletion")
        };
}
