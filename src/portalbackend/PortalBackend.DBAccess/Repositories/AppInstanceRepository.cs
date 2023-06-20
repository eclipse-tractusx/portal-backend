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

using Microsoft.EntityFrameworkCore;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

public class AppInstanceRepository : IAppInstanceRepository
{
    private readonly PortalDbContext _portalDbContext;

    /// <summary>
    /// Creates a new instance of <see cref="AppInstanceRepository"/>
    /// </summary>
    /// <param name="portalDbContext">The portal db context</param>
    public AppInstanceRepository(PortalDbContext portalDbContext)
    {
        _portalDbContext = portalDbContext;
    }

    /// <inheritdoc />
    public AppInstance CreateAppInstance(Guid appId, Guid iamClientId) =>
        _portalDbContext.AppInstances.Add(new AppInstance(Guid.NewGuid(), appId, iamClientId)).Entity;

    /// <inheritdoc />
    public void RemoveAppInstance(Guid appInstanceId) =>
        _portalDbContext.AppInstances.Remove(new AppInstance(appInstanceId, Guid.Empty, Guid.Empty));

    /// <inheritdoc />
    public void CreateAppInstanceAssignedServiceAccounts(
        IEnumerable<(Guid AppInstanceId, Guid CompanyServiceAccountId)> instanceAccounts) =>
        _portalDbContext.AppInstanceAssignedServiceAccounts.AddRange(instanceAccounts
            .Select(x => new AppInstanceAssignedCompanyServiceAccount(
                x.AppInstanceId,
                x.CompanyServiceAccountId)));

    /// <inheritdoc />
    public Task<bool> CheckInstanceExistsForOffer(Guid offerId) =>
        _portalDbContext.AppInstances.AnyAsync(ai => ai.AppId == offerId);

    /// <inheritdoc />
    public IAsyncEnumerable<Guid> GetAssignedServiceAccounts(Guid appInstanceId) =>
        _portalDbContext.AppInstanceAssignedServiceAccounts
            .Where(x => x.AppInstanceId == appInstanceId)
            .Select(x => x.CompanyServiceAccountId)
            .ToAsyncEnumerable();

    /// <inheritdoc />
    public Task<bool> CheckInstanceHasAssignedSubscriptions(Guid appInstanceId) =>
        _portalDbContext.AppSubscriptionDetails
            .Where(detail => detail.AppInstanceId == appInstanceId)
            .AnyAsync();

    /// <inheritdoc />
    public void RemoveAppInstanceAssignedServiceAccounts(Guid appInstanceId, IEnumerable<Guid> serviceAccountIds) =>
        _portalDbContext.AppInstanceAssignedServiceAccounts
            .RemoveRange(serviceAccountIds.Select(x => new AppInstanceAssignedCompanyServiceAccount(appInstanceId, x)));
}
