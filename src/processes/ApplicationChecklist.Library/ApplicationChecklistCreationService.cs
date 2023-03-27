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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.ApplicationChecklist.Library;

public class ApplicationChecklistCreationService : IApplicationChecklistCreationService
{
    private readonly IPortalRepositories _portalRepositories;

    public ApplicationChecklistCreationService(IPortalRepositories portalRepositories)
    {
        _portalRepositories = portalRepositories;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<(ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId)>> CreateInitialChecklistAsync(Guid applicationId)
    {
        var (bpn, existingChecklistEntryTypeIds) = await _portalRepositories.GetInstance<IApplicationRepository>().GetBpnAndChecklistCheckForApplicationIdAsync(applicationId).ConfigureAwait(false);
        return CreateEntries(applicationId, existingChecklistEntryTypeIds, bpn);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<(ApplicationChecklistEntryTypeId TypeId, ApplicationChecklistEntryStatusId StatusId)>> CreateMissingChecklistItems(Guid applicationId, IEnumerable<ApplicationChecklistEntryTypeId> existingChecklistEntryTypeIds)
    {
        var bpn = await _portalRepositories.GetInstance<IApplicationRepository>().GetBpnForApplicationIdAsync(applicationId).ConfigureAwait(false);
        return CreateEntries(applicationId, existingChecklistEntryTypeIds, bpn);
    }

    private IEnumerable<(ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId)> CreateEntries(Guid applicationId, IEnumerable<ApplicationChecklistEntryTypeId> existingChecklistEntryTypeIds, string? bpn)
    {
        var missingEntries = Enum.GetValues<ApplicationChecklistEntryTypeId>()
            .Except(existingChecklistEntryTypeIds);
        if (missingEntries.Any())
        {
            var newEntries = missingEntries.Select(x => (ApplicationChecklistEntryTypeId: x, ApplicationChecklistStatusTypeId: GetInitialChecklistStatus(x, bpn)));
            _portalRepositories.GetInstance<IApplicationChecklistRepository>()
                .CreateChecklistForApplication(applicationId, newEntries);
            return newEntries;
        }
        return Enumerable.Empty<(ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId)>();
    }

    private static ApplicationChecklistEntryStatusId GetInitialChecklistStatus(ApplicationChecklistEntryTypeId applicationChecklistEntryTypeId, string? bpn) =>
        applicationChecklistEntryTypeId switch
        {
            ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER => string.IsNullOrWhiteSpace(bpn)
                ? ApplicationChecklistEntryStatusId.TO_DO
                : ApplicationChecklistEntryStatusId.DONE,
            _ => ApplicationChecklistEntryStatusId.TO_DO
        };

    public IEnumerable<ProcessStepTypeId> GetInitialProcessStepTypeIds(IEnumerable<(ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId)> checklistEntries)
    {
        foreach (var (entryTypeId, statusId) in checklistEntries)
        {
            switch(entryTypeId)
            {
                case ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION:
                    yield return ProcessStepTypeId.VERIFY_REGISTRATION;
                    yield return ProcessStepTypeId.DECLINE_APPLICATION;
                    break;
                case ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER when (statusId == ApplicationChecklistEntryStatusId.TO_DO):
                    yield return ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PUSH;
                    yield return ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_MANUAL;
                    break;
                default: // IDENTITY_WALLET, CLEARING_HOUSE and SELF_DESCRIPTION_LP start defered.
                    break;
            }
        }
    }
}
