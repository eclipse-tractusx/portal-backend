namespace CatenaX.NetworkServices.Administration.Service.Models;

public record UserIdentityProviderData(Guid companyUserId, string? firstName, string? lastName, string? email, IEnumerable<UserIdentityProviderLinkData> identityProviders);

public record UserIdentityProviderLinkData(Guid identityProviderId, string userId, string userName);

public record UserLinkData(string userId, string userName);
