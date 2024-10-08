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

using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Worker.Library;

public interface IProcessTypeExecutor<TProcessTypeId, TProcessStepTypeId>
    where TProcessTypeId : struct, IConvertible
    where TProcessStepTypeId : struct, IConvertible
{
    record InitializationResult(bool Modified, IEnumerable<TProcessStepTypeId>? ScheduleStepTypeIds);

    record StepExecutionResult(bool Modified, ProcessStepStatusId ProcessStepStatusId, IEnumerable<TProcessStepTypeId>? ScheduleStepTypeIds, IEnumerable<TProcessStepTypeId>? SkipStepTypeIds, string? ProcessMessage);

    ValueTask<InitializationResult> InitializeProcess(Guid processId, IEnumerable<TProcessStepTypeId> processStepTypeIds);
    ValueTask<bool> IsLockRequested(TProcessStepTypeId processStepTypeId);

    /// <summary>
    /// tbd
    /// </summary>
    /// <param name="processStepTypeId"></param>
    /// <param name="processStepTypeIds"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    ValueTask<StepExecutionResult> ExecuteProcessStep(TProcessStepTypeId processStepTypeId, IEnumerable<TProcessStepTypeId> processStepTypeIds, CancellationToken cancellationToken);

    bool IsExecutableStepTypeId(TProcessStepTypeId processStepTypeId);
    TProcessTypeId GetProcessTypeId();
    IEnumerable<TProcessStepTypeId> GetExecutableStepTypeIds();
}
