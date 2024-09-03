/********************************************************************************
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Validation;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Context;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Worker.Library;

public static class ProcessExecutionServiceExtensions
{
    public static IServiceCollection AddProcessExecutionService<TProcessTypeId, TProcessStepTypeId>(this IServiceCollection services, IConfigurationSection section)
        where TProcessTypeId : struct, IConvertible
        where TProcessStepTypeId : struct, IConvertible
    {
        services.AddOptions<ProcessExecutionServiceSettings>()
            .Bind(section)
            .EnvironmentalValidation(section);
        services
            .AddScoped<IProcessRepositories, ProcessRepositories<TProcessTypeId, TProcessStepTypeId>>()
            .AddTransient<ProcessExecutionService<TProcessTypeId, TProcessStepTypeId>>()
            .AddTransient<IProcessExecutor<TProcessTypeId, TProcessStepTypeId>, ProcessExecutor<TProcessTypeId, TProcessStepTypeId>>()
            .AddTransient<IDateTimeProvider, UtcDateTimeProvider>();
        return services;
    }
}
