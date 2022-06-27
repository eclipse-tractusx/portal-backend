using CatenaX.NetworkServices.Administration.Service.Models;
using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.Mailing.SendMail;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.Provisioning.Library;
using CatenaX.NetworkServices.Provisioning.Library.Models;
using Microsoft.Extensions.Options;
using PasswordGenerator;
namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic
{
    public class InvitationBusinessLogic : IInvitationBusinessLogic
    {
        private readonly IProvisioningManager _provisioningManager;
        private readonly IPortalRepositories _portalRepositories;
        private readonly IMailingService _mailingService;
        private readonly InvitationSettings _settings;
        private readonly ILogger<InvitationBusinessLogic> _logger;

        public InvitationBusinessLogic(
            IProvisioningManager provisioningManager,
            IPortalRepositories portalRepositories,
            IMailingService mailingService,
            IOptions<InvitationSettings> configuration,
            ILogger<InvitationBusinessLogic> logger
            )
        {
            _provisioningManager = provisioningManager;
            _portalRepositories = portalRepositories;
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

            try
            {
                await _provisioningManager.AssignClientRolesToCentralUserAsync(centralUserId, _settings.InvitedUserInitialRoles).ConfigureAwait(false);
            }
            catch(NotFoundException nfe)
            {
                throw new Exception("invalid configuration, configured roles do not exist in keycloak", nfe);
            }

            var applicationRepository = _portalRepositories.GetInstance<IApplicationRepository>();
            var userRepository = _portalRepositories.GetInstance<IUserRepository>();
            var userRolesRepository = _portalRepositories.GetInstance<IUserRolesRepository>();
            var identityProviderRepository = _portalRepositories.GetInstance<IIdentityProviderRepository>();

            var company = _portalRepositories.GetInstance<ICompanyRepository>().CreateCompany(invitationData.organisationName);
            var application = applicationRepository.CreateCompanyApplication(company, CompanyApplicationStatusId.CREATED);
            var companyUser = userRepository.CreateCompanyUser(invitationData.firstName, invitationData.lastName, invitationData.email, company.Id, CompanyUserStatusId.ACTIVE);
            await foreach(var userRoleId in userRolesRepository.GetUserRoleIdsUntrackedAsync(_settings.InvitedUserInitialRoles).ConfigureAwait(false))
            {
                userRolesRepository.CreateCompanyUserAssignedRole(companyUser.Id, userRoleId);
            }
            applicationRepository.CreateInvitation(application.Id, companyUser);
            var identityprovider = identityProviderRepository.CreateSharedIdentityProvider(company);
            identityProviderRepository.CreateIamIdentityProvider(identityprovider, idpName);
            userRepository.CreateIamUser(companyUser, centralUserId);

            await _portalRepositories.SaveAsync().ConfigureAwait(false);

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
