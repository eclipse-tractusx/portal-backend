namespace CatenaX.NetworkServices.Provisioning.Library.Models;

public record IdentityProviderConfigSaml(string DisplayName, string RedirectUrl, string ClientId, bool Enabled, string EntityId, string SingleSignOnServiceUrl);
public record IdentityProviderEditableConfigSaml(string alias, string displayName, string entityId, string singleSignOnServiceUrl);
