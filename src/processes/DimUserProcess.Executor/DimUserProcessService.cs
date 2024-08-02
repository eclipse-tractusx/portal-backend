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

using Org.Eclipse.TractusX.Portal.Backend.Dim.Library;
using Org.Eclipse.TractusX.Portal.Backend.Dim.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.DimUserCreationProcess.Executor;

public class DimUserProcessService(
    IDimService dimService,
    IPortalRepositories portalRepositories) : IDimUserProcessService
{
    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> CreateDeleteDimUser(Guid processId, Guid dimServiceAccountId, bool createUser, CancellationToken cancellationToken)
    {
        var serviceAccountRepository = portalRepositories.GetInstance<IServiceAccountRepository>();
        var (isValid, bpn, name) = await serviceAccountRepository.GetDimServiceAccountData(dimServiceAccountId)
            .ConfigureAwait(ConfigureAwaitOptions.None);

        if (!isValid)
        {
            throw new NotFoundException($"DimServiceAccountId {dimServiceAccountId} does not exist");
        }

        if (string.IsNullOrWhiteSpace(bpn))
        {
            throw new ConflictException("Bpn must not be null");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ConflictException("Service Account Name must not be empty");
        }

        var dimName = string.Concat(name.Where(c => !char.IsWhiteSpace(c))); // DIM doesn't accept whitespace chars in name
        if (createUser)
        {
            await dimService.CreateTechnicalUser(bpn, new TechnicalUserData(processId, dimName), cancellationToken).ConfigureAwait(false);
        }
        else
        {
            await dimService.DeleteTechnicalUser(bpn, new TechnicalUserData(processId, dimName), cancellationToken).ConfigureAwait(false);
        }

        return (Enumerable.Repeat(createUser ? ProcessStepTypeId.AWAIT_CREATE_DIM_TECHNICAL_USER_RESPONSE : ProcessStepTypeId.AWAIT_DELETE_DIM_TECHNICAL_USER, 1), ProcessStepStatusId.DONE, true, null);
    }
}
