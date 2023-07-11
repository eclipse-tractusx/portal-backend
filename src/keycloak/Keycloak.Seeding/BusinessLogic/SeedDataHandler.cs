/********************************************************************************
 * Copyright (c) 2021, 2023 BMW Group AG
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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Models;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.BusinessLogic;

public class SeedDataHandler : ISeedDataHandler
{
    private static readonly JsonSerializerOptions Options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, IncludeFields = true, PropertyNameCaseInsensitive = false };    
    private KeycloakRealm? jsonRealm;
    private IReadOnlyDictionary<string, string>? _idOfClients;

    public async Task Import(string path)
    {
        using (var stream = File.OpenRead(path))
        {
            jsonRealm = await JsonSerializer.DeserializeAsync<KeycloakRealm>(stream, Options).ConfigureAwait(false) ?? throw new ConfigurationException($"cannot deserialize realm from {path}");
        }
    }

    public string Realm
    {
        get => jsonRealm?.Realm ?? throw new ConflictException("realm must not be null");
    }

    public KeycloakRealm KeycloakRealm
    {
        get => jsonRealm ?? throw new InvalidOperationException("Import has not been called");
    }

    public IEnumerable<ClientModel> Clients
    {
        get => jsonRealm?.Clients ?? Enumerable.Empty<ClientModel>();
    }

    public IReadOnlyDictionary<string, IEnumerable<RoleModel>> ClientRoles
    {
        get => jsonRealm?.Roles?.Client?.ToDictionary(x => x.Key, x => x.Value.AsEnumerable()) ?? Enumerable.Empty<(string, IEnumerable<RoleModel>)>().ToDictionary(x => x.Item1, x => x.Item2);
    }

    public IReadOnlyList<RoleModel> RealmRoles
    {
        get => jsonRealm?.Roles?.Realm ?? Enumerable.Empty<RoleModel>().ToList();
    }

    public IEnumerable<IdentityProviderModel> IdentityProviders
    {
        get => jsonRealm?.IdentityProviders ?? Enumerable.Empty<IdentityProviderModel>();
    }
    
    public IEnumerable<IdentityProviderMapperModel> IdentityProviderMappers
    {
        get => jsonRealm?.IdentityProviderMappers ?? Enumerable.Empty<IdentityProviderMapperModel>();
    }
    
    public IReadOnlyDictionary<string, string> ClientsDictionary
    {
        set => _idOfClients = value;
    }

    public string GetIdOfClient(string clientId) =>
        (_idOfClients ?? throw new InvalidOperationException("ClientsDictionary has not been set before"))
            .TryGetValue(clientId, out var id)
                ? id ?? throw new ConflictException($"id of client must not be null {clientId}")
                : throw new ConflictException($"clientId is unknown {clientId}");
}
