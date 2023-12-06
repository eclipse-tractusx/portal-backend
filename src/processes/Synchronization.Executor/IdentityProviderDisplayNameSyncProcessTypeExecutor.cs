/********************************************************************************
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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.Worker.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.Synchronization.Executor;

public class IdentityProviderDisplayNameSyncProcessTypeExecutor : IProcessTypeExecutor
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly IProvisioningManager _provisioningManager;

    private static readonly IEnumerable<ProcessStepTypeId> _executableProcessSteps = ImmutableArray.Create(ProcessStepTypeId.SYNCHRONIZE_IDP_DISPLAY_NAME);

    public IdentityProviderDisplayNameSyncProcessTypeExecutor(IPortalRepositories portalRepositories, IProvisioningManager provisioningManager)
    {
        _portalRepositories = portalRepositories;
        _provisioningManager = provisioningManager;
    }

    public ProcessTypeId GetProcessTypeId() => ProcessTypeId.IDP_DISPLAY_NAME_SYNC;
    public bool IsExecutableStepTypeId(ProcessStepTypeId processStepTypeId) => _executableProcessSteps.Contains(processStepTypeId);
    public IEnumerable<ProcessStepTypeId> GetExecutableStepTypeIds() => _executableProcessSteps;
    public ValueTask<bool> IsLockRequested(ProcessStepTypeId processStepTypeId) => new(false);

    public async ValueTask<IProcessTypeExecutor.InitializationResult> InitializeProcess(Guid processId, IEnumerable<ProcessStepTypeId> processStepTypeIds) =>
        await Task.FromResult(new IProcessTypeExecutor.InitializationResult(false, null));

    public async ValueTask<IProcessTypeExecutor.StepExecutionResult> ExecuteProcessStep(ProcessStepTypeId processStepTypeId, IEnumerable<ProcessStepTypeId> processStepTypeIds, CancellationToken cancellationToken)
    {
        IEnumerable<ProcessStepTypeId>? nextStepTypeIds;
        ProcessStepStatusId stepStatusId;
        bool modified;
        string? processMessage;

        try
        {
            (nextStepTypeIds, stepStatusId, modified, processMessage) = processStepTypeId switch
            {
                ProcessStepTypeId.SYNCHRONIZE_IDP_DISPLAY_NAME => await SynchonizeNextDisplayName().ConfigureAwait(false),
                _ => throw new UnexpectedConditionException($"unexpected processStepTypeId {processStepTypeId} for process {ProcessTypeId.IDP_DISPLAY_NAME_SYNC}")
            };
        }
        catch (Exception ex) when (ex is not SystemException)
        {
            (stepStatusId, processMessage, nextStepTypeIds) = ProcessError(ex);
            modified = true;
        }

        return new IProcessTypeExecutor.StepExecutionResult(modified, stepStatusId, nextStepTypeIds, null, processMessage);
    }

    private async Task<(IEnumerable<ProcessStepTypeId>? NextStepTypeIds, ProcessStepStatusId StepStatusId, bool Modified, string? ProcessMessage)> SynchonizeNextDisplayName()
    {
        var idpRepository = _portalRepositories.GetInstance<IIdentityProviderRepository>();
        var iamIdpAlias = idpRepository.GetNextIdpsWithoutDisplayName();
        await using var enumerator = iamIdpAlias.GetAsyncEnumerator();
        if (!await enumerator.MoveNextAsync().ConfigureAwait(false))
        {
            return (null, ProcessStepStatusId.DONE, false, "no idps to synchronize found");
        }

        var (idpId, alias) = enumerator.Current;
        var displayName = await _provisioningManager.GetIdentityProviderDisplayName(alias).ConfigureAwait(false);
        idpRepository.AttachAndModifyIamIdentityProvider(alias, idpId,
            i =>
            {
                i.DisplayName = null;
            },
            i =>
            {
                i.DisplayName = displayName;
            });
        var nextStepTypeIds = await enumerator.MoveNextAsync().ConfigureAwait(false)
            ? Enumerable.Repeat(ProcessStepTypeId.SYNCHRONIZE_IDP_DISPLAY_NAME, 1) // in case there are further idps eligible for sync reschedule the same stepTypeId
            : null;
        return (nextStepTypeIds, ProcessStepStatusId.DONE, true, $"synchronized idp {idpId} with alias {alias} and display name {displayName}");
    }

    private static (ProcessStepStatusId StatusId, string? ProcessMessage, IEnumerable<ProcessStepTypeId>? nextSteps) ProcessError(Exception ex) =>
        (ex is ServiceException { IsRecoverable: true } ? ProcessStepStatusId.TODO : ProcessStepStatusId.FAILED, ex.Message, null);
}
