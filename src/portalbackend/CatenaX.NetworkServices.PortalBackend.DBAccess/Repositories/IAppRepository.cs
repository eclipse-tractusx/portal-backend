using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

/// <summary>
/// Repository for accessing apps on persistence layer.
/// </summary>
public interface IAppRepository
{
    /// <summary>
    /// Checks if an app with the given id exists in the persistence layer. 
    /// </summary>
    /// <param name="appId">Id of the app.</param>
    /// <returns><c>true</c> if an app exists on the persistence layer with the given id, <c>false</c> if not.</returns>
    public Task<bool> CheckAppExistsById(Guid appId);

    /// <summary>
    /// Retrieves app provider company details by app id.
    /// </summary>
    /// <param name="appId">ID of the app.</param>
    /// <returns>Tuple of provider company details.</returns>
    public Task<(string appName, string providerName, string providerContactEmail)> GetAppProviderDetailsAsync(Guid appId);

    /// <summary>
    /// Get Client Name by App Id
    /// </summary>
    /// <param name="appId"></param>
    /// <returns>Client Name</returns>
    Task<string?> GetAppAssignedClientIdUntrackedAsync(Guid appId);

    /// <summary>
    /// Adds an app to the database
    /// </summary>
    /// <param name="appEntity">The app that should be added to the database</param>
    void AddApp(App appEntity);

    /// <summary>
    /// Gets all active apps with an optional filtered with the languageShortName
    /// </summary>
    /// <param name="languageShortName">The optional language shortName</param>
    /// <returns>Returns a async enumerable of <see cref="AppData"/></returns>
    IAsyncEnumerable<AppData> GetAllActiveAppsAsync(string? languageShortName);

    /// <summary>
    /// Gets the details of an app by its id
    /// </summary>
    /// <param name="appId">Id of the application to get details for</param>
    /// <param name="companyId">OPTIONAL: Id of the company</param>
    /// <param name="languageShortName">OPTIONAL: language shortName</param>
    /// <returns>Returns the details of the application</returns>
    Task<AppDetailsData> GetDetailsByIdAsync(Guid appId, Guid? companyId, string? languageShortName);
}
