using CatenaX.NetworkServices.App.Service.ViewModels;
using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatenaX.NetworkServices.App.Service.BusinessLogic
{
    /// <summary>
    /// Implementation of <see cref="IAppsBusinessLogic"/>.
    /// </summary>
    public class AppsBusinessLogic : IAppsBusinessLogic
    {
        private const string ERROR_STRING = "ERROR";
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
            await foreach(var app in context.Apps.AsQueryable()
                .AsNoTracking()
                .Where(app => app.DateReleased.HasValue && app.DateReleased <= DateTime.UtcNow)
                .Select(a => new {
                    a.Id,
                    Name = (string?)a.Name,
                    VendorCompanyName = a.ProviderCompany.Name, // This translates into a 'left join' which does return null for all columns if the foreingn key is null. The '!' just makes the compiler happy
                    UseCaseNames = a.UseCases.Select(uc => uc.Name),
                    ThumbnailUrl = (string?)a.ThumbnailUrl,
                    ShortDescription = languageShortName == null
                        ? null
                        : (a.AppDescriptions
                            .Where(description => description.LanguageShortName == languageShortName)
                            .Select(description => description.DescriptionShort)
                            .SingleOrDefault() ?? ERROR_STRING),
                    LicenseText = a.AppLicenses
                        .Select(license => license.Licensetext)
                        .FirstOrDefault()
                }).AsAsyncEnumerable())
                {
                    yield return new AppViewModel(
                        app.Name ?? ERROR_STRING,
                        app.ShortDescription ?? string.Empty,
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
                    LongDescription = languageShortName == null
                        ? null
                        : (a.AppDescriptions
                            .Where(description => description.LanguageShortName == languageShortName)
                            .Select(description => description.DescriptionLong)
                            .SingleOrDefault() ?? ERROR_STRING),
                    Price = a.AppLicenses
                        .Select(license => license.Licensetext)
                        .FirstOrDefault(),
                    Tags = a.Tags.Select(t => t.Name),
                    IsPurchased = companyId == null ?
                        (bool?)null :
                        a.Companies.Any(c => c.Id == companyId)
                })
                .SingleAsync().ConfigureAwait(false);

            return new AppDetailsViewModel(
                app.Title ?? ERROR_STRING,
                app.LeadPictureUri ?? ERROR_STRING,
                app.ProviderUri ?? ERROR_STRING,
                app.Provider,
                app.ContactEmail ?? ERROR_STRING,
                app.ContactNumber ?? ERROR_STRING,
                app.LongDescription ?? string.Empty,
                app.Price ?? ERROR_STRING
                )
            {
                Id = app.Id,
                IsPurchased = app.IsPurchased,
                Tags = app.Tags,
                UseCases = app.UseCases,
                DetailPictureUris = app.DetailPictureUris
            };
        }

        /// <inheritdoc/>
        public IAsyncEnumerable<Guid> GetAllFavouriteAppsForUserAsync(string userId)
        {
            return this.context.IamUsers.AsNoTracking()
                .Include(u => u.CompanyUser!.Apps)
                .Where(u => u.UserEntityId == userId) // Id is unique, so single user
                .SelectMany(u => u.CompanyUser!.Apps.Select(a => a.Id))
                .ToAsyncEnumerable();
        }

        /// <inheritdoc/>
        public async Task RemoveFavouriteAppForUserAsync(Guid appId, string userId)
        {
            var companyUserId = await GetCompanyUserIdbyIamUserIdAsync(userId).ConfigureAwait(false);
            var rowToRemove = new CompanyUserAssignedAppFavourite(appId, companyUserId);
            this.context.CompanyUserAssignedAppFavourites.Attach(rowToRemove);
            this.context.CompanyUserAssignedAppFavourites.Remove(rowToRemove);
            await this.context.SaveChangesAsync().ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task AddFavouriteAppForUserAsync(Guid appId, string userId)
        {
            var companyUserId = await GetCompanyUserIdbyIamUserIdAsync(userId).ConfigureAwait(false);
            await this.context.CompanyUserAssignedAppFavourites.AddAsync(
                new CompanyUserAssignedAppFavourite(appId, companyUserId)
            ).ConfigureAwait(false);
            await this.context.SaveChangesAsync().ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task AddCompanyAppSubscriptionAsync(Guid appId, string userId)
        {
            var companyId = await GetCompanyIdByIamUserIdAsync(userId).ConfigureAwait(false);
            await this.context.CompanyAssignedApps.AddAsync(
                new CompanyAssignedApp(appId, companyId)
            ).ConfigureAwait(false);
            await this.context.SaveChangesAsync().ConfigureAwait(false);
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
}
