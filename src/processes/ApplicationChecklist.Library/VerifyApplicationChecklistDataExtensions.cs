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
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Library;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.ApplicationChecklist.Library;

public static class VerifyApplicationChecklistDataExtensions
{
    public static void ValidateApplicationChecklistData(
        this VerifyChecklistData? checklistData,
        Guid applicationId,
        ApplicationChecklistEntryTypeId entryTypeId, IEnumerable<ApplicationChecklistEntryStatusId> entryStatusIds,
        IEnumerable<ProcessStepStatusId> processStepStatusIds)
    {
        if (checklistData is null)
        {
            throw new NotFoundException($"application {applicationId} does not exist");
        }

        if (!checklistData.IsSubmitted)
        {
            throw new ConflictException($"application {applicationId} is not in status SUBMITTED");
        }

        if (checklistData.Process == null)
        {
            throw new ConflictException($"application {applicationId} is not associated with a checklist-process");
        }

        if (checklistData.Process.IsLocked())
        {
            throw new ConflictException($"checklist-process {checklistData.Process.Id} of {applicationId} is locked, lock expiry is set to {checklistData.Process.LockExpiryDate}");
        }

        if (checklistData.Checklist == null || checklistData.ProcessSteps == null)
        {
            throw new UnexpectedConditionException("checklist or processSteps should never be null here");
        }

        if (checklistData.ProcessSteps == null || checklistData.ProcessSteps.Any(step => !processStepStatusIds.Contains(step.ProcessStepStatusId)))
        {
            throw new UnexpectedConditionException($"processSteps should never have other status than {string.Join(",", processStepStatusIds)} here");
        }

        if (!checklistData.Checklist.Any(entry => entry.TypeId == entryTypeId && entryStatusIds.Contains(entry.StatusId)))
        {
            throw new ConflictException($"application {applicationId} does not have a checklist entry for {entryTypeId} in status {string.Join(", ", entryStatusIds)}");
        }
    }

    public static IApplicationChecklistService.ManualChecklistProcessStepData CreateManualChecklistProcessStepData(this VerifyChecklistData checklistData, Guid applicationId, ApplicationChecklistEntryTypeId entryTypeId, ProcessStep processStep) =>
        new IApplicationChecklistService.ManualChecklistProcessStepData(
            applicationId,
            checklistData.Process ?? throw new UnexpectedConditionException("checklistData.Process should never be null here"),
            processStep.Id,
            entryTypeId,
            checklistData.Checklist?.ToImmutableDictionary(entry => entry.TypeId, entry => new ValueTuple<ApplicationChecklistEntryStatusId, string?>(entry.StatusId, entry.Comment)) ?? throw new UnexpectedConditionException("checklistData.Checklist should never be null here"),
            checklistData.ProcessSteps ?? throw new UnexpectedConditionException("checklistData.ProcessSteps should never be null here"));
}
