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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Checklist.Library;

public sealed class ChecklistService : IChecklistService
{
    private readonly IPortalRepositories _portalRepositories;

    public ChecklistService(IPortalRepositories portalRepositories)
    {
        _portalRepositories = portalRepositories;
    }

    async Task<IChecklistService.ManualChecklistProcessStepData> IChecklistService.VerifyChecklistEntryAndProcessSteps(Guid applicationId, ApplicationChecklistEntryTypeId entryTypeId, IEnumerable<ApplicationChecklistEntryStatusId> entryStatusIds, ProcessStepTypeId processStepTypeId, IEnumerable<ApplicationChecklistEntryTypeId>? entryTypeIds, IEnumerable<ProcessStepTypeId>? processStepTypeIds)
    {
        var allProcessStepTypeIds = processStepTypeIds == null
            ? new [] { processStepTypeId }
            : processStepTypeIds.Append(processStepTypeId);

        var allEntryTypeIds = entryTypeIds == null
            ? new [] { entryTypeId }
            : entryTypeIds.Append(entryTypeId);

        var checklistData = await _portalRepositories.GetInstance<IApplicationChecklistRepository>()
            .GetChecklistProcessStepData(applicationId, allEntryTypeIds, allProcessStepTypeIds).ConfigureAwait(false);

        checklistData.ValidateChecklistData(applicationId, entryTypeId, entryStatusIds, new [] { ProcessStepStatusId.TODO });
        var processStep = checklistData!.ProcessSteps!.SingleOrDefault(step => step.ProcessStepTypeId == processStepTypeId);
        if (processStep is null)
        {
            throw new ConflictException($"application {applicationId} checklist entry {entryTypeId}, process step {processStepTypeId} is not eligible to run");
        }
        return checklistData.CreateManualChecklistProcessStepData(applicationId, entryTypeId, processStep);
    }

    public void SkipProcessSteps(IChecklistService.ManualChecklistProcessStepData context, IEnumerable<ProcessStepTypeId> processStepTypeIds)
    {
        var processStepRepository = _portalRepositories.GetInstance<IProcessStepRepository>();
        foreach (var processStepGroup in context.ProcessSteps.GroupBy(step => step.ProcessStepTypeId).IntersectBy(processStepTypeIds, step => step.Key))
        {
            var firstModified = false;
            foreach (var processStep in processStepGroup)
            {
                processStepRepository.AttachAndModifyProcessStep(
                    processStep.Id,
                    null,
                    step => step.ProcessStepStatusId =
                        firstModified
                            ? ProcessStepStatusId.DUPLICATE
                            : ProcessStepStatusId.SKIPPED);
                firstModified = true;
            }
        }
    }

    public void FinalizeChecklistEntryAndProcessSteps(IChecklistService.ManualChecklistProcessStepData context, Action<ApplicationChecklistEntry>? modifyApplicationChecklistEntry, IEnumerable<ProcessStepTypeId>? nextProcessStepTypeIds)
    {
        var applicationChecklistRepository = _portalRepositories.GetInstance<IApplicationChecklistRepository>();
        var processStepRepository = _portalRepositories.GetInstance<IProcessStepRepository>();

        if (modifyApplicationChecklistEntry != null)
        {
            applicationChecklistRepository
                .AttachAndModifyApplicationChecklist(context.ApplicationId, context.EntryTypeId, modifyApplicationChecklistEntry);
        }
        processStepRepository.AttachAndModifyProcessStep(context.ProcessStepId, null, step => step.ProcessStepStatusId = ProcessStepStatusId.DONE);
        if (nextProcessStepTypeIds == null || !nextProcessStepTypeIds.Any())
        {
            return;
        }

        processStepRepository.CreateProcessStepRange(
            nextProcessStepTypeIds
                .Except(context.ProcessSteps.Select(step => step.ProcessStepTypeId))
                .Select(stepTypeId => (stepTypeId, ProcessStepStatusId.TODO, context.ProcessId)));
    }

    public Task<IChecklistService.WorkerChecklistProcessStepExecutionResult> HandleServiceErrorAsync(Exception exception, ProcessStepTypeId manualProcessTriggerStep)
    {
        return Task.FromResult(
            exception is ServiceException { IsRecoverable: true }
                ? new IChecklistService.WorkerChecklistProcessStepExecutionResult(
                    ProcessStepStatusId.TODO,
                    item =>
                    {
                        item.Comment = exception.ToString();
                    },
                    null,
                    null,
                    true)
                : new IChecklistService.WorkerChecklistProcessStepExecutionResult(
                    ProcessStepStatusId.FAILED,
                    item =>
                    {
                        item.ApplicationChecklistEntryStatusId = ApplicationChecklistEntryStatusId.FAILED;
                        item.Comment = exception.ToString();
                    },
                    new [] { manualProcessTriggerStep },
                    null,
                    true));
    }
}
