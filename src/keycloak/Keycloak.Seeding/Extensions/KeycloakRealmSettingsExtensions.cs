/********************************************************************************
 * Copyright (c) 2024 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Models;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Extensions;

public static class KeycloakRealmSettingsExtensions
{
    public static IReadOnlyDictionary<ConfigurationKey, bool>? GetFlatDictionary(this KeycloakRealmSettings realmSettings) =>
        realmSettings.SeederConfigurations?
            .Join(
                Enum.GetValues<ConfigurationKey>(),
                config => config.Key,
                key => key.ToString(),
                (SeederConfiguration config, ConfigurationKey key) => KeyValuePair.Create(key, GetFlat(config)))
            .ToImmutableDictionary();

    private static bool GetFlat(SeederConfiguration config) =>
        config.Create || config.Update || config.Delete || (config.SeederConfigurations != null && config.SeederConfigurations.Any(GetFlat));

    public static SeederConfigurationModel GetConfigurationDictionaries(this KeycloakRealmSettings realmSettings) =>
        new(
            realmSettings.Create,
            realmSettings.Update,
            realmSettings.Delete,
            realmSettings.SeederConfigurations?.ToImmutableDictionary(sc =>
                sc.Key,
                ConvertSeederConfigToSeederConfigurationModel));

    private static SeederConfigurationModel ConvertSeederConfigToSeederConfigurationModel(this SeederConfiguration seederConfig) =>
        new(
            seederConfig.Create,
            seederConfig.Update,
            seederConfig.Delete,
            seederConfig.SeederConfigurations?.ToImmutableDictionary(sc =>
                sc.Key,
                ConvertSeederConfigToSeederConfigurationModel));
}
