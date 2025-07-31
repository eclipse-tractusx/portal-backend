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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Next.Entities;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Next.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Next.Models;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Next.DBAccess;

public class ProcessStepRepository<TProcess, TProcessType, TProcessStep, TProcessStepType>(IProcessRepositoryContextAccess<TProcess, TProcessType, TProcessStep, TProcessStepType> dbContext) :
    IProcessStepRepository
    where TProcess : class, IProcess, IProcessNavigation<TProcessStep>
    where TProcessType : class, IProcessType, IProcessTypeNavigation<TProcessStep>
    where TProcessStep : class, IProcessStep, IProcessStepNavigation<TProcess, TProcessType, TProcessStepType>
    where TProcessStepType : class, IProcessStepType, IProcessStepTypeNavigation<TProcessStep, TProcessType>
{
    public IProcess CreateProcess() =>
        dbContext.Processes.Add(dbContext.CreateProcess(Guid.NewGuid(), Guid.NewGuid())).Entity;

    public IProcessStep CreateProcessStep<TProcessTypeId, TProcessStepTypeId>(TProcessTypeId processTypeId, TProcessStepTypeId processStepTypeId, Enums.ProcessStepStatusId processStepStatusId, Guid processId)
        where TProcessTypeId : struct, IConvertible
        where TProcessStepTypeId : struct, IConvertible =>
        dbContext.ProcessSteps.Add(dbContext.CreateProcessStep(Guid.NewGuid(), Convert.ToInt32(processTypeId), Convert.ToInt32(processStepTypeId), processStepStatusId, processId, DateTimeOffset.UtcNow)).Entity;

    public IEnumerable<IProcessStep> CreateProcessStepRange<TProcessTypeId, TProcessStepTypeId>(IEnumerable<(TProcessTypeId ProcessTypeId, TProcessStepTypeId ProcessStepTypeId, Enums.ProcessStepStatusId ProcessStepStatusId, Guid ProcessId)> processStepTypeStatus)
        where TProcessTypeId : struct, IConvertible
        where TProcessStepTypeId : struct, IConvertible
    {
        var processSteps = processStepTypeStatus.Select(x => dbContext.CreateProcessStep(Guid.NewGuid(), Convert.ToInt32(x.ProcessTypeId), Convert.ToInt32(x.ProcessStepTypeId), x.ProcessStepStatusId, x.ProcessId, DateTimeOffset.UtcNow)).ToArray();
        dbContext.ProcessSteps.AddRange(processSteps);
        return processSteps;
    }

    public void AttachAndModifyProcessStep(Guid processStepId, Action<IProcessStep>? initialize, Action<IProcessStep> modify)
    {
        var step = dbContext.CreateProcessStep(processStepId, 0, 0, default, Guid.Empty, default);
        initialize?.Invoke(step);
        dbContext.ProcessSteps.Attach(step);
        step.DateLastChanged = DateTimeOffset.UtcNow;
        modify(step);
    }

    public void AttachAndModifyProcessSteps(IEnumerable<(Guid ProcessStepId, Action<IProcessStep>? Initialize, Action<IProcessStep> Modify)> processStepIdsInitializeModifyData)
    {
        var stepModifyData = processStepIdsInitializeModifyData.Select(data =>
            {
                var step = dbContext.CreateProcessStep(data.ProcessStepId, 0, 0, default, Guid.Empty, default);
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

    public IAsyncEnumerable<IProcess> GetActiveProcesses(DateTimeOffset lockExpiryDate) =>
        dbContext.Processes
            .AsNoTracking()
            .Where(process =>
                process.ProcessSteps.Any(step => step.ProcessStepType!.IsExecutable && step.ProcessStepStatusId == ProcessStepStatusId.TODO) &&
                (process.LockExpiryDate == null || process.LockExpiryDate < lockExpiryDate))
            .AsAsyncEnumerable();

    public IAsyncEnumerable<(int ProcessTypeId, Guid ProcessStepId, int ProcessStepTypeId)> GetProcessStepData(Guid processId) =>
        dbContext.ProcessSteps
            .AsNoTracking()
            .Where(step =>
                step.ProcessId == processId &&
                step.ProcessStepStatusId == ProcessStepStatusId.TODO)
            .OrderBy(step => step.ProcessStepTypeId)
            .Select(step =>
                new ValueTuple<int, Guid, int>(
                    step.ProcessTypeId,
                    step.Id,
                    step.ProcessStepTypeId))
            .AsAsyncEnumerable();

    public Task<(bool ProcessExists, VerifyProcessData ProcessData)> IsValidProcess(Guid processId, int processTypeId, IEnumerable<int> processStepTypeIds) =>
        dbContext.Processes
            .AsNoTracking()
            .Where(x => x.Id == processId && x.ProcessSteps.Any(ps => ps.ProcessTypeId.Equals(processTypeId)))
            .Select(x => new ValueTuple<bool, VerifyProcessData>(
                true,
                new VerifyProcessData(
                    x,
                    x.ProcessSteps
                        .Where(step =>
                            step.ProcessTypeId.Equals(processTypeId) &&
                            processStepTypeIds.Contains(step.ProcessStepTypeId) &&
                            step.ProcessStepStatusId == ProcessStepStatusId.TODO))
            ))
            .SingleOrDefaultAsync();
}
