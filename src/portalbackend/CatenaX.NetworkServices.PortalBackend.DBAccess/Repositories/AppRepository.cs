using CatenaX.NetworkServices.Framework.Models;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
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
    public async Task<bool> CheckAppExistsById(Guid appId) => 
        await _context.Apps.AnyAsync(x => x.Id == appId);

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

    /// <inheritdoc/>
    public Task<string?> GetAppAssignedClientIdUntrackedAsync(Guid appId) =>
        _context.AppAssignedClients.AsNoTracking()
            .Where(appClient => appClient.AppId == appId)
            .Select(appClient => appClient.IamClient!.ClientClientId)
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public void AddApp(App appEntity) => _context.Apps.Add(appEntity);

    /// <inheritdoc />
    public IAsyncEnumerable<AppData> GetAllActiveAppsAsync(string? languageShortName)
    {
        return _context.Apps.AsNoTracking()
            .Where(app => app.DateReleased.HasValue && app.DateReleased <= DateTime.UtcNow)
            .Select(a => new {
                a.Id,
                a.Name,
                VendorCompanyName = a.ProviderCompany!.Name, // This translates into a 'left join' which does return null for all columns if the foreingn key is null. The '!' just makes the compiler happy
                UseCaseNames = a.UseCases.Select(uc => uc.Name),
                a.ThumbnailUrl,
                ShortDescription =
                    _context.Languages.SingleOrDefault(l => l.ShortName == languageShortName) == null 
                        ? null 
                        : a.AppDescriptions.SingleOrDefault(d => d.LanguageShortName == languageShortName)!.DescriptionShort
                          ?? a.AppDescriptions.SingleOrDefault(d => d.LanguageShortName == Constants.DefaultLanguage)!.DescriptionShort,
                LicenseText = a.AppLicenses
                    .Select(license => license.Licensetext)
                    .FirstOrDefault()
            }).AsAsyncEnumerable().Select(app => new AppData(
                app.Name ?? Constants.ErrorString,
                app.ShortDescription ?? Constants.ErrorString,
                app.VendorCompanyName ?? Constants.ErrorString,
                app.LicenseText ?? Constants.ErrorString,
                app.ThumbnailUrl ?? Constants.ErrorString
                )
            {
                Id = app.Id,
                UseCases = app.UseCaseNames.Select(name => name).ToList()
            });
    }

    /// <inheritdoc />
    public async Task<AppDetailsData> GetDetailsByIdAsync(Guid appId, Guid? companyId, string? languageShortName)
    {
        var app = await this._context.Apps.AsNoTracking()
            .Where(a => a.Id == appId)
            .Select(a => new
            {
                a.Id,
                Title = a.Name,
                LeadPictureUri = a.ThumbnailUrl,
                DetailPictureUris = a.AppDetailImages.Select(adi => adi.ImageUrl),
                ProviderUri = a.MarketingUrl,
                a.Provider,
                a.ContactEmail,
                a.ContactNumber,
                UseCases = a.UseCases.Select(u => u.Name),
                LongDescription = 
                    _context.Languages.SingleOrDefault(l => l.ShortName == languageShortName) == null 
                        ? null 
                        : a.AppDescriptions.SingleOrDefault(d => d.LanguageShortName == languageShortName)!.DescriptionLong
                          ?? a.AppDescriptions.SingleOrDefault(d => d.LanguageShortName == Constants.DefaultLanguage)!.DescriptionLong,
                Price = a.AppLicenses
                    .Select(license => license.Licensetext)
                    .FirstOrDefault(),
                Tags = a.Tags.Select(t => t.Name),
                IsPurchased = companyId == null ?
                    (bool?)null :
                    a.Companies.Any(c => c.Id == companyId),
                Languages = a.SupportedLanguages.Select(l => l.ShortName)
            })
            .SingleAsync();
        
        return new AppDetailsData(
            app.Title ?? Constants.ErrorString,
            app.LeadPictureUri ?? Constants.ErrorString,
            app.ProviderUri ?? Constants.ErrorString,
            app.Provider,
            app.LongDescription ?? Constants.ErrorString,
            app.Price ?? Constants.ErrorString
        )
        {
            Id = app.Id,
            IsSubscribed = app.IsPurchased,
            Tags = app.Tags,
            UseCases = app.UseCases,
            DetailPictureUris = app.DetailPictureUris,
            ContactEmail = app.ContactEmail,
            ContactNumber = app.ContactNumber,
            Languages = app.Languages
        };
    }
}
