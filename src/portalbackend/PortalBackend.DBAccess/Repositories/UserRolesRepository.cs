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

using Microsoft.EntityFrameworkCore;
using Org.CatenaX.Ng.Portal.Backend.Framework.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Repositories;

public class UserRolesRepository : IUserRolesRepository
{
    private readonly PortalDbContext _dbContext;

    public UserRolesRepository(PortalDbContext portalDbContext)
    {
        _dbContext = portalDbContext;
    }

    ///<inheritdoc/>
    public UserRole CreateAppUserRole(Guid appId, string role) =>
        _dbContext.UserRoles.Add(
            new UserRole(
                Guid.NewGuid(),
                role,
                appId
            ))
            .Entity;
    
    ///<inheritdoc/>
    public UserRole DeleteUserRole(Guid roleId) =>
        _dbContext.Remove(new UserRole(roleId,null!,Guid.Empty)).Entity;

    ///<inheritdoc/>
    public UserRoleDescription CreateAppUserRoleDescription(Guid roleId, string languageCode, string description) =>
        _dbContext.UserRoleDescriptions.Add(
            new UserRoleDescription(
                roleId,
                languageCode,
                description
            ))
            .Entity;

    public CompanyUserAssignedRole CreateCompanyUserAssignedRole(Guid companyUserId, Guid userRoleId) =>
        _dbContext.CompanyUserAssignedRoles.Add(
            new CompanyUserAssignedRole(
                companyUserId,
                userRoleId
            )).Entity;

    public IAsyncEnumerable<CompanyUser> GetCompanyUserRolesIamUsersAsync(IEnumerable<Guid> companyUserIds, Guid companyUserId) =>
        _dbContext.Companies
            .Where(company => company.CompanyUsers.Any(cu => cu.Id == companyUserId))
            .SelectMany(company => company.CompanyUsers.Where(companyUser => companyUserIds.Contains(companyUser.Id)))
            .Include(companyUser => companyUser.CompanyUserAssignedRoles)
            .Include(companyUser => companyUser.IamUser)
            .AsAsyncEnumerable();

    public CompanyUserAssignedRole RemoveCompanyUserAssignedRole(CompanyUserAssignedRole companyUserAssignedRole) =>
        _dbContext.Remove(companyUserAssignedRole).Entity;

    public IAsyncEnumerable<UserRoleData> GetUserRoleDataUntrackedAsync(IEnumerable<Guid> userRoleIds) =>
        _dbContext.UserRoles
            .AsNoTracking()
            .Where(userRole => userRoleIds.Contains(userRole.Id))
            .Select(userRole => new UserRoleData(
                userRole.Id,
                userRole.Offer!.AppInstances.First().IamClient!.ClientClientId,
                userRole.UserRoleText))
            .ToAsyncEnumerable();

    public async IAsyncEnumerable<Guid> GetUserRoleIdsUntrackedAsync(IDictionary<string, IEnumerable<string>> clientRoles)
    {
        foreach (var clientRole in clientRoles)
        {
            await foreach (var userRoleId in _dbContext.UserRoles
                .AsNoTracking()
                .Where(userRole => userRole.Offer!.AppInstances.Any(x => x.IamClient!.ClientClientId == clientRole.Key) && clientRole.Value.Contains(userRole.UserRoleText))
                .Select(userRole => userRole.Id)
                .AsAsyncEnumerable()
                .ConfigureAwait(false))
            {
                yield return userRoleId;
            }
        }
    }

    public IAsyncEnumerable<UserRoleModificationData> GetAssignedAndMatchingAppRoles(Guid companyUserId, IEnumerable<string> userRoles, Guid offerId) =>
        _dbContext.UserRoles
            .AsNoTracking()
            .Where(role =>
                role.OfferId == offerId &&
                (userRoles.Contains(role.UserRoleText) || 
                role.CompanyUsers.Any(user => user.Id == companyUserId)))
            .Select(userRole => new UserRoleModificationData(
                userRole.UserRoleText,
                userRole.Id,
                userRole.CompanyUsers.Any(user => user.Id == companyUserId)
            ))
            .ToAsyncEnumerable();

    public IAsyncEnumerable<UserRoleModificationData> GetAssignedAndMatchingCoreOfferRoles(Guid companyUserId, IEnumerable<string> userRoles, Guid offerId) =>
        _dbContext.UserRoles
            .AsNoTracking()
            .Where(role =>
                role.OfferId == offerId &&
                role.UserRoleCollections.Any(collection => collection.CompanyRoleAssignedRoleCollection!.CompanyRole.Companies.Any(company => company.CompanyUsers.Any(user => user.Id == companyUserId))) &&
                (userRoles.Contains(role.UserRoleText) || 
                role.CompanyUsers.Any(user => user.Id == companyUserId)))
            .Select(userRole => new UserRoleModificationData(
                userRole.UserRoleText,
                userRole.Id,
                userRole.CompanyUsers.Any(user => user.Id == companyUserId)
            ))
            .ToAsyncEnumerable();

    public async IAsyncEnumerable<UserRoleData> GetUserRoleDataUntrackedAsync(IDictionary<string, IEnumerable<string>> clientRoles)
    {
        foreach (var clientRole in clientRoles)
        {
            await foreach (var userRoleData in _dbContext.UserRoles
                .AsNoTracking()
                .Where(userRole => userRole.Offer!.AppInstances.Any(ai => ai.IamClient!.ClientClientId == clientRole.Key) && clientRole.Value.Contains(userRole.UserRoleText))
                .Select(userRole => new {
                    Id = userRole.Id,
                    Text = userRole.UserRoleText
                })
                .AsAsyncEnumerable())
            {
                yield return new UserRoleData(
                    userRoleData.Id,
                    clientRole.Key,
                    userRoleData.Text
                );
            }
        }
    }

    public IAsyncEnumerable<UserRoleData> GetOwnCompanyPortalUserRoleDataUntrackedAsync(string clientId, IEnumerable<string> roles, string iamUserId) =>
        _dbContext.UserRoles
            .AsNoTracking()
            .Where(userRole => userRole.Offer!.AppInstances.Any(ai => ai.IamClient!.ClientClientId == clientId) &&
                roles.Contains(userRole.UserRoleText) &&
                userRole.UserRoleCollections.Any(collection => collection.CompanyRoleAssignedRoleCollection!.CompanyRole.Companies.Any(company => company.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == iamUserId))))
            .Select(userRole => new UserRoleData(
                userRole.Id,
                clientId,
                userRole.UserRoleText))
            .AsAsyncEnumerable();

    public IAsyncEnumerable<(Guid OfferId, Guid RoleId, string RoleText, string Description)> GetCoreOfferRolesAsync(string iamUserId, string languageShortName) =>
        _dbContext.UserRoles
            .AsNoTracking()
            .Where(role => role.UserRoleCollections.Any(collection => collection.CompanyRoleAssignedRoleCollection!.CompanyRole.Companies.Any(company => company.CompanyUsers.Any(user => user.IamUser!.UserEntityId == iamUserId))))
            .OrderBy(role => role.OfferId)
            .Select(role => new ValueTuple<Guid,Guid,string,string>(
                role.OfferId,
                role.Id,
                role.UserRoleText,
                role.UserRoleDescriptions.SingleOrDefault(desc => desc.LanguageShortName == languageShortName)!.Description))
            .ToAsyncEnumerable();

    public IAsyncEnumerable<OfferRoleInfo> GetAppRolesAsync(Guid offerId, string iamUserid, string languageShortName) =>
        _dbContext.UserRoles
            .Where(role => role.OfferId == offerId &&
                role.Offer!.OfferSubscriptions.Any(subscription => subscription.Company!.CompanyUsers.Any(user => user.IamUser!.UserEntityId == iamUserid)))
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

    public IAsyncEnumerable<UserRoleWithDescription> GetServiceAccountRolesAsync(string clientId, string? languageShortName = null) =>
       _dbContext.UserRoles
           .AsNoTracking()
           .Where(userRole => userRole.Offer!.AppInstances.Any(x => x.IamClient!.ClientClientId == clientId))
           .Select(userRole => new UserRoleWithDescription(
                   userRole.Id,
                   userRole.UserRoleText,
                   userRole.UserRoleDescriptions.SingleOrDefault(desc =>
                   desc.LanguageShortName == (languageShortName ?? Constants.DefaultLanguage))!.Description))
           .AsAsyncEnumerable();

    /// <inheritdoc />
    public Task<List<string>> GetUserRolesForOfferIdAsync(Guid offerId) => 
        _dbContext.UserRoles
            .Where(x => x.OfferId == offerId)
            .Select(x => x.UserRoleText)
            .ToListAsync();
}
