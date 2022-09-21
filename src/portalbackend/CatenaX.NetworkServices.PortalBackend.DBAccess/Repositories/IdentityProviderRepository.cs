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

using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using Microsoft.EntityFrameworkCore;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

/// <inheritdoc/>
public class IdentityProviderRepository : IIdentityProviderRepository
{
    private readonly PortalDbContext _context;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalDbContext">Portal DB context.</param>
    public IdentityProviderRepository(PortalDbContext portalDbContext)
    {
        _context = portalDbContext;
    }

    /// <inheritdoc/>
    public IdentityProvider CreateIdentityProvider(IdentityProviderCategoryId identityProviderCategory) =>
        _context.IdentityProviders
            .Add(new IdentityProvider(
            Guid.NewGuid(),
            identityProviderCategory,
            DateTimeOffset.UtcNow)).Entity;

    public CompanyIdentityProvider CreateCompanyIdentityProvider(Guid companyId, Guid identityProviderId) =>
        _context.CompanyIdentityProviders
            .Add(new CompanyIdentityProvider(
                companyId,
                identityProviderId
            )).Entity;

    /// <inheritdoc/>
    public IamIdentityProvider CreateIamIdentityProvider(IdentityProvider identityProvider, string idpAlias) =>
        _context.IamIdentityProviders.Add(
            new IamIdentityProvider(
                idpAlias,
                identityProvider.Id)).Entity;

    public Task<(string Alias, IdentityProviderCategoryId IamIdentityProviderCategory, bool IsOwnCompany)> GetOwnCompanyIdentityProviderAliasUntrackedAsync(Guid identityProviderId, string iamUserId) =>
        _context.IdentityProviders
            .Where(identityProvider => identityProvider.Id == identityProviderId)
            .Select(identityProvider =>
                new ValueTuple<string,IdentityProviderCategoryId,bool>(
                    identityProvider.IamIdentityProvider!.IamIdpAlias,
                    identityProvider.IdentityProviderCategoryId,
                    identityProvider.Companies.Any(
                        company => company.CompanyUsers.Any(
                            companyUser => companyUser.IamUser!.UserEntityId == iamUserId))))
            .SingleOrDefaultAsync();

    public Task<(bool IsSameCompany, string Alias, IdentityProviderCategoryId IdentityProviderCategory, IEnumerable<string> Aliase)> GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync(Guid identityProviderId, string iamUserId) =>
        _context.IdentityProviders
            .Where(identityProvider => identityProvider.Id == identityProviderId)
            .Select(identityProvider => new {
                IdentityProvider = identityProvider,
                Company = identityProvider.Companies.SingleOrDefault(company => company.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == iamUserId))
            })
            .Select(item =>
                new ValueTuple<bool,string,IdentityProviderCategoryId,IEnumerable<string>>(
                    item.Company != null,
                    item.IdentityProvider.IamIdentityProvider!.IamIdpAlias,
                    item.IdentityProvider.IdentityProviderCategoryId,
                    item.Company!.IdentityProviders.Select(identityProvider => identityProvider.IamIdentityProvider!.IamIdpAlias)
                ))
            .SingleOrDefaultAsync();

    public Task<(Guid CompanyId, int LinkedCompaniesCount, string Alias, IdentityProviderCategoryId IdentityProviderCategory, IEnumerable<string> Aliase)> GetOwnCompanyIdentityProviderDeletionDataUntrackedAsync(Guid identityProviderId, string iamUserId) =>
        _context.IdentityProviders
            .Where(identityProvider => identityProvider.Id == identityProviderId)
            .Select(identityProvider => new {
                IdentityProvider = identityProvider,
                Company = identityProvider.Companies.SingleOrDefault(company => company.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == iamUserId))
            })
            .Select(item =>
                new ValueTuple<Guid,int,string,IdentityProviderCategoryId,IEnumerable<string>>(
                    item.Company!.Id,
                    item.IdentityProvider.Companies.Count,
                    item.IdentityProvider.IamIdentityProvider!.IamIdpAlias,
                    item.IdentityProvider.IdentityProviderCategoryId,
                    item.Company.IdentityProviders.Select(identityProvider => identityProvider.IamIdentityProvider!.IamIdpAlias)
                ))
            .SingleOrDefaultAsync();

    public IAsyncEnumerable<(Guid IdentityProviderId, IdentityProviderCategoryId CategoryId, string Alias)> GetOwnCompanyIdentityProviderCategoryDataUntracked(string iamUserId) =>
        _context.IamUsers
            .AsNoTracking()
            .Where(iamUser => iamUser.UserEntityId == iamUserId)
            .SelectMany(iamUser => iamUser.CompanyUser!.Company!.IdentityProviders)
            .Select(identityProvider => new ValueTuple<Guid,IdentityProviderCategoryId,string>(
                identityProvider.Id,
                identityProvider.IdentityProviderCategoryId,
                identityProvider.IamIdentityProvider!.IamIdpAlias
            ))
            .ToAsyncEnumerable();

    public IAsyncEnumerable<(Guid IdentityProviderId, string Alias)> GetOwnCompanyIdentityProviderAliasDataUntracked(string iamUserId, IEnumerable<Guid> identityProviderIds) =>
        _context.IamUsers
            .AsNoTracking()
            .Where(iamUser => iamUser.UserEntityId == iamUserId)
            .SelectMany(iamUser => iamUser.CompanyUser!.Company!.IdentityProviders)
            .Where(identityProvider => identityProviderIds.Contains(identityProvider.Id))
            .Select(identityProvider => new ValueTuple<Guid,string>(
                identityProvider.Id,
                identityProvider.IamIdentityProvider!.IamIdpAlias
            ))
            .ToAsyncEnumerable();

    public Task<(string? UserEntityId, string? Alias, bool IsSameCompany)> GetIamUserIsOwnCompanyIdentityProviderAliasAsync(Guid companyUserId, Guid identityProviderId, string iamUserId) =>
        _context.CompanyUsers
            .AsNoTracking()
            .Where(companyUser => companyUser.Id == companyUserId)
            .Select(companyUser => new ValueTuple<string?,string?,bool>(
                companyUser.IamUser!.UserEntityId,
                companyUser.Company!.IdentityProviders
                    .Where(identityProvider => identityProvider.Id == identityProviderId)
                    .Select(identityProvider => identityProvider.IamIdentityProvider!.IamIdpAlias)
                    .SingleOrDefault(),
                companyUser.Company!.CompanyUsers.Any(companyUser => companyUser.IamUser!.UserEntityId == iamUserId)))
            .SingleOrDefaultAsync();
}
