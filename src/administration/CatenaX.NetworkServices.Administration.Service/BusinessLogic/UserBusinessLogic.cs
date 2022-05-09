using CatenaX.NetworkServices.Administration.Service.Models;
using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.Mailing.SendMail;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.Provisioning.Library;
using CatenaX.NetworkServices.Provisioning.Library.Models;
using CatenaX.NetworkServices.Provisioning.DBAccess;
using Microsoft.Extensions.Options;
using PasswordGenerator;

namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic
{
    public class UserBusinessLogic : IUserBusinessLogic
    {
        private readonly IProvisioningManager _provisioningManager;
        private readonly IProvisioningDBAccess _provisioningDBAccess;
        private readonly IPortalBackendDBAccess _portalDBAccess;
        private readonly IMailingService _mailingService;
        private readonly ILogger<UserBusinessLogic> _logger;
        private readonly UserSettings _settings;

        public UserBusinessLogic(
            IProvisioningManager provisioningManager,
            IProvisioningDBAccess provisioningDBAccess,
            IPortalBackendDBAccess portalDBAccess,
            IMailingService mailingService,
            ILogger<UserBusinessLogic> logger,
            IOptions<UserSettings> settings)
        {
            _provisioningManager = provisioningManager;
            _provisioningDBAccess = provisioningDBAccess;
            _portalDBAccess = portalDBAccess;
            _mailingService = mailingService;
            _logger = logger;
            _settings = settings.Value;
        }

        public async Task<IEnumerable<string>> CreateTenantUsersAsync(IEnumerable<UserCreationInfo> usersToCreate, string tenant, string createdById)
        {
            var companyIds = _portalDBAccess.GetCompanyIdsForShardIdpAliasUntrackedAsync(tenant).GetAsyncEnumerator();
            if (!await companyIds.MoveNextAsync().ConfigureAwait(false))
            {
                throw new ArgumentException($"tenant {tenant} is not associated with a companyId");
            }
            var companyId = companyIds.Current;
            if (await companyIds.MoveNextAsync().ConfigureAwait(false))
            {
                throw new ArgumentOutOfRangeException($"tenant {tenant} is associated with more than a single company");
            }
            await companyIds.DisposeAsync().ConfigureAwait(false);
            return await CreateCompanyUsersAsync(usersToCreate, companyId, createdById).ConfigureAwait(false);
        }

        public async Task<IEnumerable<string>> CreateCompanyUsersAsync(IEnumerable<UserCreationInfo> usersToCreate, Guid companyId, string createdById)
        {
            var companyIdpData = await _portalDBAccess.GetCompanyNameIdpAliasUntrackedAsync(companyId, createdById);
            if (companyIdpData == null)
            {
                throw new ForbiddenException($"user {createdById} is not associated with company {companyId}");
            }
            if (companyIdpData.IdpAlias == null)
            {
                throw new ArgumentException($"company {companyId} is not associated with a shared idp");
            }
            var clientId = _settings.Portal.KeyCloakClientID;
            var pwd = new Password();
            List<string> userList = new List<string>();
            foreach (UserCreationInfo user in usersToCreate)
            {
                try
                {
                    var password = pwd.Next();
                    var centralUserId = await _provisioningManager.CreateSharedUserLinkedToCentralAsync(companyIdpData.IdpAlias, new UserProfile(
                        user.userName ?? user.eMail,
                        user.eMail,
                        companyIdpData.CompanyName
                    ) {
                        FirstName = user.firstName,
                        LastName = user.lastName,
                        Password = password,
                        BusinessPartnerNumber = companyIdpData.Bpn
                    }).ConfigureAwait(false);

                    if (!String.IsNullOrWhiteSpace(user.Role))
                    {
                        var clientRoleNames = new Dictionary<string, IEnumerable<string>>
                        {
                            { clientId, new [] { user.Role } }
                        };
                        await _provisioningManager.AssignClientRolesToCentralUserAsync(centralUserId, clientRoleNames).ConfigureAwait(false);
                    }

                    var companyUser = _portalDBAccess.CreateCompanyUser(user.firstName, user.lastName, user.eMail, companyId);
                    var iamUser = _portalDBAccess.CreateIamUser(companyUser, centralUserId);

                    userList.Add(user.eMail);

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
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Error while creating user {user.userName ?? user.eMail}");
                }
                await _portalDBAccess.SaveAsync().ConfigureAwait(false);
            }
            return userList;
        }

        public Task<IEnumerable<JoinedUserInfo>> GetUsersAsync(
            string? tenant,
            string? userId = null,
            string? providerUserId = null,
            string? userName = null,
            string? firstName = null,
            string? lastName = null,
            string? email = null)
        {
            if (String.IsNullOrWhiteSpace(tenant))
            {
                throw new ArgumentNullException("tenant must not be empty");
            }
            return _provisioningManager.GetJoinedUsersAsync(
                tenant,
                userId,
                providerUserId,
                userName,
                firstName,
                lastName,
                email
            );
        }

        public Task<IEnumerable<string>> GetAppRolesAsync(string? clientId)
        {
            if (String.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentNullException("clientId must not be empty");
            }
            return _provisioningManager.GetClientRolesAsync(clientId);
        }

        public async Task DeleteUserAsync(string? tenant, string? userId)
        {
            if (String.IsNullOrWhiteSpace(tenant))
            {
                throw new ArgumentNullException("tenant must not be empty");
            }
            if (String.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentNullException("userId must not be empty");
            }
            var userIdShared = await _provisioningManager.GetProviderUserIdForCentralUserIdAsync(userId);
            await _provisioningManager.DeleteSharedAndCentralUserAsync(tenant, userIdShared).ConfigureAwait(false);
        }

        public async Task<IEnumerable<string>> DeleteUsersAsync(UserIds? usersToDelete, string? tenant)
        {
            if (usersToDelete == null)
            {
                throw new ArgumentException("usersToDelete must not be null");
            }
            if (String.IsNullOrWhiteSpace(tenant))
            {
                throw new ArgumentException("tenant must not be empty");
            }
            return (await Task.WhenAll(usersToDelete.userIds.Select(async userId =>
            {
                try
                {
                    await _provisioningManager.DeleteSharedAndCentralUserAsync(tenant, userId).ConfigureAwait(false);
                    return userId;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Error while deleting user {userId}, {e.Message}");
                    return null!;
                }
            })).ConfigureAwait(false)).Where(userName => userName != null);
        }

        public async Task<bool> AddBpnAttributeAtRegistrationApprovalAsync(Guid? companyId)
        {
            if (!companyId.HasValue)
            {
                throw new ArgumentNullException("companyId must not be empty");
            }
            foreach (var tenant in await _portalDBAccess.GetIdpAliaseForCompanyIdUntrackedAsync(companyId.Value).ToListAsync().ConfigureAwait(false))
            {
                var usersToUpdate = (await _provisioningManager.GetJoinedUsersAsync(tenant).ConfigureAwait(false))
                    .Select(g => g.userId);
                try
                {
                    foreach (var userBpn in await _portalDBAccess.GetBpnForUsersUntrackedAsync(usersToUpdate).ToListAsync().ConfigureAwait(false))
                    {
                        await _provisioningManager.AddBpnAttributetoUserAsync(userBpn.userId, Enumerable.Repeat(userBpn.bpn, 1));
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Error while adding BPN attribute to {usersToUpdate}");
                }
            }
            return true;
        }

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
                    await _provisioningManager.AddBpnAttributetoUserAsync(user.userId, user.bpns).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Error while adding BPN attribute to {user.userId}");
                }
            }
            return true;
        }

        //TODO: full functionality is not yet delivered and currently the service is working with a submitted Json file
        public async Task<bool> PostRegistrationWelcomeEmailAsync(WelcomeData welcomeData)
        {
            var mailParameters = new Dictionary<string, string>
            {
                { "userName", welcomeData.userName },
                { "companyName", welcomeData.companyName },
                { "url", $"{_settings.Portal.BasePortalAddress}"},
            };

            await _mailingService.SendMails(welcomeData.email, mailParameters, new List<string> { "EmailRegistrationWelcomeTemplate" }).ConfigureAwait(false);

            return true;
        }

        public Task<bool> ResetUserPasswordAsync(string realm, string userId)
        {
            IEnumerable<string> requiredActions = new List<string>() { "UPDATE_PASSWORD" };
            return _provisioningManager.ResetUserPasswordAsync(realm, userId, requiredActions);
        }

        public async Task<bool> CanResetPassword(string userId)
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
    }
}
