using CatenaX.NetworkServices.Administration.Service.BusinessLogic;
using CatenaX.NetworkServices.Administration.Service.Models;
using CatenaX.NetworkServices.Framework.Models;
using CatenaX.NetworkServices.Keycloak.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatenaX.NetworkServices.Administration.Service.Controllers;

/// <summary>
/// Controller providing actions for displaying, filtering and updating connectors for companies.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class ConnectorsController : ControllerBase
{
    private readonly IConnectorsBusinessLogic _businessLogic;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="connectorsBusinessLogic">Connectors business logic.</param>
    public ConnectorsController(IConnectorsBusinessLogic connectorsBusinessLogic)
    {
        _businessLogic = connectorsBusinessLogic;
    }

    /// <summary>
    /// Retrieves all company connectors for currently logged in user.
    /// </summary>
    /// <param name="page">Optional query parameter defining the requested page number.</param>
    /// <param name="size">Optional query parameter defining the number of connectors listed per page.</param>
    /// <returns>Paginated result of connector view models.</returns>
    /// <remarks>Example: GET: /api/connectors</remarks>
    /// <response code="200">Returns a list of all of the current user's company's connectors.</response>
    [HttpGet]
    [Route("")]
    [Authorize(Roles = "view_connectors")]
    [ProducesResponseType(typeof(Pagination.Response<ConnectorViewModel>), StatusCodes.Status200OK)]
    public Task<Pagination.Response<ConnectorViewModel>> GetCompanyConnectorsForCurrentUserAsync([FromQuery] int page = 1, [FromQuery] int size = 15) =>
        this.WithIamUserId(iamUserId => _businessLogic.GetAllCompanyConnectorViewModelsForIamUserAsyncEnum(iamUserId, page, size));

    /// <summary>
    /// Created a new connector with provided parameters from body.
    /// </summary>
    /// <param name="connectorInputModel">Input model of the connector to be created.</param>
    /// <returns>View model of the created connector.</returns>
    /// <remarks>Example: POST: /api/connectors</remarks>
    /// <response code="201">Returns a view model of the created connector.</response>
    [HttpPost]
    [Route("")]
    [Authorize(Roles = "add_connectors")]
    [ProducesResponseType(typeof(ActionResult<ConnectorViewModel>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ConnectorViewModel>> CreateConnectorAsync([FromBody] ConnectorInputModel connectorInputModel) =>
        CreatedAtRoute(string.Empty, await _businessLogic.CreateConnectorAsync(connectorInputModel));

    /// <summary>
    /// Removes a connector from persistence layer by id.
    /// </summary>
    /// <param name="connectorId">ID of the connector to be deleted.</param>
    /// <remarks>Example: DELETE: /api/connectors/5636F9B9-C3DE-4BA5-8027-00D17A2FECFB</remarks>
    /// <response code="204">Empty response on success.</response>
    [HttpDelete]
    [Route("{connectorId}")]
    [Authorize(Roles = "delete_connectors")]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteConnectorAsync([FromRoute] Guid connectorId)
    {
        await _businessLogic.DeleteConnectorAsync(connectorId);
        return NoContent();
    }
        
}
