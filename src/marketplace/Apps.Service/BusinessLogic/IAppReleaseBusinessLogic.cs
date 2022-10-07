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
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.CatenaX.Ng.Portal.Backend.Offers.Library.Models;

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
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<int> CreateAppDocumentAsync(Guid appId, DocumentTypeId documentTypeId, IFormFile document, string userId, CancellationToken cancellationToken);
    
    /// <summary>
    /// Add User Role for App
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="appAssignedDesc"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task AddAppUserRoleAsync(Guid appId, IEnumerable<AppUserRole> appAssignedDesc, string userId);
    
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
}
