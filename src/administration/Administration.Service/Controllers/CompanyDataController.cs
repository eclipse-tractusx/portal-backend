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
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Authentication;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using System.Net;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Controllers;

/// <summary>
/// Creates a new instance of <see cref="CompanyDataController"/>
/// </summary>
[ApiController]
[Route("api/administration/companydata")]
[Produces("application/json")]
[Consumes("application/json")]
public class CompanyDataController : ControllerBase
{
    private readonly ICompanyDataBusinessLogic _logic;

    /// <summary>
    /// Creates a new instance of <see cref="CompanyDataController"/>
    /// </summary>
    /// <param name="logic">The company data business logic</param>
    public CompanyDataController(ICompanyDataBusinessLogic logic)
    {
        _logic = logic;
    }

    /// <summary>
    /// Gets the company with its address
    /// </summary>
    /// <returns>the company with its address</returns>
    /// <remarks>Example: GET: api/administration/companydata/ownCompanyDetails</remarks>
    /// <response code="200">Returns the company with its address.</response>
    /// <response code="409">user is not associated with  company.</response>
    [HttpGet]
    [Authorize(Roles = "view_company_data")]
    [Route("ownCompanyDetails")]
    [ProducesResponseType(typeof(CompanyAddressDetailData), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public Task<CompanyAddressDetailData> GetOwnCompanyDetailsAsync() =>
        this.WithIamUserId(iamUserId => _logic.GetOwnCompanyDetailsAsync(iamUserId));

    /// <summary>
    /// Gets the CompanyAssigned UseCase details
    /// </summary>
    /// <returns>the CompanyAssigned UseCase details</returns>
    /// <remarks>Example: GET: api/administration/companydata/preferredUseCases</remarks>
    /// <response code="200">Returns the CompanyAssigned UseCase details.</response>
    [HttpGet]
    [Authorize(Roles = "view_use_cases")]
    [Route("preferredUseCases")]
    [ProducesResponseType(typeof(CompanyAssignedUseCaseData), StatusCodes.Status200OK)]
    public IAsyncEnumerable<CompanyAssignedUseCaseData> GetCompanyAssigendUseCaseDetailsAsync() =>
       this.WithIamUserId(iamUserId => _logic.GetCompanyAssigendUseCaseDetailsAsync(iamUserId));

    /// <summary>
    /// Create the CompanyAssigned UseCase details
    /// </summary>
    /// <remarks>Example: POST: api/administration/companydata/preferredUseCases</remarks>
    /// <response code="204">NoContentResult</response>
    /// <response code="208">UseCaseId already existis</response>
    /// <response code="409">Company Status is Incorrect</response>
    [HttpPost]
    [Authorize(Roles = "set_company_use_cases")]
    [Route("preferredUseCases")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<StatusCodeResult> CreateCompanyAssignedUseCaseDetailsAsync([FromBody] UseCaseIdDetails data) =>
        await this.WithIamUserId(iamUserId => _logic.CreateCompanyAssignedUseCaseDetailsAsync(iamUserId, data.useCaseId)).ConfigureAwait(false)
            ? StatusCode((int)HttpStatusCode.Created)
            : NoContent();

    /// <summary>
    /// Remove the CompanyAssigned UseCase details by UseCaseId
    /// </summary>
    /// <remarks>Example: DELETE: api/administration/companydata/preferredUseCases</remarks>
    /// <response code="204">NoContentResult</response>
    /// <response code="409">Company Status is Incorrect</response>
    /// <response code="409">UseCaseId is not available</response>
    [HttpDelete]
    [Authorize(Roles = "set_company_use_cases")]
    [Route("preferredUseCases")]
    [ProducesResponseType(typeof(NoContentResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<NoContentResult> RemoveCompanyAssignedUseCaseDetailsAsync([FromBody] UseCaseIdDetails data)
    {
        await this.WithIamUserId(iamUserId => _logic.RemoveCompanyAssignedUseCaseDetailsAsync(iamUserId, data.useCaseId)).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>
    /// Gets the companyrole and ConsentAgreement Details
    /// </summary>
    /// <returns>the Companyrole and ConsentAgreement details</returns>
    /// <remarks>Example: GET: api/administration/companydata/companyRolesAndConsents</remarks>
    /// <response code="200">Returns the Companyrole and Consent details.</response>
    /// <response code="409">No Companyrole or Incorrect Status</response>
    [HttpGet]
    [Authorize(Roles = "view_company_data")]
    [Route("companyRolesAndConsents")]
    [ProducesResponseType(typeof(CompanyRoleConsentViewData), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public IAsyncEnumerable<CompanyRoleConsentViewData> GetCompanyRoleAndConsentAgreementDetailsAsync([FromQuery] string? languageShortName = null) =>
        this.WithIamUserId(iamUserId => _logic.GetCompanyRoleAndConsentAgreementDetailsAsync(iamUserId, languageShortName));

    /// <summary>
    /// Post the companyrole and Consent Details
    /// </summary>
    /// <returns>Create Companyrole and Consent details</returns>
    /// <remarks>Example: POST: api/administration/companydata/companyRolesAndConsents</remarks>
    /// <response code="204">Created the Companyrole and Consent details.</response>
    /// <response code="409">companyRole already exists</response>
    /// <response code="409">All agreement need to get signed</response>
    [HttpPost]
    [Authorize(Roles = "view_company_data")]
    [Route("companyRolesAndConsents")]
    [ProducesResponseType(typeof(NoContentResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<NoContentResult> CreateCompanyRoleAndConsentAgreementDetailsAsync([FromBody] IEnumerable<CompanyRoleConsentDetails> companyRoleConsentDetails)
    {
        await this.WithIamUserId(iamUserId => _logic.CreateCompanyRoleAndConsentAgreementDetailsAsync(iamUserId, companyRoleConsentDetails));
        return NoContent();
    }
}
