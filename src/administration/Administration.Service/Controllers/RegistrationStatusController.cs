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
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Web;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Web;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.Web.Identity;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Controllers;

[ApiController]
[EnvironmentRoute("MVC_ROUTING_BASEPATH", "[controller]")]
[Produces("application/json")]
[Consumes("application/json")]
public class RegistrationStatusController : ControllerBase
{
    private readonly IRegistrationStatusBusinessLogic _logic;

    /// <summary>
    /// Creates a new instance of <see cref="RegistrationStatusController"/>
    /// </summary>
    /// <param name="logic">The business logic for the registration</param>
    public RegistrationStatusController(IRegistrationStatusBusinessLogic logic)
    {
        _logic = logic;
    }

    /// <summary>
    /// Gets the callback address of the onboarding service provider
    /// </summary>
    /// <returns>the callback data of the osp</returns>
    /// <remarks>Example: GET: api/administration/registrationstatus/callback</remarks>
    /// <response code="200">Returns the company with its address.</response>
    /// <response code="400">Company is no onboarding service provider.</response>
    [HttpGet]
    [Authorize(Roles = "configure_partner_registration")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("callback")]
    [ProducesResponseType(typeof(OnboardingServiceProviderCallbackRequestData), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public Task<OnboardingServiceProviderCallbackResponseData> GetCallbackAddress() =>
        _logic.GetCallbackAddress();

    /// <summary>
    /// Sets the callback address of the onboarding service provider
    /// </summary>
    /// <returns>NoContent</returns>
    /// <remarks>Example: POST: api/administration/registrationstatus/callback</remarks>
    /// <response code="204">Returns no content.</response>
    [HttpPost]
    [Authorize(Roles = "configure_partner_registration")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("callback")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<NoContentResult> SetCallbackAddress(OnboardingServiceProviderCallbackRequestData requestData)
    {
        await _logic.SetCallbackAddress(requestData).ConfigureAwait(false);
        return NoContent();
    }
}
