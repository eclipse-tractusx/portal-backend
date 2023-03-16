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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.ApplicationChecklist.Library;

public static class ApplicationChecklistEntryTypeIdExtensions
{
    private static readonly ImmutableDictionary<ApplicationChecklistEntryTypeId,IEnumerable<ProcessStepTypeId>> _manualProcessStepIds = new (ApplicationChecklistEntryTypeId EntryTypeId, IEnumerable<ProcessStepTypeId> StepTypeId)[] {
            (ApplicationChecklistEntryTypeId.CLEARING_HOUSE, new [] { ProcessStepTypeId.RETRIGGER_CLEARING_HOUSE, ProcessStepTypeId.TRIGGER_OVERRIDE_CLEARING_HOUSE }),
            (ApplicationChecklistEntryTypeId.IDENTITY_WALLET, new [] { ProcessStepTypeId.RETRIGGER_IDENTITY_WALLET }),
            (ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP, new [] { ProcessStepTypeId.RETRIGGER_SELF_DESCRIPTION_LP }),
            (ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, new [] { ProcessStepTypeId.RETRIGGER_BUSINESS_PARTNER_NUMBER_PUSH, ProcessStepTypeId.RETRIGGER_BUSINESS_PARTNER_NUMBER_PULL }),
        }.ToImmutableDictionary(x => x.EntryTypeId, x => x.StepTypeId);

    public static IEnumerable<ProcessStepTypeId> GetManualTriggerProcessStepIds(this ApplicationChecklistEntryTypeId entryTypeId) =>
        _manualProcessStepIds.TryGetValue(entryTypeId, out var stepTypeId)
            ? stepTypeId
            : Enumerable.Empty<ProcessStepTypeId>();

    public static IEnumerable<ProcessStepTypeId> GetManualTriggerProcessStepIds(this IEnumerable<ApplicationChecklistEntryTypeId> entryTypeIds) =>
        _manualProcessStepIds.IntersectBy(entryTypeIds, x => x.Key).SelectMany(x => x.Value).Distinct();

    public static (ProcessStepTypeId ProcessStepTypeId, ApplicationChecklistEntryStatusId ChecklistEntryStatusId) GetNextProcessStepDataForManualTriggerProcessStepId(this ProcessStepTypeId processStepTypeId) =>
        processStepTypeId switch
        {
            ProcessStepTypeId.RETRIGGER_CLEARING_HOUSE => (ProcessStepTypeId.START_CLEARING_HOUSE, ApplicationChecklistEntryStatusId.TO_DO),
            ProcessStepTypeId.RETRIGGER_IDENTITY_WALLET => (ProcessStepTypeId.CREATE_IDENTITY_WALLET, ApplicationChecklistEntryStatusId.TO_DO),
            ProcessStepTypeId.RETRIGGER_SELF_DESCRIPTION_LP => (ProcessStepTypeId.START_SELF_DESCRIPTION_LP, ApplicationChecklistEntryStatusId.TO_DO),
            ProcessStepTypeId.RETRIGGER_BUSINESS_PARTNER_NUMBER_PUSH => (ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PUSH, ApplicationChecklistEntryStatusId.TO_DO),
            ProcessStepTypeId.RETRIGGER_BUSINESS_PARTNER_NUMBER_PULL => (ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PULL, ApplicationChecklistEntryStatusId.IN_PROGRESS),
            ProcessStepTypeId.TRIGGER_OVERRIDE_CLEARING_HOUSE => (ProcessStepTypeId.START_OVERRIDE_CLEARING_HOUSE, ApplicationChecklistEntryStatusId.TO_DO),
            _ => default,
        };
}
