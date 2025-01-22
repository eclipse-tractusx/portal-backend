/********************************************************************************
 * Copyright (c) 2024 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Encryption;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.IssuerComponent.Library.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.IssuerComponent.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.IssuerComponent.Library.Service;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.ApplicationChecklist.Library;

namespace Org.Eclipse.TractusX.Portal.Backend.IssuerComponent.Library.BusinessLogic;

public class IssuerComponentBusinessLogic(
    IPortalRepositories repositories,
    IIssuerComponentService service,
    IApplicationChecklistService checklistService,
    IOptions<IssuerComponentSettings> options)
    : IIssuerComponentBusinessLogic
{
    private readonly IssuerComponentSettings _settings = options.Value;

    public async Task<IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult> CreateBpnlCredential(IApplicationChecklistService.WorkerChecklistProcessStepData context, CancellationToken cancellationToken)
    {
        var applicationId = context.ApplicationId;
        var (exists, holder, businessPartnerNumber, walletInformation) = await repositories.GetInstance<IApplicationRepository>().GetBpnlCredentialIformationByApplicationId(applicationId).ConfigureAwait(false);
        if (!exists)
        {
            throw new NotFoundException($"CompanyApplication {applicationId} does not exist");
        }

        if (holder is null)
        {
            throw new ConflictException("The holder must be set");
        }

        if (businessPartnerNumber is null)
        {
            throw new ConflictException("The bpn must be set");
        }

        if (walletInformation is null)
        {
            throw new ConflictException("The wallet information must be set");
        }

        var cryptoConfig = _settings.EncryptionConfigs.SingleOrDefault(x => x.Index == walletInformation.EncryptionMode) ?? throw new ConfigurationException($"EncryptionModeIndex {walletInformation.EncryptionMode} is not configured");
        var secret = CryptoHelper.Decrypt(walletInformation.ClientSecret, walletInformation.InitializationVector, Convert.FromHexString(cryptoConfig.EncryptionKey), cryptoConfig.CipherMode, cryptoConfig.PaddingMode);

        var callbackUrl = $"{_settings.CallbackBaseUrl}/api/administration/registration/issuer/bpncredential";
        var data = new CreateBpnCredentialRequest(holder, businessPartnerNumber, new TechnicalUserDetails(walletInformation.WalletUrl, walletInformation.ClientId, secret), callbackUrl);
        await service.CreateBpnlCredential(data, cancellationToken).ConfigureAwait(false);
        return new IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult(
            ProcessStepStatusId.DONE,
            checklist =>
            {
                checklist.ApplicationChecklistEntryStatusId = ApplicationChecklistEntryStatusId.IN_PROGRESS;
            },
            [ProcessStepTypeId.AWAIT_BPN_CREDENTIAL_RESPONSE],
            null,
            true,
            null);
    }

    public async Task StoreBpnlCredentialResponse(Guid applicationId, IssuerResponseData data)
    {
        var context = await checklistService
            .VerifyChecklistEntryAndProcessSteps(
                applicationId,
                ApplicationChecklistEntryTypeId.BPNL_CREDENTIAL,
                [ApplicationChecklistEntryStatusId.IN_PROGRESS],
                ProcessStepTypeId.AWAIT_BPN_CREDENTIAL_RESPONSE,
                processStepTypeIds: [ProcessStepTypeId.REQUEST_MEMBERSHIP_CREDENTIAL])
            .ConfigureAwait(false);

        checklistService.FinalizeChecklistEntryAndProcessSteps(
            context,
            item => item.ApplicationChecklistEntryStatusId = ApplicationChecklistEntryStatusId.IN_PROGRESS,
            item =>
            {
                item.ApplicationChecklistEntryStatusId = data.Status == IssuerResponseStatus.UNSUCCESSFUL
                    ? ApplicationChecklistEntryStatusId.FAILED
                    : ApplicationChecklistEntryStatusId.DONE;
                item.Comment = data.Message;
            },
            data.Status == IssuerResponseStatus.SUCCESSFUL
                ? [ProcessStepTypeId.REQUEST_MEMBERSHIP_CREDENTIAL]
                : null);
    }

    public async Task<IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult> CreateMembershipCredential(IApplicationChecklistService.WorkerChecklistProcessStepData context, CancellationToken cancellationToken)
    {
        var applicationId = context.ApplicationId;
        var (exists, holder, businessPartnerNumber, walletInformation) = await repositories.GetInstance<IApplicationRepository>().GetBpnlCredentialIformationByApplicationId(applicationId).ConfigureAwait(false);
        if (!exists)
        {
            throw new NotFoundException($"CompanyApplication {applicationId} does not exist");
        }

        if (holder is null)
        {
            throw new ConflictException("The holder must be set");
        }

        if (businessPartnerNumber is null)
        {
            throw new ConflictException("The bpn must be set");
        }

        if (walletInformation is null)
        {
            throw new ConflictException("The wallet information must be set");
        }

        var cryptoConfig = _settings.EncryptionConfigs.SingleOrDefault(x => x.Index == walletInformation.EncryptionMode) ?? throw new ConfigurationException($"EncryptionModeIndex {walletInformation.EncryptionMode} is not configured");
        var secret = CryptoHelper.Decrypt(walletInformation.ClientSecret, walletInformation.InitializationVector, Convert.FromHexString(cryptoConfig.EncryptionKey), cryptoConfig.CipherMode, cryptoConfig.PaddingMode);

        var callbackUrl = $"{_settings.CallbackBaseUrl}/api/administration/registration/issuer/membershipcredential";
        var data = new CreateMembershipCredentialRequest(holder, businessPartnerNumber, "catena-x", new TechnicalUserDetails(walletInformation.WalletUrl, walletInformation.ClientId, secret), callbackUrl);
        await service.CreateMembershipCredential(data, cancellationToken).ConfigureAwait(false);
        return new IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult(
            ProcessStepStatusId.DONE,
            checklist =>
            {
                checklist.ApplicationChecklistEntryStatusId = ApplicationChecklistEntryStatusId.IN_PROGRESS;
            },
            [ProcessStepTypeId.AWAIT_MEMBERSHIP_CREDENTIAL_RESPONSE],
            null,
            true,
            null);
    }

    public async Task StoreMembershipCredentialResponse(Guid applicationId, IssuerResponseData data)
    {
        var context = await checklistService
            .VerifyChecklistEntryAndProcessSteps(
                applicationId,
                ApplicationChecklistEntryTypeId.MEMBERSHIP_CREDENTIAL,
                [ApplicationChecklistEntryStatusId.IN_PROGRESS],
                ProcessStepTypeId.AWAIT_MEMBERSHIP_CREDENTIAL_RESPONSE,
                processStepTypeIds: [ProcessStepTypeId.START_CLEARING_HOUSE])
            .ConfigureAwait(false);

        checklistService.FinalizeChecklistEntryAndProcessSteps(
            context,
            item => item.ApplicationChecklistEntryStatusId = ApplicationChecklistEntryStatusId.IN_PROGRESS,
            item =>
            {
                item.ApplicationChecklistEntryStatusId = data.Status == IssuerResponseStatus.UNSUCCESSFUL
                    ? ApplicationChecklistEntryStatusId.FAILED
                    : ApplicationChecklistEntryStatusId.DONE;
                item.Comment = data.Message;
            },
            data.Status == IssuerResponseStatus.SUCCESSFUL
                ? [ProcessStepTypeId.START_CLEARING_HOUSE]
                : null);
    }

    public async Task<Guid> CreateFrameworkCredentialData(Guid useCaseFrameworkVersionId, string frameworkId, Guid identityId, string token, CancellationToken cancellationToken)
    {
        var (holder, businessPartnerNumber, walletInformation) = await repositories.GetInstance<ICompanyRepository>().GetWalletData(identityId).ConfigureAwait(false);
        if (holder is null)
        {
            throw new ConflictException("The holder must be set");
        }

        if (businessPartnerNumber is null)
        {
            throw new ConflictException("The bpn must be set");
        }

        if (walletInformation is null)
        {
            throw new ConflictException("The wallet information must be set");
        }

        var cryptoConfig = _settings.EncryptionConfigs.SingleOrDefault(x => x.Index == walletInformation.EncryptionMode) ?? throw new ConfigurationException($"EncryptionModeIndex {walletInformation.EncryptionMode} is not configured");
        var secret = CryptoHelper.Decrypt(walletInformation.ClientSecret, walletInformation.InitializationVector, Convert.FromHexString(cryptoConfig.EncryptionKey), cryptoConfig.CipherMode, cryptoConfig.PaddingMode);

        var data = new CreateFrameworkCredentialRequest(holder, businessPartnerNumber, frameworkId, useCaseFrameworkVersionId, new TechnicalUserDetails(walletInformation.WalletUrl, walletInformation.ClientId, secret), null);
        return await service.CreateFrameworkCredential(data, token, cancellationToken).ConfigureAwait(false);
    }
}
