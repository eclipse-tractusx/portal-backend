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

using Flurl.Http;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.Worker.Library;
using System.Collections.Immutable;
using System.Net;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.DimUserCreationProcess.Executor;

public class DimUserCreationProcessTypeExecutor(
    IPortalRepositories portalRepositories,
    IDimUserCreationProcessService dimUserCreationProcessService)
    : IProcessTypeExecutor
{
    private static readonly IEnumerable<int> RecoverableStatusCodes =
    [
        (int)HttpStatusCode.BadGateway,
        (int)HttpStatusCode.ServiceUnavailable,
        (int)HttpStatusCode.GatewayTimeout
    ];

    private static readonly IEnumerable<ProcessStepTypeId> ExecutableProcessSteps =
    [
        ProcessStepTypeId.CREATE_DIM_TECHNICAL_USER
    ];

    private Guid _dimServiceAccountId;
    private Guid _processId;

    public bool IsExecutableStepTypeId(ProcessStepTypeId processStepTypeId) =>
        ExecutableProcessSteps.Contains(processStepTypeId);

    public IEnumerable<ProcessStepTypeId> GetExecutableStepTypeIds() => ExecutableProcessSteps;

    public async ValueTask<IProcessTypeExecutor.InitializationResult> InitializeProcess(Guid processId, IEnumerable<ProcessStepTypeId> processStepTypeIds)
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

    public ValueTask<bool> IsLockRequested(ProcessStepTypeId processStepTypeId) => ValueTask.FromResult(false);

    public ProcessTypeId GetProcessTypeId() => ProcessTypeId.DIM_TECHNICAL_USER;

    public async ValueTask<IProcessTypeExecutor.StepExecutionResult> ExecuteProcessStep(ProcessStepTypeId processStepTypeId, IEnumerable<ProcessStepTypeId> processStepTypeIds, CancellationToken cancellationToken)
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
                ProcessStepTypeId.CREATE_DIM_TECHNICAL_USER => await dimUserCreationProcessService
                    .CreateDimUser(_processId, _dimServiceAccountId, cancellationToken)
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

        return new IProcessTypeExecutor.StepExecutionResult(modified, stepStatusId, nextStepTypeIds, null,
            processMessage);
    }

    private static (ProcessStepStatusId StatusId, string? ProcessMessage, IEnumerable<ProcessStepTypeId>? nextSteps) ProcessError(Exception ex) =>
        ex switch
        {
            ServiceException { IsRecoverable: true } => (ProcessStepStatusId.TODO, ex.Message, null),
            FlurlHttpException { StatusCode: not null } flurlHttpException when
                RecoverableStatusCodes.Contains(flurlHttpException.StatusCode.Value) => (ProcessStepStatusId.TODO,
                    ex.Message, null),
            _ => (ProcessStepStatusId.FAILED, ex.Message, Enumerable.Repeat(ProcessStepTypeId.RETRIGGER_CREATE_DIM_TECHNICAL_USER, 1))
        };
}
