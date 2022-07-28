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

using CatenaX.NetworkServices.Framework.Models;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using Microsoft.EntityFrameworkCore;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

/// Implementation of <see cref="IUserRepository"/> accessing database with EF Core.
public class UserRepository : IUserRepository
{
    private readonly PortalDbContext _dbContext;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="portalDbContext">PortalDb context.</param>
    public UserRepository(PortalDbContext portalDbContext)
    {
        _dbContext = portalDbContext;
    }

    public CompanyUser CreateCompanyUser(string? firstName, string? lastName, string email, Guid companyId,
        CompanyUserStatusId companyUserStatusId) =>
        _dbContext.CompanyUsers.Add(
            new CompanyUser(
                Guid.NewGuid(),
                companyId,
                companyUserStatusId,
                DateTimeOffset.UtcNow)
            {
                Firstname = firstName,
                Lastname = lastName,
                Email = email,
            }).Entity;

    public IamUser CreateIamUser(CompanyUser companyUser, string iamUserId) =>
        _dbContext.IamUsers.Add(
            new IamUser(
                iamUserId,
                companyUser.Id)).Entity;

    public IQueryable<CompanyUser> GetOwnCompanyUserQuery(
        string adminUserId,
        Guid? companyUserId = null,
        string? userEntityId = null,
        string? firstName = null,
        string? lastName = null,
        string? email = null) =>
        _dbContext.CompanyUsers
            .Where(companyUser => companyUser.IamUser!.UserEntityId == adminUserId)
            .SelectMany(companyUser => companyUser.Company!.CompanyUsers)
            .Where(companyUser =>
                userEntityId != null ? companyUser.IamUser!.UserEntityId == userEntityId : true
                && companyUserId.HasValue ? companyUser.Id == companyUserId!.Value : true
                && firstName != null ? companyUser.Firstname == firstName : true
                && lastName != null ? companyUser.Lastname == lastName : true
                && email != null ? companyUser.Email == email : true);

    public Task<bool> IsOwnCompanyUserWithEmailExisting(string email, string adminUserId) =>
        _dbContext.IamUsers
            .Where(iamUser => iamUser.UserEntityId == adminUserId)
            .SelectMany(iamUser => iamUser.CompanyUser!.Company!.CompanyUsers)
            .AnyAsync(companyUser => companyUser!.Email == email);

    public Task<CompanyUserDetails?> GetOwnCompanyUserDetailsUntrackedAsync(Guid companyUserId, string iamUserId) =>
        _dbContext.CompanyUsers
            .AsNoTracking()
            .Where(companyUser => companyUser.Id == companyUserId
                                  && companyUser.CompanyUserStatusId == CompanyUserStatusId.ACTIVE
                                  && companyUser.Company!.CompanyUsers.Any(companyUser =>
                                      companyUser.IamUser!.UserEntityId == iamUserId))
            .Select(companyUser => new CompanyUserDetails(
                companyUser.Id,
                companyUser.DateCreated,
                companyUser.CompanyUserAssignedBusinessPartners.Select(assignedPartner =>
                    assignedPartner.BusinessPartnerNumber),
                companyUser.Company!.Name,
                companyUser.CompanyUserStatusId)
            {
                FirstName = companyUser.Firstname,
                LastName = companyUser.Lastname,
                Email = companyUser.Email
            })
            .SingleOrDefaultAsync();

    public Task<CompanyUserBusinessPartners?> GetOwnCompanyUserWithAssignedBusinessPartnerNumbersUntrackedAsync(
        Guid companyUserId, string adminUserId) =>
        _dbContext.IamUsers
            .AsNoTracking()
            .Where(iamUser => iamUser.UserEntityId == adminUserId)
            .SelectMany(iamUser => iamUser.CompanyUser!.Company!.CompanyUsers)
            .Where(companyUser => companyUser.Id == companyUserId)
            .Select(companyUser => new CompanyUserBusinessPartners(
                companyUser.IamUser!.UserEntityId,
                companyUser.CompanyUserAssignedBusinessPartners.Select(assignedPartner =>
                    assignedPartner.BusinessPartnerNumber)
            ))
            .SingleOrDefaultAsync();

    public Task<Guid> GetCompanyIdForIamUserUntrackedAsync(string iamUserId) =>
        _dbContext.IamUsers
            .AsNoTracking()
            .Where(iamUser =>
                iamUser.UserEntityId == iamUserId)
            .Select(iamUser =>
                iamUser.CompanyUser!.Company!.Id)
            .SingleOrDefaultAsync();

    public Task<(Guid CompanyId, string? CompanyName, string? BusinessPartnerNumber, string? IdpAlias)> GetCompanyNameIdpAliasUntrackedAsync(string iamUserId) =>
        _dbContext.IamUsers
            .AsNoTracking()
            .Where(iamUser => iamUser.UserEntityId == iamUserId)
            .Select(iamUser => iamUser!.CompanyUser!.Company)
            .Select(company => ((Guid CompanyId, string? CompanyName, string? BusinessPartnerNumber, string? IdpAlias))new (
                company!.Id,
                company.Name,
                company!.BusinessPartnerNumber,
                company!.IdentityProviders
                    .Where(identityProvider => identityProvider.IdentityProviderCategoryId == IdentityProviderCategoryId.KEYCLOAK_SHARED)
                    .SingleOrDefault()!.IamIdentityProvider!.IamIdpAlias)).SingleOrDefaultAsync();

    /// <inheritdoc/>
    public Task<CompanyIamUser?> GetIdpUserByIdUntrackedAsync(Guid companyUserId, string adminUserId) =>
        _dbContext.CompanyUsers.AsNoTracking()
            .Where(companyUser => companyUser.Id == companyUserId
                                  && companyUser.Company!.CompanyUsers.Any(companyUser =>
                                      companyUser.IamUser!.UserEntityId == adminUserId))
            .Select(companyUser => new CompanyIamUser
            {
                TargetIamUserId = companyUser.IamUser!.UserEntityId,
                IdpName = companyUser.Company!.IdentityProviders
                    .Select(identityProvider => identityProvider.IamIdentityProvider!.IamIdpAlias)
                    .SingleOrDefault(),
                RoleIds = companyUser.CompanyUserAssignedRoles.Select(companyUserAssignedRole =>
                    companyUserAssignedRole.UserRoleId)
            }).SingleOrDefaultAsync();

    public Task<CompanyUserDetails?> GetUserDetailsUntrackedAsync(string iamUserId) =>
        _dbContext.CompanyUsers
            .AsNoTracking()
            .Where(companyUser => companyUser.IamUser!.UserEntityId == iamUserId)
            .Select(companyUser => new CompanyUserDetails(
                companyUser.Id,
                companyUser.DateCreated,
                companyUser.CompanyUserAssignedBusinessPartners.Select(assignedPartner =>
                    assignedPartner.BusinessPartnerNumber),
                companyUser.Company!.Name,
                companyUser.CompanyUserStatusId)
            {
                FirstName = companyUser.Firstname,
                LastName = companyUser.Lastname,
                Email = companyUser.Email
            })
            .SingleOrDefaultAsync();

    public Task<CompanyUserWithIdpBusinessPartnerData?> GetUserWithCompanyIdpAsync(string iamUserId) =>
        _dbContext.CompanyUsers
            .Where(companyUser => companyUser.IamUser!.UserEntityId == iamUserId
                                  && companyUser!.Company!.IdentityProviders
                                      .Any(identityProvider =>
                                          identityProvider.IdentityProviderCategoryId ==
                                          IdentityProviderCategoryId.KEYCLOAK_SHARED))
            .Include(companyUser => companyUser.Company)
            .Include(companyUser => companyUser.IamUser)
            .Select(companyUser => new CompanyUserWithIdpBusinessPartnerData(
                companyUser,
                companyUser.Company!.IdentityProviders.Where(identityProvider =>
                        identityProvider.IdentityProviderCategoryId == IdentityProviderCategoryId.KEYCLOAK_SHARED)
                    .Select(identityProvider => identityProvider.IamIdentityProvider!.IamIdpAlias)
                    .SingleOrDefault()!,
                companyUser.CompanyUserAssignedBusinessPartners.Select(assignedPartner =>
                    assignedPartner.BusinessPartnerNumber)
            ))
            .SingleOrDefaultAsync();

    public Task<CompanyUserWithIdpData?> GetUserWithIdpAsync(string iamUserId) =>
        _dbContext.CompanyUsers
            .Where(companyUser => companyUser.IamUser!.UserEntityId == iamUserId
                                  && companyUser!.Company!.IdentityProviders
                                      .Any(identityProvider =>
                                          identityProvider.IdentityProviderCategoryId ==
                                          IdentityProviderCategoryId.KEYCLOAK_SHARED))
            .Include(companyUser => companyUser.CompanyUserAssignedRoles)
            .Include(companyUser => companyUser.IamUser)
            .Select(companyUser => new CompanyUserWithIdpData(
                companyUser,
                companyUser.Company!.IdentityProviders.Where(identityProvider =>
                        identityProvider.IdentityProviderCategoryId == IdentityProviderCategoryId.KEYCLOAK_SHARED)
                    .Select(identityProvider => identityProvider.IamIdentityProvider!.IamIdpAlias)
                    .SingleOrDefault()!
            ))
            .SingleOrDefaultAsync();

    public Task<Guid> GetCompanyUserIdForUserApplicationUntrackedAsync(Guid applicationId, string iamUserId) =>
        _dbContext.IamUsers
            .AsNoTracking()
            .Where(iamUser =>
                iamUser.UserEntityId == iamUserId
                && iamUser.CompanyUser!.Company!.CompanyApplications.Any(application =>
                    application.Id == applicationId))
            .Select(iamUser =>
                iamUser.CompanyUserId
            )
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<Guid> GetCompanyUserIdForIamUserUntrackedAsync(string userId) =>
        _dbContext.CompanyUsers.AsNoTracking()
            .Where(cu => cu.IamUser!.UserEntityId == userId)
            .Select(cu => cu.Id)
            .SingleAsync();

    /// <inheritdoc />
    public IAsyncEnumerable<Guid> GetAllFavouriteAppsForUserUntrackedAsync(string userId) =>
        _dbContext.IamUsers.AsNoTracking()
            .Where(u => u.UserEntityId == userId) // Id is unique, so single user
            .SelectMany(u => u.CompanyUser!.Apps.Select(a => a.Id))
            .ToAsyncEnumerable();

    /// <inheritdoc />
    public IAsyncEnumerable<BusinessAppData> GetAllBusinessAppDataForUserIdAsync(string userId) =>
        _dbContext.IamUsers.AsNoTracking().Where(u => u.UserEntityId == userId)
            .SelectMany(u => u.CompanyUser!.Company!.BoughtApps)
            .Intersect(
                _dbContext.IamUsers.AsNoTracking().Where(u => u.UserEntityId == userId)
                    .SelectMany(u => u.CompanyUser!.UserRoles.SelectMany(r => r.IamClient!.Apps))
            )
            .Select(app => new BusinessAppData(
                app.Id,
                app.Name ?? Constants.ErrorString,
                app.AppUrl ?? Constants.ErrorString,
                app.ThumbnailUrl ?? Constants.ErrorString,
                app.Provider
            )).AsAsyncEnumerable();

    /// <inheritdoc />
    public IAsyncEnumerable<(Guid CompanyUserId, bool IsIamUser)> GetCompanyUserWithIamUserCheck(string iamUserId, Guid companyUserId) =>
        _dbContext.CompanyUsers.Where(x => x.IamUser!.UserEntityId == iamUserId || x.Id == companyUserId)
            .Select(companyUser => ((Guid CompanyUserId, bool IsIamUser)) new ValueTuple<Guid, bool>(companyUser.Id, companyUser.IamUser!.UserEntityId == iamUserId))
            .ToAsyncEnumerable();

    /// <inheritdoc />
    public IAsyncEnumerable<(Guid CompanyUserId, bool IsCatenaXAdmin, bool IsCompanyAdmin)> GetCatenaAndCompanyAdminIdAsync(Guid companyId) =>
        _dbContext.CompanyUsers
            .Where(x => (x.Company!.Name == Constants.CatenaXCompanyName && x.Lastname == Constants.CxAdminUsername) ||
                        (x.CompanyId == companyId && x.UserRoles.Any(x => x.UserRoleText == Constants.CompanyAdminRole)))
            .Select(x => ((Guid CompanyUserId, bool IsCatenaXAdmin, bool IsCompanyAdmin)) new ValueTuple<Guid, bool, bool>(x.Id, x.Lastname == Constants.CxAdminUsername, x.CompanyId == companyId && x.UserRoles.Any(y => y.UserRoleText == Constants.CompanyAdminRole)))
            .ToAsyncEnumerable();

    /// <inheritdoc />
    public Task<Guid> GetCxAdminIdAsync() =>
        _dbContext.CompanyUsers
            .Where(x => x.Company!.Name == Constants.CatenaXCompanyName && x.Lastname == Constants.CxAdminUsername)
            .Select(x => x.Id)
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<Guid> GetCompanyAdminIdAsync(Guid companyId) =>
        _dbContext.CompanyUsers
            .Where(x => x.CompanyId == companyId && x.UserRoles.Any(x => x.UserRoleText == Constants.CompanyAdminRole))
            .Select(x => x.Id)
            .SingleOrDefaultAsync();
}
