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
using Org.Eclipse.TractusX.Portal.Backend.Clearinghouse.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Controllers;

[ApiController]
[Route("api/administration/registration")]
[Produces("application/json")]
[Consumes("application/json")]
public class RegistrationController : ControllerBase
{
    private readonly IRegistrationBusinessLogic _logic;
    
    /// <summary>
    /// Creates a new instance of <see cref="RegistrationController"/>
    /// </summary>
    /// <param name="logic">The business logic for the registration</param>
    public RegistrationController(IRegistrationBusinessLogic logic)
    {
        _logic = logic;
    }

    /// <summary>
    /// Gets the company with its address
    /// </summary>
    /// <param name="applicationId" example="4f0146c6-32aa-4bb1-b844-df7e8babdcb4"></param>
    /// <returns>the company with its address</returns>
    /// <remarks>Example: GET: api/administration/registration/application/4f0146c6-32aa-4bb1-b844-df7e8babdcb4/companyDetailsWithAddress</remarks>
    /// <response code="200">Returns the company with its address.</response>
    /// <response code="400">No applicationId was set.</response>
    /// <response code="404">Application ID not found.</response>
    [HttpGet]
    [Authorize(Roles = "view_submitted_applications")]
    [Route("application/{applicationId}/companyDetailsWithAddress")]
    [ProducesResponseType(typeof(CompanyWithAddressData), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public Task<CompanyWithAddressData> GetCompanyWithAddressAsync([FromRoute] Guid applicationId) =>
        _logic.GetCompanyWithAddressAsync(applicationId);
    
    /// <summary>
    /// Get Application Detail by Company Name or Status
    /// </summary>
    /// <param name="page">page index start from 0</param>
    /// <param name="size">size to get number of records</param>
    /// <param name="companyApplicationStatusFilter">Search by company applicationstatus</param>
    /// <param name="companyName">search by company name</param>
    /// <returns>Company Application Details</returns>
    /// <remarks>
    /// Example: GET: api/administration/registration/applications?companyName=Car&amp;page=0&amp;size=4&amp;companyApplicationStatus=Closed <br />
    /// Example: GET: api/administration/registration/applications?page=0&amp;size=4
    /// </remarks>
    /// <response code="200">Result as a Company Application Details</response>
    [HttpGet]
    [Authorize(Roles = "view_submitted_applications")]
    [Route("applications")]
    [ProducesResponseType(typeof(Pagination.Response<CompanyApplicationDetails>), StatusCodes.Status200OK)]
    public Task<Pagination.Response<CompanyApplicationDetails>> GetApplicationDetailsAsync([FromQuery] int page, [FromQuery] int size, [FromQuery] CompanyApplicationStatusFilter? companyApplicationStatusFilter = null, [FromQuery] string? companyName = null) =>
        _logic.GetCompanyApplicationDetailsAsync(page, size,companyApplicationStatusFilter, companyName);

    /// <summary>
    /// fetch all applications details with company user details.
    /// </summary>
    /// <param name="page">Optional query parameter defining the page index start from 0</param>
    /// <param name="size">Optional query parameter defining the size to get number of records</param>
    /// <param name="companyName">Optional query parameter defining the company name to get number of records as per company name</param>
    /// <returns>All Company Applications Details along with user details</returns>
    /// <remarks>Example: GET: api/administration/registration/applicationsWithStatus?page=0&amp;size=15</remarks>
    /// <response code="200">Result as a All Company Applications Details</response>
    [HttpGet]
    [Authorize(Roles = "invite_new_partner")]
    [Route("applicationsWithStatus")]
    [ProducesResponseType(typeof(Pagination.Response<CompanyApplicationWithCompanyUserDetails>), StatusCodes.Status200OK)]
    public Task<Pagination.Response<CompanyApplicationWithCompanyUserDetails>> GetAllCompanyApplicationsDetailsAsync([FromQuery] int page = 0, [FromQuery] int size = 15, [FromQuery] string? companyName = null) =>
        _logic.GetAllCompanyApplicationsDetailsAsync(page, size, companyName);

    /// <summary>
    /// Update the BPN for a Company
    /// </summary>
    /// <param name="applicationId"></param>
    /// <param name="bpn"></param>
    /// <returns></returns>
    /// <remarks>Example: POST: /api/administration/registration/application/6126fdc8-f572-4a63-a5b3-d5e52cb58136/BPNL00000003CSGV/bpn</remarks>
    /// <response code="200">Successfully Updated the BPN.</response>
    /// <response code="404">application Id not found</response>
    /// <response code="400">Bpn is not 16 character long or alphanumeric or does not start with BPNL</response>
    /// <response code="409">Bpn is already assigned or alphanumeric or application for company is not pending</response>
    [HttpPost]
    [Route("application/{applicationId}/{bpn}/bpn")]
    [Authorize(Roles = "approve_new_partner")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public Task UpdateCompanyBpn([FromRoute] Guid applicationId, [FromRoute] string bpn) =>
        _logic.UpdateCompanyBpn(applicationId, bpn);

    /// <summary>
    /// Approves the registration verification for the application with the given id
    /// </summary>
    /// <param name="applicationId">Id of the application that should be approved</param>
    /// <remarks>
    /// Example: POST: api/administration/registration/application/4f0146c6-32aa-4bb1-b844-df7e8babdcb4/approve
    /// </remarks>
    /// <response code="204">Successfully approved the application</response>
    /// <response code="400">Either the CompanyApplication is not in status SUBMITTED, or there is no checklist entry of type Registration_Verification.</response>
    /// <response code="404">Application ID not found.</response>
    [HttpPost]
    [Authorize(Roles = "approve_new_partner")]
    [Route("applications/{applicationId}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<NoContentResult> ApproveApplication([FromRoute] Guid applicationId)
    {
        await _logic.ApproveRegistrationVerification(applicationId).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>
    /// Declines the registration verification for the application with the given id
    /// </summary>
    /// <param name="applicationId">Id of the application that should be declined</param>
    /// <param name="data">Comment to explain why the application got declined</param>
    /// <remarks>
    /// Example: POST: api/administration/registration/application/4f0146c6-32aa-4bb1-b844-df7e8babdcb4/decline
    /// </remarks>
    /// <response code="204">Successfully declined the application</response>
    /// <response code="400">Either the CompanyApplication is not in status SUBMITTED, or there is no checklist entry of type Registration_Verification.</response>
    /// <response code="404">Application ID not found.</response>
    [HttpPost]
    // [Authorize(Roles = "decline_new_partner")]
    [Route("applications/{applicationId}/decline")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<NoContentResult> DeclineApplication([FromRoute] Guid applicationId, [FromBody] RegistrationDeclineData data)
    {
        await _logic.DeclineRegistrationVerification(applicationId, data.Comment).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>
    /// Processes the clearinghouse response
    /// </summary>
    /// <param name="responseData">Response data from clearinghouse</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>NoContent</returns>
    /// Example: POST: api/administration/registration/clearinghouse
    /// <response code="204">Empty response on success.</response>
    /// <response code="400">Either the CompanyApplication is not in status SUBMITTED or the clearing_house process is not in status IN_PROGRESS.</response>
    /// <response code="404">No application found for the bpn.</response>
    [HttpPost]
    [Authorize(Roles = "update_application_checklist_value")]
    [Route("clearinghouse")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<NoContentResult> ProcessClearinghouseResponse([FromBody] ClearinghouseResponseData responseData, CancellationToken cancellationToken)
    {
        await _logic.ProcessClearinghouseResponseAsync(responseData, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }
    
    /// <summary>
    /// Gets the information of an applications checklist
    /// </summary>
    /// <param name="applicationId">Id of the application the checklist should be provided for</param>
    /// <remarks>
    /// Example: GET: api/administration/registration/application/4f0146c6-32aa-4bb1-b844-df7e8babdcb4/checklistDetails
    /// </remarks>
    /// <response code="200">The checklist information for the application</response>
    /// <response code="404">Application ID not found.</response>
    [HttpGet]
    [Authorize(Roles = "approve_new_partner")]
    [Route("applications/{applicationId}/checklistDetails")]
    [ProducesResponseType(typeof(ChecklistDetails), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public Task<IEnumerable<ChecklistDetails>> GetChecklistForApplication([FromRoute] Guid applicationId) =>
        _logic.GetChecklistForApplicationAsync(applicationId);
    
    /// <summary>
    /// Retriggers the last failed to override the clearinghouse-result
    /// </summary>
    /// <param name="applicationId" example="">Id of the application that should be triggered</param>
    /// <returns>NoContent</returns>
    /// Example: POST: api/administration/registration/application/4f0146c6-32aa-4bb1-b844-df7e8babdcb4/override-clearinghouse
    /// <response code="204">Empty response on success.</response>
    /// <response code="400">Either the CompanyApplication is not in status SUBMITTED or the next step can't automatically retriggered.</response>
    /// <response code="404">No application found for the applicationId.</response>
    [HttpPost]
    [Authorize(Roles = "approve_new_partner")]
    [Route("application/{applicationId}/override-clearinghouse")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<NoContentResult> OverrideClearinghouseChecklist([FromRoute] Guid applicationId)
    {
        await _logic.TriggerChecklistAsync(applicationId, ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ProcessStepTypeId.TRIGGER_OVERRIDE_CLEARING_HOUSE).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>
    /// Retriggers the last failed step 
    /// </summary>
    /// <param name="applicationId" example="">Id of the application that should be triggered</param>
    /// <returns>NoContent</returns>
    /// Example: POST: api/administration/registration/application/4f0146c6-32aa-4bb1-b844-df7e8babdcb4/retrigger-clearinghouse
    /// <response code="204">Empty response on success.</response>
    /// <response code="400">Either the CompanyApplication is not in status SUBMITTED or the next step can't automatically retriggered.</response>
    /// <response code="404">No application found for the applicationId.</response>
    [HttpPost]
    [Authorize(Roles = "approve_new_partner")]
    [Route("application/{applicationId}/retrigger-clearinghouse")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<NoContentResult> RetriggerClearinghouseChecklist([FromRoute] Guid applicationId)
    {
        await _logic.TriggerChecklistAsync(applicationId, ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ProcessStepTypeId.RETRIGGER_CLEARING_HOUSE).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>
    /// Retriggers the last failed step 
    /// </summary>
    /// <param name="applicationId" example="">Id of the application that should be triggered</param>
    /// <returns>NoContent</returns>
    /// Example: POST: api/administration/registration/application/4f0146c6-32aa-4bb1-b844-df7e8babdcb4/trigger-identity-wallet<br />
    /// <response code="204">Empty response on success.</response>
    /// <response code="400">Either the CompanyApplication is not in status SUBMITTED or the next step can't automatically retriggered.</response>
    /// <response code="404">No application found for the applicationId.</response>
    [HttpPost]
    [Authorize(Roles = "approve_new_partner")]
    [Route("application/{applicationId}/trigger-identity-wallet")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<NoContentResult> TriggerIdentityWallet([FromRoute] Guid applicationId)
    {
        await _logic.TriggerChecklistAsync(applicationId, ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ProcessStepTypeId.RETRIGGER_IDENTITY_WALLET).ConfigureAwait(false);
        return NoContent();
    }
    
    /// <summary>
    /// Retriggers the last failed step 
    /// </summary>
    /// <param name="applicationId" example="">Id of the application that should be triggered</param>
    /// <returns>NoContent</returns>
    /// Example: POST: api/administration/registration/application/4f0146c6-32aa-4bb1-b844-df7e8babdcb4/trigger-self-description <br />
    /// <response code="204">Empty response on success.</response>
    /// <response code="400">Either the CompanyApplication is not in status SUBMITTED or the next step can't automatically retriggered.</response>
    /// <response code="404">No application found for the applicationId.</response>
    [HttpPost]
    [Authorize(Roles = "approve_new_partner")]
    [Route("application/{applicationId}/trigger-self-description")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<NoContentResult> TriggerSelfDescription([FromRoute] Guid applicationId)
    {
        await _logic.TriggerChecklistAsync(applicationId, ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP, ProcessStepTypeId.RETRIGGER_SELF_DESCRIPTION_LP).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>
    /// Retriggers the last failed step 
    /// </summary>
    /// <param name="applicationId" example="">Id of the application that should be triggered</param>
    /// <param name="processTypeId">Optional: The process type id that should be retriggered</param>
    /// <returns>NoContent</returns>
    /// Example: POST: api/administration/registration/application/4f0146c6-32aa-4bb1-b844-df7e8babdcb4/trigger-bpn <br />
    /// <response code="204">Empty response on success.</response>
    /// <response code="400">Either the CompanyApplication is not in status SUBMITTED or the next step can't automatically retriggered.</response>
    /// <response code="404">No application found for the applicationId.</response>
    [HttpPost]
    [Authorize(Roles = "approve_new_partner")]
    [Route("application/{applicationId}/trigger-bpn")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<NoContentResult> TriggerBpn([FromRoute] Guid applicationId, [FromQuery] ProcessStepTypeId processTypeId)
    {
        await _logic.TriggerChecklistAsync(applicationId, ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, processTypeId).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>
    /// Processes the clearinghouse self description push
    /// </summary>
    /// <param name="data">The response data for the self description</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// Example: POST: api/administration/registration/clearinghouse/selfDescription <br />
    /// <response code="204">Empty response on success.</response>
    /// <response code="400">The CompanyApplication is not in status SUBMITTED.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost]
    [Authorize(Roles = "update_application_checklist_value")]
    [Route("clearinghouse/selfDescription")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<NoContentResult> ProcessClearinghouseSelfDescription([FromBody] SelfDescriptionResponseData data, CancellationToken cancellationToken)
    {
        await _logic.ProcessClearinghouseSelfDescription(data, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }
    
    /// <summary>
    /// Retrieves a specific document for the given id.
    /// </summary>
    /// <param name="documentId" example="4ad087bb-80a1-49d3-9ba9-da0b175cd4e3">Id of the document to get.</param>
    /// <returns>Returns the file.</returns>
    /// <remarks>Example: GET: /api/administration/registration/documents/4ad087bb-80a1-49d3-9ba9-da0b175cd4e3</remarks>
    /// <response code="200">Returns the file.</response>
    /// <response code="404">Record not found.</response>
    [HttpGet]
    [Route("documents/{documentId}")]
    [Authorize(Roles = "approve_new_partner")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetDocumentContentFileAsync([FromRoute] Guid documentId)
    {
        var (fileName, content, contentType) = await _logic.GetDocumentAsync(documentId).ConfigureAwait(false);
        return File(content, contentType, fileName);
    }
}
