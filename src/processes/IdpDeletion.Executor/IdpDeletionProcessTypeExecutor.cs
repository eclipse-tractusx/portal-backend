/********************************************************************************
 * Copyright (c) 2021, 2023 BMW Group AG
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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.Worker.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.IdpDeletion.Executor;

public class IdpDeletionProcessTypeExecutor : IProcessTypeExecutor
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly IProvisioningManager _provisioningManager;

    private readonly IEnumerable<ProcessStepTypeId> _executableProcessSteps = ImmutableArray.Create(
        ProcessStepTypeId.TRIGGER_DELETE_IDP_SHARED_REALM,
        ProcessStepTypeId.TRIGGER_DELETE_IDP_SHARED_SERVICEACCOUNT,
        ProcessStepTypeId.TRIGGER_DELETE_IDENTITY_LINKED_USERS,
        ProcessStepTypeId.TRIGGER_DELETE_CENTRAL_IDENTITY_PROVIDER,
        ProcessStepTypeId.TRIGGER_DELETE_IDENTITY_PROVIDER,
        ProcessStepTypeId.RETRIGGER_DELETE_IDP_SHARED_REALM,
        ProcessStepTypeId.RETRIGGER_DELETE_IDP_SHARED_SERVICEACCOUNT,
        ProcessStepTypeId.RETRIGGER_DELETE_IDENTITY_LINKED_USERS,
        ProcessStepTypeId.RETRIGGER_DELETE_CENTRAL_IDENTITY_PROVIDER,
        ProcessStepTypeId.RETRIGGER_DELETE_IDENTITY_PROVIDER);

    private IdpData _idpData = null!;
    private Guid _companyId;
    private string? _iamuserId;

    public IdpDeletionProcessTypeExecutor(IPortalRepositories portalRepositories, IProvisioningManager provisioningManager)
    {
        _portalRepositories = portalRepositories;
        _provisioningManager = provisioningManager;
    }

    public ProcessTypeId GetProcessTypeId() => ProcessTypeId.IDP_DELETION;
    public bool IsExecutableStepTypeId(ProcessStepTypeId processStepTypeId) => _executableProcessSteps.Contains(processStepTypeId);
    public IEnumerable<ProcessStepTypeId> GetExecutableStepTypeIds() => _executableProcessSteps;
    public ValueTask<bool> IsLockRequested(ProcessStepTypeId processStepTypeId) => new(false);

    public async ValueTask<IProcessTypeExecutor.InitializationResult> InitializeProcess(Guid processId, IEnumerable<ProcessStepTypeId> processStepTypeIds)
    {
        var (idpData, companyId, _companyUserId) = await _portalRepositories.GetInstance<IIdentityProviderRepository>().GetIdentityProviderDataForProcessIdAsync(processId).ConfigureAwait(false);

        if (idpData == null)
        {
            throw new NotFoundException($"process {processId} does not exist or is not associated with an Identity Provider");
        }
        _idpData = idpData;
        _companyId = companyId;
        _iamuserId = await _provisioningManager.GetUserByUserName(_companyUserId.ToString()).ConfigureAwait(false);
        return new IProcessTypeExecutor.InitializationResult(false, null);
    }

    public async ValueTask<IProcessTypeExecutor.StepExecutionResult> ExecuteProcessStep(ProcessStepTypeId processStepTypeId, IEnumerable<ProcessStepTypeId> processStepTypeIds, CancellationToken cancellationToken)
    {
        if (_idpData == null)
        {
            throw new UnexpectedConditionException("IdentityProvider data should never be empty here");
        }

        IEnumerable<ProcessStepTypeId>? nextStepTypeIds;
        ProcessStepStatusId stepStatusId;
        bool modified;
        string? processMessage;

        try
        {
            (nextStepTypeIds, stepStatusId, modified, processMessage) = processStepTypeId switch
            {
                ProcessStepTypeId.TRIGGER_DELETE_IDP_SHARED_REALM => await _provisioningManager.TriggerDeleteSharedRealmAsync(_idpData.IamAlias).ConfigureAwait(false),
                ProcessStepTypeId.TRIGGER_DELETE_IDP_SHARED_SERVICEACCOUNT => await _provisioningManager.TriggerDeleteIdpSharedServiceAccount(_idpData.IamAlias).ConfigureAwait(false),
                ProcessStepTypeId.TRIGGER_DELETE_IDENTITY_LINKED_USERS => await TriggerDeleteLinkedCentralIdentityProvider(_iamuserId!).ConfigureAwait(false),
                ProcessStepTypeId.TRIGGER_DELETE_CENTRAL_IDENTITY_PROVIDER => TriggerDeleteCentralIdentityProvider(_idpData),
                ProcessStepTypeId.TRIGGER_DELETE_IDENTITY_PROVIDER => TriggerDeleteIdentityProvider(_idpData),
                _ => (null, ProcessStepStatusId.TODO, false, null)
            };
        }
        catch (Exception ex) when (ex is not SystemException)
        {
            (stepStatusId, processMessage, nextStepTypeIds) = ProcessError(ex, processStepTypeId);
            modified = true;
        }
        return new IProcessTypeExecutor.StepExecutionResult(modified, stepStatusId, nextStepTypeIds, null, processMessage);
    }

    private static (ProcessStepStatusId StatusId, string? ProcessMessage, IEnumerable<ProcessStepTypeId>? nextSteps) ProcessError(Exception ex, ProcessStepTypeId processStepTypeId)
    {
        return ex switch
        {
            ServiceException { IsRecoverable: true } => (ProcessStepStatusId.TODO, ex.Message, null),
            _ => (ProcessStepStatusId.FAILED, ex.Message, processStepTypeId.GetIdpDeletionRetriggerStep())
        };
    }
    private async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> TriggerDeleteLinkedCentralIdentityProvider(string userId)
    {
        var identityProviderRepository = _portalRepositories.GetInstance<IIdentityProviderRepository>();
        var result = await _provisioningManager.GetProviderUserLinkDataForCentralUserIdAsync(userId).ToListAsync().ConfigureAwait(false);
        if (result.Count() > 1)
        {
            var identityProviderIds = await identityProviderRepository.GetIdentityproviderIdAsync(result.Select(x => x.Alias)).ToListAsync().ConfigureAwait(false);
            foreach (var ipId in identityProviderIds)
            {
                identityProviderRepository.DeleteCompanyIdentityProvider(_companyId, ipId);
            }
            return (new[] { ProcessStepTypeId.TRIGGER_DELETE_IDENTITY_PROVIDER }, ProcessStepStatusId.DONE, false, null);
        }
        else
        {
            return (new[] { ProcessStepTypeId.TRIGGER_DELETE_CENTRAL_IDENTITY_PROVIDER }, ProcessStepStatusId.DONE, false, null);
        }
    }
    private (IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage) TriggerDeleteCentralIdentityProvider(IdpData idpData)
    {
        var identityProviderRepository = _portalRepositories.GetInstance<IIdentityProviderRepository>();
        identityProviderRepository.DeleteCompanyIdentityProvider(_companyId, idpData.IdentityProviderId);
        return (new[] { ProcessStepTypeId.TRIGGER_DELETE_IDENTITY_PROVIDER }, ProcessStepStatusId.DONE, false, null);
    }
    private (IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage) TriggerDeleteIdentityProvider(IdpData idpData)
    {
        var identityProviderRepository = _portalRepositories.GetInstance<IIdentityProviderRepository>();
        identityProviderRepository.DeleteIamIdentityProvider(idpData.IamAlias);
        identityProviderRepository.DeleteIdentityProvider(idpData.IdentityProviderId);
        return (null, ProcessStepStatusId.DONE, false, null);
    }
}
