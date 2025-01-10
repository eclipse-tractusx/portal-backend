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

using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Entities;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Context;

/// <summary>
/// Repository for accessing and creating processSteps on persistence layer.
/// </summary>
public interface IProcessStepRepository<TProcessType, TProcessStepType, TProcessTypeId, TProcessStepTypeId>
    where TProcessType : class, IProcess<TProcessTypeId>
    where TProcessStepType : class, IProcessStep<TProcessStepTypeId>
    where TProcessTypeId : struct, IConvertible
    where TProcessStepTypeId : struct, IConvertible
{
    TProcessType CreateProcess(TProcessTypeId processTypeId);
    IEnumerable<TProcessType> CreateProcessRange(IEnumerable<TProcessTypeId> processTypeIds);
    TProcessStepType CreateProcessStep(TProcessStepTypeId processStepTypeId, ProcessStepStatusId processStepStatusId, Guid processId);
    IEnumerable<TProcessStepType> CreateProcessStepRange(IEnumerable<(TProcessStepTypeId ProcessStepTypeId, ProcessStepStatusId ProcessStepStatusId, Guid ProcessId)> processStepTypeStatus);
    void AttachAndModifyProcessStep(Guid processStepId, Action<TProcessStepType>? initialize, Action<TProcessStepType> modify);
    void AttachAndModifyProcessSteps(IEnumerable<(Guid ProcessStepId, Action<TProcessStepType>? Initialize, Action<TProcessStepType> Modify)> processStepIdsInitializeModifyData);
    IAsyncEnumerable<TProcessType> GetActiveProcesses(IEnumerable<TProcessTypeId> processTypeIds, IEnumerable<TProcessStepTypeId> processStepTypeIds, DateTimeOffset lockExpiryDate);
    IAsyncEnumerable<(Guid ProcessStepId, TProcessStepTypeId ProcessStepTypeId)> GetProcessStepData(Guid processId);
    public Task<(bool ProcessExists, IVerifyProcessData<TProcessType, TProcessStepType, TProcessTypeId, TProcessStepTypeId> ProcessData)> IsValidProcess(Guid processId, TProcessTypeId processTypeId, IEnumerable<TProcessStepTypeId> processStepTypeIds);
}
