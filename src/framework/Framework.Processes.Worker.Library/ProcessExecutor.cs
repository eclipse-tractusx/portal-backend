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
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Context;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Worker.Library;

public class ProcessExecutor<TProcessTypeId, TProcessStepTypeId> : IProcessExecutor<TProcessTypeId, TProcessStepTypeId>
    where TProcessTypeId : struct, IConvertible
    where TProcessStepTypeId : struct, IConvertible
{
    private readonly ImmutableDictionary<TProcessTypeId, IProcessTypeExecutor<TProcessTypeId, TProcessStepTypeId>> _executors;
    private readonly IProcessStepRepository<TProcessTypeId, TProcessStepTypeId> _processStepRepository;
    private readonly ILogger<ProcessExecutor<TProcessTypeId, TProcessStepTypeId>> _logger;

    public ProcessExecutor(IEnumerable<IProcessTypeExecutor<TProcessTypeId, TProcessStepTypeId>> executors, IProcessRepositories processRepositories, ILogger<ProcessExecutor<TProcessTypeId, TProcessStepTypeId>> logger)
    {
        _processStepRepository = processRepositories.GetInstance<IProcessStepRepository<TProcessTypeId, TProcessStepTypeId>>();
        _executors = executors.ToImmutableDictionary(executor => executor.GetProcessTypeId());
        _logger = logger;
    }

    public IEnumerable<TProcessTypeId> GetRegisteredProcessTypeIds() => _executors.Keys;
    public IEnumerable<TProcessStepTypeId> GetExecutableStepTypeIds() => _executors.Values.SelectMany(executor => executor.GetExecutableStepTypeIds());

    public async IAsyncEnumerable<IProcessExecutor<TProcessTypeId, TProcessStepTypeId>.ProcessExecutionResult> ExecuteProcess(Guid processId, TProcessTypeId processTypeId, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (!_executors.TryGetValue(processTypeId, out var executor))
        {
            throw new UnexpectedConditionException($"processType {processTypeId} is not a registered executable processType.");
        }

        var allSteps = await _processStepRepository
            .GetProcessStepData(processId)
            .PreSortedGroupBy(x => x.ProcessStepTypeId, x => x.ProcessStepId)
            .ToDictionaryAsync(g => g.Key, g => g.AsEnumerable(), cancellationToken)
            .ConfigureAwait(false);

        var context = new ProcessContext(
            processId,
            allSteps,
            new ProcessStepTypeSet(allSteps.Keys.Where(x => executor.IsExecutableStepTypeId(x))),
            executor);

        var (modified, initialStepTypeIds) = await executor.InitializeProcess(processId, context.AllSteps.Keys).ConfigureAwait(false);

        modified |= ScheduleProcessStepTypeIds(initialStepTypeIds, context);

        yield return modified
            ? IProcessExecutor<TProcessTypeId, TProcessStepTypeId>.ProcessExecutionResult.SaveRequested
            : IProcessExecutor<TProcessTypeId, TProcessStepTypeId>.ProcessExecutionResult.Unmodified;

        while (context.ExecutableStepTypeIds.TryGetNext(out var stepTypeId))
        {
            if (await executor.IsLockRequested(stepTypeId).ConfigureAwait(false))
            {
                yield return IProcessExecutor<TProcessTypeId, TProcessStepTypeId>.ProcessExecutionResult.LockRequested;
            }

            ProcessStepStatusId resultStepStatusId;
            IEnumerable<TProcessStepTypeId>? scheduleStepTypeIds;
            IEnumerable<TProcessStepTypeId>? skipStepTypeIds;
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
                yield return IProcessExecutor<TProcessTypeId, TProcessStepTypeId>.ProcessExecutionResult.Unmodified;
            }

            modified |= SetProcessStepStatus(stepTypeId, resultStepStatusId, context, processMessage);
            modified |= SkipProcessStepTypeIds(skipStepTypeIds, context);
            modified |= ScheduleProcessStepTypeIds(scheduleStepTypeIds, context);

            yield return modified
                ? IProcessExecutor<TProcessTypeId, TProcessStepTypeId>.ProcessExecutionResult.SaveRequested
                : IProcessExecutor<TProcessTypeId, TProcessStepTypeId>.ProcessExecutionResult.Unmodified;
        }
    }

    private bool ScheduleProcessStepTypeIds(IEnumerable<TProcessStepTypeId>? scheduleStepTypeIds, ProcessContext context)
    {
        if (scheduleStepTypeIds == null || !scheduleStepTypeIds.Any())
        {
            return false;
        }

        var newStepTypeIds = scheduleStepTypeIds.Except(context.AllSteps.Keys).ToList();
        if (!newStepTypeIds.Any())
        {
            return false;
        }

        foreach (var newStep in _processStepRepository.CreateProcessStepRange(newStepTypeIds.Select(stepTypeId => (stepTypeId, ProcessStepStatusId.TODO, context.ProcessId))))
        {
            var processStepTypeId = newStep.ProcessStepTypeId;
            context.AllSteps.Add(processStepTypeId, new[] { newStep.Id });
            if (context.Executor.IsExecutableStepTypeId(processStepTypeId))
            {
                context.ExecutableStepTypeIds.Add(processStepTypeId);
            }
        }

        return true;
    }

    private bool SkipProcessStepTypeIds(IEnumerable<TProcessStepTypeId>? skipStepTypeIds, ProcessContext context)
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

    private bool SetProcessStepStatus(TProcessStepTypeId stepTypeId, ProcessStepStatusId stepStatusId, ProcessContext context, string? processMessage)
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

    private sealed record ProcessContext(
        Guid ProcessId,
        IDictionary<TProcessStepTypeId, IEnumerable<Guid>> AllSteps,
        ProcessStepTypeSet ExecutableStepTypeIds,
        IProcessTypeExecutor<TProcessTypeId, TProcessStepTypeId> Executor
    );

    private sealed class ProcessStepTypeSet(IEnumerable<TProcessStepTypeId> items)
    {
        private readonly HashSet<TProcessStepTypeId> _items = [.. items];

        public bool TryGetNext(out TProcessStepTypeId item)
        {
            using var enumerator = _items.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                item = default;
                return false;
            }
            item = enumerator.Current;
            _items.Remove(item);
            return true;
        }

        public void Add(TProcessStepTypeId item) => _items.Add(item);

        public void Remove(TProcessStepTypeId item) => _items.Remove(item);
    }
}
