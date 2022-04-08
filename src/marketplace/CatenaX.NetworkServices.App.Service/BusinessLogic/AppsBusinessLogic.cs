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

        public async Task<IEnumerable<AppViewModel>> GetAllActiveAppsAsync(string? languageShortName = null)
        {
            var activeApps = await this.context.Apps
                .Include(a => a.AppDescriptions)
                .Include(a => a.VendorCompany)
                .Include(a => a.UseCases)
                .Include(a => a.AppLicenses)
                .Where(a => a.DateReleased.HasValue && a.DateReleased <= DateTime.UtcNow)
                .ToListAsync();

            var mappedApps = activeApps.Select(a => new AppViewModel
            {
                Id = a.Id,
                Title = a.Name ?? "",
                Provider = a.VendorCompany?.Name ?? "",
                UseCases = a.UseCases.Select(uc => uc.Name ?? "").ToList(),
                LeadPictureUri = a.ThumbnailUrl ?? "",
                ShortDescription = languageShortName is null ? 
                    "" : 
                    a.AppDescriptions.FirstOrDefault(ad => ad.AppId == a.Id && ad.LanguageShortName == languageShortName)?.DescriptionShort ?? "",
                Price = a.AppLicenses.FirstOrDefault()?.Licensetext ?? ""
            });
            return mappedApps;
        }
    }
}
