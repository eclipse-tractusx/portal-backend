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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

public interface IUserRolesBusinessLogic
{
    IAsyncEnumerable<OfferRoleInfos> GetCoreOfferRoles(Guid companyId, string? languageShortName);
    IAsyncEnumerable<OfferRoleInfo> GetAppRolesAsync(Guid appId, Guid companyId, string? languageShortName);

    /// <summary>
    /// Update Role to User
    /// </summary>
    /// <param name="offerId"></param>
    /// <param name="companyUserId"></param>
    /// <param name="roles"></param>
    /// <param name="identity">Admin User</param>
    /// <returns>messages</returns>
    Task<IEnumerable<UserRoleWithId>> ModifyCoreOfferUserRolesAsync(Guid offerId, Guid companyUserId, IEnumerable<string> roles, IdentityData identity);

    /// <summary>
    /// Update Role to User
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="companyUserId"></param>
    /// <param name="roles"></param>
    /// <param name="identity">Admin User Id</param>
    /// <returns>messages</returns>
    Task<IEnumerable<UserRoleWithId>> ModifyAppUserRolesAsync(Guid appId, Guid companyUserId, IEnumerable<string> roles, IdentityData identity);

    /// <summary>
    /// Update Role to User
    /// </summary>
    /// <param name="appId">app Id</param>
    /// <param name="userRoleInfo">User and Role Information like CompanyUser Id and Role Name</param>
    /// <param name="identity">Admin User</param>
    /// <returns>messages</returns>
    [Obsolete("to be replaced by endpoint UserRolesBusinessLogic.ModifyAppUserRolesAsync. Remove as soon frontend is adjusted")]
    Task<IEnumerable<UserRoleWithId>> ModifyUserRoleAsync(Guid appId, UserRoleInfo userRoleInfo, IdentityData identity);
}
