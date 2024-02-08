/********************************************************************************
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
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Service;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Web.Identity;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Controllers;

[ApiController]
[Route("api/administration/registration/[controller]")]
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
    /// Registers a partner company
    /// </summary>
    /// <param name="data">Data for the registration</param>
    /// Example: POST: api/administration/registration/network/{externalId}/retrigger-synchronize-users
    /// <response code="200">Empty response on success.</response>
    [HttpPost]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Authorize(Roles = "create_partner_registration")]
    [Route("partnerRegistration")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<OkResult> PartnerRegister([FromBody] PartnerRegistrationData data)
    {
        await _logic.HandlePartnerRegistration(data).ConfigureAwait(false);
        return this.Ok();
    }

    /// <summary>
    /// Retriggers the last failed step
    /// </summary>
    /// <param name="externalId" example="">Id of the externalId that should be triggered</param>
    /// <returns>NoContent</returns>
    /// Example: POST: api/administration/registration/network/{externalId}/retrigger-synchronize-users
    /// <response code="204">Empty response on success.</response>
    /// <response code="404">No registration found for the externalId.</response>
    [HttpPost]
    [Authorize(Roles = "approve_new_partner")]
    [Authorize(Policy = PolicyTypes.CompanyUser)]
    [Route("{externalId}/retrigger-synchronize-users")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<NoContentResult> RetriggerSynchronizeUser([FromRoute] string externalId)
    {
        await _logic.RetriggerProcessStep(externalId, ProcessStepTypeId.RETRIGGER_SYNCHRONIZE_USER).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>
    /// Retriggers the last failed step
    /// </summary>
    /// <param name="externalId" example="">Id of the externalId that should be triggered</param>
    /// <returns>NoContent</returns>
    /// Example: POST: api/administration/registration/network/{externalId}/retrigger-callback-osp-approve
    /// <response code="204">Empty response on success.</response>
    /// <response code="404">No registration found for the externalId.</response>
    [HttpPost]
    [Authorize(Roles = "approve_new_partner")]
    [Authorize(Policy = PolicyTypes.CompanyUser)]
    [Route("{externalId}/retrigger-callback-osp-approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<NoContentResult> RetriggerCallbackOspApprove([FromRoute] string externalId)
    {
        await _logic.RetriggerProcessStep(externalId, ProcessStepTypeId.RETRIGGER_CALLBACK_OSP_APPROVED).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>
    /// Retriggers the last failed step
    /// </summary>
    /// <param name="externalId" example="">Id of the externalId that should be triggered</param>
    /// <returns>NoContent</returns>
    /// Example: POST: api/administration/registration/network/{externalId}/retrigger-callback-osp-decline
    /// <response code="204">Empty response on success.</response>
    /// <response code="404">No registration found for the externalId.</response>
    [HttpPost]
    [Authorize(Roles = "approve_new_partner")]
    [Authorize(Policy = PolicyTypes.CompanyUser)]
    [Route("{externalId}/retrigger-callback-osp-decline")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<NoContentResult> RetriggerCallbackOspDecline([FromRoute] string externalId)
    {
        await _logic.RetriggerProcessStep(externalId, ProcessStepTypeId.RETRIGGER_CALLBACK_OSP_DECLINED).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>
    /// Retriggers the last failed step
    /// </summary>
    /// <param name="externalId" example="">Id of the externalId that should be triggered</param>
    /// <returns>NoContent</returns>
    /// Example: POST: api/administration/registration/network/{externalId}/retrigger-callback-osp-submitted
    /// <response code="204">Empty response on success.</response>
    /// <response code="404">No registration found for the externalId.</response>
    [HttpPost]
    [Authorize(Roles = "approve_new_partner")]
    [Authorize(Policy = PolicyTypes.CompanyUser)]
    [Route("{externalId}/retrigger-callback-osp-submitted")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<NoContentResult> RetriggerCallbackOspSubmitted([FromRoute] string externalId)
    {
        await _logic.RetriggerProcessStep(externalId, ProcessStepTypeId.RETRIGGER_CALLBACK_OSP_SUBMITTED).ConfigureAwait(false);
        return NoContent();
    }
}
