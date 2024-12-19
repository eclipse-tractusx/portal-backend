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
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.Worker.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.UserProvisioning.Executor.Provisioning;

public class UserProvisioningProcessTypeExecutor(
    IPortalRepositories portalRepositories,
    IProvisioningManager provisioningManager)
    : IProcessTypeExecutor
{
    private static readonly IEnumerable<ProcessStepTypeId> ExecutableProcessSteps =
    [
        ProcessStepTypeId.DELETE_CENTRAL_USER,
        ProcessStepTypeId.DELETE_COMPANYUSER_ASSIGNED_PROCESS,
    ];

    private Guid _userId = Guid.Empty;
    private Guid _processId = Guid.Empty;

    public ProcessTypeId GetProcessTypeId() => ProcessTypeId.USER_PROVISIONING;
    public bool IsExecutableStepTypeId(ProcessStepTypeId processStepTypeId) => ExecutableProcessSteps.Contains(processStepTypeId);
    public IEnumerable<ProcessStepTypeId> GetExecutableStepTypeIds() => ExecutableProcessSteps;
    public ValueTask<bool> IsLockRequested(ProcessStepTypeId processStepTypeId) => ValueTask.FromResult(false);

    public async ValueTask<IProcessTypeExecutor.InitializationResult> InitializeProcess(Guid processId, IEnumerable<ProcessStepTypeId> processStepTypeIds)
    {
        var userId = await portalRepositories.GetInstance<IUserRepository>().GetCompanyUserIdForProcessIdAsync(processId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (userId == Guid.Empty)
        {
            throw new ConflictException($"process {processId} does not exist or is not associated with an CompanyUser");
        }

        _userId = userId;
        _processId = processId;
        return new IProcessTypeExecutor.InitializationResult(false, null);
    }

    public async ValueTask<IProcessTypeExecutor.StepExecutionResult> ExecuteProcessStep(ProcessStepTypeId processStepTypeId, IEnumerable<ProcessStepTypeId> processStepTypeIds, CancellationToken cancellationToken)
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
            (stepStatusId, processMessage, nextStepTypeIds) = ex.ProcessError(processStepTypeId, this.GetProcessTypeId());
            modified = true;
        }

        return new IProcessTypeExecutor.StepExecutionResult(modified, stepStatusId, nextStepTypeIds, null, processMessage);
    }

    private async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> DeleteCentralUser(Guid companyUserId)
    {
        var userId = await provisioningManager.GetUserByUserName(companyUserId.ToString()).ConfigureAwait(ConfigureAwaitOptions.None);
        if (userId == null)
        {
            return ([ProcessStepTypeId.DELETE_COMPANYUSER_ASSIGNED_PROCESS], ProcessStepStatusId.SKIPPED, false, $"User {companyUserId} not found by username");
        }

        try
        {
            await provisioningManager.DeleteCentralRealmUserAsync(userId).ConfigureAwait(ConfigureAwaitOptions.None);
            return ([ProcessStepTypeId.DELETE_COMPANYUSER_ASSIGNED_PROCESS], ProcessStepStatusId.DONE, false, null);
        }
        catch (KeycloakEntityNotFoundException)
        {
            return ([ProcessStepTypeId.DELETE_COMPANYUSER_ASSIGNED_PROCESS], ProcessStepStatusId.SKIPPED, false, $"User {userId} not found");
        }
    }

    private (IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage) DeleteCompanyUserAssignedProcess(Guid companyUserId, Guid processId)
    {
        portalRepositories.GetInstance<IUserRepository>().DeleteCompanyUserAssignedProcess(companyUserId, processId);
        return ([], ProcessStepStatusId.DONE, true, null);
    }
}
