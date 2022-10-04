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

using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Microsoft.EntityFrameworkCore;
using Org.CatenaX.Ng.Portal.Backend.Framework.Models;

namespace Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Repositories;

public class UserRolesRepository : IUserRolesRepository
{
    private readonly PortalDbContext _dbContext;

    public UserRolesRepository(PortalDbContext portalDbContext)
    {
        _dbContext = portalDbContext;
    }

    public CompanyUserAssignedRole CreateCompanyUserAssignedRole(Guid companyUserId, Guid userRoleId) =>
        _dbContext.CompanyUserAssignedRoles.Add(
            new CompanyUserAssignedRole(
                companyUserId,
                userRoleId
            )).Entity;

    public IAsyncEnumerable<CompanyUser> GetCompanyUserRolesIamUsersAsync(IEnumerable<Guid> companyUserIds, string iamUserId) =>
        _dbContext.CompanyUsers
            .Where(companyUser => companyUser.IamUser!.UserEntityId == iamUserId)
            .SelectMany(companyUser => companyUser.Company!.CompanyUsers)
            .Where(companyUser => companyUserIds.Contains(companyUser.Id) && companyUser.IamUser!.UserEntityId != null)
            .Include(companyUser => companyUser.CompanyUserAssignedRoles)
            .Include(companyUser => companyUser.IamUser)
            .AsAsyncEnumerable();

    public IAsyncEnumerable<(Guid CompanyUserId, Guid UserRoleId)> GetExistingRolesByNameForUserAsync(IEnumerable<string> roleNames, string iamUserId) =>
        _dbContext.CompanyUserAssignedRoles
            .Where(cuar => 
                roleNames.Any(x => x == cuar.UserRole!.UserRoleText) &&
                cuar.CompanyUser!.IamUser!.UserEntityId == iamUserId)
            .Select(cuar => new ValueTuple<Guid, Guid>(cuar.CompanyUserId, cuar.UserRoleId))
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

    public IAsyncEnumerable<CompanyUserRoleDeletionData> GetAssignedRolesForDeletion(Guid companyUserId, IEnumerable<string> userRoles) =>
        _dbContext.CompanyUserAssignedRoles
            .AsNoTracking()
            .Where(x => x.CompanyUserId == companyUserId && userRoles.All(ur => ur != x.UserRole!.UserRoleText))
            .Select(x => new CompanyUserRoleDeletionData(
                    x.CompanyUserId,
                    x.UserRoleId,
                    x.UserRole!.UserRoleText))
            .ToAsyncEnumerable();
    
    public IAsyncEnumerable<UserRoleWithId> GetRolesToAdd(string clientClientId, Guid companyUserId, IEnumerable<string> userRoles) =>
        _dbContext.UserRoles
            .AsNoTracking()
            .Where(userRole =>
                userRole.Offer!.AppInstances.Any(x => x.IamClient!.ClientClientId == clientClientId) &&
                userRoles.Contains(userRole.UserRoleText) &&
                !userRole.CompanyUsers.SingleOrDefault(x => x.Id == companyUserId)!.CompanyUserAssignedRoles.Any(x => userRoles.Contains(x.UserRole!.UserRoleText)))
            .Select(userRole => new UserRoleWithId(
                userRole.UserRoleText,
                userRole.Id
            ))
            .ToAsyncEnumerable();

    public IAsyncEnumerable<UserRoleWithId> GetUserRoleWithIdsUntrackedAsync(string clientClientId, IEnumerable<string> userRoles) =>
        _dbContext.UserRoles
            .AsNoTracking()
            .Where(userRole => userRole.Offer!.AppInstances.Any(x => x.IamClient!.ClientClientId == clientClientId) && userRoles.Contains(userRole.UserRoleText))
            .Select(userRole => new UserRoleWithId(
                userRole.UserRoleText,
                userRole.Id
            ))
            .AsAsyncEnumerable();

    public async IAsyncEnumerable<UserRoleData> GetUserRoleDataUntrackedAsync(IDictionary<string, IEnumerable<string>> clientRoles)
    {
        foreach (var clientRole in clientRoles)
        {
            await foreach (var userRoleData in _dbContext.UserRoles
                .AsNoTracking()
                .Where(userRole => userRole.Offer!.AppInstances.Any(ai => ai.IamClient!.ClientClientId == clientRole.Key) && clientRole.Value.Contains(userRole.UserRoleText))
                .Select(userRole => new UserRoleData(
                    userRole.Id,
                    userRole.Offer!.AppInstances.Single(ai => ai.IamClient!.ClientClientId == clientRole.Key).IamClient!.ClientClientId,
                    userRole.UserRoleText
                ))
                .AsAsyncEnumerable())
            {
                yield return userRoleData;
            }
        }
    }

    public IAsyncEnumerable<(string Role,Guid Id)> GetUserRolesWithIdAsync(string keyCloakClientId) =>
        _dbContext.UserRoles
            .AsNoTracking()
            .Where(userRole => userRole.Offer!.AppInstances.Any(x => x.IamClient!.ClientClientId == keyCloakClientId))
            .Select(userRole => new ValueTuple<string,Guid>(userRole.UserRoleText, userRole.Id))
            .AsAsyncEnumerable();

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
