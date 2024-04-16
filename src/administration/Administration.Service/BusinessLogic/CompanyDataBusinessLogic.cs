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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Async;
using Org.Eclipse.TractusX.Portal.Backend.Framework.DateTimeProvider;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Web;
using Org.Eclipse.TractusX.Portal.Backend.IssuerComponent.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

public class CompanyDataBusinessLogic(
    IPortalRepositories portalRepositories,
    IDateTimeProvider dateTimeProvider,
    IIdentityService identityService,
    IIssuerComponentBusinessLogic _issuerComponentBusinessLogic,
    IOptions<CompanyDataSettings> options) : ICompanyDataBusinessLogic
{
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
            throw new ConflictException($"company {companyId} is not a valid company");
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
            throw new ConflictException("Company Status is Incorrect");
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
            throw new ConflictException("Company Status is Incorrect");
        }

        if (!useCaseDetails.IsUseCaseIdExists)
        {
            throw new ConflictException($"UseCaseId {useCaseId} is not available");
        }

        companyRepositories.RemoveCompanyAssignedUseCase(companyId, useCaseId);
        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public async IAsyncEnumerable<CompanyRoleConsentViewData> GetCompanyRoleAndConsentAgreementDetailsAsync(string? languageShortName)
    {
        var companyId = _identityData.CompanyId;
        if (languageShortName != null && !await portalRepositories.GetInstance<ILanguageRepository>().IsValidLanguageCode(languageShortName).ConfigureAwait(ConfigureAwaitOptions.None))
        {
            throw new ControllerArgumentException($"language {languageShortName} is not a valid languagecode");
        }

        var companyRepositories = portalRepositories.GetInstance<ICompanyRepository>();
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

        var companyRepositories = portalRepositories.GetInstance<ICompanyRepository>();
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
    public Task<Guid> CreateUseCaseParticipation(UseCaseParticipationCreationData data, CancellationToken cancellationToken) =>
        _issuerComponentBusinessLogic.CreateFrameworkCredentialData(data.VerifiedCredentialExternalTypeDetailId, data.Framework, _identityData.IdentityId, cancellationToken);

    /// <inheritdoc />
    public async Task CreateCompanyCertificate(CompanyCertificateCreationData data, CancellationToken cancellationToken)
    {
        var documentContentType = data.Document.ContentType.ParseMediaTypeId();
        documentContentType.CheckDocumentContentType(_settings.CompanyCertificateMediaTypes);

        var companyCertificateRepository = portalRepositories.GetInstance<ICompanyCertificateRepository>();
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
        var doc = portalRepositories.GetInstance<IDocumentRepository>().CreateDocument(document.FileName, documentContent,
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

        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    /// <inheritdoc />    
    public async IAsyncEnumerable<CompanyCertificateBpnData> GetCompanyCertificatesByBpn(string businessPartnerNumber)
    {
        if (string.IsNullOrWhiteSpace(businessPartnerNumber))
        {
            throw new ControllerArgumentException("businessPartnerNumber must not be empty");
        }

        var companyCertificateRepository = portalRepositories.GetInstance<ICompanyCertificateRepository>();

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
            portalRepositories.GetInstance<ICompanyCertificateRepository>().GetActiveCompanyCertificatePaginationSource(sorting, certificateStatus, certificateType, _identityData.CompanyId));

    public async Task<DimUrlsResponse> GetDimServiceUrls() =>
        new(
            $"{await portalRepositories.GetInstance<ICompanyRepository>().GetWalletServiceUrl(_identityData.CompanyId).ConfigureAwait(ConfigureAwaitOptions.None)}/oauth/token",
            _settings.DecentralIdentityManagementAuthUrl
        );

    /// <inheritdoc />
    public async Task<int> DeleteCompanyCertificateAsync(Guid documentId)
    {
        var companyCertificateRepository = portalRepositories.GetInstance<ICompanyCertificateRepository>();

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
            throw new NotFoundException($"Company certificate document {documentId} does not exist");
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
            throw new NotFoundException($"Company certificate document {documentId} does not exist");
        }
        if (!documentDetails.IsStatusLocked)
        {
            throw new ForbiddenException($"Document {documentId} status is not locked");
        }

        return (documentDetails.FileName, documentDetails.Content, documentDetails.MediaTypeId.MapToMediaType());
    }
}
