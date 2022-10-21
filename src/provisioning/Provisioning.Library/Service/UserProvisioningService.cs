/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using PasswordGenerator;
using System.Runtime.CompilerServices;
namespace Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;

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
        string clientId,
        IAsyncEnumerable<UserCreationInfoIdp> userCreationInfos,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var userRepository = _portalRepositories.GetInstance<IUserRepository>();
        var userRolesRepository = _portalRepositories.GetInstance<IUserRolesRepository>();

        var (companyId, companyName, businessPartnerNumber, creatorId, alias, isSharedIdp) = companyNameIdpAliasData;

        var userRoleIds = await userRolesRepository
            .GetUserRolesWithIdAsync(clientId)
            .ToDictionaryAsync(
                roleWithId => roleWithId.Role,
                roleWithId => roleWithId.Id,
                cancellationToken
            )
            .ConfigureAwait(false);

        var passwordProvider = new OptionalPasswordProvider(isSharedIdp);

        await foreach(var user in userCreationInfos)
        {
            IamUser? iamUser = null;
            Exception? error = null;

            var nextPassword = passwordProvider.NextOptionalPassword();

            try
            {
                ValidateRoles(user.Roles, userRoleIds);

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

                await AssignRolesToNewUserAsync(userRolesRepository, user.Roles, iamUser, clientId, userRoleIds).ConfigureAwait(false);
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

    private Task<string> CreateSharedIdpUserOrReturnUserId(UserCreationInfoIdp user, string alias, string? password, bool isSharedIdp) =>
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

    private static IamUser CreateOptionalCompanyUserAndIamUser(IUserRepository userRepository, UserCreationInfoIdp user, string centralUserId, Guid companyId, Guid creatorId, Guid existingCompanyUserId)
    {
        var companyUserId = existingCompanyUserId == Guid.Empty
            ? userRepository.CreateCompanyUser(user.FirstName, user.LastName, user.Email, companyId, CompanyUserStatusId.ACTIVE, creatorId).Id
            : existingCompanyUserId;

        return userRepository.CreateIamUser(companyUserId, centralUserId);
    }

    public async Task<CompanyNameIdpAliasData> GetCompanyNameIdpAliasData(Guid identityProviderId, string iamUserId)
    {
        var result = await _portalRepositories.GetInstance<IIdentityProviderRepository>().GetCompanyNameIdpAliasUntrackedAsync(identityProviderId, iamUserId).ConfigureAwait(false);
        if (result == default)
        {
            throw new ControllerArgumentException($"user {iamUserId} is not associated with any company");
        }

        if (result.IdpAlias == null)
        {
            throw new ArgumentOutOfRangeException($"user {iamUserId} is not associated with own idp {identityProviderId}");
        }

        if (result.CompanyName == null)
        {
            throw new ConflictException($"assertion failed: companyName of company {result.CompanyId} should never be null here");
        }

        return new CompanyNameIdpAliasData(result.CompanyId, result.CompanyName, result.BusinessPartnerNumber, result.companyUserId, result.IdpAlias, result.IsSharedIdp);
    }

    public async Task<CompanyNameIdpAliasData> GetCompanyNameSharedIdpAliasData(string iamUserId)
    {
        var result = await _portalRepositories.GetInstance<IIdentityProviderRepository>().GetCompanyNameSharedIdpAliasUntrackedAsync(iamUserId).ConfigureAwait(false);
        if (result == default)
        {
            throw new ControllerArgumentException($"user {iamUserId} is not associated with any company");
        }

        if (result.IdpAlias == null)
        {
            throw new ArgumentOutOfRangeException($"user {iamUserId} is not associated with any shared idp");
        }

        if (result.CompanyName == null)
        {
            throw new ConflictException($"assertion failed: companyName of company {result.CompanyId} should never be null here");
        }

        return new CompanyNameIdpAliasData(result.CompanyId, result.CompanyName, result.BusinessPartnerNumber, result.companyUserId, result.IdpAlias, true);
    }

    private async Task<Guid> ValidateDuplicateIdpUsersAsync(IUserRepository userRepository, string alias, UserCreationInfoIdp user, Guid companyId)
    {
        Guid existingCompanyUserId = Guid.Empty;

        await foreach (var (userEntityId, companyUserId) in userRepository.GetMatchingCompanyIamUsersByNameEmail(user.FirstName, user.LastName, user.Email, companyId).ConfigureAwait(false))
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

    private static void ValidateRoles(IEnumerable<string> roles,IDictionary<string,Guid> userRoleIds)
    {
        if (roles.Any())
        {
            var invalidRoles = roles.Except(userRoleIds.Keys);
            if (invalidRoles.Any())
            {
                throw new ControllerArgumentException($"invalid Roles: [{string.Join(", ",invalidRoles)}]");
            }
        }
    }

    private async Task AssignRolesToNewUserAsync(IUserRolesRepository userRolesRepository, IEnumerable<string> roles, IamUser iamUser, string clientId, IDictionary<string,Guid> userRoleIds)
    {
        if (roles.Any())
        {
            var clientRoleNames = new Dictionary<string, IEnumerable<string>>
            {
                { clientId, roles }
            };
            var (_, assignedRoles) = (await _provisioningManager.AssignClientRolesToCentralUserAsync(iamUser.UserEntityId, clientRoleNames).ConfigureAwait(false)).Single();
            foreach (var role in assignedRoles)
            {
                userRolesRepository.CreateCompanyUserAssignedRole(iamUser.CompanyUserId, userRoleIds[role]);
            }
            if (assignedRoles.Count() < roles.Count())
            {
                throw new ConflictException($"invalid role data, client: {clientId}, [{String.Join(", ", roles.Except(assignedRoles))}] has not been assigned in keycloak");
            }
        }
    }
}
