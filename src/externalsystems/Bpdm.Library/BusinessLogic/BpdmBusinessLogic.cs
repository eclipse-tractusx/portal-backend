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
using Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.ApplicationChecklist.Library;

namespace Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library.BusinessLogic;

public class BpdmBusinessLogic(
    IPortalRepositories portalRepositories,
    IBpdmService bpdmService,
    IOptions<BpdmServiceSettings> options)
    : IBpdmBusinessLogic
{
    private readonly BpdmServiceSettings _settings = options.Value;

    public async Task<IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult> PushLegalEntity(IApplicationChecklistService.WorkerChecklistProcessStepData context, CancellationToken cancellationToken)
    {
        var result = await portalRepositories.GetInstance<IApplicationRepository>().GetBpdmDataForApplicationAsync(context.ApplicationId).ConfigureAwait(ConfigureAwaitOptions.None);

        if (result == default)
        {
            throw new NotFoundException($"Application {context.ApplicationId} does not exists.");
        }

        if (result.BpdmData == null)
        {
            throw new UnexpectedConditionException($"BpdmData should never be null here");
        }

        var data = result.BpdmData;
        if (!string.IsNullOrWhiteSpace(data.BusinessPartnerNumber))
        {
            throw new ConflictException($"BusinessPartnerNumber is already set");
        }

        if (string.IsNullOrWhiteSpace(data.Alpha2Code))
        {
            throw new ConflictException("Alpha2Code must not be empty");
        }

        if (string.IsNullOrWhiteSpace(data.City))
        {
            throw new ConflictException("City must not be empty");
        }

        if (string.IsNullOrWhiteSpace(data.StreetName))
        {
            throw new ConflictException("StreetName must not be empty");
        }

        var bpdmTransferData = new BpdmTransferData(
            context.ApplicationId.ToString(),
            data.CompanyName,
            data.ShortName,
            data.Alpha2Code,
            data.ZipCode,
            data.City,
            data.StreetName,
            data.StreetNumber,
            data.Region,
            data.Identifiers);

        await bpdmService.PutInputLegalEntity(bpdmTransferData, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        if (!_settings.StartSharingStateAsReady)
        {
            await bpdmService.SetSharingStateToReady(context.ApplicationId.ToString(), cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        }

        return new IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult(
            ProcessStepStatusId.DONE,
            entry => entry.ApplicationChecklistEntryStatusId = ApplicationChecklistEntryStatusId.IN_PROGRESS,
            new[] { ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PULL },
            null,
            true,
            null);
    }

    public async Task<IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult> HandlePullLegalEntity(IApplicationChecklistService.WorkerChecklistProcessStepData context, CancellationToken cancellationToken)
    {
        var result = await portalRepositories.GetInstance<IApplicationRepository>()
            .GetBpdmDataForApplicationAsync(context.ApplicationId).ConfigureAwait(ConfigureAwaitOptions.None);

        if (result == default)
        {
            throw new UnexpectedConditionException($"CompanyApplication {context.ApplicationId} does not exist");
        }

        var sharingState = await bpdmService.GetSharingState(context.ApplicationId, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        if (sharingState.SharingProcessStarted == null)
        {
            return new IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult(ProcessStepStatusId.TODO, null, null, null, false, "SharingProcessStarted was not set");
        }

        return sharingState.SharingStateType switch
        {
            BpdmSharingStateType.Success =>
                await HandlePullLegalEntityInternal(context, result.CompanyId, result.BpdmData, cancellationToken),
            BpdmSharingStateType.Error =>
                throw new ServiceException($"ErrorCode: {sharingState.SharingErrorCode}, ErrorMessage: {sharingState.SharingErrorMessage}"),
            _ => new IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult(ProcessStepStatusId.TODO, null, null, null, false, null)
        };
    }

    private async Task<IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult> HandlePullLegalEntityInternal(
        IApplicationChecklistService.WorkerChecklistProcessStepData context,
        Guid companyId,
        BpdmData data,
        CancellationToken cancellationToken)
    {
        var legalEntity = await bpdmService.FetchInputLegalEntity(context.ApplicationId.ToString(), cancellationToken)
            .ConfigureAwait(ConfigureAwaitOptions.None);

        if (string.IsNullOrEmpty(legalEntity.LegalEntity?.Bpnl))
        {
            return new IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult(ProcessStepStatusId.TODO, null, null, null, false, null);
        }

        portalRepositories.GetInstance<ICompanyRepository>().AttachAndModifyCompany(
            companyId,
            company =>
            {
                company.BusinessPartnerNumber = data.BusinessPartnerNumber;
            },
            company =>
            {
                company.BusinessPartnerNumber = legalEntity.LegalEntity?.Bpnl;
            });

        var registrationValidationFailed = context.Checklist[ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION] == ApplicationChecklistEntryStatusId.FAILED;

        return new IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult(
            ProcessStepStatusId.DONE,
            entry => entry.ApplicationChecklistEntryStatusId = ApplicationChecklistEntryStatusId.DONE,
            registrationValidationFailed
                ? null
                : new[] { CreateWalletStep() },
            new[] { ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_MANUAL },
            true,
            null);
    }

    private ProcessStepTypeId CreateWalletStep() => _settings.UseDimWallet ? ProcessStepTypeId.CREATE_DIM_WALLET : ProcessStepTypeId.CREATE_IDENTITY_WALLET;
}
