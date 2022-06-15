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
    /// <response code="404">No company found for applicationId.</response>
    /// <response code="500">Internal server error occured, e.g. a database error.</response>
    [HttpGet]
    [Authorize(Roles = "view_submitted_applications")]
    [Route("application/{applicationId}/companyDetailsWithAddress")]
    [ProducesResponseType(typeof(CompanyWithAddress), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public Task<CompanyWithAddress> GetCompanyWithAddressAsync([FromRoute] Guid applicationId) =>
        _logic.GetCompanyWithAddressAsync(applicationId);

    /// <summary>
    /// Gets the application details with pagination
    /// </summary>
    /// <param name="page"></param>
    /// <param name="size"></param>
    /// <returns>a given size of details for the given page</returns>
    /// Example: GET: api/administration/registration/applications
    /// <response code="200">Returns a given size of details for the given page.</response>
    /// <response code="500">Internal server error occured, e.g. a database error.</response>
    [HttpGet]
    [Authorize(Roles = "view_submitted_applications")]
    [Route("applications")]
    [ProducesResponseType(typeof(Pagination.Response<CompanyApplicationDetails>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public Task<Pagination.Response<CompanyApplicationDetails>> GetApplicationDetailsAsync([FromQuery]int page, [FromQuery]int size) =>
        _logic.GetCompanyApplicationDetailsAsync(page, size);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="applicationId" example="4f0146c6-32aa-4bb1-b844-df7e8babdcb4">Id of the application that should be approved</param>
    /// <returns>the result as a boolean</returns>
    /// Example: PUT: api/administration/registration/application/4f0146c6-32aa-4bb1-b844-df7e8babdcb4/approveRequest
    /// <response code="200">the result as a boolean.</response>
    /// <response code="400">Either the CompanyApplication is not in status SUBMITTED or the BusinessPartnerNumber (bpn) for the given CompanyApplications company is empty.</response>
    /// <response code="500">Internal server error occured, e.g. a database error.</response>
    [HttpPut]
    [Authorize(Roles = "approve_new_partner")]
    [Route("application/{applicationId}/approveRequest")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType( StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public Task<bool> ApprovePartnerRequest([FromRoute] Guid applicationId) =>
            _logic.ApprovePartnerRequest(applicationId);
}
