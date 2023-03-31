/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

public class ProcessStepRepository : IProcessStepRepository
{
    private readonly PortalDbContext _context;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="portalDbContext">PortalDb context.</param>
    public ProcessStepRepository(PortalDbContext portalDbContext)
    {
        _context = portalDbContext;
    }

    public Process CreateProcess(ProcessTypeId processTypeId) =>
        _context.Add(new Process(Guid.NewGuid(), processTypeId, Guid.NewGuid())).Entity;

    public IEnumerable<ProcessStep> CreateProcessStepRange(IEnumerable<(ProcessStepTypeId ProcessStepTypeId, ProcessStepStatusId ProcessStepStatusId, Guid ProcessId)> processStepTypeStatus)
    {
        var processSteps = processStepTypeStatus.Select(x => new ProcessStep(Guid.NewGuid(), x.ProcessStepTypeId, x.ProcessStepStatusId, x.ProcessId, DateTimeOffset.UtcNow)).ToList();
        _context.AddRange(processSteps);
        return processSteps;
    }

    public void AttachAndModifyProcessStep(Guid processStepId, Action<ProcessStep>? initialize, Action<ProcessStep> modify)
    {
        var step = new ProcessStep(processStepId, default, default, Guid.Empty, default);
        initialize?.Invoke(step);
        _context.Attach(step);
        step.DateLastChanged = DateTimeOffset.UtcNow;
        modify(step);
    }

    public IAsyncEnumerable<Process> GetActiveProcesses(IEnumerable<ProcessTypeId> processTypeIds, IEnumerable<ProcessStepTypeId> processStepTypeIds, DateTimeOffset lockExpiryDate) =>
        _context.Processes
            .AsNoTracking()
            .Where(process =>
                processTypeIds.Contains(process.ProcessTypeId) &&
                process.ProcessSteps.Any(step => processStepTypeIds.Contains(step.ProcessStepTypeId) && step.ProcessStepStatusId == ProcessStepStatusId.TODO) &&
                (process.LockExpiryDate == null || process.LockExpiryDate < lockExpiryDate))
            .AsAsyncEnumerable();

    public IAsyncEnumerable<(Guid ProcessStepId, ProcessStepTypeId ProcessStepTypeId)> GetProcessStepData(Guid processId) =>
        _context.ProcessSteps
            .AsNoTracking()
            .Where(step =>
                step.ProcessId == processId &&
                step.ProcessStepStatusId == ProcessStepStatusId.TODO)
            .OrderBy(step => step.ProcessStepTypeId)
            .Select(step =>
                new ValueTuple<Guid, ProcessStepTypeId>(
                    step.Id,
                    step.ProcessStepTypeId))
            .AsAsyncEnumerable();

    /// <inheritdoc />
    public Process AttachAndModifyProcess(Process process, Action<Process> setOptionalField)
    {
        _context.Processes.Attach(process);
        setOptionalField.Invoke(process);
        return process;
    }
}
