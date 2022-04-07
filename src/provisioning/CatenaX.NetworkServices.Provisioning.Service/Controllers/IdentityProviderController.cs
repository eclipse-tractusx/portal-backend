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
    public class IdentityProviderController : ControllerBase
    {

        private readonly ILogger<IdentityProviderController> _logger;
        private readonly IIdentityProviderBusinessLogic _logic;

        public IdentityProviderController(ILogger<IdentityProviderController> logger, IIdentityProviderBusinessLogic logic)
        {
            _logger = logger;
            _logic = logic;
        }

        [HttpPost]
        [Authorize(Roles="setup_idp")]
        [Route("identityprovider/setup")]
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
