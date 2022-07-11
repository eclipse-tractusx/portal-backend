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
    public IdentityProvider CreateSharedIdentityProvider(Company company)
    {
        var idp = new IdentityProvider(
            Guid.NewGuid(),
            IdentityProviderCategoryId.KEYCLOAK_SHARED,
            DateTimeOffset.UtcNow);
        idp.Companies.Add(company);
        return _context.IdentityProviders.Add(idp).Entity;
    }

    /// <inheritdoc/>

    public IamIdentityProvider CreateIamIdentityProvider(IdentityProvider identityProvider, string idpAlias) =>
        _context.IamIdentityProviders.Add(
            new IamIdentityProvider(
                idpAlias,
                identityProvider.Id)).Entity;

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
