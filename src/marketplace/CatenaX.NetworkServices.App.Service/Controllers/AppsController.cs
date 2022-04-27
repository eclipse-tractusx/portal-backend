using CatenaX.NetworkServices.App.Service.BusinessLogic;
using CatenaX.NetworkServices.App.Service.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace CatenaX.NetworkServices.App.Service.Controllers
{
    /// <summary>
    /// Controller providing actions for displaying, filtering and updating applications and user assigned favourites.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AppsController : ControllerBase
    {
        private readonly ILogger<AppsController> logger;
        private readonly IAppsBusinessLogic appsBusinessLogic;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="logger">Logger dependency.</param>
        /// <param name="appsBusinessLogic">Logic dependency.</param>
        public AppsController(ILogger<AppsController> logger, IAppsBusinessLogic appsBusinessLogic)
        {
            this.logger = logger;
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
        public IAsyncEnumerable<AppViewModel> GetAllActiveAppsAsync([FromQuery] string? lang = null)
        {
            return this.appsBusinessLogic.GetAllActiveAppsAsync(lang);
        }

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
        public async Task<ActionResult<AppDetailsViewModel>> GetAppDetailsByIdAsync([FromRoute] Guid appId, [FromQuery] string? lang = null)
        {
            var userId = GetIamUserIdFromClaims();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest("User information not provided in claims.");
            }

            return Ok(await this.appsBusinessLogic.GetAppDetailsByIdAsync(appId, userId, lang).ConfigureAwait(false));
        }

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
        public ActionResult<IAsyncEnumerable<Guid>> GetAllFavouriteAppsForCurrentUserAsync()
        {
            var userId = GetIamUserIdFromClaims();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest("User information not provided in claims.");
            }

            return Ok(this.appsBusinessLogic.GetAllFavouriteAppsForUserAsync(userId));
        }

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
        public async Task<IActionResult> AddFavouriteAppForCurrentUserAsync([FromRoute] Guid appId)
        {
            var userId = GetIamUserIdFromClaims();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest("User information not provided in claims.");
            }

            try
            {
                await this.appsBusinessLogic.AddFavouriteAppForUserAsync(appId, userId).ConfigureAwait(false);
            }
            catch (DbUpdateException)
            {
                return BadRequest($"Parameters are invalid or app is already favourited.");
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
            
            return Ok();
        }

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
        public async Task<IActionResult> RemoveFavouriteAppForCurrentUserAsync([FromRoute] Guid appId)
        {
            var userId = GetIamUserIdFromClaims();
            if(string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest("User information not provided in claims.");
            }

            try
            {
                await this.appsBusinessLogic.RemoveFavouriteAppForUserAsync(appId, userId).ConfigureAwait(false);
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest($"Parameters are invalid or favourite does not exist.");
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }

            return Ok();
        }

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
        public async Task<IActionResult> AddCompanyAppSubscriptionAsync([FromRoute] Guid appId)
        {
            var userId = GetIamUserIdFromClaims();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest("User information not provided in claims.");
            }

            try
            {
                await this.appsBusinessLogic.AddCompanyAppSubscriptionAsync(appId, userId).ConfigureAwait(false);
            }
            catch (DbUpdateException)
            {
                return BadRequest($"Parameters are invalid or app is already subscribed to.");
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }

            return Ok();
        }

        private string? GetIamUserIdFromClaims() => User.Claims.SingleOrDefault(c => c.Type == "sub")?.Value;
    }
}
