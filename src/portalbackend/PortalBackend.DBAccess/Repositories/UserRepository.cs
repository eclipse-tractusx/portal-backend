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

    public IAsyncEnumerable<CompanyApplicationWithStatus> GetApplicationsWithStatusUntrackedAsync(Guid userCompanyId) =>
        _dbContext.CompanyApplications
            .AsNoTracking()
            .Where(app => app.CompanyId == userCompanyId)
            .Select(companyApplication => new CompanyApplicationWithStatus
            {
                ApplicationId = companyApplication.Id,
                ApplicationStatus = companyApplication.ApplicationStatusId
            })
            .AsAsyncEnumerable();

    public CompanyUser CreateCompanyUser(string? firstName, string? lastName, string email, Guid companyId,
        UserStatusId userStatusId, Guid lastEditorId) =>
        _dbContext.CompanyUsers.Add(
            new CompanyUser(
                Guid.NewGuid(),
                    companyId,
                    userStatusId,
                    DateTimeOffset.UtcNow,
                    lastEditorId)
            {
                Firstname = firstName,
                Lastname = lastName,
                Email = email,
            }).Entity;

    public void AttachAndModifyCompanyUser(Guid companyUserId, Action<CompanyUser>? initialize, Action<CompanyUser> setOptionalParameters)
    {
        var companyUser = new CompanyUser(companyUserId, Guid.Empty, default, default, Guid.Empty);
        initialize?.Invoke(companyUser);
        var updatedEntity = _dbContext.Attach(companyUser).Entity;
        setOptionalParameters.Invoke(updatedEntity);
    }

    public IQueryable<CompanyUser> GetOwnCompanyUserQuery(
        string adminUserId,
        Guid? companyUserId = null,
        string? userEntityId = null,
        string? firstName = null,
        string? lastName = null,
        string? email = null,
        IEnumerable<UserStatusId>? statusIds = null)
    {
        return _dbContext.CompanyUsers.AsNoTracking()
            .Where(companyUser => companyUser.UserEntityId == adminUserId)
            .SelectMany(companyUser => companyUser.Company!.Identities.Where(i => i.IdentityTypeId == IdentityTypeId.COMPANY_USER).Select(i => i.CompanyUser!))
            .Where(companyUser =>
                (userEntityId == null || companyUser.UserEntityId == userEntityId) &&
                (!companyUserId.HasValue || companyUser.Id == companyUserId.Value) &&
                (firstName == null || companyUser.Firstname == firstName) &&
                (lastName == null || companyUser.Lastname == lastName) &&
                (email == null || EF.Functions.ILike(companyUser.Email!, $"%{email.EscapeForILike()}%")) &&
                (statusIds == null || statusIds.Contains(companyUser.UserStatusId)));
    }

    // TODO (PS): Check
    public Task<(string UserEntityId, string? FirstName, string? LastName, string? Email)> GetUserEntityDataAsync(Guid companyUserId, Guid companyId) =>
        _dbContext.CompanyUsers
            .AsNoTracking()
            .Where(companyUser => companyUser.Id == companyUserId &&
                                  companyUser.CompanyId == companyId &&
                                  companyUser.UserEntityId != null)
            .Select(companyUser => new ValueTuple<string, string?, string?, string?>(
                companyUser.UserEntityId!,
                companyUser.Firstname,
                companyUser.Lastname,
                companyUser.Email))
            .SingleOrDefaultAsync();

    public IAsyncEnumerable<(string? UserEntityId, Guid CompanyUserId)> GetMatchingCompanyIamUsersByNameEmail(string firstName, string lastName, string email, Guid companyId, IEnumerable<UserStatusId> companyUserStatusIds) =>
        _dbContext.CompanyUsers
            .AsNoTracking()
            .Where(companyUser =>
                companyUser.CompanyId == companyId &&
                companyUserStatusIds.Contains(companyUser.UserStatusId) &&
                (companyUser.Email == email ||
                 companyUser.Firstname == firstName ||
                 companyUser.Lastname == lastName))
            .Select(companyUser => new ValueTuple<string?, Guid>(
                companyUser.UserEntityId,
                companyUser.Firstname == firstName && companyUser.Lastname == lastName && companyUser.Email == email
                    ? companyUser.Id
                    : Guid.Empty))
            .AsAsyncEnumerable();

    public Task<(Guid companyId, Guid companyUserId)> GetOwnCompanyAndCompanyUserId(string iamUserId) =>
        _dbContext.CompanyUsers
            .AsNoTracking()
            .Where(user => user.UserEntityId == iamUserId)
            .Select(user => new ValueTuple<Guid, Guid>(user.CompanyId, user.Id))
            .SingleOrDefaultAsync();

    public Task<Guid> GetOwnCompanyId(string iamUserId) =>
        _dbContext.CompanyUsers
            .AsNoTracking()
            .Where(user => user.UserEntityId == iamUserId)
            .Select(user => user.CompanyId)
            .SingleOrDefaultAsync();

    public Task<(CompanyInformationData companyInformation, Guid companyUserId, string? userEmail)> GetOwnCompanyInformationWithCompanyUserIdAndEmailAsync(string iamUserId) =>
        _dbContext.CompanyUsers
            .AsNoTracking()
            .Where(user => user.UserEntityId == iamUserId)
            .Select(user => new ValueTuple<CompanyInformationData, Guid, string?>(
                new CompanyInformationData(
                    user.CompanyId,
                    user.Company!.Name,
                    user.Company!.Address!.CountryAlpha2Code,
                    user.Company!.BusinessPartnerNumber
                ),
                user.Id,
                user.Email))
            .SingleOrDefaultAsync();

    public Task<bool> IsOwnCompanyUserWithEmailExisting(string email, string adminUserId) =>
        _dbContext.CompanyUsers
            .Where(user => user.UserEntityId == adminUserId)
            .SelectMany(user => user.Company!.Identities.Where(i => i.IdentityTypeId == IdentityTypeId.COMPANY_USER).Select(x => x.CompanyUser!))
            .AnyAsync(companyUser => companyUser!.Email == email);

    public Task<CompanyUserDetails?> GetOwnCompanyUserDetailsUntrackedAsync(Guid companyUserId, Guid userCompanyId) =>
        _dbContext.CompanyUsers
            .AsNoTracking()
            .Where(companyUser =>
                companyUser.Id == companyUserId &&
                companyUser.UserStatusId == UserStatusId.ACTIVE &&
                companyUser.CompanyId == userCompanyId)
            .Select(companyUser => new CompanyUserDetails(
                companyUser.Id,
                companyUser.DateCreated,
                companyUser.CompanyUserAssignedBusinessPartners.Select(assignedPartner =>
                    assignedPartner.BusinessPartnerNumber),
                companyUser.Company!.Name,
                companyUser.UserStatusId,
                companyUser.IdentityAssignedRoles.Select(x => x.UserRole!.Offer!)
                    .Distinct()
                    .Select(offer => new CompanyUserAssignedRoleDetails(
                        offer.Id,
                        offer.UserRoles.Where(role => companyUser.IdentityAssignedRoles.Select(x => x.UserRole).Contains(role)).Select(x => x.UserRoleText)
                    )))
            {
                FirstName = companyUser.Firstname,
                LastName = companyUser.Lastname,
                Email = companyUser.Email
            })
            .SingleOrDefaultAsync();

    public Task<CompanyUserBusinessPartners?> GetOwnCompanyUserWithAssignedBusinessPartnerNumbersUntrackedAsync(Guid companyUserId, string adminUserId) =>
        _dbContext.CompanyUsers
            .AsNoTracking()
            .Where(user => user.UserEntityId == adminUserId)
            .SelectMany(user => user.Company!.Identities.Where(i => i.IdentityTypeId == IdentityTypeId.COMPANY_USER).Select(i => i.CompanyUser!))
            .Where(companyUser => companyUser.Id == companyUserId)
            .Select(companyUser => new CompanyUserBusinessPartners(
                companyUser.UserEntityId!,
                companyUser.CompanyUserAssignedBusinessPartners.Select(assignedPartner =>
                    assignedPartner.BusinessPartnerNumber)
            ))
            .SingleOrDefaultAsync();

    public Task<Guid> GetCompanyIdForIamUserUntrackedAsync(string iamUserId) =>
        _dbContext.CompanyUsers
            .AsNoTracking()
            .Where(user => user.UserEntityId == iamUserId)
            .Select(user => user.Company!.Id)
            .SingleOrDefaultAsync();

    public Task<(Guid CompanyId, string? Bpn, IEnumerable<Guid> TechnicalUserRoleIds)> GetCompanyIdAndBpnRolesForIamUserUntrackedAsync(string iamUserId, string technicalUserClientId) =>
        _dbContext.CompanyUsers
            .AsNoTracking()
            .Where(usre => usre.UserEntityId == iamUserId)
            .Select(user => new ValueTuple<Guid, string?, IEnumerable<Guid>>(
                user.Company!.Id,
                user.Company!.BusinessPartnerNumber,
                user.Company!.CompanyAssignedRoles.SelectMany(car => car.CompanyRole!.CompanyRoleAssignedRoleCollection!.UserRoleCollection!.UserRoles.Where(ur => ur.Offer!.AppInstances.Any(ai => ai.IamClient!.ClientClientId == technicalUserClientId)).Select(ur => ur.Id)).Distinct()))
            .SingleOrDefaultAsync();

    public Task<CompanyOwnUserDetails?> GetUserDetailsUntrackedAsync(string iamUserId, IEnumerable<Guid> userRoleIds) =>
        _dbContext.CompanyUsers
            .AsNoTracking()
            .AsSplitQuery()
            .Where(companyUser => companyUser.UserEntityId == iamUserId)
            .Select(companyUser => new CompanyOwnUserDetails(
                companyUser.Id,
                companyUser.DateCreated,
                companyUser.CompanyUserAssignedBusinessPartners.Select(assignedPartner =>
                    assignedPartner.BusinessPartnerNumber),
                companyUser.Company!.Name,
                companyUser.UserStatusId,
                companyUser.Company!.OfferSubscriptions
                    .Where(app => app.OfferSubscriptionStatusId == OfferSubscriptionStatusId.ACTIVE)
                    .Select(app => new CompanyUserAssignedRoleDetails(
                        app.OfferId,
                        app.Offer!.UserRoles
                            .Where(role => role.IdentityAssignedRoles.Select(u => u.Identity!).Any(user => user.Id == companyUser.Id && user.IdentityTypeId == IdentityTypeId.COMPANY_USER))
                            .Select(role => role.UserRoleText)
                    )),
                companyUser.Company.Identities.Where(i => i.IdentityTypeId == IdentityTypeId.COMPANY_USER && i.IdentityAssignedRoles.Any(role => userRoleIds.Contains(role.UserRoleId))).Select(i => i.CompanyUser!)
                    .Select(admin => new CompanyUserAdminDetails(
                        admin.Id,
                        admin.Email)))
            {
                FirstName = companyUser.Firstname,
                LastName = companyUser.Lastname,
                Email = companyUser.Email
            })
            .SingleOrDefaultAsync();

    public Task<CompanyUserWithIdpBusinessPartnerData?> GetUserWithCompanyIdpAsync(string iamUserId) =>
        _dbContext.CompanyUsers
            .Where(companyUser => companyUser.UserEntityId == iamUserId
                                  && companyUser!.Company!.IdentityProviders
                                      .Any(identityProvider =>
                                          identityProvider.IdentityProviderCategoryId ==
                                          IdentityProviderCategoryId.KEYCLOAK_SHARED))
            // .Include(companyUser => companyUser.Company)
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
                            .Where(role => role.IdentityAssignedRoles.Select(u => u.Identity!).Any(user => user.Id == companyUser.Id && user.IdentityTypeId == IdentityTypeId.COMPANY_USER))
                            .Select(role => role.UserRoleText)
                    ))))
            .SingleOrDefaultAsync();

    public Task<Guid> GetCompanyUserIdForUserApplicationUntrackedAsync(Guid applicationId, string iamUserId) =>
        _dbContext.CompanyUsers
            .AsNoTracking()
            .Where(user =>
                user.UserEntityId == iamUserId
                && user.Company!.CompanyApplications.Any(application =>
                    application.Id == applicationId))
            .Select(iamUser =>
                iamUser.Id
            )
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<Guid> GetCompanyUserIdForIamUserUntrackedAsync(string userId) =>
        _dbContext.CompanyUsers.AsNoTracking()
            .Where(cu => cu.UserEntityId == userId)
            .Select(cu => cu.Id)
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public IAsyncEnumerable<(Guid CompanyUserId, bool IsIamUser, string CompanyName, Guid CompanyId)> GetCompanyUserWithIamUserCheckAndCompanyName(string iamUserId, Guid? salesManagerId) =>
        _dbContext.CompanyUsers.Where(x => x.UserEntityId == iamUserId || (salesManagerId.HasValue && x.Id == salesManagerId.Value))
            .Select(companyUser => new ValueTuple<Guid, bool, string, Guid>(companyUser.Id, companyUser.UserEntityId == iamUserId, companyUser.Company!.Name, companyUser.CompanyId))
            .ToAsyncEnumerable();

    /// <inheritdoc />
    public IAsyncEnumerable<Guid> GetAllFavouriteAppsForUserUntrackedAsync(string userId) =>
        _dbContext.CompanyUsers.AsNoTracking()
            .Where(u => u.UserEntityId == userId) // Id is unique, so single user
            .SelectMany(u => u.Offers.Select(a => a.Id))
            .ToAsyncEnumerable();

    /// <inheritdoc />
    public IAsyncEnumerable<(Guid CompanyUserId, bool IsIamUser)> GetCompanyUserWithIamUserCheck(string iamUserId, Guid companyUserId) =>
        _dbContext.CompanyUsers.Where(x => x.UserEntityId == iamUserId || x.Id == companyUserId)
            .Select(companyUser => new ValueTuple<Guid, bool>(companyUser.Id, companyUser.UserEntityId == iamUserId))
            .ToAsyncEnumerable();

    /// <inheritdoc />
    public IAsyncEnumerable<Guid> GetCompanyUserWithRoleIdForCompany(IEnumerable<Guid> userRoleIds, Guid companyId) =>
        _dbContext.CompanyUsers
            .Where(x =>
                x.CompanyId == companyId &&
                x.UserStatusId == UserStatusId.ACTIVE &&
                x.IdentityAssignedRoles.Select(u => u.UserRoleId).Any(userRoleIds.Contains))
            .Select(x => x.Id)
            .ToAsyncEnumerable();

    /// <inheritdoc />
    public IAsyncEnumerable<Guid> GetCompanyUserWithRoleId(IEnumerable<Guid> userRoleIds) =>
        _dbContext.CompanyUsers
            .Where(x =>
                x.UserStatusId == UserStatusId.ACTIVE &&
                x.IdentityAssignedRoles.Select(u => u.UserRoleId).Any(userRoleIds.Contains))
            .Select(x => x.Id)
            .ToAsyncEnumerable();

    /// <inheritdoc />
    public IAsyncEnumerable<(string Email, string? FirstName, string? LastName)> GetCompanyUserEmailForCompanyAndRoleId(IEnumerable<Guid> userRoleIds, Guid companyId) =>
        _dbContext.CompanyUsers
            .Where(x =>
                x.CompanyId == companyId &&
                x.UserStatusId == UserStatusId.ACTIVE &&
                x.IdentityAssignedRoles.Select(u => u.UserRoleId).Any(userRoleIds.Contains) &&
                x.Email != null)
            .Select(x => new ValueTuple<string, string?, string?>(x.Email!, x.Firstname, x.Lastname))
            .ToAsyncEnumerable();

    /// <inheritdoc />
    public Task<OfferIamUserData?> GetAppAssignedIamClientUserDataUntrackedAsync(Guid offerId, Guid companyUserId, Guid userCompanyId) =>
        _dbContext.CompanyUsers.AsNoTracking()
            .Where(companyUser => companyUser.Id == companyUserId)
            .Select(companyUser => new
            {
                User = companyUser,
                Subscriptions = companyUser.Company!.OfferSubscriptions.Where(subscription => subscription.OfferId == offerId)
            })
            .Select(x => new OfferIamUserData(
                x.Subscriptions.Any(),
                x.Subscriptions.Select(subscription => subscription.AppSubscriptionDetail!.AppInstance!.IamClient!.ClientClientId).Distinct(),
                x.User.UserEntityId,
                x.User.CompanyId == userCompanyId,
                x.Subscriptions.Select(s => s.Offer!.Name).FirstOrDefault(),
                x.User.Firstname,
                x.User.Lastname))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<CoreOfferIamUserData?> GetCoreOfferAssignedIamClientUserDataUntrackedAsync(Guid offerId, Guid companyUserId, Guid userCompanyId) =>
        _dbContext.CompanyUsers.AsNoTracking()
            .Where(companyUser => companyUser.Id == companyUserId)
            .Select(companyUser => new
            {
                User = companyUser,
                Offer = companyUser.Company!.CompanyAssignedRoles
                    .SelectMany(assigned => assigned.CompanyRole!.CompanyRoleAssignedRoleCollection!.UserRoleCollection!.UserRoles)
                    .Select(role => role.Offer)
                    .FirstOrDefault(offer => offer!.Id == offerId)
            })
            .Select(x => new CoreOfferIamUserData(
                x.Offer != null,
                x.Offer!.AppInstances.Select(instance => instance.IamClient!.ClientClientId),
                x.User.UserEntityId,
                x.User.CompanyId == userCompanyId,
                x.User.Firstname,
                x.User.Lastname))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public IAsyncEnumerable<Guid> GetServiceProviderCompanyUserWithRoleIdAsync(Guid offerId, List<Guid> userRoleIds) =>
        _dbContext.Offers
            .Where(x => x.Id == offerId)
            .SelectMany(x => x.ProviderCompany!.Identities)
            .Where(x =>
                x.UserStatusId == UserStatusId.ACTIVE &&
                x.IdentityAssignedRoles.Select(u => u.UserRoleId).Any(userRoleIds.Contains))
            .Select(x => x.Id)
            .ToAsyncEnumerable();

    public Func<int, int, Task<Pagination.Source<CompanyAppUserDetails>?>> GetOwnCompanyAppUsersPaginationSourceAsync(
        Guid appId,
        string iamUserId,
        IEnumerable<OfferSubscriptionStatusId> subscriptionStatusIds,
        IEnumerable<UserStatusId> companyUserStatusIds,
        CompanyUserFilter filter)
    {
        var (firstName, lastName, email, roleName, hasRole) = filter;

        return (skip, take) => Pagination.CreateSourceQueryAsync(
            skip,
            take,
            _dbContext.CompanyUsers.AsNoTracking()
                .Where(companyUser => companyUser.UserEntityId == iamUserId &&
                                    companyUser.Company!.OfferSubscriptions.Any(subscription => subscription.OfferId == appId && subscriptionStatusIds.Contains(subscription.OfferSubscriptionStatusId)))
                .SelectMany(companyUser => companyUser.Company!.Identities.Where(x => x.IdentityTypeId == IdentityTypeId.COMPANY_USER).Select(i => i.CompanyUser!))
                .Where(companyUser =>
                    (firstName == null || EF.Functions.ILike(companyUser.Firstname!, $"%{firstName.EscapeForILike()}%")) &&
                    (lastName == null || EF.Functions.ILike(companyUser.Lastname!, $"%{lastName.EscapeForILike()}%")) &&
                    (email == null || EF.Functions.ILike(companyUser.Email!, $"%{email.EscapeForILike()}%")) &&
                    (roleName == null || companyUser.IdentityAssignedRoles.Any(userRole => userRole.UserRole!.OfferId == appId && EF.Functions.ILike(userRole.UserRole!.UserRoleText, $"%{roleName.EscapeForILike()}%"))) &&
                    (!hasRole.HasValue || !hasRole.Value || companyUser.IdentityAssignedRoles.Any(userRole => userRole.UserRole!.OfferId == appId)) &&
                    (!hasRole.HasValue || hasRole.Value || companyUser.IdentityAssignedRoles.All(userRole => userRole.UserRole!.OfferId != appId)) &&
                    companyUserStatusIds.Contains(companyUser.UserStatusId))
                .GroupBy(companyUser => companyUser.CompanyId),
            null,
            companyUser => new CompanyAppUserDetails(
                    companyUser.Id,
                    companyUser.UserStatusId,
                    companyUser.IdentityAssignedRoles!.Where(userRole => userRole.UserRole!.Offer!.Id == appId).Select(userRole => userRole.UserRole!.UserRoleText))
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
            .Where(companyUser => companyUser.UserEntityId == iamUserId)
            .Select(companyUser => new ValueTuple<string?, CompanyUserAccountData>(
                companyUser.Company!.IdentityProviders.SingleOrDefault(identityProvider => identityProvider.IdentityProviderCategoryId == IdentityProviderCategoryId.KEYCLOAK_SHARED)!.IamIdentityProvider!.IamIdpAlias,
                new CompanyUserAccountData(
                    companyUser.Id,
                    companyUser.UserEntityId,
                    companyUser.CompanyUserAssignedBusinessPartners.Select(assignedPartner =>
                        assignedPartner.BusinessPartnerNumber),
                    companyUser.IdentityAssignedRoles.Select(assignedRole =>
                        assignedRole.UserRoleId),
                    companyUser.Offers.Select(offer => offer.Id),
                    companyUser.Invitations.Select(invitation => invitation.Id))
            ))
            .SingleOrDefaultAsync();

    public IAsyncEnumerable<CompanyUserAccountData> GetCompanyUserAccountDataUntrackedAsync(IEnumerable<Guid> companyUserIds, Guid companyUserId) =>
       _dbContext.CompanyUsers.AsNoTracking().AsSplitQuery()
           .Where(companyUser => companyUserIds.Contains(companyUser.Id) &&
               companyUser.Company!.Identities.Any(user => user.Id == companyUserId))
           .Select(companyUser => new CompanyUserAccountData(
               companyUser.Id,
               companyUser.UserEntityId,
               companyUser.CompanyUserAssignedBusinessPartners.Select(assignedPartner =>
                   assignedPartner.BusinessPartnerNumber),
               companyUser.IdentityAssignedRoles.Select(assignedRole =>
                   assignedRole.UserRoleId),
               companyUser.Offers.Select(offer => offer.Id),
               companyUser.Invitations.Select(invitation => invitation.Id)
           ))
           .AsAsyncEnumerable();

    /// <inheritdoc />
    public Task<(IEnumerable<Guid> RoleIds, bool IsSameCompany, Guid UserCompanyId)> GetRolesAndCompanyMembershipUntrackedAsync(Guid userCompanyId, IEnumerable<Guid> roleIds, Guid companyUserId) =>
        _dbContext.CompanyUsers.AsNoTracking()
            .Where(companyUser => companyUser.Id == companyUserId)
            .Select(companyUser => new ValueTuple<IEnumerable<Guid>, bool, Guid>(
                companyUser.IdentityAssignedRoles.Where(assignedRole => roleIds.Contains(assignedRole.UserRoleId)).Select(assignedRole => assignedRole.UserRoleId),
                companyUser.CompanyId == userCompanyId,
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
        _dbContext.CompanyUsers
            .Where(x => x.UserEntityId == iamUserId)
            .Select(x => x.Company!.BusinessPartnerNumber)
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<IdentityData?> GetUserDataByUserEntityId(string userEntityId) =>
        _dbContext.Identities
            .Where(x => x.UserEntityId == userEntityId)
            .Select(x => new IdentityData(x.UserEntityId!, x.Id, x.IdentityTypeId, x.CompanyId))
            .SingleOrDefaultAsync();
}
