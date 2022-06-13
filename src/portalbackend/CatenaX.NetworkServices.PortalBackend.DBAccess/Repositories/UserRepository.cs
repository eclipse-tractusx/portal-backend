using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
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


    public Task<Guid> GetCompanyIdForIamUserUntrackedAsync(string iamUserId) =>
        _dbContext.IamUsers
            .AsNoTracking()
            .Where(iamUser =>
                iamUser.UserEntityId == iamUserId)
            .Select(iamUser =>
                iamUser.CompanyUser!.Company!.Id)
            .SingleOrDefaultAsync();

    /// <inheritdoc/>
    public Task<CompanyIamUser> GetIdpUserByIdAsync(Guid companyUserId, string adminUserId) =>
           _dbContext.CompanyUsers.AsNoTracking()
               .Where(companyUser => companyUser.Id == companyUserId
                   && companyUser.Company!.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == adminUserId))
               .Select(companyUser => new CompanyIamUser
               {
                   TargetIamUserId = companyUser.IamUser!.UserEntityId,
                   TargetCompanyUserId = companyUser.IamUser!.CompanyUserId,
                   IdpName = companyUser.Company!.IdentityProviders
                       .Select(identityProvider => identityProvider.IamIdentityProvider!.IamIdpAlias)
                       .SingleOrDefault(),
                   RoleIds = companyUser.CompanyUserAssignedRoles.Select(companyUserAssignedRole => companyUserAssignedRole.UserRoleId)
               }).SingleOrDefaultAsync();

    /// <inheritdoc/>
    public Task<string> GetAppAssignedRolesClientIdAsync(Guid appId) =>
             _dbContext.AppAssignedClients.AsNoTracking()
                 .Where(appClient => appClient.AppId == appId)
                 .Select(appClient => appClient.IamClient!.ClientClientId)
                 .SingleOrDefaultAsync();
}
