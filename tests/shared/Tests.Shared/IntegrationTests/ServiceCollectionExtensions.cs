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

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.TestSeeds;

namespace Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.IntegrationTests;

public static class ServiceCollectionExtensions
{
    public static void RemoveProdDbContext<T>(this IServiceCollection services) where T : DbContext
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<T>));
        if (descriptor != null)
            services.Remove(descriptor);
    }

    public static void EnsureDbCreatedWithSeeding<TSeedingData>(this IServiceCollection services) where TSeedingData : IBaseSeeding
    {
        var serviceProvider = services.BuildServiceProvider();

        using var scope = serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var context = scopedServices.GetRequiredService<PortalDbContext>();
        context.Database.Migrate();
        var result = (Activator.CreateInstance(typeof(TSeedingData)) as IBaseSeeding)?.SeedData();
        result?.Invoke(context);

        context.SaveChanges();
    }
}
