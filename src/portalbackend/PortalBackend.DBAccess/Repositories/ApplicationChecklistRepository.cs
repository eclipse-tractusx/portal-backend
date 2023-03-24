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

using Microsoft.EntityFrameworkCore;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
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
    public void CreateChecklistForApplication(Guid applicationId, IEnumerable<(ApplicationChecklistEntryTypeId TypeId, ApplicationChecklistEntryStatusId StatusId)> checklistEntries)
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
    public ApplicationChecklistEntry AttachAndModifyApplicationChecklist(Guid applicationId, ApplicationChecklistEntryTypeId applicationChecklistTypeId, Action<ApplicationChecklistEntry> setFields)
    {
        var entity = new ApplicationChecklistEntry(applicationId, applicationChecklistTypeId, default, default);
        _portalDbContext.ApplicationChecklist.Attach(entity);
        entity.DateLastChanged = DateTimeOffset.UtcNow;
        setFields.Invoke(entity);
        return entity;
    }

    public Task<(bool IsValidProcessId, Guid ApplicationId, CompanyApplicationStatusId ApplicationStatusId, IEnumerable<(ApplicationChecklistEntryTypeId EntryTypeId, ApplicationChecklistEntryStatusId EntryStatusId)> Checklist)> GetChecklistData(Guid processId) =>
        _portalDbContext.Processes
            .AsNoTracking()
            .Where(process => process.Id == processId)
            .Select(process =>
                new ValueTuple<bool,Guid,CompanyApplicationStatusId,IEnumerable<(ApplicationChecklistEntryTypeId,ApplicationChecklistEntryStatusId)>>(
                    true,
                    process.CompanyApplication!.Id,
                    process.CompanyApplication.ApplicationStatusId,
                    process.CompanyApplication.ApplicationChecklistEntries.Select(entry => new ValueTuple<ApplicationChecklistEntryTypeId,ApplicationChecklistEntryStatusId>(
                        entry.ApplicationChecklistEntryTypeId,
                        entry.ApplicationChecklistEntryStatusId
                    ))
                ))
            .SingleOrDefaultAsync();

    public Task<VerifyChecklistData?> GetChecklistProcessStepData(Guid applicationId, IEnumerable<ApplicationChecklistEntryTypeId> entryTypeIds, IEnumerable<ProcessStepTypeId> processStepTypeIds) =>
        _portalDbContext.CompanyApplications
            .AsNoTracking()
            .AsSplitQuery()
            .Where(application => application.Id == applicationId)
            .Select(application => new {
                Application = application,
                IsSubmitted = application.ApplicationStatusId == CompanyApplicationStatusId.SUBMITTED,
                Process = application.ChecklistProcess,
            })
            .Select(x => new VerifyChecklistData(
                x.IsSubmitted,
                x.IsSubmitted
                    ? x.Process
                    : null,
                x.IsSubmitted
                    ? x.Application.ApplicationChecklistEntries
                        .Where(entry => entryTypeIds.Contains(entry.ApplicationChecklistEntryTypeId))
                        .Select(entry => new ValueTuple<ApplicationChecklistEntryTypeId,ApplicationChecklistEntryStatusId>(entry.ApplicationChecklistEntryTypeId, entry.ApplicationChecklistEntryStatusId))
                    : null,
                x.IsSubmitted
                    ? x.Process!.ProcessSteps
                        .Where(step =>
                            processStepTypeIds.Contains(step.ProcessStepTypeId) && 
                            step.ProcessStepStatusId == ProcessStepStatusId.TODO)
                    : null))
            .SingleOrDefaultAsync();
}
