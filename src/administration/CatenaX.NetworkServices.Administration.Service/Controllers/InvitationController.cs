using CatenaX.NetworkServices.Administration.Service.BusinessLogic;
using CatenaX.NetworkServices.Administration.Service.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System.Threading.Tasks;

namespace CatenaX.NetworkServices.Administration.Service.Controllers
{
    [ApiController]
    [Route("api/administration/invitation")]
    public class InvitationController : ControllerBase
    {
        private readonly IInvitationBusinessLogic _logic;

        public InvitationController(ILogger<InvitationController> logger, IInvitationBusinessLogic logic)
        {
            _logic = logic;
        }

        [HttpPost]
        [Authorize(Roles = "invite_new_partner")]
        public Task ExecuteInvitation([FromBody] CompanyInvitationData InvitationData) =>
            _logic.ExecuteInvitation(InvitationData);
    }
}
