/********************************************************************************
 * Copyright (c) 2022 Microsoft and BMW Group AG
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

using Microsoft.Extensions.Logging;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Async;
using Org.Eclipse.TractusX.Portal.Backend.Framework.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Next.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Next.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Next.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Worker.Library.Next.Extensions;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Worker.Library.Next;

public class ProcessExecutor<TProcessTypeId> : IProcessExecutor
    where TProcessTypeId : struct, Enum
{
    private readonly ImmutableDictionary<TProcessTypeId, IProcessTypeExecutor<TProcessTypeId>> _executors;
    private readonly IProcessStepRepository _processStepRepository;
    private readonly ILogger<ProcessExecutor<TProcessTypeId>> _logger;

    public ProcessExecutor(IEnumerable<IProcessTypeExecutor<TProcessTypeId>> executors, IRepositories processRepositories, ILogger<ProcessExecutor<TProcessTypeId>> logger)
    {
        _processStepRepository = processRepositories.GetInstance<IProcessStepRepository>();
        _executors = executors.ToImmutableDictionary(x => x.GetProcessTypeId());
        _logger = logger;
    }

    public async IAsyncEnumerable<IProcessExecutor.ProcessExecutionResult> ExecuteProcess(Guid processId, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var allSteps = await _processStepRepository
            .GetProcessStepData(processId)
            .PreSortedGroupBy(x => x.ProcessTypeId, x => new { x.ProcessStepTypeId, x.ProcessStepId })
            .ToDictionaryAsync(g => g.Key, g => g.AsEnumerable(), cancellationToken)
            .ConfigureAwait(false);

        foreach (var (processTypeId, steps) in allSteps)
        {
            if (!_executors.TryGetValue((TProcessTypeId)(object)processTypeId, out var executor))
            {
                throw new UnexpectedConditionException($"processType {processTypeId} is not a registered executable processType.");
            }

            var executableProcessStepTypeIds = ((TProcessTypeId)(object)processTypeId).GetExecutableProcessStepTypeIdsForProcessType();
            var context = new ProcessContext(
                processId,
                steps.GroupBy(x => x.ProcessStepTypeId, x => x.ProcessStepId).ToImmutableSortedDictionary<IGrouping<int, Guid>, int, IEnumerable<Guid>>(x => x.Key, guids => guids.AsEnumerable()),
                new ProcessStepTypeSet(allSteps.Keys.Where<int>(x => executableProcessStepTypeIds.Contains(x))),
                executor);

            await foreach (var p in ExecuteProcessForType(executor, context, cancellationToken))
                yield return p;
        }
    }

    private async IAsyncEnumerable<IProcessExecutor.ProcessExecutionResult> ExecuteProcessForType(
        IProcessTypeExecutor<TProcessTypeId> executor,
        ProcessContext context,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var (modified, initialStepTypeIds) = await executor.InitializeProcess(context.ProcessId, context.AllSteps.Keys);

        modified |= ScheduleProcessStepTypeIds(initialStepTypeIds, context);

        yield return modified
            ? IProcessExecutor.ProcessExecutionResult.SaveRequested
            : IProcessExecutor.ProcessExecutionResult.Unmodified;

        while (context.ExecutableStepTypeIds.TryGetNext(out var stepTypeId))
        {
            var isLockRequested = await executor.IsLockRequested(stepTypeId).ConfigureAwait(false);
            if (isLockRequested)
            {
                yield return IProcessExecutor.ProcessExecutionResult.LockRequested;
            }

            ProcessStepStatusId resultStepStatusId;
            IEnumerable<int>? scheduleStepTypeIds;
            IEnumerable<int>? skipStepTypeIds;
            string? processMessage;
            bool success;
            try
            {
                (modified, resultStepStatusId, scheduleStepTypeIds, skipStepTypeIds, processMessage) = await executor.ExecuteProcessStep(stepTypeId, context.AllSteps.Keys, cancellationToken).ConfigureAwait(false);
                success = true;
            }
            catch (Exception e) when (e is not SystemException)
            {
                resultStepStatusId = ProcessStepStatusId.FAILED;
                processMessage = $"{e.GetType()}: {e.Message}";
                scheduleStepTypeIds = null;
                skipStepTypeIds = null;
                modified = false;
                success = false;
            }

            if (!success)
            {
                yield return IProcessExecutor.ProcessExecutionResult.Unmodified;
            }

            modified |= SetProcessStepStatus(stepTypeId, resultStepStatusId, context, processMessage);
            modified |= SkipProcessStepTypeIds(skipStepTypeIds, context);
            modified |= ScheduleProcessStepTypeIds(scheduleStepTypeIds, context);

            yield return modified
                ? IProcessExecutor.ProcessExecutionResult.SaveRequested
                : IProcessExecutor.ProcessExecutionResult.Unmodified;
        }
    }

    private bool ScheduleProcessStepTypeIds(IEnumerable<int>? scheduleStepTypeIds, ProcessContext context)
    {
        if (scheduleStepTypeIds == null || !scheduleStepTypeIds.Any())
        {
            return false;
        }

        var newStepTypeIds = scheduleStepTypeIds.Except(context.AllSteps.Keys).ToList();
        if (newStepTypeIds.Count == 0)
        {
            return false;
        }

        foreach (var newStep in _processStepRepository.CreateProcessStepRange(newStepTypeIds.Select(stepTypeId => (context.Executor.GetProcessTypeId(), stepTypeId, ProcessStepStatusId.TODO, context.ProcessId))))
        {
            var processStepTypeId = newStep.ProcessStepTypeId;
            context.AllSteps.Add(processStepTypeId, [newStep.Id]);
            if (context.Executor.IsExecutableStepTypeId(processStepTypeId))
            {
                context.ExecutableStepTypeIds.Add(processStepTypeId);
            }
        }

        return true;
    }

    private bool SkipProcessStepTypeIds(IEnumerable<int>? skipStepTypeIds, ProcessContext context)
    {
        if (skipStepTypeIds == null || !skipStepTypeIds.Any())
        {
            return false;
        }

        var modified = false;
        foreach (var skipStepTypeId in skipStepTypeIds)
        {
            var skippedStep = SetProcessStepStatus(skipStepTypeId, ProcessStepStatusId.SKIPPED, context, null);
            if (skippedStep)
            {
                _logger.LogInformation("Skipped step {SkipStepTypeId} for process {ProcessId}", skipStepTypeId, context.ProcessId);
            }

            modified |= skippedStep;
        }

        return modified;
    }

    private bool SetProcessStepStatus(int stepTypeId, ProcessStepStatusId stepStatusId, ProcessContext context, string? processMessage)
    {
        if ((stepStatusId == ProcessStepStatusId.TODO && processMessage == null) || !context.AllSteps.Remove(stepTypeId, out var stepIds))
        {
            return false;
        }

        var isFirst = true;
        foreach (var stepId in stepIds)
        {
            _processStepRepository.AttachAndModifyProcessStep(stepId, null, step =>
            {
                step.ProcessStepStatusId = isFirst ? stepStatusId : ProcessStepStatusId.DUPLICATE;
                step.Message = processMessage;
            });
            isFirst = false;
        }

        if (context.Executor.IsExecutableStepTypeId(stepTypeId))
        {
            context.ExecutableStepTypeIds.Remove(stepTypeId);
        }

        return true;
    }

    public sealed record ProcessContext(
        Guid ProcessId,
        IDictionary<int, IEnumerable<Guid>> AllSteps,
        ProcessStepTypeSet ExecutableStepTypeIds,
        IProcessTypeExecutor<TProcessTypeId> Executor
    );

    public sealed class ProcessStepTypeSet(IEnumerable<int> items)
    {
        private readonly HashSet<int> _items = [.. items];

        public bool TryGetNext(out int item)
        {
            using var enumerator = _items.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                item = 0;
                return false;
            }

            item = enumerator.Current;
            _items.Remove(item);
            return true;
        }

        public void Add(int item) => _items.Add(item);

        public void Remove(int item) => _items.Remove(item);
    }
}
