/********************************************************************************
 * Copyright (c) 2024 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Context;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Extensions;

public static class RetriggerExtensions
{
    public static async Task TriggerProcessStep<TProcessTypeId, TProcessStepTypeId>(
        this TProcessStepTypeId stepToTrigger, Guid processId,
        IProcessRepositories processRepositories,
        Func<TProcessStepTypeId, (TProcessTypeId, TProcessStepTypeId)> getProcessStepForRetrigger)
        where TProcessTypeId : struct, IConvertible
        where TProcessStepTypeId : struct, IConvertible
    {
        var (processType, nextStep) = getProcessStepForRetrigger(stepToTrigger);

        var (validProcessId, processData) = await processRepositories.GetInstance<IProcessStepRepository<TProcessTypeId, TProcessStepTypeId>>().IsValidProcess(processId, processType, Enumerable.Repeat(stepToTrigger, 1)).ConfigureAwait(ConfigureAwaitOptions.None);
        if (!validProcessId)
        {
            throw new NotFoundException($"process {processId} does not exist");
        }

        var context = processData.CreateManualProcessData(stepToTrigger, processRepositories, () => $"processId {processId}");

        context.ScheduleProcessSteps(Enumerable.Repeat(nextStep, 1));
        context.FinalizeProcessStep();
        await processRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }
}
