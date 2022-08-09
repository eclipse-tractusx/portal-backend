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
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
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
    public CompanyAssignedApp CreateCompanyAssignedApp(Guid appId, Guid companyId, AppSubscriptionStatusId appSubscriptionStatusId ) =>
        _context.CompanyAssignedApps.Add(new CompanyAssignedApp(appId, companyId, appSubscriptionStatusId)).Entity;

    public IQueryable<CompanyUser> GetOwnCompanyAppUsersUntrackedAsync(
        Guid appId,
        string iamUserId,
        string? firstName = null,
        string? lastName = null,
        string? email = null,
        string? roleName = null) =>
            _context.CompanyUsers
                .Where(companyUser => companyUser.UserRoles.Any(userRole => userRole.IamClient!.Apps.Any(app => app.Id == appId))
                 && companyUser.IamUser!.UserEntityId == iamUserId)
                .SelectMany(companyUser => companyUser.Company!.CompanyUsers)
                .Where(companyUser => firstName != null ? EF.Functions.ILike(companyUser!.Firstname, $"{firstName}%") : true
                    && lastName != null ? EF.Functions.ILike(companyUser!.Lastname, $"{lastName}%") : true
                    && email != null ? EF.Functions.ILike(companyUser!.Email, $"{email}%") : true
                    && roleName != null ? companyUser.UserRoles.Any(userRole => EF.Functions.ILike(userRole.UserRoleText, $"{roleName}%")) : true);

    /// <inheritdoc />
    public IAsyncEnumerable<AppWithSubscriptionStatus> GetOwnCompanySubscribedAppSubscriptionStatusesUntrackedAsync(string iamUserId) =>
        _context.IamUsers.AsNoTracking()
            .Where(iamUser => iamUser.UserEntityId == iamUserId)
            .SelectMany(iamUser => iamUser.CompanyUser!.Company!.CompanyAssignedApps)
            .Select(s => new AppWithSubscriptionStatus(s.AppId, s.AppSubscriptionStatusId))
            .ToAsyncEnumerable();

    /// <inheritdoc />
    public IAsyncEnumerable<AppCompanySubscriptionStatusData> GetOwnCompanyProvidedAppSubscriptionStatusesUntrackedAsync(string iamUserId) =>
        _context.CompanyAssignedApps.AsNoTracking()
            .Where(s => s.App!.ProviderCompany!.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == iamUserId))
            .GroupBy(s => s.AppId)
            .Select(g => new AppCompanySubscriptionStatusData
            {
                AppId = g.Key,
                CompanySubscriptionStatuses = g.Select(s =>
                    new CompanySubscriptionStatusData(s.CompanyId, s.AppSubscriptionStatusId))
            })
            .ToAsyncEnumerable();

    /// <inheritdoc />
    public Task<(CompanyAssignedApp? companyAssignedApp, bool isMemberOfCompanyProvidingApp)> GetCompanyAssignedAppDataForProvidingCompanyUserAsync(Guid appId, Guid companyId, string iamUserId) =>
        _context.Apps
            .Where(app => app.Id == appId)
            .Select(app => ((CompanyAssignedApp? companyAssignedApp, bool isMemberOfCompanyProvidingApp)) new (
                app!.CompanyAssignedApps.Where(assignedApp => assignedApp.CompanyId == companyId).SingleOrDefault(),
                app.ProviderCompany!.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == iamUserId)
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<(CompanyAssignedApp? companyAssignedApp, bool _)> GetCompanyAssignedAppDataForCompanyUserAsync(Guid appId, string iamUserId) =>
        _context.Apps
            .Where(app => app.Id == appId)
            .Select(app => ((CompanyAssignedApp? companyAssignedApp, bool _)) new (
                app!.CompanyAssignedApps.Where(assignedApp => assignedApp.Company!.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == iamUserId)).SingleOrDefault(),
                true
            ))
            .SingleOrDefaultAsync();

    public Task<(Guid companyId, CompanyAssignedApp? companyAssignedApp)> GetCompanyIdWithAssignedAppForCompanyUserAsync(Guid appId, string iamUserId) =>
        _context.IamUsers
            .Where(iamUser => iamUser.UserEntityId == iamUserId)
            .Select(iamUser => iamUser.CompanyUser!.Company)
            .Select(company => ((Guid companyId, CompanyAssignedApp?)) new (
                company!.Id,
                company.CompanyAssignedApps.SingleOrDefault(assignedApp => assignedApp.AppId == appId)
            ))
            .SingleOrDefaultAsync();
}
