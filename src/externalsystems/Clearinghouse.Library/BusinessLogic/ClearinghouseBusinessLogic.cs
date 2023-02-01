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

using Org.Eclipse.TractusX.Portal.Backend.Clearinghouse.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Clearinghouse.Library.BusinessLogic;

public class ClearinghouseBusinessLogic : IClearinghouseBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly IClearinghouseService _clearinghouseService;

    public ClearinghouseBusinessLogic(IPortalRepositories portalRepositories, IClearinghouseService clearinghouseService)
    {
        _portalRepositories = portalRepositories;
        _clearinghouseService = clearinghouseService;
    }

    /// <inheritdoc />
    public async Task ProcessClearinghouseResponseAsync(string bpn, ClearinghouseResponseData data, CancellationToken cancellationToken)
    {
        var result = await _portalRepositories.GetInstance<IApplicationRepository>().GetSubmittedIdAndClearinghouseChecklistStatusByBpn(bpn).ConfigureAwait(false);
        if (result == default)
        {
            throw new NotFoundException($"No companyApplication for BPN {bpn} is not in status SUBMITTED");
        }

        if (result.StatusId != ApplicationChecklistEntryStatusId.IN_PROGRESS)
        {
            throw new ConflictException($"Checklist Item {ApplicationChecklistEntryTypeId.CLEARING_HOUSE} is not in status {ApplicationChecklistEntryStatusId.IN_PROGRESS}");
        }
        
        _portalRepositories.GetInstance<IApplicationChecklistRepository>()
            .AttachAndModifyApplicationChecklist(result.ApplicationId,
                ApplicationChecklistEntryTypeId.CLEARING_HOUSE,
                item =>
                {
                    item.ApplicationChecklistEntryStatusId = data.Status == ClearinghouseResponseStatus.DECLINE ? ApplicationChecklistEntryStatusId.FAILED : ApplicationChecklistEntryStatusId.DONE;
                    item.Comment = data.Message;
                });
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    public async Task TriggerCompanyDataPost(Guid applicationId, string decentralizedIdentifier, CancellationToken cancellationToken)
    {
        var data = await _portalRepositories.GetInstance<IApplicationRepository>()
            .GetClearinghouseDataForApplicationId(applicationId).ConfigureAwait(false);
        if (data is null)
        {
            throw new ConflictException($"Application {applicationId} does not exists.");
        }

        if (data.ApplicationStatusId != CompanyApplicationStatusId.SUBMITTED)
        {
            throw new ConflictException($"CompanyApplication {applicationId} is not in status SUBMITTED");
        }

        if (string.IsNullOrWhiteSpace(data.ParticipantDetails.Bpn))
        {
            throw new ConflictException("BusinessPartnerNumber is null");
        }

        var transferData = new ClearinghouseTransferData(
            data.ParticipantDetails,
            new IdentityDetails(decentralizedIdentifier, data.UniqueIds));

        await _clearinghouseService.TriggerCompanyDataPost(transferData, cancellationToken).ConfigureAwait(false);
    }
}
