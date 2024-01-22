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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Linq;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;
using Org.Eclipse.TractusX.Portal.Backend.Processes.ApplicationChecklist.Library;
using Org.Eclipse.TractusX.Portal.Backend.Processes.Library;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Model;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Registration.Service.BusinessLogic;

public class NetworkBusinessLogic : INetworkBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly IIdentityData _identityData;
    private readonly IApplicationChecklistCreationService _checklistService;

    public NetworkBusinessLogic(IPortalRepositories portalRepositories, IIdentityService identityService, IApplicationChecklistCreationService checklistService)
    {
        _portalRepositories = portalRepositories;
        _identityData = identityService.IdentityData;
        _checklistService = checklistService;
    }

    public async Task Submit(PartnerSubmitData submitData)
    {
        var companyId = _identityData.CompanyId;
        var userId = _identityData.IdentityId;
        var data = await _portalRepositories.GetInstance<INetworkRepository>()
            .GetSubmitData(companyId)
            .ConfigureAwait(false);
        if (!data.Exists)
        {
            throw new NotFoundException($"Company {companyId} not found");
        }

        if (data.CompanyApplications.Count() != 1)
        {
            throw new ConflictException($"Company {companyId} has no or more than one application");
        }

        if (data.ProcessId == null)
        {
            throw new ConflictException("There must be an process");
        }

        var companyApplication = data.CompanyApplications.Single();
        if (companyApplication.CompanyApplicationStatusId != CompanyApplicationStatusId.CREATED)
        {
            throw new ConflictException($"Application {companyApplication.CompanyApplicationId} is not in state CREATED");
        }

        submitData.Agreements.Where(x => x.ConsentStatusId != ConsentStatusId.ACTIVE).IfAny(inactive =>
            throw new ControllerArgumentException($"All agreements must be agreed to. Agreements that are not active: {string.Join(",", inactive.Select(x => x.AgreementId))}", nameof(submitData.Agreements)));

        data.CompanyRoleAgreementIds
            .ExceptBy(submitData.CompanyRoles, x => x.CompanyRoleId)
            .IfAny(missing =>
                throw new ControllerArgumentException($"CompanyRoles {string.Join(",", missing.Select(x => x.CompanyRoleId))} are missing", nameof(submitData.CompanyRoles)));

        var requiredAgreementIds = data.CompanyRoleAgreementIds
            .SelectMany(x => x.AgreementIds)
            .Distinct().ToImmutableList();

        requiredAgreementIds.Except(submitData.Agreements.Where(x => x.ConsentStatusId == ConsentStatusId.ACTIVE).Select(x => x.AgreementId))
            .IfAny(missing =>
                throw new ControllerArgumentException($"All Agreements for the company roles must be agreed to, missing agreementIds: {string.Join(",", missing)}", nameof(submitData.Agreements)));

        _portalRepositories.GetInstance<IConsentRepository>()
            .CreateConsents(requiredAgreementIds.Select(agreementId => (agreementId, companyId, userId, ConsentStatusId.ACTIVE)));

        var entries = await _checklistService.CreateInitialChecklistAsync(companyApplication.CompanyApplicationId);
        var processId = _portalRepositories.GetInstance<IProcessStepRepository>().CreateProcess(ProcessTypeId.APPLICATION_CHECKLIST).Id;
        _portalRepositories.GetInstance<IProcessStepRepository>()
            .CreateProcessStepRange(
                _checklistService
                    .GetInitialProcessStepTypeIds(entries)
                    .Select(processStepTypeId => (processStepTypeId, ProcessStepStatusId.TODO, processId))
                    // in addition to the initial steps of new process application_checklist also create next step for process network_registration
                    .Append((ProcessStepTypeId.TRIGGER_CALLBACK_OSP_SUBMITTED, ProcessStepStatusId.TODO, data.ProcessId.Value)));

        _portalRepositories.GetInstance<IApplicationRepository>().AttachAndModifyCompanyApplication(companyApplication.CompanyApplicationId,
            ca =>
            {
                ca.ApplicationStatusId = CompanyApplicationStatusId.SUBMITTED;
                ca.ChecklistProcessId = processId;
            });

        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    public async Task DeclineOsp(Guid applicationId, DeclineOspData declineData)
    {
        var companyId = _identityData.CompanyId;
        var validStatus = new[]
        {
            CompanyApplicationStatusId.CREATED, CompanyApplicationStatusId.ADD_COMPANY_DATA,
            CompanyApplicationStatusId.INVITE_USER, CompanyApplicationStatusId.SELECT_COMPANY_ROLE,
            CompanyApplicationStatusId.UPLOAD_DOCUMENTS, CompanyApplicationStatusId.VERIFY
        };
        var networkRepository = _portalRepositories.GetInstance<INetworkRepository>();
        var data = await networkRepository.GetDeclineDataForApplicationId(applicationId, CompanyApplicationTypeId.EXTERNAL, validStatus, companyId).ConfigureAwait(false);
        if (!data.Exists)
        {
            throw new NotFoundException($"CompanyApplication {applicationId} does not exist");
        }

        if (!data.IsValidCompany)
        {
            throw new ForbiddenException($"User is not allowed to decline application {applicationId}");
        }

        if (!data.IsValidTypeId)
        {
            throw new ConflictException("Only external registrations can be declined");
        }

        if (!data.IsValidStatusId)
        {
            throw new ConflictException($"The status of the application {applicationId} must be one of the following: {string.Join(",", validStatus.Select(x => x.ToString()))}");
        }

        var (companyData, invitationData, processData) = data.Data!.Value;
        _portalRepositories.GetInstance<IApplicationRepository>().AttachAndModifyCompanyApplication(applicationId, ca => { ca.ApplicationStatusId = CompanyApplicationStatusId.CANCELLED_BY_CUSTOMER; });
        _portalRepositories.GetInstance<ICompanyRepository>().AttachAndModifyCompany(
            companyId,
            c => { c.CompanyStatusId = companyData.CompanyStatusId; },
            c => { c.CompanyStatusId = CompanyStatusId.REJECTED; });

        _portalRepositories.GetInstance<IInvitationRepository>().AttachAndModifyInvitations(invitationData.Select(
            x =>
                new ValueTuple<Guid, Action<Invitation>?, Action<Invitation>>(
                    x.InvitationId,
                    i => { i.InvitationStatusId = x.StatusId; },
                    i => { i.InvitationStatusId = InvitationStatusId.DECLINED; })));

        var context = processData.CreateManualProcessData(ProcessStepTypeId.MANUAL_DECLINE, _portalRepositories, () => $"applicationId {applicationId}");
        context.SkipProcessSteps(context.ProcessSteps.Where(x => x.ProcessStepStatusId == ProcessStepStatusId.TODO).Select(x => x.ProcessStepTypeId));
        context.ScheduleProcessSteps(Enumerable.Repeat(ProcessStepTypeId.REMOVE_KEYCLOAK_USERS, 1));
        context.FinalizeProcessStep();

        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }
}
