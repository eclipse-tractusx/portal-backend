/********************************************************************************
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

    public void DeleteIdentityProvider(Guid identityProviderId) =>
        _context.IdentityProviders.Remove(new IdentityProvider(identityProviderId, default, default, Guid.Empty, default));

    public CompanyIdentityProvider CreateCompanyIdentityProvider(Guid companyId, Guid identityProviderId) =>
        _context.CompanyIdentityProviders
            .Add(new CompanyIdentityProvider(
                companyId,
                identityProviderId
            )).Entity;

    public void DeleteCompanyIdentityProvider(Guid companyId, Guid identityProviderId) =>
        _context.Remove(new CompanyIdentityProvider(companyId, identityProviderId));

    public void CreateCompanyIdentityProviders(IEnumerable<(Guid CompanyId, Guid IdentityProviderId)> companyIdIdentityProviderIds) =>
        _context.CompanyIdentityProviders
            .AddRange(companyIdIdentityProviderIds.Select(x => new CompanyIdentityProvider(
                x.CompanyId,
                x.IdentityProviderId
            )));

    /// <inheritdoc/>
    public IamIdentityProvider CreateIamIdentityProvider(Guid identityProviderId, string idpAlias) =>
        _context.IamIdentityProviders.Add(
            new IamIdentityProvider(
                idpAlias,
                identityProviderId)).Entity;

    public void DeleteIamIdentityProvider(string idpAlias) =>
        _context.IamIdentityProviders.Remove(new IamIdentityProvider(idpAlias, Guid.Empty));

    public Task<string?> GetSharedIdentityProviderIamAliasDataUntrackedAsync(Guid companyId) =>
        _context.IdentityProviders
            .AsNoTracking()
            .Where(identityProvider =>
                identityProvider.IdentityProviderTypeId == IdentityProviderTypeId.SHARED &&
                identityProvider.Companies.Any(company => company.Id == companyId))
            .Select(identityProvider => identityProvider.IamIdentityProvider!.IamIdpAlias)
            .SingleOrDefaultAsync();

    public Task<(string? Alias, bool IsValidUser)> GetIdpCategoryIdByUserIdAsync(Guid companyUserId, Guid userCompanyId) =>
        _context.CompanyUsers.AsNoTracking()
            .Where(companyUser => companyUser.Id == companyUserId
                && companyUser.Identity!.CompanyId == userCompanyId)
            .Select(companyUser => new ValueTuple<string?, bool>(
                companyUser.Identity!.Company!.IdentityProviders
                    .Where(identityProvider => identityProvider.IdentityProviderTypeId == IdentityProviderTypeId.SHARED)
                    .Select(identityProvider => identityProvider.IamIdentityProvider!.IamIdpAlias)
                    .SingleOrDefault(),
                true))
            .SingleOrDefaultAsync();

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

    public Task<(string? Alias, IdentityProviderCategoryId IamIdentityProviderCategory, bool IsOwnerCompany, IdentityProviderTypeId TypeId, IEnumerable<ConnectedCompanyData> ConnectedCompanies)> GetOwnIdentityProviderWithConnectedCompanies(Guid identityProviderId, Guid companyId) =>
        _context.IdentityProviders
            .Where(identityProvider => identityProvider.Id == identityProviderId)
            .Select(identityProvider =>
                new ValueTuple<string?, IdentityProviderCategoryId, bool, IdentityProviderTypeId, IEnumerable<ConnectedCompanyData>>(
                    identityProvider.IamIdentityProvider!.IamIdpAlias,
                    identityProvider.IdentityProviderCategoryId,
                    identityProvider.OwnerId == companyId,
                    identityProvider.IdentityProviderTypeId,
                    identityProvider.Companies.Select(c => new ConnectedCompanyData(c.Id, c.Name))
                    ))
            .SingleOrDefaultAsync();

    public Task<(bool IsOwner, string? Alias, IdentityProviderCategoryId IdentityProviderCategory, IdentityProviderTypeId IdentityProviderTypeId, IEnumerable<(Guid CompanyId, IEnumerable<string> Aliase)>? CompanyIdAliase, bool CompanyUsersLinked, string IdpOwnerName)> GetOwnCompanyIdentityProviderUpdateDataUntrackedAsync(Guid identityProviderId, Guid companyId, bool queryAliase) =>
        _context.IdentityProviders
            .Where(identityProvider => identityProvider.Id == identityProviderId)
            .Select(identityProvider =>
                new ValueTuple<bool, string?, IdentityProviderCategoryId, IdentityProviderTypeId, IEnumerable<(Guid, IEnumerable<string>)>?, bool, string>(
                    identityProvider.OwnerId == companyId,
                    identityProvider.IamIdentityProvider!.IamIdpAlias,
                    identityProvider.IdentityProviderCategoryId,
                    identityProvider.IdentityProviderTypeId,
                    queryAliase
                        ? identityProvider.Companies
                            .Select(c => new ValueTuple<Guid, IEnumerable<string>>(
                                c.Id,
                                c.IdentityProviders
                                    .Where(i => i.IamIdentityProvider != null)
                                    .Select(i => i.IamIdentityProvider!.IamIdpAlias)))
                        : null,
                    identityProvider.CompanyUserAssignedIdentityProviders.Any(),
                    identityProvider.Owner!.Name
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

    public Task<(Guid IdentityProviderId, string? Alias)> GetSingleManagedIdentityProviderAliasDataUntracked(Guid companyId) =>
        _context.IdentityProviders
            .AsNoTracking()
            .Where(identityProvider =>
                identityProvider.OwnerId == companyId &&
                identityProvider.IdentityProviderTypeId == IdentityProviderTypeId.MANAGED)
            .Select(identityProvider => new ValueTuple<Guid, string?>(
                identityProvider.Id,
                identityProvider.IamIdentityProvider!.IamIdpAlias
            ))
            .SingleOrDefaultAsync();

    public IAsyncEnumerable<(Guid IdentityProviderId, string? Alias)> GetManagedIdentityProviderAliasDataUntracked(Guid companyId, IEnumerable<Guid> identityProviderIds) =>
        _context.IdentityProviders
            .AsNoTracking()
            .Where(identityProvider =>
                identityProvider.OwnerId == companyId &&
                identityProvider.IdentityProviderTypeId == IdentityProviderTypeId.MANAGED &&
                identityProviderIds.Contains(identityProvider.Id))
            .Select(identityProvider => new ValueTuple<Guid, string?>(
                identityProvider.Id,
                identityProvider.IamIdentityProvider!.IamIdpAlias
            ))
            .ToAsyncEnumerable();

    public Task<(bool IsValidUser, string? Alias, bool IsSameCompany)> GetIamUserIsOwnCompanyIdentityProviderAliasAsync(Guid companyUserId, Guid identityProviderId, Guid companyId) =>
        _context.CompanyUsers
            .AsNoTracking()
            .Where(companyUser => companyUser.Id == companyUserId)
            .Select(companyUser => new ValueTuple<bool, string?, bool>(
                true,
                companyUser.Identity!.Company!.IdentityProviders
                    .Where(identityProvider => identityProvider.Id == identityProviderId)
                    .Select(identityProvider => identityProvider.IamIdentityProvider!.IamIdpAlias)
                    .SingleOrDefault(),
                companyUser.Identity!.CompanyId == companyId))
            .SingleOrDefaultAsync();

    public Task<((Guid CompanyId, string? CompanyName, string? BusinessPartnerNumber) Company,
                (Guid CompanyUserId, string? FirstName, string? LastName, string? Email) CompanyUser,
                IEnumerable<(Guid IdentityProviderId, string Alias)> IdpAliase)>
        GetCompanyNameIdpAliaseUntrackedAsync(Guid companyUserId, Guid? applicationId, IdentityProviderCategoryId identityProviderCategoryId, IdentityProviderTypeId identityProviderTypeId) =>
            _context.CompanyUsers
                .AsNoTracking()
                .Where(companyUser => companyUser.Id == companyUserId &&
                    (applicationId == null || companyUser.Identity!.Company!.CompanyApplications.Any(application => application.Id == applicationId)))
                .Select(companyUser => new ValueTuple<(Guid, string?, string?), (Guid, string?, string?, string?), IEnumerable<(Guid, string)>>(
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
                        .Select(identityProvider => new ValueTuple<Guid, string>(identityProvider.Id, identityProvider.IamIdentityProvider!.IamIdpAlias))))
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

    public IAsyncEnumerable<Guid> GetIdpLinkedCompanyUserIds(Guid identityProviderId, Guid companyId) =>
        _context.CompanyUserAssignedIdentityProviders
            .Where(x =>
                x.IdentityProviderId == identityProviderId &&
                x.CompanyUser!.Identity!.CompanyId == companyId)
            .Select(x => x.CompanyUserId)
            .ToAsyncEnumerable();

    public IAsyncEnumerable<(Guid CompanyId, CompanyStatusId CompanyStatusId, bool HasMoreIdentityProviders, IEnumerable<(Guid IdentityId, bool IsLinkedCompanyUser, (string? UserMail, string? FirstName, string? LastName) Userdata, bool IsInUserRoles, IEnumerable<Guid> UserRoleIds)> Identities)> GetManagedIdpLinkedData(Guid identityProviderId, IEnumerable<Guid> userRoleIds) =>
        _context.IdentityProviders
            .AsSplitQuery()
            .Where(x => x.Id == identityProviderId)
            .SelectMany(x => x.Companies.Select(c => new ValueTuple<Guid, CompanyStatusId, bool, IEnumerable<(Guid, bool, ValueTuple<string?, string?, string?>, bool, IEnumerable<Guid>)>>(
                c.Id,
                c.CompanyStatusId,
                c.IdentityProviders.Any(idp => idp.Id != identityProviderId),
                c.Identities
                    .Select(i => new ValueTuple<Guid, bool, ValueTuple<string?, string?, string?>, bool, IEnumerable<Guid>>(
                    i.Id,
                    i.CompanyUser!.CompanyUserAssignedIdentityProviders.Any(cuIdp => cuIdp.IdentityProviderId == identityProviderId),
                    new ValueTuple<string?, string?, string?>(i.CompanyUser.Email, i.CompanyUser.Firstname, i.CompanyUser.Lastname),
                    i.IdentityAssignedRoles.Any(assigned => userRoleIds.Contains(assigned.UserRoleId)),
                    i.IdentityAssignedRoles.Select(u => u.UserRoleId))))))
            .ToAsyncEnumerable();

    /// <inheritdoc />
    public IAsyncEnumerable<(string Email, string? FirstName, string? LastName)> GetCompanyUserEmailForIdpWithoutOwnerAndRoleId(IEnumerable<Guid> userRoleIds, Guid identityProviderId) =>
        _context.CompanyUsers
            .Where(x =>
                x.Identity!.Company!.IdentityProviders.Any(idp => idp.Id == identityProviderId && idp.OwnerId != x.Identity!.CompanyId) &&
                x.Identity!.UserStatusId == UserStatusId.ACTIVE &&
                x.Identity!.IdentityAssignedRoles.Any(assigned => userRoleIds.Contains(assigned.UserRoleId)) &&
                x.Email != null)
            .Select(x => new ValueTuple<string, string?, string?>(x.Email!, x.Firstname, x.Lastname))
            .ToAsyncEnumerable();
}
