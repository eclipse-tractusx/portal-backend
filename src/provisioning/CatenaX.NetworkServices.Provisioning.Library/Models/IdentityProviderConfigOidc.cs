using CatenaX.NetworkServices.Provisioning.Library.Enums;

namespace CatenaX.NetworkServices.Provisioning.Library.Models;

public record IdentityProviderConfigOidc(string DisplayName, string RedirectUrl, string ClientId, bool Enabled, string AuthorizationUrl, IamIdentityProviderClientAuthMethod ClientAuthMethod, IamIdentityProviderSignatureAlgorithm? SignatureAlgorithm);
public record IdentityProviderEditableConfigOidc(string Alias, string DisplayName, bool Enabled, string AuthorizationUrl, IamIdentityProviderClientAuthMethod ClientAuthMethod, string ClientId, string? Secret = null, IamIdentityProviderSignatureAlgorithm? SignatureAlgorithm = null);
