using CatenaX.NetworkServices.Provisioning.Library.Models;

namespace CatenaX.NetworkServices.Provisioning.Library;

public interface IProvisioningManager
{
    Task<string> GetNextCentralIdentityProviderNameAsync();
    Task<string> GetNextServiceAccountClientIdAsync();
    Task SetupSharedIdpAsync(string idpName, string organisationName);
    Task<string> CreateSharedUserLinkedToCentralAsync(string idpName, UserProfile userProfile);
    Task AssignClientRolesToCentralUserAsync(string centralUserId, IDictionary<string,IEnumerable<string>> clientRoleNames);
    Task<IEnumerable<string>> GetClientRolesAsync(string clientId);
    Task<IEnumerable<string>> GetClientRolesCompositeAsync(string clientId);
    Task<string> SetupOwnIdpAsync(string organisationName, string clientId, string metadataUrl, string clientAuthMethod, string? clientSecret);
    Task<string?> GetProviderUserIdForCentralUserIdAsync(string identityProvider, string userId);
    Task<bool> DeleteSharedRealmUserAsync(string idpName, string userIdShared);
    Task<bool> DeleteCentralRealmUserAsync(string userIdCentral);
    Task<string> SetupClientAsync(string redirectUrl);
    Task<ServiceAccountData> SetupCentralServiceAccountClientAsync(string clientId, ClientConfigData config);
    Task UpdateCentralClientAsync(string internalClientId, ClientConfigData config);
    Task<ClientAuthData> GetCentralClientAuthDataAsync(string internalClientId);
    Task<ClientAuthData> ResetCentralClientAuthDataAsync(string internalClientId);
    Task AddBpnAttributetoUserAsync(string centralUserId, IEnumerable<string> bpns);
    Task<bool> ResetSharedUserPasswordAsync(string realm, string userId);
    Task<IEnumerable<string>> GetClientRoleMappingsForUserAsync(string userId, string clientId);
}
