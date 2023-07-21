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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Linq;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Factory;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.ClientScopes;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.ProtocolMappers;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.BusinessLogic;

public class ClientScopesUpdater : IClientScopesUpdater
{
    private readonly IKeycloakFactory _keycloakFactory;
    private readonly ISeedDataHandler _seedData;

    public ClientScopesUpdater(IKeycloakFactory keycloakFactory, ISeedDataHandler seedDataHandler)
    {
        _keycloakFactory = keycloakFactory;
        _seedData = seedDataHandler;
    }

    public async Task UpdateClientScopes(string instanceName, CancellationToken cancellationToken)
    {
        var keycloak = _keycloakFactory.CreateKeycloakClient(instanceName);
        var realm = _seedData.Realm;

        var clientScopes = await keycloak.GetClientScopesAsync(realm, cancellationToken).ConfigureAwait(false);
        var seedClientScopes = _seedData.ClientScopes;

        if (clientScopes.ExceptBy(seedClientScopes.Select(x => x.Name), x => x.Name).IfAny(
            async deleteScopes =>
            {
                foreach (var deleteScope in deleteScopes)
                {
                    await keycloak.DeleteClientScopeAsync(
                        realm,
                        deleteScope.Id ?? throw new ConflictException($"clientScope.Id is null: {deleteScope.Name}"),
                        cancellationToken).ConfigureAwait(false);
                }
            },
            out var deleteTask))
        {
            await deleteTask!.ConfigureAwait(false);
        }

        if (seedClientScopes.ExceptBy(clientScopes.Select(x => x.Name), x => x.Name).IfAny(
            async addScopes =>
            {
                foreach (var addScope in addScopes)
                {
                    await keycloak.CreateClientScopeAsync(realm, CreateClientScope(null, addScope, true), cancellationToken).ConfigureAwait(false);
                }
            },
            out var addTask))
        {
            await addTask!.ConfigureAwait(false);
        }

        if (clientScopes.Join(
            seedClientScopes,
            x => x.Name,
            x => x.Name,
            (clientScope, update) => (ClientScope: clientScope, Update: update)).IfAny(
                async joinedScopes =>
                {
                    foreach (var (clientScope, update) in joinedScopes)
                    {
                        await UpdateClientScopeWithProtocolMappers(clientScope, update).ConfigureAwait(false);
                    }
                },
                out var updateTask))
        {
            await updateTask!.ConfigureAwait(false);
        }

        async Task UpdateClientScopeWithProtocolMappers(ClientScope clientScope, ClientScopeModel update)
        {
            if (clientScope.Id == null)
                throw new ConflictException($"clientScope.Id is null: {clientScope.Name}");

            if (!CompareClientScope(clientScope, update))
            {
                await keycloak.UpdateClientScopeAsync(
                    realm,
                    clientScope.Id,
                    CreateClientScope(clientScope.Id, update, false),
                    cancellationToken).ConfigureAwait(false);
            }

            var mappers = clientScope.ProtocolMappers ?? Enumerable.Empty<ProtocolMapper>();
            var updateMappers = update.ProtocolMappers ?? Enumerable.Empty<ProtocolMapperModel>();

            if (mappers.ExceptBy(updateMappers.Select(x => x.Name), x => x.Name).IfAny(
                async deleteMappers =>
                {
                    foreach (var mapper in deleteMappers)
                    {
                        await keycloak.DeleteProtocolMapperAsync(
                            realm,
                            clientScope.Id,
                            mapper.Id ?? throw new ConflictException($"protocolMapper.Id is null {mapper.Name}"),
                            cancellationToken).ConfigureAwait(false);
                    }
                },
                out var deleteMappersTask))
            {
                await deleteMappersTask!.ConfigureAwait(false);
            }

            if (updateMappers.ExceptBy(mappers.Select(x => x.Name), x => x.Name).IfAny(
                async addMappers =>
                {
                    foreach (var update in addMappers)
                    {
                        await keycloak.CreateProtocolMapperAsync(
                            realm,
                            clientScope.Id,
                            CreateProtocolMapper(null, update),
                            cancellationToken).ConfigureAwait(false);
                    }
                },
                out var addMappersTask))
            {
                await addMappersTask!.ConfigureAwait(false);
            }

            if (mappers.Join(
                updateMappers,
                x => x.Name,
                x => x.Name,
                (mapper, update) => (Mapper: mapper, Update: update))
                .Where(x => !CompareProtocolMapper(x.Mapper, x.Update))
                .IfAny(
                    async joinedMappers =>
                    {
                        foreach (var (mapper, update) in joinedMappers)
                        {
                            await keycloak.UpdateProtocolMapperAsync(
                                realm,
                                clientScope.Id,
                                mapper.Id ?? throw new ConflictException($"protocolMapper.Id is null {mapper.Name}"),
                                CreateProtocolMapper(mapper.Id, update),
                                cancellationToken).ConfigureAwait(false);
                        }
                    },
                    out var updateMappersTask))
            {
                await updateMappersTask!.ConfigureAwait(false);
            }
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
            ProtocolMappers = includeProtocolMappers ? clientScope?.ProtocolMappers?.Select(x => CreateProtocolMapper(x.Id, x)) : null
        };
    
    private static bool CompareClientScope(ClientScope scope, ClientScopeModel update) =>
        scope.Name == update.Name &&
        scope.Description == update.Description &&
        scope.Protocol == update.Protocol &&
        (scope.Attributes == null && update.Attributes == null ||
        scope.Attributes != null && update.Attributes != null &&
        CompareClientScopeAttributes(scope.Attributes, update.Attributes));

    private static Attributes CreateClientScopeAttributes(IReadOnlyDictionary<string, string> update) =>
        new Attributes
        {
            ConsentScreenText = update.TryGetValue("consent.screen.text", out var consentScreenText) ? consentScreenText : null,
            DisplayOnConsentScreen = update.TryGetValue("display.on.consent.screen", out var displayOnConsentScreen) ? displayOnConsentScreen : null,
            IncludeInTokenScope = update.TryGetValue("include.in.token.scope", out var includeInTokenScope) ? includeInTokenScope : null,
        };

    private static bool CompareClientScopeAttributes(Attributes attributes, IReadOnlyDictionary<string, string> update) =>
        attributes.ConsentScreenText == (update.TryGetValue("consent.screen.text", out var consentScreenText) ? consentScreenText : null) &&
        attributes.DisplayOnConsentScreen == (update.TryGetValue("display.on.consent.screen", out var displayOnConsentScreen) ? displayOnConsentScreen : null) &&
        attributes.IncludeInTokenScope == (update.TryGetValue("include.in.token.scope", out var includeInTokenScope) ? includeInTokenScope : null);

    private static ProtocolMapper CreateProtocolMapper(string? id, ProtocolMapperModel x) =>
        new ProtocolMapper()
        {
            Id = id,
            Name = x.Name,
            Protocol = x.Protocol,
            _ProtocolMapper = x.ProtocolMapper,
            ConsentRequired = x.ConsentRequired,
            Config = x.Config == null ? null : CreateProtocolMapperConfig(x.Config)
        };
    
    private static bool CompareProtocolMapper(ProtocolMapper mapper, ProtocolMapperModel update) =>
        mapper.Name == update.Name &&
        mapper.Protocol == update.Protocol &&
        mapper._ProtocolMapper == update.ProtocolMapper &&
        mapper.ConsentRequired == update.ConsentRequired &&
        (mapper.Config == null && update.Config == null ||
        mapper.Config != null && update.Config != null &&
        CompareProtocolMapperConfig(mapper.Config, update.Config));

    private static Config CreateProtocolMapperConfig(IReadOnlyDictionary<string, string> update) =>
        new Config
        {
            Single = update.TryGetValue("single", out var single) ? single : null,
            AttributeNameFormat = update.TryGetValue("attribute.nameformat", out var attributeNameFormat) ? attributeNameFormat : null,
            AttributeName = update.TryGetValue("attribute.name", out var attributeName) ? attributeName : null,
            UserInfoTokenClaim = update.TryGetValue("userinfo.token.claim", out var userInfoTokenClaim) ? userInfoTokenClaim : null,
            UserAttribute = update.TryGetValue("user.attribute", out var userAttribute) ? userAttribute : null,
            IdTokenClaim = update.TryGetValue("id.token.claim", out var idTokenClaim) ? idTokenClaim : null,
            AccessTokenClaim = update.TryGetValue("access.token.claim", out var accessTokenClaim) ? accessTokenClaim : null,
            ClaimName = update.TryGetValue("claim.name", out var claimName) ? claimName : null,
            JsonTypelabel = update.TryGetValue("jsonType.label", out var jsonTypeLabel) ? jsonTypeLabel : null,
            UserAttributeFormatted = update.TryGetValue("user.attribute.formated", out var userAttributeFormated) ? userAttributeFormated : null,
            UserAttributeCountry = update.TryGetValue("user.attribute.country", out var userAttributeCountry) ? userAttributeCountry : null,
            UserAttributePostalCode = update.TryGetValue("user.attribute.postal_code", out var userAttributePostalCode) ? userAttributePostalCode : null,
            UserAttributeStreet = update.TryGetValue("user.attribute.street", out var userAttributeStreet) ? userAttributeStreet : null,
            UserAttributeRegion = update.TryGetValue("user.attribute.region", out var userAttributeRegion) ? userAttributeRegion : null,
            UserAttributeLocality = update.TryGetValue("user.attribute.locality", out var userAttributeLocality) ? userAttributeLocality : null,
            IncludedClientAudience = update.TryGetValue("included.client.audience", out var includedClientAudience) ? includedClientAudience : null,
            Multivalued = update.TryGetValue("multivalued", out var multiValued) ? multiValued : null
        };

    private static bool CompareProtocolMapperConfig(Config config, IReadOnlyDictionary<string, string> update) =>
        config.Single == (update.TryGetValue("single", out var single) ? single : null) &&
            config.AttributeNameFormat == (update.TryGetValue("attribute.nameformat", out var attributeNameFormat) ? attributeNameFormat : null) &&
            config.AttributeName == (update.TryGetValue("attribute.name", out var attributeName) ? attributeName : null) &&
            config.UserInfoTokenClaim == (update.TryGetValue("userinfo.token.claim", out var userInfoTokenClaim) ? userInfoTokenClaim : null) &&
            config.UserAttribute == (update.TryGetValue("user.attribute", out var userAttribute) ? userAttribute : null) &&
            config.IdTokenClaim == (update.TryGetValue("id.token.claim", out var idTokenClaim) ? idTokenClaim : null) &&
            config.AccessTokenClaim == (update.TryGetValue("access.token.claim", out var accessTokenClaim) ? accessTokenClaim : null) &&
            config.ClaimName == (update.TryGetValue("claim.name", out var claimName) ? claimName : null) &&
            config.JsonTypelabel == (update.TryGetValue("jsonType.label", out var jsonTypeLabel) ? jsonTypeLabel : null) &&
            config.UserAttributeFormatted == (update.TryGetValue("user.attribute.formated", out var userAttributeFormated) ? userAttributeFormated : null) &&
            config.UserAttributeCountry == (update.TryGetValue("user.attribute.country", out var userAttributeCountry) ? userAttributeCountry : null) &&
            config.UserAttributePostalCode == (update.TryGetValue("user.attribute.postal_code", out var userAttributePostalCode) ? userAttributePostalCode : null) &&
            config.UserAttributeStreet == (update.TryGetValue("user.attribute.street", out var userAttributeStreet) ? userAttributeStreet : null) &&
            config.UserAttributeRegion == (update.TryGetValue("user.attribute.region", out var userAttributeRegion) ? userAttributeRegion : null) &&
            config.UserAttributeLocality == (update.TryGetValue("user.attribute.locality", out var userAttributeLocality) ? userAttributeLocality : null) &&
            config.IncludedClientAudience == (update.TryGetValue("included.client.audience", out var includedClientAudience) ? includedClientAudience : null) &&
            config.Multivalued == (update.TryGetValue("multivalued", out var multiValued) ? multiValued : null);
}
