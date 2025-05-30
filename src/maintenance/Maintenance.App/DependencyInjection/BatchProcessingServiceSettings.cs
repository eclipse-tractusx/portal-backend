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
/// Settings for the <see cref="BatchProcessingService"/>
/// </summary>
public class BatchProcessingServiceSettings
{
    /// <summary>
    /// Documents older than this configured value will be deleted
    /// </summary>
    [Required]
    public int DeleteDocumentsIntervalInDays { get; set; }

    /// <summary>
    /// Companies waiting for a clearing house validation step that is older than this configured value will be triggered
    /// </summary>
    [Required]
    public int RetriggerClearinghouseIntervalInDays { get; set; }
}

/// <summary>
/// Extensions for the <see cref="BatchProcessingService"/>
/// </summary>
public static class BatchProcessingServiceExtensions
{
    /// <summary>
    /// Adds the <see cref="BatchProcessingService"/> to the service collection
    /// </summary>
    /// <param name="services">The service collection used for di</param>
    /// <param name="section">The configuration section to get the settings from</param>
    /// <returns>The enhanced service collection</returns>
    public static IServiceCollection AddBatchProcessing(this IServiceCollection services, IConfigurationSection section)
    {
        services.AddOptions<BatchProcessingServiceSettings>().Bind(section);
        services
            .AddTransient<IBatchProcessingService, BatchProcessingService>();
        return services;
    }
}
