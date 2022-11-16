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
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.CatenaX.Ng.Portal.Backend.Apps.Service.BusinessLogic;

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
    Task<int> CreateAppDocumentAsync(Guid appId, DocumentTypeId documentTypeId, IFormFile document, string iamUserId, CancellationToken cancellationToken);
    
    /// <summary>
    /// Add User Role for App
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="appAssignedDesc"></param>
    /// <param name="iamUserId"></param>
    /// <returns></returns>
    Task<IEnumerable<AppRoleData>> AddAppUserRoleAsync(Guid appId, IEnumerable<AppUserRole> appAssignedDesc, string iamUserId);
    
    /// <summary>
    /// Return Agreements for App_Contract Category
    /// </summary>
    /// <returns></returns>
    IAsyncEnumerable<AgreementData> GetOfferAgreementDataAsync();

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
    Task<int> SubmitOfferConsentAsync(Guid appId, OfferAgreementConsent offerAgreementConsents, string userId);
    
    /// <summary>
    /// Return Offer with Consent Status
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<OfferProviderResponse> GetAppDetailsForStatusAsync(Guid appId, string userId);
    
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
    Task<Pagination.Response<InReviewAppData>> GetAllInReviewStatusAppsAsync(int page = 0, int size = 15);

    /// <summary>
    /// Update app status and create notification
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="iamUserId"></param>
    /// <returns></returns>
    Task SubmitAppReleaseRequestAsync(Guid appId, string iamUserId);
    
    /// <summary>
    /// Add User ROle for Active App and create notification
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="appUserRolesDescription"></param>
    /// <param name="iamUserId"></param>
    /// <returns></returns>
    Task<IEnumerable<AppRoleData>>  AddActiveAppUserRoleAsync(Guid appId, IEnumerable<AppUserRole> appUserRolesDescription, string iamUserId);
}
