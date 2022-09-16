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
public interface IAppReleaseRepository
{
    /// <summary>
    /// Return the Company User Id
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<Guid> GetCompanyUserIdForAppUntrackedAsync(Guid appId, string userId);
    
    /// <summary>
    /// Add app Id and Document Id in App Assigned Document table 
    /// </summary>
    /// <param name="offerId"></param>
    /// <param name="documentId"></param>
    /// <returns></returns>
    OfferAssignedDocument CreateOfferAssignedDocument(Guid offerId, Guid documentId);
    
    /// <summary>
    /// Verify that user is linked to the appId
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<bool> IsProviderCompanyUserAsync(Guid appId,string userId);
    
    /// <summary>
    /// Add User Role for App Id
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="role"></param>
    /// <returns></returns>
    UserRole CreateAppUserRole(Guid appId, string role);
    
    /// <summary>
    /// Add User Role for App Description
    /// </summary>
    /// <param name="roleId"></param>
    /// <param name="languageCode"></param>
    /// <param name="description"></param>
    /// <returns></returns>
    UserRoleDescription CreateAppUserRoleDescription(Guid roleId, string languageCode, string description);
}
