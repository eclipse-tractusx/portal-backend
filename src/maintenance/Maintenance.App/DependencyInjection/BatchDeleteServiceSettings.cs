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

using Org.Eclipse.TractusX.Portal.Backend.Maintenance.App.Services;
using System.ComponentModel.DataAnnotations;

namespace Org.Eclipse.TractusX.Portal.Backend.Maintenance.App.DependencyInjection;

/// <summary>
/// Settings for the <see cref="BatchDeleteService"/>
/// </summary>
public class BatchDeleteServiceSettings
{
    /// <summary>
    /// Documents older than this configured value will be deleted
    /// </summary>
    [Required]
    public int DeleteIntervalInDays { get; set; }
}

/// <summary>
/// Extensions for the <see cref="BatchDeleteService"/>
/// </summary>
public static class BatchDeleteServiceExtensions
{
    /// <summary>
    /// Adds the <see cref="BatchDeleteService"/> to the service collection
    /// </summary>
    /// <param name="services">The service collection used for di</param>
    /// <param name="section">The configuration section to get the settings from</param>
    /// <returns>The enhanced service collection</returns>
    public static IServiceCollection AddBatchDelete(this IServiceCollection services, IConfigurationSection section)
    {
        services.AddOptions<BatchDeleteServiceSettings>().Bind(section);
        services
            .AddTransient<IBatchDeleteService, BatchDeleteService>();
        return services;
    }
}
