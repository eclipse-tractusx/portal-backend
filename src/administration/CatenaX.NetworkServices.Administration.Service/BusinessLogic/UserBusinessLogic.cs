using CatenaX.NetworkServices.Administration.Service.Models;
using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.Mailing.SendMail;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.Provisioning.Library;
using CatenaX.NetworkServices.Provisioning.Library.Models;
using CatenaX.NetworkServices.Provisioning.DBAccess;
using Microsoft.Extensions.Options;
using PasswordGenerator;

namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic
{
    /// <summary>
    /// Implementation of <see cref="IUserBusinessLogic"/>.
    /// </summary>
    public class UserBusinessLogic : IUserBusinessLogic
    {
        private readonly IProvisioningManager _provisioningManager;
        private readonly IProvisioningDBAccess _provisioningDBAccess;
        private readonly IPortalBackendDBAccess _portalDBAccess;
        private readonly IPortalRepositories _portalRepositories;
        private readonly IUserRepository _userRepository;
        private readonly IMailingService _mailingService;
        private readonly ILogger<UserBusinessLogic> _logger;
        private readonly UserSettings _settings;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="provisioningManager">Provisioning Manager</param>
        /// <param name="provisioningDBAccess">Provisioning DBAccess</param>
        /// <param name="portalDBAccess">Portal DBAccess</param>
        /// <param name="mailingService">Mailing Service</param>
        /// <param name="logger">logger</param>
        /// <param name="settings">Settings</param>
        /// <param name="portalRepositories">Portal Repositories</param>
        public UserBusinessLogic(
            IProvisioningManager provisioningManager,
            IProvisioningDBAccess provisioningDBAccess,
            IPortalBackendDBAccess portalDBAccess,
            IPortalRepositories portalRepositories,
            IMailingService mailingService,
            ILogger<UserBusinessLogic> logger,
            IOptions<UserSettings> settings)
        {
            _provisioningManager = provisioningManager;
            _provisioningDBAccess = provisioningDBAccess;
            _portalDBAccess = portalDBAccess;
            _portalRepositories = portalRepositories;
            _userRepository = _portalRepositories.GetInstance<IUserRepository>();
            _mailingService = mailingService;
            _logger = logger;
            _settings = settings.Value;
            _portalRepositories = portalRepositories;
        }

        public async IAsyncEnumerable<string> CreateOwnCompanyUsersAsync(IEnumerable<UserCreationInfo> usersToCreate, string createdById)
        {
            var companyIdpData = await _portalDBAccess.GetCompanyNameIdpAliasUntrackedAsync(createdById);
            if (companyIdpData == null)
            {
                throw new ArgumentOutOfRangeException($"user {createdById} is not associated with any company");
            }
            if (companyIdpData.IdpAlias == null)
            {
                throw new ArgumentOutOfRangeException($"user {createdById} is not associated with any shared idp");
            }

            var clientId = _settings.Portal.KeyCloakClientID;

            var roles = usersToCreate
                    .SelectMany(user => user.Roles)
                    .Where(role => !String.IsNullOrWhiteSpace(role))
                    .Distinct();

            var companyRoleIds = await _portalDBAccess.GetUserRoleWithIdsUntrackedAsync(
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
                if (!companyRoleIds.ContainsKey(role))
                {
                    throw new ArgumentException($"invalid Role: {role}");
                }
            }

            var pwd = new Password();

            foreach (UserCreationInfo user in usersToCreate)
            {
                bool success = false;
                try
                {
                    var password = pwd.Next();
                    var centralUserId = await _provisioningManager.CreateSharedUserLinkedToCentralAsync(companyIdpData.IdpAlias, new UserProfile(
                        user.userName ?? user.eMail,
                        user.eMail,
                        companyIdpData.CompanyName
                    )
                    {
                        FirstName = user.firstName,
                        LastName = user.lastName,
                        Password = password,
                        BusinessPartnerNumber = companyIdpData.BusinessPartnerNumber
                    }).ConfigureAwait(false);

                    var validRoles = user.Roles.Where(role => !String.IsNullOrWhiteSpace(role));
                    if (validRoles.Count() > 0)
                    {
                        var clientRoleNames = new Dictionary<string, IEnumerable<string>>
                        {
                            { clientId, user.Roles }
                        };
                        await _provisioningManager.AssignClientRolesToCentralUserAsync(centralUserId, clientRoleNames).ConfigureAwait(false);
                    }

                    var companyUser = _portalDBAccess.CreateCompanyUser(user.firstName, user.lastName, user.eMail, companyIdpData.CompanyId, CompanyUserStatusId.ACTIVE);

                    foreach (var role in validRoles)
                    {
                        _portalDBAccess.CreateCompanyUserAssignedRole(companyUser.Id, companyRoleIds[role]);
                    }

                    _portalDBAccess.CreateIamUser(companyUser, centralUserId);

                    var inviteTemplateName = "PortalTemplate";
                    if (!string.IsNullOrWhiteSpace(user.Message))
                    {
                        inviteTemplateName = "PortalTemplateWithMessage";
                    }

                    var mailParameters = new Dictionary<string, string>
                    {
                        { "password", password },
                        { "companyname", companyIdpData.CompanyName },
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

                await _portalDBAccess.SaveAsync().ConfigureAwait(false);

                if (success)
                {
                    yield return user.eMail;
                }
            }
        }

        public IAsyncEnumerable<CompanyUserData> GetOwnCompanyUserDatasAsync(
            string adminUserId,
            Guid? companyUserId = null,
            string? userEntityId = null,
            string? firstName = null,
            string? lastName = null,
            string? email = null,
            CompanyUserStatusId? status = null)
        {
            if (!companyUserId.HasValue
                && String.IsNullOrWhiteSpace(userEntityId)
                && String.IsNullOrWhiteSpace(firstName)
                && String.IsNullOrWhiteSpace(lastName)
                && String.IsNullOrWhiteSpace(email)
                && !status.HasValue)
            {
                throw new ArgumentNullException("not all of userEntityId, companyUserId, firstName, lastName, email, status may be null");
            }
            return _portalDBAccess.GetCompanyUserDetailsUntrackedAsync(
                adminUserId,
                companyUserId,
                userEntityId,
                firstName,
                lastName,
                email,
                status);
        }

        public async IAsyncEnumerable<ClientRoles> GetClientRolesAsync(Guid appId, string? languageShortName = null)
        {

            var app = await _portalDBAccess.GetAppAssignedClientsAsync(appId).ConfigureAwait(false);
            if (app.Equals(Guid.Empty))
            {
                throw new NotFoundException($"app {appId} does not found");
            }

            if (languageShortName != null)
            {
                var language = await _portalDBAccess.GetLanguageAsync(languageShortName);
                if (language == null)
                {
                    throw new NotFoundException($"language {languageShortName} does not exist");
                }
            }
            await foreach (var roles in _portalDBAccess.GetClientRolesAsync(appId, languageShortName).ConfigureAwait(false))
            {
                yield return new ClientRoles(roles.RoleId, roles.Role, roles.Description);
            }
        }

        public async Task<CompanyUserDetails> GetOwnCompanyUserDetails(Guid companyUserId, string adminUserId)
        {
            var details = await _userRepository.GetOwnCompanyUserDetailsUntrackedAsync(companyUserId, adminUserId).ConfigureAwait(false);
            if (details == null)
            {
                throw new NotFoundException($"no company-user data found for user {companyUserId} in company of {adminUserId}");
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
            if (!await _provisioningManager.UpdateSharedRealmUserAsync(
                iamIdpAlias,
                userIdShared,
                ownCompanyUserEditableDetails.FirstName ?? "",
                ownCompanyUserEditableDetails.LastName ?? "",
                ownCompanyUserEditableDetails.Email ?? "").ConfigureAwait(false))
            {
                throw new Exception($"failed to update shared realm userid {userIdShared} in realm {iamIdpAlias}");
            }
            companyUser.Firstname = ownCompanyUserEditableDetails.FirstName;
            companyUser.Lastname = ownCompanyUserEditableDetails.LastName;
            companyUser.Email = ownCompanyUserEditableDetails.Email;
            await _portalRepositories.SaveAsync().ConfigureAwait(false);
            return new CompanyUserDetails(
                companyUser.Id,
                companyUser.DateCreated,
                userData.BusinessPartnerNumbers,
                companyUser.Company!.Name,
                companyUser.CompanyUserStatusId)
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
                throw new ArgumentOutOfRangeException($"iamUser {iamUserId} is not a shared idp user");
            }
            if (userData.CompanyUser.Id != companyUserId)
            {
                throw new ForbiddenException($"invalid companyUserId {companyUserId} for user {iamUserId}");
            }
            if (await DeleteUserInternalAsync(userData.CompanyUser, userData.IamIdpAlias).ConfigureAwait(false))
            {
                return await _portalRepositories.SaveAsync().ConfigureAwait(false);
            }
            return -1;
        }

        public async IAsyncEnumerable<Guid> DeleteOwnCompanyUsersAsync(IEnumerable<Guid> companyUserIds, string adminUserId)
        {
            var iamIdpAlias = await _portalDBAccess.GetSharedIdentityProviderIamAliasUntrackedAsync(adminUserId);
            if (iamIdpAlias == null)
            {
                throw new ArgumentOutOfRangeException($"iamUser {adminUserId} is not a shared idp user");
            }
            await foreach (var companyUser in _portalDBAccess.GetCompanyUserRolesIamUsersAsync(companyUserIds, adminUserId).ConfigureAwait(false))
            {
                var success = false;
                try
                {
                    success = await DeleteUserInternalAsync(companyUser, iamIdpAlias).ConfigureAwait(false);
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
            await _portalDBAccess.SaveAsync().ConfigureAwait(false);
        }

        private async Task<bool> DeleteUserInternalAsync(CompanyUser companyUser, string iamIdpAlias)
        {
            var userIdShared = await _provisioningManager.GetProviderUserIdForCentralUserIdAsync(iamIdpAlias, companyUser.IamUser!.UserEntityId).ConfigureAwait(false);
            if (userIdShared != null
                && (await _provisioningManager.DeleteSharedRealmUserAsync(iamIdpAlias, userIdShared).ConfigureAwait(false))
                && (await _provisioningManager.DeleteCentralRealmUserAsync(companyUser.IamUser!.UserEntityId).ConfigureAwait(false))) //TODO doesn't handle the case where user is both shared and own idp user
            {
                foreach (var assignedRole in companyUser.CompanyUserAssignedRoles)
                {
                    _portalDBAccess.RemoveCompanyUserAssignedRole(assignedRole);
                }
                _portalDBAccess.RemoveIamUser(companyUser.IamUser);
                companyUser.CompanyUserStatusId = CompanyUserStatusId.INACTIVE;
                return true;
            }
            return false;
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
            var idpUserName = await _portalDBAccess.GetIdpCategoryIdByUserIdAsync(companyUserId, adminUserId).ConfigureAwait(false);
            if (idpUserName != null && !string.IsNullOrWhiteSpace(idpUserName.TargetIamUserId) && !string.IsNullOrWhiteSpace(idpUserName.IdpName))
            {
                if (await CanResetPassword(adminUserId).ConfigureAwait(false))
                {
                    var updatedPassword = await _provisioningManager.ResetSharedUserPasswordAsync(idpUserName.IdpName, idpUserName.TargetIamUserId).ConfigureAwait(false);
                    if (!updatedPassword)
                    {
                        throw new Exception("password reset failed");
                    }
                    return updatedPassword;
                }
                throw new ArgumentException($"cannot reset password more often than {_settings.PasswordReset.MaxNoOfReset} in {_settings.PasswordReset.NoOfHours} hours");
            }
            throw new NotFoundException($"Cannot identify companyId or shared idp : companyUserId {companyUserId} is not associated with the same company as adminUserId {adminUserId}");
        }

        /// <inheritdoc/>
        public async Task<string> AddUserRoleAsync(Guid appId, UserRoleInfo userRoleInfo, string adminUserId)
        {
            var companyUser = await _portalRepositories.GetInstance<IUserRepository>().GetIdpUserByIdUntrackedAsync(userRoleInfo.CompanyUserId, adminUserId).ConfigureAwait(false);
            if (companyUser == null || string.IsNullOrWhiteSpace(companyUser.IdpName))
            {
                throw new NotFoundException($"Cannot identify companyId or shared idp : companyUserId {userRoleInfo.CompanyUserId} is not associated with the same company as adminUserId {adminUserId}");
            }
            if (string.IsNullOrWhiteSpace(companyUser.TargetIamUserId))
            {
                throw new NotFoundException($"User not found");
            }
            var iamClientId = await _portalRepositories.GetInstance<IAppRepository>().GetAppAssignedClientIdUntrackedAsync(appId).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(iamClientId))
            {
                throw new NotFoundException($"appId {appId} not found");
            }
            var roles = userRoleInfo.Roles.Where(role => !String.IsNullOrWhiteSpace(role)).Distinct();
            var clientRoleNames = new Dictionary<string, IEnumerable<string>>
                        {
                            { iamClientId, roles }
                        };
            var userRoleRepository = _portalRepositories.GetInstance<IUserRolesRepository>();
            var companyRoleIds = await userRoleRepository.GetUserRoleIdsUntrackedAsync(clientRoleNames).ToListAsync().ConfigureAwait(false);
            if (companyRoleIds.Count() == 0)
            {
                throw new NotFoundException($"User role not existing");
            }
            if (roles.Count() > 0)
            {
                await _provisioningManager.AssignClientRolesToCentralUserAsync(companyUser.TargetIamUserId, clientRoleNames).ConfigureAwait(false);
            }
            string message = string.Empty;
            foreach (var role in companyRoleIds)
            {
                if (!companyUser.RoleIds.Contains(role))
                {
                    userRoleRepository.CreateCompanyUserAssignedRole(userRoleInfo.CompanyUserId, role);
                    message = "user role added";
                }

            }
            await _portalRepositories.SaveAsync().ConfigureAwait(false);
            return string.IsNullOrWhiteSpace(message) ? "user role already added" : message;
        }
    }
}
