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
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Web;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Web;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Web.Identity;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Controllers;

/// <summary>
/// Controller providing actions execute invitation
/// </summary>
[ApiController]
[EnvironmentRoute("MVC_ROUTING_BASEPATH", "invitation")]
[Produces("application/json")]
[Consumes("application/json")]
public class InvitationController : ControllerBase
{
    private readonly IInvitationBusinessLogic _logic;

    /// <summary>
    /// Creates a new instance of <see cref="InvitationController"/>
    /// </summary>
    /// <param name="logic">The invitation business logic</param>
    public InvitationController(IInvitationBusinessLogic logic)
    {
        _logic = logic;
    }

    /// <summary>
    /// Executes the invitation
    /// </summary>
    /// <param name="invitationData"></param>
    /// <returns></returns>
    /// <remarks>
    /// Example: POST: api/administration/invitation
    /// </remarks>
    /// <response code="200">Successfully executed the invitation.</response>
    /// <response code="400">Missing mandatory input values (e.g. email, organization name, etc.)</response>
    /// <response code="500">Internal Server Error.</response>
    /// <response code="502">Bad Gateway Service Error.</response>
    /// <response code="409">user is not associated with  company.</response>
    [HttpPost]
    [Authorize(Roles = "invite_new_partner")]
    [Authorize(Policy = PolicyTypes.CompanyUser)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status502BadGateway)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public Task ExecuteInvitation([FromBody] CompanyInvitationData invitationData) =>
        _logic.ExecuteInvitation(invitationData);

    /// <summary>
    /// Retriggers the last failed step
    /// </summary>
    /// <param name="processId" example="251e4596-5ff0-4176-b544-840b04ebeb93">Id of the process that should be triggered</param>
    /// <returns>NoContent</returns>
    /// Example: POST: api/administration/invitation/{processId}/retrigger-setup-idp
    /// <response code="204">Empty response on success.</response>
    /// <response code="404">No registration found for the externalId.</response>
    [HttpPost]
    [Authorize(Roles = "invite_new_partner")]
    [Authorize(Policy = PolicyTypes.CompanyUser)]
    [Route("{processId}/retrigger-setup-idp")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<NoContentResult> RetriggerSetupIdp([FromRoute] Guid processId)
    {
        await _logic.RetriggerSetupIdp(processId).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>
    /// Retriggers the last failed step
    /// </summary>
    /// <param name="processId" example="251e4596-5ff0-4176-b544-840b04ebeb93">Id of the process that should be triggered</param>
    /// <returns>NoContent</returns>
    /// Example: POST: api/administration/invitation/{processId}/retrigger-create-database-idp
    /// <response code="204">Empty response on success.</response>
    /// <response code="404">No registration found for the externalId.</response>
    [HttpPost]
    [Authorize(Roles = "invite_new_partner")]
    [Authorize(Policy = PolicyTypes.CompanyUser)]
    [Route("{processId}/retrigger-create-database-idp")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<NoContentResult> RetriggerCreateDatabaseIdp([FromRoute] Guid processId)
    {
        await _logic.RetriggerCreateDatabaseIdp(processId).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>
    /// Retriggers the last failed step
    /// </summary>
    /// <param name="processId" example="251e4596-5ff0-4176-b544-840b04ebeb93">Id of the process that should be triggered</param>
    /// <returns>NoContent</returns>
    /// Example: POST: api/administration/invitation/{processId}/retrigger-create-user
    /// <response code="204">Empty response on success.</response>
    /// <response code="404">No registration found for the externalId.</response>
    [HttpPost]
    [Authorize(Roles = "invite_new_partner")]
    [Authorize(Policy = PolicyTypes.CompanyUser)]
    [Route("{processId}/retrigger-create-user")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<NoContentResult> RetriggerInvitationCreateUser([FromRoute] Guid processId)
    {
        await _logic.RetriggerInvitationCreateUser(processId).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>
    /// Retriggers the last failed step
    /// </summary>
    /// <param name="processId" example="251e4596-5ff0-4176-b544-840b04ebeb93">Id of the process that should be triggered</param>
    /// <returns>NoContent</returns>
    /// Example: POST: api/administration/invitation/{processId}/retrigger-send-mail
    /// <response code="204">Empty response on success.</response>
    /// <response code="404">No registration found for the externalId.</response>
    [HttpPost]
    [Authorize(Roles = "invite_new_partner")]
    [Authorize(Policy = PolicyTypes.CompanyUser)]
    [Route("{processId}/retrigger-send-mail")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<NoContentResult> RetriggerInvitationSendMail([FromRoute] Guid processId)
    {
        await _logic.RetriggerInvitationSendMail(processId).ConfigureAwait(false);
        return NoContent();
    }
}
