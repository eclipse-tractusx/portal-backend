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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.Worker.Library;
using System.Net;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.DimUserCreationProcess.Executor;

public class DimUserBaseProcessTypeExecutor(
    IPortalRepositories portalRepositories,
    IDimUserProcessService dimUserProcessService,
    ProcessTypeId processTypeId)
{
    private readonly IEnumerable<ProcessStepTypeId> _executableProcessSteps = processTypeId == ProcessTypeId.DIM_TECHNICAL_USER ?
        [ProcessStepTypeId.CREATE_DIM_TECHNICAL_USER] :
        [ProcessStepTypeId.DELETE_DIM_TECHNICAL_USER];

    private static readonly IEnumerable<int> RecoverableStatusCodes =
    [
        (int)HttpStatusCode.BadGateway,
        (int)HttpStatusCode.ServiceUnavailable,
        (int)HttpStatusCode.GatewayTimeout
    ];

    private Guid _dimServiceAccountId;
    private Guid _processId;

    public ProcessTypeId GetProcessTypeId() => processTypeId;

    public bool IsExecutableStepTypeId(ProcessStepTypeId processStepTypeId) =>
        _executableProcessSteps.Contains(processStepTypeId);

    public IEnumerable<ProcessStepTypeId> GetExecutableStepTypeIds() => _executableProcessSteps;

#pragma warning disable IDE0060
    public async ValueTask<IProcessTypeExecutor.InitializationResult> InitializeProcess(Guid processId, IEnumerable<ProcessStepTypeId> processStepTypeIds)
#pragma warning restore IDE0060
    {
        _dimServiceAccountId = Guid.Empty;

        var result = await portalRepositories.GetInstance<IServiceAccountRepository>()
            .GetDimServiceAccountIdForProcess(processId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (result == Guid.Empty)
        {
            throw new NotFoundException(
                $"process {processId} does not exist or is not associated with an dim service account");
        }

        _dimServiceAccountId = result;
        _processId = processId;
        return new IProcessTypeExecutor.InitializationResult(false, null);
    }

#pragma warning disable CA1822
#pragma warning disable IDE0060
    public ValueTask<bool> IsLockRequested(ProcessStepTypeId processStepTypeId) => ValueTask.FromResult(false);
#pragma warning restore CA1822
#pragma warning restore IDE0060

#pragma warning disable IDE0060
    public async ValueTask<IProcessTypeExecutor.StepExecutionResult> ExecuteProcessStep(ProcessStepTypeId processStepTypeId, IEnumerable<ProcessStepTypeId> processStepTypeIds, CancellationToken cancellationToken)
#pragma warning restore IDE0060
    {
        if (_dimServiceAccountId == Guid.Empty)
            throw new UnexpectedConditionException("dimServiceAccountId should never be empty here");

        IEnumerable<ProcessStepTypeId>? nextStepTypeIds;
        ProcessStepStatusId stepStatusId;
        bool modified;
        string? processMessage;

        try
        {
            (nextStepTypeIds, stepStatusId, modified, processMessage) = processStepTypeId switch
            {
                ProcessStepTypeId.CREATE_DIM_TECHNICAL_USER => await dimUserProcessService
                    .CreateDeleteDimUser(_processId, _dimServiceAccountId, true, cancellationToken)
                    .ConfigureAwait(ConfigureAwaitOptions.None),
                ProcessStepTypeId.DELETE_DIM_TECHNICAL_USER => await dimUserProcessService
                    .CreateDeleteDimUser(_processId, _dimServiceAccountId, false, cancellationToken)
                    .ConfigureAwait(ConfigureAwaitOptions.None),
                _ => throw new UnexpectedConditionException(
                    $"Execution for {processStepTypeId} is currently not supported.")
            };
        }
        catch (Exception ex) when (ex is not SystemException)
        {
            (stepStatusId, processMessage, nextStepTypeIds) = ProcessError(ex);
            modified = true;
        }

        return new IProcessTypeExecutor.StepExecutionResult(modified, stepStatusId, nextStepTypeIds, null, processMessage);
    }

    private (ProcessStepStatusId StatusId, string? ProcessMessage, IEnumerable<ProcessStepTypeId>? nextSteps) ProcessError(Exception ex) =>
        ex switch
        {
            ServiceException { IsRecoverable: true } => (ProcessStepStatusId.TODO, ex.Message, null),
            FlurlHttpException { StatusCode: not null } flurlHttpException when
                RecoverableStatusCodes.Contains(flurlHttpException.StatusCode.Value) => (ProcessStepStatusId.TODO,
                    ex.Message, null),
            _ => (ProcessStepStatusId.FAILED, ex.Message, Enumerable.Repeat(GetProcessTypeId() == ProcessTypeId.DIM_TECHNICAL_USER ? ProcessStepTypeId.RETRIGGER_CREATE_DIM_TECHNICAL_USER : ProcessStepTypeId.RETRIGGER_DELETE_DIM_TECHNICAL_USER, 1))
        };
}
