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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

public interface IUserRolesRepository
{
    IEnumerable<UserRole> CreateAppUserRoles(IEnumerable<(Guid AppId, string Role)> appIdRoles);

    /// <summary>
    /// Delete an existing User Role
    /// </summary>
    /// <param name="roleId"></param>
    /// <returns></returns>
    UserRole DeleteUserRole(Guid roleId);

    IEnumerable<UserRoleDescription> CreateAppUserRoleDescriptions(IEnumerable<(Guid RoleId, string LanguageCode, string Description)> roleLanguageDescriptions);
    CompanyUserAssignedRole CreateCompanyUserAssignedRole(Guid companyUserId, Guid companyUserRoleId);
    CompanyUserAssignedRole DeleteCompanyUserAssignedRole(Guid companyUserId, Guid userRoleId);
    void DeleteCompanyUserAssignedRoles(IEnumerable<(Guid CompanyUserId, Guid UserRoleId)> companyUserAssignedRoleIds);
    IAsyncEnumerable<UserRoleData> GetUserRoleDataUntrackedAsync(IEnumerable<Guid> userRoleIds);
    IAsyncEnumerable<Guid> GetUserRoleIdsUntrackedAsync(IDictionary<string, IEnumerable<string>> clientRoles);
    IAsyncEnumerable<UserRoleData> GetUserRoleDataUntrackedAsync(IDictionary<string, IEnumerable<string>> clientRoles);
    IAsyncEnumerable<UserRoleData> GetOwnCompanyPortalUserRoleDataUntrackedAsync(string clientId, IEnumerable<string> roles, string iamUserId);
    IAsyncEnumerable<(Guid OfferId, Guid RoleId, string RoleText, string Description)> GetCoreOfferRolesAsync(string iamUserId, string languageShortName);
    IAsyncEnumerable<OfferRoleInfo> GetAppRolesAsync(Guid offerId, string iamUserid, string languageShortName);
    IAsyncEnumerable<string> GetClientRolesCompositeAsync(string keyCloakClientId);
    IAsyncEnumerable<UserRoleWithDescription> GetServiceAccountRolesAsync(string clientId,string? languageShortName = null);

    /// <summary>
    /// Gets all user role ids for the given offerId
    /// </summary>
    /// <param name="offerId">Id of the offer the roles are assigned to.</param>
    /// <returns>Returns a list of user role ids</returns>
    IAsyncEnumerable<string> GetUserRolesForOfferIdAsync(Guid offerId);

    IAsyncEnumerable<UserRoleModificationData> GetAssignedAndMatchingAppRoles(Guid companyUserId, IEnumerable<string> userRoles, Guid offerId);
    IAsyncEnumerable<UserRoleModificationData> GetAssignedAndMatchingCoreOfferRoles(Guid companyUserId, IEnumerable<string> userRoles, Guid offerId);

    /// <summary>
    /// Get user name data by assinged roles
    /// </summary>
    /// <param name="iamUserId"></param>
    /// <param name="clientRoles"></param>
    /// <returns></returns>
    IAsyncEnumerable<CompanyUserNameData> GetUserDataByAssignedRoles(string iamUserId, IDictionary<string, IEnumerable<string>> clientRoles);

    IAsyncEnumerable<(string ClientClientId, IEnumerable<(Guid UserRoleId, string UserRoleText)> UserRoles)> GetUserRolesByClientId(IEnumerable<string> iamClientIds);
    
    IAsyncEnumerable<(Guid CompanyUserId, string UserEntityId, IEnumerable<Guid> UserRoleIds)> GetUserWithUserRolesForApplicationId(Guid applicationId, IEnumerable<Guid> userRoleIds);
    IAsyncEnumerable<Guid> GetRolesForClient(string technicalUserProfileClient);
}
