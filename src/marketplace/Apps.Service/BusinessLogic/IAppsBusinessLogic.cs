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

using Org.Eclipse.TractusX.Portal.Backend.Apps.Service.ViewModels;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Offers.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Apps.Service.BusinessLogic;

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
    public IAsyncEnumerable<AppData> GetAllActiveAppsAsync(string? languageShortName);

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
    /// <returns>Async enumerable of user's company's subscribed apps' statuses.</returns>
    public IAsyncEnumerable<AppWithSubscriptionStatus> GetCompanySubscribedAppSubscriptionStatusesForUserAsync(string iamUserId);

    /// <summary>
    /// Retrieves subscription statuses of provided apps of the provided user's company.
    /// </summary>
    /// <param name="page"></param>
    /// <param name="size"></param>
    /// <param name="iamUserId">IAM ID of the user to retrieve app subscription statuses for.</param>
    /// <param name="sorting"></param>
    /// <param name="statusId"></param>
    /// <param name="offerId"></param>
    /// <returns>Async enumberable of user's company's provided apps' statuses.</returns>
    public Task<Pagination.Response<OfferCompanySubscriptionStatusResponse>> GetCompanyProvidedAppSubscriptionStatusesForUserAsync(int page, int size, string iamUserId, SubscriptionStatusSorting? sorting, OfferSubscriptionStatusId? statusId, Guid? offerId);

    /// <summary>
    /// Adds a subscription relation between an application and a user's company.
    /// </summary>
    /// <param name="appId">ID of the app to subscribe to.</param>
    /// <param name="offerAgreementConsentData">The agreement consent data</param>
    /// <param name="iamUserId">ID of the user that initiated app subscription for their company.</param>
    /// <param name="accessToken">Access token of the current User</param>
    public Task<Guid> AddOwnCompanyAppSubscriptionAsync(Guid appId, IEnumerable<OfferAgreementConsentData> offerAgreementConsentData, string iamUserId, string accessToken);

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
    /// Retrieve Company Owned App Data
    /// </summary>
    /// <param name="userId">IAM ID of the user to retrieve own company app.</param>
    /// <returns>Async enumberable of company owned apps data</returns>
    IAsyncEnumerable<AllOfferData> GetCompanyProvidedAppsDataForUserAsync(string userId);

    /// <summary>
    /// Auto setup the app.
    /// </summary>
    /// <param name="data">The offer subscription id and url for the service</param>
    /// <param name="iamUserId">Id of the iam user</param>
    /// <returns>Returns the response data</returns>
    Task<OfferAutoSetupResponseData> AutoSetupAppAsync(OfferAutoSetupData data, string iamUserId);

    /// <summary>
    /// Gets the app agreement data
    /// </summary>
    /// <param name="appId">Id of the app to get the agreements for</param>
    /// <returns>Returns IAsyncEnumerable of agreement data</returns>
    IAsyncEnumerable<AgreementData> GetAppAgreement(Guid appId);

    /// <summary>
    /// Deactivate Offer Status by appId
    /// </summary>
    /// <param name="appId">Id of the app</param>
    /// <param name="iamUserId">Id of the iamUser</param>
    public Task DeactivateOfferByAppIdAsync(Guid appId, string iamUserId);

    /// <summary>
    /// Retrieve Document Content for document type  by ID
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="documentId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>byte Array Content</returns>
    Task<(byte[] Content, string ContentType, string FileName)> GetAppDocumentContentAsync(Guid appId, Guid documentId, CancellationToken cancellationToken);

    /// <summary>
    /// Create OfferAssigned AppLeadImage Document by appId
    /// </summary>
    /// <param name="appId">Id of the app</param>
    /// <param name="iamUserId">Id of the iamUser</param>
    /// <param name="document">Document Data</param>
    /// <param name="cancellationToken">cancellationToken</param>
    Task CreateOfferAssignedAppLeadImageDocumentByIdAsync(Guid appId, string iamUserId, IFormFile document, CancellationToken cancellationToken);
    
    /// <summary>
    /// Get technical user profiles for a specific offer
    /// </summary>
    /// <param name="offerId">Id of the offer</param>
    /// <param name="iamUserId">Id of the iam user</param>
    /// <returns>AsyncEnumerable with the technical user profile information</returns>
    Task<IEnumerable<TechnicalUserProfileInformation>> GetTechnicalUserProfilesForOffer(Guid offerId, string iamUserId);
    
    /// <summary>
    /// Creates or updates the technical user profiles
    /// </summary>
    /// <param name="appId">Id of the app</param>
    /// <param name="data">The technical user profiles</param>
    /// <param name="iamUserId">Id of the iam user</param>
    Task UpdateTechnicalUserProfiles(Guid appId, IEnumerable<TechnicalUserProfileData> data, string iamUserId);

    /// <summary>
    /// Gets the information for the subscription
    /// </summary>
    /// <param name="appId">Id of the app</param>
    /// <param name="subscriptionId">Id of the subscription</param>
    /// <param name="iamUserId">Id of the iam user</param>
    /// <returns>Returns the details of the subscription</returns>
    Task<ProviderSubscriptionDetailData> GetSubscriptionDetailForProvider(Guid appId, Guid subscriptionId, string iamUserId);
    
    /// <summary>
    /// Gets the information for the subscription
    /// </summary>
    /// <param name="appId">Id of the app</param>
    /// <param name="subscriptionId">Id of the subscription</param>
    /// <param name="iamUserId">Id of the iam user</param>
    /// <returns>Returns the details of the subscription</returns>
    Task<SubscriberSubscriptionDetailData> GetSubscriptionDetailForSubscriber(Guid appId, Guid subscriptionId, string iamUserId);
}
