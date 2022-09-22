﻿using CatenaX.NetworkServices.Administration.Service.BusinessLogic;
using CatenaX.NetworkServices.Administration.Service.Models;
using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.Keycloak.Authentication;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatenaX.NetworkServices.Administration.Service.Controllers;

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
    /// Adds detail data to the service provider
    /// </summary>
    /// <param name="companyId" example="ccf3cd17-c4bc-4cec-a041-2da709b787b0">Id of the service provider</param>
    /// <param name="data">Data to be added to the service provider</param>
    /// <returns></returns>
    /// <remarks>Example: POST: api/administration/serviceprovider/ccf3cd17-c4bc-4cec-a041-2da709b787b0</remarks>
    /// <response code="201">The service provider details were added successfully.</response>
    /// <response code="400">The given data are incorrect.</response>
    /// <response code="404">Service Provider was not found.</response>
    [HttpPost]
    [Route("{serviceProviderId}")]
    [Authorize(Roles = "add_service_offering")]
    [ProducesResponseType(typeof(OkResult), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OkResult>> ExecuteCompanyUserCreation([FromQuery] Guid companyId, [FromBody] ServiceProviderDetailData data)
    {
        await this.WithIamUserId(createdByName =>
            _logic.CreateServiceProviderCompanyDetails(companyId, data, createdByName)).ConfigureAwait(false);
        return Ok();
    }
}