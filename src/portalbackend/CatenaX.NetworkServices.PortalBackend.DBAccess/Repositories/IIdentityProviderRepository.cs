using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

/// <summary>
/// Repository for User Management on persistence layer.
/// </summary>
public interface IIdentityProviderRepository
{
    IdentityProvider CreateIdentityProvider(IdentityProviderCategoryId identityProviderCategory);
    IamIdentityProvider CreateIamIdentityProvider(IdentityProvider identityProvider, string idpAlias);
    CompanyIdentityProvider CreateCompanyIdentityProvider(Guid companyId, Guid identityProviderId);
    Task<(string Alias, IdentityProviderCategoryId IamIdentityProviderCategory, bool IsOwnCompany)> GetOwnCompanyIdentityProviderAliasUntrackedAsync(Guid identityProviderId, string iamUserId);
    IAsyncEnumerable<(Guid IdentityProviderId, IdentityProviderCategoryId CategoryId, string Alias)> GetOwnCompanyIdentityProviderDataUntracked(string iamUserId, IEnumerable<Guid>? identityProviderIds = null);
    Task<(string? UserEntityId, string? Alias, bool IsSameCompany)> GetIamUserIsOwnCompanyIdentityProviderAliasAsync(Guid companyUserId, Guid identityProviderId, string iamUserId);
}
