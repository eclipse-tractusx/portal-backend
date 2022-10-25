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

using Org.CatenaX.Ng.Portal.Backend.Apps.Service.ViewModels;
using Org.CatenaX.Ng.Portal.Backend.Framework.Models;
using Org.CatenaX.Ng.Portal.Backend.Offers.Library.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;

namespace Org.CatenaX.Ng.Portal.Backend.Apps.Service.BusinessLogic;

/// <summary>
/// Business logic for handling app-related operations. Includes persistence layer access.
/// </summary>
public interface IAppsBusinessLogic
{
    /// <summary>
    /// Get all active marketplace apps.
    /// </summary>
    /// <param name="languageShortName">Optional two character language specifier for the app description. No description if not provided.</param>
    /// <returns>List of active marketplace apps.</returns>
    public IAsyncEnumerable<AppData> GetAllActiveAppsAsync(string? languageShortName = null);

    /// <summary>
    /// Get all apps that a user has been assigned roles in.
    /// </summary>
    /// <param name="userId">ID of the user to get available apps for.</param>
    /// <returns>List of available apps for user.</returns>
    public IAsyncEnumerable<BusinessAppData> GetAllUserUserBusinessAppsAsync(string userId);

    /// <summary>
    /// Get detailed application data for a single app by id.
    /// </summary>
    /// <param name="appId">Persistence ID of the application to be retrieved.</param>
    /// <param name="iamUserId">ID of the user to evaluate app purchase status for. No company purchase status if not provided.</param>
    /// <param name="languageShortName">Optional two character language specifier for the localization of the app description. No description if not provided.</param>
    /// <returns>AppDetailsViewModel of the requested application.</returns>
    public Task<AppDetailResponse> GetAppDetailsByIdAsync(Guid appId, string iamUserId, string? languageShortName = null);

    /// <summary>
    /// Get IDs of all favourite apps of the user by ID.
    /// </summary>
    /// <param name="userId">ID of the user to get favourite apps for.</param>
    /// <returns>List of IDs of user's favourite apps.</returns>
    public IAsyncEnumerable<Guid> GetAllFavouriteAppsForUserAsync(string userId);

    /// <summary>
    /// Adds an app to a user's favourites.
    /// </summary>
    /// <param name="appId">ID of the app to add to user's favourites.</param>
    /// <param name="userId">ID of the user to add app favourite to.</param>
    public Task AddFavouriteAppForUserAsync(Guid appId, string userId);

    /// <summary>
    /// Removes an app from a user's favourites.
    /// </summary>
    /// <param name="appId">ID of the app to remove from user's favourites.</param>
    /// <param name="userId">ID of the user to remove app favourite from.</param>
    public Task RemoveFavouriteAppForUserAsync(Guid appId, string userId);

    /// <summary>
    /// Retrieves subscription statuses of subscribed apps of the provided user's company.
    /// </summary>
    /// <param name="iamUserId">IAM ID of the user to retrieve app subscription statuses for.</param>
    /// <returns>Async enumberable of user's company's subscribed apps' statuses.</returns>
    public IAsyncEnumerable<AppWithSubscriptionStatus> GetCompanySubscribedAppSubscriptionStatusesForUserAsync(string iamUserId);

    /// <summary>
    /// Retrieves subscription statuses of provided apps of the provided user's company.
    /// </summary>
    /// <param name="iamUserId">IAM ID of the user to retrieve app subscription statuses for.</param>
    /// <returns>Async enumberable of user's company's provided apps' statuses.</returns>
    public IAsyncEnumerable<AppCompanySubscriptionStatusData> GetCompanyProvidedAppSubscriptionStatusesForUserAsync(string iamUserId);

    /// <summary>
    /// Adds a subscription relation between an application and a user's company.
    /// </summary>
    /// <param name="appId">ID of the app to subscribe to.</param>
    /// <param name="iamUserId">ID of the user that initiated app subscription for their company.</param>
    /// <param name="accessToken">Access token of the current User</param>
    public Task<Guid> AddOwnCompanyAppSubscriptionAsync(Guid appId, string iamUserId, string accessToken);

    /// <summary>
    /// Activates a pending app subscription for an app provided by the current user's company.
    /// </summary>
    /// <param name="appId">ID of the pending app to be activated.</param>
    /// <param name="subscribingCompanyId">ID of the company subscribing the app.</param>
    /// <param name="iamUserId">IAM ID of the user requesting the activation.</param>
    public Task ActivateOwnCompanyProvidedAppSubscriptionAsync(Guid appId, Guid subscribingCompanyId, string iamUserId);

    /// <summary>
    /// Unsubscribes an app for the current users company.
    /// </summary>
    /// <param name="appId">ID of the app to unsubscribe from.</param>
    /// <param name="iamUserId">ID of the user that initiated app unsubscription for their company.</param>
    public Task UnsubscribeOwnCompanyAppSubscriptionAsync(Guid appId, string iamUserId);

    /// <summary>
    /// Creates an application and returns its generated ID.
    /// </summary>
    /// <param name="appInputModel">Input model for app creation.</param>
    /// <returns>Guid of the created app.</returns>
    public Task<Guid> CreateAppAsync(AppInputModel appInputModel);

    /// <summary>
    /// Retrieve Company Owned App Data
    /// </summary>
    /// <param name="userId">IAM ID of the user to retrieve own company app.</param>
    /// <returns>Async enumberable of company owned apps data</returns>
    IAsyncEnumerable<AllAppData> GetCompanyProvidedAppsDataForUserAsync(string userId);
    
    /// <summary>
    /// Creates an application and returns its generated ID.
    /// </summary>
    /// <param name="appRequestModel"></param>
    /// <returns>Guid of the created app.</returns>
    Task<Guid> AddAppAsync(AppRequestModel appRequestModel);

    /// <summary>
    /// Retrieves all in review status apps in the marketplace.
    /// </summary>
    Task<Pagination.Response<InReviewAppData>> GetAllInReviewStatusAppsAsync(int page = 0, int size = 15);
    
    /// <summary>
    /// Update app status and create notification
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="iamUserId"></param>
    /// <returns></returns>
    Task SubmitAppReleaseRequestAsync(Guid appId, string iamUserId);

    /// <summary>
    /// Auto setup the app.
    /// </summary>
    /// <param name="data">The offer subscription id and url for the service</param>
    /// <param name="iamUserId">Id of the iam user</param>
    /// <returns>Returns the response data</returns>
    Task<OfferAutoSetupResponseData> AutoSetupAppAsync(OfferAutoSetupData data, string iamUserId);
}
