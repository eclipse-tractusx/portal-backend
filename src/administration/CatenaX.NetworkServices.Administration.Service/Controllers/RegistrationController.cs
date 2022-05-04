using System;
using System.Net;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using CatenaX.NetworkServices.Administration.Service.BusinessLogic;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

namespace CatenaX.NetworkServices.Administration.Service.Controllers
{
    [ApiController]
    [Route("api/administration/registration")]
    public class RegistrationController : ControllerBase
    {

        private readonly ILogger<RegistrationController> _logger;
        private readonly IRegistrationBusinessLogic _logic;
        public RegistrationController(ILogger<RegistrationController> logger, IRegistrationBusinessLogic logic)
        {
            _logger = logger;
            _logic = logic;
        }
        [HttpGet]
        [Authorize(Roles = "view_submitted_applications")]
        [Route("application/{applicationId}/companyDetailsWithAddress")]
        [ProducesResponseType(typeof(CompanyWithAddress), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetCompanyWithAddressAsync([FromRoute] Guid applicationId)
        {
            try
            {
                return Ok(await _logic.GetCompanyWithAddressAsync(applicationId).ConfigureAwait(false));
            }
            catch(Exception e)
            {
                _logger.LogError(e.ToString());
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
