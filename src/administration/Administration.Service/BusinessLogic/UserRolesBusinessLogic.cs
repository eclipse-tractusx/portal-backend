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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Async;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

public class UserRolesBusinessLogic: IUserRolesBusinessLogic
{
    private static readonly JsonSerializerOptions _options = new (){ PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private readonly IPortalRepositories _portalRepositories;
    private readonly IProvisioningManager _provisioningManager;

    public UserRolesBusinessLogic(IPortalRepositories portalRepositories, IProvisioningManager provisioningManager)
    {
        _portalRepositories = portalRepositories;
        _provisioningManager = provisioningManager;
    }

    public IAsyncEnumerable<OfferRoleInfos> GetCoreOfferRoles(string iamUserId, string? languageShortName) =>
        _portalRepositories.GetInstance<IUserRolesRepository>().GetCoreOfferRolesAsync(iamUserId, languageShortName ?? IUserRolesBusinessLogic.DEFAULT_LANGUAGE)
            .PreSortedGroupBy(x => x.OfferId)
            .Select(x => new OfferRoleInfos(x.Key, x.Select(s => new OfferRoleInfo(s.RoleId,s.RoleText,s.Description))));

    public IAsyncEnumerable<OfferRoleInfo> GetAppRolesAsync(Guid appId, string iamUserId, string? languageShortName) =>
        _portalRepositories.GetInstance<IUserRolesRepository>()
            .GetAppRolesAsync(appId, iamUserId, languageShortName ?? IUserRolesBusinessLogic.DEFAULT_LANGUAGE);

    public Task<IEnumerable<UserRoleWithId>> ModifyCoreOfferUserRolesAsync(Guid offerId, Guid companyUserId, IEnumerable<string> roles, string iamUserId)
    {
        return ModifyUserRolesInternal(
            async () =>
            {
                var result = await _portalRepositories.GetInstance<IUserRepository>()
                    .GetCoreOfferAssignedIamClientUserDataUntrackedAsync(offerId, companyUserId, iamUserId).ConfigureAwait(false);
                return result == null
                    ? null
                    : new OfferIamUserData(
                        result.IsValidOffer,
                        result.IamClientIds,
                        result.IamUserId,
                        result.IsSameCompany,
                        "Portal",
                        result.Firstname,
                        result.Lastname
                    );
            },
            (Guid companyUserId, IEnumerable<string> roles, Guid offerId) => _portalRepositories.GetInstance<IUserRolesRepository>()
                .GetAssignedAndMatchingCoreOfferRoles(companyUserId, roles, offerId),
            offerId, companyUserId, roles, iamUserId,
            data =>
            {
                var userName = $"{data.firstname} {data.lastname}";
                return (JsonSerializer.Serialize(new
                {
                    OfferId = data.offerId,
                    CoreOfferName = data.offerName,
                    Username = string.IsNullOrWhiteSpace(userName) ? "User" : userName,
                    RemovedRoles = string.Join(",", data.removedRoles),
                    AddedRoles = string.Join(",", data.addedRoles)
                }, _options), NotificationTypeId.ROLE_UPDATE_CORE_OFFER);
            });
    }

    public Task<IEnumerable<UserRoleWithId>> ModifyAppUserRolesAsync(Guid appId, Guid companyUserId, IEnumerable<string> roles, string iamUserId) =>
        ModifyUserRolesInternal(
            () => _portalRepositories.GetInstance<IUserRepository>()
                .GetAppAssignedIamClientUserDataUntrackedAsync(appId, companyUserId, iamUserId),
            (Guid companyUserId, IEnumerable<string> roles, Guid offerId) => _portalRepositories.GetInstance<IUserRolesRepository>()
                .GetAssignedAndMatchingAppRoles(companyUserId, roles, offerId),
            appId, companyUserId, roles, iamUserId, 
            data =>
            {
                var userName = $"{data.firstname} {data.lastname}";
                return (JsonSerializer.Serialize(new
                {
                    OfferId = data.offerId,
                    AppName = data.offerName,
                    Username = string.IsNullOrWhiteSpace(userName) ? "User" : userName,
                    RemovedRoles = string.Join(",", data.removedRoles),
                    AddedRoles = string.Join(",", data.addedRoles)
                }, _options), NotificationTypeId.ROLE_UPDATE_APP_OFFER);
            });

    [Obsolete("to be replaced by endpoint UserRolesBusinessLogic.ModifyAppUserRolesAsync. Remove as soon frontend is adjusted")]
    public Task<IEnumerable<UserRoleWithId>> ModifyUserRoleAsync(Guid appId, UserRoleInfo userRoleInfo, string adminUserId) =>
        ModifyUserRolesInternal(
            () => _portalRepositories.GetInstance<IUserRepository>()
                .GetAppAssignedIamClientUserDataUntrackedAsync(appId, userRoleInfo.CompanyUserId, adminUserId),
            (Guid companyUserId, IEnumerable<string> roles, Guid offerId) => _portalRepositories.GetInstance<IUserRolesRepository>()
                .GetAssignedAndMatchingAppRoles(companyUserId, roles, offerId),
            appId, userRoleInfo.CompanyUserId, userRoleInfo.Roles, adminUserId, null);

    private async Task<IEnumerable<UserRoleWithId>> ModifyUserRolesInternal(
        Func<Task<OfferIamUserData?>> getIamUserData,
        Func<Guid, IEnumerable<string>, Guid, IAsyncEnumerable<UserRoleModificationData>> getUserRoleModificationData,
        Guid offerId, Guid companyUserId, IEnumerable<string> roles, string iamUserId,
        Func<(Guid offerId, string offerName, string? firstname, string? lastname, IEnumerable<string> removedRoles, IEnumerable<string> addedRoles), (string content, NotificationTypeId notificationTypeId)>? getNotificationData)
    {
        var result = await getIamUserData().ConfigureAwait(false);
        if (result == default || string.IsNullOrWhiteSpace(result.IamUserId))
        {
            throw new NotFoundException($"iamUserId for user {companyUserId} not found");
        }
        
        if (!result.IsSameCompany)
        {
            throw new ForbiddenException(
                $"CompanyUserId {companyUserId} is not associated with the same company as adminUserId {iamUserId}");
        }

        if (!result.IsValidOffer)
        {
            throw new NotFoundException($"offerId {offerId} not found for user {companyUserId}");
        }

        if (!result.IamClientIds.Any())
        {
            throw new ConflictException($"offerId {offerId} is not associated with any keycloak-client");
        }

        if (result.IamUserId == null)
        {
            throw new ConflictException($"user {companyUserId} is not associated with any iamUser");
        }

        if (string.IsNullOrWhiteSpace(result.OfferName))
        {
            throw new ConflictException("OfferName must be set here.");
        }

        var distinctRoles = roles.Where(role => !string.IsNullOrWhiteSpace(role)).Distinct().ToList();
        var existingRoles = await getUserRoleModificationData(companyUserId, distinctRoles, offerId).ToListAsync().ConfigureAwait(false);
        var nonExistingRoles = distinctRoles.Except(existingRoles.Select(r => r.CompanyUserRoleText));
        if (nonExistingRoles.Any())
        {
            throw new ControllerArgumentException($"Invalid roles {string.Join(",", nonExistingRoles)}", nameof(roles));
        }
        var rolesToAdd = existingRoles.Where(role => !role.IsAssignedToUser);
        var rolesToDelete =  existingRoles.Where(x => x.IsAssignedToUser).ExceptBy(distinctRoles, role => role.CompanyUserRoleText);

        var rolesNotAdded = rolesToAdd.Any()
            ? rolesToAdd.Except(await AddRoles(companyUserId, result.IamClientIds, rolesToAdd, result.IamUserId).ConfigureAwait(false))
            : Enumerable.Empty<UserRoleModificationData>();

        if (rolesToDelete.Any())
        {
            await DeleteRoles(companyUserId, result.IamClientIds, rolesToDelete, iamUserId).ConfigureAwait(false);
        }

        if (getNotificationData != null)
        {
            var data = (
                offerId,
                result.OfferName,
                result.Firstname,
                result.Lastname,
                rolesToDelete.Select(x => x.CompanyUserRoleText),
                rolesToAdd.Select(x => x.CompanyUserRoleText)
            );
            var notificationData = getNotificationData(data);
            _portalRepositories.GetInstance<INotificationRepository>().CreateNotification(companyUserId,
                notificationData.notificationTypeId, false,
                notification =>
                {
                    notification.Content = notificationData.content;
                });
        }

        await _portalRepositories.SaveAsync().ConfigureAwait(false);
        
        return rolesNotAdded.Select(x => new UserRoleWithId(x.CompanyUserRoleText, x.CompanyUserRoleId));
    }

    private async Task<IEnumerable<UserRoleModificationData>> AddRoles(Guid companyUserId, IEnumerable<string> iamClientIds, IEnumerable<UserRoleModificationData> rolesToAdd, string iamUserId)
    {
        var userRoleRepository = _portalRepositories.GetInstance<IUserRolesRepository>();
        var clientRoleNames = iamClientIds.ToDictionary(clientId => clientId, _ => rolesToAdd.Select(x => x.CompanyUserRoleText));
        var assignedRoles = await _provisioningManager.AssignClientRolesToCentralUserAsync(iamUserId, clientRoleNames).SingleAsync().ConfigureAwait(false);

        var rolesAdded = rolesToAdd.IntersectBy(assignedRoles.Roles, role => role.CompanyUserRoleText).ToList();
        foreach (var roleWithId in rolesAdded)
        {
            userRoleRepository.CreateCompanyUserAssignedRole(companyUserId, roleWithId.CompanyUserRoleId);
        }
        return rolesAdded;
    }

    private async Task DeleteRoles(Guid companyUserId, IEnumerable<string> iamClientIds, IEnumerable<UserRoleModificationData> rolesToDelete, string iamUserId)
    {
        var roleNamesToDelete = iamClientIds.ToDictionary(clientId => clientId, _ => rolesToDelete.Select(x => x.CompanyUserRoleText));
        await _provisioningManager.DeleteClientRolesFromCentralUserAsync(iamUserId, roleNamesToDelete)
            .ConfigureAwait(false);
        _portalRepositories.RemoveRange(rolesToDelete.Select(x =>
            new CompanyUserAssignedRole(companyUserId, x.CompanyUserRoleId)));
    }
}
