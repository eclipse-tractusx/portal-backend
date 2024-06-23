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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Models;
using System.Collections.Immutable;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.BusinessLogic;

public class SeedDataHandler : ISeedDataHandler
{
    private readonly IEnumerable<KeycloakRealmSettings>? _keycloakRealms;

    public SeedDataHandler(IOptions<KeycloakSeederSettings> options)
    {
        _keycloakRealms = options.Value.Realms;
    }

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        IncludeFields = true,
        PropertyNameCaseInsensitive = false
    };

    private KeycloakRealm? _jsonRealm;
    private IReadOnlyDictionary<string, string>? _idOfClients;

    public async Task Import(string path, CancellationToken cancellationToken)
    {
        KeycloakRealm jsonRealm;
        using (var stream = File.OpenRead(path))
        {
            jsonRealm = await JsonSerializer.DeserializeAsync<KeycloakRealm>(stream, Options, cancellationToken)
                                    .ConfigureAwait(false) ?? throw new ConfigurationException($"cannot deserialize realm from {path}");
        }
        var configRealm = _keycloakRealms?.SingleOrDefault(x => x.Realm == (jsonRealm.Realm ?? throw new ConflictException("realm must not be null")));
        _jsonRealm = configRealm == null
            ? jsonRealm
            : jsonRealm.Merge(configRealm.ToModel());

        _idOfClients = null;
    }

    public string Realm
    {
        get => _jsonRealm?.Realm ?? throw new ConflictException("realm must not be null");
    }

    public KeycloakRealm KeycloakRealm
    {
        get => _jsonRealm ?? throw new InvalidOperationException("Import has not been called");
    }

    public IEnumerable<ClientModel> Clients
    {
        get => _jsonRealm?.Clients ?? Enumerable.Empty<ClientModel>();
    }

    public IReadOnlyDictionary<string, IEnumerable<RoleModel>> ClientRoles
    {
        get => _jsonRealm?.Roles?.Client ?? Enumerable.Empty<(string, IEnumerable<RoleModel>)>()
            .ToImmutableDictionary(x => x.Item1, x => x.Item2);
    }

    public IEnumerable<RoleModel> RealmRoles
    {
        get => _jsonRealm?.Roles?.Realm ?? Enumerable.Empty<RoleModel>();
    }

    public IEnumerable<IdentityProviderModel> IdentityProviders
    {
        get => _jsonRealm?.IdentityProviders ?? Enumerable.Empty<IdentityProviderModel>();
    }

    public IEnumerable<IdentityProviderMapperModel> IdentityProviderMappers
    {
        get => _jsonRealm?.IdentityProviderMappers ?? Enumerable.Empty<IdentityProviderMapperModel>();
    }

    public IEnumerable<UserModel> Users
    {
        get => _jsonRealm?.Users ?? Enumerable.Empty<UserModel>();
    }

    public IEnumerable<AuthenticationFlowModel> TopLevelCustomAuthenticationFlows
    {
        get => _jsonRealm?.AuthenticationFlows?.Where(x => (x.TopLevel ?? false) && !(x.BuiltIn ?? false)) ??
               Enumerable.Empty<AuthenticationFlowModel>();
    }

    public IEnumerable<ClientScopeModel> ClientScopes
    {
        get => _jsonRealm?.ClientScopes ?? Enumerable.Empty<ClientScopeModel>();
    }

    public IReadOnlyDictionary<string, string> ClientsDictionary
    {
        get => _idOfClients ?? throw new InvalidOperationException("ClientInternalIds have not been set");
    }

    public IReadOnlyDictionary<string, IEnumerable<ClientScopeMappingModel>> ClientScopeMappings
    {
        get => _jsonRealm?.ClientScopeMappings ?? Enumerable.Empty<(string, IEnumerable<ClientScopeMappingModel>)>()
            .ToImmutableDictionary(x => x.Item1, x => x.Item2);
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
        _jsonRealm?.AuthenticationFlows?.SingleOrDefault(x => x.Alias == (alias ?? throw new ConflictException("alias is null"))) ?? throw new ConflictException($"authenticationFlow {alias} does not exist in seeding-data");

    public IEnumerable<AuthenticationExecutionModel> GetAuthenticationExecutions(string? alias) =>
        GetAuthenticationFlow(alias).AuthenticationExecutions ?? Enumerable.Empty<AuthenticationExecutionModel>();

    public AuthenticatorConfigModel GetAuthenticatorConfig(string? alias) =>
        _jsonRealm?.AuthenticatorConfig?.SingleOrDefault(x => x.Alias == (alias ?? throw new ConflictException("alias is null"))) ?? throw new ConflictException($"authenticatorConfig {alias} does not exist");
}
