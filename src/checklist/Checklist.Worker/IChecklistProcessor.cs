/********************************************************************************
 * Copyright (c) 2021,2023 Microsoft and BMW Group AG
 * Copyright (c) 2021,2023 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Checklist.Worker;

/// <summary>
/// Service that executes open/pending processSteps of a checklist and processes their result.
/// </summary>
public interface IChecklistProcessor
{
    /// <summary>
    /// Processes the possible automated steps of the checklist
    /// </summary>
    /// <param name="applicationId">Id of the application to process the checklist</param>
    /// <param name="checklistEntries">The checklist entries to process</param>
    /// <param name="processSteps">The eligible processSteps</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    IAsyncEnumerable<(ApplicationChecklistEntryTypeId TypeId, ApplicationChecklistEntryStatusId StatusId)> ProcessChecklist(Guid applicationId, IEnumerable<(ApplicationChecklistEntryTypeId EntryTypeId, ApplicationChecklistEntryStatusId EntryStatusId)> checklistEntries, IEnumerable<ProcessStep> processSteps, CancellationToken cancellationToken);
}
