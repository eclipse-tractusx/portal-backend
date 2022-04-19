using CatenaX.NetworkServices.App.Service.ViewModels;

namespace CatenaX.NetworkServices.App.Service.BusinessLogic
{
    /// <summary>
    /// Business logic for handling app-related operations. Includes persistence layer access.
    /// </summary>
    public interface IAppsBusinessLogic
    {
        /// <summary>
        /// Get all active marketplace apps.
        /// </summary>
        /// <param name="languageShortName">Optional two character language specifier for the app description. No description if not provided.</param>
        /// <returns>List of active marketplace apps.</returns>
        public IAsyncEnumerable<AppViewModel> GetAllActiveAppsAsync(string? languageShortName = null);

        /// <summary>
        /// Get IDs of all favourite apps of the user by ID.
        /// </summary>
        /// <param name="userId">ID of the user to get favourite apps for.</param>
        /// <returns>List of IDs of user's favourite apps.</returns>
        public Task<IEnumerable<Guid>> GetAllFavouriteAppsForUserAsync(string userId);

        /// <summary>
        /// Adds an app to a user's favourites.
        /// </summary>
        /// <param name="appId">ID of the app to add to user's favourites.</param>
        /// <param name="userId">ID of the user to add app favourite to.</param>
        public Task AddFavouriteAppForUserAsync(Guid appId, string userId);

        /// <summary>
        /// Removes an app from a user's favourites.
        /// </summary>
        /// <param name="appId">ID of the app to remove from user's favourites.</param>
        /// <param name="userId">ID of the user to remove app favourite from.</param>
        public Task RemoveFavouriteAppForUserAsync(Guid appId, string userId);
    }
}
