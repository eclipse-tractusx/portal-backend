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

using Microsoft.EntityFrameworkCore;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Identity;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

public class UserRolesRepository : IUserRolesRepository
{
    private readonly PortalDbContext _dbContext;

    public UserRolesRepository(PortalDbContext portalDbContext)
    {
        _dbContext = portalDbContext;
    }

    public IEnumerable<UserRole> CreateAppUserRoles(IEnumerable<(Guid AppId, string Role)> appIdRoles)
    {
        var appRoles = appIdRoles.Select(x => new UserRole(Guid.NewGuid(), x.Role, x.AppId)).ToList();
        _dbContext.AddRange(appRoles);
        return appRoles;
    }

    ///<inheritdoc/>
    public UserRole DeleteUserRole(Guid roleId) =>
        _dbContext.Remove(new UserRole(roleId, null!, Guid.Empty)).Entity;

    public IEnumerable<UserRoleDescription> CreateAppUserRoleDescriptions(IEnumerable<(Guid RoleId, string LanguageCode, string Description)> roleLanguageDescriptions)
    {
        var roleDescriptions = roleLanguageDescriptions.Select(x => new UserRoleDescription(x.RoleId, x.LanguageCode, x.Description)).ToList();
        _dbContext.AddRange(roleDescriptions);
        return roleDescriptions;
    }

    public IdentityAssignedRole CreateIdentityAssignedRole(Guid companyUserId, Guid companyUserRoleId) =>
        _dbContext.IdentityAssignedRoles.Add(
            new IdentityAssignedRole(
                companyUserId,
                companyUserRoleId

            )).Entity;

    public void CreateIdentityAssignedRoleRange(IEnumerable<(Guid CompanyUserId, Guid CompanyUserRoleId)> companyUserRoleIds) =>
        _dbContext.IdentityAssignedRoles.AddRange(
            companyUserRoleIds.Select(x =>
                new IdentityAssignedRole(
                    x.CompanyUserId,
                    x.CompanyUserRoleId)));

    public IdentityAssignedRole DeleteIdentityAssignedRole(Guid companyUserId, Guid userRoleId) =>
        _dbContext.IdentityAssignedRoles.Remove(
            new IdentityAssignedRole(
                companyUserId,
                userRoleId
            )).Entity;

    public void DeleteCompanyUserAssignedRoles(IEnumerable<(Guid CompanyUserId, Guid UserRoleId)> companyUserAssignedRoleIds) =>
        _dbContext.IdentityAssignedRoles.RemoveRange(companyUserAssignedRoleIds.Select(ids => new IdentityAssignedRole(ids.CompanyUserId, ids.UserRoleId)));

    public IAsyncEnumerable<UserRoleData> GetUserRoleDataUntrackedAsync(IEnumerable<Guid> userRoleIds) =>
        _dbContext.UserRoles
            .AsNoTracking()
            .Where(userRole => userRoleIds.Contains(userRole.Id))
            .Select(userRole => new UserRoleData(
                userRole.Id,
                userRole.Offer!.AppInstances.First().IamClient!.ClientClientId,
                userRole.UserRoleText))
            .ToAsyncEnumerable();

    public async IAsyncEnumerable<UserRoleData> GetUserRoleDataUntrackedAsync(IEnumerable<UserRoleConfig> clientRoles)
    {
        foreach (var clientRole in clientRoles)
        {
            await foreach (var userRoleData in _dbContext.UserRoles
                .AsNoTracking()
                .Where(userRole => userRole.Offer!.AppInstances.Any(ai => ai.IamClient!.ClientClientId == clientRole.ClientId) && clientRole.UserRoleNames.Contains(userRole.UserRoleText))
                .Select(userRole => new
                {
                    Id = userRole.Id,
                    Text = userRole.UserRoleText
                })
                .AsAsyncEnumerable()
                .ConfigureAwait(false))
            {
                yield return new UserRoleData(
                    userRoleData.Id,
                    clientRole.ClientId,
                    userRoleData.Text
                );
            }
        }
    }

    public async IAsyncEnumerable<Guid> GetUserRoleIdsUntrackedAsync(IEnumerable<UserRoleConfig> clientRoles)
    {
        foreach (var clientRole in clientRoles)
        {
            await foreach (var userRoleId in _dbContext.UserRoles
                .AsNoTracking()
                .Where(userRole => userRole.Offer!.AppInstances.Any(x => x.IamClient!.ClientClientId == clientRole.ClientId) && clientRole.UserRoleNames.Contains(userRole.UserRoleText))
                .Select(userRole => userRole.Id)
                .AsAsyncEnumerable()
                .ConfigureAwait(false))
            {
                yield return userRoleId;
            }
        }
    }

    public IAsyncEnumerable<(string UserRoleText, Guid RoleId, bool IsAssigned)> GetAssignedAndMatchingAppRoles(Guid identityId, IEnumerable<string> userRoles, Guid offerId) =>
        _dbContext.UserRoles
            .AsNoTracking()
            .Where(role => role.OfferId == offerId)
            .Select(role => new
            {
                Role = role,
                IsAssigned = role.IdentityAssignedRoles.Any(iar => iar.IdentityId == identityId)
            })
            .Where(x =>
                userRoles.Contains(x.Role.UserRoleText) ||
                x.IsAssigned)
            .Select(x => new ValueTuple<string, Guid, bool>(
                x.Role.UserRoleText,
                x.Role.Id,
                x.IsAssigned
            ))
            .ToAsyncEnumerable();

    public IAsyncEnumerable<UserRoleModificationData> GetAssignedAndMatchingCoreOfferRoles(Guid identityId, IEnumerable<string> userRoles, Guid offerId) =>
        _dbContext.UserRoles
            .AsNoTracking()
            .Where(role => role.OfferId == offerId)
            .Select(role => new
            {
                Role = role,
                IsRequested = userRoles.Contains(role.UserRoleText),
                IsAssigned = role.IdentityAssignedRoles.Any(iar => iar.IdentityId == identityId),
                IsAssignable = role.UserRoleCollections.Any(collection => collection.CompanyRoleAssignedRoleCollection!.CompanyRole!.CompanyAssignedRoles.Any(assigned => assigned.Company!.Identities.Any(identity => identity.Id == identityId)))
            })
            // x.IsRequested && x.IsAssigned && x.IsAssignable ||   // no change but required to detect duplicates
            // x.IsRequested && !x.IsAssigned && x.IsAssignable ||  // to be assigned
            // !x.IsRequested && x.IsAssigned ||                    // to be unassigned
            // x.IsRequested && !x.IsAssignable                     // invalid
            // can be simplified to:
            .Where(x => x.IsRequested || x.IsAssigned)
            .Select(x => new UserRoleModificationData(
                x.Role.UserRoleText,
                x.Role.Id,
                x.IsAssigned,
                x.IsAssignable
            ))
            .ToAsyncEnumerable();

    public IAsyncEnumerable<UserRoleData> GetOwnCompanyPortalUserRoleDataUntrackedAsync(string clientId, IEnumerable<string> roles, Guid companyId) =>
        _dbContext.UserRoles
            .AsNoTracking()
            .Where(userRole => userRole.Offer!.AppInstances.Any(ai => ai.IamClient!.ClientClientId == clientId) &&
                roles.Contains(userRole.UserRoleText) &&
                userRole.UserRoleCollections.Any(collection => collection.CompanyRoleAssignedRoleCollection!.CompanyRole!.CompanyAssignedRoles.Any(assigned => assigned.CompanyId == companyId)))
            .Select(userRole => new UserRoleData(
                userRole.Id,
                clientId,
                userRole.UserRoleText))
            .AsAsyncEnumerable();

    public IAsyncEnumerable<(Guid OfferId, Guid RoleId, string RoleText, string Description)> GetCoreOfferRolesAsync(Guid companyId, string languageShortName, string clientId) =>
        _dbContext.UserRoles
            .AsNoTracking()
            .Where(role => role.UserRoleCollections.Any(collection => collection.CompanyRoleAssignedRoleCollection!.CompanyRole!.CompanyAssignedRoles.Any(assigned => assigned.CompanyId == companyId)) &&
                           role.Offer!.AppInstances.Any(ai => ai.IamClient!.ClientClientId == clientId))
            .OrderBy(role => role.OfferId)
            .Select(role => new ValueTuple<Guid, Guid, string, string>(
                role.OfferId,
                role.Id,
                role.UserRoleText,
                role.UserRoleDescriptions.SingleOrDefault(desc => desc.LanguageShortName == languageShortName)!.Description))
            .ToAsyncEnumerable();

    public IAsyncEnumerable<OfferRoleInfo> GetAppRolesAsync(Guid offerId, Guid companyId, string languageShortName) =>
        _dbContext.UserRoles
            .Where(role => role.OfferId == offerId &&
                role.Offer!.OfferSubscriptions.Any(subscription => subscription.CompanyId == companyId))
            .Select(role => new OfferRoleInfo(
                role.Id,
                role.UserRoleText,
                role.UserRoleDescriptions.SingleOrDefault(desc => desc.LanguageShortName == languageShortName)!.Description
            )).AsAsyncEnumerable();

    public IAsyncEnumerable<string> GetClientRolesCompositeAsync(string keyCloakClientId) =>
        _dbContext.UserRoles
            .AsNoTracking()
            .Where(userRole => userRole.Offer!.AppInstances.Any(x => x.IamClient!.ClientClientId == keyCloakClientId))
            .Select(userRole => userRole.UserRoleText)
            .AsAsyncEnumerable();

    IAsyncEnumerable<UserRoleWithDescriptionTransferData> IUserRolesRepository.GetServiceAccountRolesAsync(Guid companyId, string clientId, IEnumerable<Guid> externalRoleIds, string languageShortName) =>
        _dbContext.UserRoles
            .AsNoTracking()
            .Where(ur => ur.Offer!.AppInstances.Any(ai => ai.IamClient!.ClientClientId == clientId) &&
                ur.UserRoleCollections.Any(urc =>
                    urc.CompanyRoleAssignedRoleCollection!.CompanyRole!.CompanyAssignedRoles.Any(car =>
                        car.CompanyId == companyId)))
            .Select(userRole => new UserRoleWithDescriptionTransferData(
                userRole.Id,
                userRole.UserRoleText,
                userRole.UserRoleDescriptions.SingleOrDefault(desc =>
                    desc.LanguageShortName == languageShortName)!.Description,
                externalRoleIds.Contains(userRole.Id)))
            .AsAsyncEnumerable();

    /// <inheritdoc />
    public IAsyncEnumerable<string> GetUserRolesForOfferIdAsync(Guid offerId) =>
        _dbContext.UserRoles
            .Where(x => x.OfferId == offerId)
            .Select(x => x.UserRoleText)
            .AsAsyncEnumerable();

    /// <inheritdoc />
    public async IAsyncEnumerable<CompanyUserNameData> GetUserDataByAssignedRoles(Guid companyId, IEnumerable<UserRoleConfig> clientRoles)
    {
        foreach (var clientRole in clientRoles)
        {
            await foreach (var companyUserData in _dbContext.IdentityAssignedRoles
                .AsNoTracking()
                .Where(companyAssignedUserRole => companyAssignedUserRole.UserRole!.Offer!.AppInstances.Any(ai => ai.IamClient!.ClientClientId == clientRole.ClientId) &&
                                                  clientRole.UserRoleNames.Contains(companyAssignedUserRole.UserRole.UserRoleText) &&
                                                  companyAssignedUserRole.Identity!.CompanyId == companyId)
                .Where(x => x.Identity!.IdentityTypeId == IdentityTypeId.COMPANY_USER)
                .Select(companyAssignedUserRole => new CompanyUserNameData(
                    companyAssignedUserRole.Identity!.Id,
                    companyAssignedUserRole.Identity.CompanyUser!.Firstname,
                    companyAssignedUserRole.Identity.CompanyUser!.Lastname
                ))
                .AsAsyncEnumerable())
            {
                yield return companyUserData;
            }
        }
    }

    public IAsyncEnumerable<(Guid IdentityId, IEnumerable<(string ClientClientId, Guid UserRoleId, string UserRoleText)> InstanceRoleData)> GetUsersWithUserRolesForApplicationId(Guid applicationId, IEnumerable<string> iamClientIds) =>
        _dbContext.Identities
            .Where(identity => identity.Company!.CompanyApplications.Any(companyApplication => companyApplication.Id == applicationId))
            .Select(identity => new
            {
                Identity = identity,
                RoleData = identity.IdentityAssignedRoles.SelectMany(identityAssignedRole =>
                    identityAssignedRole.UserRole!.Offer!.AppInstances
                        .Where(instance => iamClientIds.Contains(instance.IamClient!.ClientClientId))
                        .Select(appInstance => new
                        {
                            appInstance.IamClient!.ClientClientId,
                            identityAssignedRole.UserRole
                        }))
            })
            .Where(x => x.RoleData.Any())
            .Select(x => new ValueTuple<Guid, IEnumerable<(string, Guid, string)>>(
                x.Identity.Id,
                x.RoleData.Select(roleData => new ValueTuple<string, Guid, string>(
                            roleData.ClientClientId,
                            roleData.UserRole.Id,
                            roleData.UserRole.UserRoleText))))
            .Take(2)
            .ToAsyncEnumerable();

    /// <inheritdoc />
    public IAsyncEnumerable<Guid> GetRolesForClient(string technicalUserProfileClient) =>
        _dbContext.AppInstances
            .AsNoTracking()
            .Where(instance => technicalUserProfileClient == instance.IamClient!.ClientClientId)
            .SelectMany(instance => instance.App!.UserRoles.Select(role => role.Id))
            .ToAsyncEnumerable();

    /// <inheritdoc />
    public Task<(bool IsValid, bool IsActive, IEnumerable<ActiveAppRoleDetails>? AppRoleDetails)> GetActiveOfferRolesAsync(Guid offerId, OfferTypeId offerTypeId, string? languageShortName, string defaultLanguageShortName) =>
        _dbContext.Offers
            .AsNoTracking()
            .Where(offer => offer!.Id == offerId && offer.OfferTypeId == offerTypeId)
            .Select(offer => new
            {
                Active = offer.OfferStatusId == OfferStatusId.ACTIVE,
                Roles = offer.UserRoles
            })
            .Select(x => new ValueTuple<bool, bool, IEnumerable<ActiveAppRoleDetails>?>(
                true,
                x.Active,
                x.Active
                    ? x.Roles.Select(role =>
                        new ActiveAppRoleDetails(
                            role.Id,
                            role.UserRoleText,
                            role.UserRoleDescriptions.Where(description =>
                                (languageShortName != null && description.LanguageShortName == languageShortName) ||
                                    description.LanguageShortName == defaultLanguageShortName)
                                .Select(description => new ActiveAppUserRoleDescription(
                                    description.LanguageShortName,
                                    description.Description))))
                    : null))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<(bool IsValid, bool IsProvider, IEnumerable<ActiveAppRoleDetails>? AppRoleDetails)> GetOfferProviderRolesAsync(Guid offerId, OfferTypeId offerTypeId, Guid companyId, string? languageShortName, string defaultLanguageShortName) =>
        _dbContext.Offers
            .AsNoTracking()
            .Where(offer => offer!.Id == offerId && offer.OfferTypeId == offerTypeId)
            .Select(offer => new
            {
                Provider = offer.ProviderCompanyId == companyId,
                Roles = offer.UserRoles
            })
            .Select(x => new ValueTuple<bool, bool, IEnumerable<ActiveAppRoleDetails>?>(
                true,
                x.Provider,
                x.Provider
                    ? x.Roles.Select(role =>
                        new ActiveAppRoleDetails(
                            role.Id,
                            role.UserRoleText,
                            role.UserRoleDescriptions.Where(description =>
                                (languageShortName != null && description.LanguageShortName == languageShortName) ||
                                    description.LanguageShortName == defaultLanguageShortName)
                                .Select(description => new ActiveAppUserRoleDescription(
                                    description.LanguageShortName,
                                    description.Description))))
                    : null))
            .SingleOrDefaultAsync();
}
