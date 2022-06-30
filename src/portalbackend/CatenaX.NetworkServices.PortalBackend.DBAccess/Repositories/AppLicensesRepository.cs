using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

/// <inheritdoc />
public class AppLicensesRepository : IAppLicensesRepository
{
    private readonly PortalDbContext _dbContext;

    /// <summary>
    /// Creates a new instance of <see cref="AppLicensesRepository"/>
    /// </summary>
    /// <param name="dbContext">Access to the database</param>
    public AppLicensesRepository(PortalDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    /// <inheritdoc />
    public void AddAppLicenses(AppLicense license) =>
        _dbContext.AppLicenses.Add(license);
}
