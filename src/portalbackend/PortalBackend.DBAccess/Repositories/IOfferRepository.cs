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

using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using PortalBackend.DBAccess.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

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

    /// <summary>
    /// Gets all active apps with an optional filtered with the languageShortName
    /// </summary>
    /// <param name="languageShortName">The optional language shortName</param>
    /// <returns>Returns a async enumerable of (Guid Id, string? Name, string VendorCompanyName, IEnumerable<string> UseCaseNames, string? ThumbnailUrl, string? ShortDescription, string? LicenseText)> GetAllActiveAppsAsync(string? languageShortName)</returns>
    IAsyncEnumerable<ActiveAppData> GetAllActiveAppsAsync(string? languageShortName, string defaultLanguageShortName);

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

    void CreateDeleteAppAssignedUseCases(Guid appId, IEnumerable<Guid> initialUseCases, IEnumerable<Guid> modifyUseCases);

    /// <summary>
    /// Adds <see cref="OfferDescription"/>s to the database
    /// </summary>
    /// <param name="offerDescriptions">The app descriptions that should be added to the database</param>
    void AddOfferDescriptions(IEnumerable<(Guid offerId, string languageShortName, string descriptionLong, string descriptionShort)> offerDescriptions);

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
    IAsyncEnumerable<AllOfferData> GetProvidedOffersData(OfferTypeId offerTypeId, string iamUserId);

    /// <summary>
    /// Gets the client roles for a specific app
    /// </summary>
    /// <param name="appId">id of the app to get the client roles for</param>
    /// <param name="languageShortName">The language short names</param>
    /// <returns>Returns an asyncEnumerable from ClientRoles</returns>
    [Obsolete("only referenced by code that is marked as obsolte")]
    IAsyncEnumerable<ClientRoles> GetClientRolesAsync(Guid appId, string languageShortName);

    /// <summary>
    /// Check whether the app is in status created and whether the
    /// loggedin user belongs to the apps provider company
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="userId"></param>
    /// <returns>ValueTuple, first item is true if the app is in status CREATED,
    /// second item is true if the user is eligible to edit it</returns>
    Task<(bool IsAppCreated, bool IsProviderUser, string? ContactEmail, string? ContactNumber, string? MarketingUrl, IEnumerable<LocalizedDescription> Descriptions)> GetOfferDetailsForUpdateAsync(Guid appId, string userId, OfferTypeId offerTypeId);

    /// Get Offer Release data by Offer Id
    /// </summary>
    /// <param name="offerId">Id of the offer</param>
    /// <param name="offerTypeId">Type of the offer</param>
    /// <returns></returns>
    Task<OfferReleaseData?> GetOfferReleaseDataByIdAsync(Guid offerId, OfferTypeId offerTypeId);

    /// <summary>
    /// Gets all service detail data from the persistence storage as pagination 
    /// </summary>
    /// <returns>Returns an Pagination</returns>
    Func<int, int, Task<Pagination.Source<ServiceOverviewData>?>> GetActiveServicesPaginationSource(ServiceOverviewSorting? sorting, ServiceTypeId? serviceTypeId, string languageShortName);

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
    Func<int, int, Task<Pagination.Source<InReviewAppData>?>> GetAllInReviewStatusAppsAsync(IEnumerable<OfferStatusId> offerStatusIds, OfferSorting? sorting);

    /// <summary>
    /// Retrieve Offer Detail with Status
    /// </summary>
    /// <param name="offerId"></param>
    /// <param name="userId"></param>
    /// <param name="offerTypeId"></param>
    /// <returns></returns>
    Task<(OfferProviderData? OfferProviderData, bool IsProviderCompanyUser)> GetProviderOfferDataWithConsentStatusAsync(Guid offerId, string userId, OfferTypeId offerTypeId);

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
    Task<(bool OfferExists, bool IsStatusCreated, Guid CompanyUserId)> GetProviderCompanyUserIdForOfferUntrackedAsync(Guid offerId, string userId, OfferStatusId offerStatusId, OfferTypeId offerTypeId);

    /// <summary>
    /// Verify that user is linked to the appId ,offerstatus is in created state and roleId exist
    /// </summary>
    /// <param name="offerId"></param>
    /// <param name="userId"></param>
    /// <param name="offerStatusId"></param>
    /// <param name="roleId"></param>
    /// <returns></returns>
    Task<(bool OfferStatus, bool IsProviderCompanyUser, bool IsRoleIdExist)> GetAppUserRoleUntrackedAsync(Guid offerId, string userId, OfferStatusId offerStatusId, Guid roleId);

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
        IEnumerable<string> languageCodes);

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
    /// Removes <see cref="ServiceDetail"/>s to the databasethe database
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
    Task<(bool OfferExists, string? AppName, Guid CompanyUserId, Guid? ProviderCompanyId, IEnumerable<string> ClientClientIds)> GetInsertActiveAppUserRoleDataAsync(Guid offerId, string userId, OfferTypeId offerTypeId);

    /// <summary>
    /// Retireve and Validate Offer Status for App
    /// </summary>
    /// <param name="offerId"></param>
    /// <param name="offerTypeId"></param>
    /// <returns></returns>
    Task<(bool IsStatusInReview, string? OfferName, Guid? ProviderCompanyId, bool IsSingleInstance, IEnumerable<(Guid InstanceId, string ClientId)> Instances)> GetOfferStatusDataByIdAsync(Guid offerId, OfferTypeId offerTypeId);

    /// <summary>
    /// Gets the data needed for declining an offer
    /// </summary>
    /// <param name="offerId">If of the offer</param>
    /// <param name="iamUserId">Id of the iamUser</param>
    /// <param name="offerType">Type of the offer</param>
    /// <returns>Returns the data needed to decline an offer</returns>
    Task<(string? OfferName, OfferStatusId OfferStatus, Guid? CompanyId, IEnumerable<DocumentStatusData> ActiveDocumentStatusDatas)> GetOfferDeclineDataAsync(Guid offerId, string iamUserId, OfferTypeId offerType);

    /// <summary>
    /// Retireve and Validate Offer Status by offerId and type
    /// </summary>
    /// <param name="offerId"></param>
    /// <param name ="iamUserId"></param>
    /// <param name="offerTypeId"></param>
    /// <returns></returns>
    Task<(bool IsStatusActive, bool IsUserCompanyProvider)> GetOfferActiveStatusDataByIdAsync(Guid offerId, OfferTypeId offerTypeId, string iamUserId);

    /// <summary>
    /// Adds <see cref="OfferAssignedPrivacyPolicy"/>s to the database
    /// </summary>
    /// <param name="privacyPolicies">The privacy policies that should be added to the database</param>
    void AddAppAssignedPrivacyPolicies(IEnumerable<(Guid appId, PrivacyPolicyId privacyPolicy)> privacyPolicies);

    /// <summary>
    /// Add offer Id and privacy policy Id in Offer Assigned Privacy Policies table 
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="initialPrivacyPolicy"></param>
    /// <param name="modifyPrivacyPolicy"></param>
    /// <returns></returns>
    void CreateDeleteAppAssignedPrivacyPolicies(Guid appId, IEnumerable<PrivacyPolicyId> initialPrivacyPolicy, IEnumerable<PrivacyPolicyId> modifyPrivacyPolicy);

    /// <summary>
    /// Gets InReview Offer Data for App by ID
    /// </summary>
    /// <param name="id"></param>
    /// <param name ="offerTypeId"></param>
    Task<InReviewOfferData?> GetInReviewAppDataByIdAsync(Guid id, OfferTypeId offerTypeId);

    /// <summary>
    /// Gets Offer Descriptions Data for Apps
    /// </summary>
    /// <param name="appId"></param>
    /// <param name ="iamUserId"></param>
    /// <param name="offerTypeId"></param>
    /// <returns></returns>
    Task<(bool IsStatusActive, bool IsProviderCompanyUser, IEnumerable<LocalizedDescription>? OfferDescriptionDatas)> GetActiveOfferDescriptionDataByIdAsync(Guid appId, OfferTypeId offerTypeId, string iamUserId);

    /// <summary>
    /// Create, Update and Delete Offer Descriptions Data
    /// </summary>
    /// <param name="offerId"></param>
    /// <param name ="initialItems"></param>
    /// <param name="modifiedItems"></param>
    /// <returns></returns>
    void CreateUpdateDeleteOfferDescriptions(Guid offerId, IEnumerable<LocalizedDescription> initialItems, IEnumerable<(string LanguageCode, string LongDescription, string ShortDescription)> modifiedItems);

    /// <summary>
    /// Delete the OfferAssignedDocument 
    /// </summary>
    /// <param name="offerId"></param>
    /// <param name="documentId"></param>
    void RemoveOfferAssignedDocument(Guid offerId, Guid documentId);

    /// Verify that user is linked to the appId ,offerstatus is in created state
    /// </summary>
    /// <param name="offerId"></param>
    /// <param name="userId"></param>
    /// <param name="offerStatusId"></param>
    /// <returns></returns>
    Task<(bool IsValidApp, bool IsOfferType, bool IsOfferStatus, bool IsProviderCompanyUser, AppDeleteData? DeleteData)> GetAppDeleteDataAsync(Guid offerId, OfferTypeId offerTypeId, string userId, OfferStatusId offerStatusId);

    /// <summary>
    /// Delete Offer Assigned Licenses
    /// </summary>
    /// <param name="offerLicenseIds"></param>
    void RemoveOfferAssignedLicenses(IEnumerable<(Guid OfferId, Guid LicenseId)> offerLicenseIds);

    /// <summary>
    /// Delete Offer Assigned Use Cases
    /// </summary>
    /// <param name="offerUseCaseIds"></param>
    void RemoveOfferAssignedUseCases(IEnumerable<(Guid OfferId, Guid UseCaseId)> offerUseCaseIds);

    /// <summary>
    /// Delete Offer Assigned Privacy Policies
    /// </summary>
    /// <param name="offerPrivacyPolicyIds"></param>
    void RemoveOfferAssignedPrivacyPolicies(IEnumerable<(Guid OfferId, PrivacyPolicyId PrivacyPolicyId)> offerPrivacyPolicyIds);

    /// <summary>
    /// Delete Offer Assigned Documents
    /// </summary>
    /// <param name="offerDocumentIds"></param>
    void RemoveOfferAssignedDocuments(IEnumerable<(Guid OfferId, Guid DocumentId)> offerDocumentIds);

    /// <summary>
    /// Delete Offer Tags
    /// </summary>
    /// <param name="offerTagNames"></param>
    void RemoveOfferTags(IEnumerable<(Guid OfferId, string TagName)> offerTagNames);

    /// <summary>
    /// Delete Offer Description
    /// </summary>
    /// <param name="offerLanguageShortNames"></param>
    void RemoveOfferDescriptions(IEnumerable<(Guid OfferId, string LanguageShortName)> offerLanguageShortNames);

    /// <summary>
    /// Delete Offer
    /// </summary>
    /// <param name="offerIds"></param>
    void RemoveOffer(Guid offerId);

    /// <summary>
    /// Gets Active OfferAssigned AppLeadImage Documents
    /// </summary>
    /// <param name="offerId"></param>
    /// <param name ="iamUserId"></param>
    /// <param name="offerTypeId"></param>
    /// <returns></returns>
    Task<(bool IsStatusActive, Guid CompanyUserId, IEnumerable<DocumentStatusData> documentStatusDatas)> GetOfferAssignedAppLeadImageDocumentsByIdAsync(Guid offerId, string iamUserId, OfferTypeId offerTypeId);

    /// <summary>
    /// Retrieve Service Detail
    /// </summary>
    /// <param name="serviceId"></param>
    /// <returns></returns>
    Task<ServiceDetailsData?> GetServiceDetailsByIdAsync(Guid serviceId);

    /// <summary>
    /// Retrieves all status offer in the marketplace.
    /// </summary>
    /// <param name="offerStatusIds"></param>
    /// <param name="offerTypeId"></param>
    /// <param name="userId"></param>
    /// <param name="sorting"></param>
    /// <param name="offerName"></param>
    Func<int, int, Task<Pagination.Source<AllOfferStatusData>?>> GetCompanyProvidedServiceStatusDataAsync(IEnumerable<OfferStatusId> offerStatusIds, OfferTypeId offerTypeId, string userId, OfferSorting? sorting, string? offerName);

    /// <summary>
    /// Retrieves all in review status offer in the marketplace.
    /// </summary>
    /// <param name="offerStatusIds"></param>
    /// <param name="offerTypeId"></param>
    /// <param name="sorting"></param>
    /// <param name="offerName"></param>
    /// <param name="languageShortName"></param>
    Func<int, int, Task<Pagination.Source<InReviewServiceData>?>> GetAllInReviewStatusServiceAsync(IEnumerable<OfferStatusId> offerStatusIds, OfferTypeId offerTypeId, OfferSorting? sorting, string? offerName, string languageShortName, string defaultLanguageShortName);

    /// Gets the data for the app including the instance type information
    /// </summary>
    /// <param name="offerId"></param>
    /// <param name="iamUserId"></param>
    /// <param name="offerTypeId"></param>
    /// <returns></returns>
    Task<(OfferStatusId OfferStatus, bool IsUserOfProvidingCompany, AppInstanceSetupTransferData? SetupTransferData, IEnumerable<(Guid AppInstanceId, Guid ClientId, string ClientClientId)> AppInstanceData)> GetOfferWithSetupDataById(Guid offerId, string iamUserId, OfferTypeId offerTypeId);

    /// <summary>
    /// Creates a new instance of <see cref="AppInstanceSetup"/>
    /// </summary>
    /// <param name="appId">id of the app</param>
    /// <param name="isSingleInstance">defines whether the app is a single instance</param>
    /// <param name="setOptionalParameter">Action to set optional parameters for the app instance setup</param>
    /// <returns>The created entity</returns>
    AppInstanceSetup CreateAppInstanceSetup(Guid appId, Action<AppInstanceSetup>? setOptionalParameter);

    /// <summary>
    /// Gets the single instance offer data
    /// </summary>
    /// <param name="offerId">id of the offer</param>
    /// <param name="offerTypeId">id of the offer type</param>
    /// <returns>Returns the single instance offer data</returns>
    Task<SingleInstanceOfferData?> GetSingleInstanceOfferData(Guid offerId, OfferTypeId offerTypeId);

    /// <summary>
    /// Updates the <see cref="AppInstanceSetup"/>
    /// </summary>
    /// <param name="appInstanceSetupId">Id of the appInstanceSetup that should be updated</param>
    /// <param name="offerId">Id of the offer</param>
    /// <param name="setOptionalParameters">Sets the values that should be updated</param>
    /// <param name="initializeParameter">Initializes the parameters</param>
    void AttachAndModifyAppInstanceSetup(Guid appInstanceSetupId, Guid offerId, Action<AppInstanceSetup> setOptionalParameters, Action<AppInstanceSetup>? initializeParameter = null);

    /// <summary>
    /// Gets the related service account data
    /// </summary>
    /// <param name="offerId"></param>
    /// <returns></returns>
    Task<(bool IsSingleInstance, IEnumerable<IEnumerable<UserRoleData>> ServiceAccountProfiles, string? OfferName)> GetServiceAccountProfileData(Guid offerId, OfferTypeId offerTypeId);

    /// <summary>
    /// Gets the related service account data
    /// </summary>
    /// <param name="subscriptionId"></param>
    /// <returns></returns>
    Task<(bool IsSingleInstance, IEnumerable<IEnumerable<UserRoleData>> ServiceAccountProfiles, string? OfferName)> GetServiceAccountProfileDataForSubscription(Guid subscriptionId);
}
