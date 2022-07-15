namespace CatenaX.NetworkServices.Administration.Service.Models;

public record UserIdentityProviderData(Guid companyUserId, string? firstName, string? lastName, string? email, IEnumerable<UserIdentityProviderLinkData> identityProviderLinks);

public record UserIdentityProviderLinkData(string alias, string userId, string userName);
