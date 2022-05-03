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
    [Route("api/administration/invitation")]
    public class InvitationController : ControllerBase
    {

        private readonly ILogger<InvitationController> _logger;
        private readonly IInvitationBusinessLogic _logic;

        public InvitationController(ILogger<InvitationController> logger, IInvitationBusinessLogic logic)
        {
            _logger = logger;
            _logic = logic;
        }

        [HttpPost]
        [Authorize(Roles = "invite_new_partner")]
        public async Task<IActionResult> ExecuteInvitation([FromBody] CompanyInvitationData InvitationData)
        {
            try
            {
                await _logic.ExecuteInvitation(InvitationData).ConfigureAwait(false);
                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
