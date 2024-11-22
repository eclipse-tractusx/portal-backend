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
public class ApplicationChecklistHandlerService(
    IBpdmBusinessLogic bpdmBusinessLogic,
    ICustodianBusinessLogic custodianBusinessLogic,
    IClearinghouseBusinessLogic clearinghouseBusinessLogic,
    ISdFactoryBusinessLogic sdFactoryBusinessLogic,
    IDimBusinessLogic dimBusinessLogic,
    IIssuerComponentBusinessLogic issuerComponentBusinessLogic,
    IBpnDidResolverBusinessLogic bpnDidResolverBusinessLogic,
    IApplicationActivationService applicationActivationService,
    IApplicationChecklistService checklistService) : IApplicationChecklistHandlerService
{
    private readonly ImmutableDictionary<ProcessStepTypeId, IApplicationChecklistHandlerService.ProcessStepExecution> _stepExecutions = ImmutableDictionary.CreateRange<ProcessStepTypeId, IApplicationChecklistHandlerService.ProcessStepExecution>([
        new(ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PUSH, new(ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, true, bpdmBusinessLogic.PushLegalEntity, (ex, _, _) => checklistService.HandleServiceErrorAsync(ex, ProcessStepTypeId.RETRIGGER_BUSINESS_PARTNER_NUMBER_PUSH))),
        new(ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_PULL, new(ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, false, bpdmBusinessLogic.HandlePullLegalEntity, (ex, _, _) => checklistService.HandleServiceErrorAsync(ex, ProcessStepTypeId.RETRIGGER_BUSINESS_PARTNER_NUMBER_PULL))),
        new(ProcessStepTypeId.CREATE_IDENTITY_WALLET, new(ApplicationChecklistEntryTypeId.IDENTITY_WALLET, true, custodianBusinessLogic.CreateIdentityWalletAsync, (ex, _, _) => checklistService.HandleServiceErrorAsync(ex, ProcessStepTypeId.RETRIGGER_IDENTITY_WALLET))),
        new(ProcessStepTypeId.CREATE_DIM_WALLET, new(ApplicationChecklistEntryTypeId.IDENTITY_WALLET, true, dimBusinessLogic.CreateDimWalletAsync, (ex, _, _) => checklistService.HandleServiceErrorAsync(ex, ProcessStepTypeId.RETRIGGER_CREATE_DIM_WALLET))),
        new(ProcessStepTypeId.VALIDATE_DID_DOCUMENT, new(ApplicationChecklistEntryTypeId.IDENTITY_WALLET, true, dimBusinessLogic.ValidateDidDocument, (ex, _, _) => checklistService.HandleServiceErrorAsync(ex, ProcessStepTypeId.RETRIGGER_VALIDATE_DID_DOCUMENT))),
        new(ProcessStepTypeId.TRANSMIT_BPN_DID, new(ApplicationChecklistEntryTypeId.IDENTITY_WALLET, true, bpnDidResolverBusinessLogic.TransmitDidAndBpn, (ex, _, _) => checklistService.HandleServiceErrorAsync(ex, ProcessStepTypeId.RETRIGGER_TRANSMIT_DID_BPN))),
        new(ProcessStepTypeId.REQUEST_BPN_CREDENTIAL, new(ApplicationChecklistEntryTypeId.BPNL_CREDENTIAL, true, issuerComponentBusinessLogic.CreateBpnlCredential, (ex, _, _) => checklistService.HandleServiceErrorAsync(ex, ProcessStepTypeId.RETRIGGER_REQUEST_BPN_CREDENTIAL))),
        new(ProcessStepTypeId.REQUEST_MEMBERSHIP_CREDENTIAL, new(ApplicationChecklistEntryTypeId.MEMBERSHIP_CREDENTIAL, true, issuerComponentBusinessLogic.CreateMembershipCredential, (ex, _, _) => checklistService.HandleServiceErrorAsync(ex, ProcessStepTypeId.RETRIGGER_REQUEST_MEMBERSHIP_CREDENTIAL))),
        new(ProcessStepTypeId.START_CLEARING_HOUSE, new(ApplicationChecklistEntryTypeId.CLEARING_HOUSE, true, clearinghouseBusinessLogic.HandleClearinghouse, (ex, _, _) => checklistService.HandleServiceErrorAsync(ex, ProcessStepTypeId.RETRIGGER_CLEARING_HOUSE))),
        new(ProcessStepTypeId.START_OVERRIDE_CLEARING_HOUSE, new(ApplicationChecklistEntryTypeId.CLEARING_HOUSE, true, clearinghouseBusinessLogic.HandleClearinghouse, (ex, _, _) => checklistService.HandleServiceErrorAsync(ex, ProcessStepTypeId.RETRIGGER_CLEARING_HOUSE))),
        new(ProcessStepTypeId.START_SELF_DESCRIPTION_LP, new(ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP, true, sdFactoryBusinessLogic.StartSelfDescriptionRegistration, (ex, _, _) => checklistService.HandleServiceErrorAsync(ex, ProcessStepTypeId.RETRIGGER_SELF_DESCRIPTION_LP))),
        new(ProcessStepTypeId.START_APPLICATION_ACTIVATION, new(ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION, true, applicationActivationService.StartApplicationActivation, null)),
        new(ProcessStepTypeId.ASSIGN_INITIAL_ROLES, new(ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION, true, applicationActivationService.AssignRoles, (ex, _, _) => checklistService.HandleServiceErrorAsync(ex, ProcessStepTypeId.RETRIGGER_ASSIGN_INITIAL_ROLES))),
        new(ProcessStepTypeId.ASSIGN_BPN_TO_USERS, new(ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION, true, applicationActivationService.AssignBpn, (ex, _, _) => checklistService.HandleServiceErrorAsync(ex, ProcessStepTypeId.RETRIGGER_ASSIGN_BPN_TO_USERS))),
        new(ProcessStepTypeId.REMOVE_REGISTRATION_ROLES, new(ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION, true, applicationActivationService.RemoveRegistrationRoles, (ex, _, _) => checklistService.HandleServiceErrorAsync(ex, ProcessStepTypeId.RETRIGGER_REMOVE_REGISTRATION_ROLES))),
        new(ProcessStepTypeId.SET_THEME, new(ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION, true, applicationActivationService.SetTheme, (ex, _, _) => checklistService.HandleServiceErrorAsync(ex, ProcessStepTypeId.RETRIGGER_SET_THEME))),
        new(ProcessStepTypeId.SET_MEMBERSHIP, new(ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION, true, applicationActivationService.SetMembership, (ex, _, _) => checklistService.HandleServiceErrorAsync(ex, ProcessStepTypeId.RETRIGGER_SET_MEMBERSHIP))),
        new(ProcessStepTypeId.SET_CX_MEMBERSHIP_IN_BPDM, new(ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION, true, applicationActivationService.SetCxMembership, (ex, _, _) => checklistService.HandleServiceErrorAsync(ex, ProcessStepTypeId.RETRIGGER_SET_CX_MEMBERSHIP_IN_BPDM))),
        new(ProcessStepTypeId.FINISH_APPLICATION_ACTIVATION, new(ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION, true, applicationActivationService.SaveApplicationActivationToDatabase, null))
    ]);

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
