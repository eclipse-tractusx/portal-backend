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

using Org.Eclipse.TractusX.Portal.Backend.Apps.Service.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Apps.Service.ViewModels;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Linq;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

namespace Org.Eclipse.TractusX.Portal.Backend.Apps.Service.Extensions;

/// <summary>
/// Extension methods for the apps
/// </summary>
public static class AppExtensions
{
    /// <summary>
    /// Validates the app user role data
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="appUserRolesDescription"></param>
    /// <exception cref="ControllerArgumentException"></exception>
    public static void ValidateAppUserRole(Guid appId, IEnumerable<AppUserRole> appUserRolesDescription)
    {
        if (appId == Guid.Empty)
        {
            throw ControllerArgumentException.Create(AppExtensionErrors.APP_ARG_APP_ID_NOT_EMPTY);
        }
        var descriptions = appUserRolesDescription.SelectMany(x => x.Descriptions).Where(item => !string.IsNullOrWhiteSpace(item.LanguageCode)).Distinct();
        if (!descriptions.Any())
        {
            throw ControllerArgumentException.Create(AppExtensionErrors.APP_ARG_LANG_CODE_NOT_EMPTY);
        }
        appUserRolesDescription.DuplicatesBy(x => x.Role).IfAny(duplicateRoles => throw ControllerArgumentException.Create(AppExtensionErrors.APP_ARG_ROLES_ARE_AMBIGUOUS, new ErrorParameter[] { new("duplicateRoles", string.Join(",", duplicateRoles.Select(x => x.Role))) }));
    }

    /// <summary>
    /// Creates the user roles with their descriptions
    /// </summary>
    /// <remarks>Doesn't save the changes</remarks>
    /// <param name="userRolesRepository">repository</param>
    /// <param name="appId">id of the app to create the roles for</param>
    /// <param name="userRoles">the user roles to add</param>
    /// <returns>returns the created appRoleData</returns>
    public static async Task<IEnumerable<AppRoleData>> CreateUserRolesWithDescriptions(IUserRolesRepository userRolesRepository, Guid appId, IEnumerable<AppUserRole> userRoles)
    {
        userRoles = await GetUniqueAppUserRoles(userRolesRepository, appId, userRoles);
        return userRoles.Zip(
            userRolesRepository.CreateAppUserRoles(userRoles.Select(x => (appId, x.Role))),
            (AppUserRole appUserRole, UserRole userRole) =>
                {
                    userRolesRepository.CreateAppUserRoleDescriptions(appUserRole.Descriptions.Select(appUserRoleDescription => (userRole.Id, appUserRoleDescription.LanguageCode, appUserRoleDescription.Description)));
                    return new AppRoleData(userRole.Id, appUserRole.Role);
                })
            .ToList();
    }

    /// <summary>
    /// Get unique roles by eleminating the duplicate roles from the request (client) and existing roles from the Database
    /// </summary>
    /// <remarks></remarks>
    /// <param name="userRolesRepository">repository</param>
    /// <param name="appId">id of the app</param>
    /// <param name="userRoles">the app user roles</param>
    /// <returns>returns the filtered and unique roles</returns>
    private static async Task<IEnumerable<AppUserRole>> GetUniqueAppUserRoles(IUserRolesRepository userRolesRepository, Guid appId, IEnumerable<AppUserRole> userRoles)
    {
        var existingRoles = await userRolesRepository.GetUserRolesForOfferIdAsync(appId).ToListAsync().ConfigureAwait(false);
        return userRoles.ExceptBy(existingRoles, userRole => userRole.Role);
    }
}
