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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Checklist.Library;

public interface IChecklistCreationService
{
    /// <summary>
    /// Creates the initial checklist for the given application
    /// </summary>
    /// <param name="applicationId">Id of the application</param>
    Task CreateInitialChecklistAsync(Guid applicationId);

    /// <summary>
    /// Creates the missing items for the application
    /// </summary>
    /// <remarks>
    /// <b>The DbContext will be cleared</b>
    /// </remarks>
    /// <param name="applicationId">ID of the application the items should be created for</param>
    /// <param name="existingChecklistEntryTypeIds">The currently existing <see cref="ApplicationChecklistEntryTypeId"/></param>
    /// <returns>The created ChecklistApplication Items</returns>
    Task<IEnumerable<(ApplicationChecklistEntryTypeId TypeId, ApplicationChecklistEntryStatusId StatusId)>> CreateMissingChecklistItems(Guid applicationId, IEnumerable<ApplicationChecklistEntryTypeId> existingChecklistEntryTypeIds);
    IEnumerable<ProcessStepTypeId> GetInitialProcessStepTypeIds(IEnumerable<(ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId)> checklistEntries);
}
