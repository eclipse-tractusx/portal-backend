using CatenaX.NetworkServices.Administration.Service.BusinessLogic;
using CatenaX.NetworkServices.Administration.Service.Models;
using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.Framework.Models;
using CatenaX.NetworkServices.Keycloak.Authentication;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatenaX.NetworkServices.Administration.Service.Controllers;

/// <summary>
/// Creates a new instance of <see cref="CompanyDataController"/>
/// </summary>
[ApiController]
[Route("api/administration/companydata")]
[Produces("application/json")]
[Consumes("application/json")]
public class CompanyDataController : ControllerBase
{
    private readonly ICompanyDataBusinessLogic _logic;

    /// <summary>
    /// Creates a new instance of <see cref="CompanyDataController"/>
    /// </summary>
    /// <param name="logic">The company data business logic</param>
    public CompanyDataController(ICompanyDataBusinessLogic logic)
    {
        _logic = logic;
    }

    /// <summary>
    /// Gets the company with its address
    /// </summary>
    /// <returns>the company with its address</returns>
    /// <remarks>Example: GET: api/administration/companydata/ownCompanyDetails</remarks>
    /// <response code="200">Returns the company with its address.</response>
    /// <response code="400">No company data was found.</response>
    [HttpGet]
    [Authorize(Roles = "view_company_data")]
    [Route("ownCompanyDetails")]
    [ProducesResponseType(typeof(CompanyWithAddress), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public Task<CompanyWithAddress> GetOwnCompanyDetailsAsync() =>
        this.WithIamUserId(iamUserId =>_logic.GetOwnCompanyDetailsAsync(iamUserId));
}
