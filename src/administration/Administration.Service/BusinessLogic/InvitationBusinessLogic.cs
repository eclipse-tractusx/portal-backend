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

using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Context;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Extensions;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

public class InvitationBusinessLogic : IInvitationBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalRepositories">Portal Repositories</param>
    public InvitationBusinessLogic(
        IPortalRepositories portalRepositories)
    {
        _portalRepositories = portalRepositories;
    }

    public Task<CompanyInvitationResponse> ExecuteInvitation(CompanyInvitationData invitationData)
    {
        if (string.IsNullOrWhiteSpace(invitationData.Email))
        {
            throw new ControllerArgumentException("email must not be empty", "email");
        }

        if (!invitationData.OrganisationName.IsValidCompanyName())
        {
            throw ControllerArgumentException.Create(ValidationExpressionErrors.INCORRECT_COMPANY_NAME, [new ErrorParameter("name", nameof(invitationData.OrganisationName))]);
        }

        return ExecuteInvitationInternalAsync(invitationData);
    }

    private async Task<CompanyInvitationResponse> ExecuteInvitationInternalAsync(CompanyInvitationData invitationData)
    {
        var (userName, firstName, lastName, email, organisationName) = invitationData;
        var processStepRepository = _portalRepositories.GetInstance<IProcessStepRepository<ProcessTypeId, ProcessStepTypeId>>();
        var processId = processStepRepository.CreateProcess(ProcessTypeId.INVITATION).Id;
        processStepRepository.CreateProcessStep(ProcessStepTypeId.INVITATION_CREATE_CENTRAL_IDP, ProcessStepStatusId.TODO, processId);

        var company = _portalRepositories.GetInstance<ICompanyRepository>().CreateCompany(organisationName);

        var applicationRepository = _portalRepositories.GetInstance<IApplicationRepository>();
        var applicationId = applicationRepository.CreateCompanyApplication(company.Id, CompanyApplicationStatusId.CREATED, CompanyApplicationTypeId.INTERNAL).Id;
        _portalRepositories.GetInstance<ICompanyInvitationRepository>().CreateCompanyInvitation(applicationId, firstName, lastName, email, processId, ci =>
        {
            if (!string.IsNullOrWhiteSpace(userName))
            {
                ci.UserName = userName;
            }
        });

        await _portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
        return new CompanyInvitationResponse(applicationId, company.Id);
    }

    public Task RetriggerCreateCentralIdp(Guid processId) => ProcessStepTypeId.RETRIGGER_INVITATION_CREATE_CENTRAL_IDP.TriggerProcessStep(processId, _portalRepositories, ProcessTypeExtensions.GetProcessStepForRetrigger);
    public Task RetriggerCreateSharedIdpServiceAccount(Guid processId) => ProcessStepTypeId.RETRIGGER_INVITATION_CREATE_SHARED_IDP_SERVICE_ACCOUNT.TriggerProcessStep(processId, _portalRepositories, ProcessTypeExtensions.GetProcessStepForRetrigger);
    public Task RetriggerAddRealmRole(Guid processId) => ProcessStepTypeId.RETRIGGER_INVITATION_ADD_REALM_ROLE.TriggerProcessStep(processId, _portalRepositories, ProcessTypeExtensions.GetProcessStepForRetrigger);

    public Task RetriggerInviteSharedClient(Guid processId) => ProcessStepTypeId.RETRIGGER_INVITATION_CREATE_SHARED_CLIENT.TriggerProcessStep(processId, _portalRepositories, ProcessTypeExtensions.GetProcessStepForRetrigger);

    public Task RetriggerUpdateCentralIdpUrls(Guid processId) => ProcessStepTypeId.RETRIGGER_INVITATION_UPDATE_CENTRAL_IDP_URLS.TriggerProcessStep(processId, _portalRepositories, ProcessTypeExtensions.GetProcessStepForRetrigger);
    public Task RetriggerCreateCentralIdpOrgMapper(Guid processId) => ProcessStepTypeId.RETRIGGER_INVITATION_CREATE_CENTRAL_IDP_ORG_MAPPER.TriggerProcessStep(processId, _portalRepositories, ProcessTypeExtensions.GetProcessStepForRetrigger);
    public Task RetriggerCreateSharedRealmIdpClient(Guid processId) => ProcessStepTypeId.RETRIGGER_INVITATION_CREATE_SHARED_REALM.TriggerProcessStep(processId, _portalRepositories, ProcessTypeExtensions.GetProcessStepForRetrigger);
    public Task RetriggerEnableCentralIdp(Guid processId) => ProcessStepTypeId.RETRIGGER_INVITATION_ENABLE_CENTRAL_IDP.TriggerProcessStep(processId, _portalRepositories, ProcessTypeExtensions.GetProcessStepForRetrigger);
    public Task RetriggerCreateDatabaseIdp(Guid processId) => ProcessStepTypeId.RETRIGGER_INVITATION_CREATE_DATABASE_IDP.TriggerProcessStep(processId, _portalRepositories, ProcessTypeExtensions.GetProcessStepForRetrigger);
    public Task RetriggerInvitationCreateUser(Guid processId) => ProcessStepTypeId.RETRIGGER_INVITATION_CREATE_USER.TriggerProcessStep(processId, _portalRepositories, ProcessTypeExtensions.GetProcessStepForRetrigger);
}
