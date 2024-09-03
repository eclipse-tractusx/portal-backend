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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Worker.Library;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.IdentityProviderProvisioning.Executor;

public class IdentityProviderProvisioningProcessTypeExecutor(
    IPortalRepositories portalRepositories,
    IProvisioningManager provisioningManager)
    : IProcessTypeExecutor<ProcessTypeId, ProcessStepTypeId>
{
    private static readonly IEnumerable<ProcessStepTypeId> ExecutableProcessSteps =
    [
        ProcessStepTypeId.DELETE_IDP_SHARED_REALM,
        ProcessStepTypeId.DELETE_IDP_SHARED_SERVICEACCOUNT,
        ProcessStepTypeId.DELETE_CENTRAL_IDENTITY_PROVIDER,
        ProcessStepTypeId.DELETE_IDENTITY_PROVIDER,
    ];

    private IdpData? _idpData;

    public ProcessTypeId GetProcessTypeId() => ProcessTypeId.IDENTITYPROVIDER_PROVISIONING;
    public bool IsExecutableStepTypeId(ProcessStepTypeId processStepTypeId) => ExecutableProcessSteps.Contains(processStepTypeId);
    public IEnumerable<ProcessStepTypeId> GetExecutableStepTypeIds() => ExecutableProcessSteps;
    public ValueTask<bool> IsLockRequested(ProcessStepTypeId processStepTypeId) => ValueTask.FromResult(false);

    public async ValueTask<IProcessTypeExecutor<ProcessTypeId, ProcessStepTypeId>.InitializationResult> InitializeProcess(Guid processId, IEnumerable<ProcessStepTypeId> processStepTypeIds)
    {
        _idpData = await portalRepositories.GetInstance<IIdentityProviderRepository>().GetIdentityProviderDataForProcessIdAsync(processId).ConfigureAwait(ConfigureAwaitOptions.None)
            ?? throw new ConflictException($"process {processId} does not exist or is not associated with an Identity Provider");

        return new IProcessTypeExecutor<ProcessTypeId, ProcessStepTypeId>.InitializationResult(false, null);
    }

    public async ValueTask<IProcessTypeExecutor<ProcessTypeId, ProcessStepTypeId>.StepExecutionResult> ExecuteProcessStep(ProcessStepTypeId processStepTypeId, IEnumerable<ProcessStepTypeId> processStepTypeIds, CancellationToken cancellationToken)
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
                ProcessStepTypeId.DELETE_IDP_SHARED_REALM => await DeleteSharedRealmAsync(_idpData).ConfigureAwait(ConfigureAwaitOptions.None),
                ProcessStepTypeId.DELETE_IDP_SHARED_SERVICEACCOUNT => await DeleteIdpSharedServiceAccount(_idpData).ConfigureAwait(ConfigureAwaitOptions.None),
                ProcessStepTypeId.DELETE_CENTRAL_IDENTITY_PROVIDER => await DeleteCentralIdentityProvider(_idpData.IamAlias).ConfigureAwait(ConfigureAwaitOptions.None),
                ProcessStepTypeId.DELETE_IDENTITY_PROVIDER => await DeleteIdentityProvider(_idpData.IdentityProviderId).ConfigureAwait(ConfigureAwaitOptions.None),
                _ => (null, ProcessStepStatusId.TODO, false, null)
            };
        }
        catch (Exception ex) when (ex is not SystemException)
        {
            (stepStatusId, processMessage, nextStepTypeIds) = ProcessError(ex, processStepTypeId);
            modified = true;
        }

        return new IProcessTypeExecutor<ProcessTypeId, ProcessStepTypeId>.StepExecutionResult(modified, stepStatusId, nextStepTypeIds, null, processMessage);
    }

    private static (ProcessStepStatusId StatusId, string? ProcessMessage, IEnumerable<ProcessStepTypeId>? nextSteps) ProcessError(Exception ex, ProcessStepTypeId processStepTypeId)
    {
        return ex switch
        {
            ServiceException { IsRecoverable: true } => (ProcessStepStatusId.TODO, ex.Message, null),
            _ => (ProcessStepStatusId.FAILED, ex.Message, processStepTypeId.GetIdentityProviderProvisioningRetriggerStep())
        };
    }

    private async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> DeleteSharedRealmAsync(IdpData idpData)
    {
        if (idpData.IdentityProviderTypeId != IdentityProviderTypeId.SHARED)
        {
            return ([ProcessStepTypeId.DELETE_CENTRAL_IDENTITY_PROVIDER], ProcessStepStatusId.SKIPPED, false, $"IdentityProvider {idpData.IamAlias} is not a shared idp");
        }

        try
        {
            await provisioningManager.DeleteSharedRealmAsync(idpData.IamAlias).ConfigureAwait(ConfigureAwaitOptions.None);
            return ([ProcessStepTypeId.DELETE_IDP_SHARED_SERVICEACCOUNT], ProcessStepStatusId.DONE, false, null);
        }
        catch (KeycloakEntityNotFoundException)
        {
            return ([ProcessStepTypeId.DELETE_IDP_SHARED_SERVICEACCOUNT], ProcessStepStatusId.SKIPPED, false, $"Shared Idp realm {idpData.IamAlias} not found");
        }
    }

    private async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> DeleteIdpSharedServiceAccount(IdpData idpData)
    {
        if (idpData.IdentityProviderTypeId != IdentityProviderTypeId.SHARED)
        {
            return ([ProcessStepTypeId.DELETE_CENTRAL_IDENTITY_PROVIDER], ProcessStepStatusId.SKIPPED, false, $"IdentityProvider {idpData.IamAlias} is not a shared idp");
        }

        try
        {
            await provisioningManager.DeleteIdpSharedServiceAccount(idpData.IamAlias).ConfigureAwait(ConfigureAwaitOptions.None);
            return ([ProcessStepTypeId.DELETE_CENTRAL_IDENTITY_PROVIDER], ProcessStepStatusId.DONE, false, null);
        }
        catch (KeycloakEntityNotFoundException)
        {
            return ([ProcessStepTypeId.DELETE_CENTRAL_IDENTITY_PROVIDER], ProcessStepStatusId.SKIPPED, false, $"Shared Idp admin-serviceaccount for realm {idpData.IamAlias} not found");
        }
    }

    private async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> DeleteCentralIdentityProvider(string alias)
    {
        try
        {
            await provisioningManager.DeleteCentralIdentityProviderAsync(alias).ConfigureAwait(ConfigureAwaitOptions.None);
            return ([ProcessStepTypeId.DELETE_IDENTITY_PROVIDER], ProcessStepStatusId.DONE, false, null);
        }
        catch (KeycloakEntityNotFoundException)
        {
            return ([ProcessStepTypeId.DELETE_IDENTITY_PROVIDER], ProcessStepStatusId.SKIPPED, false, $"Central IdentityProvider {alias} not found");
        }
    }

    private async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> DeleteIdentityProvider(Guid identityProviderId)
    {
        var identityProviderRepository = portalRepositories.GetInstance<IIdentityProviderRepository>();
        var alias = await identityProviderRepository.GetIamIdentityProviderForIdp(identityProviderId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (alias != null)
        {
            identityProviderRepository.DeleteIamIdentityProvider(alias);
        }

        identityProviderRepository.DeleteIdentityProvider(identityProviderId);
        return (null, ProcessStepStatusId.DONE, true, null);
    }
}
