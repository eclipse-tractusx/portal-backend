using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.Provisioning.Library.Enums;
using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.Administration.Service.Models;

public record IdentityProviderDetails(Guid identityProviderId, IdentityProviderCategoryId identityProviderCategoryId, string displayName, string redirectUrl, string clientId, bool enabled)
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IdentityProviderDetailsOIDC? oidc { get; init; } = null;
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IdentityProviderDetailsSAML? saml { get; init; } = null;
}

public record IdentityProviderDetailsOIDC(string authorizationUrl, IamIdentityProviderClientAuthMethod clientAuthMethod);

public record IdentityProviderDetailsSAML(string serviceProviderEntityId, string singleSignOnServiceUrl);
