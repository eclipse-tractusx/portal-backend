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

using Org.Eclipse.TractusX.Portal.Backend.Checklist.Service.Bpdm;
using Org.Eclipse.TractusX.Portal.Backend.Checklist.Service.Bpdm.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Checklist.Service;

public class ChecklistService : IChecklistService
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly IBpdmService _bpdmService;

    public ChecklistService(IPortalRepositories portalRepositories, IBpdmService bpdmService)
    {
        _portalRepositories = portalRepositories;
        _bpdmService = bpdmService;
    }

    /// <inheritdoc />
    public async Task CreateInitialChecklistAsync(Guid applicationId)
    {
        var bpn =  await _portalRepositories.GetInstance<IApplicationRepository>().GetBpnForApplicationIdAsync(applicationId).ConfigureAwait(false);
        var checklistEntries = Enum.GetValues<ChecklistEntryTypeId>()
            .Select(x => 
                new ValueTuple<ChecklistEntryTypeId, ChecklistEntryStatusId>(x, GetChecklistStatus(x, bpn))
            );
        _portalRepositories.GetInstance<IApplicationChecklistRepository>()
            .CreateChecklistForApplication(applicationId, checklistEntries);
    }

    /// <inheritdoc />
    public async Task TriggerBpnDataPush(Guid applicationId, BpdmData data, CancellationToken cancellationToken)
    {
        var bpdmTransferData = new BpdmTransferData(data.CompanyName, data.AlphaCode2, data.ZipCode, data.City, data.Street);
        await _bpdmService.TriggerBpnDataPush(bpdmTransferData, cancellationToken).ConfigureAwait(false);
        await this.UpdateBpnStatusAsync(applicationId, ChecklistEntryStatusId.IN_PROGRESS).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task UpdateBpnStatusAsync(Guid applicationId, ChecklistEntryStatusId statusId)
    {
        _portalRepositories.GetInstance<IApplicationChecklistRepository>()
            .AttachAndModifyApplicationChecklist(applicationId, ChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, checklist =>
            {
                checklist.StatusId = statusId;
            });
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    private static ChecklistEntryStatusId GetChecklistStatus(ChecklistEntryTypeId checklistEntryTypeId, string? bpn) =>
        checklistEntryTypeId switch
        {
            ChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER => string.IsNullOrWhiteSpace(bpn)
                ? ChecklistEntryStatusId.TO_DO
                : ChecklistEntryStatusId.DONE,
            _ => ChecklistEntryStatusId.TO_DO
        };
}
