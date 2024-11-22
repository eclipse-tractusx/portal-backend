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
    public static bool ModificationAllowed(this SeederConfiguration config, string configKey)
    {
        var specificConfig = config.Entities?.SingleOrDefault(x => x.Key.Equals(configKey, StringComparison.OrdinalIgnoreCase));
        if (specificConfig != null)
        {
            return specificConfig.Create ||
                   specificConfig.Update ||
                   specificConfig.Delete ||
                   specificConfig.Entities?.Any(e => e.Create || e.Update || e.Delete) == true;
        }

        return config.Create || config.Update || config.Delete;
    }

    public static bool ModificationAllowed(this SeederConfiguration config, string configKey, ModificationType modificationType) =>
        config.ModificationAllowed(configKey, modificationType, null);

    public static bool ModificationAllowed(this SeederConfiguration config, string configKey, ModificationType modificationType, string? modelKey)
    {
        var specificConfig = config.Entities?.SingleOrDefault(x => x.Key.Equals(configKey, StringComparison.OrdinalIgnoreCase));
        if (modelKey == null)
            return specificConfig?.ModifyAllowed(modificationType) ?? config.ModifyAllowed(modificationType);

        // If we have a configuration for a specific entry return its value
        var specificEntry = specificConfig?.Entities?.SingleOrDefault(c => c.Key == modelKey);
        if (specificEntry != null)
        {
            return specificEntry.ModifyAllowed(modificationType);
        }

        // If we don't have a specific value return the specific configuration value if we have one
        return specificConfig?.ModifyAllowed(modificationType) ?? config.ModifyAllowed(modificationType);
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
