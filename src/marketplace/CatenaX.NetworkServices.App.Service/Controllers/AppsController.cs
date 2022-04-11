using CatenaX.NetworkServices.App.Service.BusinessLogic;
using CatenaX.NetworkServices.App.Service.ViewModels;
using Microsoft.AspNetCore.Mvc;
namespace CatenaX.NetworkServices.App.Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppsController : ControllerBase
    {
        private readonly ILogger<AppsController> logger;
        private readonly IAppsBusinessLogic appsBusinessLogic;

        public AppsController(ILogger<AppsController> logger, IAppsBusinessLogic appsBusinessLogic)
        {
            this.logger = logger;
            this.appsBusinessLogic = appsBusinessLogic;
        }

        [HttpGet]
        [Route("active")]
        public async Task<ActionResult<IEnumerable<AppViewModel>>> GetAllActiveApps([FromQuery] string? lang = null)
        {
            return Ok(await this.appsBusinessLogic.GetAllActiveAppsAsync(lang).ToListAsync().ConfigureAwait(false));
        }

        [HttpGet]
        [Route("favourites/for/user/{userId}")]
        public async Task<ActionResult<IEnumerable<Guid>>> GetAllFavouriteAppsForUser([FromRoute] Guid userId)
        {
            return Ok(await this.appsBusinessLogic.GetAllFavouriteAppsForUserAsync(userId));
        }

        [HttpPost]
        [Route("{appId}/favourite/for/user/{userId}")]
        public async Task<IActionResult> AddFavouriteAppForUser([FromRoute] Guid appId, [FromRoute] Guid userId)
        {
            try
            {
                await this.appsBusinessLogic.AddFavouriteAppForUserAsync(appId, userId);
            }
            catch (ArgumentOutOfRangeException)
            {
                return BadRequest($"There is no user with ID '{userId}'.");
            }
            catch (Exception)
            {
                throw;
            }
            
            return Accepted();
        }

        [HttpDelete]
        [Route("{appId}/favourite/for/user/{userId}")]
        public async Task<IActionResult> RemoveFavouriteAppForUser([FromRoute] Guid appId, [FromRoute] Guid userId)
        {
            try
            {
                await this.appsBusinessLogic.RemoveFavouriteAppForUserAsync(appId, userId);
            }
            catch (ArgumentOutOfRangeException)
            {
                return BadRequest($"There is no user with ID '{userId}'.");
            }
            catch (Exception)
            {
                throw;
            }

            return Ok();
        }
    }
}
