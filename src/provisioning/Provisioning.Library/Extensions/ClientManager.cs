/********************************************************************************
 * Copyright (c) 2021, 2023 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.Keycloak.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Clients;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.ProtocolMappers;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;

public partial class ProvisioningManager
{
    private static readonly IReadOnlyDictionary<string, IamClientAuthMethod> CredentialTypesIamClientAuthMethodDictionary = new Dictionary<string, IamClientAuthMethod>()
    {
        { "jwt", IamClientAuthMethod.JWT },
        { "secret", IamClientAuthMethod.SECRET },
        { "x509", IamClientAuthMethod.X509 },
        { "secret-jwt", IamClientAuthMethod.SECRET_JWT }
    };

    private static readonly IReadOnlyDictionary<IamClientAuthMethod, string> IamClientAuthMethodsInternalDictionary = new Dictionary<IamClientAuthMethod, string>()
    {
        { IamClientAuthMethod.JWT, "client-jwt" },
        { IamClientAuthMethod.SECRET, "client-secret" },
        { IamClientAuthMethod.X509, "client-x509" },
        { IamClientAuthMethod.SECRET_JWT, "client-secret-jwt" }
    };

    public async Task<string> UpdateCentralClientAsync(string clientId, ClientConfigData config)
    {
        var client = await GetCentralClientAsync(clientId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (client.Id == null)
            throw new KeycloakEntityConflictException($"id of client {clientId} is null");
        client.Name = config.Name;
        client.ClientAuthenticatorType = IamClientAuthMethodToInternal(config.IamClientAuthMethod);
        await _CentralIdp.UpdateClientAsync(_Settings.CentralRealm, client.Id, client).ConfigureAwait(ConfigureAwaitOptions.None);
        return client.Id;
    }

    public async Task DeleteCentralClientAsync(string clientId)
    {
        var idOfClient = await GetIdOfCentralClientAsync(clientId).ConfigureAwait(ConfigureAwaitOptions.None) ?? throw new KeycloakEntityNotFoundException($"client {clientId} not found in keycloak");
        await _CentralIdp.DeleteClientAsync(_Settings.CentralRealm, idOfClient).ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public async Task UpdateClient(string clientId, string url, string redirectUrl)
    {
        var idOfClient = await GetIdOfCentralClientAsync(clientId).ConfigureAwait(ConfigureAwaitOptions.None);

        var client = await _CentralIdp.GetClientAsync(_Settings.CentralRealm, idOfClient).ConfigureAwait(ConfigureAwaitOptions.None);
        client.BaseUrl = url;
        client.RedirectUris = Enumerable.Repeat(redirectUrl, 1);
        await _CentralIdp.UpdateClientAsync(_Settings.CentralRealm, idOfClient, client).ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public async Task EnableClient(string clientId)
    {
        var idOfClient = await GetIdOfCentralClientAsync(clientId).ConfigureAwait(ConfigureAwaitOptions.None);
        var client = await _CentralIdp.GetClientAsync(_Settings.CentralRealm, idOfClient).ConfigureAwait(ConfigureAwaitOptions.None);
        client.Enabled = true;
        await _CentralIdp.UpdateClientAsync(_Settings.CentralRealm, idOfClient, client).ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public async Task<ClientAuthData> GetCentralClientAuthDataAsync(string internalClientId)
    {
        var credentials = await _CentralIdp.GetClientSecretAsync(_Settings.CentralRealm, internalClientId).ConfigureAwait(ConfigureAwaitOptions.None);
        return new ClientAuthData(
            CredentialsTypeToIamClientAuthMethod(credentials.Type))
        {
            Secret = credentials.Value
        };
    }

    public async Task<ClientAuthData> ResetCentralClientAuthDataAsync(string clientId)
    {
        var idOfClient = await GetIdOfCentralClientAsync(clientId).ConfigureAwait(ConfigureAwaitOptions.None);
        var credentials = await _CentralIdp.GenerateClientSecretAsync(_Settings.CentralRealm, idOfClient).ConfigureAwait(ConfigureAwaitOptions.None);
        return new ClientAuthData(
            CredentialsTypeToIamClientAuthMethod(credentials.Type))
        {
            Secret = credentials.Value
        };
    }

    public async Task<string> GetIdOfCentralClientAsync(string clientId) =>
        (await _CentralIdp.GetClientsAsync(_Settings.CentralRealm, clientId: clientId, viewableOnly: true).ConfigureAwait(ConfigureAwaitOptions.None))
            .SingleOrDefault()?.Id ?? throw new KeycloakEntityNotFoundException($"clientId {clientId} not found in central keycloak");

    private async Task<Client> GetCentralClientAsync(string clientId) =>
        (await _CentralIdp.GetClientsAsync(_Settings.CentralRealm, clientId: clientId, viewableOnly: true).ConfigureAwait(ConfigureAwaitOptions.None))
            .SingleOrDefault() ?? throw new KeycloakEntityNotFoundException($"clientId {clientId} not found in central keycloak");

    private async Task<string> CreateCentralOIDCClientAsync(string clientId, string redirectUri, string? baseUrl, bool enabled)
    {
        var newClient = Clone(_Settings.CentralOIDCClient);
        newClient.ClientId = clientId;
        newClient.RedirectUris = Enumerable.Repeat(redirectUri, 1);
        newClient.Enabled = enabled;
        if (!string.IsNullOrEmpty(baseUrl))
        {
            newClient.BaseUrl = baseUrl;
        }
        return await _CentralIdp.CreateClientAndRetrieveClientIdAsync(_Settings.CentralRealm, newClient).ConfigureAwait(ConfigureAwaitOptions.None) ?? throw new KeycloakNoSuccessException($"failed to create new client {clientId} in central realm");
    }

    private Task CreateCentralOIDCClientAudienceMapperAsync(string internalClientId, string clientAudienceId) =>
        _CentralIdp.CreateClientProtocolMapperAsync(_Settings.CentralRealm, internalClientId, new ProtocolMapper
        {
            Name = $"{clientAudienceId}-mapper",
            Protocol = "openid-connect",
            _ProtocolMapper = "oidc-audience-mapper",
            ConsentRequired = false,
            Config = new Config
            {
                IncludedClientAudience = clientAudienceId,
                IdTokenClaim = "false",
                AccessTokenClaim = "true",
            }
        });

    private static IamClientAuthMethod CredentialsTypeToIamClientAuthMethod(string clientAuthMethod)
    {
        if (!CredentialTypesIamClientAuthMethodDictionary.TryGetValue(clientAuthMethod, out var iamClientAuthMethod))
        {
            throw new ArgumentException($"unexpected value of clientAuthMethod: {clientAuthMethod}", nameof(clientAuthMethod));
        }
        return iamClientAuthMethod;
    }

    private static string IamClientAuthMethodToInternal(IamClientAuthMethod iamClientAuthMethod)
    {
        if (!IamClientAuthMethodsInternalDictionary.TryGetValue(iamClientAuthMethod, out var clientAuthMethod))
        {
            throw new ArgumentException($"unexpected value of IamClientAuthMethod: {iamClientAuthMethod}", nameof(iamClientAuthMethod));
        }
        return clientAuthMethod;
    }

    private async Task<string> GetNextClientIdAsync() =>
        _Settings.ClientPrefix + (await _ProvisioningDBAccess!.GetNextClientSequenceAsync().ConfigureAwait(ConfigureAwaitOptions.None));
}
