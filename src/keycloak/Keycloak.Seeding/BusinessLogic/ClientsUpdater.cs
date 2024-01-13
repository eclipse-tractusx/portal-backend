/********************************************************************************
 * Copyright (c) 2021, 2024 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Factory;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.Clients;
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
            var client = (await keycloak.GetClientsAsync(realm, clientId: update.ClientId, cancellationToken: cancellationToken).ConfigureAwait(false)).SingleOrDefault(x => x.ClientId == update.ClientId);
            if (client == null)
            {
                var id = await keycloak.CreateClientAndRetrieveClientIdAsync(realm, CreateUpdateClient(null, update), cancellationToken).ConfigureAwait(false);
                if (id == null)
                    throw new KeycloakNoSuccessException($"creation of client {update.ClientId} did not return the expected result");

                // load newly created client as keycloak may create default protocolmappers on client-creation
                client = await keycloak.GetClientAsync(realm, id).ConfigureAwait(false);
            }

            if (client.Id == null)
                throw new ConflictException($"client.Id must not be null: clientId {update.ClientId}");

            if (!CompareClient(client, update))
            {
                var updateClient = CreateUpdateClient(client, update);
                await keycloak.UpdateClientAsync(
                    realm,
                    client.Id,
                    updateClient,
                    cancellationToken).ConfigureAwait(false);
            }

            await UpdateClientProtocollMappers(keycloak, realm, client.Id, client, update, cancellationToken).ConfigureAwait(false);

            yield return (update.ClientId, client.Id);
        }
    }

    private static async Task UpdateClientProtocollMappers(KeycloakClient keycloak, string realm, string clientId, Client client, ClientModel update, CancellationToken cancellationToken)
    {
        var clientProtocolMappers = client.ProtocolMappers ?? Enumerable.Empty<ClientProtocolMapper>();
        var updateProtocolMappers = update.ProtocolMappers ?? Enumerable.Empty<ProtocolMapperModel>();

        await DeleteObsoleteClientProtocolMappers(keycloak, realm, clientId, clientProtocolMappers, updateProtocolMappers, cancellationToken).ConfigureAwait(false);
        await CreateMissingClientProtocolMappers(keycloak, realm, clientId, clientProtocolMappers, updateProtocolMappers, cancellationToken).ConfigureAwait(false);
        await UpdateExistingClientProtocolMappers(keycloak, realm, clientId, clientProtocolMappers, updateProtocolMappers, cancellationToken).ConfigureAwait(false);
    }

    private static async Task DeleteObsoleteClientProtocolMappers(KeycloakClient keycloak, string realm, string clientId, IEnumerable<ClientProtocolMapper> clientProtocolMappers, IEnumerable<ProtocolMapperModel> updateProtocolMappers, CancellationToken cancellationToken)
    {
        foreach (var mapper in clientProtocolMappers.ExceptBy(updateProtocolMappers.Select(x => x.Name), x => x.Name))
        {
            await keycloak.DeleteClientProtocolMapperAsync(
                realm,
                clientId,
                mapper.Id ?? throw new ConflictException($"protocolMapper.Id is null {mapper.Name}"),
                cancellationToken).ConfigureAwait(false);
        }
    }

    private static async Task CreateMissingClientProtocolMappers(KeycloakClient keycloak, string realm, string clientId, IEnumerable<ClientProtocolMapper> clientProtocolMappers, IEnumerable<ProtocolMapperModel> updateProtocolMappers, CancellationToken cancellationToken)
    {
        foreach (var update in updateProtocolMappers.ExceptBy(clientProtocolMappers.Select(x => x.Name), x => x.Name))
        {
            await keycloak.CreateClientProtocolMapperAsync(
                realm,
                clientId,
                ProtocolMappersUpdater.CreateProtocolMapper(null, update),
                cancellationToken).ConfigureAwait(false);
        }
    }

    private static async Task UpdateExistingClientProtocolMappers(KeycloakClient keycloak, string realm, string clientId, IEnumerable<ClientProtocolMapper> clientProtocolMappers, IEnumerable<ProtocolMapperModel> updateProtocolMappers, CancellationToken cancellationToken)
    {
        foreach (var (mapper, update) in clientProtocolMappers
            .Join(
                updateProtocolMappers,
                x => x.Name,
                x => x.Name,
                (mapper, update) => (Mapper: mapper, Update: update))
            .Where(
                x => !CompareClientProtocolMapper(x.Mapper, x.Update)))
        {
            await keycloak.UpdateClientProtocolMapperAsync(
                realm,
                clientId,
                mapper.Id ?? throw new ConflictException($"protocolMapper.Id is null {mapper.Name}"),
                ProtocolMappersUpdater.CreateProtocolMapper(mapper.Id, update),
                cancellationToken).ConfigureAwait(false);
        }
    }

    private static Client CreateUpdateClient(Client? client, ClientModel update) => new() // secret is not updated as it cannot be read via the keycloak api
    {
        Id = client?.Id,
        ClientId = update.ClientId,
        RootUrl = client?.RootUrl ?? update.RootUrl, // only set the root url if no url is already set
        Name = update.Name,
        Description = update.Description,
        BaseUrl = client?.BaseUrl ?? update.BaseUrl, // only set the base url if no url is already set
        SurrogateAuthRequired = update.SurrogateAuthRequired,
        Enabled = update.Enabled,
        AlwaysDisplayInConsole = update.AlwaysDisplayInConsole,
        ClientAuthenticatorType = update.ClientAuthenticatorType,
        RedirectUris = client == null || client.RedirectUris.IsNullOrEmpty() ? update.RedirectUris : client.RedirectUris, // only set the redirect uris if there aren't any set
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
        Attributes = update.Attributes?.ToDictionary(x => x.Key, x => x.Value),
        AuthenticationFlowBindingOverrides = update.AuthenticationFlowBindingOverrides?.ToDictionary(x => x.Key, x => x.Value),
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
        AuthorizationServicesEnabled = update.AuthorizationServicesEnabled
    };

    private static bool CompareClient(Client client, ClientModel update) => // secret is not compared as it cannot be read via the keycloak api
        client.ClientId == update.ClientId &&
        client.RootUrl == update.RootUrl &&
        client.Name == update.Name &&
        client.Description == update.Description &&
        client.BaseUrl == update.BaseUrl &&
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
        client.Attributes.NullOrContentEqual(update.Attributes) &&
        client.AuthenticationFlowBindingOverrides.NullOrContentEqual(update.AuthenticationFlowBindingOverrides) &&
        client.FullScopeAllowed == update.FullScopeAllowed &&
        client.NodeReregistrationTimeout == update.NodeReRegistrationTimeout &&
        client.DefaultClientScopes.NullOrContentEqual(update.DefaultClientScopes) &&
        client.OptionalClientScopes.NullOrContentEqual(update.OptionalClientScopes) &&
        CompareClientAccess(client.Access, update.Access) &&
        client.AuthorizationServicesEnabled == update.AuthorizationServicesEnabled;

    private static bool CompareClientAccess(ClientAccess? access, ClientAccessModel? updateAccess) =>
        access == null && updateAccess == null ||
        access != null && updateAccess != null &&
        access.Configure == updateAccess.Configure &&
        access.Manage == updateAccess.Manage &&
        access.View == updateAccess.View;

    private static bool CompareClientProtocolMapper(ClientProtocolMapper mapper, ProtocolMapperModel update) =>
        mapper.Name == update.Name &&
        mapper.Protocol == update.Protocol &&
        mapper.ProtocolMapper == update.ProtocolMapper &&
        mapper.ConsentRequired == update.ConsentRequired &&
        (mapper.Config == null && update.Config == null ||
        mapper.Config != null && update.Config != null &&
        CompareClientProtocolMapperConfig(mapper.Config, update.Config));

    private static bool CompareClientProtocolMapperConfig(ClientConfig config, IReadOnlyDictionary<string, string> update) =>
        config.UserInfoTokenClaim == update.GetValueOrDefault("userinfo.token.claim") &&
        config.UserAttribute == update.GetValueOrDefault("user.attribute") &&
        config.IdTokenClaim == update.GetValueOrDefault("id.token.claim") &&
        config.AccessTokenClaim == update.GetValueOrDefault("access.token.claim") &&
        config.ClaimName == update.GetValueOrDefault("claim.name") &&
        config.JsonTypelabel == update.GetValueOrDefault("jsonType.label") &&
        config.FriendlyName == update.GetValueOrDefault("friendly.name") &&
        config.AttributeName == update.GetValueOrDefault("attribute.name");
}
