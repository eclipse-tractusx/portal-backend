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
using Org.Eclipse.TractusX.Portal.Backend.OnboardingServiceProvider.Library;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.NetworkRegistration.Library;
using Org.Eclipse.TractusX.Portal.Backend.Processes.Worker.Library;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.NetworkRegistration.Executor;

public class NetworkRegistrationProcessTypeExecutor : IProcessTypeExecutor
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly INetworkRegistrationHandler _networkRegistrationHandler;
    private readonly IOnboardingServiceProviderBusinessLogic _onboardingServiceProviderBusinessLogic;

    private readonly IEnumerable<ProcessStepTypeId> _executableProcessSteps = ImmutableArray.Create(
        ProcessStepTypeId.SYNCHRONIZE_USER,
        ProcessStepTypeId.TRIGGER_CALLBACK_OSP_SUBMITTED,
        ProcessStepTypeId.TRIGGER_CALLBACK_OSP_DECLINED,
        ProcessStepTypeId.TRIGGER_CALLBACK_OSP_APPROVED,
        ProcessStepTypeId.REMOVE_KEYCLOAK_USERS);

    private Guid _networkRegistrationId;

    public NetworkRegistrationProcessTypeExecutor(
        IPortalRepositories portalRepositories,
        INetworkRegistrationHandler networkRegistrationHandler,
        IOnboardingServiceProviderBusinessLogic onboardingServiceProviderBusinessLogic)
    {
        _portalRepositories = portalRepositories;
        _networkRegistrationHandler = networkRegistrationHandler;
        _onboardingServiceProviderBusinessLogic = onboardingServiceProviderBusinessLogic;
    }

    public ProcessTypeId GetProcessTypeId() => ProcessTypeId.PARTNER_REGISTRATION;
    public bool IsExecutableStepTypeId(ProcessStepTypeId processStepTypeId) => _executableProcessSteps.Contains(processStepTypeId);
    public IEnumerable<ProcessStepTypeId> GetExecutableStepTypeIds() => _executableProcessSteps;
    public ValueTask<bool> IsLockRequested(ProcessStepTypeId processStepTypeId) => new(false);

    public async ValueTask<IProcessTypeExecutor.InitializationResult> InitializeProcess(Guid processId, IEnumerable<ProcessStepTypeId> processStepTypeIds)
    {
        var result = await _portalRepositories.GetInstance<INetworkRepository>().GetNetworkRegistrationDataForProcessIdAsync(processId).ConfigureAwait(false);
        if (result == Guid.Empty)
        {
            throw new NotFoundException($"process {processId} does not exist or is not associated with an offer subscription");
        }

        _networkRegistrationId = result;
        return new IProcessTypeExecutor.InitializationResult(false, null);
    }

    public async ValueTask<IProcessTypeExecutor.StepExecutionResult> ExecuteProcessStep(ProcessStepTypeId processStepTypeId, IEnumerable<ProcessStepTypeId> processStepTypeIds, CancellationToken cancellationToken)
    {
        if (_networkRegistrationId == Guid.Empty)
        {
            throw new UnexpectedConditionException("networkRegistrationId should never be empty here");
        }

        IEnumerable<ProcessStepTypeId>? nextStepTypeIds;
        ProcessStepStatusId stepStatusId;
        bool modified;
        string? processMessage;

        try
        {
            (nextStepTypeIds, stepStatusId, modified, processMessage) = processStepTypeId switch
            {
                ProcessStepTypeId.SYNCHRONIZE_USER => await _networkRegistrationHandler.SynchronizeUser(_networkRegistrationId)
                    .ConfigureAwait(false),
                ProcessStepTypeId.TRIGGER_CALLBACK_OSP_SUBMITTED => await _onboardingServiceProviderBusinessLogic.TriggerProviderCallback(_networkRegistrationId, ProcessStepTypeId.TRIGGER_CALLBACK_OSP_SUBMITTED, cancellationToken)
                    .ConfigureAwait(false),
                ProcessStepTypeId.TRIGGER_CALLBACK_OSP_APPROVED => await _onboardingServiceProviderBusinessLogic.TriggerProviderCallback(_networkRegistrationId, ProcessStepTypeId.TRIGGER_CALLBACK_OSP_APPROVED, cancellationToken)
                    .ConfigureAwait(false),
                ProcessStepTypeId.TRIGGER_CALLBACK_OSP_DECLINED => await _onboardingServiceProviderBusinessLogic.TriggerProviderCallback(_networkRegistrationId, ProcessStepTypeId.TRIGGER_CALLBACK_OSP_DECLINED, cancellationToken)
                    .ConfigureAwait(false),
                ProcessStepTypeId.REMOVE_KEYCLOAK_USERS => await _networkRegistrationHandler.RemoveKeycloakUser(_networkRegistrationId)
                    .ConfigureAwait(false),
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
            _ => (ProcessStepStatusId.FAILED, ex.Message, processStepTypeId.GetNetworkRetriggerStep())
        };
    }
}
