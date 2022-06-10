using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.Mailing.SendMail;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.Provisioning.Library;
using CatenaX.NetworkServices.Provisioning.Library.Models;
using CatenaX.NetworkServices.Registration.Service.BPN;
using CatenaX.NetworkServices.Registration.Service.BPN.Model;
using CatenaX.NetworkServices.Registration.Service.Model;
using CatenaX.NetworkServices.Registration.Service.RegistrationAccess;
using Microsoft.Extensions.Options;
using PasswordGenerator;
using System.Security.Cryptography;
using System.Text;

namespace CatenaX.NetworkServices.Registration.Service.BusinessLogic
{
    public class RegistrationBusinessLogic : IRegistrationBusinessLogic
    {
        private readonly RegistrationSettings _settings;
        private readonly IRegistrationDBAccess _dbAccess;
        private readonly IMailingService _mailingService;
        private readonly IBPNAccess _bpnAccess;
        private readonly IProvisioningManager _provisioningManager;
        private readonly IPortalBackendDBAccess _portalDBAccess;
        private readonly ILogger<RegistrationBusinessLogic> _logger;

        public RegistrationBusinessLogic(IOptions<RegistrationSettings> settings, IRegistrationDBAccess registrationDBAccess, IMailingService mailingService, IBPNAccess bpnAccess, IProvisioningManager provisioningManager, IPortalBackendDBAccess portalDBAccess, ILogger<RegistrationBusinessLogic> logger)
        {
            _settings = settings.Value;
            _dbAccess = registrationDBAccess;
            _mailingService = mailingService;
            _bpnAccess = bpnAccess;
            _provisioningManager = provisioningManager;
            _portalDBAccess = portalDBAccess;
            _logger = logger;
        }

        public Task<IEnumerable<string>> GetClientRolesCompositeAsync() =>
            _provisioningManager.GetClientRolesCompositeAsync(_settings.KeyCloakClientID);

        public Task<List<FetchBusinessPartnerDto>> GetCompanyByIdentifierAsync(string companyIdentifier, string token) =>
            _bpnAccess.FetchBusinessPartner(companyIdentifier, token);

        public Task SetIdpAsync(SetIdp idpToSet) =>
            _dbAccess.SetIdp(idpToSet);

        public async Task<int> UploadDocumentAsync(Guid applicationId, IFormFile document, DocumentTypeId documentTypeId, string iamUserId)
        {
            if (string.IsNullOrEmpty(document.FileName))
            {
                throw new ArgumentNullException("File name is must not be null");
            }

            var companyUserId = await _portalDBAccess.GetCompanyUserIdForUserApplicationUntrackedAsync(applicationId, iamUserId).ConfigureAwait(false);
            if (companyUserId.Equals(Guid.Empty))
            {
                throw new ForbiddenException($"iamUserId {iamUserId} is not assigned with CompanyAppication {applicationId}");
            }
            var documentName = document.FileName;

            using (var ms = new MemoryStream())
            {
                document.CopyTo(ms);
                var fileBytes = ms.ToArray();
                var documentContent = Convert.ToBase64String(fileBytes);
                using (SHA256 hashSHA256 = SHA256.Create())
                {
                    byte[] hashValue = hashSHA256.ComputeHash(Encoding.UTF8.GetBytes(documentContent));
                    // hash = Encoding.UTF8.GetString(hashValue);
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < hashValue.Length; i++)
                    {
                        builder.Append(hashValue[i].ToString("x2"));
                    }
                    var hash = builder.ToString();
                    _portalDBAccess.CreateDocument(applicationId, companyUserId, documentName, documentContent, hash, 0, documentTypeId);
                }
            }
            return await _portalDBAccess.SaveAsync().ConfigureAwait(false);
        }

        public async IAsyncEnumerable<CompanyApplication> GetAllApplicationsForUserWithStatus(string userId)
        {
            await foreach (var applicationWithStatus in _portalDBAccess.GetApplicationsWithStatusUntrackedAsync(userId).ConfigureAwait(false))
            {
                yield return new CompanyApplication
                {
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
            if (companyWithAddress.CountryAlpha2Code.Length != 2)
            {
                throw new ArgumentException("CountryAlpha2Code must be 2 chars");
            }
            var company = await _portalDBAccess.GetCompanyWithAdressAsync(applicationId, companyWithAddress.CompanyId).ConfigureAwait(false);
            if (company == null)
            {
                throw new NotFoundException($"CompanyApplication {applicationId} for CompanyId {companyWithAddress.CompanyId} not found");
            }
            company.BusinessPartnerNumber = companyWithAddress.BusinessPartnerNumber;
            company.Name = companyWithAddress.Name;
            company.Shortname = companyWithAddress.Shortname;
            company.TaxId = companyWithAddress.TaxId;
            if (company.Address == null)
            {
                company.Address = _portalDBAccess.CreateAddress(
                        companyWithAddress.City,
                        companyWithAddress.Streetname,
                        companyWithAddress.CountryAlpha2Code
                    );
            }
            else
            {
                company.Address.City = companyWithAddress.City;
                company.Address.Streetname = companyWithAddress.Streetname;
                company.Address.CountryAlpha2Code = companyWithAddress.CountryAlpha2Code;
            }
            company.Address.Zipcode = companyWithAddress.Zipcode;
            company.Address.Region = companyWithAddress.Region;
            company.Address.Streetadditional = companyWithAddress.Streetadditional;
            company.Address.Streetnumber = companyWithAddress.Streetnumber;
            company.CompanyStatusId = CompanyStatusId.PENDING;
            await _portalDBAccess.SaveAsync().ConfigureAwait(false);
        }

        public async Task<int> InviteNewUserAsync(Guid applicationId, UserCreationInfo userCreationInfo, string createdById)
        {
            var userCheck = await _portalDBAccess.IsUserAlreadyExist(createdById);
            if (userCheck)
            {
                throw new ForbiddenException($"user {createdById} does already exist");
            }

            var applicationData = await _portalDBAccess.GetCompanyNameIdWithSharedIdpAliasUntrackedAsync(applicationId, createdById).ConfigureAwait(false);
            if (applicationData == null)
            {
                throw new ForbiddenException($"user {createdById} is not associated with application {applicationId}");
            }
            if (applicationData.IdpAlias == null)
            {
                throw new NotFoundException($"shared idp for CompanyApplication {applicationId} not found");
            }

            var clientId = _settings.KeyCloakClientID;

            var roles = userCreationInfo.Roles
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

            var password = new Password().Next();
            var centralUserId = await _provisioningManager.CreateSharedUserLinkedToCentralAsync(
                applicationData.IdpAlias,
                new UserProfile(
                    userCreationInfo.userName ?? userCreationInfo.eMail,
                    userCreationInfo.eMail,
                    applicationData.CompanyName
                )
                {
                    FirstName = userCreationInfo.firstName,
                    LastName = userCreationInfo.lastName,
                    Password = password
                }).ConfigureAwait(false);

            if (roles.Count() > 0)
            {
                var clientRoleNames = new Dictionary<string, IEnumerable<string>>
                {
                    { _settings.KeyCloakClientID, roles }
                };
                await _provisioningManager.AssignClientRolesToCentralUserAsync(centralUserId, clientRoleNames).ConfigureAwait(false);
            }
            var companyUser = _portalDBAccess.CreateCompanyUser(userCreationInfo.firstName, userCreationInfo.lastName, userCreationInfo.eMail, applicationData.CompanyId, CompanyUserStatusId.ACTIVE);

            foreach (var role in roles)
            {
                _portalDBAccess.CreateCompanyUserAssignedRole(companyUser.Id, companyRoleIds[role]);
            }

            _portalDBAccess.CreateIamUser(companyUser, centralUserId);
            _portalDBAccess.CreateInvitation(applicationId, companyUser);

            var modified = await _portalDBAccess.SaveAsync().ConfigureAwait(false);

            var inviteTemplateName = "invite";
            if (!string.IsNullOrWhiteSpace(userCreationInfo.Message))
            {
                inviteTemplateName = "inviteWithMessage";
            }

            var mailParameters = new Dictionary<string, string>
            {
                { "password", password },
                { "companyname", applicationData.CompanyName },
                { "message", userCreationInfo.Message ?? "" },
                { "nameCreatedBy", createdById },
                { "url", _settings.BasePortalAddress },
                { "username", userCreationInfo.eMail },
            };

            await _mailingService.SendMails(userCreationInfo.eMail, mailParameters, new List<string> { inviteTemplateName, "password" }).ConfigureAwait(false);

            return modified;
        }

        public async Task<int> SetApplicationStatusAsync(Guid applicationId, CompanyApplicationStatusId status)
        {
            if (status == 0)
            {
                throw new ArgumentNullException("status must not be null");
            }
            var application = await _portalDBAccess.GetCompanyApplicationAsync(applicationId).ConfigureAwait(false);
            if (application == null)
            {
                throw new NotFoundException($"CompanyApplication {applicationId} not found");
            }
            application.ApplicationStatusId = status;
            return await _portalDBAccess.SaveAsync().ConfigureAwait(false);
        }

        public async Task<CompanyApplicationStatusId> GetApplicationStatusAsync(Guid applicationId)
        {
            var result = (CompanyApplicationStatusId?)await _portalDBAccess.GetApplicationStatusUntrackedAsync(applicationId).ConfigureAwait(false);
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

            var companyRoleAgreementConsentData = await _portalDBAccess.GetCompanyRoleAgreementConsentDataAsync(applicationId, iamUserId).ConfigureAwait(false);

            if (companyRoleAgreementConsentData == null)
            {
                throw new ForbiddenException($"iamUserId {iamUserId} is not assigned with CompanyApplication {applicationId}");
            }

            var companyUserId = companyRoleAgreementConsentData.CompanyUserId;
            var companyId = companyRoleAgreementConsentData.CompanyId;
            var companyAssignedRoles = companyRoleAgreementConsentData.CompanyAssignedRoles;
            var activeConsents = companyRoleAgreementConsentData.Consents;

            var companyRoleAssignedAgreements = new Dictionary<CompanyRoleId, IEnumerable<Guid>>();
            await foreach (var companyRoleAgreement in _portalDBAccess.GetAgreementAssignedCompanyRolesUntrackedAsync(companyRoleIdsToSet).ConfigureAwait(false))
            {
                companyRoleAssignedAgreements[companyRoleAgreement.CompanyRoleId] = companyRoleAgreement.AgreementIds;
            }

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
                _portalDBAccess.CreateCompanyAssignedRole(companyId, companyRoleIdToAdd);
            }

            foreach (var consentToRemove in activeConsents
                .Where(activeConsent =>
                    !agreementConsentsToSet.Any(agreementConsent =>
                        agreementConsent.AgreementId == activeConsent.AgreementId
                        && agreementConsent.ConsentStatusId == ConsentStatusId.ACTIVE)))
            {
                consentToRemove.ConsentStatusId = ConsentStatusId.INACTIVE;
            }

            foreach (var agreementConsentToAdd in agreementConsentsToSet
                .Where(agreementConsent =>
                    agreementConsent.ConsentStatusId == ConsentStatusId.ACTIVE
                    && !activeConsents.Any(activeConsent =>
                        activeConsent.AgreementId == agreementConsent.AgreementId)))
            {
                _portalDBAccess.CreateConsent(agreementConsentToAdd.AgreementId, companyId, companyUserId, ConsentStatusId.ACTIVE);
            }

            return await _portalDBAccess.SaveAsync().ConfigureAwait(false);
        }

        public async Task<CompanyRoleAgreementConsents> GetRoleAgreementConsentsAsync(Guid applicationId, string iamUserId)
        {
            var result = await _portalDBAccess.GetCompanyRoleAgreementConsentStatusUntrackedAsync(applicationId, iamUserId).ConfigureAwait(false);
            if (result == null)
            {
                throw new ForbiddenException($"iamUserId {iamUserId} is not assigned with CompanyApplication {applicationId}");
            }
            return result;
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

            await _mailingService.SendMails(userEmail, mailParameters, new List<string> { "SubmitRegistrationTemplate" });
            return true;
        }

        public async IAsyncEnumerable<InvitedUser> GetInvitedUsersAsync(Guid applicationId)
        {
            await foreach (var item in _portalDBAccess.GetInvitedUserDetailsUntrackedAsync(applicationId).ConfigureAwait(false))
            {
                var userRoles = await _provisioningManager.GetClientRoleMappingsForUserAsync(item.UserId, _settings.KeyCloakClientID).ConfigureAwait(false);
                yield return new InvitedUser(
                    item.InvitationStatus,
                    item.EmailId,
                    userRoles
                );
            }
        }
        
              //TODO: Need to implement storage for document upload
        public IAsyncEnumerable<UploadDocuments> GetUploadedDocumentsAsync(Guid applicationId, DocumentTypeId documentTypeId, string iamUserId) =>
            _portalDBAccess.GetUploadedDocumentsAsync(applicationId,documentTypeId,iamUserId);

        public async Task<int> SetInvitationStatusAsync(string iamUserId)
        {
            var invitationData = await _portalDBAccess.GetInvitationStatusAsync(iamUserId).ConfigureAwait(false);

            if (invitationData == null)
            {
                throw new ForbiddenException($"iamUserId {iamUserId} is not associated with invitation");
            }

            if (invitationData.InvitationStatusId != InvitationStatusId.CREATED)
            {
                throw new ArgumentException($"invitation status is no longer in status 'CREATED'");
            }

            invitationData.InvitationStatusId = InvitationStatusId.PENDING;

            return await _portalDBAccess.SaveAsync().ConfigureAwait(false);
        }

        public async Task<RegistrationData> GetRegistrationDataAsync(Guid applicationId, string iamUserId)
        {
            var registrationData = await _portalDBAccess.GetRegistrationDataUntrackedAsync(applicationId, iamUserId).ConfigureAwait(false);
            if (registrationData == null)
            {
                throw new ForbiddenException($"iamUserId {iamUserId} is not assigned with CompanyApplication {applicationId}");
            }
            return registrationData;
        }
    }
}
