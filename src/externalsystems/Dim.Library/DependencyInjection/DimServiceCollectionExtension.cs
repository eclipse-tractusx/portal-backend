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
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Dim.Library.BusinessLogic;
using Org.Eclipse.TractusX.Portal.Backend.Framework.HttpClientExtensions;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Validation;

namespace Org.Eclipse.TractusX.Portal.Backend.Dim.Library.DependencyInjection;

public static class DimServiceCollectionExtension
{
    public static IServiceCollection AddDimService(this IServiceCollection services, IConfigurationSection section)
    {
        services.AddOptions<DimSettings>()
            .Bind(section)
            .ValidateDataAnnotations()
            .ValidateDistinctValues(section)
            .ValidateOnStart();
        services.AddTransient<LoggingHandler<DimService>>();

        var sp = services.BuildServiceProvider();
        var settings = sp.GetRequiredService<IOptions<DimSettings>>();
        services.AddCustomHttpClientWithAuthentication<DimService>(settings.Value.BaseAddress);

        RegisterUniversalResolver(settings.Value, services);
        services
            .AddTransient<IDimService, DimService>()
            .AddTransient<IDimBusinessLogic, DimBusinessLogic>();

        return services;
    }

    private static void RegisterUniversalResolver(DimSettings settings, IServiceCollection services)
    {
        var baseAddress = settings.UniversalResolverAddress.EndsWith("/")
            ? settings.UniversalResolverAddress
            : $"{settings.UniversalResolverAddress}/";
        services.AddHttpClient("universalResolver", c =>
        {
            c.BaseAddress = new Uri(baseAddress);
        });
    }
}
