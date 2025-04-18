/********************************************************************************
 * Copyright (c) 2022 BMW Group AG
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

using Microsoft.EntityFrameworkCore;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

public class CompanyRolesRepository : ICompanyRolesRepository
{
    private readonly PortalDbContext _dbContext;

    public CompanyRolesRepository(PortalDbContext portalDbContext)
    {
        _dbContext = portalDbContext;
    }

    public CompanyAssignedRole CreateCompanyAssignedRole(Guid companyId, CompanyRoleId companyRoleId) =>
        _dbContext.CompanyAssignedRoles.Add(
            new CompanyAssignedRole(
                companyId,
                companyRoleId
            )).Entity;

    public void CreateCompanyAssignedRoles(Guid companyId, IEnumerable<CompanyRoleId> companyRoleIds) =>
        _dbContext.AddRange(companyRoleIds.Select(companyRoleId => new CompanyAssignedRole(companyId, companyRoleId)));

    public void RemoveCompanyAssignedRoles(Guid companyId, IEnumerable<CompanyRoleId> companyRoleIds) =>
        _dbContext.CompanyAssignedRoles.RemoveRange(companyRoleIds.Select(companyRoleId => new CompanyAssignedRole(companyId, companyRoleId)));

    public Task<CompanyRoleAgreementConsentData?> GetCompanyRoleAgreementConsentDataAsync(Guid applicationId) =>
        _dbContext.CompanyApplications
            .Where(application => application.Id == applicationId)
            .Select(application => new CompanyRoleAgreementConsentData(
                application.CompanyId,
                application.ApplicationStatusId,
                application.Company!.CompanyAssignedRoles.Select(ar => ar.CompanyRoleId),
                application.Company!.Consents.Select(c => new ConsentData(c.Id, c.ConsentStatusId, c.AgreementId))))
            .SingleOrDefaultAsync();

    public IAsyncEnumerable<(CompanyRoleId CompanyRoleId, IEnumerable<AgreementStatusData> AgreementStatusData)> GetAgreementAssignedCompanyRolesUntrackedAsync(IEnumerable<CompanyRoleId> companyRoleIds) =>
        _dbContext.CompanyRoles
            .AsNoTracking()
            .Where(companyRole => companyRole.CompanyRoleRegistrationData!.IsRegistrationRole && companyRoleIds.Contains(companyRole.Id))
            .Select(companyRole => new ValueTuple<CompanyRoleId, IEnumerable<AgreementStatusData>>(
                companyRole.Id,
                companyRole.AgreementAssignedCompanyRoles.Where(x => x.Agreement!.AgreementStatusId == AgreementStatusId.ACTIVE)
                    .Select(agreementAssignedCompanyRole => new AgreementStatusData(
                        agreementAssignedCompanyRole.AgreementId,
                        agreementAssignedCompanyRole.Agreement!.AgreementStatusId
                    ))
            )).AsAsyncEnumerable();

    public Task<CompanyRoleAgreementConsents?> GetCompanyRoleAgreementConsentStatusUntrackedAsync(Guid applicationId, Guid companyId) =>
        _dbContext.Companies
            .AsNoTracking()
            .Where(company =>
                company.Id == companyId &&
                company.CompanyApplications.Any(application => application.Id == applicationId))
            .Select(company => new CompanyRoleAgreementConsents(
                company.CompanyAssignedRoles.Select(companyAssignedRole => companyAssignedRole.CompanyRoleId),
                company.Consents
                    .Where(consent => consent.ConsentStatusId == ConsentStatusId.ACTIVE && consent.Agreement!.AgreementStatusId == AgreementStatusId.ACTIVE && consent.Agreement.AgreementAssignedCompanyRoles.Any())
                    .Select(consent => new AgreementConsentStatus(
                        consent.AgreementId,
                        consent.ConsentStatusId
                    )))).SingleOrDefaultAsync();

    public async IAsyncEnumerable<CompanyRoleData> GetCompanyRoleAgreementsUntrackedAsync()
    {
        await foreach (var role in _dbContext.CompanyRoles
            .AsNoTracking()
            .Where(companyRole => companyRole.CompanyRoleRegistrationData!.IsRegistrationRole)
            .Select(companyRole => new
            {
                Id = companyRole.Id,
                Descriptions = companyRole.CompanyRoleDescriptions.Select(description => new
                {
                    ShortName = description.LanguageShortName,
                    Description = description.Description
                }),
                Agreements = companyRole.AgreementAssignedCompanyRoles.Where(agreementAssignedCompanyRole => agreementAssignedCompanyRole.Agreement!.AgreementStatusId == AgreementStatusId.ACTIVE).Select(agreementAssignedCompanyRole => agreementAssignedCompanyRole.AgreementId)
            })
            .AsAsyncEnumerable())
        {
            yield return new CompanyRoleData(
                role.Id,
                role.Descriptions.ToDictionary(d => d.ShortName, d => d.Description),
                role.Agreements);
        }
    }

    public IAsyncEnumerable<CompanyRolesDetails> GetCompanyRolesAsync(string? languageShortName = null) =>
        _dbContext.CompanyRoles
            .AsNoTracking()
            .Where(companyRole => companyRole.CompanyRoleRegistrationData!.IsRegistrationRole)
            .Select(companyRole => new CompanyRolesDetails(
                companyRole.Label,
                companyRole.CompanyRoleDescriptions.SingleOrDefault(desc =>
                    desc.LanguageShortName == (languageShortName ?? Constants.DefaultLanguage))!.Description
            )).AsAsyncEnumerable();
}
