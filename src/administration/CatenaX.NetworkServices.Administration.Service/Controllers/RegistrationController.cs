using CatenaX.NetworkServices.Administration.Service.BusinessLogic;
using CatenaX.NetworkServices.Framework.Models;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CatenaX.NetworkServices.Administration.Service.Controllers;

[ApiController]
[Route("api/administration/registration")]
public class RegistrationController : ControllerBase
{
    private readonly IRegistrationBusinessLogic _logic;
    public RegistrationController(IRegistrationBusinessLogic logic)
    {
        _logic = logic;
    }

    /// <summary>
    /// Gets the company with its address
    /// </summary>
    /// <param name="applicationId"></param>
    /// <returns>the company with its address</returns>
    /// <response code="200">Returns the company with its address.</response>
    /// <response code="401">User is unauthorized.</response>
    [HttpGet]
    [Authorize(Roles = "view_submitted_applications")]
    [Route("application/{applicationId}/companyDetailsWithAddress")]
    [ProducesResponseType(typeof(CompanyWithAddress), (int)HttpStatusCode.OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public Task<CompanyWithAddress> GetCompanyWithAddressAsync([FromRoute] Guid applicationId) =>
        _logic.GetCompanyWithAddressAsync(applicationId);

    /// <summary>
    /// Gets the application details with pagination
    /// </summary>
    /// <param name="page"></param>
    /// <param name="size"></param>
    /// <returns>a given size of details for the given page</returns>
    /// <response code="200">Returns a given size of details for the given page.</response>
    /// <response code="401">User is unauthorized.</response>
    [HttpGet]
    [Authorize(Roles = "view_submitted_applications")]
    [Route("applications")]
    [ProducesResponseType(typeof(Pagination.Response<CompanyApplicationDetails>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public Task<Pagination.Response<CompanyApplicationDetails>> GetApplicationDetailsAsync([FromQuery]int page, [FromQuery]int size) =>
        _logic.GetCompanyApplicationDetailsAsync(page, size);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="applicationId">Id of the application that should be approved</param>
    /// <returns>the result as a boolean</returns>
    /// <response code="200">the result as a boolean.</response>
    /// <response code="401">User is unauthorized.</response>
    [HttpPut]
    [Authorize(Roles = "approve_new_partner")]
    [Route("application/{applicationId}/approveRequest")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType( StatusCodes.Status401Unauthorized)]
    public Task<bool> ApprovePartnerRequest([FromRoute] Guid applicationId) =>
            _logic.ApprovePartnerRequest(applicationId);
}
