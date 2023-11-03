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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Library;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.OfferSubscription.Library.Extensions;

public static class OfferSubscriptionExtensions
{
    public static IEnumerable<ProcessStepTypeId>? GetRetriggerStep(this ProcessStepTypeId processStepTypeId) =>
        processStepTypeId switch
        {
            ProcessStepTypeId.TRIGGER_PROVIDER => new[] { ProcessStepTypeId.RETRIGGER_PROVIDER },
            ProcessStepTypeId.OFFERSUBSCRIPTION_CLIENT_CREATION => new[] { ProcessStepTypeId.RETRIGGER_OFFERSUBSCRIPTION_CLIENT_CREATION },
            ProcessStepTypeId.OFFERSUBSCRIPTION_TECHNICALUSER_CREATION => new[] { ProcessStepTypeId.RETRIGGER_OFFERSUBSCRIPTION_TECHNICALUSER_CREATION },
            ProcessStepTypeId.TRIGGER_PROVIDER_CALLBACK => new[] { ProcessStepTypeId.RETRIGGER_PROVIDER_CALLBACK },
            _ => null
        };

    public static ProcessStepTypeId GetStepToRetrigger(this ProcessStepTypeId retriggerProcessStep) =>
        retriggerProcessStep switch
        {
            ProcessStepTypeId.RETRIGGER_PROVIDER => ProcessStepTypeId.TRIGGER_PROVIDER,
            ProcessStepTypeId.RETRIGGER_OFFERSUBSCRIPTION_CLIENT_CREATION => ProcessStepTypeId.OFFERSUBSCRIPTION_CLIENT_CREATION,
            ProcessStepTypeId.RETRIGGER_OFFERSUBSCRIPTION_TECHNICALUSER_CREATION => ProcessStepTypeId.OFFERSUBSCRIPTION_TECHNICALUSER_CREATION,
            ProcessStepTypeId.RETRIGGER_PROVIDER_CALLBACK => ProcessStepTypeId.TRIGGER_PROVIDER_CALLBACK,
            _ => throw new ConflictException($"Step {retriggerProcessStep} is not retriggerable")
        };
}
