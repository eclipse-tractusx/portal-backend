/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
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

using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Org.CatenaX.Ng.Portal.Backend.Tests.Shared.IntegrationTests;

public class IntegrationTestFactory<TProgram> : WebApplicationFactory<TProgram>, IAsyncLifetime
    where TProgram : class 
{
    private readonly TestcontainerDatabase _container;
    public IList<Action<PortalDbContext>>? SetupDbActions { get; set; }

    public IntegrationTestFactory()
    {
        _container = new TestcontainersBuilder<PostgreSqlTestcontainer>()
            .WithDatabase(new PostgreSqlTestcontainerConfiguration
            {
                Database = "test_db",
                Username = "postgres",
                Password = "postgres",
            })
            .WithImage("postgres")
            .WithCleanUp(true)
            .WithName(Guid.NewGuid().ToString())
            .Build();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var projectDir = Directory.GetCurrentDirectory();
        var configPath = Path.Combine(projectDir, "appsettings.IntegrationTests.json");

        builder.ConfigureAppConfiguration((context, conf) =>
        {
            conf.AddJsonFile(configPath, true);
        });
        builder.ConfigureTestServices(services =>
        {
            services.RemoveProdDbContext<PortalDbContext>();
            services.AddDbContext<PortalDbContext>(options =>
            {
                options.UseNpgsql(_container.ConnectionString,
                    x => x.MigrationsAssembly(typeof(PortalDbContextFactory).Assembly.GetName().Name)
                        .MigrationsHistoryTable("__efmigrations_history_portal"));
            });
            services.EnsureDbCreated(SetupDbActions);
            services.AddSingleton<IPolicyEvaluator, FakePolicyEvaluator>();
        });
    }

    public async Task InitializeAsync() => await _container.StartAsync();

    public new async Task DisposeAsync() => await _container.DisposeAsync();
}
