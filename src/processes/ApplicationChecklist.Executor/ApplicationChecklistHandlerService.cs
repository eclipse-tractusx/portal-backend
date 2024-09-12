/********************************************************************************
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.ApplicationActivation.Library;
using Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.BpnDidResolver.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Clearinghouse.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Custodian.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Dim.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.IssuerComponent.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.ApplicationChecklist.Library;
using Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library.BusinessLogic;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.ApplicationChecklist.Executor;

/// <inheritdoc />
public class ApplicationChecklistHandlerService : IApplicationChecklistHandlerService
{
    private readonly IBpdmBusinessLogic _bpdmBusinessLogic;
    private readonly ICustodianBusinessLogic _custodianBusinessLogic;
    private readonly IClearinghouseBusinessLogic _clearinghouseBusinessLogic;
    private readonly ISdFactoryBusinessLogic _sdFactoryBusinessLogic;
    private readonly IDimBusinessLogic _dimBusinessLogic;
    private readonly IIssuerComponentBusinessLogic _issuerComponentBusinessLogic;
    private readonly IBpnDidResolverBusinessLogic _bpnDidResolverBusinessLogic;
    private readonly IApplicationActivationService _applicationActivationService;
    private readonly IApplicationChecklistService _checklistService;

    private readonly ImmutableDictionary<ProcessStepTypeId, IApplicationChecklistHandlerService.ProcessStepExecution> _stepExecutions;

    /// <inheritdoc />
    public ApplicationChecklistHandlerService(
        IBpdmBusinessLogic bpdmBusinessLogic,
        ICustodianBusinessLogic custodianBusinessLogic,
        IClearinghouseBusinessLogic clearinghouseBusinessLogic,
        ISdFactoryBusinessLogic sdFactoryBusinessLogic,
        IDimBusinessLogic dimBusinessLogic,
        IIssuerComponentBusinessLogic issuerComponentBusinessLogic,
        IBpnDidResolverBusinessLogic bpnDidResolverBusinessLogic,
        IApplicationActivationService applicationActivationService,
        IApplicationChecklistService checklistService)
    {
        _bpdmBusinessLogic = bpdmBusinessLogic;
        _custodianBusinessLogic = custodianBusinessLogic;
        _clearinghouseBusinessLogic = clearinghouseBusinessLogic;
        _sdFactoryBusinessLogic = sdFactoryBusinessLogic;
        _dimBusinessLogic = dimBusinessLogic;
        _issuerComponentBusinessLogic = issuerComponentBusinessLogic;
        _bpnDidResolverBusinessLogic = bpnDidResolverBusinessLogic;
        _applicationActivationService = applicationActivationService;
        _checklistService = checklistService;

        _stepExecutions = new (ProcessStepTypeId ProcessStepTypeId, IApplicationChecklistHandlerService.ProcessStepExecution StepExecution)[]
        {
            (ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PUSH, new (ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, true, (context, cancellationToken) => _bpdmBusinessLogic.PushLegalEntity(context, cancellationToken), (ex, _, _) => _checklistService.HandleServiceErrorAsync(ex, ProcessStepTypeId.RETRIGGER_BUSINESS_PARTNER_NUMBER_PUSH))),
            (ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PULL, new (ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, false, (context, cancellationToken) => _bpdmBusinessLogic.HandlePullLegalEntity(context, cancellationToken), (ex, _, _) => _checklistService.HandleServiceErrorAsync(ex, ProcessStepTypeId.RETRIGGER_BUSINESS_PARTNER_NUMBER_PULL))),
            (ProcessStepTypeId.CREATE_IDENTITY_WALLET, new (ApplicationChecklistEntryTypeId.IDENTITY_WALLET, true, (context, cancellationToken) => _custodianBusinessLogic.CreateIdentityWalletAsync(context, cancellationToken), (ex, _, _) => _checklistService.HandleServiceErrorAsync(ex, ProcessStepTypeId.RETRIGGER_IDENTITY_WALLET))),
            (ProcessStepTypeId.CREATE_DIM_WALLET, new (ApplicationChecklistEntryTypeId.IDENTITY_WALLET, true, (context, cancellationToken) => _dimBusinessLogic.CreateDimWalletAsync(context, cancellationToken), (ex, _, _) => _checklistService.HandleServiceErrorAsync(ex, ProcessStepTypeId.RETRIGGER_CREATE_DIM_WALLET))),
            (ProcessStepTypeId.VALIDATE_DID_DOCUMENT, new (ApplicationChecklistEntryTypeId.IDENTITY_WALLET, true, (context, cancellationToken) => _dimBusinessLogic.ValidateDidDocument(context, cancellationToken), (ex, _, _) => _checklistService.HandleServiceErrorAsync(ex, ProcessStepTypeId.RETRIGGER_VALIDATE_DID_DOCUMENT))),
            (ProcessStepTypeId.TRANSMIT_BPN_DID, new (ApplicationChecklistEntryTypeId.IDENTITY_WALLET, true, (context, cancellationToken) => _bpnDidResolverBusinessLogic.TransmitDidAndBpn(context, cancellationToken), (ex, _, _) => _checklistService.HandleServiceErrorAsync(ex, ProcessStepTypeId.RETRIGGER_TRANSMIT_DID_BPN))),
            (ProcessStepTypeId.REQUEST_BPN_CREDENTIAL, new (ApplicationChecklistEntryTypeId.BPNL_CREDENTIAL, true, (context, cancellationToken) => _issuerComponentBusinessLogic.CreateBpnlCredential(context, cancellationToken), (ex, _, _) => _checklistService.HandleServiceErrorAsync(ex, ProcessStepTypeId.RETRIGGER_REQUEST_BPN_CREDENTIAL))),
            (ProcessStepTypeId.REQUEST_MEMBERSHIP_CREDENTIAL, new (ApplicationChecklistEntryTypeId.MEMBERSHIP_CREDENTIAL, true, (context, cancellationToken) => _issuerComponentBusinessLogic.CreateMembershipCredential(context, cancellationToken), (ex, _, _) => _checklistService.HandleServiceErrorAsync(ex, ProcessStepTypeId.RETRIGGER_REQUEST_MEMBERSHIP_CREDENTIAL))),
            (ProcessStepTypeId.START_CLEARING_HOUSE, new (ApplicationChecklistEntryTypeId.CLEARING_HOUSE, true, (context, cancellationToken) => _clearinghouseBusinessLogic.HandleClearinghouse(context, cancellationToken), (ex, _, _) => _checklistService.HandleServiceErrorAsync(ex, ProcessStepTypeId.RETRIGGER_CLEARING_HOUSE))),
            (ProcessStepTypeId.START_OVERRIDE_CLEARING_HOUSE, new (ApplicationChecklistEntryTypeId.CLEARING_HOUSE, true, (context, cancellationToken) => _clearinghouseBusinessLogic.HandleClearinghouse(context, cancellationToken), (ex, _, _) => _checklistService.HandleServiceErrorAsync(ex, ProcessStepTypeId.RETRIGGER_CLEARING_HOUSE))),
            (ProcessStepTypeId.START_SELF_DESCRIPTION_LP, new (ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP, true, (context, cancellationToken) => _sdFactoryBusinessLogic.StartSelfDescriptionRegistration(context, cancellationToken), (ex, _, _) => _checklistService.HandleServiceErrorAsync(ex, ProcessStepTypeId.RETRIGGER_SELF_DESCRIPTION_LP))),
            (ProcessStepTypeId.ACTIVATE_APPLICATION, new (ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION, true, (context, cancellationToken) => _applicationActivationService.HandleApplicationActivation(context, cancellationToken), null)),
        }.ToImmutableDictionary(x => x.ProcessStepTypeId, x => x.StepExecution);
    }

    /// <inheritdoc />
    public IApplicationChecklistHandlerService.ProcessStepExecution GetProcessStepExecution(ProcessStepTypeId stepTypeId)
    {
        if (!_stepExecutions.TryGetValue(stepTypeId, out var execution))
        {
            throw new ConflictException($"no execution defined for processStep {stepTypeId}");
        }

        return execution;
    }

    /// <inheritdoc />
    public bool IsExecutableProcessStep(ProcessStepTypeId stepTypeId) => _stepExecutions.ContainsKey(stepTypeId);

    /// <inheritdoc />
    public IEnumerable<ProcessStepTypeId> GetExecutableStepTypeIds() => _stepExecutions.Keys;
}
