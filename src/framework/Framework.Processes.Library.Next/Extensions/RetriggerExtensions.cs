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

using Org.Eclipse.TractusX.Portal.Backend.Framework.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Next.DBAccess;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Next.Extensions;

public static class RetriggerExtensions
{
    public static async Task TriggerProcessStep<TProcessTypeId, TProcessStepTypeId>(
        this TProcessStepTypeId stepToTrigger,
        Guid processId,
        IRepositories processRepositories)
        where TProcessTypeId : struct, Enum
        where TProcessStepTypeId : struct, Enum
    {
        var (processType, nextStep) = stepToTrigger.GetStepToRetrigger<TProcessTypeId, TProcessStepTypeId>();

        var (validProcessId, processData) = await processRepositories.GetInstance<IProcessStepRepository>().IsValidProcess(processId, Convert.ToInt32(processType), Enumerable.Repeat(Convert.ToInt32(stepToTrigger), 1)).ConfigureAwait(ConfigureAwaitOptions.None);
        if (!validProcessId)
        {
            throw new NotFoundException($"process {processId} does not exist");
        }

        var context = processData.CreateManualProcessData<TProcessStepTypeId>(stepToTrigger, processRepositories, () => $"processId {processId}");

        context.ScheduleProcessSteps(processType, Enumerable.Repeat(nextStep, 1));
        context.FinalizeProcessStep();
        await processRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }
}
