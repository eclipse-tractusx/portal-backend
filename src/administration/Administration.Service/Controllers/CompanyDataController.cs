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
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Library;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Authentication;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
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
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("ownCompanyDetails")]
    [ProducesResponseType(typeof(CompanyAddressDetailData), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public Task<CompanyAddressDetailData> GetOwnCompanyDetailsAsync() =>
        this.WithCompanyId(companyId => _logic.GetCompanyDetailsAsync(companyId));

    /// <summary>
    /// Gets the CompanyAssigned UseCase details
    /// </summary>
    /// <returns>the CompanyAssigned UseCase details</returns>
    /// <remarks>Example: GET: api/administration/companydata/preferredUseCases</remarks>
    /// <response code="200">Returns the CompanyAssigned UseCase details.</response>
    [HttpGet]
    [Authorize(Roles = "view_use_cases")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("preferredUseCases")]
    [ProducesResponseType(typeof(CompanyAssignedUseCaseData), StatusCodes.Status200OK)]
    public IAsyncEnumerable<CompanyAssignedUseCaseData> GetCompanyAssigendUseCaseDetailsAsync() =>
       this.WithCompanyId(companyId => _logic.GetCompanyAssigendUseCaseDetailsAsync(companyId));

    /// <summary>
    /// Create the CompanyAssigned UseCase details
    /// </summary>
    /// <remarks>Example: POST: api/administration/companydata/preferredUseCases</remarks>
    /// <response code="204">NoContentResult</response>
    /// <response code="208">UseCaseId already existis</response>
    /// <response code="409">Company Status is Incorrect</response>
    [HttpPost]
    [Authorize(Roles = "set_company_use_cases")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("preferredUseCases")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<StatusCodeResult> CreateCompanyAssignedUseCaseDetailsAsync([FromBody] UseCaseIdDetails data) =>
        await this.WithCompanyId(companyId => _logic.CreateCompanyAssignedUseCaseDetailsAsync(companyId, data.useCaseId)).ConfigureAwait(false)
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
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("preferredUseCases")]
    [ProducesResponseType(typeof(NoContentResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<NoContentResult> RemoveCompanyAssignedUseCaseDetailsAsync([FromBody] UseCaseIdDetails data)
    {
        await this.WithCompanyId(companyId => _logic.RemoveCompanyAssignedUseCaseDetailsAsync(companyId, data.useCaseId)).ConfigureAwait(false);
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
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("companyRolesAndConsents")]
    [ProducesResponseType(typeof(CompanyRoleConsentViewData), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public IAsyncEnumerable<CompanyRoleConsentViewData> GetCompanyRoleAndConsentAgreementDetailsAsync([FromQuery] string? languageShortName = null) =>
        this.WithCompanyId(companyId => _logic.GetCompanyRoleAndConsentAgreementDetailsAsync(companyId, languageShortName));

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
    [Authorize(Policy = PolicyTypes.ValidIdentity)]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("companyRolesAndConsents")]
    [ProducesResponseType(typeof(NoContentResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<NoContentResult> CreateCompanyRoleAndConsentAgreementDetailsAsync([FromBody] IEnumerable<CompanyRoleConsentDetails> companyRoleConsentDetails)
    {
        await this.WithUserIdAndCompanyId(identity => _logic.CreateCompanyRoleAndConsentAgreementDetailsAsync(identity, companyRoleConsentDetails));
        return NoContent();
    }

    /// <summary>
    /// Gets the UseCaseParticipations for the own company
    /// </summary>
    /// <remarks>Example: Get: api/administration/companydata/useCaseParticipation</remarks>
    /// <returns>All UseCaseParticipations of the own company</returns>
    /// <response code="200">Returns a collection of UseCaseParticipation.</response>
    [HttpGet]
    [Authorize(Roles = "view_use_case_participation")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("useCaseParticipation")]
    [ProducesResponseType(typeof(IEnumerable<UseCaseParticipationData>), StatusCodes.Status200OK)]
    public Task<IEnumerable<UseCaseParticipationData>> GetUseCaseParticipation([FromQuery] string? language) =>
        this.WithCompanyId(companyId => _logic.GetUseCaseParticipationAsync(companyId, language));

    /// <summary>
    /// Gets the Ssi certifications for the own company
    /// </summary>
    /// <returns>All ssi certifications of the own company</returns>
    /// <remarks>Example: Get: api/administration/companydata/certificates</remarks>
    /// <response code="200">Returns a collection of certificates.</response>
    [HttpGet]
    [Authorize(Roles = "view_certificates")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("certificates")]
    [ProducesResponseType(typeof(IEnumerable<SsiCertificateTransferData>), StatusCodes.Status200OK)]
    public Task<IEnumerable<SsiCertificateData>> GetSsiCertificationData() =>
        this.WithCompanyId(companyId => _logic.GetSsiCertificatesAsync(companyId));

    /// <summary>
    /// Gets the Ssi certifications for the own company
    /// </summary>
    /// <returns>All ssi certifications of the own company</returns>
    /// <remarks>Example: Get: api/administration/companydata/certificateTypes</remarks>
    /// <response code="200">Returns a collection of certificates.</response>
    [HttpGet]
    [Authorize(Roles = "request_ssicredential")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("certificateTypes")]
    [ProducesResponseType(typeof(IEnumerable<SsiCertificateTransferData>), StatusCodes.Status200OK)]
    public IAsyncEnumerable<VerifiedCredentialTypeId> GetCertificateTypes() =>
        _logic.GetCertificateTypes();

    /// <summary>
    /// Creates the useCaseParticipation
    /// </summary>
    /// <param name="data">The type and document</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>The id of the created use case participation</returns>
    /// <remarks>Example: POST: api/administration/companydata/useCaseParticipation</remarks>
    /// <response code="204">Successfully created the use case particiation.</response>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [Authorize(Roles = "request_ssicredential")]
    [Authorize(Policy = PolicyTypes.ValidIdentity)]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("useCaseParticipation")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<NoContentResult> CreateUseCaseParticipation([FromForm] UseCaseParticipationCreationData data, CancellationToken cancellationToken)
    {
        await this.WithUserIdAndCompanyId(identity => _logic.CreateUseCaseParticipation(identity, data, cancellationToken)).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>
    /// Creates the ssiCertificate
    /// </summary>
    /// <param name="data">The type and document</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>The id of the created use case participation</returns>
    /// <remarks>Example: POST: api/administration/companydata/certificates</remarks>
    /// <response code="204">Successfully created the ssi certificate.</response>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [Authorize(Roles = "request_ssicredential")]
    [Authorize(Policy = PolicyTypes.ValidIdentity)]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("certificates")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<NoContentResult> CreateSsiCertificate([FromForm] SsiCertificateCreationData data, CancellationToken cancellationToken)
    {
        await this.WithUserIdAndCompanyId(identity => _logic.CreateSsiCertificate(identity, data, cancellationToken)).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>
    ///     Gets all outstanding, existing and inactive credentials
    /// </summary>
    /// <remarks>Example: Get: /api/administration/companydata/credentials/</remarks>
    /// <param name="page">The page to get</param>
    /// <param name="size">Amount of entries</param>
    /// <param name="companySsiDetailStatusId">OPTIONAL: Filter for the status</param>
    /// <param name="credentialTypeId">OPTIONAL: The type of the credential that should be returned</param>
    /// <param name="companyName">OPTIONAL: Search string for the company name</param>
    /// <param name="sorting">Defines the sorting of the list</param>
    /// <response code="200">Collection of the credentials.</response>
    [HttpGet]
    [Authorize(Roles = "decision_ssicredential")]
    [Authorize(Policy = PolicyTypes.ValidIdentity)]
    [Route("credentials", Name = nameof(GetCredentials))]
    [ProducesResponseType(typeof(IEnumerable<CredentialDetailData>), StatusCodes.Status200OK)]
    public Task<Pagination.Response<CredentialDetailData>> GetCredentials(
        [FromQuery] int page = 0,
        [FromQuery] int size = 15,
        [FromQuery] CompanySsiDetailStatusId? companySsiDetailStatusId = null,
        [FromQuery] VerifiedCredentialTypeId? credentialTypeId = null,
        [FromQuery] string? companyName = null,
        [FromQuery] CompanySsiDetailSorting? sorting = null) =>
        _logic.GetCredentials(page, size, companySsiDetailStatusId, credentialTypeId, companyName, sorting);

    /// <summary>
    /// Approves the given credential
    /// </summary>
    /// <remarks>Example: PUT: api/administration/companydata/credentials/{credentialId}/approval</remarks>
    /// <param name="credentialId">Id of the entry that should be approved</param>
    /// <param name="cts">Cancellation Token</param>
    /// <returns>No Content</returns>
    /// <response code="204">Successfully approved the credentials.</response>
    [HttpPut]
    [Authorize(Roles = "decision_ssicredential")]
    [Authorize(Policy = PolicyTypes.ValidIdentity)]
    [Route("credentials/{credentialId}/approval")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<NoContentResult> ApproveCredential([FromRoute] Guid credentialId, CancellationToken cts)
    {
        await this.WithUserId(userId => _logic.ApproveCredential(userId, credentialId, cts)).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>
    /// Creates the ssiCertificate
    /// </summary>
    /// <remarks>Example: PUT: api/administration/companydata/credentials/{credentialId}/reject</remarks>
    /// <param name="credentialId">Id of the entry that should be approved</param>
    /// <returns>No Content</returns>
    /// <response code="204">Successfully rejected the credentials.</response>
    [HttpPut]
    [Authorize(Roles = "decision_ssicredential")]
    [Authorize(Policy = PolicyTypes.ValidIdentity)]
    [Route("credentials/{credentialId}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<NoContentResult> RejectCredential([FromRoute] Guid credentialId)
    {
        await this.WithUserId(userId => _logic.RejectCredential(userId, credentialId)).ConfigureAwait(false);
        return NoContent();
    }
}
