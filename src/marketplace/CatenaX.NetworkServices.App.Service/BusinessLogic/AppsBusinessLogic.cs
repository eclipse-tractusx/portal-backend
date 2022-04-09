using CatenaX.NetworkServices.App.Service.ViewModels;
using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using Microsoft.EntityFrameworkCore;

namespace CatenaX.NetworkServices.App.Service.BusinessLogic
{
    public class AppsBusinessLogic : IAppsBusinessLogic
    {
        private readonly PortalDBContext context;

        public AppsBusinessLogic(PortalDBContext context)
        {
            this.context = context;
        }

        public async IAsyncEnumerable<AppViewModel> GetAllActiveAppsAsync(string? languageShortName = null)
        {
            await foreach(var app in context.Apps.AsQueryable()
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
                        Title = app.Name ?? "",
                        Provider = app.VendorCompanyName ?? "",
                        UseCases = app.UseCaseNames.Select(name => name ?? "").ToList(),
                        LeadPictureUri = app.ThumbnailUrl ?? "",
                        ShortDescription = app.ShortDescription ?? "",
                        Price = app.LicenseText ?? ""
                    };
                }
        }

        public async Task<IEnumerable<Guid>> GetAllFavouriteAppsForUser(Guid userId)
        {
            return await this.context.IamUsers.AsNoTracking()
                .Include(u => u.CompanyUser!.Apps)
                .Where(u => u.Id == userId) // Id is unique, so single user
                .SelectMany(u => u.CompanyUser!.Apps.Select(a => a.Id))
                .ToListAsync();
        }
    }
}
