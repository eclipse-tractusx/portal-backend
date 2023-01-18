using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Authentication;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Controllers;

[ApiController]
[Route("api/administration/[controller]")]
[Produces("application/json")]
[Consumes("application/json")]
public class ServiceProviderController : ControllerBase
{
    private readonly IServiceProviderBusinessLogic _logic;
    
    /// <summary>
    /// Creates a new instance of <see cref="ServiceProviderController"/> 
    /// </summary>
    /// <param name="logic">The Service Provider Buisness Logic</param>
    public ServiceProviderController(IServiceProviderBusinessLogic logic)
    {
        _logic = logic;
    }

    /// <summary>
    /// get detail data of the calling users service provider
    /// </summary>
    /// <returns></returns>
    /// <remarks>Example: GET: api/administration/serviceprovider/owncompany</remarks>
    /// <response code="200">The service provider details.</response>
    /// <response code="400">The given data are incorrect.</response>
    /// <response code="403">The calling users company is not a service-provider</response>
    /// <response code="404">Service Provider was not found.</response>
    [HttpGet]
    [Route("owncompany", Name = nameof(GetServiceProviderCompanyDetail))]
    [Authorize(Roles = "add_service_offering")]
    [ProducesResponseType(typeof(ProviderDetailReturnData), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public Task<ProviderDetailReturnData> GetServiceProviderCompanyDetail() =>
        this.WithIamUserId(iamUserId => _logic.GetServiceProviderCompanyDetailsAsync(iamUserId));
    
    /// <summary>
    /// Sets detail data to the calling users service provider
    /// </summary>
    /// <param name="data">Service provider detail data</param>
    /// <returns></returns>
    /// <remarks>Example: PUT: api/administration/serviceprovider/owncompany</remarks>
    /// <response code="200">Sets the Service Provider Details data successfully.</response>
    /// <response code="400">The given data are incorrect.</response>
    /// <response code="404">Service Provider was not found.</response>
    [HttpPut]
    [Route("owncompany")]
    [Authorize(Roles = "add_service_offering")]
    [ProducesResponseType(typeof(NoContentResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<NoContentResult> SetServiceProviderCompanyDetail([FromBody] ServiceProviderDetailData data)
    {
        await this.WithIamUserId(iamUserId => _logic.SetServiceProviderCompanyDetailsAsync(data, iamUserId)).ConfigureAwait(false);
        return NoContent();
    }
}
