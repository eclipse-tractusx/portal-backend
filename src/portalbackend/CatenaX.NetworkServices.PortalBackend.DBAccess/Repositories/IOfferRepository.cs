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

using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

/// <summary>
/// Repository for accessing apps on persistence layer.
/// </summary>
public interface IOfferRepository
{
    /// <summary>
    /// Checks if an app with the given id exists in the persistence layer. 
    /// </summary>
    /// <param name="appId">Id of the app.</param>
    /// <returns><c>true</c> if an app exists on the persistence layer with the given id, <c>false</c> if not.</returns>
    public Task<bool> CheckAppExistsById(Guid appId);

    /// <summary>
    /// Retrieves app provider company details by app id.
    /// </summary>
    /// <param name="appId">ID of the app.</param>
    /// <returns>Tuple of provider company details.</returns>
    public Task<OfferProviderDetailsData?> GetOfferProviderDetailsAsync(Guid appId);

    /// <summary>
    /// Get Client Name by App Id
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="companyId"></param>
    /// <returns>Client Name</returns>
    Task<string?> GetAppAssignedClientIdUntrackedAsync(Guid appId, Guid companyId);

    /// <summary>
    /// Adds an app to the database
    /// </summary>
    /// <param name="provider">Provider of the app</param>
    /// <param name="offerType">Type of the app</param>
    /// <param name="setOptionalParameters">Action to set the optional parameters</param>
    Offer CreateOffer(string provider, OfferTypeId offerType, Action<Offer>? setOptionalParameters = null);

    /// <summary>
    /// Gets all active apps with an optional filtered with the languageShortName
    /// </summary>
    /// <param name="languageShortName">The optional language shortName</param>
    /// <returns>Returns a async enumerable of <see cref="AppData"/></returns>
    IAsyncEnumerable<AppData> GetAllActiveAppsAsync(string? languageShortName);

    /// <summary>
    /// Gets the details of an app by its id
    /// </summary>
    /// <param name="appId">Id of the application to get details for</param>
    /// <param name="iamUserId">OPTIONAL: iamUserId of the company the calling user belongs to</param>
    /// <param name="languageShortName">OPTIONAL: language shortName</param>
    /// <returns>Returns the details of the application</returns>
    Task<AppDetailsData> GetAppDetailsByIdAsync(Guid appId, string iamUserId, string? languageShortName);

    /// <summary>
    /// Adds an <see cref="OfferLicense"/> to the database
    /// </summary>
    /// <param name="licenseText">Text of the license</param>
    OfferLicense CreateOfferLicenses(string licenseText);

    /// <summary>
    /// Adds an <see cref="OfferAssignedLicense"/> to the database
    /// </summary>
    /// <param name="appId">Id of the application</param>
    /// <param name="appLicenseId">Id of the app license</param>
    OfferAssignedLicense CreateOfferAssignedLicense(Guid appId, Guid appLicenseId);

    /// <summary>
    /// Adds the given app favourite to the database
    /// </summary>
    /// <param name="appId">Id of the app</param>
    /// <param name="companyUserId">Id of the company User</param>
    CompanyUserAssignedAppFavourite CreateAppFavourite(Guid appId, Guid companyUserId);

    /// <summary>
    /// Adds <see cref="AppAssignedUseCase"/>s to the database
    /// </summary>
    /// <param name="useCases">The use cases that should be added to the database</param>
    void AddAppAssignedUseCases(IEnumerable<(Guid appId, Guid useCaseId)> appUseCases);

    /// <summary>
    /// Adds <see cref="OfferDescription"/>s to the database
    /// </summary>
    /// <param name="appDescriptions">The app descriptions that should be added to the database</param>
    void AddOfferDescriptions(IEnumerable<(Guid appId, string languageShortName, string descriptionLong, string descriptionShort)> appDescriptions);

    /// <summary>
    /// Adds <see cref="AppLanguage"/>s to the database
    /// </summary>
    /// <param name="appLanguages">The app languages that should be added to the database</param>
    void AddAppLanguages(IEnumerable<(Guid appId, string languageShortName)> appLanguages);

    /// <summary>
    /// Retrieve all app data
    /// </summary>
    /// <param name="iamUserId">IAM ID of the user to retrieve own company app.</param>
    /// <returns>Return Async Enumerable of App Data</returns>
    IAsyncEnumerable<AllAppData> GetProvidedAppsData(string iamUserId);
    
    /// <summary>
    /// Gets the client roles for a specific app
    /// </summary>
    /// <param name="appId">id of the app to get the client roles for</param>
    /// <param name="languageShortName">The language short names</param>
    /// <returns>Returns an asyncEnumerable from ClientRoles</returns>
    IAsyncEnumerable<ClientRoles> GetClientRolesAsync(Guid appId, string? languageShortName = null);

    /// <summary>
    /// Check whether the app is in status created and whether the
    /// loggedin user belongs to the apps provider company
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="userId"></param>
    /// <returns>ValueTuple, first item is true if the app is in status CREATED,
    /// second item is true if the user is eligible to edit it</returns>
    Task<(bool IsAppCreated, bool IsProviderUser, string? ContactEmail, string? ContactNumber, string? MarketingUrl, IEnumerable<(string LanguageShortName ,string DescriptionLong,string DescriptionShort)> Descriptions, IEnumerable<(Guid Id, string Url)> ImageUrls)> GetAppDetailsForUpdateAsync(Guid appId, string userId);
    
    /// <summary>
    /// Add App Detail Images
    /// </summary>
    /// <param name="appImages"></param>
    void AddAppDetailImages(IEnumerable<(Guid appId, string imageUrl)> appImages);

    /// <summary>
    /// Get App Release data by App Id
    /// </summary>
    /// <param name="appId"></param>
    /// <returns></returns>
    Task<OfferReleaseData?> GetOfferReleaseDataByIdAsync(Guid offerId);

    /// <summary>
    /// Checks if an service with the given id exists in the persistence layer. 
    /// </summary>
    /// <param name="serviceId">Id of the service.</param>
    /// <returns><c>true</c> if an service exists on the persistence layer with the given id, <c>false</c> if not.</returns>
    public Task<bool> CheckServiceExistsById(Guid serviceId);

    /// <summary>
    /// Gets all service detail data from the persistence storage as queryable 
    /// </summary>
    /// <returns>Returns an <see cref="IQueryable{ServiceDetailData}"/></returns>
    IQueryable<(Guid id, string? name, string provider, string? thumbnailUrl, string? contactEmail, string? price)> GetActiveServices();

    /// <summary>
    /// Gets the service details for the given id
    /// </summary>
    /// <param name="serviceId">the service to get from the persistence storage</param>
    /// <param name="languageShortName">the language short code for the descriptions</param>
    /// <param name="iamUserId">Id of the iam User</param>
    /// <returns>Returns the ServiceDetailData or null</returns>
    Task<ServiceDetailData?> GetServiceDetailByIdUntrackedAsync(Guid serviceId, string languageShortName, string iamUserId);

    /// <summary>
    /// Retrieves all in review status apps in the marketplace.
    /// </summary>
    IQueryable<Offer> GetAllInReviewStatusAppsAsync();
}
