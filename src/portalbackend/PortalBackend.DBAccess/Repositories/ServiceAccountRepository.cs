/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

public class ServiceAccountRepository : IServiceAccountRepository
{
    private readonly PortalDbContext _dbContext;

    public ServiceAccountRepository(PortalDbContext portalDbContext)
    {
        _dbContext = portalDbContext;
    }

    public CompanyServiceAccount CreateCompanyServiceAccount(
        Guid companyId,
        CompanyServiceAccountStatusId companyServiceAccountStatusId,
        string name,
        string description,
        CompanyServiceAccountTypeId companyServiceAccountTypeId,
        Action<CompanyServiceAccount>? setOptionalParameters = null)
    {
        var serviceAccount = _dbContext.CompanyServiceAccounts.Add(
            new CompanyServiceAccount(
                Guid.NewGuid(),
                companyId,
                companyServiceAccountStatusId,
                name,
                description,
                DateTimeOffset.UtcNow,
                companyServiceAccountTypeId)).Entity;
        setOptionalParameters?.Invoke(serviceAccount);
        return serviceAccount;
    }

    public void AttachAndModifyCompanyServiceAccount(
        Guid id,
        Action<CompanyServiceAccount>? initialize,
        Action<CompanyServiceAccount> modify)
    {
        var companyServiceAccount = new CompanyServiceAccount(
            id,
            Guid.Empty,
            default,
            null!,
            null!,
            default,
            default);
        initialize?.Invoke(companyServiceAccount);
        _dbContext.Attach(companyServiceAccount);
        modify(companyServiceAccount);
    }

    public IamServiceAccount CreateIamServiceAccount(string clientId, string clientClientId, string userEntityId, Guid companyServiceAccountId) =>
        _dbContext.IamServiceAccounts.Add(
            new IamServiceAccount(
                clientId,
                clientClientId,
                userEntityId,
                companyServiceAccountId)).Entity;

    public CompanyServiceAccountAssignedRole CreateCompanyServiceAccountAssignedRole(Guid companyServiceAccountId, Guid userRoleId) =>
        _dbContext.CompanyServiceAccountAssignedRoles.Add(
            new CompanyServiceAccountAssignedRole(
                companyServiceAccountId,
                userRoleId)).Entity;

    public IamServiceAccount RemoveIamServiceAccount(IamServiceAccount iamServiceAccount) =>
        _dbContext.Remove(iamServiceAccount).Entity;

    public CompanyServiceAccountAssignedRole RemoveCompanyServiceAccountAssignedRole(CompanyServiceAccountAssignedRole companyServiceAccountAssignedRole) =>
        _dbContext.Remove(companyServiceAccountAssignedRole).Entity;

    public Task<CompanyServiceAccountWithRoleDataClientId?> GetOwnCompanyServiceAccountWithIamClientIdAsync(Guid serviceAccountId, string adminUserId) =>
        _dbContext.CompanyServiceAccounts
            .Where(serviceAccount =>
                serviceAccount.Id == serviceAccountId
                && serviceAccount.CompanyServiceAccountStatusId == CompanyServiceAccountStatusId.ACTIVE
                && serviceAccount.ServiceAccountOwner!.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == adminUserId))
            .Select(serviceAccount => new CompanyServiceAccountWithRoleDataClientId(
                    serviceAccount.Id,
                    serviceAccount.CompanyServiceAccountStatusId,
                    serviceAccount.Name,
                    serviceAccount.Description,
                    serviceAccount.CompanyServiceAccountTypeId,
                    serviceAccount.OfferSubscriptionId,
                    serviceAccount.IamServiceAccount!.ClientId,
                    serviceAccount.IamServiceAccount.ClientClientId,
                    serviceAccount.CompanyServiceAccountAssignedRoles
                        .Select(assignedRole => assignedRole.UserRole)
                        .Select(userRole => new UserRoleData(
                            userRole!.Id,
                            userRole.Offer!.AppInstances.First().IamClient!.ClientClientId,
                            userRole.UserRoleText))))
            .SingleOrDefaultAsync();

    public Task<CompanyServiceAccount?> GetOwnCompanyServiceAccountWithIamServiceAccountRolesAsync(Guid serviceAccountId, string adminUserId) =>
        _dbContext.CompanyServiceAccounts
            .Where(serviceAccount =>
                serviceAccount.Id == serviceAccountId
                && serviceAccount.CompanyServiceAccountStatusId == CompanyServiceAccountStatusId.ACTIVE
                && serviceAccount.ServiceAccountOwner!.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == adminUserId))
            .Include(serviceAccount => serviceAccount.IamServiceAccount)
            .Include(serviceAccount => serviceAccount.CompanyServiceAccountAssignedRoles)
            .SingleOrDefaultAsync();

    public Task<CompanyServiceAccountDetailedData?> GetOwnCompanyServiceAccountDetailedDataUntrackedAsync(Guid serviceAccountId, string adminUserId) =>
        _dbContext.CompanyServiceAccounts
            .AsNoTracking()
            .Where(serviceAccount =>
                serviceAccount.Id == serviceAccountId
                && serviceAccount.CompanyServiceAccountStatusId == CompanyServiceAccountStatusId.ACTIVE
                && serviceAccount.ServiceAccountOwner!.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == adminUserId))
            .Select(serviceAccount => new CompanyServiceAccountDetailedData(
                    serviceAccount.Id,
                    serviceAccount.IamServiceAccount!.ClientId,
                    serviceAccount.IamServiceAccount.ClientClientId,
                    serviceAccount.IamServiceAccount.UserEntityId,
                    serviceAccount.Name,
                    serviceAccount.Description,
                    serviceAccount.CompanyServiceAccountAssignedRoles
                        .Select(assignedRole => assignedRole.UserRole)
                        .Select(userRole => new UserRoleData(
                            userRole!.Id,
                            userRole.Offer!.AppInstances.First().IamClient!.ClientClientId,
                            userRole.UserRoleText)),
                    serviceAccount.CompanyServiceAccountTypeId,
                    serviceAccount.OfferSubscriptionId))
            .SingleOrDefaultAsync();

    public Func<int,int,Task<Pagination.Source<CompanyServiceAccountData>?>> GetOwnCompanyServiceAccountsUntracked(string adminUserId) =>
        (skip, take) => Pagination.CreateSourceQueryAsync(
            skip,
            take,
            _dbContext.CompanyServiceAccounts
                .AsNoTracking()
                .Where(serviceAccount =>
                    serviceAccount.ServiceAccountOwner!.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == adminUserId) &&
                    serviceAccount.CompanyServiceAccountStatusId == CompanyServiceAccountStatusId.ACTIVE)
                .GroupBy(serviceAccount => serviceAccount.ServiceAccountOwnerId),
            serviceAccounts => serviceAccounts.OrderBy(serviceAccount => serviceAccount.Name),
            serviceAccount => new CompanyServiceAccountData(
                        serviceAccount.Id,
                        serviceAccount.IamServiceAccount!.ClientClientId,
                        serviceAccount.Name,
                        serviceAccount.CompanyServiceAccountTypeId,
                        serviceAccount.OfferSubscriptionId)
        ).SingleOrDefaultAsync();
}
