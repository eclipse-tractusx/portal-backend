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
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Service;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.OfferSubscription.Executor.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Processes.Worker.Library;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.OfferSubscription.Executor;

public class OfferSubscriptionProcessTypeExecutor : IProcessTypeExecutor
{
    private readonly IOfferSubscriptionService _offerSubscriptionService;
    private readonly IOfferSetupService _offerSetupService;
    private readonly IOfferSubscriptionsRepository _offerSubscriptionsRepository;

    private readonly IReadOnlyList<ProcessStepTypeId> _executableProcessSteps = new List<ProcessStepTypeId>
    {
        ProcessStepTypeId.TRIGGER_PROVIDER,
        ProcessStepTypeId.ACTIVATE_SUBSCRIPTION
    };

    private Guid _offerSubscriptionId;
    private readonly OfferSubscriptionsProcessSettings _settings;

    public OfferSubscriptionProcessTypeExecutor(
        IOfferSubscriptionService offerSubscriptionService,
        IOfferSetupService offerSetupService,
        IPortalRepositories portalRepositories,
        IOptions<OfferSubscriptionsProcessSettings> options)
    {
        _offerSubscriptionService = offerSubscriptionService;
        _offerSetupService = offerSetupService;
        _offerSubscriptionsRepository = portalRepositories.GetInstance<IOfferSubscriptionsRepository>();
        _settings = options.Value;
    }

    public ProcessTypeId GetProcessTypeId() => ProcessTypeId.OFFER_SUBSCRIPTION;
    public bool IsExecutableStepTypeId(ProcessStepTypeId processStepTypeId) => _executableProcessSteps.Contains(processStepTypeId);
    public IEnumerable<ProcessStepTypeId> GetExecutableStepTypeIds() => _executableProcessSteps;
    public ValueTask<bool> IsLockRequested(ProcessStepTypeId processStepTypeId) => new(false);

    public async ValueTask<IProcessTypeExecutor.InitializationResult> InitializeProcess(Guid processId, IEnumerable<ProcessStepTypeId> processStepTypeIds)
    {
        _offerSubscriptionId = default;

        var result = await _offerSubscriptionsRepository.GetOfferSubscriptionDataForProcessIdAsync(processId).ConfigureAwait(false);
        if (result == null)
        {
            throw new NotFoundException($"process {processId} does not exist");
        }
        if (result.OfferSubscriptionId == Guid.Empty)
        {
            throw new ConflictException($"process {processId} is not associated with an offerSubscription");
        }
        if (result.StatusId == OfferSubscriptionStatusId.ACTIVE)
        {
            throw new ConflictException($"offer subscription {result.OfferSubscriptionId} is already ACTIVE");
        }

        _offerSubscriptionId = result.OfferSubscriptionId;
        return new IProcessTypeExecutor.InitializationResult(false, null);
    }

    public async ValueTask<IProcessTypeExecutor.StepExecutionResult> ExecuteProcessStep(ProcessStepTypeId processStepTypeId, IEnumerable<ProcessStepTypeId> processStepTypeIds, CancellationToken cancellationToken)
    {
        if (_offerSubscriptionId == Guid.Empty)
        {
            throw new UnexpectedConditionException("offerSubscriptionId should never be null or empty here");
        }

        IEnumerable<ProcessStepTypeId>? nextStepTypeIds;
        IEnumerable<ProcessStepTypeId>? stepsToSkip;
        ProcessStepStatusId stepStatusId;
        bool modified;
        string? processMessage;

        try
        {
            (nextStepTypeIds, stepsToSkip, stepStatusId, modified, processMessage) = processStepTypeId switch
            {
                ProcessStepTypeId.TRIGGER_PROVIDER => await _offerSubscriptionService
                    .TriggerProvider(_offerSubscriptionId, _settings.ServiceManagerRoles)
                    .ConfigureAwait(false),
                ProcessStepTypeId.SINGLE_INSTANCE_SUBSCRIPTION_DETAILS_CREATION => await _offerSetupService
                    .CreateSingleInstanceSubscriptionDetail(_offerSubscriptionId)
                    .ConfigureAwait(false),
                ProcessStepTypeId.OFFERSUBSCRIPTION_CLIENT_CREATION => await _offerSetupService
                    .CreateClient(_offerSubscriptionId)
                    .ConfigureAwait(false),
                ProcessStepTypeId.OFFERSUBSCRIPTION_TECHNICALUSER_CREATION => await _offerSetupService
                    .CreateTechnicalUser(_offerSubscriptionId, _settings.ItAdminRoles, _settings.ServiceAccountRoles)
                    .ConfigureAwait(false),
                ProcessStepTypeId.ACTIVATE_SUBSCRIPTION => await _offerSetupService
                    .ActivateSubscription(_offerSubscriptionId, _settings.ItAdminRoles, _settings.BasePortalAddress)
                    .ConfigureAwait(false),
                _ => (null, null, ProcessStepStatusId.TODO, false, null)
            };
        }
        catch (Exception ex) when (ex is not SystemException)
        {
            (stepStatusId, processMessage) = ProcessError(ex);
            modified = true;
            nextStepTypeIds = null;
            stepsToSkip = null;
        }

        return new IProcessTypeExecutor.StepExecutionResult(modified, stepStatusId, nextStepTypeIds, stepsToSkip, processMessage);
    }

    private static (ProcessStepStatusId StatusId, string? ProcessMessage) ProcessError(Exception ex) =>
        ex is ServiceException { IsRecoverable: true }
            ? (ProcessStepStatusId.TODO, null)
            : (ProcessStepStatusId.FAILED, ex.Message);
}
