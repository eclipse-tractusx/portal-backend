using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using CatenaX.NetworkServices.Provisioning.Library.Enums;
using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.Administration.Service.Models;

public record IdentityProviderDetails(Guid identityProviderId, string alias, IdentityProviderCategoryId identityProviderCategoryId, string displayName, string redirectUrl, bool enabled)
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IdentityProviderDetailsOIDC? oidc { get; init; } = null;
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IdentityProviderDetailsSAML? saml { get; init; } = null;
}

public record IdentityProviderDetailsOIDC(string authorizationUrl, string clientId, IamIdentityProviderClientAuthMethod clientAuthMethod)
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IamIdentityProviderSignatureAlgorithm? signatureAlgorithm { get; init; } = null;
}

public record IdentityProviderDetailsSAML(string serviceProviderEntityId, string singleSignOnServiceUrl);
