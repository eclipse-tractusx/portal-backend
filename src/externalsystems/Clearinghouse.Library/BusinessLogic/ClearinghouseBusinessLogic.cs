/********************************************************************************
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Clearinghouse.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Custodian.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Async;
using Org.Eclipse.TractusX.Portal.Backend.Framework.DateTimeProvider;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.ApplicationChecklist.Library;

namespace Org.Eclipse.TractusX.Portal.Backend.Clearinghouse.Library.BusinessLogic;

public class ClearinghouseBusinessLogic(
    IPortalRepositories portalRepositories,
    IClearinghouseService clearinghouseService,
    ICustodianBusinessLogic custodianBusinessLogic,
    IApplicationChecklistService checklistService,
    IDateTimeProvider dateTimeProvider,
    IOptions<ClearinghouseSettings> options)
    : IClearinghouseBusinessLogic
{
    private readonly ClearinghouseSettings _settings = options.Value;

    public async Task<IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult> HandleClearinghouse(IApplicationChecklistService.WorkerChecklistProcessStepData context, CancellationToken cancellationToken)
    {
        var overwrite = context.ProcessStepTypeId switch
        {
            ProcessStepTypeId.START_OVERRIDE_CLEARING_HOUSE => true,
            ProcessStepTypeId.START_CLEARING_HOUSE => false,
            _ => throw new UnexpectedConditionException($"HandleClearingHouse called for unexpected processStepTypeId {context.ProcessStepTypeId}. Expected {ProcessStepTypeId.START_CLEARING_HOUSE} or {ProcessStepTypeId.START_OVERRIDE_CLEARING_HOUSE}")
        };

        string companyDid;
        if (_settings.UseDimWallet)
        {
            var (exists, did) = await portalRepositories.GetInstance<IApplicationRepository>()
                .GetDidForApplicationId(context.ApplicationId).ConfigureAwait(ConfigureAwaitOptions.None);
            if (!exists || string.IsNullOrWhiteSpace(did))
            {
                throw new ConflictException($"Did must be set for Application {context.ApplicationId}");
            }

            companyDid = did;
        }
        else
        {
            var walletData = await custodianBusinessLogic.GetWalletByBpnAsync(context.ApplicationId, cancellationToken);
            if (walletData == null || string.IsNullOrEmpty(walletData.Did))
            {
                throw new ConflictException($"Decentralized Identifier for application {context.ApplicationId} is not set");
            }

            companyDid = walletData.Did;
        }

        await TriggerCompanyDataPost(context.ApplicationId, companyDid, overwrite, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);

        return new IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult(
            ProcessStepStatusId.DONE,
            entry => entry.ApplicationChecklistEntryStatusId = ApplicationChecklistEntryStatusId.IN_PROGRESS,
            [ProcessStepTypeId.AWAIT_CLEARING_HOUSE_RESPONSE],
            null,
            true,
            null);
    }

    private async Task TriggerCompanyDataPost(Guid applicationId, string decentralizedIdentifier, bool overwrite, CancellationToken cancellationToken)
    {
        var data = await portalRepositories.GetInstance<IApplicationRepository>()
            .GetClearinghouseDataForApplicationId(applicationId).ConfigureAwait(ConfigureAwaitOptions.None);
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

        await clearinghouseService.TriggerCompanyDataPost(transferData, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public async Task ProcessEndClearinghouse(Guid applicationId, ClearinghouseResponseData data, CancellationToken cancellationToken)
    {
        var context = await checklistService
            .VerifyChecklistEntryAndProcessSteps(
                applicationId,
                ApplicationChecklistEntryTypeId.CLEARING_HOUSE,
                [ApplicationChecklistEntryStatusId.IN_PROGRESS],
                ProcessStepTypeId.AWAIT_CLEARING_HOUSE_RESPONSE,
                processStepTypeIds: [ProcessStepTypeId.START_SELF_DESCRIPTION_LP])
            .ConfigureAwait(ConfigureAwaitOptions.None);

        var declined = data.Status == ClearinghouseResponseStatus.DECLINE;

        checklistService.FinalizeChecklistEntryAndProcessSteps(
            context,
            null,
            item =>
            {
                item.ApplicationChecklistEntryStatusId = declined
                    ? ApplicationChecklistEntryStatusId.FAILED
                    : ApplicationChecklistEntryStatusId.DONE;
                item.Comment = data.Message;
            },
            declined
                ? [ProcessStepTypeId.MANUAL_TRIGGER_OVERRIDE_CLEARING_HOUSE]
                : [ProcessStepTypeId.START_SELF_DESCRIPTION_LP]);
    }

    public async Task CheckEndClearinghouseProcesses(CancellationToken cancellationToken)
    {
        var applicationIds = await portalRepositories.GetInstance<IApplicationChecklistRepository>()
            .GetApplicationsForClearinghouseRetrigger(dateTimeProvider.OffsetNow.AddDays(-_settings.RetriggerEndClearinghouseIntervalInDays))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (applicationIds.Count == 0)
            return;

        await foreach (var context in applicationIds
                                        .Select(applicationId =>
                                            checklistService.VerifyChecklistEntryAndProcessSteps(
                                                applicationId,
                                                ApplicationChecklistEntryTypeId.CLEARING_HOUSE,
                                                [ApplicationChecklistEntryStatusId.IN_PROGRESS],
                                                ProcessStepTypeId.AWAIT_CLEARING_HOUSE_RESPONSE))
                                        .TasksToAsyncEnumerable().WithCancellation(cancellationToken))
        {
            checklistService.FinalizeChecklistEntryAndProcessSteps(
                context,
                null,
                item =>
                {
                    item.ApplicationChecklistEntryStatusId = ApplicationChecklistEntryStatusId.TO_DO;
                    item.Comment = "Reset to retrigger clearinghouse";
                },
                [ProcessStepTypeId.START_OVERRIDE_CLEARING_HOUSE]);
        }

        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }
}
