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
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[Consumes("application/json")]
public class ServicesController : ControllerBase
{
    private readonly IServiceBusinessLogic _serviceBusinessLogic;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="serviceBusinessLogic">Logic dependency.</param>
    public ServicesController(IServiceBusinessLogic serviceBusinessLogic)
    {
        _serviceBusinessLogic = serviceBusinessLogic;
    }

    /// <summary>
    /// Retrieves all active services in the marketplace.
    /// </summary>
    /// <param name="page" example="0">Optional the page of the services.</param>
    /// <param name="size" example="15">Amount of services that should be returned, default is 15.</param>
    /// <param name="sorting" example="ProviderAsc">Optional Sorting of the pagination</param>
    /// <param name="serviceTypeId">Optional filter for service type ids</param>
    /// <returns>Collection of all active services.</returns>
    /// <remarks>Example: GET: /api/services/active</remarks>
    /// <response code="200">Returns the list of all active services.</response>
    [HttpGet]
    [Route("active")]
    [Authorize(Roles = "view_service_offering")]
    [ProducesResponseType(typeof(Pagination.Response<ServiceOverviewData>), StatusCodes.Status200OK)]
    public Task<Pagination.Response<ServiceOverviewData>> GetAllActiveServicesAsync([FromQuery] int page = 0, [FromQuery] int size = 15, [FromQuery] ServiceOverviewSorting? sorting = null, [FromQuery] ServiceTypeId? serviceTypeId = null) =>
        _serviceBusinessLogic.GetAllActiveServicesAsync(page, size, sorting, serviceTypeId);

    /// <summary>
    /// Creates a new service offering.
    /// </summary>
    /// <param name="data">The data for the new service offering.</param>
    /// <remarks>Example: POST: /api/services/addservice</remarks>
    /// <response code="201">Returns the newly created service id.</response>
    /// <response code="400">The given service offering data were invalid.</response>
    [HttpPost]
    [Route("addservice")]
    [Authorize(Roles = "add_service_offering")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<CreatedAtRouteResult> CreateServiceOffering([FromBody] ServiceOfferingData data)
    {
        var id = await this.WithIamUserId(iamUserId => _serviceBusinessLogic.CreateServiceOfferingAsync(data, iamUserId)).ConfigureAwait(false);
        return CreatedAtRoute(nameof(GetServiceDetails), new { serviceId = id }, id);
    }

    /// <summary>
    /// Adds a new service subscription.
    /// </summary>
    /// <param name="serviceId" example="D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645">Id for the service the user wants to subscribe to.</param>
    /// <param name="offerAgreementConsentData">The agreement consent data</param>
    /// <remarks>Example: POST: /api/services/D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645/subscribe</remarks>
    /// <response code="201">Returns success</response>
    /// <response code="400">Company or company user wasn't assigned to the user.</response>
    /// <response code="404">No Service was found for the given id.</response>
    [HttpPost]
    [Route("{serviceId}/subscribe")]
    [Authorize(Roles = "subscribe_service")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<CreatedAtRouteResult> AddServiceSubscription([FromRoute] Guid serviceId, [FromBody] IEnumerable<OfferAgreementConsentData> offerAgreementConsentData)
    {
        var serviceSubscriptionId = await this.WithIamUserAndBearerToken(auth => _serviceBusinessLogic.AddServiceSubscription(serviceId, offerAgreementConsentData, auth.iamUserId, auth.bearerToken)).ConfigureAwait(false);
        return CreatedAtRoute(nameof(GetSubscriptionDetail), new { subscriptionId = serviceSubscriptionId }, serviceSubscriptionId);
    }

    /// <summary>
    /// Gets the Subscription Detail Data
    /// </summary>
    /// <param name="subscriptionId" example="D3B1ECA2-6148-4008-9E6C-C1C2AEA5C646">Id for the subscription the wants to retrieve.</param>
    /// <remarks>Example: Get: /api/services/subscription/D3B1ECA2-6148-4008-9E6C-C1C2AEA5C646</remarks>
    /// <response code="200">Returns the subscription details.</response>
    /// <response code="404">Service was not found.</response>
    [HttpGet]
    [Route("subscription/{subscriptionId}", Name = nameof(GetSubscriptionDetail))]
    [Authorize(Roles = "view_service_offering")]
    [ProducesResponseType(typeof(SubscriptionDetailData), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public Task<SubscriptionDetailData> GetSubscriptionDetail([FromRoute] Guid subscriptionId) => 
        this.WithIamUserId(iamUserId => _serviceBusinessLogic.GetSubscriptionDetailAsync(subscriptionId, iamUserId));

    /// <summary>
    /// Retrieves service offer details for the respective service id.
    /// </summary>
    /// <param name="serviceId" example="D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645">Id for the service the wants to retrieve.</param>
    /// <param name="lang" example="de">OPTIONAL: Optional two character language specifier for the service description. Default response is set to english.</param>
    /// <remarks>Example: Get: /api/services/D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645</remarks>
    /// <response code="200">Returns the service details.</response>
    /// <response code="404">Service was not found.</response>
    [HttpGet]
    [Route("{serviceId}", Name = nameof(GetServiceDetails))]
    [Authorize(Roles = "view_service_offering")]
    [ProducesResponseType(typeof(ServiceDetailData), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public Task<ServiceDetailData> GetServiceDetails([FromRoute] Guid serviceId, [FromQuery] string? lang = "en") => 
        this.WithIamUserId(iamUserId => _serviceBusinessLogic.GetServiceDetailsAsync(serviceId, lang!, iamUserId));
    
    /// <summary>
    /// Creates new service agreement consents 
    /// </summary>
    /// <remarks>Example: Post: /api/services/D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645/serviceAgreementConsent</remarks>
    /// <response code="204">Returns no content.</response>
    /// <response code="400">Company or company user wasn't assigned to the user.</response>
    /// <response code="404">No Service was found for the given id.</response>
    [HttpPost]
    [Route("{subscriptionId}/serviceAgreementConsents")]
    [Authorize(Roles = "add_service_offering")]
    [ProducesResponseType(typeof(NoContentResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<NoContentResult> CreateOrUpdateServiceAgreementConsents([FromRoute] Guid subscriptionId, [FromBody] IEnumerable<OfferAgreementConsentData> offerAgreementConsentData)
    {
        await this.WithIamUserId(iamUserId => _serviceBusinessLogic.CreateOrUpdateServiceAgreementConsentAsync(subscriptionId, offerAgreementConsentData, iamUserId).ConfigureAwait(false));
        return this.NoContent();
    }

    /// <summary>
    /// Creates new service agreement consents 
    /// </summary>
    /// <param name="subscriptionId" example="D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645">Id for the service subscription the consent should get set for.</param>
    /// <param name="offerAgreementConsentData">the service agreement consent.</param>
    /// <remarks>Example: Post: /api/services/D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645/serviceAgreementConsent</remarks>
    /// <response code="201">Returns the id of the created consent.</response>
    /// <response code="400">Company or company user wasn't assigned to the user.</response>
    /// <response code="404">No Service was found for the given id.</response>
    [HttpPost]
    [Route("{subscriptionId}/serviceAgreementConsent")]
    [Authorize(Roles = "add_service_offering")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<CreatedAtRouteResult> CreateServiceAgreementConsent([FromRoute] Guid subscriptionId, [FromBody] OfferAgreementConsentData offerAgreementConsentData)
    {
        var consentId = await this.WithIamUserId(iamUserId =>
            _serviceBusinessLogic.CreateServiceAgreementConsentAsync(subscriptionId, offerAgreementConsentData, iamUserId)
                .ConfigureAwait(false));
        return CreatedAtRoute(nameof(GetServiceAgreementConsentDetail), new { serviceConsentId = consentId }, consentId);
    }

    /// <summary>
    /// Gets the service agreement consent details.
    /// </summary>
    /// <param name="serviceConsentId" example="D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645">Id for the service consent to retrieve.</param>
    /// <remarks>Example: Get: /api/services/serviceConsent/D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645</remarks>
    /// <response code="200">Returns the service consent details.</response>
    /// <response code="404">Consent was not found.</response>
    [HttpGet]
    [Route("serviceConsent/{serviceConsentId}", Name = nameof(GetServiceAgreementConsentDetail))]
    [Authorize(Roles = "view_service_offering")]
    [ProducesResponseType(typeof(ConsentDetailData), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public Task<ConsentDetailData> GetServiceAgreementConsentDetail([FromRoute] Guid serviceConsentId) => 
        _serviceBusinessLogic.GetServiceConsentDetailDataAsync(serviceConsentId);

    /// <summary>
    /// Gets all agreements 
    /// </summary>
    /// <param name="serviceId" example="D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645">Id for the service consent to retrieve.</param>
    /// <remarks>Example: GET: /api/services/serviceAgreementData/D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645</remarks>
    /// <response code="200">Returns the service agreement data.</response>
    [HttpGet]
    [Route("serviceAgreementData/{serviceId}")]
    [Authorize(Roles = "subscribe_service_offering")]
    [ProducesResponseType(typeof(AgreementData), StatusCodes.Status200OK)]
    public IAsyncEnumerable<AgreementData> GetServiceAgreement([FromRoute] Guid serviceId) =>
        _serviceBusinessLogic.GetServiceAgreement(serviceId);

    /// <summary>
    /// Auto setup the service
    /// </summary>
    /// <remarks>Example: POST: /api/services/autoSetup</remarks>
    /// <response code="200">Returns the service agreement data.</response>
    /// <response code="400">Offer Subscription is pending or not the providing company.</response>
    /// <response code="404">Offer Subscription not found.</response>
    [HttpPost]
    [Route("autoSetup")]
    [Authorize(Roles = "activate_subscription")]
    [ProducesResponseType(typeof(OfferAutoSetupResponseData), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<OfferAutoSetupResponseData> AutoSetupService([FromBody] OfferAutoSetupData data)
        => await this.WithIamUserId(iamUserId => _serviceBusinessLogic.AutoSetupServiceAsync(data, iamUserId));

    /// <summary>
    /// Updates the service
    /// </summary>
    /// <param name="serviceId" example="D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645">Id for the service to update.</param>
    /// <param name="data">The request data to update the service</param>
    /// <remarks>Example: PUT: /api/services/{serviceId}</remarks>
    /// <response code="204">Service was successfully updated.</response>
    /// <response code="400">Offer Subscription is not in state created or user is not in the same company.</response>
    /// <response code="404">Offer Subscription not found.</response>
    [HttpPut]
    [Route("{serviceId:guid}")]
    [Authorize(Roles = "update_service_offering")]
    [ProducesResponseType(typeof(NoContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<NoContentResult> UpdateService([FromRoute] Guid serviceId, [FromBody] ServiceUpdateRequestData data)
    {
        await this.WithIamUserId(iamUserId => _serviceBusinessLogic.UpdateServiceAsync(serviceId, data, iamUserId));
        return NoContent();
    }
    
    /// <summary>
    /// Retrieves subscription statuses of provided services of the currently logged in user's company.
    /// </summary>
    /// <remarks>Example: GET: /api/services/provided/subscription-status</remarks>
    /// <response code="200">Returns list of applicable service subscription statuses.</response>
    /// <response code="400">If sub claim is empty/invalid or user does not exist.</response>
    [HttpGet]
    [Route("provided/subscription-status")]
    [Authorize(Roles = "view_service_subscriptions")]
    [ProducesResponseType(typeof(Pagination.Response<OfferCompanySubscriptionStatusData>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public Task<Pagination.Response<OfferCompanySubscriptionStatusData>> GetCompanyProvidedServiceSubscriptionStatusesForCurrentUserAsync([FromQuery] int page = 0, [FromQuery] int size = 15, [FromQuery] SubscriptionStatusSorting? sorting = null, [FromQuery] OfferSubscriptionStatusId? statusId = null) =>
        this.WithIamUserId(userId => _serviceBusinessLogic.GetCompanyProvidedServiceSubscriptionStatusesForUserAsync(page, size, userId, sorting, statusId));

    /// <summary>
    /// Submit an Service for release
    /// </summary>
    /// <param name="serviceId" example="D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645">ID of the service.</param>
    /// <remarks>Example: PUT: /api/services/D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645/submit</remarks>
    /// <response code="204">The service was successfully submitted for release.</response>
    /// <response code="400">Either the sub claim is empty/invalid, user does not exist or the subscription might not have the correct status or the companyID is incorrect.</response>
    /// <response code="404">service does not exist.</response>
    [HttpPut]
    [Route("{serviceId:guid}/submit")]
    [Authorize(Roles = "add_service_offering")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<NoContentResult> SubmitService([FromRoute] Guid serviceId)
    {
        await this.WithIamUserId(userId => _serviceBusinessLogic.SubmitServiceAsync(serviceId, userId)).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>
    /// Approve Service to change status from IN_REVIEW to Active and create notification
    /// </summary>
    /// <param name="serviceId"></param>
    /// <remarks>Example: PUT: /api/services/D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645/approveService</remarks>
    /// <response code="204">The service was successfully submitted to Active State.</response>
    /// <response code="409">Service is in InCorrect Status</response>
    /// <response code="404">service does not exist.</response>
    [HttpPut]
    [Route("{serviceId}/approveService")]
    [Authorize(Roles = "approve_service_release")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<NoContentResult> ApproveServiceRequest([FromRoute] Guid serviceId)
    {
        await this.WithIamUserId(userId => _serviceBusinessLogic.ApproveServiceRequestAsync(serviceId, userId)).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>
    /// Declines the service request
    /// </summary>
    /// <param name="serviceId" example="D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645">Id of the service that should be declined</param>
    /// <param name="data">the data of the decline request</param>
    /// <remarks>Example: PUT: /api/services/D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645/decline</remarks>
    /// <response code="204">NoContent.</response>
    /// <response code="400">If sub claim is empty/invalid or user does not exist.</response>
    /// <response code="404">If service does not exists.</response>
    [HttpPut]
    [Route("{serviceId:guid}/declineService")]
    [Authorize(Roles = "decline_service_release")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<NoContentResult> DeclineServiceRequest([FromRoute] Guid serviceId, [FromBody] OfferDeclineRequest data)
    {
        await this.WithIamUserId(userId => _serviceBusinessLogic.DeclineServiceRequestAsync(serviceId, userId, data)).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>
    /// Upload document for active service in the marketplace for given serviceId for same company as user
    /// </summary>
    /// <param name="serviceId"></param>
    /// <param name="documentTypeId"></param>
    /// <param name="document"></param>
    /// <param name="cancellationToken"></param>
    /// <remarks>Example: PUT: /api/services/updateservicedoc/{serviceId}/documentType/{documentTypeId}/documents</remarks>
    /// <response code="200">Successfully uploaded the document</response>
    /// <response code="400">If sub claim is empty/invalid or user does not exist, or any other parameters are invalid.</response>
    /// <response code="404">service does not exist.</response>
    /// <response code="403">The user is not assigned with the service.</response>
    /// <response code="415">Only PDF files are supported.</response>
    [HttpPut]
    [Route("updateservicedoc/{serviceId}/documentType/{documentTypeId}/documents")]
    [Authorize(Roles = "add_service_offering")]
    [Consumes("multipart/form-data")]
    [RequestFormLimits(ValueLengthLimit = 819200, MultipartBodyLengthLimit = 819200)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status415UnsupportedMediaType)]
    public Task<int> UpdateServiceDocumentAsync([FromRoute] Guid serviceId, [FromRoute] DocumentTypeId documentTypeId, [FromForm(Name = "document")] IFormFile document, CancellationToken cancellationToken) =>
         this.WithIamUserId(iamUserId => _serviceBusinessLogic.CreateServiceDocumentAsync(serviceId, documentTypeId, document, iamUserId, cancellationToken));
}
