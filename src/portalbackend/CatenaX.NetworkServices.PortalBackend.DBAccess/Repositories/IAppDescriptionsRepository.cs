using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

/// <summary>
/// Repository for persistence layer access relating <see cref="AppDescription"/> entities.
/// </summary>
public interface IAppDescriptionsRepository
{
    /// <summary>
    /// Adds <see cref="AppDescription"/>s to the database
    /// </summary>
    /// <param name="appDescriptions">The app descriptions that should be added to the database</param>
    void AddAppDescriptions(IEnumerable<AppDescription> appDescriptions);
}
