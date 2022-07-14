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
/// Repository for accessing company assigned apps on persistence layer.
/// </summary>
public interface ICompanyAssignedAppsRepository
{
    /// <summary>
    /// Adds the given company assigned app to the database
    /// </summary>
    /// <param name="appId">Id of the assigned app</param>
    /// <param name="companyId">Id of the company</param>
    CompanyAssignedApp CreateCompanyAssignedApp(Guid appId, Guid companyId);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="companyId"></param>
    IAsyncEnumerable<(Guid AppId, AppSubscriptionStatusId AppSubscriptionStatus)> GetCompanySubscribedAppSubscriptionStatusesForCompanyUntrackedAsync(Guid companyId);

    /// <summary>
    /// Finds the company assigned app with the company id and app id
    /// </summary>
    /// <param name="companyId">id of the company</param>
    /// <param name="appId">id of the app</param>
    /// <returns>Returns the found app or null</returns>
    Task<CompanyAssignedApp?> FindAsync(Guid companyId, Guid appId);

    /// <summary>
    /// Gets the provided app subscription statuses for the user and given company
    /// </summary>
    /// <param name="companyId">Id of the company</param>
    /// <returns>Returns a IAsyncEnumerable of the found <see cref="AppCompanySubscriptionStatusData"/></returns>
    IAsyncEnumerable<AppCompanySubscriptionStatusData> GetCompanyProvidedAppSubscriptionStatusesForUserAsync(Guid companyId);
}
