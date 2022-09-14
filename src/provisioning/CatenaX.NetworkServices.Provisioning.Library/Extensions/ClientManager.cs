/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
 *
 * See the NOTICE file(s) distributed with this work for additional
 * information regarding copyright ownership.
 *
 * This program and the accompanying materials are made available under the
 * terms of the Apache License, Version 2.0 which is available at
 * https://www.apache.org/licenses/LICENSE-2.0.
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 *
 * SPDX-License-Identifier: Apache-2.0
 ********************************************************************************/

using CatenaX.NetworkServices.Keycloak.ErrorHandling;
using CatenaX.NetworkServices.Provisioning.Library.Models;
using CatenaX.NetworkServices.Provisioning.Library.Enums;
using Keycloak.Net;
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
        client.Name = config.Name;
        client.ClientAuthenticatorType = IamClientAuthMethodToInternal(config.IamClientAuthMethod);
        if (! await _CentralIdp.UpdateClientAsync(_Settings.CentralRealm, internalClientId, client).ConfigureAwait(false))
        {
            throw new Exception($"failed to update client {internalClientId}");
        }
    }

    public async Task DeleteCentralClientAsync(string internalClientId)
    {
        if (! await _CentralIdp.DeleteClientAsync(_Settings.CentralRealm, internalClientId).ConfigureAwait(false))
        {
            throw new KeycloakNoSuccessException($"failed to delete client {internalClientId} in central keycloak {_Settings.CentralRealm}");
        }
    }

    public async Task<ClientAuthData> GetCentralClientAuthDataAsync(string internalClientId)
    {
        var credentials = await _CentralIdp.GetClientSecretAsync(_Settings.CentralRealm, internalClientId).ConfigureAwait(false);
        return new ClientAuthData(
            CredentialsTypeToIamClientAuthMethod(credentials.Type))
            {
                Secret = credentials.Value
            };
    }

    public async Task<ClientAuthData> ResetCentralClientAuthDataAsync(string internalClientId)
    {
        var credentials = await _CentralIdp.GenerateClientSecretAsync(_Settings.CentralRealm, internalClientId).ConfigureAwait(false);
        return new ClientAuthData(
            CredentialsTypeToIamClientAuthMethod(credentials.Type))
            {
                Secret = credentials.Value
            };
    }

    private async Task<Client?> GetCentralClientViewableAsync(string clientId)
    {
        var client = (await _CentralIdp.GetClientsAsync(_Settings.CentralRealm, clientId: clientId, viewableOnly: true).ConfigureAwait(false))
            .SingleOrDefault();
        return client;
    }

    private async Task CreateSharedRealmIdentityProviderClientAsync(KeycloakClient keycloak, string realm, IdentityProviderClientConfig config)
    {
        var newClient = Clone(_Settings.SharedRealmClient);
        newClient.RedirectUris = Enumerable.Repeat<string>(config.RedirectUri, 1);
        newClient.Attributes["jwks.url"] = config.JwksUrl;
        if (! await keycloak.CreateClientAsync(realm,newClient))
        {
            throw new KeycloakNoSuccessException($"failed to create shared realm {realm} client for redirect-uri {config.RedirectUri}");
        }
    }

    private async Task<string> CreateCentralOIDCClientAsync(string clientId, string redirectUri)
    {
        var newClient = Clone(_Settings.CentralOIDCClient);
        newClient.ClientId = clientId;
        newClient.RedirectUris = Enumerable.Repeat<string>(redirectUri, 1);
        var newClientId = await _CentralIdp.CreateClientAndRetrieveClientIdAsync(_Settings.CentralRealm, newClient).ConfigureAwait(false);
        if (newClientId == null)
        {
            throw new KeycloakNoSuccessException($"failed to create new client {clientId} in central realm");
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
            throw new KeycloakNoSuccessException($"failed to create audience-mapper for audience: {clientAudienceId}, internal clientid: {internalClientId}");
        }
    }

    private async Task<string?> GetCentralInternalClientIdFromClientIDAsync(string clientId) =>
        (await GetCentralClientViewableAsync(clientId).ConfigureAwait(false))?.Id;

    private IamClientAuthMethod CredentialsTypeToIamClientAuthMethod(string clientAuthMethod)
    {
        if (!CredentialTypesIamClientAuthMethodDictionary.TryGetValue(clientAuthMethod, out var iamClientAuthMethod))
        {
            throw new ArgumentException($"unexpected value of clientAuthMethod: {clientAuthMethod}", nameof(clientAuthMethod));
        }
        return iamClientAuthMethod;
    }

    private string IamClientAuthMethodToInternal(IamClientAuthMethod iamClientAuthMethod)
    {
        if (!IamClientAuthMethodsInternalDictionary.TryGetValue(iamClientAuthMethod, out var clientAuthMethod))
        {
            throw new ArgumentException($"unexpected value of IamClientAuthMethod: {iamClientAuthMethod}", nameof(iamClientAuthMethod));
        }
        return clientAuthMethod;
    }

    private async Task<string> GetNextClientIdAsync() =>
        _Settings.ClientPrefix + (await _ProvisioningDBAccess!.GetNextClientSequenceAsync().ConfigureAwait(false));

    private static T Clone<T>(T cloneObject) 
        where T : class =>
        JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(cloneObject))!;
}
