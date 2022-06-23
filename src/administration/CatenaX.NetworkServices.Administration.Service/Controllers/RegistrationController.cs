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

    [HttpGet]
    [Authorize(Roles = "view_submitted_applications")]
    [Route("application/{applicationId}/companyDetailsWithAddress")]
    [ProducesResponseType(typeof(CompanyWithAddress), (int)HttpStatusCode.OK)]
    public Task<CompanyWithAddress> GetCompanyWithAddressAsync([FromRoute] Guid applicationId) =>
        _logic.GetCompanyWithAddressAsync(applicationId);
    
    /// <summary>
    /// Fet Application Detail by Company Name or Status
    /// </summary>
    /// <param name="page">page index start from 0</param>
    /// <param name="size">size to get number of records</param>
    /// <param name="companyName">search by company name</param>
    /// <returns>Company Application Details</returns>
    /// <remarks>Example: GET: api/administration/registration/applications?companyName=Car&page=0&size=4</remarks>
    /// <remarks>Example: GET: api/administration/registration/applications?page=0&size=4</remarks>
    /// <response code="200">Result as a Company Application Details</response>
    [HttpGet]
    [Authorize(Roles = "view_submitted_applications")]
    [Route("applications")]
    [ProducesResponseType(typeof(Pagination.Response<CompanyApplicationDetails>), StatusCodes.Status200OK)]
    public Task<Pagination.Response<CompanyApplicationDetails>> GetApplicationDetailsAsync([FromQuery]int page, [FromQuery]int size, [FromQuery]string? companyName = null) =>
        _logic.GetCompanyApplicationDetailsAsync(page, size, companyName);

    [HttpPut]
    [Authorize(Roles = "approve_new_partner")]
    [Route("application/{applicationId}/approveRequest")]
    public Task<bool> ApprovePartnerRequest([FromRoute] Guid applicationId) =>
            _logic.ApprovePartnerRequest(applicationId);

    /// <summary>
    /// Decline the Partner Registration Request
    /// </summary>
    /// <param name="applicationId" example="31404026-64ee-4023-a122-3c7fc40e57b1">Company Application Id for which request will be declined</param>
    /// <returns>Result as a boolean</returns>
    /// <remarks>Example: PUT: api/administration/registration/application/31404026-64ee-4023-a122-3c7fc40e57b1/declineRequest</remarks>
    /// <response code="200">Result as a boolean</response>
    /// <response code="400">Company Application Not in Submitted State or Username has no assigned emailid.</response>
    [HttpPut]
    [Authorize(Roles = "decline_new_partner")]
    [Route("application/{applicationId}/declineRequest")]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public Task<bool> DeclinePartnerRequest([FromRoute] Guid applicationId) =>
            _logic.DeclinePartnerRequest(applicationId);
}
