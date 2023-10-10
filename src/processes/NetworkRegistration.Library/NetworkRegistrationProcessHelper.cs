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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.Library;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.NetworkRegistration.Library;

public class NetworkRegistrationProcessHelper : INetworkRegistrationProcessHelper
{
    private readonly IPortalRepositories _portalRepositories;

    public NetworkRegistrationProcessHelper(IPortalRepositories portalRepositories)
    {
        _portalRepositories = portalRepositories;
    }

    /// <inheritdoc />
    public async Task TriggerProcessStep(Guid externalId, ProcessStepTypeId stepToTrigger)
    {
        var nextStep = stepToTrigger switch
        {
            ProcessStepTypeId.RETRIGGER_SYNCHRONIZE_USER => ProcessStepTypeId.SYNCHRONIZE_USER,
            ProcessStepTypeId.RETRIGGER_CALLBACK_OSP_APPROVED => ProcessStepTypeId.TRIGGER_CALLBACK_OSP_APPROVED,
            ProcessStepTypeId.RETRIGGER_CALLBACK_OSP_DECLINED => ProcessStepTypeId.TRIGGER_CALLBACK_OSP_DECLINED,
            ProcessStepTypeId.RETRIGGER_CALLBACK_OSP_SUBMITTED => ProcessStepTypeId.TRIGGER_CALLBACK_OSP_SUBMITTED,
            _ => throw new ConflictException($"Step {stepToTrigger} is not retriggerable")
        };

        var networkRepository = _portalRepositories.GetInstance<INetworkRepository>();
        var (registrationIdExists, processData) = await networkRepository.IsValidRegistration(externalId, Enumerable.Repeat(stepToTrigger, 1)).ConfigureAwait(false);
        if (!registrationIdExists)
        {
            throw new NotFoundException($"external registration {externalId} does not exist");
        }

        var context = processData.CreateManualProcessData(stepToTrigger, _portalRepositories, () => $"externalId {externalId}");

        context.ScheduleProcessSteps(Enumerable.Repeat(nextStep, 1));
        context.FinalizeProcessStep();
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }
}
