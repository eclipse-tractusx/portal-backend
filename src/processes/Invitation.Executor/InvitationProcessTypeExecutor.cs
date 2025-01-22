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

using Flurl.Http;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Worker.Library;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.Invitation.Executor.Extensions;
using System.Collections.Immutable;
using System.Net;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.Invitation.Executor;

public class InvitationProcessTypeExecutor : IProcessTypeExecutor<ProcessTypeId, ProcessStepTypeId>
{
    private static readonly IEnumerable<int> RecoverableStatusCodes = ImmutableArray.Create(
        (int)HttpStatusCode.BadGateway,
        (int)HttpStatusCode.ServiceUnavailable,
        (int)HttpStatusCode.GatewayTimeout);

    private static readonly IEnumerable<ProcessStepTypeId> ExecutableProcessSteps = ImmutableArray.Create(
        ProcessStepTypeId.INVITATION_CREATE_CENTRAL_IDP,
        ProcessStepTypeId.INVITATION_CREATE_SHARED_IDP_SERVICE_ACCOUNT,
        ProcessStepTypeId.INVITATION_UPDATE_CENTRAL_IDP_URLS,
        ProcessStepTypeId.INVITATION_ADD_REALM_ROLE,
        ProcessStepTypeId.INVITATION_CREATE_CENTRAL_IDP_ORG_MAPPER,
        ProcessStepTypeId.INVITATION_CREATE_SHARED_REALM,
        ProcessStepTypeId.INVITATION_CREATE_SHARED_CLIENT,
        ProcessStepTypeId.INVITATION_ENABLE_CENTRAL_IDP,
        ProcessStepTypeId.INVITATION_CREATE_DATABASE_IDP,
        ProcessStepTypeId.INVITATION_CREATE_USER);

    private static readonly IEnumerable<ProcessStepTypeId> LockableProcessSteps = ImmutableArray.Create(
        ProcessStepTypeId.INVITATION_CREATE_CENTRAL_IDP,
        ProcessStepTypeId.INVITATION_CREATE_SHARED_IDP_SERVICE_ACCOUNT,
        ProcessStepTypeId.INVITATION_CREATE_CENTRAL_IDP_ORG_MAPPER,
        ProcessStepTypeId.INVITATION_CREATE_SHARED_REALM,
        ProcessStepTypeId.INVITATION_CREATE_SHARED_CLIENT,
        ProcessStepTypeId.INVITATION_CREATE_DATABASE_IDP,
        ProcessStepTypeId.INVITATION_CREATE_USER);

    private readonly IPortalRepositories _portalRepositories;
    private readonly IInvitationProcessService _invitationProcessService;
    private Guid _companyInvitationId;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalRepositories">Portal Repositories</param>
    /// <param name="invitationProcessService">Invitation Process Service</param>
    public InvitationProcessTypeExecutor(IPortalRepositories portalRepositories, IInvitationProcessService invitationProcessService)
    {
        _portalRepositories = portalRepositories;
        _invitationProcessService = invitationProcessService;
    }

    public bool IsExecutableStepTypeId(ProcessStepTypeId processStepTypeId) => ExecutableProcessSteps.Contains(processStepTypeId);
    public IEnumerable<ProcessStepTypeId> GetExecutableStepTypeIds() => ExecutableProcessSteps;

    public async ValueTask<IProcessTypeExecutor<ProcessTypeId, ProcessStepTypeId>.InitializationResult> InitializeProcess(Guid processId, IEnumerable<ProcessStepTypeId> processStepTypeIds)
    {
        _companyInvitationId = Guid.Empty;

        var result = await _portalRepositories.GetInstance<ICompanyInvitationRepository>().GetCompanyInvitationForProcessId(processId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (result == Guid.Empty)
        {
            throw new NotFoundException($"process {processId} does not exist or is not associated with an company invitation");
        }

        _companyInvitationId = result;
        return new IProcessTypeExecutor<ProcessTypeId, ProcessStepTypeId>.InitializationResult(false, null);
    }

    public ValueTask<bool> IsLockRequested(ProcessStepTypeId processStepTypeId) => new(LockableProcessSteps.Contains(processStepTypeId));

    public ProcessTypeId GetProcessTypeId() => ProcessTypeId.INVITATION;

    public async ValueTask<IProcessTypeExecutor<ProcessTypeId, ProcessStepTypeId>.StepExecutionResult> ExecuteProcessStep(ProcessStepTypeId processStepTypeId, IEnumerable<ProcessStepTypeId> processStepTypeIds, CancellationToken cancellationToken)
    {
        if (_companyInvitationId == Guid.Empty)
        {
            throw new UnexpectedConditionException("companyInvitationId should never be empty here");
        }

        IEnumerable<ProcessStepTypeId>? nextStepTypeIds;
        ProcessStepStatusId stepStatusId;
        bool modified;
        string? processMessage;

        try
        {
            (nextStepTypeIds, stepStatusId, modified, processMessage) = processStepTypeId switch
            {
                ProcessStepTypeId.INVITATION_CREATE_CENTRAL_IDP => await _invitationProcessService.CreateCentralIdp(_companyInvitationId)
                    .ConfigureAwait(ConfigureAwaitOptions.None),
                ProcessStepTypeId.INVITATION_CREATE_SHARED_IDP_SERVICE_ACCOUNT => await _invitationProcessService.CreateSharedIdpServiceAccount(_companyInvitationId)
                    .ConfigureAwait(ConfigureAwaitOptions.None),
                ProcessStepTypeId.INVITATION_ADD_REALM_ROLE => await _invitationProcessService.AddRealmRoleMappingsToUserAsync(_companyInvitationId)
                    .ConfigureAwait(ConfigureAwaitOptions.None),
                ProcessStepTypeId.INVITATION_UPDATE_CENTRAL_IDP_URLS => await _invitationProcessService.UpdateCentralIdpUrl(_companyInvitationId)
                    .ConfigureAwait(ConfigureAwaitOptions.None),
                ProcessStepTypeId.INVITATION_CREATE_CENTRAL_IDP_ORG_MAPPER => await _invitationProcessService.CreateCentralIdpOrgMapper(_companyInvitationId)
                    .ConfigureAwait(ConfigureAwaitOptions.None),
                ProcessStepTypeId.INVITATION_CREATE_SHARED_REALM => await _invitationProcessService.CreateSharedIdpRealm(_companyInvitationId)
                    .ConfigureAwait(ConfigureAwaitOptions.None),
                ProcessStepTypeId.INVITATION_CREATE_SHARED_CLIENT => await _invitationProcessService.CreateSharedClient(_companyInvitationId)
                    .ConfigureAwait(ConfigureAwaitOptions.None),
                ProcessStepTypeId.INVITATION_ENABLE_CENTRAL_IDP => await _invitationProcessService.EnableCentralIdp(_companyInvitationId)
                    .ConfigureAwait(ConfigureAwaitOptions.None),
                ProcessStepTypeId.INVITATION_CREATE_DATABASE_IDP => await _invitationProcessService.CreateIdpDatabase(_companyInvitationId)
                    .ConfigureAwait(ConfigureAwaitOptions.None),
                ProcessStepTypeId.INVITATION_CREATE_USER => await _invitationProcessService.CreateUser(_companyInvitationId, cancellationToken)
                    .ConfigureAwait(ConfigureAwaitOptions.None),
                _ => throw new UnexpectedConditionException($"Execution for {processStepTypeId} is currently not supported.")
            };
        }
        catch (Exception ex) when (ex is not SystemException)
        {
            (stepStatusId, processMessage, nextStepTypeIds) = ProcessError(ex, processStepTypeId);
            modified = true;
        }

        return new IProcessTypeExecutor<ProcessTypeId, ProcessStepTypeId>.StepExecutionResult(modified, stepStatusId, nextStepTypeIds, null, processMessage);
    }

    private static (ProcessStepStatusId StatusId, string? ProcessMessage, IEnumerable<ProcessStepTypeId>? nextSteps) ProcessError(Exception ex, ProcessStepTypeId processStepTypeId) =>
        ex switch
        {
            ServiceException { IsRecoverable: true } => (ProcessStepStatusId.TODO, ex.Message, null),
            FlurlHttpException { StatusCode: not null } flurlHttpException when RecoverableStatusCodes.Contains(flurlHttpException.StatusCode.Value) => (ProcessStepStatusId.TODO, ex.Message, null),
            _ => (ProcessStepStatusId.FAILED, ex.Message, processStepTypeId.GetInvitationRetriggerStep())
        };
}
