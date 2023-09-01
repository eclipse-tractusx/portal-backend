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
using Org.Eclipse.TractusX.Portal.Backend.Apps.Service.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Apps.Service.ViewModels;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Library;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.PublicInfos;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Authentication;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Apps.Service.Controllers;

/// <summary>
/// Controller providing actions for displaying, filtering and updating applications and user assigned favourites.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[Consumes("application/json")]
public class AppsController : ControllerBase
{
    private readonly IAppsBusinessLogic _appsBusinessLogic;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="appsBusinessLogic">Logic dependency.</param>
    public AppsController(IAppsBusinessLogic appsBusinessLogic)
    {
        _appsBusinessLogic = appsBusinessLogic;
    }

    /// <summary>
    /// Retrieves all active apps in the marketplace.
    /// </summary>
    /// <param name="lang" example="en">Optional two character language specifier for the app description. Will be empty if not provided.</param>
    /// <returns>Collection of all active marketplace apps.</returns>
    /// <remarks>Example: GET: /api/apps/active</remarks>
    /// <response code="200">Returns the list of all active marketplace apps.</response>
    [HttpGet]
    [Route("active")]
    [Authorize(Roles = "view_apps")]
    [ProducesResponseType(typeof(IAsyncEnumerable<AppData>), StatusCodes.Status200OK)]
    public IAsyncEnumerable<AppData> GetAllActiveAppsAsync([FromQuery] string? lang = null) =>
        _appsBusinessLogic.GetAllActiveAppsAsync(lang);

    /// <summary>
    /// Get all apps that currently logged in user has been assigned roles in.
    /// </summary>
    /// <returns>Collection of BusinessAppViewModels user has been assigned active roles in.</returns>
    /// <remarks>Example: GET: /api/apps/business</remarks>
    /// <response code="200">Returns the list of the user's business apps.</response>
    [HttpGet]
    [Route("business")]
    [Authorize(Roles = "view_apps")]
    [Authorize(Policy = PolicyTypes.CompanyUser)]
    [ProducesResponseType(typeof(IAsyncEnumerable<BusinessAppData>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public IAsyncEnumerable<BusinessAppData> GetAllBusinessAppsForCurrentUserAsync() =>
        this.WithUserId(userId => _appsBusinessLogic.GetAllUserUserBusinessAppsAsync(userId));

    /// <summary>
    /// Retrieves app details for an app referenced by id.
    /// </summary>
    /// <param name="appId" example="D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645">ID of the app to retrieve.</param>
    /// <param name="lang" example="en">Optional two character language specifier for the app description. Will be empty if not provided.</param>
    /// <returns>AppDetailsViewModel for requested application.</returns>
    /// <remarks>Example: GET: /api/apps/D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645</remarks>
    /// <response code="200">Returns the requested app details.</response>
    /// <response code="404">App not found.</response>
    [HttpGet]
    [Route("{appId}", Name = nameof(GetAppDetailsByIdAsync))]
    [Authorize(Roles = "view_apps")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [ProducesResponseType(typeof(AppDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public Task<AppDetailResponse> GetAppDetailsByIdAsync([FromRoute] Guid appId, [FromQuery] string? lang = null) =>
        this.WithCompanyId(companyId => _appsBusinessLogic.GetAppDetailsByIdAsync(appId, companyId, lang));

    /// <summary>
    /// Retrieves IDs of all favourite apps of the current user (by sub claim).
    /// </summary>
    /// <returns>Collection of IDs of favourite apps.</returns>
    /// <remarks>Example: GET: /api/apps/favourites</remarks>
    /// <response code="200">Returns the list of favourite apps of current user.</response>
    /// <response code="400">If sub claim is empty/invalid.</response>
    [HttpGet]
    [Route("favourites")]
    [Authorize(Roles = "view_apps")]
    [Authorize(Policy = PolicyTypes.ValidIdentity)]
    [ProducesResponseType(typeof(IAsyncEnumerable<Guid>), StatusCodes.Status200OK)]
    public IAsyncEnumerable<Guid> GetAllFavouriteAppsForCurrentUserAsync() =>
        this.WithUserId(userId => _appsBusinessLogic.GetAllFavouriteAppsForUserAsync(userId));

    /// <summary>
    /// Adds an app to current user's favourites.
    /// </summary>
    /// <param name="appId" example="D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645">ID of the app to add to user favourites.</param>
    /// <remarks>Example: POST: /api/apps/D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645/favourite</remarks>
    /// <response code="204">Favourite app was successfully added to user.</response>
    /// <response code="400">If sub claim is empty/invalid or user does not exist.</response>
    [HttpPost]
    [Route("{appId}/favourite")]
    [Authorize(Roles = "view_apps")]
    [Authorize(Policy = PolicyTypes.ValidIdentity)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddFavouriteAppForCurrentUserAsync([FromRoute] Guid appId)
    {
        await this.WithUserId(userId => _appsBusinessLogic.AddFavouriteAppForUserAsync(appId, userId)).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>
    /// Removes an app from current user's favourites.
    /// </summary>
    /// <param name="appId" example="D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645">ID of the app to remove from user favourites.</param>
    /// <remarks>Example: DELETE: /api/apps/D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645/favourite</remarks>
    /// <response code="204">Favourite app was successfully removed from user.</response>
    /// <response code="400">If sub claim is empty/invalid or user does not exist.</response>
    [HttpDelete]
    [Route("{appId}/favourite")]
    [Authorize(Roles = "view_apps")]
    [Authorize(Policy = PolicyTypes.ValidIdentity)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoveFavouriteAppForCurrentUserAsync([FromRoute] Guid appId)
    {
        await this.WithUserId(userId => _appsBusinessLogic.RemoveFavouriteAppForUserAsync(appId, userId)).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>
    /// Retrieves subscription statuses of apps.
    /// </summary>
    /// <remarks>Example: GET: /api/apps/subscribed/subscription-status</remarks>
    /// <response code="200">Returns list of applicable apps subscription statuses.</response>
    /// <response code="400">If sub claim is empty/invalid or user does not exist.</response>
    [HttpGet]
    [Route("subscribed/subscription-status")]
    [Authorize(Roles = "view_subscription")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [ProducesResponseType(typeof(Pagination.Response<OfferSubscriptionStatusDetailData>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public Task<Pagination.Response<OfferSubscriptionStatusDetailData>> GetCompanySubscribedAppSubscriptionStatusesForUserAsync([FromQuery] int page = 0, [FromQuery] int size = 15) =>
        this.WithCompanyId(companyId => _appsBusinessLogic.GetCompanySubscribedAppSubscriptionStatusesForUserAsync(page, size, companyId));

    /// <summary>
    /// Retrieves subscription statuses of provided apps of the currently logged in user's company.
    /// </summary>
    /// <remarks>Example: GET: /api/apps/provided/subscription-status</remarks>
    /// <response code="200">Returns list of applicable app subscription statuses.</response>
    [HttpGet]
    [Route("provided/subscription-status")]
    [Authorize(Roles = "view_app_subscription")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [ProducesResponseType(typeof(Pagination.Response<OfferCompanySubscriptionStatusResponse>), StatusCodes.Status200OK)]
    public Task<Pagination.Response<OfferCompanySubscriptionStatusResponse>> GetCompanyProvidedAppSubscriptionStatusesForCurrentUserAsync([FromQuery] int page = 0, [FromQuery] int size = 15, [FromQuery] SubscriptionStatusSorting? sorting = null, [FromQuery] OfferSubscriptionStatusId? statusId = null, [FromQuery] Guid? offerId = null) =>
        this.WithCompanyId(companyId => _appsBusinessLogic.GetCompanyProvidedAppSubscriptionStatusesForUserAsync(page, size, companyId, sorting, statusId, offerId));

    /// <summary>
    /// Adds an app to current user's company's subscriptions.
    /// </summary>
    /// <param name="appId" example="D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645">ID of the app to subscribe to.</param>
    /// <param name="offerAgreementConsentData">The agreement consent data</param>
    /// <remarks>Example: POST: /api/apps/D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645/subscribe</remarks>
    /// <response code="204">App was successfully subscribed to.</response>
    /// <response code="400">If sub claim is empty/invalid or user does not exist.</response>
    /// <response code="404">If appId does not exist.</response>
    /// <response code="409"></response>
    /// <response code="500">Internal Server Error</response>
    [HttpPost]
    [Route("{appId}/subscribe")]
    [Authorize(Roles = "subscribe_apps")]
    [Authorize(Policy = PolicyTypes.ValidIdentity)]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddCompanyAppSubscriptionAsync([FromRoute] Guid appId, [FromBody] IEnumerable<OfferAgreementConsentData> offerAgreementConsentData)
    {
        await this.WithUserIdAndCompanyId(identity => _appsBusinessLogic.AddOwnCompanyAppSubscriptionAsync(appId, offerAgreementConsentData, identity));
        return NoContent();
    }

    /// <summary>
    /// Retrieve all app marketplace agreements mandatory to be agreed before releasing an app on the CX marketplace 
    /// </summary>
    /// <param name="appId" example="D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645">Id for the app consent to retrieve.</param>
    /// <remarks>Example: GET: /api/apps/appAgreementData/D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645</remarks>
    /// <response code="200">Returns the app agreement data.</response>
    [HttpGet]
    [Route("appAgreementData/{appId}")]
    [Authorize(Roles = "subscribe_apps")]
    [ProducesResponseType(typeof(AgreementData), StatusCodes.Status200OK)]
    public IAsyncEnumerable<AgreementData> GetAppAgreement([FromRoute] Guid appId) =>
        _appsBusinessLogic.GetAppAgreement(appId);

    /// <summary>
    /// Activates a pending app subscription.
    /// </summary>
    /// <param name="subscriptionId" example="D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645">ID of the subscription to activate.</param>
    /// <remarks>Example: PUT: /api/apps/supscription/{subscriptiondId}/activate</remarks>
    /// <response code="204">App subscription was successfully activated.</response>
    /// <response code="400">If sub claim is empty/invalid or user does not exist, or any other parameters are invalid.</response>
    /// <response code="404">App does not exist.</response>
    /// <response code="409">App Name not set.</response>
    /// <response code="500">Internal Server Error.</response>
    [HttpPut]
    [Route("/subscription/{subscriptionId}/activate")]
    [Authorize(Roles = "activate_subscription")]
    [Authorize(Policy = PolicyTypes.ValidIdentity)]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ActivateCompanyAppSubscriptionAsync([FromRoute] Guid subscriptionId)
    {
        await this.WithUserIdAndCompanyId(identity => _appsBusinessLogic.ActivateOwnCompanyProvidedAppSubscriptionAsync(subscriptionId, identity)).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>
    /// Unsubscribes an app from the current user's company's subscriptions.
    /// </summary>
    /// <param name="subscriptionId" example="D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645">ID of the subscription to unsubscribe from.</param>
    /// <remarks>Example: PUT: /api/apps/{subscriptionId}/unsubscribe</remarks>
    /// <response code="204">The app was successfully unsubscribed from.</response>
    /// <response code="400">Either the sub claim is empty/invalid, user does not exist or the subscription might not have the correct status or the companyID is incorrect.</response>
    /// <response code="404">App does not exist.</response>
    [HttpPut]
    [Route("{subscriptionId}/unsubscribe")]
    [Authorize(Roles = "unsubscribe_apps")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnsubscribeCompanyAppSubscriptionAsync([FromRoute] Guid subscriptionId)
    {
        await this.WithCompanyId(companyId => _appsBusinessLogic.UnsubscribeOwnCompanyAppSubscriptionAsync(subscriptionId, companyId)).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>
    /// Get all company owned apps.
    /// </summary>
    /// <returns>Collection of app data of apps that are provided by the calling users company</returns>
    /// <remarks>Example: GET: /api/apps/provided</remarks>
    /// <response code="200">Returns list of apps provided by the user assigned company.</response>

    [HttpGet]
    [Route("provided")]
    [Authorize(Roles = "app_management")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [ProducesResponseType(typeof(Pagination.Response<AllOfferData>), StatusCodes.Status200OK)]
    public Task<Pagination.Response<AllOfferData>> GetAppDataAsync([FromQuery] int page = 0, [FromQuery] int size = 15, [FromQuery] OfferSorting? sorting = null, [FromQuery] string? offerName = null, [FromQuery] AppStatusIdFilter? statusId = null) =>
        this.WithCompanyId(companyId => _appsBusinessLogic.GetCompanyProvidedAppsDataForUserAsync(page, size, companyId, sorting, offerName, statusId));

    /// <summary>
    /// Auto setup the app
    /// </summary>
    /// <remarks>Example: POST: /api/apps/autoSetup</remarks>
    /// <response code="200">Returns the app agreement data.</response>
    /// <response code="400">Offer Subscription is pending or not the providing company.</response>
    /// <response code="404">Offer Subscription not found.</response>
    [Obsolete("Will be removed in the future, please use /start-autoSetup in the future")]
    [HttpPost]
    [Route("autoSetup")]
    [Authorize(Roles = "activate_subscription")]
    [Authorize(Policy = PolicyTypes.ValidIdentity)]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [ProducesResponseType(typeof(OfferAutoSetupResponseData), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public Task<OfferAutoSetupResponseData> AutoSetupApp([FromBody] OfferAutoSetupData data) =>
        this.WithUserIdAndCompanyId(identity => _appsBusinessLogic.AutoSetupAppAsync(data, identity));

    /// <summary>
    /// Auto setup the app
    /// </summary>
    /// <remarks>Example: POST: /api/apps/start-autoSetup</remarks>
    /// <response code="204">The auto setup has successfully been started.</response>
    /// <response code="400">Offer Subscription is pending or not the providing company.</response>
    /// <response code="404">Offer Subscription not found.</response>
    [HttpPost]
    [Route("start-autoSetup")]
    [Authorize(Roles = "activate_subscription")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [PublicUrl(CompanyRoleId.APP_PROVIDER)]
    public async Task<NoContentResult> StartAutoSetupAppProcess([FromBody] OfferAutoSetupData data)
    {
        await this.WithCompanyId(companyId => _appsBusinessLogic.StartAutoSetupAsync(data, companyId)).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>
    /// Triggers the activation of a single instance app subscription
    /// </summary>
    /// <remarks>Example: PUT: /api/apps/subscription/{offerSubscriptionId}/activate-single-instance</remarks>
    /// <response code="204">The activation of the subscription has successfully been started.</response>
    /// <response code="400">Offer Subscription is pending or not the providing company.</response>
    /// <response code="404">Offer Subscription not found.</response>
    [HttpPut]
    [Route("subscription/{offerSubscriptionId}/activate-single-instance")]
    [Authorize(Roles = "activate_subscription")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<NoContentResult> ActivateSingleInstance([FromRoute] Guid offerSubscriptionId)
    {
        await this.WithCompanyId(companyId => _appsBusinessLogic.ActivateSingleInstance(offerSubscriptionId, companyId)).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>
    /// Retrieve Document Content for document type "App Lead Image" and "App Image" by ID
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="documentId"></param>
    /// <param name="cancellationToken">the cancellationToken (generated by the framework)</param>
    /// <remarks>Example: GET: /api/apps/{appId}/appDocuments/{documentId}</remarks>
    /// <response code="200">Returns the document Content</response>
    /// <response code="400">Document / App id not found or document type not supported.</response>
    /// <response code="404">document not found.</response>
    /// <response code="415">UnSupported Media Type.</response>
    [HttpGet]
    [Authorize(Roles = "view_documents")]
    [Route("{appId}/appDocuments/{documentId}")]
    [Produces("image/jpeg", "image/png", "image/gif", "image/svg+xml", "image/tiff", "application/pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status415UnsupportedMediaType)]
    public async Task<FileResult> GetAppDocumentContentAsync([FromRoute] Guid appId, [FromRoute] Guid documentId, CancellationToken cancellationToken)
    {
        var (content, contentType, fileName) = await _appsBusinessLogic.GetAppDocumentContentAsync(appId, documentId, cancellationToken).ConfigureAwait(false);
        return File(content, contentType, fileName);
    }

    /// <summary>
    /// Retrieves the details of a subscription
    /// </summary>
    /// <param name="appId">id of the app to receive the details for</param>
    /// <param name="subscriptionId">id of the subscription to receive the details for</param>
    /// <remarks>Example: GET: /api/apps/{appId}/subscription/{subscriptionId}/provider</remarks>
    /// <response code="200">Returns the subscription details for the provider</response>
    /// <response code="403">User's company does not provide the app.</response>
    /// <response code="404">No app or subscription found.</response>
    [HttpGet]
    [Authorize(Roles = "app_management")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("{appId}/subscription/{subscriptionId}/provider")]
    [ProducesResponseType(typeof(AppProviderSubscriptionDetailData), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [PublicUrl(CompanyRoleId.APP_PROVIDER)]
    public Task<AppProviderSubscriptionDetailData> GetSubscriptionDetailForProvider([FromRoute] Guid appId, [FromRoute] Guid subscriptionId) =>
        this.WithCompanyId(companyId => _appsBusinessLogic.GetSubscriptionDetailForProvider(appId, subscriptionId, companyId));

    /// <summary>
    /// Retrieves the details of a subscription
    /// </summary>
    /// <param name="appId">id of the app to receive the details for</param>
    /// <param name="subscriptionId">id of the subscription to receive the details for</param>
    /// <remarks>Example: GET: /api/apps/{appId}/subscription/{subscriptionId}/subscriber</remarks>
    /// <response code="200">Returns the subscription details for the subscriber</response>
    /// <response code="403">User's company does not provide the app.</response>
    /// <response code="404">No app or subscription found.</response>
    [HttpGet]
    [Authorize(Roles = "subscribe_apps")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [Route("{appId}/subscription/{subscriptionId}/subscriber")]
    [ProducesResponseType(typeof(SubscriberSubscriptionDetailData), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public Task<SubscriberSubscriptionDetailData> GetSubscriptionDetailForSubscriber([FromRoute] Guid appId, [FromRoute] Guid subscriptionId) =>
        this.WithCompanyId(companyId => _appsBusinessLogic.GetSubscriptionDetailForSubscriber(appId, subscriptionId, companyId));

    /// <summary>
    /// Retrieves Active subscription statuses of apps.
    /// </summary>
    /// <remarks>Example: GET: /api/apps/subscribed/activesubscriptions</remarks>
    /// <response code="200">Returns list of applicable active apps subscription statuses.</response>
    /// <response code="400">If sub claim is empty/invalid or user does not exist.</response>
    [HttpGet]
    [Route("subscribed/activesubscriptions")]
    [Authorize(Roles = "view_subscription")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [ProducesResponseType(typeof(ActiveOfferSubscriptionStatusData), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public IAsyncEnumerable<ActiveOfferSubscriptionStatusData> GetOwnCompanyActiveSubscribedAppSubscriptionStatusesForUserAsync() =>
        this.WithCompanyId(companyId => _appsBusinessLogic.GetOwnCompanyActiveSubscribedAppSubscriptionStatusesForUserAsync(companyId));

    /// <summary>
    /// Retrieves subscription statuses of apps.
    /// </summary>
    /// <remarks>Example: GET: /api/apps/subscribed/subscriptions</remarks>
    /// <response code="200">Returns list of applicable active apps subscription statuses.</response>
    /// <response code="400">If sub claim is empty/invalid or user does not exist.</response>
    [HttpGet]
    [Route("subscribed/subscriptions")]
    [Authorize(Roles = "view_subscription")]
    [Authorize(Policy = PolicyTypes.ValidCompany)]
    [ProducesResponseType(typeof(OfferSubscriptionData), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public IAsyncEnumerable<OfferSubscriptionData> GetOwnCompanySubscribedAppOfferSubscriptionDataForUserAsync() =>
        this.WithCompanyId(companyId => _appsBusinessLogic.GetOwnCompanySubscribedAppOfferSubscriptionDataForUserAsync(companyId));

}
