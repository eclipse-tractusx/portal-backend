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

using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Async;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

public class CompanyDataBusinessLogic : ICompanyDataBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="portalRepositories"></param>
    public CompanyDataBusinessLogic(IPortalRepositories portalRepositories)
    {
        _portalRepositories = portalRepositories;
    }

    /// <inheritdoc/>
    public async Task<CompanyAddressDetailData> GetOwnCompanyDetailsAsync(string iamUserId)
    {
        var result = await _portalRepositories.GetInstance<ICompanyRepository>().GetOwnCompanyDetailsAsync(iamUserId).ConfigureAwait(false);
        if (result == null)
        {
            throw new ConflictException($"user {iamUserId} is not associated with any company");
        }
        return result;
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<CompanyAssignedUseCaseData> GetCompanyAssigendUseCaseDetailsAsync(string iamUserId) =>
        _portalRepositories.GetInstance<ICompanyRepository>().GetCompanyAssigendUseCaseDetailsAsync(iamUserId);

    /// <inheritdoc/>
    public async Task<bool> CreateCompanyAssignedUseCaseDetailsAsync(string iamUserId, Guid useCaseId)
    {
        var companyRepositories = _portalRepositories.GetInstance<ICompanyRepository>();
        var useCaseDetails = await companyRepositories.GetCompanyStatusAndUseCaseIdAsync(iamUserId, useCaseId).ConfigureAwait(false);
        if (!useCaseDetails.IsActiveCompanyStatus)
        {
            throw new ConflictException("Company Status is Incorrect");
        }
        if (useCaseDetails.IsUseCaseIdExists)
        {
            return false;
        }
        companyRepositories.CreateCompanyAssignedUseCase(useCaseDetails.CompanyId, useCaseId);
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
        return true;
    }

    /// <inheritdoc/>
    public async Task RemoveCompanyAssignedUseCaseDetailsAsync(string iamUserId, Guid useCaseId)
    {
        var companyRepositories = _portalRepositories.GetInstance<ICompanyRepository>();
        var useCaseDetails = await companyRepositories.GetCompanyStatusAndUseCaseIdAsync(iamUserId, useCaseId).ConfigureAwait(false);
        if (!useCaseDetails.IsActiveCompanyStatus)
        {
            throw new ConflictException("Company Status is Incorrect");
        }
        if (!useCaseDetails.IsUseCaseIdExists)
        {
            throw new ConflictException($"UseCaseId {useCaseId} is not available");
        }
        companyRepositories.RemoveCompanyAssignedUseCase(useCaseDetails.CompanyId, useCaseId);
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    public async IAsyncEnumerable<CompanyRoleConsentViewData> GetCompanyRoleAndConsentAgreementDetailsAsync(string iamUserId, string? languageShortName)
    {
        if (languageShortName != null && !await _portalRepositories.GetInstance<ILanguageRepository>().IsValidLanguageCode(languageShortName).ConfigureAwait(false))
        {
            throw new ControllerArgumentException($"language {languageShortName} is not a valid languagecode");
        }

        var companyRepositories = _portalRepositories.GetInstance<ICompanyRepository>();
        var companyData = await companyRepositories.GetCompanyStatusDataAsync(iamUserId).ConfigureAwait(false);
        if (companyData == default)
        {
            throw new NotFoundException($"User {iamUserId} is not associated with any company");
        }
        if (!companyData.IsActive)
        {
            throw new ConflictException("Company Status is Incorrect");
        }
        await foreach (var data in companyRepositories.GetCompanyRoleAndConsentAgreementDataAsync(companyData.CompanyId, languageShortName ?? Constants.DefaultLanguage).ConfigureAwait(false))
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
    public async Task CreateCompanyRoleAndConsentAgreementDetailsAsync(string iamUserId, IEnumerable<CompanyRoleConsentDetails> companyRoleConsentDetails)
    {
        if (!companyRoleConsentDetails.Any())
        {
            return;
        }
        var companyRepositories = _portalRepositories.GetInstance<ICompanyRepository>();
        var result = await companyRepositories.GetCompanyRolesDataAsync(iamUserId, companyRoleConsentDetails.Select(x => x.CompanyRole)).ConfigureAwait(false);
        if (result == default)
        {
            throw new ForbiddenException($"user {iamUserId} is not associated with any company");
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
            throw new ConflictException($"companyRoles [{string.Join(", ", result.CompanyRoleIds)}] are already assigned to company {result.CompanyId}");
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
            result.CompanyId,
            result.CompanyUserId,
            DateTimeOffset.UtcNow);

        _portalRepositories.GetInstance<ICompanyRolesRepository>().CreateCompanyAssignedRoles(result.CompanyId, joined.Select(x => x.CompanyRoleId));

        await _portalRepositories.SaveAsync();
    }
}
