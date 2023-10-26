/********************************************************************************
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

using Microsoft.Extensions.DependencyInjection;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.DependencyInjection;

public static class RegistrationHelper
{
    public static IServiceCollection AutoRegister(this IServiceCollection services) =>
        services.AutoRegister(typeof(ITransient), ServiceLifetime.Transient)
            .AutoRegister(typeof(IScoped), ServiceLifetime.Scoped)
            .AutoRegister(typeof(ISingleton), ServiceLifetime.Singleton);

    private static IServiceCollection AutoRegister(this IServiceCollection services, Type interfaceType, ServiceLifetime lifetime)
    {
        var interfaces = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(t => t.IsInterface && t != interfaceType && interfaceType.IsAssignableFrom(t))
            .Distinct();
        foreach (var type in interfaces)
        {
            services.AddRegistration(type, lifetime);
        }

        return services;
    }

    public static IServiceCollection AddRegistration(this IServiceCollection services, Type interfaceType, ServiceLifetime lifetime)
    {
        var interfaceTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(t => interfaceType.IsAssignableFrom(t) && t is { IsClass: true, IsAbstract: false })
                .Select(t => new
                {
                    Service = t.GetInterfaces().FirstOrDefault(),
                    Implementation = t
                })
                .Where(t => t.Service is not null && interfaceType.IsAssignableFrom(t.Service))
                .Select(t => new
                {
                    Service = t.Service!,
                    t.Implementation
                });

        foreach (var type in interfaceTypes)
        {
            services.AddService(type.Service, type.Implementation, lifetime);
        }

        return services;
    }

    private static IServiceCollection AddService(this IServiceCollection services, Type serviceType, Type implementationType, ServiceLifetime lifetime) =>
        lifetime switch
        {
            ServiceLifetime.Transient => services.AddTransient(serviceType, implementationType),
            ServiceLifetime.Scoped => services.AddScoped(serviceType, implementationType),
            ServiceLifetime.Singleton => services.AddSingleton(serviceType, implementationType),
            _ => throw new ArgumentException("Invalid lifeTime", nameof(lifetime))
        };
}
