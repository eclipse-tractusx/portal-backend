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
    public static bool IsModificationAllowed(this SeederConfiguration config, ConfigurationKey configKey) =>
        config.Create || config.Update || config.Delete ||
        (config.SeederConfigurations != null &&
            IsModificationAllowed(config.SeederConfigurations, configKey.ToString(), false, out var _));

    private static bool IsModificationAllowed(
        IEnumerable<SeederConfiguration> configurations,
        string targetKey,
        bool isKeyParentConfig,
        out SeederConfiguration? matchingConfig)
    {
        matchingConfig = null;

        foreach (var config in configurations)
        {
            if (config.SeederConfigurations != null && IsModificationAllowed(config.SeederConfigurations, targetKey, isKeyParentConfig || config.Key == targetKey, out matchingConfig))
            {
                return true;
            }

            if ((config.Key != targetKey && !isKeyParentConfig) || config is { Create: false, Update: false, Delete: false })
            {
                continue;
            }

            matchingConfig = config;
            return true;
        }

        return false;
    }

    public static bool ModificationAllowed(this KeycloakSeederConfigModel config, ModificationType modificationType) =>
        config.ModificationAllowed(modificationType, null);

    public static bool ModificationAllowed(this KeycloakSeederConfigModel config, ModificationType modificationType, string? entityKey)
    {
        var (defaultConfig, specificConfig) = config;
        if (entityKey is null)
            return specificConfig?.ModifyAllowed(modificationType) ?? defaultConfig.ModifyAllowed(modificationType);

        // If we have a configuration for a specific entry return its value
        var specificEntry = specificConfig?.SeederConfigurations?.SingleOrDefault(c => c.Key.Equals(entityKey, StringComparison.OrdinalIgnoreCase));
        if (specificEntry != null)
        {
            return specificEntry.ModifyAllowed(modificationType);
        }

        // If we don't have a specific value return the specific configuration value if we have one
        return specificConfig?.ModifyAllowed(modificationType) ?? defaultConfig.ModifyAllowed(modificationType);
    }

    public static bool ModificationAllowed(this KeycloakSeederConfigModel config, string containingEntityKey, ConfigurationKey configKey, ModificationType modificationType, string? entityKey)
    {
        var containingEntityTypeConfig = config.SpecificConfiguration?.SeederConfigurations?.SingleOrDefault(x => x.Key.Equals(containingEntityKey, StringComparison.OrdinalIgnoreCase))?.SeederConfigurations?.SingleOrDefault(x => x.Key.Equals(configKey.ToString(), StringComparison.OrdinalIgnoreCase));
        if (containingEntityTypeConfig is null)
        {
            var configModel = config with { SpecificConfiguration = config.DefaultSettings.SeederConfigurations?.SingleOrDefault(x => x.Key.Equals(configKey.ToString(), StringComparison.OrdinalIgnoreCase)) };
            return configModel.ModificationAllowed(modificationType, entityKey);
        }

        if (entityKey is null)
        {
            return containingEntityTypeConfig.ModifyAllowed(modificationType);
        }

        // If we have a configuration for a specific entry return its value
        var entity = containingEntityTypeConfig.SeederConfigurations?.SingleOrDefault(c => c.Key.Equals(entityKey, StringComparison.OrdinalIgnoreCase));
        return entity?.ModifyAllowed(modificationType) ?? config.ModificationAllowed(modificationType, entityKey);
    }

    private static bool ModifyAllowed(this SeederConfiguration configuration, ModificationType modificationType) =>
        modificationType switch
        {
            ModificationType.Create => configuration.Create,
            ModificationType.Update => configuration.Update,
            ModificationType.Delete => configuration.Delete,
            _ => throw new ArgumentOutOfRangeException(nameof(modificationType), modificationType, null)
        };
}
