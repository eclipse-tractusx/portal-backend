/********************************************************************************
 * Copyright (c) 2021, 2023 Microsoft and BMW Group AG
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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.OfferSubscription.Library.Extensions;

public static class VerifyOfferSubscriptionProcessDataExtensions
{
    public static IEnumerable<ProcessStepTypeId>? GetRetriggerStep(this ProcessStepTypeId processStepTypeId) =>
        processStepTypeId switch
        {
            ProcessStepTypeId.TRIGGER_PROVIDER => new []{ProcessStepTypeId.RETRIGGER_TRIGGER_PROVIDER},
            ProcessStepTypeId.OFFERSUBSCRIPTION_CLIENT_CREATION => new []{ProcessStepTypeId.RETRIGGER_OFFERSUBSCRIPTION_CLIENT_CREATION},
            ProcessStepTypeId.SINGLE_INSTANCE_SUBSCRIPTION_DETAILS_CREATION => new []{ProcessStepTypeId.RETRIGGER_SINGLE_INSTANCE_SUBSCRIPTION_DETAILS_CREATION},
            ProcessStepTypeId.OFFERSUBSCRIPTION_TECHNICALUSER_CREATION =>new []{ ProcessStepTypeId.RETRIGGER_OFFERSUBSCRIPTION_TECHNICALUSER_CREATION},
            ProcessStepTypeId.ACTIVATE_SUBSCRIPTION => new []{ProcessStepTypeId.RETRIGGER_ACTIVATE_SUBSCRIPTION},
            ProcessStepTypeId.TRIGGER_PROVIDER_CALLBACK => new []{ProcessStepTypeId.RETRIGGER_TRIGGER_PROVIDER_CALLBACK},
            _ => null
        };

    public static ProcessStepTypeId GetStepToRetrigger(this ProcessStepTypeId retriggerProcessStep) =>
        retriggerProcessStep switch
        {
            ProcessStepTypeId.RETRIGGER_TRIGGER_PROVIDER => ProcessStepTypeId.TRIGGER_PROVIDER,
            ProcessStepTypeId.RETRIGGER_OFFERSUBSCRIPTION_CLIENT_CREATION => ProcessStepTypeId.OFFERSUBSCRIPTION_CLIENT_CREATION,
            ProcessStepTypeId.RETRIGGER_SINGLE_INSTANCE_SUBSCRIPTION_DETAILS_CREATION => ProcessStepTypeId.SINGLE_INSTANCE_SUBSCRIPTION_DETAILS_CREATION,
            ProcessStepTypeId.RETRIGGER_OFFERSUBSCRIPTION_TECHNICALUSER_CREATION => ProcessStepTypeId.OFFERSUBSCRIPTION_TECHNICALUSER_CREATION,
            ProcessStepTypeId.RETRIGGER_ACTIVATE_SUBSCRIPTION => ProcessStepTypeId.ACTIVATE_SUBSCRIPTION,
            ProcessStepTypeId.RETRIGGER_TRIGGER_PROVIDER_CALLBACK => ProcessStepTypeId.TRIGGER_PROVIDER_CALLBACK,
            _ => throw new ConflictException($"Step {retriggerProcessStep} is not retriggerable")
        };

    public static ProcessStep ValidateOfferSubscriptionProcessData(this VerifyOfferSubscriptionProcessData? processData,
        Guid offerSubscriptionId,
        IEnumerable<ProcessStepStatusId> processStepStatusIds, ProcessStepTypeId processStepTypeId)
    {
        if (processData is null)
        {
            throw new NotFoundException($"offer subscription {offerSubscriptionId} does not exist");
        }

        if (processData.IsActive)
        {
            throw new ConflictException($"offer subscription {offerSubscriptionId} is already activated");
        }

        if (processData.Process == null)
        {
            throw new ConflictException($"offer subscription {offerSubscriptionId} is not associated with a process");
        }

        if (processData.Process.IsLocked())
        {
            throw new ConflictException($"process {processData.Process.Id} of {offerSubscriptionId} is locked, lock expiry is set to {processData.Process.LockExpiryDate}");
        }

        if (processData.ProcessSteps == null)
        {
            throw new UnexpectedConditionException("processSteps should never be null here");
        }

        if (processData.ProcessSteps.Any(step => !processStepStatusIds.Contains(step.ProcessStepStatusId)))
        {
            throw new UnexpectedConditionException($"processSteps should never have other status then {string.Join(",", processStepStatusIds)} here");
        }

        if (processData.ProcessSteps.Count(step => step.ProcessStepTypeId == processStepTypeId) != 1)
        {
            throw new ConflictException($"offer subscription {offerSubscriptionId} process step {processStepTypeId} is not eligible to run");
        }

        return processData.ProcessSteps.Single(step => step.ProcessStepTypeId == processStepTypeId);
    }

    public static IOfferSubscriptionProcessService.ManualOfferSubscriptionProcessStepData CreateManualOfferSubscriptionProcessStepData(this VerifyOfferSubscriptionProcessData checklistData, Guid offerSubscriptionId, ProcessStep processStep) =>
        new(offerSubscriptionId, checklistData.Process!, processStep.Id, checklistData.ProcessSteps!);
}
