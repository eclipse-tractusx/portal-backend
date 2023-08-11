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

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.DateTimeProvider;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Seeding;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Migrations.Seeder;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Auditing;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using Testcontainers.PostgreSql;
using Xunit.Extensions.AssemblyFixture;

[assembly: TestFramework(AssemblyFixtureFramework.TypeName, AssemblyFixtureFramework.AssemblyName)]
namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Tests.Setup;

public class TestDbFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithDatabase("test_db")
        .WithImage("postgres")
        .WithCleanUp(true)
        .WithName(Guid.NewGuid().ToString())
        .Build();

    /// <summary>
    /// Foreach test a new portalDbContext will be created and filled with the custom seeding data. 
    /// </summary>
    /// <remarks>
    /// In this method the migrations don't need to get executed since they are already on the testcontainer.
    /// Because of that the EnsureCreatedAsync is enough.
    /// </remarks>
    /// <param name="seedActions">Additional data for the database</param>
    /// <returns>Returns the created PortalDbContext</returns>
    public async Task<PortalDbContext> GetPortalDbContext(params Action<PortalDbContext>[] seedActions)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PortalDbContext>();

        optionsBuilder.UseNpgsql(
            _container.GetConnectionString(),
            x => x.MigrationsAssembly(typeof(BatchInsertSeeder).Assembly.GetName().Name)
                .MigrationsHistoryTable("__efmigrations_history_portal")
        );
        var context = new PortalDbContext(optionsBuilder.Options, new AuditHandlerV1(new FakeIdentityService(), new UtcDateTimeProvider()));
        await context.Database.EnsureCreatedAsync().ConfigureAwait(false);
        foreach (var seedAction in seedActions)
        {
            seedAction.Invoke(context);
        }

        await context.SaveChangesAsync().ConfigureAwait(false);
        return context;
    }

    /// <summary>
    /// This method is used to initially setup the database and run all migrations
    /// </summary>
    public async Task InitializeAsync()
    {
        await _container.StartAsync()
            .ConfigureAwait(false);

        var optionsBuilder = new DbContextOptionsBuilder<PortalDbContext>();

        optionsBuilder.UseNpgsql(
            _container.GetConnectionString(),
            x => x.MigrationsAssembly(typeof(BatchInsertSeeder).Assembly.GetName().Name)
                .MigrationsHistoryTable("__efmigrations_history_portal")
        );
        var context = new PortalDbContext(optionsBuilder.Options, new AuditHandlerV1(new FakeIdentityService(), new UtcDateTimeProvider()));
        await context.Database.MigrateAsync();

        var seederOptions = Options.Create(new SeederSettings
        {
            TestDataEnvironments = new[] { "test" },
            DataPaths = new[] { "Seeder/Data" }
        });
        var insertSeeder = new BatchInsertSeeder(context,
            LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<BatchInsertSeeder>(),
            seederOptions);
        await insertSeeder.ExecuteAsync(CancellationToken.None);
        var updateSeeder = new BatchUpdateSeeder(context,
            LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<BatchUpdateSeeder>(),
            seederOptions);
        await updateSeeder.ExecuteAsync(CancellationToken.None);
    }

    /// <inheritdoc />
    public async Task DisposeAsync()
    {
        await _container.DisposeAsync()
            .ConfigureAwait(false);
    }
}
