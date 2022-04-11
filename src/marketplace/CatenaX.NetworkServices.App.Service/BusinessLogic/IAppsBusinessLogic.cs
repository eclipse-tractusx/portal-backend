using CatenaX.NetworkServices.App.Service.ViewModels;

namespace CatenaX.NetworkServices.App.Service.BusinessLogic
{
    public interface IAppsBusinessLogic
    {
        public IAsyncEnumerable<AppViewModel> GetAllActiveAppsAsync(string? languageShortName = null);

        public Task<IEnumerable<Guid>> GetAllFavouriteAppsForUserAsync(Guid userId);

        public Task AddFavouriteAppForUserAsync(Guid appId, Guid userId);

        public Task RemoveFavouriteAppForUserAsync(Guid appId, Guid userId);
    }
}
