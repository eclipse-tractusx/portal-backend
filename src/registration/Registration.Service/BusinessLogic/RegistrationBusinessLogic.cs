/********************************************************************************
 * Copyright (c) 2021, 2023 Microsoft and BMW Group AG
 * Copyright (c) 2021, 2023 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Mailing.SendMail;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.ApplicationChecklist.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Bpn;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Bpn.Model;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Model;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Org.Eclipse.TractusX.Portal.Backend.Registration.Service.BusinessLogic;

public class RegistrationBusinessLogic : IRegistrationBusinessLogic
{
    private readonly RegistrationSettings _settings;
    private readonly IMailingService _mailingService;
    private readonly IBpnAccess _bpnAccess;
    private readonly IProvisioningManager _provisioningManager;
    private readonly IUserProvisioningService _userProvisioningService;
    private readonly IPortalRepositories _portalRepositories;
    private readonly ILogger<RegistrationBusinessLogic> _logger;
    private readonly IApplicationChecklistCreationService _checklistService;

    private static readonly Regex bpnRegex = new Regex(@"(\w|\d){16}", RegexOptions.None, TimeSpan.FromSeconds(1));

    public RegistrationBusinessLogic(
        IOptions<RegistrationSettings> settings,
        IMailingService mailingService,
        IBpnAccess bpnAccess,
        IProvisioningManager provisioningManager,
        IUserProvisioningService userProvisioningService,
        ILogger<RegistrationBusinessLogic> logger,
        IPortalRepositories portalRepositories,
        IApplicationChecklistCreationService checklistService)
    {
        _settings = settings.Value;
        _mailingService = mailingService;
        _bpnAccess = bpnAccess;
        _provisioningManager = provisioningManager;
        _userProvisioningService = userProvisioningService;
        _logger = logger;
        _portalRepositories = portalRepositories;
        _checklistService = checklistService;
    }

    public IAsyncEnumerable<string> GetClientRolesCompositeAsync() =>
        _portalRepositories.GetInstance<IUserRolesRepository>().GetClientRolesCompositeAsync(_settings.KeycloakClientID);

    [Obsolete($"use {nameof(GetCompanyBpdmDetailDataByBusinessPartnerNumber)} instead")]
    public IAsyncEnumerable<FetchBusinessPartnerDto> GetCompanyByIdentifierAsync(string companyIdentifier, string token, CancellationToken cancellationToken)
    {
        if (!bpnRegex.IsMatch(companyIdentifier))
        {
            throw new ControllerArgumentException("BPN must contain exactly 16 digits or letters.", nameof(companyIdentifier));
        }

        return _bpnAccess.FetchBusinessPartner(companyIdentifier, token, cancellationToken);
    }

    public Task<CompanyBpdmDetailData> GetCompanyBpdmDetailDataByBusinessPartnerNumber(string businessPartnerNumber, string token, CancellationToken cancellationToken)
    {
        if (!bpnRegex.IsMatch(businessPartnerNumber))
        {
            throw new ControllerArgumentException("BPN must contain exactly 16 digits or letters.", nameof(businessPartnerNumber));
        }
        return GetCompanyBpdmDetailDataByBusinessPartnerNumberInternal(businessPartnerNumber, token, cancellationToken);
    }

    private async Task<CompanyBpdmDetailData> GetCompanyBpdmDetailDataByBusinessPartnerNumberInternal(string businessPartnerNumber, string token, CancellationToken cancellationToken)
    {
        var legalEntity = await _bpnAccess.FetchLegalEntityByBpn(businessPartnerNumber, token, cancellationToken).ConfigureAwait(false);
        if (!businessPartnerNumber.Equals(legalEntity.Bpn, StringComparison.OrdinalIgnoreCase))
        {
            throw new ConflictException("Bpdm did return incorrect bpn legal-entity-data");
        }
        BpdmLegalEntityAddressDto? legalEntityAddress;
        try
        {
            legalEntityAddress = await _bpnAccess.FetchLegalEntityAddressByBpn(businessPartnerNumber, token, cancellationToken).SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (InvalidOperationException)
        {
            throw new ConflictException($"bpdm returned more than a single legalEntityAddress for {businessPartnerNumber}");
        }
        if (legalEntityAddress == null)
        {
            throw new ConflictException($"bpdm returned no legalEntityAddress for {businessPartnerNumber}");
        }
        if (!businessPartnerNumber.Equals(legalEntityAddress.LegalEntity, StringComparison.OrdinalIgnoreCase))
        {
            throw new ConflictException("Bpdm did return incorrect bpn address-data");
        }

        var legalAddress = legalEntityAddress.LegalAddress;

        var country = legalAddress.Country.TechnicalKey;

        var bpdmIdentifiers = ParseBpdmIdentifierDtos(legalEntity.Identifiers).ToList();
        var assignedIdentifiersResult = await _portalRepositories.GetInstance<IStaticDataRepository>()
            .GetCountryAssignedIdentifiers(bpdmIdentifiers.Select(x => x.BpdmIdentifierId), country).ConfigureAwait(false);

        if (!assignedIdentifiersResult.IsValidCountry)
        {
            throw new ConflictException($"Bpdm did return invalid country {country} in address-data");
        }

        var portalIdentifiers = assignedIdentifiersResult.Identifiers.Join(
                bpdmIdentifiers,
                assignedIdentifier => assignedIdentifier.BpdmIdentifierId,
                bpdmIdentifier => bpdmIdentifier.BpdmIdentifierId,
                (countryIdentifier, bpdmIdentifier) => (countryIdentifier.UniqueIdentifierId, bpdmIdentifier.Value));

        TItem? SingleOrDefaultChecked<TItem>(IEnumerable<TItem> items, string itemName)
        {
            try
            {
                return items.SingleOrDefault();
            }
            catch (InvalidOperationException)
            {
                throw new ConflictException($"bpdm returned more than a single {itemName} in legal entity for {businessPartnerNumber}");
            }
        }

        BpdmNameDto? name = SingleOrDefaultChecked(legalEntity.Names, nameof(name));
        string? administrativeArea = SingleOrDefaultChecked(legalAddress.AdministrativeAreas, nameof(administrativeArea))?.Value;
        string? postCode = SingleOrDefaultChecked(legalAddress.PostCodes, nameof(postCode))?.Value;
        string? locality = SingleOrDefaultChecked(legalAddress.Localities, nameof(locality))?.Value;
        BpdmThoroughfareDto? thoroughfare = SingleOrDefaultChecked(legalAddress.Thoroughfares, nameof(thoroughfare));

        return new CompanyBpdmDetailData(
            businessPartnerNumber,
            country,
            name?.Value ?? "",
            name?.ShortName ?? "",
            locality ?? "",
            thoroughfare?.Value ?? "",
            administrativeArea,
            null, // TODO clarify how to map from bpdm data
            thoroughfare?.Number,
            postCode,
            portalIdentifiers.Select(identifier => new CompanyUniqueIdData(identifier.UniqueIdentifierId, identifier.Value))
        );
    }

    private static IEnumerable<(BpdmIdentifierId BpdmIdentifierId, string Value)> ParseBpdmIdentifierDtos(IEnumerable<BpdmIdentifierDto> bpdmIdentifierDtos)
    {
        foreach (var identifier in bpdmIdentifierDtos)
        {
            if (Enum.TryParse<BpdmIdentifierId>(identifier.Type.TechnicalKey, out var bpdmIdentifierId))
            {
                yield return (bpdmIdentifierId, identifier.Value);
            }
        }
    }

    public async Task<int> UploadDocumentAsync(Guid applicationId, IFormFile document, DocumentTypeId documentTypeId, (Guid UserId, Guid CompanyId) identity, CancellationToken cancellationToken)
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

        var validApplicationForCompany = await _portalRepositories.GetInstance<IApplicationRepository>().IsValidApplicationForCompany(applicationId, identity.CompanyId).ConfigureAwait(false);
        if (!validApplicationForCompany)
        {
            throw new ForbiddenException($"The users company is not assigned with application {applicationId}");
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

        _portalRepositories.GetInstance<IDocumentRepository>().CreateDocument(documentName, documentContent, hash, document.ContentType.ParseMediaTypeId(), documentTypeId, doc =>
        {
            doc.CompanyUserId = identity.UserId;
        });
        return await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    public async Task<(string FileName, byte[] Content, string MediaType)> GetDocumentContentAsync(Guid documentId, Guid userId)
    {
        var documentRepository = _portalRepositories.GetInstance<IDocumentRepository>();
        var documentDetails = await documentRepository.GetDocumentIdWithCompanyUserCheckAsync(documentId, userId).ConfigureAwait(false);
        if (documentDetails.DocumentId == Guid.Empty)
        {
            throw new NotFoundException($"document {documentId} does not exist.");
        }

        if (!documentDetails.IsSameUser)
        {
            throw new ForbiddenException($"The user is not permitted to access document {documentId}.");
        }

        var document = await documentRepository.GetDocumentByIdAsync(documentId).ConfigureAwait(false);
        if (document is null)
        {
            throw new NotFoundException($"document {documentId} does not exist.");
        }
        return (document.DocumentName, document.DocumentContent, document.MediaTypeId.MapToMediaType());
    }

    public async IAsyncEnumerable<CompanyApplicationData> GetAllApplicationsForUserWithStatus(Guid companyId)
    {
        await foreach (var applicationWithStatus in _portalRepositories.GetInstance<IUserRepository>().GetApplicationsWithStatusUntrackedAsync(companyId).ConfigureAwait(false))
        {
            yield return new CompanyApplicationData
            {
                ApplicationId = applicationWithStatus.ApplicationId,
                ApplicationStatus = applicationWithStatus.ApplicationStatus
            };
        }
    }

    public async Task<CompanyDetailData> GetCompanyDetailData(Guid applicationId, Guid companyId)
    {
        var result = await _portalRepositories.GetInstance<IApplicationRepository>().GetCompanyApplicationDetailDataAsync(applicationId, companyId).ConfigureAwait(false);
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
            result.CountryNameDe,
            result.UniqueIds.Select(id => new CompanyUniqueIdData(id.UniqueIdentifierId, id.Value))
        );
    }

    public Task SetCompanyDetailDataAsync(Guid applicationId, CompanyDetailData companyDetails, Guid companyId)
    {
        if (string.IsNullOrWhiteSpace(companyDetails.Name))
        {
            throw new ControllerArgumentException("Name must not be empty", nameof(companyDetails.Name));
        }
        if (string.IsNullOrWhiteSpace(companyDetails.City))
        {
            throw new ControllerArgumentException("City must not be empty", nameof(companyDetails.City));
        }
        if (string.IsNullOrWhiteSpace(companyDetails.StreetName))
        {
            throw new ControllerArgumentException("Streetname must not be empty", nameof(companyDetails.StreetName));
        }
        if (companyDetails.CountryAlpha2Code.Length != 2)
        {
            throw new ControllerArgumentException("CountryAlpha2Code must be 2 chars", nameof(companyDetails.CountryAlpha2Code));
        }
        var emptyIds = companyDetails.UniqueIds.Where(uniqueId => string.IsNullOrWhiteSpace(uniqueId.Value));
        if (emptyIds.Any())
        {
            throw new ControllerArgumentException($"uniqueIds must not contain empty values: '{string.Join(", ", emptyIds.Select(uniqueId => uniqueId.UniqueIdentifierId))}'", nameof(companyDetails.UniqueIds));
        }
        var distinctIds = companyDetails.UniqueIds.DistinctBy(uniqueId => uniqueId.UniqueIdentifierId);
        if (distinctIds.Count() < companyDetails.UniqueIds.Count())
        {
            var duplicateIds = companyDetails.UniqueIds.Except(distinctIds);
            throw new ControllerArgumentException($"uniqueIds must not contain duplicate types: '{string.Join(", ", duplicateIds.Select(uniqueId => uniqueId.UniqueIdentifierId))}'", nameof(companyDetails.UniqueIds));
        }
        return SetCompanyDetailDataInternal(applicationId, companyDetails, companyId);
    }

    private async Task SetCompanyDetailDataInternal(Guid applicationId, CompanyDetailData companyDetails, Guid companyId)
    {
        await ValidateCountryAssignedIdentifiers(companyDetails).ConfigureAwait(false);

        var applicationRepository = _portalRepositories.GetInstance<IApplicationRepository>();
        var companyRepository = _portalRepositories.GetInstance<ICompanyRepository>();

        var companyApplicationData = await GetAndValidateApplicationData(applicationId, companyDetails, companyId, applicationRepository).ConfigureAwait(false);

        var addressId = CreateOrModifyAddress(companyApplicationData, companyDetails, companyRepository);

        ModifyCompany(addressId, companyApplicationData, companyDetails, companyRepository);

        companyRepository.CreateUpdateDeleteIdentifiers(companyDetails.CompanyId, companyApplicationData.UniqueIds, companyDetails.UniqueIds.Select(x => (x.UniqueIdentifierId, x.Value)));

        UpdateApplicationStatus(applicationId, companyApplicationData.ApplicationStatusId, UpdateApplicationSteps.CompanyWithAddress, applicationRepository);

        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    private async Task ValidateCountryAssignedIdentifiers(CompanyDetailData companyDetails)
    {
        if (companyDetails.UniqueIds.Any())
        {
            var assignedIdentifiers = await _portalRepositories.GetInstance<ICountryRepository>()
                .GetCountryAssignedIdentifiers(
                    companyDetails.CountryAlpha2Code,
                    companyDetails.UniqueIds.Select(uniqueId => uniqueId.UniqueIdentifierId))
                .ConfigureAwait(false);

            if (!assignedIdentifiers.IsValidCountry)
            {
                throw new ControllerArgumentException($"{companyDetails.CountryAlpha2Code} is not a valid country-code", nameof(companyDetails.UniqueIds));
            }
            if (assignedIdentifiers.UniqueIdentifierIds.Count() < companyDetails.UniqueIds.Count())
            {
                var invalidIds = companyDetails.UniqueIds.ExceptBy(assignedIdentifiers.UniqueIdentifierIds, uniqueId => uniqueId.UniqueIdentifierId);
                throw new ControllerArgumentException($"invalid uniqueIds for country {companyDetails.CountryAlpha2Code}: '{string.Join(", ", invalidIds.Select(uniqueId => uniqueId.UniqueIdentifierId))}'", nameof(companyDetails.UniqueIds));
            }
        }
    }

    private static async Task<CompanyApplicationDetailData> GetAndValidateApplicationData(Guid applicationId, CompanyDetailData companyDetails, Guid companyId, IApplicationRepository applicationRepository)
    {
        var companyApplicationData = await applicationRepository
            .GetCompanyApplicationDetailDataAsync(applicationId, companyId, companyDetails.CompanyId)
            .ConfigureAwait(false);

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
                c.BusinessPartnerNumber = initialData.BusinessPartnerNumber;
                c.Name = initialData.Name;
                c.Shortname = initialData.ShortName;
                c.CompanyStatusId = initialData.CompanyStatusId;
                c.AddressId = initialData.AddressId;
            },
            c =>
            {
                c.BusinessPartnerNumber = modifyData.BusinessPartnerNumber;
                c.Name = modifyData.Name;
                c.Shortname = modifyData.ShortName;
                c.CompanyStatusId = CompanyStatusId.PENDING;
                c.AddressId = addressId;
            });

    public Task<int> InviteNewUserAsync(Guid applicationId, UserCreationInfoWithMessage userCreationInfo, Guid userId)
    {
        if (string.IsNullOrEmpty(userCreationInfo.eMail))
        {
            throw new ControllerArgumentException($"email must not be empty");
        }
        return InviteNewUserInternalAsync(applicationId, userCreationInfo, userId);
    }

    private async Task<int> InviteNewUserInternalAsync(Guid applicationId, UserCreationInfoWithMessage userCreationInfo, Guid userId)
    {
        if (await _portalRepositories.GetInstance<IUserRepository>().IsOwnCompanyUserWithEmailExisting(userCreationInfo.eMail, userId))
        {
            throw new ControllerArgumentException($"user with email {userCreationInfo.eMail} does already exist");
        }

        var (companyNameIdpAliasData, createdByName) = await _userProvisioningService.GetCompanyNameSharedIdpAliasData(userId, applicationId).ConfigureAwait(false);

        IEnumerable<UserRoleData>? userRoleDatas = null;

        if (userCreationInfo.Roles.Any())
        {
            var clientRoles = new Dictionary<string, IEnumerable<string>> {
                { _settings.KeycloakClientID, userCreationInfo.Roles }
            };
            userRoleDatas = await _userProvisioningService.GetRoleDatas(clientRoles).ToListAsync().ConfigureAwait(false);
        }

        var userCreationInfoIdps = new[] { new UserCreationRoleDataIdpInfo(
            userCreationInfo.firstName ?? "",
            userCreationInfo.lastName ?? "",
            userCreationInfo.eMail,
            userRoleDatas ?? Enumerable.Empty<UserRoleData>(),
            userCreationInfo.userName ?? userCreationInfo.eMail,
            ""
        )}.ToAsyncEnumerable();

        var (newCompanyUserId, _, password, error) = await _userProvisioningService.CreateOwnCompanyIdpUsersAsync(companyNameIdpAliasData, userCreationInfoIdps).SingleAsync().ConfigureAwait(false);

        if (error != null)
        {
            throw error;
        }

        _portalRepositories.GetInstance<IApplicationRepository>().CreateInvitation(applicationId, newCompanyUserId);

        var modified = await _portalRepositories.SaveAsync().ConfigureAwait(false);

        var inviteTemplateName = "invite";
        if (!string.IsNullOrWhiteSpace(userCreationInfo.Message))
        {
            inviteTemplateName = "inviteWithMessage";
        }

        var companyDisplayName = await _userProvisioningService.GetIdentityProviderDisplayName(companyNameIdpAliasData.IdpAlias).ConfigureAwait(false);

        var mailParameters = new Dictionary<string, string>
        {
            { "password", password },
            { "companyName", companyDisplayName },
            { "message", userCreationInfo.Message ?? "" },
            { "nameCreatedBy", createdByName },
            { "url", _settings.BasePortalAddress },
            { "username", userCreationInfo.eMail },
        };

        await _mailingService.SendMails(userCreationInfo.eMail, mailParameters, new List<string> { inviteTemplateName, "password" }).ConfigureAwait(false);

        return modified;
    }

    public async Task<int> SetOwnCompanyApplicationStatusAsync(Guid applicationId, CompanyApplicationStatusId status, Guid companyId)
    {
        if (status == 0)
        {
            throw new ControllerArgumentException("status must not be null");
        }

        var applicationRepository = _portalRepositories.GetInstance<IApplicationRepository>();
        var applicationUserData = await applicationRepository.GetOwnCompanyApplicationUserDataAsync(applicationId, companyId).ConfigureAwait(false);
        if (!applicationUserData.Exists)
        {
            throw new NotFoundException($"CompanyApplication {applicationId} not found");
        }

        ValidateCompanyApplicationStatus(applicationId, status, applicationUserData, applicationRepository);

        return await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    public async Task<CompanyApplicationStatusId> GetOwnCompanyApplicationStatusAsync(Guid applicationId, Guid companyId)
    {
        var result = await _portalRepositories.GetInstance<IApplicationRepository>().GetOwnCompanyApplicationStatusUserDataUntrackedAsync(applicationId, companyId).ConfigureAwait(false);
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

    public async Task<int> SubmitRoleConsentAsync(Guid applicationId, CompanyRoleAgreementConsents roleAgreementConsentStatuses, Guid userId, Guid companyId)
    {
        var companyRoleIdsToSet = roleAgreementConsentStatuses.CompanyRoleIds;
        var agreementConsentsToSet = roleAgreementConsentStatuses.AgreementConsentStatuses;

        var companyRolesRepository = _portalRepositories.GetInstance<ICompanyRolesRepository>();
        var consentRepository = _portalRepositories.GetInstance<IConsentRepository>();

        var companyRoleAgreementConsentData = await companyRolesRepository.GetCompanyRoleAgreementConsentDataAsync(applicationId).ConfigureAwait(false);

        if (companyRoleAgreementConsentData == null)
        {
            throw new NotFoundException($"application {applicationId} does not exist");
        }

        var (applicationCompanyId, applicationStatusId, companyAssignedRoleIds, consents) = companyRoleAgreementConsentData;
        if (applicationCompanyId != companyId)
        {
            throw new ForbiddenException($"The users company is not assigned with CompanyApplication {applicationId}");
        }

        var companyRoleAssignedAgreements = await companyRolesRepository.GetAgreementAssignedCompanyRolesUntrackedAsync(companyRoleIdsToSet)
            .ToDictionaryAsync(x => x.CompanyRoleId, x => x.AgreementIds)
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
                            agreementConsent.AgreementId == assignedAgreementId
                            && agreementConsent.ConsentStatusId == ConsentStatusId.ACTIVE))))
        {
            throw new ControllerArgumentException("consent must be given to all CompanyRole assigned agreements");
        }

        companyRolesRepository.RemoveCompanyAssignedRoles(companyId, companyAssignedRoleIds.Except(companyRoleIdsToSet));

        foreach (var companyRoleId in companyRoleIdsToSet.Except(companyAssignedRoleIds))
        {
            companyRolesRepository.CreateCompanyAssignedRole(companyId, companyRoleId);
        }

        HandleConsent(consents, agreementConsentsToSet, consentRepository, companyId, userId);

        UpdateApplicationStatus(applicationId, applicationStatusId, UpdateApplicationSteps.CompanyRoleAgreementConsents, _portalRepositories.GetInstance<IApplicationRepository>());

        return await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    public async Task<CompanyRoleAgreementConsents> GetRoleAgreementConsentsAsync(Guid applicationId, Guid userId)
    {
        var result = await _portalRepositories.GetInstance<ICompanyRolesRepository>().GetCompanyRoleAgreementConsentStatusUntrackedAsync(applicationId, userId).ConfigureAwait(false);
        if (result == null)
        {
            throw new ForbiddenException($"user is not assigned with CompanyApplication {applicationId}");
        }
        return result;
    }

    public async Task<CompanyRoleAgreementData> GetCompanyRoleAgreementDataAsync() =>
        new(
            (await _portalRepositories.GetInstance<ICompanyRolesRepository>().GetCompanyRoleAgreementsUntrackedAsync().ToListAsync().ConfigureAwait(false)).AsEnumerable(),
            (await _portalRepositories.GetInstance<IAgreementRepository>().GetAgreementsForCompanyRolesUntrackedAsync().ToListAsync().ConfigureAwait(false)).AsEnumerable()
        );

    public async Task<bool> SubmitRegistrationAsync(Guid applicationId, Guid userId)
    {
        var applicationRepository = _portalRepositories.GetInstance<IApplicationRepository>();
        var applicationUserData = await applicationRepository.GetOwnCompanyApplicationUserEmailDataAsync(applicationId, userId).ConfigureAwait(false);
        if (applicationUserData == null)
        {
            throw new NotFoundException($"application {applicationId} does not exist");
        }

        if (!applicationUserData.IsApplicationCompanyUser)
        {
            throw new ForbiddenException($"userId {userId} is not associated with CompanyApplication {applicationId}");
        }

        if (applicationUserData.DocumentDatas.Any())
        {
            var documentRepository = _portalRepositories.GetInstance<IDocumentRepository>();
            foreach (var document in applicationUserData.DocumentDatas)
            {
                documentRepository.AttachAndModifyDocument(
                    document.DocumentId,
                    doc => doc.DocumentStatusId = document.StatusId,
                    doc => doc.DocumentStatusId = DocumentStatusId.LOCKED);
            }
        }

        if (GetAndValidateUpdateApplicationStatus(applicationUserData.CompanyApplicationStatusId, UpdateApplicationSteps.SubmitRegistration) != CompanyApplicationStatusId.SUBMITTED)
        {
            throw new UnexpectedConditionException("updateStatus should allways be SUBMITTED here");
        }

        var entries = await _checklistService.CreateInitialChecklistAsync(applicationId);

        var process = _portalRepositories.GetInstance<IProcessStepRepository>().CreateProcess(ProcessTypeId.APPLICATION_CHECKLIST);

        _portalRepositories.GetInstance<IProcessStepRepository>()
            .CreateProcessStepRange(
                _checklistService
                    .GetInitialProcessStepTypeIds(entries)
                    .Select(processStepTypeId => (processStepTypeId, ProcessStepStatusId.TODO, process.Id)));

        _portalRepositories.GetInstance<IApplicationRepository>()
            .AttachAndModifyCompanyApplication(
                applicationId,
                application =>
                {
                    application.ApplicationStatusId = CompanyApplicationStatusId.SUBMITTED;
                    application.ChecklistProcessId = process.Id;
                });

        await _portalRepositories.SaveAsync().ConfigureAwait(false);

        var mailParameters = new Dictionary<string, string>
        {
            { "url", $"{_settings.BasePortalAddress}"},
        };

        if (applicationUserData.Email != null)
        {
            await _mailingService.SendMails(applicationUserData.Email, mailParameters, new[] { "SubmitRegistrationTemplate" });
        }
        else
        {
            _logger.LogInformation("user {userId} has no email-address", userId);
        }

        return true;
    }

    public async IAsyncEnumerable<InvitedUser> GetInvitedUsersAsync(Guid applicationId)
    {
        await foreach (var item in _portalRepositories.GetInstance<IInvitationRepository>().GetInvitedUserDetailsUntrackedAsync(applicationId).ConfigureAwait(false))
        {
            if (string.IsNullOrWhiteSpace(item.UserId))
            {
                throw new ConflictException("UserEntityId must be set.");
            }

            var userRoles = await _provisioningManager.GetClientRoleMappingsForUserAsync(item.UserId, _settings.KeycloakClientID).ConfigureAwait(false);
            yield return new InvitedUser(
                item.InvitationStatus,
                item.EmailId,
                userRoles
            );
        }
    }

    public async Task<IEnumerable<UploadDocuments>> GetUploadedDocumentsAsync(Guid applicationId, DocumentTypeId documentTypeId, Guid userId)
    {
        var result = await _portalRepositories.GetInstance<IDocumentRepository>().GetUploadedDocumentsAsync(applicationId, documentTypeId, userId).ConfigureAwait(false);
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

    public async Task<int> SetInvitationStatusAsync(Guid userId)
    {
        var invitationData = await _portalRepositories.GetInstance<IInvitationRepository>().GetInvitationStatusAsync(userId).ConfigureAwait(false);

        if (invitationData == null)
        {
            throw new ForbiddenException($"user is not associated with invitation");
        }

        if (invitationData.InvitationStatusId is InvitationStatusId.CREATED or InvitationStatusId.PENDING)
        {
            invitationData.InvitationStatusId = InvitationStatusId.ACCEPTED;
        }

        return await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    public async Task<CompanyRegistrationData> GetRegistrationDataAsync(Guid applicationId, Guid companyId)
    {
        var (isValidApplicationId, isValidCompany, data) = await _portalRepositories.GetInstance<IApplicationRepository>().GetRegistrationDataUntrackedAsync(applicationId, companyId, _settings.DocumentTypeIds).ConfigureAwait(false);
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
            data.CountryDe,
            data.CompanyRoleIds,
            data.AgreementConsentStatuses.Select(consentStatus => new AgreementConsentStatusForRegistrationData(consentStatus.AgreementId, consentStatus.ConsentStatusId)),
            data.DocumentNames.Select(name => new RegistrationDocumentNames(name)),
            data.Identifiers.Select(identifier => new CompanyUniqueIdData(identifier.UniqueIdentifierId, identifier.Value))
        );
    }

    public IAsyncEnumerable<CompanyRolesDetails> GetCompanyRoles(string? languageShortName = null) =>
        _portalRepositories.GetInstance<ICompanyRolesRepository>().GetCompanyRolesAsync(languageShortName);

    private static void HandleConsent(IEnumerable<ConsentData> consents, IEnumerable<AgreementConsentStatus> agreementConsentsToSet,
        IConsentRepository consentRepository, Guid companyId, Guid companyUserId)
    {
        var consentsToInactivate = consents
            .Where(consent =>
                !agreementConsentsToSet.Any(agreementConsent =>
                    agreementConsent.AgreementId == consent.AgreementId
                    && agreementConsent.ConsentStatusId == ConsentStatusId.ACTIVE));
        consentRepository.AttachAndModifiesConsents(consentsToInactivate.Select(x => x.ConsentId), consent =>
        {
            consent.ConsentStatusId = ConsentStatusId.INACTIVE;
            consent.LastEditorId = companyUserId;
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
            consent.LastEditorId = companyUserId;
        });

        foreach (var agreementConsentToAdd in agreementConsentsToSet
                     .Where(agreementConsent =>
                         agreementConsent.ConsentStatusId == ConsentStatusId.ACTIVE
                         && !consents.Any(activeConsent =>
                             activeConsent.AgreementId == agreementConsent.AgreementId)))
        {
            consentRepository.CreateConsent(agreementConsentToAdd.AgreementId, companyId, companyUserId,
                ConsentStatusId.ACTIVE);
        }
    }

    private static void ValidateCompanyApplicationStatus(Guid applicationId,
        CompanyApplicationStatusId status,
        (bool Exists, CompanyApplicationStatusId StatusId) applicationData,
        IApplicationRepository applicationRepository)
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
                $"invalid status update requested {status}, current status is {applicationData.StatusId}, possible values are: {CompanyApplicationStatusId.SUBMITTED}");
        }

        applicationRepository.AttachAndModifyCompanyApplication(applicationId, a =>
        {
            a.ApplicationStatusId = status;
        });
    }

    private static void UpdateApplicationStatus(Guid applicationId, CompanyApplicationStatusId applicationStatusId, UpdateApplicationSteps type, IApplicationRepository applicationRepository)
    {
        var updateStatus = GetAndValidateUpdateApplicationStatus(applicationStatusId, type);
        if (updateStatus != default)
        {
            applicationRepository.AttachAndModifyCompanyApplication(applicationId, ca => ca.ApplicationStatusId = updateStatus);
        }
    }

    private static CompanyApplicationStatusId GetAndValidateUpdateApplicationStatus(CompanyApplicationStatusId applicationStatusId, UpdateApplicationSteps type)
    {
        return type switch
        {
            UpdateApplicationSteps.CompanyWithAddress
                when (applicationStatusId == CompanyApplicationStatusId.CREATED ||
                    applicationStatusId == CompanyApplicationStatusId.ADD_COMPANY_DATA) => CompanyApplicationStatusId.INVITE_USER,

            UpdateApplicationSteps.CompanyRoleAgreementConsents
                when (applicationStatusId == CompanyApplicationStatusId.CREATED ||
                    applicationStatusId == CompanyApplicationStatusId.ADD_COMPANY_DATA ||
                    applicationStatusId == CompanyApplicationStatusId.INVITE_USER ||
                    applicationStatusId == CompanyApplicationStatusId.SELECT_COMPANY_ROLE) => CompanyApplicationStatusId.UPLOAD_DOCUMENTS,

            UpdateApplicationSteps.SubmitRegistration
                when (applicationStatusId == CompanyApplicationStatusId.CREATED ||
                    applicationStatusId == CompanyApplicationStatusId.ADD_COMPANY_DATA ||
                    applicationStatusId == CompanyApplicationStatusId.INVITE_USER ||
                    applicationStatusId == CompanyApplicationStatusId.SELECT_COMPANY_ROLE ||
                    applicationStatusId == CompanyApplicationStatusId.UPLOAD_DOCUMENTS) => throw new ForbiddenException($"Application status is not fitting to the pre-requisite"),

            UpdateApplicationSteps.SubmitRegistration
                when applicationStatusId == CompanyApplicationStatusId.VERIFY => CompanyApplicationStatusId.SUBMITTED,

            _ when (applicationStatusId == CompanyApplicationStatusId.SUBMITTED ||
                applicationStatusId == CompanyApplicationStatusId.CONFIRMED ||
                applicationStatusId == CompanyApplicationStatusId.DECLINED) => throw new ForbiddenException($"Application is already closed"),

            _ => default
        };
    }

    public async Task<bool> DeleteRegistrationDocumentAsync(Guid documentId, Guid companyId)
    {
        if (documentId == Guid.Empty)
        {
            throw new ControllerArgumentException($"documentId must not be empty");
        }
        var documentRepository = _portalRepositories.GetInstance<IDocumentRepository>();
        var details = await documentRepository.GetDocumentDetailsForApplicationUntrackedAsync(documentId, companyId, _settings.ApplicationStatusIds).ConfigureAwait(false);
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

    public async Task<IEnumerable<UniqueIdentifierData>> GetCompanyIdentifiers(string alpha2Code)
    {
        var uniqueIdentifierData = await _portalRepositories.GetInstance<IStaticDataRepository>().GetCompanyIdentifiers(alpha2Code).ConfigureAwait(false);

        if (!uniqueIdentifierData.IsValidCountryCode)
        {
            throw new NotFoundException($"invalid country code {alpha2Code}");
        }
        return uniqueIdentifierData.IdentifierIds.Select(identifierId => new UniqueIdentifierData((int)identifierId, identifierId));
    }

    public async Task<(string fileName, byte[] content, string mediaType)> GetRegistrationDocumentAsync(Guid documentId)
    {
        var documentRepository = _portalRepositories.GetInstance<IDocumentRepository>();

        var documentDetails = await documentRepository.GetDocumentAsync(documentId, _settings.RegistrationDocumentTypeIds).ConfigureAwait(false);
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
}
