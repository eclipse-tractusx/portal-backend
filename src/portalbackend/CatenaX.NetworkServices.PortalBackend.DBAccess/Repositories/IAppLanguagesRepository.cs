using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

/// <summary>
/// Repository for persistence layer access relating <see cref="AppLanguage"/> entities.
/// </summary>
public interface IAppLanguagesRepository
{
    /// <summary>
    /// Adds <see cref="AppLanguage"/>s to the database
    /// </summary>
    /// <param name="appLanguages">The app languages that should be added to the database</param>
    void AddAppLanguages(IEnumerable<AppLanguage> appLanguages);
}
