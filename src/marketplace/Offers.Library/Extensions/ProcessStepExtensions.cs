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

using Microsoft.Extensions.Logging;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Offers.Library.Extensions;

public static class ProcessStepExtensions
{
    public static ProcessStepTypeId? GetProcessStepTypeId(this IEnumerable<(ProcessStepTypeId ProcessStepTypeId, ProcessStepStatusId ProcessStepStatusId)> processSteps, Guid offerId, ILogger logger)
    {
        if (processSteps.Count(p => p.ProcessStepStatusId == ProcessStepStatusId.TODO) > 1)
        {
            logger.Log(LogLevel.Error, "Offers: {OfferIds} contain more than one process step in todo", string.Join(",", offerId));
        }

        return processSteps.Any(p => p.ProcessStepStatusId == ProcessStepStatusId.TODO)
            ? processSteps.Single(p => p.ProcessStepStatusId == ProcessStepStatusId.TODO).ProcessStepTypeId
            : null;
    }
}
