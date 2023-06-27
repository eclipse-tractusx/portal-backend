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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Async;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Linq;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Web;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

public class CompanyDataBusinessLogic : ICompanyDataBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly CompanyDataSettings _settings;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="portalRepositories"></param>
    /// <param name="options"></param>
    public CompanyDataBusinessLogic(IPortalRepositories portalRepositories, IOptions<CompanyDataSettings> options)
    {
        _portalRepositories = portalRepositories;
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
        if (result.CompanyRoleIds.Any())
        {
            throw new ConflictException($"companyRoles [{string.Join(", ", result.CompanyRoleIds)}] are already assigned to company {identity.CompanyId}");
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
                    ActiveAgreements: details.Agreements.Where(x => x.ConsentStatus == ConsentStatusId.ACTIVE),
                    MissingAgreementIds: data.Value.Except(details.Agreements.Where(x => x.ConsentStatus == ConsentStatusId.ACTIVE).Select(x => x.AgreementId)),
                    ExtraAgreementIds: details.Agreements.ExceptBy(data.Value, x => x.AgreementId).Select(x => x.AgreementId)))
            .ToList();

        var missing = joined.Where(x => x.MissingAgreementIds.Any());
        if (missing.Any())
        {
            throw new ControllerArgumentException($"All agreements need to get signed. Missing active consents: [{string.Join(", ", missing.Select(x => $"{x.CompanyRoleId}: [{string.Join(", ", x.MissingAgreementIds)}]"))}]");
        }
        var extra = joined.Where(x => x.ExtraAgreementIds.Any());
        if (extra.Any())
        {
            throw new ControllerArgumentException($"Agreements not associated with requested companyRoles: [{string.Join(", ", extra.Select(x => $"{x.CompanyRoleId}: [{string.Join(", ", x.ExtraAgreementIds)}]"))}]");
        }

        _portalRepositories.GetInstance<IConsentRepository>().AddAttachAndModifyConsents(
            result.ConsentStatusDetails,
            joined.SelectMany(x => x.ActiveAgreements).DistinctBy(active => active.AgreementId).Select(active => (active.AgreementId, active.ConsentStatus)).ToList(),
            identity.CompanyId,
            identity.UserId,
            DateTimeOffset.UtcNow);

        _portalRepositories.GetInstance<ICompanyRolesRepository>().CreateCompanyAssignedRoles(identity.CompanyId, joined.Select(x => x.CompanyRoleId));

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
    public async Task CreateUseCaseParticipation((Guid UserId, Guid CompanyId) identity, UseCaseParticipationCreationData data, CancellationToken cts)
    {
        var (verifiedCredentialExternalTypeDetailId, credentialTypeId, document) = data;
        var documentContentType = document.ContentType.ParseMediaTypeId();
        documentContentType.CheckDocumentContent(_settings.UseCaseParticipationMediaTypes);

        var companyCredentialDetailsRepository = _portalRepositories.GetInstance<ICompanySsiDetailsRepository>();
        if (!await companyCredentialDetailsRepository.CheckCredentialTypeIdExistsForExternalTypeDetailVersionId(verifiedCredentialExternalTypeDetailId, credentialTypeId))
        {
            throw new ControllerArgumentException($"VerifiedCredentialExternalTypeDetail {verifiedCredentialExternalTypeDetailId} does not exist");
        }

        await HandleSsiCreationAsync(identity, credentialTypeId, VerifiedCredentialTypeKindId.USE_CASE, verifiedCredentialExternalTypeDetailId, document, companyCredentialDetailsRepository, cts).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task CreateSsiCertificate((Guid UserId, Guid CompanyId) identity, SsiCertificateCreationData data, CancellationToken cts)
    {
        var (credentialTypeId, document) = data;
        var documentContentType = document.ContentType.ParseMediaTypeId();
        documentContentType.CheckDocumentContent(_settings.SsiCertificateMediaTypes);

        var companyCredentialDetailsRepository = _portalRepositories.GetInstance<ICompanySsiDetailsRepository>();
        var checkResult = await companyCredentialDetailsRepository.CheckSsiCertificateType(credentialTypeId);
        if (!checkResult)
        {
            throw new ControllerArgumentException($"{credentialTypeId} is not assigned to a certificate");
        }

        await HandleSsiCreationAsync(identity, credentialTypeId, VerifiedCredentialTypeKindId.CERTIFICATE, null, document, companyCredentialDetailsRepository, cts).ConfigureAwait(false);
    }

    private async Task HandleSsiCreationAsync(
        (Guid UserId, Guid CompanyId) identity,
        VerifiedCredentialTypeId credentialTypeId,
        VerifiedCredentialTypeKindId kindId,
        Guid? verifiedCredentialExternalTypeDetailId,
        IFormFile document,
        ICompanySsiDetailsRepository companyCredentialDetailsRepository,
        CancellationToken cts)
    {
        if (await companyCredentialDetailsRepository.CheckSsiDetailsExistsForCompany(identity.CompanyId, credentialTypeId, kindId, verifiedCredentialExternalTypeDetailId).ConfigureAwait(false))
        {
            throw new ControllerArgumentException("Credential request already existing");
        }

        var (documentContent, hash) = await document.GetContentAndHash(cts).ConfigureAwait(false);
        var doc = _portalRepositories.GetInstance<IDocumentRepository>().CreateDocument(document.FileName, documentContent,
            hash, document.ContentType.ParseMediaTypeId(), DocumentTypeId.PRESENTATION, x =>
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
}
