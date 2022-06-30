using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

/// <inheritdoc />
public class AppLanguagesRepository : IAppLanguagesRepository
{
    private readonly PortalDbContext _dbContext;

    /// <summary>
    /// Creates a new instance of <see cref="AppLanguagesRepository"/>
    /// </summary>
    /// <param name="dbContext">Access to the database</param>
    public AppLanguagesRepository(PortalDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public void AddAppLanguages(IEnumerable<AppLanguage> appLanguages) =>
        _dbContext.AppLanguages.AddRange(appLanguages);
}
