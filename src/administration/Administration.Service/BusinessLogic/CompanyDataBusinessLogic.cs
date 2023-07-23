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
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Custodian.Library;
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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using PortalBackend.PortalEntities.Identity;
using System.Globalization;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

public class CompanyDataBusinessLogic : ICompanyDataBusinessLogic
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly IPortalRepositories _portalRepositories;
    private readonly IMailingService _mailingService;
    private readonly ICustodianService _custodianService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly CompanyDataSettings _settings;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="portalRepositories"></param>
    /// <param name="mailingService"></param>
    /// <param name="custodianService"></param>
    /// <param name="dateTimeProvider"></param>
    /// <param name="options"></param>
    public CompanyDataBusinessLogic(IPortalRepositories portalRepositories, IMailingService mailingService, ICustodianService custodianService, IDateTimeProvider dateTimeProvider, IOptions<CompanyDataSettings> options)
    {
        _portalRepositories = portalRepositories;
        _mailingService = mailingService;
        _custodianService = custodianService;
        _dateTimeProvider = dateTimeProvider;
        _settings = options.Value;
    }

    /// <inheritdoc/>
    public async Task<CompanyAddressDetailData> GetCompanyDetailsAsync(Guid companyId)
    {
        var result = await _portalRepositories.GetInstance<ICompanyRepository>().GetCompanyDetailsAsync(companyId).ConfigureAwait(false);
        if (result == null)
        {
            throw new ConflictException($"company {companyId} is not a valid company");
        }
        return result;
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<CompanyAssignedUseCaseData> GetCompanyAssigendUseCaseDetailsAsync(Guid companyId) =>
        _portalRepositories.GetInstance<ICompanyRepository>().GetCompanyAssigendUseCaseDetailsAsync(companyId);

    /// <inheritdoc/>
    public async Task<bool> CreateCompanyAssignedUseCaseDetailsAsync(Guid companyId, Guid useCaseId)
    {
        var companyRepositories = _portalRepositories.GetInstance<ICompanyRepository>();
        var useCaseDetails = await companyRepositories.GetCompanyStatusAndUseCaseIdAsync(companyId, useCaseId).ConfigureAwait(false);
        if (!useCaseDetails.IsActiveCompanyStatus)
        {
            throw new ConflictException("Company Status is Incorrect");
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
    public async Task RemoveCompanyAssignedUseCaseDetailsAsync(Guid companyId, Guid useCaseId)
    {
        var companyRepositories = _portalRepositories.GetInstance<ICompanyRepository>();
        var useCaseDetails = await companyRepositories.GetCompanyStatusAndUseCaseIdAsync(companyId, useCaseId).ConfigureAwait(false);
        if (!useCaseDetails.IsActiveCompanyStatus)
        {
            throw new ConflictException("Company Status is Incorrect");
        }
        if (!useCaseDetails.IsUseCaseIdExists)
        {
            throw new ConflictException($"UseCaseId {useCaseId} is not available");
        }
        companyRepositories.RemoveCompanyAssignedUseCase(companyId, useCaseId);
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    public async IAsyncEnumerable<CompanyRoleConsentViewData> GetCompanyRoleAndConsentAgreementDetailsAsync(Guid companyId, string? languageShortName)
    {
        if (languageShortName != null && !await _portalRepositories.GetInstance<ILanguageRepository>().IsValidLanguageCode(languageShortName).ConfigureAwait(false))
        {
            throw new ControllerArgumentException($"language {languageShortName} is not a valid languagecode");
        }

        var companyRepositories = _portalRepositories.GetInstance<ICompanyRepository>();
        var statusData = await companyRepositories.GetCompanyStatusDataAsync(companyId).ConfigureAwait(false);
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
                        : x.ConsentStatus
                ))
            );
        }
    }

    /// <inheritdoc/>
    public async Task CreateCompanyRoleAndConsentAgreementDetailsAsync((Guid UserId, Guid CompanyId) identity, IEnumerable<CompanyRoleConsentDetails> companyRoleConsentDetails)
    {
        if (!companyRoleConsentDetails.Any())
        {
            return;
        }
        var companyRepositories = _portalRepositories.GetInstance<ICompanyRepository>();
        var result = await companyRepositories.GetCompanyRolesDataAsync(identity.CompanyId, companyRoleConsentDetails.Select(x => x.CompanyRole)).ConfigureAwait(false);
        if (!result.IsValidCompany)
        {
            throw new ConflictException($"company {identity.CompanyId} does not exist");
        }
        if (!result.IsCompanyActive)
        {
            throw new ConflictException("Company Status is Incorrect");
        }
        if (result.CompanyRoleIds == null || result.ConsentStatusDetails == null)
        {
            throw new UnexpectedConditionException("neither CompanyRoleIds nor ConsentStatusDetails should ever be null here");
        }

        var agreementAssignedRoleData = await companyRepositories.GetAgreementAssignedRolesDataAsync(companyRoleConsentDetails.Select(x => x.CompanyRole))
                .PreSortedGroupBy(x => x.CompanyRoleId, x => x.AgreementId)
                .ToDictionaryAsync(g => g.Key, g => g.AsEnumerable()).ConfigureAwait(false);

        var joined = companyRoleConsentDetails
            .Join(agreementAssignedRoleData,
                details => details.CompanyRole,
                data => data.Key,
                (details, data) => (
                    CompanyRoleId: details.CompanyRole,
                    AllActiveAgreements: details.Agreements.All(x => x.ConsentStatus == ConsentStatusId.ACTIVE),
                    AllInActiveAgreements: details.Agreements.All(x => x.ConsentStatus == ConsentStatusId.INACTIVE),
                    Agreements: details.Agreements,
                    MissingAgreementIds: data.Value.Except(details.Agreements.Where(x => x.ConsentStatus == ConsentStatusId.ACTIVE).Select(x => x.AgreementId)),
                    ExtraAgreementIds: details.Agreements.ExceptBy(data.Value, x => x.AgreementId).Select(x => x.AgreementId)))
            .ToList();

        var missing = joined.Where(x => x.MissingAgreementIds.Any() && !x.AllInActiveAgreements);
        if (missing.Any())
        {
            throw new ControllerArgumentException($"All agreements need to get signed as Active or InActive. Missing consents: [{string.Join(", ", missing.Select(x => $"{x.CompanyRoleId}: [{string.Join(", ", x.MissingAgreementIds)}]"))}]");
        }
        if (!joined.Any(x => x.AllActiveAgreements))
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
            joined.SelectMany(x => x.Agreements).DistinctBy(active => active.AgreementId).Select(active => (active.AgreementId, active.ConsentStatus)).ToList(),
            identity.CompanyId,
            identity.UserId,
            _dateTimeProvider.OffsetNow);

        var companyRolesRepository = _portalRepositories.GetInstance<ICompanyRolesRepository>();

        companyRolesRepository.CreateCompanyAssignedRoles(identity.CompanyId, joined.Where(j => j.AllActiveAgreements && !result.CompanyRoleIds.Contains(j.CompanyRoleId)).Select(x => x.CompanyRoleId));
        companyRolesRepository.RemoveCompanyAssignedRoles(identity.CompanyId, joined.Where(j => j.AllInActiveAgreements && result.CompanyRoleIds.Contains(j.CompanyRoleId)).Select(x => x.CompanyRoleId));

        await _portalRepositories.SaveAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<UseCaseParticipationData>> GetUseCaseParticipationAsync(Guid companyId, string? language) =>
        await _portalRepositories
            .GetInstance<ICompanySsiDetailsRepository>()
            .GetUseCaseParticipationForCompany(companyId, language ?? Constants.DefaultLanguage)
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
    public async Task<IEnumerable<SsiCertificateData>> GetSsiCertificatesAsync(Guid companyId) =>
        await _portalRepositories
            .GetInstance<ICompanySsiDetailsRepository>()
            .GetSsiCertificates(companyId)
            .Select(x => new SsiCertificateData(
                x.CredentialType,
                x.SsiDetailData.CatchingInto(
                    data => data
                        .Select(d => new CompanySsiDetailData(
                            d.CredentialId,
                            d.ParticipationStatus,
                            d.ExpiryDate,
                            d.Document))
                        .SingleOrDefault(),
                    (InvalidOperationException _) => new ConflictException("There should only be one pending or active ssi detail be assigned")
                )))
            .ToListAsync()
            .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task CreateUseCaseParticipation((Guid UserId, Guid CompanyId) identity, UseCaseParticipationCreationData data, CancellationToken cancellationToken)
    {
        var (verifiedCredentialExternalTypeDetailId, credentialTypeId, document) = data;
        var documentContentType = document.ContentType.ParseMediaTypeId();
        documentContentType.CheckDocumentContentType(_settings.UseCaseParticipationMediaTypes);

        var companyCredentialDetailsRepository = _portalRepositories.GetInstance<ICompanySsiDetailsRepository>();
        if (!await companyCredentialDetailsRepository.CheckCredentialTypeIdExistsForExternalTypeDetailVersionId(verifiedCredentialExternalTypeDetailId, credentialTypeId).ConfigureAwait(false))
        {
            throw new ControllerArgumentException($"VerifiedCredentialExternalTypeDetail {verifiedCredentialExternalTypeDetailId} does not exist");
        }

        await HandleSsiCreationAsync(identity, new(credentialTypeId, VerifiedCredentialTypeKindId.USE_CASE, verifiedCredentialExternalTypeDetailId, document, documentContentType), companyCredentialDetailsRepository, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task CreateSsiCertificate((Guid UserId, Guid CompanyId) identity, SsiCertificateCreationData data, CancellationToken cancellationToken)
    {
        var (credentialTypeId, document) = data;
        var documentContentType = document.ContentType.ParseMediaTypeId();
        documentContentType.CheckDocumentContentType(_settings.SsiCertificateMediaTypes);

        var companyCredentialDetailsRepository = _portalRepositories.GetInstance<ICompanySsiDetailsRepository>();
        if (!await companyCredentialDetailsRepository.CheckSsiCertificateType(credentialTypeId).ConfigureAwait(false))
        {
            throw new ControllerArgumentException($"{credentialTypeId} is not assigned to a certificate");
        }

        await HandleSsiCreationAsync(identity, new(credentialTypeId, VerifiedCredentialTypeKindId.CERTIFICATE, null, document, documentContentType), companyCredentialDetailsRepository, cancellationToken).ConfigureAwait(false);
    }

    private async Task HandleSsiCreationAsync(
        (Guid UserId, Guid CompanyId) identity,
        (VerifiedCredentialTypeId CredentialTypeId, VerifiedCredentialTypeKindId kindId, Guid? VerifiedCredentialExternalTypeDetailId, IFormFile Document, MediaTypeId MediaTypeId) creationData,
        ICompanySsiDetailsRepository companyCredentialDetailsRepository,
        CancellationToken cancellationToken)
    {
        var (credentialTypeId, kindId, verifiedCredentialExternalTypeDetailId, document, mediaTypeId) = creationData;
        if (await companyCredentialDetailsRepository.CheckSsiDetailsExistsForCompany(identity.CompanyId, credentialTypeId, kindId, verifiedCredentialExternalTypeDetailId).ConfigureAwait(false))
        {
            throw new ControllerArgumentException("Credential request already existing");
        }

        var (documentContent, hash) = await document.GetContentAndHash(cancellationToken).ConfigureAwait(false);
        var doc = _portalRepositories.GetInstance<IDocumentRepository>().CreateDocument(document.FileName, documentContent,
            hash, mediaTypeId, DocumentTypeId.PRESENTATION, x =>
            {
                x.CompanyUserId = identity.UserId;
                x.DocumentStatusId = DocumentStatusId.PENDING;
            });

        companyCredentialDetailsRepository.CreateSsiDetails(identity.CompanyId, credentialTypeId, doc.Id, CompanySsiDetailStatusId.PENDING, identity.UserId, details =>
            {
                if (verifiedCredentialExternalTypeDetailId != null)
                {
                    details.VerifiedCredentialExternalTypeUseCaseDetailId = verifiedCredentialExternalTypeDetailId;
                }
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
    public async Task ApproveCredential(Guid userId, Guid credentialId, CancellationToken cancellationToken)
    {
        var companySsiRepository = _portalRepositories.GetInstance<ICompanySsiDetailsRepository>();
        var (exists, data) = await companySsiRepository.GetSsiApprovalData(credentialId).ConfigureAwait(false);
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

        var typeValue = data.Type.GetEnumValue() ?? throw new UnexpectedConditionException($"VerifiedCredentialType {data.Type.ToString()} does not exists");
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
                await _custodianService.TriggerFrameworkAsync(data.Bpn, data.UseCaseDetailData!, cancellationToken).ConfigureAwait(false);
                break;
            case VerifiedCredentialTypeKindId.CERTIFICATE:
                await _custodianService.TriggerDismantlerAsync(data.Bpn, data.Type, cancellationToken).ConfigureAwait(false);
                break;
            default:
                throw new ArgumentOutOfRangeException($"{data.Kind} is currently not supported");
        }

        await _portalRepositories.SaveAsync().ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(data.RequesterData.RequesterEmail))
        {
            var userName = string.Join(" ", new[] { data.RequesterData.Firstname, data.RequesterData.Lastname }.Where(item => !string.IsNullOrWhiteSpace(item)));
            var mailParameters = new Dictionary<string, string>
            {
                { "userName", !string.IsNullOrWhiteSpace(userName) ?  userName : data.RequesterData.RequesterEmail },
                { "requestName", typeValue },
                { "companyName", data.CompanyName },
                { "credentialType", typeValue },
                { "expiryDate", data.ExpiryDate == null ? string.Empty : data.ExpiryDate.Value.ToString("o", CultureInfo.InvariantCulture) }
            };

            await _mailingService.SendMails(data.RequesterData.RequesterEmail, mailParameters, Enumerable.Repeat("CredentialApproval", 1)).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async Task RejectCredential(Guid userId, Guid credentialId)
    {
        var companySsiRepository = _portalRepositories.GetInstance<ICompanySsiDetailsRepository>();
        var (exists, status, type, requesterId, requesterEmail, requesterFirstname, requesterLastname) = await companySsiRepository.GetSsiRejectionData(credentialId).ConfigureAwait(false);
        if (!exists)
        {
            throw new NotFoundException($"CompanySsiDetail {credentialId} does not exists");
        }

        if (status != CompanySsiDetailStatusId.PENDING)
        {
            throw new ConflictException($"Credential {credentialId} must be {CompanySsiDetailStatusId.PENDING}");
        }
        var typeValue = type.GetEnumValue() ?? throw new UnexpectedConditionException($"VerifiedCredentialType {type.ToString()} does not exists");
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
                { "userName", !string.IsNullOrWhiteSpace(userName) ?  userName : requesterEmail },
                { "requestName", typeValue }
            };

            await _mailingService.SendMails(requesterEmail, mailParameters, Enumerable.Repeat("CredentialRejected", 1)).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public IAsyncEnumerable<VerifiedCredentialTypeId> GetCertificateTypes() =>
        _portalRepositories.GetInstance<ICompanySsiDetailsRepository>().GetCertificateTypes();
}
