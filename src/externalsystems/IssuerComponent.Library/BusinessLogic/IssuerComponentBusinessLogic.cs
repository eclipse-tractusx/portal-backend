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
using Org.Eclipse.TractusX.Portal.Backend.IssuerComponent.Library.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.IssuerComponent.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.IssuerComponent.Library.Service;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.ApplicationChecklist.Library;

namespace Org.Eclipse.TractusX.Portal.Backend.IssuerComponent.Library.BusinessLogic;

public class IssuerComponentBusinessLogic : IIssuerComponentBusinessLogic
{
    private readonly IPortalRepositories _repositories;
    private readonly IIssuerComponentService _service;
    private readonly IApplicationChecklistService _checklistService;
    private readonly IssuerComponentSettings _settings;

    public IssuerComponentBusinessLogic(
        IPortalRepositories repositories,
        IIssuerComponentService service,
        IApplicationChecklistService checklistService,
        IOptions<IssuerComponentSettings> options)
    {
        _repositories = repositories;
        _service = service;
        _checklistService = checklistService;
        _settings = options.Value;
    }

    public async Task<IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult> CreateBpnlCredential(IApplicationChecklistService.WorkerChecklistProcessStepData context, CancellationToken cancellationToken)
    {
        var applicationId = context.ApplicationId;
        var (exists, holder, businessPartnerNumber, walletInformation) = await _repositories.GetInstance<IApplicationRepository>().GetBpnlCredentialIformationByApplicationId(applicationId).ConfigureAwait(false);
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
        await _service.CreateBpnlCredential(data, cancellationToken).ConfigureAwait(false);
        return new IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult(
            ProcessStepStatusId.DONE,
            checklist =>
            {
                checklist.ApplicationChecklistEntryStatusId = ApplicationChecklistEntryStatusId.IN_PROGRESS;
            },
            new[] { ProcessStepTypeId.STORED_BPN_CREDENTIAL },
            null,
            true,
            null);
    }

    public async Task StoreBpnlCredential(Guid applicationId, IssuerResponseData data)
    {
        var context = await _checklistService
            .VerifyChecklistEntryAndProcessSteps(
                applicationId,
                ApplicationChecklistEntryTypeId.BPNL_CREDENTIAL,
                new[] { ApplicationChecklistEntryStatusId.IN_PROGRESS },
                ProcessStepTypeId.STORED_BPN_CREDENTIAL,
                processStepTypeIds: new[] { ProcessStepTypeId.REQUEST_MEMBERSHIP_CREDENTIAL })
            .ConfigureAwait(false);

        _checklistService.FinalizeChecklistEntryAndProcessSteps(
            context,
            null,
            item =>
            {
                item.ApplicationChecklistEntryStatusId = data.Status == IssuerResponseStatus.UNSUCCESSFUL
                    ? ApplicationChecklistEntryStatusId.FAILED
                    : ApplicationChecklistEntryStatusId.DONE;
                item.Comment = data.Message;
            },
        data.Status == IssuerResponseStatus.SUCCESSFUL ? new[] { ProcessStepTypeId.REQUEST_MEMBERSHIP_CREDENTIAL } : null);
    }

    public async Task<IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult> CreateMembershipCredential(IApplicationChecklistService.WorkerChecklistProcessStepData context, CancellationToken cancellationToken)
    {
        var applicationId = context.ApplicationId;
        var (exists, holder, businessPartnerNumber, walletInformation) = await _repositories.GetInstance<IApplicationRepository>().GetBpnlCredentialIformationByApplicationId(applicationId).ConfigureAwait(false);
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
        await _service.CreateMembershipCredential(data, cancellationToken).ConfigureAwait(false);
        return new IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult(
            ProcessStepStatusId.DONE,
            checklist =>
            {
                checklist.ApplicationChecklistEntryStatusId = ApplicationChecklistEntryStatusId.IN_PROGRESS;
            },
            new[] { ProcessStepTypeId.STORED_MEMBERSHIP_CREDENTIAL },
            null,
            true,
            null);
    }

    public async Task StoreMembershipCredential(Guid applicationId, IssuerResponseData data)
    {
        var context = await _checklistService
            .VerifyChecklistEntryAndProcessSteps(
                applicationId,
                ApplicationChecklistEntryTypeId.MEMBERSHIP_CREDENTIAL,
                new[] { ApplicationChecklistEntryStatusId.IN_PROGRESS },
                ProcessStepTypeId.STORED_MEMBERSHIP_CREDENTIAL,
                processStepTypeIds: new[] { ProcessStepTypeId.START_CLEARING_HOUSE })
            .ConfigureAwait(false);

        _checklistService.FinalizeChecklistEntryAndProcessSteps(
            context,
            null,
            item =>
            {
                item.ApplicationChecklistEntryStatusId = data.Status == IssuerResponseStatus.UNSUCCESSFUL
                    ? ApplicationChecklistEntryStatusId.FAILED
                    : ApplicationChecklistEntryStatusId.DONE;
                item.Comment = data.Message;
            },
            data.Status == IssuerResponseStatus.SUCCESSFUL
                ? new[] { ProcessStepTypeId.START_CLEARING_HOUSE }
                : null);
    }
}
