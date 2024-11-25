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
    public static bool ModificationAllowed(this KeycloakRealmSettings config, ConfigurationKeys configKey)
    {
        if (config.SeederConfiguration != null &&
            IsModificationAllowed(config.SeederConfiguration, configKey.ToString(), out var _))
        {
            return true;
        }

        return config.Create || config.Update || config.Delete;
    }

    private static bool IsModificationAllowed(
        IEnumerable<SeederConfiguration> configurations,
        string targetKey,
        out SeederConfiguration? matchingConfig)
    {
        matchingConfig = null;

        foreach (var config in configurations)
        {
            if (config.Entities != null && IsModificationAllowed(config.Entities, targetKey, out matchingConfig))
            {
                return true;
            }

            if (config.Key != targetKey || config is { Create: false, Update: false, Delete: false })
            {
                continue;
            }

            matchingConfig = config;
            return true;
        }

        return false;
    }

    public static bool ModificationAllowed(this KeycloakRealmSettings config, ConfigurationKeys configKey, ModificationType modificationType) =>
        config.ModificationAllowed(configKey, modificationType, null);

    public static bool ModificationAllowed(this KeycloakRealmSettings config, ConfigurationKeys configKey, ModificationType modificationType, string? entityKey)
    {
        var specificConfig = config.SeederConfiguration?.SingleOrDefault(x => x.Key.Equals(configKey.ToString(), StringComparison.OrdinalIgnoreCase));
        if (entityKey is null)
            return specificConfig?.ModifyAllowed(modificationType) ?? config.ModifyAllowed(modificationType);

        // If we have a configuration for a specific entry return its value
        var specificEntry = specificConfig?.Entities?.SingleOrDefault(c => c.Key.Equals(entityKey, StringComparison.OrdinalIgnoreCase));
        if (specificEntry != null)
        {
            return specificEntry.ModifyAllowed(modificationType);
        }

        // If we don't have a specific value return the specific configuration value if we have one
        return specificConfig?.ModifyAllowed(modificationType) ?? config.ModifyAllowed(modificationType);
    }

    public static bool ModificationAllowed(this KeycloakRealmSettings config, ConfigurationKeys containingConfigKey, string containingEntityKey, ConfigurationKeys configKey, ModificationType modificationType, string? entityKey)
    {
        var containingEntityTypeConfig = config.SeederConfiguration?.SingleOrDefault(x => x.Key.Equals(containingConfigKey.ToString(), StringComparison.OrdinalIgnoreCase))?.Entities?.SingleOrDefault(x => x.Key.Equals(containingEntityKey, StringComparison.OrdinalIgnoreCase))?.Entities?.SingleOrDefault(x => x.Key.Equals(configKey));
        if (containingEntityTypeConfig is null)
        {
            return config.ModificationAllowed(configKey, modificationType, entityKey);
        }

        if (entityKey is null)
        {
            return containingEntityTypeConfig?.ModifyAllowed(modificationType) ?? config.ModifyAllowed(modificationType);
        }

        // If we have a configuration for a specific entry return its value
        var entity = containingEntityTypeConfig.Entities?.SingleOrDefault(c => c.Key.Equals(entityKey, StringComparison.OrdinalIgnoreCase));
        return entity?.ModifyAllowed(modificationType) ?? config.ModificationAllowed(configKey, modificationType, entityKey);
    }

    private static bool ModifyAllowed(this KeycloakRealmSettings configuration, ModificationType modificationType) =>
        modificationType switch
        {
            ModificationType.Create => configuration.Create,
            ModificationType.Update => configuration.Update,
            ModificationType.Delete => configuration.Delete,
            _ => throw new ArgumentOutOfRangeException(nameof(modificationType), modificationType, null)
        };

    private static bool ModifyAllowed(this SeederConfiguration configuration, ModificationType modificationType) =>
        modificationType switch
        {
            ModificationType.Create => configuration.Create,
            ModificationType.Update => configuration.Update,
            ModificationType.Delete => configuration.Delete,
            _ => throw new ArgumentOutOfRangeException(nameof(modificationType), modificationType, null)
        };
}
