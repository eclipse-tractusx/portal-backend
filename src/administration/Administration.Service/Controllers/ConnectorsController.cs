/********************************************************************************
 * Copyright (c) 2021, 2023 BMW Group AG
 * Copyright (c) 2021, 2023 Contributors to the Eclipse Foundation
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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Library;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Authentication;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Controllers;

/// <summary>
/// Controller providing actions for displaying, filtering and updating connectors for companies.
/// </summary>
[Route("api/administration/[controller]")]
[ApiController]
[Produces("application/json")]
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
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [ProducesResponseType(typeof(Pagination.Response<ConnectorData>), StatusCodes.Status200OK)]
    public Task<Pagination.Response<ConnectorData>> GetCompanyConnectorsForCurrentUserAsync([FromQuery] int page = 0, [FromQuery] int size = 15) =>
        this.WithCompanyId(companyId => _businessLogic.GetAllCompanyConnectorDatas(companyId, page, size));

    /// <summary>
    /// Retrieves all company connectors for currently logged in user.
    /// </summary>
    /// <param name="page" example="0">Optional query parameter defining the requested page number.</param>
    /// <param name="size" example="15">Optional query parameter defining the number of connectors listed per page.</param>
    /// <returns>Paginated result of connector view models.</returns>
    /// <remarks>
    /// Example: GET: /api/administration/connectors/managed <br />
    /// Example: GET: /api/administration/connectors/managed?page=0&amp;size=15
    /// </remarks>
    /// <response code="200">Returns a list of all of the current user's company's connectors.</response>
    [HttpGet]
    [Route("managed")]
    [Authorize(Roles = "view_connectors")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [ProducesResponseType(typeof(Pagination.Response<ConnectorData>), StatusCodes.Status200OK)]
    public Task<Pagination.Response<ManagedConnectorData>> GetManagedConnectorsForCurrentUserAsync([FromQuery] int page = 0, [FromQuery] int size = 15) =>
        this.WithCompanyId(companyId => _businessLogic.GetManagedConnectorForCompany(companyId, page, size));

    /// <summary>
    /// Retrieves company connector details for the given connetor id.
    /// </summary>
    /// <param name="connectorId" example="5636F9B9-C3DE-4BA5-8027-00D17A2FECFB">ID of the connector for which the details are to be displayed.</param>
    /// <remarks>Example: GET: /api/administration/connectors/5636F9B9-C3DE-4BA5-8027-00D17A2FECFB</remarks>
    /// <response code="200">Returns details of the requested connector.</response>
    /// <response code="404">Connector ID not found.</response>
    /// <response code="403">user does not belong to company of companyUserId.</response>
    [HttpGet]
    [Route("{connectorId}", Name = nameof(GetCompanyConnectorByIdForCurrentUserAsync))]
    [Authorize(Roles = "view_connectors")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [ProducesResponseType(typeof(ConnectorData), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public Task<ConnectorData> GetCompanyConnectorByIdForCurrentUserAsync([FromRoute] Guid connectorId) =>
        this.WithCompanyId(companyId => _businessLogic.GetCompanyConnectorData(connectorId, companyId));

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
    [Obsolete("Please use the /api/administration/connectors/daps endpoint instead. This Endpoint will be removed in a future release.")]
    [HttpPost]
    [Route("")]
    [Authorize(Roles = "add_connectors")]
    [ProducesResponseType(typeof(ConnectorData), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    public Task<CreatedAtRouteResult> CreateConnectorAsync([FromBody] ConnectorInputModel connectorInputModel, CancellationToken cancellationToken) =>
        this.CreateConnectorWithDapsAsync(connectorInputModel, cancellationToken);

    /// <summary>
    /// Creates a new connector with provided parameters from body, also registers connector at sd factory service.
    /// </summary>
    /// <param name="connectorInputModel">Input model of the connector to be created.</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>View model of the created connector.</returns>
    /// <remarks>Example: POST: /api/administration/connectors/daps</remarks>
    /// <response code="201">Returns a view model of the created connector.</response>
    /// <response code="400">Input parameter are invalid.</response>
    /// <response code="503">Access to SD factory failed with the given status code.</response>
    [HttpPost]
    [Route("daps")]
    [Authorize(Roles = "add_connectors")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Authorize(Policy = PolicyTypes.ValidIdentity)]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<CreatedAtRouteResult> CreateConnectorWithDapsAsync([FromForm] ConnectorInputModel connectorInputModel, CancellationToken cancellationToken)
    {
        var connectorId = await this.WithUserIdAndCompanyId(identity => _businessLogic.CreateConnectorAsync(connectorInputModel, identity, cancellationToken)).ConfigureAwait(false);
        return CreatedAtRoute(nameof(GetCompanyConnectorByIdForCurrentUserAsync), new { connectorId }, connectorId);
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
    [Obsolete("Please use the /api/administration/connectors/managed-daps endpoint instead. This Endpoint will be removed in a future release.")]
    [HttpPost]
    [Route("managed")]
    [Authorize(Roles = "add_connectors")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    public Task<CreatedAtRouteResult> CreateManagedConnectorAsync([FromBody] ManagedConnectorInputModel connectorInputModel, CancellationToken cancellationToken) =>
        this.CreateManagedConnectorWithDapsAsync(connectorInputModel, cancellationToken);

    /// <summary>
    /// Creates a new connector with provided parameters from body, also registers connector at sd factory service.
    /// </summary>
    /// <param name="connectorInputModel">Input model of the connector to be created.</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>View model of the created connector.</returns>
    /// <remarks>Example: POST: /api/administration/connectors/managed-daps</remarks>
    /// <response code="201">Returns a view model of the created connector.</response>
    /// <response code="400">Input parameter are invalid.</response>
    /// <response code="503">Access to SD factory failed with the given status code.</response>
    [HttpPost]
    [Route("managed-daps")]
    [Authorize(Roles = "add_connectors")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Authorize(Policy = PolicyTypes.ValidIdentity)]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<CreatedAtRouteResult> CreateManagedConnectorWithDapsAsync([FromForm] ManagedConnectorInputModel connectorInputModel, CancellationToken cancellationToken)
    {
        var connectorId = await this.WithUserIdAndCompanyId(identity => _businessLogic.CreateManagedConnectorAsync(connectorInputModel, identity, cancellationToken)).ConfigureAwait(false);
        return CreatedAtRoute(nameof(GetCompanyConnectorByIdForCurrentUserAsync), new { connectorId }, connectorId);
    }

    /// <summary>
    /// Triggers the daps endpoint for the given connector.
    /// </summary>
    /// <param name="connectorId" example="5636F9B9-C3DE-4BA5-8027-00D17A2FECFB">Id of the connector to trigger the daps call.</param>
    /// <param name="certificate"></param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>View model of the created connector.</returns>
    /// <remarks>Example: POST: /api/administration/connectors/trigger-daps/5636F9B9-C3DE-4BA5-8027-00D17A2FECFB</remarks>
    /// <response code="200">Returns true if the daps call was successful, otherwise false.</response>
    /// <response code="400">Input parameter are invalid.</response>
    /// <response code="404">Connector was not found.</response>
    /// <response code="503">Access to SD factory failed with the given status code.</response>
    /// <response code="403">user does not belong to company of companyUserId.</response>
    [HttpPost]
    [Route("trigger-daps/{connectorId:guid}")]
    [Authorize(Roles = "add_connectors")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Authorize(Policy = PolicyTypes.ValidIdentity)]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<bool> TriggerDapsAuth([FromRoute] Guid connectorId, [FromForm] IFormFile certificate, CancellationToken cancellationToken) =>
        await this.WithUserIdAndCompanyId(identity => _businessLogic.TriggerDapsAsync(connectorId, certificate, identity, cancellationToken)).ConfigureAwait(false);

    /// <summary>
    /// Removes a connector from persistence layer by id.
    /// </summary>
    /// <param name="connectorId" example="5636F9B9-C3DE-4BA5-8027-00D17A2FECFB">ID of the connector to be deleted.</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <remarks>Example: DELETE: /api/administration/connectors/5636F9B9-C3DE-4BA5-8027-00D17A2FECFB</remarks>
    /// <response code="204">Empty response on success.</response>
    /// <response code="404">Record not found.</response>
    /// <response code="409">Connector status does not match a deletion scenario. Deletion declined.</response>
    [HttpDelete]
    [Route("{connectorId}")]
    [Authorize(Roles = "delete_connectors")]
    [Authorize(Policy = PolicyTypes.ValidIdentity)]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteConnectorAsync([FromRoute] Guid connectorId, CancellationToken cancellationToken)
    {
        await this.WithUserId(userId => _businessLogic.DeleteConnectorAsync(connectorId, userId, cancellationToken));
        return NoContent();
    }

    /// <summary>
    /// Retrieve company connector end point with bpn.
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

    /// <summary>
    /// Processes the clearinghouse self description push
    /// </summary>
    /// <param name="data">The response data for the self description</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// Example: POST: api/administration/connectors/clearinghouse/selfDescription <br />
    /// <response code="204">Empty response on success.</response>
    /// <response code="409">Connector has document assigned.</response>
    /// <response code="404">Record Not Found.</response>
    [HttpPost]
    [Authorize(Roles = "submit_connector_sd")]
    [Route("clearinghouse/selfDescription")]
    [Authorize(Policy = PolicyTypes.ValidIdentity)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<NoContentResult> ProcessClearinghouseSelfDescription([FromBody] SelfDescriptionResponseData data, CancellationToken cancellationToken)
    {
        await this.WithUserId(userId => _businessLogic.ProcessClearinghouseSelfDescription(data, userId, cancellationToken).ConfigureAwait(false));
        return NoContent();
    }

    /// <summary>
    /// Updates the connector url
    /// </summary>
    /// <param name="connectorId" example="5636F9B9-C3DE-4BA5-8027-00D17A2FECFB">Id of the connector to trigger the daps call.</param>
    /// <param name="data">The update data</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>NoContent Result.</returns>
    /// <remarks>Example: PUT: /api/administration/connectors/{connectorId}/connectorUrl</remarks>
    /// <response code="204">Url was successfully updated.</response>
    /// <response code="400">Input parameter are invalid.</response>
    /// <response code="403">user does not belong to host company of the connector.</response>
    /// <response code="404">Connector was not found.</response>
    /// <response code="503">Access to Daps failed with the given status code.</response>
    [HttpPut]
    [Route("{connectorId:guid}/connectorUrl")]
    [Authorize(Roles = "modify_connectors")]
    [Authorize(Policy = PolicyTypes.ValidIdentity)]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<NoContentResult> UpdateConnectorUrl([FromRoute] Guid connectorId, [FromBody] ConnectorUpdateRequest data, CancellationToken cancellationToken)
    {
        await this.WithUserIdAndCompanyId(identity => _businessLogic.UpdateConnectorUrl(connectorId, data, identity, cancellationToken))
            .ConfigureAwait(false);
        return NoContent();
    }
}
