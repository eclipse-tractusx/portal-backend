/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

public interface IUserRolesRepository
{
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
    CompanyUserAssignedRole CreateCompanyUserAssignedRole(Guid companyUserId, Guid companyUserRoleId);
    IAsyncEnumerable<CompanyUser> GetCompanyUserRolesIamUsersAsync(IEnumerable<Guid> companyUserIds, Guid companyUserId);
    CompanyUserAssignedRole RemoveCompanyUserAssignedRole(CompanyUserAssignedRole companyUserAssignedRole);
    IAsyncEnumerable<UserRoleData> GetUserRoleDataUntrackedAsync(IEnumerable<Guid> userRoleIds);
    IAsyncEnumerable<Guid> GetUserRoleIdsUntrackedAsync(IDictionary<string, IEnumerable<string>> clientRoles);
    IAsyncEnumerable<UserRoleWithId> GetUserRoleWithIdsUntrackedAsync(string clientClientId, IEnumerable<string> userRoles);
    IAsyncEnumerable<UserRoleData> GetUserRoleDataUntrackedAsync(IDictionary<string, IEnumerable<string>> clientRoles);
    IAsyncEnumerable<(string Role,Guid Id)> GetUserRolesWithIdAsync(string keyCloakClientId);
    IAsyncEnumerable<string> GetClientRolesCompositeAsync(string keyCloakClientId);
    IAsyncEnumerable<UserRoleWithDescription> GetServiceAccountRolesAsync(string clientId,string? languageShortName = null);

    /// <summary>
    /// Gets all user role ids for the given offerId
    /// </summary>
    /// <param name="offerId">Id of the offer the roles are assigned to.</param>
    /// <returns>Returns a list of user role ids</returns>
    Task<List<string>> GetUserRolesForOfferIdAsync(Guid offerId);

    IAsyncEnumerable<UserRoleModificationData> GetAssignedAndMatchingRoles(Guid companyUserId, IEnumerable<string> userRoles, Guid offerId);
}
