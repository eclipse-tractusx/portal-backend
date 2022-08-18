using CatenaX.NetworkServices.Administration.Service.BusinessLogic;
using CatenaX.NetworkServices.Administration.Service.Models;
using CatenaX.NetworkServices.Framework.ErrorHandling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatenaX.NetworkServices.Administration.Service.Controllers;

/// <summary>
/// Creates a new instance of <see cref="PartnerNetworkController"/>
/// </summary>
[ApiController]
[Route("api/administration/partnernetwork")]
[Produces("application/json")]
[Consumes("application/json")]
public class PartnerNetworkController : ControllerBase
{
    private readonly IPartnerNetworkBusinessLogic _logic;

    /// <summary>
    /// Creates a new instance of <see cref="PartnerNetworkController"/>
    /// </summary>
    /// <param name="logic">The partner network business logic</param>
    public PartnerNetworkController(IPartnerNetworkBusinessLogic logic)
    {
        _logic = logic;
    }

    /// <summary> Get all member companies</summary>
    /// <returns>Returns all the active member companies bpn.</returns>
    /// <remarks>Example: Get: api/administration/partnernetwork/memberCompanies</remarks>
    /// <response code="200">Returns all the active member companies bpn.</response>

    [HttpGet]
    [Authorize(Roles = "view_membership")]
    [Route("memberCompanies")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IAsyncEnumerable<string?> GetAllMemberCompaniesBPNAsync() =>
        _logic.GetAllMemberCompaniesBPNAsync();
}
