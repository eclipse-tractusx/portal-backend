using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using Microsoft.EntityFrameworkCore;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

public class AppUserRepository : IAppUserRepository
{
    private readonly PortalDbContext _dbContext;

    public AppUserRepository(PortalDbContext portalDbContext)
    {
        _dbContext = portalDbContext;
    }

    public IQueryable<CompanyUser> GetCompanyAppUsersUntrackedAsync(Guid appId, string iamUserId) =>
        _dbContext.CompanyAssignedApps
            .AsNoTracking()
            .Where(app => app.AppId == appId
                && app.Company!.CompanyStatusId == CompanyStatusId.ACTIVE
                && app.Company!.CompanyUsers!.Any(user => user.IamUser!.UserEntityId == iamUserId))
            .SelectMany(app => app.Company!.CompanyUsers);
}
