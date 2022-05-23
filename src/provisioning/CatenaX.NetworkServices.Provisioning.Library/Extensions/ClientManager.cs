using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.Provisioning.Library.Models;
using CatenaX.NetworkServices.Provisioning.Library.Enums;
using Keycloak.Net.Models.Clients;
using Keycloak.Net.Models.ProtocolMappers;
using System.Text.Json;

namespace CatenaX.NetworkServices.Provisioning.Library;

public partial class ProvisioningManager
{
    private static readonly IReadOnlyDictionary<string,IamClientAuthMethod> CredentialTypesIamClientAuthMethodDictionary = new Dictionary<string,IamClientAuthMethod>()
    {
        { "jwt", IamClientAuthMethod.JWT },
        { "secret", IamClientAuthMethod.SECRET },
        { "x509", IamClientAuthMethod.X509 },
        { "secret-jwt", IamClientAuthMethod.SECRET_JWT }
    };

    private static readonly IReadOnlyDictionary<IamClientAuthMethod,string> IamClientAuthMethodsInternalDictionary = new Dictionary<IamClientAuthMethod,string>()
    {
        { IamClientAuthMethod.JWT, "client-jwt" },
        { IamClientAuthMethod.SECRET, "client-secret" },
        { IamClientAuthMethod.X509, "client-x509" },
        { IamClientAuthMethod.SECRET_JWT, "client-secret-jwt" }
    };

    public async Task UpdateCentralClientAsync(string internalClientId, ClientConfigData config)
    {
        var client = await _CentralIdp.GetClientAsync(_Settings.CentralRealm, internalClientId).ConfigureAwait(false);
        if (client == null)
        {
            throw new NotFoundException($"failed to retrieve central client {internalClientId}");
        }
        client.Name = config.Name;
        client.ClientAuthenticatorType = IamClientAuthMethodToInternal(config.IamClientAuthMethod);
        if (! await _CentralIdp.UpdateClientAsync(_Settings.CentralRealm, internalClientId, client).ConfigureAwait(false))
        {
            throw new Exception($"failed to update client {internalClientId}");
        }
    }

    public async Task<ClientAuthData> GetCentralClientAuthDataAsync(string internalClientId)
    {
        var credentials = await _CentralIdp.GetClientSecretAsync(_Settings.CentralRealm, internalClientId).ConfigureAwait(false);
        if (credentials == null)
        {
            throw new NotFoundException($"credentials of client {internalClientId} not found in keycloak");
        }
        return new ClientAuthData(
            CredentialsTypeToIamClientAuthMethod(credentials.Type))
            {
                Secret = credentials.Value
            };
    }

    public async Task<ClientAuthData> ResetCentralClientAuthDataAsync(string internalClientId)
    {
        var credentials = await _CentralIdp.GenerateClientSecretAsync(_Settings.CentralRealm, internalClientId).ConfigureAwait(false);
        if (credentials == null)
        {
            throw new NotFoundException($"credentials of client {internalClientId} not found in keycloak");
        }
        return new ClientAuthData(
            CredentialsTypeToIamClientAuthMethod(credentials.Type))
            {
                Secret = credentials.Value
            };
    }

    private async Task<Client> GetCentralClientViewableAsync(string clientId)
    {
        var client = (await _CentralIdp.GetClientsAsync(_Settings.CentralRealm, clientId: clientId, viewableOnly: true).ConfigureAwait(false))
            .SingleOrDefault();
        if (client == null)
        {
            throw new NotFoundException($"failed to retrieve central client {clientId}");
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

    private async Task<string> GetCentralInternalClientIdFromClientIDAsync(string clientId) =>
        (await GetCentralClientViewableAsync(clientId).ConfigureAwait(false))
            .Id;

    private IamClientAuthMethod CredentialsTypeToIamClientAuthMethod(string clientAuthMethod)
    {
        try
        {
            return CredentialTypesIamClientAuthMethodDictionary[clientAuthMethod];
        }
        catch (KeyNotFoundException)
        {
            throw new ArgumentException($"unexpected value of clientAuthMethod: {clientAuthMethod}","clientAuthMethod");
        }
    }

    private string IamClientAuthMethodToInternal(IamClientAuthMethod iamClientAuthMethod)
    {
        try
        {
            return IamClientAuthMethodsInternalDictionary[iamClientAuthMethod];
        }
        catch (KeyNotFoundException)
        {
            throw new ArgumentException($"unexpected value of IamClientAuthMethod: {iamClientAuthMethod}","authMethod");
        }
    }

    private async Task<string> GetNextClientIdAsync() =>
        _Settings.ClientPrefix + (await _ProvisioningDBAccess!.GetNextClientSequenceAsync().ConfigureAwait(false));

    private Client CloneClient(Client client) =>
        JsonSerializer.Deserialize<Client>(JsonSerializer.Serialize(client))!;
}
