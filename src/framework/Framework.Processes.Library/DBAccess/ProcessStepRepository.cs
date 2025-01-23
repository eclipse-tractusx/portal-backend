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

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.DBAccess;

public class ProcessStepRepository<TProcess, TProcessType, TProcessStep, TProcessStepType, TProcessTypeId, TProcessStepTypeId>(IProcessRepositoryContextAccess<TProcess, TProcessType, TProcessStep, TProcessStepType, TProcessTypeId, TProcessStepTypeId> dbContext) :
    IProcessStepRepository<TProcessTypeId, TProcessStepTypeId>
    where TProcess : class, IProcess<TProcessTypeId>, IProcessNavigation<TProcessType, TProcessStep, TProcessTypeId, TProcessStepTypeId>
    where TProcessType : class, IProcessType<TProcessTypeId>, IProcessTypeNavigation<TProcess, TProcessTypeId>
    where TProcessStep : class, IProcessStep<TProcessStepTypeId>, IProcessStepNavigation<TProcess, TProcessStepType, TProcessTypeId, TProcessStepTypeId>
    where TProcessStepType : class, IProcessStepType<TProcessStepTypeId>, IProcessStepTypeNavigation<TProcessStep, TProcessStepTypeId>
    where TProcessTypeId : struct, IConvertible
    where TProcessStepTypeId : struct, IConvertible
{
    public IProcess<TProcessTypeId> CreateProcess(TProcessTypeId processTypeId) =>
        dbContext.Processes.Add(dbContext.CreateProcess(Guid.NewGuid(), processTypeId, Guid.NewGuid())).Entity;

    public IEnumerable<IProcess<TProcessTypeId>> CreateProcessRange(IEnumerable<TProcessTypeId> processTypeIds)
    {
        var processes = processTypeIds.Select(x => dbContext.CreateProcess(Guid.NewGuid(), x, Guid.NewGuid())).ToImmutableList();
        dbContext.Processes.AddRange(processes);
        return processes;
    }

    public IProcessStep<TProcessStepTypeId> CreateProcessStep(TProcessStepTypeId processStepTypeId, ProcessStepStatusId processStepStatusId, Guid processId) =>
        dbContext.ProcessSteps.Add(dbContext.CreateProcessStep(Guid.NewGuid(), processStepTypeId, processStepStatusId, processId, DateTimeOffset.UtcNow)).Entity;

    public IEnumerable<IProcessStep<TProcessStepTypeId>> CreateProcessStepRange(IEnumerable<(TProcessStepTypeId ProcessStepTypeId, ProcessStepStatusId ProcessStepStatusId, Guid ProcessId)> processStepTypeStatus)
    {
        var processSteps = processStepTypeStatus.Select(x => dbContext.CreateProcessStep(Guid.NewGuid(), x.ProcessStepTypeId, x.ProcessStepStatusId, x.ProcessId, DateTimeOffset.UtcNow)).ToImmutableList();
        dbContext.ProcessSteps.AddRange(processSteps);
        return processSteps;
    }

    public void AttachAndModifyProcessStep(Guid processStepId, Action<IProcessStep<TProcessStepTypeId>>? initialize, Action<IProcessStep<TProcessStepTypeId>> modify)
    {
        var step = dbContext.CreateProcessStep(processStepId, default, default, Guid.Empty, default);
        initialize?.Invoke(step);
        dbContext.ProcessSteps.Attach(step);
        step.DateLastChanged = DateTimeOffset.UtcNow;
        modify(step);
    }

    public void AttachAndModifyProcessSteps(IEnumerable<(Guid ProcessStepId, Action<IProcessStep<TProcessStepTypeId>>? Initialize, Action<IProcessStep<TProcessStepTypeId>> Modify)> processStepIdsInitializeModifyData)
    {
        var stepModifyData = processStepIdsInitializeModifyData.Select(data =>
            {
                var step = dbContext.CreateProcessStep(data.ProcessStepId, default, default, Guid.Empty, default);
                data.Initialize?.Invoke(step);
                return (Step: step, data.Modify);
            }).ToImmutableList();
        dbContext.ProcessSteps.AttachRange(stepModifyData.Select(data => data.Step));
        stepModifyData.ForEach(data =>
            {
                data.Step.DateLastChanged = DateTimeOffset.UtcNow;
                data.Modify(data.Step);
            });
    }

    public IAsyncEnumerable<IProcess<TProcessTypeId>> GetActiveProcesses(IEnumerable<TProcessTypeId> processTypeIds, IEnumerable<TProcessStepTypeId> processStepTypeIds, DateTimeOffset lockExpiryDate) =>
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
