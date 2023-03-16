/********************************************************************************
 * Copyright (c) 2021, 2023 BMW Group AG
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

using Org.Eclipse.TractusX.Portal.Backend.Framework.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.OfferSubscription.Library.Extensions;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.OfferSubscription.Library;

public interface IOfferSubscriptionProcessService
{
    record ManualOfferSubscriptionProcessStepData(Guid OfferSubscriptionId, Process Process, Guid ProcessStepId, IEnumerable<ProcessStep> ProcessSteps);

    Task<ManualOfferSubscriptionProcessStepData> VerifySubscriptionAndProcessSteps(Guid offerSubscriptionId, ProcessStepTypeId processStepTypeId, IEnumerable<ProcessStepTypeId>? processStepTypeIds);
    void FinalizeChecklistEntryAndProcessSteps(IOfferSubscriptionProcessService.ManualOfferSubscriptionProcessStepData context, IEnumerable<ProcessStepTypeId>? nextProcessStepTypeIds);
}

public class OfferSubscriptionProcessService : IOfferSubscriptionProcessService
{
    private readonly IPortalRepositories _portalRepositories;

    public OfferSubscriptionProcessService(IPortalRepositories portalRepositories)
    {
        _portalRepositories = portalRepositories;
    }

    async Task<IOfferSubscriptionProcessService.ManualOfferSubscriptionProcessStepData> IOfferSubscriptionProcessService.VerifySubscriptionAndProcessSteps(Guid offerSubscriptionId, ProcessStepTypeId processStepTypeId, IEnumerable<ProcessStepTypeId>? processStepTypeIds)
    {
        var allProcessStepTypeIds = processStepTypeIds == null
            ? new[] { processStepTypeId }
            : processStepTypeIds.Append(processStepTypeId);

        var processData = await _portalRepositories.GetInstance<IOfferSubscriptionsRepository>()
            .GetProcessStepData(offerSubscriptionId, allProcessStepTypeIds).ConfigureAwait(false);

        processData.ValidateOfferSubscriptionProcessData(offerSubscriptionId, new[] { ProcessStepStatusId.TODO });
        var processStep = processData!.ProcessSteps!.SingleOrDefault(step => step.ProcessStepTypeId == processStepTypeId);
        if (processStep is null)
        {
            throw new ConflictException($"offer subscription {offerSubscriptionId} process step {processStepTypeId} is not eligible to run");
        }
        return processData.CreateManualOfferSubscriptionProcessStepData(offerSubscriptionId, processStep);
    }

    public void RequestLock(IOfferSubscriptionProcessService.ManualOfferSubscriptionProcessStepData context, DateTimeOffset lockExpiryDate)
    {
        _portalRepositories.Attach(context.Process);
        var isLocked = context.Process.TryLock(lockExpiryDate);
        if (!isLocked)
        {
            throw new UnexpectedConditionException("process TryLock should never fail here");
        }
    }

    public void SkipProcessSteps(IOfferSubscriptionProcessService.ManualOfferSubscriptionProcessStepData context, IEnumerable<ProcessStepTypeId> processStepTypeIds)
    {
        var processStepRepository = _portalRepositories.GetInstance<IProcessStepRepository>();
        foreach (var processStepGroup in context.ProcessSteps.GroupBy(step => step.ProcessStepTypeId).IntersectBy(processStepTypeIds, step => step.Key))
        {
            var firstModified = false;
            foreach (var processStep in processStepGroup)
            {
                processStepRepository.AttachAndModifyProcessStep(
                    processStep.Id,
                    null,
                    step => step.ProcessStepStatusId =
                        firstModified
                            ? ProcessStepStatusId.DUPLICATE
                            : ProcessStepStatusId.SKIPPED);
                firstModified = true;
            }
        }
    }

    public void FinalizeChecklistEntryAndProcessSteps(IOfferSubscriptionProcessService.ManualOfferSubscriptionProcessStepData context, IEnumerable<ProcessStepTypeId>? nextProcessStepTypeIds)
    {
        var processStepRepository = _portalRepositories.GetInstance<IProcessStepRepository>();
        processStepRepository.AttachAndModifyProcessStep(context.ProcessStepId, null, step => step.ProcessStepStatusId = ProcessStepStatusId.DONE);
        if (nextProcessStepTypeIds == null || !nextProcessStepTypeIds.Any())
        {
            return;
        }

        processStepRepository.CreateProcessStepRange(
            nextProcessStepTypeIds
                .Except(context.ProcessSteps.Select(step => step.ProcessStepTypeId))
                .Select(stepTypeId => (stepTypeId, ProcessStepStatusId.TODO, context.Process.Id)));

        if (context.Process.ReleaseLock())
            return;

        _portalRepositories.Attach(context.Process);
        context.Process.UpdateVersion();
    }
}
