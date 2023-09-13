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
    public IdentityProvider CreateIdentityProvider(IdentityProviderCategoryId identityProviderCategory, IdentityProviderTypeId identityProviderTypeId, Guid owner, Action<IdentityProvider>? setOptionalFields)
    {
        var idp = new IdentityProvider(
            Guid.NewGuid(),
            identityProviderCategory,
            identityProviderTypeId,
            owner,
            DateTimeOffset.UtcNow);
        setOptionalFields?.Invoke(idp);
        return _context.IdentityProviders
            .Add(idp).Entity;
    }

    public CompanyIdentityProvider CreateCompanyIdentityProvider(Guid companyId, Guid identityProviderId) =>
        _context.CompanyIdentityProviders
            .Add(new CompanyIdentityProvider(
                companyId,
                identityProviderId
            )).Entity;

    /// <inheritdoc/>
    public IamIdentityProvider CreateIamIdentityProvider(Guid identityProviderId, string idpAlias) =>
        _context.IamIdentityProviders.Add(
            new IamIdentityProvider(
                idpAlias,
                identityProviderId)).Entity;

    public Task<string?> GetSharedIdentityProviderIamAliasDataUntrackedAsync(Guid companyId) =>
        _context.IdentityProviders
            .AsNoTracking()
            .Where(identityProvider =>
                identityProvider.IdentityProviderTypeId == IdentityProviderTypeId.SHARED &&
                identityProvider.Companies.Any(company => company.Id == companyId))
            .Select(identityProvider => identityProvider.IamIdentityProvider!.IamIdpAlias)
            .SingleOrDefaultAsync();

    public Task<IdpUser?> GetIdpCategoryIdByUserIdAsync(Guid companyUserId, Guid userCompanyId) =>
        _context.CompanyUsers.AsNoTracking()
            .Where(companyUser => companyUser.Id == companyUserId
                && companyUser.Identity!.CompanyId == userCompanyId)
            .Select(companyUser => new IdpUser
            {
                TargetIamUserId = companyUser.Identity!.UserEntityId,
                IdpName = companyUser.Identity!.Company!.IdentityProviders
                    .Where(identityProvider => identityProvider.IdentityProviderTypeId == IdentityProviderTypeId.SHARED)
                    .Select(identityProvider => identityProvider.IamIdentityProvider!.IamIdpAlias)
                    .SingleOrDefault()
            }).SingleOrDefaultAsync();

    public Task<(string? Alias, IdentityProviderCategoryId IamIdentityProviderCategory, bool IsOwnOrOwnerCompany, IdentityProviderTypeId TypeId)> GetOwnCompanyIdentityProviderAliasUntrackedAsync(Guid identityProviderId, Guid companyId) =>
        _context.IdentityProviders
            .Where(identityProvider => identityProvider.Id == identityProviderId)
            .Select(identityProvider =>
                new ValueTuple<string?, IdentityProviderCategoryId, bool, IdentityProviderTypeId>(
                    identityProvider.IamIdentityProvider!.IamIdpAlias,
                    identityProvider.IdentityProviderCategoryId,
                    identityProvider.OwnerId == companyId || identityProvider.Companies.Any(company => company.Id == companyId),
                    identityProvider.IdentityProviderTypeId))
            .SingleOrDefaultAsync();

    public Task<(bool IsOwner, string? Alias, IdentityProviderCategoryId IdentityProviderCategory, IdentityProviderTypeId IdentityProviderTypeId, IEnumerable<(Guid CompanyId, IEnumerable<string> Aliase)>? CompanyIdAliase)> GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync(Guid identityProviderId, Guid companyId, bool queryAliase) =>
        _context.IdentityProviders
            .Where(identityProvider => identityProvider.Id == identityProviderId)
            .Select(identityProvider =>
                new ValueTuple<bool, string?, IdentityProviderCategoryId, IdentityProviderTypeId, IEnumerable<(Guid, IEnumerable<string>)>?>(
                    identityProvider.OwnerId == companyId,
                    identityProvider.IamIdentityProvider!.IamIdpAlias,
                    identityProvider.IdentityProviderCategoryId,
                    identityProvider.IdentityProviderTypeId,
                    queryAliase
                        ? identityProvider.Companies.Select(c => new ValueTuple<Guid, IEnumerable<string>>(c.Id, c.IdentityProviders.Where(i => i.IamIdentityProvider != null).Select(i => i.IamIdentityProvider!.IamIdpAlias)))
                        : null
                ))
            .SingleOrDefaultAsync();

    public IAsyncEnumerable<(Guid IdentityProviderId, IdentityProviderCategoryId CategoryId, string? Alias, IdentityProviderTypeId TypeId)> GetCompanyIdentityProviderCategoryDataUntracked(Guid companyId) =>
        _context.IdentityProviders
            .AsNoTracking()
            .Where(identityProvider => identityProvider.OwnerId == companyId || identityProvider.Companies.Any(company => company.Id == companyId))
            .Select(identityProvider => new ValueTuple<Guid, IdentityProviderCategoryId, string?, IdentityProviderTypeId>(
                identityProvider.Id,
                identityProvider.IdentityProviderCategoryId,
                identityProvider.IamIdentityProvider!.IamIdpAlias,
                identityProvider.IdentityProviderTypeId
            ))
            .ToAsyncEnumerable();

    public IAsyncEnumerable<(Guid IdentityProviderId, string Alias)> GetOwnCompanyIdentityProviderAliasDataUntracked(Guid companyId, IEnumerable<Guid> identityProviderIds) =>
        _context.IdentityProviders
            .AsNoTracking()
            .Where(identityProvider =>
                identityProvider.Companies.Any(company => company.Id == companyId) &&
                identityProviderIds.Contains(identityProvider.Id))
            .Select(identityProvider => new ValueTuple<Guid, string>(
                identityProvider.Id,
                identityProvider.IamIdentityProvider!.IamIdpAlias
            ))
            .ToAsyncEnumerable();

    public Task<(string? UserEntityId, string? Alias, bool IsSameCompany)> GetIamUserIsOwnCompanyIdentityProviderAliasAsync(Guid companyUserId, Guid identityProviderId, Guid companyId) =>
        _context.CompanyUsers
            .AsNoTracking()
            .Where(companyUser => companyUser.Id == companyUserId)
            .Select(companyUser => new ValueTuple<string?, string?, bool>(
                companyUser.Identity!.UserEntityId,
                companyUser.Identity!.Company!.IdentityProviders
                    .Where(identityProvider => identityProvider.Id == identityProviderId)
                    .Select(identityProvider => identityProvider.IamIdentityProvider!.IamIdpAlias)
                    .SingleOrDefault(),
                companyUser.Identity!.CompanyId == companyId))
            .SingleOrDefaultAsync();

    public Task<((Guid CompanyId, string? CompanyName, string? BusinessPartnerNumber) Company,
                (Guid CompanyUserId, string? FirstName, string? LastName, string? Email) CompanyUser,
                IEnumerable<string> IdpAliase)>
        GetCompanyNameIdpAliaseUntrackedAsync(Guid companyUserId, Guid? applicationId, IdentityProviderCategoryId identityProviderCategoryId, IdentityProviderTypeId identityProviderTypeId) =>
            _context.CompanyUsers
                .AsNoTracking()
                .Where(companyUser => companyUser.Id == companyUserId &&
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
                        .Where(identityProvider =>
                            identityProvider.IdentityProviderCategoryId == identityProviderCategoryId &&
                            identityProvider.IdentityProviderTypeId == identityProviderTypeId)
                        .Select(identityProvider => identityProvider.IamIdentityProvider!.IamIdpAlias)))
                .SingleOrDefaultAsync();

    public Task<((Guid CompanyId, string? CompanyName, string? BusinessPartnerNumber) Company,
        (Guid CompanyUserId, string? FirstName, string? LastName, string? Email) CompanyUser,
        (string? IdpAlias, bool IsSharedIdp) IdentityProvider)>
        GetCompanyNameIdpAliasUntrackedAsync(Guid identityProviderId, Guid companyUserId) =>
            _context.CompanyUsers
                .AsNoTracking()
                .Where(companyUser => companyUser.Id == companyUserId)
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
                        s.IdentityProvider.IdentityProviderTypeId == IdentityProviderTypeId.SHARED)))
                .SingleOrDefaultAsync();
}
