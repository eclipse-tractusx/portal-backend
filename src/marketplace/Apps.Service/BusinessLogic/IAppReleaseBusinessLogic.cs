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
/// Business logic for handling app release-related operations. Includes persistence layer access.
/// </summary>
public interface IAppReleaseBusinessLogic
{
    /// <summary>
    /// Update an App
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="updateModel"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    [Obsolete("This Method is not used anymore,  Planning to delete it with release 3.1")]
    Task UpdateAppAsync(Guid appId, AppEditableDetail updateModel, string userId);
    
    /// <summary>
    /// Upload document for given company user for appId
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="documentTypeId"></param>
    /// <param name="document"></param>
    /// <param name="iamUserId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task CreateAppDocumentAsync(Guid appId, DocumentTypeId documentTypeId, IFormFile document, string iamUserId, CancellationToken cancellationToken);
    
    /// <summary>
    /// Add User Role for App
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="userRoles"></param>
    /// <param name="iamUserId"></param>
    /// <returns></returns>
    Task<IEnumerable<AppRoleData>> AddAppUserRoleAsync(Guid appId, IEnumerable<AppUserRole> userRoles, string iamUserId);
    
    /// <summary>
    /// Return Agreements for App_Contract Category
    /// </summary>
    /// <returns></returns>
    IAsyncEnumerable<AgreementDocumentData> GetOfferAgreementDataAsync();

    /// <summary>
    /// Return Offer Agreement Consent
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<OfferAgreementConsent> GetOfferAgreementConsentById(Guid appId, string userId);
    
    /// <summary>
    /// Update Agreement Consent
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="offerAgreementConsents"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<IEnumerable<ConsentStatusData>> SubmitOfferConsentAsync(Guid appId, OfferAgreementConsent offerAgreementConsents, string userId);
    
    /// <summary>
    /// Return Offer with Consent Status
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<AppProviderResponse> GetAppDetailsForStatusAsync(Guid appId, string userId);
    
    /// <summary>
    /// Delete User Role by appId and roleId
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="roleId"></param>
    /// <param name="iamUserId"></param>
    /// <returns></returns>
    Task DeleteAppRoleAsync(Guid appId, Guid roleId, string iamUserId);
    
    /// <summary>
    /// Get Sales Manager Data
    /// </summary>
    /// <param name="iamUserId"></param>
    /// <returns></returns>
    IAsyncEnumerable<CompanyUserNameData> GetAppProviderSalesManagersAsync(string iamUserId);

    /// <summary>
    /// Creates an application and returns its generated ID.
    /// </summary>
    /// <param name="appRequestModel"></param>
    /// <param name="iamUserId"></param>
    /// <returns>Guid of the created app.</returns>
    Task<Guid> AddAppAsync(AppRequestModel appRequestModel, string iamUserId);

    /// <summary>
    /// Creates an application and returns its generated ID.
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="appRequestModel"></param>
    /// <param name="iamUserId"></param>
    /// <returns>Guid of the created app.</returns>
    Task UpdateAppReleaseAsync(Guid appId, AppRequestModel appRequestModel, string iamUserId);

    /// <summary>
    /// Retrieves all in review status apps in the marketplace.
    /// </summary>
    Task<Pagination.Response<InReviewAppData>> GetAllInReviewStatusAppsAsync(int page, int size, OfferSorting? sorting, OfferStatusIdFilter? offerStatusIdFilter);

    /// <summary>
    /// Update app status and create notification
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="iamUserId"></param>
    /// <returns></returns>
    Task SubmitAppReleaseRequestAsync(Guid appId, string iamUserId);

    /// <summary>
    /// Approve App Status from IN_Review to Active
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="iamUserId"></param>
    /// <returns></returns>
    Task ApproveAppRequestAsync(Guid appId, string iamUserId);

    /// <summary>
    /// Get All Privacy Policy
    /// </summary>
    /// <returns></returns>
    Task<PrivacyPolicyData> GetPrivacyPolicyDataAsync();

    /// <summary>
    /// Declines the app request
    /// </summary>
    /// <param name="appId">Id of the app</param>
    /// <param name="iamUserId">Id of the iamUser</param>
    /// <param name="data">The decline request data</param>
    Task DeclineAppRequestAsync(Guid appId, string iamUserId, OfferDeclineRequest data);

    /// <summary>
    /// Gets InReview App Details Data by Id
    /// </summary>
    /// <param name="appId">Id of the app</param>
    Task<InReviewAppDetails> GetInReviewAppDetailsByIdAsync(Guid appId);

    /// <summary>
    /// Delete the App Document
    /// </summary>
    /// <param name="documentId"></param>
    /// <param name="iamUserId"></param>
    /// <returns></returns>
    Task DeleteAppDocumentsAsync(Guid documentId, string iamUserId);

    ///<summary>
    /// Delete App
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="iamUserId"></param>
    /// <returns></returns>
    Task DeleteAppAsync(Guid appId, string iamUserId);

    /// <summary>
    /// Sets the instance type and all related data for the app
    /// </summary>
    /// <param name="appId">Id of the app</param>
    /// <param name="data">the data for the app instance</param>
    /// <param name="iamUserId">the current user</param>
    Task SetInstanceType(Guid appId, AppInstanceSetupData data, string iamUserId);
        
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
}
