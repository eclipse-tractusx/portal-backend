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

using Org.CatenaX.Ng.Portal.Backend.Administration.Service.Models;
using Org.CatenaX.Ng.Portal.Backend.Framework.ErrorHandling;
using Org.CatenaX.Ng.Portal.Backend.Framework.Models;
using Org.CatenaX.Ng.Portal.Backend.Mailing.SendMail;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.CatenaX.Ng.Portal.Backend.Provisioning.Library;
using Org.CatenaX.Ng.Portal.Backend.Provisioning.Library.Models;
using Org.CatenaX.Ng.Portal.Backend.Provisioning.Library.Service;
using Org.CatenaX.Ng.Portal.Backend.Provisioning.DBAccess;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Org.CatenaX.Ng.Portal.Backend.Administration.Service.BusinessLogic;

/// <summary>
/// Implementation of <see cref="IUserBusinessLogic"/>.
/// </summary>
public class UserBusinessLogic : IUserBusinessLogic
{
    private readonly IProvisioningManager _provisioningManager;
    private readonly IUserProvisioningService _userProvisioningService;
    private readonly IProvisioningDBAccess _provisioningDbAccess;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IMailingService _mailingService;
    private readonly ILogger<UserBusinessLogic> _logger;
    private readonly UserSettings _settings;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="provisioningManager">Provisioning Manager</param>
    /// <param name="userProvisioningService">User Provisioning Service</param>
    /// <param name="provisioningDbAccess">Provisioning DBAccess</param>
    /// <param name="mailingService">Mailing Service</param>
    /// <param name="logger">logger</param>
    /// <param name="settings">Settings</param>
    /// <param name="portalRepositories">Portal Repositories</param>
    public UserBusinessLogic(
        IProvisioningManager provisioningManager,
        IUserProvisioningService userProvisioningService,
        IProvisioningDBAccess provisioningDbAccess,
        IPortalRepositories portalRepositories,
        IMailingService mailingService,
        ILogger<UserBusinessLogic> logger,
        IOptions<UserSettings> settings)
    {
        _provisioningManager = provisioningManager;
        _userProvisioningService = userProvisioningService;
        _provisioningDbAccess = provisioningDbAccess;
        _portalRepositories = portalRepositories;
        _mailingService = mailingService;
        _logger = logger;
        _settings = settings.Value;
    }

    public async IAsyncEnumerable<string> CreateOwnCompanyUsersAsync(IEnumerable<UserCreationInfo> userList, string iamUserId)
    {
        var (companyNameIdpAliasData, nameCreatedBy) = await GetCompanyNameSharedIdpAliasCreatorData(iamUserId).ConfigureAwait(false);

        var distinctRoles = userList.SelectMany(user => user.Roles).Distinct().ToList();

        var roleDatas = await GetOwnCompanyUserRoleData(distinctRoles, iamUserId).ConfigureAwait(false);

        var userCreationInfoIdps = userList.Select(user =>
            new UserCreationRoleDataIdpInfo(
                user.firstName ?? "",
                user.lastName ?? "",
                user.eMail,
                roleDatas.IntersectBy(user.Roles, roleData => roleData.UserRoleText),
                user.userName ?? user.eMail,
                ""
            )).ToAsyncEnumerable();

        var emailData = userList.ToDictionary(
            user => user.userName ?? user.eMail,
            user => (user.eMail, user.Message));

        await foreach(var (_, userName, password, error) in _userProvisioningService.CreateOwnCompanyIdpUsersAsync(companyNameIdpAliasData, userCreationInfoIdps).ConfigureAwait(false))
        {
            var (email, message) = emailData[userName];

            if (error != null)
            {
                _logger.LogError(error, "Error while creating user {UserName} ({Email})", userName, email);
                continue;
            }

            var inviteTemplateName = string.IsNullOrWhiteSpace(message)
                ? "PortalTemplate"
                : "PortalTemplateWithMessage";

            var mailParameters = new Dictionary<string, string>
            {
                { "password", password ?? "" },
                { "companyname", companyNameIdpAliasData.CompanyName },
                { "message", message ?? "" },
                { "nameCreatedBy", nameCreatedBy },
                { "url", _settings.Portal.BasePortalAddress },
                { "username", userName },
            };

            try
            {
                await _mailingService.SendMails(email, mailParameters, new List<string> { inviteTemplateName, "PasswordForPortalTemplate" }).ConfigureAwait(false);
            }
            catch(Exception e)
            {
                _logger.LogError(e, "Error sending email to {Email} after creating user {UserName}", email, userName);
            }

            yield return email;
        }
    }

    private Task<IEnumerable<UserRoleData>> GetOwnCompanyUserRoleData(IEnumerable<string> roles, string iamUserId)
    {
        if (!roles.Any())
        {
            Task.FromResult(Enumerable.Empty<UserRoleData>());
        }
        return _userProvisioningService.GetOwnCompanyPortalRoleDatas(_settings.Portal.KeyCloakClientID, roles, iamUserId);
    }

    private async Task<(CompanyNameIdpAliasData CompanyNameIdpAliasData, string CreatedByName)> GetCompanyNameSharedIdpAliasCreatorData(string iamUserId)
    {
        var result = await _portalRepositories.GetInstance<IIdentityProviderRepository>().GetCompanyNameIdpAliaseUntrackedAsync(iamUserId, IdentityProviderCategoryId.KEYCLOAK_SHARED).ConfigureAwait(false);
        if (result == default)
        {
            throw new ControllerArgumentException($"user {iamUserId} is not associated with any company");
        }
        var (company, companyUser, idpAliase) = result;
        if (company.CompanyName == null)
        {
            throw new UnexpectedConditionException($"assertion failed: companyName of company {company.CompanyId} should never be null here");
        }
        if (!idpAliase.Any())
        {
            throw new ControllerArgumentException($"user {iamUserId} is not associated with any shared idp");
        }
        if (idpAliase.Count() > 1)
        {
            throw new ConflictException($"user {iamUserId} is associated with more than one shared idp");
        }
        
        var companyNameIdpAliasData = new CompanyNameIdpAliasData(company.CompanyId, company.CompanyName, company.BusinessPartnerNumber, companyUser.CompanyUserId, idpAliase.First(), true);
        var createdByName = CreateNameString(companyUser.FirstName, companyUser.LastName, companyUser.Email, iamUserId);

        return (companyNameIdpAliasData,createdByName);
    }

    private static string CreateNameString(string? firstName, string? lastName, string? email, string iamUserId)
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
        return firstName == null && lastName == null && email == null ? iamUserId : sb.ToString();
    }

    public async Task<Guid> CreateOwnCompanyIdpUserAsync(Guid identityProviderId, UserCreationInfoIdp userCreationInfo, string iamUserId)
    {
        var companyNameIdpAliasData = await _userProvisioningService.GetCompanyNameIdpAliasData(identityProviderId, iamUserId).ConfigureAwait(false);

        var roleDatas = await GetOwnCompanyUserRoleData(userCreationInfo.Roles, iamUserId).ConfigureAwait(false);

        var result = await _userProvisioningService.CreateOwnCompanyIdpUsersAsync(
                companyNameIdpAliasData,
                Enumerable.Repeat(
                    new UserCreationRoleDataIdpInfo(
                    userCreationInfo.FirstName,
                    userCreationInfo.LastName,
                    userCreationInfo.Email,
                    roleDatas ?? Enumerable.Empty<UserRoleData>(),
                    userCreationInfo.UserName,
                    userCreationInfo.UserId
                ),1).ToAsyncEnumerable())
            .FirstAsync()
            .ConfigureAwait(false);

        if(result.Error != null)
        {
            throw result.Error;
        }
        return result.CompanyUserId;
    }

    public Task<Pagination.Response<CompanyUserData>> GetOwnCompanyUserDatasAsync(
        string adminUserId,
        int page, 
        int size,
        Guid? companyUserId = null,
        string? userEntityId = null,
        string? firstName = null,
        string? lastName = null,
        string? email = null
        )
    {
        
        var companyUsers = _portalRepositories.GetInstance<IUserRepository>().GetOwnCompanyUserQuery(
            adminUserId,
            companyUserId,
            userEntityId,
            firstName,
            lastName,
            email
        );
        return Pagination.CreateResponseAsync<CompanyUserData>(
            page,
            size,
            _settings.ApplicationsMaxPageSize,
            (int skip, int take) => new Pagination.AsyncSource<CompanyUserData>(
                companyUsers.CountAsync(),
                companyUsers.OrderByDescending(companyUser => companyUser.DateCreated)
                .Skip(skip)
                .Take(take)
                .Select(companyUser => new CompanyUserData(
                companyUser.IamUser!.UserEntityId,
                companyUser.Id,
                companyUser.CompanyUserStatusId,
                companyUser.UserRoles.Select(userRole => userRole.UserRoleText))
            {
                FirstName = companyUser.Firstname,
                LastName = companyUser.Lastname,
                Email = companyUser.Email
            })
            .AsAsyncEnumerable()));
    }

    [Obsolete("to be replaced by UserRolesBusinessLogic.GetAppRolesAsync. Remove as soon frontend is adjusted")]
    public async IAsyncEnumerable<ClientRoles> GetClientRolesAsync(Guid appId, string? languageShortName = null)
    {
        var appRepository = _portalRepositories.GetInstance<IOfferRepository>();
        if (!await appRepository.CheckAppExistsById(appId).ConfigureAwait(false))
        {
            throw new NotFoundException($"app {appId} does not found");
        }

        if (languageShortName != null)
        {
            var language = await _portalRepositories.GetInstance<ILanguageRepository>().GetLanguageAsync(languageShortName);
            if (language == null)
            {
                throw new ArgumentException($"language {languageShortName} does not exist");
            }
        }
        await foreach (var roles in appRepository.GetClientRolesAsync(appId, languageShortName).ConfigureAwait(false))
        {
            yield return new ClientRoles(roles.RoleId, roles.Role, roles.Description);
        }
    }

    public async Task<CompanyUserDetails> GetOwnCompanyUserDetailsAsync(Guid companyUserId, string iamUserId)
    {
        var details = await _portalRepositories.GetInstance<IUserRepository>().GetOwnCompanyUserDetailsUntrackedAsync(companyUserId, iamUserId).ConfigureAwait(false);
        if (details == null)
        {
            throw new NotFoundException($"no company-user data found for user {companyUserId} in company of {iamUserId}");
        }
        return details;
    }

    public async Task<int> AddOwnCompanyUsersBusinessPartnerNumbersAsync(Guid companyUserId, IEnumerable<string> businessPartnerNumbers, string adminUserId)
    {
        if (businessPartnerNumbers.Any(businessPartnerNumber => businessPartnerNumber.Length > 20))
        {
            throw new ControllerArgumentException("businessPartnerNumbers must not exceed 20 characters", nameof(businessPartnerNumbers));
        }
        var user = await _portalRepositories.GetInstance<IUserRepository>().GetOwnCompanyUserWithAssignedBusinessPartnerNumbersUntrackedAsync(companyUserId, adminUserId).ConfigureAwait(false);
        if (user == null || user.UserEntityId == null)
        {
            throw new NotFoundException($"user {companyUserId} not found in company of {adminUserId}");
        }

        var businessPartnerRepository = _portalRepositories.GetInstance<IUserBusinessPartnerRepository>();
        await _provisioningManager.AddBpnAttributetoUserAsync(user.UserEntityId, businessPartnerNumbers).ConfigureAwait(false);
        foreach (var businessPartnerToAdd in businessPartnerNumbers.Except(user.AssignedBusinessPartnerNumbers))
        {
            businessPartnerRepository.CreateCompanyUserAssignedBusinessPartner(companyUserId, businessPartnerToAdd);
        }

        return await _portalRepositories.SaveAsync();
    }

    public Task<int> AddOwnCompanyUsersBusinessPartnerNumberAsync(Guid companyUserId, string businessPartnerNumber, string adminUserId) =>
        AddOwnCompanyUsersBusinessPartnerNumbersAsync(companyUserId, Enumerable.Repeat(businessPartnerNumber, 1), adminUserId);

    public async Task<CompanyUserDetails> GetOwnUserDetails(string iamUserId)
    {
        var details = await _portalRepositories.GetInstance<IUserRepository>().GetUserDetailsUntrackedAsync(iamUserId).ConfigureAwait(false);
        if (details == null)
        {
            throw new NotFoundException($"no company-user data found for user {iamUserId}");
        }
        return details;
    }

    public async Task<CompanyUserDetails> UpdateOwnUserDetails(Guid companyUserId, OwnCompanyUserEditableDetails ownCompanyUserEditableDetails, string iamUserId)
    {
        var userData = await _portalRepositories.GetInstance<IUserRepository>().GetUserWithCompanyIdpAsync(iamUserId).ConfigureAwait(false);
        if (userData == null)
        {
            throw new ArgumentOutOfRangeException($"iamUser {iamUserId} is not a shared idp user");
        }
        if (userData.CompanyUser.Id != companyUserId)
        {
            throw new ForbiddenException($"invalid companyUserId {companyUserId} for user {iamUserId}");
        }
        var companyUser = userData.CompanyUser;
        var iamIdpAlias = userData.IamIdpAlias;
        var userIdShared = await _provisioningManager.GetProviderUserIdForCentralUserIdAsync(iamIdpAlias, companyUser.IamUser!.UserEntityId).ConfigureAwait(false);
        if (userIdShared == null)
        {
            throw new NotFoundException($"no shared realm userid found for {companyUser.IamUser!.UserEntityId} in realm {iamIdpAlias}");
        }
        await _provisioningManager.UpdateSharedRealmUserAsync(
            iamIdpAlias,
            userIdShared,
            ownCompanyUserEditableDetails.FirstName ?? "",
            ownCompanyUserEditableDetails.LastName ?? "",
            ownCompanyUserEditableDetails.Email ?? "").ConfigureAwait(false);

        companyUser.Firstname = ownCompanyUserEditableDetails.FirstName;
        companyUser.Lastname = ownCompanyUserEditableDetails.LastName;
        companyUser.Email = ownCompanyUserEditableDetails.Email;
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
        return new CompanyUserDetails(
            companyUser.Id,
            companyUser.DateCreated,
            userData.BusinessPartnerNumbers,
            companyUser.Company!.Name,
            companyUser.CompanyUserStatusId,
            userData.AssignedRoles)
        {
            FirstName = companyUser.Firstname,
            LastName = companyUser.Lastname,
            Email = companyUser.Email
        };
    }

    public async Task<int> DeleteOwnUserAsync(Guid companyUserId, string iamUserId)
    {
        var iamIdpAliasAccountData = await _portalRepositories.GetInstance<IUserRepository>().GetSharedIdentityProviderUserAccountDataUntrackedAsync(iamUserId);
        if (iamIdpAliasAccountData == default)
        {
            throw new ConflictException($"iamUser {iamUserId} is not associated with any companyUser");
        }
        var (sharedIdpAlias, accountData) = iamIdpAliasAccountData;
        if (accountData.CompanyUserId != companyUserId)
        {
            throw new ForbiddenException($"invalid companyUserId {companyUserId} for user {iamUserId}");
        }
        await DeleteUserInternalAsync(sharedIdpAlias,accountData,companyUserId).ConfigureAwait(false);
        return await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    public async IAsyncEnumerable<Guid> DeleteOwnCompanyUsersAsync(IEnumerable<Guid> companyUserIds, string iamUserId)
    {
        var iamIdpAliasData = await _portalRepositories.GetInstance<IIdentityProviderRepository>().GetSharedIdentityProviderIamAliasDataUntrackedAsync(iamUserId);
        if (iamIdpAliasData == default)
        {
            throw new ConflictException($"iamUser {iamUserId} is not associated with any companyUser");
        }
        var (iamIdpAlias, adminUserId) = iamIdpAliasData;

        await foreach (var accountData in _portalRepositories.GetInstance<IUserRepository>().GetCompanyUserAccountDataUntrackedAsync(companyUserIds, adminUserId).ConfigureAwait(false))
        {
            var success = false;
            try
            {
                await DeleteUserInternalAsync(iamIdpAlias, accountData, adminUserId).ConfigureAwait(false);
                success = true;
            }
            catch (Exception e)
            {
                if (iamIdpAlias == null)
                {
                    _logger.LogError(e, "Error while deleting companyUser {companyUserId}", accountData.CompanyUserId);
                }
                else
                {
                    _logger.LogError(e, "Error while deleting companyUser {companyUserId} from shared idp {iamIdpAlias}", accountData.CompanyUserId, iamIdpAlias);
                }
            }
            if (success)
            {
                yield return accountData.CompanyUserId;
            }
        }
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    private async Task DeleteUserInternalAsync(string? sharedIdpAlias, CompanyUserAccountData accountData, Guid administratorId)
    {
        var (companyUserId, userEntityId, businessPartnerNumbers, roleIds, offerIds, invitationIds) = accountData;
        var userRepository = _portalRepositories.GetInstance<IUserRepository>();
        if (userEntityId != null)
        {
            await DeleteIamUserAsync(sharedIdpAlias, userEntityId, userRepository).ConfigureAwait(false);
        }
        userRepository.AttachAndModifyCompanyUser(companyUserId, companyUser =>
        {
            companyUser.CompanyUserStatusId = CompanyUserStatusId.DELETED;
            companyUser.LastEditorId = administratorId;
        });
        DeleteAssignedBusinessPartnerNumbers(businessPartnerNumbers, companyUserId);
        DeleteAssignedOffers(offerIds, companyUserId);
        DeleteAssignedUserRoles(roleIds, companyUserId);
        DeleteAssignedInvitations(invitationIds);
    }

    private async Task DeleteIamUserAsync(string? sharedIdpAlias, string userEntityId, IUserRepository userRepository)
    {
        if (sharedIdpAlias != null)
        {
            var userIdShared = await _provisioningManager.GetProviderUserIdForCentralUserIdAsync(sharedIdpAlias, userEntityId).ConfigureAwait(false);
            if (userIdShared != null)
            {
                await _provisioningManager.DeleteSharedRealmUserAsync(sharedIdpAlias, userIdShared).ConfigureAwait(false);
            }
        }
        await _provisioningManager.DeleteCentralRealmUserAsync(userEntityId).ConfigureAwait(false);
        userRepository.DeleteIamUser(userEntityId);
    }

    private void DeleteAssignedBusinessPartnerNumbers(IEnumerable<string> businessPartnerNumbers, Guid companyUserId)
    {
        if (businessPartnerNumbers.Any())
        {
            var userBusinessPartnerRepository = _portalRepositories.GetInstance<IUserBusinessPartnerRepository>();
            foreach (var businessPartnerNumber in businessPartnerNumbers)
            {
                userBusinessPartnerRepository.DeleteCompanyUserAssignedBusinessPartner(companyUserId, businessPartnerNumber);
            }
        }
    }

    private void DeleteAssignedOffers(IEnumerable<Guid>offerIds, Guid companyUserId)
    {
        if (offerIds.Any())
        {
            var offerRepository = _portalRepositories.GetInstance<IOfferRepository>();
            foreach (var offerId in offerIds)
            {
                offerRepository.DeleteAppFavourite(offerId, companyUserId);
            }
        }
    }

    private void DeleteAssignedUserRoles(IEnumerable<Guid> userRoleIds, Guid companyUserId)
    {
        if (userRoleIds.Any())
        {
            var userRolesRepository = _portalRepositories.GetInstance<IUserRolesRepository>();
            foreach (var assignedRole in userRoleIds)
            {
                userRolesRepository.DeleteCompanyUserAssignedRole(companyUserId, assignedRole);
            }
        }
    }

    private void DeleteAssignedInvitations(IEnumerable<Guid> invitationIds)
    {   
        if (invitationIds.Any())
        {
            var applicationRepository = _portalRepositories.GetInstance<IApplicationRepository>();
            foreach (var invitationId in invitationIds)
            {
                applicationRepository.DeleteInvitation(invitationId);
            }
        }
    }

    private async Task<bool> CanResetPassword(string userId)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

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

    public async Task<bool> ExecuteOwnCompanyUserPasswordReset(Guid companyUserId, string adminUserId)
    {
        var idpUserName = await _portalRepositories.GetInstance<IIdentityProviderRepository>().GetIdpCategoryIdByUserIdAsync(companyUserId, adminUserId).ConfigureAwait(false);
        if (idpUserName != null && !string.IsNullOrWhiteSpace(idpUserName.TargetIamUserId) && !string.IsNullOrWhiteSpace(idpUserName.IdpName))
        {
            if (await CanResetPassword(adminUserId).ConfigureAwait(false))
            {
                await _provisioningManager.ResetSharedUserPasswordAsync(idpUserName.IdpName, idpUserName.TargetIamUserId).ConfigureAwait(false);
                return true;
            }
            throw new ArgumentException($"cannot reset password more often than {_settings.PasswordReset.MaxNoOfReset} in {_settings.PasswordReset.NoOfHours} hours");
        }
        throw new NotFoundException($"Cannot identify companyId or shared idp : companyUserId {companyUserId} is not associated with the same company as adminUserId {adminUserId}");
    }

    public Task<Pagination.Response<CompanyAppUserDetails>> GetOwnCompanyAppUsersAsync(
        Guid appId, 
        string iamUserId, 
        int page, 
        int size, 
        string? firstName = null, 
        string? lastName = null, 
        string? email = null,
        string? roleName = null)
    {
        var appUsers = _portalRepositories.GetInstance<IUserRepository>().GetOwnCompanyAppUsersUntrackedAsync(
            appId, 
            iamUserId,
            firstName,
            lastName,
            email,
            roleName);

        return Pagination.CreateResponseAsync(
            page,
            size,
            15,
            (int skip, int take) => new Pagination.AsyncSource<CompanyAppUserDetails>(
                appUsers.CountAsync(),
                appUsers.OrderBy(companyUser => companyUser.Id)
                    .Skip(skip)
                    .Take(take)
                    .Select(companyUser => new CompanyAppUserDetails(
                        companyUser.Id,
                        companyUser.CompanyUserStatusId,
                        companyUser.UserRoles!.Where(userRole => userRole.Offer!.Id == appId).Select(userRole => userRole.UserRoleText))
                    {
                        FirstName = companyUser.Firstname,
                        LastName = companyUser.Lastname,
                        Email = companyUser.Email
                    }).AsAsyncEnumerable()));
    }

    public async Task<int> DeleteOwnUserBusinessPartnerNumbersAsync(Guid companyUserId, string businessPartnerNumber, string adminUserId)
    {
        var userBusinessPartnerRepository = _portalRepositories.GetInstance<IUserBusinessPartnerRepository>();

        var userWithBpn = await userBusinessPartnerRepository.GetOwnCompanyUserWithAssignedBusinessPartnerNumbersAsync(companyUserId, adminUserId, businessPartnerNumber).ConfigureAwait(false);
        
        if (userWithBpn == default)
        {
            throw new NotFoundException($"user {companyUserId} does not exist");
        }

        if (!userWithBpn.IsAssignedBusinessPartner)
        {
            throw new ForbiddenException($"businessPartnerNumber {businessPartnerNumber} is not assigned to user {companyUserId}");
        }

        if (userWithBpn.UserEntityId == null)
        {
            throw new Exception($"user {companyUserId} is not associated with a user in keycloak");
        }

        if (!userWithBpn.IsValidUser)
        {
            throw new ForbiddenException($"companyUserId {companyUserId} and adminUserId {adminUserId} do not belong to same company");
        }

        userBusinessPartnerRepository.DeleteCompanyUserAssignedBusinessPartner(companyUserId, businessPartnerNumber);
        
        await _provisioningManager.DeleteCentralUserBusinessPartnerNumberAsync(userWithBpn.UserEntityId, businessPartnerNumber).ConfigureAwait(false);

        return await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }
}
