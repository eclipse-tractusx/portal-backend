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

using Org.CatenaX.Ng.Portal.Backend.Administration.Service.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.CatenaX.Ng.Portal.Backend.Provisioning.Library.Models;
using Org.CatenaX.Ng.Portal.Backend.Framework.Models;

namespace Org.CatenaX.Ng.Portal.Backend.Administration.Service.BusinessLogic;
/// <summary>
/// Business Logic for Handling User Management Operation
/// </summary>
public interface IUserBusinessLogic
{
    IAsyncEnumerable<string> CreateOwnCompanyUsersAsync(IEnumerable<UserCreationInfo> userList, string iamUserId);
    Task<Guid> CreateOwnCompanyIdpUserAsync(Guid identityProviderId, UserCreationInfoIdp userCreationInfo, string iamUserId);
    Task<Pagination.Response<CompanyUserData>> GetOwnCompanyUserDatasAsync(string adminUserId, int page, int size, Guid? companyUserId = null, string? userEntityId = null, string? firstName = null, string? lastName = null, string? email = null);
    IAsyncEnumerable<ClientRoles> GetClientRolesAsync(Guid appId, string? languageShortName = null);
    Task<CompanyUserDetails> GetOwnCompanyUserDetailsAsync(Guid companyUserId, string iamUserId);
    Task<int> AddOwnCompanyUsersBusinessPartnerNumbersAsync(Guid companyUserId, IEnumerable<string> businessPartnerNumbers, string adminUserId);
    Task<int> AddOwnCompanyUsersBusinessPartnerNumberAsync(Guid companyUserId, string businessPartnerNumber, string adminUserId);
    Task<CompanyUserDetails> GetOwnUserDetails(string iamUserId);
    Task<CompanyUserDetails> UpdateOwnUserDetails(Guid companyUserId, OwnCompanyUserEditableDetails ownCompanyUserEditableDetails, string iamUserId);
    Task<int> DeleteOwnUserAsync(Guid companyUserId, string iamUserId);
    IAsyncEnumerable<Guid> DeleteOwnCompanyUsersAsync(IEnumerable<Guid> companyUserIds, string iamUserId);
    Task<bool> AddBpnAttributeAsync(IEnumerable<UserUpdateBpn>? usersToUdpateWithBpn);
    Task<bool> ExecuteOwnCompanyUserPasswordReset(Guid companyUserId, string adminUserId);
    Task<Pagination.Response<CompanyAppUserDetails>> GetOwnCompanyAppUsersAsync( Guid appId,string iamUserId, int page, int size, string? firstName = null, string? lastName = null, string? email = null,string? roleName = null);

    /// <summary>
    /// Update Role to User
    /// </summary>
    /// <param name="appId">app Id</param>
    /// <param name="userRoleInfo">User and Role Information like CompanyUser Id and Role Name</param>
    /// <param name="adminUserId">Admin User Id</param>
    /// <returns>messages</returns>
    Task<IEnumerable<UserRoleWithId>> ModifyUserRoleAsync(Guid appId, UserRoleInfo userRoleInfo, string adminUserId);

    Task<int> DeleteOwnUserBusinessPartnerNumbersAsync(Guid companyUserId, string businessPartnerNumber, string adminUserId);
}
