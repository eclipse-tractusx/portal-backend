using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

public class CompanyUserAssignedAppFavouritesRepository : ICompanyUserAssignedAppFavouritesRepository
{
    private readonly PortalDbContext _dbContext;

    /// <summary>
    /// Creates a new instance of <see cref="CompanyUserAssignedAppFavouritesRepository"/>
    /// </summary>
    /// <param name="dbContext">Access to the database</param>
    public CompanyUserAssignedAppFavouritesRepository(PortalDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    /// <inheritdoc />
    public void RemoveFavouriteAppForUser(Guid appId, Guid companyUserId)
    {
        var rowToRemove = new CompanyUserAssignedAppFavourite(appId, companyUserId);
        this._dbContext.CompanyUserAssignedAppFavourites.Attach(rowToRemove);
        this._dbContext.CompanyUserAssignedAppFavourites.Remove(rowToRemove);
    }

    /// <inheritdoc />
    public void AddAppFavourite(CompanyUserAssignedAppFavourite appFavourite) => 
        this._dbContext.CompanyUserAssignedAppFavourites.Add(appFavourite);
}
