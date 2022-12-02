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

using Org.CatenaX.Ng.Portal.Backend.Framework.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Repositories;

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
    /// <param name="offerId">ID of the app.</param>
    /// <param name="offerTypeId">Id of the offer type.</param>
    /// <returns>Tuple of provider company details.</returns>
    public Task<OfferProviderDetailsData?> GetOfferProviderDetailsAsync(Guid offerId, OfferTypeId offerTypeId);

    /// <summary>
    /// Adds an app to the database
    /// </summary>
    /// <param name="provider">Provider of the app</param>
    /// <param name="offerType">Type of the app</param>
    /// <param name="setOptionalParameters">Action to set the optional parameters</param>
    Offer CreateOffer(string provider, OfferTypeId offerType, Action<Offer>? setOptionalParameters = null);

    void AttachAndModifyOffer(Guid offerId, Action<Offer> setOptionalParameters, Action<Offer>? initializeParemeters = null);

    Offer DeleteOffer(Guid offerId);

    /// <summary>
    /// Gets all active apps with an optional filtered with the languageShortName
    /// </summary>
    /// <param name="languageShortName">The optional language shortName</param>
    /// <returns>Returns a async enumerable of (Guid Id, string? Name, string VendorCompanyName, IEnumerable<string> UseCaseNames, string? ThumbnailUrl, string? ShortDescription, string? LicenseText)> GetAllActiveAppsAsync(string? languageShortName)</returns>
    IAsyncEnumerable<(Guid Id, string? Name, string VendorCompanyName, IEnumerable<string> UseCaseNames, string? ThumbnailUrl, string? ShortDescription, string? LicenseText)> GetAllActiveAppsAsync(string? languageShortName);

    /// <summary>
    /// Gets the details of an app by its id
    /// </summary>
    /// <param name="offerId">Id of the offer to get details for</param>
    /// <param name="iamUserId">OPTIONAL: iamUserId of the company the calling user belongs to</param>
    /// <param name="languageShortName">language shortName</param>
    /// <param name="defaultLanguageShortName">default language shortName</param>
    /// <param name="offerTypeId">Id of the offer type</param>
    /// <returns>Returns the details of the application</returns>
    Task<OfferDetailsData?> GetOfferDetailsByIdAsync(Guid offerId, string iamUserId, string? languageShortName, string defaultLanguageShortName, OfferTypeId offerTypeId);

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
    void DeleteAppFavourites(IEnumerable<(Guid AppId, Guid CompanyUserId)> appFavoriteIds);
    /// <summary>
    /// Add app Id and Document Id in App Assigned Document table 
    /// </summary>
    /// <param name="offerId"></param>
    /// <param name="documentId"></param>
    /// <returns></returns>
    OfferAssignedDocument CreateOfferAssignedDocument(Guid offerId, Guid documentId);

    /// <summary>
    /// Adds <see cref="AppAssignedUseCase"/>s to the database
    /// </summary>
    /// <param name="appUseCases">The use cases that should be added to the database</param>
    void AddAppAssignedUseCases(IEnumerable<(Guid appId, Guid useCaseId)> appUseCases);

    /// <summary>
    /// Adds <see cref="OfferDescription"/>s to the database
    /// </summary>
    /// <param name="offerDescriptions">The app descriptions that should be added to the database</param>
    void AddOfferDescriptions(IEnumerable<(Guid offerId, string languageShortName, string descriptionLong, string descriptionShort)> offerDescriptions);

    void RemoveOfferDescriptions(IEnumerable<(Guid offerId, string languageShortName)> offerDescriptionIds);

    void AttachAndModifyOfferDescription(Guid offerId, string languageShortName, Action<OfferDescription> setOptionalParameters);

    /// <summary>
    /// Adds <see cref="AppLanguage"/>s to the database
    /// </summary>
    /// <param name="appLanguages">The app languages that should be added to the database</param>
    void AddAppLanguages(IEnumerable<(Guid appId, string languageShortName)> appLanguages);

    /// <summary>
    /// Removes <see cref="AppLanguage"/>s to the database
    /// </summary>
    /// <param name="appLanguageIds">appIds and languageShortNames of the app languages to be removed from the database</param>
    void RemoveAppLanguages(IEnumerable<(Guid appId, string languageShortName)> appLanguageIds);

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
    [Obsolete("only referenced by code that is marked as obsolte")]
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

    void RemoveOfferDetailImages(IEnumerable<Guid> imageIds);

    /// <summary>
    /// Get App Release data by App Id
    /// </summary>
    /// <param name="offerId"></param>
    /// <returns></returns>
    Task<OfferReleaseData?> GetOfferReleaseDataByIdAsync(Guid offerId);

    /// <summary>
    /// Gets all service detail data from the persistence storage as pagination 
    /// </summary>
    /// <returns>Returns an Pagination</returns>
    Func<int,int,Task<Pagination.Source<ServiceOverviewData>?>> GetActiveServicesPaginationSource(ServiceOverviewSorting? sorting, ServiceTypeId? serviceTypeId);

    /// <summary>
    /// Gets the service details for the given id
    /// </summary>
    /// <param name="serviceId">the service to get from the persistence storage</param>
    /// <param name="languageShortName">the language short code for the descriptions</param>
    /// <param name="iamUserId">Id of the iam User</param>
    /// <param name="offerTypeId">Id of the offer type</param>
    /// <returns>Returns the ServiceDetailData or null</returns>
    Task<OfferDetailData?> GetOfferDetailByIdUntrackedAsync(Guid serviceId, string languageShortName, string iamUserId, OfferTypeId offerTypeId);

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
    /// <param name="offerStatusIds"></param>
    /// <param name="skip"></param>
    /// <param name="take"></param>
    /// <param name="sorting"></param>
    Func<int,int,Task<Pagination.Source<InReviewAppData>?>> GetAllInReviewStatusAppsAsync(IEnumerable<OfferStatusId> offerStatusIds, OfferSorting? sorting);
    
    /// <summary>
    /// Retrieve Offer Detail with Status
    /// </summary>
    /// <param name="offerId"></param>
    /// <param name="userId"></param>
    /// <param name="offerTypeId"></param>
    /// <returns></returns>
    Task<(OfferProviderData OfferProviderData, bool IsProviderCompanyUser)> GetProviderOfferDataWithConsentStatusAsync(Guid offerId, string userId, OfferTypeId offerTypeId);

    /// <summary>
    /// Verify that user is linked to the appId
    /// </summary>
    /// <param name="offerId"></param>
    /// <param name="userId"></param>
    /// <param name="offerTypeId"></param>
    /// <returns></returns>
    Task<(bool OfferExists, bool IsProviderCompanyUser)> IsProviderCompanyUserAsync(Guid offerId, string userId, OfferTypeId offerTypeId);

    /// <summary>
    /// Return the Company User Id
    /// </summary>
    /// <param name="offerId"></param>
    /// <param name="userId"></param>
    /// <param name="offerStatusId"></param>
    /// <param name="offerTypeId"></param>
    /// <returns></returns>
    Task<(bool OfferExists, Guid CompanyUserId)> GetProviderCompanyUserIdForOfferUntrackedAsync(Guid offerId, string userId, OfferStatusId offerStatusId, OfferTypeId offerTypeId);
    
    /// <summary>
    /// Verify that user is linked to the appId ,offerstatus is in created state and roleId exist
    /// </summary>
    /// <param name="offerId"></param>
    /// <param name="userId"></param>
    /// <param name="offerStatusId"></param>
    /// <param name="roleId"></param>
    /// <returns></returns>
    Task<(bool OfferStatus, bool IsProviderCompanyUser,bool IsRoleIdExist)> GetAppUserRoleUntrackedAsync(Guid offerId, string userId, OfferStatusId offerStatusId, Guid roleId);

    /// <summary>
    /// Gets all data needed for the app update
    /// </summary>
    /// <param name="appId">Id of the requested app</param>
    /// <param name="iamUserId">Id of the current IamUser</param>
    /// <param name="languageCodes">the languageCodes for the app</param>
    /// <param name="useCaseIds">ids of the usecases</param>
    /// <returns></returns>
    Task<AppUpdateData?> GetAppUpdateData(
        Guid appId,
        string iamUserId,
        IEnumerable<string> languageCodes,
        IEnumerable<Guid> useCaseIds);

    /// <summary>
    /// Updates the licenseText of the given offerLicense
    /// </summary>
    /// <param name="offerLicenseId">id of the offer license</param>
    /// <param name="setOptionalParameters">action to modify newly attached OfferLicence</param>
    /// <returns>the updated entity</returns>
    void AttachAndModifyOfferLicense(Guid offerLicenseId, Action<OfferLicense> setOptionalParameters);

    /// <summary>
    /// Removes the offer assigned offer license from the database
    /// </summary>
    /// <param name="offerId">id of the app</param>
    /// <param name="offerLicenseId">id of the offer license</param>
    void RemoveOfferAssignedLicense(Guid offerId, Guid offerLicenseId);

    /// <summary>
    /// Adds the service types to the service
    /// </summary>
    /// <param name="serviceAssignedServiceTypes"></param>
    void AddServiceAssignedServiceTypes(IEnumerable<(Guid serviceId, ServiceTypeId serviceTypeId)> serviceAssignedServiceTypes);

    /// <summary>
    /// Removes <see cref="ServiceAssignedServiceType"/>s to the database
    /// </summary>
    /// <param name="serviceAssignedServiceTypes">serviceIds and serviceTypeIds of the assigned service types to be removed from the database</param>
    void RemoveServiceAssignedServiceTypes(IEnumerable<(Guid serviceId, ServiceTypeId serviceTypeId)> serviceAssignedServiceTypes);

    /// <summary>
    /// Gets the needed update data for a service
    /// </summary>
    /// <param name="serviceId">Id of the service</param>
    /// <param name="serviceTypeIds">Ids of the assigned service types</param>
    /// <param name="iamUserId">id of the current user</param>
    /// <returns>The found service update data</returns>
    Task<ServiceUpdateData?> GetServiceUpdateData(Guid serviceId, IEnumerable<ServiceTypeId> serviceTypeIds, string iamUserId);
    
    /// <summary>
    /// Validate Company User and Retrieve CompanyUserid with App Name
    /// </summary>
    /// <param name="offerId"></param>
    /// <param name="userId"></param>
    /// <param name="offerTypeId"></param>
    /// <returns></returns>
    Task<(bool OfferExists, string? AppName, Guid CompanyUserId)> GetOfferNameProviderCompanyUserAsync(Guid offerId, string userId, OfferTypeId offerTypeId);

    /// <summary>
    /// Retireve and Validate Offer Status for App
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="offerTypeId"></param>
    /// <returns></returns>
    Task<(bool IsStatusInReview, string? OfferName)> GetOfferStatusDataByIdAsync(Guid appId, OfferTypeId offerTypeId);
}
