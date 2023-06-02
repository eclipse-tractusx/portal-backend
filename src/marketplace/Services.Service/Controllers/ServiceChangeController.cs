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
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Library;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Authentication;
using Org.Eclipse.TractusX.Portal.Backend.Services.Service.BusinessLogic;

namespace Org.Eclipse.TractusX.Portal.Backend.Services.Service.Controllers;

/// <summary>
/// Controller providing actions for updating applications.
/// </summary>

[Route("api/service/[controller]")]
[ApiController]
[Produces("application/json")]
[Consumes("application/json")]
public class ServiceChangeController : ControllerBase
{
    private readonly IServiceChangeBusinessLogic _serviceChangeBusinessLogic;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="serviceChangeBusinessLogic">Logic dependency.</param>
    public ServiceChangeController(IServiceChangeBusinessLogic serviceChangeBusinessLogic)
    {
        _serviceChangeBusinessLogic = serviceChangeBusinessLogic;
    }

    /// <summary>
    /// Deactivate the OfferStatus By serviceId
    /// </summary>
    /// <param name="serviceId" example="3c77a395-a7e7-40f2-a519-ac16498e0a79">Id of the service that should be deactive</param>
    /// <remarks>Example: PUT: /api/service/servicechanges/3c77a395-a7e7-40f2-a519-ac16498e0a79/deactivateService</remarks>
    /// <response code="204">The Service Successfully Deactivated</response>
    /// <response code="400">invalid or user does not exist.</response>
    /// <response code="404">If service does not exists.</response>
    /// <response code="403">Missing Permission</response>
    /// <response code="409">Offer is in incorrect state</response>
    [HttpPut]
    [Route("{serviceId:guid}/deactivateService")]
    [Authorize(Roles = "update_service_offering")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<NoContentResult> DeactivateService([FromRoute] Guid serviceId)
    {
        await this.WithCompanyId(companyId => _serviceChangeBusinessLogic.DeactivateOfferByServiceIdAsync(serviceId, companyId)).ConfigureAwait(false);
        return NoContent();
    }
}
