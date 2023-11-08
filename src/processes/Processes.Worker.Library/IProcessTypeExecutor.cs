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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.Worker.Library;

public interface IProcessTypeExecutor
{
    record InitializationResult(bool Modified, IEnumerable<ProcessStepTypeId>? ScheduleStepTypeIds);
    record StepExecutionResult(bool Modified, ProcessStepStatusId ProcessStepStatusId, IEnumerable<ProcessStepTypeId>? ScheduleStepTypeIds, IEnumerable<ProcessStepTypeId>? SkipStepTypeIds, string? ProcessMessage);

    ValueTask<InitializationResult> InitializeProcess(Guid processId, IEnumerable<ProcessStepTypeId> processStepTypeIds);
    ValueTask<bool> IsLockRequested(ProcessStepTypeId processStepTypeId);

    /// <summary>
    /// tbd
    /// </summary>
    /// <param name="processStepTypeId"></param>
    /// <param name="processStepTypeIds"></param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="NotFoundException">Is thrown if entity is not found</exception>
    /// <exception cref="UnexpectedConditionException">Is thrown if ...</exception>
    /// <returns></returns>
    ValueTask<StepExecutionResult> ExecuteProcessStep(ProcessStepTypeId processStepTypeId, IEnumerable<ProcessStepTypeId> processStepTypeIds, CancellationToken cancellationToken);
    bool IsExecutableStepTypeId(ProcessStepTypeId processStepTypeId);
    ProcessTypeId GetProcessTypeId();
    IEnumerable<ProcessStepTypeId> GetExecutableStepTypeIds();
}
