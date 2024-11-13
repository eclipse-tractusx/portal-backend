/********************************************************************************
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library;
using Org.Eclipse.TractusX.Portal.Backend.Bpdm.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.DateTimeProvider;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Linq;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Web;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;
using Org.Eclipse.TractusX.Portal.Backend.Processes.ApplicationChecklist.Library;
using Org.Eclipse.TractusX.Portal.Backend.Processes.Mailing.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Common;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Model;
using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace Org.Eclipse.TractusX.Portal.Backend.Registration.Service.BusinessLogic;

public class RegistrationBusinessLogic(
    IOptions<RegistrationSettings> settings,
    IBpnAccess bpnAccess,
    IUserProvisioningService userProvisioningService,
    ILogger<RegistrationBusinessLogic> logger,
    IPortalRepositories portalRepositories,
    IApplicationChecklistCreationService checklistService,
    IIdentityService identityService,
    IDateTimeProvider dateTimeProvider,
    IMailingProcessCreation mailingProcessCreation) : IRegistrationBusinessLogic
{
    private readonly IIdentityData _identityData = identityService.IdentityData;
    private readonly RegistrationSettings _settings = settings.Value;

    private static readonly Regex bpnRegex = new(@"(\w|\d){16}", RegexOptions.None, TimeSpan.FromSeconds(1));

    public IAsyncEnumerable<string> GetClientRolesCompositeAsync() =>
        portalRepositories.GetInstance<IUserRolesRepository>().GetClientRolesCompositeAsync(_settings.KeycloakClientID);

    public Task<CompanyBpdmDetailData> GetCompanyBpdmDetailDataByBusinessPartnerNumber(string businessPartnerNumber, string token, CancellationToken cancellationToken)
    {
        if (!bpnRegex.IsMatch(businessPartnerNumber))
        {
            throw new ControllerArgumentException("BPN must contain exactly 16 digits or letters.", nameof(businessPartnerNumber));
        }
        return GetCompanyBpdmDetailDataByBusinessPartnerNumberInternal(businessPartnerNumber.ToUpper(), token, cancellationToken);
    }

    private async Task<CompanyBpdmDetailData> GetCompanyBpdmDetailDataByBusinessPartnerNumberInternal(string businessPartnerNumber, string token, CancellationToken cancellationToken)
    {
        var legalEntity = await bpnAccess.FetchLegalEntityByBpn(businessPartnerNumber, token, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        if (!businessPartnerNumber.Equals(legalEntity.Bpn, StringComparison.OrdinalIgnoreCase))
        {
            throw new ConflictException("Bpdm did return incorrect bpn legal-entity-data");
        }

        var country = legalEntity.LegalEntityAddress?.PhysicalPostalAddress?.Country?.TechnicalKey ??
                      throw new ConflictException("Legal-entity-data did not contain a valid country identifier");

        var bpdmIdentifiers = ParseBpdmIdentifierDtos(legalEntity.Identifiers).ToList();
        var assignedIdentifiersResult = await portalRepositories.GetInstance<IStaticDataRepository>()
            .GetCountryAssignedIdentifiers(bpdmIdentifiers.Select(x => x.BpdmIdentifierId), country).ConfigureAwait(ConfigureAwaitOptions.None);

        if (!assignedIdentifiersResult.IsValidCountry)
        {
            throw new ConflictException($"Bpdm did return invalid country {country} in address-data");
        }

        var portalIdentifiers = assignedIdentifiersResult.Identifiers.Join(
                bpdmIdentifiers,
                assignedIdentifier => assignedIdentifier.BpdmIdentifierId,
                bpdmIdentifier => bpdmIdentifier.BpdmIdentifierId,
                (countryIdentifier, bpdmIdentifier) => (countryIdentifier.UniqueIdentifierId, bpdmIdentifier.Value));

        var physicalPostalAddress = legalEntity.LegalEntityAddress.PhysicalPostalAddress;
        return new CompanyBpdmDetailData(
            businessPartnerNumber,
            country,
            legalEntity.LegalName ?? "",
            legalEntity.LegalShortName ?? "",
            physicalPostalAddress?.City ?? "",
            physicalPostalAddress?.Street?.Name ?? "",
            physicalPostalAddress?.District,
            null,
            physicalPostalAddress?.Street?.HouseNumber,
            physicalPostalAddress?.PostalCode,
            portalIdentifiers.Select(identifier => new CompanyUniqueIdData(identifier.UniqueIdentifierId, identifier.Value))
        );
    }

    private static IEnumerable<(BpdmIdentifierId BpdmIdentifierId, string Value)> ParseBpdmIdentifierDtos(IEnumerable<BpdmIdentifierDto> bpdmIdentifierDtos) =>
        bpdmIdentifierDtos
            .Select(dto => (Valid: Enum.TryParse<BpdmIdentifierId>(dto.Type.TechnicalKey, out var bpdmIdentifierId), IdentifierId: bpdmIdentifierId, dto.Value))
            .Where(x => x.Valid)
            .Select(x => (x.IdentifierId, x.Value));

    public async Task UploadDocumentAsync(Guid applicationId, IFormFile document, DocumentTypeId documentTypeId, CancellationToken cancellationToken)
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

        if (!_settings.DocumentTypeIds.Contains(documentTypeId))
        {
            throw new ControllerArgumentException($"documentType must be either: {string.Join(",", _settings.DocumentTypeIds)}");
        }

        var validApplicationForCompany = await portalRepositories.GetInstance<IApplicationRepository>().IsValidApplicationForCompany(applicationId, _identityData.CompanyId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (!validApplicationForCompany)
        {
            throw new ForbiddenException($"The users company is not assigned with application {applicationId}");
        }

        portalRepositories.GetInstance<IApplicationRepository>().AttachAndModifyCompanyApplication(applicationId, application =>
        {
            application.DateLastChanged = dateTimeProvider.OffsetNow;
        });
        var mediaTypeId = document.ContentType.ParseMediaTypeId();
        var (content, hash) = await document.GetContentAndHash(cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);

        portalRepositories.GetInstance<IDocumentRepository>().CreateDocument(document.FileName, content, hash, mediaTypeId, documentTypeId, doc =>
        {
            doc.CompanyUserId = _identityData.IdentityId;
        });

        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public async Task<(string FileName, byte[] Content, string MediaType)> GetDocumentContentAsync(Guid documentId)
    {
        var documentRepository = portalRepositories.GetInstance<IDocumentRepository>();
        var documentDetails = await documentRepository.GetDocumentIdWithCompanyUserCheckAsync(documentId, _identityData.IdentityId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (documentDetails.DocumentId == Guid.Empty)
        {
            throw new NotFoundException($"document {documentId} does not exist.");
        }

        if (!documentDetails.IsSameUser && !documentDetails.IsRoleOperator)
        {
            throw new ForbiddenException($"The user is not permitted to access document {documentId}.");
        }

        if (documentDetails.IsStatusConfirmed)
        {
            throw new ForbiddenException($"Documents not accessible as onboarding process finished {documentId}.");
        }

        var document = await documentRepository.GetDocumentByIdAsync(documentId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (document is null)
        {
            throw new NotFoundException($"document {documentId} does not exist.");
        }
        return (document.DocumentName, document.DocumentContent, document.MediaTypeId.MapToMediaType());
    }

    public IAsyncEnumerable<CompanyApplicationWithStatus> GetAllApplicationsForUserWithStatus() =>
        portalRepositories.GetInstance<IUserRepository>().GetApplicationsWithStatusUntrackedAsync(_identityData.CompanyId);

    public async Task<IEnumerable<CompanyApplicationDeclineData>> GetApplicationsDeclineData()
    {
        var data = await portalRepositories.GetInstance<IApplicationRepository>().GetCompanyApplicationsDeclineData(_identityData.IdentityId, _settings.ApplicationDeclineStatusIds).ConfigureAwait(ConfigureAwaitOptions.None);
        var user = NameHelper.CreateNameString(data.FirstName, data.LastName, data.Email, "unknown user");

        return data.Applications.Select(application =>
            new CompanyApplicationDeclineData(
                application.ApplicationId,
                application.ApplicationStatusId,
                user,
                data.CompanyName,
                application.InvitedUsers.Select(user => NameHelper.CreateNameString(user.FirstName, user.LastName, user.Email, "unknown user"))
        ));
    }

    public async Task<CompanyDetailData> GetCompanyDetailData(Guid applicationId)
    {
        var result = await portalRepositories.GetInstance<IApplicationRepository>().GetCompanyApplicationDetailDataAsync(applicationId, _identityData.CompanyId, null).ConfigureAwait(ConfigureAwaitOptions.None);
        if (result == null)
        {
            throw new NotFoundException($"CompanyApplication {applicationId} not found");
        }
        if (!result.IsUserOfCompany)
        {
            throw new ForbiddenException($"The users company is not assigned with CompanyApplication {applicationId}");
        }
        return new CompanyDetailData(
            result.CompanyId,
            result.Name,
            result.City ?? "",
            result.Streetname ?? "",
            result.CountryAlpha2Code ?? "",
            result.BusinessPartnerNumber,
            result.ShortName,
            result.Region,
            result.Streetadditional,
            result.Streetnumber,
            result.Zipcode,
            result.UniqueIds.Select(id => new CompanyUniqueIdData(id.UniqueIdentifierId, id.Value))
        );
    }

    public Task SetCompanyDetailDataAsync(Guid applicationId, CompanyDetailData companyDetails)
    {
        companyDetails.ValidateData();
        return SetCompanyDetailDataInternal(applicationId, companyDetails);
    }

    private async Task SetCompanyDetailDataInternal(Guid applicationId, CompanyDetailData companyDetails)
    {
        await companyDetails.ValidateDatabaseData(
            bpn => portalRepositories.GetInstance<ICompanyRepository>().CheckBpnExists(bpn),
            alpha2Code => portalRepositories.GetInstance<ICountryRepository>()
                .CheckCountryExistsByAlpha2CodeAsync(alpha2Code),
            (countryAlpha2Code, uniqueIdentifierIds) =>
                portalRepositories.GetInstance<ICountryRepository>()
                    .GetCountryAssignedIdentifiers(
                        countryAlpha2Code,
                        uniqueIdentifierIds),
            false).ConfigureAwait(ConfigureAwaitOptions.None);

        var applicationRepository = portalRepositories.GetInstance<IApplicationRepository>();
        var companyRepository = portalRepositories.GetInstance<ICompanyRepository>();

        var companyApplicationData = await GetAndValidateApplicationData(applicationId, companyDetails, applicationRepository).ConfigureAwait(ConfigureAwaitOptions.None);

        var addressId = CreateOrModifyAddress(companyApplicationData, companyDetails, companyRepository);

        ModifyCompany(addressId, companyApplicationData, companyDetails, companyRepository);

        companyRepository.CreateUpdateDeleteIdentifiers(companyDetails.CompanyId, companyApplicationData.UniqueIds, companyDetails.UniqueIds.Select(x => (x.UniqueIdentifierId, x.Value)));

        UpdateApplicationStatus(applicationId, companyApplicationData.ApplicationStatusId, UpdateApplicationSteps.CompanyWithAddress, applicationRepository, dateTimeProvider);

        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    private async Task<CompanyApplicationDetailData> GetAndValidateApplicationData(Guid applicationId, CompanyDetailData companyDetails, IApplicationRepository applicationRepository)
    {
        var companyApplicationData = await applicationRepository
            .GetCompanyApplicationDetailDataAsync(applicationId, _identityData.CompanyId, companyDetails.CompanyId)
            .ConfigureAwait(ConfigureAwaitOptions.None);

        if (companyApplicationData == null)
        {
            throw new NotFoundException(
                $"CompanyApplication {applicationId} for CompanyId {companyDetails.CompanyId} not found");
        }

        if (!companyApplicationData.IsUserOfCompany)
        {
            throw new ForbiddenException($"users company is not assigned with CompanyApplication {applicationId}");
        }
        return companyApplicationData;
    }

    private static Guid CreateOrModifyAddress(CompanyApplicationDetailData initialData, CompanyDetailData modifyData, ICompanyRepository companyRepository)
    {
        if (initialData.AddressId.HasValue)
        {
            companyRepository.AttachAndModifyAddress(
                initialData.AddressId.Value,
                a =>
                {
                    a.City = initialData.City!;
                    a.Streetname = initialData.Streetname!;
                    a.CountryAlpha2Code = initialData.CountryAlpha2Code!;
                    a.Zipcode = initialData.Zipcode;
                    a.Region = initialData.Region;
                    a.Streetadditional = initialData.Streetadditional;
                    a.Streetnumber = initialData.Streetnumber;
                },
                a =>
                {
                    a.City = modifyData.City;
                    a.Streetname = modifyData.StreetName;
                    a.CountryAlpha2Code = modifyData.CountryAlpha2Code;
                    a.Zipcode = modifyData.ZipCode;
                    a.Region = modifyData.Region;
                    a.Streetadditional = modifyData.StreetAdditional;
                    a.Streetnumber = modifyData.StreetNumber;
                }
            );
            return initialData.AddressId.Value;
        }
        else
        {
            return companyRepository.CreateAddress(
                modifyData.City,
                modifyData.StreetName,
                modifyData.CountryAlpha2Code,
                a =>
                {
                    a.Zipcode = modifyData.ZipCode;
                    a.Region = modifyData.Region;
                    a.Streetadditional = modifyData.StreetAdditional;
                    a.Streetnumber = modifyData.StreetNumber;
                }
            ).Id;
        }
    }

    private static void ModifyCompany(Guid addressId, CompanyApplicationDetailData initialData, CompanyDetailData modifyData, ICompanyRepository companyRepository) =>
        companyRepository.AttachAndModifyCompany(
            modifyData.CompanyId,
            c =>
            {
                c.BusinessPartnerNumber = initialData.BusinessPartnerNumber?.ToUpper();
                c.Name = initialData.Name;
                c.Shortname = initialData.ShortName;
                c.CompanyStatusId = initialData.CompanyStatusId;
                c.AddressId = initialData.AddressId;
            },
            c =>
            {
                c.BusinessPartnerNumber = string.IsNullOrEmpty(modifyData.BusinessPartnerNumber) ? null : modifyData.BusinessPartnerNumber?.ToUpper();
                c.Name = modifyData.Name;
                c.Shortname = modifyData.ShortName;
                c.CompanyStatusId = CompanyStatusId.PENDING;
                c.AddressId = addressId;
            });

    public Task<int> InviteNewUserAsync(Guid applicationId, UserCreationInfoWithMessage userCreationInfo)
    {
        if (string.IsNullOrEmpty(userCreationInfo.eMail))
        {
            throw new ControllerArgumentException($"email must not be empty");
        }
        return InviteNewUserInternalAsync(applicationId, userCreationInfo);
    }

    private async Task<int> InviteNewUserInternalAsync(Guid applicationId, UserCreationInfoWithMessage userCreationInfo)
    {
        if (await portalRepositories.GetInstance<IUserRepository>().IsOwnCompanyUserWithEmailExisting(userCreationInfo.eMail, _identityData.CompanyId))
        {
            throw new ControllerArgumentException($"user with email {userCreationInfo.eMail} does already exist");
        }

        var (companyNameIdpAliasData, createdByName) = await userProvisioningService.GetCompanyNameSharedIdpAliasData(_identityData.IdentityId, applicationId).ConfigureAwait(ConfigureAwaitOptions.None);

        IEnumerable<UserRoleData>? userRoleDatas = null;

        if (userCreationInfo.Roles.Any())
        {
            var clientRoles = new[] { new UserRoleConfig(_settings.KeycloakClientID, userCreationInfo.Roles) };
            userRoleDatas = await userProvisioningService.GetRoleDatas(clientRoles).ToListAsync().ConfigureAwait(false);
        }

        var userCreationInfoIdps = new[] { new UserCreationRoleDataIdpInfo(
            userCreationInfo.firstName ?? "",
            userCreationInfo.lastName ?? "",
            userCreationInfo.eMail,
            userRoleDatas ?? Enumerable.Empty<UserRoleData>(),
            userCreationInfo.userName ?? userCreationInfo.eMail,
            "",
            UserStatusId.ACTIVE,
            true
        )}.ToAsyncEnumerable();

        var (newCompanyUserId, _, password, error) = await userProvisioningService.CreateOwnCompanyIdpUsersAsync(companyNameIdpAliasData, userCreationInfoIdps).SingleAsync().ConfigureAwait(false);

        if (error != null)
        {
            throw error;
        }

        portalRepositories.GetInstance<IApplicationRepository>().CreateInvitation(applicationId, newCompanyUserId);
        portalRepositories.GetInstance<IApplicationRepository>().AttachAndModifyCompanyApplication(applicationId, application =>
        {
            application.DateLastChanged = dateTimeProvider.OffsetNow;
        });

        var inviteTemplateName = "invite";
        if (!string.IsNullOrWhiteSpace(userCreationInfo.Message))
        {
            inviteTemplateName = "inviteWithMessage";
        }

        var modified = await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);

        var companyDisplayName = await userProvisioningService.GetIdentityProviderDisplayName(companyNameIdpAliasData.IdpAlias).ConfigureAwait(ConfigureAwaitOptions.None) ?? companyNameIdpAliasData.IdpAlias;
        var mailParameters = ImmutableDictionary.CreateRange(new[]
        {
            KeyValuePair.Create("password", password),
            KeyValuePair.Create("companyName", companyDisplayName),
            KeyValuePair.Create("message", userCreationInfo.Message ?? ""),
            KeyValuePair.Create("nameCreatedBy", createdByName),
            KeyValuePair.Create("url", _settings.BasePortalAddress),
            KeyValuePair.Create("passwordResendUrl", _settings.PasswordResendAddress),
            KeyValuePair.Create("username", userCreationInfo.eMail),
        });

        mailingProcessCreation.CreateMailProcess(userCreationInfo.eMail, inviteTemplateName, mailParameters);
        mailingProcessCreation.CreateMailProcess(userCreationInfo.eMail, "password", mailParameters);
        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
        return modified;
    }

    public async Task<int> SetOwnCompanyApplicationStatusAsync(Guid applicationId, CompanyApplicationStatusId status)
    {
        if (status == 0)
        {
            throw new ControllerArgumentException("status must not be null");
        }

        var applicationRepository = portalRepositories.GetInstance<IApplicationRepository>();
        var applicationUserData = await applicationRepository.GetOwnCompanyApplicationUserDataAsync(applicationId, _identityData.CompanyId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (!applicationUserData.Exists)
        {
            throw new NotFoundException($"CompanyApplication {applicationId} not found");
        }

        if (applicationUserData.StatusId != status)
        {
            ValidateCompanyApplicationStatus(applicationId, status, applicationUserData, applicationRepository, dateTimeProvider);
            return await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
        }

        return 0;
    }

    public async Task<CompanyApplicationStatusId> GetOwnCompanyApplicationStatusAsync(Guid applicationId)
    {
        var result = await portalRepositories.GetInstance<IApplicationRepository>().GetOwnCompanyApplicationStatusUserDataUntrackedAsync(applicationId, _identityData.CompanyId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (!result.Exists)
        {
            throw new NotFoundException($"CompanyApplication {applicationId} not found");
        }
        if (!result.IsUserOfCompany)
        {
            throw new ForbiddenException($"users company is not associated with application {applicationId}");
        }
        return result.ApplicationStatus;
    }

    public async Task<int> SubmitRoleConsentAsync(Guid applicationId, CompanyRoleAgreementConsents roleAgreementConsentStatuses)
    {
        var companyRoleIdsToSet = roleAgreementConsentStatuses.CompanyRoleIds;
        var agreementConsentsToSet = roleAgreementConsentStatuses.AgreementConsentStatuses;

        var companyRolesRepository = portalRepositories.GetInstance<ICompanyRolesRepository>();
        var consentRepository = portalRepositories.GetInstance<IConsentRepository>();

        var companyRoleAgreementConsentData = await companyRolesRepository.GetCompanyRoleAgreementConsentDataAsync(applicationId).ConfigureAwait(ConfigureAwaitOptions.None);

        if (companyRoleAgreementConsentData == null)
        {
            throw new NotFoundException($"application {applicationId} does not exist");
        }

        var companyId = _identityData.CompanyId;
        var userId = _identityData.IdentityId;
        var (applicationCompanyId, applicationStatusId, companyAssignedRoleIds, consents) = companyRoleAgreementConsentData;
        if (applicationCompanyId != companyId)
        {
            throw new ForbiddenException($"The users company is not assigned with CompanyApplication {applicationId}");
        }

        var companyRoleAssignedAgreements = await companyRolesRepository.GetAgreementAssignedCompanyRolesUntrackedAsync(companyRoleIdsToSet)
            .ToDictionaryAsync(x => x.CompanyRoleId, x => x.AgreementStatusData)
            .ConfigureAwait(false);

        var invalidRoles = companyRoleIdsToSet.Except(companyRoleAssignedAgreements.Keys);
        if (invalidRoles.Any())
        {
            throw new ControllerArgumentException($"invalid companyRole: {string.Join(", ", invalidRoles)}");
        }
        if (!companyRoleIdsToSet
            .All(companyRoleIdToSet =>
                companyRoleAssignedAgreements[companyRoleIdToSet].All(assignedAgreementId =>
                    agreementConsentsToSet
                        .Any(agreementConsent =>
                            agreementConsent.AgreementId == assignedAgreementId.AgreementId
                            && agreementConsent.ConsentStatusId == ConsentStatusId.ACTIVE))))
        {
            throw new ControllerArgumentException("consent must be given to all CompanyRole assigned agreements");
        }

        var extraAgreement = agreementConsentsToSet.ExceptBy(companyRoleAssignedAgreements.SelectMany(x => x.Value).Select(x => x.AgreementId), x => x.AgreementId);
        if (extraAgreement.Any())
        {
            throw new ControllerArgumentException($"Agreements which not associated with requested companyRoles: {string.Join(", ", extraAgreement.Select(x => x.AgreementId))}");
        }

        companyRolesRepository.RemoveCompanyAssignedRoles(companyId, companyAssignedRoleIds.Except(companyRoleIdsToSet));

        foreach (var companyRoleId in companyRoleIdsToSet.Except(companyAssignedRoleIds))
        {
            companyRolesRepository.CreateCompanyAssignedRole(companyId, companyRoleId);
        }

        HandleConsent(consents, agreementConsentsToSet.ExceptBy(companyRoleAssignedAgreements.SelectMany(x => x.Value).Where(x => x.AgreementStatusId == AgreementStatusId.INACTIVE).Select(x => x.AgreementId), x => x.AgreementId), consentRepository, companyId, userId);
        UpdateApplicationStatus(applicationId, applicationStatusId, UpdateApplicationSteps.CompanyRoleAgreementConsents, portalRepositories.GetInstance<IApplicationRepository>(), dateTimeProvider);

        return await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public async Task<CompanyRoleAgreementConsents> GetRoleAgreementConsentsAsync(Guid applicationId)
    {
        var result = await portalRepositories.GetInstance<ICompanyRolesRepository>().GetCompanyRoleAgreementConsentStatusUntrackedAsync(applicationId, _identityData.CompanyId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (result == null)
        {
            throw new ForbiddenException($"user is not assigned with CompanyApplication {applicationId}");
        }
        return result;
    }

    public async Task<CompanyRoleAgreementData> GetCompanyRoleAgreementDataAsync() =>
        new(
            (await portalRepositories.GetInstance<ICompanyRolesRepository>().GetCompanyRoleAgreementsUntrackedAsync().ToListAsync().ConfigureAwait(false)).AsEnumerable(),
            (await portalRepositories.GetInstance<IAgreementRepository>().GetAgreementsForCompanyRolesUntrackedAsync().ToListAsync().ConfigureAwait(false)).AsEnumerable()
        );

    public async Task<bool> SubmitRegistrationAsync(Guid applicationId)
    {
        var applicationUserData = await GetAndValidateCompanyDataDetails(applicationId, _settings.SubmitDocumentTypeIds).ConfigureAwait(false);

        if (GetAndValidateUpdateApplicationStatus(applicationUserData.CompanyApplicationStatusId, UpdateApplicationSteps.SubmitRegistration) != CompanyApplicationStatusId.SUBMITTED)
        {
            throw new UnexpectedConditionException("updateStatus should always be SUBMITTED here");
        }

        if (applicationUserData.DocumentDatas != null && applicationUserData.DocumentDatas.Any())
        {
            portalRepositories.GetInstance<IDocumentRepository>().AttachAndModifyDocuments(
               applicationUserData.DocumentDatas.Select(x => new ValueTuple<Guid, Action<Document>?, Action<Document>>(
                   x.DocumentId,
                   doc => doc.DocumentStatusId = x.StatusId,
                   doc => doc.DocumentStatusId = DocumentStatusId.LOCKED)));
        }

        var entries = await checklistService.CreateInitialChecklistAsync(applicationId);

        var process = portalRepositories.GetInstance<IProcessStepRepository>().CreateProcess(ProcessTypeId.APPLICATION_CHECKLIST);

        portalRepositories.GetInstance<IProcessStepRepository>()
            .CreateProcessStepRange(
                checklistService
                    .GetInitialProcessStepTypeIds(entries)
                    .Select(processStepTypeId => (processStepTypeId, ProcessStepStatusId.TODO, process.Id)));

        portalRepositories.GetInstance<IApplicationRepository>()
            .AttachAndModifyCompanyApplication(
                applicationId,
                application =>
                {
                    application.ApplicationStatusId = CompanyApplicationStatusId.SUBMITTED;
                    application.ChecklistProcessId = process.Id;
                    application.DateLastChanged = dateTimeProvider.OffsetNow;
                });

        var mailParameters = ImmutableDictionary.CreateRange(new[]
        {
            KeyValuePair.Create("url", $"{_settings.BasePortalAddress}"),
        });

        if (applicationUserData.Email != null)
        {
            mailingProcessCreation.CreateMailProcess(applicationUserData.Email, "SubmitRegistrationTemplate", mailParameters);
        }
        else
        {
            logger.LogInformation("user {userId} has no email-address", _identityData.IdentityId);
        }

        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);

        return true;
    }

    private async ValueTask<CompanyApplicationUserEmailData> GetAndValidateCompanyDataDetails(Guid applicationId, IEnumerable<DocumentTypeId> docTypeIds)
    {
        var userId = _identityData.IdentityId;
        var applicationUserData = await portalRepositories.GetInstance<IApplicationRepository>()
            .GetOwnCompanyApplicationUserEmailDataAsync(applicationId, userId, docTypeIds).ConfigureAwait(ConfigureAwaitOptions.None);

        if (applicationUserData == null)
        {
            throw new NotFoundException($"application {applicationId} does not exist");
        }
        if (!applicationUserData.IsApplicationCompanyUser)
        {
            throw new ForbiddenException($"userId {userId} is not associated with CompanyApplication {applicationId}");
        }
        if (string.IsNullOrWhiteSpace(applicationUserData.CompanyData.Name))
        {
            throw new ConflictException($"Company Name must not be empty");
        }
        if (applicationUserData.CompanyData.AddressId == null)
        {
            throw new ConflictException($"Address must not be empty");
        }
        if (string.IsNullOrWhiteSpace(applicationUserData.CompanyData.Streetname))
        {
            throw new ConflictException($"Street Name must not be empty");
        }
        if (string.IsNullOrWhiteSpace(applicationUserData.CompanyData.City))
        {
            throw new ConflictException($"City must not be empty");
        }
        if (string.IsNullOrWhiteSpace(applicationUserData.CompanyData.Country))
        {
            throw new ConflictException($"Country must not be empty");
        }
        if (!applicationUserData.CompanyData.UniqueIds.Any())
        {
            throw new ConflictException($"Company Identifiers [{string.Join(", ", applicationUserData.CompanyData.UniqueIds)}] must not be empty");
        }
        if (!applicationUserData.CompanyData.CompanyRoleIds.Any())
        {
            throw new ConflictException($"Company assigned role [{string.Join(", ", applicationUserData.CompanyData.CompanyRoleIds)}] must not be empty");
        }
        if (!applicationUserData.AgreementConsentStatuses.Any())
        {
            throw new ConflictException($"Agreement and Consent must not be empty");
        }
        if (!_settings.IsSubmitDocumentValidationOptional && !applicationUserData.DocumentDatas.Any())
        {
            throw new ConflictException($"At least one Document type Id must be [{string.Join(", ", docTypeIds)}]");
        }
        return applicationUserData;
    }

    public IAsyncEnumerable<InvitedUser> GetInvitedUsersAsync(Guid applicationId) =>
        portalRepositories.GetInstance<IInvitationRepository>()
            .GetInvitedUserDetailsUntrackedAsync(applicationId)
            .Select(x =>
                new InvitedUser(
                    x.InvitationStatus,
                    x.EmailId,
                    x.Roles));

    public async Task<IEnumerable<UploadDocuments>> GetUploadedDocumentsAsync(Guid applicationId, DocumentTypeId documentTypeId)
    {
        var result = await portalRepositories.GetInstance<IDocumentRepository>().GetUploadedDocumentsAsync(applicationId, documentTypeId, _identityData.IdentityId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (result == default)
        {
            throw new NotFoundException($"application {applicationId} not found");
        }
        if (!result.IsApplicationAssignedUser)
        {
            throw new ForbiddenException($"The user is not associated with application {applicationId}");
        }
        return result.Documents;
    }

    public async Task<int> SetInvitationStatusAsync()
    {
        var invitationData = await portalRepositories.GetInstance<IInvitationRepository>().GetInvitationStatusAsync(_identityData.IdentityId).ConfigureAwait(ConfigureAwaitOptions.None);

        if (invitationData == null)
        {
            throw new ForbiddenException($"user is not associated with invitation");
        }

        if (invitationData.InvitationStatusId is InvitationStatusId.CREATED or InvitationStatusId.PENDING)
        {
            invitationData.InvitationStatusId = InvitationStatusId.ACCEPTED;
        }
        portalRepositories.GetInstance<IApplicationRepository>().AttachAndModifyCompanyApplication(invitationData.CompanyApplicationId, application =>
        {
            application.DateLastChanged = dateTimeProvider.OffsetNow;
        });
        return await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public async Task<CompanyRegistrationData> GetRegistrationDataAsync(Guid applicationId)
    {
        var (isValidApplicationId, isValidCompany, data) = await portalRepositories.GetInstance<IApplicationRepository>().GetRegistrationDataUntrackedAsync(applicationId, _identityData.CompanyId, _settings.DocumentTypeIds).ConfigureAwait(ConfigureAwaitOptions.None);
        if (!isValidApplicationId)
        {
            throw new NotFoundException($"application {applicationId} does not exist");
        }
        if (!isValidCompany)
        {
            throw new ForbiddenException($"The users company is not assigned with CompanyApplication {applicationId}");
        }
        if (data == null)
        {
            throw new UnexpectedConditionException($"registrationData should never be null for application {applicationId}");
        }
        return new CompanyRegistrationData(
            data.CompanyId,
            data.Name,
            data.BusinessPartnerNumber,
            data.ShortName,
            data.City,
            data.Region,
            data.StreetAdditional,
            data.StreetName,
            data.StreetNumber,
            data.ZipCode,
            data.CountryAlpha2Code,
            data.CompanyRoleIds,
            data.AgreementConsentStatuses.Select(consentStatus => new AgreementConsentStatusForRegistrationData(consentStatus.AgreementId, consentStatus.ConsentStatusId)),
            data.DocumentNames.Select(name => new RegistrationDocumentNames(name)),
            data.Identifiers.Select(identifier => new CompanyUniqueIdData(identifier.UniqueIdentifierId, identifier.Value))
        );
    }

    public IAsyncEnumerable<CompanyRolesDetails> GetCompanyRoles(string? languageShortName = null) =>
        portalRepositories.GetInstance<ICompanyRolesRepository>().GetCompanyRolesAsync(languageShortName);

    private static void HandleConsent(IEnumerable<ConsentData> consents, IEnumerable<AgreementConsentStatus> agreementConsentsToSet,
        IConsentRepository consentRepository, Guid companyId, Guid userId)
    {
        var consentsToInactivate = consents
            .Where(consent =>
                !agreementConsentsToSet.Any(agreementConsent =>
                    agreementConsent.AgreementId == consent.AgreementId
                    && agreementConsent.ConsentStatusId == ConsentStatusId.ACTIVE));
        consentRepository.AttachAndModifiesConsents(consentsToInactivate.Select(x => x.ConsentId), consent =>
        {
            consent.ConsentStatusId = ConsentStatusId.INACTIVE;
        });

        var consentsToActivate = consents
            .Where(consent =>
                agreementConsentsToSet.Any(agreementConsent =>
                    agreementConsent.AgreementId == consent.AgreementId &&
                    consent.ConsentStatusId == ConsentStatusId.INACTIVE &&
                    agreementConsent.ConsentStatusId == ConsentStatusId.ACTIVE));
        consentRepository.AttachAndModifiesConsents(consentsToActivate.Select(x => x.ConsentId), consent =>
        {
            consent.ConsentStatusId = ConsentStatusId.ACTIVE;
        });

        foreach (var agreementConsentToAdd in agreementConsentsToSet
                     .Where(agreementConsent =>
                         agreementConsent.ConsentStatusId == ConsentStatusId.ACTIVE
                         && !consents.Any(activeConsent =>
                             activeConsent.AgreementId == agreementConsent.AgreementId)))
        {
            consentRepository.CreateConsent(agreementConsentToAdd.AgreementId, companyId, userId,
                ConsentStatusId.ACTIVE);
        }
    }

    private static void ValidateCompanyApplicationStatus(Guid applicationId,
        CompanyApplicationStatusId status,
        (bool Exists, CompanyApplicationStatusId StatusId) applicationData,
        IApplicationRepository applicationRepository, IDateTimeProvider dateTimeProvider)
    {
        var allowedCombination = new (CompanyApplicationStatusId applicationStatus, CompanyApplicationStatusId status)[]
        {
            new(CompanyApplicationStatusId.CREATED, CompanyApplicationStatusId.ADD_COMPANY_DATA),
            new(CompanyApplicationStatusId.ADD_COMPANY_DATA, CompanyApplicationStatusId.INVITE_USER),
            new(CompanyApplicationStatusId.INVITE_USER, CompanyApplicationStatusId.SELECT_COMPANY_ROLE),
            new(CompanyApplicationStatusId.SELECT_COMPANY_ROLE, CompanyApplicationStatusId.UPLOAD_DOCUMENTS),
            new(CompanyApplicationStatusId.UPLOAD_DOCUMENTS, CompanyApplicationStatusId.VERIFY),
            new(CompanyApplicationStatusId.VERIFY, CompanyApplicationStatusId.SUBMITTED),
        };

        if (!Array.Exists(
                allowedCombination,
                x => x.applicationStatus == applicationData.StatusId && x.status == status))
        {
            throw new ArgumentException(
                $"invalid status update requested {status}, current status is {applicationData.StatusId}, possible values are: {string.Join(",", allowedCombination.Where(x => x.applicationStatus == status).Select(x => x.applicationStatus))}");
        }

        applicationRepository.AttachAndModifyCompanyApplication(applicationId, a =>
        {
            a.ApplicationStatusId = status;
            a.DateLastChanged = dateTimeProvider.OffsetNow;
        });
    }

    private static void UpdateApplicationStatus(Guid applicationId, CompanyApplicationStatusId applicationStatusId, UpdateApplicationSteps type, IApplicationRepository applicationRepository, IDateTimeProvider dateTimeProvider)
    {
        var updateStatus = GetAndValidateUpdateApplicationStatus(applicationStatusId, type);
        if (updateStatus != default)
        {
            applicationRepository.AttachAndModifyCompanyApplication(applicationId, ca =>
            {
                ca.ApplicationStatusId = updateStatus;
                ca.DateLastChanged = dateTimeProvider.OffsetNow;
            });
        }
    }

    private static CompanyApplicationStatusId GetAndValidateUpdateApplicationStatus(CompanyApplicationStatusId applicationStatusId, UpdateApplicationSteps type)
    {
        return type switch
        {
            UpdateApplicationSteps.CompanyWithAddress
                when applicationStatusId == CompanyApplicationStatusId.CREATED ||
                    applicationStatusId == CompanyApplicationStatusId.ADD_COMPANY_DATA => CompanyApplicationStatusId.INVITE_USER,

            UpdateApplicationSteps.CompanyRoleAgreementConsents
                when applicationStatusId == CompanyApplicationStatusId.CREATED ||
                    applicationStatusId == CompanyApplicationStatusId.ADD_COMPANY_DATA ||
                    applicationStatusId == CompanyApplicationStatusId.INVITE_USER ||
                    applicationStatusId == CompanyApplicationStatusId.SELECT_COMPANY_ROLE => CompanyApplicationStatusId.UPLOAD_DOCUMENTS,

            UpdateApplicationSteps.SubmitRegistration
                when applicationStatusId == CompanyApplicationStatusId.CREATED ||
                    applicationStatusId == CompanyApplicationStatusId.ADD_COMPANY_DATA ||
                    applicationStatusId == CompanyApplicationStatusId.INVITE_USER ||
                    applicationStatusId == CompanyApplicationStatusId.SELECT_COMPANY_ROLE ||
                    applicationStatusId == CompanyApplicationStatusId.UPLOAD_DOCUMENTS => throw new ForbiddenException($"Application status is not fitting to the pre-requisite"),

            UpdateApplicationSteps.SubmitRegistration
                when applicationStatusId == CompanyApplicationStatusId.VERIFY => CompanyApplicationStatusId.SUBMITTED,

            _ when applicationStatusId == CompanyApplicationStatusId.SUBMITTED ||
                applicationStatusId == CompanyApplicationStatusId.CONFIRMED ||
                applicationStatusId == CompanyApplicationStatusId.DECLINED => throw new ForbiddenException($"Application is already closed"),

            _ => default
        };
    }

    public async Task<bool> DeleteRegistrationDocumentAsync(Guid documentId)
    {
        if (documentId == Guid.Empty)
        {
            throw new ControllerArgumentException($"documentId must not be empty");
        }
        var documentRepository = portalRepositories.GetInstance<IDocumentRepository>();
        var details = await documentRepository.GetDocumentDetailsForApplicationUntrackedAsync(documentId, _identityData.CompanyId, _settings.ApplicationStatusIds).ConfigureAwait(ConfigureAwaitOptions.None);
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

        portalRepositories.GetInstance<IApplicationRepository>().AttachAndModifyCompanyApplications(
            details.applicationId.Select(applicationId => new ValueTuple<Guid, Action<CompanyApplication>?, Action<CompanyApplication>>(
                applicationId,
                null,
                application => application.DateLastChanged = dateTimeProvider.OffsetNow)));

        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
        return true;
    }

    public async Task<IEnumerable<UniqueIdentifierData>> GetCompanyIdentifiers(string alpha2Code)
    {
        var uniqueIdentifierData = await portalRepositories.GetInstance<IStaticDataRepository>().GetCompanyIdentifiers(alpha2Code).ConfigureAwait(ConfigureAwaitOptions.None);

        if (!uniqueIdentifierData.IsValidCountryCode)
        {
            throw new NotFoundException($"invalid country code {alpha2Code}");
        }
        return uniqueIdentifierData.IdentifierIds.Select(identifierId => new UniqueIdentifierData((int)identifierId, identifierId));
    }

    public async Task<(string fileName, byte[] content, string mediaType)> GetRegistrationDocumentAsync(Guid documentId)
    {
        var documentRepository = portalRepositories.GetInstance<IDocumentRepository>();

        var documentDetails = await documentRepository.GetDocumentAsync(documentId, _settings.RegistrationDocumentTypeIds).ConfigureAwait(ConfigureAwaitOptions.None);
        if (documentDetails == default)
        {
            throw new NotFoundException($"document {documentId} does not exist.");
        }
        if (!documentDetails.IsDocumentTypeMatch)
        {
            throw new NotFoundException($"document {documentId} does not exist.");
        }

        return (documentDetails.FileName, documentDetails.Content, documentDetails.MediaTypeId.MapToMediaType());
    }

    public async Task DeclineApplicationRegistrationAsync(Guid applicationId)
    {
        var (isValidApplicationId, isValidCompany, declineData) = await portalRepositories.GetInstance<IApplicationRepository>()
            .GetDeclineApplicationDataForApplicationId(applicationId, _identityData.CompanyId, _settings.ApplicationDeclineStatusIds)
            .ConfigureAwait(ConfigureAwaitOptions.None);

        if (!isValidApplicationId)
        {
            throw new NotFoundException($"Application {applicationId} does not exits");
        }

        if (!isValidCompany)
        {
            throw new ForbiddenException("User is not allowed to decline this application");
        }

        if (declineData == null)
        {
            throw new UnexpectedConditionException("ApplicationDeclineData should never be null here");
        }

        DeclineApplication(applicationId);
        DeleteCompany(_identityData.CompanyId);
        DeclineInvitations(declineData.InvitationsStatusDatas);
        DeactivateDocuments(declineData.DocumentStatusDatas);
        ScheduleDeleteIdentityProviders(_identityData.CompanyId, declineData.IdentityProviderStatusDatas);
        ScheduleDeleteCompanyUsers(declineData.CompanyUserStatusDatas);
        await portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    private void DeclineApplication(Guid applicationId) =>
        portalRepositories.GetInstance<IApplicationRepository>()
            .AttachAndModifyCompanyApplication(applicationId, application =>
            {
                application.ApplicationStatusId = CompanyApplicationStatusId.DECLINED;
                application.DateLastChanged = DateTimeOffset.UtcNow;
            });

    private void DeleteCompany(Guid companyId) =>
        portalRepositories.GetInstance<ICompanyRepository>()
            .AttachAndModifyCompany(
                companyId,
                null,
                company => company.CompanyStatusId = CompanyStatusId.DELETED);

    private void DeclineInvitations(IEnumerable<InvitationsStatusData> invitationsStatusDatas) =>
        portalRepositories.GetInstance<IInvitationRepository>()
            .AttachAndModifyInvitations(
                invitationsStatusDatas.Select(data => new ValueTuple<Guid, Action<Invitation>?, Action<Invitation>>(
                    data.InvitationId,
                    invitation => invitation.InvitationStatusId = data.InvitationStatusId,
                    invitation => invitation.InvitationStatusId = InvitationStatusId.DECLINED)));

    private void DeactivateDocuments(IEnumerable<DocumentStatusData> documentStatusDatas) =>
        portalRepositories.GetInstance<IDocumentRepository>()
            .AttachAndModifyDocuments(
                documentStatusDatas.Select(data => new ValueTuple<Guid, Action<Document>?, Action<Document>>(
                    data.DocumentId,
                    document => document.DocumentStatusId = data.StatusId,
                    document => document.DocumentStatusId = DocumentStatusId.INACTIVE)));

    private void ScheduleDeleteIdentityProviders(Guid companyId, IEnumerable<IdentityProviderStatusData> identityProviderStatusDatas)
    {
        var identityProviderRepository = portalRepositories.GetInstance<IIdentityProviderRepository>();
        var processStepRepository = portalRepositories.GetInstance<IProcessStepRepository>();

        identityProviderStatusDatas
            .Where(data => data.IdentityProviderTypeId == IdentityProviderTypeId.MANAGED)
            .IfAny(managed =>
            {
                identityProviderRepository.DeleteCompanyIdentityProviderRange(managed.Select(x => (companyId, x.IdentityProviderId)));
            });

        identityProviderStatusDatas
            .Where(data => data.IdentityProviderTypeId == IdentityProviderTypeId.SHARED || data.IdentityProviderTypeId == IdentityProviderTypeId.OWN)
            .IfAny(notManaged =>
            {
                var processDatas = notManaged
                    .Zip(processStepRepository.CreateProcessRange(Enumerable.Repeat(ProcessTypeId.IDENTITYPROVIDER_PROVISIONING, notManaged.Count())))
                    .Select(x => (
                        x.First.IdentityProviderTypeId,
                        x.First.IdentityProviderId,
                        ProcessId: x.Second.Id
                    ))
                    .ToImmutableList();

                processStepRepository.CreateProcessStepRange(
                    processDatas.Select(x => (
                        x.IdentityProviderTypeId switch
                        {
                            IdentityProviderTypeId.SHARED => ProcessStepTypeId.DELETE_IDP_SHARED_REALM,
                            IdentityProviderTypeId.OWN => ProcessStepTypeId.DELETE_CENTRAL_IDENTITY_PROVIDER,
                            _ => throw new UnexpectedConditionException("IdentityProviderTypeId should allways be shared or own here")
                        },
                        ProcessStepStatusId.TODO,
                        x.ProcessId)));

                identityProviderRepository.CreateIdentityProviderAssignedProcessRange(
                    processDatas.Select(x => (
                        x.IdentityProviderId,
                        x.ProcessId)));
            });
    }

    private void ScheduleDeleteCompanyUsers(IEnumerable<CompanyUserStatusData> companyUserDatas)
    {
        portalRepositories.GetInstance<IUserRepository>()
            .AttachAndModifyIdentities(
                companyUserDatas
                    .Select(data => new ValueTuple<Guid, Action<Identity>?, Action<Identity>>(
                            data.CompanyUserId,
                            identity => identity.UserStatusId = data.UserStatusId,
                            identity =>
                            {
                                identity.UserStatusId = UserStatusId.DELETED;
                            })));

        portalRepositories.GetInstance<IUserRolesRepository>()
            .DeleteCompanyUserAssignedRoles(
                companyUserDatas.SelectMany(data => data.IdentityAssignedRoleIds.Select(roleId => (data.CompanyUserId, roleId))));

        var processStepRepository = portalRepositories.GetInstance<IProcessStepRepository>();
        var processIds = processStepRepository
            .CreateProcessRange(Enumerable.Repeat(ProcessTypeId.USER_PROVISIONING, companyUserDatas.Count()))
            .Select(x => x.Id)
            .ToImmutableList();
        processStepRepository.CreateProcessStepRange(
            processIds.Select(processId => (ProcessStepTypeId.DELETE_CENTRAL_USER, ProcessStepStatusId.TODO, processId)));
        portalRepositories.GetInstance<IUserRepository>()
            .CreateCompanyUserAssignedProcessRange(
                companyUserDatas.Select(x => x.CompanyUserId).Zip(processIds));
    }
}
