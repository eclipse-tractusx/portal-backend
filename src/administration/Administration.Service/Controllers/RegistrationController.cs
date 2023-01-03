/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Authentication;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

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
    [ProducesResponseType(typeof(CompanyWithAddress), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public Task<CompanyWithAddress> GetCompanyWithAddressAsync([FromRoute] Guid applicationId) =>
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
    public Task<Pagination.Response<CompanyApplicationDetails>> GetApplicationDetailsAsync([FromQuery]int page, [FromQuery]int size,[FromQuery] CompanyApplicationStatusFilter? companyApplicationStatusFilter = null, [FromQuery]string? companyName = null) =>
        _logic.GetCompanyApplicationDetailsAsync(page, size,companyApplicationStatusFilter, companyName);

    /// <summary>
    /// Approves the partner request
    /// </summary>
    /// <param name="applicationId" example="4f0146c6-32aa-4bb1-b844-df7e8babdcb4">Id of the application that should be approved</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>the result as a boolean</returns>
    /// Example: PUT: api/administration/registration/application/4f0146c6-32aa-4bb1-b844-df7e8babdcb4/approveRequest
    /// <response code="200">the result as a boolean.</response>
    /// <response code="400">Either the CompanyApplication is not in status SUBMITTED, the BusinessPartnerNumber (bpn) for the given CompanyApplications company is empty or no applicationId was set.</response>
    /// <response code="404">Application ID not found.</response>
    /// <response code="500">Internal Server Error.</response>
    /// <response code="502">Bad Gateway Service Error.</response>
    [HttpPut]
    [Authorize(Roles = "approve_new_partner")]
    [Route("application/{applicationId}/approveRequest")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status502BadGateway)]
    public Task<bool> ApprovePartnerRequest([FromRoute] Guid applicationId, CancellationToken cancellationToken) =>
        this.WithIamUserAndBearerToken((auth) => _logic.ApprovePartnerRequest(auth.iamUserId, auth.bearerToken, applicationId, cancellationToken));

    /// <summary>
    /// Decline the Partner Registration Request
    /// </summary>
    /// <param name="applicationId" example="31404026-64ee-4023-a122-3c7fc40e57b1">Company Application Id for which request will be declined</param>
    /// <returns>Result as a boolean</returns>
    /// <remarks>Example: PUT: api/administration/registration/application/31404026-64ee-4023-a122-3c7fc40e57b1/declineRequest</remarks>
    /// <response code="200">Result as a boolean</response>
    /// <response code="400">Either the Company Application was not in Submitted State, the Username has no assigned emailid or no applicationId was set.</response>
    [HttpPut]
    [Authorize(Roles = "decline_new_partner")]
    [Route("application/{applicationId}/declineRequest")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public Task<bool> DeclinePartnerRequest([FromRoute] Guid applicationId) =>
            _logic.DeclinePartnerRequest(applicationId);

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
    /// Approves the partner request
    /// </summary>
    /// <param name="applicationId" example="4f0146c6-32aa-4bb1-b844-df7e8babdcb4">Id of the application that should be approved</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>the result as a boolean</returns>
    /// Example: PUT: api/administration/registration/application/4f0146c6-32aa-4bb1-b844-df7e8babdcb4/approveRequest
    /// <response code="200">the result as a boolean.</response>
    /// <response code="400">Either the CompanyApplication is not in status SUBMITTED, the BusinessPartnerNumber (bpn) for the given CompanyApplications company is empty or no applicationId was set.</response>
    /// <response code="404">Application ID not found.</response>
    /// <response code="500">Internal Server Error.</response>
    /// <response code="502">Bad Gateway Service Error.</response>
    [HttpPut]
    [Authorize(Roles = "approve_new_partner")]
    [Route("application/{applicationId}/trigger-bpn")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status502BadGateway)]
    public async Task<NoContentResult> TriggerBpnDataPush([FromRoute] Guid applicationId, CancellationToken cancellationToken)
    {
        await this.WithIamUserId(user => _logic.TriggerBpnDataPushAsync(user, applicationId, cancellationToken)).ConfigureAwait(false);
        return NoContent();
    }
}
