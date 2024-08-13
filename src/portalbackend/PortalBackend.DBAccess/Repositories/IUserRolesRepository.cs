/********************************************************************************
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

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
    IdentityAssignedRole CreateIdentityAssignedRole(Guid companyUserId, Guid companyUserRoleId);
    void CreateIdentityAssignedRoleRange(IEnumerable<(Guid CompanyUserId, Guid CompanyUserRoleId)> companyUserRoleIds);
    IdentityAssignedRole DeleteIdentityAssignedRole(Guid companyUserId, Guid userRoleId);
    void DeleteCompanyUserAssignedRoles(IEnumerable<(Guid CompanyUserId, Guid UserRoleId)> companyUserAssignedRoleIds);
    IAsyncEnumerable<UserRoleData> GetUserRoleDataUntrackedAsync(IEnumerable<Guid> userRoleIds);
    IAsyncEnumerable<UserRoleData> GetUserRoleDataUntrackedAsync(IEnumerable<UserRoleConfig> clientRoles);
    IAsyncEnumerable<Guid> GetUserRoleIdsUntrackedAsync(IEnumerable<UserRoleConfig> clientRoles);
    IAsyncEnumerable<UserRoleData> GetOwnCompanyPortalUserRoleDataUntrackedAsync(string clientId, IEnumerable<string> roles, Guid companyId);
    IAsyncEnumerable<(Guid OfferId, Guid RoleId, string RoleText, string Description)> GetCoreOfferRolesAsync(Guid companyId, string languageShortName, string clientId);
    IAsyncEnumerable<OfferRoleInfo> GetAppRolesAsync(Guid offerId, Guid companyId, string languageShortName);
    IAsyncEnumerable<string> GetClientRolesCompositeAsync(string keyCloakClientId);

    IAsyncEnumerable<UserRoleWithDescription> GetServiceAccountRolesAsync(Guid companyId, string clientId, string languageShortName);

    /// <summary>
    /// Gets all user role ids for the given offerId
    /// </summary>
    /// <param name="offerId">Id of the offer the roles are assigned to.</param>
    /// <returns>Returns a list of user role ids</returns>
    IAsyncEnumerable<string> GetUserRolesForOfferIdAsync(Guid offerId);

    IAsyncEnumerable<(string UserRoleText, Guid RoleId, bool IsAssigned)> GetAssignedAndMatchingAppRoles(Guid identityId, IEnumerable<string> userRoles, Guid offerId);
    IAsyncEnumerable<UserRoleModificationData> GetAssignedAndMatchingCoreOfferRoles(Guid identityId, IEnumerable<string> userRoles, Guid offerId);

    /// <summary>
    /// Get user name data by assinged roles
    /// </summary>
    /// <param name="companyId"></param>
    /// <param name="clientRoles"></param>
    /// <returns></returns>
    IAsyncEnumerable<CompanyUserNameData> GetUserDataByAssignedRoles(Guid companyId, IEnumerable<UserRoleConfig> clientRoles);

    IAsyncEnumerable<(string ClientClientId, IEnumerable<(Guid UserRoleId, string UserRoleText)> UserRoles)> GetUserRolesByClientId(IEnumerable<string> iamClientIds);

    IAsyncEnumerable<(Guid CompanyUserId, IEnumerable<Guid> UserRoleIds)> GetUserWithUserRolesForApplicationId(Guid applicationId, IEnumerable<Guid> userRoleIds);
    IAsyncEnumerable<Guid> GetRolesForClient(string technicalUserProfileClient);

    /// <summary>
    /// Gets userRoles for an offerId
    /// </summary>
    /// <param name="offerId"></param>
    /// <param name="languageShortName"></param>
    /// <returns></returns>
    Task<(bool IsValid, bool IsActive, IEnumerable<ActiveAppRoleDetails>? AppRoleDetails)> GetActiveOfferRolesAsync(Guid offerId, OfferTypeId offerTypeId, string? languageShortName, string defaultLanguageShortName);

    /// <summary>
    /// Gets userRoles for an app provider
    /// </summary>
    /// <param name="offerId"></param>
    /// <param name="languageShortName"></param>
    /// <returns></returns>
    Task<(bool IsValid, bool IsProvider, IEnumerable<ActiveAppRoleDetails>? AppRoleDetails)> GetOfferProviderRolesAsync(Guid offerId, OfferTypeId offerTypeId, Guid companyId, string? languageShortName, string defaultLanguageShortName);
}
