using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

/// <summary>
/// Repository for User Management on persistence layer.
/// </summary>
public interface IIdentityProviderRepository
{
    IdentityProvider CreateSharedIdentityProvider(Company company);
    IamIdentityProvider CreateIamIdentityProvider(IdentityProvider identityProvider, string idpAlias);
}
