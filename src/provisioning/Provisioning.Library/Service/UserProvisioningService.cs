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

using Org.Eclipse.TractusX.Portal.Backend.Framework.Async;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using PasswordGenerator;
using System.Runtime.CompilerServices;
using System.Text;

namespace Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;

public class UserProvisioningService : IUserProvisioningService
{
    private static readonly IEnumerable<UserStatusId> ValidCompanyUserStatusIds = new[] { UserStatusId.ACTIVE, UserStatusId.INACTIVE, UserStatusId.PENDING };
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

        var (companyId, companyName, businessPartnerNumber, alias, identityProviderId, isSharedIdp) = companyNameIdpAliasData;

        var passwordProvider = new OptionalPasswordProvider(isSharedIdp);

        await foreach (var user in userCreationInfos)
        {
            (string UserEntityId, Guid CompanyUserId) userdata = default;
            Exception? error = null;

            var nextPassword = passwordProvider.NextOptionalPassword();
            try
            {
                var (identity, companyUserId) = await GetOrCreateCompanyUser(userRepository, alias, user, companyId, identityProviderId, businessPartnerNumber);

                userdata.CompanyUserId = companyUserId;
                if (!string.IsNullOrWhiteSpace(identity?.UserEntityId))
                {
                    userdata.UserEntityId = identity.UserEntityId;
                }

                cancellationToken.ThrowIfCancellationRequested();

                var providerUserId = await CreateSharedIdpUserOrReturnUserId(user, alias, nextPassword, isSharedIdp).ConfigureAwait(false);
                await HandleCentralKeycloakCreation(user, companyUserId, companyName, businessPartnerNumber, identity, Enumerable.Repeat(new IdentityProviderLink(alias, providerUserId, user.UserName), 1), userRepository, userRolesRepository).ConfigureAwait(false);
            }
            catch (Exception e) when (e is not OperationCanceledException)
            {
                error = e;
            }

            if (userdata == default && error == null)
            {
                error = new UnexpectedConditionException($"failed to create companyUser for provider userid {user.UserId}, username {user.UserName} while not throwing any error");
            }

            await _portalRepositories.SaveAsync().ConfigureAwait(false);

            yield return new(userdata.CompanyUserId, user.UserName, nextPassword, error);
        }
    }

    public async Task HandleCentralKeycloakCreation(UserCreationRoleDataIdpInfo user, Guid companyUserId, string companyName, string? businessPartnerNumber, Identity? identity, IEnumerable<IdentityProviderLink> identityProviderLinks, IUserRepository userRepository, IUserRolesRepository userRolesRepository)
    {
        var centralUserId = await CreateCentralUserWithProviderLinks(companyUserId, user, companyName, businessPartnerNumber, identityProviderLinks).ConfigureAwait(false);
        (string UserEntityId, Guid CompanyUserId) userdata = new(centralUserId, companyUserId);
        if (identity == null)
        {
            userRepository.AttachAndModifyIdentity(companyUserId, null, cu =>
            {
                cu.UserEntityId = centralUserId;
                cu.UserStatusId = user.UserStatusId;
            });
        }
        else
        {
            identity.UserEntityId = centralUserId;
            identity.UserStatusId = user.UserStatusId;
        }

        await AssignRolesToNewUserAsync(userRolesRepository, user.RoleDatas, userdata).ConfigureAwait(false);
    }

    private async Task<string> CreateCentralUserWithProviderLinks(Guid companyUserId, UserCreationRoleDataIdpInfo user, string companyName, string? businessPartnerNumber, IEnumerable<IdentityProviderLink> identityProviderLinks)
    {
        var centralUserId = await _provisioningManager.CreateCentralUserAsync(
            new UserProfile(
                companyUserId.ToString(),
                user.FirstName,
                user.LastName,
                user.Email,
                user.Enabled
            ),
            _provisioningManager.GetStandardAttributes(
                organisationName: companyName,
                businessPartnerNumber: businessPartnerNumber
            )
        ).ConfigureAwait(false);

        foreach (var identityProviderLink in identityProviderLinks)
        {
            await _provisioningManager.AddProviderUserLinkToCentralUserAsync(centralUserId,
                    new IdentityProviderLink(identityProviderLink.Alias, identityProviderLink.UserId, identityProviderLink.UserName)).ConfigureAwait(false);
        }

        return centralUserId;
    }

    public async Task<(Identity? identity, Guid companyUserId)> GetOrCreateCompanyUser(
        IUserRepository userRepository,
        string alias,
        UserCreationRoleDataIdpInfo user,
        Guid companyId,
        Guid identityProviderId,
        string? businessPartnerNumber)
    {
        var businessPartnerRepository = _portalRepositories.GetInstance<IUserBusinessPartnerRepository>();

        Identity? identity = null;
        var companyUserId = await ValidateDuplicateIdpUsersAsync(userRepository, alias, user, companyId).ConfigureAwait(false);
        if (companyUserId != Guid.Empty)
        {
            return (identity, companyUserId);
        }

        identity = userRepository.CreateIdentity(companyId, user.UserStatusId, IdentityTypeId.COMPANY_USER);
        companyUserId = userRepository.CreateCompanyUser(identity.Id, user.FirstName, user.LastName, user.Email).Id;
        if (businessPartnerNumber != null)
        {
            businessPartnerRepository.CreateCompanyUserAssignedBusinessPartner(companyUserId, businessPartnerNumber);
        }

        userRepository.AddCompanyUserAssignedIdentityProvider(companyUserId, identityProviderId, user.UserId, user.UserName);

        return (identity, companyUserId);
    }

    private sealed class OptionalPasswordProvider
    {
        private readonly Password? password;

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
                    user.Enabled,
                    password))
            : Task.FromResult(user.UserId);

    public async Task<(CompanyNameIdpAliasData IdpAliasData, string NameCreatedBy)> GetCompanyNameIdpAliasData(Guid identityProviderId, Guid companyUserId)
    {
        var result = await _portalRepositories.GetInstance<IIdentityProviderRepository>().GetCompanyNameIdpAliasUntrackedAsync(identityProviderId, companyUserId).ConfigureAwait(false);
        if (result == default)
        {
            throw new ControllerArgumentException($"user {companyUserId} does not exist");
        }

        var (company, companyUser, identityProvider) = result;
        if (identityProvider.IdpAlias == null)
        {
            throw new ControllerArgumentException($"user {companyUserId} is not associated with own idp {identityProviderId}");
        }

        if (company.CompanyName == null)
        {
            throw new ConflictException($"assertion failed: companyName of company {company.CompanyId} should never be null here");
        }

        var createdByName = CreateNameString(companyUser.FirstName, companyUser.LastName, companyUser.Email);

        return (new CompanyNameIdpAliasData(company.CompanyId, company.CompanyName, company.BusinessPartnerNumber, identityProvider.IdpAlias, identityProviderId, identityProvider.IsSharedIdp), createdByName);
    }

    public async Task<(CompanyNameIdpAliasData IdpAliasData, string NameCreatedBy)> GetCompanyNameSharedIdpAliasData(Guid companyUserId, Guid? applicationId = null)
    {
        var result = await _portalRepositories.GetInstance<IIdentityProviderRepository>().GetCompanyNameIdpAliaseUntrackedAsync(companyUserId, applicationId, IdentityProviderCategoryId.KEYCLOAK_OIDC, IdentityProviderTypeId.SHARED).ConfigureAwait(false);
        if (result == default)
        {
            throw applicationId == null
                ? new ControllerArgumentException($"user {companyUserId} does not exist")
                : new ControllerArgumentException($"user {companyUserId} is not associated with application {applicationId}");
        }

        var (company, companyUser, idpAliase) = result;
        if (company.CompanyName == null)
        {
            throw new ConflictException($"assertion failed: companyName of company {company.CompanyId} should never be null here");
        }

        if (!idpAliase.Any())
        {
            throw new ConflictException($"user {companyUserId} is not associated with any shared idp");
        }

        if (idpAliase.Count() > 1)
        {
            throw new ConflictException($"user {companyUserId} is associated with more than one shared idp");
        }

        var createdByName = CreateNameString(companyUser.FirstName, companyUser.LastName, companyUser.Email);

        var idpAlias = idpAliase.First();
        return (new CompanyNameIdpAliasData(company.CompanyId, company.CompanyName, company.BusinessPartnerNumber, idpAlias.Alias, idpAlias.IdentityProviderId, true), createdByName);
    }

    private static string CreateNameString(string? firstName, string? lastName, string? email)
    {
        var sb = new StringBuilder();
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

        return firstName == null && lastName == null && email == null ? "Dear User" : sb.ToString();
    }

    public Task<string> GetIdentityProviderDisplayName(string idpAlias) =>
        _provisioningManager.GetCentralIdentityProviderDisplayName(idpAlias);

    private async Task<Guid> ValidateDuplicateIdpUsersAsync(IUserRepository userRepository, string alias, UserCreationRoleDataIdpInfo user, Guid companyId)
    {
        var existingCompanyUserId = Guid.Empty;

        await foreach (var (userEntityId, companyUserId) in userRepository.GetMatchingCompanyIamUsersByNameEmail(user.FirstName, user.LastName, user.Email, companyId, ValidCompanyUserStatusIds).ConfigureAwait(false))
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
            catch (KeycloakEntityNotFoundException)
            {
                // when searching for duplicates this is not a validation-error
            }
        }

        return existingCompanyUserId;
    }

    public async Task AssignRolesToNewUserAsync(IUserRolesRepository userRolesRepository, IEnumerable<UserRoleData> roleDatas, (string UserEntityId, Guid CompanyUserId) userdata)
    {
        if (roleDatas.Any())
        {
            var clientRoleNames = roleDatas.GroupBy(roleInfo => roleInfo.ClientClientId).ToDictionary(group => group.Key, group => group.Select(roleInfo => roleInfo.UserRoleText));

            var messages = new List<string>();

            await foreach (var assigned in _provisioningManager.AssignClientRolesToCentralUserAsync(userdata.UserEntityId, clientRoleNames))
            {
                foreach (var role in assigned.Roles)
                {
                    var roleId = roleDatas.First(roleInfo => roleInfo.ClientClientId == assigned.Client && roleInfo.UserRoleText == role).UserRoleId;
                    userRolesRepository.CreateIdentityAssignedRole(userdata.CompanyUserId, roleId);
                }

                messages.AddRange(clientRoleNames[assigned.Client].Except(assigned.Roles).Select(roleName => $"clientId: {assigned.Client}, role: {roleName}"));
            }

            if (messages.Any())
            {
                throw new ConflictException($"invalid role data [{string.Join(", ", messages)}] has not been assigned in keycloak");
            }
        }
    }

    public async IAsyncEnumerable<UserRoleData> GetRoleDatas(IEnumerable<UserRoleConfig> clientRoles)
    {
        await foreach (var roleDataGrouping in _portalRepositories.GetInstance<IUserRolesRepository>()
                                .GetUserRoleDataUntrackedAsync(clientRoles)
                                .PreSortedGroupBy(d => d.ClientClientId))
        {
            ValidateRoleData(roleDataGrouping, roleDataGrouping.Key, clientRoles.Single(x => x.ClientId == roleDataGrouping.Key).UserRoleNames);
            foreach (var data in roleDataGrouping)
            {
                yield return data;
            }
        }
    }

    public async Task<IEnumerable<UserRoleData>> GetOwnCompanyPortalRoleDatas(string clientId, IEnumerable<string> roles, Guid companyId)
    {
        var roleDatas = await _portalRepositories.GetInstance<IUserRolesRepository>()
            .GetOwnCompanyPortalUserRoleDataUntrackedAsync(clientId, roles, companyId).ToListAsync().ConfigureAwait(false);
        ValidateRoleData(roleDatas, clientId, roles);
        return roleDatas;
    }

    private static void ValidateRoleData(IEnumerable<UserRoleData> roleData, string clientId, IEnumerable<string> roles)
    {
        var invalid = roles.Except(roleData.Select(r => r.UserRoleText));

        if (invalid.Any())
        {
            throw new ControllerArgumentException($"invalid roles: clientId: '{clientId}', roles: [{String.Join(", ", invalid)}]");
        }
    }
}
