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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Checklist.Library;
using Org.Eclipse.TractusX.Portal.Backend.Clearinghouse.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Custodian.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

namespace Org.Eclipse.TractusX.Portal.Backend.Clearinghouse.Library.BusinessLogic;

public class ClearinghouseBusinessLogic : IClearinghouseBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly IClearinghouseService _clearinghouseService;
    private readonly ICustodianBusinessLogic _custodianBusinessLogic;
    private readonly IChecklistService _checklistService;
    private readonly ClearinghouseSettings _settings;

    public ClearinghouseBusinessLogic(
        IPortalRepositories portalRepositories,
        IClearinghouseService clearinghouseService,
        ICustodianBusinessLogic custodianBusinessLogic,
        IChecklistService checklistService,
        IOptions<ClearinghouseSettings> options)
    {
        _portalRepositories = portalRepositories;
        _clearinghouseService = clearinghouseService;
        _custodianBusinessLogic = custodianBusinessLogic;
        _checklistService = checklistService;
        _settings = options.Value;
    }

    public async Task<(Action<ApplicationChecklistEntry>?,IEnumerable<ProcessStepTypeId>?,bool)> HandleClearinghouse(IChecklistService.WorkerChecklistProcessStepData context, CancellationToken cancellationToken)
    {
        if (context.ProcessStepTypeId is not ProcessStepTypeId.START_CLEARING_HOUSE and not ProcessStepTypeId.START_OVERRIDE_CLEARING_HOUSE)
        {
            throw new UnexpectedConditionException($"HandleClearingHouse called for unexpected processStepTypeId {context.ProcessStepTypeId}. Expected {ProcessStepTypeId.START_CLEARING_HOUSE} or {ProcessStepTypeId.START_OVERRIDE_CLEARING_HOUSE}");
        }
        var walletData = await _custodianBusinessLogic.GetWalletByBpnAsync(context.ApplicationId, cancellationToken);
        if (walletData == null || string.IsNullOrEmpty(walletData.Did))
        {
            throw new ConflictException($"Decentralized Identifier for application {context.ApplicationId} is not set");
        }
        
        var overwrite = context.ProcessStepTypeId == ProcessStepTypeId.START_OVERRIDE_CLEARING_HOUSE;
        await TriggerCompanyDataPost(context.ApplicationId, walletData.Did, overwrite, cancellationToken).ConfigureAwait(false);

        return (
            entry => entry.ApplicationChecklistEntryStatusId = ApplicationChecklistEntryStatusId.IN_PROGRESS,
            new [] { ProcessStepTypeId.END_CLEARING_HOUSE },
            true);
    }

    private async Task TriggerCompanyDataPost(Guid applicationId, string decentralizedIdentifier, bool overwrite, CancellationToken cancellationToken)
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
            new IdentityDetails(decentralizedIdentifier, data.UniqueIds),
            _settings.CallbackUrl,
            overwrite);

        await _clearinghouseService.TriggerCompanyDataPost(transferData, cancellationToken).ConfigureAwait(false);
    }

    public async Task ProcessEndClearinghouse(Guid applicationId, ClearinghouseResponseData data, CancellationToken cancellationToken)
    {
        var context = await _checklistService
            .VerifyChecklistEntryAndProcessSteps(
                applicationId,
                ApplicationChecklistEntryTypeId.CLEARING_HOUSE,
                new [] { ApplicationChecklistEntryStatusId.IN_PROGRESS },
                ProcessStepTypeId.END_CLEARING_HOUSE,
                processStepTypeIds: new [] { ProcessStepTypeId.START_SELF_DESCRIPTION_LP })
            .ConfigureAwait(false);

        var declined = data.Status == ClearinghouseResponseStatus.DECLINE;

        _checklistService.FinalizeChecklistEntryAndProcessSteps(
            context,
            item =>
            {
                item.ApplicationChecklistEntryStatusId = declined
                    ? ApplicationChecklistEntryStatusId.FAILED
                    : ApplicationChecklistEntryStatusId.DONE;
                item.Comment = data.Message;
            },
            declined
                ? new [] { ProcessStepTypeId.TRIGGER_OVERRIDE_CLEARING_HOUSE }
                : new [] { ProcessStepTypeId.START_SELF_DESCRIPTION_LP });
    }
}
