/********************************************************************************
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

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Custodian.Library;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Async;
using Org.Eclipse.TractusX.Portal.Backend.Framework.DateTimeProvider;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Linq;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Web;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;
using Org.Eclipse.TractusX.Portal.Backend.Processes.Mailing.Library;
using System.Collections.Immutable;
using System.Globalization;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

public class CompanyDataBusinessLogic : ICompanyDataBusinessLogic
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly IPortalRepositories _portalRepositories;
    private readonly ICustodianService _custodianService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IIdentityData _identityData;
    private readonly IMailingProcessCreation _mailingProcessCreation;
    private readonly CompanyDataSettings _settings;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="portalRepositories"></param>
    /// <param name="custodianService"></param>
    /// <param name="dateTimeProvider"></param>
    /// <param name="identityService"></param>
    /// <param name="mailingProcessCreation"></param>
    /// <param name="options"></param>
    public CompanyDataBusinessLogic(IPortalRepositories portalRepositories, ICustodianService custodianService, IDateTimeProvider dateTimeProvider, IIdentityService identityService, IMailingProcessCreation mailingProcessCreation, IOptions<CompanyDataSettings> options)
    {
        _portalRepositories = portalRepositories;
        _custodianService = custodianService;
        _dateTimeProvider = dateTimeProvider;
        _identityData = identityService.IdentityData;
        _mailingProcessCreation = mailingProcessCreation;
        _settings = options.Value;
    }

    /// <inheritdoc/>
    public async Task<CompanyAddressDetailData> GetCompanyDetailsAsync()
    {
        var companyId = _identityData.CompanyId;
        var result = await _portalRepositories.GetInstance<ICompanyRepository>().GetCompanyDetailsAsync(companyId)
            .ConfigureAwait(ConfigureAwaitOptions.None);
        if (result == null)
        {
            throw new ConflictException($"company {companyId} is not a valid company");
        }

        return result;
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<CompanyAssignedUseCaseData> GetCompanyAssigendUseCaseDetailsAsync() =>
        _portalRepositories.GetInstance<ICompanyRepository>()
            .GetCompanyAssigendUseCaseDetailsAsync(_identityData.CompanyId);

    /// <inheritdoc/>
    public async Task<bool> CreateCompanyAssignedUseCaseDetailsAsync(Guid useCaseId)
    {
        var companyId = _identityData.CompanyId;
        var companyRepositories = _portalRepositories.GetInstance<ICompanyRepository>();
        var useCaseDetails = await companyRepositories.GetCompanyStatusAndUseCaseIdAsync(companyId, useCaseId)
            .ConfigureAwait(ConfigureAwaitOptions.None);
        if (!useCaseDetails.IsActiveCompanyStatus)
        {
            throw new ConflictException("Company Status is Incorrect");
        }

        if (useCaseDetails.IsUseCaseIdExists)
        {
            return false;
        }

        companyRepositories.CreateCompanyAssignedUseCase(companyId, useCaseId);
        await _portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
        return true;
    }

    /// <inheritdoc/>
    public async Task RemoveCompanyAssignedUseCaseDetailsAsync(Guid useCaseId)
    {
        var companyId = _identityData.CompanyId;
        var companyRepositories = _portalRepositories.GetInstance<ICompanyRepository>();
        var useCaseDetails = await companyRepositories.GetCompanyStatusAndUseCaseIdAsync(companyId, useCaseId)
            .ConfigureAwait(ConfigureAwaitOptions.None);
        if (!useCaseDetails.IsActiveCompanyStatus)
        {
            throw new ConflictException("Company Status is Incorrect");
        }

        if (!useCaseDetails.IsUseCaseIdExists)
        {
            throw new ConflictException($"UseCaseId {useCaseId} is not available");
        }

        companyRepositories.RemoveCompanyAssignedUseCase(companyId, useCaseId);
        await _portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public async IAsyncEnumerable<CompanyRoleConsentViewData> GetCompanyRoleAndConsentAgreementDetailsAsync(string? languageShortName)
    {
        var companyId = _identityData.CompanyId;
        if (languageShortName != null && !await _portalRepositories.GetInstance<ILanguageRepository>().IsValidLanguageCode(languageShortName).ConfigureAwait(ConfigureAwaitOptions.None))
        {
            throw new ControllerArgumentException($"language {languageShortName} is not a valid languagecode");
        }

        var companyRepositories = _portalRepositories.GetInstance<ICompanyRepository>();
        var statusData = await companyRepositories.GetCompanyStatusDataAsync(companyId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (statusData == default)
        {
            throw new NotFoundException($"company {companyId} does not exist");
        }

        if (!statusData.IsActive)
        {
            throw new ConflictException("Company Status is Incorrect");
        }

        await foreach (var data in companyRepositories.GetCompanyRoleAndConsentAgreementDataAsync(companyId, languageShortName ?? Constants.DefaultLanguage).ConfigureAwait(false))
        {
            yield return new CompanyRoleConsentViewData(
                data.CompanyRoleId,
                data.RoleDescription,
                data.CompanyRolesActive,
                data.Agreements.Select(x => new ConsentAgreementViewData(
                    x.AgreementId,
                    x.AgreementName,
                    x.DocumentId,
                    x.ConsentStatus == 0
                        ? null
                        : x.ConsentStatus,
                    x.AgreementLink,
                    x.Mandatory
                ))
            );
        }
    }

    /// <inheritdoc/>
    public async Task CreateCompanyRoleAndConsentAgreementDetailsAsync(IEnumerable<CompanyRoleConsentDetails> companyRoleConsentDetails)
    {
        if (!companyRoleConsentDetails.Any())
        {
            return;
        }

        var companyRepositories = _portalRepositories.GetInstance<ICompanyRepository>();
        var result = await companyRepositories
            .GetCompanyRolesDataAsync(_identityData.CompanyId, companyRoleConsentDetails.Select(x => x.CompanyRole))
            .ConfigureAwait(ConfigureAwaitOptions.None);
        if (!result.IsValidCompany)
        {
            throw new ConflictException($"company {_identityData.CompanyId} does not exist");
        }

        if (!result.IsCompanyActive)
        {
            throw new ConflictException("Company Status is Incorrect");
        }

        if (result.CompanyRoleIds == null || result.ConsentStatusDetails == null)
        {
            throw new UnexpectedConditionException("neither CompanyRoleIds nor ConsentStatusDetails should ever be null here");
        }

        var agreementAssignedRoleData = await companyRepositories
            .GetAgreementAssignedRolesDataAsync(companyRoleConsentDetails.Select(x => x.CompanyRole))
            .PreSortedGroupBy(x => x.CompanyRoleId, x => x.agreementStatusData)
            .ToDictionaryAsync(g => g.Key, g => g.AsEnumerable()).ConfigureAwait(false);

        var joined = companyRoleConsentDetails
            .Join(agreementAssignedRoleData,
                details => details.CompanyRole,
                data => data.Key,
                (details, data) => (
                    CompanyRoleId: details.CompanyRole,
                    AllActiveAgreements: details.Agreements.All(x => x.ConsentStatus == ConsentStatusId.ACTIVE),
                    AllInActiveAgreements: details.Agreements.All(x => x.ConsentStatus == ConsentStatusId.INACTIVE),
                    Agreements: details.Agreements.ExceptBy(
                        data.Value.Where(x => x.AgreementStatusId == AgreementStatusId.INACTIVE)
                            .Select(x => x.AgreementId), x => x.AgreementId),
                    MissingAgreementIds: data.Value.Select(x => x.AgreementId).Except(details.Agreements
                        .Where(x => x.ConsentStatus == ConsentStatusId.ACTIVE).Select(x => x.AgreementId)),
                    ExtraAgreementIds: details.Agreements
                        .ExceptBy(data.Value.Select(x => x.AgreementId), x => x.AgreementId)
                        .Select(x => x.AgreementId)))
            .ToList();

        var missing = joined.Where(x => x.MissingAgreementIds.Any() && !x.AllInActiveAgreements);
        if (missing.Any())
        {
            throw new ControllerArgumentException($"All agreements need to get signed as Active or InActive. Missing consents: [{string.Join(", ", missing.Select(x => $"{x.CompanyRoleId}: [{string.Join(", ", x.MissingAgreementIds)}]"))}]");
        }

        if (!joined.Exists(x => x.AllActiveAgreements))
        {
            throw new ConflictException("Company can't unassign from all roles, Atleast one Company role need to signed as active");
        }

        var extra = joined.Where(x => x.ExtraAgreementIds.Any());
        if (extra.Any())
        {
            throw new ControllerArgumentException($"Agreements not associated with requested companyRoles: [{string.Join(", ", extra.Select(x => $"{x.CompanyRoleId}: [{string.Join(", ", x.ExtraAgreementIds)}]"))}]");
        }

        _portalRepositories.GetInstance<IConsentRepository>().AddAttachAndModifyConsents(
            result.ConsentStatusDetails,
            joined.SelectMany(x => x.Agreements).DistinctBy(active => active.AgreementId)
                .Select(active => (active.AgreementId, active.ConsentStatus)).ToList(),
            _identityData.CompanyId,
            _identityData.IdentityId,
            _dateTimeProvider.OffsetNow);

        var companyRolesRepository = _portalRepositories.GetInstance<ICompanyRolesRepository>();

        companyRolesRepository.CreateCompanyAssignedRoles(_identityData.CompanyId,
            joined.Where(j => j.AllActiveAgreements && !result.CompanyRoleIds.Contains(j.CompanyRoleId))
                .Select(x => x.CompanyRoleId));
        companyRolesRepository.RemoveCompanyAssignedRoles(_identityData.CompanyId,
            joined.Where(j => j.AllInActiveAgreements && result.CompanyRoleIds.Contains(j.CompanyRoleId))
                .Select(x => x.CompanyRoleId));

        await _portalRepositories.SaveAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<UseCaseParticipationData>> GetUseCaseParticipationAsync(string? language) =>
        await _portalRepositories
            .GetInstance<ICompanySsiDetailsRepository>()
            .GetUseCaseParticipationForCompany(_identityData.CompanyId, language ?? Constants.DefaultLanguage)
            .Select(x => new UseCaseParticipationData(
                x.UseCase,
                x.Description,
                x.CredentialType,
                x.VerifiedCredentials
                    .Select(y =>
                        new CompanySsiExternalTypeDetailData(
                            y.ExternalDetailData,
                            y.SsiDetailData.CatchingInto(
                                data => data
                                    .Select(d => new CompanySsiDetailData(
                                        d.CredentialId,
                                        d.ParticipationStatus,
                                        d.ExpiryDate,
                                        d.Document))
                                    .SingleOrDefault(),
                                (InvalidOperationException _) => new ConflictException("There should only be one pending or active ssi detail be assigned"))))
                    .ToList()))
            .ToListAsync()
            .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<IEnumerable<SsiCertificateData>> GetSsiCertificatesAsync() =>
        await _portalRepositories
            .GetInstance<ICompanySsiDetailsRepository>()
            .GetSsiCertificates(_identityData.CompanyId)
            .Select(x => new SsiCertificateData(
                x.CredentialType,
                x.SsiDetailData.Select(d => new CompanySsiDetailData(
                    d.CredentialId,
                    d.ParticipationStatus,
                    d.ExpiryDate,
                    d.Document)
                )))
            .ToListAsync()
            .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task CreateUseCaseParticipation(UseCaseParticipationCreationData data, CancellationToken cancellationToken)
    {
        var (verifiedCredentialExternalTypeDetailId, credentialTypeId, document) = data;
        var documentContentType = document.ContentType.ParseMediaTypeId();
        documentContentType.CheckDocumentContentType(_settings.UseCaseParticipationMediaTypes);

        var companyCredentialDetailsRepository = _portalRepositories.GetInstance<ICompanySsiDetailsRepository>();
        if (!await companyCredentialDetailsRepository.CheckCredentialTypeIdExistsForExternalTypeDetailVersionId(verifiedCredentialExternalTypeDetailId, credentialTypeId).ConfigureAwait(ConfigureAwaitOptions.None))
        {
            throw new ControllerArgumentException($"VerifiedCredentialExternalTypeDetail {verifiedCredentialExternalTypeDetailId} does not exist");
        }

        await HandleSsiCreationAsync(credentialTypeId, VerifiedCredentialTypeKindId.USE_CASE, verifiedCredentialExternalTypeDetailId, document, documentContentType, companyCredentialDetailsRepository, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
    }

    /// <inheritdoc />
    public async Task CreateSsiCertificate(SsiCertificateCreationData data, CancellationToken cancellationToken)
    {
        var (credentialTypeId, document) = data;
        var documentContentType = document.ContentType.ParseMediaTypeId();
        documentContentType.CheckDocumentContentType(_settings.SsiCertificateMediaTypes);

        var companyCredentialDetailsRepository = _portalRepositories.GetInstance<ICompanySsiDetailsRepository>();
        if (!await companyCredentialDetailsRepository.CheckSsiCertificateType(credentialTypeId).ConfigureAwait(ConfigureAwaitOptions.None))
        {
            throw new ControllerArgumentException($"{credentialTypeId} is not assigned to a certificate");
        }

        await HandleSsiCreationAsync(credentialTypeId, VerifiedCredentialTypeKindId.CERTIFICATE, null, document, documentContentType, companyCredentialDetailsRepository, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
    }

    private async Task HandleSsiCreationAsync(
        VerifiedCredentialTypeId credentialTypeId,
        VerifiedCredentialTypeKindId kindId,
        Guid? verifiedCredentialExternalTypeDetailId,
        IFormFile document,
        MediaTypeId mediaTypeId,
        ICompanySsiDetailsRepository companyCredentialDetailsRepository,
        CancellationToken cancellationToken)
    {
        if (await companyCredentialDetailsRepository.CheckSsiDetailsExistsForCompany(_identityData.CompanyId, credentialTypeId, kindId, verifiedCredentialExternalTypeDetailId).ConfigureAwait(ConfigureAwaitOptions.None))
        {
            throw new ControllerArgumentException("Credential request already existing");
        }

        var (documentContent, hash) = await document.GetContentAndHash(cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        var doc = _portalRepositories.GetInstance<IDocumentRepository>().CreateDocument(document.FileName, documentContent,
            hash, mediaTypeId, DocumentTypeId.PRESENTATION, x =>
            {
                x.CompanyUserId = _identityData.IdentityId;
                x.DocumentStatusId = DocumentStatusId.PENDING;
            });

        companyCredentialDetailsRepository.CreateSsiDetails(_identityData.CompanyId, credentialTypeId, doc.Id, CompanySsiDetailStatusId.PENDING, _identityData.IdentityId, details =>
            {
                if (verifiedCredentialExternalTypeDetailId != null)
                {
                    details.VerifiedCredentialExternalTypeUseCaseDetailId = verifiedCredentialExternalTypeDetailId;
                }
            });

        await _portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    /// <inheritdoc />
    public async Task CreateCompanyCertificate(CompanyCertificateCreationData data, CancellationToken cancellationToken)
    {
        var documentContentType = data.Document.ContentType.ParseMediaTypeId();
        documentContentType.CheckDocumentContentType(_settings.CompanyCertificateMediaTypes);

        var companyCertificateRepository = _portalRepositories.GetInstance<ICompanyCertificateRepository>();
        if (!await companyCertificateRepository.CheckCompanyCertificateType(data.CertificateType).ConfigureAwait(ConfigureAwaitOptions.None))
        {
            throw new ControllerArgumentException($"{data.CertificateType} is not assigned to a certificate");
        }

        await HandleCompanyCertificateCreationAsync(data.CertificateType, data.Document, documentContentType, companyCertificateRepository, data.ExpiryDate, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
    }

    private async Task HandleCompanyCertificateCreationAsync(CompanyCertificateTypeId companyCertificateTypeId,
        IFormFile document,
        MediaTypeId mediaTypeId,
        ICompanyCertificateRepository companyCertificateRepository,
        DateTimeOffset? expiryDate,
        CancellationToken cancellationToken)
    {
        var (documentContent, hash) = await document.GetContentAndHash(cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        var doc = _portalRepositories.GetInstance<IDocumentRepository>().CreateDocument(document.FileName, documentContent,
            hash, mediaTypeId, DocumentTypeId.COMPANY_CERTIFICATE, x =>
            {
                x.CompanyUserId = _identityData.IdentityId;
                x.DocumentStatusId = DocumentStatusId.LOCKED;
            });

        companyCertificateRepository.CreateCompanyCertificate(_identityData.CompanyId, companyCertificateTypeId, doc.Id,
            x =>
            {
                x.ValidTill = expiryDate?.ToUniversalTime();
            });

        await _portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    /// <inheritdoc />
    public Task<Pagination.Response<CredentialDetailData>> GetCredentials(int page, int size, CompanySsiDetailStatusId? companySsiDetailStatusId, VerifiedCredentialTypeId? credentialTypeId, string? companyName, CompanySsiDetailSorting? sorting)
    {
        var query = _portalRepositories
            .GetInstance<ICompanySsiDetailsRepository>()
            .GetAllCredentialDetails(companySsiDetailStatusId, credentialTypeId, companyName);
        var sortedQuery = sorting switch
        {
            CompanySsiDetailSorting.CompanyAsc or null => query.OrderBy(c => c.Company!.Name),
            CompanySsiDetailSorting.CompanyDesc => query.OrderByDescending(c => c.Company!.Name),
            _ => query
        };

        return Pagination.CreateResponseAsync(page, size, _settings.MaxPageSize, (skip, take) =>
            new Pagination.AsyncSource<CredentialDetailData>
            (
                query.CountAsync(),
                sortedQuery
                    .Skip(skip)
                    .Take(take)
                    .Select(c => new CredentialDetailData(
                        c.Id,
                        c.CompanyId,
                        c.Company!.Name,
                        c.VerifiedCredentialTypeId,
                        c.VerifiedCredentialType!.VerifiedCredentialTypeAssignedUseCase!.UseCase!.Name,
                        c.CompanySsiDetailStatusId,
                        c.ExpiryDate,
                        new DocumentData(c.Document!.Id, c.Document!.DocumentName),
                        c.VerifiedCredentialExternalTypeUseCaseDetailVersion == null
                            ? null
                            : new ExternalTypeDetailData(
                                c.VerifiedCredentialExternalTypeUseCaseDetailVersion!.Id,
                                c.VerifiedCredentialExternalTypeUseCaseDetailVersion.VerifiedCredentialExternalTypeId,
                                c.VerifiedCredentialExternalTypeUseCaseDetailVersion.Version,
                                c.VerifiedCredentialExternalTypeUseCaseDetailVersion.Template,
                                c.VerifiedCredentialExternalTypeUseCaseDetailVersion.ValidFrom,
                                c.VerifiedCredentialExternalTypeUseCaseDetailVersion.Expiry))
                    ).AsAsyncEnumerable()
            ));
    }

    /// <inheritdoc />
    public async Task ApproveCredential(Guid credentialId, CancellationToken cancellationToken)
    {
        var companySsiRepository = _portalRepositories.GetInstance<ICompanySsiDetailsRepository>();
        var userId = _identityData.IdentityId;
        var (exists, data) = await companySsiRepository.GetSsiApprovalData(credentialId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (!exists)
        {
            throw new NotFoundException($"CompanySsiDetail {credentialId} does not exists");
        }

        if (data.Status != CompanySsiDetailStatusId.PENDING)
        {
            throw new ConflictException($"Credential {credentialId} must be {CompanySsiDetailStatusId.PENDING}");
        }

        if (string.IsNullOrWhiteSpace(data.Bpn))
        {
            throw new UnexpectedConditionException($"Bpn should be set for company {data.CompanyName}");
        }

        if (data is { Kind: VerifiedCredentialTypeKindId.USE_CASE, UseCaseDetailData: null })
        {
            throw new ConflictException("The VerifiedCredentialExternalTypeUseCaseDetail must be set");
        }

        var typeValue = data.Type.GetEnumValue() ?? throw new UnexpectedConditionException($"VerifiedCredentialType {data.Type} does not exists");
        var content = JsonSerializer.Serialize(new { Type = data.Type, CredentialId = credentialId }, Options);
        _portalRepositories.GetInstance<INotificationRepository>().CreateNotification(data.RequesterData.RequesterId, NotificationTypeId.CREDENTIAL_APPROVAL, false, n =>
            {
                n.CreatorUserId = userId;
                n.Content = content;
            });

        companySsiRepository.AttachAndModifyCompanySsiDetails(credentialId, c =>
            {
                c.CompanySsiDetailStatusId = data.Status;
            },
            c =>
            {
                c.CompanySsiDetailStatusId = CompanySsiDetailStatusId.ACTIVE;
                c.DateLastChanged = _dateTimeProvider.OffsetNow;
            });

        switch (data.Kind)
        {
            case VerifiedCredentialTypeKindId.USE_CASE:
                await _custodianService.TriggerFrameworkAsync(data.Bpn, data.UseCaseDetailData!, cancellationToken)
                    .ConfigureAwait(ConfigureAwaitOptions.None);
                break;
            case VerifiedCredentialTypeKindId.CERTIFICATE:
                await _custodianService.TriggerDismantlerAsync(data.Bpn, data.Type, cancellationToken)
                    .ConfigureAwait(ConfigureAwaitOptions.None);
                break;
            default:
                throw new ArgumentOutOfRangeException($"{data.Kind} is currently not supported");
        }

        if (!string.IsNullOrWhiteSpace(data.RequesterData.RequesterEmail))
        {
            var userName = string.Join(" ", new[] { data.RequesterData.Firstname, data.RequesterData.Lastname }.Where(item => !string.IsNullOrWhiteSpace(item)));
            var mailParameters = ImmutableDictionary.CreateRange(new KeyValuePair<string, string>[]
            {
                new("userName", !string.IsNullOrWhiteSpace(userName) ? userName : data.RequesterData.RequesterEmail),
                new("requestName", typeValue),
                new("companyName", data.CompanyName),
                new("credentialType", typeValue),
                new(
                    "expiryDate", data.ExpiryDate == null ? string.Empty : data.ExpiryDate.Value.ToString("o", CultureInfo.InvariantCulture)
                )
            });
            _mailingProcessCreation.CreateMailProcess(data.RequesterData.RequesterEmail, "CredentialApproval", mailParameters);
        }
        await _portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    /// <inheritdoc />
    public async Task RejectCredential(Guid credentialId)
    {
        var companySsiRepository = _portalRepositories.GetInstance<ICompanySsiDetailsRepository>();
        var userId = _identityData.IdentityId;
        var (exists, status, type, requesterId, requesterEmail, requesterFirstname, requesterLastname) = await companySsiRepository.GetSsiRejectionData(credentialId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (!exists)
        {
            throw new NotFoundException($"CompanySsiDetail {credentialId} does not exists");
        }

        if (status != CompanySsiDetailStatusId.PENDING)
        {
            throw new ConflictException($"Credential {credentialId} must be {CompanySsiDetailStatusId.PENDING}");
        }

        var typeValue = type.GetEnumValue() ?? throw new UnexpectedConditionException($"VerifiedCredentialType {type} does not exists");
        var content = JsonSerializer.Serialize(new { Type = type, CredentialId = credentialId }, Options);
        _portalRepositories.GetInstance<INotificationRepository>().CreateNotification(requesterId, NotificationTypeId.CREDENTIAL_REJECTED, false, n =>
            {
                n.CreatorUserId = userId;
                n.Content = content;
            });

        companySsiRepository.AttachAndModifyCompanySsiDetails(credentialId, c =>
            {
                c.CompanySsiDetailStatusId = status;
            },
            c =>
            {
                c.CompanySsiDetailStatusId = CompanySsiDetailStatusId.INACTIVE;
                c.DateLastChanged = _dateTimeProvider.OffsetNow;
            });

        if (!string.IsNullOrWhiteSpace(requesterEmail))
        {
            var userName = string.Join(" ", new[] { requesterFirstname, requesterLastname }.Where(item => !string.IsNullOrWhiteSpace(item)));
            var mailParameters = ImmutableDictionary.CreateRange(new[]
            {
                KeyValuePair.Create("userName", !string.IsNullOrWhiteSpace(userName) ? userName : requesterEmail),
                KeyValuePair.Create("requestName", typeValue)
            });
            _mailingProcessCreation.CreateMailProcess(requesterEmail, "CredentialRejected", mailParameters);
        }
        await _portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<VerifiedCredentialTypeId> GetCertificateTypes() =>
        _portalRepositories.GetInstance<ICompanySsiDetailsRepository>().GetCertificateTypes(_identityData.CompanyId);

    /// <inheritdoc />    
    public async IAsyncEnumerable<CompanyCertificateBpnData> GetCompanyCertificatesByBpn(string businessPartnerNumber)
    {
        if (string.IsNullOrWhiteSpace(businessPartnerNumber))
        {
            throw new ControllerArgumentException("businessPartnerNumber must not be empty");
        }

        var companyCertificateRepository = _portalRepositories.GetInstance<ICompanyCertificateRepository>();

        var companyId = await companyCertificateRepository.GetCompanyIdByBpn(businessPartnerNumber).ConfigureAwait(ConfigureAwaitOptions.None);
        if (companyId == Guid.Empty)
        {
            throw new ControllerArgumentException($"company does not exist for {businessPartnerNumber}");
        }

        await foreach (var data in companyCertificateRepository.GetCompanyCertificateData(companyId))
        {
            yield return data;
        }
    }

    public Task<Pagination.Response<CompanyCertificateData>> GetAllCompanyCertificatesAsync(int page, int size, CertificateSorting? sorting, CompanyCertificateStatusId? certificateStatus, CompanyCertificateTypeId? certificateType) =>
        Pagination.CreateResponseAsync(
            page,
            size,
            _settings.MaxPageSize,
            _portalRepositories.GetInstance<ICompanyCertificateRepository>().GetActiveCompanyCertificatePaginationSource(sorting, certificateStatus, certificateType, _identityData.CompanyId));

    public async Task<DimUrlsResponse> GetDimServiceUrls() =>
        new DimUrlsResponse(
            _settings.DecentralIdentityManagementAuthUrl,
            await _portalRepositories.GetInstance<ICompanyRepository>().GetWalletServiceUrl(_identityData.CompanyId).ConfigureAwait(ConfigureAwaitOptions.None));

    /// <inheritdoc />
    public async Task<int> DeleteCompanyCertificateAsync(Guid documentId)
    {
        var companyCertificateRepository = _portalRepositories.GetInstance<ICompanyCertificateRepository>();

        var details = await companyCertificateRepository.GetCompanyCertificateDocumentDetailsForIdUntrackedAsync(documentId, _identityData.CompanyId).ConfigureAwait(ConfigureAwaitOptions.None);

        var certificateCount = details.CompanyCertificateId.Count();
        if (certificateCount > 1)
        {
            throw new ConflictException($"There must not be multiple active certificates for document {documentId}");
        }

        if (details.DocumentId == Guid.Empty)
        {
            throw new NotFoundException("Document is not existing");
        }

        if (!details.IsSameCompany)
        {
            throw new ForbiddenException("User is not allowed to delete this document");
        }

        companyCertificateRepository.AttachAndModifyCompanyCertificateDocumentDetails(documentId, null, c =>
            {
                c.DocumentStatusId = DocumentStatusId.INACTIVE;
                c.DateLastChanged = _dateTimeProvider.OffsetNow;
            });

        if (certificateCount == 1)
        {
            companyCertificateRepository.AttachAndModifyCompanyCertificateDetails(details.CompanyCertificateId.SingleOrDefault(), null, c =>
            {
                c.CompanyCertificateStatusId = CompanyCertificateStatusId.INACTIVE;
            });
        }

        return await _portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    /// <inheritdoc />
    public async Task<(string FileName, byte[] Content, string MediaType)> GetCompanyCertificateDocumentByCompanyIdAsync(Guid documentId)
    {
        var documentDetails = await _portalRepositories.GetInstance<ICompanyCertificateRepository>()
            .GetCompanyCertificateDocumentByCompanyIdDataAsync(documentId, _identityData.CompanyId, DocumentTypeId.COMPANY_CERTIFICATE)
            .ConfigureAwait(ConfigureAwaitOptions.None);

        if (!documentDetails.Exists)
        {
            throw new NotFoundException($"Company certificate document {documentId} does not exist");
        }

        return (documentDetails.FileName, documentDetails.Content, documentDetails.MediaTypeId.MapToMediaType());
    }

    /// <inheritdoc />
    public async Task<(string FileName, byte[] Content, string MediaType)> GetCompanyCertificateDocumentAsync(Guid documentId)
    {
        var documentDetails = await _portalRepositories.GetInstance<ICompanyCertificateRepository>()
            .GetCompanyCertificateDocumentDataAsync(documentId, DocumentTypeId.COMPANY_CERTIFICATE)
            .ConfigureAwait(ConfigureAwaitOptions.None);

        if (!documentDetails.Exists)
        {
            throw new NotFoundException($"Company certificate document {documentId} does not exist");
        }
        if (!documentDetails.IsStatusLocked)
        {
            throw new ForbiddenException($"Document {documentId} status is not locked");
        }

        return (documentDetails.FileName, documentDetails.Content, documentDetails.MediaTypeId.MapToMediaType());
    }
}
