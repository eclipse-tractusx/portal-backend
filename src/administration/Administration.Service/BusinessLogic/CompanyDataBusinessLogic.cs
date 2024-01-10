/********************************************************************************
 * Copyright (c) 2021, 2023 BMW Group AG
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
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Custodian.Library;
using Org.Eclipse.TractusX.Portal.Backend.Custodian.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Async;
using Org.Eclipse.TractusX.Portal.Backend.Framework.DateTimeProvider;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Linq;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Web;
using Org.Eclipse.TractusX.Portal.Backend.Mailing.SendMail;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;
using System.Globalization;
using System.Text.Json;
using ErrorParameter = Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.ErrorParameter;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

public class CompanyDataBusinessLogic : ICompanyDataBusinessLogic
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly IPortalRepositories _portalRepositories;
    private readonly IMailingService _mailingService;
    private readonly ICustodianService _custodianService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IIdentityData _identityData;
    private readonly CompanyDataSettings _settings;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="portalRepositories"></param>
    /// <param name="mailingService"></param>
    /// <param name="custodianService"></param>
    /// <param name="dateTimeProvider"></param>
    /// <param name="identityService"></param>
    /// <param name="options"></param>
    public CompanyDataBusinessLogic(IPortalRepositories portalRepositories, IMailingService mailingService, ICustodianService custodianService, IDateTimeProvider dateTimeProvider, IIdentityService identityService, IOptions<CompanyDataSettings> options)
    {
        _portalRepositories = portalRepositories;
        _mailingService = mailingService;
        _custodianService = custodianService;
        _dateTimeProvider = dateTimeProvider;
        _identityData = identityService.IdentityData;
        _settings = options.Value;
    }

    /// <inheritdoc/>
    public async Task<CompanyAddressDetailData> GetCompanyDetailsAsync()
    {
        var companyId = _identityData.CompanyId;
        var result = await _portalRepositories.GetInstance<ICompanyRepository>().GetCompanyDetailsAsync(companyId).ConfigureAwait(false);
        if (result == null)
        {
            throw ConflictException.Create(CompanyDataErrors.INVALID_COMPANY, new ErrorParameter[] { new("companyId", companyId.ToString()) });
        }
        return result;
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<CompanyAssignedUseCaseData> GetCompanyAssigendUseCaseDetailsAsync() =>
        _portalRepositories.GetInstance<ICompanyRepository>().GetCompanyAssigendUseCaseDetailsAsync(_identityData.CompanyId);

    /// <inheritdoc/>
    public async Task<bool> CreateCompanyAssignedUseCaseDetailsAsync(Guid useCaseId)
    {
        var companyId = _identityData.CompanyId;
        var companyRepositories = _portalRepositories.GetInstance<ICompanyRepository>();
        var useCaseDetails = await companyRepositories.GetCompanyStatusAndUseCaseIdAsync(companyId, useCaseId).ConfigureAwait(false);
        if (!useCaseDetails.IsActiveCompanyStatus)
        {
            throw ConflictException.Create(CompanyDataErrors.INVALID_COMPANY_STATUS);
        }
        if (useCaseDetails.IsUseCaseIdExists)
        {
            return false;
        }
        companyRepositories.CreateCompanyAssignedUseCase(companyId, useCaseId);
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
        return true;
    }

    /// <inheritdoc/>
    public async Task RemoveCompanyAssignedUseCaseDetailsAsync(Guid useCaseId)
    {
        var companyId = _identityData.CompanyId;
        var companyRepositories = _portalRepositories.GetInstance<ICompanyRepository>();
        var useCaseDetails = await companyRepositories.GetCompanyStatusAndUseCaseIdAsync(companyId, useCaseId).ConfigureAwait(false);
        if (!useCaseDetails.IsActiveCompanyStatus)
        {
            throw ConflictException.Create(CompanyDataErrors.INVALID_COMPANY_STATUS);
        }
        if (!useCaseDetails.IsUseCaseIdExists)
        {
            throw ConflictException.Create(CompanyDataErrors.USE_CASE_NOT_FOUND, new ErrorParameter[] { new("useCaseId", useCaseId.ToString()) });
        }
        companyRepositories.RemoveCompanyAssignedUseCase(companyId, useCaseId);
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    public async IAsyncEnumerable<CompanyRoleConsentViewData> GetCompanyRoleAndConsentAgreementDetailsAsync(string? languageShortName)
    {
        var companyId = _identityData.CompanyId;
        if (languageShortName != null && !await _portalRepositories.GetInstance<ILanguageRepository>().IsValidLanguageCode(languageShortName).ConfigureAwait(false))
        {
            throw ControllerArgumentException.Create(CompanyDataErrors.INVALID_LANGUAGECODE, new ErrorParameter[] { new("languageShortName", languageShortName) });
        }

        var companyRepositories = _portalRepositories.GetInstance<ICompanyRepository>();
        var statusData = await companyRepositories.GetCompanyStatusDataAsync(companyId).ConfigureAwait(false);
        if (statusData == default)
        {
            throw NotFoundException.Create(CompanyDataErrors.COMPANY_NOT_FOUND, new ErrorParameter[] { new("companyId", companyId.ToString()) });
        }
        if (!statusData.IsActive)
        {
            throw ConflictException.Create(CompanyDataErrors.INVALID_COMPANY_STATUS);
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
                    x.AgreementLink
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
        var result = await companyRepositories.GetCompanyRolesDataAsync(_identityData.CompanyId, companyRoleConsentDetails.Select(x => x.CompanyRole)).ConfigureAwait(false);
        if (!result.IsValidCompany)
        {
            throw ConflictException.Create(CompanyDataErrors.COMPANY_NOT_FOUND, new ErrorParameter[] { new("companyId", _identityData.CompanyId.ToString()) });
        }
        if (!result.IsCompanyActive)
        {
            throw ConflictException.Create(CompanyDataErrors.INVALID_COMPANY_STATUS);
        }
        if (result.CompanyRoleIds == null || result.ConsentStatusDetails == null)
        {
            throw UnexpectedConditionException.Create(CompanyDataErrors.COMPANY_ROLE_IDS_CONSENT_STATUS_NULL);
        }

        var agreementAssignedRoleData = await companyRepositories.GetAgreementAssignedRolesDataAsync(companyRoleConsentDetails.Select(x => x.CompanyRole))
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
                    Agreements: details.Agreements.ExceptBy(data.Value.Where(x => x.AgreementStatusId == AgreementStatusId.INACTIVE).Select(x => x.AgreementId), x => x.AgreementId),
                    MissingAgreementIds: data.Value.Select(x => x.AgreementId).Except(details.Agreements.Where(x => x.ConsentStatus == ConsentStatusId.ACTIVE).Select(x => x.AgreementId)),
                    ExtraAgreementIds: details.Agreements.ExceptBy(data.Value.Select(x => x.AgreementId), x => x.AgreementId).Select(x => x.AgreementId)))
            .ToList();

        var missing = joined.Where(x => x.MissingAgreementIds.Any() && !x.AllInActiveAgreements);
        if (missing.Any())
        {
            throw ControllerArgumentException.Create(CompanyDataErrors.MISSING_AGREEMENTS, new ErrorParameter[] { new("missingConsents", string.Join(", ", missing.Select(x => $"{x.CompanyRoleId}: [{string.Join(", ", x.MissingAgreementIds)}]"))) });
        }
        if (!joined.Any(x => x.AllActiveAgreements))
        {
            throw ConflictException.Create(CompanyDataErrors.UNASSIGN_ALL_ROLES);
        }
        var extra = joined.Where(x => x.ExtraAgreementIds.Any());
        if (extra.Any())
        {
            throw ControllerArgumentException.Create(CompanyDataErrors.UNASSIGN_ALL_ROLES, new ErrorParameter[] { new("companyRoles", string.Join(", ", extra.Select(x => $"{x.CompanyRoleId}: [{string.Join(", ", x.ExtraAgreementIds)}]"))) });
        }

        _portalRepositories.GetInstance<IConsentRepository>().AddAttachAndModifyConsents(
            result.ConsentStatusDetails,
            joined.SelectMany(x => x.Agreements).DistinctBy(active => active.AgreementId).Select(active => (active.AgreementId, active.ConsentStatus)).ToList(),
            _identityData.CompanyId,
            _identityData.IdentityId,
            _dateTimeProvider.OffsetNow);

        var companyRolesRepository = _portalRepositories.GetInstance<ICompanyRolesRepository>();

        companyRolesRepository.CreateCompanyAssignedRoles(_identityData.CompanyId, joined.Where(j => j.AllActiveAgreements && !result.CompanyRoleIds.Contains(j.CompanyRoleId)).Select(x => x.CompanyRoleId));
        companyRolesRepository.RemoveCompanyAssignedRoles(_identityData.CompanyId, joined.Where(j => j.AllInActiveAgreements && result.CompanyRoleIds.Contains(j.CompanyRoleId)).Select(x => x.CompanyRoleId));

        await _portalRepositories.SaveAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<UseCaseParticipationData>> GetUseCaseParticipationAsync(string? language) =>
        await _portalRepositories
            .GetInstance<ICompanySsiDetailsRepository>()
            .GetUseCaseParticipationForCompany(_identityData.CompanyId, language ?? Constants.DefaultLanguage, _dateTimeProvider.OffsetNow)
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
                                (InvalidOperationException _) => throw ConflictException.Create(CompanyDataErrors.MULTIPLE_SSI_DETAIL))))
                    .ToList()))
            .ToListAsync()
            .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<IEnumerable<CertificateParticipationData>> GetSsiCertificatesAsync() =>
        await _portalRepositories
            .GetInstance<ICompanySsiDetailsRepository>()
            .GetSsiCertificates(_identityData.CompanyId, _dateTimeProvider.OffsetNow)
            .Select(x => new CertificateParticipationData(
                x.CredentialType,
                x.Credentials
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
                                (InvalidOperationException _) => throw ConflictException.Create(CompanyDataErrors.MULTIPLE_SSI_DETAIL))))
                    .ToList()))
            .ToListAsync()
            .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task CreateUseCaseParticipation(UseCaseParticipationCreationData data, CancellationToken cancellationToken)
    {
        var (verifiedCredentialExternalTypeDetailId, credentialTypeId, document) = data;
        var documentContentType = document.ContentType.ParseMediaTypeId();
        documentContentType.CheckDocumentContentType(_settings.UseCaseParticipationMediaTypes);

        var companyCredentialDetailsRepository = _portalRepositories.GetInstance<ICompanySsiDetailsRepository>();
        var expiryDate = await companyCredentialDetailsRepository.CheckCredentialTypeIdExistsForExternalTypeDetailVersionId(verifiedCredentialExternalTypeDetailId, credentialTypeId).ConfigureAwait(false);
        if (expiryDate == default)
        {
            throw ControllerArgumentException.Create(CompanyDataErrors.EXTERNAL_TYPE_DETAIL_NOT_FOUND, new ErrorParameter[] { new("verifiedCredentialExternalTypeDetailId", verifiedCredentialExternalTypeDetailId.ToString()) });
        }

        if (expiryDate < _dateTimeProvider.OffsetNow)
        {
            throw ControllerArgumentException.Create(CompanyDataErrors.EXPIRY_DATE_IN_PAST);
        }

        await HandleSsiCreationAsync(credentialTypeId, VerifiedCredentialTypeKindId.USE_CASE, verifiedCredentialExternalTypeDetailId, document, documentContentType, companyCredentialDetailsRepository, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task CreateSsiCertificate(SsiCertificateCreationData data, CancellationToken cancellationToken)
    {
        var (verifiedCredentialExternalTypeDetailId, credentialTypeId, document) = data;
        var documentContentType = document.ContentType.ParseMediaTypeId();
        documentContentType.CheckDocumentContentType(_settings.SsiCertificateMediaTypes);

        var companyCredentialDetailsRepository = _portalRepositories.GetInstance<ICompanySsiDetailsRepository>();
        var (exists, detailVersionIds) = await companyCredentialDetailsRepository.CheckSsiCertificateType(credentialTypeId).ConfigureAwait(false);
        if (!exists)
        {
            throw ControllerArgumentException.Create(CompanyDataErrors.CREDENTIAL_NO_CERTIFICATE, new ErrorParameter[] { new("credentialTypeId", credentialTypeId.ToString()) });
        }

        if (verifiedCredentialExternalTypeDetailId == null && detailVersionIds.Count() != 1)
        {
            throw ControllerArgumentException.Create(CompanyDataErrors.EXTERNAL_TYPE_DETAIL_ID_NOT_SET, Enumerable.Empty<ErrorParameter>(), nameof(data.VerifiedCredentialExternalTypeDetailId));
        }

        if (verifiedCredentialExternalTypeDetailId != null && !detailVersionIds.Contains(verifiedCredentialExternalTypeDetailId.Value))
        {
            throw ControllerArgumentException.Create(CompanyDataErrors.EXTERNAL_TYPE_DETAIL_NOT_FOUND, new ErrorParameter[] { new("verifiedCredentialExternalTypeDetailId", verifiedCredentialExternalTypeDetailId.Value.ToString()) });
        }

        await HandleSsiCreationAsync(credentialTypeId, VerifiedCredentialTypeKindId.CERTIFICATE, verifiedCredentialExternalTypeDetailId ?? detailVersionIds.Single(), document, documentContentType, companyCredentialDetailsRepository, cancellationToken).ConfigureAwait(false);
    }

    private async Task HandleSsiCreationAsync(
        VerifiedCredentialTypeId credentialTypeId, VerifiedCredentialTypeKindId kindId, Guid verifiedCredentialExternalTypeDetailId, IFormFile document, MediaTypeId mediaTypeId,
        ICompanySsiDetailsRepository companyCredentialDetailsRepository,
        CancellationToken cancellationToken)
    {
        if (await companyCredentialDetailsRepository.CheckSsiDetailsExistsForCompany(_identityData.CompanyId, credentialTypeId, kindId, verifiedCredentialExternalTypeDetailId).ConfigureAwait(false))
        {
            throw ControllerArgumentException.Create(CompanyDataErrors.CREDENTIAL_ALREADY_EXISTING);
        }

        var (documentContent, hash) = await document.GetContentAndHash(cancellationToken).ConfigureAwait(false);
        var doc = _portalRepositories.GetInstance<IDocumentRepository>().CreateDocument(document.FileName, documentContent,
            hash, mediaTypeId, DocumentTypeId.PRESENTATION, x =>
            {
                x.CompanyUserId = _identityData.IdentityId;
                x.DocumentStatusId = DocumentStatusId.PENDING;
            });

        companyCredentialDetailsRepository.CreateSsiDetails(_identityData.CompanyId, credentialTypeId, doc.Id, CompanySsiDetailStatusId.PENDING, _identityData.IdentityId,
            ssi =>
            {
                ssi.VerifiedCredentialExternalTypeDetailVersionId = verifiedCredentialExternalTypeDetailId;
            });

        await _portalRepositories.SaveAsync().ConfigureAwait(false);
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
                        c.VerifiedCredentialExternalTypeDetailVersion == null
                            ? null
                            : new ExternalTypeDetailData(
                                c.VerifiedCredentialExternalTypeDetailVersion.Id,
                                c.VerifiedCredentialExternalTypeDetailVersion.VerifiedCredentialExternalTypeId,
                                c.VerifiedCredentialExternalTypeDetailVersion.Version,
                                c.VerifiedCredentialExternalTypeDetailVersion.Template,
                                c.VerifiedCredentialExternalTypeDetailVersion.ValidFrom,
                                c.VerifiedCredentialExternalTypeDetailVersion.Expiry))
                    ).AsAsyncEnumerable()
            ));
    }

    /// <inheritdoc />
    public async Task ApproveCredential(Guid credentialId, CancellationToken cancellationToken)
    {
        var companySsiRepository = _portalRepositories.GetInstance<ICompanySsiDetailsRepository>();
        var userId = _identityData.IdentityId;
        var (exists, data) = await companySsiRepository.GetSsiApprovalData(credentialId).ConfigureAwait(false);
        var (bpn, externalTypeId, template, version, dataExpiryDate) = ValidateApprovalData(credentialId, exists, data);

        var typeValue = data.Type.GetEnumValue() ?? throw UnexpectedConditionException.Create(CompanyDataErrors.CREDENTIAL_TYPE_NOT_FOUND, new ErrorParameter[] { new("verifiedCredentialType", data.Type.ToString()) });
        var content = JsonSerializer.Serialize(new { Type = data.Type, CredentialId = credentialId }, Options);
        _portalRepositories.GetInstance<INotificationRepository>().CreateNotification(data.RequesterData.RequesterId, NotificationTypeId.CREDENTIAL_APPROVAL, false, n =>
        {
            n.CreatorUserId = userId;
            n.Content = content;
        });

        var expiryDate = GetExpiryDate(dataExpiryDate);
        companySsiRepository.AttachAndModifyCompanySsiDetails(credentialId, c =>
            {
                c.CompanySsiDetailStatusId = data.Status;
                c.ExpiryDate = DateTimeOffset.MinValue;
            },
            c =>
            {
                c.CompanySsiDetailStatusId = CompanySsiDetailStatusId.ACTIVE;
                c.DateLastChanged = _dateTimeProvider.OffsetNow;
                c.ExpiryDate = expiryDate;
            });

        if (data.Kind == VerifiedCredentialTypeKindId.USE_CASE)
        {
            await _custodianService.TriggerFrameworkAsync(
                new CustodianFrameworkRequest
                (
                    bpn,
                    externalTypeId,
                    template,
                    version ?? throw ConflictException.Create(CompanyDataErrors.EMPTY_VERSION),
                    expiryDate
                ), cancellationToken).ConfigureAwait(false);
        }
        else if (data.Kind == VerifiedCredentialTypeKindId.CERTIFICATE)
        {
            await _custodianService.TriggerDismantlerAsync(bpn, data.Type, expiryDate, cancellationToken).ConfigureAwait(false);
        }

        await _portalRepositories.SaveAsync().ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(data.RequesterData.RequesterEmail))
        {
            return;
        }

        var userName = string.Join(" ", new[] { data.RequesterData.Firstname, data.RequesterData.Lastname }.Where(item => !string.IsNullOrWhiteSpace(item)));
        var mailParameters = new Dictionary<string, string>
        {
            { "userName", !string.IsNullOrWhiteSpace(userName) ? userName : data.RequesterData.RequesterEmail },
            { "requestName", typeValue },
            { "companyName", data.CompanyName },
            { "credentialType", typeValue },
            { "expiryDate", expiryDate.ToString("o", CultureInfo.InvariantCulture) }
        };

        await _mailingService.SendMails(data.RequesterData.RequesterEmail, mailParameters,
            Enumerable.Repeat("CredentialApproval", 1)).ConfigureAwait(false);
    }

    private static (string Bpn, VerifiedCredentialExternalTypeId VerifiedCredentialExternalTypeId, string? Template, string? Version, DateTimeOffset ExpiryDate) ValidateApprovalData(Guid credentialId, bool exists, SsiApprovalData data)
    {
        if (!exists)
        {
            throw NotFoundException.Create(CompanyDataErrors.SSI_DETAILS_NOT_FOUND, new ErrorParameter[] { new("credentialId", credentialId.ToString()) });
        }

        if (data.Status != CompanySsiDetailStatusId.PENDING)
        {
            throw ConflictException.Create(CompanyDataErrors.CREDENTIAL_NOT_PENDING, new ErrorParameter[] { new("credentialId", credentialId.ToString()), new("status", CompanySsiDetailStatusId.PENDING.ToString()) });
        }

        if (string.IsNullOrWhiteSpace(data.Bpn))
        {
            throw UnexpectedConditionException.Create(CompanyDataErrors.BPN_NOT_SET, new ErrorParameter[] { new("companyName", data.CompanyName) });
        }

        if (data.DetailData == null)
        {
            throw ConflictException.Create(CompanyDataErrors.EXTERNAL_TYPE_DETAIL_ID_NOT_SET);
        }

        var (externalTypeId, template, version, expiryDate) = data.DetailData;
        if (data.Kind != VerifiedCredentialTypeKindId.USE_CASE && data.Kind != VerifiedCredentialTypeKindId.CERTIFICATE)
        {
            throw ConflictException.Create(CompanyDataErrors.KIND_NOT_SUPPORTED, new ErrorParameter[] { new("kind", data.Kind != null ? data.Kind.Value.ToString() : "empty kind") });
        }

        if (data.Kind == VerifiedCredentialTypeKindId.USE_CASE && string.IsNullOrWhiteSpace(data.DetailData.Version))
        {
            throw ConflictException.Create(CompanyDataErrors.EMPTY_VERSION);
        }

        return (data.Bpn, externalTypeId, template, version, expiryDate);
    }

    private DateTimeOffset GetExpiryDate(DateTimeOffset expiryDate)
    {
        var now = _dateTimeProvider.OffsetNow;
        var future = now.AddMonths(12);

        if (expiryDate < now)
        {
            throw ConflictException.Create(CompanyDataErrors.EXPIRY_DATE_IN_PAST);
        }

        return expiryDate > future ? future : expiryDate;
    }

    /// <inheritdoc />
    public async Task RejectCredential(Guid credentialId)
    {
        var companySsiRepository = _portalRepositories.GetInstance<ICompanySsiDetailsRepository>();
        var userId = _identityData.IdentityId;
        var (exists, status, type, requesterId, requesterEmail, requesterFirstname, requesterLastname) = await companySsiRepository.GetSsiRejectionData(credentialId).ConfigureAwait(false);
        if (!exists)
        {
            throw NotFoundException.Create(CompanyDataErrors.SSI_DETAILS_NOT_FOUND, new ErrorParameter[] { new("credentialId", credentialId.ToString()) });
        }

        if (status != CompanySsiDetailStatusId.PENDING)
        {
            throw ConflictException.Create(CompanyDataErrors.CREDENTIAL_NOT_PENDING, new ErrorParameter[] { new("credentialId", credentialId.ToString()), new("status", CompanySsiDetailStatusId.PENDING.ToString()) });
        }
        var typeValue = type.GetEnumValue() ?? throw UnexpectedConditionException.Create(CompanyDataErrors.CREDENTIAL_TYPE_NOT_FOUND, new ErrorParameter[] { new("verifiedCredentialType", type.ToString()) });
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

        await _portalRepositories.SaveAsync().ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(requesterEmail))
        {
            var userName = string.Join(" ", new[] { requesterFirstname, requesterLastname }.Where(item => !string.IsNullOrWhiteSpace(item)));
            var mailParameters = new Dictionary<string, string>
            {
                { "userName", !string.IsNullOrWhiteSpace(userName) ? userName : requesterEmail },
                { "requestName", typeValue },
                { "reason", "Declined by the Operator" }
            };

            await _mailingService.SendMails(requesterEmail, mailParameters, Enumerable.Repeat("CredentialRejected", 1)).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public IAsyncEnumerable<VerifiedCredentialTypeId> GetCertificateTypes() =>
        _portalRepositories.GetInstance<ICompanySsiDetailsRepository>().GetCertificateTypes(_identityData.CompanyId);
}
