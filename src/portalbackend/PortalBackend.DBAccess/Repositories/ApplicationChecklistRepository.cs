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

using System.Collections;
using Microsoft.EntityFrameworkCore;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

public class ApplicationChecklistRepository : IApplicationChecklistRepository
{
    private readonly PortalDbContext _portalDbContext;

    /// <summary>
    /// Creates a new instance of <see cref="ApplicationChecklistRepository"/>
    /// </summary>
    /// <param name="portalDbContext">The portal db context</param>
    public ApplicationChecklistRepository(PortalDbContext portalDbContext)
    {
        _portalDbContext = portalDbContext;
    }

    /// <inheritdoc />
    public void CreateChecklistForApplication(Guid applicationId, IEnumerable<(ChecklistEntryTypeId TypeId, ChecklistEntryStatusId StatusId)> checklistEntries)
    {
        var entries = checklistEntries
            .Select(x => new ApplicationChecklistEntry(
                applicationId,
                x.TypeId,
                x.StatusId,
                DateTimeOffset.UtcNow));
        _portalDbContext.ApplicationChecklist.AddRange(entries);
    }

    /// <inheritdoc />
    public void AttachAndModifyApplicationChecklist(Guid applicationId, ChecklistEntryTypeId checklistTypeId, Action<ApplicationChecklistEntry> setFields)
    {
        var entity = new ApplicationChecklistEntry(applicationId, checklistTypeId, default, default);
        _portalDbContext.ApplicationChecklist.Attach(entity);
        entity.DateLastChanged = DateTimeOffset.UtcNow;
        setFields.Invoke(entity);
    }

    /// <inheritdoc />
    public Task<Dictionary<ChecklistEntryTypeId, ChecklistEntryStatusId>> GetChecklistDataAsync(Guid applicationId) =>
        _portalDbContext.ApplicationChecklist
            .Where(x => x.ApplicationId == applicationId)
            .Select(x => new ValueTuple<ChecklistEntryTypeId, ChecklistEntryStatusId>(x.ChecklistEntryTypeId, x.StatusId))
            .ToDictionaryAsync(x => x.Item1, x => x.Item2);

    /// <inheritdoc />
    public Task<Dictionary<Guid, IEnumerable<(ChecklistEntryTypeId TypeId, ChecklistEntryStatusId StatusId)>>> GetChecklistDataGroupedByApplicationId() =>
        _portalDbContext.ApplicationChecklist
            .GroupBy(x => x.ApplicationId)
            .Where(x => x.Any(e => e.StatusId == ChecklistEntryStatusId.TO_DO))
            .Select(x => new ValueTuple<Guid, IEnumerable<(ChecklistEntryTypeId TypeId, ChecklistEntryStatusId StatusId)>>(x.Key, x.Select(e => new ValueTuple<ChecklistEntryTypeId, ChecklistEntryStatusId>(e.ChecklistEntryTypeId, e.StatusId))))
            .ToDictionaryAsync(x => x.Item1, x => x.Item2);
}
