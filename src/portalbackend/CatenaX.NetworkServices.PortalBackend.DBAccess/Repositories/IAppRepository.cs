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
}
