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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Async;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.OfferSubscription.Executor.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Processes.Worker.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.NetworkRegistration.Executor;

public class NetworkRegistrationProcessTypeExecutor : IProcessTypeExecutor
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly IUserProvisioningService _userProvisioningService;

    private readonly IEnumerable<ProcessStepTypeId> _executableProcessSteps = ImmutableArray.Create(
        ProcessStepTypeId.SYNCHRONIZE_USER);

    private readonly NetworkRegistrationProcessSettings _settings;
    private Guid _networkRegistrationId;

    public NetworkRegistrationProcessTypeExecutor(
        IPortalRepositories portalRepositories,
        IUserProvisioningService userProvisioningService,
        IOptions<NetworkRegistrationProcessSettings> options)
    {
        _portalRepositories = portalRepositories;
        _userProvisioningService = userProvisioningService;

        _settings = options.Value;
    }

    public ProcessTypeId GetProcessTypeId() => ProcessTypeId.PARTNER_REGISTRATION;
    public bool IsExecutableStepTypeId(ProcessStepTypeId processStepTypeId) => _executableProcessSteps.Contains(processStepTypeId);
    public IEnumerable<ProcessStepTypeId> GetExecutableStepTypeIds() => _executableProcessSteps;
    public ValueTask<bool> IsLockRequested(ProcessStepTypeId processStepTypeId) => new(false);

    public async ValueTask<IProcessTypeExecutor.InitializationResult> InitializeProcess(Guid processId, IEnumerable<ProcessStepTypeId> processStepTypeIds)
    {
        var result = await _portalRepositories.GetInstance<IOfferSubscriptionsRepository>().GetNetworkRegistrationDataForProcessIdAsync(processId).ConfigureAwait(false); // TODO (PS): Move to other repo
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
            throw new UnexpectedConditionException("offerSubscriptionId should never be empty here");
        }

        IEnumerable<ProcessStepTypeId>? nextStepTypeIds;
        ProcessStepStatusId stepStatusId;
        bool modified;
        string? processMessage;

        try
        {
            (nextStepTypeIds, stepStatusId, modified, processMessage) = processStepTypeId switch
            {
                ProcessStepTypeId.SYNCHRONIZE_USER => await SynchronizeUser(_networkRegistrationId)
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

    private async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> SynchronizeUser(Guid networkRegistrationId)
    {
        var companyAssignedIdentityProviders = await _portalRepositories.GetInstance<IUserRepository>()
            .GetUserAssignedIdentityProviderForNetworkRegistration(networkRegistrationId)
            .ToListAsync()
            .ConfigureAwait(false);
        var roleData = await _userProvisioningService.GetRoleDatas(_settings.InitialRoles).ToListAsync().ConfigureAwait(false);

        foreach (var cu in companyAssignedIdentityProviders)
        {
            await _userProvisioningService.CreateCentralUserWithProviderLinks(cu.CompanyUserId, new UserCreationRoleDataIdpInfo(cu.FirstName, cu.LastName, cu.Email, roleData, cu.UserName, cu.UserId), cu.CompanyName, cu.Bpn, cu.Alias, cu.ProviderUserId);
        }

        return new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(
                null,
                ProcessStepStatusId.DONE,
                false,
                null);
    }

    private static (ProcessStepStatusId StatusId, string? ProcessMessage, IEnumerable<ProcessStepTypeId>? nextSteps) ProcessError(Exception ex, ProcessStepTypeId processStepTypeId)
    {
        return ex switch
        {
            ServiceException { IsRecoverable: true } => (ProcessStepStatusId.TODO, ex.Message, null),
            _ => (ProcessStepStatusId.FAILED, ex.Message, processStepTypeId.GetRetriggerStep())
        };
    }
}
