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

using Flurl.Http;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.OfferProvider.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Service;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.OfferSubscription.Executor.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Processes.OfferSubscription.Library.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.Processes.Worker.Library;
using System.Collections.Immutable;
using System.Net;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.OfferSubscription.Executor;

public class OfferSubscriptionProcessTypeExecutor : IProcessTypeExecutor
{
    private static readonly IEnumerable<int> RecoverableStatusCodes = ImmutableArray.Create(
        (int)HttpStatusCode.BadGateway,
        (int)HttpStatusCode.ServiceUnavailable,
        (int)HttpStatusCode.GatewayTimeout);

    private readonly IOfferProviderBusinessLogic _offerProviderBusinessLogic;
    private readonly IOfferSetupService _offerSetupService;
    private readonly IOfferSubscriptionsRepository _offerSubscriptionsRepository;

    private readonly IEnumerable<ProcessStepTypeId> _executableProcessSteps = ImmutableArray.Create(
        ProcessStepTypeId.TRIGGER_PROVIDER,
        ProcessStepTypeId.SINGLE_INSTANCE_SUBSCRIPTION_DETAILS_CREATION,
        ProcessStepTypeId.OFFERSUBSCRIPTION_CLIENT_CREATION,
        ProcessStepTypeId.OFFERSUBSCRIPTION_TECHNICALUSER_CREATION,
        ProcessStepTypeId.ACTIVATE_SUBSCRIPTION,
        ProcessStepTypeId.TRIGGER_PROVIDER_CALLBACK);

    private Guid _offerSubscriptionId;
    private readonly OfferSubscriptionsProcessSettings _settings;

    public OfferSubscriptionProcessTypeExecutor(
        IOfferProviderBusinessLogic offerProviderBusinessLogic,
        IOfferSetupService offerSetupService,
        IPortalRepositories portalRepositories,
        IOptions<OfferSubscriptionsProcessSettings> options)
    {
        _offerProviderBusinessLogic = offerProviderBusinessLogic;
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
        _offerSubscriptionId = Guid.Empty;

        var result = await _offerSubscriptionsRepository.GetOfferSubscriptionDataForProcessIdAsync(processId).ConfigureAwait(false);
        if (result == Guid.Empty)
        {
            throw new NotFoundException($"process {processId} does not exist or is not associated with an offer subscription");
        }

        _offerSubscriptionId = result;
        return new IProcessTypeExecutor.InitializationResult(false, null);
    }

    public async ValueTask<IProcessTypeExecutor.StepExecutionResult> ExecuteProcessStep(ProcessStepTypeId processStepTypeId, IEnumerable<ProcessStepTypeId> processStepTypeIds, CancellationToken cancellationToken)
    {
        if (_offerSubscriptionId == Guid.Empty)
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
                ProcessStepTypeId.TRIGGER_PROVIDER => await _offerProviderBusinessLogic
                    .TriggerProvider(_offerSubscriptionId, cancellationToken)
                    .ConfigureAwait(false),
                ProcessStepTypeId.SINGLE_INSTANCE_SUBSCRIPTION_DETAILS_CREATION => await _offerSetupService
                    .CreateSingleInstanceSubscriptionDetail(_offerSubscriptionId)
                    .ConfigureAwait(false),
                ProcessStepTypeId.OFFERSUBSCRIPTION_CLIENT_CREATION => await _offerSetupService
                    .CreateClient(_offerSubscriptionId)
                    .ConfigureAwait(false),
                ProcessStepTypeId.OFFERSUBSCRIPTION_TECHNICALUSER_CREATION => await _offerSetupService
                    .CreateTechnicalUser(_offerSubscriptionId, _settings.ItAdminRoles)
                    .ConfigureAwait(false),
                ProcessStepTypeId.ACTIVATE_SUBSCRIPTION => await _offerSetupService
                    .ActivateSubscription(_offerSubscriptionId, _settings.ItAdminRoles, _settings.ServiceManagerRoles, _settings.BasePortalAddress)
                    .ConfigureAwait(false),
                ProcessStepTypeId.TRIGGER_PROVIDER_CALLBACK => await _offerProviderBusinessLogic
                    .TriggerProviderCallback(_offerSubscriptionId, cancellationToken)
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
            ServiceException { IsRecoverable: true } => (ProcessStepStatusId.TODO, null, null),
            FlurlHttpException { StatusCode: { } } flurlHttpException when RecoverableStatusCodes.Contains(flurlHttpException.StatusCode.Value) => (ProcessStepStatusId.TODO, ex.Message, null),
            _ => (ProcessStepStatusId.FAILED, ex.Message, processStepTypeId.GetRetriggerStep())
        };
    }
}
