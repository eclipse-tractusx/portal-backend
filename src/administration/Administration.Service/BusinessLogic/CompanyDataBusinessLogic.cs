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
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Async;
using Org.Eclipse.TractusX.Portal.Backend.Framework.DateTimeProvider;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Identity;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Linq;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Web;
using Org.Eclipse.TractusX.Portal.Backend.IssuerComponent.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Text.RegularExpressions;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

public class CompanyDataBusinessLogic(
    IPortalRepositories portalRepositories,
    IDateTimeProvider dateTimeProvider,
    IIdentityService identityService,
    IIssuerComponentBusinessLogic issuerComponentBusinessLogic,
    IOptions<CompanyDataSettings> options) : ICompanyDataBusinessLogic
{
    private static readonly Regex BpnsRegex = new(ValidationExpressions.Bpns, RegexOptions.Compiled, TimeSpan.FromSeconds(1));
    private static readonly Regex EcmRegex = new(ValidationExpressions.ExternalCertificateNumber, RegexOptions.Compiled, TimeSpan.FromSeconds(1));
    private readonly IIdentityData _identityData = identityService.IdentityData;
    private readonly CompanyDataSettings _settings = options.Value;

    /// <inheritdoc/>
    public async Task<CompanyAddressDetailData> GetCompanyDetailsAsync()
    {
        var companyId = _identityData.CompanyId;
        var result = await portalRepositories.GetInstance<ICompanyRepository>().GetCompanyDetailsAsync(companyId)
            .ConfigureAwait(ConfigureAwaitOptions.None);
        if (result == null)
        {
            throw ConflictException.Create(AdministrationCompanyDataErrors.COMPANY_DATA_CONFLICT_INVALID_COMPANY, new ErrorParameter[] { new(nameof(companyId), companyId.ToString()) });
        }

        return result;
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<CompanyAssignedUseCaseData> GetCompanyAssigendUseCaseDetailsAsync() =>
        portalRepositories.GetInstance<ICompanyRepository>()
            .GetCompanyAssigendUseCaseDetailsAsync(_identityData.CompanyId);

    /// <inheritdoc/>
    public async Task<bool> CreateCompanyAssignedUseCaseDetailsAsync(Guid useCaseId)
    {
        var companyId = _identityData.CompanyId;
        var companyRepositories = portalRepositories.GetInstance<ICompanyRepository>();
        var useCaseDetails = await companyRepositories.GetCompanyStatusAndUseCaseIdAsync(companyId, useCaseId)
            .ConfigureAwait(ConfigureAwaitOptions.None);
        if (!useCaseDetails.IsActiveCompanyStatus)
        {
            throw ConflictException.Create(AdministrationCompanyDataErrors.COMPANY_DATA_CONFLICT_INCORR_COMPANY_STATUS);
        }

        if (useCaseDetails.IsUseCaseIdExists)
        {
            return false;
        }

        companyRepositories.CreateCompanyAssignedUseCase(companyId, useCaseId);
        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
        return true;
    }

    /// <inheritdoc/>
    public async Task RemoveCompanyAssignedUseCaseDetailsAsync(Guid useCaseId)
    {
        var companyId = _identityData.CompanyId;
        var companyRepositories = portalRepositories.GetInstance<ICompanyRepository>();
        var useCaseDetails = await companyRepositories.GetCompanyStatusAndUseCaseIdAsync(companyId, useCaseId)
            .ConfigureAwait(ConfigureAwaitOptions.None);
        if (!useCaseDetails.IsActiveCompanyStatus)
        {
            throw ConflictException.Create(AdministrationCompanyDataErrors.COMPANY_DATA_CONFLICT_INCORR_COMPANY_STATUS);
        }

        if (!useCaseDetails.IsUseCaseIdExists)
        {
            throw ConflictException.Create(AdministrationCompanyDataErrors.COMPANY_DATA_CONFLICT_USECASEID_NOT_AVAL);
        }

        companyRepositories.RemoveCompanyAssignedUseCase(companyId, useCaseId);
        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public async IAsyncEnumerable<CompanyRoleConsentViewData> GetCompanyRoleAndConsentAgreementDetailsAsync(string? languageShortName)
    {
        var companyId = _identityData.CompanyId;
        if (languageShortName != null && !await portalRepositories.GetInstance<ILanguageRepository>().IsValidLanguageCode(languageShortName).ConfigureAwait(ConfigureAwaitOptions.None))
        {
            throw ControllerArgumentException.Create(AdministrationCompanyDataErrors.COMPANY_DATA_ARGUMENT_LANG_CODE_NOT_VALID, new ErrorParameter[] { new(nameof(languageShortName), languageShortName) });
        }

        var companyRepositories = portalRepositories.GetInstance<ICompanyRepository>();
        var statusData = await companyRepositories.GetCompanyStatusDataAsync(companyId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (statusData == default)
        {
            throw ConflictException.Create(AdministrationCompanyDataErrors.COMPANY_DATA_NOT_COMPANY_NOT_EXIST, new ErrorParameter[] { new(nameof(companyId), companyId.ToString()) });
        }

        if (!statusData.IsActive)
        {
            throw ConflictException.Create(AdministrationCompanyDataErrors.COMPANY_DATA_CONFLICT_INCORR_COMPANY_STATUS);
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

        var companyRepositories = portalRepositories.GetInstance<ICompanyRepository>();
        var result = await companyRepositories
            .GetCompanyRolesDataAsync(_identityData.CompanyId, companyRoleConsentDetails.Select(x => x.CompanyRole))
            .ConfigureAwait(ConfigureAwaitOptions.None);
        if (!result.IsValidCompany)
        {
            throw ConflictException.Create(AdministrationCompanyDataErrors.COMPANY_DATA_NOT_COMPANY_NOT_EXIST, new ErrorParameter[] { new("companyId", _identityData.CompanyId.ToString()) });
        }

        if (!result.IsCompanyActive)
        {
            throw ConflictException.Create(AdministrationCompanyDataErrors.COMPANY_DATA_CONFLICT_INCORR_COMPANY_STATUS);
        }

        if (result.CompanyRoleIds == null || result.ConsentStatusDetails == null)
        {
            throw UnexpectedConditionException.Create(AdministrationCompanyDataErrors.COMPANY_DATA_UNEXP_COMP_ROLES_NOR_DETAILS_NULL);
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
            throw ControllerArgumentException.Create(AdministrationCompanyDataErrors.COMPANY_DATA_ARGUMENT_AGREEMENT_ACTIVE_INACTIVE_MISSING, new ErrorParameter[] { new("consentType", string.Join(", ", missing.Select(x => $"{x.CompanyRoleId}: [{string.Join(", ", x.MissingAgreementIds)}]"))) });
        }

        if (!joined.Exists(x => x.AllActiveAgreements))
        {
            throw ConflictException.Create(AdministrationCompanyDataErrors.COMPANY_DATA_CONFLICT_NOT_UNASSIGN_ALL_ROLES_ATLEAST_ONE_ACTIVE_NEEDED);
        }

        var extra = joined.Where(x => x.ExtraAgreementIds.Any());
        if (extra.Any())
        {
            throw ControllerArgumentException.Create(AdministrationCompanyDataErrors.COMPANY_DATA_ARGUMENT_AGREEMENT_NOT_ASSOCIATE_COMPANY_ROLES, new ErrorParameter[] { new("companyRoles", string.Join(", ", extra.Select(x => $"{x.CompanyRoleId}: [{string.Join(", ", x.ExtraAgreementIds)}]"))) });
        }

        portalRepositories.GetInstance<IConsentRepository>().AddAttachAndModifyConsents(
            result.ConsentStatusDetails,
            joined.SelectMany(x => x.Agreements).DistinctBy(active => active.AgreementId)
                .Select(active => (active.AgreementId, active.ConsentStatus)).ToList(),
            _identityData.CompanyId,
            _identityData.IdentityId,
            dateTimeProvider.OffsetNow);

        var companyRolesRepository = portalRepositories.GetInstance<ICompanyRolesRepository>();

        companyRolesRepository.CreateCompanyAssignedRoles(_identityData.CompanyId,
            joined.Where(j => j.AllActiveAgreements && !result.CompanyRoleIds.Contains(j.CompanyRoleId))
                .Select(x => x.CompanyRoleId));
        companyRolesRepository.RemoveCompanyAssignedRoles(_identityData.CompanyId,
            joined.Where(j => j.AllInActiveAgreements && result.CompanyRoleIds.Contains(j.CompanyRoleId))
                .Select(x => x.CompanyRoleId));

        await portalRepositories.SaveAsync();
    }

    /// <inheritdoc />
    public Task<Guid> CreateUseCaseParticipation(UseCaseParticipationCreationData data, string token, CancellationToken cancellationToken) =>
        issuerComponentBusinessLogic.CreateFrameworkCredentialData(data.VerifiedCredentialExternalTypeDetailId, data.CredentialType, _identityData.IdentityId, token, cancellationToken);

    /// <inheritdoc />
    public async Task CreateCompanyCertificate(CompanyCertificateCreationData data, CancellationToken cancellationToken)
    {
        if (data.ExternalCertificateNumber != null && !EcmRegex.IsMatch(data.ExternalCertificateNumber))
        {
            throw ControllerArgumentException.Create(AdministrationCompanyDataErrors.COMPANY_DATA_ARGUMENT_EXTER_CERT_APLHA_LENGTH);
        }

        if (data.Sites != null && data.Sites.Any(bpn => !BpnsRegex.IsMatch(bpn)))
        {
            throw ControllerArgumentException.Create(AdministrationCompanyDataErrors.COMPANY_DATA_ARGUMENT_PREFIXED_BPNS_SIXTEEN_CHAR);
        }

        var now = dateTimeProvider.OffsetNow;

        if (data.ValidFrom > now)
        {
            throw ControllerArgumentException.Create(AdministrationCompanyDataErrors.COMPANY_DATA_ARGUMENT_NOT_GREATER_CURR_DATE);
        }

        if (data.ValidTill < now)
        {
            throw ControllerArgumentException.Create(AdministrationCompanyDataErrors.COMPANY_DATA_ARGUMENT_SHOULD_GREATER_THAN_CURR_DATE);
        }

        if (data.Issuer != null && !data.Issuer.IsValidCompanyName())
        {
            throw ControllerArgumentException.Create(ValidationExpressionErrors.INCORRECT_COMPANY_NAME, [new ErrorParameter("name", nameof(data.Issuer))]);
        }

        var documentContentType = data.Document.ContentType.ParseMediaTypeId();
        documentContentType.CheckDocumentContentType(_settings.CompanyCertificateMediaTypes);

        var companyCertificateRepository = portalRepositories.GetInstance<ICompanyCertificateRepository>();
        if (!await companyCertificateRepository.CheckCompanyCertificateType(data.CertificateType).ConfigureAwait(ConfigureAwaitOptions.None))
        {
            throw ControllerArgumentException.Create(AdministrationCompanyDataErrors.COMPANY_DATA_ARGUMENT_CERT_TYPE_NOT_ASSIGN_CERTIFICATE, new ErrorParameter[] { new("certificateType", data.CertificateType.ToString()) });
        }

        await HandleCompanyCertificateCreationAsync(data.CertificateType, data.Document, documentContentType, companyCertificateRepository, data.ExternalCertificateNumber, data.Sites, data.ValidFrom, data.ValidTill, data.Issuer, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
    }

    private async Task HandleCompanyCertificateCreationAsync(CompanyCertificateTypeId companyCertificateTypeId,
        IFormFile document,
        MediaTypeId mediaTypeId,
        ICompanyCertificateRepository companyCertificateRepository,
        string? externalCertificateNumber,
        IEnumerable<string>? sites,
        DateTimeOffset? validFrom,
        DateTimeOffset? validTill,
        string? issuer,
        CancellationToken cancellationToken)
    {
        var (documentContent, hash) = await document.GetContentAndHash(cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        var doc = portalRepositories.GetInstance<IDocumentRepository>().CreateDocument(document.FileName, documentContent,
            hash, mediaTypeId, DocumentTypeId.COMPANY_CERTIFICATE, document.Length, x =>
            {
                x.CompanyUserId = _identityData.IdentityId;
                x.DocumentStatusId = DocumentStatusId.LOCKED;
            });

        var companyCertificate = companyCertificateRepository.CreateCompanyCertificate(_identityData.CompanyId, companyCertificateTypeId, CompanyCertificateStatusId.ACTIVE, doc.Id,
            x =>
            {
                x.ExternalCertificateNumber = externalCertificateNumber;
                x.Issuer = issuer;
                x.ValidFrom = validFrom;
                x.ValidTill = validTill;
            });

        sites?.IfAny(x => companyCertificateRepository.CreateCompanyCertificateAssignedSites(companyCertificate.Id, x));

        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    /// <inheritdoc />    
    public async IAsyncEnumerable<CompanyCertificateBpnData> GetCompanyCertificatesByBpn(string businessPartnerNumber)
    {
        if (string.IsNullOrWhiteSpace(businessPartnerNumber))
        {
            throw ControllerArgumentException.Create(AdministrationCompanyDataErrors.COMPANY_DATA_ARGUMENT_BPN_NOT_EMPTY);
        }

        var companyCertificateRepository = portalRepositories.GetInstance<ICompanyCertificateRepository>();

        var companyId = await companyCertificateRepository.GetCompanyIdByBpn(businessPartnerNumber).ConfigureAwait(ConfigureAwaitOptions.None);
        if (companyId == Guid.Empty)
        {
            throw ControllerArgumentException.Create(AdministrationCompanyDataErrors.COMPANY_DATA_ARGUMENT_COMP_NOT_EXISTS_FOR_BPN, new ErrorParameter[] { new(nameof(businessPartnerNumber), businessPartnerNumber) });
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
            portalRepositories.GetInstance<ICompanyCertificateRepository>().GetActiveCompanyCertificatePaginationSource(sorting, certificateStatus, certificateType, _identityData.CompanyId));

    public async Task<DimUrlsResponse> GetDimServiceUrls()
    {
        var (bpnl, did, walletServiceUrl) = await portalRepositories.GetInstance<ICompanyRepository>().GetDimServiceUrls(_identityData.CompanyId).ConfigureAwait(ConfigureAwaitOptions.None);
        return new(
            _settings.IssuerDid,
            bpnl,
            did,
            _settings.BpnDidResolverUrl,
            walletServiceUrl is null ? null : $"{walletServiceUrl}/oauth/token",
            _settings.DecentralIdentityManagementAuthUrl
        );
    }

    /// <inheritdoc />
    public async Task<int> DeleteCompanyCertificateAsync(Guid documentId)
    {
        var companyCertificateRepository = portalRepositories.GetInstance<ICompanyCertificateRepository>();

        var details = await companyCertificateRepository.GetCompanyCertificateDocumentDetailsForIdUntrackedAsync(documentId, _identityData.CompanyId).ConfigureAwait(ConfigureAwaitOptions.None);

        var certificateCount = details.CompanyCertificateId.Count();
        if (certificateCount > 1)
        {
            throw ConflictException.Create(AdministrationCompanyDataErrors.COMPANY_DATA_CONFLICT_MULTIPLE_ACTIVE_CERT_NOT_ALLOWED_ONE_DOC, new ErrorParameter[] { new(nameof(documentId), documentId.ToString()) });
        }

        if (details.DocumentId == Guid.Empty)
        {
            throw NotFoundException.Create(AdministrationCompanyDataErrors.COMPANY_DATA_NOT_DOC_NOT_EXIST);
        }

        if (!details.IsSameCompany)
        {
            throw ForbiddenException.Create(AdministrationCompanyDataErrors.COMPANY_DATA_FORBIDDEN_USER_NOT_ALLOW_DEL_DOC);
        }

        companyCertificateRepository.AttachAndModifyCompanyCertificateDocumentDetails(documentId, null, c =>
            {
                c.DocumentStatusId = DocumentStatusId.INACTIVE;
                c.DateLastChanged = dateTimeProvider.OffsetNow;
            });

        if (certificateCount == 1)
        {
            companyCertificateRepository.AttachAndModifyCompanyCertificateDetails(details.CompanyCertificateId.SingleOrDefault(), null, c =>
            {
                c.CompanyCertificateStatusId = CompanyCertificateStatusId.INACTIVE;
            });
        }

        return await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    /// <inheritdoc />
    public async Task<(string FileName, byte[] Content, string MediaType)> GetCompanyCertificateDocumentByCompanyIdAsync(Guid documentId)
    {
        var documentDetails = await portalRepositories.GetInstance<ICompanyCertificateRepository>()
            .GetCompanyCertificateDocumentByCompanyIdDataAsync(documentId, _identityData.CompanyId, DocumentTypeId.COMPANY_CERTIFICATE)
            .ConfigureAwait(ConfigureAwaitOptions.None);

        if (!documentDetails.Exists)
        {
            throw NotFoundException.Create(AdministrationCompanyDataErrors.COMPANY_DATA_NOT_COMP_CERT_DOC_NOT_EXIST, new ErrorParameter[] { new(nameof(documentId), documentId.ToString()) });
        }

        return (documentDetails.FileName, documentDetails.Content, documentDetails.MediaTypeId.MapToMediaType());
    }

    /// <inheritdoc />
    public async Task<(string FileName, byte[] Content, string MediaType)> GetCompanyCertificateDocumentAsync(Guid documentId)
    {
        var documentDetails = await portalRepositories.GetInstance<ICompanyCertificateRepository>()
            .GetCompanyCertificateDocumentDataAsync(documentId, DocumentTypeId.COMPANY_CERTIFICATE)
            .ConfigureAwait(ConfigureAwaitOptions.None);

        if (!documentDetails.Exists)
        {
            throw NotFoundException.Create(AdministrationCompanyDataErrors.COMPANY_DATA_NOT_COMP_CERT_DOC_NOT_EXIST, new ErrorParameter[] { new(nameof(documentId), documentId.ToString()) });
        }

        if (!documentDetails.IsStatusLocked)
        {
            throw ForbiddenException.Create(AdministrationCompanyDataErrors.COMPANY_DATA_FORBIDDEN_DOC_STATUS_NOT_LOCKED, new ErrorParameter[] { new(nameof(documentId), documentId.ToString()) });
        }

        return (documentDetails.FileName, documentDetails.Content, documentDetails.MediaTypeId.MapToMediaType());
    }

    public Task<Pagination.Response<CompanyMissingSdDocumentData>> GetCompaniesWithMissingSdDocument(int page, int size) =>
        Pagination.CreateResponseAsync(
            page,
            size,
            _settings.MaxPageSize,
            portalRepositories.GetInstance<ICompanyRepository>().GetCompaniesWithMissingSdDocument());

    public async Task TriggerSelfDescriptionCreation()
    {
        var companiesWithMissingSd = portalRepositories.GetInstance<ICompanyRepository>().GetCompanyIdsWithMissingSelfDescription();
        var processStepRepository = portalRepositories.GetInstance<IPortalProcessStepRepository>();
        var companyRepository = portalRepositories.GetInstance<ICompanyRepository>();
        await foreach (var companyId in companiesWithMissingSd)
        {
            var processId = processStepRepository.CreateProcess(ProcessTypeId.SELF_DESCRIPTION_CREATION).Id;
            processStepRepository.CreateProcessStep(ProcessStepTypeId.SELF_DESCRIPTION_COMPANY_CREATION, ProcessStepStatusId.TODO, processId);
            companyRepository.AttachAndModifyCompany(companyId, c => c.SdCreationProcessId = null, c => c.SdCreationProcessId = processId);
        }

        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public Task RetriggerSelfDescriptionCreation(Guid processId) => RetriggerSelfDescriptionCompanyCreation(processId, ProcessStepTypeId.RETRIGGER_SELF_DESCRIPTION_COMPANY_CREATION);

    public Task RetriggerSelfDescriptionResponseCreation(Guid processId) => RetriggerSelfDescriptionCompanyCreation(processId, ProcessStepTypeId.RETRIGGER_AWAIT_SELF_DESCRIPTION_COMPANY_RESPONSE);

    private async Task RetriggerSelfDescriptionCompanyCreation(Guid processId, ProcessStepTypeId stepToTrigger)
    {
        const ProcessStepTypeId NextStep = ProcessStepTypeId.SELF_DESCRIPTION_COMPANY_CREATION;

        var (validProcessId, processData) = await portalRepositories.GetInstance<IPortalProcessStepRepository>().IsValidProcess(processId, ProcessTypeId.SELF_DESCRIPTION_CREATION, Enumerable.Repeat(stepToTrigger, 1)).ConfigureAwait(ConfigureAwaitOptions.None);
        if (!validProcessId)
        {
            throw NotFoundException.Create(AdministrationCompanyDataErrors.COMPANY_DATA_NOT_PROCESSID_NOT_EXIST, new ErrorParameter[] { new(nameof(processId), processId.ToString()) });
        }

        var context = processData.CreateManualProcessData(stepToTrigger, portalRepositories, () => $"processId {processId}");

        context.ScheduleProcessSteps(Enumerable.Repeat(NextStep, 1));
        context.FinalizeProcessStep();
        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }
}
