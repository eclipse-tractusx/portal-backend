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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;
using System.Text.RegularExpressions;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

public class InvitationBusinessLogic : IInvitationBusinessLogic
{
    private static readonly Regex Company = new(ValidationExpressions.Company, RegexOptions.Compiled, TimeSpan.FromSeconds(1));
    private readonly IProvisioningManager _provisioningManager;
    private readonly IUserProvisioningService _userProvisioningService;
    private readonly IPortalRepositories _portalRepositories;
    private readonly InvitationSettings _settings;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="provisioningManager">Provisioning Manager</param>
    /// <param name="userProvisioningService">User Provisioning Service</param>
    /// <param name="portalRepositories">Portal Repositories</param>
    /// <param name="settings">Settings</param>
    public InvitationBusinessLogic(
        IProvisioningManager provisioningManager,
        IUserProvisioningService userProvisioningService,
        IPortalRepositories portalRepositories,
        IOptions<InvitationSettings> settings)
    {
        _provisioningManager = provisioningManager;
        _userProvisioningService = userProvisioningService;
        _portalRepositories = portalRepositories;
        _settings = settings.Value;
    }

    public Task ExecuteInvitation(CompanyInvitationData invitationData)
    {
        if (string.IsNullOrWhiteSpace(invitationData.Email))
        {
            throw new ControllerArgumentException("email must not be empty", "email");
        }

        if (string.IsNullOrWhiteSpace(invitationData.OrganisationName))
        {
            throw new ControllerArgumentException("organisationName must not be empty", "organisationName");
        }

        if (!string.IsNullOrEmpty(invitationData.OrganisationName) && !Company.IsMatch(invitationData.OrganisationName))
        {
            throw new ControllerArgumentException("OrganisationName length must be 3-40 characters and *+=#%\\s not used as one of the first three characters in the Organisation name", "organisationName");
        }

        return ExecuteInvitationInternalAsync(invitationData);
    }

    private async Task ExecuteInvitationInternalAsync(CompanyInvitationData invitationData)
    {
        var (userName, firstName, lastName, email, organisationName) = invitationData;
        var processStepRepository = _portalRepositories.GetInstance<IProcessStepRepository>();
        var processId = processStepRepository.CreateProcess(ProcessTypeId.INVITATION).Id;
        processStepRepository.CreateProcessStep(ProcessStepTypeId.INVITATION_CREATE_CENTRAL_IDP, ProcessStepStatusId.TODO, processId);
        _portalRepositories.GetInstance<ICompanyInvitationRepository>().CreateCompanyInvitation(firstName, lastName, email, organisationName, processId, ci =>
            {
                if (!string.IsNullOrWhiteSpace(userName))
                {
                    ci.UserName = userName;
                }
            });
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
        var processStepRepository = _portalRepositories.GetInstance<IProcessStepRepository>();
        var processId = processStepRepository.CreateProcess(ProcessTypeId.MAILING).Id;
        processStepRepository.CreateProcessStep(ProcessStepTypeId.SEND_MAIL, ProcessStepStatusId.TODO, processId);
        var mailParameters = new Dictionary<string, string>
        {
            { "password", password ?? "" },
            { "companyName", invitationData.OrganisationName },
            { "url", _settings.RegistrationAppAddress },
            { "passwordResendUrl", _settings.PasswordResendAddress },
            { "closeApplicationUrl", _settings.CloseApplicationAddress },
        };

        _portalRepositories.GetInstance<IMailingInformationRepository>().CreateMailingInformation(processId, invitationData.Email, "RegistrationTemplate", mailParameters);
        _portalRepositories.GetInstance<IMailingInformationRepository>().CreateMailingInformation(processId, invitationData.Email, "PasswordForRegistrationTemplate", mailParameters);
    }

    public Task RetriggerCreateCentralIdp(Guid processId) => TriggerProcessStepInternal(processId, ProcessStepTypeId.RETRIGGER_INVITATION_CREATE_CENTRAL_IDP);
    public Task RetriggerCreateSharedIdpServiceAccount(Guid processId) => TriggerProcessStepInternal(processId, ProcessStepTypeId.RETRIGGER_INVITATION_CREATE_SHARED_IDP_SERVICE_ACCOUNT);
    public Task RetriggerUpdateCentralIdpUrls(Guid processId) => TriggerProcessStepInternal(processId, ProcessStepTypeId.RETRIGGER_INVITATION_UPDATE_CENTRAL_IDP_URLS);
    public Task RetriggerCreateCentralIdpOrgMapper(Guid processId) => TriggerProcessStepInternal(processId, ProcessStepTypeId.RETRIGGER_INVITATION_CREATE_CENTRAL_IDP_ORG_MAPPER);
    public Task RetriggerCreateSharedRealmIdpClient(Guid processId) => TriggerProcessStepInternal(processId, ProcessStepTypeId.RETRIGGER_INVITATION_CREATE_SHARED_REALM_IDP_CLIENT);
    public Task RetriggerEnableCentralIdp(Guid processId) => TriggerProcessStepInternal(processId, ProcessStepTypeId.RETRIGGER_INVITATION_ENABLE_CENTRAL_IDP);
    public Task RetriggerCreateDatabaseIdp(Guid processId) => TriggerProcessStepInternal(processId, ProcessStepTypeId.RETRIGGER_INVITATION_CREATE_DATABASE_IDP);
    public Task RetriggerInvitationCreateUser(Guid processId) => TriggerProcessStepInternal(processId, ProcessStepTypeId.RETRIGGER_INVITATION_CREATE_USER);
    public Task RetriggerInvitationSendMail(Guid processId) => TriggerProcessStepInternal(processId, ProcessStepTypeId.RETRIGGER_INVITATION_SEND_MAIL);

    private async Task TriggerProcessStepInternal(Guid processId, ProcessStepTypeId stepToTrigger)
    {
        var nextStep = stepToTrigger switch
        {
            ProcessStepTypeId.RETRIGGER_INVITATION_CREATE_CENTRAL_IDP => ProcessStepTypeId.INVITATION_CREATE_CENTRAL_IDP,
            ProcessStepTypeId.RETRIGGER_INVITATION_CREATE_SHARED_IDP_SERVICE_ACCOUNT => ProcessStepTypeId.INVITATION_CREATE_SHARED_IDP_SERVICE_ACCOUNT,
            ProcessStepTypeId.RETRIGGER_INVITATION_UPDATE_CENTRAL_IDP_URLS => ProcessStepTypeId.INVITATION_UPDATE_CENTRAL_IDP_URLS,
            ProcessStepTypeId.RETRIGGER_INVITATION_CREATE_CENTRAL_IDP_ORG_MAPPER => ProcessStepTypeId.INVITATION_CREATE_CENTRAL_IDP_ORG_MAPPER,
            ProcessStepTypeId.RETRIGGER_INVITATION_CREATE_SHARED_REALM_IDP_CLIENT => ProcessStepTypeId.INVITATION_CREATE_SHARED_REALM_IDP_CLIENT,
            ProcessStepTypeId.RETRIGGER_INVITATION_ENABLE_CENTRAL_IDP => ProcessStepTypeId.INVITATION_ENABLE_CENTRAL_IDP,
            ProcessStepTypeId.RETRIGGER_INVITATION_CREATE_USER => ProcessStepTypeId.INVITATION_CREATE_USER,
            ProcessStepTypeId.RETRIGGER_INVITATION_CREATE_DATABASE_IDP => ProcessStepTypeId.INVITATION_CREATE_DATABASE_IDP,
            ProcessStepTypeId.RETRIGGER_INVITATION_SEND_MAIL => ProcessStepTypeId.INVITATION_SEND_MAIL,
            _ => throw new UnexpectedConditionException($"Step {stepToTrigger} is not retriggerable")
        };

        var processStepRepository = _portalRepositories.GetInstance<IProcessStepRepository>();
        var (registrationIdExists, processData) = await processStepRepository.IsValidProcess(processId, ProcessTypeId.INVITATION, Enumerable.Repeat(stepToTrigger, 1)).ConfigureAwait(false);
        if (!registrationIdExists)
        {
            throw new NotFoundException($"process {processId} does not exist");
        }

        var context = processData.CreateManualProcessData(stepToTrigger, _portalRepositories, () => $"processId {processId}");

        context.ScheduleProcessSteps(Enumerable.Repeat(nextStep, 1));
        context.FinalizeProcessStep();
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }
}
