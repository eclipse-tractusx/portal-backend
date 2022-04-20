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
        private readonly PortalDBContext context;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="context">Database context dependency.</param>
        public AppsBusinessLogic(PortalDBContext context)
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
                    Id = a.Id,
                    Name = a.Name,
                    VendorCompanyName = a.VendorCompany!.Name, // This translates into a 'left join' which does return null for all columns if the foreingn key is null. The '!' just makes the compiler happy
                    UseCaseNames = a.UseCases.Select(uc => uc.Name),
                    ThumbnailUrl = a.ThumbnailUrl,
                    ShortDescription = languageShortName == null
                        ? null
                        : a.AppDescriptions
                            .Where(description => description.LanguageShortName == languageShortName)
                            .Select(description => description.DescriptionShort)
                            .SingleOrDefault(),
                    LicenseText = a.AppLicenses
                        .Select(license => license.Licensetext)
                        .FirstOrDefault()
                }).AsAsyncEnumerable())
                {
                    yield return new AppViewModel {
                        Id = app.Id,
                        Title = app.Name ?? string.Empty,
                        Provider = app.VendorCompanyName ?? string.Empty,
                        UseCases = app.UseCaseNames.Select(name => name ?? string.Empty).ToList(),
                        LeadPictureUri = app.ThumbnailUrl ?? string.Empty,
                        ShortDescription = app.ShortDescription ?? string.Empty,
                        Price = app.LicenseText ?? string.Empty
                    };
                }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Guid>> GetAllFavouriteAppsForUserAsync(string userId)
        {
            return await this.context.IamUsers.AsNoTracking()
                .Include(u => u.CompanyUser!.Apps)
                .Where(u => u.UserEntityId == userId) // Id is unique, so single user
                .SelectMany(u => u.CompanyUser!.Apps.Select(a => a.Id))
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task RemoveFavouriteAppForUserAsync(Guid appId, string userId)
        {
            var companyUserId = await this.context.CompanyUsers.AsNoTracking().Where(cu => cu.IamUser!.UserEntityId == userId).Select(cu => cu.Id).SingleAsync();
            var rowToRemove = new CompanyUserAssignedAppFavourite
            {
                AppId = appId,
                CompanyUserId = companyUserId
            };
            this.context.CompanyUserAssignedAppFavourites.Attach(rowToRemove);
            this.context.CompanyUserAssignedAppFavourites.Remove(rowToRemove);
            await this.context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task AddFavouriteAppForUserAsync(Guid appId, string userId)
        {
            var companyUserId = await this.context.CompanyUsers.AsNoTracking().Where(cu => cu.IamUser!.UserEntityId == userId).Select(cu => cu.Id).SingleAsync();
            await this.context.CompanyUserAssignedAppFavourites.AddAsync(
                new CompanyUserAssignedAppFavourite
                {
                    AppId = appId,
                    CompanyUserId = companyUserId
                }
            );
            await this.context.SaveChangesAsync();
        }
    }
}
