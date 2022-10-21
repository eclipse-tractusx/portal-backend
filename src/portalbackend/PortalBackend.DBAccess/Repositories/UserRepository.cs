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

using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Microsoft.EntityFrameworkCore;
using PortalBackend.DBAccess.Models;

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

    public Task<RegistrationData?> GetRegistrationDataUntrackedAsync(Guid applicationId, string iamUserId) =>
        _dbContext.IamUsers
            .AsNoTracking()
            .Where(iamUser =>
                iamUser.UserEntityId == iamUserId
                && iamUser.CompanyUser!.Company!.CompanyApplications.Any(application => application.Id == applicationId))
            .Select(iamUser => iamUser.CompanyUser!.Company)
            .Select(company => new RegistrationData(
                company!.Id,
                company.Name,
                company.CompanyAssignedRoles!.Select(companyAssignedRole => companyAssignedRole.CompanyRoleId),
                company.CompanyUsers.SelectMany(companyUser => companyUser!.Documents!.Select(document => new RegistrationDocumentNames(document.DocumentName))),
                company.Consents.Where(consent => consent.ConsentStatusId == PortalBackend.PortalEntities.Enums.ConsentStatusId.ACTIVE)
                    .Select(consent => new AgreementConsentStatusForRegistrationData(
                        consent.AgreementId, consent.ConsentStatusId)))
            {
                City = company.Address!.City,
                Streetname = company.Address.Streetname,
                CountryAlpha2Code = company.Address.CountryAlpha2Code,
                BusinessPartnerNumber = company.BusinessPartnerNumber,
                Shortname = company.Shortname,
                Region = company.Address.Region,
                Streetadditional = company.Address.Streetadditional,
                Streetnumber = company.Address.Streetnumber,
                Zipcode = company.Address.Zipcode,
                CountryDe = company.Address.Country!.CountryNameDe,
                TaxId = company.TaxId
            }).SingleOrDefaultAsync();

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

    public CompanyUser AttachAndModifyCompanyUser(Guid companyUserId, Action<CompanyUser>? setOptionalParameters = null)
    {
        var companyUser = _dbContext.Attach(new CompanyUser(companyUserId, Guid.Empty, default, default, Guid.Empty)).Entity;
        setOptionalParameters?.Invoke(companyUser);
        return companyUser;
    }

    public IamUser CreateIamUser(Guid companyUserId, string iamUserId) =>
        _dbContext.IamUsers.Add(
            new IamUser(
                iamUserId,
                companyUserId)).Entity;

    public IamUser RemoveIamUser(IamUser iamUser) =>
        _dbContext.Remove(iamUser).Entity;

    public IQueryable<CompanyUser> GetOwnCompanyUserQuery(
        string adminUserId,
        Guid? companyUserId = null,
        string? userEntityId = null,
        string? firstName = null,
        string? lastName = null,
        string? email = null)
        {
        char[] escapeChar = { '%', '_', '[', ']', '^' };
        return _dbContext.CompanyUsers.AsNoTracking()
            .Where(companyUser => companyUser.IamUser!.UserEntityId == adminUserId)
            .SelectMany(companyUser => companyUser.Company!.CompanyUsers)
            .Where(companyUser =>
                (userEntityId == null || companyUser.IamUser!.UserEntityId == userEntityId) &&
                (!companyUserId.HasValue || companyUser.Id == companyUserId.Value) &&
                (firstName == null || companyUser.Firstname == firstName) &&
                (lastName == null || companyUser.Lastname == lastName) &&
                (email == null || EF.Functions.ILike(companyUser.Email!, $"%{email.Trim(escapeChar)}%")));
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

    public IAsyncEnumerable<(string? UserEntityId, Guid CompanyUserId)> GetMatchingCompanyIamUsersByNameEmail(string firstName, string lastName, string email, Guid companyId) =>
        _dbContext.CompanyUsers
            .AsNoTracking()
            .Where(companyUser =>
                companyUser.CompanyId == companyId &&
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

    public Task<(Guid CompanyId, string Bpn)> GetCompanyIdAndBpnForIamUserUntrackedAsync(string iamUserId) =>
        _dbContext.IamUsers
            .AsNoTracking()
            .Where(iamUser => iamUser.UserEntityId == iamUserId)
            .Select(iamUser => new ValueTuple<Guid, string>(iamUser.CompanyUser!.Company!.Id, iamUser.CompanyUser!.Company!.BusinessPartnerNumber!))
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

    public Task<CompanyUserWithIdpData?> GetUserWithSharedIdpDataAsync(string iamUserId) =>
        _dbContext.CompanyUsers
            .Where(companyUser => companyUser.IamUser!.UserEntityId == iamUserId)
            .Include(companyUser => companyUser.CompanyUserAssignedRoles)
            .Include(companyUser => companyUser.IamUser)
            .Select(companyUser => new CompanyUserWithIdpData(
                companyUser,
                companyUser.Company!.IdentityProviders.SingleOrDefault(identityProvider => identityProvider.IdentityProviderCategoryId == IdentityProviderCategoryId.KEYCLOAK_SHARED)!.IamIdentityProvider!.IamIdpAlias
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
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<(Guid UserId, string Email)> GetCompanyUserIdAndEmailForIamUserUntrackedAsync(string userId) =>
        _dbContext.CompanyUsers.AsNoTracking()
            .Where(cu => cu.IamUser!.UserEntityId == userId)
            .Select(cu => new ValueTuple<Guid, string>(cu.Id, cu.Email!))
            .SingleAsync();

    /// <inheritdoc />
    public IAsyncEnumerable<(Guid CompanyUserId, bool IsIamUser, string CompanyShortName, Guid CompanyId)> GetCompanyUserWithIamUserCheckAndCompanyShortName(string iamUserId, Guid salesManagerId) => 
        _dbContext.CompanyUsers.Where(x => x.IamUser!.UserEntityId == iamUserId || x.Id == salesManagerId)
            .Select(companyUser => new ValueTuple<Guid, bool, string, Guid>(companyUser.Id, companyUser.IamUser!.UserEntityId == iamUserId, companyUser.Company!.Shortname!, companyUser.CompanyId))
            .ToAsyncEnumerable();

    /// <inheritdoc />
    public IAsyncEnumerable<Guid> GetAllFavouriteAppsForUserUntrackedAsync(string userId) =>
        _dbContext.IamUsers.AsNoTracking()
            .Where(u => u.UserEntityId == userId) // Id is unique, so single user
            .SelectMany(u => u.CompanyUser!.Offers.Select(a => a.Id))
            .ToAsyncEnumerable();

    /// <inheritdoc />
    public IAsyncEnumerable<BusinessAppData> GetAllBusinessAppDataForUserIdAsync(string userId) =>
        _dbContext.IamUsers.AsNoTracking().Where(u => u.UserEntityId == userId)
            .SelectMany(u => u.CompanyUser!.Company!.BoughtOffers)
            .Intersect(
                _dbContext.IamUsers.AsNoTracking().Where(u => u.UserEntityId == userId)
                    .SelectMany(u => u.CompanyUser!.UserRoles.Select(r => r.Offer))
                    .Where(x => x != null)
                    .Select(x => x!)
            )
            .Select(app => new BusinessAppData(
                app!.Id,
                app.Name ?? Constants.ErrorString,
                app.OfferSubscriptions.FirstOrDefault(x => x.OfferId == app.Id) == null ? Constants.ErrorString : app.OfferSubscriptions.First(x => x.OfferId == app.Id).AppSubscriptionDetail!.AppSubscriptionUrl ?? Constants.ErrorString,
                app.ThumbnailUrl ?? Constants.ErrorString,
                app.Provider
            )).AsAsyncEnumerable();

    /// <inheritdoc />
    public IAsyncEnumerable<(Guid CompanyUserId, bool IsIamUser)> GetCompanyUserWithIamUserCheck(string iamUserId, Guid companyUserId) =>
        _dbContext.CompanyUsers.Where(x => x.IamUser!.UserEntityId == iamUserId || x.Id == companyUserId)
            .Select(companyUser => new ValueTuple<Guid, bool>(companyUser.Id, companyUser.IamUser!.UserEntityId == iamUserId))
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
    public Task<Guid> GetServiceAccountCompany(string iamUserId) =>
        _dbContext.IamServiceAccounts
            .Where(x => x.UserEntityId == iamUserId)
            .Select(x => x.CompanyServiceAccount!.CompanyId)
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<(string? IamClientId, string IamUserId, bool IsSameCompany)> GetAppAssignedIamClientUserDataUntrackedAsync(Guid offerId, Guid companyUserId, string iamUserId) => 
        _dbContext.CompanyUsers.AsNoTracking()
            .Where(companyUser => companyUser.Id == companyUserId)
            .Select(companyUser => new ValueTuple<string?, string, bool>( 
                companyUser.Company!.OfferSubscriptions.SingleOrDefault(subscription => subscription.OfferId == offerId)!.AppSubscriptionDetail!.AppInstance!.IamClient!.ClientClientId, 
                companyUser.IamUser!.UserEntityId,
                companyUser.Company.CompanyUsers.Any(cu => cu.IamUser!.UserEntityId == iamUserId)))
            .SingleOrDefaultAsync();
}
