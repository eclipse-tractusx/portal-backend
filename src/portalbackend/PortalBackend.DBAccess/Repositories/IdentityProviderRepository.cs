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
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

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

    public Task<(string? SharedIdpAlias, Guid CompanyUserId)> GetSharedIdentityProviderIamAliasDataUntrackedAsync(Guid companyUserId) =>
        _context.CompanyUsers.AsNoTracking()
            .Where(companyUser => companyUser.Id == companyUserId)
            .Select(companyUser => new ValueTuple<string?, Guid>(
                companyUser.Identity!.Company!.IdentityProviders.SingleOrDefault(identityProvider => identityProvider.IdentityProviderCategoryId == IdentityProviderCategoryId.KEYCLOAK_SHARED)!.IamIdentityProvider!.IamIdpAlias,
                companyUser.Id
            ))
            .SingleOrDefaultAsync();

    public Task<IdpUser?> GetIdpCategoryIdByUserIdAsync(Guid companyUserId, Guid userCompanyId) =>
        _context.CompanyUsers.AsNoTracking()
            .Where(companyUser => companyUser.Id == companyUserId
                && companyUser.Identity!.CompanyId == userCompanyId)
            .Select(companyUser => new IdpUser
            {
                TargetIamUserId = companyUser.Identity!.UserEntityId,
                IdpName = companyUser.Identity!.Company!.IdentityProviders
                    .Where(identityProvider => identityProvider.IdentityProviderCategoryId == IdentityProviderCategoryId.KEYCLOAK_SHARED)
                    .Select(identityProvider => identityProvider.IamIdentityProvider!.IamIdpAlias)
                    .SingleOrDefault()
            }).SingleOrDefaultAsync();

    public Task<(string Alias, IdentityProviderCategoryId IamIdentityProviderCategory, bool IsOwnCompany)> GetOwnCompanyIdentityProviderAliasUntrackedAsync(Guid identityProviderId, Guid userCompanyId) =>
        _context.IdentityProviders
            .Where(identityProvider => identityProvider.Id == identityProviderId)
            .Select(identityProvider =>
                new ValueTuple<string, IdentityProviderCategoryId, bool>(
                    identityProvider.IamIdentityProvider!.IamIdpAlias,
                    identityProvider.IdentityProviderCategoryId,
                    identityProvider.Companies.Any(company => company.Id == userCompanyId)))
            .SingleOrDefaultAsync();

    public Task<(bool IsSameCompany, string Alias, IdentityProviderCategoryId IdentityProviderCategory, IEnumerable<string> Aliase)> GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync(Guid identityProviderId, Guid userCompanyId) =>
        _context.IdentityProviders
            .Where(identityProvider => identityProvider.Id == identityProviderId)
            .Select(identityProvider => new
            {
                IdentityProvider = identityProvider,
                Company = identityProvider.Companies.SingleOrDefault(company => company.Id == userCompanyId)
            })
            .Select(item =>
                new ValueTuple<bool, string, IdentityProviderCategoryId, IEnumerable<string>>(
                    item.Company != null,
                    item.IdentityProvider.IamIdentityProvider!.IamIdpAlias,
                    item.IdentityProvider.IdentityProviderCategoryId,
                    item.Company!.IdentityProviders.Select(identityProvider => identityProvider.IamIdentityProvider!.IamIdpAlias)
                ))
            .SingleOrDefaultAsync();

    public Task<(Guid CompanyId, int LinkedCompaniesCount, string Alias, IdentityProviderCategoryId IdentityProviderCategory, IEnumerable<string> Aliase)> GetOwnCompanyIdentityProviderDeletionDataUntrackedAsync(Guid identityProviderId, string iamUserId) =>
        _context.IdentityProviders
            .Where(identityProvider => identityProvider.Id == identityProviderId)
            .Select(identityProvider => new
            {
                IdentityProvider = identityProvider,
                Company = identityProvider.Companies.SingleOrDefault(company => company.Identities.Any(i => i.UserEntityId == iamUserId && i.IdentityTypeId == IdentityTypeId.COMPANY_USER))
            })
            .Select(item =>
                new ValueTuple<Guid, int, string, IdentityProviderCategoryId, IEnumerable<string>>(
                    item.Company!.Id,
                    item.IdentityProvider.Companies.Count,
                    item.IdentityProvider.IamIdentityProvider!.IamIdpAlias,
                    item.IdentityProvider.IdentityProviderCategoryId,
                    item.Company.IdentityProviders.Select(identityProvider => identityProvider.IamIdentityProvider!.IamIdpAlias)
                ))
            .SingleOrDefaultAsync();

    public IAsyncEnumerable<(Guid IdentityProviderId, IdentityProviderCategoryId CategoryId, string Alias)> GetOwnCompanyIdentityProviderCategoryDataUntracked(Guid companyUserId) =>
        _context.CompanyUsers
            .AsNoTracking()
            .Where(user => user.Id == companyUserId)
            .SelectMany(user => user.Identity!.Company!.IdentityProviders)
            .Select(identityProvider => new ValueTuple<Guid, IdentityProviderCategoryId, string>(
                identityProvider.Id,
                identityProvider.IdentityProviderCategoryId,
                identityProvider.IamIdentityProvider!.IamIdpAlias
            ))
            .ToAsyncEnumerable();

    public IAsyncEnumerable<(Guid IdentityProviderId, IdentityProviderCategoryId CategoryId, string Alias)> GetCompanyIdentityProviderCategoryDataUntracked(Guid companyId) =>
        _context.IdentityProviders
            .AsNoTracking()
            .Where(identityProvider => identityProvider.Companies.Any(company => company.Id == companyId))
            .Select(identityProvider => new ValueTuple<Guid, IdentityProviderCategoryId, string>(
                identityProvider.Id,
                identityProvider.IdentityProviderCategoryId,
                identityProvider.IamIdentityProvider!.IamIdpAlias
            ))
            .ToAsyncEnumerable();

    public IAsyncEnumerable<(Guid IdentityProviderId, string Alias)> GetOwnCompanyIdentityProviderAliasDataUntracked(Guid companyUserId, IEnumerable<Guid> identityProviderIds) =>
        _context.CompanyUsers
            .AsNoTracking()
            .Where(user => user.Id == companyUserId)
            .SelectMany(user => user.Identity!.Company!.IdentityProviders)
            .Where(identityProvider => identityProviderIds.Contains(identityProvider.Id))
            .Select(identityProvider => new ValueTuple<Guid, string>(
                identityProvider.Id,
                identityProvider.IamIdentityProvider!.IamIdpAlias
            ))
            .ToAsyncEnumerable();

    public Task<(string? UserEntityId, string? Alias, bool IsSameCompany)> GetIamUserIsOwnCompanyIdentityProviderAliasAsync(Guid companyUserId, Guid identityProviderId, Guid userCompanyId) =>
        _context.CompanyUsers
            .AsNoTracking()
            .Where(companyUser => companyUser.Id == companyUserId)
            .Select(companyUser => new ValueTuple<string?, string?, bool>(
                companyUser.Identity!.UserEntityId,
                companyUser.Identity!.Company!.IdentityProviders
                    .Where(identityProvider => identityProvider.Id == identityProviderId)
                    .Select(identityProvider => identityProvider.IamIdentityProvider!.IamIdpAlias)
                    .SingleOrDefault(),
                companyUser.Identity!.CompanyId == userCompanyId))
            .SingleOrDefaultAsync();

    public Task<((Guid CompanyId, string? CompanyName, string? BusinessPartnerNumber) Company,
                (Guid CompanyUserId, string? FirstName, string? LastName, string? Email) CompanyUser,
                IEnumerable<string> IdpAliase)>
        GetCompanyNameIdpAliaseUntrackedAsync(string iamUserId, Guid? applicationId, IdentityProviderCategoryId identityProviderCategoryId) =>
            _context.CompanyUsers
                .AsNoTracking()
                .Where(companyUser => companyUser.Identity!.UserEntityId == iamUserId &&
                    (applicationId == null || companyUser.Identity!.Company!.CompanyApplications.Any(application => application.Id == applicationId)))
                .Select(companyUser => new ValueTuple<(Guid, string?, string?), (Guid, string?, string?, string?), IEnumerable<string>>(
                    new ValueTuple<Guid, string?, string?>(
                        companyUser.Identity!.Company!.Id,
                        companyUser.Identity!.Company.Name,
                        companyUser.Identity!.Company!.BusinessPartnerNumber),
                    new ValueTuple<Guid, string?, string?, string?>(
                        companyUser.Id,
                        companyUser.Firstname,
                        companyUser.Lastname,
                        companyUser.Email),
                    companyUser.Identity!.Company!.IdentityProviders
                        .Where(identityProvider => identityProvider.IdentityProviderCategoryId == identityProviderCategoryId)
                        .Select(identityProvider => identityProvider.IamIdentityProvider!.IamIdpAlias)))
                .SingleOrDefaultAsync();

    public Task<((Guid CompanyId, string? CompanyName, string? BusinessPartnerNumber) Company,
        (Guid CompanyUserId, string? FirstName, string? LastName, string? Email) CompanyUser,
        (string? IdpAlias, bool IsSharedIdp) IdentityProvider)>
        GetCompanyNameIdpAliasUntrackedAsync(Guid identityProviderId, string iamUserId) =>
            _context.CompanyUsers
                .AsNoTracking()
                .Where(companyUser => companyUser.Identity!.UserEntityId == iamUserId)
                .Select(companyUser => new
                {
                    Company = companyUser!.Identity!.Company,
                    CompanyUser = companyUser,
                    IdentityProvider = companyUser.Identity!.Company!.IdentityProviders.SingleOrDefault(identityProvider => identityProvider.Id == identityProviderId)
                })
                .Select(s => new ValueTuple<(Guid, string?, string?), (Guid, string?, string?, string?), (string?, bool)>(
                    new ValueTuple<Guid, string?, string?>(
                        s.Company!.Id,
                        s.Company.Name,
                        s.Company!.BusinessPartnerNumber),
                    new ValueTuple<Guid, string?, string?, string?>(
                        s.CompanyUser!.Id,
                        s.CompanyUser.Firstname,
                        s.CompanyUser.Lastname,
                        s.CompanyUser.Email),
                    new ValueTuple<string?, bool>(
                        s.IdentityProvider!.IamIdentityProvider!.IamIdpAlias,
                        s.IdentityProvider.IdentityProviderCategoryId == IdentityProviderCategoryId.KEYCLOAK_SHARED)))
                .SingleOrDefaultAsync();
}
