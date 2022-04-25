using Keycloak.Net.Models.Clients;
using Keycloak.Net.Models.ProtocolMappers;

using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CatenaX.NetworkServices.Provisioning.Library.Models;

namespace CatenaX.NetworkServices.Provisioning.Library
{
    public partial class ProvisioningManager
    {
        private async Task<Client> GetCentralClientViewableAsync(string clientId)
        {
            var client = (await _CentralIdp.GetClientsAsync(_Settings.CentralRealm, clientId: clientId, viewableOnly: true).ConfigureAwait(false))
                .SingleOrDefault();
            if (client == null)
            {
                throw new Exception($"failed to retrieve central client {clientId}");
            }
            return client;
        }

        private async Task CreateSharedRealmIdentityProviderClientAsync(string realm, IdentityProviderClientConfig config)
        {
            var newClient = CloneClient(_Settings.SharedRealmClient);
            newClient.RedirectUris = Enumerable.Repeat<string>(config.RedirectUri, 1);
            newClient.Attributes["jwks.url"] = config.JwksUrl;
            if (! await _SharedIdp.CreateClientAsync(realm,newClient))
            {
                throw new Exception($"failed to create shared realm {realm} client for redirect-uri {config.RedirectUri}");
            }
        }

        private async Task<string> CreateCentralOIDCClientAsync(string clientId, string redirectUri)
        {
            var newClient = CloneClient(_Settings.CentralOIDCClient);
            newClient.ClientId = clientId;
            newClient.RedirectUris = Enumerable.Repeat<string>(redirectUri, 1);
            var newClientId = await _CentralIdp.CreateClientAndRetrieveClientIdAsync(_Settings.CentralRealm, newClient).ConfigureAwait(false);
            if (newClientId == null)
            {
                throw new Exception($"failed to create new client {clientId} in central realm");
            }
            return newClientId;
        }

        private async Task CreateCentralOIDCClientAudienceMapperAsync(string internalClientId, string clientAudienceId)
        {
            if (! await _CentralIdp.CreateClientProtocolMapperAsync(_Settings.CentralRealm, internalClientId, new ProtocolMapper {
                Name = $"{clientAudienceId}-mapper",
                Protocol = "openid-connect",
                _ProtocolMapper = "oidc-audience-mapper",
                ConsentRequired =  false,
                Config = new Config {
                    IncludedClientAudience = clientAudienceId,
                    IdTokenClaim = "false",
                    AccessTokenClaim = "true",
                }
            }).ConfigureAwait(false))
            {
                throw new Exception($"failed to create audience-mapper for audience: {clientAudienceId}, internal clientid: {internalClientId}");
            }
        }

        private async Task<string> GetNextClientIdAsync() =>
            _Settings.ClientPrefix + (await _ProvisioningDBAccess!.GetNextClientSequenceAsync().ConfigureAwait(false)) + "_DummySuffix"; //TODO: implement full client naming scheme

        private Client CloneClient(Client client) =>
            JsonSerializer.Deserialize<Client>(JsonSerializer.Serialize(client));

        private async Task<string> GetIdOfClientFromClientIDAsync(string clientId) =>
            (await GetCentralClientViewableAsync(clientId).ConfigureAwait(false))
                .Id;
   }
}
