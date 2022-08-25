namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

public interface ILanguageRepository
{
    /// <summary>
    /// Gets the languages with the matching short name
    /// </summary>
    /// <param name="languageShortName">the short name of the language</param>
    /// <returns>the shortname if existing otherwise false</returns>
    Task<string?> GetLanguageAsync(string languageShortName);
}