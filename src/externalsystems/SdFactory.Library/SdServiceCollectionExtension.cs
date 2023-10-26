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
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.HttpClientExtensions;
using Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library.BusinessLogic;

namespace Org.Eclipse.TractusX.Portal.Backend.SdFactory.Library;

public static class SdServiceCollectionExtension
{
    public static IServiceCollection AddSdFactoryService(this IServiceCollection services, IConfigurationSection section)
    {
        services.AddOptions<SdFactorySettings>()
            .Bind(section)
            .ValidateOnStart();
        services.AddTransient<LoggingHandler<SdFactoryService>>();

        var sp = services.BuildServiceProvider();
        var settings = sp.GetRequiredService<IOptions<SdFactorySettings>>();
        services
            .AddCustomHttpClientWithAuthentication<SdFactoryService>(settings.Value.SdFactoryUrl)
            .AddTransient<ISdFactoryService, SdFactoryService>()
            .AddTransient<ISdFactoryBusinessLogic, SdFactoryBusinessLogic>();

        return services;
    }
}
