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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Seeding;

public class SeederSettings
{
    /// <summary>
    /// Configurable paths where to check for the seeding data
    /// </summary>
    [Required]
    public IEnumerable<string> DataPaths { get; set; } = null!;

    /// <summary>
    /// Configurable environments for the testdata
    /// </summary>
    public IEnumerable<string>? TestDataEnvironments { get; set; }
}

public static class SeederSettingsExtensions
{
    public static IServiceCollection ConfigureSeederSettings(
        this IServiceCollection services,
        IConfigurationSection section)
    {
        services.AddOptions<SeederSettings>()
            .Bind(section)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        return services;
    }
}
