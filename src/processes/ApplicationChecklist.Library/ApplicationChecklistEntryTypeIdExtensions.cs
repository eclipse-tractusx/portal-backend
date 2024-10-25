/********************************************************************************
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.ApplicationChecklist.Library;

public static class ApplicationChecklistEntryTypeIdExtensions
{
    private static readonly ImmutableDictionary<ApplicationChecklistEntryTypeId, IEnumerable<ProcessStepTypeId>> ManualProcessStepIds = ImmutableDictionary.CreateRange<ApplicationChecklistEntryTypeId, IEnumerable<ProcessStepTypeId>>([
        new(ApplicationChecklistEntryTypeId.CLEARING_HOUSE, [ProcessStepTypeId.RETRIGGER_CLEARING_HOUSE, ProcessStepTypeId.MANUAL_TRIGGER_OVERRIDE_CLEARING_HOUSE]),
        new(ApplicationChecklistEntryTypeId.IDENTITY_WALLET, [ProcessStepTypeId.RETRIGGER_IDENTITY_WALLET, ProcessStepTypeId.RETRIGGER_CREATE_DIM_WALLET, ProcessStepTypeId.RETRIGGER_VALIDATE_DID_DOCUMENT]),
        new(ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP, [ProcessStepTypeId.RETRIGGER_SELF_DESCRIPTION_LP]),
        new(ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, [ProcessStepTypeId.RETRIGGER_BUSINESS_PARTNER_NUMBER_PUSH, ProcessStepTypeId.RETRIGGER_BUSINESS_PARTNER_NUMBER_PULL]),
        new(ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION, [ProcessStepTypeId.RETRIGGER_ASSIGN_INITIAL_ROLES, ProcessStepTypeId.RETRIGGER_ASSIGN_BPN_TO_USERS, ProcessStepTypeId.RETRIGGER_REMOVE_REGISTRATION_ROLES, ProcessStepTypeId.RETRIGGER_SET_THEME, ProcessStepTypeId.RETRIGGER_SET_MEMBERSHIP, ProcessStepTypeId.RETRIGGER_SET_CX_MEMBERSHIP_IN_BPDM])
    ]);

    public static IEnumerable<ProcessStepTypeId> GetManualTriggerProcessStepIds(this ApplicationChecklistEntryTypeId entryTypeId) =>
        ManualProcessStepIds.TryGetValue(entryTypeId, out var stepTypeIds)
            ? stepTypeIds
            : Enumerable.Empty<ProcessStepTypeId>();

    public static IEnumerable<ProcessStepTypeId> GetManualTriggerProcessStepIds(this IEnumerable<ApplicationChecklistEntryTypeId> entryTypeIds) =>
        ManualProcessStepIds.IntersectBy(entryTypeIds, x => x.Key).SelectMany(x => x.Value).Distinct();

    public static (ProcessStepTypeId ProcessStepTypeId, ApplicationChecklistEntryStatusId ChecklistEntryStatusId) GetNextProcessStepDataForManualTriggerProcessStepId(this ProcessStepTypeId processStepTypeId) =>
        processStepTypeId switch
        {
            ProcessStepTypeId.RETRIGGER_CLEARING_HOUSE => (ProcessStepTypeId.START_CLEARING_HOUSE, ApplicationChecklistEntryStatusId.TO_DO),
            ProcessStepTypeId.RETRIGGER_IDENTITY_WALLET => (ProcessStepTypeId.CREATE_IDENTITY_WALLET, ApplicationChecklistEntryStatusId.TO_DO),
            ProcessStepTypeId.RETRIGGER_CREATE_DIM_WALLET => (ProcessStepTypeId.CREATE_DIM_WALLET, ApplicationChecklistEntryStatusId.TO_DO),
            ProcessStepTypeId.RETRIGGER_VALIDATE_DID_DOCUMENT => (ProcessStepTypeId.VALIDATE_DID_DOCUMENT, ApplicationChecklistEntryStatusId.TO_DO),
            ProcessStepTypeId.RETRIGGER_SELF_DESCRIPTION_LP => (ProcessStepTypeId.START_SELF_DESCRIPTION_LP, ApplicationChecklistEntryStatusId.TO_DO),
            ProcessStepTypeId.RETRIGGER_BUSINESS_PARTNER_NUMBER_PUSH => (ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PUSH, ApplicationChecklistEntryStatusId.TO_DO),
            ProcessStepTypeId.RETRIGGER_BUSINESS_PARTNER_NUMBER_PULL => (ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PULL, ApplicationChecklistEntryStatusId.IN_PROGRESS),
            ProcessStepTypeId.MANUAL_TRIGGER_OVERRIDE_CLEARING_HOUSE => (ProcessStepTypeId.START_OVERRIDE_CLEARING_HOUSE, ApplicationChecklistEntryStatusId.TO_DO),
            ProcessStepTypeId.RETRIGGER_ASSIGN_INITIAL_ROLES => (ProcessStepTypeId.ASSIGN_INITIAL_ROLES, ApplicationChecklistEntryStatusId.IN_PROGRESS),
            ProcessStepTypeId.RETRIGGER_ASSIGN_BPN_TO_USERS => (ProcessStepTypeId.ASSIGN_BPN_TO_USERS, ApplicationChecklistEntryStatusId.IN_PROGRESS),
            ProcessStepTypeId.RETRIGGER_REMOVE_REGISTRATION_ROLES => (ProcessStepTypeId.REMOVE_REGISTRATION_ROLES, ApplicationChecklistEntryStatusId.IN_PROGRESS),
            ProcessStepTypeId.RETRIGGER_SET_THEME => (ProcessStepTypeId.SET_THEME, ApplicationChecklistEntryStatusId.IN_PROGRESS),
            ProcessStepTypeId.RETRIGGER_SET_MEMBERSHIP => (ProcessStepTypeId.SET_MEMBERSHIP, ApplicationChecklistEntryStatusId.IN_PROGRESS),
            ProcessStepTypeId.RETRIGGER_SET_CX_MEMBERSHIP_IN_BPDM => (ProcessStepTypeId.SET_CX_MEMBERSHIP_IN_BPDM, ApplicationChecklistEntryStatusId.IN_PROGRESS),
            _ => default,
        };
}
