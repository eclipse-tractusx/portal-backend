/********************************************************************************
 * Copyright (c) 2023 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Web;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Web.Identity;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Controllers;

[ApiController]
[EnvironmentRoute("MVC_ROUTING_BASEPATH", "registration/[controller]")]
[Produces("application/json")]
[Consumes("application/json")]
public class NetworkController(INetworkBusinessLogic logic)
    : ControllerBase
{
    /// <summary>
    /// Registers a partner company
    /// </summary>
    /// <param name="data">Data for the registration</param>
    /// Example: POST: api/administration/registration/network/{externalId}/partnerRegistration
    /// <response code="200">Empty response on success.</response>
    [HttpPost]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Authorize(Roles = "configure_partner_registration")]
    [Route("partnerRegistration")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<OkResult> PartnerRegister([FromBody] PartnerRegistrationData data)
    {
        await logic.HandlePartnerRegistration(data).ConfigureAwait(ConfigureAwaitOptions.None);
        return Ok();
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
        await logic.RetriggerProcessStep(externalId, ProcessStepTypeId.RETRIGGER_SYNCHRONIZE_USER).ConfigureAwait(ConfigureAwaitOptions.None);
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
        await logic.RetriggerProcessStep(externalId, ProcessStepTypeId.RETRIGGER_CALLBACK_OSP_APPROVED).ConfigureAwait(ConfigureAwaitOptions.None);
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
        await logic.RetriggerProcessStep(externalId, ProcessStepTypeId.RETRIGGER_CALLBACK_OSP_DECLINED).ConfigureAwait(ConfigureAwaitOptions.None);
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
        await logic.RetriggerProcessStep(externalId, ProcessStepTypeId.RETRIGGER_CALLBACK_OSP_SUBMITTED).ConfigureAwait(ConfigureAwaitOptions.None);
        return NoContent();
    }

    /// <summary>
    /// Retriggers the last failed step
    /// </summary>
    /// <param name="externalId" example="">Id of the externalId that should be triggered</param>
    /// <returns>NoContent</returns>
    /// Example: POST: api/administration/registration/network/{externalId}/retrigger-remove-keycloak-user
    /// <response code="204">Empty response on success.</response>
    /// <response code="404">No registration found for the externalId.</response>
    [HttpPost]
    [Authorize(Roles = "approve_new_partner")]
    [Authorize(Policy = PolicyTypes.CompanyUser)]
    [Route("{externalId}/retrigger-remove-keycloak-user")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<NoContentResult> RetriggerRemoveKeycloakUser([FromRoute] string externalId)
    {
        await logic.RetriggerProcessStep(externalId, ProcessStepTypeId.RETRIGGER_REMOVE_KEYCLOAK_USERS).ConfigureAwait(ConfigureAwaitOptions.None);
        return NoContent();
    }

    /// <summary>
    /// Get OSP Company Application Detail by Company Name or Status
    /// </summary>
    /// <param name="page">page index start from 0</param>
    /// <param name="size">size to get number of records</param>
    /// <param name="companyApplicationStatusFilter">Search by company applicationstatus</param>
    /// <param name="companyName">search by company name</param>
    /// <param name="externalId">search by external Id</param>
    /// <param name="dateCreatedOrderFilter">sort result by dateCreated ascending or descending</param>
    /// <returns>OSp Company Application Details</returns>
    /// <remarks>
    /// Example: GET: api/administration/registration/network/companies?companyName=Car&amp;page=0&amp;size=4&amp;companyApplicationStatus=Closed <br />
    /// Example: GET: api/administration/registration/network/companies?page=0&amp;size=4
    /// </remarks>
    /// <response code="200">Result as a OSP Company Application Details</response>
    [HttpGet]
    [Authorize(Roles = "configure_partner_registration")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("companies")]
    [ProducesResponseType(typeof(Pagination.Response<CompanyDetailsOspOnboarding>), StatusCodes.Status200OK)]
    public Task<Pagination.Response<CompanyDetailsOspOnboarding>> GetOspCompanyDetailsAsync([FromQuery] int page, [FromQuery] int size, [FromQuery] CompanyApplicationStatusFilter? companyApplicationStatusFilter = null, [FromQuery] string? companyName = null, [FromQuery] string? externalId = null, [FromQuery] DateCreatedOrderFilter? dateCreatedOrderFilter = null) =>
        logic.GetOspCompanyDetailsAsync(page, size, companyApplicationStatusFilter, companyName, externalId, dateCreatedOrderFilter);
}
