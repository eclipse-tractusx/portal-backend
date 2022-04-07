using System;
using System.Net;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using CatenaX.NetworkServices.Provisioning.Service.BusinessLogic;
using CatenaX.NetworkServices.Provisioning.Service.Models;

namespace CatenaX.NetworkServices.Provisioning.Service.Controllers
{
    [ApiController]
    [Route("api/provisioning")]
    public class ClientController : ControllerBase
    {

        private readonly ILogger<ClientController> _logger;
        private readonly IClientBusinessLogic _logic;

        public ClientController(ILogger<ClientController> logger, IClientBusinessLogic logic)
        {
            _logger = logger;
            _logic = logic;
        }

        [HttpPost]
        [Authorize(Roles="setup_client")]
        [Route("client/setup")]
        public async Task<IActionResult> CreateClient([FromBody] ClientSetupData clientSetupData)
        {
            try
            {
                var clientId = await _logic.CreateClient(clientSetupData).ConfigureAwait(false);
                if (clientId != null)
                {
                    return new OkObjectResult(clientId);
                }
                _logger.LogError("unsuccessful");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
