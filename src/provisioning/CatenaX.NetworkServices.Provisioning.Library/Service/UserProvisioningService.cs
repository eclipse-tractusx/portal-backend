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

using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.Provisioning.Library.Models;
using PasswordGenerator;

namespace CatenaX.NetworkServices.Provisioning.Library.Service;

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

    public async IAsyncEnumerable<(Guid CompanyUserId, string UserName, Exception? Error)> CreateOwnCompanyIdpUsersAsync(string clientId, IAsyncEnumerable<UserCreationInfoIdp> userCreationInfos, string iamUserId, Guid identityProviderId)
    {
        var userRepository = _portalRepositories.GetInstance<IUserRepository>();
        var userRolesRepository = _portalRepositories.GetInstance<IUserRolesRepository>();

        var (companyId, companyName, businessPartnerNumber, alias, isSharedIdp) = await (identityProviderId == Guid.Empty
            ? GetSharedIdpUserCreationCompanyIdpData(userRepository, iamUserId)
            : GetOwnIdpUserCreationCompanyIdpData(userRepository, identityProviderId, iamUserId)
        ).ConfigureAwait(false);

        if (companyName == null)
        {
            throw new ConflictException($"assertion failed: companyName of company {companyId} should never be null here");
        }

        var userRoleIds = await userRolesRepository
            .GetUserRolesWithIdAsync(clientId)
            .ToDictionaryAsync(
                roleWithId => roleWithId.Role,
                roleWithId => roleWithId.Id
            )
            .ConfigureAwait(false);

        var creatorId = await userRepository.GetCompanyUserIdForIamUserUntrackedAsync(iamUserId).ConfigureAwait(false);

        var password = isSharedIdp ? new Password() : null;

        await foreach(var user in userCreationInfos)
        {
            var companyUser = userRepository.CreateCompanyUser(user.FirstName, user.LastName, user.Email, companyId, CompanyUserStatusId.ACTIVE, creatorId);

            Exception? error = null;

            try
            {
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

                if (user.Roles.Any())
                {
                    var invalidRoles = user.Roles.Except(userRoleIds.Keys);
                    if (invalidRoles.Any())
                    {
                        throw new ControllerArgumentException($"invalid Roles: [{string.Join(", ",invalidRoles)}]");
                    }
                    var clientRoleNames = new Dictionary<string, IEnumerable<string>>
                    {
                        { clientId, user.Roles }
                    };
                    var (_, assignedRoles) = (await _provisioningManager.AssignClientRolesToCentralUserAsync(centralUserId, clientRoleNames).ConfigureAwait(false)).Single();
                    foreach (var role in assignedRoles)
                    {
                        userRolesRepository.CreateCompanyUserAssignedRole(companyUser.Id, userRoleIds[role]);
                    }
                    if (assignedRoles.Count() < user.Roles.Count())
                    {
                        throw new ConflictException($"invalid role data, client: {clientId}, [{String.Join(", ",user.Roles.Except(assignedRoles))}] has not been assigned in keycloak");
                    }
                }
            }
            catch (Exception e)
            {
                error = e;
            }

            await _portalRepositories.SaveAsync().ConfigureAwait(false);

            yield return new (companyUser.Id, user.UserName, error);
        }
    }

    private async Task<(Guid CompanyId, string? CompanyName, string? BusinessPartnerNumber, string Alias, bool IsSharedKeycloak)> GetSharedIdpUserCreationCompanyIdpData(IUserRepository userRepository, string iamUserId)
    {
        var result = await userRepository.GetCompanyNameIdpAliaseUntrackedAsync(iamUserId, IdentityProviderCategoryId.KEYCLOAK_SHARED).ConfigureAwait(false);
        if (result != default)
        {
            var idpAlias = result.IdpAliase.SingleOrDefault();
            if (idpAlias == null)
            {
                throw new ArgumentOutOfRangeException($"user {iamUserId} is not associated with any shared idp");
            }
            return new ValueTuple<Guid,string?,string?,string,bool>(result.CompanyId, result.CompanyName, result.BusinessPartnerNumber, idpAlias, true);
        }
        throw new ControllerArgumentException($"user {iamUserId} is not associated with any company");
    }

    private async Task<(Guid CompanyId, string? CompanyName, string? BusinessPartnerNumber, string Alias, bool IsSharedKeycloak)> GetOwnIdpUserCreationCompanyIdpData(IUserRepository userRepository, Guid identityProviderId, string iamUserId)
    {
        var result = await userRepository.GetCompanyNameIdpAliasUntreackedAsync(iamUserId, identityProviderId).ConfigureAwait(false);
        if (result != default)
        {
            if (result.IdpAlias == null)
            {
                throw new ArgumentOutOfRangeException($"user {iamUserId} is not associated with own idp {identityProviderId}");
            }
            return new ValueTuple<Guid,string?,string?,string,bool>(result.CompanyId, result.CompanyName, result.BusinessPartnerNumber, result.IdpAlias, false);
        }
        throw new ControllerArgumentException($"user {iamUserId} is not associated with any company");
    }
}
