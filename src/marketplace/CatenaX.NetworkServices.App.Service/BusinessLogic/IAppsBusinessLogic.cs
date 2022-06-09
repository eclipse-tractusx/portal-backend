using CatenaX.NetworkServices.App.Service.InputModels;
using CatenaX.NetworkServices.App.Service.ViewModels;

namespace CatenaX.NetworkServices.App.Service.BusinessLogic;

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
    /// Get all apps that a user has been assigned roles in.
    /// </summary>
    /// <param name="userId">ID of the user to get available apps for.</param>
    /// <returns>List of available apps for user.</returns>
    public IAsyncEnumerable<BusinessAppViewModel> GetAllUserUserBusinessAppsAsync(string userId);

    /// <summary>
    /// Get detailed application data for a single app by id.
    /// </summary>
    /// <param name="appId">Persistence ID of the application to be retrieved.</param>
    /// <param name="userId">Optional ID of the user to evaluate app purchase status for. No company purchase status if not provided.</param>
    /// <param name="languageShortName">Optional two character language specifier for the localization of the app description. No description if not provided.</param>
    /// <returns>AppDetailsViewModel of the requested application.</returns>
    public Task<AppDetailsViewModel> GetAppDetailsByIdAsync(Guid appId, string? userId = null, string? languageShortName = null);

    /// <summary>
    /// Get IDs of all favourite apps of the user by ID.
    /// </summary>
    /// <param name="userId">ID of the user to get favourite apps for.</param>
    /// <returns>List of IDs of user's favourite apps.</returns>
    public IAsyncEnumerable<Guid> GetAllFavouriteAppsForUserAsync(string userId);

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

    /// <summary>
    /// Adds a subscription relation between an application and a user's company.
    /// </summary>
    /// <param name="appId">ID of the app to subscribe to.</param>
    /// <param name="userId">ID of the user that initiated app subscription for their company.</param>
    public Task AddCompanyAppSubscriptionAsync(Guid appId, string userId);

    /// <summary>
    /// Unsubscribes an app for the current users company.
    /// </summary>
    /// <param name="appId">ID of the app to unsubscribe from.</param>
    /// <param name="userId">ID of the user that initiated app unsubscription for their company.</param>
    public Task UnsubscribeCompanyAppSubscriptionAsync(Guid appId, string userId);

    /// <summary>
    /// Creates an application and returns its generated ID.
    /// </summary>
    /// <param name="appInputModel">Input model for app creation.</param>
    /// <returns>Guid of the created app.</returns>
    public Task<Guid> CreateAppAsync(AppInputModel appInputModel);
}
