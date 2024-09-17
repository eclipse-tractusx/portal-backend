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

using Org.Eclipse.TractusX.Portal.Backend.Framework.Async;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Linq;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Models;
using System.Collections.Immutable;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.BusinessLogic;

public class SeedDataHandler : ISeedDataHandler
{
    public SeedDataHandler()
    {
    }

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        IncludeFields = true,
        PropertyNameCaseInsensitive = false
    };

    private KeycloakRealm? _keycloakRealm;
    private IReadOnlyDictionary<string, string>? _idOfClients;

    public async Task Import(KeycloakRealmSettings realmSettings, CancellationToken cancellationToken)
    {
        _keycloakRealm = (await realmSettings.DataPathes
            .AggregateAsync(
                new KeycloakRealm(),
                async (importRealm, path) => importRealm.Merge(await ReadJsonRealm(path, realmSettings.Realm, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None)),
                cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None))
            .Merge(realmSettings.ToModel());

        _idOfClients = null;
    }

    private static async Task<KeycloakRealm> ReadJsonRealm(string path, string realm, CancellationToken cancellationToken)
    {
        KeycloakRealm jsonRealm;
        using (var stream = File.OpenRead(path))
        {
            jsonRealm = await JsonSerializer.DeserializeAsync<KeycloakRealm>(stream, Options, cancellationToken)
                                    .ConfigureAwait(false) ?? throw new ConfigurationException($"cannot deserialize realm from {path}");
        }
        if (jsonRealm.Realm != null && jsonRealm.Realm != realm)
            throw new ConfigurationException($"json realm {jsonRealm.Realm} doesn't match the configured realm: {realm}");

        return jsonRealm;
    }

    public string Realm
    {
        get => _keycloakRealm?.Realm ?? throw new ConflictException("realm must not be null");
    }

    public KeycloakRealm KeycloakRealm
    {
        get => _keycloakRealm ?? throw new InvalidOperationException("Import has not been called");
    }

    public IEnumerable<ClientModel> Clients
    {
        get => _keycloakRealm?.Clients ?? Enumerable.Empty<ClientModel>();
    }

    public IEnumerable<(string ClientId, IEnumerable<RoleModel> RoleModels)> ClientRoles
    {
        get => _keycloakRealm?.Roles?.Client?.FilterNotNullValues().Select(x => (x.Key, x.Value)) ?? Enumerable.Empty<(string, IEnumerable<RoleModel>)>();
    }

    public IEnumerable<RoleModel> RealmRoles
    {
        get => _keycloakRealm?.Roles?.Realm ?? Enumerable.Empty<RoleModel>();
    }

    public IEnumerable<IdentityProviderModel> IdentityProviders
    {
        get => _keycloakRealm?.IdentityProviders ?? Enumerable.Empty<IdentityProviderModel>();
    }

    public IEnumerable<IdentityProviderMapperModel> IdentityProviderMappers
    {
        get => _keycloakRealm?.IdentityProviderMappers ?? Enumerable.Empty<IdentityProviderMapperModel>();
    }

    public IEnumerable<UserModel> Users
    {
        get => _keycloakRealm?.Users ?? Enumerable.Empty<UserModel>();
    }

    public IEnumerable<AuthenticationFlowModel> TopLevelCustomAuthenticationFlows
    {
        get => _keycloakRealm?.AuthenticationFlows?.Where(x => (x.TopLevel ?? false) && !(x.BuiltIn ?? false)) ??
               Enumerable.Empty<AuthenticationFlowModel>();
    }

    public IEnumerable<ClientScopeModel> ClientScopes
    {
        get => _keycloakRealm?.ClientScopes ?? Enumerable.Empty<ClientScopeModel>();
    }

    public IReadOnlyDictionary<string, string> ClientsDictionary
    {
        get => _idOfClients ?? throw new InvalidOperationException("ClientInternalIds have not been set");
    }

    public IEnumerable<(string ClientId, IEnumerable<ClientScopeMappingModel> ClientScopeMappingModels)> ClientScopeMappings
    {
        get => _keycloakRealm?.ClientScopeMappings?.FilterNotNullValues().Select(x => (x.Key, x.Value)) ?? Enumerable.Empty<(string, IEnumerable<ClientScopeMappingModel>)>();
    }

    public async Task SetClientInternalIds(IAsyncEnumerable<(string ClientId, string Id)> clientInternalIds)
    {
        var clientIds = new Dictionary<string, string>();
        await foreach (var (clientId, id) in clientInternalIds.ConfigureAwait(false))
        {
            clientIds[clientId] = id;
        }
        _idOfClients = clientIds.ToImmutableDictionary();
    }

    public string GetIdOfClient(string clientId) =>
        (_idOfClients ?? throw new InvalidOperationException("ClientInternalIds have not been set"))
            .GetValueOrDefault(clientId) ?? throw new ConflictException($"clientId is unknown or id of client is null {clientId}");

    public AuthenticationFlowModel GetAuthenticationFlow(string? alias) =>
        _keycloakRealm?.AuthenticationFlows?.SingleOrDefault(x => x.Alias == (alias ?? throw new ConflictException("alias is null"))) ?? throw new ConflictException($"authenticationFlow {alias} does not exist in seeding-data");

    public IEnumerable<AuthenticationExecutionModel> GetAuthenticationExecutions(string? alias) =>
        GetAuthenticationFlow(alias).AuthenticationExecutions ?? Enumerable.Empty<AuthenticationExecutionModel>();

    public AuthenticatorConfigModel GetAuthenticatorConfig(string? alias) =>
        _keycloakRealm?.AuthenticatorConfig?.SingleOrDefault(x => x.Alias == (alias ?? throw new ConflictException("alias is null"))) ?? throw new ConflictException($"authenticatorConfig {alias} does not exist");
}
