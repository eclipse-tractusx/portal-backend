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
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Authentication;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.Services.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Services.Service.ViewModels;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Services.Service.Controllers;

/// <summary>
/// Controller providing actions for displaying, filtering and updating services.
/// </summary>
[Route("api/services/[controller]")]
[ApiController]
[Produces("application/json")]
[Consumes("application/json")]
public class ServiceReleaseController : ControllerBase
{
    private readonly IServiceReleaseBusinessLogic _serviceReleaseBusinessLogic;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="serviceReleaseBusinessLogic">Logic dependency.</param>
    public ServiceReleaseController(IServiceReleaseBusinessLogic serviceReleaseBusinessLogic)
    {
        _serviceReleaseBusinessLogic = serviceReleaseBusinessLogic;
    }

    /// <summary>
    /// Return Agreement Data for offer_type_id Service
    /// </summary>
    /// <remarks>Example: GET: /api/services/servicerelease/agreementData</remarks>
    /// <response code="200">Returns the Cpllection of agreement data</response>
    [HttpGet]
    [Route("agreementData")]
    [Authorize(Roles = "add_service_offering")]
    [ProducesResponseType(typeof(IAsyncEnumerable<AgreementDocumentData>), StatusCodes.Status200OK)]
    public IAsyncEnumerable<AgreementDocumentData> GetServiceAgreementDataAsync() =>
        _serviceReleaseBusinessLogic.GetServiceAgreementDataAsync();

    /// <summary>
    /// Retrieves service details for an offer referenced by id.
    /// </summary>
    /// <param name="serviceId" example="D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645">ID of the offer to retrieve.</param>
    /// <returns>ServiceData for requested offer.</returns>
    /// <remarks>Example: GET: /api/services/servicerelease/inReview/D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645</remarks>
    /// <response code="200">Returns the requested service details.</response>
    /// <response code="404">service not found.</response>
    /// <response code="409">service is inCorrect Status.</response>
    [HttpGet]
    [Route("inReview/{serviceId}", Name = nameof(GetServiceDetailsByIdAsync))]
    [Authorize(Roles = "approve_service_release,decline_service_release")]
    [ProducesResponseType(typeof(ServiceData), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public Task<ServiceData> GetServiceDetailsByIdAsync([FromRoute] Guid serviceId) =>
        _serviceReleaseBusinessLogic.GetServiceDetailsByIdAsync(serviceId);

    /// <summary>
    /// Retrieve Service Type Data
    /// </summary>
    /// <returns>Service Type Data</returns>
    /// <remarks>Example: GET: /api/services/servicerelease/serviceTypes </remarks>
    /// <response code="200">Returns the Service Type.</response>
    [HttpGet]
    [Route("serviceTypes")]
    [Authorize(Roles = "add_service_offering")]
    [ProducesResponseType(typeof(IAsyncEnumerable<ServiceTypeData>), StatusCodes.Status200OK)]
    public IAsyncEnumerable<ServiceTypeData> GetServiceTypeDataAsync() =>
        _serviceReleaseBusinessLogic.GetServiceTypeDataAsync();
    
    /// <summary>
    /// Gets the agreement consent status for the given service id
    /// </summary>
    /// <param name="serviceId"></param>
    /// <remarks>Example: GET: /api/services/servicerelease/consent/{serviceId}</remarks>
    /// <response code="200">Returns the offer Agreement Consent data</response>
    /// <response code="404">offer does not exist.</response>
    /// <response code="403">User not associated with offer.</response>
    [HttpGet]
    [Route("consent/{serviceId}")]
    [Authorize(Roles = "add_service_offering")]
    [ProducesResponseType(typeof(OfferAgreementConsent), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public Task<OfferAgreementConsent> GetServiceAgreementConsentByIdAsync([FromRoute] Guid serviceId) =>
       this.WithIamUserId(iamUserId => _serviceReleaseBusinessLogic.GetServiceAgreementConsentAsync(serviceId, iamUserId));

    /// <summary>
    /// Return app detail with status
    /// </summary>
    /// <param name="serviceId"></param>
    /// <remarks>Example: GET: /api/services/servicerelease/{serviceId}/serviceStatus</remarks>
    /// <response code="200">Return the Offer and status data</response>
    /// <response code="404">App does not exist.</response>
    /// <response code="403">User not associated with provider company.</response>
    [HttpGet]
    [Route("{serviceId}/serviceStatus", Name = nameof(GetServiceDetailsForStatusAsync))]
    [Authorize(Roles = "add_service_offering")]
    [ProducesResponseType(typeof(ServiceProviderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public Task<ServiceProviderResponse> GetServiceDetailsForStatusAsync([FromRoute] Guid serviceId) =>
        this.WithIamUserId(iamUserId => _serviceReleaseBusinessLogic.GetServiceDetailsForStatusAsync(serviceId, iamUserId));

    /// <summary>
    /// Update or Insert Consent
    /// </summary>
    /// <param name="serviceId">Id of the service</param>
    /// <param name="offerAgreementConsents">agreement consent data</param>
    /// <remarks>Example: POST: /api/services/servicerelease/consent/{serviceId}/agreementConsents</remarks>
    /// <response code="200">Successfully submitted consent to agreements</response>
    /// <response code="403">Either the user was not found or the user is not assignable to the given application.</response>
    /// <response code="404">Service does not exist.</response>
    /// <response code="400">Service Id is incorrect.</response>
    [HttpPost]
    [Authorize(Roles = "add_service_offering")]
    [Route("consent/{serviceId}/agreementConsents")]
    [ProducesResponseType(typeof(IEnumerable<ConsentStatusData>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IEnumerable<ConsentStatusData>> SubmitOfferConsentToAgreementsAsync([FromRoute] Guid serviceId, [FromBody] OfferAgreementConsent offerAgreementConsents) => 
        await this.WithIamUserId(iamUserId => _serviceReleaseBusinessLogic.SubmitOfferConsentAsync(serviceId, offerAgreementConsents, iamUserId));
        
    /// <summary>
    /// Retrieves all in review status service in the marketplace .
    /// </summary>
    /// <param name="page">page index start from 0</param>
    /// <param name="size">size to get number of records</param>
    /// <param name="sorting">sort by</param>
    /// <param name="serviceName">search by service name</param>
    /// <param name="languageShortName">Filter by language shortname</param>
    /// <returns>Collection of all in review status marketplace service.</returns>
    /// <remarks>Example: GET: /api/services/servicerelease/inReview</remarks>
    /// <response code="200">Returns the list of all in review status marketplace service.</response>
    [HttpGet]
    [Route("inReview")]
    [Authorize(Roles = "approve_service_release,decline_service_release")]
    [ProducesResponseType(typeof(Pagination.Response<InReviewServiceData>), StatusCodes.Status200OK)]
    public Task<Pagination.Response<InReviewServiceData>> GetAllInReviewStatusServiceAsync([FromQuery] int page = 0, [FromQuery] int size = 15, [FromQuery] OfferSorting? sorting = null, [FromQuery] string? serviceName = null, [FromQuery] string? languageShortName = null) =>
        _serviceReleaseBusinessLogic.GetAllInReviewStatusServiceAsync(page, size, sorting,serviceName, languageShortName);

    /// <summary>
    /// Delete Document Assigned to Offer
    /// </summary>
    /// <param name="documentId">ID of the document to be deleted.</param>
    /// <remarks>Example: DELETE: /api/services/servicerelease/documents/{documentId}</remarks>
    /// <response code="204">Empty response on success.</response>
    /// <response code="404">Record not found.</response>
    /// <response code="409">Document or App is in InCorrect Status</response>
    /// <response code="403">User is not allowed to delete the document</response>
    /// <response code="400"> parameters are invalid.</response>
    [HttpDelete]
    [Route("documents/{documentId}")]
    [Authorize(Roles = "add_service_offering")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<NoContentResult> DeleteServiceDocumentsAsync([FromRoute] Guid documentId)
    {
        await this.WithIamUserId(iamUserId => _serviceReleaseBusinessLogic.DeleteServiceDocumentsAsync(documentId, iamUserId));
        return NoContent();
    }
}
