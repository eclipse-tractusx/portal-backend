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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using PasswordGenerator;

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

    public async IAsyncEnumerable<(Guid CompanyUserId, string UserName, Exception? Error)> CreateOwnCompanyIdpUsersAsync(CompanyNameIdpAliasData companyNameIdpAliasData, string clientId, IAsyncEnumerable<UserCreationInfoIdp> userCreationInfos)
    {
        var userRepository = _portalRepositories.GetInstance<IUserRepository>();
        var userRolesRepository = _portalRepositories.GetInstance<IUserRolesRepository>();

        var (companyId, companyName, businessPartnerNumber, creatorId, alias, isSharedIdp) = companyNameIdpAliasData;

        var userRoleIds = await userRolesRepository
            .GetUserRolesWithIdAsync(clientId)
            .ToDictionaryAsync(
                roleWithId => roleWithId.Role,
                roleWithId => roleWithId.Id
            )
            .ConfigureAwait(false);

        var password = isSharedIdp ? new Password() : null;

        await foreach(var user in userCreationInfos)
        {
            CompanyUser? companyUser = null;
            Exception? error = null;

            try
            {
                await ValidateDuplicateUsersAsync(userRepository, alias, user, companyId).ConfigureAwait(false);

                companyUser = userRepository.CreateCompanyUser(user.FirstName, user.LastName, user.Email, companyId, CompanyUserStatusId.ACTIVE, creatorId);

                var providerUserId = isSharedIdp
                    ? await _provisioningManager.CreateSharedRealmUserAsync(
                        alias,
                        new UserProfile(
                            user.UserName,
                            user.FirstName,
                            user.LastName,
                            user.Email,
                            password!.Next())).ConfigureAwait(false)
                    : user.UserId;

                var centralUserId = await _provisioningManager.CreateCentralUserAsync(
                    new UserProfile(
                        alias + "." + companyUser.Id,
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

                userRepository.CreateIamUser(companyUser, centralUserId);

                await _provisioningManager.AddProviderUserLinkToCentralUserAsync(centralUserId, new IdentityProviderLink(alias, providerUserId, user.UserName)).ConfigureAwait(false);

                await AssignRolesToNewUserAsync(userRolesRepository, user.Roles, companyUser.Id, centralUserId, clientId, userRoleIds).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                error = e;
            }
            if(companyUser == null && error == null)
            {
                error = new UnexpectedConditionException($"failed to create companyUser for provider userid {user.UserId}, username {user.UserName} while not throwing any error");
            }
            
            await _portalRepositories.SaveAsync().ConfigureAwait(false);

            yield return new (companyUser?.Id ?? Guid.Empty, user.UserName, error);
        }
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

    private async Task ValidateDuplicateUsersAsync(IUserRepository userRepository, string alias, UserCreationInfoIdp user, Guid companyId)
    {
        await foreach (var userEntityId in userRepository.GetMatchingCompanyIamUsersByNameEmail(user.FirstName, user.LastName, user.Email, companyId).ConfigureAwait(false))
        {
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
    }

    private async Task AssignRolesToNewUserAsync(IUserRolesRepository userRolesRepository, IEnumerable<string> roles, Guid companyUserId, string centralUserId, string clientId, IDictionary<string,Guid> userRoleIds)
    {
        if (roles.Any())
        {
            var invalidRoles = roles.Except(userRoleIds.Keys);
            if (invalidRoles.Any())
            {
                throw new ControllerArgumentException($"invalid Roles: [{string.Join(", ",invalidRoles)}]");
            }
            var clientRoleNames = new Dictionary<string, IEnumerable<string>>
            {
                { clientId, roles }
            };
            var (_, assignedRoles) = (await _provisioningManager.AssignClientRolesToCentralUserAsync(centralUserId, clientRoleNames).ConfigureAwait(false)).Single();
            foreach (var role in assignedRoles)
            {
                userRolesRepository.CreateCompanyUserAssignedRole(companyUserId, userRoleIds[role]);
            }
            if (assignedRoles.Count() < roles.Count())
            {
                throw new ConflictException($"invalid role data, client: {clientId}, [{String.Join(", ", roles.Except(assignedRoles))}] has not been assigned in keycloak");
            }
        }
    }
}
