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
    /// <param name="serviceProviderDetailDataId" example="ccf3cd17-c4bc-4cec-a041-2da709b787b0">Id of the service provider detail data</param>
    /// <returns></returns>
    /// <remarks>Example: POST: api/administration/serviceprovider/owncompany/ccf3cd17-c4bc-4cec-a041-2da709b787b0</remarks>
    /// <response code="200">The service provider details.</response>
    /// <response code="400">The given data are incorrect.</response>
    /// <response code="403">The calling users company is not a service-provider</response>
    /// <response code="404">Service Provider was not found.</response>
    [HttpGet]
    [Route("owncompany/{serviceProviderDetailDataId}", Name = nameof(GetServiceProviderCompanyDetail))]
    [Authorize(Roles = "add_service_offering")]
    [ProducesResponseType(typeof(ServiceProviderDetailReturnData), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public Task<ServiceProviderDetailReturnData> GetServiceProviderCompanyDetail([FromRoute] Guid serviceProviderDetailDataId) =>
        this.WithIamUserId(iamUserId => _logic.GetServiceProviderCompanyDetailsAsync(serviceProviderDetailDataId, iamUserId));

    /// <summary>
    /// Adds detail data to the calling users service provider
    /// </summary>
    /// <param name="data">Data to be added to the service provider</param>
    /// <returns></returns>
    /// <remarks>Example: POST: api/administration/serviceprovider/owncompany</remarks>
    /// <response code="201">The service provider details were added successfully.</response>
    /// <response code="400">The given data are incorrect.</response>
    /// <response code="403">The calling users company is not a service-provider</response>
    /// <response code="404">Service Provider was not found.</response>
    [HttpPost]
    [Route("owncompany")]
    [Authorize(Roles = "add_service_offering")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<CreatedAtRouteResult> CreateServiceProviderCompanyDetail([FromBody] ServiceProviderDetailData data)
    {
        var id = await this.WithIamUserId(createdByName => _logic.CreateServiceProviderCompanyDetailsAsync(data, createdByName)).ConfigureAwait(false);
        return CreatedAtRoute(nameof(GetServiceProviderCompanyDetail), new { serviceProviderDetailDataId = id }, id);
    }
}
