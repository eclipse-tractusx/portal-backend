using CatenaX.NetworkServices.Consent.Library.Data;
using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.Mailing.SendMail;
using CatenaX.NetworkServices.Provisioning.Library;
using CatenaX.NetworkServices.Provisioning.Library.Models;
using CatenaX.NetworkServices.Registration.Service.BPN;
using CatenaX.NetworkServices.Registration.Service.BPN.Model;
using CatenaX.NetworkServices.Registration.Service.Custodian;
using CatenaX.NetworkServices.Registration.Service.Model;
using CatenaX.NetworkServices.Registration.Service.RegistrationAccess;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PasswordGenerator;

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CatenaX.NetworkServices.Registration.Service.BusinessLogic
{
    public class RegistrationBusinessLogic : IRegistrationBusinessLogic
    {
        private readonly RegistrationSettings _settings;
        private readonly IRegistrationDBAccess _dbAccess;
        private readonly IMailingService _mailingService;
        private readonly IBPNAccess _bpnAccess;
        private readonly ICustodianService _custodianService;
        private readonly IProvisioningManager _provisioningManager;
        private readonly IPortalBackendDBAccess _portalDBAccess;
        private readonly ILogger<RegistrationBusinessLogic> _logger;

        public RegistrationBusinessLogic(IOptions<RegistrationSettings> settings, IRegistrationDBAccess registrationDBAccess, IMailingService mailingService, IBPNAccess bpnAccess, ICustodianService custodianService, IProvisioningManager provisioningManager, IPortalBackendDBAccess portalDBAccess, ILogger<RegistrationBusinessLogic> logger)
        {
            _settings = settings.Value;
            _dbAccess = registrationDBAccess;
            _mailingService = mailingService;
            _bpnAccess = bpnAccess;
            _custodianService = custodianService;
            _provisioningManager = provisioningManager;
            _portalDBAccess = portalDBAccess;
            _logger = logger;
        }

        public async Task<IEnumerable<string>> CreateUsersAsync(List<UserCreationInfo>? usersToCreate, string? tenant, string? createdByName)
        {
            if (usersToCreate == null)
            {
                throw new ArgumentNullException("usersToCreate must not be null");
            }
            if (String.IsNullOrWhiteSpace(tenant))
            {
                throw new ArgumentNullException("tenant must not be empty");
            }
            if (String.IsNullOrWhiteSpace(createdByName))
            {
                throw new ArgumentNullException("createdByName must not be empty");
            }
            var idpName = tenant;
            var organisationName = await _provisioningManager.GetOrganisationFromCentralIdentityProviderMapperAsync(idpName).ConfigureAwait(false);
            var clientId = _settings.KeyCloakClientID;
            var pwd = new Password();
            List<string> userList = new List<string>();
            foreach (UserCreationInfo user in usersToCreate)
            {
                try
                {
                    var password = pwd.Next();
                    var centralUserId = await _provisioningManager.CreateSharedUserLinkedToCentralAsync(idpName, new UserProfile(
                        user.userName ?? user.eMail,
                        user.firstName,
                        user.lastName,
                        user.eMail,
                        password
                    ), organisationName).ConfigureAwait(false);

                    var clientRoleNames = new Dictionary<string, IEnumerable<string>>
                    {
                        { clientId, new []{user.Role}}
                    };

                    await _provisioningManager.AssignClientRolesToCentralUserAsync(centralUserId, clientRoleNames).ConfigureAwait(false);

                    var inviteTemplateName = "invite";
                    if (!String.IsNullOrWhiteSpace(user.Message))
                    {
                        inviteTemplateName = "inviteWithMessage";
                    }

                    var mailParameters = new Dictionary<string, string>
                    {
                        { "password", password },
                        { "companyname", organisationName },
                        { "message", user.Message },
                        { "nameCreatedBy", createdByName},
                        { "url", $"{_settings.BasePortalAddress}"},
                        { "username", user.eMail},

                    };

                    await _mailingService.SendMails(user.eMail, mailParameters, new List<string> { inviteTemplateName, "password" }).ConfigureAwait(false);

                    userList.Add(user.eMail);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error while creating user");
                }
            }
            return userList;
        }

        public Task<IEnumerable<string>> GetClientRolesCompositeAsync() =>
            _provisioningManager.GetClientRolesCompositeAsync(_settings.KeyCloakClientID);

        public Task<List<FetchBusinessPartnerDto>> GetCompanyByIdentifierAsync(string companyIdentifier, string token) =>
            _bpnAccess.FetchBusinessPartner(companyIdentifier, token);

        public Task<IEnumerable<CompanyRole>> GetCompanyRolesAsync() =>
            _dbAccess.GetAllCompanyRoles();

        public Task<IEnumerable<ConsentForCompanyRole>> GetConsentForCompanyRoleAsync(int roleId) =>
            _dbAccess.GetConsentForCompanyRole(roleId);

        public Task SetCompanyRolesAsync(CompanyToRoles rolesToSet) =>
            _dbAccess.SetCompanyRoles(rolesToSet);

        public Task SetIdpAsync(SetIdp idpToSet) =>
            _dbAccess.SetIdp(idpToSet);

        public Task SignConsentAsync(SignConsentRequest signedConsent) =>
            _dbAccess.SignConsent(signedConsent);

        public Task<IEnumerable<SignedConsent>> SignedConsentsByCompanyIdAsync(string companyId) =>
            _dbAccess.SignedConsentsByCompanyId(companyId);

        public async Task CreateDocument(IFormFile document, string userName)
        {
            var name = document.FileName;
            var documentContent = "";
            var hash = "";
            using (var ms = new MemoryStream())
            {
                document.CopyTo(ms);
                var fileBytes = ms.ToArray();
                documentContent = Convert.ToBase64String(fileBytes);
                using (SHA256 hashSHA256 = SHA256.Create())
                {
                    byte[] hashValue = hashSHA256.ComputeHash(Encoding.UTF8.GetBytes(documentContent));
                    hash = Encoding.UTF8.GetString(hashValue);
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < hashValue.Length; i++)
                    {
                        builder.Append(hashValue[i].ToString("x2"));
                    }
                    hash = builder.ToString();
                }
            }
            await _dbAccess.UploadDocument(name,documentContent,hash,userName);
        }
        
        public Task CreateCustodianWalletAsync(WalletInformation information) =>
            _custodianService.CreateWallet(information.bpn, information.name);

        public async IAsyncEnumerable<CompanyApplication> GetAllApplicationsForUserWithStatus(string? userId)
        {
            if (String.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentNullException("userId must not be empty");
            }
            await foreach (var applicationWithStatus in _portalDBAccess.GetApplicationsWithStatusUntrackedAsync(userId).ConfigureAwait(false))
            {
                yield return new CompanyApplication {
                    ApplicationId = applicationWithStatus.ApplicationId,
                    ApplicationStatus = applicationWithStatus.ApplicationStatus
                };
            }
        }

        public async Task<CompanyWithAddress> GetCompanyWithAddressAsync(Guid? applicationId)
        {
            if (!applicationId.HasValue)
            {
                throw new ArgumentNullException("applicationId must not be null");
            }
            var result = await _portalDBAccess.GetCompanyWithAdressUntrackedAsync(applicationId.Value).ConfigureAwait(false);
            if (result == null)
            {
                throw new NotFoundException($"CompanyApplication {applicationId.Value} not found");
            }
            return result;
        }
        
        public async Task SetCompanyWithAddressAsync(Guid? applicationId, CompanyWithAddress? companyWithAddress)
        {
            if (!applicationId.HasValue)
            {
                throw new ArgumentNullException("applicationId must not be null");
            }
            if (companyWithAddress == null)
            {
                throw new ArgumentNullException("companyWithAddress must not be null");
            }
            if (!companyWithAddress.CompanyId.HasValue)
            {
                throw new ArgumentNullException("companyId must not be null");
            }
            if (companyWithAddress.Name == null)
            {
                throw new ArgumentNullException("Name must not be null");
            }
            if (companyWithAddress.City == null)
            {
                throw new ArgumentNullException("City must not be null");
            }
            if (companyWithAddress.Streetname == null)
            {
                throw new ArgumentNullException("Streetname must not be null");
            }
            if (companyWithAddress.Zipcode == null)
            {
                throw new ArgumentNullException("Zipcode must not be null");
            }
            if (companyWithAddress.CountryAlpha2Code == null)
            {
                throw new ArgumentNullException("CountryAlpha2Code must not be null");
            }
            var company = await _portalDBAccess.GetCompanyWithAdressAsync(applicationId.Value,companyWithAddress.CompanyId.Value).ConfigureAwait(false);
            if (company == null)
            {
                throw new NotFoundException($"CompanyApplication {applicationId.Value} for CompanyId {companyWithAddress.CompanyId} not found");
            }
            company.Bpn = companyWithAddress.Bpn;
            company.Name = companyWithAddress.Name;
            company.Shortname = companyWithAddress.Shortname;
            company.TaxId = companyWithAddress.TaxId;
            if (company.Address == null)
            {
                company.Address = _portalDBAccess.CreateAddress(
                        companyWithAddress.City,
                        companyWithAddress.Streetname,
                        (decimal)companyWithAddress.Zipcode,
                        companyWithAddress.CountryAlpha2Code
                    );
            }
            else
            {
                company.Address.City = companyWithAddress.City;
                company.Address.Streetname = companyWithAddress.Streetname;
                company.Address.Zipcode = (decimal)companyWithAddress.Zipcode;
                company.Address.CountryAlpha2Code = companyWithAddress.CountryAlpha2Code;
            }
            company.Address.Region = companyWithAddress.Region;
            company.Address.Streetadditional = companyWithAddress.Streetadditional;
            company.Address.Streetnumber = companyWithAddress.Streetnumber;
            company.CompanyStatusId = CompanyStatusId.PENDING;
            await _portalDBAccess.SaveAsync().ConfigureAwait(false);
        }

        public async Task<int> InviteNewUserAsync(Guid? applicationId, UserInvitationData? userInvitationData)
        {
            if (!applicationId.HasValue)
            {
                throw new ArgumentNullException("applicationId must not be null");
            }
            if (userInvitationData == null)
            {
                throw new ArgumentNullException("userInvitationData must not be null");
            }
            if (String.IsNullOrWhiteSpace(userInvitationData.firstName))
            {
                throw new ArgumentNullException("fistName must not be empty");
            }
            if (String.IsNullOrWhiteSpace(userInvitationData.lastName))
            {
                throw new ArgumentNullException("lastName must not be null");
            }
            if (String.IsNullOrWhiteSpace(userInvitationData.userName))
            {
                throw new ArgumentNullException("userName must not be null");
            }
            if (String.IsNullOrWhiteSpace(userInvitationData.email))
            {
                throw new ArgumentNullException("email must not be null");
            }
            var applicationData = await _portalDBAccess.GetCompanyNameIdWithSharedIdpAliasUntrackedAsync(applicationId.Value).ConfigureAwait(false);
            if (applicationData == null || applicationData.IdpAlias == null)
            {
                throw new NotFoundException($"shared idp for CompanyApplication {applicationId.Value} not found");
            }
            var password = new Password().Next();
            var iamUserId = await _provisioningManager.CreateSharedUserLinkedToCentralAsync(
                applicationData.IdpAlias,
                new UserProfile (
                    userInvitationData.userName,
                    userInvitationData.firstName,
                    userInvitationData.lastName,
                    userInvitationData.email,
                    password
                ),
                applicationData.CompanyName).ConfigureAwait(false);
            await _provisioningManager.AssignInvitedUserInitialRoles(iamUserId).ConfigureAwait(false);
            var user = _portalDBAccess.CreateCompanyUser(userInvitationData.firstName, userInvitationData.lastName, userInvitationData.email, applicationData.CompanyId);
            var invitation = _portalDBAccess.CreateInvitation(applicationId.Value, user);
            var iamUser = _portalDBAccess.CreateIamUser(user, iamUserId);
            var updates = await _portalDBAccess.SaveAsync();
            var mailParameters = new Dictionary<string, string>
            { //FIXME: parameters must match the templates for invite and password - adjust accordingly!
                { "password", password },
                { "companyname", applicationData.CompanyName },
                { "url", $"{_settings.BasePortalAddress}"},
            };

            await _mailingService.SendMails(userInvitationData.email, mailParameters, new List<string> { "invite", "password" } );

            return updates;
        }

        public async Task<int> SetApplicationStatusAsync(Guid? applicationId, CompanyApplicationStatusId? status)
        {
            if (!applicationId.HasValue)
            {
                throw new ArgumentNullException("applicationId must not be null");
            }
            if (!status.HasValue)
            {
                throw new ArgumentNullException("status must not be null");
            }
            var application = await _portalDBAccess.GetCompanyApplication(applicationId.Value).ConfigureAwait(false);
            if (application == null)
            {
                throw new NotFoundException($"CompanyApplication {applicationId.Value} not found");
            }
            application.ApplicationStatusId = status.Value;
            return await _portalDBAccess.SaveAsync().ConfigureAwait(false);
        }
            
        public async Task<CompanyApplicationStatusId> GetApplicationStatusAsync(Guid? applicationId)
        {
            if (!applicationId.HasValue)
            {
                throw new ArgumentNullException("applicationId must not be null");
            }
            var result = (CompanyApplicationStatusId?) await _portalDBAccess.GetApplicationStatusUntrackedAsync(applicationId.Value).ConfigureAwait(false);
            if (result == null)
            {
                throw new NotFoundException($"CompanyApplication {applicationId.Value} not found");
            }
            return result.Value;
        }

        public async Task<bool> SubmitRegistrationAsync(string userEmail)
        {
            var mailParameters = new Dictionary<string, string>
            {
                { "url", $"{_settings.BasePortalAddress}"},
            };

            await _mailingService.SendMails(userEmail,mailParameters, new List<string> { "SubmitRegistrationTemplate" });
            return true;
        }
    }
}
