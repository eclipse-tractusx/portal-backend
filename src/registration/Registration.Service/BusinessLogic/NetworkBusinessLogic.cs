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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Linq;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;
using Org.Eclipse.TractusX.Portal.Backend.Processes.ApplicationChecklist.Library;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Model;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Registration.Service.BusinessLogic;

public class NetworkBusinessLogic : INetworkBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly IIdentityService _identityService;
    private readonly IApplicationChecklistCreationService _checklistService;

    public NetworkBusinessLogic(IPortalRepositories portalRepositories, IIdentityService identityService, IApplicationChecklistCreationService checklistService)
    {
        _portalRepositories = portalRepositories;
        _identityService = identityService;
        _checklistService = checklistService;
    }

    public async Task Submit(PartnerSubmitData submitData, CancellationToken cancellationToken)
    {
        var companyId = _identityService.IdentityData.CompanyId;
        var userId = _identityService.IdentityData.UserId;
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
                    .Select(processStepTypeId => (processStepTypeId, ProcessStepStatusId.TODO, processId)));

        _portalRepositories.GetInstance<IApplicationRepository>().AttachAndModifyCompanyApplication(companyApplication.CompanyApplicationId,
            ca =>
            {
                ca.ApplicationStatusId = CompanyApplicationStatusId.SUBMITTED;
                ca.ChecklistProcessId = processId;
            });
        _portalRepositories.GetInstance<IProcessStepRepository>().CreateProcessStepRange(Enumerable.Repeat(new ValueTuple<ProcessStepTypeId, ProcessStepStatusId, Guid>(ProcessStepTypeId.TRIGGER_CALLBACK_OSP_SUBMITTED, ProcessStepStatusId.TODO, data.ProcessId.Value), 1));

        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }
}
