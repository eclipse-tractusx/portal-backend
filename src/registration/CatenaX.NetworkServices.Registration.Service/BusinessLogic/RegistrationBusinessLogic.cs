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

using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.Mailing.SendMail;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.Provisioning.Library;
using CatenaX.NetworkServices.Provisioning.Library.Models;
using CatenaX.NetworkServices.Registration.Service.BPN;
using CatenaX.NetworkServices.Registration.Service.BPN.Model;
using CatenaX.NetworkServices.Registration.Service.Model;
using Microsoft.Extensions.Options;
using PasswordGenerator;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

namespace CatenaX.NetworkServices.Registration.Service.BusinessLogic
{
    public class RegistrationBusinessLogic : IRegistrationBusinessLogic
    {
        private readonly RegistrationSettings _settings;
        private readonly IMailingService _mailingService;
        private readonly IBPNAccess _bpnAccess;
        private readonly IProvisioningManager _provisioningManager;
        private readonly IPortalRepositories _portalRepositories;
        private readonly ILogger<RegistrationBusinessLogic> _logger;

        public RegistrationBusinessLogic(
            IOptions<RegistrationSettings> settings, 
            IMailingService mailingService, 
            IBPNAccess bpnAccess, 
            IProvisioningManager provisioningManager, 
            ILogger<RegistrationBusinessLogic> logger, 
            IPortalRepositories portalRepositories)
        {
            _settings = settings.Value;
            _mailingService = mailingService;
            _bpnAccess = bpnAccess;
            _provisioningManager = provisioningManager;
            _logger = logger;
            _portalRepositories = portalRepositories;
        }

        public IAsyncEnumerable<string> GetClientRolesCompositeAsync() =>
            _portalRepositories.GetInstance<IUserRolesRepository>().GetClientRolesCompositeAsync(_settings.KeyCloakClientID);

        public Task<List<FetchBusinessPartnerDto>> GetCompanyByIdentifierAsync(string companyIdentifier, string token)
        {
            var regex = new Regex(@"(\w|\d){16}");
            if (!regex.IsMatch(companyIdentifier))
            {
                throw new ArgumentException("BPN must contain exactly 16 digits or letters.", nameof(companyIdentifier));
            }

            return _bpnAccess.FetchBusinessPartner(companyIdentifier, token);
        }

        public async Task<int> UploadDocumentAsync(Guid applicationId, IFormFile document, DocumentTypeId documentTypeId, string iamUserId)
        {
            if (string.IsNullOrEmpty(document.FileName))
            {
                throw new ControllerArgumentException("File name is must not be null");
            }

            // Check if document is a pdf file (also see https://www.rfc-editor.org/rfc/rfc3778.txt)
            if (!document.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
            {
                throw new UnsupportedMediaTypeException("Only .pdf files are allowed.");
            }

            var companyUserId = await _portalRepositories.GetInstance<IUserRepository>().GetCompanyUserIdForUserApplicationUntrackedAsync(applicationId, iamUserId).ConfigureAwait(false);
            if (companyUserId == Guid.Empty)
            {
                throw new ForbiddenException($"iamUserId {iamUserId} is not assigned with CompanyAppication {applicationId}");
            }

            var documentName = document.FileName;
            using var sha256 = SHA256.Create();
            using var ms = new MemoryStream((int)document.Length);
            
            document.CopyTo(ms);
            var hash = sha256.ComputeHash(ms);
            var documentContent = ms.GetBuffer();
            if (ms.Length != document.Length || documentContent.Length != document.Length)
            {
                throw new ArgumentException($"document {document.FileName} transmitted length {document.Length} doesn't match actual length {ms.Length}.");
            }
            
            _portalRepositories.GetInstance<IDocumentRepository>().CreateDocument(companyUserId, documentName, documentContent, hash, documentTypeId);
            return await _portalRepositories.SaveAsync().ConfigureAwait(false);
        }

        public async Task<(string fileName, byte[] content)> GetDocumentContentAsync(Guid documentId, string iamUserId)
        {
            var documentRepository = _portalRepositories.GetInstance<IDocumentRepository>();

            var documentDetails = await documentRepository.GetDocumentIdCompanyUserSameAsIamUserAsync(documentId, iamUserId).ConfigureAwait(false);
            if (documentDetails.DocumentId == Guid.Empty)
            {
                throw new NotFoundException($"document {documentId} does not exist.");
            }

            if (!documentDetails.IsSameUser)
            {
                throw new ForbiddenException($"user {iamUserId} is not permitted to access document {documentId}.");
            }

            var document = await documentRepository.GetDocumentByIdAsync(documentId).ConfigureAwait(false);
            if (document is null)
            {
                throw new NotFoundException($"document {documentId} does not exist.");
            }

            return (document.DocumentName, document.DocumentContent);
        }

        public async IAsyncEnumerable<CompanyApplicationData> GetAllApplicationsForUserWithStatus(string userId)
        {
            await foreach (var applicationWithStatus in _portalRepositories.GetInstance<IUserRepository>().GetApplicationsWithStatusUntrackedAsync(userId).ConfigureAwait(false))
            {
                yield return new CompanyApplicationData
                {
                    ApplicationId = applicationWithStatus.ApplicationId,
                    ApplicationStatus = applicationWithStatus.ApplicationStatus
                };
            }
        }

        public async Task<CompanyWithAddress> GetCompanyWithAddressAsync(Guid applicationId)
        {
            var result = await _portalRepositories.GetInstance<IApplicationRepository>().GetCompanyWithAdressUntrackedAsync(applicationId).ConfigureAwait(false);
            if (result == null)
            {
                throw new NotFoundException($"CompanyApplication {applicationId} not found");
            }
            return result;
        }

        public async Task SetCompanyWithAddressAsync(Guid applicationId, CompanyWithAddress companyWithAddress, string iamUserId)
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
            var companyApplicationData = await _portalRepositories.GetInstance<IApplicationRepository>().GetCompanyApplicationWithCompanyAdressUserDataAsync(applicationId, companyWithAddress.CompanyId, iamUserId).ConfigureAwait(false);
            if (companyApplicationData == null)
            {
                throw new NotFoundException($"CompanyApplication {applicationId} for CompanyId {companyWithAddress.CompanyId} not found");
            }
            if (companyApplicationData.CompanyUserId == Guid.Empty)
            {
                throw new ForbiddenException($"iamUserId {iamUserId} is not assigned with CompanyApplication {applicationId}");
            }

            var company = companyApplicationData.CompanyApplication.Company!;

            company.BusinessPartnerNumber = companyWithAddress.BusinessPartnerNumber;
            company.Name = companyWithAddress.Name;
            company.Shortname = companyWithAddress.Shortname;
            company.TaxId = companyWithAddress.TaxId;
            if (company.Address == null)
            {
                company.Address = _portalRepositories.GetInstance<ICompanyRepository>().CreateAddress(
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

            UpdateApplicationStatus(companyApplicationData.CompanyApplication, UpdateApplicationSteps.CompanyWithAddress);

            await _portalRepositories.SaveAsync().ConfigureAwait(false);
        }

        public async Task<int> InviteNewUserAsync(Guid applicationId, UserCreationInfo userCreationInfo, string createdById)
        {
            
            var userRepository = _portalRepositories.GetInstance<IUserRepository>();
            var companyRepository = _portalRepositories.GetInstance<ICompanyRepository>();
            var userRolesRepository = _portalRepositories.GetInstance<IUserRolesRepository>();
            var applicationRepository = _portalRepositories.GetInstance<IApplicationRepository>();

            if (string.IsNullOrEmpty(userCreationInfo.eMail))
            {
                throw new ControllerArgumentException($"email must not be empty");
            }

            if (await userRepository.IsOwnCompanyUserWithEmailExisting(userCreationInfo.eMail, createdById))
            {
                throw new ControllerArgumentException($"user with email {userCreationInfo.eMail} does already exist");
            }

            var applicationData = await companyRepository.GetCompanyNameIdWithSharedIdpAliasUntrackedAsync(applicationId, createdById).ConfigureAwait(false);
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
                    .Where(role => !string.IsNullOrWhiteSpace(role))
                    .Distinct()
                    .ToList();

            var companyRoleIds = await userRolesRepository.GetUserRoleWithIdsUntrackedAsync(
                clientId,
                roles
                )
                .ToDictionaryAsync(
                    companyRoleWithId => companyRoleWithId.CompanyUserRoleText,
                    companyRoleWithId => companyRoleWithId.CompanyUserRoleId
                )
                .ConfigureAwait(false);

            var invalidRoles = roles.Where(role => !companyRoleIds.ContainsKey(role)).ToList();
            if (invalidRoles.Any())
            {
                throw new ArgumentException($"invalid Role(s): {string.Join(",", invalidRoles)}");
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

            var companyUserId = await userRepository.GetCompanyUserIdForIamUserUntrackedAsync(createdById).ConfigureAwait(false);
            var companyUser = userRepository.CreateCompanyUser(userCreationInfo.firstName, userCreationInfo.lastName, userCreationInfo.eMail, applicationData.CompanyId, CompanyUserStatusId.ACTIVE, companyUserId);

            IEnumerable<string>? unassignedRoles = null;
            if (roles.Any())
            {
                var clientRoleNames = new Dictionary<string, IEnumerable<string>>
                {
                    { _settings.KeyCloakClientID, roles }
                };
                var (_, assignedRoles) = (await _provisioningManager.AssignClientRolesToCentralUserAsync(centralUserId, clientRoleNames).ConfigureAwait(false)).Single();
                foreach (var role in assignedRoles)
                {
                    userRolesRepository.CreateCompanyUserAssignedRole(companyUser.Id, companyRoleIds[role]);
                }
                unassignedRoles = roles.Except(assignedRoles);
            }

            userRepository.CreateIamUser(companyUser, centralUserId);
            applicationRepository.CreateInvitation(applicationId, companyUser);

            var modified = await _portalRepositories.SaveAsync().ConfigureAwait(false);

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

            if (unassignedRoles != null && unassignedRoles.Any())
            {
                //TODO maybe change return type to indicate error-conditions.
                throw new UnexpectedConditionException($"invalid data, failed to assign roles [{string.Join(", ",unassignedRoles)}] in keycloak");
            }

            return modified;
        }

        public async Task<int> SetOwnCompanyApplicationStatusAsync(Guid applicationId, CompanyApplicationStatusId status, string iamUserId)
        {
            if (status == 0)
            {
                throw new ControllerArgumentException("status must not be null");
            }
            var applicationUserData = await _portalRepositories.GetInstance<IApplicationRepository>().GetOwnCompanyApplicationUserDataAsync(applicationId, iamUserId).ConfigureAwait(false);
            if (applicationUserData == null)
            {
                throw new NotFoundException($"CompanyApplication {applicationId} not found");
            }
            if (applicationUserData.CompanyUserId == Guid.Empty)
            {
                throw new ForbiddenException($"user {iamUserId} is not associated with application {applicationId}");
            }

            var application = applicationUserData.CompanyApplication;
            switch (application.ApplicationStatusId)
            {
                case CompanyApplicationStatusId.CREATED:
                    if (status != CompanyApplicationStatusId.ADD_COMPANY_DATA)
                    {
                        throw new ArgumentException($"invalid status update requested {status}, current status is {application.ApplicationStatusId}, possible values are: {CompanyApplicationStatusId.ADD_COMPANY_DATA}");
                    }
                    application.ApplicationStatusId = status;
                    break;

                case CompanyApplicationStatusId.ADD_COMPANY_DATA:
                    if (status != CompanyApplicationStatusId.INVITE_USER)
                    {
                        throw new ArgumentException($"invalid status update requested {status}, current status is {application.ApplicationStatusId}, possible values are: {CompanyApplicationStatusId.INVITE_USER}");
                    }
                    application.ApplicationStatusId = status;
                    break;

                case CompanyApplicationStatusId.INVITE_USER:
                    if (status != CompanyApplicationStatusId.SELECT_COMPANY_ROLE)
                    {
                        throw new ArgumentException($"invalid status update requested {status}, current status is {application.ApplicationStatusId}, possible values are: {CompanyApplicationStatusId.SELECT_COMPANY_ROLE}");
                    }
                    application.ApplicationStatusId = status;
                    break;

                case CompanyApplicationStatusId.SELECT_COMPANY_ROLE:
                    if (status != CompanyApplicationStatusId.UPLOAD_DOCUMENTS)
                    {
                        throw new ArgumentException($"invalid status update requested {status}, current status is {application.ApplicationStatusId}, possible values are: {CompanyApplicationStatusId.UPLOAD_DOCUMENTS}");
                    }
                    application.ApplicationStatusId = status;
                    break;

                case CompanyApplicationStatusId.UPLOAD_DOCUMENTS:
                    if (status != CompanyApplicationStatusId.VERIFY)
                    {
                        throw new ArgumentException($"invalid status update requested {status}, current status is {application.ApplicationStatusId}, possible values are: {CompanyApplicationStatusId.VERIFY}");
                    }
                    application.ApplicationStatusId = status;
                    break;

                case CompanyApplicationStatusId.VERIFY:
                    if (status != CompanyApplicationStatusId.SUBMITTED)
                    {
                        throw new ArgumentException($"invalid status update requested {status}, current status is {application.ApplicationStatusId}, possible values are: {CompanyApplicationStatusId.SUBMITTED}");
                    }
                    application.ApplicationStatusId = status;
                    break;
                default:
                    throw new ArgumentException($"invalid status update requested {status}, current status is {application.ApplicationStatusId}");
            }

            return await _portalRepositories.SaveAsync().ConfigureAwait(false);
        }

        public async Task<CompanyApplicationStatusId> GetOwnCompanyApplicationStatusAsync(Guid applicationId, string iamUserId)
        {
            var applicationStatusUserData = await _portalRepositories.GetInstance<IApplicationRepository>().GetOwnCompanyApplicationStatusUserDataUntrackedAsync(applicationId, iamUserId).ConfigureAwait(false);
            if (applicationStatusUserData == null)
            {
                throw new NotFoundException($"CompanyApplication {applicationId} not found");
            }
            if (applicationStatusUserData.CompanyUserId == Guid.Empty)
            {
                throw new ForbiddenException($"user {iamUserId} is not associated with application {applicationId}");
            }
            return applicationStatusUserData.CompanyApplicationStatusId;
        }

        public async Task<int> SubmitRoleConsentAsync(Guid applicationId, CompanyRoleAgreementConsents roleAgreementConsentStatuses, string iamUserId)
        {
            var companyRoleIdsToSet = roleAgreementConsentStatuses.CompanyRoleIds;
            var agreementConsentsToSet = roleAgreementConsentStatuses.AgreementConsentStatuses;

            var companyRolesRepository = _portalRepositories.GetInstance<ICompanyRolesRepository>();

            var companyRoleAgreementConsentData = await companyRolesRepository.GetCompanyRoleAgreementConsentDataAsync(applicationId, iamUserId).ConfigureAwait(false);

            if (companyRoleAgreementConsentData == null)
            {
                throw new NotFoundException($"application {applicationId} does not exist");
            }
            if (companyRoleAgreementConsentData.CompanyUserId == Guid.Empty)
            {
                throw new ForbiddenException($"iamUserId {iamUserId} is not assigned with CompanyApplication {applicationId}");
            }

            var companyUserId = companyRoleAgreementConsentData.CompanyUserId;
            var companyId = companyRoleAgreementConsentData.CompanyId;
            var application = companyRoleAgreementConsentData.CompanyApplication;
            var companyAssignedRoles = companyRoleAgreementConsentData.CompanyAssignedRoles;
            var activeConsents = companyRoleAgreementConsentData.Consents;

            var companyRoleAssignedAgreements = new Dictionary<CompanyRoleId, IEnumerable<Guid>>();
            await foreach (var companyRoleAgreement in companyRolesRepository.GetAgreementAssignedCompanyRolesUntrackedAsync(companyRoleIdsToSet).ConfigureAwait(false))
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
                companyRolesRepository.RemoveCompanyAssignedRole(companyAssignedRoleToRemove);
            }

            foreach (var companyRoleIdToAdd in companyRoleIdsToSet
                .Where(companyRoleId =>
                    !companyAssignedRoles.Any(companyAssignedRole =>
                        companyAssignedRole.CompanyRoleId == companyRoleId)))
            {
                companyRolesRepository.CreateCompanyAssignedRole(companyId, companyRoleIdToAdd);
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
                companyRolesRepository.CreateConsent(agreementConsentToAdd.AgreementId, companyId, companyUserId, ConsentStatusId.ACTIVE);
            }

            UpdateApplicationStatus(application, UpdateApplicationSteps.CompanyRoleAgreementConsents);

            return await _portalRepositories.SaveAsync().ConfigureAwait(false);
        }

        public async Task<CompanyRoleAgreementConsents> GetRoleAgreementConsentsAsync(Guid applicationId, string iamUserId)
        {
            var result = await _portalRepositories.GetInstance<ICompanyRolesRepository>().GetCompanyRoleAgreementConsentStatusUntrackedAsync(applicationId, iamUserId).ConfigureAwait(false);
            if (result == null)
            {
                throw new ForbiddenException($"iamUserId {iamUserId} is not assigned with CompanyApplication {applicationId}");
            }
            return result;
        }

        public async Task<CompanyRoleAgreementData> GetCompanyRoleAgreementDataAsync()
        {
            var companyRolesRepository = _portalRepositories.GetInstance<ICompanyRolesRepository>();
            return new CompanyRoleAgreementData(
                (await companyRolesRepository.GetCompanyRoleAgreementsUntrackedAsync().ToListAsync().ConfigureAwait(false)).AsEnumerable(),
                (await companyRolesRepository.GetAgreementsUntrackedAsync().ToListAsync().ConfigureAwait(false)).AsEnumerable()
            );
        }

        public async Task<bool> SubmitRegistrationAsync(Guid applicationId, string iamUserId)
        {
            var applicationUserData = await _portalRepositories.GetInstance<IApplicationRepository>().GetOwnCompanyApplicationUserEmailDataAsync(applicationId, iamUserId).ConfigureAwait(false);
            if (applicationUserData == null)
            {
                throw new NotFoundException($"application {applicationId} does not exist");
            }
            if (applicationUserData.CompanyUserId == Guid.Empty)
            {
                throw new ForbiddenException($"iamUserId {iamUserId} is not assigned with CompanyApplication {applicationId}");
            }

            UpdateApplicationStatus(applicationUserData.CompanyApplication, UpdateApplicationSteps.SubmitRegistration);
            await _portalRepositories.SaveAsync().ConfigureAwait(false);

            var mailParameters = new Dictionary<string, string>
            {
                { "url", $"{_settings.BasePortalAddress}"},
            };

            if (applicationUserData.Email != null)
            {
                await _mailingService.SendMails(applicationUserData.Email, mailParameters, new List<string> { "SubmitRegistrationTemplate" });
            }
            else
            {
                _logger.LogInformation($"user {iamUserId} has no email-address");
            }

            return true;
        }

        public async IAsyncEnumerable<InvitedUser> GetInvitedUsersAsync(Guid applicationId)
        {
            await foreach (var item in _portalRepositories.GetInstance<IInvitationRepository>().GetInvitedUserDetailsUntrackedAsync(applicationId).ConfigureAwait(false))
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
            _portalRepositories.GetInstance<IDocumentRepository>().GetUploadedDocumentsAsync(applicationId,documentTypeId,iamUserId);

        public async Task<int> SetInvitationStatusAsync(string iamUserId)
        {
            var invitationData = await _portalRepositories.GetInstance<IInvitationRepository>().GetInvitationStatusAsync(iamUserId).ConfigureAwait(false);

            if (invitationData == null)
            {
                throw new ForbiddenException($"iamUserId {iamUserId} is not associated with invitation");
            }

            if (invitationData.InvitationStatusId == InvitationStatusId.CREATED
                || invitationData.InvitationStatusId == InvitationStatusId.PENDING)
            {
                invitationData.InvitationStatusId = InvitationStatusId.ACCEPTED;
            }

            return await _portalRepositories.SaveAsync().ConfigureAwait(false);
        }

        public async Task<RegistrationData> GetRegistrationDataAsync(Guid applicationId, string iamUserId)
        {
            var registrationData = await _portalRepositories.GetInstance<IUserRepository>().GetRegistrationDataUntrackedAsync(applicationId, iamUserId).ConfigureAwait(false);
            if (registrationData == null)
            {
                throw new ForbiddenException($"iamUserId {iamUserId} is not assigned with CompanyApplication {applicationId}");
            }
            return registrationData;
        }

        private void UpdateApplicationStatus(CompanyApplication application, UpdateApplicationSteps type)
        {
            if (application.ApplicationStatusId == CompanyApplicationStatusId.SUBMITTED
                || application.ApplicationStatusId == CompanyApplicationStatusId.CONFIRMED
                || application.ApplicationStatusId == CompanyApplicationStatusId.DECLINED)
            {
                throw new ForbiddenException($"Application is already closed");
            }

            switch(type)
            {
                case UpdateApplicationSteps.CompanyWithAddress:
                {
                    if (application.ApplicationStatusId == CompanyApplicationStatusId.CREATED
                        || application.ApplicationStatusId == CompanyApplicationStatusId.ADD_COMPANY_DATA)
                    {
                        application.ApplicationStatusId = CompanyApplicationStatusId.INVITE_USER;
                    }
                    break;
                }
                case UpdateApplicationSteps.CompanyRoleAgreementConsents:
                {
                    if (application.ApplicationStatusId == CompanyApplicationStatusId.CREATED
                        || application.ApplicationStatusId == CompanyApplicationStatusId.ADD_COMPANY_DATA
                        || application.ApplicationStatusId == CompanyApplicationStatusId.INVITE_USER
                        || application.ApplicationStatusId == CompanyApplicationStatusId.SELECT_COMPANY_ROLE)
                    {
                        application.ApplicationStatusId = CompanyApplicationStatusId.UPLOAD_DOCUMENTS;
                    }
                    break;
                }
                case UpdateApplicationSteps.SubmitRegistration:
                {
                    if (application.ApplicationStatusId == CompanyApplicationStatusId.CREATED
                        || application.ApplicationStatusId == CompanyApplicationStatusId.ADD_COMPANY_DATA
                        || application.ApplicationStatusId == CompanyApplicationStatusId.INVITE_USER
                        || application.ApplicationStatusId == CompanyApplicationStatusId.SELECT_COMPANY_ROLE
                        || application.ApplicationStatusId == CompanyApplicationStatusId.UPLOAD_DOCUMENTS)
                    {
                        throw new ForbiddenException($"Application status is not fitting to the pre-requisite");
                    }
                    else
                    {
                        application.ApplicationStatusId = CompanyApplicationStatusId.SUBMITTED;
                    }
                    break;
                }
            }
        }

        public IAsyncEnumerable<CompanyRolesDetails> GetCompanyRolesAsync(string? languageShortName = null) =>
            _portalRepositories.GetInstance<ICompanyRolesRepository>().GetCompanyRolesAsync(languageShortName);

    }
}
