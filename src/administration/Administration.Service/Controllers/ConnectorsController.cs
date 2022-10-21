/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
 *
 * See the NOTICE file(s) distributed with this work for additional
 * information regarding copyright ownership.
 *
 * This program and the accompanying materials are made available under the
 * terms of the Apache License, Version 2.0 which is available at
 * https://www.apache.org/licenses/LICENSE-2.0.
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 *
 * SPDX-License-Identifier: Apache-2.0
 ********************************************************************************/

using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Authentication;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Controllers;

/// <summary>
/// Controller providing actions for displaying, filtering and updating connectors for companies.
/// </summary>
[Route("api/administration/[controller]")]
[ApiController]
[Produces("application/json")]
[Consumes("application/json")]
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
    /// <param name="page" example="0">Optional query parameter defining the requested page number.</param>
    /// <param name="size" example="15">Optional query parameter defining the number of connectors listed per page.</param>
    /// <returns>Paginated result of connector view models.</returns>
    /// <remarks>
    /// Example: GET: /api/administration/connectors <br />
    /// Example: GET: /api/administration/connectors?page=0&amp;size=15
    /// </remarks>
    /// <response code="200">Returns a list of all of the current user's company's connectors.</response>
    [HttpGet]
    [Route("")]
    [Authorize(Roles = "view_connectors")]
    [ProducesResponseType(typeof(Pagination.Response<ConnectorData>), StatusCodes.Status200OK)]
    public Task<Pagination.Response<ConnectorData>> GetCompanyConnectorsForCurrentUserAsync([FromQuery] int page = 0, [FromQuery] int size = 15) =>
        this.WithIamUserId(iamUserId => _businessLogic.GetAllCompanyConnectorDatasForIamUserAsyncEnum(iamUserId, page, size));

    /// <summary>
    /// Retrieves company connector details for the given connetor id.
    /// </summary>
    /// <param name="connectorId" example="5636F9B9-C3DE-4BA5-8027-00D17A2FECFB">ID of the connector for which the details are to be displayed.</param>
    /// <remarks>Example: GET: /api/administration/connectors/5636F9B9-C3DE-4BA5-8027-00D17A2FECFB</remarks>
    /// <response code="200">Returns details of the requested connector.</response>
    /// <response code="404">Connector ID not found.</response>
    [HttpGet]
    [Route("{connectorId}", Name = nameof(GetCompanyConnectorByIdForCurrentUserAsync))]
    [Authorize(Roles = "view_connectors")]
    [ProducesResponseType(typeof(ConnectorData), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public Task<ConnectorData> GetCompanyConnectorByIdForCurrentUserAsync([FromRoute] Guid connectorId) =>
        this.WithIamUserId(iamUserId => _businessLogic.GetCompanyConnectorDataForIdIamUserAsync(connectorId, iamUserId));

    /// <summary>
    /// Creates a new connector with provided parameters from body, also registers connector at sd factory service.
    /// </summary>
    /// <param name="connectorInputModel">Input model of the connector to be created.</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>View model of the created connector.</returns>
    /// <remarks>Example: POST: /api/administration/connectors</remarks>
    /// <response code="201">Returns a view model of the created connector.</response>
    /// <response code="400">Input parameter are invalid.</response>
    /// <response code="503">Access to SD factory failed with the given status code.</response>
    [HttpPost]
    [Route("")]
    [Authorize(Roles = "add_connectors")]
    [ProducesResponseType(typeof(ConnectorData), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<CreatedAtRouteResult> CreateConnectorAsync([FromBody] ConnectorInputModel connectorInputModel, CancellationToken cancellationToken)
    {
        var connectorData = await this.WithIamUserAndBearerToken(auth => _businessLogic.CreateConnectorAsync(connectorInputModel, auth.bearerToken, auth.iamUserId, false, cancellationToken)).ConfigureAwait(false);
        return CreatedAtRoute(nameof(GetCompanyConnectorByIdForCurrentUserAsync), new { connectorId = connectorData.Id }, connectorData);
    }

    /// <summary>
    /// Creates a new connector with provided parameters from body, also registers connector at sd factory service.
    /// </summary>
    /// <param name="connectorInputModel">Input model of the connector to be created.</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>View model of the created connector.</returns>
    /// <remarks>Example: POST: /api/administration/connectors/managed</remarks>
    /// <response code="201">Returns a view model of the created connector.</response>
    /// <response code="400">Input parameter are invalid.</response>
    /// <response code="503">Access to SD factory failed with the given status code.</response>
    [HttpPost]
    [Route("managed")]
    [Authorize(Roles = "add_connectors")]
    [ProducesResponseType(typeof(ConnectorData), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<CreatedAtRouteResult> CreateManagedConnectorAsync([FromBody] ManagedConnectorInputModel connectorInputModel, CancellationToken cancellationToken)
    {
        var connectorData = await this.WithIamUserAndBearerToken(auth => _businessLogic.CreateConnectorAsync(connectorInputModel, auth.bearerToken, auth.iamUserId, true, cancellationToken)).ConfigureAwait(false);
        return CreatedAtRoute(nameof(GetCompanyConnectorByIdForCurrentUserAsync), new { connectorId = connectorData.Id }, connectorData);
    }

    /// <summary>
    /// Removes a connector from persistence layer by id.
    /// </summary>
    /// <param name="connectorId" example="5636F9B9-C3DE-4BA5-8027-00D17A2FECFB">ID of the connector to be deleted.</param>
    /// <remarks>Example: DELETE: /api/administration/connectors/5636F9B9-C3DE-4BA5-8027-00D17A2FECFB</remarks>
    /// <response code="204">Empty response on success.</response>
    /// <response code="404">Record not found.</response>
    [HttpDelete]
    [Route("{connectorId}")]
    [Authorize(Roles = "delete_connectors")]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteConnectorAsync([FromRoute] Guid connectorId)
    {
        await _businessLogic.DeleteConnectorAsync(connectorId);
        return NoContent();
    }

    /// <summary>
    /// Retrieves company connector end point for the input bpn.
    /// </summary>
    /// <param name="bpns" example="BPNL00000003CRHK">Single or List of Business Partner Number of the company.</param>
    /// <remarks>Example: POST: /api/administration/connectors/discovery</remarks>
    /// <response code="200">Returns connector end point along with bpn.</response>
    [HttpPost]
    [Route("discovery")]
    [Authorize(Roles = "view_connectors")]
    [ProducesResponseType(typeof(IAsyncEnumerable<ConnectorEndPointData>), StatusCodes.Status200OK)]
    public IAsyncEnumerable<ConnectorEndPointData> GetCompanyConnectorEndPointAsync([FromBody] IEnumerable<string> bpns) =>
        _businessLogic.GetCompanyConnectorEndPointAsync(bpns);

}