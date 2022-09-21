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
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess

{
    public class PortalBackendDBAccess : IPortalBackendDBAccess
    {
        private readonly PortalDbContext _dbContext;

        public PortalBackendDBAccess(PortalDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<string?> GetSharedIdentityProviderIamAliasUntrackedAsync(string iamUserId) =>
            _dbContext.IamUsers
                .AsNoTracking()
                .Where(iamUser => iamUser.UserEntityId == iamUserId)
                .SelectMany(iamUser => iamUser.CompanyUser!.Company!.IdentityProviders
                    .Where(identityProvider => identityProvider.IdentityProviderCategoryId == IdentityProviderCategoryId.KEYCLOAK_SHARED)
                    .Select(identityProvider => identityProvider.IamIdentityProvider!.IamIdpAlias))
                .SingleOrDefaultAsync();

        public IAsyncEnumerable<CompanyUser> GetCompanyUserRolesIamUsersAsync(IEnumerable<Guid> companyUserIds, string iamUserId) =>
            _dbContext.CompanyUsers
                .Where(companyUser => companyUser.IamUser!.UserEntityId == iamUserId)
                .SelectMany(companyUser => companyUser.Company!.CompanyUsers)
                .Where(companyUser => companyUserIds.Contains(companyUser.Id) && companyUser.IamUser!.UserEntityId != null)
                .Include(companyUser => companyUser.CompanyUserAssignedRoles)
                .Include(companyUser => companyUser.IamUser)
                .AsAsyncEnumerable();

        public CompanyUserAssignedRole RemoveCompanyUserAssignedRole(CompanyUserAssignedRole companyUserAssignedRole) =>
            _dbContext.Remove(companyUserAssignedRole).Entity;

        public IamUser RemoveIamUser(IamUser iamUser) =>
            _dbContext.Remove(iamUser).Entity;

        public Task<IdpUser?> GetIdpCategoryIdByUserIdAsync(Guid companyUserId, string adminUserId) =>
            _dbContext.CompanyUsers.AsNoTracking()
                .Where(companyUser => companyUser.Id == companyUserId
                    && companyUser.Company!.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == adminUserId))
                .Select(companyUser => new IdpUser
                {
                    TargetIamUserId = companyUser.IamUser!.UserEntityId,
                    IdpName = companyUser.Company!.IdentityProviders
                        .Where(identityProvider => identityProvider.IdentityProviderCategoryId == IdentityProviderCategoryId.KEYCLOAK_SHARED)
                        .Select(identityProvider => identityProvider.IamIdentityProvider!.IamIdpAlias)
                        .SingleOrDefault()
                }).SingleOrDefaultAsync();

        public Task<int> SaveAsync() =>
            _dbContext.SaveChangesAsync();
    }
}
