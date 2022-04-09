using CatenaX.NetworkServices.App.Service.BusinessLogic;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
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
        public async Task<IActionResult> GetAllActiveApps([FromQuery] string? lang = null)
        {
            return Ok(await this.appsBusinessLogic.GetAllActiveAppsAsync(lang).ToListAsync().ConfigureAwait(false));
        }
    }
}
