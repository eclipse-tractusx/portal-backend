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
    IAsyncEnumerable<(Guid Id, IdentityProviderCategoryId CategoryId, string Alias)> GetOwnCompanyIdentityProviderDataUntracked(string iamUserId);
    Task<(string? UserEntityId, string? FirstName, string? LastName, string? Email, IEnumerable<(string Alias, IdentityProviderCategoryId CategoryId)> IdentityProviders, bool IsSameCompany)> GetIamUserIsOwnCompanyIdentityProviderAliasAsync(Guid companyUserId, string alias, string iamUserId);
}
