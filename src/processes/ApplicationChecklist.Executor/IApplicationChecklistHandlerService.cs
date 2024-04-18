/********************************************************************************
 * Copyright (c) 2021, 2023 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.ApplicationChecklist.Library;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.ApplicationChecklist.Executor;

/// <summary>
/// Service that organizes the mapping of checklist ProcessStepExecutions to ProcessStepTypes
/// </summary>
public interface IApplicationChecklistHandlerService
{
    /// <summary>
    /// class used to map processing- and error-functions to ProcessStepTypeIds and ApplicationChecklistEntryTypeIds
    /// </summary>
    /// <param name="EntryTypeId">the ApplicationChecklistEntryTypeId this ProcessStepExecution is associated with</param>
    /// <param name="ProcessFunc">the function to be executed by ChecklistProcessor</param>
    /// <param name="ErrorFunc">the function to be executed in case ProcessFunc threw an application-exception (optional)</param>
    record ProcessStepExecution(
        ApplicationChecklistEntryTypeId EntryTypeId,
        bool RequiresLock,
        Func<IApplicationChecklistService.WorkerChecklistProcessStepData, CancellationToken, Task<IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult>> ProcessFunc,
        Func<Exception, IApplicationChecklistService.WorkerChecklistProcessStepData, CancellationToken, Task<IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult>>? ErrorFunc
    );

    /// <summary>
    /// returns the ProcessStepExecution being mapped to a particular ProcessStepTypeId
    /// </summary>
    /// <param name="stepTypeId">ProcessStepTypeId</param>
    ProcessStepExecution GetProcessStepExecution(ProcessStepTypeId stepTypeId);

    /// <summary>
    /// returns whether a ProcessStepTypeId shall be executed automatically by the ChecklistProcessor
    /// </summary>
    /// <param name="stepTypeId">ProcessStepTypeId</param>
    bool IsExecutableProcessStep(ProcessStepTypeId stepTypeId);

    /// <summary>
    /// returns the ProcessStepTypeIds that shall be executed automatically by the ChecklistProcessor
    /// </summary>
    IEnumerable<ProcessStepTypeId> GetExecutableStepTypeIds();
}
