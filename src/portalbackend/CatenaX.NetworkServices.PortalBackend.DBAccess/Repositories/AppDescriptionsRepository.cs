using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

/// <inheritdoc />
public class AppDescriptionsRepository : IAppDescriptionsRepository
{
    private readonly PortalDbContext _dbContext;

    /// <summary>
    /// Creates a new instance of <see cref="AppDescriptionsRepository"/>
    /// </summary>
    /// <param name="dbContext">Access to the database</param>
    public AppDescriptionsRepository(PortalDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public void AddAppDescriptions(IEnumerable<AppDescription> appDescriptions) =>
        _dbContext.AppDescriptions.AddRange(appDescriptions);
}
