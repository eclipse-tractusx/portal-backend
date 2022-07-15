using CatenaX.NetworkServices.Provisioning.Library.Enums;
using System.Text.Json.Serialization;

namespace CatenaX.NetworkServices.Administration.Service.Models;

public record IdentityProviderEditableDetails(string displayName, string redirectUrl, bool enabled)
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IdentityProviderEditableDetailsOIDC? oidc { get; init; } = null;
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IdentityProviderEditableDetailsSAML? saml { get; init; } = null;
}

public record IdentityProviderEditableDetailsOIDC(string authorizationUrl, IamIdentityProviderClientAuthMethod clientAuthMethod)
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? secret { get; init; } = null;
}

public record IdentityProviderEditableDetailsSAML(string serviceProviderEntityId, string singleSignOnServiceUrl);
