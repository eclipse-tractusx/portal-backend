using CatenaX.NetworkServices.Administration.Service.BusinessLogic;
using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.Framework.Models;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatenaX.NetworkServices.Administration.Service.Controllers;

[ApiController]
[Route("api/administration/registration")]
[Produces("application/json")]
[Consumes("application/json")]
public class RegistrationController : ControllerBase
{
    private readonly IRegistrationBusinessLogic _logic;
    
    /// <summary>
    /// Creates a new instance of <see cref="RegistrationController"/>
    /// </summary>
    /// <param name="logic">The business logic for the registration</param>
    public RegistrationController(IRegistrationBusinessLogic logic)
    {
        _logic = logic;
    }

    /// <summary>
    /// Gets the company with its address
    /// </summary>
    /// <param name="applicationId" example="4f0146c6-32aa-4bb1-b844-df7e8babdcb4"></param>
    /// <returns>the company with its address</returns>
    /// Example: GET: api/administration/registration/application/4f0146c6-32aa-4bb1-b844-df7e8babdcb4/companyDetailsWithAddress
    /// <response code="200">Returns the company with its address.</response>
    /// <response code="400">No applicationId was set.</response>
    /// <response code="404">No company found for applicationId.</response>
    [HttpGet]
    [Authorize(Roles = "view_submitted_applications")]
    [Route("application/{applicationId}/companyDetailsWithAddress")]
    [ProducesResponseType(typeof(CompanyWithAddress), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public Task<CompanyWithAddress> GetCompanyWithAddressAsync([FromRoute] Guid applicationId) =>
        _logic.GetCompanyWithAddressAsync(applicationId);
    
    /// <summary>
    /// Get Application Detail by Company Name or Status
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

    /// <summary>
    /// Approves the partner request
    /// </summary>
    /// <param name="applicationId" example="4f0146c6-32aa-4bb1-b844-df7e8babdcb4">Id of the application that should be approved</param>
    /// <returns>the result as a boolean</returns>
    /// Example: PUT: api/administration/registration/application/4f0146c6-32aa-4bb1-b844-df7e8babdcb4/approveRequest
    /// <response code="200">the result as a boolean.</response>
    /// <response code="400">Either the CompanyApplication is not in status SUBMITTED or the BusinessPartnerNumber (bpn) for the given CompanyApplications company is empty.</response>
    [HttpPut]
    [Authorize(Roles = "approve_new_partner")]
    [Route("application/{applicationId}/approveRequest")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType( StatusCodes.Status400BadRequest)]
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
