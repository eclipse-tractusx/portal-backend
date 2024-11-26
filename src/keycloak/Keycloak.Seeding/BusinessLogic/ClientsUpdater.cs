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
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Models;
using System.Runtime.CompilerServices;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.BusinessLogic;

public class ClientsUpdater(IKeycloakFactory keycloakFactory, ISeedDataHandler seedDataHandler)
    : IClientsUpdater
{
    public Task UpdateClients(string keycloakInstanceName, CancellationToken cancellationToken)
    {
        var realm = seedDataHandler.Realm;
        var keycloak = keycloakFactory.CreateKeycloakClient(keycloakInstanceName);
        var seederConfig = seedDataHandler.GetSpecificConfiguration(ConfigurationKey.Clients);
        var clientScopesSeederConfig = seedDataHandler.GetSpecificConfiguration(ConfigurationKey.ClientScopes);

        return seedDataHandler.SetClientInternalIds(UpdateClientsInternal(keycloak, realm, seederConfig, clientScopesSeederConfig, cancellationToken));
    }

    private async IAsyncEnumerable<(string ClientId, string Id)> UpdateClientsInternal(KeycloakClient keycloak, string realm, KeycloakSeederConfigModel seederConfig, KeycloakSeederConfigModel clientScopesSeederConfig, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var clientScopes = await keycloak.GetClientScopesAsync(realm, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        string GetClientScopeId(string scope) => clientScopes.SingleOrDefault(x => x.Name == scope)?.Id ?? throw new ConflictException($"id of clientScope {scope} is undefined");

        foreach (var update in seedDataHandler.Clients)
        {
            if (update.ClientId == null)
                throw new ConflictException($"clientId must not be null {update.Id}");

            var client = (await keycloak.GetClientsAsync(realm, clientId: update.ClientId, cancellationToken: cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None)).SingleOrDefault(x => x.ClientId == update.ClientId);
            if (client == null)
            {
                if (!seederConfig.ModificationAllowed(ModificationType.Create, update.ClientId))
                {
                    continue;
                }

                client = await CreateClient(keycloak, realm, update, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
            }
            else
            {
                if (seederConfig.ModificationAllowed(ModificationType.Update, update.ClientId))
                {
                    await UpdateClient(
                    keycloak,
                    realm,
                    client.Id ?? throw new ConflictException($"client.Id must not be null: clientId {update.ClientId}"),
                    client,
                    update,
                    cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
                }

                await UpdateClientProtocolMappers(
                    keycloak,
                    realm,
                    client.Id ?? throw new ConflictException($"client.Id must not be null: clientId {update.ClientId}"),
                    client,
                    update,
                    seederConfig,
                    cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
            }

            await UpdateDefaultClientScopes(
                keycloak,
                realm,
                client.Id ?? throw new ConflictException($"client.Id must not be null: clientId {update.ClientId}"),
                client,
                update,
                GetClientScopeId,
                clientScopesSeederConfig,
                cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);

            await UpdateOptionalClientScopes(
                keycloak,
                realm,
                client.Id,
                client,
                update,
                GetClientScopeId,
                clientScopesSeederConfig,
                cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);

            yield return (update.ClientId, client.Id);
        }
    }

    private static async Task<Client> CreateClient(KeycloakClient keycloak, string realm, ClientModel update, CancellationToken cancellationToken)
    {
        var result = await keycloak.RealmPartialImportAsync(realm, CreatePartialImportClient(update), cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        if (result.Overwritten != 0 || result.Added != 1 || result.Skipped != 0)
        {
            throw new ConflictException($"PartialImport failed to add client id: {update.Id}, clientId: {update.ClientId}");
        }

        var client = (await keycloak.GetClientsAsync(realm, clientId: update.ClientId, cancellationToken: cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None)).SingleOrDefault(x => x.ClientId == update.ClientId);
        return client ?? throw new ConflictException($"failed to read newly created client {update.ClientId}");
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
    }

    private static async Task UpdateClientProtocolMappers(KeycloakClient keycloak, string realm, string clientId, Client client, ClientModel update, KeycloakSeederConfigModel seederConfig, CancellationToken cancellationToken)
    {
        var clientProtocolMappers = client.ProtocolMappers ?? Enumerable.Empty<ProtocolMapper>();
        var updateProtocolMappers = update.ProtocolMappers ?? Enumerable.Empty<ProtocolMapperModel>();
        if (client.ClientId == null)
            throw new ConflictException("client.ClientId must never be null");

        foreach (var mapperId in clientProtocolMappers
                     .Where(x => seederConfig.ModificationAllowed(client.ClientId, ConfigurationKey.ClientProtocolMapper, ModificationType.Delete, x.Name))
                     .ExceptBy(updateProtocolMappers.Select(x => x.Name), x => x.Name)
                     .Select(x => x.Id ?? throw new ConflictException($"protocolMapper.Id is null {x.Name}")))
        {
            await keycloak.DeleteClientProtocolMapperAsync(realm, clientId, mapperId, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        }

        foreach (var mapper in updateProtocolMappers
                     .Where(x => seederConfig.ModificationAllowed(client.ClientId, ConfigurationKey.ClientProtocolMapper, ModificationType.Create, x.Name))
                     .ExceptBy(clientProtocolMappers.Select(x => x.Name), x => x.Name)
                     .Select(x => ProtocolMappersUpdater.CreateProtocolMapper(null, x)))
        {
            await keycloak.CreateClientProtocolMapperAsync(realm, clientId, mapper, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        }

        foreach (var (mapperId, mapper) in clientProtocolMappers
            .Where(x => seederConfig.ModificationAllowed(client.ClientId, ConfigurationKey.ClientProtocolMapper, ModificationType.Update, x.Name))
            .Join(
                updateProtocolMappers,
                x => x.Name,
                x => x.Name,
                (mapper, update) => (Mapper: mapper, Update: update))
            .Where(x => !ProtocolMappersUpdater.CompareProtocolMapper(x.Mapper, x.Update))
            .Select(x => (
                x.Mapper.Id ?? throw new ConflictException($"protocolMapper.Id is null {x.Mapper.Name}"),
                ProtocolMappersUpdater.CreateProtocolMapper(x.Mapper.Id, x.Update))))
        {
            await keycloak.UpdateClientProtocolMapperAsync(realm, clientId, mapperId, mapper, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        }
    }

    private static async Task UpdateOptionalClientScopes(KeycloakClient keycloak, string realm, string idOfClient, Client client, ClientModel update, Func<string, string> getClientScopeId, KeycloakSeederConfigModel seederConfig, CancellationToken cancellationToken)
    {
        var optionalScopes = client.OptionalClientScopes ?? Enumerable.Empty<string>();
        var updateScopes = update.OptionalClientScopes ?? Enumerable.Empty<string>();

        foreach (var scopeId in optionalScopes
                     .Where(x => seederConfig.ModificationAllowed(ModificationType.Delete, x))
                     .Except(updateScopes)
                     .Select(getClientScopeId))
        {
            await keycloak.DeleteOptionalClientScopeAsync(realm, idOfClient, scopeId, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        }

        foreach (var scopeId in updateScopes
                     .Where(x => seederConfig.ModificationAllowed(ModificationType.Update, x))
                     .Except(optionalScopes)
                     .Select(getClientScopeId))
        {
            await keycloak.UpdateOptionalClientScopeAsync(realm, idOfClient, scopeId, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        }
    }

    private static async Task UpdateDefaultClientScopes(KeycloakClient keycloak, string realm, string idOfClient, Client client, ClientModel update, Func<string, string> getClientScopeId, KeycloakSeederConfigModel seederConfig, CancellationToken cancellationToken)
    {
        var defaultScopes = client.DefaultClientScopes ?? Enumerable.Empty<string>();
        var updateScopes = update.DefaultClientScopes ?? Enumerable.Empty<string>();

        foreach (var scopeId in defaultScopes
                     .Where(x => seederConfig.ModificationAllowed(ModificationType.Delete, x))
                     .Except(updateScopes)
                     .Select(getClientScopeId))
        {
            await keycloak.DeleteDefaultClientScopeAsync(realm, idOfClient, scopeId, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        }

        foreach (var scopeId in updateScopes
                     .Where(x => seederConfig.ModificationAllowed(ModificationType.Update, x))
                     .Except(defaultScopes)
                     .Select(getClientScopeId))
        {
            await keycloak.UpdateDefaultClientScopeAsync(realm, idOfClient, scopeId, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
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
        Attributes = update.Attributes?.FilterNotNullValues().ToDictionary(),
        AuthenticationFlowBindingOverrides = update.AuthenticationFlowBindingOverrides?.FilterNotNullValues().ToDictionary(),
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
                    Attributes = update.Attributes?.FilterNotNullValues().ToDictionary(),
                    AuthenticationFlowBindingOverrides = update.AuthenticationFlowBindingOverrides?.FilterNotNullValues().ToDictionary(),
                    FullScopeAllowed = update.FullScopeAllowed,
                    NodeReregistrationTimeout = update.NodeReRegistrationTimeout,
                    ProtocolMappers = update.ProtocolMappers?.Select(x => ProtocolMappersUpdater.CreateProtocolMapper(x.Id, x)),
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
