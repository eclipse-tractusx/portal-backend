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

    public CompanyUser CreateCompanyUser(string? firstName, string? lastName, string email, Guid companyId, CompanyUserStatusId companyUserStatusId) =>
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

    public IamUser CreateIamUser(CompanyUser user, string iamUserEntityId) =>
        _dbContext.IamUsers.Add(
            new IamUser(
                iamUserEntityId,
                user.Id)).Entity;

    public Task<CompanyUserDetails?> GetOwnCompanyUserDetailsUntrackedAsync(Guid companyUserId, string iamUserId) =>
        _dbContext.CompanyUsers
            .AsNoTracking()
            .Where(companyUser => companyUser.Id == companyUserId
                && companyUser.CompanyUserStatusId == CompanyUserStatusId.ACTIVE
                && companyUser.Company!.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == iamUserId))
            .Select(companyUser => new CompanyUserDetails(
                companyUser.Id,
                companyUser.DateCreated,
                companyUser.CompanyUserAssignedBusinessPartners.Select(assignedPartner => assignedPartner.BusinessPartnerNumber),
                companyUser.Company!.Name,
                companyUser.CompanyUserStatusId)
                {
                    FirstName = companyUser.Firstname,
                    LastName = companyUser.Lastname,
                    Email = companyUser.Email
                })
            .SingleOrDefaultAsync();

    public Task<CompanyUserBusinessPartners?> GetOwnCompanyUserWithAssignedBusinessPartnerNumbersUntrackedAsync(Guid companyUserId, string adminUserId) =>
        _dbContext.IamUsers
            .AsNoTracking()
            .Where(iamUser => iamUser.UserEntityId == adminUserId)
            .SelectMany(iamUser => iamUser.CompanyUser!.Company!.CompanyUsers)
            .Where(companyUser => companyUser.Id == companyUserId)
            .Select(companyUser => new CompanyUserBusinessPartners(
                companyUser.IamUser!.UserEntityId,
                companyUser.CompanyUserAssignedBusinessPartners.Select(assignedPartner => assignedPartner.BusinessPartnerNumber)
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

    /// <inheritdoc/>
    public async Task<Guid?> GetCompanyUserIdForIamUserUntrackedAsync(string iamUserId) =>
        await _dbContext.IamUsers
            .AsNoTracking()
            .Where(iamUser =>
                iamUser.UserEntityId == iamUserId)
            .Select(iamUser =>
                iamUser.CompanyUser!.Id)
            .SingleOrDefaultAsync();

    /// <inheritdoc/>
    public Task<CompanyIamUser?> GetIdpUserByIdUntrackedAsync(Guid companyUserId, string adminUserId) =>
        _dbContext.CompanyUsers.AsNoTracking()
            .Where(companyUser => companyUser.Id == companyUserId
                && companyUser.Company!.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == adminUserId))
            .Select(companyUser => new CompanyIamUser
            {
                TargetIamUserId = companyUser.IamUser!.UserEntityId,
                IdpName = companyUser.Company!.IdentityProviders
                    .Select(identityProvider => identityProvider.IamIdentityProvider!.IamIdpAlias)
                    .SingleOrDefault(),
                RoleIds = companyUser.CompanyUserAssignedRoles.Select(companyUserAssignedRole => companyUserAssignedRole.UserRoleId)
            }).SingleOrDefaultAsync();

    public Task<CompanyUserDetails?> GetUserDetailsUntrackedAsync(string iamUserId) =>
        _dbContext.CompanyUsers
            .AsNoTracking()
            .Where(companyUser => companyUser.IamUser!.UserEntityId == iamUserId)
            .Select(companyUser => new CompanyUserDetails(
                companyUser.Id,
                companyUser.DateCreated,
                companyUser.CompanyUserAssignedBusinessPartners.Select(assignedPartner => assignedPartner.BusinessPartnerNumber),
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
                .Any(identityProvider => identityProvider.IdentityProviderCategoryId == IdentityProviderCategoryId.KEYCLOAK_SHARED))
        .Include(companyUser => companyUser.Company)
        .Include(companyUser => companyUser.IamUser)
        .Select(companyUser => new CompanyUserWithIdpBusinessPartnerData(
            companyUser,
            companyUser.Company!.IdentityProviders.Where(identityProvider => identityProvider.IdentityProviderCategoryId == IdentityProviderCategoryId.KEYCLOAK_SHARED)
                .Select(identityProvider => identityProvider.IamIdentityProvider!.IamIdpAlias)
                .SingleOrDefault()!,
            companyUser.CompanyUserAssignedBusinessPartners.Select(assignedPartner => assignedPartner.BusinessPartnerNumber)
        ))
        .SingleOrDefaultAsync();

    public Task<CompanyUserWithIdpData?> GetUserWithIdpAsync(string iamUserId) =>
        _dbContext.CompanyUsers
            .Where(companyUser => companyUser.IamUser!.UserEntityId == iamUserId
                && companyUser!.Company!.IdentityProviders
                    .Any(identityProvider => identityProvider.IdentityProviderCategoryId == IdentityProviderCategoryId.KEYCLOAK_SHARED))
            .Include(companyUser => companyUser.CompanyUserAssignedRoles)
            .Include(companyUser => companyUser.IamUser)
            .Select(companyUser => new CompanyUserWithIdpData(
                companyUser,
                companyUser.Company!.IdentityProviders.Where(identityProvider => identityProvider.IdentityProviderCategoryId == IdentityProviderCategoryId.KEYCLOAK_SHARED)
                    .Select(identityProvider => identityProvider.IamIdentityProvider!.IamIdpAlias)
                    .SingleOrDefault()!
            ))
            .SingleOrDefaultAsync();
    
    public Task<Guid> GetCompanyUserIdForUserApplicationUntrackedAsync(Guid applicationId, string iamUserId) =>
        _dbContext.IamUsers
            .AsNoTracking()
            .Where(iamUser =>
                iamUser.UserEntityId == iamUserId
                && iamUser.CompanyUser!.Company!.CompanyApplications.Any(application => application.Id == applicationId))
            .Select(iamUser =>
                iamUser.CompanyUserId
            )
            .SingleOrDefaultAsync();
}
