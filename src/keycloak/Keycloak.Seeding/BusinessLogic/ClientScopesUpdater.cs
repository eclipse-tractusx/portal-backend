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
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Factory;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.ClientScopes;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.ProtocolMappers;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.BusinessLogic;

public class ClientScopesUpdater(IKeycloakFactory keycloakFactory, ISeedDataHandler seedDataHandler)
    : IClientScopesUpdater
{
    public async Task UpdateClientScopes(string instanceName, CancellationToken cancellationToken)
    {
        var keycloak = keycloakFactory.CreateKeycloakClient(instanceName);
        var realm = seedDataHandler.Realm;
        var seederConfig = seedDataHandler.GetSpecificConfiguration(ConfigurationKey.ClientScopes);

        var clientScopes = await keycloak.GetClientScopesAsync(realm, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        var seedClientScopes = seedDataHandler.ClientScopes;

        await CheckAndExecute(ModificationType.Delete, keycloak, realm, clientScopes, seedClientScopes, seederConfig, cancellationToken, RemoveObsoleteClientScopes).ConfigureAwait(ConfigureAwaitOptions.None);
        await CheckAndExecute(ModificationType.Create, keycloak, realm, clientScopes, seedClientScopes, seederConfig, cancellationToken, CreateMissingClientScopes).ConfigureAwait(ConfigureAwaitOptions.None);
        await CheckAndExecute(ModificationType.Update, keycloak, realm, clientScopes, seedClientScopes, seederConfig, cancellationToken, UpdateExistingClientScopes).ConfigureAwait(ConfigureAwaitOptions.None);
    }

    private static Task CheckAndExecute(ModificationType modificationType, KeycloakClient keycloak, string realm, IEnumerable<ClientScope> clientScopes, IEnumerable<ClientScopeModel> seedClientScopes, KeycloakSeederConfigModel seederConfig, CancellationToken cancellationToken, Func<KeycloakClient, string, IEnumerable<ClientScope>, IEnumerable<ClientScopeModel>, KeycloakSeederConfigModel, CancellationToken, Task> executeLogic) =>
        seederConfig.ModificationAllowed(modificationType)
            ? executeLogic(keycloak, realm, clientScopes, seedClientScopes, seederConfig, cancellationToken)
            : Task.CompletedTask;

    private static async Task RemoveObsoleteClientScopes(KeycloakClient keycloak, string realm, IEnumerable<ClientScope> clientScopes, IEnumerable<ClientScopeModel> seedClientScopes, KeycloakSeederConfigModel seederConfig, CancellationToken cancellationToken)
    {
        foreach (var deleteScope in clientScopes
                     .Where(x => seederConfig.ModificationAllowed(ModificationType.Delete, x.Name))
                     .ExceptBy(seedClientScopes.Select(x => x.Name), x => x.Name))
        {
            await keycloak.DeleteClientScopeAsync(
                realm,
                deleteScope.Id ?? throw new ConflictException($"clientScope.Id is null: {deleteScope.Name}"),
                cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        }
    }

    private static async Task CreateMissingClientScopes(KeycloakClient keycloak, string realm, IEnumerable<ClientScope> clientScopes, IEnumerable<ClientScopeModel> seedClientScopes, KeycloakSeederConfigModel seederConfig, CancellationToken cancellationToken)
    {
        foreach (var addScope in seedClientScopes
                     .Where(x => seederConfig.ModificationAllowed(ModificationType.Create, x.Name))
                     .ExceptBy(clientScopes.Select(x => x.Name), x => x.Name))
        {
            await keycloak.CreateClientScopeAsync(realm, CreateClientScope(null, addScope, true), cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        }
    }

    private static async Task UpdateExistingClientScopes(KeycloakClient keycloak, string realm, IEnumerable<ClientScope> clientScopes, IEnumerable<ClientScopeModel> seedClientScopes, KeycloakSeederConfigModel seederConfig, CancellationToken cancellationToken)
    {
        foreach (var (clientScope, update) in clientScopes
            .Where(x => seederConfig.ModificationAllowed(ModificationType.Update, x.Name))
            .Join(
                seedClientScopes,
                x => x.Name,
                x => x.Name,
                (clientScope, update) => (ClientScope: clientScope, Update: update)))
        {
            await UpdateClientScopeWithProtocolMappers(keycloak, realm, clientScope, update, seederConfig, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        }
    }

    private static async Task UpdateClientScopeWithProtocolMappers(KeycloakClient keycloak, string realm, ClientScope clientScope, ClientScopeModel update, KeycloakSeederConfigModel seederConfig, CancellationToken cancellationToken)
    {
        if (clientScope.Id == null)
            throw new ConflictException($"clientScope.Id is null: {clientScope.Name}");

        if (clientScope.Name == null)
            throw new ConflictException($"clientScope.Name is null: {clientScope.Name}");

        if (!CompareClientScope(clientScope, update))
        {
            await keycloak.UpdateClientScopeAsync(
                realm,
                clientScope.Id,
                CreateClientScope(clientScope.Id, update, false),
                cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        }

        var mappers = clientScope.ProtocolMappers ?? Enumerable.Empty<ProtocolMapper>();
        var updateMappers = update.ProtocolMappers ?? Enumerable.Empty<ProtocolMapperModel>();

        await DeleteObsoleteProtocolMappers(keycloak, realm, clientScope.Name, clientScope.Id, mappers, updateMappers, seederConfig, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        await CreateMissingProtocolMappers(keycloak, realm, clientScope.Name, clientScope.Id, mappers, updateMappers, seederConfig, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        await UpdateExistingProtocolMappers(keycloak, realm, clientScope.Name, clientScope.Id, mappers, updateMappers, seederConfig, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
    }

    private static async Task DeleteObsoleteProtocolMappers(KeycloakClient keycloak, string realm, string clientScopeName, string clientScopeId, IEnumerable<ProtocolMapper> mappers, IEnumerable<ProtocolMapperModel> updateMappers, KeycloakSeederConfigModel seederConfig, CancellationToken cancellationToken)
    {
        foreach (var mapper in mappers
                     .Where(x => seederConfig.ModificationAllowed(clientScopeName, ConfigurationKey.ProtocolMappers, ModificationType.Delete, x.Name))
                     .ExceptBy(updateMappers.Select(x => x.Name), x => x.Name))
        {
            await keycloak.DeleteProtocolMapperAsync(
                realm,
                clientScopeId,
                mapper.Id ?? throw new ConflictException($"protocolMapper.Id is null {mapper.Name}"),
                cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        }
    }

    private static async Task CreateMissingProtocolMappers(KeycloakClient keycloak, string realm, string clientScopeName, string clientScopeId, IEnumerable<ProtocolMapper> mappers, IEnumerable<ProtocolMapperModel> updateMappers, KeycloakSeederConfigModel seederConfig, CancellationToken cancellationToken)
    {
        foreach (var update in updateMappers
                     .Where(x => seederConfig.ModificationAllowed(clientScopeName, ConfigurationKey.ProtocolMappers, ModificationType.Create, x.Name))
                     .ExceptBy(mappers.Select(x => x.Name), x => x.Name))
        {
            await keycloak.CreateProtocolMapperAsync(
                realm,
                clientScopeId,
                ProtocolMappersUpdater.CreateProtocolMapper(null, update),
                cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        }
    }

    private static async Task UpdateExistingProtocolMappers(KeycloakClient keycloak, string realm, string clientScopeName, string clientScopeId, IEnumerable<ProtocolMapper> mappers, IEnumerable<ProtocolMapperModel> updateMappers, KeycloakSeederConfigModel seederConfig, CancellationToken cancellationToken)
    {
        foreach (var (mapper, update) in mappers
            .Where(x => seederConfig.ModificationAllowed(clientScopeName, ConfigurationKey.ProtocolMappers, ModificationType.Update, x.Name))
            .Join(
                updateMappers,
                x => x.Name,
                x => x.Name,
                (mapper, update) => (Mapper: mapper, Update: update))
            .Where(x => !ProtocolMappersUpdater.CompareProtocolMapper(x.Mapper, x.Update)))
        {
            await keycloak.UpdateProtocolMapperAsync(
                realm,
                clientScopeId,
                mapper.Id ?? throw new ConflictException($"protocolMapper.Id is null {mapper.Name}"),
                ProtocolMappersUpdater.CreateProtocolMapper(mapper.Id, update),
                cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        }
    }

    private static ClientScope CreateClientScope(string? id, ClientScopeModel clientScope, bool includeProtocolMappers) =>
        new ClientScope
        {
            Id = id,
            Name = clientScope.Name,
            Description = clientScope.Description,
            Protocol = clientScope.Protocol,
            Attributes = clientScope.Attributes == null ? null : CreateClientScopeAttributes(clientScope.Attributes),
            ProtocolMappers = includeProtocolMappers ? clientScope.ProtocolMappers?.Select(x => ProtocolMappersUpdater.CreateProtocolMapper(x.Id, x)) : null
        };

    private static bool CompareClientScope(ClientScope scope, ClientScopeModel update) =>
        scope.Name == update.Name &&
        scope.Description == update.Description &&
        scope.Protocol == update.Protocol &&
        (scope.Attributes == null && update.Attributes == null ||
        scope.Attributes != null && update.Attributes != null &&
        CompareClientScopeAttributes(scope.Attributes, update.Attributes));

    private static Attributes CreateClientScopeAttributes(IReadOnlyDictionary<string, string?> update) =>
        new()
        {
            ConsentScreenText = update.GetValueOrDefault("consent.screen.text"),
            DisplayOnConsentScreen = update.GetValueOrDefault("display.on.consent.screen"),
            IncludeInTokenScope = update.GetValueOrDefault("include.in.token.scope")
        };

    private static bool CompareClientScopeAttributes(Attributes attributes, IReadOnlyDictionary<string, string?> update) =>
        attributes.ConsentScreenText == update.GetValueOrDefault("consent.screen.text") &&
        attributes.DisplayOnConsentScreen == update.GetValueOrDefault("display.on.consent.screen") &&
        attributes.IncludeInTokenScope == update.GetValueOrDefault("include.in.token.scope");
}
