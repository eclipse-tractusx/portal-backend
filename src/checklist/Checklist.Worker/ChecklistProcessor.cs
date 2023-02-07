/********************************************************************************
 * Copyright (c) 2021,2023 Microsoft and BMW Group AG
 * Copyright (c) 2021,2023 Contributors to the Eclipse Foundation
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

using Microsoft.Extensions.Logging;
using Org.Eclipse.TractusX.Portal.Backend.Checklist.Library;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Collections.Immutable;
using System.Net;
using System.Runtime.CompilerServices;

namespace Org.Eclipse.TractusX.Portal.Backend.Checklist.Worker;

/// <inheritdoc />
public class ChecklistProcessor : IChecklistProcessor
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly IChecklistHandlerService _checklistHandlerService;
    private readonly ILogger<IChecklistProcessor> _logger;

    /// <summary>
    /// Creates a new instance of <see cref="ChecklistProcessor"/>
    /// </summary>
    /// <param name="portalRepositories">Access to the repositories</param>
    /// <param name="checklistHandlerService">Handler for the checklist</param>
    /// <param name="logger">The logger</param>
    public ChecklistProcessor(
        IPortalRepositories portalRepositories,
        IChecklistHandlerService checklistHandlerService,
        ILogger<IChecklistProcessor> logger)
    {
        _portalRepositories = portalRepositories;
        _checklistHandlerService = checklistHandlerService;
        _logger = logger;
    }

    private sealed record ProcessingContext(
        Guid ApplicationId,
        IDictionary<ApplicationChecklistEntryTypeId,ApplicationChecklistEntryStatusId> Checklist,
        IDictionary<ProcessStepTypeId, IEnumerable<ProcessStep>> AllSteps,
        Queue<ProcessStepTypeId> WorkerStepTypeIds,
        IList<ProcessStepTypeId> ManualStepTypeIds,
        IApplicationChecklistRepository ChecklistRepository,
        IProcessStepRepository ProcessStepRepository);

    /// <inheritdoc />
    public async IAsyncEnumerable<(ApplicationChecklistEntryTypeId TypeId, ApplicationChecklistEntryStatusId StatusId)> ProcessChecklist(Guid applicationId, IEnumerable<(ApplicationChecklistEntryTypeId EntryTypeId, ApplicationChecklistEntryStatusId EntryStatusId)> checklistEntries, IEnumerable<ProcessStep> processSteps, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var allSteps = processSteps.GroupBy(step => step.ProcessStepTypeId).ToDictionary(group => group.Key, group => group.AsEnumerable());

        var context = new ProcessingContext(
            applicationId,
            checklistEntries.ToDictionary(entry => entry.EntryTypeId, entry => entry.EntryStatusId),
            allSteps,
            new Queue<ProcessStepTypeId>(allSteps.Keys.Where(step => !_checklistHandlerService.IsManualProcessStep(step))),
            allSteps.Keys.Where(step => _checklistHandlerService.IsManualProcessStep(step)).ToList(),
            _portalRepositories.GetInstance<IApplicationChecklistRepository>(),
            _portalRepositories.GetInstance<IProcessStepRepository>());

        _logger.LogInformation("Found {StepsCount} possible steps for application {ApplicationId}", context.WorkerStepTypeIds.Count, applicationId);

        while (context.WorkerStepTypeIds.TryDequeue(out var stepTypeId))
        {
            var execution = _checklistHandlerService.GetProcessStepExecution(stepTypeId);
            var entryStatusId = GetEntryStatusId(execution.EntryTypeId, stepTypeId, context.Checklist);
            var stepData = new IChecklistService.WorkerChecklistProcessStepData(
                applicationId,
                context.Checklist.ToImmutableDictionary(),
                context.WorkerStepTypeIds.Concat(context.ManualStepTypeIds));

            (Action<ApplicationChecklistEntry>?,IEnumerable<ProcessStepTypeId>?,bool) result;
            ProcessStepStatusId stepStatusId;
            try
            {
                result = await execution.ProcessFunc(stepData, cancellationToken).ConfigureAwait(false);
                stepStatusId = ProcessStepStatusId.DONE;
            }
            catch (Exception ex) when (ex is not SystemException)
            {
                if (execution.ErrorFunc == null)
                {
                    (stepStatusId, var modifyEntry) = ProcessError(ex);
                    result = (modifyEntry, null, true);
                }
                else
                {
                    result = await execution.ErrorFunc(ex,stepData,cancellationToken).ConfigureAwait(false);
                    stepStatusId = ProcessStepStatusId.FAILED;
                }
            }
            (entryStatusId, var modified) = ProcessResult(
                result,
                stepTypeId,
                stepStatusId,
                execution.EntryTypeId,
                entryStatusId,
                context);
            if (modified)
            {
                context.Checklist[execution.EntryTypeId] = entryStatusId;
                yield return (execution.EntryTypeId, entryStatusId);
            }
        }
    }

    private static ApplicationChecklistEntryStatusId GetEntryStatusId(
        ApplicationChecklistEntryTypeId entryTypeId,
        ProcessStepTypeId stepTypeId,
        IDictionary<ApplicationChecklistEntryTypeId,ApplicationChecklistEntryStatusId> checklist)
    {
        if (!checklist.TryGetValue(entryTypeId, out var entryStatusId))
        {
            throw new ConflictException($"no checklist entry {entryTypeId} for {stepTypeId}");
        }
        return entryStatusId;
    }

    private (ApplicationChecklistEntryStatusId EntryStatusId, bool Modified) ProcessResult(
        (Action<ApplicationChecklistEntry>? ModifyApplicationChecklistEntry,IEnumerable<ProcessStepTypeId>? NextSteps, bool Modified) executionResult,
        ProcessStepTypeId stepTypeId,
        ProcessStepStatusId stepStatusId,
        ApplicationChecklistEntryTypeId entryTypeId,
        ApplicationChecklistEntryStatusId entryStatusId,
        ProcessingContext context)
    {
        var modified = false;
        if (executionResult.Modified)
        {
            modified |= ModifyStep(stepTypeId, stepStatusId, context);
            modified |= ScheduleNextSteps(executionResult.NextSteps, context);
            if (executionResult.ModifyApplicationChecklistEntry != null)
            {
                var entry = context.ChecklistRepository
                    .AttachAndModifyApplicationChecklist(context.ApplicationId, entryTypeId,
                        executionResult.ModifyApplicationChecklistEntry);
                return (entry.ApplicationChecklistEntryStatusId, true);
            }
        }
        return (entryStatusId, modified);
    }

    private static (ProcessStepStatusId,Action<ApplicationChecklistEntry>?) ProcessError(Exception ex) =>
        ex is not ServiceException { StatusCode: HttpStatusCode.ServiceUnavailable } or HttpRequestException
            ? ( ProcessStepStatusId.FAILED,
                item => {
                    item.ApplicationChecklistEntryStatusId = ApplicationChecklistEntryStatusId.FAILED;
                    item.Comment = ex.ToString();
                })
            : ( ProcessStepStatusId.TODO,
                item => {
                    item.Comment = ex.ToString();
                });

    private bool ScheduleNextSteps(IEnumerable<ProcessStepTypeId>? nextSteps, ProcessingContext context)
    {
        if (nextSteps == null)
        {
            return false;
        }

        var modified = false;
        foreach (var nextStepTypeId in nextSteps.Except(context.AllSteps.Keys))
        {
            context.AllSteps.Add(nextStepTypeId, new[] { context.ProcessStepRepository.CreateProcessStep(nextStepTypeId, ProcessStepStatusId.TODO) });
            if (_checklistHandlerService.IsManualProcessStep(nextStepTypeId))
            {
                context.ManualStepTypeIds.Add(nextStepTypeId);
            }
            else
            {
                context.WorkerStepTypeIds.Enqueue(nextStepTypeId);
            }
            modified = true;
        }
        return modified;
    }
    
    private static bool ModifyStep(ProcessStepTypeId stepTypeId, ProcessStepStatusId statusId, ProcessingContext context)
    {
        if (!context.AllSteps.Remove(stepTypeId, out var currentSteps))
        {
            return false;
        }

        var firstModified = false;
        foreach (var processStep in currentSteps)
        {
            context.ProcessStepRepository.AttachAndModifyProcessStep(
                processStep.Id,
                null,
                step => step.ProcessStepStatusId =
                    firstModified
                        ? ProcessStepStatusId.DUPLICATE
                        : statusId);
            firstModified = true;
        }
        return firstModified;
    }
}
