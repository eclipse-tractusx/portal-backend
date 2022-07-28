using CatenaX.NetworkServices.Administration.Service.Models;
using CatenaX.NetworkServices.Provisioning.Library.Enums;

namespace CatenaX.NetworkServices.Administration.Service.BusinessLogic;

public interface IIdentityProviderBusinessLogic
{
    IAsyncEnumerable<IdentityProviderDetails> GetOwnCompanyIdentityProviders(string iamUserId);
    Task<IdentityProviderDetails> CreateOwnCompanyIdentityProvider(IamIdentityProviderProtocol protocol, string iamUserId);
    Task<IdentityProviderDetails> GetOwnCompanyIdentityProvider(Guid identityProviderId, string iamUserId);
    Task<IdentityProviderDetails> UpdateOwnCompanyIdentityProvider(Guid identityProviderId, IdentityProviderEditableDetails details, string iamUserId);
    Task DeleteOwnCompanyIdentityProvider(Guid identityProviderId, string iamUserId);
    IAsyncEnumerable<UserIdentityProviderData> GetOwnCompanyUsersIdentityProviderDataAsync(IEnumerable<Guid>? identityProviderIds, string iamUserId);
    Task<UserIdentityProviderLinkData> CreateOwnCompanyUserIdentityProviderLinkDataAsync(Guid companyUserId, UserIdentityProviderLinkData identityProviderLinkData, string iamUserId);
    Task<UserIdentityProviderLinkData> UpdateOwnCompanyUserIdentityProviderLinkDataAsync(Guid companyUserId, Guid identityProviderId, UserLinkData userLinkData, string iamUserId);
    Task<UserIdentityProviderLinkData> GetOwnCompanyUserIdentityProviderLinkDataAsync(Guid companyUserId, Guid identityProviderId, string iamUserId);
    Task DeleteOwnCompanyUserIdentityProviderDataAsync(Guid companyUserId, Guid identityProviderId, string iamUserId);
}
