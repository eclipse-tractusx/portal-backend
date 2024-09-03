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

using Microsoft.EntityFrameworkCore;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Entities;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Models;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Context;

public class ProcessStepRepository<TProcessTypeId, TProcessStepTypeId>(ProcessDbContext<TProcessTypeId, TProcessStepTypeId> dbContext) : IProcessStepRepository<TProcessTypeId, TProcessStepTypeId>
    where TProcessTypeId : struct, IConvertible
    where TProcessStepTypeId : struct, IConvertible
{
    public Process<TProcessTypeId, TProcessStepTypeId> CreateProcess(TProcessTypeId processTypeId) =>
        dbContext.Add(new Process<TProcessTypeId, TProcessStepTypeId>(Guid.NewGuid(), processTypeId, Guid.NewGuid())).Entity;

    public IEnumerable<Process<TProcessTypeId, TProcessStepTypeId>> CreateProcessRange(IEnumerable<TProcessTypeId> processTypeIds)
    {
        var processes = processTypeIds.Select(x => new Process<TProcessTypeId, TProcessStepTypeId>(Guid.NewGuid(), x, Guid.NewGuid())).ToImmutableList();
        dbContext.AddRange(processes);
        return processes;
    }

    public ProcessStep<TProcessTypeId, TProcessStepTypeId> CreateProcessStep(TProcessStepTypeId processStepTypeId, ProcessStepStatusId processStepStatusId, Guid processId) =>
        dbContext.Add(new ProcessStep<TProcessTypeId, TProcessStepTypeId>(Guid.NewGuid(), processStepTypeId, processStepStatusId, processId, DateTimeOffset.UtcNow)).Entity;

    public IEnumerable<ProcessStep<TProcessTypeId, TProcessStepTypeId>> CreateProcessStepRange(IEnumerable<(TProcessStepTypeId ProcessStepTypeId, ProcessStepStatusId ProcessStepStatusId, Guid ProcessId)> processStepTypeStatus)
    {
        var processSteps = processStepTypeStatus.Select(x => new ProcessStep<TProcessTypeId, TProcessStepTypeId>(Guid.NewGuid(), x.ProcessStepTypeId, x.ProcessStepStatusId, x.ProcessId, DateTimeOffset.UtcNow)).ToImmutableList();
        dbContext.AddRange(processSteps);
        return processSteps;
    }

    public void AttachAndModifyProcessStep(Guid processStepId, Action<ProcessStep<TProcessTypeId, TProcessStepTypeId>>? initialize, Action<ProcessStep<TProcessTypeId, TProcessStepTypeId>> modify)
    {
        var step = new ProcessStep<TProcessTypeId, TProcessStepTypeId>(processStepId, default, default, Guid.Empty, default);
        initialize?.Invoke(step);
        dbContext.Attach(step);
        step.DateLastChanged = DateTimeOffset.UtcNow;
        modify(step);
    }

    public void AttachAndModifyProcessSteps(IEnumerable<(Guid ProcessStepId, Action<ProcessStep<TProcessTypeId, TProcessStepTypeId>>? Initialize, Action<ProcessStep<TProcessTypeId, TProcessStepTypeId>> Modify)> processStepIdsInitializeModifyData)
    {
        var stepModifyData = processStepIdsInitializeModifyData.Select(data =>
            {
                var step = new ProcessStep<TProcessTypeId, TProcessStepTypeId>(data.ProcessStepId, default, default, Guid.Empty, default);
                data.Initialize?.Invoke(step);
                return (Step: step, data.Modify);
            }).ToImmutableList();
        dbContext.AttachRange(stepModifyData.Select(data => data.Step));
        stepModifyData.ForEach(data =>
            {
                data.Step.DateLastChanged = DateTimeOffset.UtcNow;
                data.Modify(data.Step);
            });
    }

    public IAsyncEnumerable<Process<TProcessTypeId, TProcessStepTypeId>> GetActiveProcesses(IEnumerable<TProcessTypeId> processTypeIds, IEnumerable<TProcessStepTypeId> processStepTypeIds, DateTimeOffset lockExpiryDate) =>
        dbContext.Processes
            .AsNoTracking()
            .Where(process =>
                processTypeIds.Contains(process.ProcessTypeId) &&
                process.ProcessSteps.Any(step => processStepTypeIds.Contains(step.ProcessStepTypeId) && step.ProcessStepStatusId == ProcessStepStatusId.TODO) &&
                (process.LockExpiryDate == null || process.LockExpiryDate < lockExpiryDate))
            .AsAsyncEnumerable();

    public IAsyncEnumerable<(Guid ProcessStepId, TProcessStepTypeId ProcessStepTypeId)> GetProcessStepData(Guid processId) =>
        dbContext.ProcessSteps
            .AsNoTracking()
            .Where(step =>
                step.ProcessId == processId &&
                step.ProcessStepStatusId == ProcessStepStatusId.TODO)
            .OrderBy(step => step.ProcessStepTypeId)
            .Select(step =>
                new ValueTuple<Guid, TProcessStepTypeId>(
                    step.Id,
                    step.ProcessStepTypeId))
            .AsAsyncEnumerable();

    public Task<(bool ProcessExists, VerifyProcessData<TProcessTypeId, TProcessStepTypeId> ProcessData)> IsValidProcess(Guid processId, TProcessTypeId processTypeId, IEnumerable<TProcessStepTypeId> processStepTypeIds) =>
        dbContext.Processes
            .AsNoTracking()
            .Where(x => x.Id == processId && x.ProcessTypeId.Equals(processTypeId))
            .Select(x => new ValueTuple<bool, VerifyProcessData<TProcessTypeId, TProcessStepTypeId>>(
                true,
                new VerifyProcessData<TProcessTypeId, TProcessStepTypeId>(
                    x,
                    x.ProcessSteps
                        .Where(step =>
                            processStepTypeIds.Contains(step.ProcessStepTypeId) &&
                            step.ProcessStepStatusId == ProcessStepStatusId.TODO))
            ))
            .SingleOrDefaultAsync();
}
