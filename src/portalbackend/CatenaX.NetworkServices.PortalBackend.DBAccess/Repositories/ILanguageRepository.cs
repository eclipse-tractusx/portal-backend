namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

public interface ILanguageRepository
{
    /// <summary>
    /// Gets the languages with the matching short name
    /// </summary>
    /// <param name="languageShortName">the short name of the language</param>
    /// <returns>the shortname if existing otherwise false</returns>
    Task<string?> GetLanguageAsync(string languageShortName);

    /// <summary>
    /// Checks whether the given language codes exists in the persistence storage
    /// </summary>
    /// <param name="languageCodes">the language codes that should be checked</param>
    /// <returns>Returns the found language codes</returns>
    Task<List<string>> GetLanguageCodesUntrackedAsync(ICollection<string> languageCodes);
}