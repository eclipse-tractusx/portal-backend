using CatenaX.NetworkServices.App.Service.InputModels;
using CatenaX.NetworkServices.App.Service.ViewModels;
using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatenaX.NetworkServices.App.Service.BusinessLogic;

/// <summary>
/// Implementation of <see cref="IAppsBusinessLogic"/>.
/// </summary>
public class AppsBusinessLogic : IAppsBusinessLogic
{
    private const string ERROR_STRING = "ERROR";
    private const string DEFAULT_LANGUAGE = "en";
    private readonly PortalDbContext context;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="context">Database context dependency.</param>
    public AppsBusinessLogic(PortalDbContext context)
    {
        this.context = context;
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<AppViewModel> GetAllActiveAppsAsync(string? languageShortName = null)
    {
        await foreach(var app in context.Apps.AsNoTracking()
            .Where(app => app.DateReleased.HasValue && app.DateReleased <= DateTime.UtcNow)
            .Select(a => new {
                a.Id,
                Name = (string?)a.Name,
                VendorCompanyName = a.ProviderCompany.Name, // This translates into a 'left join' which does return null for all columns if the foreingn key is null. The '!' just makes the compiler happy
                UseCaseNames = a.UseCases.Select(uc => uc.Name),
                ThumbnailUrl = (string?)a.ThumbnailUrl,
                ShortDescription =
                    this.context.Languages.SingleOrDefault(l => l.ShortName == languageShortName) == null 
                    ? null 
                    : a.AppDescriptions.SingleOrDefault(d => d.LanguageShortName == languageShortName)!.DescriptionShort
                      ?? a.AppDescriptions.SingleOrDefault(d => d.LanguageShortName == DEFAULT_LANGUAGE)!.DescriptionShort,
                LicenseText = a.AppLicenses
                    .Select(license => license.Licensetext)
                    .FirstOrDefault()
            }).AsAsyncEnumerable())
            {
                yield return new AppViewModel(
                    app.Name ?? ERROR_STRING,
                    app.ShortDescription ?? ERROR_STRING,
                    app.VendorCompanyName ?? ERROR_STRING,
                    app.LicenseText ?? ERROR_STRING,
                    app.ThumbnailUrl ?? ERROR_STRING) 
                {
                    Id = app.Id,
                    UseCases = app.UseCaseNames.Select(name => name).ToList()
                };
            }
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<BusinessAppViewModel> GetAllUserUserBusinessAppsAsync(string userId)
    {
        await foreach (var app in context.IamUsers.AsNoTracking().Where(u => u.UserEntityId == userId)
            .SelectMany(u => u.CompanyUser!.Company!.BoughtApps)
            .Intersect(
                context.IamUsers.AsNoTracking().Where(u => u.UserEntityId == userId)
                .SelectMany(u => u.CompanyUser!.UserRoles.SelectMany(r => r.IamClient!.Apps))
            )
            .Select( a => new
            {
                a.Id,
                a.Name,
                a.AppUrl,
                a.ThumbnailUrl,
                a.Provider
            }).AsAsyncEnumerable())
        {
            yield return new BusinessAppViewModel(
                app.Name ?? ERROR_STRING, 
                app.AppUrl ?? ERROR_STRING, 
                app.ThumbnailUrl ?? ERROR_STRING, 
                app.Provider
            )
            {
                Id = app.Id
            };
        }
    }

    /// <inheritdoc/>
    public async Task<AppDetailsViewModel> GetAppDetailsByIdAsync(Guid appId, string? userId = null, string? languageShortName = null)
    {
        var companyId = userId == null ?
            (Guid?)null :
            await GetCompanyIdByIamUserIdAsync(userId).ConfigureAwait(false);

        var app = await this.context.Apps.AsNoTracking()
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
                    this.context.Languages.SingleOrDefault(l => l.ShortName == languageShortName) == null 
                    ? null 
                    : a.AppDescriptions.SingleOrDefault(d => d.LanguageShortName == languageShortName)!.DescriptionLong
                      ?? a.AppDescriptions.SingleOrDefault(d => d.LanguageShortName == DEFAULT_LANGUAGE)!.DescriptionLong,
                Price = a.AppLicenses
                    .Select(license => license.Licensetext)
                    .FirstOrDefault(),
                Tags = a.Tags.Select(t => t.Name),
                IsPurchased = companyId == null ?
                    (bool?)null :
                    a.Companies.Any(c => c.Id == companyId),
                Languages = a.SupportedLanguages.Select(l => l.ShortName)
            })
            .SingleAsync().ConfigureAwait(false);

        return new AppDetailsViewModel(
            app.Title ?? ERROR_STRING,
            app.LeadPictureUri ?? ERROR_STRING,
            app.ProviderUri ?? ERROR_STRING,
            app.Provider,
            app.LongDescription ?? ERROR_STRING,
            app.Price ?? ERROR_STRING
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

    /// <inheritdoc/>
    public IAsyncEnumerable<Guid> GetAllFavouriteAppsForUserAsync(string userId)
    {
        return this.context.IamUsers.AsNoTracking()
            .Where(u => u.UserEntityId == userId) // Id is unique, so single user
            .SelectMany(u => u.CompanyUser!.Apps.Select(a => a.Id))
            .ToAsyncEnumerable();
    }

    /// <inheritdoc/>
    public async Task RemoveFavouriteAppForUserAsync(Guid appId, string userId)
    {
        try
        {
            var companyUserId = await GetCompanyUserIdbyIamUserIdAsync(userId).ConfigureAwait(false);
            var rowToRemove = new CompanyUserAssignedAppFavourite(appId, companyUserId);
            this.context.CompanyUserAssignedAppFavourites.Attach(rowToRemove);
            this.context.CompanyUserAssignedAppFavourites.Remove(rowToRemove);
            await this.context.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ArgumentException($"Parameters are invalid or favourite does not exist.");
        }
    }

    /// <inheritdoc/>
    public async Task AddFavouriteAppForUserAsync(Guid appId, string userId)
    {
        try
        {
            var companyUserId = await GetCompanyUserIdbyIamUserIdAsync(userId).ConfigureAwait(false);
            this.context.CompanyUserAssignedAppFavourites.Add(
                new CompanyUserAssignedAppFavourite(appId, companyUserId)
            );
            await this.context.SaveChangesAsync().ConfigureAwait(false);
        }
            catch (DbUpdateException)
        {
            throw new ArgumentException($"Parameters are invalid or app is already favourited.");
        }

    }

    /// <inheritdoc/>
    public async Task AddCompanyAppSubscriptionAsync(Guid appId, string userId)
    {
        try
        {
            var companyId = await GetCompanyIdByIamUserIdAsync(userId).ConfigureAwait(false);
            this.context.CompanyAssignedApps.Add(new CompanyAssignedApp(appId, companyId, PortalBackend.PortalEntities.Enums.AppSubscriptionStatusId.PENDING));
            await this.context.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (DbUpdateException)
        {
            throw new ArgumentException($"Parameters are invalid or app is already subscribed to.");
        }
    }

    /// <inheritdoc/>
    public async Task<Guid> CreateAppAsync(AppInputModel appInputModel)
    {
        // Add app to db
        var appEntity = new PortalBackend.PortalEntities.Entities.App(Guid.NewGuid(), appInputModel.Provider, DateTimeOffset.UtcNow)
        {
            Name = appInputModel.Title,
            MarketingUrl = appInputModel.ProviderUri,
            AppUrl = appInputModel.AppUri,
            ThumbnailUrl = appInputModel.LeadPictureUri,
            ContactEmail = appInputModel.ContactEmail,
            ContactNumber = appInputModel.ContactNumber,
            ProviderCompanyId = appInputModel.ProviderCompanyId,
            AppStatusId = PortalBackend.PortalEntities.Enums.AppStatusId.CREATED
        };
        this.context.Apps.Add(appEntity);

        var appLicenseEntity = new AppLicense(Guid.NewGuid(), appInputModel.Price);
        this.context.AppLicenses.Add(appLicenseEntity);           

        this.context.AppAssignedLicenses.Add(new AppAssignedLicense(appEntity.Id, appLicenseEntity.Id));
        this.context.AppAssignedUseCases.AddRange(appInputModel.UseCaseIds.Select(uc => new AppAssignedUseCase(appEntity.Id, uc)));
        this.context.AppDescriptions.AddRange(appInputModel.Descriptions.Select(d => new AppDescription(appEntity.Id, d.LanguageCode, d.LongDescription, d.ShortDescription)));
        this.context.AppLanguages.AddRange(appInputModel.SupportedLanguageCodes.Select(c => new AppLanguage(appEntity.Id, c)));
        await this.context.SaveChangesAsync().ConfigureAwait(false);

        return appEntity.Id;
    }

    private Task<Guid> GetCompanyUserIdbyIamUserIdAsync(string userId) => 
        this.context.CompanyUsers.AsNoTracking()
            .Where(cu => cu.IamUser!.UserEntityId == userId)
            .Select(cu => cu.Id)
            .SingleAsync();

    private Task<Guid> GetCompanyIdByIamUserIdAsync(string userId) => 
        this.context.CompanyUsers.AsNoTracking()
            .Where(cu => cu.IamUser!.UserEntityId == userId)
            .Select(cu => cu.CompanyId)
            .SingleAsync();
}
