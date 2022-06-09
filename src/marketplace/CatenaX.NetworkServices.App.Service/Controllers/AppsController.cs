using CatenaX.NetworkServices.App.Service.BusinessLogic;
using CatenaX.NetworkServices.App.Service.InputModels;
using CatenaX.NetworkServices.App.Service.ViewModels;
using CatenaX.NetworkServices.Keycloak.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatenaX.NetworkServices.App.Service.Controllers;

/// <summary>
/// Controller providing actions for displaying, filtering and updating applications and user assigned favourites.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class AppsController : ControllerBase
{
    private readonly IAppsBusinessLogic appsBusinessLogic;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="appsBusinessLogic">Logic dependency.</param>
    public AppsController(IAppsBusinessLogic appsBusinessLogic)
    {
        this.appsBusinessLogic = appsBusinessLogic;
    }

    /// <summary>
    /// Retrieves all active apps in the marketplace.
    /// </summary>
    /// <param name="lang" example="en">Optional two character language specifier for the app description. Will be empty if not provided.</param>
    /// <returns>Collection of all active marketplace apps.</returns>
    /// <remarks>Example: GET: /api/apps/active</remarks>
    /// <response code="200">Returns the list of all active marketplace apps.</response>
    [HttpGet]
    [Route("active")]
    [Authorize(Roles = "view_apps")]
    [ProducesResponseType(typeof(IAsyncEnumerable<AppViewModel>), StatusCodes.Status200OK)]
    public IAsyncEnumerable<AppViewModel> GetAllActiveAppsAsync([FromQuery] string? lang = null) =>
        this.appsBusinessLogic.GetAllActiveAppsAsync(lang);

    /// <summary>
    /// Get all apps that currently logged in user has been assigned roles in.
    /// </summary>
    /// <returns>Collection of BusinessAppViewModels user has been assigned active roles in.</returns>
    /// <remarks>Example: GET: /api/apps/business</remarks>
    /// <response code="200">Returns the list of the user's business apps.</response>
    /// <response code="400">If sub claim is empty/invalid.</response>
    [HttpGet]
    [Route("business")]
    [Authorize(Roles = "view_apps")]
    [ProducesResponseType(typeof(IAsyncEnumerable<BusinessAppViewModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public IAsyncEnumerable<BusinessAppViewModel> GetAllBusinessAppsForCurrentUserAsync() =>
        this.WithIamUserId(userId => appsBusinessLogic.GetAllUserUserBusinessAppsAsync(userId));

    /// <summary>
    /// Retrieves app details for an app referenced by id.
    /// </summary>
    /// <param name="appId" example="D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645">ID of the app to retrieve.</param>
    /// <param name="lang" example="en">Optional two character language specifier for the app description. Will be empty if not provided.</param>
    /// <returns>AppDetailsViewModel for requested application.</returns>
    /// <remarks>Example: GET: /api/apps/D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645</remarks>
    /// <response code="200">Returns the requested app details.</response>
    /// <response code="400">If sub claim is empty/invalid.</response>
    [HttpGet]
    [Route("{appId}")]
    [Authorize(Roles = "view_apps")]
    [ProducesResponseType(typeof(AppDetailsViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public Task<AppDetailsViewModel> GetAppDetailsByIdAsync([FromRoute] Guid appId, [FromQuery] string? lang = null) =>
        this.WithIamUserId(userId => this.appsBusinessLogic.GetAppDetailsByIdAsync(appId, userId, lang));

    /// <summary>
    /// Creates an app according to input model.
    /// </summary>
    /// <param name="appInputModel">Input model for app creation.</param>
    /// <returns>ID of created application.</returns>
    /// <remarks>Example: POST: /api/apps</remarks>
    /// <response code="201">Returns created app's ID.</response>
    [HttpPost]
    [Route("")]
    [Authorize(Roles = "add_app")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<ActionResult<Guid>> CreateAppAsync([FromBody] AppInputModel appInputModel) =>
        CreatedAtRoute(string.Empty, await this.appsBusinessLogic.CreateAppAsync(appInputModel));

    /// <summary>
    /// Retrieves IDs of all favourite apps of the current user (by sub claim).
    /// </summary>
    /// <returns>Collection of IDs of favourite apps.</returns>
    /// <remarks>Example: GET: /api/apps/favourites</remarks>
    /// <response code="200">Returns the list of favourite apps of current user.</response>
    /// <response code="400">If sub claim is empty/invalid.</response>
    [HttpGet]
    [Route("favourites")]
    [Authorize(Roles = "view_apps")]
    [ProducesResponseType(typeof(IAsyncEnumerable<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public IAsyncEnumerable<Guid> GetAllFavouriteAppsForCurrentUserAsync() =>
        this.WithIamUserId(userId => this.appsBusinessLogic.GetAllFavouriteAppsForUserAsync(userId));

    /// <summary>
    /// Adds an app to current user's favourites.
    /// </summary>
    /// <param name="appId" example="D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645">ID of the app to add to user favourites.</param>
    /// <remarks>Example: POST: /api/apps/D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645/favourite</remarks>
    /// <response code="200">Favourite app was successfully added to user.</response>
    /// <response code="400">If sub claim is empty/invalid or user does not exist.</response>
    [HttpPost]
    [Route("{appId}/favourite")]
    [Authorize(Roles = "view_apps")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public Task AddFavouriteAppForCurrentUserAsync([FromRoute] Guid appId) =>
        this.WithIamUserId(userId => this.appsBusinessLogic.AddFavouriteAppForUserAsync(appId, userId));

    /// <summary>
    /// Removes an app from current user's favourites.
    /// </summary>
    /// <param name="appId" example="D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645">ID of the app to remove from user favourites.</param>
    /// <remarks>Example: DELETE: /api/apps/D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645/favourite</remarks>
    /// <response code="200">Favourite app was successfully removed from user.</response>
    /// <response code="400">If sub claim is empty/invalid or user does not exist.</response>
    [HttpDelete]
    [Route("{appId}/favourite")]
    [Authorize(Roles = "view_apps")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public Task RemoveFavouriteAppForCurrentUserAsync([FromRoute] Guid appId) =>
        this.WithIamUserId(userId => this.appsBusinessLogic.RemoveFavouriteAppForUserAsync(appId, userId));

    /// <summary>
    /// Adds an app to current user's company's subscriptions.
    /// </summary>
    /// <param name="appId" example="D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645">ID of the app to subscribe to.</param>
    /// <remarks>Example: POST: /api/apps/D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645/subscribe</remarks>
    /// <response code="200">Favourite app was successfully subscribed to.</response>
    /// <response code="400">If sub claim is empty/invalid or user does not exist.</response>
    [HttpPost]
    [Route("{appId}/subscribe")]
    [Authorize(Roles = "view_apps")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public Task AddCompanyAppSubscriptionAsync([FromRoute] Guid appId) =>
        this.WithIamUserId(userId => this.appsBusinessLogic.AddCompanyAppSubscriptionAsync(appId, userId));


    /// <summary>
    /// Unsubscribes an app from the current user's company's subscriptions.
    /// </summary>
    /// <param name="appId" example="D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645">ID of the app to unsubscribe from.</param>
    /// <remarks>Example: PUT: /api/apps/D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645/unsubscribe</remarks>
    /// <response code="204">The app was successfully unsubscribed from.</response>
    /// <response code="400">If either the app or app subscription doesn't exist.</response>
    /// <response code="401">If the user is unauthorized.</response>
    /// <response code="500">If the database operation failed.</response>
    [HttpPut]
    [Route("{appId}/unsubscribe")]
    [Authorize(Roles = "unsubscribe_apps")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UnsubscribeCompanyAppSubscriptionAsync([FromRoute] Guid appId)
    {
        await this.WithIamUserId(userId => this.appsBusinessLogic.UnsubscribeCompanyAppSubscriptionAsync(appId, userId));
        return this.NoContent();
    }
}
