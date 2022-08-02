using CatenaX.NetworkServices.Administration.Service.BusinessLogic;
using CatenaX.NetworkServices.Administration.Service.Models;
using CatenaX.NetworkServices.Framework.ErrorHandling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatenaX.NetworkServices.Administration.Service.Controllers;

/// <summary>
/// Controller providing actions execute invitation
/// </summary>
[ApiController]
[Route("api/administration/invitation")]
[Produces("application/json")]
[Consumes("application/json")]
public class InvitationController : ControllerBase
{
    private readonly IInvitationBusinessLogic _logic;

    /// <summary>
    /// Creates a new instance of <see cref="InvitationController"/>
    /// </summary>
    /// <param name="logic">The invitation business logic</param>
    public InvitationController(IInvitationBusinessLogic logic)
    {
        _logic = logic;
    }

    /// <summary>
    /// Executes the invitation
    /// </summary>
    /// <param name="invitationData"></param>
    /// <returns></returns>
    /// <remarks>
    /// Example: POST: api/administration/invitation
    /// </remarks>
    /// <response code="200">Successfully executed the invitation.</response>
    /// <response code="400">Missing mandatory input values (e.g. email, organization name, etc.)</response>
    /// <response code="500">Internal Server Error.</response>
    /// <response code="502">Bad Gateway Service Error.</response>
    [HttpPost]
    [Authorize(Roles = "invite_new_partner")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status502BadGateway)]
    public Task ExecuteInvitation([FromBody] CompanyInvitationData invitationData) =>
        _logic.ExecuteInvitation(invitationData);
}
