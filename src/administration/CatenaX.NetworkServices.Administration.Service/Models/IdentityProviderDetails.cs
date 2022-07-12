using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.Provisioning.Library.Enums;
using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.Administration.Service.Models;

public class IdentityProviderDetails
{
    public IdentityProviderDetails(Guid identityProviderId, IdentityProviderCategoryId identityProviderCategoryId, string displayName, string redirectUrl, bool enabled)
    {
        IdentityProviderId = identityProviderId;
        IdentityProviderCategoryId = identityProviderCategoryId;
        DisplayName = displayName;
        RedirectUrl = redirectUrl;
        Enabled = enabled;
    }

    [JsonPropertyName("identityProviderId")]
    public Guid IdentityProviderId { get; }

    [JsonPropertyName("identityProviderCategory")]
    public IdentityProviderCategoryId IdentityProviderCategoryId { get; }

    [JsonPropertyName("displayName")]
    public string DisplayName { get; }

    [JsonPropertyName("redirectUrl")]
    public string RedirectUrl { get; set; }

    [JsonPropertyName("enabled")]
    public bool Enabled { get; }

    // OIDC related properties:
    [JsonPropertyName("clientId")]
    public string? ClientId { get; set; }

    [JsonPropertyName("authorizationUrl")]
    public string? AuthorizationUrl { get; set; }

    [JsonPropertyName("clientAuthMethod")]
    public IamIdentityProviderClientAuthMethod? ClientAuthMethod { get; set; }

    // SAML related properties:
    [JsonPropertyName("serviceProviderEntityId")]
    public string? ServiceProviderEntityId { get; set; }

    [JsonPropertyName("singleSignOnServiceUrl")]
    public string? SingleSignOnServiceUrl { get; set; }
}
