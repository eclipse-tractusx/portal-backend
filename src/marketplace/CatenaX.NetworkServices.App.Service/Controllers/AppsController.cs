using CatenaX.NetworkServices.App.Service.BusinessLogic;
using CatenaX.NetworkServices.App.Service.InputModels;
using CatenaX.NetworkServices.App.Service.ViewModels;
using CatenaX.NetworkServices.Framework.ErrorHandling;
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
    private readonly IAppsBusinessLogic _appsBusinessLogic;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="appsBusinessLogic">Logic dependency.</param>
    public AppsController(IAppsBusinessLogic appsBusinessLogic)
    {
        this._appsBusinessLogic = appsBusinessLogic;
    }

    /// <summary>
    /// Retrieves all active apps in the marketplace.
    /// </summary>
    /// <param name="lang" example="en">Optional two character language specifier for the app description. Will be empty if not provided.</param>
    /// <returns>Collection of all active marketplace apps.</returns>
    /// <remarks>Example: GET: /api/apps/active</remarks>
    /// <response code="200">Returns the list of all active marketplace apps.</response>
    /// <response code="401">User is unauthorized.</response>
    /// <response code="500">Internal server error occured, e.g. a database error.</response>
    [HttpGet]
    [Route("active")]
    [Authorize(Roles = "view_apps")]
    [ProducesResponseType(typeof(IAsyncEnumerable<AppViewModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public IAsyncEnumerable<AppViewModel> GetAllActiveAppsAsync([FromQuery] string? lang = null) =>
        this._appsBusinessLogic.GetAllActiveAppsAsync(lang);

    /// <summary>
    /// Get all apps that currently logged in user has been assigned roles in.
    /// </summary>
    /// <returns>Collection of BusinessAppViewModels user has been assigned active roles in.</returns>
    /// <remarks>Example: GET: /api/apps/business</remarks>
    /// <response code="200">Returns the list of the user's business apps.</response>
    /// <response code="400">If sub claim is empty/invalid.</response>
    /// <response code="401">User is unauthorized.</response>
    /// <response code="500">Internal server error occured, e.g. a database error.</response>
    [HttpGet]
    [Route("business")]
    [Authorize(Roles = "view_apps")]
    [ProducesResponseType(typeof(IAsyncEnumerable<BusinessAppViewModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType( StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public IAsyncEnumerable<BusinessAppViewModel> GetAllBusinessAppsForCurrentUserAsync() =>
        this.WithIamUserId(userId => _appsBusinessLogic.GetAllUserUserBusinessAppsAsync(userId));

    /// <summary>
    /// Retrieves app details for an app referenced by id.
    /// </summary>
    /// <param name="appId" example="D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645">ID of the app to retrieve.</param>
    /// <param name="lang" example="en">Optional two character language specifier for the app description. Will be empty if not provided.</param>
    /// <returns>AppDetailsViewModel for requested application.</returns>
    /// <remarks>Example: GET: /api/apps/D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645</remarks>
    /// <response code="200">Returns the requested app details.</response>
    /// <response code="400">If sub claim is empty/invalid.</response>
    /// <response code="401">User is unauthorized.</response>
    /// <response code="500">Internal server error occured, e.g. a database error.</response>
    [HttpGet]
    [Route("{appId}")]
    [Authorize(Roles = "view_apps")]
    [ProducesResponseType(typeof(AppDetailsViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType( StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public Task<AppDetailsViewModel> GetAppDetailsByIdAsync([FromRoute] Guid appId, [FromQuery] string? lang = null) =>
        this.WithIamUserId(userId => this._appsBusinessLogic.GetAppDetailsByIdAsync(appId, userId, lang));

    /// <summary>
    /// Creates an app according to input model.
    /// </summary>
    /// <param name="appInputModel">Input model for app creation.</param>
    /// <returns>ID of created application.</returns>
    /// <remarks>Example: POST: /api/apps</remarks>
    /// <response code="201">Returns created app's ID.</response>
    /// <response code="401">User is unauthorized.</response>
    /// <response code="500">Internal server error occured, e.g. a database error.</response>
    [HttpPost]
    [Route("")]
    [Authorize(Roles = "add_app")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType( StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Guid>> CreateAppAsync([FromBody] AppInputModel appInputModel) =>
        CreatedAtRoute(string.Empty, await this._appsBusinessLogic.CreateAppAsync(appInputModel));

    /// <summary>
    /// Retrieves IDs of all favourite apps of the current user (by sub claim).
    /// </summary>
    /// <returns>Collection of IDs of favourite apps.</returns>
    /// <remarks>Example: GET: /api/apps/favourites</remarks>
    /// <response code="200">Returns the list of favourite apps of current user.</response>
    /// <response code="400">If sub claim is empty/invalid.</response>
    /// <response code="401">User is unauthorized.</response>
    /// <response code="500">Internal server error occured, e.g. a database error.</response>
    [HttpGet]
    [Route("favourites")]
    [Authorize(Roles = "view_apps")]
    [ProducesResponseType(typeof(IAsyncEnumerable<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType( StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public IAsyncEnumerable<Guid> GetAllFavouriteAppsForCurrentUserAsync() =>
        this.WithIamUserId(userId => this._appsBusinessLogic.GetAllFavouriteAppsForUserAsync(userId));

    /// <summary>
    /// Adds an app to current user's favourites.
    /// </summary>
    /// <param name="appId" example="D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645">ID of the app to add to user favourites.</param>
    /// <remarks>Example: POST: /api/apps/D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645/favourite</remarks>
    /// <response code="204">Favourite app was successfully added to user.</response>
    /// <response code="400">If sub claim is empty/invalid or user does not exist.</response>
    /// <response code="401">User is unauthorized.</response>
    /// <response code="500">Internal server error occured, e.g. a database error.</response>
    [HttpPost]
    [Route("{appId}/favourite")]
    [Authorize(Roles = "view_apps")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType( StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddFavouriteAppForCurrentUserAsync([FromRoute] Guid appId)
    {
        await this.WithIamUserId(userId => this._appsBusinessLogic.AddFavouriteAppForUserAsync(appId, userId));
        return NoContent();
    }
        

    /// <summary>
    /// Removes an app from current user's favourites.
    /// </summary>
    /// <param name="appId" example="D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645">ID of the app to remove from user favourites.</param>
    /// <remarks>Example: DELETE: /api/apps/D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645/favourite</remarks>
    /// <response code="204">Favourite app was successfully removed from user.</response>
    /// <response code="400">If sub claim is empty/invalid or user does not exist.</response>
    /// <response code="401">User is unauthorized.</response>
    /// <response code="500">Internal server error occured, e.g. a database error.</response>
    [HttpDelete]
    [Route("{appId}/favourite")]
    [Authorize(Roles = "view_apps")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType( StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveFavouriteAppForCurrentUserAsync([FromRoute] Guid appId)
    {
        await this.WithIamUserId(userId => this._appsBusinessLogic.RemoveFavouriteAppForUserAsync(appId, userId));
        return NoContent();
    }
        

    /// <summary>
    /// Retrieves subscription statuses of subscribed apps of the currently logged in user's company.
    /// </summary>
    /// <remarks>Example: GET: /api/apps/subscribed/subscription-status</remarks>
    /// <response code="200">Returns list of applicable app subscription statuses.</response>
    /// <response code="400">If sub claim is empty/invalid or user does not exist.</response>
    /// <response code="401">User is unauthorized.</response>
    /// <response code="500">Internal server error occured, e.g. a database error.</response>
    [HttpGet]
    [Route("subscribed/subscription-status")]
    [Authorize(Roles = "view_subscription")]
    [ProducesResponseType(typeof(IAsyncEnumerable<AppSubscriptionStatusViewModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType( StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status502BadGateway)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public Task<IAsyncEnumerable<AppSubscriptionStatusViewModel>> GetCompanySubscribedAppSubscriptionStatusesForCurrentUserAsync() =>
        this.WithIamUserId(userId => this._appsBusinessLogic.GetCompanySubscribedAppSubscriptionStatusesForUserAsync(userId));

    /// <summary>
    /// Retrieves subscription statuses of provided apps of the currently logged in user's company.
    /// </summary>
    /// <remarks>Example: GET: /api/apps/provided/subscription-status</remarks>
    /// <response code="200">Returns list of applicable app subscription statuses.</response>
    /// <response code="400">If sub claim is empty/invalid or user does not exist.</response>
    /// <response code="401">User is unauthorized.</response>
    /// <response code="500">Internal server error occured, e.g. a database error.</response>
    [HttpGet]
    [Route("provided/subscription-status")]
    [Authorize(Roles = "view_app_subscription")]
    [ProducesResponseType(typeof(IAsyncEnumerable<AppCompanySubscriptionStatusViewModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType( StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status502BadGateway)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public Task<IAsyncEnumerable<AppCompanySubscriptionStatusViewModel>> GetCompanyProvidedAppSubscriptionStatusesForCurrentUserAsync() =>
        this.WithIamUserId(userId => this._appsBusinessLogic.GetCompanyProvidedAppSubscriptionStatusesForUserAsync(userId));

    /// <summary>
    /// Adds an app to current user's company's subscriptions.
    /// </summary>
    /// <param name="appId" example="D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645">ID of the app to subscribe to.</param>
    /// <remarks>Example: POST: /api/apps/D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645/subscribe</remarks>
    /// <response code="204">App was successfully subscribed to.</response>
    /// <response code="400">If sub claim is empty/invalid or user does not exist.</response>
    /// <response code="401">User is unauthorized.</response>
    /// <response code="500">Internal server error occured, e.g. a database error.</response>
    [HttpPost]
    [Route("{appId}/subscribe")]
    [Authorize(Roles = "subscribe_apps")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddCompanyAppSubscriptionAsync([FromRoute] Guid appId)
    {
        await this.WithIamUserId(userId => this._appsBusinessLogic.AddCompanyAppSubscriptionAsync(appId, userId));
        return NoContent();
    }

    /// <summary>
    /// Activates a pending app subscription for an app provided by the current user's company.
    /// </summary>
    /// <param name="appId" example="D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645">ID of the app to activate subscription for.</param>
    /// <param name="companyId" example="74BA5AEF-1CC7-495F-ABAA-CF87840FA6E2">ID of the company to activate subscription for.</param>
    /// <remarks>Example: PUT: /api/apps/D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645/supscription/company/74BA5AEF-1CC7-495F-ABAA-CF87840FA6E2/activate</remarks>
    /// <response code="204">App subscription was successfully activated.</response>
    /// <response code="400">If sub claim is empty/invalid or user does not exist, or any other parameters are invalid.</response>
    /// <response code="401">User is unauthorized.</response>
    /// <response code="500">Internal server error occured, e.g. a database error.</response>
    [HttpPut]
    [Route("{appId}/subscription/company/{companyId}/activate")]
    [Authorize(Roles = "activate_subscription")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ActivateCompanyAppSubscriptionAsync([FromRoute] Guid appId, [FromRoute] Guid companyId) 
    {
        await this.WithIamUserId(userId => this._appsBusinessLogic.ActivateCompanyAppSubscriptionAsync(appId, companyId, userId));
        return NoContent();
    }
}
