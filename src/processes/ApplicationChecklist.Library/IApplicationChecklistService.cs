/********************************************************************************
 * Copyright (c) 2021, 2023 Microsoft and BMW Group AG
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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.ApplicationChecklist.Library;

public interface IApplicationChecklistService
{
	record ManualChecklistProcessStepData(Guid ApplicationId, Process Process, Guid ProcessStepId, ApplicationChecklistEntryTypeId EntryTypeId, ImmutableDictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId> Checklist, IEnumerable<ProcessStep> ProcessSteps);
	record WorkerChecklistProcessStepData(Guid ApplicationId, ProcessStepTypeId ProcessStepTypeId, ImmutableDictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId> Checklist, IEnumerable<ProcessStepTypeId> ProcessStepTypeIds);
	record WorkerChecklistProcessStepExecutionResult(ProcessStepStatusId StepStatusId, Action<ApplicationChecklistEntry>? ModifyChecklistEntry, IEnumerable<ProcessStepTypeId>? ScheduleStepTypeIds, IEnumerable<ProcessStepTypeId>? SkipStepTypeIds, bool Modified, string? ProcessMessage);

	Task<ManualChecklistProcessStepData> VerifyChecklistEntryAndProcessSteps(Guid applicationId, ApplicationChecklistEntryTypeId entryTypeId, IEnumerable<ApplicationChecklistEntryStatusId> entryStatusIds, ProcessStepTypeId processStepTypeId, IEnumerable<ApplicationChecklistEntryTypeId>? entryTypeIds = null, IEnumerable<ProcessStepTypeId>? processStepTypeIds = null);
	void RequestLock(IApplicationChecklistService.ManualChecklistProcessStepData context, DateTimeOffset lockExpiryDate);
	void SkipProcessSteps(ManualChecklistProcessStepData context, IEnumerable<ProcessStepTypeId> processStepTypeIds);
	void FinalizeChecklistEntryAndProcessSteps(ManualChecklistProcessStepData context, Action<ApplicationChecklistEntry>? modifyApplicationChecklistEntry, IEnumerable<ProcessStepTypeId>? nextProcessStepTypeIds);

	Task<IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult> HandleServiceErrorAsync(Exception exception, ProcessStepTypeId manualProcessTriggerStep);
}
