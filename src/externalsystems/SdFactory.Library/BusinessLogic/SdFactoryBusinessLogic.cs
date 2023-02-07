/********************************************************************************
 * Copyright (c) 2021, 2023 Microsoft and BMW Group AG
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

using Org.Eclipse.TractusX.Portal.Backend.Checklist.Library;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library.BusinessLogic;

public class SdFactoryBusinessLogic : ISdFactoryBusinessLogic
{
    private readonly ISdFactoryService _sdFactoryService;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IChecklistService _checklistService;

    public SdFactoryBusinessLogic(ISdFactoryService sdFactoryService, IPortalRepositories portalRepositories, IChecklistService checklistService)
    {
        _sdFactoryService = sdFactoryService;
        _portalRepositories = portalRepositories;
        _checklistService = checklistService;
    }

    /// <inheritdoc />
    public Task<Guid> RegisterConnectorAsync(
        string connectorUrl,
        string businessPartnerNumber,
        CancellationToken cancellationToken) =>
        _sdFactoryService.RegisterConnectorAsync(connectorUrl, businessPartnerNumber, cancellationToken);

    /// <inheritdoc />
    public async Task<(Action<ApplicationChecklistEntry>?,IEnumerable<ProcessStepTypeId>?,bool)> RegisterSelfDescription(IChecklistService.WorkerChecklistProcessStepData context, CancellationToken cancellationToken)
    {
        await RegisterSelfDescriptionInternalAsync(context.ApplicationId, cancellationToken)
            .ConfigureAwait(false);

        var prerequisiteEntries = context.Checklist.ExceptBy(new [] { ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION, ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP }, entry => entry.Key);
        if (prerequisiteEntries.All(entry => entry.Value == ApplicationChecklistEntryStatusId.DONE))
        {
            _checklistService.ScheduleProcessSteps(context, new [] { ProcessStepTypeId.ACTIVATE_APPLICATION });
        }
        return (entry => entry.ApplicationChecklistEntryStatusId = ApplicationChecklistEntryStatusId.DONE, null, true);
    }

    private async Task RegisterSelfDescriptionInternalAsync(
        Guid applicationId,
        CancellationToken cancellationToken)
    {
        var result = await _portalRepositories.GetInstance<IApplicationRepository>()
            .GetCompanyAndApplicationDetailsWithUniqueIdentifiersAsync(applicationId)
            .ConfigureAwait(false);
        if (result == default)
        {
            throw new ConflictException($"CompanyApplication {applicationId} is not in status SUBMITTED");
        }
        var (companyId, businessPartnerNumber, countryCode, uniqueIdentifiers) = result;

        if (string.IsNullOrWhiteSpace(businessPartnerNumber))
        {
            throw new ConflictException($"BusinessPartnerNumber (bpn) for CompanyApplications {applicationId} company {companyId} is empty");
        }

        Guid? documentId = await _sdFactoryService.RegisterSelfDescriptionAsync(uniqueIdentifiers, countryCode, businessPartnerNumber, cancellationToken).ConfigureAwait(false);
        _portalRepositories.GetInstance<ICompanyRepository>().AttachAndModifyCompany(companyId, null, c =>
        {
            c.SelfDescriptionDocumentId = documentId;
        });
    }
}
