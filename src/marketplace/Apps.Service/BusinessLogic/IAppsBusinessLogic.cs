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
    /// <param name="companyId">Id of the user to get available apps for.</param>
    /// <returns>List of available apps for user.</returns>
    public IAsyncEnumerable<BusinessAppData> GetAllUserUserBusinessAppsAsync(Guid companyId);

    /// <summary>
    /// Get detailed application data for a single app by id.
    /// </summary>
    /// <param name="appId">Persistence ID of the application to be retrieved.</param>
    /// <param name="companyId">Id of the user to evaluate app purchase status for. No company purchase status if not provided.</param>
    /// <param name="languageShortName">Optional two character language specifier for the localization of the app description. No description if not provided.</param>
    /// <returns>AppDetailsViewModel of the requested application.</returns>
    public Task<AppDetailResponse> GetAppDetailsByIdAsync(Guid appId, Guid companyId, string? languageShortName = null);

    /// <summary>
    /// Get IDs of all favourite apps of the user by ID.
    /// </summary>
    /// <param name="userId">Id of the user to get favourite apps for.</param>
    /// <returns>List of IDs of user's favourite apps.</returns>
    public IAsyncEnumerable<Guid> GetAllFavouriteAppsForUserAsync(Guid userId);

    /// <summary>
    /// Adds an app to a user's favourites.
    /// </summary>
    /// <param name="appId">ID of the app to add to user's favourites.</param>
    /// <param name="userId">Id of the user to add app favourite to.</param>
    public Task AddFavouriteAppForUserAsync(Guid appId, Guid userId);

    /// <summary>
    /// Removes an app from a user's favourites.
    /// </summary>
    /// <param name="appId">ID of the app to remove from user's favourites.</param>
    /// <param name="userId">Id of the user to remove app favourite from.</param>
    public Task RemoveFavouriteAppForUserAsync(Guid appId, Guid userId);

    /// <summary>
    /// Retrieves subscription statuses of subscribed apps of the provided user's company.
    /// </summary>
    /// <param name="companyId">Id of the users company.</param>
    /// <param name ="page">page</param>
    /// <param name ="size">size</param>
    /// <returns>Returns the details of the subscription status for App user</returns>
    public Task<Pagination.Response<OfferSubscriptionStatusDetailData>> GetCompanySubscribedAppSubscriptionStatusesForUserAsync(int page, int size, Guid companyId);

    /// <summary>
    /// Retrieves subscription statuses of provided apps of the provided user's company.
    /// </summary>
    /// <param name="page"></param>
    /// <param name="size"></param>
    /// <param name="companyId"></param>
    /// <param name="sorting"></param>
    /// <param name="statusId"></param>
    /// <param name="offerId"></param>
    /// <returns>Async enumberable of user's company's provided apps' statuses.</returns>
    public Task<Pagination.Response<OfferCompanySubscriptionStatusResponse>> GetCompanyProvidedAppSubscriptionStatusesForUserAsync(int page, int size, Guid companyId, SubscriptionStatusSorting? sorting, OfferSubscriptionStatusId? statusId, Guid? offerId);

    /// <summary>
    /// Adds a subscription relation between an application and a user's company.
    /// </summary>
    /// <param name="appId">ID of the app to subscribe to.</param>
    /// <param name="offerAgreementConsentData">The agreement consent data</param>
    /// <param name="identity">Identity of the user that initiated app subscription for their company.</param>
    public Task<Guid> AddOwnCompanyAppSubscriptionAsync(Guid appId, IEnumerable<OfferAgreementConsentData> offerAgreementConsentData, (Guid UserId, Guid CompanyId) identity);

    /// <summary>
    /// Activates a pending app subscription for an app provided by the current user's company.
    /// </summary>
    /// <param name="subscriptionId">ID of the pending app to be activated.</param>
    /// <param name="identity">identity of the current user.</param>
    [Obsolete("This Method is not used anymore")]
    public Task ActivateOwnCompanyProvidedAppSubscriptionAsync(Guid subscriptionId, (Guid UserId, Guid CompanyId) identity);

    /// <summary>
    /// Unsubscribes an app for the current users company.
    /// </summary>
    /// <param name="subscriptionId">ID of the subscription to unsubscribe from.</param>
    /// <param name="companyId">Id of the users company that initiated app unsubscription.</param>
    public Task UnsubscribeOwnCompanyAppSubscriptionAsync(Guid subscriptionId, Guid companyId);

    /// <summary>
    /// Retrieve Company Owned App Data
    /// </summary>
    /// <param name="companyId">Id of the users company to retrieve own app.</param>
    /// <returns>Async enumerable of company owned apps data</returns>
    IAsyncEnumerable<AllOfferData> GetCompanyProvidedAppsDataForUserAsync(Guid companyId);

    /// <summary>
    /// Auto setup the app.
    /// </summary>
    /// <param name="data">The offer subscription id and url for the service</param>
    /// <param name="identity">Identity of the user</param>
    /// <returns>Returns the response data</returns>
    Task<OfferAutoSetupResponseData> AutoSetupAppAsync(OfferAutoSetupData data, (Guid UserId, Guid CompanyId) identity);

    /// <summary>
    /// Starts the auto setup process.
    /// </summary>
    /// <param name="data">The offer subscription id and url for the service</param>
    /// <param name="companyId">Id of the company</param>
    /// <returns>Returns the response data</returns>
    Task StartAutoSetupAsync(OfferAutoSetupData data, Guid companyId);

    /// <summary>
    /// Gets the app agreement data
    /// </summary>
    /// <param name="appId">Id of the app to get the agreements for</param>
    /// <returns>Returns IAsyncEnumerable of agreement data</returns>
    IAsyncEnumerable<AgreementData> GetAppAgreement(Guid appId);

    /// <summary>
    /// Retrieve Document Content for document type  by ID
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="documentId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>byte Array Content</returns>
    Task<(byte[] Content, string ContentType, string FileName)> GetAppDocumentContentAsync(Guid appId, Guid documentId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the information for the subscription
    /// </summary>
    /// <param name="appId">Id of the app</param>
    /// <param name="subscriptionId">Id of the subscription</param>
    /// <param name="companyId">Id of the users company</param>
    /// <returns>Returns the details of the subscription</returns>
    Task<AppProviderSubscriptionDetailData> GetSubscriptionDetailForProvider(Guid appId, Guid subscriptionId, Guid companyId);

    /// <summary>
    /// Gets the information for the subscription
    /// </summary>
    /// <param name="appId">Id of the app</param>
    /// <param name="subscriptionId">Id of the subscription</param>
    /// <param name="companyId">Id of the users company</param>
    /// <returns>Returns the details of the subscription</returns>
    Task<SubscriberSubscriptionDetailData> GetSubscriptionDetailForSubscriber(Guid appId, Guid subscriptionId, Guid companyId);
}
