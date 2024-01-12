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

using Laraue.EfCoreTriggers.PostgreSql.Extensions;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Logging;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Migrations.Seeder;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Auditing;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.TestSeeds;
using Testcontainers.PostgreSql;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
namespace Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.IntegrationTests;

public class IntegrationTestFactory<TTestClass, TSeedingData> : WebApplicationFactory<TTestClass>, IAsyncLifetime
    where TTestClass : class
    where TSeedingData : class, IBaseSeeding
{
    protected readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithDatabase("test_db")
        .WithImage("postgres")
        .WithCleanUp(true)
        .WithName(Guid.NewGuid().ToString())
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var projectDir = Directory.GetCurrentDirectory();
        var configPath = Path.Combine(projectDir, "appsettings.IntegrationTests.json");

        builder.ConfigureAppConfiguration((_, conf) =>
        {
            conf.AddJsonFile(configPath, true);
        });
        builder.ConfigureTestServices(services =>
        {
            var identityService = services.SingleOrDefault(d => d.ServiceType.GetInterfaces().Contains(typeof(IIdentityService)));
            if (identityService != null)
                services.Remove(identityService);
            services.AddScoped<IIdentityService, FakeIdentityService>();
            services.AddScoped<IAuditHandler, NoAuditHandler>();

            services.RemoveProdDbContext<PortalDbContext>();
            services.AddDbContext<PortalDbContext>(options =>
            {
                options.UseNpgsql(_container.GetConnectionString(),
                    x => x.MigrationsAssembly(typeof(BatchInsertSeeder).Assembly.GetName().Name)
                        .MigrationsHistoryTable("__efmigrations_history_portal"))
                    .UsePostgreSqlTriggers();
            });
            services.EnsureDbCreatedWithSeeding<TSeedingData>();
            services.AddSingleton<IPolicyEvaluator, FakePolicyEvaluator>();
        });
    }

    /// <inheritdoc />
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.AddLogging();
        return base.CreateHost(builder);
    }

    public async Task InitializeAsync() => await _container.StartAsync();

    public new async Task DisposeAsync() => await _container.DisposeAsync();
}
