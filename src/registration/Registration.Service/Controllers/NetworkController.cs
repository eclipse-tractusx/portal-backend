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
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Service;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Model;
using Org.Eclipse.TractusX.Portal.Backend.Web.Identity;

namespace Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Controllers;

[ApiController]
[Route("api/registration/[controller]")]
[Produces("application/json")]
[Consumes("application/json")]
public class NetworkController : ControllerBase
{
    private readonly INetworkBusinessLogic _logic;

    /// <summary>
    /// Creates a new instance of <see cref="NetworkController"/>
    /// </summary>
    /// <param name="logic">The business logic for the registration</param>
    public NetworkController(INetworkBusinessLogic logic)
    {
        _logic = logic;
    }

    /// <summary>
    /// Submits the application
    /// </summary>
    /// <param name="data">The agreements for the companyRoles</param>
    /// <returns>NoContent</returns>
    /// Example: POST: api/registration/network/partnerRegistration/submit
    /// <response code="204">Empty response on success.</response>
    /// <response code="404">No registration found for the externalId.</response>
    [HttpPost]
    [Authorize(Roles = "submit_registration")]
    [Authorize(Policy = PolicyTypes.CompanyUser)]
    [Route("partnerRegistration/submit")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<NoContentResult> Submit([FromBody] PartnerSubmitData data)
    {
        await _logic.Submit(data).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>
    /// Declines the osp registration
    /// </summary>
    /// <param name="applicationId">Id of the company application</param>
    /// <param name="declineData">Data with the information to decline</param>
    /// Example: POST: api/registration/network/{externalId}/decline
    /// <response code="200">Empty response on success.</response>
    /// <response code="404">No registration found for the externalId.</response>
    [HttpPost]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Authorize(Roles = "decline_partner_registration")]
    [Route("decline")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<OkResult> DeclineOsp([FromRoute] Guid applicationId, [FromBody] DeclineOspData declineData)
    {
        await _logic.DeclineOsp(applicationId, declineData).ConfigureAwait(false);
        return this.Ok();
    }
}
