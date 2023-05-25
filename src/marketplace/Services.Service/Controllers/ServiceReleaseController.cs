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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Authentication;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Services.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Services.Service.ViewModels;

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
       this.WithIdentityData(identity => _serviceReleaseBusinessLogic.GetServiceAgreementConsentAsync(serviceId, identity));

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
        this.WithIdentityData(identity => _serviceReleaseBusinessLogic.GetServiceDetailsForStatusAsync(serviceId, identity));

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
    /// <param name="status">Filter by status</param>
    /// <returns>Collection of all in review status marketplace service.</returns>
    /// <remarks>Example: GET: /api/services/servicerelease/inReview</remarks>
    /// <response code="200">Returns the list of all in review status marketplace service.</response>
    [HttpGet]
    [Route("inReview")]
    [Authorize(Roles = "approve_service_release,decline_service_release")]
    [ProducesResponseType(typeof(Pagination.Response<InReviewServiceData>), StatusCodes.Status200OK)]
    public Task<Pagination.Response<InReviewServiceData>> GetAllInReviewStatusServiceAsync([FromQuery] int page = 0, [FromQuery] int size = 15, [FromQuery] OfferSorting? sorting = null, [FromQuery] string? serviceName = null, [FromQuery] string? languageShortName = null, [FromQuery] ServiceReleaseStatusIdFilter? status = null) =>
        _serviceReleaseBusinessLogic.GetAllInReviewStatusServiceAsync(page, size, sorting, serviceName, languageShortName, status);

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
        await this.WithIdentityData(identity => _serviceReleaseBusinessLogic.DeleteServiceDocumentsAsync(documentId, identity));
        return NoContent();
    }

    /// <summary>
    /// Creates a new service offering.
    /// </summary>
    /// <param name="data">The data for the new service offering.</param>
    /// <remarks>Example: POST: /api/services/servicerelease/addservice</remarks>
    /// <response code="201">Returns the newly created service id.</response>
    /// <response code="400">The given service offering data were invalid i.e At lease one Service Type Id is missing or Title is less than three character or IamUser is not assignable to company user or SalesManager does not exist.</response>
    [HttpPost]
    [Route("addservice")]
    [Authorize(Roles = "add_service_offering")]
    [ProducesResponseType(typeof(OfferProviderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<CreatedAtRouteResult> CreateServiceOffering([FromBody] ServiceOfferingData data)
    {
        var id = await this.WithIamUserId(iamUserId => _serviceReleaseBusinessLogic.CreateServiceOfferingAsync(data, iamUserId)).ConfigureAwait(false);
        return CreatedAtRoute(nameof(ServiceReleaseController.GetServiceDetailsForStatusAsync), new { controller = "ServiceRelease", serviceId = id }, id);
    }

    /// <summary>
    /// Updates the service
    /// </summary>
    /// <param name="serviceId" example="D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645">Id for the service to update.</param>
    /// <param name="data">The request data to update the service</param>
    /// <remarks>Example: PUT: /api/services/servicerelease/{serviceId}</remarks>
    /// <response code="204">Service was successfully updated.</response>
    /// <response code="400">Offer Subscription is not in state created or user is not in the same company.</response>
    /// <response code="404">Offer Subscription not found.</response>
    /// <response code="403">User don't have permission to change the service.</response>
    /// <response code="409">Service is in inCorrect state</response>
    [HttpPut]
    [Route("{serviceId:guid}")]
    [Authorize(Roles = "update_service_offering")]
    [ProducesResponseType(typeof(NoContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<NoContentResult> UpdateService([FromRoute] Guid serviceId, [FromBody] ServiceUpdateRequestData data)
    {
        await this.WithIdentityData(identity => _serviceReleaseBusinessLogic.UpdateServiceAsync(serviceId, data, identity));
        return NoContent();
    }

    /// <summary>
    /// Submit an Service for release
    /// </summary>
    /// <param name="serviceId" example="D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645">ID of the service.</param>
    /// <remarks>Example: PUT: /api/services/servicerelease/D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645/submit</remarks>
    /// <response code="204">The service was successfully submitted for release.</response>
    /// <response code="400">Either the sub claim is empty/invalid, user does not exist or the subscription might not have the correct status or the companyID is incorrect.</response>
    /// <response code="404">service does not exist.</response>
    [HttpPut]
    [Route("{serviceId:guid}/submit")]
    [Authorize(Roles = "add_service_offering")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<NoContentResult> SubmitService([FromRoute] Guid serviceId)
    {
        await this.WithIdentityData(identity => _serviceReleaseBusinessLogic.SubmitServiceAsync(serviceId, identity)).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>
    /// Approve Service to change status from IN_REVIEW to Active and create notification
    /// </summary>
    /// <param name="serviceId"></param>
    /// <remarks>Example: PUT: /api/services/servicerelease/D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645/approveService</remarks>
    /// <response code="204">The service was successfully submitted to Active State.</response>
    /// <response code="409">Service is in InCorrect Status</response>
    /// <response code="404">service does not exist.</response>
    /// <response code="500">Internal server error</response>
    [HttpPut]
    [Route("{serviceId}/approveService")]
    [Authorize(Roles = "approve_service_release")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<NoContentResult> ApproveServiceRequest([FromRoute] Guid serviceId)
    {
        await this.WithIdentityData(identity => _serviceReleaseBusinessLogic.ApproveServiceRequestAsync(serviceId, identity)).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>
    /// Declines the service request
    /// </summary>
    /// <param name="serviceId" example="D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645">Id of the service that should be declined</param>
    /// <param name="data">the data of the decline request</param>
    /// <remarks>Example: PUT: /api/services/servicerelease/D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645/decline</remarks>
    /// <response code="204">NoContent.</response>
    /// <response code="400">If sub claim is empty/invalid or user does not exist.</response>
    /// <response code="404">If service does not exists.</response>
    /// <response code="403">User doest not have permission to change</response>
    /// <response code="409">Offer Type is in inCorrect state.</response>
    /// <response code="500">Internal Server Error.</response>
    [HttpPut]
    [Route("{serviceId:guid}/declineService")]
    [Authorize(Roles = "decline_service_release")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<NoContentResult> DeclineServiceRequest([FromRoute] Guid serviceId, [FromBody] OfferDeclineRequest data)
    {
        await this.WithIdentityData(identity => _serviceReleaseBusinessLogic.DeclineServiceRequestAsync(serviceId, identity, data)).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>
    /// Upload document for active service in the marketplace for given serviceId for same company as user
    /// </summary>
    /// <param name="serviceId"></param>
    /// <param name="documentTypeId"></param>
    /// <param name="document"></param>
    /// <param name="cancellationToken"></param>
    /// <remarks>Example: PUT: /api/services/servicerelease/updateservicedoc/{serviceId}/documentType/{documentTypeId}/documents</remarks>
    /// <response code="204">Successfully uploaded the document</response>
    /// <response code="400">If sub claim is empty/invalid or user does not exist, or any other parameters are invalid.</response>
    /// <response code="404">service does not exist.</response>
    /// <response code="403">The user is not assigned with the service.</response>
    /// <response code="415">Only PDF files are supported.</response>
    /// <response code="409">Offer is in inCorrect State.</response>
    [HttpPut]
    [Route("updateservicedoc/{serviceId}/documentType/{documentTypeId}/documents")]
    [Authorize(Roles = "add_service_offering")]
    [Consumes("multipart/form-data")]
    [RequestFormLimits(ValueLengthLimit = 819200, MultipartBodyLengthLimit = 819200)]
    [ProducesResponseType(typeof(NoContentResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status415UnsupportedMediaType)]
    public async Task<NoContentResult> UpdateServiceDocumentAsync([FromRoute] Guid serviceId, [FromRoute] DocumentTypeId documentTypeId, [FromForm(Name = "document")] IFormFile document, CancellationToken cancellationToken)
    {
        await this.WithIdentityData(identity => _serviceReleaseBusinessLogic.CreateServiceDocumentAsync(serviceId, documentTypeId, document, identity, cancellationToken));
        return NoContent();
    }

    /// <summary>
    /// Retrieve the technical user profile information
    /// </summary>
    /// <param name="serviceId">id of the service to receive the technical user profiles for</param>
    /// <remarks>Example: GET: /api/services/servicerelease/{serviceId}/technical-user-profiles</remarks>
    /// <response code="200">Returns a list of profiles</response>
    /// <response code="403">Requesting user is not part of the providing company for the service.</response>
    [HttpGet]
    [Route("{serviceId}/technical-user-profiles")]
    [Authorize(Roles = "add_service_offering")]
    [ProducesResponseType(typeof(NoContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public Task<IEnumerable<TechnicalUserProfileInformation>> GetTechnicalUserProfiles([FromRoute] Guid serviceId) =>
        this.WithIdentityData(identity => _serviceReleaseBusinessLogic.GetTechnicalUserProfilesForOffer(serviceId, identity));

    /// <summary>
    /// Creates and updates the technical user profiles
    /// </summary>
    /// <param name="serviceId">id of the service to receive the technical user profiles for</param>
    /// <param name="data">The data for the update of the technical user profile</param>
    /// <remarks>Example: PUT: /api/services/servicerelease/{serviceId}/technical-user-profiles</remarks>
    /// <response code="200">Returns a list of profiles</response>
    /// <response code="403">Requesting user is not part of the providing company for the service.</response>
    [HttpPut]
    [Route("{serviceId}/technical-user-profiles")]
    [Authorize(Roles = "add_service_offering")]
    [ProducesResponseType(typeof(NoContentResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<NoContentResult> CreateAndUpdateTechnicalUserProfiles([FromRoute] Guid serviceId, [FromBody] IEnumerable<TechnicalUserProfileData> data)
    {
        await this.WithIdentityData(identity => _serviceReleaseBusinessLogic.UpdateTechnicalUserProfiles(serviceId, data, identity)).ConfigureAwait(false);
        return NoContent();
    }
}
