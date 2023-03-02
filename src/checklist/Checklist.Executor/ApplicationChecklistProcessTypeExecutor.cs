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

using Org.Eclipse.TractusX.Portal.Backend.Checklist.Library;
using Org.Eclipse.TractusX.Portal.Backend.Checklist.Worker.Library;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Checklist.Executor;

public class ApplicationChecklistProcessTypeExecutor : IProcessTypeExecutor
{
    private readonly IChecklistHandlerService _checklistHandlerService;
    private readonly IChecklistCreationService _checklistCreationService;
    private readonly IApplicationChecklistRepository _checklistRepository;

    private Guid applicationId;
    private IDictionary<ApplicationChecklistEntryTypeId,ApplicationChecklistEntryStatusId>? checklist = null;

    public ApplicationChecklistProcessTypeExecutor(
        IChecklistHandlerService checklistHandlerService,
        IChecklistCreationService checklistCreationService,
        IPortalRepositories portalRepositories)
    {
        _checklistHandlerService = checklistHandlerService;
        _checklistCreationService = checklistCreationService;
        _checklistRepository = portalRepositories.GetInstance<IApplicationChecklistRepository>();
    }

    public ProcessTypeId GetProcessTypeId() => ProcessTypeId.APPLICATION_CHECKLIST;
    public bool IsExecutableStepTypeId(ProcessStepTypeId processStepTypeId) => _checklistHandlerService.IsExecutableProcessStep(processStepTypeId);
    public IEnumerable<ProcessStepTypeId> GetExecutableStepTypeIds() => _checklistHandlerService.GetExecutableStepTypeIds();

    public async Task<IProcessTypeExecutor.InitializationResult> InitializeProcess(Guid processId, IEnumerable<ProcessStepTypeId> processStepTypeIds)
    {
        applicationId = Guid.Empty;
        checklist = null;

        var result = await _checklistRepository.GetChecklistData(processId).ConfigureAwait(false);
        if (!result.IsValidProcessId)
        {
            throw new NotFoundException($"process {processId} does not exist");
        }
        if (result.ApplicationId == Guid.Empty)
        {
            throw new ConflictException($"process {processId} is not associated with an application");
        }
        if (result.ApplicationStatusId != CompanyApplicationStatusId.SUBMITTED)
        {
            throw new ConflictException($"application {result.ApplicationId} is not in status SUBMITTED");
        }

        applicationId = result.ApplicationId;
        checklist = result.Checklist.ToDictionary(x => x.EntryTypeId, x => x.EntryStatusId);

        if (Enum.GetValues<ApplicationChecklistEntryTypeId>().Except(checklist.Keys).Any())
        {
            var createdEntries = (await _checklistCreationService
                .CreateMissingChecklistItems(applicationId, checklist.Keys).ConfigureAwait(false)).ToList();

            if (createdEntries.Any())
            {
                createdEntries.ForEach(entry => checklist[entry.TypeId] = entry.StatusId);
                return new IProcessTypeExecutor.InitializationResult(true, _checklistCreationService.GetInitialProcessStepTypeIds(createdEntries));
            }
        }
        return new IProcessTypeExecutor.InitializationResult(false, null);
    }

    public async Task<IProcessTypeExecutor.StepExecutionResult> ExecuteProcessStep(ProcessStepTypeId processStepTypeId, IEnumerable<ProcessStepTypeId> processStepTypeIds, CancellationToken cancellationToken)
    {
        if (applicationId == Guid.Empty || checklist == null)
        {
            throw new UnexpectedConditionException("applicationId or checklist should never be null or empty here");
        }
        var execution = _checklistHandlerService.GetProcessStepExecution(processStepTypeId);
        if (!checklist!.ContainsKey(execution.EntryTypeId))
        {
            throw new UnexpectedConditionException($"checklist should always contain an entry for {execution.EntryTypeId} here");
        }
        var stepData = new IChecklistService.WorkerChecklistProcessStepData(
            applicationId,
            processStepTypeId,
            checklist.ToImmutableDictionary(),
            processStepTypeIds);

        Action<ApplicationChecklistEntry>? modifyChecklistEntry;
        IEnumerable<ProcessStepTypeId>? nextStepTypeIds;
        ProcessStepStatusId stepStatusId;
        bool modified;
        try
        {
            (modifyChecklistEntry, nextStepTypeIds, modified) = await execution.ProcessFunc(stepData, cancellationToken).ConfigureAwait(false);
            stepStatusId = ProcessStepStatusId.DONE;
        }
        catch (Exception ex) when (ex is not SystemException)
        {
            if (execution.ErrorFunc == null)
            {
                (stepStatusId, modifyChecklistEntry) = ProcessError(ex);
                nextStepTypeIds = null;
                modified = true;
            }
            else
            {
                (modifyChecklistEntry, nextStepTypeIds, modified) = await execution.ErrorFunc(ex, stepData, cancellationToken).ConfigureAwait(false);
                stepStatusId = ProcessStepStatusId.FAILED;
            }
        }

        modified |= ProcessChecklistEntry(
            execution.EntryTypeId,
            modifyChecklistEntry);

        return new IProcessTypeExecutor.StepExecutionResult(modified, stepStatusId, nextStepTypeIds, null);
    }

    private bool ProcessChecklistEntry(
        ApplicationChecklistEntryTypeId entryTypeId,
        Action<ApplicationChecklistEntry>? modifyApplicationChecklistEntry)
    {
        if (modifyApplicationChecklistEntry == null)
        {
            return false;
        }
        var entry = _checklistRepository
            .AttachAndModifyApplicationChecklist(
                applicationId,
                entryTypeId,
                modifyApplicationChecklistEntry);
        checklist![entryTypeId] = entry.ApplicationChecklistEntryStatusId;
        return true;
    }

    private static (ProcessStepStatusId,Action<ApplicationChecklistEntry>?) ProcessError(Exception ex) =>
        ex is not ServiceException { IsRecoverable: true }
            ? ( ProcessStepStatusId.FAILED,
                item => {
                    item.ApplicationChecklistEntryStatusId = ApplicationChecklistEntryStatusId.FAILED;
                    item.Comment = string.IsNullOrEmpty(ex.Message) ? ex.ToString() : ex.Message;
                })
            : ( ProcessStepStatusId.TODO,
                item => {
                    item.Comment = string.IsNullOrEmpty(ex.Message) ? ex.ToString() : ex.Message;
                });
}
