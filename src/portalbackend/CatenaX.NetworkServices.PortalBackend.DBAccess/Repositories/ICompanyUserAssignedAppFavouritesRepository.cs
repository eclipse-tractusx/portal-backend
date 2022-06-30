using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

public interface ICompanyUserAssignedAppFavouritesRepository
{
    /// <summary>
    /// Removes the app with the given appId for the companyUser from the database
    /// </summary>
    /// <param name="appId">Id of the app that should be removed</param>
    /// <param name="companyUserId">Id of the company user</param>
    void RemoveFavouriteAppForUser(Guid appId, Guid companyUserId);

    /// <summary>
    /// Adds the given app favourite to the database
    /// </summary>
    /// <param name="appFavourite">The appFavourite that should be added to the database</param>
    void AddAppFavourite(CompanyUserAssignedAppFavourite appFavourite);
}
