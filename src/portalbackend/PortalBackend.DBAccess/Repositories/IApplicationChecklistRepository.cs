/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

public interface IApplicationChecklistRepository
{
    /// <summary>
    /// Creates the initial checklist for given application
    /// </summary>
    /// <param name="applicationId">Id of the application to create the checklist for</param>
    /// <param name="checklistEntries">Combination of type and it's status</param>
    /// <returns>Returns the created entries</returns>
    void CreateChecklistForApplication(Guid applicationId, IEnumerable<(ApplicationChecklistEntryTypeId TypeId, ApplicationChecklistEntryStatusId StatusId)> checklistEntries);
    
    /// <summary>
    /// Attaches a checklist entry with the given id and modifies it with the action.
    /// </summary>
    /// <param name="applicationId">Id of the application to modify</param>
    /// <param name="applicationChecklistTypeId">Id of the checklistType to modify</param>
    /// <param name="setFields">Action to sets the fields</param>
    void AttachAndModifyApplicationChecklist(Guid applicationId, ApplicationChecklistEntryTypeId applicationChecklistTypeId, Action<ApplicationChecklistEntry> setFields);

    /// <summary>
    /// Gets the combination of the checklist type and status
    /// </summary>
    /// <param name="applicationId">Id of the application to get the checklist for</param>
    /// <returns>Returns the data of the checklist for the specific application</returns>
    Task<Dictionary<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>> GetChecklistDataAsync(Guid applicationId);

    /// <summary>
    /// Gets all checklist entries where at least one item is in TO_DO state grouped by the application id
    /// </summary>
    /// <param name="itemCount"></param>
    /// <returns>Returns an async enumerable with the checklist data</returns>
    IAsyncEnumerable<(Guid ApplicationId, IEnumerable<(ApplicationChecklistEntryTypeId TypeId, ApplicationChecklistEntryStatusId StatusId)> ChecklistEntries)> GetChecklistDataGroupedByApplicationId(int itemCount);
}
