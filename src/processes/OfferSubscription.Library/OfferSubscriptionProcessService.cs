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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.Library;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.OfferSubscription.Library;

public class OfferSubscriptionProcessService : IOfferSubscriptionProcessService
{
    private readonly IPortalRepositories _portalRepositories;

    public OfferSubscriptionProcessService(IPortalRepositories portalRepositories)
    {
        _portalRepositories = portalRepositories;
    }

    public async Task<ManualProcessStepData> VerifySubscriptionAndProcessSteps(Guid offerSubscriptionId, ProcessStepTypeId processStepTypeId, IEnumerable<ProcessStepTypeId>? processStepTypeIds, bool mustBePending)
    {
        var allProcessStepTypeIds = processStepTypeIds switch
        {
            null => new[] { processStepTypeId },
            _ => processStepTypeIds.Contains(processStepTypeId)
                    ? processStepTypeIds
                    : processStepTypeIds.Append(processStepTypeId)
        };

        var offerSubscriptionsRepository = _portalRepositories.GetInstance<IOfferSubscriptionsRepository>();

        if (mustBePending)
        {
            var subscriptionInfo = await offerSubscriptionsRepository.IsActiveOfferSubscription(offerSubscriptionId).ConfigureAwait(false);
            if (!subscriptionInfo.IsValidSubscriptionId)
            {
                throw new NotFoundException($"offer subscription {offerSubscriptionId} does not exist");
            }
            if (subscriptionInfo.IsActive)
            {
                throw new ConflictException($"offer subscription {offerSubscriptionId} is already activated");
            }
        }

        var processData = await offerSubscriptionsRepository
            .GetProcessStepData(offerSubscriptionId, allProcessStepTypeIds).ConfigureAwait(false);

        return processData.CreateManualProcessData(processStepTypeId, _portalRepositories, () => $"offer subscription {offerSubscriptionId}");
    }

    public void FinalizeProcessSteps(ManualProcessStepData context, IEnumerable<ProcessStepTypeId>? nextProcessStepTypeIds)
    {
        if (nextProcessStepTypeIds != null && nextProcessStepTypeIds.Any())
        {
            context.ScheduleProcessSteps(nextProcessStepTypeIds);
        }

        context.FinalizeProcessStep();
    }
}
