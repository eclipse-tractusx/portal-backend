using CatenaX.NetworkServices.Administration.Service.Models;
using CatenaX.NetworkServices.Mailing.SendMail;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.Provisioning.DBAccess;
using CatenaX.NetworkServices.Provisioning.Library;
using CatenaX.NetworkServices.Provisioning.Library.Models;
using Microsoft.Extensions.Options;
using PasswordGenerator;
namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic
{
    public class InvitationBusinessLogic : IInvitationBusinessLogic
    {
        private readonly IProvisioningManager _provisioningManager;
        private readonly IProvisioningDBAccess _provisioningDBAccess;
        private readonly IPortalBackendDBAccess _portalDBAccess;
        private readonly IMailingService _mailingService;
        private readonly InvitationSettings _settings;
        private readonly ILogger<InvitationBusinessLogic> _logger;

        public InvitationBusinessLogic(
            IProvisioningManager provisioningManager,
            IProvisioningDBAccess provisioningDBAccess,
            IPortalBackendDBAccess portalDBAccess,
            IMailingService mailingService,
            IOptions<InvitationSettings> configuration,
            ILogger<InvitationBusinessLogic> logger
            )
        {
            _provisioningManager = provisioningManager;
            _provisioningDBAccess = provisioningDBAccess;
            _portalDBAccess = portalDBAccess;
            _mailingService = mailingService;
            _settings = configuration.Value;
            _logger = logger;
        }

        public async Task ExecuteInvitation(CompanyInvitationData invitationData)
        {
            if (String.IsNullOrWhiteSpace(invitationData.email))
            {
                throw new ArgumentException("email must not be empty", "email");
            }
            if (String.IsNullOrWhiteSpace(invitationData.organisationName))
            {
                throw new ArgumentException("organisationName must not be empty", "organisationName");
            }
            var idpName = await _provisioningManager.GetNextCentralIdentityProviderNameAsync().ConfigureAwait(false);

            await _provisioningManager.SetupSharedIdpAsync(idpName, invitationData.organisationName).ConfigureAwait(false);

            var password = new Password().Next();
            var centralUserId = await _provisioningManager.CreateSharedUserLinkedToCentralAsync(idpName, new UserProfile(
                    String.IsNullOrWhiteSpace(invitationData.userName) ? invitationData.email : invitationData.userName,
                    invitationData.email,
                    invitationData.organisationName
            ) {
                FirstName = invitationData.firstName,
                LastName = invitationData.lastName,
                Password = password
            }).ConfigureAwait(false);

            await _provisioningManager.AssignClientRolesToCentralUserAsync(centralUserId,_settings.InvitedUserInitialRoles).ConfigureAwait(false);

            var company = _portalDBAccess.CreateCompany(invitationData.organisationName);
            var application = _portalDBAccess.CreateCompanyApplication(company, CompanyApplicationStatusId.CREATED);
            var companyUser = _portalDBAccess.CreateCompanyUser(invitationData.firstName, invitationData.lastName, invitationData.email, company.Id, CompanyUserStatusId.ACTIVE);
            await foreach(var userRoleId in _portalDBAccess.GetUserRoleIdsUntrackedAsync(_settings.InvitedUserInitialRoles).ConfigureAwait(false))
            {
                _portalDBAccess.CreateCompanyUserAssignedRole(companyUser.Id, userRoleId);
            }
            _portalDBAccess.CreateInvitation(application.Id, companyUser);
            var identityprovider = _portalDBAccess.CreateSharedIdentityProvider(company);
            _portalDBAccess.CreateIamIdentityProvider(identityprovider, idpName);
            _portalDBAccess.CreateIamUser(companyUser, centralUserId);

            await _portalDBAccess.SaveAsync().ConfigureAwait(false);

            var mailParameters = new Dictionary<string, string>
            {
                { "password", password },
                { "companyname", invitationData.organisationName },
                { "url", $"{_settings.RegistrationAppAddress}"},
            };

            await _mailingService.SendMails(invitationData.email, mailParameters, new List<string> { "RegistrationTemplate", "PasswordForRegistrationTemplate" }).ConfigureAwait(false);
        }
    }
}
