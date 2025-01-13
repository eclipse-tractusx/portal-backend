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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.ApplicationChecklist.Library;

namespace Org.Eclipse.TractusX.Portal.Backend.Clearinghouse.Library.BusinessLogic;

public class ClearinghouseBusinessLogic(
    IPortalRepositories portalRepositories,
    IClearinghouseService clearinghouseService,
    IApplicationChecklistService checklistService,
    IDateTimeProvider dateTimeProvider,
    IOptions<ClearinghouseSettings> options)
    : IClearinghouseBusinessLogic
{
    private readonly ClearinghouseSettings _settings = options.Value;

    public async Task<IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult> HandleClearinghouse(IApplicationChecklistService.WorkerChecklistProcessStepData context, CancellationToken cancellationToken)
    {
        var validationMode = context.ProcessStepTypeId switch
        {
            ProcessStepTypeId.START_OVERRIDE_CLEARING_HOUSE => ValidationModes.IDENTIFIER,
            ProcessStepTypeId.START_CLEARING_HOUSE => ValidationModes.LEGAL_NAME,
            _ => throw new UnexpectedConditionException($"HandleClearingHouse called for unexpected processStepTypeId {context.ProcessStepTypeId}. Expected {ProcessStepTypeId.START_CLEARING_HOUSE} or {ProcessStepTypeId.START_OVERRIDE_CLEARING_HOUSE}")
        };

        await TriggerCompanyDataPost(context.ApplicationId, validationMode, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);

        return new IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult(
            ProcessStepStatusId.DONE,
            entry => entry.ApplicationChecklistEntryStatusId = ApplicationChecklistEntryStatusId.IN_PROGRESS,
            [ProcessStepTypeId.AWAIT_CLEARING_HOUSE_RESPONSE],
            null,
            true,
            null);
    }

    private async Task TriggerCompanyDataPost(Guid applicationId, string validationMode, CancellationToken cancellationToken)
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

        if (string.IsNullOrWhiteSpace(data.Bpn))
        {
            throw new ConflictException("BusinessPartnerNumber is null");
        }

        var headers = new List<KeyValuePair<string, string>>
        {
            new("Business-Partner-Number", data.Bpn)
        }.AsEnumerable();

        var transferData = new ClearinghouseTransferData(
            data.LegalEntity,
            validationMode,
            new CallBack(_settings.CallbackUrl, headers)
        );

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

        // Company data is valid if any one of the provided identifiers was responded valid from CH
        var validData = data.ValidationUnits.FirstOrDefault(s => s.Status == ClearinghouseResponseStatus.VALID);
        var isInvalid = validData == null;
        checklistService.FinalizeChecklistEntryAndProcessSteps(
            context,
            null,
            item =>
            {
                item.ApplicationChecklistEntryStatusId = isInvalid
                    ? ApplicationChecklistEntryStatusId.FAILED
                    : ApplicationChecklistEntryStatusId.DONE;

                // There is not "Message" param available in the response in case of VALID so, thats why saving ClearinghouseResponseStatus param into the Comments in case of VALID only.
                item.Comment = isInvalid
                                ? data.ValidationUnits.FirstOrDefault(s => s.Status != ClearinghouseResponseStatus.VALID)!.Message
                                : validData!.Status.ToString();
            },
            isInvalid
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
