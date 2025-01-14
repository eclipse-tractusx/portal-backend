/********************************************************************************
 * Copyright (c) 2023 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Context;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Entities;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Extensions;

public static class VerifyProcessDataExtensions
{
    public static IManualProcessStepData<TProcessType, TProcessStepType, TProcessTypeId, TProcessStepTypeId> CreateManualProcessData<TProcessType, TProcessStepType, TProcessTypeId, TProcessStepTypeId>(
        this IVerifyProcessData<TProcessType, TProcessStepType, TProcessTypeId, TProcessStepTypeId>? processData,
        TProcessStepTypeId? processStepTypeId,
        IRepositories processRepositories,
        Func<string> getProcessEntityName)
        where TProcessType : class, IProcess<TProcessTypeId>
        where TProcessStepType : class, IProcessStep<TProcessStepTypeId>
        where TProcessTypeId : struct, IConvertible
        where TProcessStepTypeId : struct, IConvertible
    {
        if (processData is null)
        {
            throw new NotFoundException($"{getProcessEntityName()} does not exist");
        }

        if (processData.Process == null)
        {
            throw new ConflictException($"{getProcessEntityName()} is not associated with any process");
        }

        if (processData.Process.IsLocked())
        {
            throw new ConflictException($"process {processData.Process.Id} associated with {getProcessEntityName()} is locked, lock expiry is set to {processData.Process.LockExpiryDate}");
        }

        if (processData.ProcessSteps == null)
        {
            throw new UnexpectedConditionException("processSteps should never be null here");
        }

        if (processData.ProcessSteps.Any(step => step.ProcessStepStatusId != ProcessStepStatusId.TODO))
        {
            throw new UnexpectedConditionException("processSteps should never have any other status than TODO here");
        }

        if (processStepTypeId != null && processData.ProcessSteps.All(step => !step.ProcessStepTypeId.Equals(processStepTypeId)))
        {
            throw new ConflictException($"{getProcessEntityName()}, process step {processStepTypeId} is not eligible to run");
        }

        return new ManualProcessStepData<TProcessType, TProcessStepType, TProcessTypeId, TProcessStepTypeId>(processStepTypeId, processData.Process, processData.ProcessSteps, processRepositories);
    }
}

public static class ManualProcessStepDataExtensions
{
    public static void RequestLock<TProcessType, TProcessStepType, TProcessTypeId, TProcessStepTypeId>(this IManualProcessStepData<TProcessType, TProcessStepType, TProcessTypeId, TProcessStepTypeId> context, DateTimeOffset lockExpiryDate)
        where TProcessType : class, IProcess<TProcessTypeId>
        where TProcessStepType : class, IProcessStep<TProcessStepTypeId>
        where TProcessTypeId : struct, IConvertible
        where TProcessStepTypeId : struct, IConvertible
    {
        context.ProcessRepositories.Attach(context.Process);

        var isLocked = context.Process.TryLock(lockExpiryDate);
        if (!isLocked)
        {
            throw new UnexpectedConditionException("process TryLock should never fail here");
        }
    }

    public static void SkipProcessSteps<TProcessType, TProcessStepType, TProcessTypeId, TProcessStepTypeId>(this IManualProcessStepData<TProcessType, TProcessStepType, TProcessTypeId, TProcessStepTypeId> context, IEnumerable<TProcessStepTypeId> processStepTypeIds)
        where TProcessType : class, IProcess<TProcessTypeId>
        where TProcessStepType : class, IProcessStep<TProcessStepTypeId>
        where TProcessTypeId : struct, IConvertible
        where TProcessStepTypeId : struct, IConvertible =>
        context.ProcessRepositories.GetInstance<IProcessStepRepository<TProcessType, TProcessStepType, TProcessTypeId, TProcessStepTypeId>>()
            .AttachAndModifyProcessSteps(
                context.ProcessSteps
                    .Where(step => !context.ProcessStepTypeId.HasValue || !step.ProcessStepTypeId.Equals(context.ProcessStepTypeId.Value))
                    .GroupBy(step => step.ProcessStepTypeId)
                    .IntersectBy(processStepTypeIds, group => group.Key)
                    .SelectMany(group => ModifyStepStatusRange<TProcessStepType, TProcessStepTypeId>(group, ProcessStepStatusId.SKIPPED)));

    public static void SkipProcessStepsExcept<TProcessType, TProcessStepType, TProcessTypeId, TProcessStepTypeId>(this IManualProcessStepData<TProcessType, TProcessStepType, TProcessTypeId, TProcessStepTypeId> context, IEnumerable<TProcessStepTypeId> processStepTypeIds)
        where TProcessType : class, IProcess<TProcessTypeId>
        where TProcessStepType : class, IProcessStep<TProcessStepTypeId>
        where TProcessTypeId : struct, IConvertible
        where TProcessStepTypeId : struct, IConvertible =>
        context.ProcessRepositories.GetInstance<IProcessStepRepository<TProcessType, TProcessStepType, TProcessTypeId, TProcessStepTypeId>>()
            .AttachAndModifyProcessSteps(
                context.ProcessSteps
                    .Where(step => !context.ProcessStepTypeId.HasValue || !step.ProcessStepTypeId.Equals(context.ProcessStepTypeId.Value))
                    .GroupBy(step => step.ProcessStepTypeId)
                    .ExceptBy(processStepTypeIds, group => group.Key)
                    .SelectMany(group => ModifyStepStatusRange<TProcessStepType, TProcessStepTypeId>(group, ProcessStepStatusId.SKIPPED)));

    public static void ScheduleProcessSteps<TProcessType, TProcessStepType, TProcessTypeId, TProcessStepTypeId>(this IManualProcessStepData<TProcessType, TProcessStepType, TProcessTypeId, TProcessStepTypeId> context, IEnumerable<TProcessStepTypeId> processStepTypeIds)
        where TProcessType : class, IProcess<TProcessTypeId>
        where TProcessStepType : class, IProcessStep<TProcessStepTypeId>
        where TProcessTypeId : struct, IConvertible
        where TProcessStepTypeId : struct, IConvertible =>
        context.ProcessRepositories.GetInstance<IProcessStepRepository<TProcessType, TProcessStepType, TProcessTypeId, TProcessStepTypeId>>()
            .CreateProcessStepRange(
                processStepTypeIds
                    .Except(context.ProcessSteps.Select(step => step.ProcessStepTypeId))
                    .Select(stepTypeId => (stepTypeId, ProcessStepStatusId.TODO, context.Process.Id)));

    public static void FinalizeProcessStep<TProcessType, TProcessStepType, TProcessTypeId, TProcessStepTypeId>(this IManualProcessStepData<TProcessType, TProcessStepType, TProcessTypeId, TProcessStepTypeId> context)
        where TProcessType : class, IProcess<TProcessTypeId>
        where TProcessStepType : class, IProcessStep<TProcessStepTypeId>
        where TProcessTypeId : struct, IConvertible
        where TProcessStepTypeId : struct, IConvertible
    {
        if (context.ProcessStepTypeId != null)
        {
            context.ProcessRepositories.GetInstance<IProcessStepRepository<TProcessType, TProcessStepType, TProcessTypeId, TProcessStepTypeId>>().AttachAndModifyProcessSteps(
                ModifyStepStatusRange<TProcessStepType, TProcessStepTypeId>(context.ProcessSteps.Where(step => step.ProcessStepTypeId.Equals(context.ProcessStepTypeId!.Value)), ProcessStepStatusId.DONE));
        }

        context.ProcessRepositories.Attach(context.Process);
        if (!context.Process.ReleaseLock())
        {
            context.Process.UpdateVersion();
        }
    }

    private static IEnumerable<(Guid, Action<TProcessStepType>?, Action<TProcessStepType>)> ModifyStepStatusRange<TProcessStepType, TProcessStepTypeId>(IEnumerable<TProcessStepType> steps, ProcessStepStatusId processStepStatusId)
        where TProcessStepType : class, IProcessStep<TProcessStepTypeId>
        where TProcessStepTypeId : struct, IConvertible
    {
        using var enumerator = steps.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            yield break;
        }

        var current = enumerator.Current;

        yield return (
            current.Id,
            ps => ps.ProcessStepStatusId = current.ProcessStepStatusId,
            ps => ps.ProcessStepStatusId = processStepStatusId);

        while (enumerator.MoveNext())
        {
            current = enumerator.Current;
            yield return (
                current.Id,
                ps => ps.ProcessStepStatusId = current.ProcessStepStatusId,
                ps => ps.ProcessStepStatusId = ProcessStepStatusId.DUPLICATE);
        }
    }
}
