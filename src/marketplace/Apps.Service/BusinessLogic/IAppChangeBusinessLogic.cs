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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Apps.Service.BusinessLogic;

/// <summary>
/// Business logic for handling app change-related operations. Includes persistence layer access.
/// </summary>
public interface IAppChangeBusinessLogic
{
    /// <summary>
    /// Add User Role for Active App and creates a notification
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="appUserRolesDescription"></param>
    /// <param name="identity"></param>
    /// <returns>List of the created AppRoles</returns>
    Task<IEnumerable<AppRoleData>> AddActiveAppUserRoleAsync(Guid appId, IEnumerable<AppUserRole> appUserRolesDescription, (Guid UserId, Guid CompanyId) identity);

    /// <summary>
    /// Get OfferDescription by appId
    /// </summary>
    /// <param name="appId">Id of the app</param>
    /// <param name="companyId"></param>
    Task<IEnumerable<LocalizedDescription>> GetAppUpdateDescriptionByIdAsync(Guid appId, Guid companyId);

    /// <summary>
    /// Create or Update OfferDescription by appId
    /// </summary>
    /// <param name="appId">Id of the app</param>
    /// <param name="companyId"></param>
    /// <param name="offerDescriptionDatas">OfferDescription Data</param>
    Task CreateOrUpdateAppDescriptionByIdAsync(Guid appId, Guid companyId, IEnumerable<LocalizedDescription> offerDescriptionDatas);

    /// <summary>
    /// Upload OfferAssigned AppLeadImage Document by appId
    /// </summary>
    /// <param name="appId">Id of the app</param>
    /// <param name="identity"></param>
    /// <param name="document">Document Data</param>
    /// <param name="cancellationToken">cancellationToken</param>
    Task UploadOfferAssignedAppLeadImageDocumentByIdAsync(Guid appId, (Guid UserId, Guid CompanyId) identity, IFormFile document, CancellationToken cancellationToken);

    /// <summary>
    /// Deactivate Offer Status by appId
    /// </summary>
    /// <param name="appId">Id of the app</param>
    public Task DeactivateOfferByAppIdAsync(Guid appId);

    /// <summary>
    /// Updates the url of the subscription
    /// </summary>
    /// <param name="offerId">Id of the offer</param>
    /// <param name="subscriptionId">If of the subscription</param>
    /// <param name="data">the data to update the url</param>
    /// <param name="companyId"></param>
    Task UpdateTenantUrlAsync(Guid offerId, Guid subscriptionId, UpdateTenantData data, Guid companyId);

    /// <summary>
    /// Gets the Active App Documents
    /// </summary>
    /// <param name="appId">Id of the offer</param>
    Task<ActiveAppDocumentData> GetActiveAppDocumentTypeDataAsync(Guid appId);
}
