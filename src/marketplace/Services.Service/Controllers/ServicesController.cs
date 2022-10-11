/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
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

using Org.CatenaX.Ng.Portal.Backend.Framework.ErrorHandling;
using Org.CatenaX.Ng.Portal.Backend.Framework.Models;
using Org.CatenaX.Ng.Portal.Backend.Keycloak.Authentication;
using Org.CatenaX.Ng.Portal.Backend.Offers.Library.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.CatenaX.Ng.Portal.Backend.Services.Service.BusinessLogic;
using Org.CatenaX.Ng.Portal.Backend.Services.Service.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Org.CatenaX.Ng.Portal.Backend.Services.Service.Controllers;

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
    /// <returns>Collection of all active services.</returns>
    /// <remarks>Example: GET: /api/services/active</remarks>
    /// <response code="200">Returns the list of all active services.</response>
    [HttpGet]
    [Route("active")]
    [Authorize(Roles = "view_service_offering")]
    [ProducesResponseType(typeof(Pagination.Response<ServiceOverviewData>), StatusCodes.Status200OK)]
    public Task<Pagination.Response<ServiceOverviewData>> GetAllActiveServicesAsync([FromQuery] int page = 0, [FromQuery] int size = 15) =>
        _serviceBusinessLogic.GetAllActiveServicesAsync(page, size);

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
    public async Task<CreatedAtRouteResult> CreateServiceOffering([FromBody] OfferingData data)
    {
        var id = await this.WithIamUserId(iamUserId => _serviceBusinessLogic.CreateServiceOfferingAsync(data, iamUserId)).ConfigureAwait(false);
        return CreatedAtRoute(nameof(GetServiceDetails), new { serviceId = id }, id);
    }
    
    /// <summary>
    /// Adds a new service subscription.
    /// </summary>
    /// <param name="serviceId" example="D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645">Id for the service the user wants to subscribe to.</param>
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
    public async Task<CreatedAtRouteResult> AddServiceSubscription([FromRoute] Guid serviceId)
    {
        var serviceSubscriptionId = await this.WithIamUserAndBearerToken(auth => _serviceBusinessLogic.AddServiceSubscription(serviceId, auth.iamUserId, auth.bearerToken)).ConfigureAwait(false);
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
    /// Adds a new service subscription.
    /// </summary>
    /// <param name="serviceId" example="D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645">Id for the service the wants to retrieve.</param>
    /// <param name="lang" example="de">OPTIONAL: Short code for the language the translatable text should be returned in.</param>
    /// <remarks>Example: Get: /api/services/D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645</remarks>
    /// <response code="200">Returns the service details.</response>
    /// <response code="404">Service was not found.</response>
    [HttpGet]
    [Route("{serviceId}", Name = nameof(GetServiceDetails))]
    [Authorize(Roles = "view_service_offering")]
    [ProducesResponseType(typeof(OfferDetailData), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public Task<OfferDetailData> GetServiceDetails([FromRoute] Guid serviceId, [FromQuery] string? lang = "en") => 
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
    public async Task<NoContentResult> CreateOrUpdateServiceAgreementConsents([FromRoute] Guid subscriptionId, [FromBody] IEnumerable<ServiceAgreementConsentData> serviceAgreementConsentDatas)
    {
        await this.WithIamUserId(iamUserId => _serviceBusinessLogic.CreateOrUpdateServiceAgreementConsentAsync(subscriptionId, serviceAgreementConsentDatas, iamUserId).ConfigureAwait(false));
        return this.NoContent();
    }

    /// <summary>
    /// Creates new service agreement consents 
    /// </summary>
    /// <param name="subscriptionId" example="D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645">Id for the service subscription the consent should get set for.</param>
    /// <param name="serviceAgreementConsentData">the service agreement consent.</param>
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
    public async Task<CreatedAtRouteResult> CreateServiceAgreementConsent([FromRoute] Guid subscriptionId, [FromBody] ServiceAgreementConsentData serviceAgreementConsentData)
    {
        var consentId = await this.WithIamUserId(iamUserId =>
            _serviceBusinessLogic.CreateServiceAgreementConsentAsync(subscriptionId, serviceAgreementConsentData, iamUserId)
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
}
