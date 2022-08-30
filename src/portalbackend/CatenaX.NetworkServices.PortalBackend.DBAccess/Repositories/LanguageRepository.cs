using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using Microsoft.EntityFrameworkCore;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

/// <summary>
/// Handles the read access to the languages
/// </summary>
public class LanguageRepository : ILanguageRepository
{
    private readonly PortalDbContext _portalDbContext;

    /// <summary>
    /// Initializes a new instance of <see cref="LanguageRepository"/>
    /// </summary>
    /// <param name="portalDbContext">Access to the database</param>
    public LanguageRepository(PortalDbContext portalDbContext)
    {
        _portalDbContext = portalDbContext;
    }

    /// <inheritdoc />
    public Task<string?> GetLanguageAsync(string languageShortName) =>
        _portalDbContext.Languages
            .AsNoTracking()
            .Where(language => language.ShortName == languageShortName)
            .Select(language => language.ShortName)
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<List<string>> GetLanguageCodesUntrackedAsync(ICollection<string> languageCodes) =>
        _portalDbContext.Languages.AsNoTracking()
            .Where(x => languageCodes.Any(y => y == x.ShortName))
            .Select(x => x.ShortName)
            .ToListAsync();
}