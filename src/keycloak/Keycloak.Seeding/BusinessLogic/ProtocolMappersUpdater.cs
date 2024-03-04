/********************************************************************************
 * Copyright (c) 2021, 2024 BMW Group AG
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
            Single = update.GetValueOrDefault("single"),
            AttributeNameFormat = update.GetValueOrDefault("attribute.nameformat"),
            AttributeName = update.GetValueOrDefault("attribute.name"),
            UserInfoTokenClaim = update.GetValueOrDefault("userinfo.token.claim"),
            UserAttribute = update.GetValueOrDefault("user.attribute"),
            IdTokenClaim = update.GetValueOrDefault("id.token.claim"),
            AccessTokenClaim = update.GetValueOrDefault("access.token.claim"),
            ClaimName = update.GetValueOrDefault("claim.name"),
            JsonTypelabel = update.GetValueOrDefault("jsonType.label"),
            UserAttributeFormatted = update.GetValueOrDefault("user.attribute.formated"),
            UserAttributeCountry = update.GetValueOrDefault("user.attribute.country"),
            UserAttributePostalCode = update.GetValueOrDefault("user.attribute.postal_code"),
            UserAttributeStreet = update.GetValueOrDefault("user.attribute.street"),
            UserAttributeRegion = update.GetValueOrDefault("user.attribute.region"),
            UserAttributeLocality = update.GetValueOrDefault("user.attribute.locality"),
            IncludedClientAudience = update.GetValueOrDefault("included.client.audience"),
            Multivalued = update.GetValueOrDefault("multivalued"),
            UserSessionNote = update.GetValueOrDefault("user.session.note"),
        };

    private static bool CompareProtocolMapperConfig(Config config, IReadOnlyDictionary<string, string> update) =>
        config.Single == update.GetValueOrDefault("single") &&
            config.AttributeNameFormat == update.GetValueOrDefault("attribute.nameformat") &&
            config.AttributeName == update.GetValueOrDefault("attribute.name") &&
            config.UserInfoTokenClaim == update.GetValueOrDefault("userinfo.token.claim") &&
            config.UserAttribute == update.GetValueOrDefault("user.attribute") &&
            config.IdTokenClaim == update.GetValueOrDefault("id.token.claim") &&
            config.AccessTokenClaim == update.GetValueOrDefault("access.token.claim") &&
            config.ClaimName == update.GetValueOrDefault("claim.name") &&
            config.JsonTypelabel == update.GetValueOrDefault("jsonType.label") &&
            config.UserAttributeFormatted == update.GetValueOrDefault("user.attribute.formated") &&
            config.UserAttributeCountry == update.GetValueOrDefault("user.attribute.country") &&
            config.UserAttributePostalCode == update.GetValueOrDefault("user.attribute.postal_code") &&
            config.UserAttributeStreet == update.GetValueOrDefault("user.attribute.street") &&
            config.UserAttributeRegion == update.GetValueOrDefault("user.attribute.region") &&
            config.UserAttributeLocality == update.GetValueOrDefault("user.attribute.locality") &&
            config.IncludedClientAudience == update.GetValueOrDefault("included.client.audience") &&
            config.Multivalued == update.GetValueOrDefault("multivalued") &&
            config.UserSessionNote == update.GetValueOrDefault("user.session.note");
}
