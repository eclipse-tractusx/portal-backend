using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

/// Implementation of <see cref="IConnectorsRepository"/> accessing database with EF Core.
public class ConnectorsRepository : IConnectorsRepository
{
    private readonly PortalDbContext _context;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalDbContext">PortalDb context.</param>
    public ConnectorsRepository(PortalDbContext portalDbContext)
    {
        this._context = portalDbContext;
    }

    /// <inheritdoc/>
    public IQueryable<Connector> GetAllCompanyConnectorsForIamUser(string iamUserId)
    {
        return _context.IamUsers.AsNoTracking()
            .Where(u => u.UserEntityId == iamUserId)
            .SelectMany(u => u.CompanyUser!.Company!.ProvidedConnectors);
    }
}
