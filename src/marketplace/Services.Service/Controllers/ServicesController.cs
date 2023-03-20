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
    [ProducesResponseType(typeof(ServiceDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public Task<ServiceDetailResponse> GetServiceDetails([FromRoute] Guid serviceId, [FromQuery] string? lang = "en") =>
        this.WithIamUserId(iamUserId => _serviceBusinessLogic.GetServiceDetailsAsync(serviceId, lang!, iamUserId));

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
    [Obsolete("Will be removed in the future, please use /start-autoSetup in the future")]
    [HttpPost]
    [Route("autoSetup")]
    [Authorize(Roles = "activate_subscription")]
    [ProducesResponseType(typeof(OfferAutoSetupResponseData), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<OfferAutoSetupResponseData> AutoSetupService([FromBody] OfferAutoSetupData data)
        => await this.WithIamUserId(iamUserId => _serviceBusinessLogic.AutoSetupServiceAsync(data, iamUserId));

    /// <summary>
    /// Auto setup the app
    /// </summary>
    /// <remarks>Example: POST: /api/apps/start-autoSetup</remarks>
    /// <response code="200">Returns the app agreement data.</response>
    /// <response code="400">Offer Subscription is pending or not the providing company.</response>
    /// <response code="404">Offer Subscription not found.</response>
    [HttpPost]
    [Route("start-autoSetup")]
    [Authorize(Roles = "activate_subscription")]
    [ProducesResponseType(typeof(OfferAutoSetupResponseData), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<NoContentResult> AutoSetupAppProcess([FromBody] OfferAutoSetupData data)
    {
        await this.WithIamUserId(iamUserId => _serviceBusinessLogic.StartAutoSetupAsync(data, iamUserId)).ConfigureAwait(false);
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
    [ProducesResponseType(typeof(Pagination.Response<OfferCompanySubscriptionStatusResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public Task<Pagination.Response<OfferCompanySubscriptionStatusResponse>> GetCompanyProvidedServiceSubscriptionStatusesForCurrentUserAsync([FromQuery] int page = 0, [FromQuery] int size = 15, [FromQuery] SubscriptionStatusSorting? sorting = null, [FromQuery] OfferSubscriptionStatusId? statusId = null, [FromQuery] Guid? offerId = null) =>
        this.WithIamUserId(userId => _serviceBusinessLogic.GetCompanyProvidedServiceSubscriptionStatusesForUserAsync(page, size, userId, sorting, statusId, offerId));

    /// <summary>
    /// Retrieve Document Content for Service by ID
    /// </summary>
    /// <param name="serviceId"></param>
    /// <param name="documentId"></param>
    /// <param name="cancellationToken">the cancellationToken (generated by the framework)</param>
    /// <remarks>Example: GET: /api/services/{serviceId}/serviceDocuments/{documentId}</remarks>
    /// <response code="200">Returns the document Content</response>
    /// <response code="400">Document / Service id not found or document type not supported.</response>
    /// <response code="404">document not found.</response>
    /// <response code="415">UnSupported Media Type.</response>
    [HttpGet]
    [Authorize(Roles = "view_documents")]
    [Route("{serviceId}/serviceDocuments/{documentId}")]
    [Produces("image/jpeg", "image/png", "image/gif", "image/svg+xml", "image/tiff", "application/pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status415UnsupportedMediaType)]
    public async Task<FileResult> GetServiceDocumentContentAsync([FromRoute] Guid serviceId, [FromRoute] Guid documentId, CancellationToken cancellationToken)
    {
        var (content, contentType, fileName) = await _serviceBusinessLogic.GetServiceDocumentContentAsync(serviceId, documentId, cancellationToken).ConfigureAwait(false);
        return File(content, contentType, fileName);
    }

    /// <summary>
    /// Retrieves all in review status service in the marketplace .
    /// </summary>
    /// <param name="page">page index start from 0</param>
    /// <param name="size">size to get number of records</param>
    /// <param name="sorting">sort by</param>
    /// <param name="offerName">search by status Id</param>
    /// <param name="statusId">search by status Id</param>
    /// <returns>Collection of all in review status marketplace service.</returns>
    /// <remarks>Example: GET: /api/services/provided</remarks>
    /// <response code="200">Returns the list of service filtered by offer status Id.</response>
    [HttpGet]
    [Route("provided")]
    [Authorize(Roles = "add_service_offering")]
    [ProducesResponseType(typeof(Pagination.Response<AllOfferStatusData>), StatusCodes.Status200OK)]
    public Task<Pagination.Response<AllOfferStatusData>> GetCompanyProvidedServiceStatusDataAsync([FromQuery] int page = 0, [FromQuery] int size = 15, [FromQuery] OfferSorting? sorting = null, [FromQuery] string? offerName = null, [FromQuery] ServiceStatusIdFilter? statusId = null) =>
        this.WithIamUserId(userId => _serviceBusinessLogic.GetCompanyProvidedServiceStatusDataAsync(page, size, userId, sorting, offerName, statusId));

    /// <summary>
    /// Retrieves the details of a subscription
    /// </summary>
    /// <param name="serviceId">id of the service to receive the details for</param>
    /// <param name="subscriptionId">id of the subscription to receive the details for</param>
    /// <remarks>Example: GET: /api/services/{serviceId}/subscription/{subscriptionId}/provider</remarks>
    /// <response code="200">Returns the subscription details for the provider</response>
    /// <response code="403">User's company does not provide the service.</response>
    /// <response code="404">No service or subscription found.</response>
    [HttpGet]
    [Authorize(Roles = "add_service_offering")]
    [Route("{serviceId}/subscription/{subscriptionId}/provider")]
    [ProducesResponseType(typeof(ProviderSubscriptionDetailData), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public Task<ProviderSubscriptionDetailData> GetSubscriptionDetailForProvider([FromRoute] Guid serviceId, [FromRoute] Guid subscriptionId) =>
        this.WithIamUserId(iamUserId => _serviceBusinessLogic.GetSubscriptionDetailForProvider(serviceId, subscriptionId, iamUserId));

    /// <summary>
    /// Retrieves the details of a subscription
    /// </summary>
    /// <param name="serviceId">id of the service to receive the details for</param>
    /// <param name="subscriptionId">id of the subscription to receive the details for</param>
    /// <remarks>Example: GET: /api/services/{serviceId}/subscription/{subscriptionId}/subscriber</remarks>
    /// <response code="200">Returns the subscription details for the subscriber</response>
    /// <response code="403">User's company does not provide the service.</response>
    /// <response code="404">No service or subscription found.</response>
    [HttpGet]
    [Authorize(Roles = "add_service_offering")]
    [Route("{serviceId}/subscription/{subscriptionId}/subscriber")]
    [ProducesResponseType(typeof(SubscriberSubscriptionDetailData), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public Task<SubscriberSubscriptionDetailData> GetSubscriptionDetailForSubscriber([FromRoute] Guid serviceId, [FromRoute] Guid subscriptionId) =>
        this.WithIamUserId(iamUserId => _serviceBusinessLogic.GetSubscriptionDetailForSubscriber(serviceId, subscriptionId, iamUserId));

    /// <summary>
    /// Retrieves subscription statuses of services.
    /// </summary>
    /// <remarks>Example: GET: /api/services/subscribed/subscription-status</remarks>
    /// <response code="200">Returns list of applicable service subscription statuses.</response>
    /// <response code="400">If sub claim is empty/invalid or user does not exist.</response>
    [HttpGet]
    [Route("subscribed/subscription-status")]
    [Authorize(Roles = "view_subscription")]
    [ProducesResponseType(typeof(Pagination.Response<OfferSubscriptionStatusDetailData>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public Task<Pagination.Response<OfferSubscriptionStatusDetailData>> GetCompanySubscribedServiceSubscriptionStatusesForUserAsync([FromQuery] int page = 0, [FromQuery] int size = 15) =>
        this.WithIamUserId(userId => _serviceBusinessLogic.GetCompanySubscribedServiceSubscriptionStatusesForUserAsync(page, size, userId));
}
