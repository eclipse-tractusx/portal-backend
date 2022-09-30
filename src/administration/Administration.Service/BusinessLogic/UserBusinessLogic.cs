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
using Org.CatenaX.Ng.Portal.Backend.Framework.IO;
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
using PasswordGenerator;
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
    private readonly IProvisioningDBAccess _provisioningDBAccess;
    private readonly IPortalRepositories _portalRepositories;
    private readonly IUserRepository _userRepository;
    private readonly IMailingService _mailingService;
    private readonly ILogger<UserBusinessLogic> _logger;
    private readonly UserSettings _settings;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="provisioningManager">Provisioning Manager</param>
    /// <param name="userProvisioningService">User Provisioning Service</param>
    /// <param name="provisioningDBAccess">Provisioning DBAccess</param>
    /// <param name="mailingService">Mailing Service</param>
    /// <param name="logger">logger</param>
    /// <param name="settings">Settings</param>
    /// <param name="portalRepositories">Portal Repositories</param>
    public UserBusinessLogic(
        IProvisioningManager provisioningManager,
        IUserProvisioningService userProvisioningService,
        IProvisioningDBAccess provisioningDBAccess,
        IPortalRepositories portalRepositories,
        IMailingService mailingService,
        ILogger<UserBusinessLogic> logger,
        IOptions<UserSettings> settings)
    {
        _provisioningManager = provisioningManager;
        _userProvisioningService = userProvisioningService;
        _provisioningDBAccess = provisioningDBAccess;
        _portalRepositories = portalRepositories;
        _userRepository = _portalRepositories.GetInstance<IUserRepository>();
        _mailingService = mailingService;
        _logger = logger;
        _settings = settings.Value;
    }

    public async IAsyncEnumerable<string> CreateOwnCompanyUsersAsync(IEnumerable<UserCreationInfo> usersToCreate, string createdById)
    {
        var userRepository = _portalRepositories.GetInstance<IUserRepository>();
        var userRolesRepository = _portalRepositories.GetInstance<IUserRolesRepository>();

        var result = await userRepository.GetCompanyNameIdpAliaseUntrackedAsync(createdById, IdentityProviderCategoryId.KEYCLOAK_SHARED).ConfigureAwait(false);
        if (result == default)
        {
            throw new ArgumentOutOfRangeException($"user {createdById} is not associated with any company");
        }
        var (companyId, companyName, businessPartnerNumber, idpAliase) = result;
        if (companyName == null)
        {
            throw new Exception($"assertion failed: companyName of company {companyId} should never be null here");
        }
        var idpAlias = idpAliase.SingleOrDefault();
        if (idpAlias == null)
        {
            throw new ArgumentOutOfRangeException($"user {createdById} is not associated with any shared idp");
        }

        var clientId = _settings.Portal.KeyCloakClientID;

        var roles = usersToCreate
                .SelectMany(user => user.Roles)
                .Where(role => !String.IsNullOrWhiteSpace(role))
                .Distinct();

        var userRoleIds = await userRolesRepository.GetUserRoleWithIdsUntrackedAsync(
            clientId,
            roles
            )
            .ToDictionaryAsync(
                companyRoleWithId => companyRoleWithId.CompanyUserRoleText,
                companyRoleWithId => companyRoleWithId.CompanyUserRoleId
            )
            .ConfigureAwait(false);

        foreach (var role in roles)
        {
            if (!userRoleIds.ContainsKey(role))
            {
                throw new ArgumentException($"invalid Role: {role}");
            }
        }

        var pwd = new Password();

        var creatorId = await userRepository.GetCompanyUserIdForIamUserUntrackedAsync(createdById).ConfigureAwait(false);
        foreach (UserCreationInfo user in usersToCreate)
        {
            bool success = false;
            try
            {
                var password = pwd.Next();
                var centralUserId = await _provisioningManager.CreateSharedUserLinkedToCentralAsync(
                    idpAlias,
                    new UserProfile(
                        user.userName ?? user.eMail,
                        user.firstName,
                        user.lastName,
                        user.eMail,
                        password
                    ),
                    _provisioningManager.GetStandardAttributes(
                        alias: idpAlias,
                        organisationName: companyName,
                        businessPartnerNumber: businessPartnerNumber
                    )
                ).ConfigureAwait(false);

                var companyUser = userRepository.CreateCompanyUser(user.firstName, user.lastName, user.eMail, companyId, CompanyUserStatusId.ACTIVE, creatorId);

                var validRoles = user.Roles.Where(role => !String.IsNullOrWhiteSpace(role));
                if (validRoles.Count() > 0)
                {
                    var clientRoleNames = new Dictionary<string, IEnumerable<string>>
                    {
                        { clientId, validRoles }
                    };
                    var (_, assignedRoles) = (await _provisioningManager.AssignClientRolesToCentralUserAsync(centralUserId, clientRoleNames).ConfigureAwait(false)).Single();
                    foreach (var role in assignedRoles)
                    {
                        userRolesRepository.CreateCompanyUserAssignedRole(companyUser.Id, userRoleIds[role]);
                    }
                    if (assignedRoles.Count() < validRoles.Count())
                    {
                        //TODO change return-type of method to include role-assignment-error information if assignedRoles is not the same as validRoles
                        _logger.LogError($"invalid role data, client: {clientId}, [{String.Join(", ",validRoles.Except(assignedRoles))}] has not been assigned in keycloak");
                    }
                }

                userRepository.CreateIamUser(companyUser, centralUserId);

                var inviteTemplateName = "PortalTemplate";
                if (!string.IsNullOrWhiteSpace(user.Message))
                {
                    inviteTemplateName = "PortalTemplateWithMessage";
                }

                var mailParameters = new Dictionary<string, string>
                {
                    { "password", password },
                    { "companyname", companyName },
                    { "message", user.Message ?? "" },
                    { "nameCreatedBy", createdById },
                    { "url", _settings.Portal.BasePortalAddress },
                    { "username", user.eMail },
                };

                await _mailingService.SendMails(user.eMail, mailParameters, new List<string> { inviteTemplateName, "PasswordForPortalTemplate" }).ConfigureAwait(false);

                success = true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error while creating user {user.userName ?? user.eMail}");
            }

            await _portalRepositories.SaveAsync().ConfigureAwait(false);

            if (success)
            {
                yield return user.eMail;
            }
        }
    }

    public async Task<Guid> CreateOwnCompanyIdpUserAsync(Guid identityProviderId, UserCreationInfoIdp userCreationInfo, string iamUserId)
    {
        var companyNameIdpAliasData = await _userProvisioningService.GetCompanyNameIdpAliasData(identityProviderId, iamUserId).ConfigureAwait(false);
        var result = await _userProvisioningService.CreateOwnCompanyIdpUsersAsync(
                companyNameIdpAliasData,
                _settings.Portal.KeyCloakClientID,
                Enumerable.Repeat(userCreationInfo, 1).ToAsyncEnumerable())
            .FirstAsync()
            .ConfigureAwait(false);
        if(result.Error != null)
        {
            throw result.Error;
        }
        return result.CompanyUserId;
    }

    public ValueTask<IdentityProviderUserCreationStats> UploadOwnCompanyIdpUsersAsync(Guid identityProviderId, IFormFile document, string iamUserId, CancellationToken cancellationToken)
    {
        if (!document.ContentType.Equals("text/csv", StringComparison.OrdinalIgnoreCase))
        {
            throw new UnsupportedMediaTypeException($"Only contentType text/csv files are allowed.");
        }
        return UploadOwnCompanyIdpUsersInternalAsync(identityProviderId, document, iamUserId, cancellationToken);
    }

    private async ValueTask<IdentityProviderUserCreationStats> UploadOwnCompanyIdpUsersInternalAsync(Guid identityProviderId, IFormFile document, string iamUserId, CancellationToken cancellationToken)
    {
        var companyNameIdpAliasData = await _userProvisioningService.GetCompanyNameIdpAliasData(identityProviderId, iamUserId).ConfigureAwait(false);

        using var stream = document.OpenReadStream();
        var reader = new StreamReader(new CancellableStream(stream, cancellationToken), Encoding.UTF8);

        int numCreated = 0;
        var errors = new List<String>();
        int numLines = 0;

        try
        {
            await ValidateUploadOwnIdpUsersHeadersAsync(reader).ConfigureAwait(false);

            await foreach (var result in _userProvisioningService.CreateOwnCompanyIdpUsersAsync(
                companyNameIdpAliasData,
                _settings.Portal.KeyCloakClientID,
                ParseUploadOwnIdpUsersCSVLines(reader, companyNameIdpAliasData.IsShardIdp)))
            {
                numLines++;
                if (result.Error != null)
                {
                    errors.Add($"line: {numLines}, message: {result.Error.Message}");
                }
                else
                {
                    numCreated++;
                }
            }
        }
        catch(TaskCanceledException tce)
        {
            errors.Add($"line: {numLines}, message: {tce.Message}");
        }
        return new IdentityProviderUserCreationStats(numCreated, errors.Count, numLines, errors);
    }

    private static async ValueTask ValidateUploadOwnIdpUsersHeadersAsync(StreamReader reader)
    {
        var firstLine = await reader.ReadLineAsync().ConfigureAwait(false);
        if (firstLine == null)
        {
            throw new ControllerArgumentException("uploaded file contains no lines");
        }

        var headers = firstLine.Split(",").GetEnumerator();
        foreach (var csvHeader in new [] { "FirstName", "LastName", "Email", "ProviderUserName", "ProviderUserId", "Roles" })
        {
            if (!headers.MoveNext())
            {
                throw new ControllerArgumentException($"invalid format: expected '{csvHeader}', got ''");
            }
            if ((string)headers.Current != csvHeader)
            {
                throw new ControllerArgumentException($"invalid format: expected '{csvHeader}', got '{headers.Current}'");
            }
        }
    }

    private static async IAsyncEnumerable<UserCreationInfoIdp> ParseUploadOwnIdpUsersCSVLines(StreamReader reader, bool isSharedIdp)
    {
        var nextLine = await reader.ReadLineAsync().ConfigureAwait(false);

        while (nextLine != null)
        {
            var (firstName, lastName, email, providerUserName, providerUserId, roles) = ParseUploadOwnIdpUsersCSVLine(nextLine, isSharedIdp);
            yield return new UserCreationInfoIdp(firstName, lastName, email, roles, providerUserName, providerUserId);
            nextLine = await reader.ReadLineAsync().ConfigureAwait(false);
        }
    }

    private static (string FirstName, string LastName, string Email, string ProviderUserName, string ProviderUserId, IEnumerable<string> Roles) ParseUploadOwnIdpUsersCSVLine(string line, bool isSharedIdp)
    {
        var items = line.Split(",").AsEnumerable().GetEnumerator();
        if(!items.MoveNext() || string.IsNullOrWhiteSpace(items.Current))
        {
            throw new ControllerArgumentException($"value for FirstName type string expected");
        }
        var firstName = items.Current;
        if(!items.MoveNext() || string.IsNullOrWhiteSpace(items.Current))
        {
            throw new ControllerArgumentException($"value for LastName type string expected");
        }
        var lastName = items.Current;
        if(!items.MoveNext() || string.IsNullOrWhiteSpace(items.Current))
        {
            throw new ControllerArgumentException($"value for Email type string expected");
        }
        var email = items.Current;
        if(!items.MoveNext() || string.IsNullOrWhiteSpace(items.Current))
        {
            throw new ControllerArgumentException($"value for ProviderUserName type string expected");
        }
        var providerUserName = items.Current;
        if(!items.MoveNext() || (!isSharedIdp && string.IsNullOrWhiteSpace(items.Current)))
        {
            throw new ControllerArgumentException($"value for ProviderUserId type string expected");
        }
        var providerUserId = items.Current;
        var roles = ParseUploadOwnIdpUsersRoles(items).ToList();
        return (firstName, lastName, email, providerUserName, providerUserId, roles);
    }

    private static IEnumerable<string> ParseUploadOwnIdpUsersRoles(IEnumerator<string> items)
    {
        while (items.MoveNext())
        {
            if(string.IsNullOrWhiteSpace(items.Current))
            {
                throw new ControllerArgumentException($"value for Role type string expected");
            }
            yield return items.Current;
        }
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
        var details = await _userRepository.GetOwnCompanyUserDetailsUntrackedAsync(companyUserId, iamUserId).ConfigureAwait(false);
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
            throw new ArgumentException("businessPartnerNumbers must not exceed 20 characters");
        }
        var user = await _userRepository.GetOwnCompanyUserWithAssignedBusinessPartnerNumbersUntrackedAsync(companyUserId, adminUserId).ConfigureAwait(false);
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
        var details = await _userRepository.GetUserDetailsUntrackedAsync(iamUserId).ConfigureAwait(false);
        if (details == null)
        {
            throw new NotFoundException($"no company-user data found for user {iamUserId}");
        }
        return details;
    }

    public async Task<CompanyUserDetails> UpdateOwnUserDetails(Guid companyUserId, OwnCompanyUserEditableDetails ownCompanyUserEditableDetails, string iamUserId)
    {
        var userData = await _userRepository.GetUserWithCompanyIdpAsync(iamUserId).ConfigureAwait(false);
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
        var userData = await _userRepository.GetUserWithIdpAsync(iamUserId).ConfigureAwait(false);
        if (userData == null)
        {
            throw new NotFoundException($"iamUser {iamUserId} is not a shared idp user");
        }
        if (userData.CompanyUser.Id != companyUserId)
        {
            throw new ForbiddenException($"invalid companyUserId {companyUserId} for user {iamUserId}");
        }
        await DeleteUserInternalAsync(userData.CompanyUser, userData.IamIdpAlias).ConfigureAwait(false);

        return await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    public async IAsyncEnumerable<Guid> DeleteOwnCompanyUsersAsync(IEnumerable<Guid> companyUserIds, string adminUserId)
    {
        var iamIdpAlias = await _portalRepositories.GetInstance<IIdentityProviderRepository>().GetSharedIdentityProviderIamAliasUntrackedAsync(adminUserId);
        if (iamIdpAlias == null)
        {
            throw new NotFoundException($"iamUser {adminUserId} is not a shared idp user");
        }

        await foreach (var companyUser in _portalRepositories.GetInstance<IUserRolesRepository>().GetCompanyUserRolesIamUsersAsync(companyUserIds, adminUserId).ConfigureAwait(false))
        {
            var success = false;
            try
            {
                await DeleteUserInternalAsync(companyUser, iamIdpAlias).ConfigureAwait(false);
                success = true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error while deleting comapnyUser {companyUser.Id} from shared idp {iamIdpAlias}");
            }
            if (success)
            {
                yield return companyUser.Id;
            }
        }
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    private async Task DeleteUserInternalAsync(CompanyUser companyUser, string iamIdpAlias)
    {
        var userIdShared = await _provisioningManager.GetProviderUserIdForCentralUserIdAsync(iamIdpAlias, companyUser.IamUser!.UserEntityId).ConfigureAwait(false);
        if (userIdShared == null)
        {
            throw new UnexpectedConditionException($"user {companyUser.IamUser!.UserEntityId} not found in central idp");
        }
        await _provisioningManager.DeleteSharedRealmUserAsync(iamIdpAlias, userIdShared).ConfigureAwait(false);
        await _provisioningManager.DeleteCentralRealmUserAsync(companyUser.IamUser!.UserEntityId).ConfigureAwait(false); //TODO doesn't handle the case where user is both shared and own idp user

        var userRolesRepository = _portalRepositories.GetInstance<IUserRolesRepository>();
        foreach (var assignedRole in companyUser.CompanyUserAssignedRoles)
        {
            userRolesRepository.RemoveCompanyUserAssignedRole(assignedRole);
        }
        _portalRepositories.GetInstance<IUserRepository>().RemoveIamUser(companyUser.IamUser);
        companyUser.CompanyUserStatusId = CompanyUserStatusId.INACTIVE;
    }

    [Obsolete]
    public async Task<bool> AddBpnAttributeAsync(IEnumerable<UserUpdateBpn>? usersToUdpatewithBpn)
    {
        if (usersToUdpatewithBpn == null)
        {
            throw new ArgumentNullException("usersToUpdatewithBpn must not be null");
        }
        foreach (UserUpdateBpn user in usersToUdpatewithBpn)
        {
            try
            {
                await _provisioningManager.AddBpnAttributetoUserAsync(user.UserId, user.BusinessPartnerNumbers).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error while adding BPN attribute to {user.UserId}");
            }
        }
        return true;
    }

    private async Task<bool> CanResetPassword(string userId)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        var userInfo = (await _provisioningDBAccess.GetUserPasswordResetInfo(userId).ConfigureAwait(false))
            ?? _provisioningDBAccess.CreateUserPasswordResetInfo(userId, now, 0);

        if (now < userInfo.PasswordModifiedAt.AddHours(_settings.PasswordReset.NoOfHours))
        {
            if (userInfo.ResetCount < _settings.PasswordReset.MaxNoOfReset)
            {
                userInfo.ResetCount++;
                await _provisioningDBAccess.SaveAsync().ConfigureAwait(false);
                return true;
            }
        }
        else
        {
            userInfo.ResetCount = 1;
            userInfo.PasswordModifiedAt = now;
            await _provisioningDBAccess.SaveAsync().ConfigureAwait(false);
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

    public async Task<UserRoleMessage> AddUserRoleAsync(Guid appId, UserRoleInfo userRoleInfo, string adminUserId)
    {
        var companyUser = await _portalRepositories.GetInstance<IUserRepository>()
            .GetIdpUserByIdUntrackedAsync(userRoleInfo.CompanyUserId, adminUserId)
            .ConfigureAwait(false);
        if (companyUser == null || string.IsNullOrWhiteSpace(companyUser.IdpName))
        {
            throw new NotFoundException($"Cannot identify companyId or shared idp : companyUserId {userRoleInfo.CompanyUserId} is not associated with the same company as adminUserId {adminUserId}");
        }
        
        if (string.IsNullOrWhiteSpace(companyUser.TargetIamUserId))
        {
            throw new NotFoundException($"User not found");
        }
        
        var iamClientId = await _portalRepositories.GetInstance<IOfferRepository>().GetAppAssignedClientIdUntrackedAsync(appId, companyUser.CompanyId).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(iamClientId))
        {
            throw new ArgumentException($"invalid appId {appId}", nameof(appId));
        }
        var roles = userRoleInfo.Roles.Where(role => !string.IsNullOrWhiteSpace(role)).Distinct().ToList();

        var success = new List<UserRoleMessage.Message>();
        var warning = new List<UserRoleMessage.Message>();

        if (!roles.Any()) return new UserRoleMessage(success, warning);

        var userRoleRepository = _portalRepositories.GetInstance<IUserRolesRepository>();
        var userRoleWithIds = await userRoleRepository.GetUserRoleWithIdsUntrackedAsync(iamClientId, roles).ToListAsync().ConfigureAwait(false);
        if (userRoleWithIds.Count != roles.Count)
        {
            throw new ArgumentException($"invalid User roles for client {iamClientId}: [{string.Join(",", roles.Except(userRoleWithIds.Select(x => x.CompanyUserRoleText)))}]", nameof(userRoleInfo));
        }
        var unassignedRoleNames = userRoleWithIds
            .Where(x => !companyUser.RoleIds.Contains(x.CompanyUserRoleId))
            .Select(x => x.CompanyUserRoleText)
            .ToList();

        var clientRoleNames = new Dictionary<string, IEnumerable<string>>
        {
            { iamClientId, unassignedRoleNames }
        };

        var (_, assignedRoleNames) = (await _provisioningManager.AssignClientRolesToCentralUserAsync(companyUser.TargetIamUserId, clientRoleNames).ConfigureAwait(false)).Single();

        foreach (var roleWithId in userRoleWithIds)
        {
            if (assignedRoleNames.Contains(roleWithId.CompanyUserRoleText))
            {
                userRoleRepository.CreateCompanyUserAssignedRole(userRoleInfo.CompanyUserId, roleWithId.CompanyUserRoleId);
                success.Add(new UserRoleMessage.Message(roleWithId.CompanyUserRoleText, UserRoleMessage.Detail.ROLE_ADDED));
            }
            else if (unassignedRoleNames.Contains(roleWithId.CompanyUserRoleText))
            {
                warning.Add(new UserRoleMessage.Message(roleWithId.CompanyUserRoleText, UserRoleMessage.Detail.ROLE_DOESNT_EXIST));
            }
            else
            {
                success.Add(new UserRoleMessage.Message(roleWithId.CompanyUserRoleText, UserRoleMessage.Detail.ROLE_ALREADY_ADDED));
            }
        }
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
        return new UserRoleMessage(success, warning);
    }

    public async Task<int> DeleteOwnUserBusinessPartnerNumbersAsync(Guid companyUserId, string businessPartnerNumber, string adminUserId)
    {
        var userBusinessPartnerRepository = _portalRepositories.GetInstance<IUserBusinessPartnerRepository>();

        var userWithBpn = await userBusinessPartnerRepository.GetOwnCompanyUserWithAssignedBusinessPartnerNumbersAsync(companyUserId, adminUserId, businessPartnerNumber).ConfigureAwait(false);
        
        if (userWithBpn == default)
        {
            throw new NotFoundException($"user {companyUserId} does not exist");
        }

        if (userWithBpn.AssignedBusinessPartner == null)
        {
            throw new NotFoundException($"businessPartnerNumber {businessPartnerNumber} is not assigned to user {companyUserId}");
        }

        if (userWithBpn.UserEntityId == null)
        {
            throw new Exception($"user {companyUserId} is not associated with a user in keycloak");
        }

        if (!userWithBpn.IsValidUser)
        {
            throw new ForbiddenException($"companyUserId {companyUserId} and adminUserId {adminUserId} do not belong to same company");
        }

        userBusinessPartnerRepository.RemoveCompanyUserAssignedBusinessPartner(userWithBpn.AssignedBusinessPartner);
        
        await _provisioningManager.DeleteCentralUserBusinessPartnerNumberAsync(userWithBpn.UserEntityId, userWithBpn.AssignedBusinessPartner.BusinessPartnerNumber).ConfigureAwait(false);

        return await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }
}
