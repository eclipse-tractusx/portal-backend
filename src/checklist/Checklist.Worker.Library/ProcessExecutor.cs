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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Async;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Runtime.CompilerServices;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Checklist.Worker.Library;

public class ProcessExecutor : IProcessExecutor
{
    private readonly ImmutableDictionary<ProcessTypeId,IProcessTypeExecutor> _executors; 
    private readonly IProcessStepRepository _processStepRepository;

    public ProcessExecutor(IEnumerable<IProcessTypeExecutor> executors, IPortalRepositories portalRepositories)
    {
        _processStepRepository = portalRepositories.GetInstance<IProcessStepRepository>();
        _executors = executors.ToImmutableDictionary(executor => executor.GetProcessTypeId());
    }

    public IEnumerable<ProcessTypeId> GetRegisteredProcessTypeIds() => _executors.Keys;
    public IEnumerable<ProcessStepTypeId> GetExecutableStepTypeIds() => _executors.Values.SelectMany(executor => executor.GetExecutableStepTypeIds());

    public async IAsyncEnumerable<bool> ExecuteProcess(Guid processId, ProcessTypeId processTypeId, [EnumeratorCancellation] CancellationToken cancellationToken)
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

        yield return modified;

        while (context.ExecutableStepTypeIds.TryGetNext(out var stepTypeId))
        {
            ProcessStepStatusId resultStepStatusId;
            IEnumerable<ProcessStepTypeId>? scheduleStepTypeIds;
            IEnumerable<ProcessStepTypeId>? skipStepTypeIds;
            try
            {
                (modified, resultStepStatusId, scheduleStepTypeIds, skipStepTypeIds) = await executor.ExecuteProcessStep(stepTypeId, context.AllSteps.Keys, cancellationToken).ConfigureAwait(false);
            }
            catch(Exception e) when (e is not SystemException)
            {
                resultStepStatusId = ProcessStepStatusId.FAILED;
                scheduleStepTypeIds = null;
                skipStepTypeIds = null;
                modified = false;
            }
            modified |= SetProcessStepStatus(stepTypeId, resultStepStatusId, context);
            modified |= SkipProcessStepTypeIds(skipStepTypeIds, context);
            modified |= ScheduleProcessStepTypeIds(scheduleStepTypeIds, context);
            yield return modified;
        }
    }

    private bool ScheduleProcessStepTypeIds(IEnumerable<ProcessStepTypeId>? scheduleStepTypeIds, ProcessContext context)
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
        foreach (var newStep in _processStepRepository.CreateProcessStepRange(context.ProcessId, newStepTypeIds))
        {
            context.AllSteps.Add(newStep.ProcessStepTypeId, new [] { newStep.Id });
            if (context.Executor.IsExecutableStepTypeId(newStep.ProcessStepTypeId))
            {
                context.ExecutableStepTypeIds.Add(newStep.ProcessStepTypeId);
            }
        }
        return true;
    }

    private bool SkipProcessStepTypeIds(IEnumerable<ProcessStepTypeId>? skipStepTypeIds, ProcessContext context)
    {
        if (skipStepTypeIds == null || !skipStepTypeIds.Any())
        {
            return false;
        }
        var modified = false;
        foreach (var skipStepTypeId in skipStepTypeIds)
        {
            modified |= SetProcessStepStatus(skipStepTypeId, ProcessStepStatusId.SKIPPED, context);
        }
        return modified;
    }

    private bool SetProcessStepStatus(ProcessStepTypeId stepTypeId, ProcessStepStatusId stepStatusId, ProcessContext context)
    {
        if (stepStatusId == ProcessStepStatusId.TODO || !context.AllSteps.Remove(stepTypeId, out var stepIds))
        {
            return false;
        }
        bool isFirst = true;
        foreach (var stepId in stepIds)
        {
            _processStepRepository.AttachAndModifyProcessStep(stepId, null, step => { step.ProcessStepStatusId = isFirst ? stepStatusId : ProcessStepStatusId.DUPLICATE; });
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
        IDictionary<ProcessStepTypeId,IEnumerable<Guid>> AllSteps,
        ProcessStepTypeSet ExecutableStepTypeIds,
        IProcessTypeExecutor Executor
    );

    private sealed class ProcessStepTypeSet
    {
        private readonly HashSet<ProcessStepTypeId> _items;

        public ProcessStepTypeSet(IEnumerable<ProcessStepTypeId> items){
            _items = new HashSet<ProcessStepTypeId>(items);
        }

        public bool TryGetNext(out ProcessStepTypeId item)
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

        public void Add(ProcessStepTypeId item) => _items.Add(item);

        public void Remove(ProcessStepTypeId item) => _items.Remove(item);
    }
}
