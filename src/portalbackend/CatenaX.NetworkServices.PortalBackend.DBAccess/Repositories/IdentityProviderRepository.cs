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
                ((string Alias, IdentityProviderCategoryId IamIdentityProviderCategory, bool IsOwnCompany)) new (
                    identityProvider.IamIdentityProvider!.IamIdpAlias,
                    identityProvider.IdentityProviderCategoryId,
                    identityProvider.Companies.Any(
                        company => company.CompanyUsers.Any(
                            companyUser => companyUser.IamUser!.UserEntityId == iamUserId))))
            .SingleOrDefaultAsync();

    public IAsyncEnumerable<(Guid Id, IdentityProviderCategoryId CategoryId, string Alias)> GetOwnCompanyIdentityProviderDataUntracked(string iamUserId) =>
        _context.IamUsers
            .AsNoTracking()
            .Where(iamUser => iamUser.UserEntityId == iamUserId)
            .SelectMany(iamUser => iamUser.CompanyUser!.Company!.IdentityProviders)
            .Select(identityProvider => ((Guid Id, IdentityProviderCategoryId CategoryId, string Alias)) new (
                identityProvider.Id,
                identityProvider.IdentityProviderCategoryId,
                identityProvider.IamIdentityProvider!.IamIdpAlias
            ))
            .ToAsyncEnumerable();
}
