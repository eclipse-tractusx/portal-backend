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

using Org.CatenaX.Ng.Portal.Backend.Framework.Async;
using Org.CatenaX.Ng.Portal.Backend.Framework.ErrorHandling;
using Org.CatenaX.Ng.Portal.Backend.Keycloak.ErrorHandling;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.CatenaX.Ng.Portal.Backend.Provisioning.Library.Models;
using PasswordGenerator;
using System.Runtime.CompilerServices;
using System.Text;

namespace Org.CatenaX.Ng.Portal.Backend.Provisioning.Library.Service;

public class UserProvisioningService : IUserProvisioningService
{
    private readonly IProvisioningManager _provisioningManager;
    private readonly IPortalRepositories _portalRepositories;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="provisioningManager">Provisioning Manager</param>
    /// <param name="portalRepositories">Portal Repositories</param>
    public UserProvisioningService(IProvisioningManager provisioningManager, IPortalRepositories portalRepositories)
    {
        _provisioningManager = provisioningManager;
        _portalRepositories = portalRepositories;
    }

    public async IAsyncEnumerable<(Guid CompanyUserId, string UserName, string? Password, Exception? Error)> CreateOwnCompanyIdpUsersAsync(
        CompanyNameIdpAliasData companyNameIdpAliasData,
        IAsyncEnumerable<UserCreationRoleDataIdpInfo> userCreationInfos,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var userRepository = _portalRepositories.GetInstance<IUserRepository>();
        var userRolesRepository = _portalRepositories.GetInstance<IUserRolesRepository>();

        var (companyId, companyName, businessPartnerNumber, creatorId, alias, isSharedIdp) = companyNameIdpAliasData;

        var passwordProvider = new OptionalPasswordProvider(isSharedIdp);

        await foreach(var user in userCreationInfos)
        {
            IamUser? iamUser = null;
            Exception? error = null;

            var nextPassword = passwordProvider.NextOptionalPassword();

            try
            {
                var existingCompanyUserId = await ValidateDuplicateIdpUsersAsync(userRepository, alias, user, companyId).ConfigureAwait(false);

                cancellationToken.ThrowIfCancellationRequested();

                var providerUserId = await CreateSharedIdpUserOrReturnUserId(user, alias, nextPassword, isSharedIdp).ConfigureAwait(false);

                var centralUserId = await _provisioningManager.CreateCentralUserAsync(
                    new UserProfile(
                        alias + "." + providerUserId,
                        user.FirstName,
                        user.LastName,
                        user.Email
                    ),
                    _provisioningManager.GetStandardAttributes(
                        alias: alias,
                        organisationName: companyName,
                        businessPartnerNumber: businessPartnerNumber
                    )
                ).ConfigureAwait(false);

                await _provisioningManager.AddProviderUserLinkToCentralUserAsync(centralUserId, new IdentityProviderLink(alias, providerUserId, user.UserName)).ConfigureAwait(false);

                iamUser = CreateOptionalCompanyUserAndIamUser(userRepository, user, centralUserId, companyId, creatorId, existingCompanyUserId);

                await AssignRolesToNewUserAsync(userRolesRepository, user.RoleDatas, iamUser).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                if (e is OperationCanceledException)
                {
                    throw;
                }
                error = e;
            }
            if(iamUser == null && error == null)
            {
                error = new UnexpectedConditionException($"failed to create companyUser for provider userid {user.UserId}, username {user.UserName} while not throwing any error");
            }
            
            await _portalRepositories.SaveAsync().ConfigureAwait(false);

            yield return new (iamUser?.CompanyUserId ?? Guid.Empty, user.UserName, nextPassword, error);
        }
    }

    private sealed class OptionalPasswordProvider
    {
        readonly Password? password;
        
        public OptionalPasswordProvider(bool createOptionalPasswords)
        {
            password = createOptionalPasswords ? new Password() : null;
        }

        public string? NextOptionalPassword() => password?.Next();
    }

    private Task<string> CreateSharedIdpUserOrReturnUserId(UserCreationRoleDataIdpInfo user, string alias, string? password, bool isSharedIdp) =>
        isSharedIdp
            ? _provisioningManager.CreateSharedRealmUserAsync(
                alias,
                new UserProfile(
                    user.UserName,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    password))
            : Task.FromResult(user.UserId);

    private static IamUser CreateOptionalCompanyUserAndIamUser(IUserRepository userRepository, UserCreationRoleDataIdpInfo user, string centralUserId, Guid companyId, Guid creatorId, Guid existingCompanyUserId)
    {
        var companyUserId = existingCompanyUserId == Guid.Empty
            ? userRepository.CreateCompanyUser(user.FirstName, user.LastName, user.Email, companyId, CompanyUserStatusId.ACTIVE, creatorId).Id
            : existingCompanyUserId;

        return userRepository.CreateIamUser(companyUserId, centralUserId);
    }

    public async Task<(CompanyNameIdpAliasData IdpAliasData, string NameCreatedBy)> GetCompanyNameIdpAliasData(Guid identityProviderId, string iamUserId)
    {
        var result = await _portalRepositories.GetInstance<IIdentityProviderRepository>().GetCompanyNameIdpAliasUntrackedAsync(identityProviderId, iamUserId).ConfigureAwait(false);
        if (result == default)
        {
            throw new ControllerArgumentException($"user {iamUserId} is not associated with any company");
        }
        var (company, companyUser, identityProvider) = result;
        if (identityProvider.IdpAlias == null)
        {
            throw new ControllerArgumentException($"user {iamUserId} is not associated with own idp {identityProviderId}");
        }

        if (company.CompanyName == null)
        {
            throw new ConflictException($"assertion failed: companyName of company {company.CompanyId} should never be null here");
        }

        var createdByName = CreateNameString(companyUser.FirstName, companyUser.LastName, companyUser.Email, companyUser.CompanyUserId);

        return (new CompanyNameIdpAliasData(company.CompanyId, company.CompanyName, company.BusinessPartnerNumber, companyUser.CompanyUserId, identityProvider.IdpAlias, identityProvider.IsSharedIdp), createdByName);
    }

    public async Task<(CompanyNameIdpAliasData IdpAliasData, string NameCreatedBy)> GetCompanyNameSharedIdpAliasData(string iamUserId)
    {
        var result = await _portalRepositories.GetInstance<IIdentityProviderRepository>().GetCompanyNameIdpAliaseUntrackedAsync(iamUserId, IdentityProviderCategoryId.KEYCLOAK_SHARED).ConfigureAwait(false);
        if (result == default)
        {
            throw new ControllerArgumentException($"user {iamUserId} is not associated with any company");
        }
        var (company, companyUser, idpAliase) = result;
        if (company.CompanyName == null)
        {
            throw new ConflictException($"assertion failed: companyName of company {company.CompanyId} should never be null here");
        }
        if (!idpAliase.Any())
        {
            throw new ConflictException($"user {iamUserId} is not associated with any shared idp");
        }
        if (idpAliase.Count() > 1)
        {
            throw new ConflictException($"user {iamUserId} is associated with more than one shared idp");
        }

        var createdByName = CreateNameString(companyUser.FirstName, companyUser.LastName, companyUser.Email, companyUser.CompanyUserId);

        return (new CompanyNameIdpAliasData(company.CompanyId, company.CompanyName, company.BusinessPartnerNumber, companyUser.CompanyUserId, idpAliase.First(), true), createdByName);
    }

    private static string CreateNameString(string? firstName, string? lastName, string? email, Guid companyUserId)
    {
        StringBuilder sb = new StringBuilder();
        if (firstName != null)
        {
            sb.Append(firstName);
        }
        if (lastName != null)
        {
            sb.AppendFormat((firstName == null ? "{0}" : ", {0}"), lastName);
        }
        if (email != null)
        {
            sb.AppendFormat((firstName == null && lastName == null) ? "{0}" : " ({0})", email);
        }
        return firstName == null && lastName == null && email == null ? companyUserId.ToString() : sb.ToString();
    }

    public Task<string> GetIdentityProviderDisplayName(string idpAlias) =>
        _provisioningManager.GetCentralIdentityProviderDisplayName(idpAlias);

    private async Task<Guid> ValidateDuplicateIdpUsersAsync(IUserRepository userRepository, string alias, UserCreationRoleDataIdpInfo user, Guid companyId)
    {
        Guid existingCompanyUserId = Guid.Empty;

        var validCompanyUserStatusIds = new [] { CompanyUserStatusId.ACTIVE, CompanyUserStatusId.INACTIVE };

        await foreach (var (userEntityId, companyUserId) in userRepository.GetMatchingCompanyIamUsersByNameEmail(user.FirstName, user.LastName, user.Email, companyId, validCompanyUserStatusIds ).ConfigureAwait(false))
        {
            if (userEntityId == null)
            {
                if (companyUserId != Guid.Empty)
                {
                    existingCompanyUserId = companyUserId;
                }
                continue;
            }
            try
            {
                if (await _provisioningManager.GetProviderUserLinkDataForCentralUserIdAsync(userEntityId).AnyAsync(link =>
                    alias == link.Alias && (user.UserId == link.UserId || user.UserName == link.UserName)).ConfigureAwait(false))
                {
                    throw new ConflictException($"existing user {userEntityId} in keycloak for provider userid {user.UserId}, {user.UserName}");
                }
            }
            catch(KeycloakEntityNotFoundException)
            {
                // when searching for duplicates this is not a validation-error
            }
        }
        return existingCompanyUserId;
    }

    private async Task AssignRolesToNewUserAsync(IUserRolesRepository userRolesRepository, IEnumerable<UserRoleData> roleDatas, IamUser iamUser)
    {
        if (roleDatas.Any())
        {
            var clientRoleNames = roleDatas.GroupBy(roleInfo => roleInfo.ClientClientId).ToDictionary(group => group.Key, group => group.Select(roleInfo => roleInfo.UserRoleText));

            var messages = new List<string>();

            await foreach(var assigned in _provisioningManager.AssignClientRolesToCentralUserAsync(iamUser.UserEntityId, clientRoleNames))
            {
                foreach(var role in assigned.Roles)
                {
                    var roleId = roleDatas.First(roleInfo => roleInfo.ClientClientId == assigned.Client && roleInfo.UserRoleText == role).UserRoleId;
                    userRolesRepository.CreateCompanyUserAssignedRole(iamUser.CompanyUserId, roleId);
                }
                messages.AddRange(clientRoleNames[assigned.Client].Except(assigned.Roles).Select(roleName => $"clientId: {assigned.Client}, role: {roleName}"));
            }

            if (messages.Any())
            {
                throw new ConflictException($"invalid role data [{String.Join(", ", messages)}] has not been assigned in keycloak");
            }
        }
    }

    public async IAsyncEnumerable<UserRoleData> GetRoleDatas(IDictionary<string,IEnumerable<string>> clientRoles)
    {
        await foreach (var roleDataGrouping in _portalRepositories.GetInstance<IUserRolesRepository>()
            .GetUserRoleDataUntrackedAsync(clientRoles)
            .PreSortedGroupBy(d => d.ClientClientId))
        {
            ValidateRoleData(roleDataGrouping, roleDataGrouping.Key, clientRoles[roleDataGrouping.Key]);
            foreach (var data in roleDataGrouping)
            {
                yield return data;
            }
        }
    }

    public async Task<IEnumerable<UserRoleData>> GetOwnCompanyPortalRoleDatas(string clientId, IEnumerable<string> roles, string iamUserId)
    {
        var roleDatas = await _portalRepositories.GetInstance<IUserRolesRepository>()
            .GetOwnCompanyPortalUserRoleDataUntrackedAsync(clientId, roles, iamUserId).ToListAsync().ConfigureAwait(false);
        ValidateRoleData(roleDatas, clientId, roles);
        return roleDatas;
    }

    private static void ValidateRoleData(IEnumerable<UserRoleData> roleData, string clientId, IEnumerable<string> roles)
    {
        var invalid = roles.Except(roleData.Select(r => r.UserRoleText));

        if (invalid.Any())
        {
            throw new ControllerArgumentException($"invalid roles: clientId: '{clientId}', roles: [{String.Join(", ",invalid)}]");
        }
    }
}
