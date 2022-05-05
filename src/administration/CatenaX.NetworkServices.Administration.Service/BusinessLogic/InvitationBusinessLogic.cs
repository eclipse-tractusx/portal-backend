using CatenaX.NetworkServices.Administration.Service.Models;
using CatenaX.NetworkServices.Mailing.SendMail;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.Provisioning.DBAccess;
using CatenaX.NetworkServices.Provisioning.Library;
using CatenaX.NetworkServices.Provisioning.Library.Models;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PasswordGenerator;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic
{
    public class InvitationBusinessLogic : IInvitationBusinessLogic
    {
        private readonly IProvisioningManager _provisioningManager;
        private readonly IProvisioningDBAccess _provisioningDBAccess;
        private readonly IPortalBackendDBAccess _portalDBAccess;
        private readonly IMailingService _mailingService;
        private readonly ILogger<InvitationBusinessLogic> _logger;
        private readonly string _registrationAppAddress;

        public InvitationBusinessLogic(
            IProvisioningManager provisioningManager,
            IProvisioningDBAccess provisioningDBAccess,
            IPortalBackendDBAccess portalDBAccess,
            IMailingService mailingService,
            ILogger<InvitationBusinessLogic> logger,
            IConfiguration configuration)
        {
            _provisioningManager = provisioningManager;
            _provisioningDBAccess = provisioningDBAccess;
            _portalDBAccess = portalDBAccess;
            _mailingService = mailingService;
            _logger = logger;
            _registrationAppAddress = configuration["RegistrationAppAddress"];
        }

        public async Task ExecuteInvitation(CompanyInvitationData invitationData)
        {
            var idpName = await _provisioningManager.GetNextCentralIdentityProviderNameAsync().ConfigureAwait(false);

            await _provisioningManager.SetupSharedIdpAsync(idpName, invitationData.organisationName).ConfigureAwait(false);

            var password = new Password().Next();
            var centralUserId = await _provisioningManager.CreateSharedUserLinkedToCentralAsync(idpName, new UserProfile(
                    invitationData.userName ?? invitationData.email,
                    invitationData.firstName,
                    invitationData.lastName,
                    invitationData.email,
                    password
            ), invitationData.organisationName).ConfigureAwait(false);

            await _provisioningManager.AssignInvitedUserInitialRoles(centralUserId).ConfigureAwait(false);

            var company = _portalDBAccess.CreateCompany(invitationData.organisationName);
            var application = _portalDBAccess.CreateCompanyApplication(company, CompanyApplicationStatusId.CREATED);
            var companyUser = _portalDBAccess.CreateCompanyUser(invitationData.firstName, invitationData.lastName, invitationData.email, company.Id);
            _portalDBAccess.CreateInvitation(application.Id, companyUser);
            var identityprovider = _portalDBAccess.CreateSharedIdentityProvider(company);
            _portalDBAccess.CreateIamIdentityProvider(identityprovider, idpName);
            _portalDBAccess.CreateIamUser(companyUser, centralUserId);

            await _portalDBAccess.SaveAsync().ConfigureAwait(false);

            var mailParameters = new Dictionary<string, string>
            {
                { "password", password },
                { "companyname", invitationData.organisationName },
                { "url", $"{_registrationAppAddress}"},
            };

            await _mailingService.SendMails(invitationData.email, mailParameters, new List<string> { "RegistrationTemplate", "PasswordForRegistrationTemplate" }).ConfigureAwait(false);
        }
    }
}
