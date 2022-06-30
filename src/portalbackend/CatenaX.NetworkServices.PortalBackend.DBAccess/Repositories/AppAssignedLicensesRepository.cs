using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

/// <inheritdoc />
public class AppAssignedLicensesRepository : IAppAssignedLicensesRepository
{
    private readonly PortalDbContext _dbContext;

    /// <summary>
    /// Creates a new instance of <see cref="AppAssignedLicensesRepository"/>
    /// </summary>
    /// <param name="dbContext">Access to the database</param>
    public AppAssignedLicensesRepository(PortalDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public void AddAppAssignedLicense(AppAssignedLicense appAssignedLicense) => 
        _dbContext.AppAssignedLicenses.Add(appAssignedLicense);
}
