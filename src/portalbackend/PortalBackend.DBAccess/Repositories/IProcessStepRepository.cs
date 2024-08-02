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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

/// <summary>
/// Repository for accessing and creating processSteps on persistence layer.
/// </summary>
public interface IProcessStepRepository
{
    Process CreateProcess(ProcessTypeId processTypeId);
    IEnumerable<Process> CreateProcessRange(IEnumerable<ProcessTypeId> processTypeIds);
    ProcessStep CreateProcessStep(ProcessStepTypeId processStepTypeId, ProcessStepStatusId processStepStatusId, Guid processId);
    IEnumerable<ProcessStep> CreateProcessStepRange(IEnumerable<(ProcessStepTypeId ProcessStepTypeId, ProcessStepStatusId ProcessStepStatusId, Guid ProcessId)> processStepTypeStatus);
    void AttachAndModifyProcessStep(Guid processStepId, Action<ProcessStep>? initialize, Action<ProcessStep> modify);
    void AttachAndModifyProcessSteps(IEnumerable<(Guid ProcessStepId, Action<ProcessStep>? Initialize, Action<ProcessStep> Modify)> processStepIdsInitializeModifyData);
    IAsyncEnumerable<Process> GetActiveProcesses(IEnumerable<ProcessTypeId> processTypeIds, IEnumerable<ProcessStepTypeId> processStepTypeIds, DateTimeOffset lockExpiryDate);
    IAsyncEnumerable<(Guid ProcessStepId, ProcessStepTypeId ProcessStepTypeId)> GetProcessStepData(Guid processId);
    public Task<(bool ProcessExists, VerifyProcessData ProcessData)> IsValidProcess(Guid processId, ProcessTypeId processTypeId, IEnumerable<ProcessStepTypeId> processStepTypeIds);
    Task<(ProcessTypeId ProcessTypeId, VerifyProcessData ProcessData, Guid? ServiceAccountId)> GetProcessDataForServiceAccountCallback(Guid processId, IEnumerable<ProcessStepTypeId> processStepTypeIds);
    Task<(ProcessTypeId ProcessTypeId, VerifyProcessData ProcessData, Guid? ServiceAccountId)> GetProcessDataForServiceAccountDeletionCallback(Guid processId, IEnumerable<ProcessStepTypeId> processStepTypeIds);
}
