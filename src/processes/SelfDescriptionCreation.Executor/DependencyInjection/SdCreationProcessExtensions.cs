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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Validation;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Worker.Library;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.SelfDescriptionCreation.Executor.DependencyInjection;

public static class SdCreationProcessExtensions
{
    public static IServiceCollection AddSelfDescriptionCreationProcessExecutor(this IServiceCollection services, IConfiguration config)
    {
        var section = config.GetSection("SelfDescriptionCreationProcess");
        services.AddOptions<SelfDescriptionProcessSettings>()
            .Bind(section)
            .EnvironmentalValidation(section);

        return services
            .AddTransient<IProcessTypeExecutor<ProcessTypeId, ProcessStepTypeId>, SdCreationProcessTypeExecutor>();
    }
}
