/********************************************************************************
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Identity;
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

    public IAsyncEnumerable<CompanyApplicationWithStatus> GetApplicationsWithStatusUntrackedAsync(Guid companyId) =>
        _dbContext.CompanyApplications
            .AsNoTracking()
            .Where(app => app.CompanyId == companyId)
            .Select(companyApplication => new CompanyApplicationWithStatus(
                    companyApplication.Id,
                    companyApplication.ApplicationStatusId,
                    companyApplication.CompanyApplicationTypeId,
                    companyApplication.ApplicationChecklistEntries.Select(ace =>
                        new ApplicationChecklistData(ace.ApplicationChecklistEntryTypeId, ace.ApplicationChecklistEntryStatusId))))
            .AsAsyncEnumerable();

    public CompanyUser CreateCompanyUser(Guid identityId, string? firstName, string? lastName, string email) =>
        _dbContext.CompanyUsers.Add(new CompanyUser(identityId)
        {
            Firstname = firstName,
            Lastname = lastName,
            Email = email,
        }).Entity;

    /// <inheritdoc />
    public Identity CreateIdentity(Guid companyId, UserStatusId userStatusId, IdentityTypeId identityTypeId, Action<Identity>? setOptionalFields)
    {
        var identity = new Identity(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            companyId,
            userStatusId,
            identityTypeId);
        setOptionalFields?.Invoke(identity);
        return _dbContext.Identities.Add(identity).Entity;
    }

    public void AttachAndModifyCompanyUser(Guid companyUserId, Action<CompanyUser>? initialize, Action<CompanyUser> setOptionalParameters)
    {
        var companyUser = new CompanyUser(companyUserId);
        initialize?.Invoke(companyUser);
        var updatedEntity = _dbContext.Attach(companyUser).Entity;
        setOptionalParameters.Invoke(updatedEntity);
    }

    public IQueryable<CompanyUser> GetOwnCompanyUserQuery(
        Guid companyId,
        Guid? companyUserId = null,
        string? firstName = null,
        string? lastName = null,
        string? email = null,
        IEnumerable<UserStatusId>? statusIds = null)
    {
        return _dbContext.CompanyUsers
            .AsNoTracking()
            .Where(companyUser => companyUser.Identity!.CompanyId == companyId &&
                (!companyUserId.HasValue || companyUser.Id == companyUserId.Value) &&
                (firstName == null || companyUser.Firstname == firstName) &&
                (lastName == null || companyUser.Lastname == lastName) &&
                (email == null || EF.Functions.ILike(companyUser.Email!, $"%{email.EscapeForILike()}%")) &&
                (statusIds == null || statusIds.Contains(companyUser.Identity!.UserStatusId)));
    }

    public Func<int, int, Task<Pagination.Source<CompanyUserTransferData>?>> GetOwnCompanyUserData(
        Guid companyId,
        Guid? companyUserId = null,
        string? firstName = null,
        string? lastName = null,
        string? email = null,
        IEnumerable<UserStatusId>? statusIds = null) =>
        (skip, take) => Pagination.CreateSourceQueryAsync(
            skip,
            take,
        _dbContext.CompanyUsers
            .AsSplitQuery()
            .Where(companyUser =>
                companyUser.Identity!.CompanyId == companyId &&
                (!companyUserId.HasValue || companyUser.Id == companyUserId.Value) &&
                (firstName == null || companyUser.Firstname == firstName) &&
                (lastName == null || companyUser.Lastname == lastName) &&
                (email == null || EF.Functions.ILike(companyUser.Email!, $"%{email.EscapeForILike()}%")) &&
                (statusIds == null || statusIds.Contains(companyUser.Identity!.UserStatusId)))
            .GroupBy(x => x.Identity!.CompanyId),
        x => x.OrderBy(cu => cu.Identity!.DateCreated),
        cu => new CompanyUserTransferData(
                cu.Id,
                cu.Identity!.UserStatusId,
                cu.Identity.DateCreated,
                cu.Firstname,
                cu.Lastname,
                cu.Email,
                cu.Identity!.IdentityAssignedRoles.Select(x => x.UserRole!).Select(userRole => new UserRoleData(
                    userRole.Id,
                    userRole.Offer!.AppInstances.First().IamClient!.ClientClientId,
                    userRole.UserRoleText)),
                cu.CompanyUserAssignedIdentityProviders.Select(x => new IdpUserTransferId(x.IdentityProvider!.IamIdentityProvider!.IamIdpAlias, x.ProviderId))
            ))
            .SingleOrDefaultAsync();

    public Task<(string? FirstName, string? LastName, string? Email)> GetUserEntityDataAsync(Guid companyUserId, Guid companyId) =>
        _dbContext.CompanyUsers
            .AsNoTracking()
            .Where(companyUser => companyUser.Id == companyUserId &&
                                  companyUser.Identity!.CompanyId == companyId)
            .Select(companyUser => new ValueTuple<string?, string?, string?>(
                companyUser.Firstname,
                companyUser.Lastname,
                companyUser.Email))
            .SingleOrDefaultAsync();

    public IAsyncEnumerable<(Guid CompanyUserId, bool IsFullMatch)> GetMatchingCompanyIamUsersByNameEmail(string firstName, string lastName, string email, Guid companyId, IEnumerable<UserStatusId> companyUserStatusIds) =>
        _dbContext.CompanyUsers
            .AsNoTracking()
            .Where(companyUser =>
                companyUser.Identity!.CompanyId == companyId &&
                companyUser.Identity.IdentityTypeId == IdentityTypeId.COMPANY_USER &&
                companyUserStatusIds.Contains(companyUser.Identity!.UserStatusId) &&
                (companyUser.Email == email ||
                 companyUser.Firstname == firstName ||
                 companyUser.Lastname == lastName))
            .Select(companyUser => new ValueTuple<Guid, bool>(
                    companyUser.Id,
                    companyUser.Firstname == firstName && companyUser.Lastname == lastName && companyUser.Email == email))
            .AsAsyncEnumerable();

    public Task<bool> IsOwnCompanyUserWithEmailExisting(string email, Guid companyId) =>
        _dbContext.CompanyUsers
            .AnyAsync(companyUser => companyUser.Identity!.CompanyId == companyId && companyUser.Email == email);

    public Task<CompanyUserDetailTransferData?> GetOwnCompanyUserDetailsUntrackedAsync(Guid companyUserId, Guid companyId) =>
        _dbContext.CompanyUsers
            .AsNoTracking()
            .AsSplitQuery()
            .Where(companyUser =>
                companyUser.Id == companyUserId &&
                companyUser.Identity!.UserStatusId == UserStatusId.ACTIVE &&
                companyUser.Identity!.CompanyId == companyId)
            .Select(companyUser => new CompanyUserDetailTransferData(
                companyUser.Id,
                companyUser.Identity!.DateCreated,
                companyUser.CompanyUserAssignedBusinessPartners.Select(assignedPartner =>
                    assignedPartner.BusinessPartnerNumber),
                companyUser.Identity!.Company!.Name,
                companyUser.Identity!.UserStatusId,
                companyUser.Identity!.IdentityAssignedRoles.Select(x => x.UserRole!.Offer!)
                    .Distinct()
                    .Select(offer => new CompanyUserAssignedRoleDetails(
                        offer.Id,
                        offer.UserRoles.Where(role => companyUser.Identity!.IdentityAssignedRoles.Select(x => x.UserRole).Contains(role)).Select(x => x.UserRoleText)
                    )),
                companyUser.CompanyUserAssignedIdentityProviders.Select(cuidp => new IdpUserTransferId(cuidp.IdentityProvider!.IamIdentityProvider!.IamIdpAlias, cuidp.ProviderId)),
                companyUser.Firstname,
                companyUser.Lastname,
                companyUser.Email))
            .SingleOrDefaultAsync();

    public Task<(IEnumerable<string> AssignedBusinessPartnerNumbers, bool IsValidUser)> GetOwnCompanyUserWithAssignedBusinessPartnerNumbersUntrackedAsync(Guid companyUserId, Guid companyId) =>
        _dbContext.CompanyUsers
            .AsNoTracking()
            .Where(companyUser =>
                companyUser.Id == companyUserId &&
                companyUser.Identity!.CompanyId == companyId)
            .Select(companyUser => new ValueTuple<IEnumerable<string>, bool>(
                companyUser.CompanyUserAssignedBusinessPartners.Select(assignedPartner => assignedPartner.BusinessPartnerNumber),
                true))
            .SingleOrDefaultAsync();

    public Task<CompanyOwnUserTransferDetails?> GetUserDetailsUntrackedAsync(Guid companyUserId, IEnumerable<Guid> userRoleIds) =>
        _dbContext.CompanyUsers
            .AsNoTracking()
            .AsSplitQuery()
            .Where(companyUser => companyUser.Id == companyUserId)
            .Select(companyUser => new CompanyOwnUserTransferDetails(
                companyUser.Id,
                companyUser.Identity!.DateCreated,
                companyUser.CompanyUserAssignedBusinessPartners.Select(assignedPartner =>
                    assignedPartner.BusinessPartnerNumber),
                companyUser.Identity!.Company!.Name,
                companyUser.Identity!.UserStatusId,
                companyUser.Identity!.Company!.OfferSubscriptions
                    .Where(app => app.OfferSubscriptionStatusId == OfferSubscriptionStatusId.ACTIVE)
                    .Select(app => new CompanyUserAssignedRoleDetails(
                        app.OfferId,
                        app.Offer!.UserRoles
                            .Where(role => role.IdentityAssignedRoles.Select(u => u.Identity!).Any(user => user.Id == companyUser.Id && user.IdentityTypeId == IdentityTypeId.COMPANY_USER))
                            .Select(role => role.UserRoleText)
                    )),
                companyUser.Identity!.Company.Identities.Where(i => i.IdentityTypeId == IdentityTypeId.COMPANY_USER && i.IdentityAssignedRoles.Any(role => userRoleIds.Contains(role.UserRoleId))).Select(i => i.CompanyUser!)
                    .Select(admin => new CompanyUserAdminDetails(
                        admin.Id,
                        admin.Email)),
                companyUser.CompanyUserAssignedIdentityProviders.Select(cuidp => new IdpUserTransferId(cuidp.IdentityProvider!.IamIdentityProvider!.IamIdpAlias, cuidp.ProviderId)),
                companyUser.Firstname,
                companyUser.Lastname,
                companyUser.Email))
            .SingleOrDefaultAsync();

    public Task<CompanyUserWithIdpBusinessPartnerData?> GetUserWithCompanyIdpAsync(Guid companyUserId) =>
        _dbContext.CompanyUsers
            .AsSplitQuery()
            .Where(companyUser => companyUser.Id == companyUserId
                                  && companyUser.Identity!.Company!.IdentityProviders
                                      .Any(identityProvider => identityProvider.IdentityProviderTypeId == IdentityProviderTypeId.SHARED))
            .Select(companyUser => new CompanyUserWithIdpBusinessPartnerData(
                new CompanyUserInformation(
                    companyUser.Id,
                    companyUser.Email,
                    companyUser.Firstname,
                    companyUser.Lastname,
                    companyUser.Identity!.Company!.Name,
                    companyUser.Identity.DateCreated,
                    companyUser.Identity.UserStatusId),
                companyUser.Identity!.Company!.IdentityProviders.Where(identityProvider =>
                        identityProvider.IdentityProviderTypeId == IdentityProviderTypeId.SHARED)
                    .Select(identityProvider => identityProvider.IamIdentityProvider!.IamIdpAlias)
                    .SingleOrDefault()!,
                companyUser.CompanyUserAssignedBusinessPartners.Select(assignedPartner =>
                    assignedPartner.BusinessPartnerNumber),
                companyUser.Identity!.Company!.OfferSubscriptions
                    .Where(app => app.OfferSubscriptionStatusId == OfferSubscriptionStatusId.ACTIVE)
                    .Select(app => new CompanyUserAssignedRoleDetails(
                        app.OfferId,
                        app.Offer!.UserRoles
                            .Where(role => role.IdentityAssignedRoles.Select(u => u.Identity!).Any(user => user.Id == companyUser.Id && user.IdentityTypeId == IdentityTypeId.COMPANY_USER))
                            .Select(role => role.UserRoleText)
                    ))))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public IAsyncEnumerable<Guid> GetAllFavouriteAppsForUserUntrackedAsync(Guid companyUserId) =>
        _dbContext.CompanyUsers.AsNoTracking()
            .Where(u => u.Id == companyUserId)
            .SelectMany(u => u.Offers.Select(a => a.Id))
            .ToAsyncEnumerable();

    /// <inheritdoc />
    public IAsyncEnumerable<Guid> GetCompanyUserWithRoleIdForCompany(IEnumerable<Guid> userRoleIds, Guid companyId) =>
        _dbContext.CompanyUsers
            .Where(x =>
                x.Identity!.CompanyId == companyId &&
                x.Identity!.UserStatusId == UserStatusId.ACTIVE &&
                x.Identity!.IdentityAssignedRoles.Select(u => u.UserRoleId).Any(u => userRoleIds.Any(ur => u == ur)))
            .Select(x => x.Id)
            .ToAsyncEnumerable();

    /// <inheritdoc />
    public IAsyncEnumerable<Guid> GetCompanyUserWithRoleId(IEnumerable<Guid> userRoleIds) =>
        _dbContext.CompanyUsers
            .Where(x =>
                x.Identity!.UserStatusId == UserStatusId.ACTIVE &&
                x.Identity!.IdentityAssignedRoles.Select(u => u.UserRoleId).Any(u => userRoleIds.Any(ur => ur == u)))
            .Select(x => x.Id)
            .ToAsyncEnumerable();

    /// <inheritdoc />
    public IAsyncEnumerable<(string Email, string? FirstName, string? LastName)> GetCompanyUserEmailForCompanyAndRoleId(IEnumerable<Guid> userRoleIds, Guid companyId) =>
        _dbContext.CompanyUsers
            .Where(x =>
                x.Identity!.CompanyId == companyId &&
                x.Identity!.UserStatusId == UserStatusId.ACTIVE &&
                x.Identity!.IdentityAssignedRoles.Select(u => u.UserRoleId).Any(u => userRoleIds.Any(ur => ur == u)) &&
                x.Email != null)
            .Select(x => new ValueTuple<string, string?, string?>(x.Email!, x.Firstname, x.Lastname))
            .ToAsyncEnumerable();

    /// <inheritdoc />
    public Task<OfferIamUserData?> GetAppAssignedIamClientUserDataUntrackedAsync(Guid offerId, Guid companyUserId, Guid companyId) =>
        _dbContext.CompanyUsers.AsNoTracking()
            .Where(companyUser => companyUser.Id == companyUserId)
            .Select(companyUser => new
            {
                User = companyUser,
                Subscriptions = companyUser.Identity!.Company!.OfferSubscriptions.Where(subscription => subscription.OfferId == offerId && subscription.OfferSubscriptionStatusId == OfferSubscriptionStatusId.ACTIVE)
            })
            .Select(x => new OfferIamUserData(
                x.Subscriptions.Any(),
                x.Subscriptions.Select(subscription => subscription.AppSubscriptionDetail!.AppInstance!.IamClient!.ClientClientId).Distinct(),
                x.User.Identity!.CompanyId == companyId,
                x.Subscriptions.Select(s => s.Offer!.Name).FirstOrDefault(),
                x.User.Firstname,
                x.User.Lastname))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<CoreOfferIamUserData?> GetCoreOfferAssignedIamClientUserDataUntrackedAsync(Guid offerId, Guid companyUserId, Guid companyId) =>
        _dbContext.CompanyUsers.AsNoTracking()
            .Where(companyUser => companyUser.Id == companyUserId)
            .Select(companyUser => new
            {
                User = companyUser,
                Offer = companyUser.Identity!.Company!.CompanyAssignedRoles
                    .SelectMany(assigned => assigned.CompanyRole!.CompanyRoleAssignedRoleCollection!.UserRoleCollection!.UserRoles)
                    .Select(role => role.Offer)
                    .FirstOrDefault(offer => offer!.Id == offerId)
            })
            .Select(x => new CoreOfferIamUserData(
                x.Offer != null,
                x.Offer!.AppInstances.Select(instance => instance.IamClient!.ClientClientId),
                x.User.Identity!.CompanyId == companyId,
                x.User.Firstname,
                x.User.Lastname))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public IAsyncEnumerable<Guid> GetServiceProviderCompanyUserWithRoleIdAsync(Guid offerId, IEnumerable<Guid> userRoleIds) =>
        _dbContext.Offers
            .Where(x => x.Id == offerId)
            .SelectMany(x => x.ProviderCompany!.Identities)
            .Where(x =>
                x.UserStatusId == UserStatusId.ACTIVE &&
                x.IdentityAssignedRoles.Select(u => u.UserRoleId).Any(u => userRoleIds.Contains(u)))
            .Select(x => x.Id)
            .ToAsyncEnumerable();

    public Func<int, int, Task<Pagination.Source<CompanyAppUserDetails>?>> GetOwnCompanyAppUsersPaginationSourceAsync(
        Guid appId,
        Guid companyUserId,
        IEnumerable<OfferSubscriptionStatusId> subscriptionStatusIds,
        IEnumerable<UserStatusId> companyUserStatusIds,
        CompanyUserFilter filter)
    {
        var (firstName, lastName, email, roleName, hasRole) = filter;

        return (skip, take) => Pagination.CreateSourceQueryAsync(
            skip,
            take,
            _dbContext.CompanyUsers.AsNoTracking()
                .Where(companyUser => companyUser.Id == companyUserId &&
                                    companyUser.Identity!.Company!.OfferSubscriptions.Any(subscription => subscription.OfferId == appId && subscriptionStatusIds.Contains(subscription.OfferSubscriptionStatusId)))
                .SelectMany(companyUser => companyUser.Identity!.Company!.Identities.Where(x => x.IdentityTypeId == IdentityTypeId.COMPANY_USER).Select(i => i.CompanyUser!))
                .Where(companyUser =>
                    (firstName == null || EF.Functions.ILike(companyUser.Firstname!, $"%{firstName.EscapeForILike()}%")) &&
                    (lastName == null || EF.Functions.ILike(companyUser.Lastname!, $"%{lastName.EscapeForILike()}%")) &&
                    (email == null || EF.Functions.ILike(companyUser.Email!, $"%{email.EscapeForILike()}%")) &&
                    (roleName == null || companyUser.Identity!.IdentityAssignedRoles.Any(userRole => userRole.UserRole!.OfferId == appId && EF.Functions.ILike(userRole.UserRole!.UserRoleText, $"%{roleName.EscapeForILike()}%"))) &&
                    (!hasRole.HasValue || !hasRole.Value || companyUser.Identity!.IdentityAssignedRoles.Any(userRole => userRole.UserRole!.OfferId == appId)) &&
                    (!hasRole.HasValue || hasRole.Value || companyUser.Identity!.IdentityAssignedRoles.All(userRole => userRole.UserRole!.OfferId != appId)) &&
                    companyUserStatusIds.Contains(companyUser.Identity!.UserStatusId))
                .GroupBy(companyUser => companyUser.Identity!.CompanyId),
            null,
            companyUser => new CompanyAppUserDetails(
                    companyUser.Id,
                    companyUser.Identity!.UserStatusId,
                    companyUser.Identity!.IdentityAssignedRoles!.Where(userRole => userRole.UserRole!.Offer!.Id == appId).Select(userRole => userRole.UserRole!.UserRoleText))
            {
                FirstName = companyUser.Firstname,
                LastName = companyUser.Lastname,
                Email = companyUser.Email
            }
        ).SingleOrDefaultAsync();
    }

    /// <inheritdoc />
    public Task<(string? SharedIdpAlias, CompanyUserAccountData AccountData)> GetSharedIdentityProviderUserAccountDataUntrackedAsync(Guid companyUserId) =>
        _dbContext.CompanyUsers.AsNoTracking().AsSplitQuery()
            .Where(companyUser => companyUser.Id == companyUserId)
            .Select(companyUser => new ValueTuple<string?, CompanyUserAccountData>(
                companyUser.Identity!.Company!.IdentityProviders.SingleOrDefault(identityProvider => identityProvider.IdentityProviderTypeId == IdentityProviderTypeId.SHARED)!.IamIdentityProvider!.IamIdpAlias,
                new CompanyUserAccountData(
                    companyUser.Id,
                    companyUser.CompanyUserAssignedBusinessPartners.Select(assignedPartner =>
                        assignedPartner.BusinessPartnerNumber),
                    companyUser.Identity!.IdentityAssignedRoles.Select(assignedRole =>
                        assignedRole.UserRoleId),
                    companyUser.Offers.Select(offer => offer.Id),
                    companyUser.Invitations.Select(invitation => invitation.Id))
            ))
            .SingleOrDefaultAsync();

    public IAsyncEnumerable<CompanyUserAccountData> GetCompanyUserAccountDataUntrackedAsync(IEnumerable<Guid> companyUserIds, Guid companyId) =>
       _dbContext.CompanyUsers.AsNoTracking().AsSplitQuery()
           .Where(companyUser => companyUserIds.Contains(companyUser.Id) &&
               companyUser.Identity!.Company!.Id == companyId)
           .Select(companyUser => new CompanyUserAccountData(
               companyUser.Id,
               companyUser.CompanyUserAssignedBusinessPartners.Select(assignedPartner =>
                   assignedPartner.BusinessPartnerNumber),
               companyUser.Identity!.IdentityAssignedRoles.Select(assignedRole =>
                   assignedRole.UserRoleId),
               companyUser.Offers.Select(offer => offer.Id),
               companyUser.Invitations.Select(invitation => invitation.Id)
           ))
           .AsAsyncEnumerable();

    /// <inheritdoc />
    public Task<(bool IsSameCompany, IEnumerable<Guid> RoleIds)> GetRolesForCompanyUser(Guid companyId, IEnumerable<Guid> roleIds, Guid companyUserId) =>
        _dbContext.Identities.AsNoTracking()
            .Where(i => i.Id == companyUserId)
            .Select(i => new ValueTuple<bool, IEnumerable<Guid>>(
                i.CompanyId == companyId,
                i.IdentityAssignedRoles.Select(x => x.UserRoleId).Where(x => roleIds.Contains(x))))
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
    public Task<string?> GetCompanyBpnForIamUserAsync(Guid companyUserId) =>
        _dbContext.CompanyUsers
            .Where(x => x.Id == companyUserId)
            .Select(x => x.Identity!.Company!.BusinessPartnerNumber)
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Identity AttachAndModifyIdentity(Guid identityId, Action<Identity>? initialize, Action<Identity> modify)
    {
        var companyUser = new Identity(identityId, default, Guid.Empty, default, default);
        initialize?.Invoke(companyUser);
        var updatedEntity = _dbContext.Attach(companyUser).Entity;
        modify.Invoke(updatedEntity);
        return updatedEntity;
    }

    public void AttachAndModifyIdentities(IEnumerable<(Guid IdentityId, Action<Identity>? Initialize, Action<Identity> Modify)> identities)
    {
        var initial = identities.Select(x =>
            {
                var identity = new Identity(x.IdentityId, default, Guid.Empty, default, default);
                x.Initialize?.Invoke(identity);
                return (Identity: identity, modify: x.Modify);
            }
        ).ToList();
        _dbContext.AttachRange(initial.Select(x => x.Identity));
        initial.ForEach(x => x.modify(x.Identity));
    }

    public CompanyUserAssignedIdentityProvider AddCompanyUserAssignedIdentityProvider(Guid companyUserId, Guid identityProviderId, string providerId, string userName) =>
        _dbContext.CompanyUserAssignedIdentityProviders.Add(new CompanyUserAssignedIdentityProvider(companyUserId, identityProviderId, providerId, userName)).Entity;

    public void RemoveCompanyUserAssignedIdentityProviders(IEnumerable<(Guid CompanyUserId, Guid IdentityProviderId)> companyUserIdentityProviderIds) =>
        _dbContext.CompanyUserAssignedIdentityProviders.RemoveRange(companyUserIdentityProviderIds.Select(x => new CompanyUserAssignedIdentityProvider(x.CompanyUserId, x.IdentityProviderId, null!, null!)));

    public IAsyncEnumerable<CompanyUserIdentityProviderProcessData> GetUserAssignedIdentityProviderForNetworkRegistration(Guid networkRegistrationId) =>
        _dbContext.CompanyUsers
            .Where(cu =>
                cu.Identity!.UserStatusId == UserStatusId.PENDING &&
                cu.Identity.Company!.NetworkRegistration!.Id == networkRegistrationId)
            .Select(cu =>
                new CompanyUserIdentityProviderProcessData(
                    cu.Id,
                    cu.Firstname,
                    cu.Lastname,
                    cu.Email,
                    cu.Identity!.Company!.Name,
                    cu.Identity.Company.BusinessPartnerNumber,
                    cu.CompanyUserAssignedIdentityProviders.Select(assigned =>
                    new ProviderLinkTransferData(
                        assigned.UserName,
                        assigned.IdentityProvider!.IamIdentityProvider!.IamIdpAlias,
                        assigned.ProviderId))
                ))
            .ToAsyncEnumerable();

    public Task<(bool Exists, string ProviderId, string Username)> GetCompanyUserAssignedIdentityProvider(Guid companyUserId, Guid identityProviderId) =>
        _dbContext.CompanyUserAssignedIdentityProviders
            .Where(x => x.IdentityProviderId == identityProviderId && x.CompanyUserId == companyUserId)
            .Select(x => new ValueTuple<bool, string, string>(
                true,
                x.ProviderId,
                x.UserName
            ))
            .SingleOrDefaultAsync();

    public void AttachAndModifyUserAssignedIdentityProvider(Guid companyUserId, Guid identityProviderId, Action<CompanyUserAssignedIdentityProvider>? initialize, Action<CompanyUserAssignedIdentityProvider> modify)
    {
        var companyUser = new CompanyUserAssignedIdentityProvider(companyUserId, identityProviderId, null!, null!);
        initialize?.Invoke(companyUser);
        var updatedEntity = _dbContext.Attach(companyUser).Entity;
        modify.Invoke(updatedEntity);
    }

    public Task<(bool Exists, string? RecipientMail)> GetUserMailData(Guid companyUserId) =>
        _dbContext.CompanyUsers
            .Where(x => x.Id == companyUserId)
            .Select(x => new ValueTuple<bool, string?>(true, x.Email))
            .SingleOrDefaultAsync();

    public Task<bool> CheckUserExists(Guid companyUserId) =>
        _dbContext.CompanyUsers.AnyAsync(x => x.Id == companyUserId);

    public IAsyncEnumerable<Guid> GetNextIdentitiesForNetworkRegistration(Guid networkRegistrationId, IEnumerable<UserStatusId> validUserStates) =>
        _dbContext.CompanyUsers
            .Where(cu =>
                validUserStates.Any(v => v == cu.Identity!.UserStatusId) &&
                cu.Identity!.Company!.NetworkRegistration!.Id == networkRegistrationId)
            .Select(x => x.Id)
            .Take(2)
            .ToAsyncEnumerable();

    public void CreateCompanyUserAssignedProcessRange(IEnumerable<(Guid CompanyUserId, Guid ProcessId)> companyUserProcessIds) =>
        _dbContext.AddRange(companyUserProcessIds.Select(x => new CompanyUserAssignedProcess(x.CompanyUserId, x.ProcessId)));

    public Task<Guid> GetCompanyUserIdForProcessIdAsync(Guid processId) =>
        _dbContext.CompanyUserAssignedProcesses
            .Where(cuap => cuap.ProcessId == processId)
            .Select(cuap => cuap.CompanyUserId)
            .SingleOrDefaultAsync();

    public void DeleteCompanyUserAssignedProcess(Guid companyUserId, Guid processId) =>
        _dbContext.Remove(new CompanyUserAssignedProcess(companyUserId, processId));
}
