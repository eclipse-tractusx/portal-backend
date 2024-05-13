/********************************************************************************
 * Copyright (c) 2024 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.ApplicationChecklist.Library;

namespace Org.Eclipse.TractusX.Portal.Backend.BpnDidResolver.Library.BusinessLogic;

public class BpnDidResolverBusinessLogic : IBpnDidResolverBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly IBpnDidResolverService _bpnDidResolverService;

    public BpnDidResolverBusinessLogic(IPortalRepositories portalRepositories, IBpnDidResolverService bpnDidResolverService)
    {
        _portalRepositories = portalRepositories;
        _bpnDidResolverService = bpnDidResolverService;
    }

    public async Task<IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult> TransmitDidAndBpn(IApplicationChecklistService.WorkerChecklistProcessStepData context, CancellationToken cancellationToken)
    {
        if (context.Checklist[ApplicationChecklistEntryTypeId.IDENTITY_WALLET] != ApplicationChecklistEntryStatusId.IN_PROGRESS)
        {
            return new IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult(
                ProcessStepStatusId.FAILED,
                checklistEntry => checklistEntry.Comment =
                    $"processStep CREATE_IDENTITY_WALLET failed as entries IDENTITY_WALLET must have status {ApplicationChecklistEntryStatusId.IN_PROGRESS}",
                null,
                null,
                true,
                null);
        }

        await PostDidAndBpn(context.ApplicationId, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        return new IApplicationChecklistService.WorkerChecklistProcessStepExecutionResult(
            ProcessStepStatusId.DONE,
            checklist =>
            {
                checklist.ApplicationChecklistEntryStatusId = ApplicationChecklistEntryStatusId.DONE;
            },
            [ProcessStepTypeId.REQUEST_BPN_CREDENTIAL],
            null,
            true,
            null);
    }

    private async Task PostDidAndBpn(Guid applicationId, CancellationToken cancellationToken)
    {
        var (exists, did, bpn) = await _portalRepositories.GetInstance<IApplicationRepository>().GetDidAndBpnForApplicationId(applicationId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (!exists)
        {
            throw new NotFoundException($"CompanyApplication {applicationId} does not exist");
        }

        if (string.IsNullOrWhiteSpace(did))
        {
            throw new ConflictException("There must be a did set");
        }

        if (string.IsNullOrWhiteSpace(bpn))
        {
            throw new ConflictException("There must be a bpn set");
        }

        await _bpnDidResolverService.TransmitDidAndBpn(did, bpn, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
    }
}
