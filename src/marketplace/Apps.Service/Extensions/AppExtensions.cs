/********************************************************************************
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

using Org.Eclipse.TractusX.Portal.Backend.Apps.Service.ViewModels;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Library;
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
            throw new ControllerArgumentException("AppId must not be empty");
        }
        var descriptions = appUserRolesDescription.SelectMany(x => x.Descriptions).Where(item => !string.IsNullOrWhiteSpace(item.LanguageCode)).Distinct();
        if (!descriptions.Any())
        {
            throw new ControllerArgumentException("Language Code must not be empty");
        }
    }

    /// <summary>
    /// Creates the user roles with their descriptions
    /// </summary>
    /// <remarks>Doesn't save the changes</remarks>
    /// <param name="userRolesRepository">repository</param>
    /// <param name="appId">id of the app to create the roles for</param>
    /// <param name="userRoles">the user roles to add</param>
    /// <returns>returns the created appRoleData</returns>
    public static IEnumerable<AppRoleData> CreateUserRolesWithDescriptions(IUserRolesRepository userRolesRepository, Guid appId, IEnumerable<AppUserRole> userRoles) =>
        userRoles.Zip(
            userRolesRepository.CreateAppUserRoles(userRoles.Select(x => (appId, x.Role))),
            (AppUserRole appUserRole, UserRole userRole) =>
                {
                    userRolesRepository.CreateAppUserRoleDescriptions(appUserRole.Descriptions.Select(appUserRoleDescription => (userRole.Id, appUserRoleDescription.LanguageCode, appUserRoleDescription.Description)));
                    return new AppRoleData(userRole.Id, appUserRole.Role);
                })
            .ToList();
}
