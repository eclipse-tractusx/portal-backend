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

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Extensions;

public static class ProcessTypeExtensions
{
    public static ProcessStepTypeId GetInitialProcessStepTypeIdForSaCreation(this ProcessTypeId processTypeId) =>
        processTypeId switch
        {
            ProcessTypeId.DIM_TECHNICAL_USER => ProcessStepTypeId.CREATE_DIM_TECHNICAL_USER,
            ProcessTypeId.OFFER_SUBSCRIPTION => ProcessStepTypeId.OFFERSUBSCRIPTION_CREATE_DIM_TECHNICAL_USER,
            _ => throw new ArgumentException($"ProcessType {processTypeId} is not supported")
        };

    public static (ProcessTypeId ProcessType, ProcessStepTypeId NextStep) GetProcessStepForRetrigger(this ProcessStepTypeId stepToTrigger) =>
        stepToTrigger switch
        {
            ProcessStepTypeId.RETRIGGER_INVITATION_CREATE_CENTRAL_IDP => (ProcessTypeId.INVITATION, ProcessStepTypeId.INVITATION_CREATE_CENTRAL_IDP),
            ProcessStepTypeId.RETRIGGER_INVITATION_CREATE_SHARED_IDP_SERVICE_ACCOUNT => (ProcessTypeId.INVITATION, ProcessStepTypeId.INVITATION_CREATE_SHARED_IDP_SERVICE_ACCOUNT),
            ProcessStepTypeId.RETRIGGER_INVITATION_ADD_REALM_ROLE => (ProcessTypeId.INVITATION, ProcessStepTypeId.INVITATION_ADD_REALM_ROLE),
            ProcessStepTypeId.RETRIGGER_INVITATION_UPDATE_CENTRAL_IDP_URLS => (ProcessTypeId.INVITATION, ProcessStepTypeId.INVITATION_UPDATE_CENTRAL_IDP_URLS),
            ProcessStepTypeId.RETRIGGER_INVITATION_CREATE_CENTRAL_IDP_ORG_MAPPER => (ProcessTypeId.INVITATION, ProcessStepTypeId.INVITATION_CREATE_CENTRAL_IDP_ORG_MAPPER),
            ProcessStepTypeId.RETRIGGER_INVITATION_CREATE_SHARED_REALM => (ProcessTypeId.INVITATION, ProcessStepTypeId.INVITATION_CREATE_SHARED_REALM),
            ProcessStepTypeId.RETRIGGER_INVITATION_CREATE_SHARED_CLIENT => (ProcessTypeId.INVITATION, ProcessStepTypeId.INVITATION_CREATE_SHARED_CLIENT),
            ProcessStepTypeId.RETRIGGER_INVITATION_ENABLE_CENTRAL_IDP => (ProcessTypeId.INVITATION, ProcessStepTypeId.INVITATION_ENABLE_CENTRAL_IDP),
            ProcessStepTypeId.RETRIGGER_INVITATION_CREATE_USER => (ProcessTypeId.INVITATION, ProcessStepTypeId.INVITATION_CREATE_USER),
            ProcessStepTypeId.RETRIGGER_INVITATION_CREATE_DATABASE_IDP => (ProcessTypeId.INVITATION, ProcessStepTypeId.INVITATION_CREATE_DATABASE_IDP),
            ProcessStepTypeId.RETRIGGER_CREATE_DIM_TECHNICAL_USER => (ProcessTypeId.DIM_TECHNICAL_USER, ProcessStepTypeId.CREATE_DIM_TECHNICAL_USER),
            ProcessStepTypeId.RETRIGGER_DELETE_DIM_TECHNICAL_USER => (ProcessTypeId.DIM_TECHNICAL_USER, ProcessStepTypeId.DELETE_DIM_TECHNICAL_USER),
            ProcessStepTypeId.RETRIGGER_SEND_MAIL => (ProcessTypeId.MAILING, ProcessStepTypeId.SEND_MAIL),
            ProcessStepTypeId.RETRIGGER_DELETE_IDP_SHARED_REALM => (ProcessTypeId.IDENTITYPROVIDER_PROVISIONING, ProcessStepTypeId.DELETE_IDP_SHARED_REALM),
            ProcessStepTypeId.RETRIGGER_DELETE_IDP_SHARED_SERVICEACCOUNT => (ProcessTypeId.IDENTITYPROVIDER_PROVISIONING, ProcessStepTypeId.DELETE_IDP_SHARED_SERVICEACCOUNT),
            ProcessStepTypeId.RETRIGGER_DELETE_CENTRAL_IDENTITY_PROVIDER => (ProcessTypeId.IDENTITYPROVIDER_PROVISIONING, ProcessStepTypeId.DELETE_CENTRAL_IDENTITY_PROVIDER),
            ProcessStepTypeId.RETRIGGER_DELETE_CENTRAL_USER => (ProcessTypeId.USER_PROVISIONING, ProcessStepTypeId.DELETE_CENTRAL_USER),
            ProcessStepTypeId.RETRIGGER_DELETE_BPN_FROM_CENTRAL_USER => (ProcessTypeId.USER_BPN, ProcessStepTypeId.DELETE_BPN_FROM_CENTRAL_USER),
            ProcessStepTypeId.RETRIGGER_CHECK_LEGAL_ENTITY_DATA => (ProcessTypeId.USER_BPN, ProcessStepTypeId.CHECK_LEGAL_ENTITY_DATA),
            ProcessStepTypeId.RETRIGGER_ADD_BPN_TO_IDENTITY => (ProcessTypeId.USER_BPN, ProcessStepTypeId.ADD_BPN_TO_IDENTITY),
            _ => throw new UnexpectedConditionException($"Step {stepToTrigger} is not retriggerable")
        };
}
