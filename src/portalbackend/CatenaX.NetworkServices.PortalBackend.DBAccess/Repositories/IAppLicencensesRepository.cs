using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

/// <summary>
/// Repository for persistence layer access relating <see cref="AppLicense"/> entities.
/// </summary>
public interface IAppLicensesRepository
{
    /// <summary>
    /// Adds an <see cref="AppLicense"/> to the database
    /// </summary>
    /// <param name="license">The AppLicense that should be added to the database</param>
    void AddAppLicenses(AppLicense license);
}
