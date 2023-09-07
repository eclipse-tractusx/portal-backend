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
using Org.Eclipse.TractusX.Portal.Backend.Framework.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

/// <inheritdoc/>
public class CompanyRepository : ICompanyRepository
{
    private readonly PortalDbContext _context;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalDbContext">Portal DB context.</param>
    public CompanyRepository(PortalDbContext portalDbContext)
    {
        _context = portalDbContext;
    }

    /// <inheritdoc/>
    public Company CreateCompany(string companyName) =>
        _context.Companies.Add(
            new Company(
                Guid.NewGuid(),
                companyName,
                CompanyStatusId.PENDING,
                DateTimeOffset.UtcNow)).Entity;

    public void AttachAndModifyCompany(Guid companyId, Action<Company>? initialize, Action<Company> modify)
    {
        var company = new Company(companyId, null!, default, default);
        initialize?.Invoke(company);
        _context.Attach(company);
        modify(company);
    }

    public Address CreateAddress(string city, string streetname, string countryAlpha2Code, Action<Address>? setOptionalParameters = null)
    {
        var address = _context.Addresses.Add(
            new Address(
                Guid.NewGuid(),
                city,
                streetname,
                countryAlpha2Code,
                DateTimeOffset.UtcNow
            )).Entity;
        setOptionalParameters?.Invoke(address);
        return address;
    }

    public void AttachAndModifyAddress(Guid addressId, Action<Address>? initialize, Action<Address> modify)
    {
        var address = new Address(addressId, null!, null!, null!, default);
        initialize?.Invoke(address);
        _context.Attach(address);
        modify(address);
    }

    public void CreateUpdateDeleteIdentifiers(Guid companyId, IEnumerable<(UniqueIdentifierId UniqueIdentifierId, string Value)> initialItems, IEnumerable<(UniqueIdentifierId UniqueIdentifierId, string Value)> modifiedItems) =>
        _context.AddAttachRemoveRange(
            initialItems,
            modifiedItems,
            initial => initial.UniqueIdentifierId,
            modified => modified.UniqueIdentifierId,
            identifierId => new CompanyIdentifier(companyId, identifierId, null!),
            (initial, modified) => initial.Value == modified.Value,
            (entity, initial) => entity.Value = initial.Value,
            (entity, modified) => entity.Value = modified.Value);

    public Task<(bool IsValidCompany, string CompanyName)> GetCompanyNameUntrackedAsync(Guid companyId) =>
        _context.Companies
            .Where(x => x.Id == companyId)
            .Select(company => new ValueTuple<bool, string>(true, company.Name))
            .SingleOrDefaultAsync();

    public Task<(string? Bpn, IEnumerable<Guid> TechnicalUserRoleIds)> GetBpnAndTechnicalUserRoleIds(Guid companyId, string technicalUserClientId) =>
        _context.Companies
            .AsNoTracking()
            .Where(company => company.Id == companyId)
            .Select(company => new ValueTuple<string?, IEnumerable<Guid>>(
                company!.BusinessPartnerNumber,
                company!.CompanyAssignedRoles.SelectMany(car => car.CompanyRole!.CompanyRoleAssignedRoleCollection!.UserRoleCollection!.UserRoles.Where(ur => ur.Offer!.AppInstances.Any(ai => ai.IamClient!.ClientClientId == technicalUserClientId)).Select(ur => ur.Id)).Distinct()))
            .SingleOrDefaultAsync();

    public IAsyncEnumerable<string?> GetAllMemberCompaniesBPNAsync() =>
        _context.Companies
            .AsNoTracking()
            .Where(company => company.CompanyStatusId == CompanyStatusId.ACTIVE)
            .Select(company => company.BusinessPartnerNumber)
            .AsAsyncEnumerable();

    public Task<CompanyAddressDetailData?> GetCompanyDetailsAsync(Guid companyId) =>
        _context.Companies
            .AsNoTracking()
            .Where(company => company.Id == companyId)
            .Select(company => new CompanyAddressDetailData(
                company!.Id,
                company.Name,
                company.BusinessPartnerNumber,
                company.Shortname,
                company.Address!.City,
                company.Address.Streetname,
                company.Address.CountryAlpha2Code,
                company.Address.Region,
                company.Address.Streetadditional,
                company.Address.Streetnumber,
                company.Address!.Zipcode,
                company.CompanyAssignedRoles.Select(car => car.CompanyRoleId)
                ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<(bool IsValidCompanyId, bool IsCompanyRoleOwner)> IsValidCompanyRoleOwner(Guid companyId, IEnumerable<CompanyRoleId> companyRoleIds) =>
        _context.Companies.AsNoTracking()
            .Where(company => company.Id == companyId)
            .Select(company => new ValueTuple<bool, bool>(
                true,
                company.CompanyAssignedRoles.Any(companyRole => companyRoleIds.Contains(companyRole.CompanyRoleId))
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<(Guid ProviderCompanyDetailId, string Url)> GetProviderCompanyDetailsExistsForUser(Guid companyId) =>
        _context.ProviderCompanyDetails.AsNoTracking()
            .Where(details => details.CompanyId == companyId)
            .Select(details => new ValueTuple<Guid, string>(details.Id, details.AutoSetupUrl))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    ProviderCompanyDetail ICompanyRepository.CreateProviderCompanyDetail(Guid companyId, string dataUrl, Action<ProviderCompanyDetail>? setOptionalParameter)
    {
        var providerCompanyDetail = new ProviderCompanyDetail(Guid.NewGuid(), companyId, dataUrl, DateTimeOffset.UtcNow);
        setOptionalParameter?.Invoke(providerCompanyDetail);
        return _context.ProviderCompanyDetails.Add(providerCompanyDetail).Entity;
    }

    /// <inheritdoc />
    public Task<(ProviderDetailReturnData ProviderDetailReturnData, bool IsProviderCompany)> GetProviderCompanyDetailAsync(CompanyRoleId companyRoleId, Guid companyId) =>
        _context.Companies
            .Where(company => company.Id == companyId)
            .Select(company => new ValueTuple<ProviderDetailReturnData, bool>(
                new ProviderDetailReturnData(
                    company.ProviderCompanyDetail!.Id,
                    company.Id,
                    company.ProviderCompanyDetail.AutoSetupUrl),
                company.CompanyAssignedRoles.Any(assigned => assigned.CompanyRoleId == companyRoleId)))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public void AttachAndModifyProviderCompanyDetails(Guid providerCompanyDetailId, Action<ProviderCompanyDetail> initialize, Action<ProviderCompanyDetail> modify)
    {
        var details = new ProviderCompanyDetail(providerCompanyDetailId, Guid.Empty, null!, default);
        initialize(details);
        _context.Attach(details);
        modify(details);
    }

    /// <inheritdoc />
    public Task<(string? Bpn, Guid? SelfDescriptionDocumentId)> GetCompanyBpnAndSelfDescriptionDocumentByIdAsync(Guid companyId) =>
        _context.Companies.AsNoTracking()
            .Where(x => x.Id == companyId)
            .Select(x => new ValueTuple<string?, Guid?>(x.BusinessPartnerNumber, x.SelfDescriptionDocumentId))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public IAsyncEnumerable<CompanyAssignedUseCaseData> GetCompanyAssigendUseCaseDetailsAsync(Guid userCompanyId) =>
        _context.Companies
        .Where(company => company.Id == userCompanyId)
        .SelectMany(company => company.CompanyAssignedUseCase)
        .Select(cauc => new CompanyAssignedUseCaseData(
            cauc.UseCaseId,
            cauc.UseCase!.Name))
        .AsAsyncEnumerable();

    /// <inheritdoc />
    public Task<(bool IsUseCaseIdExists, bool IsActiveCompanyStatus, bool IsValidCompany)> GetCompanyStatusAndUseCaseIdAsync(Guid companyId, Guid useCaseId) =>
        _context.Companies
        .Where(company => company.Id == companyId)
        .Select(company => new ValueTuple<bool, bool, bool>(
            company.CompanyAssignedUseCase.Any(cauc => cauc.UseCaseId == useCaseId),
            company.CompanyStatusId == CompanyStatusId.ACTIVE,
            true))
        .SingleOrDefaultAsync();

    /// <inheritdoc />
    public CompanyAssignedUseCase CreateCompanyAssignedUseCase(Guid companyId, Guid useCaseId) =>
        _context.CompanyAssignedUseCases.Add(new CompanyAssignedUseCase(companyId, useCaseId)).Entity;

    /// <inheritdoc /> 
    public void RemoveCompanyAssignedUseCase(Guid companyId, Guid useCaseId) =>
        _context.CompanyAssignedUseCases.Remove(new CompanyAssignedUseCase(companyId, useCaseId));

    /// <inheritdoc />
    public IAsyncEnumerable<CompanyRoleConsentData> GetCompanyRoleAndConsentAgreementDataAsync(Guid companyId, string languageShortName) =>
        _context.CompanyRoles
            .AsSplitQuery()
            .Where(companyRole => companyRole.CompanyRoleRegistrationData!.IsRegistrationRole)
            .Select(companyRole => new CompanyRoleConsentData(
                companyRole.Id,
                companyRole.CompanyRoleDescriptions.SingleOrDefault(lc => lc.LanguageShortName == languageShortName)!.Description,
                companyRole.CompanyAssignedRoles.Any(assigned => assigned.CompanyId == companyId),
                companyRole.AgreementAssignedCompanyRoles
                    .Select(assigned => new ConsentAgreementData(
                        assigned.AgreementId,
                        assigned.Agreement!.Name,
                        assigned.Agreement!.DocumentId,
                        assigned.Agreement.Consents.Where(consent => consent.CompanyId == companyId).OrderByDescending(consent => consent.DateCreated).Select(consent => consent.ConsentStatusId).FirstOrDefault(),
                        assigned.Agreement.AgreementLink
                    ))))
            .AsAsyncEnumerable();

    /// <inheritdoc />
    public Task<(bool IsValidCompany, bool IsCompanyActive, IEnumerable<CompanyRoleId>? CompanyRoleIds, IEnumerable<ConsentStatusDetails>? ConsentStatusDetails)> GetCompanyRolesDataAsync(Guid companyId, IEnumerable<CompanyRoleId> companyRoleIds) =>
        _context.Companies
            .AsNoTracking()
            .AsSplitQuery()
            .Where(company => company.Id == companyId)
            .Select(company => new
            {
                Company = company,
                IsActive = company!.CompanyStatusId == CompanyStatusId.ACTIVE
            })
            .Select(x => new ValueTuple<bool, bool, IEnumerable<CompanyRoleId>?, IEnumerable<ConsentStatusDetails>?>(
                true,
                x.IsActive,
                x.IsActive
                    ? x.Company.CompanyAssignedRoles.Where(assigned => companyRoleIds.Contains(assigned.CompanyRoleId)).Select(assigned => assigned.CompanyRoleId)
                    : null,
                x.IsActive
                    ? x.Company.Consents
                        .Where(consent => consent.Agreement!.AgreementAssignedCompanyRoles.Any(role => companyRoleIds.Contains(role.CompanyRoleId)))
                        .Select(consent => new ConsentStatusDetails(
                            consent.Id,
                            consent.AgreementId,
                            consent.ConsentStatusId))
                    : null))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public IAsyncEnumerable<(Guid AgreementId, CompanyRoleId CompanyRoleId)> GetAgreementAssignedRolesDataAsync(IEnumerable<CompanyRoleId> companyRoleIds) =>
        _context.AgreementAssignedCompanyRoles
            .Where(assigned => companyRoleIds.Contains(assigned.CompanyRoleId))
            .OrderBy(assigned => assigned.CompanyRoleId)
            .Select(assigned => new ValueTuple<Guid, CompanyRoleId>(
                assigned.AgreementId,
                assigned.CompanyRoleId))
            .AsAsyncEnumerable();

    /// <inheritdoc />
    public Task<(bool IsActive, bool IsValid)> GetCompanyStatusDataAsync(Guid companyId) =>
        _context.Companies
        .Where(company => company.Id == companyId)
        .Select(company => new ValueTuple<bool, bool>(
            company.CompanyStatusId == CompanyStatusId.ACTIVE,
            true
        )).SingleOrDefaultAsync();

    public Task<CompanyInformationData?> GetOwnCompanyInformationAsync(Guid companyId) =>
        _context.Companies
            .AsNoTracking()
            .Where(c => c.Id == companyId)
            .Select(user => new CompanyInformationData(
                user.Id,
                user.Name,
                user.Address!.CountryAlpha2Code,
                user.BusinessPartnerNumber
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public IAsyncEnumerable<CompanyRoleId> GetOwnCompanyRolesAsync(Guid companyId) =>
        _context.CompanyAssignedRoles
            .Where(x => x.CompanyId == companyId)
            .Select(x => x.CompanyRoleId)
            .AsAsyncEnumerable();

    /// <inheritdoc />
    public IAsyncEnumerable<OperatorBpnData> GetOperatorBpns() =>
        _context.Companies
            .Where(x =>
                x.CompanyAssignedRoles.Any(car => car.CompanyRoleId == CompanyRoleId.OPERATOR) &&
                !string.IsNullOrWhiteSpace(x.BusinessPartnerNumber))
            .Select(x => new OperatorBpnData(
                x.Name,
                x.BusinessPartnerNumber!))
            .AsAsyncEnumerable();

    public Task<(bool IsValidCompany, string CompanyName, bool IsAllowed)> CheckCompanyAndCompanyRolesAsync(Guid companyId, IEnumerable<CompanyRoleId> companyRoles) =>
        _context.Companies
            .Where(x => x.Id == companyId)
            .Select(x => new ValueTuple<bool, string, bool>(
                    true,
                    x.Name,
                    !companyRoles.Any() || x.CompanyAssignedRoles.Any(role => companyRoles.Contains(role.CompanyRoleId))
                ))
            .SingleOrDefaultAsync();
}
