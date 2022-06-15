using System.Net;
using CatenaX.NetworkServices.Framework.ErrorHandling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CatenaX.NetworkServices.Provisioning.Service.BusinessLogic;
using CatenaX.NetworkServices.Provisioning.Service.Models;

namespace CatenaX.NetworkServices.Provisioning.Service.Controllers
{
    /// <summary>
    /// The controller provides the possibility to create an identity provider
    /// </summary>
    [ApiController]
    [Route("api/provisioning")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class IdentityProviderController : ControllerBase
    {

        private readonly ILogger<IdentityProviderController> _logger;
        private readonly IIdentityProviderBusinessLogic _logic;

        /// <summary>
        /// Creates a new instance of <see cref="IdentityProviderController"/>
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="logic">Access to the identity provider business logic</param>
        public IdentityProviderController(ILogger<IdentityProviderController> logger, IIdentityProviderBusinessLogic logic)
        {
            _logger = logger;
            _logic = logic;
        }

        /// <summary>
        /// Creates a new identity provider
        /// </summary>
        /// <param name="identityProviderSetupData">The data to create the identity provider</param>
        /// <returns>Returns the name of the created identity provider</returns>
        /// <remarks>Example: Get: /api/provisioning/identityprovider/setup</remarks>
        /// <response code="200">Returns the name of the new identity provider.</response>
        /// <response code="500">Internal server error occured, e.g. a database error.</response>
        [HttpPost]
        [Authorize(Roles="setup_idp")]
        [Route("identityprovider/setup")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateIdentityProvider([FromBody] IdentityProviderSetupData identityProviderSetupData)
        {
            try
            {
                var idpName = await _logic.CreateIdentityProvider(identityProviderSetupData).ConfigureAwait(false);
                if (idpName != null)
                {
                    return new OkObjectResult(idpName);
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
