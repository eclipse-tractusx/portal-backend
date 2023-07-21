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

using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Library.Models.ProtocolMappers;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Models;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.BusinessLogic;
public static class ProtocolMappersUpdater
{
    public static ProtocolMapper CreateProtocolMapper(string? id, ProtocolMapperModel x) =>
        new ProtocolMapper()
        {
            Id = id,
            Name = x.Name,
            Protocol = x.Protocol,
            _ProtocolMapper = x.ProtocolMapper,
            ConsentRequired = x.ConsentRequired,
            Config = x.Config == null ? null : CreateProtocolMapperConfig(x.Config)
        };

    public static bool CompareProtocolMapper(ProtocolMapper mapper, ProtocolMapperModel update) =>
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
