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

public interface IProcessTypeExecutor<TProcessTypeId>
    where TProcessTypeId : struct, IConvertible
{
    record InitializationResult(bool Modified, IEnumerable<int>? ScheduleStepTypeIds);

    record StepExecutionResult(bool Modified, ProcessStepStatusId ProcessStepStatusId, IEnumerable<int>? ScheduleStepTypeIds, IEnumerable<int>? SkipStepTypeIds, string? ProcessMessage);

    TProcessTypeId GetProcessTypeId();

    ValueTask<InitializationResult> InitializeProcess(Guid processId, IEnumerable<int> processStepTypeIds);
    ValueTask<bool> IsLockRequested(int processStepTypeId);

    /// <summary>
    /// Executes the process step and returns the result
    /// </summary>
    /// <param name="processStepTypeId">Id of the processStepType that is being executed</param>
    /// <param name="processStepTypeIds">List of the processStepTypeIds</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>The result of the execution</returns>
    ValueTask<StepExecutionResult> ExecuteProcessStep(int processStepTypeId, IEnumerable<int> processStepTypeIds, CancellationToken cancellationToken);
}
