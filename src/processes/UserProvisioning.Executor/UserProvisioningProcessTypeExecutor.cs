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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.UserProvisioning.Executor;

public class UserProvisioningProcessTypeExecutor : IProcessTypeExecutor<ProcessTypeId, ProcessStepTypeId>
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly IProvisioningManager _provisioningManager;

    private static readonly IEnumerable<ProcessStepTypeId> ExecutableProcessSteps =
    [
        ProcessStepTypeId.DELETE_CENTRAL_USER,
        ProcessStepTypeId.DELETE_COMPANYUSER_ASSIGNED_PROCESS,
    ];

    private Guid _userId = Guid.Empty;
    private Guid _processId = Guid.Empty;

    public UserProvisioningProcessTypeExecutor(IPortalRepositories portalRepositories, IProvisioningManager provisioningManager)
    {
        _portalRepositories = portalRepositories;
        _provisioningManager = provisioningManager;
    }

    public ProcessTypeId GetProcessTypeId() => ProcessTypeId.USER_PROVISIONING;
    public bool IsExecutableStepTypeId(ProcessStepTypeId processStepTypeId) => ExecutableProcessSteps.Contains(processStepTypeId);
    public IEnumerable<ProcessStepTypeId> GetExecutableStepTypeIds() => ExecutableProcessSteps;
    public ValueTask<bool> IsLockRequested(ProcessStepTypeId processStepTypeId) => ValueTask.FromResult(false);

    public async ValueTask<IProcessTypeExecutor<ProcessTypeId, ProcessStepTypeId>.InitializationResult> InitializeProcess(Guid processId, IEnumerable<ProcessStepTypeId> processStepTypeIds)
    {
        var userId = await _portalRepositories.GetInstance<IUserRepository>().GetCompanyUserIdForProcessIdAsync(processId).ConfigureAwait(ConfigureAwaitOptions.None);

        if (userId == Guid.Empty)
        {
            throw new ConflictException($"process {processId} does not exist or is not associated with an CompanyUser");
        }
        _userId = userId;
        _processId = processId;
        return new IProcessTypeExecutor<ProcessTypeId, ProcessStepTypeId>.InitializationResult(false, null);
    }

    public async ValueTask<IProcessTypeExecutor<ProcessTypeId, ProcessStepTypeId>.StepExecutionResult> ExecuteProcessStep(ProcessStepTypeId processStepTypeId, IEnumerable<ProcessStepTypeId> processStepTypeIds, CancellationToken cancellationToken)
    {
        if (_userId == Guid.Empty)
        {
            throw new UnexpectedConditionException("UserId should never be empty here");
        }
        if (_processId == Guid.Empty)
        {
            throw new UnexpectedConditionException("ProcessId should never be empty here");
        }

        IEnumerable<ProcessStepTypeId>? nextStepTypeIds;
        ProcessStepStatusId stepStatusId;
        bool modified;
        string? processMessage;

        try
        {
            (nextStepTypeIds, stepStatusId, modified, processMessage) = processStepTypeId switch
            {
                ProcessStepTypeId.DELETE_CENTRAL_USER => await DeleteCentralUser(_userId).ConfigureAwait(ConfigureAwaitOptions.None),
                ProcessStepTypeId.DELETE_COMPANYUSER_ASSIGNED_PROCESS => DeleteCompanyUserAssignedProcess(_userId, _processId),
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
            _ => (ProcessStepStatusId.FAILED, ex.Message, processStepTypeId.GetUserProvisioningRetriggerStep())
        };
    }

    private async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> DeleteCentralUser(Guid companyUserId)
    {
        var userId = await _provisioningManager.GetUserByUserName(companyUserId.ToString()).ConfigureAwait(ConfigureAwaitOptions.None);
        if (userId == null)
        {
            return ([ProcessStepTypeId.DELETE_COMPANYUSER_ASSIGNED_PROCESS], ProcessStepStatusId.SKIPPED, false, $"User {companyUserId} not found by username");
        }
        try
        {
            await _provisioningManager.DeleteCentralRealmUserAsync(userId).ConfigureAwait(ConfigureAwaitOptions.None);
            return ([ProcessStepTypeId.DELETE_COMPANYUSER_ASSIGNED_PROCESS], ProcessStepStatusId.DONE, false, null);
        }
        catch (KeycloakEntityNotFoundException)
        {
            return ([ProcessStepTypeId.DELETE_COMPANYUSER_ASSIGNED_PROCESS], ProcessStepStatusId.SKIPPED, false, $"User {userId} not found");
        }
    }

    private (IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage) DeleteCompanyUserAssignedProcess(Guid companyUserId, Guid processId)
    {
        _portalRepositories.GetInstance<IUserRepository>().DeleteCompanyUserAssignedProcess(companyUserId, processId);
        return ([], ProcessStepStatusId.DONE, true, null);
    }
}
