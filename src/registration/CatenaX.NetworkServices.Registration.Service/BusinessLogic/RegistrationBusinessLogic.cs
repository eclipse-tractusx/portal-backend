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
using System.Linq;
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

        public Task SetIdpAsync(SetIdp idpToSet) =>
            _dbAccess.SetIdp(idpToSet);

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

        public async Task<CompanyWithAddress> GetCompanyWithAddressAsync(Guid applicationId)
        {
            var result = await _portalDBAccess.GetCompanyWithAdressUntrackedAsync(applicationId).ConfigureAwait(false);
            if (result == null)
            {
                throw new NotFoundException($"CompanyApplication {applicationId} not found");
            }
            return result;
        }
        
        public async Task SetCompanyWithAddressAsync(Guid applicationId, CompanyWithAddress companyWithAddress)
        {
            if (String.IsNullOrWhiteSpace(companyWithAddress.Name))
            {
                throw new ArgumentException("Name must not be empty");
            }
            if (String.IsNullOrWhiteSpace(companyWithAddress.City))
            {
                throw new ArgumentException("City must not be empty");
            }
            if (String.IsNullOrWhiteSpace(companyWithAddress.Streetname))
            {
                throw new ArgumentException("Streetname must not be empty");
            }
            if (!companyWithAddress.Zipcode.HasValue)
            {
                throw new ArgumentNullException("Zipcode must not be null");
            }
            if (companyWithAddress.CountryAlpha2Code.Length != 2)
            {
                throw new ArgumentException("CountryAlpha2Code must be 2 chars");
            }
            var company = await _portalDBAccess.GetCompanyWithAdressAsync(applicationId,companyWithAddress.CompanyId).ConfigureAwait(false);
            if (company == null)
            {
                throw new NotFoundException($"CompanyApplication {applicationId} for CompanyId {companyWithAddress.CompanyId} not found");
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
                        companyWithAddress.Zipcode.Value,
                        companyWithAddress.CountryAlpha2Code
                    );
            }
            else
            {
                company.Address.City = companyWithAddress.City;
                company.Address.Streetname = companyWithAddress.Streetname;
                company.Address.Zipcode = companyWithAddress.Zipcode.Value;
                company.Address.CountryAlpha2Code = companyWithAddress.CountryAlpha2Code;
            }
            company.Address.Region = companyWithAddress.Region;
            company.Address.Streetadditional = companyWithAddress.Streetadditional;
            company.Address.Streetnumber = companyWithAddress.Streetnumber;
            company.CompanyStatusId = CompanyStatusId.PENDING;
            await _portalDBAccess.SaveAsync().ConfigureAwait(false);
        }

        public async Task<int> InviteNewUserAsync(Guid applicationId, UserInvitationData userInvitationData)
        {
            if (String.IsNullOrWhiteSpace(userInvitationData.firstName))
            {
                throw new ArgumentNullException("fistName must not be empty");
            }
            if (String.IsNullOrWhiteSpace(userInvitationData.lastName))
            {
                throw new ArgumentNullException("lastName must not be empty");
            }
            if (String.IsNullOrWhiteSpace(userInvitationData.userName))
            {
                throw new ArgumentNullException("userName must not be empty");
            }
            if (String.IsNullOrWhiteSpace(userInvitationData.email))
            {
                throw new ArgumentNullException("email must not be empty");
            }
            var applicationData = await _portalDBAccess.GetCompanyNameIdWithSharedIdpAliasUntrackedAsync(applicationId).ConfigureAwait(false);
            if (applicationData == null || applicationData.IdpAlias == null)
            {
                throw new NotFoundException($"shared idp for CompanyApplication {applicationId} not found");
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
            var invitation = _portalDBAccess.CreateInvitation(applicationId, user);
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

        public async Task<int> SetApplicationStatusAsync(Guid applicationId, CompanyApplicationStatusId status)
        {
            if (status == 0)
            {
                throw new ArgumentNullException("status must not be null");
            }
            var application = await _portalDBAccess.GetCompanyApplication(applicationId).ConfigureAwait(false);
            if (application == null)
            {
                throw new NotFoundException($"CompanyApplication {applicationId} not found");
            }
            application.ApplicationStatusId = status;
            return await _portalDBAccess.SaveAsync().ConfigureAwait(false);
        }
            
        public async Task<CompanyApplicationStatusId> GetApplicationStatusAsync(Guid applicationId)
        {
            var result = (CompanyApplicationStatusId?) await _portalDBAccess.GetApplicationStatusUntrackedAsync(applicationId).ConfigureAwait(false);
            if (!result.HasValue)
            {
                throw new NotFoundException($"CompanyApplication {applicationId} not found");
            }
            return result.Value;
        }

        public async Task<int> SubmitRoleConsentAsync(Guid applicationId, CompanyRoleAgreementConsents roleAgreementConsentStatuses, string iamUserId)
        {
            var companyRoleIdsToSet = roleAgreementConsentStatuses.CompanyRoleIds;
            var agreementConsentsToSet = roleAgreementConsentStatuses.AgreementConsentStatuses;

            var (companyUserId, companyId, companyAssignedRoles, activeConsents) = await _portalDBAccess.GetCompanyRoleAgreementConsentsAsync(applicationId, iamUserId).ConfigureAwait(false);
            if (!companyUserId.HasValue || !companyId.HasValue)
            {
                throw new ForbiddenException($"iamUserId {iamUserId} is not assigned with CompanyApplication {applicationId}");
            }

            var companyRoleAssignedAgreements = await _portalDBAccess.GetAgreementAssignedCompanyRolesUntrackedAsync(companyRoleIdsToSet).ConfigureAwait(false);

            if (!companyRoleIdsToSet
                .All(companyRoleIdToSet => 
                    companyRoleAssignedAgreements[companyRoleIdToSet].All(assignedAgreementId => 
                        agreementConsentsToSet
                            .Any(agreementConsent =>
                                agreementConsent.AgreementId == assignedAgreementId
                                && agreementConsent.ConsentStatusId == ConsentStatusId.ACTIVE))))
            {
                throw new ArgumentException("consent must be given to all CompanyRole assigned agreements");
            }

            foreach (var companyAssignedRoleToRemove in companyAssignedRoles
                .Where(companyAssignedRole =>
                    !companyRoleIdsToSet.Contains(companyAssignedRole.CompanyRoleId)))
            {
                _portalDBAccess.RemoveCompanyAssignedRole(companyAssignedRoleToRemove);
            }

            foreach (var companyRoleIdToAdd in companyRoleIdsToSet
                .Where(companyRoleId =>
                    !companyAssignedRoles.Any(companyAssignedRole =>
                        companyAssignedRole.CompanyRoleId == companyRoleId)))
            {
                _portalDBAccess.CreateCompanyAssignedRole(companyId.Value, companyRoleIdToAdd);
            }

            foreach (var consentToRemove in activeConsents
                .Where(activeConsent =>
                    !agreementConsentsToSet.Any(agreementConsent =>
                        agreementConsent.AgreementId == activeConsent.AgreementId
                        && agreementConsent.ConsentStatusId == ConsentStatusId.ACTIVE)))
            {
                consentToRemove.ConsentStatusId = ConsentStatusId.INACTIVE;
            }

            foreach (var agreementConsentToAdd in agreementConsentsToSet.Where(agreementConsent => agreementConsent.ConsentStatusId == ConsentStatusId.ACTIVE && !activeConsents.Any(activeConsent => activeConsent.AgreementId == agreementConsent.AgreementId)))
            {
                _portalDBAccess.CreateConsent(agreementConsentToAdd.AgreementId, companyId.Value, companyUserId.Value, ConsentStatusId.ACTIVE);
            }

            return await _portalDBAccess.SaveAsync().ConfigureAwait(false);
        }

        public async Task<CompanyRoleAgreementConsents> GetRoleAgreementConsentsAsync(Guid applicationId, string iamUserId)
        {
            var (permitted, companyRoleIds, agreementConsentStatuses) = await _portalDBAccess.GetCompanyRoleAgreementConsentStatusUntrackedAsync(applicationId, iamUserId).ConfigureAwait(false);
            if (!permitted)
            {
                throw new ForbiddenException($"iamUserId {iamUserId} is not assigned with CompanyApplication {applicationId}");
            }
            return new CompanyRoleAgreementConsents(
                companyRoleIds!,
                agreementConsentStatuses.Select(agreementConsentStatus => {
                    var (agreementId,consentStatusId) = agreementConsentStatus;
                    return new AgreementConsentStatus(agreementId,consentStatusId);
                }));
        }

        public async Task<CompanyRoleAgreementData> GetCompanyRoleAgreementDataAsync()
        {
            return new CompanyRoleAgreementData(
                (await _portalDBAccess.GetCompanyRoleAgreementsUntrackedAsync().ToListAsync().ConfigureAwait(false)).AsEnumerable(),
                (await _portalDBAccess.GetAgreementsUntrackedAsync().ToListAsync().ConfigureAwait(false)).AsEnumerable()
            );
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
