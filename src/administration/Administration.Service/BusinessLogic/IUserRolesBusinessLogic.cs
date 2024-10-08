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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

public interface IUserRolesBusinessLogic
{
    IAsyncEnumerable<OfferRoleInfos> GetCoreOfferRoles(string? languageShortName);
    IAsyncEnumerable<OfferRoleInfo> GetAppRolesAsync(Guid appId, string? languageShortName);

    /// <summary>
    /// Update Role to User
    /// </summary>
    /// <param name="offerId"></param>
    /// <param name="companyUserId"></param>
    /// <param name="roles"></param>
    /// <returns>messages</returns>
    Task<IEnumerable<UserRoleWithId>> ModifyCoreOfferUserRolesAsync(Guid offerId, Guid companyUserId, IEnumerable<string> roles);

    /// <summary>
    /// Update Role to User
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="companyUserId"></param>
    /// <param name="roles"></param>
    /// <returns>messages</returns>
    Task<IEnumerable<UserRoleWithId>> ModifyAppUserRolesAsync(Guid appId, Guid companyUserId, IEnumerable<string> roles);
}
