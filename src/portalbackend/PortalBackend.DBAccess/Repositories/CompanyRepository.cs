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

    public Task<(bool Exists, string CompanyName)> GetCompanyNameUntrackedAsync(Guid companyId) =>
        _context.Companies
            .Where(x => x.Id == companyId)
            .Select(company => new ValueTuple<bool, string>(true, company.Name))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<(Guid CompanyId, Guid? SelfDescriptionDocumentId)> GetCompanyIdAndSelfDescriptionDocumentByBpnAsync(string businessPartnerNumber) =>
        _context.Companies
        .AsNoTracking()
        .Where(company => company.BusinessPartnerNumber == businessPartnerNumber)
        .Select(company => new ValueTuple<Guid, Guid?>(company.Id, company.SelfDescriptionDocumentId))
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
                company.Address!.Zipcode))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<(Guid CompanyId, bool IsServiceProviderCompany)> GetCompanyIdMatchingRoleAndIamUserOrTechnicalUserAsync(Guid userCompanyId, IEnumerable<CompanyRoleId> companyRoleIds) =>
        _context.Companies.AsNoTracking()
            .Where(company => company.Id == userCompanyId)
            .Select(company => new ValueTuple<Guid, bool>(
                company.Id,
                company.CompanyAssignedRoles.Any(companyRole => companyRoleIds.Contains(companyRole.CompanyRoleId))
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<(Guid ProviderCompanyDetailId, string Url)> GetProviderCompanyDetailsExistsForUser(Guid userCompanyId) =>
        _context.ProviderCompanyDetails.AsNoTracking()
            .Where(details => details.CompanyId == userCompanyId)
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
    public Task<(ProviderDetailReturnData ProviderDetailReturnData, bool IsProviderCompany)> GetProviderCompanyDetailAsync(CompanyRoleId companyRoleId, Guid userCompanyId) =>
        _context.Companies
            .Where(company => company.Id == userCompanyId)
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
    public Task<(bool IsUseCaseIdExists, bool IsActiveCompanyStatus, Guid CompanyId)> GetCompanyStatusAndUseCaseIdAsync(Guid userCompanyId, Guid useCaseId) =>
        _context.Companies
        .Where(company => company.Id == userCompanyId)
        .Select(company => new ValueTuple<bool, bool, Guid>(
            company.CompanyAssignedUseCase.Any(cauc => cauc.UseCaseId == useCaseId),
            company.CompanyStatusId == CompanyStatusId.ACTIVE,
            company.Id))
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
                        assigned.Agreement.Consents.Where(consent => consent.CompanyId == companyId).OrderByDescending(consent => consent.DateCreated).Select(consent => consent.ConsentStatusId).FirstOrDefault()
                    ))))
            .AsAsyncEnumerable();

    /// <inheritdoc />
    public Task<(bool IsCompanyActive, Guid CompanyId, IEnumerable<CompanyRoleId>? CompanyRoleIds, Guid CompanyUserId, IEnumerable<ConsentStatusDetails>? ConsentStatusDetails)> GetCompanyRolesDataAsync(Guid companyUserId, IEnumerable<CompanyRoleId> companyRoleIds) =>
        _context.CompanyUsers
            .AsNoTracking()
            .AsSplitQuery()
            .Where(user => user.Id == companyUserId)
            .Select(user => new
            {
                User = user,
                Company = user.Identity!.Company,
                IsActive = user.Identity!.Company!.CompanyStatusId == CompanyStatusId.ACTIVE
            })
            .Select(x => new ValueTuple<bool, Guid, IEnumerable<CompanyRoleId>?, Guid, IEnumerable<ConsentStatusDetails>?>(
                x.IsActive,
                x.Company!.Id,
                x.IsActive
                    ? x.Company.CompanyAssignedRoles.Where(assigned => companyRoleIds.Contains(assigned.CompanyRoleId)).Select(assigned => assigned.CompanyRoleId)
                    : null,
                x.User.Id,
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
    public Task<(bool IsActive, Guid CompanyId)> GetCompanyStatusDataAsync(Guid userCompanyId) =>
        _context.Companies
        .Where(company => company.Id == userCompanyId)
        .Select(company => new ValueTuple<bool, Guid>(
            company.CompanyStatusId == CompanyStatusId.ACTIVE,
            company.Id
        )).SingleOrDefaultAsync();
}
