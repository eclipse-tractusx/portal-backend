/********************************************************************************
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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Framework.DateTimeProvider;
using System.Diagnostics.CodeAnalysis;

namespace Org.Eclipse.TractusX.Portal.Backend.Credential.App.DependencyInjection;

/// <summary>
/// Extension method to register the expiry check service and dependent services
/// </summary>
[ExcludeFromCodeCoverage]
public static class ExpiryCheckServiceExtensions
{
    /// <summary>
    /// Adds the expiry check service
    /// </summary>
    /// <param name="services">the services</param>
    /// <param name="section">the configuration section to setup the settings</param>
    /// <returns>the enriched service collection</returns>
    public static IServiceCollection AddExpiryCheckService(this IServiceCollection services, IConfigurationSection section)
    {
        services.AddOptions<ExpiryCheckServiceSettings>().Bind(section);
        services
            .AddTransient<ExpiryCheckService>()
            .AddTransient<IDateTimeProvider, UtcDateTimeProvider>();
        return services;
    }
}
