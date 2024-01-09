using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.OpenIDConfiguration;

namespace Org.Eclipse.TractusX.Portal.Backend.ExternalSystems.Provisioning.Library;

public interface IIdpManagement
{
    ValueTask<string> GetNextCentralIdentityProviderNameAsync();
    Task CreateCentralIdentityProviderAsync(string alias, string displayName);
    Task<(string ClientId, string Secret)> CreateSharedIdpServiceAccountAsync(string realm);
    ValueTask UpdateCentralIdentityProviderUrlsAsync(string alias, string organisationName, string loginTheme, string clientId, string secret);
    Task CreateCentralIdentityProviderOrganisationMapperAsync(string alias, string organisationName);
    Task CreateSharedRealmIdpClientAsync(string realm, string loginTheme, string organisationName, string clientId, string secret);
    ValueTask EnableCentralIdentityProviderAsync(string alias);
}
