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

using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Linq;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Context;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

public class ServiceAccountManagement(IProvisioningManager provisioningManager, IPortalRepositories portalRepositories) : IServiceAccountManagement
{
    public async Task DeleteServiceAccount(Guid serviceAccountId, DeleteServiceAccountData result)
    {
        var userStatus = UserStatusId.DELETED;
        switch (result)
        {
            case { IsDimServiceAccount: true, CreationProcessInProgress: false }:
                userStatus = await CreateDeletionProcess(serviceAccountId, result.ProcessId).ConfigureAwait(ConfigureAwaitOptions.None);
                break;
            case { IsDimServiceAccount: true, CreationProcessInProgress: true }:
                throw ConflictException.Create(AdministrationServiceAccountErrors.TECHNICAL_USER_CREATION_IN_PROGRESS);
            default:
                if (!string.IsNullOrWhiteSpace(result.ClientClientId))
                {
                    await provisioningManager.DeleteCentralClientAsync(result.ClientClientId).ConfigureAwait(ConfigureAwaitOptions.None);
                }

                break;
        }

        portalRepositories.GetInstance<IUserRepository>().AttachAndModifyIdentity(
            serviceAccountId,
            i =>
            {
                i.UserStatusId = UserStatusId.PENDING;
            },
            i =>
            {
                i.UserStatusId = userStatus;
            });
        portalRepositories.GetInstance<IUserRolesRepository>().DeleteCompanyUserAssignedRoles(result.UserRoleIds.Select(userRoleId => (serviceAccountId, userRoleId)));
    }

    private async Task<UserStatusId> CreateDeletionProcess(Guid serviceAccountId, Guid? processId)
    {
        if (processId == null)
        {
            throw ConflictException.Create(AdministrationServiceAccountErrors.SERVICE_ACCOUNT_NOT_LINKED_TO_PROCESS, [new ErrorParameter("serviceAccountId", serviceAccountId.ToString())]);
        }

        var processData = await portalRepositories.GetInstance<ITechnicalUserRepository>()
            .GetProcessDataForTechnicalUserDeletionCallback(processId.Value, null)
            .ConfigureAwait(ConfigureAwaitOptions.None);

        var context = processData.ProcessData.CreateManualProcessData(null,
            portalRepositories, () => $"externalId {processId}");

        context.ProcessSteps.Where(step => step.ProcessStepTypeId != ProcessStepTypeId.DELETE_DIM_TECHNICAL_USER).IfAny(pending =>
            throw ConflictException.Create(AdministrationServiceAccountErrors.SERVICE_ACCOUNT_PENDING_PROCESS_STEPS, [new ErrorParameter("serviceAccountId", serviceAccountId.ToString()), new("processStepTypeIds", string.Join(",", pending))]));

        if (context.ProcessSteps.Any(step => step.ProcessStepTypeId == ProcessStepTypeId.DELETE_DIM_TECHNICAL_USER))
            return UserStatusId.DELETED;

        context.ScheduleProcessSteps([ProcessStepTypeId.DELETE_DIM_TECHNICAL_USER]);
        context.FinalizeProcessStep();
        return UserStatusId.PENDING_DELETION;
    }
}
