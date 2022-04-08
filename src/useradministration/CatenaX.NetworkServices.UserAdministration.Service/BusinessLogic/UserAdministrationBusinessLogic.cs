using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using PasswordGenerator;

using CatenaX.NetworkServices.Mailing.SendMail;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.Provisioning.Library;
using CatenaX.NetworkServices.Provisioning.Library.Models;
using CatenaX.NetworkServices.Provisioning.DBAccess;
using CatenaX.NetworkServices.UserAdministration.Service.Models;

namespace CatenaX.NetworkServices.UserAdministration.Service.BusinessLogic
{
    public class UserAdministrationBusinessLogic : IUserAdministrationBusinessLogic
    {
        private readonly IProvisioningManager _provisioningManager;
        private readonly IProvisioningDBAccess _provisioningDBAccess;
        private readonly IPortalBackendDBAccess _portalDBAccess;
        private readonly IMailingService _mailingService;
        private readonly ILogger<UserAdministrationBusinessLogic> _logger;
        private readonly UserAdministrationSettings _settings;

        public UserAdministrationBusinessLogic(
            IProvisioningManager provisioningManager,
            IProvisioningDBAccess provisioningDBAccess,
            IPortalBackendDBAccess portalDBAccess,
            IMailingService mailingService,
            ILogger<UserAdministrationBusinessLogic> logger,
            IOptions<UserAdministrationSettings> settings)
        {
            _provisioningManager = provisioningManager;
            _provisioningDBAccess = provisioningDBAccess;
            _portalDBAccess = portalDBAccess;
            _mailingService = mailingService;
            _logger = logger;
            _settings = settings.Value;
        }

        public async Task<bool> ExecuteInvitation(CompanyInvitationData invitationData)
        {
            var idpName = await _provisioningManager.GetNextCentralIdentityProviderNameAsync().ConfigureAwait(false);
            if (idpName == null) return false;
            
            if (! await _provisioningManager.SetupSharedIdpAsync(idpName, invitationData.organisationName).ConfigureAwait(false)) return false;
            
            var password = new Password().Next();
            var centralUserId = await _provisioningManager.CreateSharedUserLinkedToCentralAsync(idpName, new UserProfile {
                    UserName = invitationData.userName ?? invitationData.email,
                    FirstName = invitationData.firstName,
                    LastName = invitationData.lastName,
                    Email = invitationData.email,
                    Password = password
                }, invitationData.organisationName).ConfigureAwait(false);

            if (centralUserId == null) return false;

            if (!await _provisioningManager.AssignInvitedUserInitialRoles(centralUserId).ConfigureAwait(false)) return false;

            var company = _portalDBAccess.CreateCompany(invitationData.organisationName);
            var application = _portalDBAccess.CreateCompanyApplication(company);
            var companyUser = _portalDBAccess.CreateCompanyUser(invitationData.firstName, invitationData.lastName, invitationData.email, company.Id);
            _portalDBAccess.CreateInvitation(application.Id, companyUser);
            var identityprovider = _portalDBAccess.CreateSharedIdentityProvider(company);
            _portalDBAccess.CreateIamIdentityProvider(identityprovider,idpName);
            _portalDBAccess.CreateIamUser(companyUser,centralUserId);
          
            await _portalDBAccess.SaveAsync().ConfigureAwait(false);

            var mailParameters = new Dictionary<string, string>
            {
                { "password", password },
                { "companyname", invitationData.organisationName },
                { "url", $"{_settings.RegistrationBasePortalAddress}"},
            };

            await _mailingService.SendMails(invitationData.email, mailParameters, new List<string> { "RegistrationTemplate", "PasswordForRegistrationTemplate"} );

            return true;
        }

        public async Task<IEnumerable<string>> CreateUsersAsync(IEnumerable<UserCreationInfo> usersToCreate, string tenant, string createdByName)
        {
            var idpName = tenant;
            var organisationName = await _provisioningManager.GetOrganisationFromCentralIdentityProviderMapperAsync(idpName).ConfigureAwait(false);
            var clientId = _settings.Portal.KeyCloakClientID;
            var pwd = new Password();
            List<string> userList = new List<string>();
            foreach (UserCreationInfo user in usersToCreate)
            {
                try
                {
                    var password = pwd.Next();
                    var centralUserId = await _provisioningManager.CreateSharedUserLinkedToCentralAsync(idpName, new UserProfile {
                        UserName = user.userName ?? user.eMail,
                        FirstName = user.firstName,
                        LastName = user.lastName,
                        Email = user.eMail,
                        Password = password
                    }, organisationName).ConfigureAwait(false);

                    if (centralUserId == null) continue;

                    var clientRoleNames = new Dictionary<string, IEnumerable<string>>
                    {
                        { clientId, new []{user.Role}}
                    };

                    if (!await _provisioningManager.AssignClientRolesToCentralUserAsync(centralUserId, clientRoleNames).ConfigureAwait(false)) continue;

                    // TODO: revaluate try...catch as soon as BPN can be found at UserCreation
                    try
                    {
                        var bpn = _portalDBAccess.GetBpnForUserUntrackedAsync(centralUserId).ToEnumerable();
                        if (!await _provisioningManager.AddBpnAttributetoUserAsync(centralUserId, bpn).ConfigureAwait(false)) continue;
                    }
                    catch (InvalidOperationException e)
                    {
                        _logger.LogInformation(e, "BPN not found, will continue without");
                    }

                    var inviteTemplateName = "PortalTemplate";
                    if (!string.IsNullOrWhiteSpace(user.Message))
                    { 
                        inviteTemplateName = "PortalTemplateWithMessage";
                    }

                    var mailParameters = new Dictionary<string, string>
                    {
                        { "password", password },
                        { "companyname", organisationName },
                        { "message", user.Message },
                        { "nameCreatedBy", createdByName},
                        { "url", $"{_settings.Portal.BasePortalAddress}"},
                        { "username", user.eMail},
                    
                    };

                    await _mailingService.SendMails(user.eMail, mailParameters, new List<string> { inviteTemplateName, "PasswordForPortalTemplate" }).ConfigureAwait(false);

                    userList.Add(user.eMail);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Error while creating user {user.userName ?? user.eMail}");
                }
            }
            return userList;
        }

        public Task<IEnumerable<JoinedUserInfo>> GetUsersAsync(
            string tenant,
            string userId = null,
            string providerUserId = null,
            string userName = null,
            string firstName = null,
            string lastName = null,
            string email = null
        ) => _provisioningManager.GetJoinedUsersAsync(
                tenant,
                userId,
                providerUserId,
                userName,
                firstName,
                lastName,
                email
            );

        public Task<IEnumerable<string>> GetAppRolesAsync(string clientId) =>
            _provisioningManager.GetClientRolesAsync(clientId);

        public async Task<bool> DeleteUserAsync(string tenant, string userId)
        {
            try
            {
                var userIdShared = await _provisioningManager.GetProviderUserIdForCentralUserIdAsync(userId);
                return await _provisioningManager.DeleteSharedAndCentralUserAsync(tenant, userIdShared).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while deleting user");
                return false;
            }
        }

        public async Task<IEnumerable<string>> DeleteUsersAsync(UserIds usersToDelete, string tenant) =>
            (await Task.WhenAll(usersToDelete.userIds.Select(async userId => { 
                try {
                    return await _provisioningManager.DeleteSharedAndCentralUserAsync(tenant, userId).ConfigureAwait(false) ? userId : null;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Error while deleting user {userId}");
                    return null;
                }
            }))).Where(userName => userName != null);

        public async Task<bool> AddBpnAttributeAtRegistrationApprovalAsync(Guid companyId)
        {
            await foreach (var tenant in _portalDBAccess.GetIdpAliaseForCompanyIdUntrackedAsync(companyId))
            {
                var usersToUdpate = (await _provisioningManager.GetJoinedUsersAsync(tenant).ConfigureAwait(false))
                    .Select(g => g.userId);
                foreach (var userId in usersToUdpate)
                {
                    try
                    {
                        var bpns = _portalDBAccess.GetBpnForUserUntrackedAsync(userId).ToEnumerable();
                        await _provisioningManager.AddBpnAttributetoUserAsync(userId, bpns);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, $"Error while adding BPN attribute to {userId}");
                    }
                }
            }
            return true;
        }

        public async Task<bool> AddBpnAttributeAsync(IEnumerable<UserUpdateBpn> usersToUdpatewithBpn)
        {
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
    }
}
