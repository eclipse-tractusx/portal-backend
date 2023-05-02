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
using Org.Eclipse.TractusX.Portal.Backend.Framework.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

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

    public IAsyncEnumerable<CompanyApplicationWithStatus> GetApplicationsWithStatusUntrackedAsync(string iamUserId) =>
        _dbContext.IamUsers
            .AsNoTracking()
            .Where(iamUser => iamUser.UserEntityId == iamUserId)
            .SelectMany(iamUser => iamUser.CompanyUser!.Company!.CompanyApplications)
            .Select(companyApplication => new CompanyApplicationWithStatus
            {
                ApplicationId = companyApplication.Id,
                ApplicationStatus = companyApplication.ApplicationStatusId
            })
            .AsAsyncEnumerable();

    public CompanyUser CreateCompanyUser(string? firstName, string? lastName, string email, Guid companyId,
        CompanyUserStatusId companyUserStatusId, Guid lastEditorId) =>
        _dbContext.CompanyUsers.Add(
            new CompanyUser(
                Guid.NewGuid(),
                companyId,
                companyUserStatusId,
                DateTimeOffset.UtcNow,
                lastEditorId)
            {
                Firstname = firstName,
                Lastname = lastName,
                Email = email,
            }).Entity;

    public void AttachAndModifyCompanyUser(Guid companyUserId, Action<CompanyUser> setOptionalParameters)
    {
        var companyUser = _dbContext.Attach(new CompanyUser(companyUserId, Guid.Empty, default, default, Guid.Empty)).Entity;
        setOptionalParameters.Invoke(companyUser);
    }

    public IamUser CreateIamUser(Guid companyUserId, string iamUserId) =>
        _dbContext.IamUsers.Add(
            new IamUser(
                iamUserId,
                companyUserId)).Entity;

    public IamUser DeleteIamUser(string iamUserId) =>
        _dbContext.Remove(new IamUser(iamUserId, Guid.Empty)).Entity;

    public IQueryable<CompanyUser> GetOwnCompanyUserQuery(
        string adminUserId,
        Guid? companyUserId = null,
        string? userEntityId = null,
        string? firstName = null,
        string? lastName = null,
        string? email = null,
        IEnumerable<CompanyUserStatusId>? statusIds = null)
        {
        return _dbContext.CompanyUsers.AsNoTracking()
            .Where(companyUser => companyUser.IamUser!.UserEntityId == adminUserId)
            .SelectMany(companyUser => companyUser.Company!.CompanyUsers)
            .Where(companyUser =>
                (userEntityId == null || companyUser.IamUser!.UserEntityId == userEntityId) &&
                (!companyUserId.HasValue || companyUser.Id == companyUserId.Value) &&
                (firstName == null || companyUser.Firstname == firstName) &&
                (lastName == null || companyUser.Lastname == lastName) &&
                (email == null || EF.Functions.ILike(companyUser.Email!, $"%{email.EscapeForILike()}%")) &&
                (statusIds == null || statusIds.Contains(companyUser.CompanyUserStatusId)));
        }

    public Task<(string UserEntityId, string? FirstName, string? LastName, string? Email)> GetUserEntityDataAsync(Guid companyUserId, Guid companyId) =>
        _dbContext.CompanyUsers
            .AsNoTracking()
            .Where(companyUser => companyUser.Id == companyUserId && companyUser.CompanyId == companyId)
            .Select(companyUser => new ValueTuple<string,string?,string?,string?>(
                companyUser.IamUser!.UserEntityId,
                companyUser.Firstname,
                companyUser.Lastname,
                companyUser.Email))
            .SingleOrDefaultAsync();

    public IAsyncEnumerable<(string? UserEntityId, Guid CompanyUserId)> GetMatchingCompanyIamUsersByNameEmail(string firstName, string lastName, string email, Guid companyId, IEnumerable<CompanyUserStatusId> companyUserStatusIds) =>
        _dbContext.CompanyUsers
            .AsNoTracking()
            .Where(companyUser =>
                companyUser.CompanyId == companyId &&
                companyUserStatusIds.Contains(companyUser.CompanyUserStatusId) &&
                (companyUser.Email == email ||
                 companyUser.Firstname == firstName ||
                 companyUser.Lastname == lastName ))
            .Select(companyUser => new ValueTuple<string?,Guid>(
                companyUser.IamUser!.UserEntityId,
                companyUser.Firstname == firstName && companyUser.Lastname == lastName && companyUser.Email == email
                    ? companyUser.Id
                    : Guid.Empty))
            .AsAsyncEnumerable();

    public Task<(Guid companyId, Guid companyUserId)> GetOwnCompanyAndCompanyUserId(string iamUserId) =>
        _dbContext.IamUsers
            .AsNoTracking()
            .Where(iamUser => iamUser.UserEntityId == iamUserId)
            .Select(iamUser => new ValueTuple<Guid, Guid>(iamUser.CompanyUser!.CompanyId, iamUser.CompanyUserId))
            .SingleOrDefaultAsync();

    public Task<Guid> GetOwnCompanyId(string iamUserId) =>
        _dbContext.IamUsers
            .AsNoTracking()
            .Where(iamUser => iamUser.UserEntityId == iamUserId)
            .Select(iamUser => iamUser.CompanyUser!.CompanyId)
            .SingleOrDefaultAsync();

    public Task<(CompanyInformationData companyInformation, Guid companyUserId, string? userEmail)> GetOwnCompanyInformationWithCompanyUserIdAndEmailAsync(string iamUserId) =>
        _dbContext.IamUsers
            .AsNoTracking()
            .Where(iamUser => iamUser.UserEntityId == iamUserId)
            .Select(iamUser => new ValueTuple<CompanyInformationData, Guid, string?>(
                new CompanyInformationData(
                    iamUser.CompanyUser!.CompanyId,
                    iamUser.CompanyUser.Company!.Name,
                    iamUser.CompanyUser.Company!.Address!.CountryAlpha2Code,
                    iamUser.CompanyUser.Company!.BusinessPartnerNumber
                ),
                iamUser.CompanyUserId, 
                iamUser.CompanyUser!.Email))
            .SingleOrDefaultAsync();

    public Task<bool> IsOwnCompanyUserWithEmailExisting(string email, string adminUserId) =>
        _dbContext.IamUsers
            .Where(iamUser => iamUser.UserEntityId == adminUserId)
            .SelectMany(iamUser => iamUser.CompanyUser!.Company!.CompanyUsers)
            .AnyAsync(companyUser => companyUser!.Email == email);

    public Task<CompanyUserDetails?> GetOwnCompanyUserDetailsUntrackedAsync(Guid companyUserId, string iamUserId) =>
        _dbContext.CompanyUsers
            .AsNoTracking()
            .Where(companyUser =>
                companyUser.Id == companyUserId &&
                companyUser.CompanyUserStatusId == CompanyUserStatusId.ACTIVE &&
                companyUser.Company!.CompanyUsers.Any(cu =>
                    cu.IamUser!.UserEntityId == iamUserId))
            .Select(companyUser => new CompanyUserDetails(
                companyUser.Id,
                companyUser.DateCreated,
                companyUser.CompanyUserAssignedBusinessPartners.Select(assignedPartner =>
                    assignedPartner.BusinessPartnerNumber),
                companyUser.Company!.Name,
                companyUser.CompanyUserStatusId,
                companyUser.UserRoles.Select(x => x.Offer!)
                    .Distinct()
                    .Select(offer => new CompanyUserAssignedRoleDetails(
                        offer.Id,
                        offer.UserRoles.Where(role => companyUser.UserRoles.Contains(role)).Select(x => x.UserRoleText)
                    )))
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
            .Where(iamUser => iamUser.UserEntityId == iamUserId)
            .Select(iamUser => iamUser.CompanyUser!.Company!.Id)
            .SingleOrDefaultAsync();

    public Task<(Guid CompanyId, string? Bpn, IEnumerable<Guid> TechnicalUserRoleIds)> GetCompanyIdAndBpnRolesForIamUserUntrackedAsync(string iamUserId, string technicalUserClientId) =>
        _dbContext.IamUsers
            .AsNoTracking()
            .Where(iamUser => iamUser.UserEntityId == iamUserId)
            .Select(iamUser => new ValueTuple<Guid,string?,IEnumerable<Guid>>(
                iamUser.CompanyUser!.Company!.Id,
                iamUser.CompanyUser!.Company!.BusinessPartnerNumber,
                iamUser.CompanyUser.Company!.CompanyAssignedRoles.SelectMany(car => car.CompanyRole!.CompanyRoleAssignedRoleCollection!.UserRoleCollection!.UserRoles.Where(ur => ur.Offer!.AppInstances.Any(ai => ai.IamClient!.ClientClientId == technicalUserClientId)).Select(ur => ur.Id)).Distinct()))
            .SingleOrDefaultAsync();

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
                companyUser.CompanyUserStatusId,
                companyUser.Company!.OfferSubscriptions
                    .Where(app => app.OfferSubscriptionStatusId == OfferSubscriptionStatusId.ACTIVE)
                    .Select(app => new CompanyUserAssignedRoleDetails(
                        app.OfferId,
                        app.Offer!.UserRoles
                            .Where(role => role.CompanyUsers.Any(user => user.Id == companyUser.Id))
                            .Select(role => role.UserRoleText)
                    ))
                    )
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
            .AsSplitQuery()
            .Select(companyUser => new CompanyUserWithIdpBusinessPartnerData(
                companyUser,
                companyUser.Company!.IdentityProviders.Where(identityProvider =>
                        identityProvider.IdentityProviderCategoryId == IdentityProviderCategoryId.KEYCLOAK_SHARED)
                    .Select(identityProvider => identityProvider.IamIdentityProvider!.IamIdpAlias)
                    .SingleOrDefault()!,
                companyUser.CompanyUserAssignedBusinessPartners.Select(assignedPartner =>
                    assignedPartner.BusinessPartnerNumber),
                companyUser.Company!.OfferSubscriptions
                    .Where(app => app.OfferSubscriptionStatusId == OfferSubscriptionStatusId.ACTIVE)
                    .Select(app => new CompanyUserAssignedRoleDetails(
                        app.OfferId,
                        app.Offer!.UserRoles
                            .Where(role => role.CompanyUsers.Any(user => user.Id == companyUser.Id))
                            .Select(role => role.UserRoleText)
                    ))))
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
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public IAsyncEnumerable<(Guid CompanyUserId, bool IsIamUser, string CompanyName, Guid CompanyId)> GetCompanyUserWithIamUserCheckAndCompanyName(string iamUserId, Guid? salesManagerId) => 
        _dbContext.CompanyUsers.Where(x => x.IamUser!.UserEntityId == iamUserId || (salesManagerId.HasValue && x.Id == salesManagerId.Value))
            .Select(companyUser => new ValueTuple<Guid, bool, string, Guid>(companyUser.Id, companyUser.IamUser!.UserEntityId == iamUserId, companyUser.Company!.Name, companyUser.CompanyId))
            .ToAsyncEnumerable();

    /// <inheritdoc />
    public IAsyncEnumerable<Guid> GetAllFavouriteAppsForUserUntrackedAsync(string userId) =>
        _dbContext.IamUsers.AsNoTracking()
            .Where(u => u.UserEntityId == userId) // Id is unique, so single user
            .SelectMany(u => u.CompanyUser!.Offers.Select(a => a.Id))
            .ToAsyncEnumerable();

    /// <inheritdoc />
    public IAsyncEnumerable<(Guid CompanyUserId, bool IsIamUser)> GetCompanyUserWithIamUserCheck(string iamUserId, Guid companyUserId) =>
        _dbContext.CompanyUsers.Where(x => x.IamUser!.UserEntityId == iamUserId || x.Id == companyUserId)
            .Select(companyUser => new ValueTuple<Guid, bool>(companyUser.Id, companyUser.IamUser!.UserEntityId == iamUserId))
            .ToAsyncEnumerable();

    /// <inheritdoc />
    public IAsyncEnumerable<Guid> GetCompanyUserWithRoleIdForCompany(IEnumerable<Guid> userRoleIds, Guid companyId) =>
        _dbContext.CompanyUsers
            .Where(x => 
                x.CompanyId == companyId &&
                x.CompanyUserStatusId == CompanyUserStatusId.ACTIVE && 
                x.UserRoles.Any(ur => userRoleIds.Contains(ur.Id)))
            .Select(x => x.Id)
            .ToAsyncEnumerable();

    /// <inheritdoc />
    public IAsyncEnumerable<Guid> GetCompanyUserWithRoleId(IEnumerable<Guid> userRoleIds) =>
        _dbContext.CompanyUsers
            .Where(x => 
                x.CompanyUserStatusId == CompanyUserStatusId.ACTIVE && 
                x.UserRoles.Any(ur => userRoleIds.Contains(ur.Id)))
            .Select(x => x.Id)
            .ToAsyncEnumerable();

    /// <inheritdoc />
    public IAsyncEnumerable<(string Email, string? FirstName, string? LastName)> GetCompanyUserEmailForCompanyAndRoleId(IEnumerable<Guid> userRoleIds, Guid companyId) =>
        _dbContext.CompanyUsers
            .Where(x =>
                x.CompanyId == companyId &&
                x.CompanyUserStatusId == CompanyUserStatusId.ACTIVE &&
                x.UserRoles.Any(ur => userRoleIds.Contains(ur.Id)) &&
                x.Email != null)
            .Select(x => new ValueTuple<string, string?, string?>(x.Email!, x.Firstname, x.Lastname))
            .ToAsyncEnumerable();

    /// <inheritdoc />
    public Task<OfferIamUserData?> GetAppAssignedIamClientUserDataUntrackedAsync(Guid offerId, Guid companyUserId, string iamUserId) => 
        _dbContext.CompanyUsers.AsNoTracking()
            .Where(companyUser => companyUser.Id == companyUserId)
            .Select(companyUser => new {
                User = companyUser,
                Subscriptions = companyUser.Company!.OfferSubscriptions.Where(subscription => subscription.OfferId == offerId)
            })
            .Select(x => new OfferIamUserData( 
                x.Subscriptions.Any(),
                x.Subscriptions.Select(subscription => subscription.AppSubscriptionDetail!.AppInstance!.IamClient!.ClientClientId).Distinct(), 
                x.User.IamUser!.UserEntityId,
                x.User.Company!.CompanyUsers.Any(cu => cu.IamUser!.UserEntityId == iamUserId),
                x.Subscriptions.Select(s => s.Offer!.Name).FirstOrDefault(),
                x.User.Firstname,
                x.User.Lastname))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<CoreOfferIamUserData?> GetCoreOfferAssignedIamClientUserDataUntrackedAsync(Guid offerId, Guid companyUserId, string iamUserId) =>
        _dbContext.CompanyUsers.AsNoTracking()
            .Where(companyUser => companyUser.Id == companyUserId)
            .Select(companyUser => new {
                User = companyUser,
                Offer = companyUser.Company!.CompanyAssignedRoles
                    .SelectMany(assigned => assigned.CompanyRole!.CompanyRoleAssignedRoleCollection!.UserRoleCollection!.UserRoles)
                    .Select(role => role.Offer)
                    .FirstOrDefault(offer => offer!.Id == offerId)
            })
            .Select(x => new CoreOfferIamUserData(
                x.Offer != null,
                x.Offer!.AppInstances.Select(instance => instance.IamClient!.ClientClientId),
                x.User.IamUser!.UserEntityId,
                x.User.Company!.CompanyUsers.Any(cu => cu.IamUser!.UserEntityId == iamUserId),
                x.User.Firstname,
                x.User.Lastname))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public IAsyncEnumerable<Guid> GetServiceProviderCompanyUserWithRoleIdAsync(Guid offerId, List<Guid> userRoleIds) =>
        _dbContext.Offers
            .Where(x => x.Id == offerId)
            .SelectMany(x => x.ProviderCompany!.CompanyUsers)
            .Where(x => 
                x.CompanyUserStatusId == CompanyUserStatusId.ACTIVE && 
                x.UserRoles.Any(ur => userRoleIds.Contains(ur.Id)))
            .Select(x => x.Id)
            .ToAsyncEnumerable();
    
    public Func<int,int,Task<Pagination.Source<CompanyAppUserDetails>?>> GetOwnCompanyAppUsersPaginationSourceAsync(
        Guid appId,
        string iamUserId,
        IEnumerable<OfferSubscriptionStatusId> subscriptionStatusIds,
        IEnumerable<CompanyUserStatusId> companyUserStatusIds,
        CompanyUserFilter filter)
    {
        var (firstName, lastName, email, roleName, hasRole) = filter;

        return (skip, take) => Pagination.CreateSourceQueryAsync(
            skip,
            take,
            _dbContext.CompanyUsers.AsNoTracking()
                .Where(companyUser => companyUser.IamUser!.UserEntityId == iamUserId && 
                                    companyUser.Company!.OfferSubscriptions.Any(subscription => subscription.OfferId == appId && subscriptionStatusIds.Contains(subscription.OfferSubscriptionStatusId)))
                .SelectMany(companyUser => companyUser.Company!.CompanyUsers)
                .Where(companyUser => 
                    (firstName == null || EF.Functions.ILike(companyUser.Firstname!, $"%{firstName.EscapeForILike()}%")) &&
                    (lastName == null || EF.Functions.ILike(companyUser.Lastname!, $"%{lastName.EscapeForILike()}%")) &&
                    (email == null || EF.Functions.ILike(companyUser.Email!, $"%{email.EscapeForILike()}%")) &&
                    (roleName == null || companyUser.UserRoles.Any(userRole => userRole.OfferId == appId && EF.Functions.ILike(userRole.UserRoleText, $"%{roleName.EscapeForILike()}%"))) &&
                    (!hasRole.HasValue || !hasRole.Value || companyUser.UserRoles.Any(userRole => userRole.Offer!.Id == appId)) &&
                    (!hasRole.HasValue || hasRole.Value || companyUser.UserRoles.All(userRole => userRole.Offer!.Id != appId)) &&
                    companyUserStatusIds.Contains(companyUser.CompanyUserStatusId))
                .GroupBy(companyUser => companyUser.CompanyId),
            null,
            companyUser => new CompanyAppUserDetails(
                    companyUser.Id,
                    companyUser.CompanyUserStatusId,
                    companyUser.UserRoles!.Where(userRole => userRole.Offer!.Id == appId).Select(userRole => userRole.UserRoleText))
                {
                    FirstName = companyUser.Firstname,
                    LastName = companyUser.Lastname,
                    Email = companyUser.Email
                }
        ).SingleOrDefaultAsync();
    }

    /// <inheritdoc />
    public Task<(string? SharedIdpAlias, CompanyUserAccountData AccountData)> GetSharedIdentityProviderUserAccountDataUntrackedAsync(string iamUserId) =>
        _dbContext.CompanyUsers.AsNoTracking().AsSplitQuery()
            .Where(companyUser => companyUser.IamUser!.UserEntityId == iamUserId)
            .Select(companyUser => new ValueTuple<string?,CompanyUserAccountData>(
                companyUser.Company!.IdentityProviders.SingleOrDefault(identityProvider => identityProvider.IdentityProviderCategoryId == IdentityProviderCategoryId.KEYCLOAK_SHARED)!.IamIdentityProvider!.IamIdpAlias,
                new CompanyUserAccountData(
                    companyUser.Id,
                    companyUser.IamUser!.UserEntityId,
                    companyUser.CompanyUserAssignedBusinessPartners.Select(assignedPartner =>
                        assignedPartner.BusinessPartnerNumber),
                    companyUser.CompanyUserAssignedRoles.Select(assignedRole =>
                        assignedRole.UserRoleId),
                    companyUser.Offers.Select(offer => offer.Id),
                    companyUser.Invitations.Select(invitation=>invitation.Id))
            ))
            .SingleOrDefaultAsync();

     public IAsyncEnumerable<CompanyUserAccountData> GetCompanyUserAccountDataUntrackedAsync(IEnumerable<Guid> companyUserIds, Guid companyUserId) =>
        _dbContext.CompanyUsers.AsNoTracking().AsSplitQuery()
            .Where(companyUser => companyUserIds.Contains(companyUser.Id) &&
                companyUser.Company!.CompanyUsers.Any(user => user.Id == companyUserId))
            .Select(companyUser => new CompanyUserAccountData(
                companyUser.Id,
                companyUser.IamUser!.UserEntityId,
                companyUser.CompanyUserAssignedBusinessPartners.Select(assignedPartner =>
                    assignedPartner.BusinessPartnerNumber),
                companyUser.CompanyUserAssignedRoles.Select(assignedRole =>
                    assignedRole.UserRoleId),
                companyUser.Offers.Select(offer => offer.Id),
                companyUser.Invitations.Select(invitation=>invitation.Id)
            ))
            .AsAsyncEnumerable();
    
    /// <inheritdoc />
    public Task<(IEnumerable<Guid> RoleIds, bool IsSameCompany, Guid UserCompanyId)> GetRolesAndCompanyMembershipUntrackedAsync(string iamUserId, IEnumerable<Guid> roleIds, Guid companyUserId) =>
        _dbContext.CompanyUsers.AsNoTracking()
            .Where(companyUser => companyUser.Id == companyUserId)
            .Select(companyUser=>new ValueTuple<IEnumerable<Guid>,bool, Guid>(
                companyUser.CompanyUserAssignedRoles.Where(assignedRole => roleIds.Contains(assignedRole.UserRoleId)).Select(assignedRole => assignedRole.UserRoleId),
                companyUser.Company!.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == iamUserId),
                companyUser.CompanyId))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public IAsyncEnumerable<(bool IsApplicationCompany, bool IsApplicationPending, string? BusinessPartnerNumber, Guid CompanyId)> GetBpnForIamUserUntrackedAsync(Guid applicationId, string businessPartnerNumber) =>
        _dbContext.Companies
            .AsNoTracking()
            .Where(company => company.CompanyApplications.Any(application => application.Id == applicationId) ||
                company.BusinessPartnerNumber == businessPartnerNumber)
            .Select(company => new ValueTuple<bool, bool, string?, Guid>(
                company.CompanyApplications.Any(application => application.Id == applicationId),
                company.CompanyStatusId == CompanyStatusId.PENDING,
                company.BusinessPartnerNumber,
                company.Id))
            .AsAsyncEnumerable();

    /// <inheritdoc />
    public Task<string?> GetCompanyBpnForIamUserAsync(string iamUserId) =>
        _dbContext.IamUsers
            .Where(x => x.UserEntityId == iamUserId)
            .Select(x => x.CompanyUser!.Company!.BusinessPartnerNumber)
            .SingleOrDefaultAsync();
}
