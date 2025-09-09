/********************************************************************************
 * Copyright (c) 2025 Contributors to the Eclipse Foundation
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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Validation;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Factory;
using System.ComponentModel.DataAnnotations;

namespace Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;

public sealed class MultiKeycloakSettings
{
    [Required]
    public bool IsSharedIdpMultiInstancesEnabled { get; set; }

    [DistinctValues("x => x.Index")]
    public IEnumerable<EncryptionModeConfig> EncryptionConfigs { get; set; } = null!;

    public int EncryptionConfigIndex { get; set; }
}

public static class MultiKeycloakSettingsExtension
{
    public static IServiceCollection AddMultiKeycloak(
        this IServiceCollection services,
        IConfiguration config)
    {
        services
             .AddTransient<IKeycloakFactory, KeycloakFactory>()
             .ConfigureKeycloakSettingsMap(config.GetSection("Keycloak"))
             .AddTransient<ISharedMultiKeycloakResolver, SharedMultiKeycloakResolver>();

        var configSection = config.GetSection("MultiSharedKeycloak");
        services
             .AddOptions<MultiKeycloakSettings>()
             .Bind(configSection)
             .Validate(options =>
             {
                 if (options.IsSharedIdpMultiInstancesEnabled &&
                     (options.EncryptionConfigs == null || !options.EncryptionConfigs.Any()))
                 {
                     return false;
                 }
                 return true;
             }, "EncryptionConfigs must be provided when IsSharedIdpMultiInstancesEnabled is true");

        return services;
    }
}
