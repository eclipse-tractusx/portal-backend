using System;
using System.Net;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using CatenaX.NetworkServices.Provisioning.Library;
using CatenaX.NetworkServices.Provisioning.Library.Models;
using CatenaX.NetworkServices.Administration.Service.BusinessLogic;
using CatenaX.NetworkServices.Administration.Service.Models;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;

namespace CatenaX.NetworkServices.Administration.Service.Controllers
{
    [ApiController]
    [Route("api/administration/registration")]
    public class RegistrationAdministrationController : ControllerBase
    {

        private readonly ILogger<RegistrationAdministrationController> _logger;
        private readonly IRegistrationAdministrationBusinessLogic _logic;
        private readonly IRegistrationAdministrationBusinessLogic _registrationAdministrationBusinessLogic;
        public RegistrationAdministrationController(ILogger<RegistrationAdministrationController> logger, IRegistrationAdministrationBusinessLogic logic, IRegistrationAdministrationBusinessLogic registrationAdministrationBusinessLogic)
        {
            _logger = logger;
            _logic = logic;
            _registrationAdministrationBusinessLogic = registrationAdministrationBusinessLogic;
        }
        [HttpGet]
        [Authorize(Roles = "view_submitted_applications")]
        [Route("application/{applicationId}/companyDetailsWithAddress")]
        [ProducesResponseType(typeof(CompanyWithAddress), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetCompanyWithAddressAsync([FromRoute] Guid applicationId)
        {
            try
            {
                return Ok(await _registrationAdministrationBusinessLogic.GetCompanyWithAddressAsync(applicationId).ConfigureAwait(false));
            }
            catch(Exception e)
            {
                _logger.LogError(e.ToString());
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
