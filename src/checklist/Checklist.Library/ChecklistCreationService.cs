/********************************************************************************
 * Copyright (c) 2021,2022 Microsoft and BMW Group AG
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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Checklist.Library;

public class ChecklistCreationService : IChecklistCreationService
{
    private readonly IPortalRepositories _portalRepositories;

    public ChecklistCreationService(IPortalRepositories portalRepositories)
    {
        _portalRepositories = portalRepositories;
    }

    /// <inheritdoc />
    public async Task CreateInitialChecklistAsync(Guid applicationId)
    {
        var (bpn, checklistAlreadyExists) = await _portalRepositories.GetInstance<IApplicationRepository>().GetBpnAndChecklistCheckForApplicationIdAsync(applicationId).ConfigureAwait(false);
        if (!checklistAlreadyExists)
        {
            var checklistEntries = Enum.GetValues<ApplicationChecklistEntryTypeId>()
                .Select(x => 
                    new ValueTuple<ApplicationChecklistEntryTypeId, ApplicationChecklistEntryStatusId>(x, GetChecklistStatus(x, bpn))
                );
            _portalRepositories.GetInstance<IApplicationChecklistRepository>()
                .CreateChecklistForApplication(applicationId, checklistEntries);    
        }
    }

    private static ApplicationChecklistEntryStatusId GetChecklistStatus(ApplicationChecklistEntryTypeId applicationChecklistEntryTypeId, string? bpn) =>
        applicationChecklistEntryTypeId switch
        {
            ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER => string.IsNullOrWhiteSpace(bpn)
                ? ApplicationChecklistEntryStatusId.TO_DO
                : ApplicationChecklistEntryStatusId.DONE,
            _ => ApplicationChecklistEntryStatusId.TO_DO
        };
}
