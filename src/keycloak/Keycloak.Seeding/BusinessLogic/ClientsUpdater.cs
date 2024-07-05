/********************************************************************************
 * Copyright (c) 2023 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Linq;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Factory;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Clients;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.ProtocolMappers;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.RealmsAdmin;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Models;
using System.Runtime.CompilerServices;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.BusinessLogic;

public class ClientsUpdater : IClientsUpdater
{
    private readonly IKeycloakFactory _keycloakFactory;
    private readonly ISeedDataHandler _seedData;

    public ClientsUpdater(IKeycloakFactory keycloakFactory, ISeedDataHandler seedDataHandler)
    {
        _keycloakFactory = keycloakFactory;
        _seedData = seedDataHandler;
    }

    public Task UpdateClients(string keycloakInstanceName, CancellationToken cancellationToken)
    {
        var realm = _seedData.Realm;
        var keycloak = _keycloakFactory.CreateKeycloakClient(keycloakInstanceName);
        return _seedData.SetClientInternalIds(UpdateClientsInternal(keycloak, realm, cancellationToken));
    }

    private async IAsyncEnumerable<(string ClientId, string Id)> UpdateClientsInternal(KeycloakClient keycloak, string realm, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var update in _seedData.Clients)
        {
            if (update.ClientId == null)
                throw new ConflictException($"clientId must not be null {update.Id}");

            var client = (await keycloak.GetClientsAsync(realm, clientId: update.ClientId, cancellationToken: cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None)).SingleOrDefault(x => x.ClientId == update.ClientId);
            if (client == null)
            {
                yield return (update.ClientId, await CreateClient(keycloak, realm, update, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None));
            }
            else
            {
                await UpdateClient(
                    keycloak,
                    realm,
                    client.Id ?? throw new ConflictException($"client.Id must not be null: clientId {update.ClientId}"),
                    client,
                    update,
                    cancellationToken
                ).ConfigureAwait(ConfigureAwaitOptions.None);

                yield return (update.ClientId, client.Id);
            }
        }
    }

    private static async Task<string> CreateClient(KeycloakClient keycloak, string realm, ClientModel update, CancellationToken cancellationToken)
    {
        var result = await keycloak.RealmPartialImportAsync(realm, CreatePartialImportClient(update), cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        if (result.Overwritten != 0 || result.Added != 1 || result.Skipped != 0)
        {
            throw new ConflictException($"PartialImport failed to add client id: {update.Id}, clientId: {update.ClientId}");
        }
        var client = (await keycloak.GetClientsAsync(realm, clientId: update.ClientId, cancellationToken: cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None)).SingleOrDefault(x => x.ClientId == update.ClientId);
        return client?.Id ?? throw new ConflictException($"client.Id must not be null: clientId {update.ClientId}");
    }

    private static async Task UpdateClient(KeycloakClient keycloak, string realm, string idOfClient, Client client, ClientModel seedClient, CancellationToken cancellationToken)
    {
        if (!CompareClient(client, seedClient))
        {
            var updateClient = CreateUpdateClient(client, seedClient);
            await keycloak.UpdateClientAsync(
                realm,
                idOfClient,
                updateClient,
                cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        }

        await UpdateClientProtocollMappers(keycloak, realm, idOfClient, client, seedClient, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        await UpdateDefaultClientScopes(keycloak, realm, idOfClient, client, seedClient, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        await UpdateOptionalClientScopes(keycloak, realm, idOfClient, client, seedClient, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
    }

    private static async Task UpdateClientProtocollMappers(KeycloakClient keycloak, string realm, string clientId, Client client, ClientModel update, CancellationToken cancellationToken)
    {
        var clientProtocolMappers = client.ProtocolMappers ?? Enumerable.Empty<ProtocolMapper>();
        var updateProtocolMappers = update.ProtocolMappers ?? Enumerable.Empty<ProtocolMapperModel>();

        await DeleteObsoleteClientProtocolMappers(keycloak, realm, clientId, clientProtocolMappers, updateProtocolMappers, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        await CreateMissingClientProtocolMappers(keycloak, realm, clientId, clientProtocolMappers, updateProtocolMappers, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        await UpdateExistingClientProtocolMappers(keycloak, realm, clientId, clientProtocolMappers, updateProtocolMappers, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
    }

    private static async Task DeleteObsoleteClientProtocolMappers(KeycloakClient keycloak, string realm, string clientId, IEnumerable<ProtocolMapper> clientProtocolMappers, IEnumerable<ProtocolMapperModel> updateProtocolMappers, CancellationToken cancellationToken)
    {
        foreach (var mapper in clientProtocolMappers.ExceptBy(updateProtocolMappers.Select(x => x.Name), x => x.Name))
        {
            await keycloak.DeleteClientProtocolMapperAsync(
                realm,
                clientId,
                mapper.Id ?? throw new ConflictException($"protocolMapper.Id is null {mapper.Name}"),
                cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        }
    }

    private static async Task CreateMissingClientProtocolMappers(KeycloakClient keycloak, string realm, string clientId, IEnumerable<ProtocolMapper> clientProtocolMappers, IEnumerable<ProtocolMapperModel> updateProtocolMappers, CancellationToken cancellationToken)
    {
        foreach (var update in updateProtocolMappers.ExceptBy(clientProtocolMappers.Select(x => x.Name), x => x.Name))
        {
            await keycloak.CreateClientProtocolMapperAsync(
                realm,
                clientId,
                ProtocolMappersUpdater.CreateProtocolMapper(null, update),
                cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        }
    }

    private static async Task UpdateExistingClientProtocolMappers(KeycloakClient keycloak, string realm, string clientId, IEnumerable<ProtocolMapper> clientProtocolMappers, IEnumerable<ProtocolMapperModel> updateProtocolMappers, CancellationToken cancellationToken)
    {
        foreach (var (mapper, update) in clientProtocolMappers
            .Join(
                updateProtocolMappers,
                x => x.Name,
                x => x.Name,
                (mapper, update) => (Mapper: mapper, Update: update))
            .Where(
                x => !ProtocolMappersUpdater.CompareProtocolMapper(x.Mapper, x.Update)))
        {
            await keycloak.UpdateClientProtocolMapperAsync(
                realm,
                clientId,
                mapper.Id ?? throw new ConflictException($"protocolMapper.Id is null {mapper.Name}"),
                ProtocolMappersUpdater.CreateProtocolMapper(mapper.Id, update),
                cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        }
    }

    private static async Task UpdateOptionalClientScopes(KeycloakClient keycloak, string realm, string clientId, Client client, ClientModel update, CancellationToken cancellationToken)
    {
        var optionalScopes = client.OptionalClientScopes ?? Enumerable.Empty<string>();
        var updateScopes = update.OptionalClientScopes ?? Enumerable.Empty<string>();

        await DeleteObsoleteOptionalClientScopes(keycloak, realm, clientId, optionalScopes, updateScopes, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        await CreateMissingOptionalClientScopes(keycloak, realm, clientId, optionalScopes, updateScopes, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
    }

    private static async Task DeleteObsoleteOptionalClientScopes(KeycloakClient keycloak, string realm, string clientId, IEnumerable<string> optionalScopes, IEnumerable<string> updateScopes, CancellationToken cancellationToken)
    {
        foreach (var scope in optionalScopes.Except(updateScopes))
        {
            await keycloak.DeleteOptionalClientScopeAsync(realm, clientId, scope, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        }
    }

    private static async Task CreateMissingOptionalClientScopes(KeycloakClient keycloak, string realm, string clientId, IEnumerable<string> optionalScopes, IEnumerable<string> updateScopes, CancellationToken cancellationToken)
    {
        foreach (var scope in updateScopes.Except(optionalScopes))
        {
            await keycloak.UpdateOptionalClientScopeAsync(realm, clientId, scope, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        }
    }

    private static async Task UpdateDefaultClientScopes(KeycloakClient keycloak, string realm, string clientId, Client client, ClientModel update, CancellationToken cancellationToken)
    {
        var defaultScopes = client.DefaultClientScopes ?? Enumerable.Empty<string>();
        var updateScopes = update.DefaultClientScopes ?? Enumerable.Empty<string>();

        await DeleteObsoleteDefaultClientScopes(keycloak, realm, clientId, defaultScopes, updateScopes, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        await CreateMissingDefaultClientScopes(keycloak, realm, clientId, defaultScopes, updateScopes, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
    }

    private static async Task DeleteObsoleteDefaultClientScopes(KeycloakClient keycloak, string realm, string clientId, IEnumerable<string> optionalScopes, IEnumerable<string> updateScopes, CancellationToken cancellationToken)
    {
        foreach (var scope in optionalScopes.Except(updateScopes))
        {
            await keycloak.DeleteDefaultClientScopeAsync(realm, clientId, scope, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        }
    }

    private static async Task CreateMissingDefaultClientScopes(KeycloakClient keycloak, string realm, string clientId, IEnumerable<string> optionalScopes, IEnumerable<string> updateScopes, CancellationToken cancellationToken)
    {
        foreach (var scope in updateScopes.Except(optionalScopes))
        {
            await keycloak.UpdateDefaultClientScopeAsync(realm, clientId, scope, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        }
    }

    private static Client CreateUpdateClient(Client? client, ClientModel update) => new()
    {
        // DefaultClientScopes and OptionalClientScopes are not in scope
        Id = client?.Id,
        ClientId = update.ClientId,
        RootUrl = update.RootUrl,
        Name = update.Name,
        Description = update.Description,
        BaseUrl = update.BaseUrl,
        AdminUrl = update.AdminUrl,
        SurrogateAuthRequired = update.SurrogateAuthRequired,
        Enabled = update.Enabled,
        AlwaysDisplayInConsole = update.AlwaysDisplayInConsole,
        ClientAuthenticatorType = update.ClientAuthenticatorType,
        RedirectUris = update.RedirectUris,
        WebOrigins = update.WebOrigins,
        NotBefore = update.NotBefore,
        BearerOnly = update.BearerOnly,
        ConsentRequired = update.ConsentRequired,
        StandardFlowEnabled = update.StandardFlowEnabled,
        ImplicitFlowEnabled = update.ImplicitFlowEnabled,
        DirectAccessGrantsEnabled = update.DirectAccessGrantsEnabled,
        ServiceAccountsEnabled = update.ServiceAccountsEnabled,
        PublicClient = update.PublicClient,
        FrontChannelLogout = update.FrontchannelLogout,
        Protocol = update.Protocol,
        Attributes = update.Attributes?.FilterNotNullValues()?.ToDictionary(),
        AuthenticationFlowBindingOverrides = update.AuthenticationFlowBindingOverrides?.FilterNotNullValues()?.ToDictionary(),
        FullScopeAllowed = update.FullScopeAllowed,
        NodeReregistrationTimeout = update.NodeReRegistrationTimeout,
        Access = update.Access == null
            ? null
            : new ClientAccess
            {
                View = update.Access.View,
                Configure = update.Access.Configure,
                Manage = update.Access.Manage
            },
        AuthorizationServicesEnabled = update.AuthorizationServicesEnabled,
        Secret = update.Secret
    };

    private static bool CompareClient(Client client, ClientModel update) =>
        client.ClientId == update.ClientId &&
        client.RootUrl == update.RootUrl &&
        client.Name == update.Name &&
        client.Description == update.Description &&
        client.BaseUrl == update.BaseUrl &&
        client.AdminUrl == update.AdminUrl &&
        client.SurrogateAuthRequired == update.SurrogateAuthRequired &&
        client.Enabled == update.Enabled &&
        client.AlwaysDisplayInConsole == update.AlwaysDisplayInConsole &&
        client.ClientAuthenticatorType == update.ClientAuthenticatorType &&
        client.RedirectUris.NullOrContentEqual(update.RedirectUris) &&
        client.WebOrigins.NullOrContentEqual(update.WebOrigins) &&
        client.NotBefore == update.NotBefore &&
        client.BearerOnly == update.BearerOnly &&
        client.ConsentRequired == update.ConsentRequired &&
        client.StandardFlowEnabled == update.StandardFlowEnabled &&
        client.ImplicitFlowEnabled == update.ImplicitFlowEnabled &&
        client.DirectAccessGrantsEnabled == update.DirectAccessGrantsEnabled &&
        client.ServiceAccountsEnabled == update.ServiceAccountsEnabled &&
        client.PublicClient == update.PublicClient &&
        client.FrontChannelLogout == update.FrontchannelLogout &&
        client.Protocol == update.Protocol &&
        client.Attributes.NullOrContentEqual(update.Attributes?.FilterNotNullValues()) &&
        client.AuthenticationFlowBindingOverrides.NullOrContentEqual(update.AuthenticationFlowBindingOverrides?.FilterNotNullValues()) &&
        client.FullScopeAllowed == update.FullScopeAllowed &&
        client.NodeReregistrationTimeout == update.NodeReRegistrationTimeout &&
        CompareClientAccess(client.Access, update.Access) &&
        client.AuthorizationServicesEnabled == update.AuthorizationServicesEnabled &&
        client.Secret == update.Secret;

    private static bool CompareClientAccess(ClientAccess? access, ClientAccessModel? updateAccess) =>
        access == null && updateAccess == null ||
        access != null && updateAccess != null &&
        access.Configure == updateAccess.Configure &&
        access.Manage == updateAccess.Manage &&
        access.View == updateAccess.View;

    private static PartialImport CreatePartialImportClient(ClientModel update) =>
        new()
        {
            IfResourceExists = "FAIL",
            Clients = [
                new()
                {
                    Id = update.Id,
                    ClientId = update.ClientId,
                    RootUrl = update.RootUrl,
                    Name = update.Name,
                    Description = update.Description,
                    BaseUrl = update.BaseUrl,
                    AdminUrl = update.AdminUrl,
                    SurrogateAuthRequired = update.SurrogateAuthRequired,
                    Enabled = update.Enabled,
                    AlwaysDisplayInConsole = update.AlwaysDisplayInConsole,
                    ClientAuthenticatorType = update.ClientAuthenticatorType,
                    RedirectUris = update.RedirectUris,
                    WebOrigins = update.WebOrigins,
                    NotBefore = update.NotBefore,
                    BearerOnly = update.BearerOnly,
                    ConsentRequired = update.ConsentRequired,
                    StandardFlowEnabled = update.StandardFlowEnabled,
                    ImplicitFlowEnabled = update.ImplicitFlowEnabled,
                    DirectAccessGrantsEnabled = update.DirectAccessGrantsEnabled,
                    ServiceAccountsEnabled = update.ServiceAccountsEnabled,
                    PublicClient = update.PublicClient,
                    FrontChannelLogout = update.FrontchannelLogout,
                    Protocol = update.Protocol,
                    Attributes = update.Attributes?.FilterNotNullValues()?.ToDictionary(),
                    AuthenticationFlowBindingOverrides = update.AuthenticationFlowBindingOverrides?.FilterNotNullValues()?.ToDictionary(),
                    FullScopeAllowed = update.FullScopeAllowed,
                    NodeReregistrationTimeout = update.NodeReRegistrationTimeout,
                    DefaultClientScopes = update.DefaultClientScopes,
                    OptionalClientScopes = update.OptionalClientScopes,
                    Access = update.Access == null
                        ? null
                        : new ClientAccess
                        {
                            View = update.Access.View,
                            Configure = update.Access.Configure,
                            Manage = update.Access.Manage
                        },
                    AuthorizationServicesEnabled = update.AuthorizationServicesEnabled,
                    Secret = update.Secret
                }
            ]
        };
}
