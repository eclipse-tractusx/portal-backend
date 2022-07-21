﻿/********************************************************************************
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
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

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
    CompanyAssignedApp CreateCompanyAssignedApp(Guid appId, Guid companyId, AppSubscriptionStatusId appSubscriptionStatusId);

    IQueryable<CompanyUserAssignedRole> GetOwnCompanyAppUsersUntrackedAsync(Guid appId, Guid companyId, Guid iamClientId);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="iamUserId"></param>
    IAsyncEnumerable<AppWithSubscriptionStatus> GetOwnCompanySubscribedAppSubscriptionStatusesUntrackedAsync(string iamUserId);

    /// <summary>
    /// Gets the provided app subscription statuses for the user and given company
    /// </summary>
    /// <param name="iamUserId">Id of user of the Providercompany</param>
    /// <returns>Returns a IAsyncEnumerable of the found <see cref="AppCompanySubscriptionStatusData"/></returns>
    IAsyncEnumerable<AppCompanySubscriptionStatusData> GetOwnCompanyProvidedAppSubscriptionStatusesUntrackedAsync(string iamUserId);

    Task<(CompanyAssignedApp? companyAssignedApp, bool isMemberOfCompanyProvidingApp)> GetCompanyAssignedAppDataForProvidingCompanyUserAsync(Guid appId, Guid companyId, string iamUserId);
    
    Task<(CompanyAssignedApp? companyAssignedApp, bool _)> GetCompanyAssignedAppDataForCompanyUserAsync(Guid appId, string iamUserId);

    Task<(Guid companyId, CompanyAssignedApp? companyAssignedApp)> GetCompanyIdWithAssignedAppForCompanyUserAsync(Guid appId, string iamUserId);
}
