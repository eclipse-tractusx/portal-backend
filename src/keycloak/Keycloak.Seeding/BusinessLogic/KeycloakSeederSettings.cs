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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Validation;
using Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Models;
using System.ComponentModel.DataAnnotations;

namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.BusinessLogic;

public class KeycloakSeederSettings
{
    [Required]
    [DistinctValues]
    public IEnumerable<string> DataPathes { get; set; } = null!;

    [Required]
    public string InstanceName { get; set; } = null!;

    public IEnumerable<string>? ExcludedUserAttributes { get; set; }

    public IEnumerable<KeycloakRealmSettings>? Realms { get; set; }
}

public static class KeycloakSeederSettingsExtensions
{
    public static IServiceCollection ConfigureKeycloakSeederSettings(
        this IServiceCollection services,
        IConfigurationSection section
    )
    {
        services.AddOptions<KeycloakSeederSettings>()
            .Bind(section)
            .ValidateDistinctValues(section)
            .ValidateOnStart();
        return services;
    }
}
