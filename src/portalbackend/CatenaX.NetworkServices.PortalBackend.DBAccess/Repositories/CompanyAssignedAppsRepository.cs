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
        this._context = portalDbContext;
    }

    /// <inheritdoc />
    public async Task UpdateSubscriptionStatusAsync(Guid companyId, Guid appId, AppSubscriptionStatusId statusId)
    {
        var subscription = await this.GetActiveSubscriptionByCompanyAndAppIdAsync(companyId, appId).ConfigureAwait(false);
        if (subscription is null)
        {
            throw new ArgumentException($"There is no active subscription for company '{companyId}' and app '{appId}'", nameof(subscription));
        }

        subscription.AppSubscriptionStatusId = statusId;
        await this._context.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByAppAndCompanyIdAsync(Guid appId, Guid companyId) =>
        await this._context.CompanyAssignedApps.AnyAsync(x => x.AppId == appId && x.CompanyId == companyId);

    private async Task<CompanyAssignedApp?> GetActiveSubscriptionByCompanyAndAppIdAsync(Guid companyId, Guid appId)
    {
        return await this._context.CompanyAssignedApps
            .SingleOrDefaultAsync(x => x.CompanyId == companyId && x.AppId == appId && x.AppSubscriptionStatusId == AppSubscriptionStatusId.ACTIVE);
    }
}
