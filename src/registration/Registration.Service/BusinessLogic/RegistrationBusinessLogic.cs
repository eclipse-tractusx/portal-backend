/********************************************************************************
 * Copyright (c) 2021,2022 Microsoft and BMW Group AG
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

using Microsoft.Extensions.Options;
using Org.CatenaX.Ng.Portal.Backend.Framework.ErrorHandling;
using Org.CatenaX.Ng.Portal.Backend.Mailing.SendMail;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.CatenaX.Ng.Portal.Backend.Provisioning.Library;
using Org.CatenaX.Ng.Portal.Backend.Provisioning.Library.Models;
using Org.CatenaX.Ng.Portal.Backend.Provisioning.Library.Service;
using Org.CatenaX.Ng.Portal.Backend.Registration.Service.Model;
using Org.CatenaX.Ng.Portal.Backend.Registration.Service.Bpn;
using Org.CatenaX.Ng.Portal.Backend.Registration.Service.Bpn.Model;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Org.CatenaX.Ng.Portal.Backend.Registration.Service.BusinessLogic;

public class RegistrationBusinessLogic : IRegistrationBusinessLogic
{
    private readonly RegistrationSettings _settings;
    private readonly IMailingService _mailingService;
    private readonly IBpnAccess _bpnAccess;
    private readonly IProvisioningManager _provisioningManager;
    private readonly IUserProvisioningService _userProvisioningService;
    private readonly IPortalRepositories _portalRepositories;
    private readonly ILogger<RegistrationBusinessLogic> _logger;

    public RegistrationBusinessLogic(
        IOptions<RegistrationSettings> settings,
        IMailingService mailingService,
        IBpnAccess bpnAccess,
        IProvisioningManager provisioningManager,
        IUserProvisioningService userProvisioningService,
        ILogger<RegistrationBusinessLogic> logger,
        IPortalRepositories portalRepositories)
    {
        _settings = settings.Value;
        _mailingService = mailingService;
        _bpnAccess = bpnAccess;
        _provisioningManager = provisioningManager;
        _userProvisioningService = userProvisioningService;
        _logger = logger;
        _portalRepositories = portalRepositories;
    }

    public IAsyncEnumerable<string> GetClientRolesCompositeAsync() =>
        _portalRepositories.GetInstance<IUserRolesRepository>().GetClientRolesCompositeAsync(_settings.KeyCloakClientID);

    public IAsyncEnumerable<FetchBusinessPartnerDto> GetCompanyByIdentifierAsync(string companyIdentifier, string token, CancellationToken cancellationToken)
    {
        var regex = new Regex(@"(\w|\d){16}");
        if (!regex.IsMatch(companyIdentifier))
        {
            throw new ArgumentException("BPN must contain exactly 16 digits or letters.", nameof(companyIdentifier));
        }

        return _bpnAccess.FetchBusinessPartner(companyIdentifier, token, cancellationToken);
    }

    public async Task<int> UploadDocumentAsync(Guid applicationId, IFormFile document, DocumentTypeId documentTypeId, string iamUserId, CancellationToken cancellationToken)
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
            throw new ForbiddenException($"iamUserId {iamUserId} is not assigned with CompanyApplication {applicationId}");
        }

        var documentName = document.FileName;
        using var sha256 = SHA256.Create();
        using var ms = new MemoryStream((int)document.Length);
        
        await document.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
        var hash = sha256.ComputeHash(ms);
        var documentContent = ms.GetBuffer();
        if (ms.Length != document.Length || documentContent.Length != document.Length)
        {
            throw new ArgumentException($"document {document.FileName} transmitted length {document.Length} doesn't match actual length {ms.Length}.");
        }
        
        _portalRepositories.GetInstance<IDocumentRepository>().CreateDocument(documentName, documentContent, hash, documentTypeId, doc =>
        {
            doc.CompanyUserId = companyUserId;
        });
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

    public Task SetCompanyWithAddressAsync(Guid applicationId, CompanyWithAddress companyWithAddress, string iamUserId)
    {
        if (string.IsNullOrWhiteSpace(companyWithAddress.Name))
        {
            throw new ControllerArgumentException("Name must not be empty", nameof(companyWithAddress.Name));
        }
        if (string.IsNullOrWhiteSpace(companyWithAddress.City))
        {
            throw new ControllerArgumentException("City must not be empty", nameof(companyWithAddress.City));
        }
        if (string.IsNullOrWhiteSpace(companyWithAddress.StreetName))
        {
            throw new ControllerArgumentException("Streetname must not be empty", nameof(companyWithAddress.StreetName));
        }
        if (companyWithAddress.CountryAlpha2Code.Length != 2)
        {
            throw new ControllerArgumentException("CountryAlpha2Code must be 2 chars", nameof(companyWithAddress.CountryAlpha2Code));
        }
        return SetCompanyWithAddressInternal(applicationId, companyWithAddress, iamUserId);
    }

    private async Task SetCompanyWithAddressInternal(Guid applicationId, CompanyWithAddress companyWithAddress, string iamUserId)
    {
        var companyApplicationData = await _portalRepositories.GetInstance<IApplicationRepository>()
            .GetCompanyApplicationWithCompanyAdressUserDataAsync(applicationId, companyWithAddress.CompanyId, iamUserId)
            .ConfigureAwait(false);
        if (companyApplicationData == null)
        {
            throw new NotFoundException(
                $"CompanyApplication {applicationId} for CompanyId {companyWithAddress.CompanyId} not found");
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
                companyWithAddress.StreetName,
                companyWithAddress.CountryAlpha2Code
            );
        }
        else
        {
            company.Address.City = companyWithAddress.City;
            company.Address.Streetname = companyWithAddress.StreetName;
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

    public Task<int> InviteNewUserAsync(Guid applicationId, UserCreationInfoWithMessage userCreationInfo, string iamUserId)
    {
        if (string.IsNullOrEmpty(userCreationInfo.eMail))
        {
            throw new ControllerArgumentException($"email must not be empty");
        }
        return InviteNewUserInternalAsync(applicationId, userCreationInfo, iamUserId);
    }

    private async Task<int> InviteNewUserInternalAsync(Guid applicationId, UserCreationInfoWithMessage userCreationInfo, string iamUserId)
    {
        if (await _portalRepositories.GetInstance<IUserRepository>().IsOwnCompanyUserWithEmailExisting(userCreationInfo.eMail, iamUserId))
        {
            throw new ControllerArgumentException($"user with email {userCreationInfo.eMail} does already exist");
        }

        var applicationData = await _portalRepositories.GetInstance<ICompanyRepository>().GetCompanyNameIdWithSharedIdpAliasUntrackedAsync(applicationId, iamUserId).ConfigureAwait(false);
        if (applicationData == default)
        {
            throw new NotFoundException($"application {applicationId} does not exist");
        }
        var (companyId, companyName, idpAlias, creatorId) = applicationData;
        if (creatorId == Guid.Empty)
        {
            throw new ForbiddenException($"user {iamUserId} is not associated with application {applicationId}");
        }
        if (idpAlias == null)
        {
            throw new ConflictException($"shared idp for CompanyApplication {applicationId} not found");
        }

        var companyNameIdpAliasData = new CompanyNameIdpAliasData(
            companyId,
            companyName,
            null,
            creatorId,
            idpAlias,
            true
        );

        IEnumerable<UserRoleData>? userRoleDatas = null;

        if (userCreationInfo.Roles.Any())
        {
            var clientRoles = new Dictionary<string,IEnumerable<string>> {
                { _settings.KeyCloakClientID, userCreationInfo.Roles }
            };
            userRoleDatas = await _userProvisioningService.GetRoleDatas(clientRoles).ToListAsync().ConfigureAwait(false);
        }

        var userCreationInfoIdps = new [] { new UserCreationRoleDataIdpInfo(
            userCreationInfo.firstName ?? "",
            userCreationInfo.lastName ?? "",
            userCreationInfo.eMail,
            userRoleDatas ?? Enumerable.Empty<UserRoleData>(),
            userCreationInfo.userName ?? userCreationInfo.eMail,
            ""
        )}.ToAsyncEnumerable();

        var (companyUserId, _, password, error) = await _userProvisioningService.CreateOwnCompanyIdpUsersAsync(companyNameIdpAliasData, userCreationInfoIdps).SingleAsync().ConfigureAwait(false);

        if (error != null)
        {
            throw error;
        }

        _portalRepositories.GetInstance<IApplicationRepository>().CreateInvitation(applicationId, companyUserId);

        var modified = await _portalRepositories.SaveAsync().ConfigureAwait(false);

        var inviteTemplateName = "invite";
        if (!string.IsNullOrWhiteSpace(userCreationInfo.Message))
        {
            inviteTemplateName = "inviteWithMessage";
        }

        var companyDisplayName = await _userProvisioningService.GetIdentityProviderDisplayName(idpAlias).ConfigureAwait(false);
        
        var mailParameters = new Dictionary<string, string>
        {
            { "password", password },
            { "companyName", companyDisplayName },
            { "message", userCreationInfo.Message ?? "" },
            { "nameCreatedBy", iamUserId },
            { "url", _settings.BasePortalAddress },
            { "username", userCreationInfo.eMail },
        };

        await _mailingService.SendMails(userCreationInfo.eMail, mailParameters, new List<string> { inviteTemplateName, "password" }).ConfigureAwait(false);

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

        ValidateCompanyApplicationStatus(status, applicationUserData);

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
        var consentRepository = _portalRepositories.GetInstance<IConsentRepository>();

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

        var companyRoleAssignedAgreements = await companyRolesRepository.GetAgreementAssignedCompanyRolesUntrackedAsync(companyRoleIdsToSet)
            .ToDictionaryAsync(x => x.CompanyRoleId, x => x.AgreementIds)
            .ConfigureAwait(false);

        var invalidRoles = companyRoleIdsToSet.Except(companyRoleAssignedAgreements.Keys);
        if (invalidRoles.Any())
        {
            throw new ControllerArgumentException($"invalid companyRole: {String.Join(", ", invalidRoles)}");
        }
        if (!companyRoleIdsToSet
            .All(companyRoleIdToSet =>
                companyRoleAssignedAgreements[companyRoleIdToSet].All(assignedAgreementId =>
                    agreementConsentsToSet
                        .Any(agreementConsent =>
                            agreementConsent.AgreementId == assignedAgreementId
                            && agreementConsent.ConsentStatusId == ConsentStatusId.ACTIVE))))
        {
            throw new ControllerArgumentException("consent must be given to all CompanyRole assigned agreements");
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
            consentRepository.CreateConsent(agreementConsentToAdd.AgreementId, companyId, companyUserId, ConsentStatusId.ACTIVE);
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

    public async Task<CompanyRoleAgreementData> GetCompanyRoleAgreementDataAsync() =>
        new CompanyRoleAgreementData(
            (await _portalRepositories.GetInstance<ICompanyRolesRepository>().GetCompanyRoleAgreementsUntrackedAsync().ToListAsync().ConfigureAwait(false)).AsEnumerable(),
            (await _portalRepositories.GetInstance<IAgreementRepository>().GetAgreementsForCompanyRolesUntrackedAsync().ToListAsync().ConfigureAwait(false)).AsEnumerable()
        );

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
            _logger.LogInformation("user {IamUserId} has no email-address", iamUserId);
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

    public IAsyncEnumerable<CompanyRolesDetails> GetCompanyRoles(string? languageShortName = null) =>
        _portalRepositories.GetInstance<ICompanyRolesRepository>().GetCompanyRolesAsync(languageShortName);

    private static void ValidateCompanyApplicationStatus(CompanyApplicationStatusId status,
        CompanyApplicationUserData applicationUserData)
    {
        var application = applicationUserData.CompanyApplication;
        var allowedCombination = new List<(CompanyApplicationStatusId applicationStatus, CompanyApplicationStatusId status)>
        {
            new(CompanyApplicationStatusId.CREATED, CompanyApplicationStatusId.ADD_COMPANY_DATA),
            new(CompanyApplicationStatusId.ADD_COMPANY_DATA, CompanyApplicationStatusId.INVITE_USER),
            new(CompanyApplicationStatusId.INVITE_USER, CompanyApplicationStatusId.SELECT_COMPANY_ROLE),
            new(CompanyApplicationStatusId.SELECT_COMPANY_ROLE, CompanyApplicationStatusId.UPLOAD_DOCUMENTS),
            new(CompanyApplicationStatusId.UPLOAD_DOCUMENTS, CompanyApplicationStatusId.VERIFY),
            new(CompanyApplicationStatusId.VERIFY, CompanyApplicationStatusId.SUBMITTED),
        };

        if (!allowedCombination.Any(x =>
                x.applicationStatus == application.ApplicationStatusId &&
                x.status == status))
        {
            throw new ArgumentException(
                $"invalid status update requested {status}, current status is {application.ApplicationStatusId}, possible values are: {CompanyApplicationStatusId.SUBMITTED}");
        }

        application.ApplicationStatusId = status;
    }

    private static void UpdateApplicationStatus(CompanyApplication application, UpdateApplicationSteps type)
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

                application.ApplicationStatusId = CompanyApplicationStatusId.SUBMITTED;
                break;
            }
        }
    }

    public async Task<bool> DeleteRegistrationDocumentAsync(Guid documentId, string iamUserId)
    {
        if (documentId == Guid.Empty)
        {
            throw new ControllerArgumentException($"documentId must not be empty");
        }
        var documentRepository = _portalRepositories.GetInstance<IDocumentRepository>();
        var details = await documentRepository.GetDocumentDetailsForApplicationUntrackedAsync(documentId, iamUserId, _settings.ApplicationStatusIds).ConfigureAwait(false);
        if (details == default)
        {
            throw new NotFoundException("Document does not exist.");
        }
        if (!_settings.DocumentTypeIds.Contains(details.documentTypeId))
        {
            throw new ConflictException($"Document deletion is not allowed. DocumentType must be either :{string.Join(",", _settings.DocumentTypeIds)}");
        }
        if (details.IsQueriedApplicationStatus)
        {
            throw new ConflictException("Document deletion is not allowed. Application is already closed.");
        }
        if (!details.IsSameApplicationUser)
        {
            throw new ForbiddenException("User is not allowed to delete this document");
        }
        if (details.DocumentStatusId != DocumentStatusId.PENDING)
        {
            throw new ConflictException("Document deletion is not allowed. The document is locked.");
        }

        documentRepository.RemoveDocument(details.DocumentId);

        await this._portalRepositories.SaveAsync().ConfigureAwait(false);
        return true;
    }

}
