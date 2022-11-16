/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
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

using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Microsoft.EntityFrameworkCore;

namespace Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Repositories;

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

    public Company AttachAndModifyCompany(Guid companyId, Action<Company>? setOptionalParameters = null)
    {
        var company = _context.Attach(new Company(companyId, null!, default, default)).Entity;
        setOptionalParameters?.Invoke(company);
        return company;
    }

    public Address CreateAddress(string city, string streetname, string countryAlpha2Code) =>
        _context.Addresses.Add(
            new Address(
                Guid.NewGuid(),
                city,
                streetname,
                countryAlpha2Code,
                DateTimeOffset.UtcNow
            )).Entity;

    public Task<(string CompanyName, Guid CompanyId)> GetCompanyNameIdUntrackedAsync(string iamUserId) =>
        _context.IamUsers
            .AsNoTracking()
            .Where(iamUser => iamUser.UserEntityId == iamUserId)
            .Select(iamUser => iamUser!.CompanyUser!.Company)
            .Select(company => new ValueTuple<string,Guid>(company!.Name, company.Id))
            .SingleOrDefaultAsync();

    public Task<(Guid CompanyId, string CompanyName, string? Alias, Guid CompanyUserId)> GetCompanyNameIdWithSharedIdpAliasUntrackedAsync(Guid applicationId, string iamUserId) =>
        _context.Companies.AsNoTracking()
            .Where(company => company.CompanyApplications.Any(application => application.Id == applicationId))
            .Select(company => new ValueTuple<Guid,string,string?,Guid>(
                company.Id,
                company.Name,
                company.IdentityProviders
                    .Where(identityProvider => identityProvider.IdentityProviderCategoryId == IdentityProviderCategoryId.KEYCLOAK_SHARED)
                    .Select(identityProvider => identityProvider.IamIdentityProvider!.IamIdpAlias)
                    .SingleOrDefault(),
                company.CompanyUsers.SingleOrDefault(companyUser => companyUser.IamUser!.UserEntityId == iamUserId)!.Id
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<Guid> GetCompanyIdByBpnAsync(string businessPartnerNumber) =>
        _context.Companies
        .AsNoTracking()
        .Where(company => company.BusinessPartnerNumber == businessPartnerNumber)
        .Select(company => company.Id)
        .SingleOrDefaultAsync();

    public IAsyncEnumerable<string?> GetAllMemberCompaniesBPNAsync() =>
        _context.Companies
            .AsNoTracking()
            .Where(company => company.CompanyStatusId == CompanyStatusId.ACTIVE)
            .Select(company => company.BusinessPartnerNumber)
            .AsAsyncEnumerable();

    public Task<CompanyWithAddress?> GetOwnCompanyDetailsAsync(string iamUserId) =>
        _context.IamUsers
            .AsNoTracking()
            .Where(iamUser => iamUser.UserEntityId == iamUserId)
            .Select(iamUser => iamUser.CompanyUser!.Company)
            .Select(company => new CompanyWithAddress(
                company!.Id,
                company.Name,
                company.Address!.City,
                company.Address!.Streetname,
                company.Address!.CountryAlpha2Code)
            {
                BusinessPartnerNumber = company.BusinessPartnerNumber,
                Region = company.Address!.Region,
                Streetnumber = company.Address!.Streetnumber,
                Zipcode = company.Address!.Zipcode
            })
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<(Guid CompanyId, bool IsServiceProviderCompany)> GetCompanyIdMatchingRoleAndIamUser(string iamUserId, CompanyRoleId companyRoleId) =>
        _context.Companies.AsNoTracking()
            .Where(company => company.CompanyUsers.Any(user => user.IamUser!.UserEntityId == iamUserId))
            .Select(company => new ValueTuple<Guid, bool>(
                company.Id,
                company.CompanyRoles.Any(companyRole => companyRole.Id == companyRoleId)
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<(bool IsValidServicProviderDetailsId, bool IsSameCompany)> CheckServiceProviderDetailsExistsForUser(string iamUserId, Guid serviceProviderCompanyDetailsId) =>
        _context.ProviderCompanyDetails.AsNoTracking()
            .Where(details => details.Id == serviceProviderCompanyDetailsId)
            .Select(details => new ValueTuple<bool,bool>(
                true,
                details.Company!.CompanyUsers.Any(user => user.IamUser!.UserEntityId == iamUserId)))
            .SingleOrDefaultAsync();
    
    /// <inheritdoc />
    public ProviderCompanyDetail CreateServiceProviderCompanyDetail(Guid companyId, string dataUrl) =>
        _context.ProviderCompanyDetails.Add(new ProviderCompanyDetail(Guid.NewGuid(), companyId, dataUrl, DateTimeOffset.UtcNow)).Entity;

    /// <inheritdoc />
    public Task<(ServiceProviderDetailReturnData ServiceProviderDetailReturnData, bool IsServiceProviderCompany, bool IsCompanyUser)> GetServiceProviderCompanyDetailAsync(Guid serviceProviderDetailDataId, CompanyRoleId companyRoleId, string iamUserId) =>
        _context.ProviderCompanyDetails
            .Where(x => 
                x.Id == serviceProviderDetailDataId)
            .Select(x => new ValueTuple<ServiceProviderDetailReturnData,bool,bool>(
                new ServiceProviderDetailReturnData(x.Id, x.CompanyId, x.AutoSetupUrl),
                x.Company!.CompanyRoles.Any(companyRole => companyRole.Id == companyRoleId),
                x.Company.CompanyUsers.Any(user => user.IamUser!.UserEntityId == iamUserId)))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public ProviderCompanyDetail AttachAndModifyServiceProviderDetails(Guid serviceProviderCompanyDetailId, Action<ProviderCompanyDetail>? setOptionalParameters = null)
    {
        var serviceProviderCompanyDetail = _context.Attach(new ProviderCompanyDetail(serviceProviderCompanyDetailId, Guid.Empty, null!, default)).Entity;
        setOptionalParameters?.Invoke(serviceProviderCompanyDetail);
        return serviceProviderCompanyDetail;
    }
    
    /// <inheritdoc />
    public Task<string?> GetCompanyBpnByIdAsync(Guid companyId) =>
        _context.Companies.AsNoTracking()
            .Where(x => x.Id == companyId)
            .Select(x => x.BusinessPartnerNumber)
            .SingleOrDefaultAsync();
}
