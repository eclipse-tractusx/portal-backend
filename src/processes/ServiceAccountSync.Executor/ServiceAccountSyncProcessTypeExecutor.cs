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

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.ServiceACcountSync.Executor;

public class ServiceAccountSyncProcessTypeExecutor : IProcessTypeExecutor
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly IProvisioningManager _provisioningManager;

    private static readonly IEnumerable<ProcessStepTypeId> _executableProcessSteps = ImmutableArray.Create(ProcessStepTypeId.SYNCHRONIZE_SERVICE_ACCOUNTS);

    public ServiceAccountSyncProcessTypeExecutor(IPortalRepositories portalRepositories, IProvisioningManager provisioningManager)
    {
        _portalRepositories = portalRepositories;
        _provisioningManager = provisioningManager;
    }

    public ProcessTypeId GetProcessTypeId() => ProcessTypeId.SERVICE_ACCOUNT_SYNC;
    public bool IsExecutableStepTypeId(ProcessStepTypeId processStepTypeId) => _executableProcessSteps.Contains(processStepTypeId);
    public IEnumerable<ProcessStepTypeId> GetExecutableStepTypeIds() => _executableProcessSteps;
    public ValueTask<bool> IsLockRequested(ProcessStepTypeId processStepTypeId) => new(false);

    public async ValueTask<IProcessTypeExecutor.InitializationResult> InitializeProcess(Guid processId, IEnumerable<ProcessStepTypeId> processStepTypeIds)
    {
        return await Task.FromResult(new IProcessTypeExecutor.InitializationResult(false, null));
    }

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
                ProcessStepTypeId.SYNCHRONIZE_SERVICE_ACCOUNTS => await SynchonizeNextServiceAccount().ConfigureAwait(false),
                _ => throw new UnexpectedConditionException($"unexpected processStepTypeId {processStepTypeId} for process {ProcessTypeId.SERVICE_ACCOUNT_SYNC}")
            };
        }
        catch (Exception ex) when (ex is not SystemException)
        {
            (stepStatusId, processMessage, nextStepTypeIds) = ProcessError(ex);
            modified = true;
        }

        return new IProcessTypeExecutor.StepExecutionResult(modified, stepStatusId, nextStepTypeIds, null, processMessage);
    }

    private async Task<(IEnumerable<ProcessStepTypeId>? NextStepTypeIds, ProcessStepStatusId StepStatusId, bool Modified, string? ProcessMessage)> SynchonizeNextServiceAccount()
    {
        var userRepository = _portalRepositories.GetInstance<IUserRepository>();
        var serviceAccountIds = userRepository.GetNextServiceAccountsWithoutUserEntityId();
        await using var enumerator = serviceAccountIds.GetAsyncEnumerator();
        if (await enumerator.MoveNextAsync().ConfigureAwait(false))
        {
            var (serviceAccountId, clientClientId) = enumerator.Current;
            var userEntityId = await _provisioningManager.GetServiceAccountUserId(clientClientId).ConfigureAwait(false);
            userRepository.AttachAndModifyIdentity(serviceAccountId,
                i =>
                {
                    i.UserEntityId = null;
                },
                i =>
                {
                    i.UserEntityId = userEntityId;
                });
            return await enumerator.MoveNextAsync().ConfigureAwait(false)
                ? (null, ProcessStepStatusId.TODO, true, null)  // in case there are further serviceAccounts eligible for sync request save without step status change to repeat the unmodified step
                : (null, ProcessStepStatusId.DONE, true, null); // otherwise request save and done
        }
        return (null, ProcessStepStatusId.DONE, false, null);
    }

    private static (ProcessStepStatusId StatusId, string? ProcessMessage, IEnumerable<ProcessStepTypeId>? nextSteps) ProcessError(Exception ex) =>
        (ex is ServiceException { IsRecoverable: true } ? ProcessStepStatusId.TODO : ProcessStepStatusId.FAILED, ex.Message, null);
}
