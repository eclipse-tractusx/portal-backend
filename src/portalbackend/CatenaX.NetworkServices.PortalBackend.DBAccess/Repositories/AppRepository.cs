using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

/// Implementation of <see cref="IAppRepository"/> accessing database with EF Core.
public class AppRepository : IAppRepository
{
    private readonly PortalDbContext _context;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalDbContext">PortalDb context.</param>
    public AppRepository(PortalDbContext portalDbContext)
    {
        this._context = portalDbContext;
    }

    /// <inheritdoc />
    public async Task<bool> CheckAppExistsById(Guid appId)
    {
        return await _context.Apps.AnyAsync(x => x.Id == appId);
    }

    ///<inheritdoc/>
    public async Task<(string appName, string providerName, string providerContactEmail)> GetAppProviderDetailsAsync(Guid appId)
    {
        var appDetails = await _context.Apps.AsNoTracking().Where(a => a.Id == appId).Select(c => new
        {
            c.Name,
            c.Provider,
            c.ContactEmail
        }).SingleAsync();

        if(new []{ appDetails.Name, appDetails.Provider, appDetails.ContactEmail }.Any(d => d is null))
        {
            var nullProperties = new List<string>();
            if (appDetails.Name is null)
            {
                nullProperties.Add($"{nameof(App)}.{nameof(appDetails.Name)}");
            }
            if (appDetails.Provider is null)
            {
                nullProperties.Add($"{nameof(App)}.{nameof(appDetails.Provider)}");
            }
            if(appDetails.ContactEmail is null)
            {
                nullProperties.Add($"{nameof(App)}.{nameof(appDetails.ContactEmail)}");
            }
            throw new Exception($"The following fields of app '{appId}' have not been configured properly: {string.Join(", ", nullProperties)}");
        }

        return (appName: appDetails.Name!, providerName: appDetails.Provider!, providerContactEmail: appDetails.ContactEmail!);
    }
}
