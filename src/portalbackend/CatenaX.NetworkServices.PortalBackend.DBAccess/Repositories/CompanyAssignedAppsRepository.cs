/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
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

using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using Microsoft.EntityFrameworkCore;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

/// Implementation of <see cref="ICompanyAssignedAppsRepository"/> accessing database with EF Core.
public class CompanyAssignedAppsRepository : ICompanyAssignedAppsRepository
{
    private readonly PortalDbContext _context;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalDbContext">PortalDb context.</param>
    public CompanyAssignedAppsRepository(PortalDbContext portalDbContext)
    {
        _context = portalDbContext;
    }

    /// <inheritdoc />
    public CompanyAssignedApp CreateCompanyAssignedApp(Guid appId, Guid companyId) =>
        _context.CompanyAssignedApps.Add(new CompanyAssignedApp(appId, companyId)).Entity;

    /// <inheritdoc />
    public IAsyncEnumerable<(Guid AppId, AppSubscriptionStatusId AppSubscriptionStatus)> GetCompanySubscribedAppSubscriptionStatusesForCompanyUntrackedAsync(Guid companyId) =>
        _context.CompanyAssignedApps.AsNoTracking()
            .Where(s => s.CompanyId == companyId)
            .Select(s => ((Guid AppId, AppSubscriptionStatusId AppSubscriptionStatus))
                new ValueTuple<Guid, AppSubscriptionStatusId>(s.AppId, s.AppSubscriptionStatusId))
            .ToAsyncEnumerable();

    /// <inheritdoc />
    public ValueTask<CompanyAssignedApp?> FindAsync(Guid companyId, Guid appId) =>
        _context.CompanyAssignedApps.FindAsync(companyId, appId);

    /// <inheritdoc />
    public IAsyncEnumerable<AppCompanySubscriptionStatusData> GetCompanyProvidedAppSubscriptionStatusesForUserAsync(Guid companyId) =>
        _context.CompanyAssignedApps.AsNoTracking()
            .Where(s => s.App!.ProviderCompanyId == companyId)
            .GroupBy(s => s.AppId)
            .Select(g => new AppCompanySubscriptionStatusData
            {
                AppId = g.Key,
                CompanySubscriptionStatuses = g.Select(s => 
                    new CompanySubscriptionStatusData(s.CompanyId,s.AppSubscriptionStatusId))
                    .ToList()
            })
            .ToAsyncEnumerable();
}
