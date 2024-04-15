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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.Invitation.Executor.Extensions;

public static class InvitationProcessStepTypeIdExtensions
{
    public static IEnumerable<ProcessStepTypeId> GetInvitationRetriggerStep(this ProcessStepTypeId processStepTypeId) =>
        processStepTypeId switch
        {
            ProcessStepTypeId.INVITATION_CREATE_CENTRAL_IDP => [ProcessStepTypeId.RETRIGGER_INVITATION_CREATE_CENTRAL_IDP],
            ProcessStepTypeId.INVITATION_CREATE_SHARED_IDP_SERVICE_ACCOUNT => [ProcessStepTypeId.RETRIGGER_INVITATION_CREATE_SHARED_IDP_SERVICE_ACCOUNT],
            ProcessStepTypeId.INVITATION_UPDATE_CENTRAL_IDP_URLS => [ProcessStepTypeId.RETRIGGER_INVITATION_UPDATE_CENTRAL_IDP_URLS],
            ProcessStepTypeId.INVITATION_ADD_REALM_ROLE => [ProcessStepTypeId.RETRIGGER_INVITATION_ADD_REALM_ROLE],
            ProcessStepTypeId.INVITATION_CREATE_CENTRAL_IDP_ORG_MAPPER => [ProcessStepTypeId.RETRIGGER_INVITATION_CREATE_CENTRAL_IDP_ORG_MAPPER],
            ProcessStepTypeId.INVITATION_CREATE_SHARED_REALM => [ProcessStepTypeId.RETRIGGER_INVITATION_CREATE_SHARED_REALM],
            ProcessStepTypeId.INVITATION_CREATE_SHARED_CLIENT => [ProcessStepTypeId.RETRIGGER_INVITATION_CREATE_SHARED_CLIENT],
            ProcessStepTypeId.INVITATION_ENABLE_CENTRAL_IDP => [ProcessStepTypeId.RETRIGGER_INVITATION_ENABLE_CENTRAL_IDP],
            ProcessStepTypeId.INVITATION_CREATE_USER => [ProcessStepTypeId.RETRIGGER_INVITATION_CREATE_USER],
            ProcessStepTypeId.INVITATION_CREATE_DATABASE_IDP => [ProcessStepTypeId.RETRIGGER_INVITATION_CREATE_DATABASE_IDP],
            _ => throw new UnexpectedConditionException($"ProcessStepTypeId {processStepTypeId} is not supported for Process Invitation")
        };
}
