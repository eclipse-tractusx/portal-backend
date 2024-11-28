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

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Extensions;

public static class SeederConfigurationExtensions
{
    public static bool ModificationAllowed(this KeycloakSeederConfigModel config, ModificationType modificationType) =>
        config.ModificationAllowed(modificationType, null);

    public static bool ModificationAllowed(this KeycloakSeederConfigModel config, ModificationType modificationType, string? entityKey)
    {
        var (defaultConfig, specificConfig) = config;
        if (entityKey is null)
        {
            return specificConfig?.ModifyAllowed(modificationType) ?? defaultConfig.ModifyAllowed(modificationType);
        }

        // If we have a configuration for a specific entry return its value
        if (specificConfig?.SeederConfigurations?.TryGetValue(entityKey, out var specificEntry) ?? false)
        {
            return specificEntry.ModifyAllowed(modificationType);
        }

        // If we don't have a specific value return the specific configuration value if we have one
        return specificConfig?.ModifyAllowed(modificationType) ?? defaultConfig.ModifyAllowed(modificationType);
    }

    public static bool ModificationAllowed(this KeycloakSeederConfigModel config, string containingEntityKey, ConfigurationKey configKey, ModificationType modificationType) =>
        config.ModificationAllowed(containingEntityKey, configKey, modificationType, null);

    public static bool ModificationAllowed(this KeycloakSeederConfigModel config, string containingEntityKey, ConfigurationKey configKey, ModificationType modificationType, string? entityKey)
    {
        // Check if the specific configuration contains the entity key
        // e.g. for the users configuration check for a specific user configuration
        if (config.SpecificConfiguration?.SeederConfigurations?.TryGetValue(containingEntityKey, out var containingEntityKeyConfiguration) ?? false)
        {
            // check if the specific entity configuration has a configuration for the section
            // e.g. for the specific user configuration is there a section for federated identities
            if (!(containingEntityKeyConfiguration.SeederConfigurations?.TryGetValue(configKey.ToString(), out var containingEntityTypeConfig) ?? false))
            {
                return (config with { SpecificConfiguration = config.DefaultSettings.SeederConfigurations?.TryGetValue(configKey.ToString(), out var specificConfig) ?? false ? specificConfig : null })
                    .ModificationAllowed(modificationType, entityKey);
            }

            // if the entity key isn't set check the configuration for the type
            if (entityKey is null)
            {
                return containingEntityTypeConfig.ModifyAllowed(modificationType);
            }

            // If we have a configuration for a specific entry return its value otherwise take the section configuration
            return (containingEntityTypeConfig.SeederConfigurations?.TryGetValue(entityKey, out var entity) ?? false ? entity?.ModifyAllowed(modificationType) : null)
                ?? containingEntityTypeConfig.ModifyAllowed(modificationType);
        }

        // if no configuration isn't set check the top level configuration
        return (config with { SpecificConfiguration = config.DefaultSettings.SeederConfigurations?.TryGetValue(configKey.ToString(), out var topLevelSpecificConfig) ?? false ? topLevelSpecificConfig : null })
            .ModificationAllowed(modificationType, entityKey);
    }

    private static bool ModifyAllowed(this SeederConfigurationModel configuration, ModificationType modificationType) =>
        modificationType switch
        {
            ModificationType.Create => configuration.Create,
            ModificationType.Update => configuration.Update,
            ModificationType.Delete => configuration.Delete,
            _ => throw new ArgumentOutOfRangeException(nameof(modificationType), modificationType, null)
        };
}
