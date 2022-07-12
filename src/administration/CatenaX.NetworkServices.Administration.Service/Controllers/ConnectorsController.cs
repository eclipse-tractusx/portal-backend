/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
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

using CatenaX.NetworkServices.Administration.Service.BusinessLogic;
using CatenaX.NetworkServices.Administration.Service.Models;
using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.Framework.Models;
using CatenaX.NetworkServices.Keycloak.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatenaX.NetworkServices.Administration.Service.Controllers;

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
    [ProducesResponseType(typeof(Pagination.Response<ConnectorViewModel>), StatusCodes.Status200OK)]
    public Task<Pagination.Response<ConnectorViewModel>> GetCompanyConnectorsForCurrentUserAsync([FromQuery] int page = 0, [FromQuery] int size = 15) =>
        this.WithIamUserId(iamUserId => _businessLogic.GetAllCompanyConnectorViewModelsForIamUserAsyncEnum(iamUserId, page, size));

    /// <summary>
    /// Creates a new connector with provided parameters from body, also registers connector at sd factory service.
    /// </summary>
    /// <param name="connectorInputModel">Input model of the connector to be created.</param>
    /// <returns>View model of the created connector.</returns>
    /// <remarks>Example: POST: /api/administration/connectors</remarks>
    /// <response code="201">Returns a view model of the created connector.</response>
    /// <response code="400">Provided connector does not respect database constraints.</response>
    /// <response code="503">Access to SD factory failed with the given status code.</response>
    [HttpPost]
    [Route("")]
    [Authorize(Roles = "add_connectors")]
    [ProducesResponseType(typeof(ActionResult<ConnectorViewModel>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<ConnectorViewModel>> CreateConnectorAsync([FromBody] ConnectorInputModel connectorInputModel) =>
        CreatedAtRoute(string.Empty, await _businessLogic.CreateConnectorAsync(connectorInputModel, Request.Headers.Authorization.First().Substring("Bearer ".Length)));

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
}
