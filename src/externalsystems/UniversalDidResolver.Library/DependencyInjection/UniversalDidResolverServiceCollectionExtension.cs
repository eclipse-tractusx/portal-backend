/********************************************************************************
 * Copyright (c) 2025 Contributors to the Eclipse Foundation
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

namespace Org.Eclipse.TractusX.Portal.Backend.UniversalDidResolver.Library.DependencyInjection;

public static class UniversalDidResolverServiceCollectionExtension
{
    public static IServiceCollection AddUniversalDidResolverService(this IServiceCollection services, IConfigurationSection section)
    {
        services.AddOptions<UniversalDidResolverSettings>()
            .Bind(section);

        services.AddTransient<LoggingHandler<UniversalDidResolverService>>();

        var sp = services.BuildServiceProvider();
        var settings = sp.GetRequiredService<IOptions<UniversalDidResolverSettings>>();

        RegisterUniversalResolver(settings.Value, services);
        services
            .AddTransient<IUniversalDidResolverService, UniversalDidResolverService>();

        return services;
    }

    private static void RegisterUniversalResolver(UniversalDidResolverSettings settings, IServiceCollection services)
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
