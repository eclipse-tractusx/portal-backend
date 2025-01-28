/********************************************************************************
 * Copyright (c) 2025 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Worker.Library;

public static class ProcessTypeExecutorExtensions
{
    public static (ProcessStepStatusId StatusId, string? ProcessMessage, IEnumerable<TProcessStepTypeId>? nextSteps) ProcessError<TProcessTypeId, TProcessStepTypeId>(this Exception ex, TProcessStepTypeId processStepTypeId, TProcessTypeId processTypeId, Func<TProcessStepTypeId, (TProcessTypeId, TProcessStepTypeId)> getProcessStepForRetrigger)
        where TProcessStepTypeId : struct, IConvertible
    {
        var (expectedProcessTypeId, retriggerStep) = getProcessStepForRetrigger(processStepTypeId);
        if (expectedProcessTypeId?.Equals(processTypeId) != true)
        {
            throw new UnexpectedConditionException($"ProcessStepTypeId {processStepTypeId} is not supported for Process {processTypeId}");
        }

        return ex switch
        {
            ServiceException { IsRecoverable: true } => (ProcessStepStatusId.TODO, ex.Message, null),
            _ => (ProcessStepStatusId.FAILED, ex.Message, Enumerable.Repeat(retriggerStep, 1))
        };
    }
}
