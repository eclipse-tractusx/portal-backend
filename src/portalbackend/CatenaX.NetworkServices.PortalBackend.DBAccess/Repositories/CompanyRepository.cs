﻿/********************************************************************************
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

using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using Microsoft.EntityFrameworkCore;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

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

    public Address CreateAddress(string city, string streetname, string countryAlpha2Code) =>
        _context.Addresses.Add(
            new Address(
                Guid.NewGuid(),
                city,
                streetname,
                countryAlpha2Code,
                DateTimeOffset.UtcNow
            )).Entity;

    public Task<CompanyNameIdIdpAlias?> GetCompanyNameIdWithSharedIdpAliasUntrackedAsync(Guid applicationId, string iamUserId) =>
        _context.IamUsers
            .AsNoTracking()
            .Where(iamUser =>
                iamUser.UserEntityId == iamUserId
                && iamUser.CompanyUser!.Company!.CompanyApplications.Any(application => application.Id == applicationId))
            .Select(iamUser => iamUser.CompanyUser!.Company)
            .Select(company => new CompanyNameIdIdpAlias(
                    company!.Name,
                    company.Id)
            {
                IdpAlias = company.IdentityProviders
                        .Where(identityProvider => identityProvider.IdentityProviderCategoryId == IdentityProviderCategoryId.KEYCLOAK_SHARED)
                        .Select(identityProvider => identityProvider.IamIdentityProvider!.IamIdpAlias)
                        .SingleOrDefault()
            })
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public IAsyncEnumerable<(Guid CompanyId, string? BusinessPartnerNumber)> GetConnectorCreationCompanyDataAsync(IEnumerable<(Guid companyId, bool bpnRequested)> parameters) =>
        _context.Companies
            .AsNoTracking()
            .Where(company => parameters.Select(parameter => parameter.companyId).Contains(company.Id))
            .Select(company => ((Guid CompanyId, string? BusinessPartnerNumber)) new (
                company.Id,
                parameters.Where(parameter => parameter.bpnRequested).Select(parameter => parameter.companyId).Contains(company.Id) ? company.BusinessPartnerNumber : null
            )).AsAsyncEnumerable();
}
