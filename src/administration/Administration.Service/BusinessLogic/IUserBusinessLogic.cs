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

using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;
/// <summary>
/// Business Logic for Handling User Management Operation
/// </summary>
public interface IUserBusinessLogic
{
    IAsyncEnumerable<string> CreateOwnCompanyUsersAsync(IEnumerable<UserCreationInfo> userList, (Guid UserId, Guid CompanyId) identity);
    Task<Guid> CreateOwnCompanyIdpUserAsync(Guid identityProviderId, UserCreationInfoIdp userCreationInfo, (Guid UserId, Guid CompanyId) identity);
    Task<Pagination.Response<CompanyUserData>> GetOwnCompanyUserDatasAsync(Guid companyId, int page, int size, GetOwnCompanyUsersFilter filter);
    [Obsolete("to be replaced by UserRolesBusinessLogic.GetAppRolesAsync. Remove as soon frontend is adjusted")]
    IAsyncEnumerable<ClientRoles> GetClientRolesAsync(Guid appId, string? languageShortName = null);
    Task<CompanyUserDetails> GetOwnCompanyUserDetailsAsync(Guid companyUserId, Guid companyId);
    Task<int> AddOwnCompanyUsersBusinessPartnerNumbersAsync(Guid companyUserId, IEnumerable<string> businessPartnerNumbers, Guid companyId);
    Task<int> AddOwnCompanyUsersBusinessPartnerNumberAsync(Guid companyUserId, string businessPartnerNumber, Guid companyId);
    Task<CompanyOwnUserDetails> GetOwnUserDetails(IdentityData identity);
    Task<CompanyUserDetails> UpdateOwnUserDetails(Guid companyUserId, OwnCompanyUserEditableDetails ownCompanyUserEditableDetails, IdentityData identity);

    /// <summary>
    /// Delete User Own Account using userId
    /// </summary>
    /// <param name="companyUserId"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<int> DeleteOwnUserAsync(Guid companyUserId, Guid userId);
    IAsyncEnumerable<Guid> DeleteOwnCompanyUsersAsync(IEnumerable<Guid> companyUserIds, (Guid UserId, Guid CompanyId) identity);
    Task<bool> ExecuteOwnCompanyUserPasswordReset(Guid companyUserId, IdentityData identity);
    Task<Pagination.Response<CompanyAppUserDetails>> GetOwnCompanyAppUsersAsync(Guid appId, Guid userId, int page, int size, CompanyUserFilter filter);
    Task<int> DeleteOwnUserBusinessPartnerNumbersAsync(Guid companyUserId, string businessPartnerNumber, (Guid UserId, Guid CompanyId) identity);
}
