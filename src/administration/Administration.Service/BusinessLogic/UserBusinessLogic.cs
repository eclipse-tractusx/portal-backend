/********************************************************************************
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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Async;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Mailing.SendMail;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

/// <summary>
/// Implementation of <see cref="IUserBusinessLogic"/>.
/// </summary>
public class UserBusinessLogic : IUserBusinessLogic
{
    private readonly IProvisioningManager _provisioningManager;
    private readonly IUserProvisioningService _userProvisioningService;
    private readonly IProvisioningDBAccess _provisioningDbAccess;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IIdentityData _identityData;
    private readonly IMailingService _mailingService;
    private readonly ILogger<UserBusinessLogic> _logger;
    private readonly UserSettings _settings;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="provisioningManager">Provisioning Manager</param>
    /// <param name="userProvisioningService">User Provisioning Service</param>
    /// <param name="provisioningDbAccess">Provisioning DBAccess</param>
    /// <param name="identityService">Access to the identity</param>
    /// <param name="mailingService">Mailing Service</param>
    /// <param name="logger">logger</param>
    /// <param name="settings">Settings</param>
    /// <param name="portalRepositories">Portal Repositories</param>
    public UserBusinessLogic(
        IProvisioningManager provisioningManager,
        IUserProvisioningService userProvisioningService,
        IProvisioningDBAccess provisioningDbAccess,
        IPortalRepositories portalRepositories,
        IIdentityService identityService,
        IMailingService mailingService,
        ILogger<UserBusinessLogic> logger,
        IOptions<UserSettings> settings)
    {
        _provisioningManager = provisioningManager;
        _userProvisioningService = userProvisioningService;
        _provisioningDbAccess = provisioningDbAccess;
        _portalRepositories = portalRepositories;
        _identityData = identityService.IdentityData;
        _mailingService = mailingService;
        _logger = logger;
        _settings = settings.Value;
    }

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
        var (companyNameIdpAliasData, nameCreatedBy) = await _userProvisioningService.GetCompanyNameSharedIdpAliasData(_identityData.IdentityId).ConfigureAwait(false);

        var distinctRoles = userList.SelectMany(user => user.Roles).Distinct().ToList();

        var roleDatas = await GetOwnCompanyUserRoleData(distinctRoles).ConfigureAwait(false);

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

        var companyDisplayName = await _userProvisioningService.GetIdentityProviderDisplayName(companyNameIdpAliasData.IdpAlias).ConfigureAwait(false);

        await foreach (var (_, userName, password, error) in _userProvisioningService.CreateOwnCompanyIdpUsersAsync(companyNameIdpAliasData, userCreationInfoIdps).ConfigureAwait(false))
        {
            var email = emailData[userName];

            if (error != null)
            {
                _logger.LogError(error, "Error while creating user {UserName} ({Email})", userName, email);
                continue;
            }

            var mailParameters = new Dictionary<string, string>
            {
                { "password", password ?? "" },
                { "companyName", companyDisplayName },
                { "nameCreatedBy", nameCreatedBy },
                { "url", _settings.Portal.BasePortalAddress },
                { "passwordResendUrl", _settings.Portal.PasswordResendAddress },
            };

            try
            {
                await _mailingService.SendMails(email, mailParameters, new List<string> { "NewUserTemplate", "NewUserPasswordTemplate" }).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error sending email to {Email} after creating user {UserName}", email, userName);
            }

            yield return email;
        }
    }

    private Task<IEnumerable<UserRoleData>> GetOwnCompanyUserRoleData(IEnumerable<string> roles)
    {
        if (!roles.Any())
        {
            Task.FromResult(Enumerable.Empty<UserRoleData>());
        }

        return _userProvisioningService.GetOwnCompanyPortalRoleDatas(_settings.Portal.KeycloakClientID, roles, _identityData.CompanyId);
    }

    public async Task<Guid> CreateOwnCompanyIdpUserAsync(Guid identityProviderId, UserCreationInfoIdp userCreationInfo)
    {
        var (companyNameIdpAliasData, nameCreatedBy) = await _userProvisioningService.GetCompanyNameIdpAliasData(identityProviderId, _identityData.IdentityId).ConfigureAwait(false);
        var displayName = await _userProvisioningService.GetIdentityProviderDisplayName(companyNameIdpAliasData.IdpAlias).ConfigureAwait(false);

        if (!userCreationInfo.Roles.Any())
        {
            throw new ControllerArgumentException($"at least one role must be specified", nameof(userCreationInfo.Roles));
        }

        var roleDatas = await GetOwnCompanyUserRoleData(userCreationInfo.Roles).ConfigureAwait(false);

        var result = await _userProvisioningService.CreateOwnCompanyIdpUsersAsync(
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
                ), 1).ToAsyncEnumerable())
            .FirstAsync()
            .ConfigureAwait(false);

        if (result.Error != null)
        {
            throw result.Error;
        }

        var mailParameters = new Dictionary<string, string>
        {
            { "companyName", displayName },
            { "nameCreatedBy", nameCreatedBy },
            { "url", _settings.Portal.BasePortalAddress },
        };

        var mailTemplates = companyNameIdpAliasData.IsSharedIdp
            ? new[] { "NewUserTemplate", "NewUserPasswordTemplate" }
            : new[] { "NewUserOwnIdpTemplate" };

        if (companyNameIdpAliasData.IsSharedIdp)
        {
            mailParameters["password"] = result.Password;
        }

        try
        {
            await _mailingService.SendMails(userCreationInfo.Email, mailParameters, mailTemplates).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error sending email to {Email} after creating user {UserName}", userCreationInfo.Email, userCreationInfo.UserName);
        }

        return result.CompanyUserId;
    }

    public Task<Pagination.Response<CompanyUserData>> GetOwnCompanyUserDatasAsync(int page, int size, GetOwnCompanyUsersFilter filter)
    {
        async Task<Pagination.Source<CompanyUserData>?> GetCompanyUserData(int skip, int take)
        {
            var companyData = await _portalRepositories.GetInstance<IUserRepository>().GetOwnCompanyUserData(
                _identityData.CompanyId,
                filter.CompanyUserId,
                filter.FirstName,
                filter.LastName,
                filter.Email,
                _settings.CompanyUserStatusIds
            )(skip, take).ConfigureAwait(false);

            if (companyData == null)
                return null;

            var displayNames = await companyData.Data
                .SelectMany(x => x.IdpUserIds)
                .Select(x => x.Alias ?? throw new ConflictException("Alias must not be null"))
                .Distinct()
                .ToImmutableDictionaryAsync(GetDisplayName).ConfigureAwait(false);

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

    private async Task<string> GetDisplayName(string alias) => await _provisioningManager.GetIdentityProviderDisplayName(alias).ConfigureAwait(false) ?? throw new ConflictException($"Display Name should not be null for alias: {alias}");

    [Obsolete("to be replaced by UserRolesBusinessLogic.GetAppRolesAsync. Remove as soon frontend is adjusted")]
    public async IAsyncEnumerable<ClientRoles> GetClientRolesAsync(Guid appId, string? languageShortName = null)
    {
        var appRepository = _portalRepositories.GetInstance<IOfferRepository>();
        if (!await appRepository.CheckAppExistsById(appId).ConfigureAwait(false))
        {
            throw new NotFoundException($"app {appId} does not found");
        }

        if (languageShortName != null && !await _portalRepositories.GetInstance<ILanguageRepository>().IsValidLanguageCode(languageShortName))
        {
            throw new ArgumentException($"language {languageShortName} does not exist");
        }

        await foreach (var roles in appRepository.GetClientRolesAsync(appId, languageShortName ?? Constants.DefaultLanguage).ConfigureAwait(false))
        {
            yield return new ClientRoles(roles.RoleId, roles.Role, roles.Description);
        }
    }

    public async Task<CompanyUserDetailData> GetOwnCompanyUserDetailsAsync(Guid userId)
    {
        var companyId = _identityData.CompanyId;
        var details = await _portalRepositories.GetInstance<IUserRepository>().GetOwnCompanyUserDetailsUntrackedAsync(userId, companyId).ConfigureAwait(false);
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
                    await GetDisplayName(x.Alias ?? throw new ConflictException("Alias must not be null")).ConfigureAwait(false),
                    x.Alias,
                    x.UserId))).ConfigureAwait(false));
    }

    public async Task<int> AddOwnCompanyUsersBusinessPartnerNumbersAsync(Guid userId, IEnumerable<string> businessPartnerNumbers)
    {
        if (businessPartnerNumbers.Any(businessPartnerNumber => businessPartnerNumber.Length > 20))
        {
            throw new ControllerArgumentException("businessPartnerNumbers must not exceed 20 characters", nameof(businessPartnerNumbers));
        }

        var companyId = _identityData.CompanyId;
        var (assignedBusinessPartnerNumbers, isValidUser) = await _portalRepositories.GetInstance<IUserRepository>().GetOwnCompanyUserWithAssignedBusinessPartnerNumbersUntrackedAsync(userId, companyId).ConfigureAwait(false);
        if (!isValidUser)
        {
            throw new NotFoundException($"user {userId} not found in company {companyId}");
        }

        var iamUserId = await _provisioningManager.GetUserByUserName(userId.ToString()).ConfigureAwait(false) ?? throw new ConflictException("user {userId} not found in keycloak");
        var businessPartnerRepository = _portalRepositories.GetInstance<IUserBusinessPartnerRepository>();
        await _provisioningManager.AddBpnAttributetoUserAsync(iamUserId, businessPartnerNumbers).ConfigureAwait(false);
        foreach (var businessPartnerToAdd in businessPartnerNumbers.Except(assignedBusinessPartnerNumbers))
        {
            businessPartnerRepository.CreateCompanyUserAssignedBusinessPartner(userId, businessPartnerToAdd);
        }

        return await _portalRepositories.SaveAsync();
    }

    public Task<int> AddOwnCompanyUsersBusinessPartnerNumberAsync(Guid userId, string businessPartnerNumber) =>
        AddOwnCompanyUsersBusinessPartnerNumbersAsync(userId, Enumerable.Repeat(businessPartnerNumber, 1));

    public async Task<CompanyOwnUserDetails> GetOwnUserDetails()
    {
        var userId = _identityData.IdentityId;
        var userRoleIds = await _portalRepositories.GetInstance<IUserRolesRepository>()
            .GetUserRoleIdsUntrackedAsync(_settings.UserAdminRoles).ToListAsync().ConfigureAwait(false);
        var details = await _portalRepositories.GetInstance<IUserRepository>().GetUserDetailsUntrackedAsync(userId, userRoleIds).ConfigureAwait(false);
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
                    await GetDisplayName(x.Alias ?? throw new ConflictException("Alias must not be null")).ConfigureAwait(false),
                    x.Alias,
                    x.UserId))).ConfigureAwait(false));
    }

    public async Task<CompanyUserDetails> UpdateOwnUserDetails(Guid companyUserId, OwnCompanyUserEditableDetails ownCompanyUserEditableDetails)
    {
        var userId = _identityData.IdentityId;
        if (companyUserId != userId)
        {
            throw new ForbiddenException($"invalid userId {companyUserId} for user {userId}");
        }

        var userRepository = _portalRepositories.GetInstance<IUserRepository>();
        var userData = await userRepository.GetUserWithCompanyIdpAsync(companyUserId).ConfigureAwait(false);
        if (userData == null)
        {
            throw new ArgumentOutOfRangeException($"user {companyUserId} is not a shared idp user");
        }

        var companyUser = userData.CompanyUser;
        var iamUserId = await _provisioningManager.GetUserByUserName(companyUserId.ToString()).ConfigureAwait(false) ?? throw new ConflictException($"user {companyUserId} not found in keycloak");
        var iamIdpAlias = userData.IamIdpAlias;
        var userIdShared = await _provisioningManager.GetProviderUserIdForCentralUserIdAsync(iamIdpAlias, iamUserId).ConfigureAwait(false);
        if (userIdShared == null)
        {
            throw new NotFoundException($"no shared realm userid found for {iamUserId} in realm {iamIdpAlias}");
        }

        await _provisioningManager.UpdateSharedRealmUserAsync(
            iamIdpAlias,
            userIdShared,
            ownCompanyUserEditableDetails.FirstName ?? "",
            ownCompanyUserEditableDetails.LastName ?? "",
            ownCompanyUserEditableDetails.Email ?? "").ConfigureAwait(false);

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
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
        return new CompanyUserDetails(
            companyUserId,
            companyUser.DateCreated,
            userData.BusinessPartnerNumbers,
            companyUser.CompanyName,
            companyUser.UserStatusId,
            userData.AssignedRoles)
        {
            FirstName = companyUser.Firstname,
            LastName = companyUser.Lastname,
            Email = companyUser.Email
        };
    }

    public async Task<int> DeleteOwnUserAsync(Guid companyUserId)
    {
        var userId = _identityData.IdentityId;
        if (companyUserId != userId)
        {
            throw new ForbiddenException($"companyUser {companyUserId} is not the id of user {userId}");
        }
        var iamIdpAliasAccountData = await _portalRepositories.GetInstance<IUserRepository>().GetSharedIdentityProviderUserAccountDataUntrackedAsync(userId);
        if (iamIdpAliasAccountData == default)
        {
            throw new ConflictException($"user {userId} does not exist");
        }
        var (sharedIdpAlias, accountData) = iamIdpAliasAccountData;
        await DeleteUserInternalAsync(sharedIdpAlias, accountData).ConfigureAwait(false);
        return await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    public async IAsyncEnumerable<Guid> DeleteOwnCompanyUsersAsync(IEnumerable<Guid> userIds)
    {
        var companyId = _identityData.CompanyId;
        var iamIdpAlias = await _portalRepositories.GetInstance<IIdentityProviderRepository>().GetSharedIdentityProviderIamAliasDataUntrackedAsync(companyId);

        await foreach (var accountData in _portalRepositories.GetInstance<IUserRepository>().GetCompanyUserAccountDataUntrackedAsync(userIds, companyId).ConfigureAwait(false))
        {
            var success = false;
            try
            {
                await DeleteUserInternalAsync(iamIdpAlias, accountData).ConfigureAwait(false);
                success = true;
            }
            catch (Exception e)
            {
                if (iamIdpAlias == null)
                {
                    _logger.LogError(e, "Error while deleting companyUser {userId}", accountData.CompanyUserId);
                }
                else
                {
                    _logger.LogError(e, "Error while deleting companyUser {userId} from shared idp {iamIdpAlias}", accountData.CompanyUserId, iamIdpAlias);
                }
            }
            if (success)
            {
                yield return accountData.CompanyUserId;
            }
        }
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    private async Task DeleteUserInternalAsync(string? sharedIdpAlias, CompanyUserAccountData accountData)
    {
        var (companyUserId, businessPartnerNumbers, roleIds, offerIds, invitationIds) = accountData;
        var iamUserId = await _provisioningManager.GetUserByUserName(companyUserId.ToString()).ConfigureAwait(false);
        if (iamUserId != null)
        {
            await DeleteIamUserAsync(sharedIdpAlias, iamUserId).ConfigureAwait(false);
        }

        _portalRepositories.GetInstance<IUserRepository>().AttachAndModifyIdentity(
            companyUserId,
            null,
            i =>
            {
                i.UserStatusId = UserStatusId.DELETED;
            });

        _portalRepositories.GetInstance<IUserBusinessPartnerRepository>()
            .DeleteCompanyUserAssignedBusinessPartners(businessPartnerNumbers.Select(bpn => (companyUserId, bpn)));

        _portalRepositories.GetInstance<IOfferRepository>()
            .DeleteAppFavourites(offerIds.Select(offerId => (offerId, companyUserId)));

        _portalRepositories.GetInstance<IUserRolesRepository>()
            .DeleteCompanyUserAssignedRoles(roleIds.Select(userRoleId => (companyUserId, userRoleId)));

        _portalRepositories.GetInstance<IApplicationRepository>()
            .DeleteInvitations(invitationIds);
    }

    private async Task DeleteIamUserAsync(string? sharedIdpAlias, string iamUserId)
    {
        if (sharedIdpAlias != null)
        {
            var userIdShared = await _provisioningManager.GetProviderUserIdForCentralUserIdAsync(sharedIdpAlias, iamUserId).ConfigureAwait(false);
            if (userIdShared != null)
            {
                await _provisioningManager.DeleteSharedRealmUserAsync(sharedIdpAlias, userIdShared).ConfigureAwait(false);
            }
        }

        await _provisioningManager.DeleteCentralRealmUserAsync(iamUserId).ConfigureAwait(false);
    }

    private async Task<bool> CanResetPassword(Guid userId)
    {
        var now = DateTimeOffset.UtcNow;

        var userInfo = (await _provisioningDbAccess.GetUserPasswordResetInfo(userId).ConfigureAwait(false))
            ?? _provisioningDbAccess.CreateUserPasswordResetInfo(userId, now, 0);

        if (now < userInfo.PasswordModifiedAt.AddHours(_settings.PasswordReset.NoOfHours))
        {
            if (userInfo.ResetCount < _settings.PasswordReset.MaxNoOfReset)
            {
                userInfo.ResetCount++;
                await _provisioningDbAccess.SaveAsync().ConfigureAwait(false);
                return true;
            }
        }
        else
        {
            userInfo.ResetCount = 1;
            userInfo.PasswordModifiedAt = now;
            await _provisioningDbAccess.SaveAsync().ConfigureAwait(false);
            return true;
        }

        return false;
    }

    public async Task<bool> ExecuteOwnCompanyUserPasswordReset(Guid companyUserId)
    {
        var (alias, isValidUser) = await _portalRepositories.GetInstance<IIdentityProviderRepository>().GetIdpCategoryIdByUserIdAsync(companyUserId, _identityData.CompanyId).ConfigureAwait(false);
        if (isValidUser && !string.IsNullOrWhiteSpace(alias))
        {
            if (await CanResetPassword(_identityData.IdentityId).ConfigureAwait(false))
            {
                var iamUserId = await _provisioningManager.GetUserByUserName(companyUserId.ToString()).ConfigureAwait(false) ?? throw new ConflictException($"user {companyUserId} not found in keycloak");
                await _provisioningManager.ResetSharedUserPasswordAsync(alias, iamUserId).ConfigureAwait(false);
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
            _portalRepositories.GetInstance<IUserRepository>().GetOwnCompanyAppUsersPaginationSourceAsync(
                appId,
                _identityData.IdentityId,
                new[] { OfferSubscriptionStatusId.ACTIVE },
                new[] { UserStatusId.ACTIVE, UserStatusId.INACTIVE },
                filter));

    public async Task<int> DeleteOwnUserBusinessPartnerNumbersAsync(Guid userId, string businessPartnerNumber)
    {
        var userBusinessPartnerRepository = _portalRepositories.GetInstance<IUserBusinessPartnerRepository>();

        var (isValidUser, isAssignedBusinessPartner, isSameCompany) = await userBusinessPartnerRepository.GetOwnCompanyUserWithAssignedBusinessPartnerNumbersAsync(userId, _identityData.CompanyId, businessPartnerNumber).ConfigureAwait(false);

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

        var iamUserId = await _provisioningManager.GetUserByUserName(userId.ToString()).ConfigureAwait(false) ?? throw new ConflictException($"user {userId} is not associated with a user in keycloak");

        userBusinessPartnerRepository.DeleteCompanyUserAssignedBusinessPartner(userId, businessPartnerNumber.ToUpper());

        await _provisioningManager.DeleteCentralUserBusinessPartnerNumberAsync(iamUserId, businessPartnerNumber.ToUpper()).ConfigureAwait(false);

        return await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }
}
