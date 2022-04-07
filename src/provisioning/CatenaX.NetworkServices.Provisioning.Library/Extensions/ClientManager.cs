using Keycloak.Net.Models.Clients;
using Keycloak.Net.Models.ProtocolMappers;

using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CatenaX.NetworkServices.Provisioning.Library.Models;

namespace CatenaX.NetworkServices.Provisioning.Library
{
    public partial class ProvisioningManager
    {
        private async Task<Client> GetCentralClientViewableAsync(string clientId) =>
            (await _CentralIdp.GetClientsAsync(_Settings.CentralRealm, clientId: clientId, viewableOnly: true).ConfigureAwait(false))
                .SingleOrDefault();

        private Task<bool> CreateSharedRealmIdentityProviderClientAsync(string realm, IdentityProviderClientConfig config)
        {
            var newClient = CloneClient(_Settings.SharedRealmClient);
            newClient.RedirectUris = Enumerable.Repeat<string>(config.RedirectUri, 1);
            newClient.Attributes["jwks.url"] = config.JwksUrl;
            return _SharedIdp.CreateClientAsync(realm,newClient);
        }

        private Task<string> CreateCentralOIDCClientAsync(string clientId, string redirectUri)
        {
            var newClient = CloneClient(_Settings.CentralOIDCClient);
            newClient.ClientId = clientId;
            newClient.RedirectUris = Enumerable.Repeat<string>(redirectUri, 1);
            return _CentralIdp.CreateClientAndRetrieveClientIdAsync(_Settings.CentralRealm,newClient);
        }

        private Task<bool> CreateCentralOIDCClientAudienceMapperAsync(string internalClientId, string clientAudienceId) =>
            _CentralIdp.CreateClientProtocolMapperAsync(_Settings.CentralRealm, internalClientId, new ProtocolMapper {
                Name = $"{clientAudienceId}-mapper",
                Protocol = "openid-connect",
                _ProtocolMapper = "oidc-audience-mapper",
                ConsentRequired =  false,
                Config = new Config {
                    IncludedClientAudience = clientAudienceId,
                    IdTokenClaim = "false",
                    AccessTokenClaim = "true",
                }
            });

        private async Task<string> GetNextClientIdAsync() =>
            _Settings.ClientPrefix + (await _ProvisioningDBAccess.GetNextClientSequenceAsync().ConfigureAwait(false)).sequence_id + "_DummySuffix"; //TODO: implement full client naming scheme

        private Client CloneClient(Client client) =>
            JsonSerializer.Deserialize<Client>(JsonSerializer.Serialize(client));

        private async Task<string> GetIdOfClientFromClientIDAsync(string clientId) =>
            (await GetCentralClientViewableAsync(clientId).ConfigureAwait(false))
                .Id;
   }
}
