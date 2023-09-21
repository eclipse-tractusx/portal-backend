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
    /// Upload document for given company user for appId
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="documentTypeId"></param>
    /// <param name="document"></param>
    /// <param name="identity"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task CreateAppDocumentAsync(Guid appId, DocumentTypeId documentTypeId, IFormFile document, (Guid UserId, Guid CompanyId) identity, CancellationToken cancellationToken);

    /// <summary>
    /// Add User Role for App
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="userRoles"></param>
    /// <param name="companyId"></param>
    /// <returns></returns>
    Task<IEnumerable<AppRoleData>> AddAppUserRoleAsync(Guid appId, IEnumerable<AppUserRole> userRoles, Guid companyId);

    /// <summary>
    /// Return Agreements for App_Contract Category
    /// </summary>
    /// <returns></returns>
    IAsyncEnumerable<AgreementDocumentData> GetOfferAgreementDataAsync();

    /// <summary>
    /// Return Offer Agreement Consent
    /// </summary>
    /// <param name="appId"></param>
    /// <returns></returns>
    Task<OfferAgreementConsent> GetOfferAgreementConsentById(Guid appId);

    /// <summary>
    /// Update Agreement Consent
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="offerAgreementConsents"></param>
    /// <returns></returns>
    Task<IEnumerable<ConsentStatusData>> SubmitOfferConsentAsync(Guid appId, OfferAgreementConsent offerAgreementConsents);

    /// <summary>
    /// Return Offer with Consent Status
    /// </summary>
    /// <param name="appId"></param>
    /// <returns></returns>
    Task<AppProviderResponse> GetAppDetailsForStatusAsync(Guid appId);

    /// <summary>
    /// Delete User Role by appId and roleId
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="roleId"></param>
    /// <param name="companyId"></param>
    /// <returns></returns>
    Task DeleteAppRoleAsync(Guid appId, Guid roleId, Guid companyId);

    /// <summary>
    /// Get Sales Manager Data
    /// </summary>
    /// <param name="companyId"></param>
    /// <returns></returns>
    IAsyncEnumerable<CompanyUserNameData> GetAppProviderSalesManagersAsync(Guid companyId);

    /// <summary>
    /// Creates an application and returns its generated ID.
    /// </summary>
    /// <param name="appRequestModel"></param>
    /// <param name="companyId"></param>
    /// <returns>Guid of the created app.</returns>
    Task<Guid> AddAppAsync(AppRequestModel appRequestModel, Guid companyId);

    /// <summary>
    /// Creates an application and returns its generated ID.
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="appRequestModel"></param>
    /// <param name="companyId"></param>
    /// <returns>Guid of the created app.</returns>
    Task UpdateAppReleaseAsync(Guid appId, AppRequestModel appRequestModel, Guid companyId);

    /// <summary>
    /// Retrieves all in review status apps in the marketplace.
    /// </summary>
    Task<Pagination.Response<InReviewAppData>> GetAllInReviewStatusAppsAsync(int page, int size, OfferSorting? sorting, OfferStatusIdFilter? offerStatusIdFilter);

    /// <summary>
    /// Update app status and create notification
    /// </summary>
    /// <param name="appId"></param>
    /// <returns></returns>
    Task SubmitAppReleaseRequestAsync(Guid appId);

    /// <summary>
    /// Approve App Status from IN_Review to Active
    /// </summary>
    /// <param name="appId"></param>
    /// <returns></returns>
    Task ApproveAppRequestAsync(Guid appId);

    /// <summary>
    /// Get All Privacy Policy
    /// </summary>
    /// <returns></returns>
    Task<PrivacyPolicyData> GetPrivacyPolicyDataAsync();

    /// <summary>
    /// Declines the app request
    /// </summary>
    /// <param name="appId">Id of the app</param>
    /// <param name="data">The decline request data</param>
    Task DeclineAppRequestAsync(Guid appId, OfferDeclineRequest data);

    /// <summary>
    /// Gets InReview App Details Data by Id
    /// </summary>
    /// <param name="appId">Id of the app</param>
    Task<InReviewAppDetails> GetInReviewAppDetailsByIdAsync(Guid appId);

    /// <summary>
    /// Delete the App Document
    /// </summary>
    /// <param name="documentId"></param>
    /// <returns></returns>
    Task DeleteAppDocumentsAsync(Guid documentId);

    ///<summary>
    /// Delete App
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="companyId"></param>
    /// <returns></returns>
    Task DeleteAppAsync(Guid appId, Guid companyId);

    /// <summary>
    /// Sets the instance type and all related data for the app
    /// </summary>
    /// <param name="appId">Id of the app</param>
    /// <param name="data">the data for the app instance</param>
    /// <param name="companyId">id of the current user</param>
    Task SetInstanceType(Guid appId, AppInstanceSetupData data, Guid companyId);

    /// <summary>
    /// Get technical user profiles for a specific offer
    /// </summary>
    /// <param name="offerId">Id of the offer</param>
    /// <returns>AsyncEnumerable with the technical user profile information</returns>
    Task<IEnumerable<TechnicalUserProfileInformation>> GetTechnicalUserProfilesForOffer(Guid offerId);

    /// <summary>
    /// Creates or updates the technical user profiles
    /// </summary>
    /// <param name="appId">Id of the app</param>
    /// <param name="data">The technical user profiles</param>
    Task UpdateTechnicalUserProfiles(Guid appId, IEnumerable<TechnicalUserProfileData> data);
}
