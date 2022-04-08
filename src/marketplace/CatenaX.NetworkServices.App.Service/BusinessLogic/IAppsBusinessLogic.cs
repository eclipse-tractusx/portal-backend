using CatenaX.NetworkServices.App.Service.ViewModels;

namespace CatenaX.NetworkServices.App.Service.BusinessLogic
{
    public interface IAppsBusinessLogic
    {
        public Task<IEnumerable<AppViewModel>> GetAllActiveAppsAsync(string? languageShortName = null);

        public Task<IEnumerable<Guid>> GetAllFavouriteAppsForUser(Guid userId);
    }
}
