using System.Net;
using CatenaX.NetworkServices.Framework.ErrorHandling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CatenaX.NetworkServices.Provisioning.Service.BusinessLogic;
using CatenaX.NetworkServices.Provisioning.Service.Models;

namespace CatenaX.NetworkServices.Provisioning.Service.Controllers
{
    /// <summary>
    /// The controller provides the possibility to create a client
    /// </summary>
    [ApiController]
    [Route("api/provisioning")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class ClientController : ControllerBase
    {

        private readonly ILogger<ClientController> _logger;
        private readonly IClientBusinessLogic _logic;

        /// <summary>
        /// Creates a instance of <see cref="ClientController"/>
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="logic">Client business logic</param>
        public ClientController(ILogger<ClientController> logger, IClientBusinessLogic logic)
        {
            _logger = logger;
            _logic = logic;
        }

        /// <summary>
        /// Creates a client with the given data
        /// </summary>
        /// <param name="clientSetupData">the setup data for the new client</param>
        /// <returns>Returns the client id of the created client</returns>
        /// <remarks>Example: Get: /api/provisioning/client/setup</remarks>
        /// <response code="200">Successfully created the client.</response>
        /// <response code="500">Internal server error occured, e.g. a database error.</response>
        [HttpPost]
        [Authorize(Roles="setup_client")]
        [Route("client/setup")]
        [ProducesResponseType(typeof(IAsyncEnumerable<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
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
