/********************************************************************************
 * Copyright (c) 2023 BMW Group AG
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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Worker.Library;
using Org.Eclipse.TractusX.Portal.Backend.OnboardingServiceProvider.Library.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.NetworkRegistration.Library.DependencyInjection;

namespace Org.Eclipse.TractusX.Portal.Backend.Processes.NetworkRegistration.Executor.DependencyInjection;

public static class NetworkRegistrationProcessCollectionExtensions
{
    public static IServiceCollection AddNetworkRegistrationProcessExecutor(this IServiceCollection services, IConfiguration config) =>
        services.AddNetworkRegistrationHandler(config)
                .AddOnboardingServiceProviderService(config)
                .AddTransient<IProcessTypeExecutor<ProcessTypeId, ProcessStepTypeId>, NetworkRegistrationProcessTypeExecutor>();
}
