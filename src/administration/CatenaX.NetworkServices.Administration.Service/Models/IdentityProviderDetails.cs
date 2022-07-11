using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.Provisioning.Library.Enums;
using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.Administration.Service.Models;

public class IdentityProviderDetails
{
    public IdentityProviderDetails(Guid identityProviderId, IdentityProviderCategoryId identityProviderCategoryId, string redirectUrl, string displayName, string authorizationUrl, IamIdentityProviderClientAuthMethod clientAuthMethod, string clientId, bool enabled)
    {
        IdentityProviderId = identityProviderId;
        IdentityProviderCategoryId = identityProviderCategoryId;
        RedirectUrl = redirectUrl;
        DisplayName = displayName;
        AuthorizationUrl = authorizationUrl;
        ClientAuthMethod = clientAuthMethod;
        ClientId = clientId;
        Enabled = enabled;
    }

    [JsonPropertyName("identity_provider_id")]
    public Guid IdentityProviderId { get; }

    [JsonPropertyName("identity_provider_category")]
    public IdentityProviderCategoryId IdentityProviderCategoryId { get; }

    [JsonPropertyName("redirect_url")]
    public string RedirectUrl { get; }

    [JsonPropertyName("display_name")]
    public string DisplayName { get; }

    [JsonPropertyName("authorization_url")]
    public string AuthorizationUrl { get; }

    [JsonPropertyName("client_auth_method")]
    public IamIdentityProviderClientAuthMethod ClientAuthMethod { get; }

    [JsonPropertyName("client_id")]
    public string ClientId { get; }

    [JsonPropertyName("enabled")]
    public bool Enabled { get; }
}
