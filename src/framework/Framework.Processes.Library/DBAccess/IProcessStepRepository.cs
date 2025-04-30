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

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.DBAccess;

/// <summary>
/// Repository for accessing and creating processSteps on persistence layer.
/// </summary>
public interface IProcessStepRepository
{
    IProcess CreateProcess();

    IProcessStep CreateProcessStep<TProcessTypeId, TProcessStepTypeId>(TProcessTypeId processTypeId, TProcessStepTypeId processStepTypeId, ProcessStepStatusId processStepStatusId, Guid processId)
        where TProcessTypeId : struct, IConvertible
        where TProcessStepTypeId : struct, IConvertible;

    IEnumerable<IProcessStep> CreateProcessStepRange<TProcessTypeId, TProcessStepTypeId>(IEnumerable<(TProcessTypeId ProcessTypeId, TProcessStepTypeId ProcessStepTypeId, ProcessStepStatusId ProcessStepStatusId, Guid ProcessId)> processStepTypeStatus)
        where TProcessTypeId : struct, IConvertible
        where TProcessStepTypeId : struct, IConvertible;

    void AttachAndModifyProcessStep(Guid processStepId, Action<IProcessStep>? initialize, Action<IProcessStep> modify);
    void AttachAndModifyProcessSteps(IEnumerable<(Guid ProcessStepId, Action<IProcessStep>? Initialize, Action<IProcessStep> Modify)> processStepIdsInitializeModifyData);
    IAsyncEnumerable<IProcess> GetActiveProcesses(DateTimeOffset lockExpiryDate);
    IAsyncEnumerable<(int ProcessTypeId, Guid ProcessStepId, int ProcessStepTypeId)> GetProcessStepData(Guid processId);
    Task<(bool ProcessExists, VerifyProcessData ProcessData)> IsValidProcess(Guid processId, int processTypeId, IEnumerable<int> processStepTypeIds);
}
