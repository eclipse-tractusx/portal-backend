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

using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.Framework.Models;
using CatenaX.NetworkServices.Keycloak.Authentication;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.Service.Service.BusinessLogic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatenaX.NetworkServices.Service.Service.Controllers;

/// <summary>
/// Controller providing actions for displaying, filtering and updating services.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[Consumes("application/json")]
public class ServiceController : ControllerBase
{
    private readonly IServiceBusinessLogic _serviceBusinessLogic;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="serviceBusinessLogic">Logic dependency.</param>
    public ServiceController(IServiceBusinessLogic serviceBusinessLogic)
    {
        _serviceBusinessLogic = serviceBusinessLogic;
    }

    /// <summary>
    /// Retrieves all active services in the marketplace.
    /// </summary>
    /// <param name="page" example="0">Optional the page of the services.</param>
    /// <param name="size" example="15">Amount of services that should be returned, default is 15.</param>
    /// <returns>Collection of all active services.</returns>
    /// <remarks>Example: GET: /api/services/active</remarks>
    /// <response code="200">Returns the list of all active services.</response>
    [HttpGet]
    [Route("active")]
    [Authorize(Roles = "view_service_offering")]
    [ProducesResponseType(typeof(IAsyncEnumerable<ServiceDetailData>), StatusCodes.Status200OK)]
    public Task<Pagination.Response<ServiceDetailData>> GetAllActiveServicesAsync([FromQuery] int page = 0, [FromQuery] int size = 15) =>
        _serviceBusinessLogic.GetAllActiveServicesAsync(page, size);

    /// <summary>
    /// Creates a new service offering.
    /// </summary>
    /// <param name="data">The data for the new service offering.</param>
    /// <remarks>Example: POST: /api/services/addservice</remarks>
    /// <response code="200">Returns the newly created service id.</response>
    /// <response code="400">The given service offering data were invalid.</response>
    [HttpPost]
    [Route("addservice")]
    [Authorize(Roles = "add_service_offering")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public Task<Guid> CreateServiceOffering([FromBody] ServiceOfferingData data) =>
        this.WithIamUserId(iamUserId => _serviceBusinessLogic.CreateServiceOffering(data, iamUserId));
    
    /// <summary>
    /// Adds a new service subscription.
    /// </summary>
    /// <param name="serviceId" example="D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645">Id for the service the user wants to subscribe to.</param>
    /// <remarks>Example: POST: /api/services/D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645/subscribe</remarks>
    /// <response code="204">Returns success</response>
    /// <response code="400">Company or company user wasn't assigned to the user.</response>
    /// <response code="404">No Service was found for the given id.</response>
    [HttpPost]
    [Route("{serviceId}/subscribe")]
    [Authorize(Roles = "subscribe_service")]
    [ProducesResponseType(typeof(NoContentResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<NoContentResult> AddServiceSubscription([FromRoute] Guid serviceId)
    {
        await this.WithIamUserId(iamUserId => _serviceBusinessLogic.AddServiceSubscription(serviceId, iamUserId)).ConfigureAwait(false);
        return this.NoContent();
    }

    /// <summary>
    /// Adds a new service subscription.
    /// </summary>
    /// <param name="serviceId" example="D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645">Id for the service the wants to retrieve.</param>
    /// <param name="lang" example="de">OPTIONAL: Short code for the language the translatable text should be returned in.</param>
    /// <remarks>Example: Get: /api/services/D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645</remarks>
    /// <response code="200">Returns the service details.</response>
    /// <response code="404">Service was not found.</response>
    [HttpGet]
    [Route("{serviceId}")]
    [Authorize(Roles = "view_service_offering")]
    [ProducesResponseType(typeof(ServiceDetailData), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public Task<ServiceDetailData> GetServiceDetails([FromRoute] Guid serviceId, [FromQuery] string lang = "en") => 
        _serviceBusinessLogic.GetServiceDetailsAsync(serviceId, lang);
}
