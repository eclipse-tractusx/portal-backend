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

using Org.CatenaX.Ng.Portal.Backend.Apps.Service.BusinessLogic;
using Org.CatenaX.Ng.Portal.Backend.Apps.Service.ViewModels;
using Org.CatenaX.Ng.Portal.Backend.Framework.ErrorHandling;
using Org.CatenaX.Ng.Portal.Backend.Keycloak.Authentication;
using Org.CatenaX.Ng.Portal.Backend.Offers.Library.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Org.CatenaX.Ng.Portal.Backend.Apps.Service.Controllers;

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
    [ProducesResponseType(typeof(IAsyncEnumerable<BusinessAppData>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public IAsyncEnumerable<BusinessAppData> GetAllBusinessAppsForCurrentUserAsync() =>
        this.WithIamUserId(userId => _appsBusinessLogic.GetAllUserUserBusinessAppsAsync(userId));

    /// <summary>
    /// Retrieves app details for an app referenced by id.
    /// </summary>
    /// <param name="appId" example="D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645">ID of the app to retrieve.</param>
    /// <param name="lang" example="en">Optional two character language specifier for the app description. Will be empty if not provided.</param>
    /// <returns>AppDetailsViewModel for requested application.</returns>
    /// <remarks>Example: GET: /api/apps/D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645</remarks>
    /// <response code="200">Returns the requested app details.</response>
    /// <response code="400">If sub claim is empty/invalid.</response>
    /// <response code="404">App not found.</response>
    [HttpGet]
    [Route("{appId}", Name = nameof(GetAppDetailsByIdAsync))]
    [Authorize(Roles = "view_apps")]
    [ProducesResponseType(typeof(AppDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public Task<AppDetailResponse> GetAppDetailsByIdAsync([FromRoute] Guid appId, [FromQuery] string? lang = null) =>
        this.WithIamUserId(userId => _appsBusinessLogic.GetAppDetailsByIdAsync(appId, userId, lang));

    /// <summary>
    /// Creates an app according to input model.
    /// </summary>
    /// <param name="appInputModel">Input model for app creation.</param>
    /// <returns>ID of created application.</returns>
    /// <remarks>Example: POST: /api/apps</remarks>
    /// <response code="201">Returns created app's ID.</response>
    [HttpPost]
    [Route("")]
    [Authorize(Roles = "add_apps")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<CreatedAtRouteResult> CreateAppAsync([FromBody] AppInputModel appInputModel)
    {
        var appId = await _appsBusinessLogic.CreateAppAsync(appInputModel).ConfigureAwait(false);
        return CreatedAtRoute(nameof(GetAppDetailsByIdAsync), new { appId }, appId);
    }

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
    [ProducesResponseType(typeof(IAsyncEnumerable<Guid>), StatusCodes.Status200OK)]
    public IAsyncEnumerable<Guid> GetAllFavouriteAppsForCurrentUserAsync() =>
        this.WithIamUserId(userId => _appsBusinessLogic.GetAllFavouriteAppsForUserAsync(userId));

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
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddFavouriteAppForCurrentUserAsync([FromRoute] Guid appId)
    {
        await this.WithIamUserId(userId => _appsBusinessLogic.AddFavouriteAppForUserAsync(appId, userId)).ConfigureAwait(false);
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
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoveFavouriteAppForCurrentUserAsync([FromRoute] Guid appId)
    {
        await this.WithIamUserId(userId => _appsBusinessLogic.RemoveFavouriteAppForUserAsync(appId, userId)).ConfigureAwait(false);
        return NoContent();
    }
        

    /// <summary>
    /// Retrieves subscription statuses of subscribed apps of the currently logged in user's company.
    /// </summary>
    /// <remarks>Example: GET: /api/apps/subscribed/subscription-status</remarks>
    /// <response code="200">Returns list of applicable app subscription statuses.</response>
    /// <response code="400">If sub claim is empty/invalid or user does not exist.</response>
    [HttpGet]
    [Route("subscribed/subscription-status")]
    [Authorize(Roles = "view_subscription")]
    [ProducesResponseType(typeof(IAsyncEnumerable<(Guid AppId, OfferSubscriptionStatusId AppSubscriptionStatus)>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public IAsyncEnumerable<AppWithSubscriptionStatus> GetCompanySubscribedAppSubscriptionStatusesForCurrentUserAsync() =>
        this.WithIamUserId(userId => _appsBusinessLogic.GetCompanySubscribedAppSubscriptionStatusesForUserAsync(userId));

    /// <summary>
    /// Retrieves subscription statuses of provided apps of the currently logged in user's company.
    /// </summary>
    /// <remarks>Example: GET: /api/apps/provided/subscription-status</remarks>
    /// <response code="200">Returns list of applicable app subscription statuses.</response>
    /// <response code="400">If sub claim is empty/invalid or user does not exist.</response>
    [HttpGet]
    [Route("provided/subscription-status")]
    [Authorize(Roles = "view_app_subscription")]
    [ProducesResponseType(typeof(IAsyncEnumerable<AppCompanySubscriptionStatusData>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public IAsyncEnumerable<AppCompanySubscriptionStatusData> GetCompanyProvidedAppSubscriptionStatusesForCurrentUserAsync() =>
        this.WithIamUserId(userId => _appsBusinessLogic.GetCompanyProvidedAppSubscriptionStatusesForUserAsync(userId));

    /// <summary>
    /// Adds an app to current user's company's subscriptions.
    /// </summary>
    /// <param name="appId" example="D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645">ID of the app to subscribe to.</param>
    /// <param name="offerAgreementConsentData">The agreement consent data</param>
    /// <remarks>Example: POST: /api/apps/D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645/subscribe</remarks>
    /// <response code="204">App was successfully subscribed to.</response>
    /// <response code="400">If sub claim is empty/invalid or user does not exist.</response>
    /// <response code="404">If appId does not exist.</response>
    [HttpPost]
    [Route("{appId}/subscribe")]
    [Authorize(Roles = "subscribe_apps")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddCompanyAppSubscriptionAsync([FromRoute] Guid appId, [FromBody] IEnumerable<OfferAgreementConsentData> offerAgreementConsentData)
    {
        await this.WithIamUserAndBearerToken(auth => _appsBusinessLogic.AddOwnCompanyAppSubscriptionAsync(appId, offerAgreementConsentData, auth.iamUserId, auth.bearerToken));
        return NoContent();
    }

    /// <summary>
    /// Gets all agreements 
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
    /// Activates a pending app subscription for an app provided by the current user's company.
    /// </summary>
    /// <param name="appId" example="D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645">ID of the app to activate subscription for.</param>
    /// <param name="companyId" example="74BA5AEF-1CC7-495F-ABAA-CF87840FA6E2">ID of the company to activate subscription for.</param>
    /// <remarks>Example: PUT: /api/apps/D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645/supscription/company/74BA5AEF-1CC7-495F-ABAA-CF87840FA6E2/activate</remarks>
    /// <response code="204">App subscription was successfully activated.</response>
    /// <response code="400">If sub claim is empty/invalid or user does not exist, or any other parameters are invalid.</response>
    /// <response code="404">App does not exist.</response>
    [HttpPut]
    [Route("{appId}/subscription/company/{companyId}/activate")]
    [Authorize(Roles = "activate_subscription")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateCompanyAppSubscriptionAsync([FromRoute] Guid appId, [FromRoute] Guid companyId) 
    {
        await this.WithIamUserId(userId => _appsBusinessLogic.ActivateOwnCompanyProvidedAppSubscriptionAsync(appId, companyId, userId)).ConfigureAwait(false);
        return NoContent();
    }
    
    /// <summary>
    /// Unsubscribes an app from the current user's company's subscriptions.
    /// </summary>
    /// <param name="appId" example="D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645">ID of the app to unsubscribe from.</param>
    /// <remarks>Example: PUT: /api/apps/D3B1ECA2-6148-4008-9E6C-C1C2AEA5C645/unsubscribe</remarks>
    /// <response code="204">The app was successfully unsubscribed from.</response>
    /// <response code="400">Either the sub claim is empty/invalid, user does not exist or the subscription might not have the correct status or the companyID is incorrect.</response>
    /// <response code="404">App does not exist.</response>
    [HttpPut]
    [Route("{appId}/unsubscribe")]
    [Authorize(Roles = "unsubscribe_apps")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnsubscribeCompanyAppSubscriptionAsync([FromRoute] Guid appId)
    {
        await this.WithIamUserId(userId => _appsBusinessLogic.UnsubscribeOwnCompanyAppSubscriptionAsync(appId, userId)).ConfigureAwait(false);
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
    [ProducesResponseType(typeof(IAsyncEnumerable<AllAppData>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public IAsyncEnumerable<AllAppData> GetAppDataAsync()=>
        this.WithIamUserId(userId => _appsBusinessLogic.GetCompanyProvidedAppsDataForUserAsync(userId));

    /// <summary>
    /// Auto setup the app
    /// </summary>
    /// <remarks>Example: POST: /api/apps/autoSetup</remarks>
    /// <response code="200">Returns the app agreement data.</response>
    /// <response code="400">Offer Subscription is pending or not the providing company.</response>
    /// <response code="404">Offer Subscription not found.</response>
    [HttpPost]
    [Route("autoSetup")]
    [Authorize(Roles = "activate_subscription")]
    [ProducesResponseType(typeof(OfferAutoSetupResponseData), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public Task<OfferAutoSetupResponseData> AutoSetupService([FromBody] OfferAutoSetupData data) =>
        this.WithIamUserId(iamUserId => _appsBusinessLogic.AutoSetupAppAsync(data, iamUserId));
}
