using CatenaX.NetworkServices.Administration.Service.BusinessLogic;
using CatenaX.NetworkServices.Administration.Service.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatenaX.NetworkServices.Administration.Service.Controllers;

[ApiController]
[Route("api/administration/invitation")]
public class InvitationController : ControllerBase
{
    private readonly IInvitationBusinessLogic _logic;

    public InvitationController(IInvitationBusinessLogic logic)
    {
        _logic = logic;
    }

    /// <summary>
    /// Executes the invitation
    /// </summary>
    /// <param name="InvitationData"></param>
    /// <returns></returns>
    /// <response code="401">User is unauthorized.</response>
    [HttpPost]
    [Authorize(Roles = "invite_new_partner")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public Task ExecuteInvitation([FromBody] CompanyInvitationData InvitationData) =>
        _logic.ExecuteInvitation(InvitationData);
}
