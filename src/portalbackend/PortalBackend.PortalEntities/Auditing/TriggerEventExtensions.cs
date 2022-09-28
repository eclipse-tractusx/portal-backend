/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
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

using Laraue.EfCoreTriggers.Common.TriggerBuilders;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Auditing;

/// <summary>
/// Extension methods for the trigger event
/// </summary>
public static class TriggerEventExtensions
{
    /// <summary>
    /// Converts an <seealso cref="TriggerEvent"/> to an <seealso cref="AuditOperationId"/> 
    /// </summary>
    /// <param name="triggerEvent">The trigger event that should be converted</param>
    /// <returns>Returns an <seealso cref="AuditOperationId"/></returns>
    public static AuditOperationId GetOperationForTriggerEvent(this TriggerEvent triggerEvent) =>
        triggerEvent switch
        {
            TriggerEvent.Insert => AuditOperationId.INSERT,
            TriggerEvent.Update => AuditOperationId.UPDATE,
            TriggerEvent.Delete => AuditOperationId.DELETE,
            _ => throw new ArgumentOutOfRangeException(nameof(triggerEvent), triggerEvent, null)
        };
}