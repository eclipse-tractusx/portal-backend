/********************************************************************************
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Web;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Web;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Web.Identity;
using Org.Eclipse.TractusX.Portal.Backend.Web.PublicInfos;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Controllers;

/// <summary>
/// Controller providing actions for displaying, filtering and updating connectors for companies.
/// </summary>
[EnvironmentRoute("MVC_ROUTING_BASEPATH", "[controller]")]
[ApiController]
[Produces("application/json")]
public class ConnectorsController(IConnectorsBusinessLogic logic)
    : ControllerBase
{
    /// <summary>
    /// Retrieves all company registered own connectors and their status.
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
        logic.GetAllCompanyConnectorDatas(page, size);

    /// <summary>
    /// Retrieves all registered connectors which are managed connectors of company customers and their status.
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
    [PublicUrl(CompanyRoleId.APP_PROVIDER, CompanyRoleId.SERVICE_PROVIDER)]
    public Task<Pagination.Response<ManagedConnectorData>> GetManagedConnectorsForCurrentUserAsync([FromQuery] int page = 0, [FromQuery] int size = 15) =>
        logic.GetManagedConnectorForCompany(page, size);

    /// <summary>
    /// Retrieves connector information for a specific connector by its ID. Note: only company owned connectors can get called.
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
        logic.GetCompanyConnectorData(connectorId);

    /// <summary>
    /// Allows to register owned company connectors (self-hosted/-managed) inside the CX dataspace.
    /// </summary>
    /// <param name="connectorInputModel">Input model of the connector to be created.</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>View model of the created connector.</returns>
    /// <remarks>Example: POST: /api/administration/connectors/daps</remarks>
    /// <response code="201">Returns a view model of the created connector.</response>
    /// <response code="400">Input parameter are invalid.</response>
    /// <response code="503">Access to SD factory failed with the given status code.</response>
    [HttpPost]
    [Route("")]
    [Authorize(Roles = "add_connectors")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<CreatedAtRouteResult> CreateConnectorAsync([FromForm] ConnectorInputModel connectorInputModel, CancellationToken cancellationToken)
    {
        var connectorId = await logic.CreateConnectorAsync(connectorInputModel, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        return CreatedAtRoute(nameof(GetCompanyConnectorByIdForCurrentUserAsync), new { connectorId }, connectorId);
    }

    /// <summary>
    /// Allows to register managed connectors for 3rd parties/customers inside the CX dataspace.
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
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    [PublicUrl(CompanyRoleId.APP_PROVIDER, CompanyRoleId.SERVICE_PROVIDER)]
    public async Task<CreatedAtRouteResult> CreateManagedConnectorAsync([FromForm] ManagedConnectorInputModel connectorInputModel, CancellationToken cancellationToken)
    {
        var connectorId = await logic.CreateManagedConnectorAsync(connectorInputModel, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        return CreatedAtRoute(nameof(GetCompanyConnectorByIdForCurrentUserAsync), new { connectorId }, connectorId);
    }

    /// <summary>
    /// Removes a connector from persistence layer by id.
    /// </summary>
    /// <param name="connectorId" example="5636F9B9-C3DE-4BA5-8027-00D17A2FECFB">ID of the connector to be deleted.</param>
    /// <param name="deleteServiceAccount">if <c>true</c> the linked service account will be deleted, otherwise the connection to the connector will just be removed</param>
    /// <remarks>Example: DELETE: /api/administration/connectors/{connectorId}?deleteServiceAccount=true</remarks>
    /// <response code="204">Empty response on success.</response>
    /// <response code="404">Record not found.</response>
    /// <response code="409">Connector status does not match a deletion scenario. Deletion declined.</response>
    [HttpDelete]
    [Route("{connectorId}")]
    [Authorize(Roles = "delete_connectors")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteConnectorAsync([FromRoute] Guid connectorId, [FromQuery] bool deleteServiceAccount = false)
    {
        await logic.DeleteConnectorAsync(connectorId, deleteServiceAccount);
        return NoContent();
    }

    /// <summary>
    /// Retrieve dataspace registered connectors by their registered BPNL or retrieve them all by sending an empty array.
    /// </summary>
    /// <param name="bpns" example="BPNL00000003CRHK">Single or List of Business Partner Number of the company.</param>
    /// <remarks>Example: POST: /api/administration/connectors/discovery</remarks>
    /// <response code="200">Returns company connector per bpn.</response>
    [HttpPost]
    [Route("discovery")]
    [Authorize(Roles = "view_connectors")]
    [ProducesResponseType(typeof(IAsyncEnumerable<ConnectorEndPointData>), StatusCodes.Status200OK)]
    [PublicUrl(CompanyRoleId.APP_PROVIDER, CompanyRoleId.SERVICE_PROVIDER, CompanyRoleId.ACTIVE_PARTICIPANT)]
    public IAsyncEnumerable<ConnectorEndPointData> GetCompanyConnectorEndPointAsync([FromBody] IEnumerable<string>? bpns = null) =>
        logic.GetCompanyConnectorEndPointAsync(bpns);

    /// <summary>
    /// Asynchron callback endpoint for the clearinghouse provider to submit the connector SD document.
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
    [Authorize(Policy = PolicyTypes.ServiceAccount)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<NoContentResult> ProcessClearinghouseSelfDescription([FromBody] SelfDescriptionResponseData data, CancellationToken cancellationToken)
    {
        await logic.ProcessClearinghouseSelfDescription(data, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        return NoContent();
    }

    /// <summary>
    /// Updates the connector url
    /// </summary>
    /// <param name="connectorId" example="5636F9B9-C3DE-4BA5-8027-00D17A2FECFB">Id of the connector to trigger the daps call.</param>
    /// <param name="data">The update data</param>
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
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<NoContentResult> UpdateConnectorUrl([FromRoute] Guid connectorId, [FromBody] ConnectorUpdateRequest data)
    {
        await logic.UpdateConnectorUrl(connectorId, data)
            .ConfigureAwait(ConfigureAwaitOptions.None);
        return NoContent();
    }

    /// <summary>
    /// Retrieve the offer subscriptions for the company with the linked connectorIds.
    /// </summary>
    /// <param name="connectorIdSet" example="false">
    /// Optional: if <c>true</c> only respond with subscriptions where a link to a connector is given,
    /// if <c>false</c> it will only return subscriptions where no link to an connector exists.
    /// </param>
    /// <remarks>Example: GET: /api/administration/connectors/offerSubscriptions</remarks>
    /// <response code="200">Returns list of the offer subscriptions for the company.</response>
    [HttpGet]
    [Route("offerSubscriptions")]
    [Authorize(Roles = "view_connectors")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [ProducesResponseType(typeof(IAsyncEnumerable<ConnectorEndPointData>), StatusCodes.Status200OK)]
    public IAsyncEnumerable<OfferSubscriptionConnectorData> GetConnectorOfferSubscriptionData([FromQuery] bool? connectorIdSet) =>
        logic.GetConnectorOfferSubscriptionData(connectorIdSet);

    /// <summary>
    /// Retrieves all active connectors with missing sd document.
    /// </summary>
    /// <param name="page" example="0">Optional query parameter defining the requested page number.</param>
    /// <param name="size" example="15">Optional query parameter defining the number of connectors listed per page.</param>
    /// <returns>Paginated result of connector view models.</returns>
    /// <remarks>
    /// Example: GET: /api/administration/connectors/missing-sd-document <br />
    /// Example: GET: /api/administration/connectors/missing-sd-document?page=0&amp;size=15
    /// </remarks>
    /// <response code="200">Returns a list of all active connectors with missing sd document.</response>
    [HttpGet]
    [Route("missing-sd-document")]
    [Authorize(Roles = "view_connectors")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [ProducesResponseType(typeof(Pagination.Response<ConnectorMissingSdDocumentData>), StatusCodes.Status200OK)]
    public Task<Pagination.Response<ConnectorMissingSdDocumentData>> GetConnectorsWithMissingSdDocument([FromQuery] int page = 0, [FromQuery] int size = 15) =>
        logic.GetConnectorsWithMissingSdDocument(page, size);

    /// <summary>
    /// Triggers the process to create the missing self description documents
    /// </summary>
    /// <returns>NoContent</returns>
    /// Example: POST: /api/administration/connectors/trigger-self-description
    /// <response code="204">Empty response on success.</response>
    /// <response code="404">No Process found for the processId</response>
    [HttpPost]
    [Authorize(Roles = "approve_new_partner")]
    [Authorize(Policy = PolicyTypes.CompanyUser)]
    [Route("trigger-self-description")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<NoContentResult> TriggerSelfDescriptionProcess()
    {
        await logic.TriggerSelfDescriptionCreation().ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>
    /// Retriggers the process to create the missing self description documents
    /// </summary>
    /// <returns>NoContent</returns>
    /// Example: POST: /api/administration/connectors/retrigger-self-description/{processId}
    /// <response code="204">Empty response on success.</response>
    /// <response code="404">No Process found for the processId</response>
    [HttpPost]
    [Authorize(Roles = "approve_new_partner")]
    [Authorize(Policy = PolicyTypes.CompanyUser)]
    [Route("retrigger-self-description")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<NoContentResult> RetriggerSelfDescriptionProcess([FromRoute] Guid processId)
    {
        await logic.RetriggerSelfDescriptionCreation(processId).ConfigureAwait(false);
        return NoContent();
    }
}
