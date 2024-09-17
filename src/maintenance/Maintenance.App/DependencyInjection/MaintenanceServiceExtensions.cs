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

using Org.Eclipse.TractusX.Portal.Backend.Framework.DateTimeProvider;
using Org.Eclipse.TractusX.Portal.Backend.Maintenance.App.Services;

namespace Org.Eclipse.TractusX.Portal.Backend.Maintenance.App.DependencyInjection;

/// <summary>
/// Extension methods to register the necessary services for the maintenance job
/// </summary>
public static class MaintenanceServiceExtensions
{
    /// <summary>
    /// Adds the dependencies for the maintenance service
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The enhanced service collection</returns>
    public static IServiceCollection AddMaintenanceService(this IServiceCollection services) =>
        services
            .AddTransient<MaintenanceService>()
            .AddTransient<IDateTimeProvider, UtcDateTimeProvider>();
}
