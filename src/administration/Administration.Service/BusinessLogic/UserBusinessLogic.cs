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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Async;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Identity;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Linq;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.Mailing.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

/// <summary>
/// Implementation of <see cref="IUserBusinessLogic"/>.
/// </summary>
public class UserBusinessLogic(
    IProvisioningManager provisioningManager,
    IUserProvisioningService userProvisioningService,
    IProvisioningDBAccess provisioningDbAccess,
    IPortalRepositories portalRepositories,
    IIdentityService identityService,
    IMailingProcessCreation mailingProcessCreation,
    ILogger<UserBusinessLogic> logger,
    IBpnAccess bpnAccess,
    IOptions<UserSettings> options) : IUserBusinessLogic
{
    private readonly UserSettings _settings = options.Value;
    private readonly IIdentityData _identityData = identityService.IdentityData;

    public IAsyncEnumerable<string> CreateOwnCompanyUsersAsync(IEnumerable<UserCreationInfo> userList)
    {
        var noUserNameAndEmail = userList.Where(user => string.IsNullOrEmpty(user.userName) && string.IsNullOrEmpty(user.eMail));
        if (noUserNameAndEmail.Any())
        {
            throw new ControllerArgumentException($"userName and eMail must not both be empty '{string.Join(", ", noUserNameAndEmail.Select(user => string.Join(" ", new[] { user.firstName, user.lastName }.Where(x => x != null))))}'");
        }

        var noRoles = userList.Where(user => !user.Roles.Any());
        if (noRoles.Any())
        {
            throw new ControllerArgumentException($"at least one role must be specified for users '{string.Join(", ", noRoles.Select(user => user.userName ?? user.eMail))}'");
        }

        return CreateOwnCompanyUsersInternalAsync(userList);
    }

    private async IAsyncEnumerable<string> CreateOwnCompanyUsersInternalAsync(IEnumerable<UserCreationInfo> userList)
    {
        var (companyNameIdpAliasData, nameCreatedBy) = await userProvisioningService.GetCompanyNameSharedIdpAliasData(_identityData.IdentityId).ConfigureAwait(ConfigureAwaitOptions.None);

        var distinctRoles = userList.SelectMany(user => user.Roles).Distinct().ToList();

        var roleDatas = await GetOwnCompanyUserRoleData(distinctRoles).ConfigureAwait(ConfigureAwaitOptions.None);

        var userCreationInfoIdps = userList.Select(user =>
            new UserCreationRoleDataIdpInfo(
                user.firstName ?? "",
                user.lastName ?? "",
                user.eMail,
                roleDatas.IntersectBy(user.Roles, roleData => roleData.UserRoleText),
                user.userName ?? user.eMail,
                "",
                UserStatusId.ACTIVE,
                true
            )).ToAsyncEnumerable();

        var emailData = userList.ToDictionary(
            user => user.userName ?? user.eMail,
            user => user.eMail);

        var companyDisplayName = await userProvisioningService.GetIdentityProviderDisplayName(companyNameIdpAliasData.IdpAlias).ConfigureAwait(ConfigureAwaitOptions.None) ?? companyNameIdpAliasData.IdpAlias;

        await foreach (var (companyUserId, userName, password, error) in userProvisioningService.CreateOwnCompanyIdpUsersAsync(companyNameIdpAliasData, userCreationInfoIdps).ConfigureAwait(false))
        {
            var email = emailData[userName];

            if (error != null)
            {
                logger.LogError(error, "Error while creating user {companyUserId}", companyUserId);
                continue;
            }

            var mailParameters = ImmutableDictionary.CreateRange(new[]
            {
                KeyValuePair.Create("password", password ?? ""),
                KeyValuePair.Create("companyName", companyDisplayName),
                KeyValuePair.Create("nameCreatedBy", nameCreatedBy),
                KeyValuePair.Create("url", _settings.Portal.BasePortalAddress),
                KeyValuePair.Create("passwordResendUrl", _settings.Portal.PasswordResendAddress),
            });
            mailingProcessCreation.CreateMailProcess(email, "NewUserTemplate", mailParameters);
            mailingProcessCreation.CreateMailProcess(email, "NewUserPasswordTemplate", mailParameters);
            await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);

            yield return email;
        }
    }

    private Task<IEnumerable<UserRoleData>> GetOwnCompanyUserRoleData(IEnumerable<string> roles)
    {
        if (!roles.Any())
        {
            Task.FromResult(Enumerable.Empty<UserRoleData>());
        }

        return userProvisioningService.GetOwnCompanyPortalRoleDatas(_settings.Portal.KeycloakClientID, roles, _identityData.CompanyId);
    }

    public async Task<Guid> CreateOwnCompanyIdpUserAsync(Guid identityProviderId, UserCreationInfoIdp userCreationInfo)
    {
        var (companyNameIdpAliasData, nameCreatedBy) = await userProvisioningService.GetCompanyNameIdpAliasData(identityProviderId, _identityData.IdentityId).ConfigureAwait(ConfigureAwaitOptions.None);
        var displayName = await userProvisioningService.GetIdentityProviderDisplayName(companyNameIdpAliasData.IdpAlias).ConfigureAwait(ConfigureAwaitOptions.None) ?? companyNameIdpAliasData.IdpAlias;

        if (!userCreationInfo.Roles.Any())
        {
            throw new ControllerArgumentException($"at least one role must be specified", nameof(userCreationInfo.Roles));
        }

        var roleDatas = await GetOwnCompanyUserRoleData(userCreationInfo.Roles).ConfigureAwait(ConfigureAwaitOptions.None);

        var result = await userProvisioningService.CreateOwnCompanyIdpUsersAsync(
                companyNameIdpAliasData,
                Enumerable.Repeat(
                    new UserCreationRoleDataIdpInfo(
                    userCreationInfo.FirstName,
                    userCreationInfo.LastName,
                    userCreationInfo.Email,
                    roleDatas,
                    userCreationInfo.UserName,
                    userCreationInfo.UserId,
                    UserStatusId.ACTIVE,
                    true
                ), 1).ToAsyncEnumerable(),
                creationData =>
                {
                    var mailParameters = ImmutableDictionary.CreateBuilder<string, string>();
                    mailParameters.AddRange([
                        new("companyName", displayName),
                        new("nameCreatedBy", nameCreatedBy),
                        new("url", _settings.Portal.BasePortalAddress),
                        new("idpAlias", displayName),
                    ]);

                    IEnumerable<string> mailTemplates = companyNameIdpAliasData.IsSharedIdp
                        ? ["NewUserTemplate", "NewUserPasswordTemplate"]
                        : ["NewUserExternalIdpTemplate"];

                    if (companyNameIdpAliasData.IsSharedIdp)
                    {
                        mailParameters.Add(new("password", creationData.Password ?? throw new UnexpectedConditionException("password should never be null here")));
                    }

                    foreach (var template in mailTemplates)
                    {
                        mailingProcessCreation.CreateMailProcess(creationData.UserCreationInfo.Email, template, mailParameters.ToImmutable());
                    }
                })
            .FirstAsync()
            .ConfigureAwait(false);

        if (result.Error != null)
        {
            throw result.Error;
        }
        return result.CompanyUserId;
    }

    public Task<Pagination.Response<CompanyUserData>> GetOwnCompanyUserDatasAsync(int page, int size, GetOwnCompanyUsersFilter filter)
    {
        async Task<Pagination.Source<CompanyUserData>?> GetCompanyUserData(int skip, int take)
        {
            var companyData = await portalRepositories.GetInstance<IUserRepository>().GetOwnCompanyUserData(
                _identityData.CompanyId,
                filter.CompanyUserId,
                filter.FirstName,
                filter.LastName,
                filter.Email,
                _settings.CompanyUserStatusIds
            )(skip, take).ConfigureAwait(ConfigureAwaitOptions.None);

            if (companyData == null)
                return null;

            var displayNames = await companyData.Data
                .SelectMany(x => x.IdpUserIds)
                .Select(x => x.Alias ?? throw new ConflictException("Alias must not be null"))
                .Distinct()
                .ToImmutableDictionaryAsync(GetDisplayName).ConfigureAwait(ConfigureAwaitOptions.None);

            return new Pagination.Source<CompanyUserData>(
                companyData.Count,
                companyData.Data.Select(d => new CompanyUserData(
                    d.CompanyUserId,
                    d.UserStatusId,
                    d.FirstName,
                    d.LastName,
                    d.Email,
                    d.Roles,
                    d.IdpUserIds.Select(x =>
                        new IdpUserId(
                            displayNames[x.Alias!],
                            x.Alias!,
                            x.UserId)))));
        }

        return Pagination.CreateResponseAsync(
            page,
            size,
            _settings.ApplicationsMaxPageSize,
            GetCompanyUserData);
    }

    private async Task<string> GetDisplayName(string alias) => await provisioningManager.GetIdentityProviderDisplayName(alias).ConfigureAwait(ConfigureAwaitOptions.None) ?? throw new ConflictException($"Display Name should not be null for alias: {alias}");

    public async Task<CompanyUserDetailData> GetOwnCompanyUserDetailsAsync(Guid userId)
    {
        var companyId = _identityData.CompanyId;
        var details = await portalRepositories.GetInstance<IUserRepository>().GetOwnCompanyUserDetailsUntrackedAsync(userId, companyId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (details == null)
        {
            throw new NotFoundException($"no company-user data found for user {userId} in company {companyId}");
        }

        return new CompanyUserDetailData(
            details.CompanyUserId,
            details.CreatedAt,
            details.BusinessPartnerNumbers,
            details.CompanyName,
            details.UserStatusId,
            details.AssignedRoles,
            await Task.WhenAll(details.IdpUserIds.Select(async x =>
                new IdpUserId(
                    await GetDisplayName(x.Alias ?? throw new ConflictException("Alias must not be null")).ConfigureAwait(ConfigureAwaitOptions.None),
                    x.Alias,
                    x.UserId))).ConfigureAwait(ConfigureAwaitOptions.None),
            details.FirstName,
            details.LastName,
            details.Email);
    }

    public async Task<CompanyUsersBpnDetails> AddOwnCompanyUsersBusinessPartnerNumbersAsync(Guid userId, string token, IEnumerable<string> businessPartnerNumbers, CancellationToken cancellationToken)
    {
        var companyId = _identityData.CompanyId;
        var (assignedBusinessPartnerNumbers, isValidUser) = await portalRepositories.GetInstance<IUserRepository>().GetOwnCompanyUserWithAssignedBusinessPartnerNumbersUntrackedAsync(userId, companyId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (!isValidUser)
        {
            throw new NotFoundException($"user {userId} not found in company {companyId}");
        }

        var iamUserId = await provisioningManager.GetUserByUserName(userId.ToString()).ConfigureAwait(ConfigureAwaitOptions.None) ??
                        throw new ConflictException($"user {userId} not found in keycloak");

        var (successfulBpns, unsuccessfulBpns) = await businessPartnerNumbers.AggregateAwait(
            (SuccessfulBpns: ImmutableList.CreateBuilder<string>(), UnsuccessfulBpns: ImmutableList.CreateBuilder<UnsuccessfulBpns>()),
            async (acc, bpn) =>
            {
                var (bpns, error) = await CompanyUsersBpnCheck(bpn, token, cancellationToken).ConfigureAwait(false);
                if (error == null)
                {
                    acc.SuccessfulBpns.Add(bpns);
                }
                else
                {
                    acc.UnsuccessfulBpns.Add(new UnsuccessfulBpns(bpns, error.Message));
                }
                return acc;
            },
            acc => (acc.SuccessfulBpns.ToImmutable(), acc.UnsuccessfulBpns.ToImmutable()),
            cancellationToken
        ).ConfigureAwait(ConfigureAwaitOptions.None);

        if (successfulBpns.Count != 0)
        {
            await provisioningManager.AddBpnAttributetoUserAsync(iamUserId, successfulBpns).ConfigureAwait(false);
            successfulBpns.Except(assignedBusinessPartnerNumbers).IfAny(businessPartnersToAdd =>
                portalRepositories.GetInstance<IUserBusinessPartnerRepository>().CreateCompanyUserAssignedBusinessPartners(businessPartnersToAdd.Select(bpn => (userId, bpn))));
        }

        await portalRepositories.SaveAsync();
        return new CompanyUsersBpnDetails(successfulBpns, unsuccessfulBpns);
    }

    private async ValueTask<(string bpns, Exception? error)> CompanyUsersBpnCheck(string bpn, string token, CancellationToken cancellationToken)
    {
        Exception? error = null;
        try
        {
            if (bpn.Length > 20)
            {
                throw new ControllerArgumentException("BusinessPartnerNumbers must not exceed 20 characters");
            }

            var legalEntity = await bpnAccess.FetchLegalEntityByBpn(bpn, token, cancellationToken).ConfigureAwait(false);
            if (!bpn.Equals(legalEntity.Bpn, StringComparison.OrdinalIgnoreCase))
            {
                throw new ConflictException("Bpdm did return incorrect bpn legal-entity-data");
            }
        }
        catch (Exception ex)
        {
            error = ex;
        }

        return (bpn, error);
    }

    public Task<CompanyUsersBpnDetails> AddOwnCompanyUsersBusinessPartnerNumberAsync(Guid userId, string token, string businessPartnerNumber, CancellationToken cancellationToken) =>
        AddOwnCompanyUsersBusinessPartnerNumbersAsync(userId, token, Enumerable.Repeat(businessPartnerNumber, 1), cancellationToken);

    public async Task<CompanyOwnUserDetails> GetOwnUserDetails()
    {
        var userId = _identityData.IdentityId;
        var userRoleIds = await portalRepositories.GetInstance<IUserRolesRepository>()
            .GetUserRoleIdsUntrackedAsync(_settings.UserAdminRoles).ToListAsync().ConfigureAwait(false);
        var details = await portalRepositories.GetInstance<IUserRepository>().GetUserDetailsUntrackedAsync(userId, userRoleIds).ConfigureAwait(ConfigureAwaitOptions.None);
        if (details == null)
        {
            throw new NotFoundException($"no company-user data found for user {userId}");
        }

        return new CompanyOwnUserDetails(
            details.CompanyUserId,
            details.CreatedAt,
            details.BusinessPartnerNumbers,
            details.CompanyName,
            details.UserStatusId,
            details.AssignedRoles,
            details.AdminDetails,
            await Task.WhenAll(details.IdpUserIds.Select(async x =>
                new IdpUserId(
                    await GetDisplayName(x.Alias ?? throw new ConflictException("Alias must not be null")).ConfigureAwait(ConfigureAwaitOptions.None),
                    x.Alias,
                    x.UserId))).ConfigureAwait(ConfigureAwaitOptions.None),
            details.FirstName,
            details.LastName,
            details.Email);
    }

    public async Task<CompanyUserDetails> UpdateOwnUserDetails(Guid companyUserId, OwnCompanyUserEditableDetails ownCompanyUserEditableDetails)
    {
        var userId = _identityData.IdentityId;
        if (companyUserId != userId)
        {
            throw new ForbiddenException($"invalid userId {companyUserId} for user {userId}");
        }

        var userRepository = portalRepositories.GetInstance<IUserRepository>();
        var userData = await userRepository.GetUserWithCompanyIdpAsync(companyUserId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (userData == null)
        {
            throw new ArgumentOutOfRangeException($"user {companyUserId} is not a shared idp user");
        }

        var companyUser = userData.CompanyUser;
        var iamUserId = await provisioningManager.GetUserByUserName(companyUserId.ToString()).ConfigureAwait(ConfigureAwaitOptions.None) ?? throw new ConflictException($"user {companyUserId} not found in keycloak");
        var iamIdpAlias = userData.IamIdpAlias;
        var userIdShared = await provisioningManager.GetProviderUserIdForCentralUserIdAsync(iamIdpAlias, iamUserId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (userIdShared == null)
        {
            throw new NotFoundException($"no shared realm userid found for {iamUserId} in realm {iamIdpAlias}");
        }

        await provisioningManager.UpdateSharedRealmUserAsync(
            iamIdpAlias,
            userIdShared,
            ownCompanyUserEditableDetails.FirstName ?? "",
            ownCompanyUserEditableDetails.LastName ?? "",
            ownCompanyUserEditableDetails.Email ?? "").ConfigureAwait(ConfigureAwaitOptions.None);

        userRepository.AttachAndModifyCompanyUser(companyUserId, cu =>
            {
                cu.Firstname = companyUser.Firstname;
                cu.Lastname = companyUser.Lastname;
                cu.Email = companyUser.Email;
            },
            cu =>
            {
                cu.Firstname = ownCompanyUserEditableDetails.FirstName;
                cu.Lastname = ownCompanyUserEditableDetails.LastName;
                cu.Email = ownCompanyUserEditableDetails.Email;
            });
        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
        return new CompanyUserDetails(
            companyUserId,
            companyUser.DateCreated,
            userData.BusinessPartnerNumbers,
            companyUser.CompanyName,
            companyUser.UserStatusId,
            userData.AssignedRoles,
            companyUser.Firstname,
            companyUser.Lastname,
            companyUser.Email);
    }

    public async Task<int> DeleteOwnUserAsync(Guid companyUserId)
    {
        var userId = _identityData.IdentityId;
        if (companyUserId != userId)
        {
            throw new ForbiddenException($"companyUser {companyUserId} is not the id of user {userId}");
        }

        var iamIdpAliasAccountData = await portalRepositories.GetInstance<IUserRepository>().GetSharedIdentityProviderUserAccountDataUntrackedAsync(userId);
        if (iamIdpAliasAccountData == default)
        {
            throw new ConflictException($"user {userId} does not exist");
        }

        var (sharedIdpAlias, accountData) = iamIdpAliasAccountData;
        await DeleteUserInternalAsync(sharedIdpAlias, accountData).ConfigureAwait(ConfigureAwaitOptions.None);
        return await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public async IAsyncEnumerable<Guid> DeleteOwnCompanyUsersAsync(IEnumerable<Guid> userIds)
    {
        var companyId = _identityData.CompanyId;
        var iamIdpAlias = await portalRepositories.GetInstance<IIdentityProviderRepository>().GetSharedIdentityProviderIamAliasDataUntrackedAsync(companyId);

        bool success;
        await foreach (var accountData in portalRepositories.GetInstance<IUserRepository>().GetCompanyUserAccountDataUntrackedAsync(userIds, companyId).ConfigureAwait(false))
        {
            try
            {
                await DeleteUserInternalAsync(iamIdpAlias, accountData).ConfigureAwait(ConfigureAwaitOptions.None);
                success = true;
            }
            catch (Exception e)
            {
                success = false;
                if (iamIdpAlias == null)
                {
                    logger.LogError(e, "Error while deleting companyUser {userId}", accountData.CompanyUserId);
                }
                else
                {
                    logger.LogError(e, "Error while deleting companyUser {userId} from shared idp {iamIdpAlias}", accountData.CompanyUserId, iamIdpAlias);
                }
            }

            if (success)
            {
                yield return accountData.CompanyUserId;
            }
        }

        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    private async Task DeleteUserInternalAsync(string? sharedIdpAlias, CompanyUserAccountData accountData)
    {
        var (companyUserId, businessPartnerNumbers, roleIds, offerIds, invitationIds) = accountData;
        var iamUserId = await provisioningManager.GetUserByUserName(companyUserId.ToString()).ConfigureAwait(ConfigureAwaitOptions.None);
        if (iamUserId != null)
        {
            await DeleteIamUserAsync(sharedIdpAlias, iamUserId).ConfigureAwait(ConfigureAwaitOptions.None);
        }

        portalRepositories.GetInstance<IUserRepository>().AttachAndModifyIdentity(
            companyUserId,
            null,
            i =>
            {
                i.UserStatusId = UserStatusId.DELETED;
            });

        portalRepositories.GetInstance<IUserBusinessPartnerRepository>()
            .DeleteCompanyUserAssignedBusinessPartners(businessPartnerNumbers.Select(bpn => (companyUserId, bpn)));

        portalRepositories.GetInstance<IOfferRepository>()
            .DeleteAppFavourites(offerIds.Select(offerId => (offerId, companyUserId)));

        portalRepositories.GetInstance<IUserRolesRepository>()
            .DeleteCompanyUserAssignedRoles(roleIds.Select(userRoleId => (companyUserId, userRoleId)));

        portalRepositories.GetInstance<IApplicationRepository>()
            .DeleteInvitations(invitationIds);
    }

    private async Task DeleteIamUserAsync(string? sharedIdpAlias, string iamUserId)
    {
        if (sharedIdpAlias != null)
        {
            var userIdShared = await provisioningManager.GetProviderUserIdForCentralUserIdAsync(sharedIdpAlias, iamUserId).ConfigureAwait(ConfigureAwaitOptions.None);
            if (userIdShared != null)
            {
                await provisioningManager.DeleteSharedRealmUserAsync(sharedIdpAlias, userIdShared).ConfigureAwait(ConfigureAwaitOptions.None);
            }
        }

        await provisioningManager.DeleteCentralRealmUserAsync(iamUserId).ConfigureAwait(ConfigureAwaitOptions.None);
    }

    private async Task<bool> CanResetPassword(Guid userId)
    {
        var now = DateTimeOffset.UtcNow;

        var userInfo = (await provisioningDbAccess.GetUserPasswordResetInfo(userId).ConfigureAwait(ConfigureAwaitOptions.None))
            ?? provisioningDbAccess.CreateUserPasswordResetInfo(userId, now, 0);

        if (now < userInfo.PasswordModifiedAt.AddHours(_settings.PasswordReset.NoOfHours))
        {
            if (userInfo.ResetCount < _settings.PasswordReset.MaxNoOfReset)
            {
                userInfo.ResetCount++;
                await provisioningDbAccess.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
                return true;
            }
        }
        else
        {
            userInfo.ResetCount = 1;
            userInfo.PasswordModifiedAt = now;
            await provisioningDbAccess.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
            return true;
        }

        return false;
    }

    public async Task<bool> ExecuteOwnCompanyUserPasswordReset(Guid companyUserId)
    {
        var (alias, isValidUser) = await portalRepositories.GetInstance<IIdentityProviderRepository>().GetIdpCategoryIdByUserIdAsync(companyUserId, _identityData.CompanyId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (isValidUser && !string.IsNullOrWhiteSpace(alias))
        {
            if (await CanResetPassword(_identityData.IdentityId).ConfigureAwait(ConfigureAwaitOptions.None))
            {
                var iamUserId = await provisioningManager.GetUserByUserName(companyUserId.ToString()).ConfigureAwait(ConfigureAwaitOptions.None) ?? throw new ConflictException($"user {companyUserId} not found in keycloak");
                await provisioningManager.ResetSharedUserPasswordAsync(alias, iamUserId).ConfigureAwait(ConfigureAwaitOptions.None);
                return true;
            }

            throw new ArgumentException($"cannot reset password more often than {_settings.PasswordReset.MaxNoOfReset} in {_settings.PasswordReset.NoOfHours} hours");
        }

        throw new NotFoundException($"Cannot identify companyId or shared idp : userId {companyUserId} is not associated with admin users company {_identityData.CompanyId}");
    }

    public Task<Pagination.Response<CompanyAppUserDetails>> GetOwnCompanyAppUsersAsync(Guid appId, int page, int size, CompanyUserFilter filter) =>
        Pagination.CreateResponseAsync(
            page,
            size,
            15,
            portalRepositories.GetInstance<IUserRepository>().GetOwnCompanyAppUsersPaginationSourceAsync(
                appId,
                _identityData.IdentityId,
                new[] { OfferSubscriptionStatusId.ACTIVE },
                new[] { UserStatusId.ACTIVE, UserStatusId.INACTIVE },
                filter));

    public async Task<int> DeleteOwnUserBusinessPartnerNumbersAsync(Guid userId, string businessPartnerNumber)
    {
        var userBusinessPartnerRepository = portalRepositories.GetInstance<IUserBusinessPartnerRepository>();

        var (isValidUser, isAssignedBusinessPartner, isSameCompany) = await userBusinessPartnerRepository.GetOwnCompanyUserWithAssignedBusinessPartnerNumbersAsync(userId, _identityData.CompanyId, businessPartnerNumber.ToUpper()).ConfigureAwait(ConfigureAwaitOptions.None);

        if (!isValidUser)
        {
            throw new NotFoundException($"user {userId} does not exist");
        }

        if (!isAssignedBusinessPartner)
        {
            throw new ForbiddenException($"businessPartnerNumber {businessPartnerNumber} is not assigned to user {userId}");
        }

        if (!isSameCompany)
        {
            throw new ForbiddenException($"userId {userId} and adminUserId {_identityData.IdentityId} do not belong to same company");
        }

        var iamUserId = await provisioningManager.GetUserByUserName(userId.ToString()).ConfigureAwait(ConfigureAwaitOptions.None) ?? throw new ConflictException($"user {userId} is not associated with a user in keycloak");

        userBusinessPartnerRepository.DeleteCompanyUserAssignedBusinessPartner(userId, businessPartnerNumber.ToUpper());

        await provisioningManager.DeleteCentralUserBusinessPartnerNumberAsync(iamUserId, businessPartnerNumber.ToUpper()).ConfigureAwait(ConfigureAwaitOptions.None);

        return await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }
}
