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

using Microsoft.EntityFrameworkCore;
using Org.Eclipse.TractusX.Portal.Backend.Framework.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Identity;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

/// <inheritdoc/>
public class CompanyRepository(PortalDbContext context) : ICompanyRepository
{
    /// <inheritdoc/>
    Company ICompanyRepository.CreateCompany(string companyName, Action<Company>? setOptionalParameters)
    {
        var company = new Company(
            Guid.NewGuid(),
            companyName,
            CompanyStatusId.PENDING,
            DateTimeOffset.UtcNow);
        setOptionalParameters?.Invoke(company);
        return context.Companies.Add(company).Entity;
    }

    public void AttachAndModifyCompany(Guid companyId, Action<Company>? initialize, Action<Company> modify)
    {
        var company = new Company(companyId, null!, default, default);
        initialize?.Invoke(company);
        context.Attach(company);
        modify(company);
    }

    Address ICompanyRepository.CreateAddress(string city, string streetname, string countryAlpha2Code, Action<Address>? setOptionalParameters)
    {
        var address = new Address(
            Guid.NewGuid(),
            city,
            streetname,
            countryAlpha2Code,
            DateTimeOffset.UtcNow
        );
        setOptionalParameters?.Invoke(address);
        return context.Addresses.Add(address).Entity;
    }

    public void AttachAndModifyAddress(Guid addressId, Action<Address>? initialize, Action<Address> modify)
    {
        var address = new Address(addressId, null!, null!, null!, default);
        initialize?.Invoke(address);
        context.Attach(address);
        modify(address);
    }

    public void CreateUpdateDeleteIdentifiers(Guid companyId, IEnumerable<(UniqueIdentifierId UniqueIdentifierId, string Value)> initialItems, IEnumerable<(UniqueIdentifierId UniqueIdentifierId, string Value)> modifiedItems) =>
        context.AddAttachRemoveRange(
            initialItems,
            modifiedItems,
            initial => initial.UniqueIdentifierId,
            modified => modified.UniqueIdentifierId,
            identifierId => new CompanyIdentifier(companyId, identifierId, null!),
            (initial, modified) => initial.Value == modified.Value,
            (entity, initial) => entity.Value = initial.Value,
            (entity, modified) => entity.Value = modified.Value);

    public Task<(string? Bpn, IEnumerable<Guid> TechnicalUserRoleIds)> GetBpnAndTechnicalUserRoleIds(Guid companyId, string technicalUserClientId) =>
        context.Companies
            .AsNoTracking()
            .Where(company => company.Id == companyId)
            .Select(company => new ValueTuple<string?, IEnumerable<Guid>>(
                company!.BusinessPartnerNumber,
                company!.CompanyAssignedRoles.SelectMany(car => car.CompanyRole!.CompanyRoleAssignedRoleCollection!.UserRoleCollection!.UserRoles.Where(ur => ur.Offer!.AppInstances.Any(ai => ai.IamClient!.ClientClientId == technicalUserClientId)).Select(ur => ur.Id)).Distinct()))
            .SingleOrDefaultAsync();

    public IAsyncEnumerable<string> GetAllMemberCompaniesBPNAsync(IEnumerable<string>? bpnIds) =>
        context.Companies
            .AsNoTracking()
            .Where(company => company.CompanyStatusId == CompanyStatusId.ACTIVE &&
                (bpnIds == null || bpnIds.Contains(company.BusinessPartnerNumber) &&
                company.BusinessPartnerNumber != null))
            .Select(company => company.BusinessPartnerNumber!)
            .AsAsyncEnumerable();

    public Task<CompanyAddressDetailData?> GetCompanyDetailsAsync(Guid companyId) =>
        context.Companies
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
        context.Companies.AsNoTracking()
            .Where(company => company.Id == companyId)
            .Select(company => new ValueTuple<bool, bool>(
                true,
                company.CompanyAssignedRoles.Any(companyRole => companyRoleIds.Contains(companyRole.CompanyRoleId))
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<(Guid ProviderCompanyDetailId, string Url)> GetProviderCompanyDetailsExistsForUser(Guid companyId) =>
        context.ProviderCompanyDetails.AsNoTracking()
            .Where(details => details.CompanyId == companyId)
            .Select(details => new ValueTuple<Guid, string>(details.Id, details.AutoSetupUrl))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    ProviderCompanyDetail ICompanyRepository.CreateProviderCompanyDetail(Guid companyId, string dataUrl, Action<ProviderCompanyDetail>? setOptionalParameter)
    {
        var providerCompanyDetail = new ProviderCompanyDetail(Guid.NewGuid(), companyId, dataUrl, DateTimeOffset.UtcNow);
        setOptionalParameter?.Invoke(providerCompanyDetail);
        return context.ProviderCompanyDetails.Add(providerCompanyDetail).Entity;
    }

    /// <inheritdoc />
    public Task<(ProviderDetailReturnData ProviderDetailReturnData, bool IsProviderCompany)> GetProviderCompanyDetailAsync(CompanyRoleId companyRoleId, Guid companyId) =>
        context.Companies
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
        context.Attach(details);
        modify(details);
    }

    /// <inheritdoc />
    public Task<(string? Bpn, Guid? SelfDescriptionDocumentId)> GetCompanyBpnAndSelfDescriptionDocumentByIdAsync(Guid companyId) =>
        context.Companies.AsNoTracking()
            .Where(x => x.Id == companyId)
            .Select(x => new ValueTuple<string?, Guid?>(x.BusinessPartnerNumber, x.SelfDescriptionDocumentId))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public IAsyncEnumerable<CompanyAssignedUseCaseData> GetCompanyAssigendUseCaseDetailsAsync(Guid userCompanyId) =>
        context.Companies
        .Where(company => company.Id == userCompanyId)
        .SelectMany(company => company.CompanyAssignedUseCase)
        .Select(cauc => new CompanyAssignedUseCaseData(
            cauc.UseCaseId,
            cauc.UseCase!.Name))
        .AsAsyncEnumerable();

    /// <inheritdoc />
    public Task<(bool IsUseCaseIdExists, bool IsActiveCompanyStatus, bool IsValidCompany)> GetCompanyStatusAndUseCaseIdAsync(Guid companyId, Guid useCaseId) =>
        context.Companies
        .Where(company => company.Id == companyId)
        .Select(company => new ValueTuple<bool, bool, bool>(
            company.CompanyAssignedUseCase.Any(cauc => cauc.UseCaseId == useCaseId),
            company.CompanyStatusId == CompanyStatusId.ACTIVE,
            true))
        .SingleOrDefaultAsync();

    /// <inheritdoc />
    public CompanyAssignedUseCase CreateCompanyAssignedUseCase(Guid companyId, Guid useCaseId) =>
        context.CompanyAssignedUseCases.Add(new CompanyAssignedUseCase(companyId, useCaseId)).Entity;

    /// <inheritdoc /> 
    public void RemoveCompanyAssignedUseCase(Guid companyId, Guid useCaseId) =>
        context.CompanyAssignedUseCases.Remove(new CompanyAssignedUseCase(companyId, useCaseId));

    /// <inheritdoc />
    public IAsyncEnumerable<CompanyRoleConsentData> GetCompanyRoleAndConsentAgreementDataAsync(Guid companyId, string languageShortName) =>
        context.CompanyRoles
            .AsSplitQuery()
            .Where(companyRole => companyRole.CompanyRoleRegistrationData!.IsRegistrationRole)
            .Select(companyRole => new CompanyRoleConsentData(
                companyRole.Id,
                companyRole.CompanyRoleDescriptions.SingleOrDefault(lc => lc.LanguageShortName == languageShortName)!.Description,
                companyRole.CompanyAssignedRoles.Any(assigned => assigned.CompanyId == companyId),
                companyRole.AgreementAssignedCompanyRoles
                    .Where(assigned => assigned.Agreement!.AgreementStatusId == AgreementStatusId.ACTIVE)
                    .Select(assigned => new ConsentAgreementData(
                        assigned.AgreementId,
                        assigned.Agreement!.Name,
                        assigned.Agreement!.DocumentId,
                        assigned.Agreement.Consents.Where(consent => consent.CompanyId == companyId).OrderByDescending(consent => consent.DateCreated).Select(consent => consent.ConsentStatusId).FirstOrDefault(),
                        assigned.Agreement.AgreementLink,
                        assigned.Agreement.Mandatory
                    ))))
            .AsAsyncEnumerable();

    /// <inheritdoc />
    public Task<(bool IsValidCompany, bool IsCompanyActive, IEnumerable<CompanyRoleId>? CompanyRoleIds, IEnumerable<ConsentStatusDetails>? ConsentStatusDetails)> GetCompanyRolesDataAsync(Guid companyId, IEnumerable<CompanyRoleId> companyRoleIds) =>
        context.Companies
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
    public IAsyncEnumerable<(AgreementStatusData agreementStatusData, CompanyRoleId CompanyRoleId)> GetAgreementAssignedRolesDataAsync(IEnumerable<CompanyRoleId> companyRoleIds) =>
        context.AgreementAssignedCompanyRoles
            .Where(assigned => companyRoleIds.Contains(assigned.CompanyRoleId))
            .OrderBy(assigned => assigned.CompanyRoleId)
            .Select(assigned => new ValueTuple<AgreementStatusData, CompanyRoleId>(
                new AgreementStatusData(assigned.AgreementId, assigned.Agreement!.AgreementStatusId),
                assigned.CompanyRoleId))
            .AsAsyncEnumerable();

    /// <inheritdoc />
    public Task<(bool IsActive, bool IsValid)> GetCompanyStatusDataAsync(Guid companyId) =>
        context.Companies
        .Where(company => company.Id == companyId)
        .Select(company => new ValueTuple<bool, bool>(
            company.CompanyStatusId == CompanyStatusId.ACTIVE,
            true
        )).SingleOrDefaultAsync();

    public Task<CompanyInformationData?> GetOwnCompanyInformationAsync(Guid companyId, Guid companyUserId) =>
        context.Companies
            .AsNoTracking()
            .Where(c => c.Id == companyId)
            .Select(company => new CompanyInformationData(
                company.Id,
                company.Name,
                company.Address!.CountryAlpha2Code,
                company.BusinessPartnerNumber,
                company.Identities.Where(x => x.Id == companyUserId && x.IdentityTypeId == IdentityTypeId.COMPANY_USER).Select(x => x.CompanyUser!.Email).SingleOrDefault()
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public IAsyncEnumerable<CompanyRoleId> GetOwnCompanyRolesAsync(Guid companyId) =>
        context.CompanyAssignedRoles
            .Where(x => x.CompanyId == companyId)
            .Select(x => x.CompanyRoleId)
            .AsAsyncEnumerable();

    /// <inheritdoc />
    public IAsyncEnumerable<OperatorBpnData> GetOperatorBpns() =>
        context.Companies
            .Where(x =>
                x.CompanyAssignedRoles.Any(car => car.CompanyRoleId == CompanyRoleId.OPERATOR) &&
                !string.IsNullOrWhiteSpace(x.BusinessPartnerNumber))
            .Select(x => new OperatorBpnData(
                x.Name,
                x.BusinessPartnerNumber!))
            .AsAsyncEnumerable();

    public Task<(bool IsValidCompany, string CompanyName, bool IsAllowed)> CheckCompanyAndCompanyRolesAsync(Guid companyId, IEnumerable<CompanyRoleId> companyRoles) =>
        context.Companies
            .Where(x => x.Id == companyId)
            .Select(x => new ValueTuple<bool, string, bool>(
                    true,
                    x.Name,
                    !companyRoles.Any() || x.CompanyAssignedRoles.Any(role => companyRoles.Contains(role.CompanyRoleId))
                ))
            .SingleOrDefaultAsync();

    public Task<OnboardingServiceProviderCallbackResponseData> GetCallbackData(Guid companyId) =>
        context.Companies.Where(c => c.Id == companyId)
            .Select(c => new OnboardingServiceProviderCallbackResponseData(
                    c.OnboardingServiceProviderDetail!.CallbackUrl,
                    c.OnboardingServiceProviderDetail.AuthUrl,
                    c.OnboardingServiceProviderDetail.ClientId
                ))
            .SingleAsync();

    public Task<(bool HasCompanyRole, Guid? OnboardingServiceProviderDetailId, OspDetails? OspDetails)> GetCallbackEditData(Guid companyId, CompanyRoleId companyRoleId) =>
        context.Companies.Where(c => c.Id == companyId)
            .Select(c => new ValueTuple<bool, Guid?, OspDetails?>(
                c.CompanyAssignedRoles.Any(role => role.CompanyRoleId == companyRoleId),
                c.OnboardingServiceProviderDetail!.Id,
                c.OnboardingServiceProviderDetail == null
                    ? null
                    : new OspDetails(
                        c.OnboardingServiceProviderDetail.CallbackUrl,
                        c.OnboardingServiceProviderDetail.AuthUrl,
                        c.OnboardingServiceProviderDetail.ClientId,
                        c.OnboardingServiceProviderDetail.ClientSecret,
                        c.OnboardingServiceProviderDetail.InitializationVector,
                        c.OnboardingServiceProviderDetail.EncryptionMode)
                ))
            .SingleOrDefaultAsync();

    public void AttachAndModifyOnboardingServiceProvider(Guid onboardingServiceProviderDetailId, Action<OnboardingServiceProviderDetail>? initialize, Action<OnboardingServiceProviderDetail> setOptionalFields)
    {
        var ospDetails = new OnboardingServiceProviderDetail(onboardingServiceProviderDetailId, Guid.Empty, null!, null!, null!, null!, null, default);
        initialize?.Invoke(ospDetails);
        context.OnboardingServiceProviderDetails.Attach(ospDetails);
        setOptionalFields.Invoke(ospDetails);
    }

    public OnboardingServiceProviderDetail CreateOnboardingServiceProviderDetails(Guid companyId, string callbackUrl, string authUrl, string clientId, byte[] clientSecret, byte[]? initializationVector, int encryptionMode) =>
        context.OnboardingServiceProviderDetails.Add(new OnboardingServiceProviderDetail(Guid.NewGuid(), companyId, callbackUrl, authUrl, clientId, clientSecret, initializationVector, encryptionMode)).Entity;

    /// <inheritdoc />
    public Task<bool> CheckBpnExists(string bpn) =>
        context.Companies
            .AnyAsync(x => x.BusinessPartnerNumber == bpn);

    public void CreateWalletData(Guid companyId, string did, JsonDocument didDocument, string clientId, byte[] clientSecret, byte[]? initializationVector, int encryptionMode, string authenticationServiceUrl) =>
        context.CompanyWalletDatas.Add(new CompanyWalletData(Guid.NewGuid(), companyId, did, didDocument, clientId, clientSecret, initializationVector, encryptionMode, authenticationServiceUrl));

    public Task<(bool Exists, JsonDocument DidDocument)> GetDidDocumentById(string bpn) =>
        context.CompanyWalletDatas
            .Where(x => x.Company!.BusinessPartnerNumber == bpn)
            .Select(x => new ValueTuple<bool, JsonDocument>(true, x.DidDocument))
            .SingleOrDefaultAsync();

    public IAsyncEnumerable<(Guid CompanyId, IEnumerable<Guid> SubmittedApplicationIds)> GetCompanySubmittedApplicationIdsByBpn(string bpn) =>
        context.Companies
            .Where(x => x.BusinessPartnerNumber == bpn)
            .Select(x => new ValueTuple<Guid, IEnumerable<Guid>>(
                x.Id,
                x.CompanyApplications
                    .Where(ca => ca.ApplicationStatusId == CompanyApplicationStatusId.SUBMITTED)
                    .Select(ca => ca.Id)))
            .ToAsyncEnumerable();

    public Task<(string? Bpn, string? Did, string? WalletUrl)> GetDimServiceUrls(Guid companyId) =>
        context.Companies.Where(x => x.Id == companyId)
            .Select(x => new ValueTuple<string?, string?, string?>(
                x.BusinessPartnerNumber,
                x.CompanyWalletData!.Did,
                x.CompanyWalletData.AuthenticationServiceUrl
            ))
            .SingleOrDefaultAsync();

    public Task<(string? Holder, string? BusinessPartnerNumber, WalletInformation? WalletInformation)> GetWalletData(Guid identityId) =>
        context.Identities
            .Where(ca => ca.Id == identityId)
            .Select(ca => new
            {
                Company = ca.Company!,
                Wallet = ca.Company!.CompanyWalletData
            })
            .Select(c => new ValueTuple<string?, string?, WalletInformation?>(
                c.Company.DidDocumentLocation,
                c.Company.BusinessPartnerNumber,
                c.Wallet == null ?
                    null :
                    new WalletInformation(
                        c.Wallet.ClientId,
                        c.Wallet.ClientSecret,
                        c.Wallet.InitializationVector,
                        c.Wallet.EncryptionMode,
                        c.Wallet.AuthenticationServiceUrl
                    )))
            .SingleOrDefaultAsync();

    public void RemoveProviderCompanyDetails(Guid providerCompanyDetailId) =>
        context.ProviderCompanyDetails
            .Remove(new ProviderCompanyDetail(providerCompanyDetailId, Guid.Empty, null!, default));

    public Func<int, int, Task<Pagination.Source<CompanyMissingSdDocumentData>?>> GetCompaniesWithMissingSdDocument() =>
        (skip, take) => Pagination.CreateSourceQueryAsync(
            skip,
            take,
            context.Companies.AsNoTracking()
                .Where(c =>
                    c.CompanyStatusId == CompanyStatusId.ACTIVE &&
                    c.SelfDescriptionDocumentId == null &&
                    c.CompanyApplications.Any(ca =>
                        ca.ApplicationChecklistEntries.Any(a =>
                            a.ApplicationChecklistEntryTypeId == ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP &&
                            a.ApplicationChecklistEntryStatusId != ApplicationChecklistEntryStatusId.TO_DO &&
                            a.ApplicationChecklistEntryStatusId != ApplicationChecklistEntryStatusId.IN_PROGRESS)))
                .GroupBy(c => c.CompanyStatusId),
            c => c.OrderByDescending(company => company.Name),
            c => new CompanyMissingSdDocumentData(
                c.Id,
                c.Name)
        ).SingleOrDefaultAsync();

    public IAsyncEnumerable<Guid> GetCompanyIdsWithMissingSelfDescription() =>
        context.Companies.Where(c =>
                c.CompanyStatusId == CompanyStatusId.ACTIVE &&
                c.SelfDescriptionDocumentId == null &&
                c.SdCreationProcessId == null &&
                c.CompanyApplications.Any(ca =>
                    ca.ApplicationChecklistEntries.Any(a =>
                        a.ApplicationChecklistEntryTypeId == ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP &&
                        a.ApplicationChecklistEntryStatusId != ApplicationChecklistEntryStatusId.TO_DO &&
                        a.ApplicationChecklistEntryStatusId != ApplicationChecklistEntryStatusId.IN_PROGRESS)))
            .Select(c => c.Id)
            .ToAsyncEnumerable();

    public Task<(Guid Id, IEnumerable<(UniqueIdentifierId Id, string Value)> UniqueIdentifiers, string? BusinessPartnerNumber, string CountryCode)> GetCompanyByProcessId(Guid processId) =>
        context.Companies
            .Where(c => c.SdCreationProcessId == processId)
            .Select(c => new ValueTuple<Guid, IEnumerable<(UniqueIdentifierId Id, string Value)>, string?, string>(
                c.Id,
                c.CompanyIdentifiers.Select(ci => new ValueTuple<UniqueIdentifierId, string>(ci.UniqueIdentifierId, ci.Value)),
                c.BusinessPartnerNumber,
                c.Address!.Country!.Alpha2Code
            ))
            .SingleOrDefaultAsync();

    public Task<bool> IsExistingCompany(Guid companyId) =>
        context.Companies.AnyAsync(c => c.Id == companyId);

    public Task<(bool Exists, Guid CompanyId, IEnumerable<Guid> SubmittedCompanyApplicationId)> GetCompanyIdByBpn(string bpn) =>
        context.Companies
            .Where(x => x.BusinessPartnerNumber == bpn)
            .Select(x => new ValueTuple<bool, Guid, IEnumerable<Guid>>(true, x.Id, x.CompanyApplications.Where(a => a.ApplicationStatusId == CompanyApplicationStatusId.SUBMITTED).Select(a => a.Id)))
            .SingleOrDefaultAsync();
}
