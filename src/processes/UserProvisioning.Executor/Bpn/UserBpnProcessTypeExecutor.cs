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

using Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.Worker.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.UserProvisioning.Executor.Bpn;

public class UserBpnProcessTypeExecutor(
    IPortalRepositories portalRepositories,
    IProvisioningManager provisioningManager,
    IBpdmAccessService bpdmAccessService)
    : IProcessTypeExecutor
{
    private static readonly IEnumerable<ProcessStepTypeId> ExecutableProcessSteps =
    [
        ProcessStepTypeId.DELETE_BPN_FROM_CENTRAL_USER,
        ProcessStepTypeId.DELETE_BPN_FROM_IDENTITY,
        ProcessStepTypeId.CHECK_LEGAL_ENTITY_DATA,
        ProcessStepTypeId.ADD_BPN_TO_IDENTITY,
        ProcessStepTypeId.CLEANUP_USER_BPN
    ];

    private Guid _userId = Guid.Empty;
    private Guid _processId = Guid.Empty;
    private string _bpn = null!;

    public ProcessTypeId GetProcessTypeId() => ProcessTypeId.USER_BPN;
    public bool IsExecutableStepTypeId(ProcessStepTypeId processStepTypeId) => ExecutableProcessSteps.Contains(processStepTypeId);
    public IEnumerable<ProcessStepTypeId> GetExecutableStepTypeIds() => ExecutableProcessSteps;
    public ValueTask<bool> IsLockRequested(ProcessStepTypeId processStepTypeId) => ValueTask.FromResult(false);

    public async ValueTask<IProcessTypeExecutor.InitializationResult> InitializeProcess(Guid processId, IEnumerable<ProcessStepTypeId> processStepTypeIds)
    {
        var (userId, bpn) = await portalRepositories.GetInstance<IUserBusinessPartnerRepository>().GetForProcessIdAsync(processId).ConfigureAwait(ConfigureAwaitOptions.None);

        if (userId == Guid.Empty)
        {
            throw new ConflictException($"process {processId} does not exist or is not associated with an CompanyUser");
        }

        _userId = userId;
        _processId = processId;
        _bpn = bpn ?? throw new ConflictException($"process {processId} does not exist or is not associated with an CompanyUser");
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

        if (_bpn is null)
        {
            throw new UnexpectedConditionException("Bpn should never be empty here");
        }

        IEnumerable<ProcessStepTypeId>? nextStepTypeIds;
        ProcessStepStatusId stepStatusId;
        bool modified;
        string? processMessage;

        try
        {
            (nextStepTypeIds, stepStatusId, modified, processMessage) = processStepTypeId switch
            {
                ProcessStepTypeId.DELETE_BPN_FROM_CENTRAL_USER => await DeleteBpnFromCentralUser(_userId, _bpn).ConfigureAwait(ConfigureAwaitOptions.None),
                ProcessStepTypeId.DELETE_BPN_FROM_IDENTITY => DeleteBpnFromIdentity(_userId, _bpn),
                ProcessStepTypeId.CHECK_LEGAL_ENTITY_DATA => await CheckLegalEntityData(_bpn, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None),
                ProcessStepTypeId.ADD_BPN_TO_IDENTITY => await AddBpnToIdentity(_userId, _bpn).ConfigureAwait(ConfigureAwaitOptions.None),
                ProcessStepTypeId.CLEANUP_USER_BPN => DeleteBpnFromIdentity(_userId, _bpn),
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

    private async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> DeleteBpnFromCentralUser(Guid identityId, string businessPartnerNumber)
    {
        var userId = await provisioningManager.GetUserByUserName(identityId.ToString()).ConfigureAwait(ConfigureAwaitOptions.None);
        if (userId is null)
        {
            return ([ProcessStepTypeId.DELETE_BPN_FROM_IDENTITY], ProcessStepStatusId.SKIPPED, false, $"User {identityId} not found by username");
        }

        try
        {
            await provisioningManager.DeleteCentralUserBusinessPartnerNumberAsync(userId, businessPartnerNumber.ToUpper()).ConfigureAwait(ConfigureAwaitOptions.None);
            return ([ProcessStepTypeId.DELETE_BPN_FROM_IDENTITY], ProcessStepStatusId.DONE, false, null);
        }
        catch (KeycloakEntityNotFoundException)
        {
            return ([ProcessStepTypeId.DELETE_BPN_FROM_IDENTITY], ProcessStepStatusId.SKIPPED, false, $"User {userId} not found");
        }
    }

    private (IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage) DeleteBpnFromIdentity(Guid userId, string businessPartnerNumber)
    {
        portalRepositories.GetInstance<IUserBusinessPartnerRepository>().DeleteCompanyUserAssignedBusinessPartner(userId, businessPartnerNumber.ToUpper());
        return (null, ProcessStepStatusId.DONE, true, null);
    }

    private async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> CheckLegalEntityData(string bpn, CancellationToken cancellationToken)
    {
        try
        {
            var legalEntity = await bpdmAccessService.FetchLegalEntityByBpn(bpn, cancellationToken).ConfigureAwait(false);
            if (!bpn.Equals(legalEntity.Bpn, StringComparison.OrdinalIgnoreCase))
            {
                return ([ProcessStepTypeId.CLEANUP_USER_BPN], ProcessStepStatusId.FAILED, false, $"Bpdm {bpn} did return incorrect bpn legal-entity-data");
            }
        }
        catch (Exception ex)
        {
            return (null, ProcessStepStatusId.TODO, false, $"{ex.Message}");
        }

        return ([ProcessStepTypeId.ADD_BPN_TO_IDENTITY], ProcessStepStatusId.DONE, false, null);
    }

    private async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeId, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> AddBpnToIdentity(Guid identityId, string bpn)
    {
        var iamUserId = await provisioningManager.GetUserByUserName(identityId.ToString()).ConfigureAwait(ConfigureAwaitOptions.None) ??
                        throw new ConflictException($"user {identityId} not found in keycloak");
        await provisioningManager.AddBpnAttributetoUserAsync(iamUserId, Enumerable.Repeat(bpn, 1)).ConfigureAwait(ConfigureAwaitOptions.None);
        return (null, ProcessStepStatusId.DONE, false, null);
    }
}
